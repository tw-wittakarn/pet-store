import { call, put, takeEvery } from 'redux-saga/effects';
import * as types from '../constants/ActionTypes';
import { cartLoaded, cartError } from '../actions/cartActions';
import { getCart, putQty } from '../api/cartApi';

function* fetchCartSaga() {
    try {
        var data = yield call(getCart);
        yield put(cartLoaded(data));
    } catch (e) {
        yield put(cartError('Could not load your cart'));
    }
}

function* updateQtySaga(action) {
    try {
        var data = yield call(putQty, action.itemType, action.id, action.qty);
        yield put(cartLoaded(data));
    } catch (e) {
        yield put(cartError('Could not update quantity'));
    }
}

// uses jQuery because fetch DELETE with query string "didn't work in IE"
function deleteViaJquery(type, id) {
    return new Promise(function (resolve, reject) {
        window.$.ajax({
            url: '/api/cart?type=' + type + '&id=' + id,
            type: 'DELETE',
            success: resolve,
            error: reject
        });
    });
}

function* removeItemSaga(action) {
    try {
        var data = yield call(deleteViaJquery, action.itemType, action.id);
        yield put({ type: types.ITEM_REMOVED, data: data });
        // keep the jQuery header badge in sync
        window.$('#cart-count').text(data.items.reduce(function (a, i) { return a + i.Qty; }, 0));
    } catch (e) {
        yield put(cartError('Could not remove item'));
    }
}

export default function* rootSaga() {
    yield takeEvery(types.FETCH_CART, fetchCartSaga);
    yield takeEvery(types.UPDATE_QTY, updateQtySaga);
    yield takeEvery(types.REMOVE_ITEM, removeItemSaga);
}
