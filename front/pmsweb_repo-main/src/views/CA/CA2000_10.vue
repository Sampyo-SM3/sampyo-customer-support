<template>
    <v-container fluid class="pr-5 pl-5 pt-7">
  
      <!-- test -->
      <v-btn @click="test1()">SR요청서 띄우기 테스트</v-btn>
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
        // loading.value = true;
  
        // try {
        //   // FormData 생성 (파일 첨부를 위해)
        //   const formData = new FormData();
        //   formData.append('to', email.to);
        //   formData.append('subject', email.subject);
        //   formData.append('message', email.message);
          
        //   // 첨부 파일 추가
        //   if (email.attachments.length > 0) {
        //     email.attachments.forEach(file => {
        //       formData.append('attachments', file);
        //     });
        //   }
          
        //   // API 호출
        //   const response = await axios.post('/api/email/send', formData, {
        //     headers: {
        //       'Content-Type': 'multipart/form-data'
        //     }
        //   });
          
        //   // 성공 처리
        //   snackbar.text = '이메일이 성공적으로 전송되었습니다.';
        //   snackbar.color = 'success';
        //   snackbar.show = true;
          
        //   // 폼 초기화
        //   resetForm();
          
        // } catch (error) {
        //   console.error('이메일 전송 실패:', error);
          
        //   // 오류 처리
        //   snackbar.text = '이메일 전송에 실패했습니다. 다시 시도해주세요.';
        //   snackbar.color = 'error';
        //   snackbar.show = true;
        // } finally {
        //   loading.value = false;
        // }           
      }
   
   
    },
  

  }</script>
