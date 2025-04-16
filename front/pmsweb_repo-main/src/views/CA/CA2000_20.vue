<template>
  <v-container fluid class="pr-5 pl-5 pt-7">

    <v-dialog v-model="showAdminPopup" max-width="600px">
      <v-card>
        <v-card-title class="text-h5">
          관리자 추가하기
          <v-spacer></v-spacer>
          <v-btn icon @click="closePopup">
            <v-icon>mdi-close</v-icon>
          </v-btn>
        </v-card-title>
        <v-card-text>
          <div class="board_control">
            <div class="search_board">
              <v-row>
                <v-col cols="4">
                  <v-select
                    v-model="searchType"
                    :items="searchTypes"
                    label="검색 조건"
                    variant="outlined"
                    density="compact"
                  ></v-select>
                </v-col>
                <v-col cols="6">
                  <v-text-field
                    v-model="searchText"
                    label="검색어"
                    variant="outlined"
                    density="compact"
                    append-inner-icon="mdi-magnify"
                    @click:append-inner="searchUsers"
                  ></v-text-field>
                </v-col>
                <v-col cols="2">
                  <v-btn color="primary" @click="searchUsers">검색</v-btn>
                </v-col>
              </v-row>
            </div>
          </div>

          <v-data-table
            :headers="userHeaders"
            :items="users"
            :items-per-page="10"
            item-value="userId"
            v-model:selected="selectedUsers"
            show-select
            class="elevation-1 mt-4 mb-5"
          ></v-data-table>
        </v-card-text>
        <v-card-actions class="d-flex justify-end">
          <v-btn variant="text" color="grey" @click="closePopup">취소</v-btn>
          <v-btn variant="text" color="primary" @click="saveSelectedUsers">저장</v-btn>
        </v-card-actions>
      </v-card>
    </v-dialog>


  </v-container>
</template>

<script>
import apiClient from '@/api';

