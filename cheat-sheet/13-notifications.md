# 13: Notifications

> ⚠️ Cheat sheet (spoiler). Back to the [index](README.md). Status: ✅ reproduced · 📖 by construction. Line numbers match commit `cfb038c`.

| | |
|---|---|
| **Responsibility** | Order confirmation and welcome emails (simulated) |
| **Data owned** | `emails.log` in the process working directory |
| **Code today** | `legacy/PetStore.Web/Helpers/Utils.cs` (`SendEmail` :956, `SendOrderConfirmation` ~:966), `Controllers/AccountController.cs:85`, `Controllers/CheckoutController.cs` |
| **Target module** | Notifications (behind `IEmailSender`, asynchronous/outbox) |

## Issues

| ID | ✓ | Issue | Location | Notes / repro |
|---|---|---|---|---|
| S-20 | 📖 [ ] | Plain-text password emailed | `legacy/PetStore.Web/Controllers/AccountController.cs:85` | The welcome email says "your password is …" and is written to `emails.log`. |
| CS-28 | 📖 [ ] | Email "sending" appends to a file in the working directory | `legacy/PetStore.Web/Helpers/Utils.cs:956` | Synchronous, swallows errors, "TODO: hook up SMTP (password is in appsettings)". |

## Also affected by
- **S-16** (08): the confirmation email body includes the **full card number** (`Utils.cs:972`).
- **B-25** (07): the email's order number differs from the confirmation page's.
- **S-19** (16): the SMTP password is in `appsettings.json`.
- **T-07** (18): `emails.log` is written into the content root.
