import CA1000_20 from '@/views/CA/CA1000_20.vue'
import CA1000_10 from '@/views/CA/CA1000_10.vue'

export const caRoutes = [
  {
    path: '/CA1000_20/:receivedSeq?',
    name: 'CA1000_20',
    component: CA1000_20,
    props: true
  },  
  {  
    path: 'ca1000_10', // receivedSeq 파라미터를 선택적으로 받음
    name: 'CA1000_10',
    component: CA1000_10,    
  },    
  
  // 다른 라우트 정의...
]

export default caRoutes