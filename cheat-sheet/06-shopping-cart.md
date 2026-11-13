# 06: Shopping Cart

> ⚠️ Cheat sheet (spoiler). Back to the [index](README.md). Status: ✅ reproduced · 📖 by construction. Line numbers match commit `cfb038c`.

| | |
|---|---|
| **Responsibility** | Add, remove, update quantities; cart count badge; cart page; mini-cart |
| **Data owned** | Session key `"Cart"`: a JSON list of `CartItem` (Newtonsoft) |
| **Code today** | **Three owners:** (1) `legacy/PetStore.Web/Helpers/Utils.cs` cart region (`GetCart`, `SaveCart`, `AddToCart` :201, `RemoveFromCart` :266, `UpdateCartQty` :273, `CartCount`, `CartSubTotal`) used by `Controllers/CartController.cs` (jQuery forms); (2) `Controllers/CartApiController.cs` (its own private Load/Save, used by React); (3) Redux store in `ClientApp/src/` (reducer, sagas, `CartPage`, `CartRow`, `MiniCart`). Plus `Views/Shared/_Layout.cshtml` (badge computed in the view) and `wwwroot/Scripts/site.js` (ajax add + polling). |
| **Target module** | Cart (single server-side owner; the frontend only displays) |

## Issues

| ID | ✓ | Issue | Location | Notes / repro |
|---|---|---|---|---|
| B-16 | 📖 [ ] | **Header badge vs mini-cart disagree** | Header = sum of quantities (`legacy/PetStore.Web/Views/Shared/_Layout.cshtml:7`); MiniCart = number of lines (`ClientApp/src/components/MiniCart.jsx:30`) | 2 leashes + 1 pet: the badge shows 3, the mini-cart says "2 items". |
| B-17 | 📖 [ ] | **Session vs Redux drift** | `legacy/PetStore.Web/wwwroot/Scripts/site.js` (ajax add updates only `#cart-count`) | After an ajax "Add to cart" on a catalog page, the React mini-cart shows stale contents until reload. |
| B-18 | 📖 [ ] | Optimistic update without rollback + out-of-order responses | `ClientApp/src/reducers/rootReducer.js:41` (mutates state), `ClientApp/src/sagas/cartSagas.js:49` (`takeEvery`) | Typing "12" in qty sends PUT 1 and PUT 12 concurrently, and whichever lands last wins in the UI. A failed PUT leaves the optimistic qty displayed. |
| B-19 | 📖 [ ] | Item type case inconsistency | API stores `req.type` verbatim (`Controllers/CartApiController.cs:72`); `Utils.UpdateCartQty` compares case-sensitively (`Utils.cs:278`); `AddToCart` upper-cases (`Utils.cs:207`); `RemoveFromCart` is case-insensitive | A `"pet"` item can't be updated via `/Cart/Update`; duplicate pets are possible (`"pet"` vs `"PET"`); the PET qty = 1 rule is bypassed via the API. |
| B-28 | 📖 [ ] | Logout keeps the cart | `legacy/PetStore.Web/Helpers/Utils.cs:160` (`ClearLogin`) | Comment: "cart is kept on purpose". On a shared computer, the next user inherits the cart. |
| CS-07 | 📖 [ ] | **Duplicate cart implementations** | `Utils` cart region vs `CartApiController` (`Load`/`Save`/`Add`/`Update`/`Remove`) | Different rules: the API skips stock checks, the pet qty rule and type normalisation. |
| CS-31 | 📖 [ ] | Session as the cart store | `Utils.cs` `CART_KEY`, `Program.cs` session config | The whole cart is serialised as JSON into session on every change; it's lost on restart (in-memory distributed cache) and can't scale out. |
| FE-07 | 📖 [ ] | Derived state copied into component state | `ClientApp/src/components/CartPage.jsx` (totals), `CartRow.jsx` (qty) | The "you probably don't need derived state" anti-pattern. |
| FE-09 | 📖 [ ] | One giant reducer | `legacy/PetStore.Web/ClientApp/src/reducers/rootReducer.js` | Mixes cart, UI, coupon, user and request counters. `requestsInFlight` drifts (it's incremented on `UPDATE_QTY`/`REMOVE_ITEM` but decremented through different paths). |
| FE-10 | 📖 [ ] | `takeEvery` everywhere, no `takeLatest`/debounce/cancellation | `legacy/PetStore.Web/ClientApp/src/sagas/cartSagas.js:49` | This causes B-18. |
| FE-12 | 📖 [ ] | Sagas reach into the DOM with jQuery | `legacy/PetStore.Web/ClientApp/src/sagas/cartSagas.js:41` | React and jQuery both own `#cart-count`. |
| FE-13 | 📖 [ ] | Duplicate `FETCH_CART` on the cart page | `ClientApp/src/components/CartPage.jsx:15`, `MiniCart.jsx:8` | Both dispatch on mount, so there are two requests per page. |
| FE-16 | 📖 [ ] | Badge polling forever | `legacy/PetStore.Web/wwwroot/Scripts/site.js:35` | `/Cart/Count` every 30 seconds on every page. |

## Also affected by
- **B-05** (04): the API PUT/POST skip stock checks.
- **B-03 / FE-14** (05): the cart shows a different total from checkout.
- **S-17** (10): `CartController.Add` follows `returnUrl` and the `Referer` header (open redirect, `CartController.cs:37`).
- **S-18** (19): `/Cart/Remove` (:44) and `/Cart/Clear` (:58) change state via GET.
- **FE-03** (20): the cart page runs a stale bundle (shows "Remove" instead of "Delete" + confirm).
- **FE-06 / FE-08** (20): legacy lifecycles and reducer mutation in the cart components.
