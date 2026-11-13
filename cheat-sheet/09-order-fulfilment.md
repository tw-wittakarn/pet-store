# 09: Order Fulfilment (Back Office)

> ⚠️ Cheat sheet (spoiler). Back to the [index](README.md). Status: ✅ reproduced · 📖 by construction. Line numbers match commit `cfb038c`.

| | |
|---|---|
| **Responsibility** | Admin order list, filter by status, status transitions (New → Paid → Shipped → Cancelled) |
| **Data owned** | `Orders.Status` (free-text string) |
| **Code today** | `legacy/PetStore.Web/Controllers/AdminController.cs` (`Orders`, `UpdateOrderStatus` :206), `Helpers/Utils.cs` (`StatusLabel` :682), `Views/Admin/Orders.cshtml` |
| **Target module** | Ordering |

## Issues

| ID | ✓ | Issue | Location | Notes / repro |
|---|---|---|---|---|
| S-03 | 📖 [ ] | SQL injection in order status | `legacy/PetStore.Web/Controllers/AdminController.cs:208` | The `status` query-string value is concatenated into an UPDATE. Combined with S-07, it's reachable anonymously. |
| S-07 | ✅ [ ] | **Missing authorisation: UpdateOrderStatus** | `legacy/PetStore.Web/Controllers/AdminController.cs:206` | No `[AdminOnly]`, GET verb. Anyone can cancel or ship any order: `/Admin/UpdateOrderStatus?id=1&status=Cancelled`. |
| S-13 | 📖 [ ] | Stored XSS against admin: order notes | `legacy/PetStore.Web/Views/Admin/Orders.cshtml:34` | The checkout "Notes" field is rendered with `Html.Raw`. |

## Also affected by
- **S-16** (08): the admin orders page shows full card numbers.
- **B-22** (17): free-text statuses (`"Cancelled"` vs `"cancelled"`, any string via S-03/S-07) break revenue.
- **CS-21** (18): one user lookup per order (`AdminController.cs:195`).
- **CS-22** (18): all orders are loaded, then filtered in memory.
- **CS-24** (00): status values are magic strings; there's no state machine (any transition is allowed).
