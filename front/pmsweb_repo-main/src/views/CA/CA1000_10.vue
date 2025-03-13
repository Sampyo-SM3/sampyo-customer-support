<template style="margin-top:-30px;">
  <v-container>
    <!-- 진행 상태 표시 바 -->
    <v-row justify="center" class="mb-6 pt-6">
      <v-col cols="12" class="d-flex align-center justify-center">
        <div class="stepper-container">
          <div v-for="(status, index) in progressStatuses" :key="index" class="stepper-item"
            :class="{ active: step >= index + 1 }">
            <div class="step-circle">{{ index + 1 }}</div>
            <span class="step-label">{{ status }}</span>
            <div v-if="index < progressStatuses.length - 1" class="step-line"
              :class="{ 'step-line-active': step > index + 1 }"></div>
          </div>
        </div>
      </v-col>
    </v-row>

    <!-- 여기서부터 접수상태 버튼 -->
    <v-row class="mt-10">
      <v-col cols="auto" class="d-flex align-center">
        <div class="status-selection-container" style="margin-left:6px;">
          <div class="status-label-box">
            <span>접수상태</span>
          </div>
          <div class="status-select-box">
            <v-select v-model="selectedStatus" :items="progressStatuses" hide-details density="compact" variant="plain"
              class="status-select"></v-select>
          </div>
        </div>

        <v-btn class="action-btn save-btn" @click="saveStatus">
          저장
        </v-btn>
      </v-col>
    </v-row>

    <v-row>
      <!-- 왼쪽: 요구사항 정의서 -->
      <div class="leftForm">
        <div class="section-title">
          <div class="info-title-after"></div>요구사항 정의서
        </div>

        <v-card class="pa-4 mt- info-card">
          <!-- 개요 -->
          <div class="info-subtitle">&nbsp;개요</div>
          <v-simple-table dense class="custom-table outline1">
            <tbody>
              <tr>
                <th class="table-header">과제명</th>
                <td colspan="2" class="outlineTd">{{ inquiry.PROJECT_NAME }}</td>
                <th class="table-header">사업 부문</th>
                <td colspan="2" class="outlineTd">{{ inquiry.BUSINESS_SECTOR }}</td>
              </tr>
              <tr>
                <th class="table-header">과제 개요</th>
                <td colspan="5" class="outlineTd">{{ inquiry.PROJECT_OVERVIEW }}</td>
              </tr>
              <tr>
                <th class="table-header">기존 문제점</th>
                <td colspan="5" class="outlineTd">{{ inquiry.PAIN_POINT }}</td>
              </tr>
              <tr>
                <th class="table-header">기대 효과</th>
                <td colspan="2" class="outlineTd">{{ inquiry.EXPECTED_EFFECT }}</td>
                <th class="table-header">최종 산출물</th>
                <td colspan="2" class="outlineTd">{{ inquiry.DELIVERABLES }}</td>
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

      <!-- 오른쪽: 문의 정보 관리 및 답변 -->
      <div class="rightForm">
        <div class="section-title">
          <div class="info-title-after"></div>답변 내용
        </div>

        <v-card class="pa-4 info-card">
          <!-- 댓글 섹션 -->
          <div v-if="commentTextLength > 0">
            <div class="info-subtitle">댓글 {{ commentTextLength }}</div>
            <v-card id="commentArea" class="pa-3 mb-3 info-inner-card">
              <v-list-item v-for="(comment, index) in comments" :key="index" class="comment-item">
                <div class="comment-content">
                  <div class="comment-text">{{ comment.text }}</div>
                  <div class="comment-timestamp">{{ comment.timestamp }}</div>
                </div>
              </v-list-item>
            </v-card>
          </div>

          <!-- 댓글 입력 -->
          <div class="comment-input-container" :class="{ 'mt-20': commentTextLength === 0 }">
            <v-textarea v-model="newComment" label="댓글 입력" class="custom-textarea"></v-textarea>
            <div class="btn-container">
              <v-btn class="custom-btn" @click="addComment">등록</v-btn>
            </div>
          </div>
        </v-card>
      </div>
    </v-row>
  </v-container>
</template>

<script>
import axios from "axios";

