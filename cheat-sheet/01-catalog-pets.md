# 01: Catalog: Pets

> ⚠️ Cheat sheet (spoiler). Back to the [index](README.md). Status: ✅ reproduced · 📖 by construction. Line numbers match commit `cfb038c`.

| | |
|---|---|
| **Responsibility** | Browse, search, filter (category, price), sort and page pets; pet details; related pets; "NEW" badge; legacy `pet.aspx` / `shop/` URLs |
| **Data owned** | `Pets` (reads `Categories`) |
| **Code today** | `legacy/PetStore.Web/Controllers/PetsController.cs` (raw SQL), `HomeController.cs` (featured, via raw SQL), `Helpers/Utils.cs` (`RowToPet`, `BuildPetCardHtml`, `AgeInMonths`), `Views/Pets/*`, `Views/Shared/_PetCard.cshtml`, `Models/Pet.cs` (`IsNew`, `DisplayPrice`) |
| **Target module** | Catalog (unified `Product`) |

## Issues

| ID | ✓ | Issue | Location | Notes / repro |
|---|---|---|---|---|
| S-02 | ✅ [ ] | **SQL injection in catalog search** | `legacy/PetStore.Web/Controllers/PetsController.cs:27` | `search=zzz') OR 1=1--` returns all 20. Boolean/blind only: the same WHERE clause also feeds a one-column `COUNT(*)`, so UNION breaks that query. `minPrice`/`maxPrice` are typed but still concatenated. |
| S-11 | ✅ [ ] | **Reflected XSS: search term** | `legacy/PetStore.Web/Views/Pets/Index.cshtml:43` | `/Pets?search=<script>alert(1)</script>` executes (`Html.Raw(Model.Search)`). |
| B-09 | ✅ [ ] | **Pagination drops the last partial page** | `legacy/PetStore.Web/Controllers/PetsController.cs:43` | `total / PAGE_SIZE` uses integer division: 20/6 = 3 pages, so pets 19–20 are never listed. Supplies use `Math.Ceiling` (inconsistent). |
| B-27 | 📖 [ ] | `Pet.Age` is free text parsed by regex | `legacy/PetStore.Web/Models/Pet.cs:20`, `Helpers/Utils.cs:828` | "3 months", "2 yrs", "1.5 years"; anything else becomes 0 months. |
| CS-09 | 📖 [ ] | **Duplicated pet-card rendering** | `Helpers/Utils.cs:588` (`BuildPetCardHtml`, C# strings) vs `Views/Shared/_PetCard.cshtml` (comment: "Keep them in sync!") | They've already drifted: the home card has no discount, no category and no Details button. |
| CS-16 | 📖 [ ] | Shared view model "don't change!!" | `legacy/PetStore.Web/Models/ViewModels.cs` (`PetListModel`) | Claimed to be used by admin pages too, a fear-driven coupling comment. |
| CS-35 | 📖 [ ] | `int.Parse(Request.Query["id"])` for `pet.aspx?id=` | `legacy/PetStore.Web/Controllers/PetsController.cs:79` | Throws on bad input; the global filter swallows it (CS-25). |

## Also affected by
- **B-12** (03): deleting a category cascades and deletes its pets; a pet whose category is gone shows "Pet not found".
- **B-13** (03): the stale category cache shows the old category name in the list heading.
- **B-14** (05): the "10% off dogs over $1000" shown on cards and details is never charged.
- **S-10** (12): the description is rendered with `Html.Raw` on `Views/Pets/Details.cshtml:20`.
- **CS-08 / CS-12** (02): pets and supplies are parallel copies of one Product concept.
- **CS-21 / CS-22** (18): N+1 category counts on Home (`HomeController.cs:36`); related pets load the whole table (`PetsController.cs:91`).
- **B-11** (18): `Pet.IsNew` uses `DateTime.Now` against a `dd/MM/yyyy` string (`Models/Pet.cs:47`).
- **B-30** (19): `RedirectToAction("Index","Pets")` produces `/shop` because of the legacy route.
