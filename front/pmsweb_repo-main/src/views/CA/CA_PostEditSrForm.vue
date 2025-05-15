<template style="margin-top:-30px;">
  <v-container fluid class="pr-0 pl-0 pt-0">

    <br>

    <v-row no-gutters class="search-row top-row">
      <v-col class="search-col input-width">
        <div class="label-box colNm">
          <span class="required-star">*</span> 제목
        </div>
        <v-text-field v-model="inquiry.sub" variant="outlined" density="compact" hide-details
          class="input-area title-field" />
      </v-col>
    </v-row>

    <v-row no-gutters class="search-row middle-row">
      <v-col class="search-col input-width">
        <div class="label-box colNm"><span class="required-star">*</span>업무명</div>
        <v-text-field v-model="inquiry.taskName" variant="outlined" density="compact" hide-details
          class="input-area"></v-text-field>
      </v-col>
    </v-row>

    <v-row no-gutters class="search-row middle-row">
      <v-col class="search-col input-width">
        <div class="label-box colNm"><span class="required-star">*</span>협조</div>
        <v-text-field v-model="inquiry.help" variant="outlined" density="compact" hide-details
          class="input-area"></v-text-field>
      </v-col>
    </v-row>

    <v-row no-gutters class="search-row middle-row">
      <v-col class="search-col input-width">
        <div class="label-box colNm"><span class="required-star">*</span>개발(변경) 필요성</div>
        <v-text-field v-model="inquiry.necessity" variant="outlined" density="compact" hide-details
          class="input-area"></v-text-field>
      </v-col>
    </v-row>

    <v-row no-gutters class="search-row middle-row">
      <v-col class="search-col input-width">
        <div class="label-box colNm"><span class="required-star">*</span>기대효과</div>
        <v-text-field v-model="inquiry.effect" variant="outlined" density="compact" hide-details
          class="input-area"></v-text-field>
      </v-col>
    </v-row>

    <v-row no-gutters class="search-row middle-row">
      <v-col class="search-col input-width">
        <div class="label-box colNm"><span class="required-star">*</span>개발(변경) 모듈</div>
        <v-text-field v-model="inquiry.module" variant="outlined" density="compact" hide-details
          class="input-area"></v-text-field>
      </v-col>
    </v-row>

    <v-row no-gutters class="search-row middle-row" style="height:200px;">
      <v-col class="search-col request-period">
        <div class="label-box colNm"><span class="required-star">*</span>개발(변경)<br />업무내용</div>
      </v-col>

      <v-col style="border-right: 1px solid #e0e0e0;">
        <div>
          <div class="sub-label">변경전</div>
          <v-textarea v-model="inquiry.beforeTaskContent" variant="outlined" density="compact" hide-details
            class="input-area-L"></v-textarea>
        </div>
      </v-col>

      <!-- 변경후 -->
      <v-col>
        <div>
          <div class="sub-label">변경후</div>
          <v-textarea v-model="inquiry.afterTaskContent" variant="outlined" density="compact" hide-details
            class="input-area-L"></v-textarea>
        </div>
      </v-col>
    </v-row>

    <v-row no-gutters class="search-row middle-row">
      <v-col class="search-col input-width-half">
        <div class="label-box colNm">
          <span class="required-star">*</span>사용부서
        </div>
        <v-text-field v-model="inquiry.useDept" variant="outlined" density="compact" hide-details class="input-area"
          style="width: 100%;">
        </v-text-field>
      </v-col>

      <v-col class="search-col input-width-half">
        <div class="label-box colNm">첨부문서</div>
        <v-text-field v-model="inquiry.attachDoc" variant="outlined" density="compact" hide-details class="input-area"
          style="width: 100%;"></v-text-field>
      </v-col>
    </v-row>

    <v-row no-gutters class="search-row middle-row">
      <v-col class="search-col input-width-half">
        <div class="label-box colNm"><span class="required-star">*</span>의뢰일자</div>

        <VueDatePicker class="date-picker ml-1" :month-picker="false" preview-format="yyyy-MM-dd"
          v-model="inquiry.requestDate" :teleport="true" :enable-time-picker="false" auto-apply locale="ko"
          format="yyyy-MM-dd" :week-start="1" :allowed-dates="allowedDates"
          @update:model-value="requestDateMenu = false" v-model:open="datePickerOpen" :clearable="false"
          :text-input="false" />

        <!-- <v-menu v-model="requestDateMenu" :close-on-content-click="false" transition="scale-transition" offset-y
          min-width="auto">
          <template v-slot:activator="{ props }">
            <div class="date-field-wrapper" v-bind="props"
              style="display: flex; align-items: center; gap: 4px; width: 200px; ">
              <v-text-field v-model="formattedRequestDate" class="input-area" density="compact" hide-details readonly
                variant="outlined" />
              <v-icon size="23" color="#7A7A7A">mdi-calendar-search</v-icon>
            </div>
          </template>

