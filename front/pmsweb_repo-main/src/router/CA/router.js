import CA1000_10 from '@/views/CA/CA1000_10.vue'
import CA_PostCreateForm from '@/views/CA/CA_PostCreateForm.vue'
import CA_PostEditForm from '@/views/CA/CA_PostEditForm.vue'
import CA_PostEditSrForm from '@/views/CA/CA_PostEditSrForm.vue'
import CA_PostDetailSrForm from '@/views/CA/CA_PostDetailSrForm.vue'
import CA_PostDetailForm from '@/views/CA/CA_PostDetailForm.vue'
import CA2000_10 from '@/views/CA/CA2000_10.vue'


export const caRoutes = [
  {
    path: '/CA_PostDetailForm/:receivedSeq?', // receivedSeq 파라미터를 선택적으로 받음
    name: 'CA_PostDetailForm',
    component: CA_PostDetailForm,
    props: true
  },    
  {
    path: '/CA_PostDetailSrForm/:receivedSeq?', // receivedSeq 파라미터를 선택적으로 받음
    name: 'CA_PostDetailSrForm',
    component: CA_PostDetailSrForm,
    props: true
  },  
  {  
    path: '/ca1000_10', 
    name: 'CA1000_10',
    component: CA1000_10,    
  },    
  {
    path: '/CA_PostCreateForm', 
    name: 'CA_PostCreateForm',
    component: CA_PostCreateForm,    
  },     
  {  
    path: '/CA_PostEditForm/:receivedSeq?', 
    name: 'CA_PostEditForm',
    component: CA_PostEditForm,    
    props: true
  },  
  {  
    path: '/CA2000_10', 
    name: 'CA2000_10',
    component: CA2000_10,    
  }, 
  {  
    path: '/CA_PostEditSrForm/:receivedSeq?', 
    name: 'CA_PostEditSrForm',
    component: CA_PostEditSrForm,    
    props: true
  },    
  
]


export default caRoutes