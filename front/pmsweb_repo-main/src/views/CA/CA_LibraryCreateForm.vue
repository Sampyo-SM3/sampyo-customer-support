<template>
  <v-container fluid class="pr-0 pl-0 pt-0">

    <br>

    <v-row no-gutters class="search-row top-row">
      <v-col class="search-col product-category">
        <div class="label-box">작성자</div>
        <div class="author-value">{{ userName }}</div>
      </v-col>
    </v-row>

    <v-row no-gutters class="search-row middle-row">
      <v-col cols="4" class="search-col product-category">
        <div class="label-box">문의유형</div>
        <v-select v-model="selectedInquiryType" :items="inquiryTypeList" item-title="codeName" item-value="codeId"
          density="compact" hide-details variant="outlined" class="inquiry-select mr-8 mt-1 mb-1" placeholder="선택"
          style="margin-left:10px;" />
      </v-col>

      <v-col cols="4" class="search-col product-category">
        <div class="label-box">문의부문</div>
        <div class="category-radio-wrapper">
          <v-radio-group v-model="selectedCategory" class="small-radios" inline density="compact" hide-details
            color="#3A70B1">
            <v-radio v-for="item in categoryList" :key="item.codeId" :label="item.codeName" :value="item.codeId" />
          </v-radio-group>
        </div>
      </v-col>

      <v-col cols="4" class="search-col product-category">
        <div class="label-box">중요도</div>
        <div class="priority-radio-wrapper">
          <v-radio-group v-model="selectedPriority" class="small-radios" inline density="compact" color="#3A70B1"
            hide-details>
            <v-radio v-for="item in priorityList" :key="item.codeId" :label="item.codeName" :value="item.codeId" />
          </v-radio-group>
        </div>
      </v-col>
    </v-row>

    <v-row no-gutters class="search-row middle-row">
      <v-col class="search-col" style="max-width:350px;">
        <div class="label-box">담당자</div>
        <v-text-field class="mr-8 mt-1 mb-1 input-manager" v-model="manager" readonly hide-details density="compact"
          variant="outlined" append-icon="mdi-magnify" @click="showManagerPopup = true" style="margin-left:10px;">
        </v-text-field>
      </v-col>

      <input type="hidden" :value="managerId" name="managerId" />
      <input type="hidden" :value="managerTel" name="managerTel" />
      <input type="hidden" :value="managerEmail" name="managerEmail" />
    </v-row>

    <v-row no-gutters class="search-row middle-row">
      <!-- 제목 필드 -->
      <v-col class="search-col">
        <div class="label-box">제 목</div>
        <v-text-field class="mr-8 mt-1 mb-1" v-model="sub" placeholder="제목을 입력하세요" clearable hide-details
          density="compact" variant="outlined" style="margin-left:10px;"></v-text-field>
      </v-col>
    </v-row>

    <v-row no-gutters class="search-row middle-row">
      <!-- 내용 텍스트필드 -->
      <v-col class="search-col">
        <div class="label-box">내 용</div>
        <v-textarea v-model="etc" placeholder="내용을 입력하세요" auto-grow rows="18" clearable hide-details density="compact"
          variant="outlined" class="content-textarea">
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

        <v-btn variant="flat" color="#3A70B1" class="file-btn mt-1 mb-2 ml-2 mr-2 white-text d-flex align-center"
          @click="openFileSelector">
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
    </v-row>

    <br>

    <div class="pt-2 d-flex justify-center">
      <v-btn variant="flat" color="grey darken-2" class="mr-2 d-flex align-center custom-btn-size " size="large"
        @click="goBack">
        <v-icon size="default" class="mr-1">mdi-close</v-icon>
        취소
      </v-btn>
      &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;
      <v-btn variant="flat" color="primary" class="mr-2 d-flex align-center custom-btn-size " size="large"
        @click="insertBoard()">
        <v-icon size="default" class="mr-1">mdi-check</v-icon>
        접수
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

  <!-- 관리자 추가하기 팝업 -->
  <manager-popup :show="showManagerPopup" @manager-selected="onAdminAdded" @close="showManagerPopup = false" />
</template>

