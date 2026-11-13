# 11: Customer Profile

> ⚠️ Cheat sheet (spoiler). Back to the [index](README.md). Status: ✅ reproduced · 📖 by construction. Line numbers match commit `cfb038c`.

| | |
|---|---|
| **Responsibility** | Customer name, email, address, city, postcode; pre-filling checkout |
| **Data owned** | `Users` profile columns (`FullName`, `Email`, `Address`, `City`, `Zip`): the same table and entity as credentials |
| **Code today** | `legacy/PetStore.Web/Controllers/AccountController.cs` (`Register` binds `User`), `Controllers/CheckoutController.cs` (:32–39 prefill), `Models/User.cs`, `Views/Account/Register.cshtml`. There's **no profile edit page**. |
| **Target module** | Identity (separate a `CustomerProfile` from credentials) |

## Issues

| ID | ✓ | Issue | Location | Notes / repro |
|---|---|---|---|---|
| B-21 | 📖 [ ] | Checkout crashes if the session user no longer exists | `legacy/PetStore.Web/Controllers/CheckoutController.cs:32` then `:36` | `db.Users.Find(userId)` isn't null-checked, so `user.FullName` throws a NullReferenceException. The global filter swallows it and redirects home with "Oops!". Happens after a DB reset while the session still holds a `UserId`. |

## Also affected by
- **S-05** (10): profile and credential fields are bound together, so registering can set `IsAdmin`.
- **B-23** (10): the email validation rejects valid addresses.
- **CS-15** (19): the entity doubles as the view model.
- **CS-12** (02): `IsAdmin` is an `int` flag.
- Postcode validation is AU-only (`Utils.IsValidZip`, 4 digits). That's within scope (no multi-region), but worth noting.
