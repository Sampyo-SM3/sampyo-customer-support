const { defineConfig } = require('@vue/cli-service')
const TerserPlugin = require('terser-webpack-plugin')

module.exports = defineConfig({
  transpileDependencies: true,

  devServer: {
    historyApiFallback: true,
    // proxy: {
    //   '/api': {
    //     target: 'https://csr.sampyo.co.kr', // Spring 서버 주소        
    //     changeOrigin: true,
    //     secure: false,
    //   }
    // }    
  },

  pluginOptions: {
    vuetify: {
      // https://github.com/vuetifyjs/vuetify-loader/tree/next/packages/vuetify-loader
    }
  },

  // 운영 빌드시 console.log 제거
  configureWebpack: config => {
    if (process.env.NODE_ENV === 'production') {
      config.optimization.minimizer = [
        new TerserPlugin({
          terserOptions: {
            compress: {
              drop_console: true,
              drop_debugger: true,
              pure_funcs: ['console.log', 'console.info', 'console.debug', 'console.warn']
            },
            mangle: true
          },
          extractComments: false
        })
      ]
    }
  }
})