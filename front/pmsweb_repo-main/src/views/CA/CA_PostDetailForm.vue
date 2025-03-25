<template style="margin-top:-30px;">
  <v-container fluid class="pr-5 pl-5 pt-7">
    <v-row>
      <v-col>
        <div class="title-div">SR 요청</div>
        <div class="mt-2">
          <v-divider thickness="3" color="#578ADB"></v-divider>
        </div>
      </v-col>
    </v-row>

    <br>
    <!-- 진행 상태 표시 바 -->
    <v-row justify="center" class="mb-6 pt-6">
      <v-col cols="12" class="d-flex align-center justify-center">
        <div class="stepper-container">
          <div v-for="(status, index) in progressStatuses" :key="index" class="stepper-item"
            :class="{ active: step >= index + 1 }">
            <div class="step-circle">{{ index + 1 }}</div>
            <span class="step-label">{{ status.text }}</span>
            <div v-if="index < progressStatuses.length - 1" class="step-line"
              :class="{ 'step-line-active': step > index + 1 }"></div>
          </div>
        </div>
      </v-col>
    </v-row>

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
        <div class="label-box">제목</div>
        <div class="author-value">{{ userName }}</div>
      </v-col>
    </v-row>

    <v-row no-gutters class="search-row middle-row">
      <v-col class="search-col request-period">
        <div class="label-box">업무명</div>
        <div class="author-value">{{ inquiry.PROJECT_NAME }}</div>
      </v-col>
    </v-row>

    <v-row no-gutters class="search-row middle-row">
      <v-col class="search-col request-period">
        <div class="label-box">협조</div>
        <div class="author-value">{{ inquiry.PROJECT_NAME }}</div>
      </v-col>
    </v-row>

    <v-row no-gutters class="search-row middle-row">
      <v-col class="search-col request-period">
        <div class="label-box">개발(변경) 필요성</div>
        <div class="author-value">{{ inquiry.PROJECT_NAME }}</div>
      </v-col>
    </v-row>

    <v-row no-gutters class="search-row middle-row">
      <v-col class="search-col request-period">
        <div class="label-box">기대효과</div>
        <div class="author-value">{{ inquiry.PROJECT_NAME }}</div>
      </v-col>
    </v-row>

    <v-row no-gutters class="search-row middle-row">
      <v-col class="search-col request-period">
        <div class="label-box">개발(변경)모듈</div>
        <div class="author-value">{{ inquiry.PROJECT_NAME }}</div>
      </v-col>
    </v-row>

    <v-row no-gutters class="search-row middle-row">
      <v-col class="search-col request-period">
        <div class="label-box">개발(변경) 업무내용</div>
        <div class="author-value">{{ inquiry.PROJECT_NAME }}</div>
      </v-col>
    </v-row>

    <v-row no-gutters class="search-row middle-row">
      <v-col class="search-col" style="max-width: 200px;">
        <div class="label-box">사용부서</div>
        <div class="author-value">{{ userName }}</div>
      </v-col>

      <v-col class="search-col" style="max-width: 300px;">
        <div class="label-box">첨부문서</div>
        <div class="author-value">{{ inquiry.PROJECT_NAME }}</div>
      </v-col>
    </v-row>

    <v-row no-gutters class="search-row middle-row">
      <v-col class="search-col" style="max-width: 200px;">
        <div class="label-box">의뢰일자</div>
        <div class="author-value">{{ userName }}</div>
      </v-col>

      <v-col class="search-col" style="max-width: 300px;">
        <div class="label-box">접수일자</div>
        <div class="author-value">{{ inquiry.PROJECT_NAME }}</div>
      </v-col>
    </v-row>

    <v-row no-gutters class="search-row middle-row">
      <v-col class="search-col" style="max-width: 200px;">
        <div class="label-box">완료요청일자</div>
        <div class="author-value">{{ userName }}</div>
      </v-col>

      <v-col class="search-col" style="max-width: 300px;">
        <div class="label-box">완료일자</div>
        <div class="author-value">{{ inquiry.PROJECT_NAME }}</div>
      </v-col>
    </v-row>

    <v-row no-gutters class="search-row bottom-row">
      <v-col class="search-col request-period">
        <div class="label-box">기타</div>
        <div class="author-value">{{ inquiry.PROJECT_NAME }}</div>
      </v-col>
    </v-row>



    <!-- 하단: 댓글 섹션을 아래로 배치 -->
    <v-row>
      <v-col cols="12">
        <div class="section-title">
          <div class="info-title-after"></div>답변 내용
        </div>

        <v-card class="pa-4 info-card">
          <!-- 댓글 섹션 -->
          <div v-if="commentTextLength > 0">
            <div class="info-subtitle">댓글 {{ commentTextLength }}</div>
            <v-card id="commentArea" class="pa-3 mb-3 info-inner-card">
              <comment-tree v-for="comment in topLevelComments" :key="comment.commentId" :comment="comment"
                :all-comments="comments" @refresh="fetchComments" />
            </v-card>
          </div>

          <!-- 댓글 입력 -->
          <div class="comment-input-container" :class="{ 'mt-20': commentTextLength === 0 }">
            <v-textarea v-model="newComment.content" :label="replyTo ? `${replyTo.userId}님에게 답글 작성` : '댓글 입력'"
              class="custom-textarea"></v-textarea>
            <div class="btn-container">
              <v-btn v-if="replyTo" text @click="cancelReply" class="mr-2">답글 취소</v-btn>
              <v-btn class="custom-btn" @click="addComment()">등록</v-btn>
            </div>
          </div>

        </v-card>
      </v-col>
    </v-row>
  </v-container>
