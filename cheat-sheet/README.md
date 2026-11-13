# Cheat Sheet: Paws & Claws Legacy Pet Store

> ⚠️ **SPOILER.** This folder lists every deliberately planted security hole, bug and code smell in `legacy/`, organised by **logical boundary**. If you're practising modernisation, do your own analysis first, then use these files to compare and track progress. Background and decisions are in [`../context.md`](../context.md).

- **Baseline commit:** `cfb038c`. Line numbers refer to this commit. At that commit the app sat at the repo root; it was later moved under `legacy/` without content changes, so the line numbers still match. All paths here use the `legacy/` prefix.
- **Status:** ✅ = reproduced against the running app during the build · 📖 = present by construction (read the code)
- **Tracking:** tick the `[ ]` in each file as you fix things. Mention the issue ID (`S-01`, `B-05`, `CS-12`, `FE-08`, `T-03`) in commit messages.
- **ID prefixes:** `S` security · `B` logic bug · `CS` backend code smell · `FE` frontend · `T` tests and infrastructure.
- Each issue has **one primary home** (the boundary that should own the fix). Other boundaries it touches are listed under "Also affected by".
- **ID history:** this folder replaces the old single-file `PETSTORE_ANSWER_KEY.md`. S-01…S-22, B-01…B-20, CS-01…CS-38, FE-01…FE-20 and T-01…T-08 keep their original IDs. **B-21** (checkout crash on missing user) and **B-22** (wrong revenue) were found while splitting by boundary. **B-23…B-31** were the unnumbered "smaller" bullets in the old file (B-23 email regex, B-24 no Luhn, B-25 order-number formats, B-26 `Active=2`, B-27 free-text age, B-28 logout keeps cart, B-29 seed split on `;`, B-30 legacy route link generation, B-31 gateway sleep/approval rule).

Totals: **22 security · 31 logic bugs · 38 backend smells · 20 frontend · 8 tests/infra = 119 issues.**

---

## 1. Scope: capabilities and boundaries (the spec the legacy app implements)

When modernising, these are the behaviours to **preserve**, unless a planted bug says otherwise.

**Capabilities (in scope)**
1. **Catalog:** browse pets by category (Dogs, Cats, Birds, Fish, Reptiles), pet details, search, filter by category and price range, sort, paging. Supplies catalog with stock levels.
2. **Shopping cart:** add, remove and change quantity for pets (qty always 1, one of a kind) and supplies (quantity, limited by stock). It's stored in Session and edited by jQuery forms (`/Cart/Add`) and the React cart (`/api/cart`).
3. **Checkout:** login required. Shipping details, simulated card payment (cards starting with 4 or 5 approved, `4000000000000002` declined), order + lines saved, pets marked sold, stock decremented, confirmation page, "email" to `emails.log`.
4. **Accounts:** register, log in and log out (hand-rolled: `Users` table, MD5, identity in Session); order history.
5. **Admin back office** (session `IsAdmin` + `[AdminOnly]`): dashboard, pet management (React table, create/edit/delete, image upload, sold/featured), category CRUD, supply inventory, order status management.
6. **Data:** SQLite `petstore.db` via `EnsureCreated()` + `App_Data/seed.sql`. No migrations. Seed: 5 categories, 20 pets, 15 supplies, 3 users.
7. **Legacy structure:** MVC 5 conventions on ASP.NET Core .NET 9 (`Global.asax.cs`, `App_Start/`, vendored Bootstrap 3.4.1 and jQuery 1.12.4 in `Content/` and `Scripts/`).
8. **React islands:** cart + mini-cart (React 16 / Redux 4 / redux-saga 1, webpack 4, committed bundle); admin pet table (UMD React + in-browser Babel).

