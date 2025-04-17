import { defineStore } from "pinia";
import apiClient from '@/api';

export const useKakaoStore = defineStore("kakao", {
  state: () => ({
    isLoading: false,
    error: null,
  }),
  actions: {
    async sendAlimtalk(boardSeq, prevStatus, currentStatus) {
      console.log('-- sendAlimtalk --');
      console.log('receiptNum:', receiptNum);
      console.log('prevStatus:', prevStatus);
      console.log('currentStatus:', currentStatus);
      
      try {
        this.isLoading = true;

        const currentDateTime = new Date().toLocaleString('ko-KR', {
          year: 'numeric',
          month: '2-digit',
          day: '2-digit',
          hour: '2-digit',
          minute: '2-digit',
          second: '2-digit',
          hour12: false
        }).replace(/\. /g, '-').replace(/\./g, '').replace(/:/g, ':');

        const Message = '[삼표시멘트 업무지원센터]\n' 
                      + '접수 상태 변경 안내\n' 
                      + '■ 접수번호: ' + boardSeq + '\n' 
                      + '■ 이전상태: ' + prevStatus + '\n' 
                      + '■ 현재상태: ' + currentStatus + '\n' 
                      + '■ 변경일시: ' + currentDateTime; 

        const response = await apiClient.post('/api/kakao', {
          content: Message
        });

        if (response.data) {
          console.log('알림톡 전송 결과:', response.data);
          return response.data;
        }
      } catch (error) {
        console.error('Error sending Alimtalk:', error);
        this.error = error;
        throw error;
      } finally {
        this.isLoading = false;
      }
    }
  },
});