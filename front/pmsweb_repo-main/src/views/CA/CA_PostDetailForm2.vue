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

    <!-- 전체 래퍼: 접수상태 박스 + 버튼을 나란히 배치 -->
    <div class="d-flex align-center mb-4">
      <!-- 접수상태 박스 -->
      <v-row no-gutters class="search-row top-row bottom-row status-select-row"
        style="width: 220px; min-width: 220px; max-width: 220px;">
        <v-col class="search-col">
          <div class="label-box">접수상태</div>
          <v-select v-model="selectedStatus" :items="progressStatuses" item-title="text" item-value="value"
            density="compact" variant="plain" hide-details class="status-select" />
        </v-col>
      </v-row>

      <v-btn variant="flat" color="primary" size="small" class="save-status-btn ml-3" @click="saveStatus">
        저장
      </v-btn>
    </div>


    <v-row no-gutters class="search-row top-row">
      <v-col class="search-col product-category">
        <div class="label-box">작성자</div>
        <div class="author-value">{{ inquiry.uid }}</div>
      </v-col>
    </v-row>

    <v-row no-gutters class="search-row middle-row">
      <!-- 제목 필드 -->
      <v-col class="search-col request-period">
        <div class="label-box">제 목</div>
        <div class="author-value">{{ inquiry.sub }}</div>
      </v-col>
    </v-row>

    <v-row no-gutters class="search-row bottom-row">
      <!-- 내용 텍스트필드 -->
      <v-col class="search-col content-field">
        <div class="label-box">내 용</div>
        <div class="author-value content-textarea">{{ inquiry.context }}</div>
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
      selectedStatus: '',
      inquiry: {
        sub: "",
        context: "",
        uId: "",
      },
      progressStatuses: [],
    }
  },

  computed: {

  },

  watch: {
    receivedSeq: {
      immediate: true  // 컴포넌트 생성 시점에도 즉시 실행
    },
    selectedStatus(newVal, oldVal) {
      console.log(`📌 상태 변경: ${oldVal} → ${newVal}`);
    }
  },

  mounted() {
    this.checkLocalStorage();
    this.getUserInfo();
    this.getStatus();
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

      console.log(response);

      const processState = response.data?.processState || "P"; // 기본값 설정

      // ✅ 상태 매핑 체크 후 기본값 설정
      this.step = this.statusMapping?.[processState] ?? 1;

      // ✅ 선택된 상태 반영
      const matchedStatus = this.progressStatuses.find(status => status.value === processState);
      this.selectedStatus = matchedStatus ? matchedStatus.value : "P";

      this.inquiry = {
        sub: response.data?.sub || "",
        context: response.data?.context || "",
        uid: response.data?.uid || "",
        PROCESS_STATE: response.data?.processState || "P",
      };

      this.selectedStatus = this.inquiry.PROCESS_STATE;
    },
    async getStatus() {
      try {
        const statusList = await apiClient.get("/api/status/list");

        // 상태 이름 리스트 저장
        this.progressStatuses = statusList.data.map(status => ({
          text: status.codeName,  // UI에 표시할 값
          value: status.codeId    // 실제 선택될 값
        }));

        // 상태 매핑 (codeName → 숫자 변환용)
        this.statusMapping = statusList.data.reduce((map, status) => {
          map[status.codeId] = status.orderNum; // "P" → 1, "I" → 2, "H" → 3, "C" → 4
          return map;
        }, {});

      } catch (error) {
        console.error("❌ 오류 발생:", error);
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
      // 브라우저 히스토리에서 뒤로가기
      this.$router.go(-1);
    },
    async saveStatus() {
      try {
        const statusData = {
          seq: this.receivedSeq,
          processState: this.selectedStatus
        };

        // API 요청: 댓글 DB에 저장
        await apiClient.post("/api/updateStatus", statusData);
        alert("접수상태가 저장되었습니다.");

        // 상세정보 새로고침
        this.getDetailInquiry();

        //this.management.PROGRESS = this.selectedStatus;
      } catch (error) {
        console.error("상태 저장 실패");
        this.getDetailInquiry();
      }
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
  margin-bottom: 5px;
}

.white-text {
  color: white !important;
}

.status-select-row {
  border-radius: 8px;
  overflow: hidden;
  margin-bottom: 15px;
  height: 42px;
}

.status-select {
  margin-left: 15px;
  margin-bottom: 10px;
}

.status-select>>>.v-select__selection {
  font-size: 14.5px !important;
  margin-bottom: 2px;
  /* 원하는 크기로 조정 */
}

.mdi-menu-down::before {
  margin-right: 10px;
}

.status-select-row {
  border: 1px solid #e0e0e0;
  border-radius: 8px;
  overflow: hidden;
  height: 45px;
}

.save-status-btn {
  height: 42px;
  min-width: 60px;
  font-size: 14px;
  border-radius: 6px;
  margin-bottom: 15px;
}
</style>