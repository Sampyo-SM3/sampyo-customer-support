import Home from '@/views/Home.vue'
import CA1010_10 from '@/views/CA/CA1010_10.vue'
import CA1000_20 from '@/views/CA/CA1000_20.vue'

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
  {
    path: '/test2',
    name: 'CA1000_20',
    component: CA1000_20
  },  
  
  // 다른 라우트 정의...
]