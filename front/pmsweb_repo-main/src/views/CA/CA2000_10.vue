<template>
    <v-container fluid class="pr-5 pl-5 pt-7">
  
      <!-- test -->
      <v-btn @click="test()">SR요청서 띄우기 테스트</v-btn>
      <br>
      <br>      
      <v-btn @click="test2()">알림톡테스트</v-btn>
      <br>
      <br>      
      <v-btn @click="test3()">메일테스트</v-btn>
      <br>
      <br>
      
  
  

  
      
      
      
    </v-container>

  </template>

<script>
  import apiClient from '@/api';
  
  export default {
    data() {
      return {
     
      }
    },
  
  
    methods: {
      test () {
        console.log('--test--');

        try {       
          // 폼 타입 결정
          let formType = ''
          formType = 'WF_FORM_LEGACY_FI_STATE_UNBAN'
          formType = 'WF_FORM_SR'
          // formType = 'WF_FORM_SR_V0'
          
          // URL 및 파라미터 설정
          // const baseUrl = 'https://bluesam.sampyo.co.kr/WebSite/Approval/Forms/FormLinkForLEGACY.aspx'
          const baseUrl = 'https://bluesam.sampyo.co.kr/WebSite/Approval/Forms/FormLinkForLEGACY.aspx'        
          const params = {
            key: 1,
            empno: 1,
            legacy_form: formType,
            datatype: 'xml',
            ip: '127.0.0.1',
            db: 'tttt'
          }
          
          // 쿼리 파라미터 문자열 생성
          const queryString = new URLSearchParams(params).toString()
          const fullUrl = `${baseUrl}?${queryString}`
          
          // 새 창에서 URL 열기
          window.open(fullUrl, '_blank')
          
          
        } catch (error) {
          console.error('상신 처리 중 오류 발생:', error)
        }      
      },



      async test2() {        
        try {
          this.isLoading = true;
          
          const errorMessage = '2024-01-01 00:00:00\n' +
                          'test\n' + 
                          '오류가 발생했습니다.';

          const response = await apiClient.post('/api/kakao', {
            content: errorMessage
          });          
          
          if (response.data) {
            console.log(response.data);
          }
        } catch (error) {
          console.error('Error fetching user info:', error);
          this.error = '사용자 정보를 가져오는 중 오류가 발생했습니다';
        } finally {
          this.isLoading = false;
        }
      },      

      async test3() {  
        this.isLoading = true;
  
        try {
          // FormData 생성
          const formData = new FormData();
          formData.append('to', 'javachohj@sampyo.co.kr');
          formData.append('subject', 'test');
          formData.append('message', 'test');
          
     
          
          // API 호출
          const response = await apiClient.post('/api/email/send', formData, {
            headers: {
              'Content-Type': 'multipart/form-data'
            }
          });
          
          // 성공 처리                    
          console.log(response.data);
                 
  
          
        } catch (error) {
          console.error('이메일 전송 실패:', error);
 
        } finally {
          this.isLoading = false;
        }        
      }
   
   
    },
  

  }</script>
