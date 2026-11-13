# 20: Client Application (React, Redux, Saga, jQuery)

> ⚠️ Cheat sheet (spoiler). Back to the [index](README.md). Status: ✅ reproduced · 📖 by construction. Line numbers match commit `cfb038c`.

| | |
|---|---|
| **Responsibility** | React islands (cart page + mini-cart; admin pet table), JS build pipeline, jQuery behaviour, vendored libraries |
| **Code today** | `legacy/PetStore.Web/ClientApp/` (webpack 4 + Babel 7: `src/index.js`, `reducers/`, `sagas/`, `components/`, `api/`, `utils/`), `wwwroot/Scripts/dist/cart.bundle.js` (committed build), `wwwroot/Scripts/admin/petTable.jsx` (in-browser Babel), `wwwroot/Scripts/vendor/*` (UMD libs), `wwwroot/Scripts/site.js`, `wwwroot/Content/*` |
| **Target module** | Frontend (single build, TypeScript, server-driven totals) |

## Issues

| ID | ✓ | Issue | Location | Notes / repro |
|---|---|---|---|---|
| FE-01 | 📖 [ ] | **Two React builds on one page** | `Views/Shared/_Layout.cshtml:90` (bundle on every page) + `Views/Admin/Pets.cshtml` (UMD React) | Two React and Redux instances and two stores (`window.cartStore`, `window.adminStore`) on the admin page. |
| FE-02 | ✅ [ ] | **In-browser Babel** | `legacy/PetStore.Web/Views/Admin/Pets.cshtml:10` | About 1.7 MB `babel.min.js`, compiled on every page load. The console warns "Be sure to precompile your scripts for production". |
| FE-03 | ✅ [ ] | **Committed bundle out of date with the source** | `ClientApp/src/components/CartRow.jsx:44` (source: "Delete" + `confirm()`) vs `wwwroot/Scripts/dist/cart.bundle.js` (live: "Remove", no confirm) | Running `npm run build` "fixes" it, so do that deliberately. |
| FE-04 | 📖 [ ] | Webpack 4 needs `NODE_OPTIONS=--openssl-legacy-provider` | `legacy/PetStore.Web/ClientApp/package.json:7` | Without it the build fails on Node 17+ with `ERR_OSSL_EVP_UNSUPPORTED`. |
| FE-05 | 📖 [ ] | Deprecated and dev-only dependencies | `ClientApp/package.json:11` (`@babel/polyfill` + core-js 2, IE11 target, `fsevents@1`), `App_Start/BundleConfig.cs` (`react.development.js` served in production) | |
| FE-06 | ✅ [ ] | Legacy React lifecycles | `ClientApp/src/components/CartRow.jsx:11`, `CartPage.jsx:18` (`componentWillReceiveProps`), `wwwroot/Scripts/admin/petTable.jsx:60` (`componentWillMount`, warns in the console) | |
| FE-08 | 📖 [ ] | **State mutation in reducers** | `ClientApp/src/reducers/rootReducer.js:41` (`item.Qty = action.qty`), `wwwroot/Scripts/admin/petTable.jsx:21` (`state.pets[i].IsSold = …`) | Breaks time-travel debugging and memoisation. |
| FE-11 | 📖 [ ] | Mixed HTTP clients | `ClientApp/src/api/cartApi.js` (`fetch`), `ClientApp/src/sagas/cartSagas.js` (`window.$.ajax`, "fetch DELETE didn't work in IE"), `petTable.jsx` (`$.getJSON`, `$.post`) | |
| FE-15 | 📖 [ ] | Globals | `window.APP_CONFIG` (includes email and admin flag), `window.cartStore` (`ClientApp/src/index.js:16`), `window.adminStore` (`petTable.jsx:126`), `var PetStore = PetStore \|\| {}` (`site.js`) | |
| FE-17 | 📖 [ ] | EOL vendored libraries, no package management | `wwwroot/Scripts/jquery-1.12.4.min.js` (known XSS CVEs), Bootstrap 3.4.1, jQuery Validation 1.17; `Content/Site.css` `!important` overrides | |
| FE-18 | 📖 [ ] | No TypeScript, PropTypes, lint, frontend tests or source maps | `ClientApp/` | |
| T-08 | 📖 [ ] | Node toolchain lock-in | `ClientApp/package.json` | webpack 4, Babel 7.12, ~594 npm packages, many deprecated. |

## Also affected by
- **FE-07 / FE-09 / FE-10 / FE-12 / FE-13 / FE-16** (06): cart-specific frontend issues.
- **FE-14** (05): pricing rules duplicated in `money.js`.
- **FE-19** (19): inline scripts and styles.
- **FE-20** (12): broken admin table sorting.
- **B-15** (05): the fake coupon lives in the reducer.
