const path = require("path");
const webpack = require("webpack");

module.exports = {
    mode: "development",
    entry: {
        "site": "site.ts",
        "manage/app": "manage/app.ts",
        "room/app": "room/app.ts",
    },
    output: {
        path: path.resolve(__dirname, "wwwroot/js"),
        filename: "[name].js",
    },
    resolve: {
        modules: [
            "node_modules",
            path.join(__dirname, "wwwroot/src/ts")
        ],
        extensions: [".ts", ".js"]
    },
    module: {
        rules: [
            {
                test: /\.ts$/,
                use: [
                    { loader: "ts-loader" }
                ]
            }
        ]
    },
    devtool: "inline-source-map",
    externals: {
    },
    plugins: [
    ]
}