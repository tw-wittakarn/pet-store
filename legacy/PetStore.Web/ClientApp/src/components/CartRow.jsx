import React from 'react';
import { formatPrice } from '../utils/money';

export default class CartRow extends React.Component {
    constructor(props) {
        super(props);
        this.state = { qty: props.item.Qty };
        this.onQtyChange = this.onQtyChange.bind(this);
    }

    componentWillReceiveProps(nextProps) {
        // keep local input in sync with the store
        if (nextProps.item.Qty !== this.state.qty) {
            this.setState({ qty: nextProps.item.Qty });
        }
    }

    onQtyChange(e) {
        var qty = parseInt(e.target.value, 10);
        if (isNaN(qty)) qty = 0;
        this.setState({ qty: qty });
        this.props.onUpdateQty(this.props.item.Type, this.props.item.Id, qty);
    }

    render() {
        var item = this.props.item;
        var isPet = item.Type === 'PET' || item.Type === 'pet';
        return (
            <tr>
                <td><img src={item.Image} style={{ height: 50 }} /></td>
                <td>
                    <a href={isPet ? '/Pets/Details/' + item.Id : '/Supplies/Details/' + item.Id}>{item.Name}</a>
                    {isPet ? <span className="label label-info" style={{ marginLeft: 5 }}>pet</span> : null}
                </td>
                <td>{formatPrice(item.Price)}</td>
                <td style={{ width: 100 }}>
                    {isPet
                        ? <span>1</span>
                        : <input type="number" min="0" className="form-control input-sm" value={this.state.qty} onChange={this.onQtyChange} />}
                </td>
                <td>{formatPrice(item.Price * item.Qty)}</td>
                <td>
                    <button className="btn btn-danger btn-xs" onClick={() => { if (confirm('Remove ' + item.Name + '?')) this.props.onRemove(item.Type, item.Id); }}>
                        <span className="glyphicon glyphicon-trash"></span> Delete
                    </button>
                </td>
            </tr>
        );
    }
}
