const path = require('path');
const { VueLoaderPlugin } = require('vue-loader/dist/index')
const { CleanWebpackPlugin } = require('clean-webpack-plugin')
const { DefinePlugin } = require('webpack')
// const MiniCssExtractPlugin = require("mini-css-extract-plugin");
const AutoImport = require('unplugin-auto-import/webpack')
const Components = require('unplugin-vue-components/webpack')
const { ElementPlusResolver } = require('unplugin-vue-components/resolvers')
const AbpRazorWebpackPlugin = require('./plugin/AbpRazorWebpackPlugin/index.js')

module.exports = {
    entry: {
        MediaChart: './src/Media/Chart/main.ts',
        LotteryStatistics: './src/Lottery/Statistics/main.ts',
        LotteryBatchCreate:'./src/Lottery/LotteryBatchCreate/main.ts',
        QueueSink: './src/LogSink/QueueSink/main.ts',
        SignalRSink: './src/LogSink/SignalRSink/main.ts',
        ExpenditureAnalysis: './src/Expenditure/Analysis/main.ts',
        FileUpload:'./src/FileUpDownload/Upload/main.ts',
        TGLogin:'./src/TG/Login/main.ts'
    },
    output: {
        filename: '[name].entry.js',
        path: path.resolve(__dirname, '..', 'wwwroot', 'dist')
    },
    optimization: {
        splitChunks: {
            chunks: 'all',
            // maxSize: 393216
        },
    },
    // devtool: 'source-map',
    // mode: 'development',
    mode: 'production',
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
                // use: [{ loader: MiniCssExtractPlugin.loader }, 'css-loader']
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
        // new MiniCssExtractPlugin({
        //     filename: "[name].entry.css"
        // })
        AutoImport({
            resolvers: [
                ElementPlusResolver(),
            ],
        }),
        Components({
            resolvers: [
                ElementPlusResolver()
            ],
        }),

        new AbpRazorWebpackPlugin({ path: 'dist', htmlpath: '../Pages' })
    ]
};