**Boundaries (out of scope by design; don't count these as "missing")**
- No real payment, email or shipping integration (all simulated).
- No full SPA, only React islands inside Razor pages.
- No cloud deployment, CI/CD or containers.
- No multi-language or multi-currency support (`$`, 4-digit AU postcodes).
- One web project + one MSTest project; no enterprise layering, on purpose.
- Local use only. The security holes are deliberate training material.

---

## 2. Logical boundaries: index

### Core domain

| # | Boundary | Responsibility | Data it owns | File | S | B | CS | FE | T |
|---|---|---|---|---|---|---|---|---|---|
| 1 | Catalog: Pets | Browse, search, filter, sort, page pets; details; related; NEW badge | `Pets`, `Categories` (read) | [01-catalog-pets.md](01-catalog-pets.md) | 2 | 2 | 3 | – | – |
| 2 | Catalog: Supplies | Browse, search, filter, sort supplies; details | `Supplies` | [02-catalog-supplies.md](02-catalog-supplies.md) | – | 1 | 2 | – | – |
| 3 | Category management | Category list and order; id → name lookup | `Categories` | [03-category-management.md](03-category-management.md) | – | 2 | – | – | – |
| 4 | Inventory | Supply stock and active flag; pet sold status | `Supplies.Stock/Active`, `Pets.IsSold` | [04-inventory.md](04-inventory.md) | – | 4 | – | – | – |
| 5 | Pricing and promotions | Price display, discount, coupon, tax, shipping, totals | tax/shipping settings, discount rule | [05-pricing-promotions.md](05-pricing-promotions.md) | – | 6 | 2 | 1 | 1 |
| 6 | Shopping cart | Add, remove, update qty, count, cart page | Session `"Cart"` (`CartItem`) | [06-shopping-cart.md](06-shopping-cart.md) | – | 5 | 2 | 6 | – |
| 7 | Ordering and checkout | Validate checkout, final totals, create order, confirmation, history | `Orders`, `OrderLines` | [07-ordering-checkout.md](07-ordering-checkout.md) | 1 | 3 | 1 | – | – |
| 8 | Payment | Card validation and charging (simulated) | card number and expiry (wrongly on `Orders`) | [08-payment.md](08-payment.md) | 1 | 2 | – | – | – |
| 9 | Order fulfilment | Order list and filter; status transitions | `Orders.Status` | [09-order-fulfilment.md](09-order-fulfilment.md) | 3 | – | – | – | – |

### Supporting

| # | Boundary | Responsibility | Data it owns | File | S | B | CS | FE | T |
|---|---|---|---|---|---|---|---|---|---|
| 10 | Identity and access | Register, log in and out, hashing, current user, admin role, route protection | `Users` (credentials), Session keys | [10-identity-access.md](10-identity-access.md) | 5 | 1 | 2 | – | – |
| 11 | Customer profile | Name, address, email; checkout prefill | `Users` (profile columns) | [11-customer-profile.md](11-customer-profile.md) | – | 1 | – | – | – |
| 12 | Pet content management (admin) | Create and edit pets, upload, featured, delete, React admin table | `Pets`, `wwwroot/uploads` | [12-pet-content-admin.md](12-pet-content-admin.md) | 4 | – | – | 1 | – |
| 13 | Notifications | Confirmation and welcome emails (simulated) | `emails.log` | [13-notifications.md](13-notifications.md) | 1 | – | 1 | – | – |
| 14 | Search analytics | Record and show recent searches | `MvcApplication.RecentSearches` | [14-search-analytics.md](14-search-analytics.md) | 1 | – | – | – | – |
| 15 | Audit, logging and errors | Audit trail, error capture, exception handling | `AuditLog`, `petstore.log`, `LastError` | [15-audit-logging-errors.md](15-audit-logging-errors.md) | – | – | 3 | – | – |
| 16 | Configuration and settings | Tax, shipping, maintenance mode, admin email, connection strings, secrets | `Settings`, `appsettings.json`, statics | [16-configuration-settings.md](16-configuration-settings.md) | 1 | – | 1 | – | 1 |
| 17 | Reporting / dashboard | Counts and revenue | reads Pets/Orders/Users | [17-reporting-dashboard.md](17-reporting-dashboard.md) | – | 1 | – | – | – |

### Technical

| # | Boundary | Responsibility | File | S | B | CS | FE | T |
|---|---|---|---|---|---|---|---|---|
| 18 | Data access / persistence | Schema, seeding, connections, query patterns, storage formats | [18-data-access-persistence.md](18-data-access-persistence.md) | 1 | 2 | 6 | – | 2 |
| 19 | Web shell / presentation | Layout, routing, bundling, views, CSRF, error pages | [19-web-shell-presentation.md](19-web-shell-presentation.md) | 2 | 1 | 6 | 1 | – |
| 20 | Client application | React islands, build pipeline, jQuery | [20-client-application.md](20-client-application.md) | – | – | – | 11 | 1 |
| 0 | Cross-cutting | Issues that span every boundary (god class, statics, no DI, tests) | [00-cross-cutting.md](00-cross-cutting.md) | – | – | 9 | – | 3 |

---

## 3. Where the boundaries leak most

1. **Pricing (5)** is computed in `Utils`, `CartApiController`, `CheckoutController`, Razor views and `money.js`. Cart, API and checkout totals disagree (B-03).
2. **Cart (6)** has three owners: `Utils` cart functions, `CartApiController`, and the Redux store. Session and Redux drift apart (B-17).
3. **Inventory (4)** is split across Cart (check when adding), Checkout (decrement) and Admin, with no single owner and no transaction (B-05, B-06, B-07).
4. **Identity (10)** checks are spread across the filter attribute, manual session reads, `Utils.IsAdmin`, the layout and `window.APP_CONFIG`.
5. **Catalog Pets (1)** and **Catalog Supplies (2)** are parallel copies of one "Product" concept (CS-08, CS-12).
6. **Payment (8)** data lives inside **Ordering (7)**: the full card number is stored on `Orders` (S-16).
7. **Category (3)** deletion reaches into **Catalog (1)** through `ON DELETE CASCADE` (B-12). Supplies only reference categories by a bare `CatId`.

## 4. Suggested target modules

| Target module | Absorbs boundaries |
|---|---|
| **Catalog** (unified `Product`, Pet and Supply as variants) | 1, 2, 3, 12 |
| **Inventory** | 4 |
| **Pricing** (single source of truth, decimal) | 5, plus the pricing parts of 16 |
| **Cart** (single server-side owner; frontend only displays) | 6 |
| **Ordering** | 7, 9, 17 |
| **Payments** (behind `IPaymentGateway`; never store the full card number) | 8 |
| **Identity** (ASP.NET Core Identity) | 10, 11 |
| **Notifications** | 13 |
| **Platform / shared** (logging, config, audit) | 14, 15, 16, 18, 19 |
| **Frontend** (single build, typed, server-driven totals) | 20 |

---

## 5. Quick repro table

| What | How | ID |
|---|---|---|
| Run | `cd legacy && dotnet run --project PetStore.Web` → http://localhost:5080 | |
| Reset DB | delete `legacy/PetStore.Web/petstore.db` | |
| Login as admin with no password | username `admin' --`, any password | S-01 |
| Make yourself admin | POST `/Account/Register` with extra field `IsAdmin=1` | S-05 |
| Delete any pet anonymously | GET `/Admin/DeletePet/20` (not logged in) | S-06 |
| See another customer's order | log in as msmith → `/Checkout/Confirmation/1` | S-09 |
| Reflected XSS | `/Pets?search=<script>alert(1)</script>` | S-11 |
| Negative stock | add Dog Leash (stock 3) → React cart qty 50 → checkout → `Stock = -47` | B-05 |
| Totals mismatch | compare React cart total, the "Estimated total" line and the checkout total | B-03 |
| Float leak | checkout shows `Tax (7.000000000000001%)`, tax `153.82500000000002` | B-02 |
| Lost pets | `/Pets`: 20 pets, page size 6 → 3 pages; pets 19–20 unreachable | B-09 |
| Category delete wipes pets | admin deletes "Reptiles" → Spike, Shelly, Kaa are gone | B-12 |

## 6. Suggested modernisation path

1. **Safety net first:** characterisation tests around checkout totals, cart and auth (pin current behaviour, including bugs you decide to keep).
2. **Stop the bleeding:** parameterise SQL (S-01–S-04), view models (S-05), authorisation policy (S-06–S-08), remove `Html.Raw` (S-10–S-13), Identity/PBKDF2 (S-15), never store the full card number (S-16), anti-forgery (S-18), user-secrets (S-19).
3. **Introduce seams:** DI for DbContext, `IClock`, `ICartService`, `IPricingService`, `IPaymentGateway`; strangle `Utils.cs` region by region.
4. **Single source of truth for pricing:** one `PricingService` (decimal); the API returns totals and React displays them.
5. **Data:** EF migrations baselined from the current schema (including `AuditLog`/`Settings`), real `DateTime`/`decimal` columns, transactional checkout with optimistic concurrency on stock.
6. **Frontend:** single build (Vite), remove in-browser Babel, hooks + Redux Toolkit (or RTK Query instead of sagas), TypeScript, remove the jQuery cart code.
7. **Unify the Pet/Supply catalog** behind a `Product` abstraction.
