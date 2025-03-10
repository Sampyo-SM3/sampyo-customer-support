<template>
  <v-container>
    <!-- 진행 상태 표시 바 -->
    <v-row justify="center" class="mb-6">
      <v-col cols="12" class="d-flex align-center justify-center">
        <div class="stepper-container">
          <div v-for="(status, index) in progressStatuses" :key="index" class="stepper-item"
            :class="{ active: step >= index + 1 }">
            <div class="step-circle">{{ index + 1 }}</div>
            <span class="step-label">{{ status }}</span>
            <div v-if="index < progressStatuses.length - 1" class="step-line"></div>
          </div>
        </div>
      </v-col>
    </v-row>

    <v-row>
      <!-- 왼쪽: 고객 문의 내용 (너비 고정) -->
      <div class="leftForm">
        <div class="section-title">
          <div class="info-title-after"></div>고객 문의 내용
        </div>

        <v-card class="pa-4 info-card">
          <!-- 고객 정보 -->
          <div class="info-subtitle">고객 정보</div>
          <v-card class="pa-3 mb-3 info-inner-card">
            <v-row>
              <v-col cols="6">
                <p class="greyText"><span class="dot">▪</span> <span class="info-title">거래처명</span>
                  <span class="info-text customer-text">{{ customer.USER_NM }}</span>
                </p>
                <p class="greyText"><span class="dot">▪</span> <span class="info-title">이메일</span>
                  <span class="info-text customer-text" style="margin-left:14px;">{{ customer.EMAIL }}</span>
                </p>
              </v-col>
              <v-col cols="6">
                <p class="greyText"><span class="dot">▪</span> <span class="info-title">연락처</span>
                  <span class="info-text customer-text">{{ customer.MOBILE_NO }}</span>
                </p>
                <p class="greyText"><span class="dot">▪</span> <span class="info-title">현장명</span>
                  <span class="info-text customer-text">{{ customer.siteNm }}</span>
                </p>
              </v-col>
            </v-row>
          </v-card>

          <!-- 문의 기본 정보 -->
          <div class="info-subtitle">문의 기본 정보</div>
          <v-card class="pa-3 mb-3 info-inner-card">
            <v-row>
              <v-col cols="6">
                <p class="greyText"><span class="dot">▪</span> <span class="info-title">부문</span>
                  <span class="info-text customer-text" style="margin-left:30px;">{{ inquiry.QA_SECTOR }}</span>
                </p>
                <p class="greyText"><span class="dot">▪</span> <span class="info-title">문의구분</span>
                  <span class="info-text customer-text">{{ inquiry.QA_TYPE }}</span>
                </p>
              </v-col>
              <v-col cols="6">
                <p class="greyText"><span class="dot">▪</span> <span class="info-title">제품종류</span>
                  <span class="info-text customer-text">{{ inquiry.PRODUCT_TYPE || '미정' }}</span>
                </p>
                <p class="greyText"><span class="dot">▪</span> <span class="info-title">요청일</span>
                  <span class="info-text customer-text" style="margin-left:15px;">{{ inquiry.INSERT_DT }}</span>
                </p>
              </v-col>
            </v-row>
          </v-card>

          <!-- 고객 작성 내용 -->
          <div class="info-subtitle">고객 작성 내용</div>
          <v-card class="pa-3 mb-3 info-inner-card custom-card">
            <!-- 파란색 상단 라인 -->
            <div class="top-border"></div>

            <!-- 제목 (굵게) -->
            <p class="bold-text title-text">{{ inquiry.TITLE }}</p>

            <!-- 설명 부분 -->
            <p class="greyText description-text">{{ inquiry.DESCRIPTION }}</p>
          </v-card>

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
          <div class="comment-input-container">
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
export default {
  data() {
    return {
      step: 2,
      customer: {
        USER_NM: "배하준",
        MOBILE_NO: "010-8976-4852",
        EMAIL: "hjbae@gsenc.com",
        siteNm: "포항자이디오션",
      },
      inquiry: {
        QA_SECTOR: "몰탈",
        QA_TYPE: "자료요청",
        INSERT_DT: "2025-03-03",
        TITLE: "해주세요 그냥 해주세요",
        DESCRIPTION: "만들어줭 그냥 만들어줭요"
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
    updateStep() {
      this.step = this.progressStatuses.indexOf(this.management.PROGRESS) + 1;
    },
    addComment() {
      if (this.newComment.trim()) {
        const timestamp = new Date().toLocaleString();
        this.comments.push({ text: this.newComment, timestamp });
        this.newComment = "";
      }
    }
  },
  computed: {
    commentTextLength() {
      return this.comments.length;
    }
  }
};
</script>

<style scoped>
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
  top: 50%;
  left: 50%;
  transform: translateX(-50%);
  z-index: 1;
}

.active .step-circle {
  background-color: #1867C0;
  font-size: 20px;
}

.active .step-label {
  color: #1867C0;
}

.active+.step-line {
  background-color: #1867C0;
}

.info-title {
  color: black;
  /* 파란색 강조 */
  margin-right: 8px;
}

.section-title {
  font-size: 20px;
  margin-bottom: 10px;
  /* 박스와 간격 추가 */
}


.info-subtitle {
  font-size: 15px;
  color: #666666;
  margin-left: 5px;
  margin-bottom: 2px;
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


.info-card {
  background-color: #f9f9f9;
  border-radius: 0;
  /* 모서리를 각지게 */
  box-shadow: none !important;
  /* 그림자 제거 */
  border: 1px solid #ddd;
  /* 경계선 유지 */
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
    /* 파란색 */

}

/* 제목 박스 스타일 */
.title-text {
  font-size: 15px;
  background-color: #f5f5f5;
  /* 연한 회색 배경 */
  padding: 10px;
}

/* 설명 스타일 (왼쪽 들여쓰기) */
.description-text {
  color: #666;
  font-size: 14px;
  margin-top: 10px;
  margin-left: 15px;
  /* 🔹 설명을 제목보다 안으로 들여쓰기 */
  line-height: 1.6;
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
  width: 850px;
  margin-top: 10px;
  margin-left: 20px;
}

.rightForm {
  width: 720px;
  margin-top: 10px;
  margin-left: 20px;
}

.custom-btn {
  background-color: #1867C0;
  /* 파란색 */
  color: white;
  /* 글씨색 */
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

.customer-text {
  font-size: 15px;
  color: #555;
}
</style>
