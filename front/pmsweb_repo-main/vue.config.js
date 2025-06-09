const { defineConfig } = require('@vue/cli-service')

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
  }
})




