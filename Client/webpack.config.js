var path = require("path");
var webpack = require("webpack");
const BundleAnalyzerPlugin = require('webpack-bundle-analyzer').BundleAnalyzerPlugin;

function resolve(filePath) {
    return path.join(__dirname, filePath)
}

var babelOptions = {
    presets: [
      ["@babel/preset-env", {
          "modules": false,
          "useBuiltIns": "usage",
      }]
    ]
};

module.exports = function (evn, argv) {
    var mode = argv.mode || "development";
    var isProduction = mode === "production";
    console.log("Webpack mode: " + mode);
    return {
        devtool: "source-map",
        entry: resolve('./src/TabulaRasa.Client.fsproj'),
        output: {
            filename: 'bundle.js',
            path: resolve('./public'),
        },
        devServer: {
            proxy: {
                '/api/*': {
                  target: 'http://localhost:8080',
                  changeOrigin: true
                }, 
                '/socket': {
                    target: 'http://localhost:8080',
                    ws: true
                }
            },
            contentBase: resolve('./public'),
            port: 8090,
            hot: true,
            inline: true
        },
        module: {
            rules: [
                {
                    test: /\.fs(x|proj)?$/,
                    use: "fable-loader"
                },
                {
                    test: /\.js$/,
                    exclude: /node_modules/,
                    use: {
                        loader: 'babel-loader',
                        options: babelOptions
                    },
                },
                {
                    test: /\.(sa|c)ss$/,
                    use: [
                        "style-loader",
                        "css-loader",
                        "sass-loader"
                    ]
                }
            ]
        },
        plugins: isProduction ? [
            new BundleAnalyzerPlugin({
                generateStatsFile: true,
                analyzerMode:"static"
            })
        ] : [
            new webpack.HotModuleReplacementPlugin(),
            new webpack.NamedModulesPlugin()
        ]
    };
};