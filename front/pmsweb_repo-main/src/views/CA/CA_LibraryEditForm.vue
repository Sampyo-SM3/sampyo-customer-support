<template>
  <v-container fluid class="pr-0 pl-0 pt-0">
    <br>
    <v-row no-gutters class="search-row top-row">
      <!-- 제목 필드 -->
      <v-col class="search-col">
        <div class="label-box">제 목</div>
        <v-text-field v-model="title" placeholder="제목을 입력하세요" clearable hide-details density="compact"
          variant="outlined" class="w-100 mt-1 mb-1 mr-3" style="margin-left: 10px;" />
      </v-col>
    </v-row>

    <v-row no-gutters class="search-row middle-row">
      <!-- 내용 텍스트필드 -->
      <v-col class="search-col">
        <div class="label-box">내 용</div>
        <v-textarea v-model="content" placeholder="내용을 입력하세요" auto-grow rows="21" clearable hide-details
          density="compact" variant="outlined" class="content-textarea">
        </v-textarea>
      </v-col>
    </v-row>

    <!-- 첨부파일 -->
    <v-row no-gutters class="search-row bottom-row">
      <v-col class="select-files file-attach d-flex align-center">
        <div class="label-box">첨부파일</div>
        <v-file-input style="display: none;" ref="fileInput" class="manager-search flex-grow-1" v-model="newFiles"
          :rules="fileRules" accept="image/png, image/jpeg, application/pdf" label="파일을 선택하세요" density="compact"
          variant="outlined" prepend-icon="" multiple :loading="isFileLoading" hide-details
          @change="handleFileChange"></v-file-input>

        <v-btn variant="flat" color="#3A70B1" class="file-btn mt-2 mb-2 ml-2 mr-2 white-text d-flex align-center"
          size="small" @click="openFileSelector">
          <v-icon size="default" class="mr-1">mdi-file-upload</v-icon>
          첨부
        </v-btn>
      </v-col>

      <!-- 선택된 파일 목록 (아직 업로드되지 않은 파일) -->
      <div v-if="selectedFiles.length > 0" class="selected-files ml-5 mt-2 mb-2">
        <div class="selected-files-container">
          <div v-for="(file, index) in selectedFiles" :key="index" class="selected-files-item">
            <div class="file-info">
              <div class="file-name text-body-1">{{ file.name }}</div>
              <div class="file-size text-body-2 text-grey">{{ formatFileSize(file.size) }}</div>
            </div>
            <v-btn class="ml-3" icon="mdi-delete" variant="text" color="#E44532" density="compact"
              @click="removeSelectedFile(index)"></v-btn>
          </div>
        </div>
      </div>

      <!-- 업로드된 파일 -->
      <div v-if="uploadedFiles.length > 0" class="selected-files ml-5 mt-2 mb-2">
        <div class="selected-files-container">
          <div v-for="(file, index) in uploadedFiles" :key="index" class="selected-files-item">
            <div class="file-info">
              <div class="file-name text-body-1">{{ file.name }}</div>
              <div class="file-size text-body-2 text-grey">{{ formatFileSize(file.size) }}</div>
            </div>
            <v-btn class="ml-3" icon="mdi-delete" variant="text" color="#E44532" density="compact"
              @click="markFileForDeletion(index, file)"></v-btn>
          </div>
        </div>
      </div>
    </v-row>

    <br>

    <div class="pb-5 pt-2 d-flex justify-center">
      <v-btn variant="flat" color="grey darken-2" class="mr-2 d-flex align-center custom-btn-size " size="large"
        @click="goBack">
        <v-icon size="default" class="mr-1">mdi-close</v-icon>
        취소
      </v-btn>
      &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;
      <v-btn variant="flat" color="primary" class="mr-2 d-flex align-center custom-btn-size " size="large"
        @click="showConfirmBeforeUpdate()">
        <v-icon size="default" class="mr-1">mdi-check</v-icon>
        수정
      </v-btn>
    </div>
  </v-container>

  <!-- 스낵바로 오류 메시지 표시 -->
  <v-snackbar v-model="showError" color="warning" timeout="5000" location="center" elevation="8" variant="elevated">
    {{ errorMessages[0] }}

    <template v-slot:actions>
      <v-btn variant="text" @click="showError = false">
        닫기
      </v-btn>
    </template>
  </v-snackbar>

  <v-dialog v-model="showConfirmDialog" max-width="450px" persistent>
    <v-card class="pa-4">
      <v-card-title class="text-h6 font-weight-bold">수정 확인</v-card-title>
      <v-card-text>게시글을 수정하시겠습니까?</v-card-text>
      <v-card-actions class="justify-end">
        <v-btn text @click="showConfirmDialog = false">취소</v-btn>
        <v-btn color="primary" text @click="confirmDialog">확인</v-btn>
      </v-card-actions>
    </v-card>
  </v-dialog>

