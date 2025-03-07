// Styles
import '@mdi/font/css/materialdesignicons.css'
import 'vuetify/styles'

// Vuetify
import { createVuetify } from 'vuetify'

export default createVuetify({
  theme: {
    defaultTheme: 'light'
  },
  defaults: {
    VApp: {
      style: 'font-family: "Noto Sans KR", sans-serif;'
    }
  }
})
