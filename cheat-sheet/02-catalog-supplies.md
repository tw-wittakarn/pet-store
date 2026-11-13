# 02: Catalog: Supplies

> ⚠️ Cheat sheet (spoiler). Back to the [index](README.md). Status: ✅ reproduced · 📖 by construction. Line numbers match commit `cfb038c`.

| | |
|---|---|
| **Responsibility** | Browse, search, filter, sort supplies; details; low-stock badges |
| **Data owned** | `Supplies` (`CatId` points at `Categories` with **no foreign key**) |
| **Code today** | `legacy/PetStore.Web/Controllers/SuppliesController.cs` (EF, in-memory filtering), `Helpers/Utils.cs` (`BuildSupplyCardHtml`, `ToCurrency(decimal)`), `Views/Supplies/*`, `Models/Supply.cs`, Home "Almost gone" widget |
| **Target module** | Catalog (unified `Product`) |

## Issues

| ID | ✓ | Issue | Location | Notes / repro |
|---|---|---|---|---|
| B-20 | 📖 [ ] | Supplies search is case-sensitive; pets search isn't | `legacy/PetStore.Web/Controllers/SuppliesController.cs:27` | `string.Contains` vs SQL `LIKE`. Searching "dog" finds nothing ("Dog Leash"). |
| CS-08 | 📖 [ ] | **Duplicated catalog stack** | `SuppliesController.cs` vs `PetsController.cs` | "Copied from PetsController and switched to EF". Different paging (page size 8 vs 6; Ceiling vs integer division), filtering and sorting approaches. |
| CS-12 | 📖 [ ] | **Near-duplicate models with inconsistent naming and types** | `legacy/PetStore.Web/Models/Supply.cs` vs `Models/Pet.cs` | `Id`/`SupplyId`, `Name`/`SupplyName`, `CategoryId`/`CatId`, `Description`/`Desc`, `ImageUrl`/`ImagePath`, `double`/`decimal` price, `bool IsSold` vs `int Active`. A ready-made "extract Product" exercise. |

## Also affected by
- **CS-22** (18): the whole `Supplies` table is loaded into memory (`SuppliesController.cs:19`), because SQLite can't sort `decimal` server-side.
- **B-01** (05): `Supply.Price` is `decimal` while everything else uses `double`; it's cast to `double` when added to the cart.
- **B-26** (04): `Active` has an undocumented value `2`.
- **B-12** (03): supplies aren't cascaded; when their category is deleted they're orphaned and show "Unknown".
