import { createApp } from 'vue'
import App from './App.vue'
import router from './router'  // 새로 만든 라우터 파일을 import
import axios from 'axios'
import vuetify from './plugins/vuetify'
import { loadFonts } from './plugins/webfontloader'
import { createPinia } from 'pinia'
import '@mdi/font/css/materialdesignicons.css'

loadFonts()

const app = createApp(App)
const pinia = createPinia()

app.use(vuetify)
app.use(pinia)
app.use(router)
app.config.globalProperties.$axios = axios // 전역 프로퍼티로 추가
app.mount('#app')