import React from 'react';
import { connect } from 'react-redux';
import CartRow from './CartRow';
import { fetchCart, updateQty, removeItem, setCoupon } from '../actions/cartActions';
import { computeTotals, formatPrice, FREE_SHIPPING_OVER } from '../utils/money';
import * as types from '../constants/ActionTypes';

class CartPage extends React.Component {
    constructor(props) {
        super(props);
        this.state = { totals: computeTotals(props.items) };
    }

    componentDidMount() {
        this.props.dispatch(fetchCart());
    }

    componentWillReceiveProps(nextProps) {
        this.setState({ totals: computeTotals(nextProps.items) });
    }

    render() {
        var self = this;
        var t = this.state.totals;

        if (!this.props.loaded) return <p className="text-muted">Loading cart...</p>;

        if (this.props.items.length === 0) {
            return (
                <div className="alert alert-info">
                    Your cart is empty. <a href="/Pets">Find a friend!</a>
                </div>
            );
        }

        return (
            <div>
                {this.props.error ? <div className="alert alert-danger" onClick={() => this.props.dispatch({ type: types.CLEAR_ERROR })}>{this.props.error}</div> : null}
                <table className="table">
                    <thead>
                        <tr><th></th><th>Item</th><th>Price</th><th>Qty</th><th>Total</th><th></th></tr>
                    </thead>
                    <tbody>
                        {this.props.items.map(function (item) {
                            return <CartRow key={item.Type + item.Id} item={item}
                                onUpdateQty={function (type, id, qty) { self.props.dispatch(updateQty(type, id, qty)); }}
                                onRemove={function (type, id) { self.props.dispatch(removeItem(type, id)); }} />;
                        })}
                    </tbody>
                </table>
                <div className="row">
                    <div className="col-md-4">
                        <div className="input-group">
                            <input className="form-control" placeholder="Coupon code" value={this.props.coupon}
                                onChange={function (e) { self.props.dispatch(setCoupon(e.target.value)); }} />
                            <span className="input-group-btn">
                                <button className="btn btn-default" onClick={function () { self.props.dispatch({ type: types.APPLY_COUPON }); }}>Apply</button>
                            </span>
                        </div>
                        {this.props.couponApplied ? <p className="text-success">WOOF10 applied! (discount shown at checkout)</p> : null}
                    </div>
                    <div className="col-md-4 col-md-offset-4">
                        <table className="table table-condensed">
                            <tbody>
                                <tr><td>Subtotal</td><td className="text-right">{formatPrice(t.subTotal)}</td></tr>
                                <tr><td>Shipping {t.shipping === 0 ? <small className="text-success">(free over ${FREE_SHIPPING_OVER})</small> : null}</td><td className="text-right">{formatPrice(t.shipping)}</td></tr>
                                <tr><td>Tax</td><td className="text-right">{formatPrice(t.tax)}</td></tr>
                                <tr><th>Total</th><th className="text-right">{formatPrice(t.total)}</th></tr>
                            </tbody>
                        </table>
                    </div>
                </div>
            </div>
        );
    }
}

function mapStateToProps(state) {
    return {
        items: state.cart.items,
        loaded: state.loaded,
        error: state.error,
        coupon: state.coupon,
        couponApplied: state.couponApplied,
        total: state.cart.total
    };
}

export default connect(mapStateToProps)(CartPage);
