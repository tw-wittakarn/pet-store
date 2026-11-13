# 04: Inventory

> ⚠️ Cheat sheet (spoiler). Back to the [index](README.md). Status: ✅ reproduced · 📖 by construction. Line numbers match commit `cfb038c`.

| | |
|---|---|
| **Responsibility** | Supply stock levels and active flag; pet sold status; checking, reserving and decrementing stock |
| **Data owned** | `Supplies.Stock`, `Supplies.Active`, `Pets.IsSold` |
| **Code today** | `legacy/PetStore.Web/Helpers/Utils.cs` (`GetSupplyStock` :523, `SetSupplyStock`, `IsPetSold`, `MarkPetSold`, stock check inside `AddToCart` :201), `Controllers/CheckoutController.cs` (check :89, decrement :146–151), `Controllers/CartApiController.cs` (no checks), `Controllers/AdminController.cs` (`UpdateStock` :174, `ToggleSold` :57) |
| **Target module** | Inventory |

## Issues

| ID | ✓ | Issue | Location | Notes / repro |
|---|---|---|---|---|
| B-05 | ✅ [ ] | **Stock can go negative** | `legacy/PetStore.Web/Controllers/CartApiController.cs:87` (PUT: no stock check), `:79` (POST add: no check), `Controllers/CheckoutController.cs:89` (only checks `stock <= 0`, not `stock < qty`), `:151` | Dog Leash stock 3 → React cart qty 50 → checkout → `Stock = -47`. |
| B-06 | 📖 [ ] | **Non-atomic stock decrement (race)** | `legacy/PetStore.Web/Controllers/CheckoutController.cs:150-151` | Reads stock, then writes `stock - qty` in a separate statement with no transaction or concurrency token, so concurrent checkouts lose updates. |
| B-07 | 📖 [ ] | **Same pet can be sold twice** | `legacy/PetStore.Web/Controllers/CheckoutController.cs:146` | `IsSold` is only checked when adding to the cart (`Utils.AddToCart`), not at checkout. Two carts holding the same pet can both pay. |
| B-26 | 📖 [ ] | `Supply.Active` has an undocumented value `2` | `legacy/PetStore.Web/Models/Supply.cs:29`, `Views/Admin/Supplies.cshtml` | "1 = active, 0 = hidden, 2 = discontinued(?)". Admin edits it as a raw integer; the catalog shows only `== 1`. |

## Also affected by
- **B-08** (07): stock updates happen after the card charge and outside the order's save, with no rollback.
- **B-19** (06): via the API, the "pets are always qty 1" rule can be bypassed with a lowercase type.
- **CS-05** (18): stock is read and written with raw SQL while the order is saved with EF.
