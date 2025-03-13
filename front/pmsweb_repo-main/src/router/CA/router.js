import CA1010_10 from '@/views/CA/CA1010_10.vue'
import CA1000_20 from '@/views/CA/CA1000_20.vue'
import CA1000_10 from '@/views/CA/CA1000_10.vue'

export const caRoutes = [
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
  {  
    path: 'ca1000_10/:seq?', // seq 파라미터를 선택적으로 받음
    name: 'CA1000_10',
    component: CA1000_10
  },    
  
  // 다른 라우트 정의...
]

export default caRoutes