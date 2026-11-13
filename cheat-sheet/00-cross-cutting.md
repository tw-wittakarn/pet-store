# 00: Cross-Cutting Issues (span every boundary)

> ⚠️ Cheat sheet (spoiler). Back to the [index](README.md). Status: ✅ reproduced · 📖 by construction. Line numbers match commit `cfb038c`.

These don't belong to a single boundary: they're the structural reasons the boundaries are so tangled. Fixing them is usually what **creates** the seams the other files need.

## Issues

| ID | ✓ | Issue | Location | Notes |
|---|---|---|---|---|
| CS-01 | 📖 [ ] | **God class `Utils.cs`** (1,073 lines) | `legacy/PetStore.Web/Helpers/Utils.cs` | Regions: Constants, Security, Session/Current User, Cart, Money, Database (raw), HTML builders, Validation, Dates, Files, Strings, Misc, OLD CODE. Touches boundaries 1–19. The comment says to "add new helpers to the bottom of the file". |
| CS-02 | 📖 [ ] | **Ambient context / service locator** | `legacy/PetStore.Web/Helpers/HttpContextHelper.cs` (`Current`), used by `Utils.Session` (`Utils.cs:113`) | Static code depends on the current request, so `Utils` can't be unit-tested or used off-request. |
| CS-03 | 📖 [ ] | **Global mutable static state** | `legacy/PetStore.Web/Global.asax.cs:17` onwards: `RecentSearches`, `CategoryCache`, `OrdersPlacedSinceStartup`, `LastError`, `StartedAt`, plus mutable "constants" `TaxRate`, `ShippingFee`, `ConnectionString` | Not thread-safe, reset on restart, wrong with more than one instance. |
| CS-04 | 📖 [ ] | **No dependency injection** | every controller: `private PetStoreContext db = new PetStoreContext();` (e.g. `Controllers/HomeController.cs:15`) | DbContexts are never disposed; nothing can be swapped for tests. |
| CS-24 | 📖 [ ] | Magic numbers and strings | statuses (`"Paid"`, `"Cancelled"` vs `"cancelled"`), item types `"PET"`/`"SUPPLY"`/`"pet"`, `Active` 0/1/2, `IsAdmin` 1, `PAGE_SIZE` 6 vs hard-coded 8, `MAX_QTY` 99, `0.07`/`9.95`/`100` in C# and JS | |
| CS-29 | 📖 [ ] | Blocking calls, no `async` anywhere | `Thread.Sleep` in `Utils.ChargeCard`, synchronous EF/ADO/file IO in every action | |
| CS-36 | 📖 [ ] | Large "OLD CODE - DO NOT DELETE" commented-out block | `legacy/PetStore.Web/Helpers/Utils.cs:1020` | PayPal, cookie cart migration, old GST, weather widget, Christmas theme. |
| CS-37 | 📖 [ ] | Compiler safety turned off | `legacy/PetStore.Web/PetStore.Web.csproj` (`Nullable` and `ImplicitUsings` disabled, `NoWarn CS0618;CS0168;CS0219`, "TODO: fix warnings one day") | |
| CS-38 | 📖 [ ] | Misleading and fossil comments | `Models/Order.cs:10` says "formatted with Utils.FormatDate" (it's `FormatDateTime`); `DAL/PetStoreContext.cs` "TODO: repository pattern (2016) / DI (2019)"; `Models/Category.cs` "lazy loading is not on" | |
| T-01 | 📖 [ ] | Near-zero test coverage | `legacy/PetStore.Tests/UtilsTests.cs` (6 helper tests) | Nothing on controllers, checkout, cart, auth, API or frontend. |
| T-04 | 📖 [ ] | Untestable design | statics, `HttpContextHelper.Current`, `new DbContext()`, `DateTime.Now`, `Thread.Sleep` | Characterisation (end-to-end/HTTP) tests are the realistic first step. |
| T-05 | 📖 [ ] | No CI, analyzers or linting | repo-wide | Warnings are suppressed (CS-37). |

## Also see
- The **leak map** and **target modules** in the [index](README.md#3-where-the-boundaries-leak-most).