export default {
  data() {
    return {
      step: 1,
      selectedStatus: '미처리', // 추가된 상태 변수
      customer: {
        USER_NM: "배하준",
        MOBILE_NO: "010-8976-4852",
        EMAIL: "hjbae@gsenc.com",
        siteNm: "포항자이디오션",
      },
      inquiry: {
        PROJECT_NAME: "MQ01 몰탈 문서 자료실 개설",
        BUSINESS_SECTOR: "몰탈",
        PROJECT_OVERVIEW: "WEB에서 당사 제품 관련 자료를 자유롭게 내려 받고 인쇄할 수 있는 환경 제공 통해 고객 만족도 제고",
        PAIN_POINT: "제품 사용설명서, 제품 성적서 등 당사 몰탈 제품을 구매하는 고객들이 필수적으로 참고해야 할 문서 자료를 입수하기 어려움",
        EXPECTED_EFFECT: "업무 자동화, 고객 만족도 제고",
        DELIVERABLES: "WebSite",
        DETAIL_TASK: "수집 방식 최적화 필요",
        DETAIL_CONTENT: "수집 방식 최적화 필요",
        DETAIL_IT_DEV_REQUEST: "AI 모델 튜닝 필요"
      },
      management: {
        SECTOR: "몰탈",
        CS_MANAGER: "",
        PROGRESS: "종결",
        QA_TYPE: "자료요청",
        DEPARTMENT: "",
        RECEIPT_PATH: "WEB",
        SALES_MANAGER: "",
        INVESTIGATOR: "",
        RECEIPT_DATE: "",
        COMPLETION_DATE: "",
      },
      answer: "",
      comments: [],
      newComment: "",
      sectors: ["시멘트", "분체", "골재", "몰탈", "레미콘", "기타"],
      progressStatuses: ["미처리", "진행", "보류중", "종결"],
      qaTypes: ["제품/기술문의", "배차문의", "불편사항", "자료요청", "1:1문의"],
      receiptPaths: ["WEB", "KAKAO", "CALL", "CRM", "SIDP"],
    };
  },
  methods: {
    async fetchRequireDetail() {
      try {
        const response = await axios.get("http://localhost:8080/api/require/detail", {
          params: { seq: 1 }
        });
        console.log("📌 받아온 데이터:", response.data);
        this.requireDetail = response.data; // 데이터를 저장

        switch (response.data.processState) {
          case '미처리':
            this.step = 1;
            break;
          case '진행중':
            this.step = 2;
            break;
          case '보류중':
            this.step = 3;
            break;
          case '종결':
            this.stepteValue = 4;
            break;
          default:
            this.step = 1; // 기본값 (예외 처리)
        }

        // 받아온 데이터를 inquiry에 업데이트
        this.inquiry = {
          PROJECT_NAME: response.data.projectName,
          BUSINESS_SECTOR: response.data.businessSector,
          PROJECT_OVERVIEW: response.data.projectOverview,
          PAIN_POINT: response.data.currentIssue,
          EXPECTED_EFFECT: response.data.expectedEffect,
          DELIVERABLES: response.data.finalDeliverables,
          DETAIL_TASK: response.data.detailTask,
          DETAIL_CONTENT: response.data.detailContent,
          DETAIL_IT_DEV_REQUEST: response.data.detailItDevRequest
        };
      } catch (error) {
        console.error("❌ 오류 발생:", error);
      }
    },
    updateStep() {
      this.step = this.progressStatuses.indexOf(this.management.PROGRESS) + 1;
    },
    addComment() {
      if (this.newComment.trim()) {
        const timestamp = new Date().toLocaleString();
        this.comments.push({ text: this.newComment, timestamp });
        this.newComment = "";
      }
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
    commentTextLength() {
      return this.comments.length;
    }

  },
  created() {
    // 초기화 시 현재 상태 설정
    this.selectedStatus = this.management.PROGRESS;
  },
  mounted() {
    this.fetchRequireDetail(); // API 호출
  },
  watch: {
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
  font-size: 17px;
  line-height: 22px;
  color: #666;
  -webkit-text-size-adjust: none;
  letter-spacing: -0.05em;
  margin: 20px 0 10px;
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
  max-height: 361px;
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
  margin-left: 20px;
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
  /*height: 40px;*/
  width: 250px;
  border: 1px solid #DEE2E6;

}

.status-label-box {
  width: 80px;
  display: flex;
  align-items: center;
  justify-content: center;
  background-color: #F8F9FA;
  color: #4A5568;
  font-size: 14px;
  border-right: 1px solid #DEE2E6;
  font-weight: 500;

}

.status-select-box {
  width: 160px;
  background-color: #FFFFFF;
}

.status-select :deep(.v-field) {
  border-radius: 0;
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
  height: 34px;
  text-transform: none;
  font-size: 14px;
  border-radius: 0;
  letter-spacing: 0;
  background-color: #1867C0;
  color: white;
  box-shadow: none !important;
}
</style>