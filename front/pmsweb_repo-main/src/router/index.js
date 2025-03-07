import { createRouter, createWebHistory } from 'vue-router'
import LoginPage from '@/views/LoginPage.vue'
import MainPage from '@/components/MainPage.vue'
import DynamicComponentLoader from '@/components/DynamicComponentLoader.vue'

const routes = [
  {
    path: '/',
    name: 'Login',
    component: LoginPage
  },
  {
    path: '/main',
    name: 'Main',
    component: MainPage,
    children: [
      {
        path: '/views/:folder/:file',
        name: 'DynamicComponent',
        component: DynamicComponentLoader
      }
    ]
  }
]

const router = createRouter({
  history: createWebHistory(process.env.BASE_URL),
  routes
})

export default router