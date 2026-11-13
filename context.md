# Context: Paws & Claws Legacy Pet Store

This file records **what was built, why, and every decision behind it**. Read it before working in this repo, whether you're a person or an AI agent.

> ⚠️ **`PETSTORE_ANSWER_KEY.md` is the cheat sheet.** It lists every deliberately planted security hole, bug and code smell, with file and line numbers, a repro and a checkbox. If you're practising modernisation, **don't open it until you've done your own analysis**, then use it to compare and track progress.

---

## 1. Purpose

A **deliberately bad legacy codebase** for practising **legacy modernisation**. It's meant to look like a real app that grew from 2009 to 2021 under different teams:

- logic scattered across controllers, views, static helpers and JavaScript
- very low test coverage
- full of code smells, real security vulnerabilities and subtle logic bugs

The "bad" code is **intentional**. Don't "fix" things by accident while doing unrelated work. Every fix should be a conscious modernisation step that you can trace back to an entry in the cheat sheet.

Owner: x.keeratichayakorn@jetstar.com. Created 2026-11-13 with Claude Code (Opus 5.5).

---

## 2. Repository layout

```
pet-store/
├── context.md                 ← this file (background, decisions, scope)
├── PETSTORE_ANSWER_KEY.md     ← CHEAT SHEET: all planted issues (spoiler)
├── .gitignore
└── legacy/                    ← the legacy application
    ├── PetStore.sln
    ├── README.md              ← "in-universe" README, written as if by the original team
    ├── PetStore.Web/          ← ASP.NET Core MVC (.NET 9) in MVC 5 style
    │   ├── Global.asax.cs     ← fake MvcApplication: Application_Start + global static state
    │   ├── App_Start/         ← RouteConfig, BundleConfig, FilterConfig, DatabaseConfig (seeding)
    │   ├── App_Data/seed.sql  ← seed data + extra tables (AuditLog, Settings) EF doesn't know about
    │   ├── Controllers/       ← Home, Pets, Supplies, Cart, CartApi, Checkout, Account, Admin
    │   ├── DAL/               ← PetStoreContext (EF Core, hard-coded connection string)
    │   ├── Filters/           ← AdminOnly / MustLogin (session-based)
    │   ├── Helpers/           ← Utils.cs (1,073-line god class), HttpContextHelper (HttpContext.Current clone)
    │   ├── Models/            ← Pet, Supply, Category, User, Order, OrderLine, CartItem, view models
    │   ├── Views/             ← Razor views (logic in views, Html.Raw, ViewBag everywhere)
    │   ├── ClientApp/         ← React 16 + Redux 4 + redux-saga 1, webpack 4 + Babel 7 (cart page + mini-cart)
    │   └── wwwroot/
    │       ├── Content/       ← vendored Bootstrap 3.4.1 css, Site.css
    │       ├── Scripts/       ← vendored jQuery 1.12.4, jquery.validate, bootstrap.js, site.js
    │       │   ├── vendor/    ← UMD React/ReactDOM/Redux/ReactRedux/ReduxSaga + babel-standalone
    │       │   ├── admin/petTable.jsx  ← admin table compiled in the browser by Babel
    │       │   └── dist/cart.bundle.js ← committed webpack output (deliberately out of date with source)
    │       ├── images/        ← generated placeholder SVGs for pets and supplies
    │       └── uploads/       ← admin image uploads (unrestricted, on purpose)
    └── PetStore.Tests/        ← MSTest: UtilsTests (6), DatabaseTests (2 + 1 [Ignore]d)
```

Git: a single repo at `pet-store/`. Baseline commit **`cfb038c`** ("Initial commit"), when the project was still at the repo root. It was later moved under `legacy/` with no content changes, so the cheat-sheet line numbers still match `cfb038c`.

---

## 3. How to run

```bash
cd legacy
dotnet build                          # 0 warnings (warnings are suppressed on purpose, NoWarn)
dotnet test                           # 8 passed, 1 skipped
dotnet run --project PetStore.Web     # http://localhost:5080
```

