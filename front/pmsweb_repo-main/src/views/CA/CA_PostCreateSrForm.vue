<template style="margin-top:-30px;">
  <v-container fluid class="pr-5 pl-5 pt-7">
    <v-row>
      <v-col>
        <div class="d-flex align-center">
          <div class="title-div">SR 요청서 작성</div>
          <v-btn variant="outlined" color="primary" class="goBack-btn ml-auto mr-2" size="small"
            @click="$router.push('/views/CA/CA1000_10')">
            목록
          </v-btn>
        </div>
        <div class="mt-2">
          <v-divider thickness="3" color="#578ADB"></v-divider>
        </div>
      </v-col>
    </v-row>

    <br>
    <v-row no-gutters class="search-row top-row">
      <v-col class="search-col input-width">
        <div class="label-box colNm">제목</div>
        <v-text-field v-model="inquiry.sub" variant="outlined" density="compact" hide-details
          class="input-area title-field" />
      </v-col>
    </v-row>

    <v-row no-gutters class="search-row middle-row">
      <v-col class="search-col input-width">
        <div class="label-box colNm">업무명</div>
        <v-text-field v-model="inquiry.taskName" variant="outlined" density="compact" hide-details
          class="input-area"></v-text-field>
      </v-col>
    </v-row>

    <v-row no-gutters class="search-row middle-row">
      <v-col class="search-col input-width">
        <div class="label-box colNm">협조</div>
        <v-text-field v-model="inquiry.help" variant="outlined" density="compact" hide-details
          class="input-area"></v-text-field>
      </v-col>
    </v-row>

    <v-row no-gutters class="search-row middle-row">
      <v-col class="search-col input-width">
        <div class="label-box colNm">개발(변경) 필요성</div>
        <v-text-field v-model="inquiry.necessity" variant="outlined" density="compact" hide-details
          class="input-area"></v-text-field>
      </v-col>
    </v-row>

    <v-row no-gutters class="search-row middle-row">
      <v-col class="search-col input-width">
        <div class="label-box colNm">기대효과</div>
        <v-text-field v-model="inquiry.effect" variant="outlined" density="compact" hide-details
          class="input-area"></v-text-field>
      </v-col>
    </v-row>

    <v-row no-gutters class="search-row middle-row">
      <v-col class="search-col input-width">
        <div class="label-box colNm">개발(변경) 모듈</div>
        <v-text-field v-model="inquiry.module" variant="outlined" density="compact" hide-details
          class="input-area"></v-text-field>
      </v-col>
    </v-row>

    <v-row no-gutters class="search-row middle-row" style="height:200px;">
      <v-col class="search-col request-period" style="border-right: 1px solid #e0e0e0;">
        <div class="label-box colNm">개발(변경)<br />업무내용</div>
      </v-col>

      <v-col style="border-right: 1px solid #e0e0e0;">
        <div>
          <div class="sub-label">변경전</div>
          <v-textarea v-model="inquiry.beforeTaskContent" variant="outlined" density="compact" hide-details
            class="multiline-input" style="width: 100%;"></v-textarea>
        </div>
      </v-col>

      <!-- 변경후 -->
      <v-col>
        <div>
          <div class="sub-label">변경후</div>
          <v-textarea v-model="inquiry.afterTaskContent" variant="outlined" density="compact" hide-details
            class="multiline-input" style="width: 100%;"></v-textarea>
        </div>
      </v-col>
    </v-row>

    <v-row no-gutters class="search-row middle-row">
      <v-col class="search-col input-width-half">
        <div class="label-box colNm">사용부서</div>
        <v-text-field v-model="inquiry.useDept" variant="outlined" density="compact" hide-details class="input-area"
          style="width: 100%;"></v-text-field>
      </v-col>

      <v-col class="search-col input-width-half">
        <div class="label-box colNm">첨부문서</div>
        <v-text-field v-model="inquiry.attachDoc" variant="outlined" density="compact" hide-details class="input-area"
          style="width: 100%;"></v-text-field>
      </v-col>
    </v-row>

    <v-row no-gutters class="search-row middle-row">
      <v-col class="search-col input-width-half">
        <div class="label-box colNm">의뢰일자</div>
        <v-menu v-model="requestDateMenu" :close-on-content-click="false" transition="scale-transition" offset-y
          min-width="auto">
          <template v-slot:activator="{ props }">
            <div class="date-field-wrapper" v-bind="props"
              style="display: flex; align-items: center; gap: 4px; width: 200px; ">
              <v-text-field v-model="formattedRequestDate" class="input-area" density="compact" hide-details readonly
                variant="outlined" />
              <v-icon size="23" color="#7A7A7A">mdi-calendar-search</v-icon>
            </div>
          </template>

          <v-date-picker v-model="inquiry.requestDate" @update:model-value="requestDateMenu = false" locale="ko-KR"
            elevation="1" color="blue" width="310" first-day-of-week="1" show-adjacent-months scrollable
            :allowed-dates="allowedDates" />
        </v-menu>
      </v-col>


      <v-col class="search-col input-width-half">
        <div class="label-box colNm">접수일자</div>
        <v-menu v-model="acceptDateMenu" :close-on-content-click="false" transition="scale-transition" offset-y
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
        </v-menu>
      </v-col>
    </v-row>

    <v-row no-gutters class="search-row middle-row">
      <v-col class="search-col input-width-half">
        <div class="label-box colNm">완료요청일자</div>
        <v-menu v-model="completeRequestDateMenu" :close-on-content-click="false" transition="scale-transition" offset-y
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
        </v-menu>
      </v-col>

      <v-col class="search-col" style="max-width:600px;">
        <div class="label-box colNm">완료일자</div>
        <v-menu v-model="completeDateMenu" :close-on-content-click="false" transition="scale-transition" offset-y
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
        </v-menu>
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
        <v-text-field variant="outlined" density="compact" hide-details class="input-area"></v-text-field>
      </v-col>
    </v-row>
  </v-container>
