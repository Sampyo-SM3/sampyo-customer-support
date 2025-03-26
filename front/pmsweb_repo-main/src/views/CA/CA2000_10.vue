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
    
    <!-- 파일 업로드 컴포넌트 -->
    <v-card class="pa-4 mb-6">
      <v-card-title>파일 첨부 테스트</v-card-title>
      <v-card-text>
        <v-file-input
          v-model="newFiles"
          :rules="fileRules"
          accept="image/png, image/jpeg, application/pdf"
          label="파일 선택"
          placeholder="파일을 선택하세요"
          prepend-icon="mdi-paperclip"
          show-size
          counter
          multiple
          truncate-length="25"
          :loading="isFileLoading"
          @change="handleFileChange"
        ></v-file-input>
        
        <!-- 선택된 파일 목록 (아직 업로드되지 않은 파일) -->
        <div v-if="selectedFiles.length > 0" class="mt-4">
          <h3 class="text-subtitle-1 mb-2">선택된 파일</h3>
          <div class="selected-files">
            <div v-for="(file, index) in selectedFiles" :key="index" class="d-flex align-center py-2 border-bottom">
              <v-icon :icon="getFileIcon(file.type)" color="primary" class="me-2"></v-icon>
              <div class="file-info flex-grow-1">
                <div class="file-name text-body-1">{{ file.name }}</div>
                <div class="file-size text-body-2 text-grey">{{ formatFileSize(file.size) }}</div>
              </div>
              <v-btn
                icon="mdi-delete"
                variant="text"
                color="error"
                density="compact"
                @click="removeSelectedFile(index)"
              ></v-btn>
            </div>
          </div>
        </div>
      </v-card-text>
      
      <v-divider class="my-3"></v-divider>
      
      <v-card-text v-if="uploadedFiles.length > 0">
        <h3 class="text-subtitle-1 mb-2">업로드된 파일 목록</h3>
        <div class="selected-files mt-2">
          <div v-for="(file, index) in uploadedFiles" :key="index" class="d-flex align-center py-2 border-bottom">
            <v-icon :icon="getFileIcon(file.type)" color="primary" size="24" class="me-3"></v-icon>
            <div class="file-info flex-grow-1">
              <div class="file-name text-body-1 font-weight-medium">{{ file.name }}</div>
              <div class="file-size text-body-2 text-grey">{{ formatFileSize(file.size) }}</div>
            </div>
            <v-btn
              icon="mdi-delete"
              variant="text"
              color="error"
              density="compact"
              @click="removeFile(index)"
            ></v-btn>
          </div>
        </div>
      </v-card-text>
      
      <v-card-actions>
        <v-spacer></v-spacer>
        <v-btn
          color="primary"
          @click="uploadFiles"
          :disabled="!selectedFiles.length || isFileLoading"
          :loading="isFileLoading"
        >
          업로드
        </v-btn>
      </v-card-actions>
    </v-card>
    
    <!-- 파일 첨부 메일 테스트 -->
    <v-btn @click="test4()">파일 첨부 메일 테스트</v-btn>
    
  </v-container>
</template>

<script>
import apiClient from '@/api';

