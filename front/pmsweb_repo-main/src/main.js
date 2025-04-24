import { createApp } from 'vue';
import App from './App.vue';
import router from './router';
import axios from 'axios';
import vuetify from './plugins/vuetify';
import { loadFonts } from './plugins/webfontloader';
import { createPinia } from 'pinia';
import '@mdi/font/css/materialdesignicons.css';
import Toast, { POSITION } from 'vue-toastification';  // vue-toastification import
import 'vue-toastification/dist/index.css';  // toast 스타일 import

loadFonts();

const app = createApp(App);

const pinia = createPinia();

// Toast 설정 (한 번만 호출)
app.use(Toast, {
  position: POSITION.TOP_RIGHT,  // 위치 설정 (위 오른쪽)
  timeout: 3000,  // Toast가 자동으로 사라지기까지의 시간 (밀리초)
});

app.use(vuetify);
app.use(pinia);
app.use(router);
app.config.globalProperties.$axios = axios;  // 전역 프로퍼티로 axios 추가

app.mount('#app');
