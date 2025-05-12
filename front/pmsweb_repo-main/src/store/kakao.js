import { defineStore } from "pinia";
import apiClient from '@/api';

export const useKakaoStore = defineStore("kakao", {
  state: () => ({
    isLoading: false,
    error: null,
  }),
  actions: {
    async sendAlimtalk_Status(boardSeq, prevStatus, currentStatus, phone) {          
      if (!prevStatus || prevStatus.trim() === '') {
        console.log('이전 상태값이 없어 알림톡 발송을 중단합니다.');
        return false;
      }

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
          }).replace(/\. /g, '/').replace(/\./g, '')
            .replace(/(\d+)\/(\d+)\/(\d+)\//, '$1/$2/$3 ');
                
        const Message = '[삼표시멘트 업무지원센터]\n' 
                      + '접수 상태 변경 안내\n' 
                      + '\n'
                      + '■ 접수번호: ' + boardSeq + '\n' 
                      + '■ 이전상태: ' + prevStatus + '\n' 
                      + '■ 현재상태: ' + currentStatus + '\n' 
                      + '■ 변경일시: ' + currentDateTime; 
        const templateCode = 'STUS000002';        
                
        // console.log(Message);
        const response = await apiClient.post('/api/kakao', {        
          content: Message,
          phone: phone,
          templateCode: templateCode
        });

        if (response.data) {
          // console.log('알림톡 전송 결과:', response.data);
          return response.data;
        }
      } catch (error) {
        console.error('Error sending Alimtalk:', error);
        this.error = error;
        throw error;
      } finally {
        this.isLoading = false;
      }
    },

    async sendAlimtalk_Manager(title, managerName, authorName, phone) {        
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
          }).replace(/\. /g, '/').replace(/\./g, '')
            .replace(/(\d+)\/(\d+)\/(\d+)\//, '$1/$2/$3 ');
                
        const Message = '[삼표시멘트 업무지원센터] 업무 등록 안내\n'                          
                      + '\n'
                      + '안녕하세요, ' + managerName + '님께서\n'
                      + '아래 게시글의 담당자로 지정되었습니다.\n'
                      + '\n'
                      + '■ 게시글 제목: ' + title + '\n'
                      + '■ 등록자: ' + authorName + '\n'
                      + '■ 등록일시: ' + currentDateTime + '\n'
                      + '■ 담당자 지정일시: ' + currentDateTime + '\n'
                      + '\n'
                      + '게시글 확인 및 처리 부탁드립니다.';
        // console.log(Message);
        const templateCode = 'STUS000003';        

        const response = await apiClient.post('/api/kakao', {        
          content: Message,
          phone: phone,
          templateCode: templateCode
        });

        if (response.data) {
          // console.log('알림톡 전송 결과:', response.data);
          return response.data;
        }
      } catch (error) {
        console.error('Error sending Alimtalk_2:', error);
        this.error = error;
        throw error;
      } finally {
        this.isLoading = false;
      }
    }    
  },
});