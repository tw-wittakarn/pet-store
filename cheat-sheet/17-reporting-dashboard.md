# 17: Reporting / Dashboard

> ⚠️ Cheat sheet (spoiler). Back to the [index](README.md). Status: ✅ reproduced · 📖 by construction. Line numbers match commit `cfb038c`.

| | |
|---|---|
| **Responsibility** | Admin dashboard: pet, sold, order and customer counts; revenue; configured tax; uptime on the About page |
| **Data owned** | None (reads across `Pets`, `Orders`, `Users`, `Settings`, `AuditLog`) |
| **Code today** | `legacy/PetStore.Web/Controllers/AdminController.cs` (`Index` :20–35, raw SQL aggregates), `Views/Admin/Index.cshtml`, `Controllers/HomeController.cs` (`About`) |
| **Target module** | Ordering (read model / reporting queries) |

## Issues

| ID | ✓ | Issue | Location | Notes / repro |
|---|---|---|---|---|
| B-22 | 📖 [ ] | Revenue is wrong | `legacy/PetStore.Web/Controllers/AdminController.cs:27` | `SUM(Total) WHERE Status != 'Cancelled'` counts unpaid `New` orders and lowercase `cancelled` ones (`StatusLabel` explicitly handles that casing; any string can be set via S-03/S-07). It sums `double` (B-01). "Sold" counts pets toggled sold by hand without an order. |

## Also affected by
- **B-04** (05): the dashboard shows "Configured tax rate 0.08", which isn't what's charged.
- **S-12** (14): recent searches are rendered raw on this page.
- **S-22** (19): the last error's stack trace is shown on this page.
- **CS-10** (05): revenue is formatted with `FormatMoney` (en-US culture) while the rest of the site uses `FormatPrice`.