export default {
  data() {
    return {
      isLoading: false,
      // 파일 업로드 관련 데이터
      newFiles: [], // 새로 선택한 파일 (v-file-input에 연결됨)
      selectedFiles: [], // 업로드 대기 중인 파일들
      uploadedFiles: [], // 이미 업로드된 파일들
      isFileLoading: false,
      fileRules: [
        value => {
          return !value || !value.length || value[0].size < 5000000 || '파일 크기는 5MB 이하여야 합니다.';
        },
      ]
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
    },

    // 파일 타입에 따른 아이콘 반환
    getFileIcon(fileType) {
      if (fileType.includes('image')) {
        return 'mdi-file-image';
      } else if (fileType.includes('pdf')) {
        return 'mdi-file-pdf';
      } else {
        return 'mdi-file-document';
      }
    },

    // 파일 크기 포맷
    formatFileSize(size) {
      if (size < 1024) {
        return size + ' B';
      } else if (size < 1024 * 1024) {
        return (size / 1024).toFixed(2) + ' KB';
      } else {
        return (size / (1024 * 1024)).toFixed(2) + ' MB';
      }
    },

    // 파일 선택 변경 처리
    handleFileChange(event) {
      console.log('--handleFileChange--');
      console.log('이벤트 객체:', event);
      
      // 파일은 v-model에 바인딩된 newFiles에서 가져옵니다
      const files = this.newFiles;
      console.log('newFiles:', files);
      
      if (!files || (Array.isArray(files) && files.length === 0)) {
        console.log('선택된 파일 없음');
        return;
      }
      
      if (Array.isArray(files)) {
        console.log('여러 파일이 선택됨:', files.length);
        files.forEach((file, index) => {
          console.log(`파일[${index}] 이름:`, file.name);
          this.selectedFiles.push(file);
        });
      } else {
        console.log('단일 파일 선택됨 이름:', files.name);
        this.selectedFiles.push(files);
      }
      
      // 파일 선택 컨트롤 초기화
      this.newFiles = [];
    },
    
    // 선택된 파일 제거 (아직 업로드되지 않은 파일)
    removeSelectedFile(index) {
      this.selectedFiles.splice(index, 1);
    },
    
    // 업로드된 파일 제거
    removeFile(index) {
      this.uploadedFiles.splice(index, 1);
    },
    
    // 파일 업로드 처리
    async uploadFiles() {
      console.log('--uploadFiles--');
      console.log(this.selectedFiles);
      if (!this.selectedFiles.length) return;
      
      this.isFileLoading = true;
      
      try {
        // FormData 생성
        const formData = new FormData();
        
        this.selectedFiles.forEach((file) => {
          formData.append('files', file);
        });
        
        // API 호출 예시 (실제 엔드포인트로 대체 필요)
        const response = await apiClient.post('/api/files/upload', formData, {
          headers: {
            'Content-Type': 'multipart/form-data'
          }
        });
        
        // 업로드 성공 처리
        if (response.data) {
          console.log('파일 업로드 성공:', response.data);
          
          // 업로드 성공한 파일을 목록에 추가
          this.selectedFiles.forEach(file => {
            this.uploadedFiles.push({
              name: file.name,
              size: file.size,
              type: file.type
            });
          });
          
          // 선택된 파일 목록 초기화
          this.selectedFiles = [];
        }
      } catch (error) {
        console.error('파일 업로드 오류:', error);
        // 오류 처리
      } finally {
        this.isFileLoading = false;
      }
    },

    // 파일 첨부 이메일 테스트
    async test4() {  
      this.isLoading = true;

      try {
        // FormData 생성
        const formData = new FormData();
        formData.append('to', 'javachohj@sampyo.co.kr');
        formData.append('subject', '파일 첨부 테스트');
        formData.append('message', '테스트 메일입니다. 첨부파일을 확인해주세요.');
        
        // 업로드된 파일이 있으면 첨부
        if (this.uploadedFiles.length > 0) {
          // 예시: 실제 구현에서는 파일 ID나 경로를 전달해야 할 수 있음
          formData.append('attachments', JSON.stringify(this.uploadedFiles.map(file => file.name)));
        } else {
          alert('먼저 파일을 업로드해주세요.');
          this.isLoading = false;
          return;
        }
        
        // API 호출
        const response = await apiClient.post('/api/email/send-with-attachments', formData, {
          headers: {
            'Content-Type': 'multipart/form-data'
          }
        });
        
        // 성공 처리                    
        console.log('첨부파일 이메일 전송 결과:', response.data);
        alert('첨부파일과 함께 메일이 전송되었습니다.');
      } catch (error) {
        console.error('첨부파일 이메일 전송 실패:', error);
        alert('이메일 전송에 실패했습니다.');
      } finally {
        this.isLoading = false;
      }        
    }
  },
}
</script>

<style scoped>
.selected-files {
  border: 1px solid #e0e0e0;
  border-radius: 4px;
  background-color: #f9f9f9;
}
.selected-files > div {
  border-bottom: 1px solid #e0e0e0;
  padding: 8px 12px;
}
.selected-files > div:last-child {
  border-bottom: none;
}
.file-name {
  font-weight: 500;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}
.file-size {
  color: #757575;
  font-size: 0.85rem;
}
</style>