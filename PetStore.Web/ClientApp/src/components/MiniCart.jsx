import React from 'react';
import { connect } from 'react-redux';
import { fetchCart, toggleMiniCart } from '../actions/cartActions';
import { formatPrice } from '../utils/money';

class MiniCart extends React.Component {
    componentDidMount() {
        this.props.dispatch(fetchCart());
    }

    render() {
        var self = this;
        return (
            <a href="#" onClick={function (e) { e.preventDefault(); self.props.dispatch(toggleMiniCart()); }} style={{ position: 'relative' }}>
                <span className="glyphicon glyphicon-list-alt"></span> {this.props.count} items
                {this.props.open ?
                    <div style={{ position: 'absolute', right: 0, top: 40, background: '#fff', color: '#333', width: 250, padding: 10, border: '1px solid #ddd', zIndex: 1000 }}>
                        {this.props.items.length === 0 ? <em>Empty</em> : this.props.items.map(function (i) {
                            return <div key={i.Type + i.Id}>{i.Name} x {i.Qty} <span className="pull-right">{formatPrice(i.Price * i.Qty)}</span></div>;
                        })}
                        <hr style={{ margin: '5px 0' }} />
                        <strong>Total: {formatPrice(this.props.total)}</strong>
                    </div> : null}
            </a>
        );
    }
}

export default connect(function (state) {
    return { count: state.cart.items.length, items: state.cart.items, total: state.cart.total, open: state.miniCartOpen };
})(MiniCart);
