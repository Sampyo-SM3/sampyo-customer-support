<template>
  <v-container fluid class="pr-5 pl-5 pt-7">

    <v-row>
      <v-col>
        <div class="title-div">문의 상세보기</div>
        <div class="mt-2">
          <v-divider thickness="3" color="#578ADB"></v-divider>
        </div>
      </v-col>
    </v-row>

    <br>

    <v-row no-gutters class="search-row top-row">
      <v-col class="search-col product-category">
        <div class="label-box">작성자</div>
        <div class="author-value">{{ userName }}</div>
      </v-col>
    </v-row>

    <v-row no-gutters class="search-row middle-row">
      <!-- 제목 필드 -->
      <v-col class="search-col request-period">
        <div class="label-box">제 목</div>
        <div class="author-value">{{ inquiry.PROJECT_NAME }}</div>
      </v-col>
    </v-row>

    <v-row no-gutters class="search-row bottom-row">
      <!-- 내용 텍스트필드 -->
      <v-col class="search-col content-field">
        <div class="label-box">내 용</div>
        <div class="author-value content-textarea">{{ inquiry.PROJECT_CONTENT }}</div>
      </v-col>
    </v-row>

    <br>
    <br>

    <div class="d-flex justify-center">
      <v-btn variant="flat" color="primary" class="custom-btn mr-2 white-text d-flex align-center" size="large"
        @click="goBack">
        목록
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
</template>

<script>
import apiClient from '@/api';

export default {
  props: {
    receivedSeq: {
      type: [Number, String],
      required: false
    }
  },
  data() {
    return {
      loading: false,
      errorMessages: [],
      showError: false,
      inquiry: {
        REQUESTER_NAME: "",
        REQUESTER_DEPT_NM: "",
        REQUESTER_EMAIL: "",
        REQUESTER_PHONE: "",
        PROJECT_NAME: "",
        PROJECT_CONTENT: "",
        BUSINESS_SECTOR: "",
        REQUESTERID: "",
        PROCESS_STATE: ""
      },
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
    this.getDetailInquiry();
  },

  created() {
    // localStorage에서 사용자 정보 불러오기
    this.getUserInfo();
  },

  methods: {
    async getDetailInquiry() {
      const response = await apiClient.get("/api/require/detail", {
        params: { seq: this.receivedSeq }
      });

      this.inquiry = {
        REQUESTER_NAME: response.data?.requesterName || "",
        REQUESTER_DEPT_NM: response.data?.requesterDeptNm || "",
        REQUESTER_EMAIL: response.data?.requesterEmail || "",
        REQUESTER_PHONE: response.data?.requesterPhone || "",
        PROJECT_NAME: response.data?.projectName || "",
        BUSINESS_SECTOR: response.data?.businessSector || "",
        PROJECT_OVERVIEW: response.data?.projectOverview || "",
        PAIN_POINT: response.data?.currentIssue || "",
        EXPECTED_EFFECT: response.data?.expectedEffect || "",
        DELIVERABLES: response.data?.finalDeliverables || "",
        DETAIL_TASK: response.data?.detailTask || "",
        DETAIL_CONTENT: response.data?.detailContent || "",
        DETAIL_IT_DEV_REQUEST: response.data?.detailItDevRequest || "",
      };
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
      // 브라우저 히스토리에서 뒤로가기
      this.$router.go(-1);
    }
  }
}
</script>

<style scoped>
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
  padding-block: 10px;
  padding-left: 10px;
  width: 800px;
  font-weight: 400;
  height: 450px;
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
}

.white-text {
  color: white !important;
}
</style>