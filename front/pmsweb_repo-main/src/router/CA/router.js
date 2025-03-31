import CA1000_10 from '@/views/CA/CA1000_10.vue'
import CA_PostCreateForm from '@/views/CA/CA_PostCreateForm.vue'
import CA_PostCreateSrForm from '@/views/CA/CA_PostCreateSrForm.vue'
import CA_PostDetailForm from '@/views/CA/CA_PostDetailForm.vue'
import CA_PostDetailForm2 from '@/views/CA/CA_PostDetailForm2.vue'
import CA2000_10 from '@/views/CA/CA2000_10.vue'


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
  {
    path: '/CA_PostDetailForm2/:receivedSeq?', // receivedSeq 파라미터를 선택적으로 받음
    name: 'CA_PostDetailForm2',
    component: CA_PostDetailForm2,
    props: true
  },  
  {  
    path: 'CA2000_10', 
    name: 'CA2000_10',
    component: CA2000_10,    
  }, 
  {  
    path: '/CA_PostCreateSrForm/:receivedSeq?', 
    name: 'CA_PostCreateSrForm',
    component: CA_PostCreateSrForm,    
    props: true
  },    
  
]

export default caRoutes