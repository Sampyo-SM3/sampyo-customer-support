import CA1000_10 from '@/views/CA/CA1000_10.vue'
import CA_PostCreateForm from '@/views/CA/CA_PostCreateForm.vue'
import CA_PostDetailForm from '@/views/CA/CA_PostDetailForm.vue'


export const caRoutes = [
  {
    path: '/CA_PostDetailForm/:receivedSeq?', // receivedSeq 파라미터를 선택적으로 받음
    name: 'CA_PostDetailForm',
    component: CA_PostDetailForm,
    props: true
  },  
  {  
    path: 'ca1000_10', 
    name: 'CA1000_10',
    component: CA1000_10,    
  },    
  {
    path: '/CA_PostCreateForm', 
    name: 'CA_PostCreateForm',
    component: CA_PostCreateForm,    
  },     
  // 다른 라우트 정의...
]

export default caRoutes