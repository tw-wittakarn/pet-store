# 10: Identity and Access

> ⚠️ Cheat sheet (spoiler). Back to the [index](README.md). Status: ✅ reproduced · 📖 by construction. Line numbers match commit `cfb038c`.

| | |
|---|---|
| **Responsibility** | Register, log in, log out; password hashing; current user; admin role; route protection |
| **Data owned** | `Users` (credential columns: `Username`, `PasswordHash`, `IsAdmin`), Session keys `UserId`, `UserName`, `FullName`, `IsAdmin`, `Email` |
| **Code today** | `legacy/PetStore.Web/Controllers/AccountController.cs`, `Helpers/Utils.cs` security + session regions (`Md5` :45, `HashPassword`, `IsAdmin` :143, `SetLogin` :150, `ClearLogin` :160, `GenerateToken`), `Filters/AdminOnlyAttribute.cs` (`AdminOnly` + `MustLogin`), manual session checks in `CheckoutController`, `Views/Shared/_Layout.cshtml`, `window.APP_CONFIG`, `Program.cs` (session cookie) |
| **Target module** | Identity (ASP.NET Core Identity + policies) |

## Issues

| ID | ✓ | Issue | Location | Notes / repro |
|---|---|---|---|---|
| S-01 | ✅ [ ] | **SQL injection: login bypass** | `legacy/PetStore.Web/Controllers/AccountController.cs:29` | Username concatenated into SQL. `admin' --` with any password logs in as admin. |
| S-05 | ✅ [ ] | **Mass assignment → privilege escalation** | `legacy/PetStore.Web/Controllers/AccountController.cs:68` | `Register(User user, ...)` binds the whole entity, including `IsAdmin`, `Id` and `PasswordHash`. POST `IsAdmin=1` creates an admin (verified: `/Admin` → 200). |
| S-15 | ✅ [ ] | **Unsalted MD5 password hashing** | `legacy/PetStore.Web/Helpers/Utils.cs:45`, `:62` ("TODO: salt (ticket #1123)") | `admin123` = `0192023a7bbd73250516f069df18b500`, trivial with rainbow tables. Minimum password length is 4 (`Utils.cs:763`). |
| S-17 | ✅ [ ] | Open redirect | `legacy/PetStore.Web/Controllers/AccountController.cs:50`, `Controllers/CartController.cs:37` | `returnUrl=https://evil.example` is followed blindly. The cart also redirects to the `Referer` header. |
| S-21 | 📖 [ ] | Session and auth weaknesses | `legacy/PetStore.Web/Program.cs:20` (`HttpOnly = false`), `Utils.SetLogin` (`Utils.cs:150`), `Filters/AdminOnlyAttribute.cs:12` | No session ID regeneration on login (fixation). The admin flag is a session string with duplicated `"1"`/`"True"` checks (filter + `Utils.IsAdmin`). No HTTPS, lockout or rate limiting. The email and admin flag are leaked into `window.APP_CONFIG` (`Views/Shared/_Layout.cshtml:25`, `Utils.cs:1004`). |
| B-23 | 📖 [ ] | Email regex rejects valid addresses | `legacy/PetStore.Web/Helpers/Utils.cs:721` | TLDs are limited to 2–4 lowercase letters: `.museum`, `.travel`, and upper-case `.COM` are rejected. |
| CS-17 | 📖 [ ] | Authorisation checks scattered and duplicated | `CheckoutController` (manual session checks, twice), `MustLoginAttribute` (copy-paste of `AdminOnly`), `Utils.IsAdmin`, `_Layout.cshtml` | The role is a session string; there are no claims or policies. |
| CS-30 | 📖 [ ] | Shared static `Random` for "tokens" | `legacy/PetStore.Web/Helpers/Utils.cs:39` (`GenerateToken`) | Not thread-safe and not cryptographically secure. |

## Also affected by
- **S-06 / S-07 / S-08** (12, 09): admin actions that forgot `[AdminOnly]`.
- **S-20** (13): the welcome email contains the plain-text password.
- **S-19** (16): admin password in `appsettings.json`; test credentials in an HTML comment (`Views/Account/Login.cshtml:15`).
- **CS-15** (19): the `User` entity is used as the binding model (the cause of S-05).
- **B-28** (06): logout keeps the cart.
