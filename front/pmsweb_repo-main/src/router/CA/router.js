import Home from '@/views/Home.vue'
import CA1010_10 from '@/views/CA/CA1010_10.vue'

export const routes = [
  {
    path: '/',
    name: 'Home',
    component: Home
  },
  {
    path: '/test',
    name: 'CA1010_10',
    component: CA1010_10
  },
  
  // 다른 라우트 정의...
]