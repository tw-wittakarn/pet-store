import React from 'react';
import ReactDOM from 'react-dom';
import { createStore, applyMiddleware, compose } from 'redux';
import { Provider } from 'react-redux';
import createSagaMiddleware from 'redux-saga';
import rootReducer from './reducers/rootReducer';
import rootSaga from './sagas/cartSagas';
import CartPage from './components/CartPage';
import MiniCart from './components/MiniCart';

var sagaMiddleware = createSagaMiddleware();
var composeEnhancers = window.__REDUX_DEVTOOLS_EXTENSION_COMPOSE__ || compose;
var store = createStore(rootReducer, composeEnhancers(applyMiddleware(sagaMiddleware)));
sagaMiddleware.run(rootSaga);

window.cartStore = store; // so jQuery code can poke it if needed

var cartEl = document.getElementById('react-cart');
if (cartEl) {
    ReactDOM.render(<Provider store={store}><CartPage /></Provider>, cartEl);
}

var miniEl = document.getElementById('react-minicart');
if (miniEl) {
    ReactDOM.render(<Provider store={store}><MiniCart /></Provider>, miniEl);
}
