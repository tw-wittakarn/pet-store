import * as types from '../constants/ActionTypes';

// one reducer for everything - easier to find stuff
var initialState = {
    cart: { items: [], subTotal: 0, shipping: 0, tax: 0, total: 0, count: 0 },
    loading: false,
    loaded: false,
    error: null,
    miniCartOpen: false,
    coupon: '',
    couponApplied: false,
    user: window.APP_CONFIG || {},
    lastUpdated: null,
    requestsInFlight: 0
};

export default function rootReducer(state, action) {
    if (state === undefined) state = initialState;

    switch (action.type) {
        case types.FETCH_CART:
            return Object.assign({}, state, { loading: true, requestsInFlight: state.requestsInFlight + 1 });

        case types.CART_LOADED:
            return Object.assign({}, state, {
                loading: false,
                loaded: true,
                cart: action.data,
                lastUpdated: new Date(),
                requestsInFlight: state.requestsInFlight - 1
            });

        case types.CART_ERROR:
            return Object.assign({}, state, { loading: false, error: action.error, requestsInFlight: state.requestsInFlight - 1 });

        case types.UPDATE_QTY:
            // optimistic update so the UI feels fast
            for (var i = 0; i < state.cart.items.length; i++) {
                var item = state.cart.items[i];
                if (item.Type === action.itemType && item.Id === action.id) {
                    item.Qty = action.qty;
                }
            }
            return Object.assign({}, state, { requestsInFlight: state.requestsInFlight + 1 });

        case types.REMOVE_ITEM:
            return Object.assign({}, state, { requestsInFlight: state.requestsInFlight + 1 });

        case types.ITEM_REMOVED:
            return Object.assign({}, state, { cart: action.data, requestsInFlight: state.requestsInFlight - 1 });

        case types.TOGGLE_MINICART:
            return Object.assign({}, state, { miniCartOpen: !state.miniCartOpen });

        case types.SET_COUPON:
            return Object.assign({}, state, { coupon: action.code });

        case types.APPLY_COUPON:
            // TODO server side
            if (state.coupon === 'WOOF10') return Object.assign({}, state, { couponApplied: true });
            return state;

        case types.CLEAR_ERROR:
            return Object.assign({}, state, { error: null });

        default:
            return state;
    }
}
