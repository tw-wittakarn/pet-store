# 16: Configuration and Settings

> ⚠️ Cheat sheet (spoiler). Back to the [index](README.md). Status: ✅ reproduced · 📖 by construction. Line numbers match commit `cfb038c`.

| | |
|---|---|
| **Responsibility** | Tax rate, shipping fee, free-shipping threshold, maintenance mode, admin email, site name, connection strings, secrets |
| **Data owned** | `Settings` table (`seed.sql`, not in the EF model), `appsettings.json`, constants in `Utils`, static fields in `MvcApplication`, constants in `money.js` |
| **Code today** | `legacy/PetStore.Web/Helpers/Utils.cs` (`GetSetting` :472, `SetSetting` :485, `IsMaintenanceMode`, constants :28–37), `Global.asax.cs`, `DAL/PetStoreContext.cs:21`, `appsettings.json`, `appsettings.Development.json` (unused), `ClientApp/src/utils/money.js` |
| **Target module** | Platform / shared (`IOptions<T>`, user-secrets / key vault) |

## Issues

| ID | ✓ | Issue | Location | Notes / repro |
|---|---|---|---|---|
| S-19 | 📖 [ ] | **Secrets committed in config and code** | `legacy/PetStore.Web/appsettings.json:13-15` (admin password, "live" payment key `sk_live_…`, SMTP password); `Helpers/Utils.cs:1026` (PayPal credentials in dead code); `Views/Account/Login.cshtml:15` (test credentials in an HTML comment sent to every visitor) | |
| CS-20 | 📖 [ ] | Connection string hard-coded twice; the configured one differs and is unused | `legacy/PetStore.Web/DAL/PetStoreContext.cs:21`, `Global.asax.cs` (`ConnectionString`), `appsettings.json:10` (`App_Data/PetStore.sqlite`, never read) | Comment: "that one didn't work on the build server". |
| T-06 | 📖 [ ] | No per-environment config, health checks or structured logging | `appsettings.Development.json` unused; nothing reads `IConfiguration` | |

## Also affected by
- **B-04** (05): four or five competing sources of the tax rate (`Settings` 0.08, statics 0.07, `appsettings` 0.075, JS 0.07).
- **S-04** (18): `GetSetting`/`SetSetting` concatenate SQL.
- **CS-03** (00): settings live in mutable static fields.
- **CS-24** (00): magic numbers duplicated between C# and JS.
