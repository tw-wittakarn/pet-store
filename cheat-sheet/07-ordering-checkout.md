# 07: Ordering and Checkout

> ⚠️ Cheat sheet (spoiler). Back to the [index](README.md). Status: ✅ reproduced · 📖 by construction. Line numbers match commit `cfb038c`.

| | |
|---|---|
| **Responsibility** | Checkout form, validation, final totals, order + lines creation, confirmation page, customer order history, order numbers |
| **Data owned** | `Orders`, `OrderLines` |
| **Code today** | `legacy/PetStore.Web/Controllers/CheckoutController.cs` (`Index`, `PlaceOrder` ~110 lines, `Confirmation` :162), `Controllers/AccountController.cs` (`MyOrders` :94), `Helpers/Utils.cs` (`GenerateOrderNumber` :950, `EstimatedDelivery`), `Models/Order.cs`, `Views/Checkout/*`, `Views/Account/MyOrders.cshtml` |
| **Target module** | Ordering |

## Issues

| ID | ✓ | Issue | Location | Notes / repro |
|---|---|---|---|---|
| S-09 | ✅ [ ] | **IDOR on order confirmation** | `legacy/PetStore.Web/Controllers/CheckoutController.cs:162` | No ownership check: msmith can view jdoe's order (name, address, masked card) at `/Checkout/Confirmation/1`. Anonymous users can too. |
| B-08 | 📖 [ ] | **Card charged before the order is persisted; no rollback** | `legacy/PetStore.Web/Controllers/CheckoutController.cs:101` (charge) then `:139` (`SaveChanges`) then `:146-151` (stock) | If anything after the charge throws, the global filter swallows it and redirects home, and the customer is charged with no order. EF and raw ADO are mixed, with no transaction. |
| B-10 | ✅ [ ] | **Order history sorted by a string date** | `legacy/PetStore.Web/Controllers/AccountController.cs:94`, `Models/Order.cs:10` | `dd/MM/yyyy HH:mm` strings sort lexicographically: shown order was `28/12/2025, 12/11/2026, 05/01/2026`. |
| B-25 | 📖 [ ] | Two order-number formats + a side effect | `legacy/PetStore.Web/Helpers/Utils.cs:950` (`PS-yyMMdd-00001`, also increments `OrdersPlacedSinceStartup`) vs `Controllers/CheckoutController.cs:166` (`PS-00001`) | The email and the confirmation page show different order numbers, and the date part is the *email* date, not the order date. |
| CS-06 | 📖 [ ] | **Fat controller action** | `CheckoutController.PlaceOrder` | Validation, pricing, stock, payment, persistence, inventory, email, logging and cart clearing all in one action. Validation is hand-rolled into `ViewBag.Errors` (no ModelState or DataAnnotations). |

## Also affected by
- **B-03** (05): checkout charges a different total from the cart.
- **B-05 / B-06 / B-07** (04): stock and sold checks are incomplete or racy.
- **S-16** (08): the full card number and expiry are stored on `Orders`.
- **S-13** (09): the checkout "Notes" field is rendered raw to admins.
- **B-21** (11): checkout crashes if the session user no longer exists.
- **CS-17** (10): login is checked manually here (twice) instead of with `[MustLogin]`.
- **CS-21** (18): MyOrders loads lines per order (N+1, `AccountController.cs:97`).
- **B-11** (18): `OrderDate` uses `DateTime.Now` stored as a string.
