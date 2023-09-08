const path = require('path');
const { VueLoaderPlugin } = require('vue-loader/dist/index')
const { CleanWebpackPlugin } = require('clean-webpack-plugin')
const { DefinePlugin } = require('webpack')

module.exports = {
    entry: {
        MediaChart: './src/Media/Chart/main.ts',
        MediaExternalLink: './src/Media/ExternalLink/main.ts'
    },
    output: {
        filename: '[name].entry.js',
        path: path.resolve(__dirname, '..', 'wwwroot', 'dist')
    },
    devtool: 'source-map',
    mode: 'development',
    module: {
        rules: [
            {
                test: /\.tsx?$/,
                exclude: /node_modules/,
                use: [
                    {
                      loader: 'ts-loader',
                      options: {
                        transpileOnly: true
                      }
                    }
                  ]
            },
            {
                test: /\.css$/,
                use: ['style-loader', 'css-loader'],
            },
            {
                test: /\.(eot|woff(2)?|ttf|otf|svg)$/i,
                type: 'asset'
            },
            {
                test: /\.vue$/,
                use: 'vue-loader',
            },
        ]
    },
    plugins: [
        new VueLoaderPlugin(),
        new CleanWebpackPlugin(),
        new DefinePlugin({
            __VUE_PROD_DEVTOOLS__: false,
            __VUE_OPTIONS_API__: false,
        }),
    ]
};