<v-date-picker v-model="inquiry.requestDate" @update:model-value="requestDateMenu = false" locale="ko-KR" elevation="1"
  color="blue" width="310" first-day-of-week="1" show-adjacent-months scrollable :allowed-dates="allowedDates" />
</v-menu> -->
      </v-col>


      <v-col class="search-col input-width-half">
        <div class="label-box colNm">접수일자</div>
        <VueDatePicker class="date-picker ml-1" :month-picker="false" preview-format="yyyy-MM-dd"
          v-model="inquiry.acceptDate" :teleport="false" position="top" :enable-time-picker="false" auto-apply
          locale="ko" format="yyyy-MM-dd" :week-start="1" :allowed-dates="allowedDates"
          @update:model-value="acceptDateMenu = false" v-model:open="datePickerOpen" :clearable="false"
          :text-input="false" />

        <!-- <v-menu v-model="acceptDateMenu" :close-on-content-click="false" transition="scale-transition" offset-y
          min-width="auto">
          <template v-slot:activator="{ props }">
            <div class="date-field-wrapper" v-bind="props"
              style="display: flex; align-items: center; gap: 4px; width: 200px; ">
              <v-text-field v-model="formattedAcceptDate" class="input-area" density="compact" hide-details readonly
                variant="outlined" />
              <v-icon size="23" color="#7A7A7A">mdi-calendar-search</v-icon>
            </div>
          </template>

          <v-date-picker v-model="inquiry.acceptDate" @update:model-value="acceptDateMenu = false" locale="ko-KR"
            elevation="1" color="blue" width="300" first-day-of-week="1" show-adjacent-months scrollable
            :allowed-dates="allowedDates" />
        </v-menu> -->
      </v-col>
    </v-row>

    <v-row no-gutters class="search-row middle-row">
      <v-col class="search-col input-width-half">
        <div class="label-box colNm"><span class="required-star">*</span>완료요청일자</div>
        <VueDatePicker class="date-picker ml-1" :month-picker="false" preview-format="yyyy-MM-dd"
          v-model="inquiry.completeRequestDate" :teleport="false" position="top" :enable-time-picker="false" auto-apply
          locale="ko" format="yyyy-MM-dd" :week-start="1" :allowed-dates="allowedDates"
          @update:model-value="completeRequestDateMenu = false" v-model:open="datePickerOpen" :clearable="false"
          :text-input="false" />

        <!-- <v-menu v-model="completeRequestDateMenu" :close-on-content-click="false" transition="scale-transition" offset-y
          min-width="auto">
          <template v-slot:activator="{ props }">
            <div class="date-field-wrapper" v-bind="props"
              style="display: flex; align-items: center; gap: 4px; width: 200px; ">
              <v-text-field v-model="formattedCompleteRequestDate" class="input-area" density="compact" hide-details
                readonly variant="outlined" />
              <v-icon size="23" color="#7A7A7A">mdi-calendar-search</v-icon>
            </div>
          </template>

          <v-date-picker v-model="inquiry.completeRequestDate" @update:model-value="completeRequestDateMenu = false"
            locale="ko-KR" elevation="1" color="blue" width="300" first-day-of-week="1" show-adjacent-months scrollable
            :allowed-dates="allowedDates" /> 
        </v-menu> -->
      </v-col>

      <v-col class="search-col" style="max-width:600px;">
        <div class="label-box colNm">완료일자</div>
        <VueDatePicker class="date-picker ml-1 mt-1" :month-picker="false" preview-format="yyyy-MM-dd"
          v-model="inquiry.completeDate" :teleport="false" position="top" :enable-time-picker="false" auto-apply
          locale="ko" format="yyyy-MM-dd" :week-start="1" :allowed-dates="allowedDates"
          @update:model-value="completeDateMenu = false" v-model:open="datePickerOpen" :clearable="false"
          :text-input="false" />

        <!-- <v-menu v-model="completeDateMenu" :close-on-content-click="false" transition="scale-transition" offset-y
          min-width="auto">
          <template v-slot:activator="{ props }">
            <div class="date-field-wrapper" v-bind="props"
              style="display: flex; align-items: center; gap: 4px; width: 200px; ">
              <v-text-field v-model="formattedCompleteDate" class="input-area" density="compact" hide-details readonly
                variant="outlined" />
              <v-icon size="23" color="#7A7A7A">mdi-calendar-search</v-icon>
            </div>
          </template>

          <v-date-picker v-model="inquiry.completeDate" @update:model-value="completeDateMenu = false" locale="ko-KR"
            elevation="1" color="blue" width="300" first-day-of-week="1" show-adjacent-months scrollable
            :allowed-dates="allowedDates" />
        </v-menu> -->
      </v-col>
    </v-row>

    <v-row no-gutters class="search-row middle-row">
      <v-col class="search-col input-width">
        <div class="label-box colNm">기타</div>
        <v-text-field v-model="inquiry.etc" variant="outlined" density="compact" hide-details
          class="input-area"></v-text-field>
      </v-col>
    </v-row>

    <v-row no-gutters class="search-row bottom-row">
      <v-col class="search-col input-width">
        <div class="label-box colNm">첨부목록</div>
        <v-file-input style="display: none;" ref="fileInput" class="manager-search flex-grow-1" v-model="newFiles"
          :rules="fileRules" accept="image/png, image/jpeg, application/pdf" label="파일을 선택하세요" density="compact"
          variant="outlined" prepend-icon="" multiple :loading="isFileLoading" hide-details
          @change="handleFileChange"></v-file-input>

        <v-btn variant="flat" color="#3A70B1" class="file-btn mt-2 mb-2 ml-2 mr-2 white-text d-flex align-center"
          size="small" @click="openFileSelector">
          <v-icon size="default" class="mr-1">mdi-file-upload</v-icon>
          첨부
        </v-btn>

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
      </v-col>

    </v-row>

    <v-row class="mt-5">
      <v-col cols="12" class="d-flex justify-center">
        <v-btn variant="flat" color="grey darken-2" class="custom-btn mx-2" size="large" @click="moveSrDetail">
          <v-icon size="default" class="mr-1">mdi-close</v-icon>
          취소
        </v-btn>
        <v-btn variant="flat" color="primary" class="custom-btn mx-2" size="large" @click="submitRequest">
          <v-icon size="default" class="mr-1">mdi-check</v-icon>
          저장
        </v-btn>
      </v-col>
    </v-row>
  </v-container>