</template>

<script>
import apiClient from '@/api';

export default {
  // props 정의 추가
  props: {
    receivedSeq: {
      type: [Number, String],
      required: false
    },
    userId: JSON.parse(localStorage.getItem("userInfo"))?.id || null
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
      progressStatuses: []

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
          requestDate: data?.requestDate || null,
          acceptDate: data?.acceptDate || null,
          completeRequestDate: data?.completeRequestDate || null,
          completeDate: data?.completeDate || null,
          etc: data?.etc || "",
          uid: data?.uid || "",
          usem: data?.usem || "",
          dpId: data?.dpId || "",
          dpDn: data?.dpDn || "",
          manager: data?.manager || "",
          division: data?.division || "",
          processState: data?.processState || ""
        });

      } catch (error) {
        console.error("❌ 요구사항 불러오기 오류:", error);
      }
    },


  },
  computed: {
    formattedRequestDate: {
      get() {
        return this.inquiry.requestDate || '';
      },
      set(val) {
        this.inquiry.requestDate = val || null;
      }
    },
    formattedAcceptDate: {
      get() {
        if (!this.inquiry.acceptDate) return '';
        const date = new Date(this.inquiry.acceptDate);
        if (isNaN(date)) return '';
        return date.toISOString().slice(0, 10); // YYYY-MM-DD
      },
      set(val) {
        const parsed = new Date(val);
        this.inquiry.acceptDate = isNaN(parsed) ? null : parsed;
      }
    },
    formattedCompleteRequestDate: {
      get() {
        if (!this.inquiry.completeRequestDate) return '';
        const date = new Date(this.inquiry.completeRequestDate);
        if (isNaN(date)) return '';
        return date.toISOString().slice(0, 10); // YYYY-MM-DD
      },
      set(val) {
        const parsed = new Date(val);
        this.inquiry.completeRequestDate = isNaN(parsed) ? null : parsed;
      }
    },
    formattedCompleteDate: {
      get() {
        if (!this.inquiry.completeDate) return '';
        const date = new Date(this.inquiry.completeDate);
        if (isNaN(date)) return '';
        return date.toISOString().slice(0, 10); // YYYY-MM-DD
      },
      set(val) {
        const parsed = new Date(val);
        this.inquiry.completeDate = isNaN(parsed) ? null : parsed;
      }
    }
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

.search-col {
  display: flex;
  align-items: center;
  padding: 0;
  border-left: 1px solid #e0e0e0;
}

.request-period,
.product-category {
  max-width: 550px;
  flex-grow: 0;
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
  margin-bottom: 5px;
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
  background-color: #1867C0;
  color: white;
  font-size: 13px;
  border: none;
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

.multiline-input {
  width: 100%;
  height: 150px;
}

.goBack-btn {
  height: 35px;
  min-width: 55px;
  font-size: 14px;
  border-radius: 6px;
  margin-bottom: 10px;
}

.input-width {
  max-width: 1600px;
}

.input-width-half {
  max-width: 797px;
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
  height: 33px !important;
  font-size: 15px !important;
}

::v-deep(.input-area input) {
  padding: 0 10px !important;
  font-size: 13px !important;
}
</style>