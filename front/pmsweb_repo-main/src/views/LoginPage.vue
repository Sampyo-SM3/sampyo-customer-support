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

  <!-- 스낵바로 오류 메시지 표시 -->
  <v-snackbar
    v-model="showError"
    color="warning"
    timeout="5000"
    location="center"
    elevation="8"              
    variant="elevated" 
  >
    {{ errorMessages }}
    
    <template v-slot:actions>
      <v-btn
        variant="text"
        @click="showError = false"
      >
        닫기
      </v-btn>
    </template>
  </v-snackbar>  
</template>

<script>
import { useAuthStore } from '@/store/auth';


export default {
  name: 'LoginPage',
  data() {
    return {
      username: '',
      password: '',
      showPassword: false,
      loading: false,
      showError: false,
      errorMessages: '',
      
    }
  },
  computed: {
    // 컴포넌트 내에서 authStore에 접근할 수 있게 함
    authStore() {
      return useAuthStore();
    }
  },  
  methods: {
    async login() {      
      this.loading = true;
      let result;

      try {        
        if (this.username != 'admin') {     
          // 블루샘 계정테이블에 있는 아이디인지 먼저 확인
          result = await this.authStore.validate_blue_id({
            username: this.username,
            password: this.password
            // username: '1',
            // password: '1' 
          });    
        } else {
          result = '관리자';  
        }
             
        if (result) {    
          // 우리쪽 계정테이블에 데이터 없으면 insert후 로그인
          // 있으면 비밀번호 검증 후 로그인
          const success2 = await this.authStore.login({
            username: this.username,
            password: this.password,
            name: result
            // username: '1',
            // password: '1' 
          }); 

          if (success2) {
            this.$router.push({ name: 'Main' });
          } else {
            // 로그인 실패 (authStore에서 false 반환)
            this.showError = true;
            this.errorMessages = this.authStore.error || '로그인에 실패했습니다.';
          }   

        } else {
          // 로그인 실패 (authStore에서 false 반환)
          this.showError = true;
          this.errorMessages = this.authStore.error || '로그인에 실패했습니다.';
        }      
        
           
      } catch (error) {
        console.error('로그인 처리 중 오류:', error);
        this.errorMessages = '로그인 처리 중 오류가 발생했습니다.';
        this.showError = true;            
      } finally {
        this.loading = false;
      }
    },



    // async login() {
    //   this.loading = true;
      
    //   try {        
    //     const success = await this.authStore.login({
    //       username: this.username,
    //       password: this.password
    //       // username: '1',
    //       // password: '1' 
    //     });        
        
    //     if (success) {
    //       this.$router.push({ name: 'Main' });
    //     } else {
    //       // 로그인 실패 (authStore에서 false 반환)
    //       this.showError = true;
    //       this.errorMessages = this.authStore.error || '로그인에 실패했습니다.';
    //     }      
    //   } catch (error) {
    //     console.error('로그인 처리 중 오류:', error);
    //     this.errorMessages = '로그인 처리 중 오류가 발생했습니다.';
    //     this.showError = true;            
    //   } finally {
    //     this.loading = false;
    //   }
    // },  
    
   


    googleLogin() {
      // 여기에 Google 로그인 로직을 구현하세요
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