export default {
  data() {
    return {
      isLoading: false,
      progressStatuses: [],
      // 파일 업로드 관련 데이터
      newFiles: [], // 새로 선택한 파일 (v-file-input에 연결됨)
      selectedFiles: [], // 업로드 대기 중인 파일들
      uploadedFiles: [], // 이미 업로드된 파일들
      isFileLoading: false,
      fileRules: [
        value => {
          return !value || !value.length || value[0].size < 5000000 || '파일 크기는 5MB 이하여야 합니다.';
        },
      ],
      // 파일 덮어쓰기 관련
      showOverwriteDialog: false,
      duplicateFiles: [],
      pendingFiles: [] // 덮어쓰기 대기 중인 파일들
    }
  },

  methods: {
    test() {
      console.log('--test--');

      try {
        // 폼 타입 결정        
        const baseUrl = 'https://bluesam.sampyo.co.kr/WebSite/Approval/Forms/FormLinkForLEGACY.aspx'
        const params = {
          key: '111',  // board seq번호
          empno: 'SPH221342320005', // 사원번호
          legacy_form: 'WF_FORM_SRTEST',
          datatype: 'xml',  // 데이터 타입          
          // seq: '111', // 프로시저 호출되는 ip          
          // DATE_TEST: '111',  // board seq번호
          ip: '10.50.20.71', // 프로시저 호출되는 ip          
          db: 'SPC_TEST'     // 프로시저 호출되는 db
        };

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

          // 중복 파일 검사 (selectedFiles 내에서)
          const existingSelectedIndex = this.selectedFiles.findIndex(f => f.name === file.name);
          if (existingSelectedIndex !== -1) {
            // 선택된 파일 목록에서 중복된 파일 교체
            this.selectedFiles.splice(existingSelectedIndex, 1, file);
          } else {
            // 새 파일 추가
            this.selectedFiles.push(file);
          }
        });
      } else {
        console.log('단일 파일 선택됨 이름:', files.name);

        // 중복 파일 검사 (selectedFiles 내에서)
        const existingSelectedIndex = this.selectedFiles.findIndex(f => f.name === files.name);
        if (existingSelectedIndex !== -1) {
          // 선택된 파일 목록에서 중복된 파일 교체
          this.selectedFiles.splice(existingSelectedIndex, 1, files);
        } else {
          // 새 파일 추가
          this.selectedFiles.push(files);
        }
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
      console.log('--removeFile--');
      console.log(this.uploadedFiles[index].name);
      this.test4(this.uploadedFiles[index].name);


      this.uploadedFiles.splice(index, 1);
    },

    // 파일명 중복 확인
    checkDuplicateFiles() {
      const duplicates = [];

      // selectedFiles의 각 파일이 uploadedFiles에 이미 존재하는지 확인
      this.selectedFiles.forEach(selectedFile => {
        const isDuplicate = this.uploadedFiles.some(uploadedFile =>
          uploadedFile.name === selectedFile.name
        );

        if (isDuplicate) {
          duplicates.push(selectedFile);
        }
      });

      return duplicates;
    },

    // 파일 업로드 처리
    async uploadFiles() {
      console.log('--uploadFiles--');
      console.log(this.selectedFiles);
      if (!this.selectedFiles.length) return;

      // 파일명 중복 확인
      const duplicateFiles = this.checkDuplicateFiles();

      if (duplicateFiles.length > 0) {
        // 중복 파일이 있을 경우 확인 대화상자 표시
        this.duplicateFiles = duplicateFiles;
        this.pendingFiles = this.selectedFiles.filter(file =>
          !duplicateFiles.some(dupFile => dupFile.name === file.name)
        );
        this.showOverwriteDialog = true;
        return;
      }

      // 중복 파일이 없으면 바로 업로드 진행
      await this.processUpload(this.selectedFiles);
    },

    // 덮어쓰기 취소
    cancelOverwrite() {
      this.showOverwriteDialog = false;

      // 중복되지 않은 파일만 업로드 진행
      if (this.pendingFiles.length > 0) {
        this.processUpload(this.pendingFiles);
      }

      // 상태 초기화
      this.duplicateFiles = [];
      this.pendingFiles = [];
    },

    // 덮어쓰기 확인
    confirmOverwrite() {
      this.showOverwriteDialog = false;

      // 중복 파일 제거 (기존 업로드 파일에서)
      this.duplicateFiles.forEach(dupFile => {
        const index = this.uploadedFiles.findIndex(f => f.name === dupFile.name);
        if (index !== -1) {
          this.uploadedFiles.splice(index, 1);
        }
      });

      // 모든 선택된 파일 업로드 진행
      this.processUpload(this.selectedFiles);

      // 상태 초기화
      this.duplicateFiles = [];
      this.pendingFiles = [];
    },

    // 실제 파일 업로드 처리
    async processUpload(filesToUpload) {
      if (!filesToUpload.length) return;

      this.isFileLoading = true;

      try {
        // FormData 생성
        const formData = new FormData();

        // 백엔드에서 @RequestParam("files")로 받기 때문에 모든 파일을 'files' 이름으로 추가
        filesToUpload.forEach((file) => {
          formData.append('files', file);
        });

        // API 호출 - 백엔드 컨트롤러 경로와 일치하도록 설정
        const response = await apiClient.post('/api/fileUpload', formData, {
          headers: {
            'Content-Type': 'multipart/form-data'
          }
        });

        // 업로드 성공 처리
        if (response.data && response.data.result === 'success') {
          console.log('파일 업로드 성공:', response.data);

          // 백엔드에서 반환한 파일 목록 정보를 사용할 수 있습니다
          const uploadedFileList = response.data.files || [];
          console.log('업로드된 파일 목록:', uploadedFileList);
          console.log('총 업로드 목록 ', this.uploadedFiles);

          // 업로드 성공한 파일을 목록에 추가
          filesToUpload.forEach(file => {
            this.uploadedFiles.push({
              name: file.name,
              size: file.size,
              type: file.type
            });
          });

          // 업로드된 파일을 선택된 파일 목록에서 제거
          this.selectedFiles = this.selectedFiles.filter(selectedFile =>
            !filesToUpload.some(uploadedFile => uploadedFile.name === selectedFile.name)
          );
        }
      } catch (error) {
        console.error('파일 업로드 오류:', error);
        alert('파일 업로드 중 오류가 발생했습니다.');
      } finally {
        this.isFileLoading = false;
      }
    },

    // 파일 삭제 확인
    async test4(para_file_name) {
      this.showDeleteDialog = false;
      this.deletingFile = para_file_name;

      try {
        // FormData 사용하지 않고 URL에 파라미터 포함
        const response = await apiClient.post(`/api/fileDelete?originFile=${encodeURIComponent(this.deletingFile)}`);
        console.log('response -> ' + response.data.result);

        // 삭제 성공 처리
        if (response.data.result === 'success') {
          // 파일 목록에서 삭제된 파일 제거
          // this.files = this.files.filter(file => file.originFile !== this.fileToDelete);

        } else {
          throw new Error(response.data.message || '파일 삭제에 실패했습니다.');
        }
      } catch (error) {
        console.error('파일 삭제 중 오류 발생:', error);

      } finally {
        this.deletingFile = null;
      }
    },
    async getStatus() {
      try {
        const statusList = await apiClient.get("/api/status/list");

        // 상태 이름 리스트 저장
        this.progressStatuses = statusList.data.map(status => ({
          text: status.codeName,  // UI에 표시할 값
          value: status.codeId    // 실제 선택될 값
        }));

      } catch (error) {
        console.error("❌ 오류 발생:", error);
      }
    },
  },
  mounted() {
    this.getStatus();
    this.showAdminPopup = true;
  },
}
</script>

<style scoped>
.selected-files {
  border: 1px solid #e0e0e0;
  border-radius: 4px;
  background-color: #f9f9f9;
}

.selected-files>div {
  border-bottom: 1px solid #e0e0e0;
  padding: 8px 12px;
}

.selected-files>div:last-child {
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