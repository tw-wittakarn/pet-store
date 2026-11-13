# Paws & Claws Pet Store — Planted Issues Answer Key

> **CHEAT SHEET / SPOILER.** This lists every planted problem in the legacy app. If you're practising, find the issues yourself first, then use this to compare. Background and decisions are in [`context.md`](context.md).

- **Repo:** `/Users/wittakarn.k/Repositories/pet-store`. The legacy app lives in `legacy/`.
- **Baseline commit:** `cfb038c`. Line numbers refer to this commit. At `cfb038c` the project sat at the repo root; it was later moved under `legacy/` without content changes, so the line numbers still match. Paths below use the new `legacy/` prefix.
- **Created:** 2026-11-13
- **Status column:** ✅ = reproduced against the running app during build, 📖 = present by construction (read the code)
- **Tracking:** tick the `[ ]` boxes as you fix things.

Totals: **22 security**, **20 logic bugs**, **38 backend code smells**, **20 frontend smells/bugs**, **8 test/infra issues**.

---

## Scope: capabilities and boundaries (the "spec" the legacy app implements)

When you modernise, these are the behaviours to **preserve**, unless a planted bug says otherwise.

**Capabilities (in scope)**
1. **Catalog:** browse pets by category (Dogs, Cats, Birds, Fish, Reptiles), pet details, search, filter by category and price range, sort, paging. Supplies catalog with the same features, plus stock levels.
2. **Shopping cart:** add, remove and change quantity for pets (always qty 1, one of a kind) and supplies (quantity, limited by stock). The cart lives in the server Session. It's edited by jQuery forms on catalog pages and by the React cart page through `/api/cart`.
3. **Checkout:** requires login. Shipping details, simulated card payment (cards starting with 4 or 5 approved, `4000000000000002` declined), order creation, pets marked sold, supply stock decremented, confirmation page, "email" written to `emails.log`.
4. **Accounts:** register, log in and log out (hand-rolled: `Users` table, MD5 hash, identity in Session). Customers see their own order history.
5. **Admin back office** (session `IsAdmin` flag + `[AdminOnly]` filter): dashboard (stats, revenue, recent searches, audit log, last error), pet management with create, edit, delete, image upload and a sold/featured toggle (React table), category CRUD, supply inventory (stock, price, active flag), order list with a status change.
6. **Data:** SQLite `petstore.db`, created by EF `EnsureCreated()` and seeded from `App_Data/seed.sql` on first run. There are no migrations. Seed: 5 categories, 20 pets, 15 supplies, 3 users.
7. **Legacy structure:** MVC 5 conventions on ASP.NET Core .NET 9: `Global.asax.cs`, `App_Start/` (Route, Bundle, Filter, Database config), `Content/` and `Scripts/` with vendored Bootstrap 3.4.1 and jQuery 1.12.4.
8. **React islands:** the cart page and header mini-cart use React 16, Redux 4 and redux-saga 1, built by webpack 4 from `ClientApp/` with the bundle committed. The admin pet table uses UMD React with in-browser Babel.

