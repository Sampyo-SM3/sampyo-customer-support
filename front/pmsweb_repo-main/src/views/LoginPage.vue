<template>
  <v-app>
    <v-main class="bg-gradient-elegant-blue">
      <v-container fluid class="fill-height pa-0">
        <v-row no-gutters align="center" justify="center" class="fill-height">
          <v-card class="elevation-16 rounded-lg d-flex" width="1000" max-width="90%">
            <v-col cols="12" md="7" class="pa-0 d-none d-md-flex">
              <v-img src="@/assets/login_side.jpg" cover height="100%" width="100%" class="rounded-l-lg"></v-img>
            </v-col>
            <v-col cols="12" md="5">
              <v-card-text class="text-center pa-8 pa-sm-12">
                <!-- <h1 class="text-h3 font-weight-bold mb-2">Welcome</h1>
                <h2 class="text-h5 font-weight-bold mb-8">sampyo cement</h2> -->
                <v-img src="@/assets/SAMPYO_Cement_Signature_영문.png" class='mb-8 ml-8' alt="Sampyo Cement Logo"
                  style="width: 200px;" />
                <v-form @submit.prevent="login" class="mb-6">
                  <v-text-field v-model="username" label="Account" prepend-inner-icon="mdi-account" variant="outlined"
                    class="mb-4" color="primary"></v-text-field>
                  <v-text-field v-model="password" label="Password" prepend-inner-icon="mdi-lock"
                    :append-inner-icon="showPassword ? 'mdi-eye' : 'mdi-eye-off'"
                    @click:append-inner="showPassword = !showPassword" :type="showPassword ? 'text' : 'password'"
                    variant="outlined" color="primary"></v-text-field>
                  <v-btn block color="primary" size="large" type="submit" class="mb-6" elevation="2">
                    로그인
                  </v-btn>
                </v-form>
                <div class="text-right mb-4">
                  <v-btn variant="text" color="#787879" density="compact" class="text-subtitle2 font-weight-bold"
                    @click="openResetDialog">
                    비밀번호 초기화
                  </v-btn>
                </div>
                <!-- 개발 전으로 주석처리 -->
                <!-- <div class="text-divider mb-6" style="color: #808080;">간편 로그인</div>
                <div class="text-center">
                  <v-btn icon="mdi-google" color="surface" elevation="2" @click="googleLogin" class="rounded-circle">
                    <v-icon color="error">mdi-google</v-icon>
                  </v-btn>
                </div> -->
              </v-card-text>
            </v-col>
          </v-card>
        </v-row>
      </v-container>
    </v-main>
  </v-app>

  <!-- 스낵바로 오류 메시지 표시 -->
  <v-snackbar v-model="showError" color="warning" timeout="5000" location="center" elevation="8" variant="elevated">
    {{ errorMessages }}

    <template v-slot:actions>
      <v-btn variant="text" @click="showError = false">
        닫기
      </v-btn>
    </template>
  </v-snackbar>

  <!-- 비밀번호 초기화 confirm 창 -->
  <v-dialog v-model="showConfirm" persistent max-width="600" transition="dialog-bottom-transition">
    <v-card class="pa-6 rounded-lg" elevation="10">
      <div class="text-center mb-2">
        <v-icon color="warning" size="50">mdi-alert-circle-outline</v-icon>
      </div>

      <div class="text-h6 font-weight-bold text-center mb-2">
        비밀번호를 초기화하시겠습니까?
      </div>

      <!-- 버튼 -->
      <v-card-actions class="justify-center">
        <v-btn variant="outlined" color="#1E88E5" class="mr-2" @click="showConfirm = false">
          취소
        </v-btn>

        <v-btn variant="flat" color="#1E88E5" class="white--text" @click="confirmReset">
          초기화
        </v-btn>
      </v-card-actions>
    </v-card>
  </v-dialog>

  <!-- 성공 스낵바 -->
  <v-snackbar v-model="showSuccess" timeout="3000" color="success" elevation="2" class="center-snackbar">
    <div class="snackbar-text">
      {{ successMessage }}
    </div>
  </v-snackbar>

  <!-- 실패 스낵바 -->
  <v-snackbar v-model="showFail" timeout="2000" color="error" elevation="2" class="center-snackbar">
    <div class="snackbar-text">
      {{ errorMessage }}
    </div>
  </v-snackbar>
</template>

