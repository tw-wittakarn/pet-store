# 19: Web Shell and Presentation

> ⚠️ Cheat sheet (spoiler). Back to the [index](README.md). Status: ✅ reproduced · 📖 by construction. Line numbers match commit `cfb038c`.

| | |
|---|---|
| **Responsibility** | Startup, routing, bundling, layout, views and partials, HTML helpers, CSRF, error pages |
| **Code today** | `legacy/PetStore.Web/Program.cs`, `Global.asax.cs` (fake `MvcApplication`), `App_Start/RouteConfig.cs`, `App_Start/BundleConfig.cs`, `App_Start/FilterConfig.cs`, `Views/**` (esp. `Views/Shared/_Layout.cshtml`), `Helpers/Utils.cs` HTML builders (:588–715), `Helpers/HttpContextHelper.cs` |
| **Target module** | Platform / shared |

## Issues

| ID | ✓ | Issue | Location | Notes / repro |
|---|---|---|---|---|
| S-18 | 📖 [ ] | **No CSRF protection anywhere** | all `[HttpPost]` actions; state-changing GETs: `Controllers/CartController.cs:44` (Remove), `:58` (Clear), `Controllers/AdminController.cs:159` (DeleteCategory), `:118` (DeletePet), `:206` (UpdateOrderStatus) | No `[ValidateAntiForgeryToken]` and no `AutoValidateAntiforgeryToken` filter; forms have no tokens. |
| S-22 | 📖 [ ] | Information disclosure | `legacy/PetStore.Web/Program.cs:33` (`UseDeveloperExceptionPage` always on), `Views/Shared/Error.cshtml` (prints `LastError` stack trace), `App_Start/FilterConfig.cs:27` (`exception.Message` shown to users), admin dashboard last error, `Home/About` (uptime, order counter) | |
| B-30 | ✅ [ ] | Legacy route hijacks link generation | `legacy/PetStore.Web/App_Start/RouteConfig.cs:12` | `legacy-shop` (`shop/{categoryId?}`) is registered before `default`, so `RedirectToAction("Index","Pets")` produces `/shop` (seen when a pet's category was deleted). |
| CS-13 | 📖 [ ] | **Business logic in views** | `Views/Shared/_Layout.cshtml:7` (deserialises the session cart), `Views/Shared/_PetCard.cshtml:15` (discount), `Views/Home/About.cshtml:3` (DB settings query), `Views/Admin/Supplies.cshtml` (`GetCategoryName` per row), `Views/Cart/Index.cshtml:29` (tax label math) | |
| CS-14 | 📖 [ ] | `ViewBag` everywhere for typed data | e.g. `(List<Pet>)ViewBag.Featured`, `(double)ViewBag.Total`, `(Dictionary<int,int>)ViewBag.Counts` | Runtime cast failures instead of compile-time checks. |
| CS-15 | 📖 [ ] | Domain entities used as view and binding models | `AccountController.Register(User user)`, `AdminController.CreatePet/EditPet(Pet pet)` | The cause of S-05; over-posting on Pet (`IsSold`, `ImageUrl`, `CreatedDate`). |
| CS-32 | 📖 [ ] | Fake `Global.asax.cs` / `MvcApplication.Application_Start()` | `legacy/PetStore.Web/Global.asax.cs`, called from `Program.cs` | A port leftover that hides startup order and global state. |
| CS-33 | 📖 [ ] | Hand-rolled bundling | `legacy/PetStore.Web/App_Start/BundleConfig.cs` | No minification or fingerprinting; hard-coded `?v=3` cache-busting; serves React **development** builds. |
| CS-34 | 📖 [ ] | Legacy URL routes kept in the main route table | `App_Start/RouteConfig.cs` (`pet.aspx`, `shop/{categoryId?}`) | Should be redirects (301), not routes. The cause of B-30. |
| FE-19 | 📖 [ ] | Inline scripts and styles | `Views/Checkout/Index.cshtml` (inline `<script>`), `_Layout.cshtml` (inline `<style>`, `window.APP_CONFIG`), inline `style=` throughout views and React | Blocks a strict CSP. |

## Also affected by
- **S-10 / S-11 / S-12 / S-13**: every `Html.Raw` in views (12, 01, 14, 09).
- **CS-02** (00): `HttpContextHelper.Current`.
- **CS-09** (01): HTML built in C# strings (`Utils` builders).
- **CS-25** (15): the global exception filter registered in `FilterConfig`.
