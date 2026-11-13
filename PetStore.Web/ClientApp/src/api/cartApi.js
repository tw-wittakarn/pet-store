var BASE = '/api/cart';

export function getCart() {
    return fetch(BASE, { credentials: 'same-origin' }).then(function (r) { return r.json(); });
}

export function putQty(type, id, qty) {
    return fetch(BASE, {
        method: 'PUT',
        credentials: 'same-origin',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ type: type, id: id, qty: qty })
    }).then(function (r) { return r.json(); });
}
