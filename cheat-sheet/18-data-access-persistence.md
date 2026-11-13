# 18: Data Access and Persistence

> ⚠️ Cheat sheet (spoiler). Back to the [index](README.md). Status: ✅ reproduced · 📖 by construction. Line numbers match commit `cfb038c`.

| | |
|---|---|
| **Responsibility** | Schema creation, seeding, connections, query patterns, storage formats for dates and money |
| **Code today** | `legacy/PetStore.Web/DAL/PetStoreContext.cs` (EF Core), `App_Start/DatabaseConfig.cs` (static-constructor seeding), `App_Data/seed.sql`, `Helpers/Utils.cs` raw DB region (:416–586: `GetConnection`, `ExecuteSql`, `ExecuteScalar`, `GetDataTable`, `Esc`, `Query`, `RowToPet`), every controller (`new PetStoreContext()`) |
| **Target module** | Platform / shared (EF Core with migrations, repositories or query services, DI-scoped DbContext) |

## Issues

| ID | ✓ | Issue | Location | Notes / repro |
|---|---|---|---|---|
| S-04 | 📖 [ ] | SQL injection-prone raw helpers | `legacy/PetStore.Web/Helpers/Utils.cs:472`, `:485` (`GetSetting`/`SetSetting` concatenate key and value); every `Utils.GetDataTable("... " + id)` (`AddToCart` :211, :231; stock helpers ~:523–548) | Ints are currently safe, but the pattern invites injection. `Utils.Esc` (quote doubling) is used inconsistently. There are no parameters anywhere in the raw layer. |
| B-11 | 📖 [ ] | **Local time and string dates everywhere** | `DateTime.Now` in `Utils.cs` (`FormatDate`, `Now`, `TimeAgo`, `IsValidExpiry`, `EstimatedDelivery`, `Log` :495), `Models/Pet.cs:47` (`IsNew`), `GenerateOrderNumber` | No UTC. Dates are stored as strings in three formats (`dd/MM/yyyy`, `dd/MM/yyyy HH:mm`, culture-dependent `ToString()`). "NEW" badges and card expiry depend on server time and timezone. |
| B-29 | 📖 [ ] | Seed loader splits on `;` | `legacy/PetStore.Web/App_Start/DatabaseConfig.cs:31` | Any seed text containing `;` breaks seeding. Failures are swallowed (`:50`) and the site runs with an empty catalog. |
| CS-05 | 📖 [ ] | **Two data-access styles, sometimes in the same action** | EF Core vs string-built ADO.NET: `AdminController.SaveCategory` (raw INSERT, EF UPDATE); `CheckoutController.PlaceOrder` (EF order, raw stock); `PetsController` (raw) vs `SuppliesController` (EF); Home (both) | No unit of work or transaction spans them. |
| CS-18 | 📖 [ ] | **No migrations; part of the schema is invisible to EF** | `App_Start/DatabaseConfig.cs` (`EnsureCreated`), `App_Data/seed.sql:3-4` (`AuditLog`, `Settings`) | The schema can't evolve; nobody can trace where the tables come from. |
| CS-19 | 📖 [ ] | Seeding in a static constructor | `legacy/PetStore.Web/App_Start/DatabaseConfig.cs:14` | Triggered by an empty `Initialize()` call; runs once per process (also in tests); exceptions are swallowed. |
| CS-21 | 📖 [ ] | N+1 queries | `Controllers/HomeController.cs:36` (count per category), `Controllers/AccountController.cs:97` (lines per order), `Controllers/AdminController.cs:195` (user per order), `Views/Admin/Supplies.cshtml` (`GetCategoryName` per row; cache masks it) | |
| CS-22 | 📖 [ ] | Loading whole tables into memory | `Controllers/PetsController.cs:91` (`db.Pets.ToList().Where`), `Controllers/SuppliesController.cs:19`, Home low-stock, `AdminController.Orders` (filter in memory), `AdminController.Supplies` | |
| CS-23 | 📖 [ ] | Dates and ages stored as strings | `Models/Pet.cs:20` (`Age`), `:34` (`CreatedDate`), `Models/Order.cs:10` (`OrderDate`) | The cause of B-10, B-11 and FE-20. |
| T-02 | 📖 [ ] | Brittle DB test | `legacy/PetStore.Tests/DatabaseTests.cs:24` (`Assert.AreEqual(20, …)`) | Creates a real `petstore.db` in the test bin folder via the static seeding; depends on the working directory; state persists between runs (a deleted pet in a reused DB breaks it). |
| T-07 | 📖 [ ] | Runtime files written into the content root | `petstore.db`, `emails.log`, `petstore.log`, `wwwroot/uploads` | The app can't run read-only or in containers without changes. |

## Also affected by
- **B-01** (05): `REAL` vs `TEXT` (decimal) money columns.
- **B-12** (03): the `ON DELETE CASCADE` created by `EnsureCreated`.
- **CS-04** (00): `new PetStoreContext()` field initialisers, never disposed.
- **CS-20** (16): connection string hard-coded.