</template>



<script>
import apiClient from '@/api';
import dayjs from 'dayjs'
import utc from 'dayjs/plugin/utc'
import timezone from 'dayjs/plugin/timezone'
import { inject, onMounted } from 'vue';
import VueDatePicker from '@vuepic/vue-datepicker';
import '@vuepic/vue-datepicker/dist/main.css';


dayjs.extend(utc)
dayjs.extend(timezone)
dayjs.tz.setDefault('Asia/Seoul')  // KST 기준 설정

export default {
  components: {
    VueDatePicker
  },
  props: {
    receivedSeq: {
      type: [Number, String],
      required: false
    },
    userId: JSON.parse(localStorage.getItem("userInfo"))?.id || null
  },
  setup() {
    const extraBreadcrumb = inject('extraBreadcrumb', null);
    const listButtonLink = inject('listButtonLink', null);
    onMounted(() => {
      if (extraBreadcrumb) {
        extraBreadcrumb.value = 'SR 요청서 작성';  // 🔥 추가하고 싶은 값
      }

      if (listButtonLink) {
        listButtonLink.value = '/views/CA/CA1000_10';  // 🔥 현재 페이지에 맞는 "목록" 경로 설정
      }
    });

    return {};
  },
  unmounted() { // ❗ 컴포넌트가 언마운트될 때
    const listButtonLink = inject('listButtonLink', null);
    if (listButtonLink) {
      listButtonLink.value = null; // 🔥 페이지 벗어날 때 목록버튼 없애기
    }

    const extraBreadcrumb = inject('extraBreadcrumb', null);
    if (extraBreadcrumb) {
      extraBreadcrumb.value = null; // 페이지 벗어날 때 목록버튼 없애기
    }
  },
  data() {
    return {
      userInfo: null,       //사용자 ID
      requestDateMenu: false,
      acceptDateMenu: false,
      completeRequestDateMenu: false,
      completeDateMenu: false,
      step: 1,
      selectedStatus: '', // 추가된 상태 변수
      inquiry: {
        sub: "",
        context: "",
        taskName: "",
        help: "",
        necessity: "",
        effect: "",
        module: "",
        beforeTaskContent: "",
        afterTaskContent: "",
        useDept: "",
        attachDoc: "",
        requestDate: null,
        acceptDate: null,
        completeRequestDate: null,
        completeDate: null,
        etc: "",
        uid: "",
        usem: "",
        dpId: "",
        dpDn: "",
        manager: "",
        division: "",
        processState: ""
      },
      management: {
        PROGRESS: ""
      },
      progressStatuses: [],
      newFiles: [],
      selectedFiles: [],
      uploadedFiles: [],
      deleteFileList: [], //삭제할 파일 정보
      showOverwriteDialog: false,
      duplicateFiles: [],
      pendingFiles: [],
      fileRules: [
        value => {
          return !value || !value.length || value[0].size < 5000000 || '파일 크기는 5MB 이하여야 합니다.';
        },
      ],

    };
  },
  methods: {
    async fetchRequireDetail() {
      try {
        const response = await apiClient.get("/api/require/detail", {
          params: { seq: this.receivedSeq }
        });

        const data = response.data;

        if (!data) {
          console.warn("❗ 불러온 데이터 없음");
          return;
        }

        // ✅ inquiry 값 덮어쓰기 (필드 유지)
        Object.assign(this.inquiry, {
          sub: data?.sub || "",
          taskName: data?.taskName || "",
          help: data?.help || "",
          necessity: data?.necessity || "",
          effect: data?.effect || "",
          module: data?.module || "",
          beforeTaskContent: data?.beforeTaskContent || "",
          afterTaskContent: data?.afterTaskContent || "",
          useDept: data?.useDept || "",
          attachDoc: data?.attachDoc || "",
          requestDate: data?.requestDate ? new Date(data.requestDate) : null,
          acceptDate: data?.acceptDate ? new Date(data.acceptDate) : null,
          completeRequestDate: data?.completeRequestDate ? new Date(data.completeRequestDate) : null,
          completeDate: data?.completeDate ? new Date(data.completeDate) : null,
          etc: data?.etc || "",
          uid: data?.uid || "",
          usem: data?.usem || "",
          dpId: data?.dpId || "",
          dpDn: data?.dpDn || "",
          manager: data?.manager || "",
          division: data?.division || "",
          processState: data?.processState || "",
        });

        //첨부파일 리스트 불러오기
        try {
          const fileList = await apiClient.get("/api/file-attach/fileList", {
            params: { seq: this.receivedSeq }
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
          console.error("오류 발생:", error);
        }

      } catch (error) {
        console.error("요구사항 불러오기 오류:", error);
      }
    },
    async submitRequest() {
      if (confirm("저장하시겠습니까?")) {

        if (!this.inquiry.sub || this.inquiry.sub.trim() === '') {
          alert("제목은 필수 항목입니다.");
          return;
        }
        if (!this.inquiry.taskName || this.inquiry.taskName.trim() === '') {
          alert("업무명은 필수 항목입니다.");
          return;
        }
        if (!this.inquiry.help || this.inquiry.help.trim() === '') {
          alert("협조는 필수 항목입니다.");
          return;
        }
        if (!this.inquiry.necessity || this.inquiry.necessity.trim() === '') {
          alert("개발(변경) 필요성은 필수 항목입니다.");
          return;
        }
        if (!this.inquiry.effect || this.inquiry.effect.trim() === '') {
          alert("기대효과는 필수 항목입니다.");
          return;
        }
        if (!this.inquiry.module || this.inquiry.module.trim() === '') {
          alert("개발(변경) 모듈은 필수 항목입니다.");
          return;
        }
        if (!this.inquiry.beforeTaskContent || this.inquiry.beforeTaskContent.trim() === '' || !this.inquiry.afterTaskContent || this.inquiry.afterTaskContent.trim() === '') {
          alert("개발(변경) 업무내용은 필수 항목입니다.");
          return;
        }
        if (!this.inquiry.useDept || this.inquiry.useDept.trim() === '') {
          alert("사용부서는 필수 항목입니다.");
          return;
        }
        if (!this.inquiry.requestDate) {
          alert("의뢰일자는 필수 항목입니다.");
          return;
        }
        if (!this.inquiry.completeRequestDate) {
          alert("완료요청일자는 필수 항목입니다.");
          return;
        }

        try {
          const param = {
            seq: this.receivedSeq,
            sub: this.inquiry.sub,                        //제목
            taskName: this.inquiry.taskName,              //업무명
            help: this.inquiry.help,              //협조
            necessity: this.inquiry.necessity,    //개발(변경) 필요성
            effect: this.inquiry.effect,          //기대효과
            module: this.inquiry.module,          //개발(변경) 모듈
            beforeTaskContent: this.inquiry.beforeTaskContent,    //개발(변경) 업무내용 - 변경전
            afterTaskContent: this.inquiry.afterTaskContent,    //개발(변경) 업무내용 - 변경후
            useDept: this.inquiry.useDept,        //사용부서
            attachDoc: this.inquiry.attachDoc,    //첨부문서
            requestDate: this.inquiry.requestDate,//의뢰일자
            acceptDate: this.inquiry.acceptDate,  //접수일자
            completeRequestDate: this.inquiry.completeRequestDate,    //완료요청일자
            completeDate: this.inquiry.completeDate,                  //완료일자
            etc: this.inquiry.etc,                  //기타
          };

          const boardSeq = this.receivedSeq; // 등록된 게시글의 seq

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

          if (this.deleteFileList.length > 0) {
            for (const { file } of this.deleteFileList) {
              await this.removeFile(file);
            }
            this.deleteFileList = [];
          }

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

          // API 요청: 댓글 DB에 저장
          await apiClient.post("/api/require/updateSrForm", param);
          alert("저장하였습니다.");

          this.$router.push({
            name: 'CA_PostDetailSrForm',
            params: { receivedSeq: this.receivedSeq }
          })

        } catch (error) {
          alert("저장에 실패하였습니다. 관리자에게 문의하세요.");
        }
      }
    },
    moveSrDetail() {
      if (confirm("수정을 취소하시겠습니까?")) {
        this.$router.push({
          name: 'CA_PostDetailSrForm',
          params: { receivedSeq: this.receivedSeq }
        })
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
      await apiClient.post("/api/file-attach/deleteFile", {
        seq: file.seq,
        boardSeq: this.receivedSeq,
        fileName: file.name
      });

      this.fileDelete(file.name);
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
      // 파일 없으면 false 리턴
      if (!this.selectedFiles || this.selectedFiles.length === 0) {
        return false;
      }

      // 파일명 중복 확인
      const duplicateFiles = this.checkDuplicateFiles();

      if (duplicateFiles.length > 0) {
        return false;
      }

      // 중복 파일이 없으면 바로 업로드 진행
      try {
        await this.processUpload(this.selectedFiles);
        return true; // 성공 시 true 리턴
      } catch (error) {
        return false; // 실패 시 false 리턴
      }
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
  },
  computed: {
    formattedRequestDate: {
      get() {
        if (!this.inquiry.requestDate) return '';
        return dayjs(this.inquiry.requestDate).tz().format('YYYY-MM-DD');
      },
      set(val) {
        if (!val) {
          this.inquiry.requestDate = null;
        } else {
          // 여기가 핵심: 문자열로 직접 저장 금지!
          const parsed = dayjs.tz(val, 'Asia/Seoul').toDate();
          this.inquiry.requestDate = isNaN(parsed.getTime()) ? null : parsed;
        }
      }
    },
    formattedAcceptDate: {
      get() {
        if (!this.inquiry.acceptDate) return '';
        return dayjs(this.inquiry.acceptDate).tz().format('YYYY-MM-DD');
      },
      set(val) {
        if (!val) {
          this.inquiry.acceptDate = null;
        } else {
          this.inquiry.acceptDate = dayjs.tz(val, 'Asia/Seoul').toDate();
        }
      }
    },
    formattedCompleteRequestDate: {
      get() {
        if (!this.inquiry.completeRequestDate) return '';
        return dayjs(this.inquiry.completeRequestDate).tz().format('YYYY-MM-DD');
      },
      set(val) {
        if (!val) {
          this.inquiry.completeRequestDate = null;
        } else {
          this.inquiry.completeRequestDate = dayjs.tz(val, 'Asia/Seoul').toDate();
        }
      }
    },
    formattedCompleteDate: {
      get() {
        if (!this.inquiry.completeDate) return '';
        return dayjs(this.inquiry.completeDate).tz().format('YYYY-MM-DD');
      },
      set(val) {
        if (!val) {
          this.inquiry.completeDate = null;
        } else {
          this.inquiry.completeDate = dayjs.tz(val, 'Asia/Seoul').toDate();
        }
      }
    },
  },
  created() {
    this.userInfo = JSON.parse(localStorage.getItem("userInfo"));
  },
  mounted() {
    // 요구사항 정의서 데이터 가져오기
    this.fetchRequireDetail(); // API 호출
  },
  watch: {
    receivedSeq: {
      immediate: true  // 컴포넌트 생성 시점에도 즉시 실행
    },
  }
};
</script>

<style scoped>
.template {
  font-family: "Noto Sans KR", sans-serif;
}

/* 폼 디자인 */
.product-category {
  display: flex;
  flex-direction: row;
  /* 가로 방향으로 배치 */
  align-items: center;
  flex-wrap: nowrap;
  /* 줄바꿈 방지 */
  width: 100%;
}

.request-period,
.product-category {
  max-width: 550px;
  flex-grow: 0;
}

.author-value {
  font-size: 14px;
  padding-left: 15px;
  white-space: nowrap;
  display: flex;
  align-items: center;
}

.title-div {
  font-size: 25px;
}

.manager-search,
.content-textarea {
  display: flex;
  justify-content: flex-start;
  align-items: flex-start;
  padding: 15px;
  width: 800px;
  height: 450px;
  font-size: 15px;
  font-weight: 400;
  overflow-y: auto;
  white-space: pre-wrap;
  word-break: break-word;
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
  /* align-items: center; */
  padding: 0;
  border-left: 1px solid #e0e0e0;
}



.label-box {
  width: 80px;
  flex-shrink: 0;
  height: 100%;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 14px;
  font-weight: 500;
  color: #578ADB;
  background-color: #f5f5f5;
  white-space: nowrap;
  padding: 0 4px;
  border-right: 1px solid #eaeaea;
  /* margin-bottom: 5px; */
  color: #333333 !important;
  background-color: #e6eef8 !important;
}

.save-status-btn {
  height: 42px;
  min-width: 60px;
  font-size: 14px;
  border-radius: 6px;
  margin-bottom: 15px;
}

.colNm {
  width: 140px;
}

.sub-label {
  font-weight: 500;
  font-size: 13.5px;
  border-bottom: 1px solid #e0e0e0;
  height: 35px;
  flex-shrink: 0;
  display: flex;
  align-items: center;
  justify-content: center;
  white-space: nowrap;
  padding: 0 4px;
  border-right: 1px solid #eaeaea;
  margin-bottom: 5px;
  color: #333333 !important;
  background-color: #e6eef8 !important;
}

.custom-btn {
  font-size: 13px;
  box-shadow: none;
  border-radius: 6px;
  margin-top: -10px !important;
  margin-bottom: 15px;
  min-width: 60px;
}

.btn-container {
  display: flex;
  justify-content: flex-end;
}

.multiline-box {
  font-size: 14px;
  white-space: nowrap;
  display: flex;
  align-items: center;
  padding: 10px;
}

.input-width {
  max-width: 1600px;

  align-items: center;
}

.input-width-half {
  max-width: 797px;
  align-items: center;
}

.input-area-L {
  margin-left: 5px;
  margin-right: 5px;
  font-size: 13px !important;
}

.bottom-buttons-container {
  display: flex;
  justify-content: center;
  width: 100%;
  margin-top: 20px;
  margin-bottom: 20px;
  padding: 0 15px;
}

::v-deep(.input-area-L .v-field) {
  width: 715px;
  height: 152px !important;
  font-size: 13px !important;
}

::v-deep(.input-area-L input) {
  padding: 0 10px !important;
  font-size: 13px !important;
}

::v-deep(.date-input .v-field) {
  margin-left: 5px;
  margin-right: 5px;
  height: 33px !important;
  font-size: 15px !important;
}

::v-deep(.input-area .v-field) {
  margin-left: 5px;
  margin-right: 5px;
  height: 40px !important;
  font-size: 15px !important;
}


::v-deep(.input-area input) {
  padding: 0 10px !important;
  font-size: 13px !important;
}

/* 첨부파일 관련 css */
.white-text {
  color: white !important;
}

.file-btn {
  font-size: 14px;
  height: 35px;
  border-radius: 10px;
}

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

.required-star {
  color: #FF0000;
  margin-right: 2px;
  font-weight: bold;
}

.date-picker {
  width: 160px;
  max-width: 160px;
}

:deep(.dp__input) {
  border: 1px solid #b0b0b0;
  box-shadow: none;
  font-size: 14px;
  border-radius: 4px;
}

:deep(.dp__main) {
  font-family: inherit;
  z-index: 100;
}

:deep(.dp__theme_light) {
  --dp-primary-color: #2196F3;
  --dp-border-radius: 8px;
}

:deep(.dp__overlay_cell_active) {
  background-color: var(--dp-primary-color);
  color: white;
}
</style>