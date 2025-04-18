// stores/auth.js
import { defineStore } from "pinia";
import axios from "axios";
import apiClient from '@/api';

export const useAuthStore = defineStore("auth", {
  state: () => ({
    userId: null,
    userInfo: null,
    isAuthenticated: false,
    isLoading: false,
    error: null,
    token: null
  }),
  actions: {
    // 로그인 시도
    async login(credentials) {   
      console.log('--login--');
      try {
        this.isLoading = true;
        this.error = null;

        // 전화번호에서 하이픈(-) 제거
        const phoneWithoutHyphens = credentials.phone ? credentials.phone.replace(/-/g, '') : credentials.phone;        
              
        const response = await apiClient.post('/api/login', {
          id: credentials.username,
          password: credentials.password,          
          name: credentials.name,
          phone: phoneWithoutHyphens,
          email: credentials.email,
        });
        console.log('22)');

        // 로그인 성공
        if (response.data) {
          this.userId = response.data.id;
          this.userInfo = response.data;
          this.isAuthenticated = true;

          // 사용자 정보 로컬 스토리지에 저장
          localStorage.setItem('user_id', response.data.id);
          localStorage.setItem('userInfo', JSON.stringify(response.data));          
          localStorage.setItem('isAuthenticated', true);   

          // 토큰이 있는 경우 저장
          if (response.data.token) {
            this.token = response.data.token;
            localStorage.setItem('auth_token', response.data.token);
            // API 호출 시 인증 헤더 설정
            axios.defaults.headers.common['Authorization'] = `Bearer ${response.data.token}`;
          }
          

          
          return true;
        }
        
        return false;
      } catch (error) {
        this.error = error.response?.data?.message || '로그인 중 오류가 발생했습니다';
        console.error('Login error:', error);
        return false;
      } finally {
        this.isLoading = false;
      }
    },


    async validate_blue_id(credentials) {     
      // console.log('-- validate_blue_id --');       
      try {
        this.isLoading = true;
        this.error = null;
                
        const response = await apiClient.post('/api/validate-blue-id', {
          id: credentials.username,
          password: credentials.password,          
        });

        this.userId = response.data.id;
        this.userInfo = response.data;      
                                                   
        const responseData = {
          ...response.data,
          bool: true // boolean 필드 추가
        };        

        return responseData;        
      } catch (error) {        
        if (error.response) {
          // console.log('error-1');                              
          this.error = error.response.data.message;
          // console.log(this.error);
        } else if (error.request) {
          // console.log('error-2');          
          this.error = error.response.data.message;
        } else {          
          // console.log('error-3');          
          this.error = error.response.data.message;
        }
                 
        return {
          bool: false,
          data: this.error
        };

      } finally {
        this.isLoading = false;
      }
    },    
    
    // 로그아웃
    logout() {
      this.userId = null;
      this.userInfo = null;
      this.isAuthenticated = false;
      this.token = null;
      
      // 로컬 스토리지 정리
      localStorage.removeItem('auth_token');
      localStorage.removeItem('user_id');
      
      // API 헤더 제거
      delete axios.defaults.headers.common['Authorization'];
      
      // 필요한 경우 서버에 로그아웃 요청
      // axios.post('http://222222:8080/api/logout');
    },
    
    // 사용자 정보 가져오기 --> 아직 사용안하는중
    async fetchUserInfo() {
      if (!this.userId) return;
      
      try {
        this.isLoading = true;
        
        const response = await apiClient.get(`/api/user/${this.userId}`);
        
        if (response.data) {
          this.userInfo = response.data;
        }
      } catch (error) {
        console.error('Error fetching user info:', error);
        this.error = '사용자 정보를 가져오는 중 오류가 발생했습니다';
      } finally {
        this.isLoading = false;
      }
    },
    
    // 로그인 상태 확인 (페이지 새로고침 후에도 로그인 상태 유지)
    checkAuth() {
      const token = localStorage.getItem('auth_token');
      const userId = localStorage.getItem('user_id');
      
      if (token && userId) {
        this.token = token;
        this.userId = userId;
        this.isAuthenticated = true;
        axios.defaults.headers.common['Authorization'] = `Bearer ${token}`;
        
        // 필요한 경우 사용자 정보 다시 가져오기
        this.fetchUserInfo();
        
        return true;
      }
      
      return false;
    },
    
    // 에러 초기화
    clearError() {
      this.error = null;
    }
  },
  
  getters: {
    // 로그인 여부
    loggedIn: (state) => state.isAuthenticated,
    
    // 사용자 이름 (필요시)
    userName: (state) => state.userInfo?.name || '사용자',
    
    // 사용자 정보가 있는지 여부
    hasUserInfo: (state) => !!state.userInfo
  }
});