</template>

<script>
import apiClient from '@/api';
import { inject, onMounted } from 'vue';

export default {
  props: {
    receivedSeq: {
      type: [Number, String],
      required: false
    },
  },
  components: {
  },
  setup() {
    const extraBreadcrumb = inject('extraBreadcrumb', null);
    const listButtonLink = inject('listButtonLink', null);
    onMounted(() => {
      if (extraBreadcrumb) {
        extraBreadcrumb.value = '게시글 수정';
      }

      if (listButtonLink) {
        listButtonLink.value = '/views/CA/CA2000_10';
      }
    });
  },
  unmounted() { // ❗ 컴포넌트가 언마운트될 때
    const listButtonLink = inject('listButtonLink', null);
    if (listButtonLink) {
      listButtonLink.value = null; // 페이지 벗어날 때 목록버튼 없애기
    }

    const extraBreadcrumb = inject('extraBreadcrumb', null);
    if (extraBreadcrumb) {
      extraBreadcrumb.value = null; // 페이지 벗어날 때 목록버튼 없애기
    }
  },
  data() {
    return {
      loading: false,
      errorMessages: [],
      showError: false,
      showManagerPopup: false,
      userName: null,
      userId: null,
      title: '',
      content: '',
      fileAttach: '',
      // 파일 업로드 관련 데이터
      newFiles: [], // 새로 선택한 파일 (v-file-input에 연결됨)
      selectedFiles: [], // 업로드 대기 중인 파일들
      uploadedFiles: [], // 이미 업로드된 파일들
      deleteFileList: [],
      isFileLoading: false,
      fileRules: [
        value => {
          return !value || !value.length || value[0].size < 5000000 || '파일 크기는 5MB 이하여야 합니다.';
        },
      ],
      showConfirmDialog: false,
      confirmCallback: null,
    }
  },

  computed: {

  },

  watch: {
    receivedSeq: {
      immediate: true  // 컴포넌트 생성 시점에도 즉시 실행
    },

  },

  mounted() {
    this.checkLocalStorage();
    this.getUserInfo();
  },

  created() {
    this.getUserInfo();
    this.fetchRequireDetail();
  },

  methods: {
    async fetchRequireDetail() {
      try {
        const response = await apiClient.get("/api/library/detail", {
          params: { seq: this.receivedSeq }
        });

        const data = response.data;

        if (!data) {
          console.warn("❗ 불러온 데이터 없음");
          return;
        }

        this.title = response.data?.title || "";
        this.content = response.data?.content || "";
        this.dpNm = response.data?.dpNm || "";
      } catch (error) {
        console.error("❌ 요구사항 불러오기 오류:", error);
      }

      //첨부파일 리스트 불러오기
      try {
        const fileList = await apiClient.get("/api/file-attach/fileList", {
          params: { seq: this.receivedSeq, boardType: 'CA1000_10' }
        });

        this.uploadedFiles = Array.isArray(fileList.data)
          ? fileList.data
            .filter(file => file && file.fileName)
            .map(file => ({
              name: file.fileName,
              size: file.fileSize,
              type: file.fileType || '',
              seq: file.seq
            }))
          : [];


      } catch (error) {
        console.error("❌ 오류 발생:", error);
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
    handleFileChange() {
      // 파일은 v-model에 바인딩된 newFiles에서 가져옵니다
      const files = this.newFiles;

      if (!files || (Array.isArray(files) && files.length === 0)) {
        return;
      }

      if (Array.isArray(files)) {
        files.forEach((file) => {
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

    markFileForDeletion(index, file) {
      const exists = this.deleteFileList.some(item => item.file.seq === file.seq);

      if (!exists) {
        this.deleteFileList.push({ index, file });
      }

      this.uploadedFiles.splice(index, 1); // UI에서는 즉시 제거
    },

    // 업로드된 파일 제거
    async removeFile(file) {

      console.log("랴ㅣㄷ" + file);

      await apiClient.post("/api/file-attach/deleteFile", {
        seq: file.seq,
        boardSeq: this.receivedSeq,
        fileName: file.name,
        boardType: 'CA2000_10'
      });

      this.fileDelete(file.name);
    },

    // 실제 파일 업로드 처리
    async processUpload(filesToUpload) {
      if (!Array.isArray(filesToUpload) || filesToUpload.length === 0) {
        throw new Error('업로드할 파일이 없습니다.');
      }

      this.isFileLoading = true;

      try {
        // FormData 생성
        const formData = new FormData();

        // 모든 파일을 'files' 이름으로 추가
        filesToUpload.forEach((file) => {
          formData.append('files', file);
        });

        // API 호출
        const response = await apiClient.post('/api/fileUpload', formData, {
          headers: {
            'Content-Type': 'multipart/form-data'
          }
        });

        // 업로드 성공 처리
        if (response.data && response.data.result === 'success') {
          // console.log('파일 업로드 성공:', response.data);

          // 업로드 성공한 파일을 목록에 추가
          filesToUpload.forEach(file => {
            this.uploadedFiles.push({
              name: file.name,
              size: file.size,
              type: file.type
            });
          });

          return true;
        } else {
          throw new Error('파일 업로드 실패');
        }
      } catch (error) {
        console.error('파일 업로드 오류:', error);
        throw error; // 호출한 곳으로 에러 전파
      } finally {
        this.isFileLoading = false;
      }
    },

    // 파일 삭제 확인
    async fileDelete(para_file_name) {
      this.showDeleteDialog = false;
      this.deletingFile = para_file_name;

      try {
        // FormData 사용하지 않고 URL에 파라미터 포함
        const response = await apiClient.post(`/api/fileDelete?originFile=${encodeURIComponent(this.deletingFile)}`);

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

    openFileSelector() {
      this.$refs.fileInput.$el.querySelector('input[type="file"]').click();
    },

    validateBoard() {
      this.errorMessages = [];

      if (!this.title || this.title.trim() === '') {
        this.errorMessages.push('제목을 입력해주세요.');
        this.showError = true;
        return false;
      }

      // 내용 검증
      if (!this.content || this.content.trim() === '') {
        this.errorMessages.push('내용을 입력해주세요.');
        this.showError = true;
        return false;
      }

      return true;
    },

    async updateBoard() {
      try {
        this.showError = false;

        if (!this.validateBoard()) {
          return; // 검증 실패 시 함수 종료
        }

        this.loading = true;

        const boardData = {
          "seq": this.receivedSeq,
          "title": this.title,
          "content": this.content,
        };

        // 게시글 등록 및 seq 값 반환
        await apiClient.post("/api/library/update", boardData);
        const boardSeq = this.receivedSeq; // 등록된 게시글의 seq

        // selectedFiles 배열의 각 파일에 대해 반복
        const fileAttachPromises = this.selectedFiles.map(async (file) => {
          try {
            // 파일명은 게시물 기준으로 중복 관리함
            const fileName = `${boardSeq}_${file.name}`;

            // 원본 파일 객체의 이름 변경
            const modifiedFile = new File([file], fileName, {
              type: file.type,
              lastModified: file.lastModified
            });

            const fileAttachData = {
              boardSeq: boardSeq,
              fileName: fileName,
              fileSize: modifiedFile.size,
              fileType: modifiedFile.type,
              boardType: 'CA2000_10'
            };

            // FileAttach 테이블 INSERT API 호출
            const attachResponse = await apiClient.post('/api/file-attach/insert', fileAttachData);

            // 파일서버 업로드 API 호출
            const additionalResponse = await this.processUpload([modifiedFile]);


            return {
              fileName: file.name,
              status: 'success',
              attachResponse,
              additionalResponse
            };
          } catch (error) {
            console.error(`파일 ${file.name} 처리 중 오류:`, error);
            return {
              fileName: file.name,
              status: 'error',
              error: error.message
            };
          }
        });

        if (this.deleteFileList.length > 0) {
          for (const { file } of this.deleteFileList) {
            await this.removeFile(file);
          }
          this.deleteFileList = [];
        }

        // 모든 파일 첨부 및 추가 API 호출을 동시에 실행
        const responses = await Promise.all(fileAttachPromises);

        // 결과 분석
        const errorFiles = responses.filter(response => response.status === 'error');

        // 실패한 파일이 있으면 사용자에게 알림
        if (errorFiles.length > 0) {
          this.errorMessages = errorFiles.map(file => `${file.fileName}: ${file.error}`);
          this.showError = true;
        }

        this.$router.push({
          name: 'CA_LibraryDetailForm',
          params: { receivedSeq: this.receivedSeq }
        })

      } catch (error) {
        console.error("게시글 수정 중 오류:", error);
        this.errorMessages = [error.message || "게시글 수정 중 오류가 발생했습니다."];
        this.showError = true;
      } finally {
        this.loading = false;
      }
    },

    checkLocalStorage() {
      const midMenuFromStorage = localStorage.getItem('midMenu');
      const subMenuFromStorage = localStorage.getItem('subMenu');

      this.savedMidMenu = midMenuFromStorage ? JSON.parse(midMenuFromStorage) : null;
      this.savedSubMenu = subMenuFromStorage ? JSON.parse(subMenuFromStorage) : null;
    },

    getUserInfo() {
      // localStorage에서 userInfo를 가져와서 userName에 할당
      this.userName = JSON.parse(localStorage.getItem("userInfo"))?.name || null;
      this.userId = JSON.parse(localStorage.getItem("userInfo"))?.id || null;
    },

    goBack() {
      this.$router.go(-1);
    },

    showConfirmBeforeUpdate() {
      this.confirmCallback = this.updateBoard;
      this.showConfirmDialog = true;
    },

    confirmDialog() {
      this.showConfirmDialog = false;
      if (this.confirmCallback) this.confirmCallback();
    },


  }
}
</script>

<style scoped>
.selected-files-container {
  display: flex;
  flex-wrap: wrap;
  gap: 10px;
  /* 아이템 사이 간격 */
}

.selected-files-item {
  display: flex;
  align-items: center;
  border: 1px solid #e0e0e0;
  border-radius: 4px;
  padding: 8px 12px;
  max-width: 250px;
  /* 원하는 너비로 조정 */
}

.file-info {
  flex-grow: 1;
  min-width: 0;
  /* 텍스트 자르기 위해 필요 */
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


.author-value {
  font-size: 15px;
  padding-left: 15px;
  white-space: nowrap;
  display: flex;
  align-items: center;
  color: #3A3C3F;
}

.title-div {
  font-size: 25px;
}

.custom-btn {
  font-size: 14px;
  height: 35px;
  border-radius: 10px;
}

.search-row {
  display: flex;
  align-items: stretch;
  min-height: 50px;
  border-top: 1px solid #e0e0e0;
  border-bottom: 0;
  border-left: 1px solid #e0e0e0;
  border-right: 1px solid #e0e0e0;
  /* 하단 테두리 제거 */
}

.search-row.top-row {
  border-top: 3px solid #e0e0e0;
  border-top-left-radius: 8px;
  border-top-right-radius: 8px;
  overflow: hidden;
}

.search-row.bottom-row {
  border-bottom: 2px solid #e0e0e0;
  border-bottom-left-radius: 8px;
  border-bottom-right-radius: 8px;
  overflow: hidden;
}

/* 새로 추가된 스타일 */
.search-row.top-row .search-col:first-child {
  border-top-left-radius: 8px;
}

.search-row.bottom-row .search-col:first-child {
  border-bottom-left-radius: 8px;
}

.search-row.top-row .search-col:last-child {
  border-top-right-radius: 8px;
}

.search-row.bottom-row .search-col:last-child {
  border-bottom-right-radius: 8px;
}

.search-col {
  display: flex;
  align-items: center;
  padding: 0;
  border-left: 1px solid #e0e0e0;
}

.select-files {
  display: flex;
  align-items: center;
  padding: 0;
  border-left: 1px solid #e0e0e0;
  width: 290px;
}

.manager-search {
  padding-block: 10px;
  padding-inline: 10px;
  width: 800px;
  font-weight: 400;
}

.content-textarea {
  padding-block: 10px;
  padding-inline: 10px;
  width: 100px !important;
  font-weight: 400;
}

.file-attach {
  max-width: 550px;
  flex-grow: 0;

}

.label-box {
  color: #333333 !important;
  background-color: #e6eef8 !important;
  width: 100px;
  flex-shrink: 0;
  height: 100%;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 15px;
  font-weight: 500;
  white-space: nowrap;
  padding: 0 4px;
  border-right: 1px solid #eaeaea;
}

.white-text {
  color: white !important;
}

.custom-btn-size {
  font-size: 13px;
  box-shadow: none;
  border-radius: 6px;
  margin-top: -10px !important;
  margin-bottom: 15px;
  min-width: 60px;
}

.v-card-title {
  padding-bottom: 0;
}

.v-card-text {
  padding-top: 8px;
  padding-bottom: 16px;
}
</style>