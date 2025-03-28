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
      <v-col class="search-col product-category">
        <div class="label-box colNm">제목</div>
        <div class="author-value">{{ inquiry.sub }}</div>
      </v-col>
    </v-row>

    <v-row no-gutters class="search-row middle-row">
      <v-col class="search-col request-period">
        <div class="label-box colNm">업무명</div>
        <div class="author-value">{{ inquiry.taskName }}</div>
      </v-col>
    </v-row>

    <v-row no-gutters class="search-row middle-row">
      <v-col class="search-col request-period">
        <div class="label-box colNm">협조</div>
        <div class="author-value">{{ inquiry.help }}</div>
      </v-col>
    </v-row>

    <v-row no-gutters class="search-row middle-row">
      <v-col class="search-col request-period">
        <div class="label-box colNm">개발(변경) 필요성</div>
        <div class="author-value">{{ inquiry.necessity }}</div>
      </v-col>
    </v-row>
    <v-row no-gutters class="search-row middle-row">
      <v-col class="search-col request-period">
        <div class="label-box colNm">기대효과</div>
        <div class="author-value">{{ inquiry.effect }}</div>
      </v-col>
    </v-row>

    <v-row no-gutters class="search-row middle-row">
      <v-col class="search-col request-period">
        <div class="label-box colNm">개발(변경) 모듈</div>
        <div class="author-value">{{ inquiry.module }}</div>
      </v-col>
    </v-row>

    <v-row no-gutters class="search-row middle-row" style="height:200px;">
      <v-col class="search-col request-period" style="border-right: 1px solid #e0e0e0;">
        <div class="label-box colNm">개발(변경)<br />업무내용</div>
      </v-col>

      <v-col style="border-right: 1px solid #e0e0e0;">
        <div>
          <div class="sub-label">변경전</div>
          <div class="multiline-box">
            {{ inquiry.beforeTaskContent }}
          </div>
        </div>
      </v-col>

      <!-- 변경후 -->
      <v-col>
        <div>
          <div class="sub-label">변경후</div>
          <div class="multiline-box">
            {{ inquiry.afterTaskContent }}
          </div>
        </div>
      </v-col>
    </v-row>

    <v-row no-gutters class="search-row middle-row">
      <v-col cols="6" class="search-col d-flex align-center">
        <div class="label-box colNm">사용부서</div>
        <div class="author-value">{{ inquiry.useDept }}</div>
      </v-col>

      <v-col class="search-col d-flex align-center" style="max-width: 300px;">
        <div class="label-box colNm">첨부문서</div>
        <div class="author-value">{{ inquiry.attachDoc }}</div>
      </v-col>
    </v-row>

    <v-row no-gutters class="search-row middle-row">
      <v-col cols="6" class="search-col d-flex align-center">
        <div class="label-box colNm">의뢰일자</div>
        <div class="author-value">{{ inquiry.requestDate }}</div>
      </v-col>

      <v-col class="search-col" style="max-width: 300px;">
        <div class="label-box colNm">접수일자</div>
        <div class="author-value">{{ inquiry.acceptDate }}</div>
      </v-col>
    </v-row>

    <v-row no-gutters class="search-row middle-row">
      <v-col cols="6" class="search-col d-flex align-center">
        <div class="label-box colNm">완료요청일자</div>
        <div class="author-value">{{ inquiry.completeRequestDate }}</div>
      </v-col>

      <v-col class="search-col" style="max-width: 300px;">
        <div class="label-box colNm">완료일자</div>
        <div class="author-value">{{ inquiry.completeDate }}</div>
      </v-col>
    </v-row>

    <v-row no-gutters class="search-row middle-row">
      <v-col class="search-col request-period">
        <div class="label-box colNm">기타</div>
        <div class="author-value">{{ inquiry.etc }}</div>
      </v-col>
    </v-row>

    <v-row no-gutters class="search-row bottom-row">
      <v-col class="search-col request-period">
        <div class="label-box colNm">첨부목록</div>
        <div class="author-value"></div>
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
        requestDate: "",
        acceptDate: "",
        completeRequestDate: "",
        completeDate: "",
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
      answer: "",
      sectors: ["시멘트", "분체", "골재", "몰탈", "레미콘", "기타"],
      progressStatuses: [],
      qaTypes: ["제품/기술문의", "배차문의", "불편사항", "자료요청", "1:1문의"],
      receiptPaths: ["WEB", "KAKAO", "CALL", "CRM", "SIDP"],

    };
  },
  methods: {
    async fetchRequireDetail() {
      try {
        const response = await apiClient.get("/api/require/detail", {
          params: { seq: this.receivedSeq }
        });

        // ✅ response.data 또는 processState가 존재하는지 확인 후 할당
        if (!response.data || !response.data.processState) {
          console.warn("⚠ processState 값이 없습니다. 기본값(P)로 설정합니다.");
        }

        const processState = response.data?.processState || "P"; // 기본값 설정

        // ✅ 상태 매핑 체크 후 기본값 설정
        this.step = this.statusMapping?.[processState] ?? 1;

        // ✅ 선택된 상태 반영
        const matchedStatus = this.progressStatuses.find(status => status.value === processState);
        this.selectedStatus = matchedStatus ? matchedStatus.value : "P";

        // ✅ 받아온 데이터를 inquiry에 업데이트
        this.inquiry = {

          sub: response.data?.sub || "",
          context: response.data?.context || "",
          taskName: response.data?.taskName || "",
          help: response.data?.help || "",
          necessity: response.data?.necessity || "",
          effect: response.data?.effect || "",
          module: response.data?.module || "",
          beforeTaskContent: response.data?.beforeTaskContent || "",
          afterTaskContent: response.data?.afterTaskContent || "",
          useDept: response.data?.useDept || "",
          attachDoc: response.data?.attachDoc || "",
          requestDate: response.data?.requestDate || "",
          acceptDate: response.data?.acceptDate || "",
          completeRequestDate: response.data?.completeRequestDate || "",
          completeDate: response.data?.completeDate || "",
          etc: response.data?.etc || "",
          uid: response.data?.uid || "",
          usem: response.data?.usem || "",
          dpId: response.data?.dpId || "",
          dpDn: response.data?.dpDn || "",
          manager: response.data?.manager || "",
          division: response.data?.division || "",
          processState: response.data?.processState || "",
          management: {
            PROGRESS: processState
          }
        };
      } catch (error) {
        console.error("❌ 오류 발생:", error);
      }
    },

  },
  computed: {

  },
  created() {
    // 초기화 시 현재 상태 설정
    this.selectedStatus = this.management.PROGRESS;

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
    selectedStatus(newVal, oldVal) {
      console.log(`📌 상태 변경: ${oldVal} → ${newVal}`);
    }
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

.goBack-btn {
  height: 35px;
  min-width: 55px;
  font-size: 14px;
  border-radius: 6px;
  margin-bottom: 10px;
}
</style>