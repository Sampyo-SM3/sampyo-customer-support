<template style="margin-top:-30px;">
  <v-container class="ml-16 mr-16">
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

    <!-- 접수 상태 버튼 -->
    <v-row class="mt-10">
      <v-col cols="auto" class="d-flex align-center">
        <div class="status-selection-container">
          <div class="status-label-box">
            <span>접수상태</span>
          </div>
          <div class="status-select-box">
            <v-select v-model="selectedStatus" :items="progressStatuses" item-title="text" item-value="value"
              hide-details density="compact" variant="plain" class="status-select"></v-select>
          </div>
        </div>
        <v-btn class=" action-btn save-btn" @click="saveStatus">
          저장
        </v-btn>
      </v-col>
    </v-row>

    <v-row>
      <v-col cols="12">
        <!-- 상단: 요구사항 정의서 -->
        <div>
          <div class="section-title">
            <div class="info-title-after"></div>요구사항 정의서
          </div>

          <v-card class="pa-4 mt- info-card">
            <!-- 요청자 정보 -->
            <div class="info-subtitle">&nbsp;요청자 정보</div>
            <v-simple-table dense class="custom-table outline1">
              <tbody>
                <tr>
                  <th class="table-header">요청자</th>
                  <td class="outlineTd">{{ inquiry.REQUESTER_NAME }}</td>
                  <th class="table-header">소속</th>
                  <td class="outlineTd">{{ inquiry.REQUESTER_DEPT_NM }}</td>
                  <th class="table-header">이메일</th>
                  <td class="outlineTd">{{ inquiry.REQUESTER_EMAIL }}</td>
                  <th class="table-header">연락처</th>
                  <td class="outlineTd">{{ inquiry.REQUESTER_PHONE }}</td>
                </tr>
              </tbody>
            </v-simple-table>

            <!-- 개요 -->
            <div class="info-subtitle pt-5">&nbsp;개요</div>
            <v-simple-table dense class="custom-table outline1">
              <tbody>
                <tr>
                  <th class="table-header">과제명</th>
                  <td class="outlineTd">{{ inquiry.PROJECT_NAME }}</td>
                  <th class="table-header">과제 개요</th>
                  <td class="outlineTd">{{ inquiry.PROJECT_OVERVIEW }}</td>
                  <th class="table-header">사업 부문</th>
                  <td class="outlineTd">{{ inquiry.BUSINESS_SECTOR }}</td>
                </tr>
                <tr>
                  <th class="table-header">기존 문제점</th>
                  <td class="outlineTd">{{ inquiry.PAIN_POINT }}</td>
                  <th class="table-header">기대 효과</th>
                  <td class="outlineTd">{{ inquiry.EXPECTED_EFFECT }}</td>
                  <th class="table-header">최종 산출물</th>
                  <td class="outlineTd">{{ inquiry.DELIVERABLES }}</td>
                </tr>
              </tbody>
            </v-simple-table>

            <!-- 세부 요구사항 -->
            <div class="info-subtitle pt-5">&nbsp;세부 요구사항</div>
            <v-simple-table dense class="custom-table outline2">
              <thead>
                <tr>
                  <th>세부 실행 과제</th>
                  <th>내용</th>
                  <th>IT 개발 요청</th>
                </tr>
              </thead>
              <tbody>
                <tr>
                  <td class="outlineTd">{{ inquiry.DETAIL_TASK }}</td>
                  <td class="outlineTd">{{ inquiry.DETAIL_CONTENT }}</td>
                  <td class="outlineTd">{{ inquiry.DETAIL_IT_DEV_REQUEST }}</td>
                </tr>
              </tbody>
            </v-simple-table>
          </v-card>
        </div>
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
import axios from "axios";
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
        const statusList = await axios.get("http://localhost:8080/api/status/list");

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
        await this.getStatus();

        const response = await axios.get("http://localhost:8080/api/require/detail", {
          params: { seq: this.receivedSeq }
        });
        this.requireDetail = response.data; // 데이터를 저장

        this.step = this.statusMapping[response.data.processState] || 1;

        // ✅ 선택된 상태 반영 (P, I, H, C → UI에서 표시할 text 값)
        const matchedStatus = this.progressStatuses.find(status => status.value === response.data.processState);
        this.selectedStatus = matchedStatus ? matchedStatus.value : "P";

        // 받아온 데이터를 inquiry에 업데이트
        this.inquiry = {
          REQUESTER_NAME: response.data.requesterName,
          REQUESTER_DEPT_NM: response.data.requesterDeptNm,
          REQUESTER_EMAIL: response.data.requesterEmail,
          REQUESTER_PHONE: response.data.requesterPhone,
          PROJECT_NAME: response.data.projectName,
          BUSINESS_SECTOR: response.data.businessSector,
          PROJECT_OVERVIEW: response.data.projectOverview,
          PAIN_POINT: response.data.currentIssue,
          EXPECTED_EFFECT: response.data.expectedEffect,
          DELIVERABLES: response.data.finalDeliverables,
          DETAIL_TASK: response.data.detailTask,
          DETAIL_CONTENT: response.data.detailContent,
          DETAIL_IT_DEV_REQUEST: response.data.detailItDevRequest,
          management: {
            PROGRESS: response.data.processState || "P" // 업데이트된 상태 적용
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
        postId: Number(this.newComment.postId) || 1, // 게시글 ID
        userId: this.userInfo.id || "", // 유저 ID
        content: this.newComment.content, // 댓글 내용
        parentId: newParentId, // 부모 댓글 ID (없으면 NULL)
        depth: this.replyTo ? Number(this.replyTo.depth) + 1 : 0, // 대댓글이면 +1, 최상위 댓글이면 0
        createdAt: new Date().toISOString(),
        deleteYn: "N"
      };

      try {
        // API 요청: 댓글 DB에 저장
        await axios.post("http://localhost:8080/api/insertComment", commentData);
        alert("댓글 등록 성공!");

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
        // const response = await axios.get(`http://localhost:8080/api/comments/${this.receivedSeq}`);
        this.comments = [];
        const response = await axios.get(`http://localhost:8080/api/comments?postId=${this.receivedSeq}`);
        // http://localhost:8080/api/comments?postId=1
        this.comments = response.data;
      } catch (error) {
        console.error('댓글 조회 실패:', error);
      }
      try {
        const response = await axios.get(`http://localhost:8080/api/comments/${this.receivedSeq}`);
        this.comments = response.data;
      } catch (error) {
        console.error('댓글 조회 실패:', error);
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
    saveStatus() {
      this.management.PROGRESS = this.selectedStatus;
      this.updateStep();
      // 여기에 저장 로직을 추가할 수 있습니다
      alert('상태가 저장되었습니다: ' + this.selectedStatus);
    }
  },
  computed: {
    topLevelComments() {
      return this.comments.filter(comment => !comment.parentId);
    },
    commentTextLength() {
      return this.comments.length;
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

.info-title {
  color: black;
  /* 파란색 강조 */
  margin-right: 8px;
}

.section-title {
  font-size: 20px;
  margin-bottom: 15px;
  font-weight: 500;
}

.info-subtitle {
  font-size: 16px;
  line-height: 22px;
  color: #666;
  -webkit-text-size-adjust: none;
  letter-spacing: -0.05em;
  margin: 20px 0 6px;
  font-weight: 500;
}

.info-title-after {
  content: "";
  display: inline-block;
  width: 6px;
  height: 17px;
  background-color: #1867C0;
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

.custom-table {
  width: 100%;
  display: table !important;
  border-collapse: collapse;
}

.custom-table th,
.custom-table td {
  border: 1px solid #ddd;
  padding: 8px;
  text-align: left;
}

.custom-table th {
  background-color: #f1f1f1;
  font-weight: bold;
}

.custom-table td {
  background-color: white !important;
}

.dot {
  color: #1867C0;
  /* 파란색 점 */
  font-weight: bold;
  margin-right: 5px;
}

.separator {
  color: #E1E1E1;
  /* 색상 변경 */
  margin-right: 5px;
}

.info-inner-card {
  background-color: #ffffff;
  border-radius: 0;
  /* 내부 카드도 각지게 */
  padding: 12px;
  box-shadow: none !important;
  border: 1px solid #E3E3E3;
}

.greyText {
  color: #747470;
}

.info-title-after {
  content: "";
  display: inline-block;
  width: 6px;
  height: 17px;
  background-color: #1867C0;
  margin-right: 10px;
  margin-bottom: 3px;
  position: relative;
  top: 4px;
}

#commentArea {
  max-height: auto;
  overflow-y: auto;
  overflow-x: hidden;
  border: 1px solid #E3E3E3;
}

.comment-item {
  display: flex;
  width: 100%;
  padding: 10px;
}

.comment-content {
  display: flex;
  flex-direction: column;
  width: 100%;
}

.comment-text {
  text-align: left;
  font-size: 16px;
  margin-bottom: 5px;
}

.comment-timestamp {
  text-align: right;
  font-size: 12px;
  color: gray;
}

.info-text::before {
  content: "";
  display: inline-block;
  width: 1px;
  height: 11px;
  background-color: #e1e1e1;
  margin-right: 10px;
  margin-left: 20px;
}

.top-border {
  width: 100%;
  height: 2px;
  background-color: #1867C0
}

/* 제목 박스 스타일 */
.title-text {
  font-size: 15px;
  background-color: #f5f5f5;
  padding: 10px;
}

/* 카드 스타일 수정 */
.custom-card {
  background-color: #ffffff;
  border-radius: 0;
  /* 모서리 각지게 */
  border: 1px solid #ddd;
  padding: 15px;
}

.leftForm {
  width: 900px;
  margin-top: 10px;
}

.rightForm {
  width: 670px;
  margin-top: 10px;
  margin-left: 20px;
}

.custom-btn {
  background-color: #1867C0;
  color: white;
  font-size: 13px;
  border: none;
  box-shadow: none;
  border-radius: 0;
  margin-top: -10px !important;
}

.comment-input-container {
  display: flex;
  flex-direction: column;
  gap: 5px;
}

.btn-container {
  display: flex;
  justify-content: flex-end;
}

.mt-20 {
  margin-top: 20px;
}

/* 개요테이블 제목 가로길이 */
.outline1 .table-header {
  width: 130px !important;
  font-size: 14.5px;
  font-weight: 500;
  color: #333333;
  font-size: 14px;
  /* color: #753333; */
}

.outline2 thead th {
  width: 130px !important;
  color: #333333;
  font-size: 14px;
  font-weight: 500;
}

.outlineTd {
  font-size: 13.5px;
  color: #666666;
  font-family: "Noto Sans KR"
}

.status-selection-container {
  display: flex;
  width: 250px;
  height: 38px;
  border: 1px solid #DEE2E6;
  margin-left: 6px;
}

.status-label-box {
  width: 90px;
  display: flex;
  align-items: center;
  justify-content: center;
  background-color: #F8F9FA;
  color: #4A5568;
  font-size: 14px;
  border-right: 1px solid #DEE2E6;
  font-weight: 500;
  border-radius: 0px;
}

.status-select-box {
  width: 150px;
  font-size: 14px;
  background-color: #FFFFFF;
}

.status-select :deep(.v-field) {
  border-radius: 0px;
  box-shadow: none !important;
}

.status-select :deep(.v-field__input) {
  padding: 0 12px;
  font-size: 14px;
  display: flex;
  align-items: center;
}

.action-btn {
  width: 65px;
  height: 40px;
  text-transform: none;
  font-size: 14px;
  border-radius: 4px;
  letter-spacing: 0;
  background-color: #1867C0;
  color: white;
  box-shadow: none !important;
  margin-left: 8px;
}

.gap-4 {
  gap: 5px;
}

.comment-item {
  margin-bottom: 16px;
  border-bottom: 1px solid #eee;
}

.comment-header {
  display: flex;
  justify-content: space-between;
  margin-bottom: 8px;
}

.comment-user {
  font-weight: 500;
  color: #333;
}

.comment-timestamp {
  font-size: 12px;
  color: #666;
}

.comment-actions {
  margin-top: 8px;
  display: flex;
  justify-content: flex-end;
}

.comment-text {
  color: #444;
  font-size: 14px;
  line-height: 1.5;
}
</style>