- DB: `legacy/PetStore.Web/petstore.db` is created and seeded on first run. **Delete it to reset.** (It's git-ignored.)
- Accounts: `admin`/`admin123` (admin), `jdoe`/`password`, `msmith`/`password`
- Test cards: anything starting with `4` or `5` is approved; `4000000000000002` is declined. Expiry is `MM/YY`, CVV is 3+ digits, postcode is 4 digits.
- Rebuild the React cart: `cd legacy/PetStore.Web/ClientApp && npm install && npm run build`. The script sets `NODE_OPTIONS=--openssl-legacy-provider` because webpack 4 breaks on Node 17+. ⚠️ Rebuilding **removes** the planted "stale bundle" smell (FE-03).
- Runtime files written into the content root: `petstore.db`, `emails.log`, `petstore.log`, `wwwroot/uploads/*` (all git-ignored).

### Environment this was built on
- macOS (Apple Silicon), .NET SDK 9.0.306, Node 20.18.1 / npm 11.6.1
- No Mono, MSBuild, IIS or Docker. That's why real .NET Framework 4.8 was ruled out (see Q1).
- NuGet: Microsoft.EntityFrameworkCore.Sqlite 9.0.10, Microsoft.Data.Sqlite 9.0.10, Newtonsoft.Json 13.0.3
- npm: react 16.14.0, react-dom 16.14.0, redux 4.0.5, react-redux 7.2.0, redux-saga 1.1.3, @babel/polyfill 7.12.1, webpack 4.46.0, webpack-cli 3.3.12, babel-loader 8.2.2, @babel/core 7.12.10, preset-env 7.12.11, preset-react 7.12.10
- Vendored UMD: same React/Redux/Saga versions + @babel/standalone 7.12.17

---

## 4. Capabilities and boundaries (agreed scope)

### Capabilities (in scope)
1. **Catalog:** browse pets by category (Dogs, Cats, Birds, Fish, Reptiles), pet details, search, filter by category and price range, sort, paging. **Supplies** catalog (food, toys, gear) with stock levels, filter, search and sort.
2. **Shopping cart:** add, remove and change quantity. Pets are one-of-a-kind (qty always 1); supplies have stock counts. The cart is stored in the server **Session** (JSON). It's edited by jQuery ajax forms on catalog pages (`/Cart/Add`) **and** by the React cart page via `/api/cart` (JSON API).
3. **Checkout:** login required. Shipping details, **simulated** card payment, order + order lines saved, pets marked sold, supply stock decremented, confirmation page, "confirmation email" appended to `emails.log`.
4. **Accounts:** register, log in and log out with **hand-rolled auth**: `Users` table, unsalted MD5, user identity and `IsAdmin` flag stored in Session. Order history ("My orders").
5. **Admin back office** (`[AdminOnly]` session filter, which some actions forget):
   - dashboard: counts, revenue, configured tax rate, recent searches, last error, audit log
   - pets: React table (filter, sort, toggle sold), create/edit with image upload, delete
   - categories: create, edit, delete
   - inventory: supply stock, price and active flag
   - orders: list, filter by status, change status
6. **Data:** SQLite via a **mix** of EF Core and raw ADO.NET with string-concatenated SQL. Schema comes from `EnsureCreated()` + `seed.sql` run from a static constructor. **No migrations.**
7. **Legacy flavour:** MVC 5 conventions recreated on ASP.NET Core (`Global.asax.cs`, `App_Start/`, `Content/` and `Scripts/` folders, `HttpContextHelper.Current`, `ViewBag` everywhere, giant `Utils.cs`, global static state).
8. **React islands (a half-finished migration):** the cart page + header mini-cart (webpack bundle) and the admin pet table (in-browser Babel). jQuery and React both manipulate cart state, so the two can drift apart.

### Boundaries (out of scope by design)
- No real payment gateway, email sending or shipping integration (all simulated).
- **No full SPA:** only React islands mounted into Razor pages.
- No cloud deployment, CI/CD or containerisation.
- No multi-language or multi-currency support.
- One web project + one test project. No enterprise layering (on purpose).
- Local use only. The security vulnerabilities are deliberate training material and must never be deployed.

---

## 5. Decision log (from the planning "grill" session)

| # | Question | Decision |
|---|---|---|
| Q1 | Target runtime: real .NET Framework 4.8 MVC 5 (Windows only), SDK-style net48, or ASP.NET Core in legacy style? | **ASP.NET Core MVC on .NET 9, written legacy-style.** Real .NET Framework can't run on this Mac. |
| Q2 | Purpose | **Practising legacy modernisation.** Code must be scattered, smelly and barely tested. |
| Q3 | Capability/boundary list | **Accepted as proposed** (all 7 capabilities; Accounts + Admin built together). |
| Q4 | Data store | **SQLite** (instead of the suggested LocalDB). |
| Q5 | Tests | **MSTest, a few only**, not full coverage (consistent with Q2). |
| Q6 | Data access style | **Mixed and inconsistent:** EF Core in some places, raw `Microsoft.Data.Sqlite` with concatenated SQL in others, sometimes in the same action. |
| Q7 | Authentication | **Hand-rolled:** Users table, unsalted MD5, Session-based login, custom `[AdminOnly]` filter that some admin actions forget. |
| Q8 | Deliberate security vulnerabilities | **Yes** (SQLi, XSS via `Html.Raw`, unrestricted upload, GET deletes without anti-forgery, and more). |
| Q9 | Deliberate logic bugs | **Yes** (`double` money, non-atomic stock, cart vs checkout totals, `DateTime.Now`/timezones, and more). |
| Q10 | Answer key | **Yes, separate from the code.** Originally in `~/temp`; now `pet-store/PETSTORE_ANSWER_KEY.md` (outside `legacy/`). |
| Q11 | Legacy MVC 5 structure | **Yes:** `App_Start/`, `Content/`, `Scripts/`, `HttpContextHelper.Current`, `ViewBag`, 1,000+ line `Utils.cs`, static `MvcApplication` state. |
| Q12 | Which tests | **Realistic poor tests:** a handful of `Utils` tests, one brittle DB-dependent test, one `[Ignore]` "fix later" test. Nothing on controllers or checkout. |
| Q13 | Git | **`git init` + a single initial commit** as the baseline to diff refactors against. |
| Q14 | Naming | `PetStore.sln`, `PetStore.Web`, `PetStore.Tests`, root namespace `PetStore`. Store brand: "Paws & Claws Pet Store". |
| Q15 | What's sold | **Pets (one-off) and supplies (stock)** as near-duplicate models and code paths (`Pet` vs `Supply`), ready for an "extract a Product abstraction" exercise. |
| Q16 | Seed data and images | 5 categories, 20 pets, 15 supplies, admin + 2 customers; **locally generated placeholder SVGs**. |
| Q17 | Frontend libraries | **Vendored** Bootstrap 3.4 + jQuery 1.12 + jQuery Validation, **plus React, Redux and Redux Saga** (added by the user). |
| Q18 | DB creation | **Auto create + seed on first run** via `EnsureCreated()` + `seed.sql` from a static constructor. No migrations. |
| Q19 | Where React lives | **Half-finished migration "islands":** React cart page + mini-cart calling `CartApiController`, while jQuery buttons still post to MVC `CartController` (Session and Redux drift). Admin pet list in React; admin edit form stays Razor. |
| Q20 | React era and style | **React 16 class components** (legacy lifecycles), **Redux 4** with hand-written constants, `connect()`, one giant reducer, **redux-saga** with `takeEvery`, no cancellation. No hooks, RTK or TypeScript. |
| Q21 | Build pipeline | **Both:** webpack 4 + Babel 7 for the cart (built bundle committed and slightly stale), in-browser `babel-standalone` for the admin table ("build was broken that week"). |

---

## 6. What was verified during the build

- `dotnet build`: 0 warnings, 0 errors. `dotnet test`: 8 passed, 1 skipped.
- All public pages, admin pages, static assets and the bundle returned HTTP 200.
- Real browser check (playwright-cli): the React cart and mini-cart render. The admin React table renders with in-browser Babel. The console shows the expected legacy warnings (in-browser Babel, `componentWillMount`).
- Planted issues reproduced live (✅ in the cheat sheet), including:
  - SQLi login bypass (`admin' --`) and boolean SQLi in search
  - mass-assignment `IsAdmin=1` at register gives admin access
  - anonymous `DeletePet` and `UpdateOrderStatus`; anonymous `PetsJson` leak
  - IDOR on order confirmation; reflected XSS in search; open redirect
  - stock driven to **-47**; full card number stored and shown to admin
  - `Tax (7.000000000000001%)` on checkout; pagination hides pets 19–20
  - string-date sort in My Orders; stale category cache
  - **category delete cascades and deletes all its pets** (found during verification: EF `ON DELETE CASCADE` + Microsoft.Data.Sqlite enables foreign keys)
  - legacy route `shop/{categoryId?}` hijacks link generation (`RedirectToAction("Index","Pets")` → `/shop`), also found during verification

Fixed during the build (not a planted issue): the pet placeholder SVGs were generated off-by-one (zsh arrays start at 1) and were regenerated correctly with bash before the commit.

---

## 7. Conventions for future work in this repo

- **Treat `legacy/` as the "before" picture.** Keep it intact as a reference, or modernise it in place on a branch. Either way, diff against `cfb038c`.
- A modernised version, if built, should go in a **sibling folder** (e.g. `pet-store/modern/`) or a branch, so before and after can be compared side by side.
- When fixing an issue, reference its cheat-sheet ID (`S-01`, `B-05`, `CS-12`, `FE-08`, `T-03`) in the commit message and tick its checkbox in `PETSTORE_ANSWER_KEY.md`.
- Write **characterisation tests first** (pin current behaviour, bugs included) before refactoring. The `[Ignore]`d test `Checkout_Total_Matches_Cart_Total` already documents bug B-03 and fails when un-ignored.
- Don't run `npm run build` in `ClientApp` unless you intend to fix FE-03 (stale bundle).
- Don't deploy this anywhere. It's intentionally vulnerable.

---

## 8. Cheat-sheet summary (details in `PETSTORE_ANSWER_KEY.md`)

| Category | Count | Highlights |
|---|---|---|
| Security (S-xx) | 22 | SQLi login + search, mass assignment, missing `[AdminOnly]`, GET deletes / no CSRF, IDOR, stored/reflected XSS, unrestricted upload, MD5, plain-text card numbers, secrets in config |
| Logic bugs (B-xx) | 20 | `double` money, 3–4 different total calculations, 4 tax-rate sources, negative stock, double-sold pets, charge before persist, pagination, string dates, cascade delete, stale cache, fake discount and coupon, Session/Redux drift |
| Backend smells (CS-xx) | 38 | `Utils.cs` god class, global statics, `HttpContext.Current`, no DI, mixed EF/raw SQL, fat controllers, duplicated cart/catalog/HTML code, N+1, no migrations, swallowed exceptions |
| Frontend (FE-xx) | 20 | two React copies, in-browser Babel, stale bundle, webpack 4 OpenSSL hack, legacy lifecycles, reducer mutation, `takeEvery` races, jQuery inside sagas, duplicated pricing logic |
| Tests / infra (T-xx) | 8 | near-zero coverage, brittle DB test, ignored failing test, untestable statics, no CI, runtime files in content root |

The cheat sheet also has a capability/boundary section, a quick repro table and a suggested modernisation path.