</template>


<script>
import apiClient from '@/api';
import CommentTree from '@/components/CommentTree.vue';  // CommentTree 컴포넌트 import

export default {
  // props 정의 추가
  props: {
    receivedSeq: {
      type: [Number, String],
      required: false
    },
    userId: JSON.parse(localStorage.getItem("userInfo"))?.id || null
  },
  components: {
    CommentTree
  },
  data() {
    return {
      userInfo: null,       //사용자 ID

      step: 1,
      selectedStatus: '', // 추가된 상태 변수
      inquiry: {
        REQUESTER_NAME: "",
        REQUESTER_DEPT_NM: "",
        REQUESTER_EMAIL: "",
        REQUESTER_PHONE: "",
        PROJECT_NAME: "",
        BUSINESS_SECTOR: "",
        PROJECT_OVERVIEW: "",
        PAIN_POINT: "",
        EXPECTED_EFFECT: "",
        DELIVERABLES: "",
        DETAIL_TASK: "",
        DETAIL_CONTENT: "",
        DETAIL_IT_DEV_REQUEST: "",
        REQUESTERID: "",
        // 아래 데이터는 DETAIL_TASK, DETAIL_CONTENT, DETAIL_IT_DEV_REQUEST로 가져오고 있습니다.
        // 추후 데이터를 한 줄 씩 보여주는 방식으로 변경하면 아래 주석 부분을 사용해야 합니다.
        /////////////////////////////////////////////////////////////////////////////////
        // DETAIL_REQUIREMENTS: [
        //   {
        //     taskName: "1-1 몰탈 문서발급 메뉴 생성",
        //     description: "스마트 오더 홈페이지에 몰탈 제품 관련 문서 자료를 다운받을 수 있는 자료실 개설",
        //     itRequest: "스마트 오더 '몰탈 문서발급' 메뉴 신설, 회원 및 사업자 로그인 후 접근 가능"
        //   },
        //   {
        //     taskName: "1-2 삼표 스마트오더 홈페이지 접근성 개선",
        //     description: "네이버, 구글 등 주요 포털 사이트에서 '삼표 몰탈', '삼표 문서', '삼표 스마트오더' 검색 시 상위 노출되도록 조정",
        //     itRequest: "네이버 고객센터 등 연락하여 검색 로직 수정 요청"
        //   }
        // ]
      },
      management: {
        PROGRESS: ""
      },
      answer: "",
      comments: [],
      newComment: {
        content: "", // 댓글 내용
        postId: null, // 게시글 ID
        userId: "test_user", // 유저 ID
        parentId: null // 부모 댓글 ID
      },
      replyTo: null,
      sectors: ["시멘트", "분체", "골재", "몰탈", "레미콘", "기타"],
      progressStatuses: [],
      qaTypes: ["제품/기술문의", "배차문의", "불편사항", "자료요청", "1:1문의"],
      receiptPaths: ["WEB", "KAKAO", "CALL", "CRM", "SIDP"],

    };
  },
  methods: {
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
    async fetchRequireDetail() {
      try {
        console.log("🚀 fetchRequireDetail() 실행!");

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
          management: {
            PROGRESS: processState
          }
        };
      } catch (error) {
        console.error("❌ 오류 발생:", error);
      }
    },
    updateStep() {
      this.step = this.progressStatuses.indexOf(this.management.PROGRESS) + 1;
    },
    async addComment() {

      if (!this.newComment.content) {
        alert("댓글을 입력해주세요.");
        return;
      }

      // 부모 댓글인지 확인 후 parentId 설정
      var newParentId = this.replyTo ? this.replyTo.commentId : null;

      // 백엔드로 보낼 데이터 객체
      const commentData = {
        postId: this.receivedSeq, // 게시글 ID
        userId: this.userInfo.id || "", // 유저 ID
        content: this.newComment.content, // 댓글 내용
        parentId: newParentId, // 부모 댓글 ID (없으면 NULL)
        depth: this.replyTo ? Number(this.replyTo.depth) + 1 : 0, // 대댓글이면 +1, 최상위 댓글이면 0
        createdAt: new Date().toISOString(),
        deleteYn: "N"
      };

      try {
        // API 요청: 댓글 DB에 저장
        await apiClient.post("/api/insertComment", commentData);
        alert("댓글이 저장되었습니다.");

        // 입력 필드 초기화
        this.newComment.content = "";
        this.replyTo = null;

        // 댓글 목록 새로고침
        this.fetchComments();

      } catch (error) {
        console.error("댓글 등록 실패");
        this.fetchComments();
      }
    },
    async fetchComments() {

      try {
        // const response = await apiClient.get(`/api/comments/${this.receivedSeq}`);
        this.comments = [];
        const response = await apiClient.get(`/api/comments?postId=${this.receivedSeq}`);
        // /api/comments?postId=1
        this.comments = response.data;
      } catch (error) {
        console.error('댓글 조회 실패:', error);
        this.comments = []; // ✅ 오류 발생 시 빈 배열 설정
      }
      try {
        const response = await apiClient.get(`/api/comments/${this.receivedSeq}`);
        this.comments = response.data;
      } catch (error) {
        console.error('댓글 조회 실패:', error);
        this.comments = []; // ✅ 오류 발생 시 빈 배열 설정
      }
    },
    handleReply(comment) {
      this.replyTo = comment;
    },

    cancelReply() {
      this.replyTo = null;
      this.newComment.newComment = '';
    },
    // 추가된 메서드
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
        this.fetchRequireDetail();

        //this.management.PROGRESS = this.selectedStatus;
        //this.updateStep();
      } catch (error) {
        console.error("상태 저장 실패");
        this.fetchRequireDetail();
      }



    }
  },
  computed: {
    topLevelComments() {
      return Array.isArray(this.comments) ? this.comments.filter(comment => !comment.parentId) : [];
    },
    commentTextLength() {
      return Array.isArray(this.comments) ? this.comments.length : 0;
    }
  },
  created() {
    // 초기화 시 현재 상태 설정
    this.selectedStatus = this.management.PROGRESS;

    this.userInfo = JSON.parse(localStorage.getItem("userInfo"));
  },
  mounted() {
    //미처리 리스트 가져오기
    this.getStatus();

    // 요구사항 정의서 데이터 가져오기
    this.fetchRequireDetail(); // API 호출

    // 댓글 데이터 가져오기
    this.fetchComments();

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

.stepper-container {
  display: flex;
  align-items: center;
  justify-content: space-between;
  width: 100%;
  max-width: 600px;
  position: relative;
  user-select: none;
}

.stepper-item {
  display: flex;
  align-items: center;
  position: relative;
  flex: 1;
}

.step-circle {
  width: 48px;
  height: 48px;
  border-radius: 50%;
  background-color: lightgray;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 18px;
  font-weight: bold;
  color: white;
  z-index: 2;
  user-select: none;
}

.step-label {
  margin-top: 12px;
  text-align: center;
  font-size: 14px;
  font-weight: bold;
  position: absolute;
  bottom: -30px;
  left: 15%;
  transform: translateX(-50%);
  user-select: none;
}

.step-line {
  position: absolute;
  width: 100%;
  height: 5px;
  background-color: lightgray;
  /* 기본 회색 */
  top: 50%;
  left: 50%;
  transform: translateX(-50%);
  z-index: 1;
  transition: background-color 0.3s ease-in-out;
  /* 색상 변경 애니메이션 */
}

/* ✅ 진행된 상태일 때 파란색으로 변경 */
.step-line-active {
  background-color: #5B9BD5;
}

.active .step-circle {
  background-color: #1867C0;
  font-size: 20px;
}

.active .step-label {
  color: #1867C0;
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

.section-title {
  font-size: 17px;
  margin-bottom: 15px;
  font-weight: 400;
}

.info-title-after {
  content: "";
  display: inline-block;
  width: 6px;
  height: 17px;
  background-color: #578ADB;
  margin-right: 10px;
  margin-bottom: 3px;
  position: relative;
  top: 4px;
}

.info-card {
  background-color: #f9f9f9;
  border-radius: 0;
  box-shadow: none;
  border: 1px solid #ddd;
  padding-top: 0 !important;
}
</style>