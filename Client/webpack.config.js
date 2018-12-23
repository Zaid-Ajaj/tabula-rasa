const path = require("path");
const webpack = require("webpack");
const BundleAnalyzerPlugin = require('webpack-bundle-analyzer').BundleAnalyzerPlugin;

function resolve(filePath) {
    return path.join(__dirname, filePath)
}

var babelOptions = {
    presets: [
      ["@babel/preset-env", {
          "modules": false,
          "useBuiltIns": "entry",
      }]
    ]
};

var isProduction = process.argv.indexOf("-p") >= 0;
console.log("Bundling for " + (isProduction ? "production" : "development") + "...");

module.exports = {
    entry: resolve('./src/TabulaRasa.Client.fsproj'),
    output: {
        filename: 'bundle.js',
        path: resolve('./public'),
    },
    resolve: {
        modules: [
            "node_modules", resolve("./node_modules/")
        ]
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
                use: {
                    loader: "fable-loader",
                    options: {
                        babel: babelOptions,
                        define: isProduction ? [] : ["DEBUG"]
                    }
                }
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
        // generate bundle report in production build
        new BundleAnalyzerPlugin({
            generateStatsFile: true,
            analyzerMode: "static"
        })
    ] : [
        new webpack.HotModuleReplacementPlugin(),
        new webpack.NamedModulesPlugin()
    ]
};
