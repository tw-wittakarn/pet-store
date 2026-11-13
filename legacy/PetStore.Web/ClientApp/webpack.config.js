var path = require('path');

module.exports = {
    entry: ['@babel/polyfill', './src/index.js'],
    output: {
        path: path.resolve(__dirname, '../wwwroot/Scripts/dist'),
        filename: 'cart.bundle.js'
    },
    module: {
        rules: [
            { test: /\.jsx?$/, exclude: /node_modules/, use: 'babel-loader' }
        ]
    },
    resolve: { extensions: ['.js', '.jsx'] },
    externals: {
        jquery: 'jQuery'
    },
    performance: { hints: false }
};
