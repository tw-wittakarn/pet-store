# 15: Audit, Logging and Error Handling

> ⚠️ Cheat sheet (spoiler). Back to the [index](README.md). Status: ✅ reproduced · 📖 by construction. Line numbers match commit `cfb038c`.

| | |
|---|---|
| **Responsibility** | Audit trail (logins, admin actions, orders), error capture, exception handling |
| **Data owned** | `AuditLog` table (created by `seed.sql`, **not in the EF model**), `petstore.log` fallback file, `MvcApplication.LastError` |
| **Code today** | `legacy/PetStore.Web/Helpers/Utils.cs` (`Log` :490), `Global.asax.cs` (`Application_Error`, `LastError`), `App_Start/FilterConfig.cs` (`HandleErrorAttribute`), `Views/Admin/Index.cshtml` (audit and last error), `Views/Shared/Error.cshtml` |
| **Target module** | Platform / shared (`ILogger` + structured logging; a proper audit store) |

## Issues

| ID | ✓ | Issue | Location | Notes / repro |
|---|---|---|---|---|
| CS-25 | ✅ [ ] | **Global exception filter swallows everything** | `legacy/PetStore.Web/App_Start/FilterConfig.cs:27-28` | Every exception becomes "Oops! Something went wrong: {message}" and a redirect to `/`. Hides B-08, B-21, CS-35 and broken links. |
| CS-26 | 📖 [ ] | Empty `catch {}` blocks | throughout `legacy/PetStore.Web/Helpers/Utils.cs` (`Log`, `GetSetting`, `GetCart`, `SendEmail`, `DecryptCard`), `App_Start/DatabaseConfig.cs:50` | Failures disappear silently. |
| CS-27 | 📖 [ ] | DB-based logging with string SQL + file fallback | `legacy/PetStore.Web/Helpers/Utils.cs:490-506` | No `ILogger`. The timestamp uses the culture-dependent `DateTime.Now.ToString()` (:495). It falls back to `petstore.log` in the working directory. |

## Also affected by
- **S-22** (19): stack traces and `LastError` are shown to users and admins; the developer exception page is always on.
- **CS-03** (00): `LastError` is global mutable state.
- **CS-18** (18): `AuditLog` exists only in `seed.sql`.
- **T-06 / T-07** (16, 18): no health checks; log files are written into the content root.
