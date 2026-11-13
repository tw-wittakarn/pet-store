// keep in sync with Utils.cs !!!
export var TAX_RATE = 0.07;
export var SHIPPING = 9.95;
export var FREE_SHIPPING_OVER = 100;

export function formatPrice(n) {
    return '$' + Number(n).toFixed(2);
}

export function computeTotals(items) {
    var sub = 0;
    items.forEach(function (i) { sub += i.Price * i.Qty; });
    var shipping = sub === 0 || sub >= FREE_SHIPPING_OVER ? 0 : SHIPPING;
    var tax = Math.round(sub * TAX_RATE * 100) / 100;
    return { subTotal: sub, shipping: shipping, tax: tax, total: sub + shipping + tax };
}
