# 03: Category Management

> ⚠️ Cheat sheet (spoiler). Back to the [index](README.md). Status: ✅ reproduced · 📖 by construction. Line numbers match commit `cfb038c`.

| | |
|---|---|
| **Responsibility** | Category list and order; create, edit, delete; id → name lookup used everywhere |
| **Data owned** | `Categories` |
| **Code today** | `legacy/PetStore.Web/Controllers/AdminController.cs` (`Categories`, `SaveCategory` :141 mixes raw INSERT and EF update, `DeleteCategory` :159 raw DELETE), `Helpers/Utils.cs` (`GetCategoryName` + static cache :510, `BuildCategoryDropdown`), `Views/Admin/Categories.cshtml` |
| **Target module** | Catalog |

## Issues

| ID | ✓ | Issue | Location | Notes / repro |
|---|---|---|---|---|
| B-12 | ✅ [ ] | **Deleting a category cascades and deletes all its pets** | `legacy/PetStore.Web/Controllers/AdminController.cs:161` + EF `ON DELETE CASCADE` from `EnsureCreated` | Microsoft.Data.Sqlite turns foreign keys on. Deleting "Reptiles" silently deletes Spike, Shelly and Kaa (and their order history references dangle). There's no warning, and it's a GET with no CSRF protection (S-18). Supplies have no FK, so they become orphans showing "Unknown". |
| B-13 | ✅ [ ] | **Stale category cache** | `legacy/PetStore.Web/Helpers/Utils.cs:510` (`MvcApplication.CategoryCache`) | Rename "Dogs" → "Puppies": the pet list heading still says "Dogs" while the details breadcrumb says "Puppies". It's never invalidated by SaveCategory/DeleteCategory and isn't thread-safe. |

## Also affected by
- **S-18** (19): `DeleteCategory` is a GET link with no anti-forgery token.
- **S-04** (18): `SaveCategory` builds the INSERT with string concatenation, escaped only by `Utils.Esc`.
- **CS-03** (00): the cache lives in global mutable static state.
- **CS-05** (18): the same action uses raw SQL for insert and EF for update.