<script>
import { useAuthStore } from '@/store/auth';
import apiClient from '@/api';

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
      showConfirm: false,
      showSuccess: false,
      successMessage: '',
      showFail: false,
      errorMessage: ''
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
      // console.log('--로그인페이지 로그인버튼 클릭--')  
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

        if (!result.bool) {
          console.log('validate_blue_id 에러발생');
          this.showError = true;
          this.errorMessages = result.data;

          return;
        }

        let para_companyCd = result.companyCd;
        let para_name = result.name;
        let para_phone = result.phone;
        let para_email = result.email;
        let para_deptCd = result.deptCd;

        // 블루샘 아이디 존재여부 통과
        if (Object.keys(result).length > 0) {
          // console.log('-- 블루샘 아이디 존재여부 통과 --');
          // console.log(result.phone);          
          // 우리쪽 계정테이블에 데이터 없으면 insert후 로그인
          // 있으면 비밀번호 검증 후 로그인

          const success2 = await this.authStore.login({
            companyCd: result.companyCd,
            username: this.username,
            password: this.password,
            name: result.name,
            phone: result.phone,
            email: result.email,
            deptCd: result.deptCd,
            // username: '1',
            // password: '1' 
          });

          if (success2) {
            // console.log('success2');
            // this.$router.push({ name: 'Main' });
            // 처음 로그인할 경우 메뉴가 안보이는 현상 때문에 임시로 한번 더 로그인처리
            await this.login2(para_name, para_phone, para_email, para_deptCd, para_companyCd);
          } else {
            // 로그인 실패 (authStore에서 false 반환)
            this.showError = true;
            this.errorMessages = this.authStore.error || '로그인에 실패했습니다.-2';
          }

        } else {
          // 로그인 실패 (authStore에서 false 반환)
          this.showError = true;
          this.errorMessages = this.authStore.error || '로그인에 실패했습니다.-3';
        }


      } catch (error) {
        console.error('로그인 처리 중 오류:', error);
        this.errorMessages = '로그인 처리 중 오류가 발생했습니다.';
        this.showError = true;
      } finally {
        this.loading = false;
      }
    },

    // 처음 로그인할 경우 메뉴가 안보이는 현상 때문에 임시로 한번 더 로그인처리
    async login2(para_name, para_phone, para_email, para_deptCd, para_companyCd) {
      // console.log('--login2--');
      // console.log(para_name);
      // console.log(para_phone);
      // console.log(para_email);
      // console.log(para_deptCd);
      try {
        // console.log('-- 블루샘 아이디 존재여부 통과 --');
        // console.log(result.phone);          
        // 우리쪽 계정테이블에 데이터 없으면 insert후 로그인
        // 있으면 비밀번호 검증 후 로그인
        const success2 = await this.authStore.login({
          companyCd: para_companyCd,
          username: this.username,
          password: this.password,
          name: para_name,
          phone: para_phone,
          email: para_email,
          deptCd: para_deptCd
          // username: '1',
          // password: '1' 


        });
        // console.log('2');
        if (success2) {
          // console.log('3');
          this.$router.push({ name: 'Main' });
        } else {
          console.log('fail!!!');
          console.log(success2);
        }
      } catch (error) {
        console.error('로그인 처리 중 오류_2:', error);
        this.errorMessages = '로그인 처리 중 오류가 발생했습니다_2';
        this.showError = true;
      }
    },

    openResetDialog() {
      if (!this.username || this.username.trim() === '') {
        this.errorMessages = '아이디를 입력해주세요.';
        this.showError = true;
      } else {
        this.showConfirm = true;
      }
    },

    async confirmReset() {

      const res = await apiClient.get("/api/chkExistUserId", {
        params: { id: this.username }
      });

      if (!res.data) {
        this.errorMessage = '존재하지 않는 사용자입니다.';
        this.showFail = true;
        this.showConfirm = false;
        return;
      }

      try {
        await apiClient.post("/api/resetPassword", {
          id: this.username,
          newPw: 'sampyo1234'
        });

        this.successMessage = '초기 비밀번호는 sampyo1234입니다.';
        this.showSuccess = true;
      } catch (error) {
        this.errorMessage = '비밀번호 초기화에 실패했습니다. 관리자에게 문의하세요.';
        this.showFail = true;
      } finally {
        this.showConfirm = false;
      }
    }

  }
}
</script>

<style>
.bg-gradient-elegant-blue {
  background: linear-gradient(135deg,
      rgba(142, 168, 195, 0.8) 0%,
      rgba(236, 242, 247, 0.7) 100%);
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