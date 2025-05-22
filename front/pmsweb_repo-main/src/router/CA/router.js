import CA1000_10 from '@/views/CA/CA1000_10.vue'
import CA_PostCreateForm from '@/views/CA/CA_PostCreateForm.vue'
import CA_PostEditForm from '@/views/CA/CA_PostEditForm.vue'
import CA_PostEditSrForm from '@/views/CA/CA_PostEditSrForm.vue'
import CA_PostDetailSrForm from '@/views/CA/CA_PostDetailSrForm.vue'
import CA_PostDetailForm from '@/views/CA/CA_PostDetailForm.vue'
import CA2000_10 from '@/views/CA/CA2000_10.vue'
import CA_LibraryCreateForm from '@/views/CA/CA_LibraryCreateForm.vue'
import CA_LibraryDetailForm from '@/views/CA/CA_LibraryDetailForm.vue'
import CA_LibraryEditForm from '@/views/CA/CA_LibraryEditForm.vue'

export const caRoutes = [
  {
    path: 'CA_PostDetailForm/:receivedSeq?', // ✅ 상대경로
    name: 'CA_PostDetailForm',
    component: CA_PostDetailForm,
    props: true
  },
  {
    path: 'CA_PostDetailSrForm/:receivedSeq?',
    name: 'CA_PostDetailSrForm',
    component: CA_PostDetailSrForm,
    props: true
  },
  {
    path: 'ca1000_10',
    name: 'CA1000_10',
    component: CA1000_10
  },
  {
    path: 'CA_PostCreateForm',
    name: 'CA_PostCreateForm',
    component: CA_PostCreateForm
  },
  {
    path: 'CA_PostEditForm/:receivedSeq?',
    name: 'CA_PostEditForm',
    component: CA_PostEditForm,
    props: true
  },
  {
    path: 'CA2000_10',
    name: 'CA2000_10',
    component: CA2000_10
  },
  {
    path: 'CA_PostEditSrForm/:receivedSeq?',
    name: 'CA_PostEditSrForm',
    component: CA_PostEditSrForm,
    props: true
  },
  {
    path: 'CA_LibraryCreateForm',
    name: 'CA_LibraryCreateForm',
    component: CA_LibraryCreateForm,
    props: true
  },
  {
    path: 'CA_LibraryDetailForm/:receivedSeq?',
    name: 'CA_LibraryDetailForm',
    component: CA_LibraryDetailForm,
    props: true
  },
  {
    path: 'CA_LibraryEditForm/:receivedSeq?',
    name: 'CA_LibraryEditForm',
    component: CA_LibraryEditForm,
    props: true
  },
  
]

export default caRoutes
