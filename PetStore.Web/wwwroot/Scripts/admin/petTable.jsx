// Admin pet table - compiled in the browser by babel-standalone (see Views/Admin/Pets.cshtml)
// uses the UMD globals React, ReactDOM, Redux, ReactRedux, ReduxSaga

var createSagaMiddleware = ReduxSaga.default;
var call = ReduxSaga.effects.call;
var put = ReduxSaga.effects.put;
var takeEvery = ReduxSaga.effects.takeEvery;

var initialState = { pets: [], loading: false, filter: '', sortBy: 'Id', error: null };

function adminReducer(state, action) {
    if (state === undefined) state = initialState;
    switch (action.type) {
        case 'LOAD_PETS': return Object.assign({}, state, { loading: true });
        case 'LOAD_PETS_OK': return Object.assign({}, state, { loading: false, pets: action.pets });
        case 'LOAD_PETS_FAIL': return Object.assign({}, state, { loading: false, error: action.error });
        case 'SET_FILTER': return Object.assign({}, state, { filter: action.filter });
        case 'SET_SORT': return Object.assign({}, state, { sortBy: action.sortBy });
        case 'TOGGLE_SOLD_OK':
            for (var i = 0; i < state.pets.length; i++) {
                if (state.pets[i].Id === action.id) state.pets[i].IsSold = action.isSold;
            }
            return Object.assign({}, state);
        default: return state;
    }
}

function fetchPets() {
    return new Promise(function (resolve, reject) {
        $.getJSON('/Admin/PetsJson').done(resolve).fail(reject);
    });
}

function postToggle(id) {
    return new Promise(function (resolve, reject) {
        $.post('/Admin/ToggleSold', { id: id }).done(resolve).fail(reject);
    });
}

function* loadPetsSaga() {
    try {
        var pets = yield call(fetchPets);
        yield put({ type: 'LOAD_PETS_OK', pets: pets });
    } catch (e) {
        yield put({ type: 'LOAD_PETS_FAIL', error: 'Could not load pets' });
    }
}

function* toggleSoldSaga(action) {
    var res = yield call(postToggle, action.id);
    yield put({ type: 'TOGGLE_SOLD_OK', id: action.id, isSold: res.isSold });
}

function* rootSaga() {
    yield takeEvery('LOAD_PETS', loadPetsSaga);
    yield takeEvery('TOGGLE_SOLD', toggleSoldSaga);
}

class PetTable extends React.Component {
    componentWillMount() {
        this.props.dispatch({ type: 'LOAD_PETS' });
    }

    sorted() {
        var self = this;
        var f = this.props.filter.toLowerCase();
        var list = this.props.pets.filter(function (p) {
            return p.Name.toLowerCase().indexOf(f) >= 0 || (p.Breed || '').toLowerCase().indexOf(f) >= 0;
        });
        return list.sort(function (a, b) {
            return a[self.props.sortBy] > b[self.props.sortBy] ? 1 : -1;
        });
    }

    render() {
        var self = this;
        if (this.props.loading) return <p>Loading pets...</p>;
        if (this.props.error) return <div className="alert alert-danger">{this.props.error}</div>;
        return (
            <div>
                <input className="form-control" placeholder="filter..." style={{ marginBottom: 10 }}
                    value={this.props.filter}
                    onChange={function (e) { self.props.dispatch({ type: 'SET_FILTER', filter: e.target.value }); }} />
                <table className="table table-striped table-condensed">
                    <thead>
                        <tr>
                            {['Id', 'Name', 'Category', 'Breed', 'Price', 'CreatedDate'].map(function (col) {
                                return <th key={col} style={{ cursor: 'pointer' }} onClick={function () { self.props.dispatch({ type: 'SET_SORT', sortBy: col }); }}>{col}</th>;
                            })}
                            <th>Sold</th><th></th>
                        </tr>
                    </thead>
                    <tbody>
                        {this.sorted().map(function (p) {
                            return (
                                <tr key={p.Id}>
                                    <td>{p.Id}</td>
                                    <td><a href={'/Pets/Details/' + p.Id}>{p.Name}</a> {p.IsFeatured ? <span className="label label-info">featured</span> : null}</td>
                                    <td>{p.Category}</td>
                                    <td>{p.Breed}</td>
                                    <td>${p.Price}</td>
                                    <td>{p.CreatedDate}</td>
                                    <td><input type="checkbox" checked={p.IsSold} onChange={function () { self.props.dispatch({ type: 'TOGGLE_SOLD', id: p.Id }); }} /></td>
                                    <td>
                                        <a href={'/Admin/EditPet/' + p.Id} className="btn btn-default btn-xs">Edit</a>{' '}
                                        <a href={'/Admin/DeletePet/' + p.Id} className="btn btn-danger btn-xs" onClick={function (e) { if (!confirm('Delete ' + p.Name + '?')) e.preventDefault(); }}>Delete</a>
                                    </td>
                                </tr>
                            );
                        })}
                    </tbody>
                </table>
                <p className="text-muted">{this.props.pets.length} pets</p>
            </div>
        );
    }
}

var ConnectedPetTable = ReactRedux.connect(function (state) {
    return { pets: state.pets, loading: state.loading, filter: state.filter, sortBy: state.sortBy, error: state.error };
})(PetTable);

var sagaMiddleware = createSagaMiddleware();
var adminStore = Redux.createStore(adminReducer, Redux.applyMiddleware(sagaMiddleware));
sagaMiddleware.run(rootSaga);
window.adminStore = adminStore; // for debugging

ReactDOM.render(
    <ReactRedux.Provider store={adminStore}><ConnectedPetTable /></ReactRedux.Provider>,
    document.getElementById('admin-pet-table')
);
