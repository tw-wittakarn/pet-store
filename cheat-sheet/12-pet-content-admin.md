# 12: Pet Content Management (Admin)

> ⚠️ Cheat sheet (spoiler). Back to the [index](README.md). Status: ✅ reproduced · 📖 by construction. Line numbers match commit `cfb038c`.

| | |
|---|---|
| **Responsibility** | Create and edit pets, image upload, featured and sold flags, delete; React admin pet table; JSON feed |
| **Data owned** | `Pets` (write side), `wwwroot/uploads/` |
| **Code today** | `legacy/PetStore.Web/Controllers/AdminController.cs` (`Pets`, `PetsJson` :45, `ToggleSold` :57, `CreatePet` :75, `EditPet` :98, `DeletePet` :118), `Helpers/Utils.cs` (`SaveUpload` :847, `DeleteFile`, `SanitizeHtml` :708), `Views/Admin/Pets.cshtml`, `Views/Admin/EditPet.cshtml`, `wwwroot/Scripts/admin/petTable.jsx` |
| **Target module** | Catalog (admin side) |

## Issues

| ID | ✓ | Issue | Location | Notes / repro |
|---|---|---|---|---|
| S-06 | ✅ [ ] | **Missing authorisation: DeletePet via GET** | `legacy/PetStore.Web/Controllers/AdminController.cs:118` | No `[AdminOnly]`, GET verb, no anti-forgery. Anonymous `/Admin/DeletePet/20` deletes the pet. CSRF-able with `<img src>`. |
| S-08 | ✅ [ ] | Missing authorisation: PetsJson data feed | `legacy/PetStore.Web/Controllers/AdminController.cs:45` | Anonymous users can read the full admin pet list. |
| S-10 | ✅ [ ] | **Stored XSS: pet description** | write: `Helpers/Utils.cs:708` (`SanitizeHtml`), used in `CreatePet`/`EditPet`; render: `legacy/PetStore.Web/Views/Pets/Details.cshtml:20` (`Html.Raw`) | The "sanitiser" strips only the literal lowercase `<script>` and `</script>`. `<img src=x onerror=alert(1)>`, `<SCRIPT>` and `<script >` all pass. The edit form says "HTML ok". |
| S-14 | 📖 [ ] | **Unrestricted file upload / path traversal** | `legacy/PetStore.Web/Helpers/Utils.cs:854` | Keeps the client `file.FileName`, with no extension, MIME or size checks. Saved into `wwwroot/uploads` and served statically (`.html`, `.svg` with script). Filenames collide and overwrite each other. |
| FE-20 | 📖 [ ] | Admin table sort is broken | `legacy/PetStore.Web/wwwroot/Scripts/admin/petTable.jsx:70` | The comparator never returns 0; sorting `CreatedDate` (`dd/MM/yyyy` strings) is lexicographic; `.sort` mutates the filtered array. |

## Also affected by
- **FE-02** (20): the table is compiled in the browser by `babel-standalone`.
- **FE-06 / FE-08** (20): `componentWillMount` (:60); reducer mutation in `TOGGLE_SOLD_OK` (:21).
- **FE-10** (06): `toggleSoldSaga` (:49) has no error handling.
- **CS-15** (19): `CreatePet(Pet pet)` / `EditPet(Pet pet)` bind the entity directly.
- **S-18** (19): no anti-forgery on create, edit or toggle.
- **B-11** (18): `CreatedDate` is set as a `dd/MM/yyyy` string from `DateTime.Now`.
