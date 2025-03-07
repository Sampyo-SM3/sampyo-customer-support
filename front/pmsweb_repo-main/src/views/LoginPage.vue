<template>
  <v-app>
    <v-main class="bg-gradient-elegant-blue">
      <v-container fluid class="fill-height pa-0">
        <v-row no-gutters align="center" justify="center" class="fill-height">
          <v-card class="elevation-16 rounded-lg d-flex" width="1000" max-width="90%">
            <v-col cols="12" md="7" class="pa-0 d-none d-md-flex">
              <v-img
                src="@/assets/login_side.jpg"
                cover
                height="100%"
                width="100%"
                class="rounded-l-lg"
              ></v-img>
            </v-col>
            <v-col cols="12" md="5">
              <v-card-text class="text-center pa-8 pa-sm-12">
                <h1 class="text-h3 font-weight-bold mb-2">Welcome</h1>
                <h2 class="text-h5 font-weight-bold mb-8">sampyo cement</h2>
                <v-form @submit.prevent="login" class="mb-8">
                  <v-text-field
                    v-model="username"
                    label="Account"
                    prepend-inner-icon="mdi-account"
                    variant="outlined"
                    class="mb-4"
                    color="primary"
                  ></v-text-field>
                  <v-text-field
                    v-model="password"
                    label="Password"
                    prepend-inner-icon="mdi-lock"
                    :append-inner-icon="showPassword ? 'mdi-eye' : 'mdi-eye-off'"
                    @click:append-inner="showPassword = !showPassword"
                    :type="showPassword ? 'text' : 'password'"
                    variant="outlined"
                    color="primary"
                    class="mb-2"
                  ></v-text-field>
                  <div class="text-right mb-4">
                    <v-btn variant="text" color="primary" density="compact" class="text-caption">
                      비밀번호를 잊으셨나요?
                    </v-btn>
                  </div>
                  <v-btn
                    block
                    color="primary"
                    size="large"
                    type="submit"
                    class="mb-6"
                    elevation="2"
                  >
                    로그인
                  </v-btn>
                </v-form>
                <div class="text-divider mb-6" style="color: #808080;">간편 로그인</div>
                <div class="text-center">
                  <v-btn
                    icon="mdi-google"
                    color="surface"
                    elevation="2"
                    @click="googleLogin"
                    class="rounded-circle"
                  >
                    <v-icon color="error">mdi-google</v-icon>
                  </v-btn>
                </div>
              </v-card-text>
            </v-col>
          </v-card>
        </v-row>
      </v-container>
    </v-main>
  </v-app>
</template>

<script>
import { ref } from 'vue'
import { useRouter } from 'vue-router'

// import axios from 'axios'

// const apiResponse = ref('')
const loading = ref(false)
const error = ref(null)


export default {
  name: 'LoginPage',
  setup() {
    const router = useRouter()
    const username = ref('')
    const password = ref('')
    const showPassword = ref(false)

    const login = async () => {
      console.log('로그인 시도:', username.value, password.value)

      // test

      loading.value = true
      error.value = null
      
      /*
      try {
        // Spring Boot API 엔드포인트 호출
        const response = await axios.get('http://localhost:8080/api')
        
        // 응답 데이터를 변수에 저장
        apiResponse.value = response.data
        
        // 콘솔에 결과 출력
        console.log('API 응답 결과:', response.data)
      } catch (err) {
        error.value = 'API 호출 중 오류가 발생했습니다: ' + err.message
        console.error('API 호출 오류:', err)
      } finally {
        loading.value = false
      }  
        */
       
        
        // try {
        //   console.log('1');
        //   // Spring Boot API 엔드포인트 호출 - 변경된 URL 사용
        //   // const response = await axios.get('http://localhost:29000/api/user?id=12345')      
        //   const response = await axios.get('http://192.168.41.153:29000/api/users?id=123')      
        //   console.log('2');                    

        //   // 응답 데이터를 변수에 저장
        //   apiResponse.value = response.data
          
        //   // 콘솔에 결과 출력
        //   console.log('API 응답 결과:', response.data)
          
        //   // 사용자 정보 메시지로 표시
        //   const userData = response.data
        //   const formattedDate = new Date(userData.enter_dt).toLocaleDateString()
          
        //   alert(`
        //     사용자 정보:
        //     ID: ${userData.id}
        //     이름: ${userData.name}
        //     나이: ${userData.age}
        //     이메일: ${userData.email}
        //     가입일: ${formattedDate}
        //   `)
          
        // } catch (err) {
        //   error.value = 'API 호출 중 오류가 발생했습니다: ' + err.message
        //   console.error('API 호출 오류:', err)                    
        // } finally {
        //   loading.value = false
        // }        


      // 여기에 실제 로그인 로직을 구현하세요
      // 예를 들어, API 호출을 통한 인증 등
      
      // 로그인 성공 후 MainPage로 이동
      router.push({ name: 'Main' })

      
    }

    const googleLogin = () => {
      console.log('Google 로그인 시도')
      // 여기에 Google 로그인 로직을 구현하세요
    }

    return {
      username,
      password,
      showPassword,
      login,
      googleLogin
    }
  }
}
</script>

<style>
.bg-gradient-elegant-blue {
  background: linear-gradient(135deg,
     rgba(142, 168, 195, 0.8) 0%,
     rgba(236, 242, 247, 0.7) 100%
  );
}

.v-main {
  display: flex;
  align-items: center;
  min-height: 100vh;
  width: 100vw;
}

.v-container {
  height: 100vh;
  width: 100vw;
}

.v-card {
  margin: 0 auto;
  box-shadow: 0 4px 30px rgba(0, 0, 0, 0.1);
  background-color: rgba(255, 255, 255, 0.9);
  overflow: hidden;
}

.v-btn {
  text-transform: none;
  font-weight: 500;
}

.text-divider {
  display: flex;
  align-items: center;
  --text-divider-gap: 1rem;
}

.text-divider::before,
.text-divider::after {
  content: '';
  height: 1px;
  background-color: silver;
  flex-grow: 1;
}

.text-divider::before {
  margin-right: var(--text-divider-gap);
}

.text-divider::after {
  margin-left: var(--text-divider-gap);
}

@media (max-width: 960px) {
  .v-card {
    width: 100% !important;
    max-width: 100% !important;
    height: 100vh !important;
    border-radius: 0 !important;
  }
}
</style>