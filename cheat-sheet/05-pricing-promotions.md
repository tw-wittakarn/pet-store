# 05: Pricing and Promotions

> ⚠️ Cheat sheet (spoiler). Back to the [index](README.md). Status: ✅ reproduced · 📖 by construction. Line numbers match commit `cfb038c`.

| | |
|---|---|
| **Responsibility** | Price display and formatting, the "big dog" discount, the coupon, tax, shipping, order totals |
| **Data owned** | Tax rate and shipping settings (**4–5 sources**), discount rule, coupon rule |
| **Code today** | `legacy/PetStore.Web/Helpers/Utils.cs` money region (:314–410: `FormatPrice`, `FormatMoney`, `ToCurrency`×2, `CalculateLineTax`, `CalculateTax`, `CalcTax2`, `CalculateShipping`, `GetShipping`, `CalculateCartTotal`, `ApplyDiscount`), `Global.asax.cs` (`TaxRate`, `ShippingFee`, `FreeShippingThreshold`), `Controllers/CartApiController.cs` (`CartJson`), `Controllers/CheckoutController.cs`, `Views/Cart/Index.cshtml`, `Views/Shared/_PetCard.cshtml`, `ClientApp/src/utils/money.js`, `ClientApp/src/reducers/rootReducer.js` (coupon) |
| **Target module** | Pricing (single source of truth, `decimal`) |

## Issues

| ID | ✓ | Issue | Location | Notes / repro |
|---|---|---|---|---|
| B-01 | ✅ [ ] | **`double` used for money** | `legacy/PetStore.Web/Models/Pet.cs:22`, `Models/Order.cs` (all totals), `Models/CartItem.cs`; `Supply.Price` is `decimal` (`Models/Supply.cs:19`) | Order saved `Total = 2351.325`; the API returns `tax: 86.79300000000002`. `double`/`decimal` are mixed, with casts in `CartApiController` and `Utils.AddToCart`. |
| B-02 | ✅ [ ] | Float noise shown to the user | `legacy/PetStore.Web/Views/Checkout/Index.cshtml:46` | Renders `Tax (7.000000000000001%)` and the unformatted `@Model.Tax` (`153.82500000000002`). |
| B-03 | 📖 [ ] | **Three or four different total calculations** | Razor estimate: `Utils.CalculateCartTotal` (`Utils.cs:379`), per-line rounded tax, shipping free if `> 100`. React: `ClientApp/src/utils/money.js:13`, tax rounded once, free if `>= 100`. Cart API: `Controllers/CartApiController.cs:42-44`, unrounded `sub * TAX`. Checkout: `Utils.CalcTax2` (`Utils.cs:360`), **tax on subtotal + shipping** | The customer sees one total in the cart and is charged another. A subtotal of exactly $100 is free shipping in React and checkout, but not in the Razor estimate. The ignored test T-03 shows it: 39.33 vs ≈ 40.03. |
| B-04 | ✅ [ ] | **Four or five sources of the tax rate** | `Utils.TAX = 0.07` (`Utils.cs:28`), `MvcApplication.TaxRate = 0.07` (`Global.asax.cs:12`), `Settings.TaxRate = 0.08` (`App_Data/seed.sql:6`, shown on the admin dashboard as "configured"), `appsettings.json` `AppSettings:TaxRate = 0.075` (never read), `money.js:2` | The admin believes the tax is 8%; customers pay 7%. |
| B-14 | 📖 [ ] | **Advertised discount never charged** | `legacy/PetStore.Web/Helpers/Utils.cs:387` (`ApplyDiscount`), used in `Views/Shared/_PetCard.cshtml:15` and `Views/Pets/Details.cshtml` | "10% off dogs over $1000" is displayed, but the cart and checkout charge full price. The home page card doesn't show it at all (CS-09). |
| B-15 | 📖 [ ] | Coupon "WOOF10" is fake | `legacy/PetStore.Web/ClientApp/src/reducers/rootReducer.js:60` | The UI says "applied! (discount shown at checkout)", but the server knows nothing about it. |
| CS-10 | 📖 [ ] | Six formatting helpers for one job | `Utils.FormatPrice` (:314), `FormatMoney` (:320, en-US culture), `ToCurrency(decimal)` (:326), `ToCurrency(object)`, `Pet.DisplayPrice`, `Supply.DisplayPrice` | Each formats slightly differently (thousands separators, culture). |
| CS-11 | 📖 [ ] | "DEPRECATED but still used" shipping function | `legacy/PetStore.Web/Helpers/Utils.cs:375` (`GetShipping`) vs `CalculateShipping` (:367) | `>=` vs `>` threshold, and different constants (`MvcApplication` vs `Utils`). |
| FE-14 | 📖 [ ] | Pricing rules duplicated in JS | `legacy/PetStore.Web/ClientApp/src/utils/money.js:2` ("keep in sync with Utils.cs !!!") | The React cart ignores the totals the API already returns and computes its own. |
| T-03 | 📖 [ ] | The ignored test is a real failing test | `legacy/PetStore.Tests/DatabaseTests.cs:37` | `[Ignore("fails sometimes ... fix later")]`. It **always** fails: it documents B-03. |

## Also affected by
- **S-19 / B-04** (16): `appsettings.json` holds an unused tax rate next to the secrets.
- **CS-24** (00): magic numbers `0.07`, `9.95`, `100` repeated in C# and JS.
