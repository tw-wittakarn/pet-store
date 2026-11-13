# 08: Payment

> ⚠️ Cheat sheet (spoiler). Back to the [index](README.md). Status: ✅ reproduced · 📖 by construction. Line numbers match commit `cfb038c`.

| | |
|---|---|
| **Responsibility** | Card validation, charging (simulated gateway), masking and storing payment data |
| **Data owned** | Card number and expiry, **wrongly stored on `Orders`** (should never be stored) |
| **Code today** | `legacy/PetStore.Web/Helpers/Utils.cs` (`IsValidCard` :731, `IsValidExpiry`, `ChargeCard` :769, `MaskCard`, `EncryptCard` :89 unused, `DecryptCard`, commented-out PayPal :1020+), `Controllers/CheckoutController.cs` (:101, :122) |
| **Target module** | Payments (behind `IPaymentGateway`; tokenised, never store the full card number) |

## Issues

| ID | ✓ | Issue | Location | Notes / repro |
|---|---|---|---|---|
| S-16 | ✅ [ ] | **Full card number stored in plain text** | `legacy/PetStore.Web/Controllers/CheckoutController.cs:122`, `Models/Order.cs:20` | It's shown in full on the admin orders page (`Views/Admin/Orders.cshtml:21`) and written to `emails.log` (`Utils.cs:972`). Expiry is stored too. `EncryptCard` (just base64) exists but is never used. The CVV posts back into the form on validation errors. |
| B-24 | 📖 [ ] | No Luhn check | `legacy/PetStore.Web/Helpers/Utils.cs:731` | `IsValidCard` only checks 13–19 digits. |
| B-31 | 📖 [ ] | Gateway simulation blocks the thread; naive approval | `legacy/PetStore.Web/Helpers/Utils.cs:769-774` | `Thread.Sleep(500)` on a request thread; approval rule is `StartsWith("4") \|\| StartsWith("5")`; `4000000000000002` is declined. The amount is ignored. |

## Also affected by
- **B-08** (07): the charge happens before the order is saved, with no compensation.
- **B-11** (18): `IsValidExpiry` compares against server local time.
- **S-19** (16): PayPal credentials in commented-out code (`Utils.cs:1026`) and a "live" payment key in `appsettings.json`.
- **CS-36** (00): the dead PayPal integration code.
