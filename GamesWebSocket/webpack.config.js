const path = require('path');

module.exports = {
  entry: './wwwroot/scripts/app.js',
  module: {
    rules: [
      {
         exclude: /node_modules/,
         test: /\.tsx?$/,
         use: 'ts-loader',
      },
    ],
  },
  resolve: {
    extensions: ['.js','.tsx', '.ts'],
  },
  output: {
    library: {
      name: 'app_name',
      type: 'var'
    },
    filename: 'bundle.js',
    path: path.resolve(__dirname, '../scripts'),
  }
};