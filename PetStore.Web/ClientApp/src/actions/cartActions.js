import * as types from '../constants/ActionTypes';

export function fetchCart() {
    return { type: types.FETCH_CART };
}

export function cartLoaded(data) {
    return { type: types.CART_LOADED, data: data };
}

export function cartError(error) {
    return { type: types.CART_ERROR, error: error };
}

export function updateQty(itemType, id, qty) {
    return { type: types.UPDATE_QTY, itemType: itemType, id: id, qty: qty };
}

export function removeItem(itemType, id) {
    return { type: types.REMOVE_ITEM, itemType: itemType, id: id };
}

export function toggleMiniCart() {
    return { type: types.TOGGLE_MINICART };
}

export function setCoupon(code) {
    return { type: types.SET_COUPON, code: code };
}