**Boundaries (out of scope by design; don't count these as "missing")**
- No real payment gateway, email or shipping integration (all simulated).
- No full SPA: only React islands mounted into Razor pages.
- No cloud deployment, CI/CD or containers.
- No multi-language or multi-currency support (AUD-style `$`, 4-digit AU postcodes).
- One web project + one MSTest project. No layered or enterprise architecture, on purpose.
- Local use only. The security holes are deliberate training material.

---

## 0. Quick repro cheat sheet

| What | How |
|---|---|
| Run | `cd legacy && dotnet run --project PetStore.Web` → http://localhost:5080 |
| Reset DB | delete `legacy/PetStore.Web/petstore.db` |
| Login as admin with no password | username `admin' --`, any password |
| Make yourself admin | POST `/Account/Register` with extra field `IsAdmin=1` |
| Delete any pet anonymously | GET `/Admin/DeletePet/20` (not logged in) |
| Negative stock | add Dog Leash (stock 3) → in React cart set qty 50 → checkout → `Supplies.Stock = -47` |
| Totals mismatch | add any item, compare Cart (React) total vs "Estimated total" line vs Checkout total |
| Float leak | Checkout page shows `Tax (7.000000000000001%)` and tax like `153.82500000000002` |
| Lost pets on last page | `/Pets` has 20 pets, page size 6 → only 3 pages; pets 19–20 unreachable |

---

## 1. Security vulnerabilities

| # | ✓ | Issue | Location | Notes / repro |
|---|---|---|---|---|
| S-01 | ✅ [ ] | **SQL injection: login bypass** | `legacy/PetStore.Web/Controllers/AccountController.cs:29` | Username concatenated into SQL. `admin' --` logs in as admin with any password. |
| S-02 | ✅ [ ] | **SQL injection: catalog search** | `legacy/PetStore.Web/Controllers/PetsController.cs:27` | `search=zzz') OR 1=1--` returns all 20. Boolean/blind only: the same WHERE clause also feeds a one-column `COUNT(*)` query (line ~42), so UNION breaks that query. `minPrice`/`maxPrice` are typed, but they're still concatenated. |
| S-03 | 📖 [ ] | SQL injection: order status | `legacy/PetStore.Web/Controllers/AdminController.cs:208` | `status` string concatenated. Combined with S-07 it's reachable anonymously. |
| S-04 | 📖 [ ] | SQL injection: settings and raw helpers | `legacy/PetStore.Web/Helpers/Utils.cs:472`, `:485` | `GetSetting`/`SetSetting` concatenate key/value. Also every `Utils.GetDataTable("... " + id)` pattern in `Utils.cs` (AddToCart ~:211, :230, stock helpers ~:528–548). Ints are currently safe, but the pattern invites injection. `Utils.Esc` (quote doubling) is used inconsistently. |
| S-05 | ✅ [ ] | **Mass assignment → privilege escalation** | `legacy/PetStore.Web/Controllers/AccountController.cs:68` | `Register(User user, ...)` binds the whole entity, including `IsAdmin`, `Id` and `PasswordHash`. Posting `IsAdmin=1` creates an admin. |
| S-06 | ✅ [ ] | **Missing authorisation: DeletePet via GET** | `legacy/PetStore.Web/Controllers/AdminController.cs:118` | No `[AdminOnly]`, GET verb, no anti-forgery token. Anonymous `/Admin/DeletePet/20` deletes the pet. Also CSRF-able with an `<img src>`. |
| S-07 | ✅ [ ] | **Missing authorisation: UpdateOrderStatus** | `legacy/PetStore.Web/Controllers/AdminController.cs:206` | No `[AdminOnly]`, GET. Anyone can cancel or ship any order. |
| S-08 | ✅ [ ] | Missing authorisation: PetsJson data feed | `legacy/PetStore.Web/Controllers/AdminController.cs:45` | Anonymous users can read the full admin pet list. |
| S-09 | ✅ [ ] | **IDOR on order confirmation** | `legacy/PetStore.Web/Controllers/CheckoutController.cs:162` | No ownership check. msmith can view jdoe's order (name, address, masked card) at `/Checkout/Confirmation/1`. |
| S-10 | ✅ [ ] | **Stored XSS: pet description** | `legacy/PetStore.Web/Views/Pets/Details.cshtml:20` (`Html.Raw`) + weak filter `legacy/PetStore.Web/Helpers/Utils.cs:708` | `SanitizeHtml` only strips the literal lowercase `<script>`. `<img src=x onerror=alert(1)>` or `<SCRIPT>` pass. |
| S-11 | ✅ [ ] | **Reflected XSS: search term** | `legacy/PetStore.Web/Views/Pets/Index.cshtml:43` | `/Pets?search=<script>alert(1)</script>` executes. |
| S-12 | 📖 [ ] | Stored XSS against admin: recent searches | `legacy/PetStore.Web/Views/Admin/Index.cshtml:21` | Anonymous search terms are stored in a static list and rendered with `Html.Raw` on the admin dashboard, so an anonymous user can get script running in an admin's browser. |
| S-13 | 📖 [ ] | Stored XSS against admin: order notes | `legacy/PetStore.Web/Views/Admin/Orders.cshtml:34` | Checkout "Notes" rendered with `Html.Raw`. |
| S-14 | 📖 [ ] | **Unrestricted file upload / path traversal** | `legacy/PetStore.Web/Helpers/Utils.cs:854` | Keeps the client `file.FileName`. No extension, MIME or size checks. Files go into `wwwroot/uploads` (served statically: `.html`, `.svg` with script). Filenames can overwrite each other. |
| S-15 | ✅ [ ] | **Unsalted MD5 password hashing** | `legacy/PetStore.Web/Helpers/Utils.cs:45`, `:62` | `admin123` = `0192023a7bbd73250516f069df18b500` (rainbow-table trivial). The minimum password length is 4 (`Utils.cs:763`). |
| S-16 | ✅ [ ] | **Full card number stored in plain text** | `legacy/PetStore.Web/Controllers/CheckoutController.cs:122`, model `legacy/PetStore.Web/Models/Order.cs:20` | It's also shown in full on the admin orders page (`legacy/PetStore.Web/Views/Admin/Orders.cshtml:21`) and written to `emails.log` (`Utils.cs:972`). `EncryptCard` (base64) exists but is never used (`Utils.cs:89`). Expiry is stored too. |
| S-17 | ✅ [ ] | Open redirect | `legacy/PetStore.Web/Controllers/AccountController.cs:50`, `legacy/PetStore.Web/Controllers/CartController.cs:37` | `returnUrl=https://evil.example` is followed blindly. Cart also redirects to the `Referer` header. |
| S-18 | 📖 [ ] | No CSRF protection anywhere | all `[HttpPost]` actions; GET state changes: `CartController.cs:44` (Remove), `:58` (Clear), `AdminController.cs` DeleteCategory (~:159) | There's no `[ValidateAntiForgeryToken]` and no `AutoValidateAntiforgeryToken` global filter. |
| S-19 | 📖 [ ] | Secrets committed in config and code | `legacy/PetStore.Web/appsettings.json:13-15` (admin password, "live" payment key, SMTP password); `legacy/PetStore.Web/Helpers/Utils.cs:1026` (PayPal creds in dead code); `legacy/PetStore.Web/Views/Account/Login.cshtml:15` (test creds in an HTML comment sent to every visitor) | |
| S-20 | 📖 [ ] | Plain-text password emailed | `legacy/PetStore.Web/Controllers/AccountController.cs:85` | The welcome email contains the user's password (written to `emails.log`). |
| S-21 | 📖 [ ] | Session/auth weaknesses | `legacy/PetStore.Web/Program.cs:20` (`HttpOnly = false`), `Utils.SetLogin` (`Utils.cs` ~:150) | No session ID regeneration on login (session fixation). The admin flag lives in session as a string, and the `"1"`/`"True"` checks are duplicated (`legacy/PetStore.Web/Filters/AdminOnlyAttribute.cs:12`, `Utils.IsAdmin`). There's no HTTPS, no lockout and no rate limiting. User info is leaked into `window.APP_CONFIG` (`legacy/PetStore.Web/Views/Shared/_Layout.cshtml:25`, `Utils.cs:1004`). |
| S-22 | 📖 [ ] | Information disclosure | `legacy/PetStore.Web/Program.cs:33` (`UseDeveloperExceptionPage` always on), `legacy/PetStore.Web/Views/Shared/Error.cshtml` (prints `LastError` stack trace), `legacy/PetStore.Web/App_Start/FilterConfig.cs:27` (puts `exception.Message` in the UI), the admin dashboard shows the last stack trace, `Home/About` shows uptime | |

---

## 2. Logic bugs (behaviour a refactor must consciously preserve or fix)

| # | ✓ | Bug | Location | Notes / repro |
|---|---|---|---|---|
| B-01 | ✅ [ ] | **`double` used for money** | `legacy/PetStore.Web/Models/Pet.cs:22`, `legacy/PetStore.Web/Models/Order.cs` (all totals), `CartItem.Price`; while `Supply.Price` is `decimal` (`legacy/PetStore.Web/Models/Supply.cs:19`) | Order 1 saved `Total = 2351.325`. The API returns `tax: 86.79300000000002`. Mixed `double`/`decimal` with casts in `CartApiController.cs` ~:80 and `Utils.AddToCart`. |
| B-02 | ✅ [ ] | Float noise shown to the user | `legacy/PetStore.Web/Views/Checkout/Index.cshtml:46` | Renders `Tax (7.000000000000001%)` and the unformatted `@Model.Tax`. |
| B-03 | 📖 [ ] | **Three (four) different total calculations** | Cart Razor estimate: `Utils.CalculateCartTotal` (`Utils.cs` ~:378) → per-line rounded tax, shipping free if `> 100`. React cart: `legacy/PetStore.Web/ClientApp/src/utils/money.js:13` → tax on subtotal rounded once, free if `>= 100`. Cart API: `legacy/PetStore.Web/Controllers/CartApiController.cs:42-44` → unrounded `sub * TAX`, `>= 100`. Checkout: `legacy/PetStore.Web/Helpers/Utils.cs:360` `CalcTax2` → **tax on subtotal + shipping**, `>= 100`. | The customer sees one total in the cart and is charged another at checkout. A subtotal of exactly $100 is free shipping in React and checkout but not in the Razor estimate. |
| B-04 | ✅ [ ] | Four sources of tax rate | `Utils.TAX = 0.07` (`Utils.cs:28`), `MvcApplication.TaxRate = 0.07` (`legacy/PetStore.Web/Global.asax.cs:12`), `Settings.TaxRate = 0.08` (`legacy/PetStore.Web/App_Data/seed.sql:6`, shown on the admin dashboard as "configured"), `appsettings.json AppSettings:TaxRate = 0.075` (never read), plus `money.js:2` | The admin believes tax is 8%. |
| B-05 | ✅ [ ] | **Stock can go negative** | `legacy/PetStore.Web/Controllers/CartApiController.cs:87` (PUT: no stock check), `:79` (POST add: no stock check), `legacy/PetStore.Web/Controllers/CheckoutController.cs:89` (only checks `stock <= 0`, not `stock < qty`), `:151` | Leash stock 3 → order qty 50 → stock `-47`. |
| B-06 | 📖 [ ] | **Non-atomic stock decrement (race)** | `legacy/PetStore.Web/Controllers/CheckoutController.cs:150-151` | It reads the stock, then writes `stock - qty` in a separate statement with no transaction, so concurrent checkouts lose updates. |
| B-07 | 📖 [ ] | **Same pet can be sold twice** | `legacy/PetStore.Web/Controllers/CheckoutController.cs:146` | `IsSold` is checked only when adding to the cart (`Utils.AddToCart`), not at checkout. Two users with the same pet in their carts both pay. |
| B-08 | 📖 [ ] | Card charged before the order is persisted; no rollback | `legacy/PetStore.Web/Controllers/CheckoutController.cs:101` then `:139` | If `SaveChanges` or a stock update throws, the global filter swallows it and redirects home, and the customer is charged with no order. The order, lines and stock updates aren't in one transaction (EF and raw ADO are mixed). |
| B-09 | ✅ [ ] | **Pagination drops the last partial page** | `legacy/PetStore.Web/Controllers/PetsController.cs:43` | `total / PAGE_SIZE` uses integer division: 20/6 = 3 pages, so pets 19–20 are never listed. Supplies use `Math.Ceiling` correctly (inconsistent). |
| B-10 | ✅ [ ] | **Order history sorted by a string date** | `legacy/PetStore.Web/Controllers/AccountController.cs:94`, `legacy/PetStore.Web/Models/Order.cs:10` | `dd/MM/yyyy` strings sort lexicographically: shown order was `28/12/2025, 12/11/2026, 05/01/2026`. |
| B-11 | 📖 [ ] | Local time and timezones everywhere | `DateTime.Now` in `Utils.cs` (FormatDate, Now, TimeAgo, IsValidExpiry, EstimatedDelivery, Log `:495` uses culture-dependent `ToString()`), `Pet.IsNew` `legacy/PetStore.Web/Models/Pet.cs:47`, `GenerateOrderNumber` | There's no UTC. "NEW" badges depend on server time. Card expiry is validated against server local time. Dates are stored as strings in three formats (`dd/MM/yyyy`, `dd/MM/yyyy HH:mm`, `DateTime.ToString()`). |
| B-12 | ✅ [ ] | **Deleting a category cascades and deletes all its pets** | `legacy/PetStore.Web/Controllers/AdminController.cs:161` (raw `DELETE`) + `ON DELETE CASCADE` from EF `EnsureCreated` | Microsoft.Data.Sqlite enables foreign keys, so deleting "Reptiles" silently deletes Spike, Shelly and Kaa. Supplies have no FK (`CatId`), so they become orphans showing "Unknown". |
| B-13 | ✅ [ ] | Stale category cache | `legacy/PetStore.Web/Helpers/Utils.cs:510` (`MvcApplication.CategoryCache`) | Rename "Dogs" → "Puppies": the Pets list heading still says "Dogs" while the details breadcrumb says "Puppies". It's never invalidated, isn't thread-safe, and isn't touched by SaveCategory/DeleteCategory. |
| B-14 | 📖 [ ] | Advertised discount never charged | `legacy/PetStore.Web/Helpers/Utils.cs:387` `ApplyDiscount`, used in `legacy/PetStore.Web/Views/Shared/_PetCard.cshtml:15` and `Pets/Details` | "10% off dogs over $1000" is displayed on the list and details pages, but the cart and checkout charge full price. The home page card (`Utils.BuildPetCardHtml`) doesn't show the discount at all. |
| B-15 | 📖 [ ] | Coupon "WOOF10" is fake | `legacy/PetStore.Web/ClientApp/src/reducers/rootReducer.js:60` | The UI says "applied, discount shown at checkout", but the server knows nothing about it. |
| B-16 | 📖 [ ] | **Header badge vs mini-cart disagree** | Header = sum of quantities (`legacy/PetStore.Web/Views/Shared/_Layout.cshtml:7`), MiniCart = number of lines (`legacy/PetStore.Web/ClientApp/src/components/MiniCart.jsx:30`) | With 2 leashes + 1 pet the badge shows 3 and the mini-cart says "2 items". |
| B-17 | 📖 [ ] | **Session vs Redux drift** | `legacy/PetStore.Web/wwwroot/Scripts/site.js` (jQuery ajax add updates only `#cart-count`), Redux store not refreshed | After an ajax "Add to cart" on a catalog page, the React mini-cart keeps showing stale contents until reload. |
| B-18 | 📖 [ ] | Optimistic update without rollback + out-of-order responses | `legacy/PetStore.Web/ClientApp/src/reducers/rootReducer.js:41` (mutates state in place), `legacy/PetStore.Web/ClientApp/src/sagas/cartSagas.js:49` (`takeEvery`, no `takeLatest`/debounce) | Typing "12" in qty sends PUT 1 and PUT 12 concurrently. Whichever response lands last wins in the UI. A failed PUT leaves the optimistic qty displayed. |
| B-19 | 📖 [ ] | Item type case inconsistency | API stores `req.type` verbatim (`legacy/PetStore.Web/Controllers/CartApiController.cs:72`). `Utils.UpdateCartQty` compares case-sensitively (`Utils.cs:278`), while `AddToCart` upper-cases (`Utils.cs:207`) and `RemoveFromCart` compares case-insensitively | A `"pet"`-typed item can't be updated via `/Cart/Update`, and duplicate pets can be added (`"pet"` vs `"PET"`). The PET qty-forced-to-1 rule is bypassed via the API. |
| B-20 | 📖 [ ] | Supplies search is case-sensitive; pets search isn't | `legacy/PetStore.Web/Controllers/SuppliesController.cs:27` (`string.Contains`) vs SQL `LIKE` in Pets | Searching "dog" in supplies finds nothing ("Dog Leash"). |

Smaller, related ones:
- [ ] `IsValidEmail` rejects TLDs longer than 4 characters, such as `.museum` and `.travel` (`Utils.cs:721`). There's no Luhn check (`Utils.cs:731`).
- [ ] `GenerateOrderNumber` increments a global counter as a side effect, and the confirmation page uses a **different** number format (`"PS-" + id` vs `"PS-yyMMdd-id"`) (`Utils.cs:950`, `CheckoutController.cs` ~:166).
- [ ] `Supply.Active` has an undocumented value `2` ("discontinued?"). The admin page edits it as a raw integer.
- [ ] `Pet.Age` is free text parsed by regex (`Utils.AgeInMonths`).
- [ ] `ClearLogin` keeps the cart, so on a shared computer the next user inherits it.
- [ ] Seed loader splits on `;`, so any seed text containing `;` breaks seeding (`legacy/PetStore.Web/App_Start/DatabaseConfig.cs:31`). Failures are swallowed and the site runs empty (`:50`).
- [ ] Link generation picks the legacy route: `RedirectToAction("Index","Pets")` produces `/shop`, because `legacy-shop` is registered before `default` (`legacy/PetStore.Web/App_Start/RouteConfig.cs`).
- [ ] `ChargeCard` blocks the request thread with `Thread.Sleep(500)` (`Utils.cs:773`). The "approval" rule is `StartsWith("4")||StartsWith("5")`.

---

## 3. Backend code smells / architecture

**Structure and coupling**
- [ ] CS-01 **God class** `legacy/PetStore.Web/Helpers/Utils.cs` (1,073 lines). It mixes security, session, cart, money, raw DB, HTML generation, validation, dates, file IO, email, config and dead code.
- [ ] CS-02 **Service locator / ambient context:** `HttpContextHelper.Current` (`legacy/PetStore.Web/Helpers/HttpContextHelper.cs`) is used by `Utils.Session` (`Utils.cs:113`). Static code depends on the request.
- [ ] CS-03 **Global mutable static state:** `legacy/PetStore.Web/Global.asax.cs:17` onwards (`RecentSearches`, `CategoryCache`, `OrdersPlacedSinceStartup`, `LastError`). It isn't thread-safe, resets on restart and breaks with more than one instance.
- [ ] CS-04 **No dependency injection:** every controller does `new PetStoreContext()` in a field initialiser and never disposes it (e.g. `HomeController.cs:15`).
- [ ] CS-05 **Two data-access styles:** EF Core and string-built ADO.NET, sometimes in the same action (AdminController SaveCategory: raw INSERT vs EF update; Checkout: EF order + raw stock update).
- [ ] CS-06 **Fat controllers:** `CheckoutController.PlaceOrder` (~110 lines) does validation, pricing, stock, payment, persistence, inventory, email, logging and cart clearing.
- [ ] CS-07 **Duplicate cart implementations:** `Utils` cart functions vs a private copy in `CartApiController` (Load/Save/Add/Update/Remove), with different rules.
- [ ] CS-08 **Duplicated catalog stacks:** `PetsController` (raw SQL, broken paging) vs `SuppliesController` ("copied and switched to EF", in-memory filtering).
- [ ] CS-09 **Duplicated HTML rendering:** `Utils.BuildPetCardHtml` (C# strings) vs `Views/Shared/_PetCard.cshtml`. They have already drifted: no discount, no category, no details button.
- [ ] CS-10 Multiple formatting helpers that do almost the same thing: `FormatPrice`, `FormatMoney`, `ToCurrency(decimal)`, `ToCurrency(object)`, `Pet.DisplayPrice`, `Supply.DisplayPrice`.
- [ ] CS-11 Multiple shipping/tax functions with a "DEPRECATED but still used" comment (`Utils.GetShipping`, `Utils.cs:375`).
- [ ] CS-12 Two near-duplicate models `Pet` and `Supply` with inconsistent naming (`Id`/`SupplyId`, `Name`/`SupplyName`, `CategoryId`/`CatId`, `Description`/`Desc`, `ImageUrl`/`ImagePath`), types (`double`/`decimal`) and flags (`bool IsSold` vs `int Active`, `int IsAdmin`).
- [ ] CS-13 Business logic in views: the layout deserialises the session cart (`_Layout.cshtml:7`), `_PetCard` computes discounts, `Home/About` queries settings, `Admin/Supplies` calls `Utils.GetCategoryName` per row (N+1).
- [ ] CS-14 `ViewBag` everywhere for typed data, cast in views (`(List<Pet>)ViewBag.Featured`, `(double)ViewBag.Total`).
- [ ] CS-15 Domain entities used as view and binding models (`Register(User user)`, `EditPet(Pet pet)`). This is the cause of S-05.
- [ ] CS-16 `PetListModel` is commented "used by admin pages AND pets index (don't change!!)".
- [ ] CS-17 Manual auth checks are duplicated: Checkout checks the session by hand twice instead of `[MustLogin]`. `MustLoginAttribute` is a copy-paste of `AdminOnly`. The role is a session string.

**Data and persistence**
- [ ] CS-18 **No migrations:** schema from `EnsureCreated()` + `seed.sql` creates extra tables EF doesn't know about (`AuditLog`, `Settings`), see `legacy/PetStore.Web/App_Data/seed.sql:3`.
- [ ] CS-19 Seeding runs in a **static constructor** triggered by a no-op `Initialize()` (`legacy/PetStore.Web/App_Start/DatabaseConfig.cs:14`). Exceptions are swallowed.
- [ ] CS-20 The connection string is hard-coded in two places (`DAL/PetStoreContext.cs:21`, `Global.asax.cs`). The one in `appsettings.json:10` is different and unused.
- [ ] CS-21 N+1 queries: `HomeController.cs:36` (count per category), `AccountController.cs:97` (lines per order), `AdminController.cs:195` (user per order).
- [ ] CS-22 Loading whole tables into memory: `PetsController.cs:91` (`db.Pets.ToList().Where(...)`), `SuppliesController.cs:19`, Home low-stock, `Admin.Orders` filters in memory.
- [ ] CS-23 Dates and ages stored as strings (`Pet.CreatedDate`, `Order.OrderDate`, `Pet.Age`).
- [ ] CS-24 Magic numbers and strings: status strings (`"Paid"`, `"Cancelled"` vs `"cancelled"`), item types `"PET"`/`"SUPPLY"`, `Active` 0/1/2, `PAGE_SIZE` 6 vs hard-coded 8 in Supplies, `MAX_QTY`.

**Error handling, logging, ops**
- [ ] CS-25 A global exception filter swallows everything and redirects home (`App_Start/FilterConfig.cs:28`).
- [ ] CS-26 Empty `catch {}` blocks throughout `Utils.cs` (logging, settings, cart deserialisation).
- [ ] CS-27 Logging is DB-based via string SQL with a file fallback in the CWD, and there's no `ILogger`.
- [ ] CS-28 Email "sending" appends to `emails.log` in the CWD.
- [ ] CS-29 Blocking calls (`Thread.Sleep`) and no `async` anywhere.
- [ ] CS-30 `Random` is a static shared instance (not thread-safe) and is used for "tokens" (`Utils.cs:39`).
- [ ] CS-31 Session holds the full cart as JSON (Newtonsoft) and user identity, and the session cookie isn't HttpOnly.

**Legacy port leftovers**
- [ ] CS-32 A fake `Global.asax.cs` / `MvcApplication.Application_Start` called from `Program.cs`.
- [ ] CS-33 A hand-rolled `BundleConfig.Render` (`App_Start/BundleConfig.cs`) with hard-coded cache-busting `?v=3`.
- [ ] CS-34 `RouteConfig` legacy routes (`pet.aspx`, `shop/{id}`) shadow the default route for link generation.
- [ ] CS-35 `Pets/Details` parses `Request.Query["id"]` with `int.Parse` (it throws on bad input) to support `pet.aspx?id=` (`PetsController.cs:79`).
- [ ] CS-36 Big "OLD CODE - DO NOT DELETE" block of commented-out code (`Utils.cs:1020`).
- [ ] CS-37 `Nullable` and `ImplicitUsings` are disabled, and warnings are suppressed with `NoWarn` (`PetStore.Web.csproj`).
- [ ] CS-38 A misleading comment: `Order.OrderDate` says "formatted with Utils.FormatDate", but it's actually `FormatDateTime`. Also the "TODO: repository pattern (2016) / DI (2019)" comments.

---

## 4. Frontend (jQuery + React/Redux/Saga) smells and bugs

- [ ] FE-01 **Two React builds on one page:** the webpack bundle (React inside) is loaded on every page via the layout, and the admin page also loads the UMD React (`Views/Admin/Pets.cshtml`). There are two React and Redux instances and two stores (`window.cartStore`, `window.adminStore`).
- [ ] FE-02 **In-browser Babel** for the admin table (`Views/Admin/Pets.cshtml:10`, about 1.7 MB `babel.min.js`). The console warns "precompile your scripts".
- [ ] FE-03 **Committed bundle out of date with the source:** `ClientApp/src/components/CartRow.jsx:44` has a "Delete" button with a confirm dialog, but `wwwroot/Scripts/dist/cart.bundle.js` still renders "Remove" with no confirm. Proof: the live cart page shows "Remove".
- [ ] FE-04 **Webpack 4 + Node 17+ requires `NODE_OPTIONS=--openssl-legacy-provider`** (`ClientApp/package.json:7`). Builds fail with `ERR_OSSL_EVP_UNSUPPORTED` without it.
- [ ] FE-05 Deprecated `@babel/polyfill` + core-js 2 (`package.json:11`), IE11 target, `fsevents@1` warnings. React development builds are served in production for the admin page (`react.development.js`, `BundleConfig.cs`).
- [ ] FE-06 Legacy lifecycles: `componentWillReceiveProps` (`CartRow.jsx:11`, `CartPage.jsx:18`), `componentWillMount` (`wwwroot/Scripts/admin/petTable.jsx:60`, warns in console).
- [ ] FE-07 Derived state copied into component state (`CartPage` totals, `CartRow` qty), the classic "you probably don't need derived state" anti-pattern.
- [ ] FE-08 **State mutation in reducers:** `rootReducer.js:41` (`item.Qty = action.qty`), `petTable.jsx:21` (`state.pets[i].IsSold = ...`).
- [ ] FE-09 One giant reducer mixing cart, UI, coupon, user and request counters. `requestsInFlight` goes negative or stays positive (decremented on error paths inconsistently).
- [ ] FE-10 `takeEvery` everywhere: no `takeLatest`, debounce or cancellation (`cartSagas.js:49`). `toggleSoldSaga` has no error handling (`petTable.jsx:49`).
- [ ] FE-11 Mixed HTTP clients: `fetch` (`api/cartApi.js`) and `window.$.ajax` inside a saga (`cartSagas.js`, "fetch DELETE didn't work in IE"), plus `$.getJSON`/`$.post` in the admin table.
- [ ] FE-12 Sagas reach into the DOM with jQuery to update the header badge (`cartSagas.js:41`). React and jQuery both own `#cart-count`.
- [ ] FE-13 Duplicate `FETCH_CART` on the cart page: both `CartPage` and `MiniCart` dispatch on mount (`CartPage.jsx:15`, `MiniCart.jsx:8`).
- [ ] FE-14 Business rules duplicated in JS with a "keep in sync with Utils.cs !!!" comment (`ClientApp/src/utils/money.js:2`). The React page ignores the server-computed totals the API already returns.
- [ ] FE-15 Globals: `window.APP_CONFIG` (includes email and admin flag), `window.cartStore`, `window.adminStore`, `var PetStore = PetStore || {}`.
- [ ] FE-16 `site.js` polls `/Cart/Count` every 30 seconds on every page forever (`wwwroot/Scripts/site.js:35`).
- [ ] FE-17 jQuery 1.12.4 and Bootstrap 3.4.1 vendored (both EOL; jQuery 1.x has known XSS CVEs). There's no package manager for the vendored libraries. CSS uses `!important` overrides (`Content/Site.css`).
- [ ] FE-18 No TypeScript, no PropTypes, no lint, no frontend tests, no source maps.
- [ ] FE-19 Inline `<script>` in views (`Checkout/Index.cshtml`) and inline styles everywhere.
- [ ] FE-20 The admin table sorts with a comparator that never returns 0, sorts mixed types (string `CreatedDate` in `dd/MM/yyyy`), and `Array.prototype.sort` mutates the filtered list.

---

## 5. Tests and infrastructure

- [ ] T-01 Coverage is almost zero: only `Utils` helpers are tested (`legacy/PetStore.Tests/UtilsTests.cs`). There are no tests for controllers, checkout, cart, auth or the API.
- [ ] T-02 A brittle DB test asserts an exact seed count (`legacy/PetStore.Tests/DatabaseTests.cs:24`, `Assert.AreEqual(20, ...)`). It creates a real `petstore.db` in the test bin folder, depends on the CWD and shares state between runs.
- [ ] T-03 `[Ignore("fails sometimes on build server - fix later")]` (`DatabaseTests.cs:37`). **It actually fails every time**: it documents B-03 (cart 39.33 vs checkout ≈ 40.03).
- [ ] T-04 Untestable design: statics, `HttpContextHelper.Current`, `new DbContext()` and `DateTime.Now` make seams very hard to add. Characterisation tests are the first step.
- [ ] T-05 No CI, no linting/analyzers, warnings suppressed.
- [ ] T-06 No health checks, structured logging or config per environment (`appsettings.Development.json` is unused).
- [ ] T-07 Runtime files are written into the content root (`petstore.db`, `emails.log`, `petstore.log`, `wwwroot/uploads`), so the app can't run read-only or in containers.
- [ ] T-08 Node toolchain lock-in: webpack 4, babel 7.12 and 594 npm packages, many deprecated.

---

## 6. Suggested modernisation path (for reference only)

1. **Safety net first:** add characterisation tests around checkout totals, cart and auth. Pin the current behaviour, including the bugs you decide to keep.
2. **Stop the bleeding (security):** S-01/02/03/04 (parameterise), S-05 (view models), S-06/07/08 (authorisation policy), S-10–13 (remove `Html.Raw`), S-15 (ASP.NET Core Identity / PBKDF2), S-16 (never store PAN), S-18 (anti-forgery), S-19 (user-secrets).
3. **Introduce seams:** DI for DbContext, `IClock`, `ICartService`, `IPricingService`, `IPaymentGateway`. Strangle `Utils.cs` region by region.
4. **Single source of truth for pricing:** one `PricingService` (decimal), with the API returning totals and React displaying them.
5. **Data:** EF migrations (baseline from the current schema, including `AuditLog`/`Settings`), proper `DateTime`/`decimal` columns, transactional checkout with optimistic concurrency on stock.
6. **Frontend:** a single build (Vite), remove in-browser Babel, hooks + Redux Toolkit (RTK Query could replace sagas), TypeScript, remove jQuery cart code. Then decide between React islands or full SPA.
7. **Unify the Pet/Supply catalog** behind a `Product` abstraction.