<script>
import apiClient from '@/api';
import ManagerPopup from '@/components/ManagerPopup';
import { inject, onMounted } from 'vue';
import { useKakaoStore } from '@/store/kakao';

export default {
  components: {
    ManagerPopup
  },
  setup() {
    const kakaoStore = useKakaoStore();
    const extraBreadcrumb = inject('extraBreadcrumb', null);
    const listButtonLink = inject('listButtonLink', null);

    onMounted(() => {
      if (extraBreadcrumb) {
        extraBreadcrumb.value = '자료실 게시글 작성';
      }

      if (listButtonLink) {
        listButtonLink.value = '/views/CA/CA2000_10';
      }
    });

    return { kakaoStore };
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
      manager: '',
      managerId: '',
      managerTel: '',
      managerEmail: '',
      userId: null,
      sub: '',
      etc: '',
      content: '',
      selectedManager: null,
      fileAttach: '',

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
      pendingFiles: [], // 덮어쓰기 대기 중인 파일들 
      inquiryTypeList: [],
      categoryList: [],
      priorityList: [],
      selectedInquiryType: null,
      selectedCategory: null,
      selectedPriority: null,
    }
  },

  computed: {

  },

  watch: {
  },

  mounted() {
    this.checkLocalStorage();
    this.getUserInfo();
    this.getCodes();
  },

  created() {
    // localStorage에서 사용자 정보 불러오기
    this.getUserInfo();
  },

  methods: {
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

      // this.removeFile(index);
    },

    // 업로드된 파일 제거
    removeFile(index) {
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

    // 실제 파일 업로드 처리
    async processUpload(filesToUpload) {
      if (!filesToUpload.length) throw new Error('업로드할 파일이 없습니다.');

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
    async test4(para_file_name) {
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
      // ref를 사용하여 파일 input 트리거
      this.$refs.fileInput.$el.querySelector('input[type="file"]').click();
    },

    validateBoard() {
      // 검증 초기화
      this.errorMessages = [];

      // 제목 검증
      if (!this.sub || this.sub.trim() === '') {
        this.errorMessages.push('제목을 입력해주세요.');
        this.showError = true;
        return false;
      }

      // 내용 검증
      if (!this.etc || this.etc.trim() === '') {
        this.errorMessages.push('내용을 입력해주세요.');
        this.showError = true;
        return false;
      }

      return true;
    },

    async insertBoard() {
      try {
        this.showError = false;

        if (!this.validateBoard()) {
          return; // 검증 실패 시 함수 종료
        }

        this.loading = true;



        const boardData = {
          "sub": this.sub,
          "etc": this.etc,
          "writerId": this.userId,
          "uid": this.userName,
          "dpId": JSON.parse(localStorage.getItem("userInfo"))?.deptCd || null,
          "manager": this.manager,
          "managerId": this.managerId,
          "managerTel": this.managerTel,
          "managerEmail": this.managerEmail,
          "processState": "C",
          "division": JSON.parse(localStorage.getItem("userInfo"))?.companyCd || null,
          "inquiryType": this.selectedInquiryType, // 예: 'Q1'
          "inquiryPart": this.selectedCategory,    // 예: 'ERP'
          "priority": this.selectedPriority        // 예: 'P1'
        };

        // 게시글 등록 및 seq 값 반환
        const response = await apiClient.post("/api/require/insert", boardData);
        const boardSeq = response.data; // 등록된 게시글의 seq

        // console.log('test_1');
        await this.kakaoStore.sendAlimtalk_Manager(this.sub, this.manager, this.userName, this.managerTel);

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

        // 모든 파일 첨부 및 추가 API 호출을 동시에 실행
        const responses = await Promise.all(fileAttachPromises);

        // 결과 분석
        // const successFiles = responses.filter(response => response.status === 'success');
        const errorFiles = responses.filter(response => response.status === 'error');

        // 실패한 파일이 있으면 사용자에게 알림
        if (errorFiles.length > 0) {
          this.errorMessages = errorFiles.map(file => `${file.fileName}: ${file.error}`);
          this.showError = true;
        }

        // 모든 성공 시 페이지 이동 또는 추가 로직
        this.$router.push({ name: 'CA1000_10' });

      } catch (error) {
        console.error(error);
      } finally {
        this.loading = false;
      }
    },

    checkLocalStorage() {
      const midMenuFromStorage = localStorage.getItem('midMenu');
      const subMenuFromStorage = localStorage.getItem('subMenu');

      this.savedMidMenu = midMenuFromStorage ? JSON.parse(midMenuFromStorage) : null;
      this.savedSubMenu = subMenuFromStorage ? JSON.parse(subMenuFromStorage) : null;

      // console.log('메뉴 클릭 후 midMenu:', this.savedMidMenu);
      // console.log('메뉴 클릭 후 subMenu:', this.savedSubMenu);
    },

    getUserInfo() {
      // localStorage에서 userInfo를 가져와서 userName에 할당
      this.userName = JSON.parse(localStorage.getItem("userInfo"))?.name || null;
      this.userId = JSON.parse(localStorage.getItem("userInfo"))?.id || null;
    },

    goBack() {
      // 브라우저 히스토리에서 뒤로가기
      this.$router.go(-1);
    },
    onAdminAdded(selectedManager) {
      this.manager = selectedManager.name;
      this.managerId = selectedManager.usrId;
      this.managerTel = selectedManager.handTelNo;
      this.managerEmail = selectedManager.emailAddr;

      this.selectedManager = selectedManager;
      // console.log(selectedManager);
    },
    async getCodes() {
      try {
        // 문의유형
        const inquiryRes = await apiClient.get("/api/code/list", {
          params: { category: 'INQUIRY_TYPE' }
        });
        this.inquiryTypeList = inquiryRes.data;

        // 문의부문
        const categoryRes = await apiClient.get("/api/code/list", {
          params: { category: 'INQUIRY_PART' }
        });
        this.categoryList = categoryRes.data;

        // 중요도
        const priorityRes = await apiClient.get("/api/code/list", {
          params: { category: 'PRIORITY' }
        });
        this.priorityList = priorityRes.data;

      } catch (error) {
        console.error('코드 리스트 조회 실패:', error);
      }
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
  min-height: 40px;
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

.label-box {
  /* 색상 변경 */
  color: #333333 !important;
  /* 이전: #578ADB */
  background-color: #e6eef8 !important;
  /* 이전: #f5f5f5 */
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

.product-category {
  max-width: 550px;
  flex-grow: 0;
  display: flex;
  flex-direction: row;
  /* 가로 방향으로 배치 */
  align-items: center;
  flex-wrap: nowrap;
  /* 줄바꿈 방지 */
  width: 100%;
}

.label-box {
  width: 100px;
  flex-shrink: 0;
  height: 100%;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 15px;
  font-weight: 500;
  color: #578ADB;
  background-color: #f5f5f5;
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

::v-deep(.input-manager .v-field) {
  width: 740px;
  height: 40px !important;
  font-size: 15px !important;
}

.priority-radio-wrapper {
  display: flex;
  align-items: center;
  padding-left: 15px;
}

.v-radio {
  margin-right: 10px;
}

.category-radio-wrapper {
  display: flex;
  align-items: center;
  padding-left: 15px;
}

.inquiry-radio-wrapper {
  display: flex;
  align-items: center;
  padding-left: 15px;
}

.small-radios :deep(.v-label) {
  color: black !important;
  font-weight: 500;
  font-size: 15px;
  /* 라벨 글자 크기를 약 0.8rem로 감소 */
}

.small-radios :deep(.v-selection-control__input svg),
.small-radios :deep(.v-selection-control__input .v-icon) {
  font-size: 17px;
  margin-bottom: 0.5px;
  /* 라디오 아이콘 크기를 약 1rem(16px)로 감소 */
}

.inquiry-select :deep(.v-field) {
  height: 37px !important;
}

.inquiry-select :deep(.v-field__input) {
  font-size: 15px;
}

.inquiry-select :deep(.v-list-item-title) {
  font-size: 15px;
}

.inquiry-select :deep(.v-input__control) {
  width: 167px !important;
  /* 원하는 너비로 조정 */
  min-width: 167px !important;
}
</style>