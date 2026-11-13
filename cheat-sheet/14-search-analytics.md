# 14: Search Analytics

> ⚠️ Cheat sheet (spoiler). Back to the [index](README.md). Status: ✅ reproduced · 📖 by construction. Line numbers match commit `cfb038c`.

| | |
|---|---|
| **Responsibility** | Record recent catalog searches; show them on the admin dashboard |
| **Data owned** | `MvcApplication.RecentSearches` (static in-memory `List<string>`, last 10) |
| **Code today** | `legacy/PetStore.Web/Helpers/Utils.cs` (`AddRecentSearch` :977), called from `PetsController` and `SuppliesController`; displayed in `Views/Admin/Index.cshtml` |
| **Target module** | Platform / shared |

## Issues

| ID | ✓ | Issue | Location | Notes / repro |
|---|---|---|---|---|
| S-12 | 📖 [ ] | **Stored XSS against admin via recent searches** | `legacy/PetStore.Web/Views/Admin/Index.cshtml:21` | Anonymous search terms are stored and rendered with `Html.Raw` on the admin dashboard, so an anonymous user can get script running in an admin session. |

## Also affected by
- **CS-03** (00): a global, non-thread-safe `List<string>` shared by all users; `Insert(0, …)` + `RemoveAt` can race; lost on restart; wrong with more than one instance.
- **S-02** (01): the search terms that get recorded are also the SQL-injection vector.
