<template>
  <v-container>
    <!-- 진행 상태 표시 바 (커스텀 디자인) test!!-->
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
      <!-- 왼쪽: 고객 문의 내용 -->
      <v-col cols="6">
        <v-card class="pa-4">
          <!-- 고객 문의 내용 제목 -->
          <v-card-title class="section-title">
            <!-- <v-icon class="mr-1 mb-1">mdi-account-question-outline</v-icon> 고객 문의 내용 -->
            <div class="info-title-after"></div>고객 문의 내용
          </v-card-title>

          <v-divider class="mb-3"></v-divider>

          <!-- 고객 정보 -->
          <div class="info-subtitle">고객 정보</div>
          <v-card class="pa-3 mb-3">
            <v-row>
              <v-col cols="6">
                <p class="greyText"><span class="dot">▪</span> <span class="info-title">거래처명</span> <span
                    class="separator">|</span> {{
                      customer.USER_NM }}</p>
                <p class="greyText"><span class="dot">▪</span> <span class="info-title">이메일</span> <span
                    class="separator">&nbsp;&nbsp;&nbsp;&nbsp;|</span>
                  {{
                    customer.EMAIL }}</p>
              </v-col>
              <v-col cols="6">
                <p class="greyText"><span class="dot">▪</span> <span class="info-title">연락처</span> <span
                    class="separator">|</span> {{
                      customer.MOBILE_NO }}</p>
                <p class="greyText"><span class="dot">▪</span> <span class="info-title">현장명</span> <span
                    class="separator">|</span> {{
                      customer.siteNm }}</p>
              </v-col>
            </v-row>
          </v-card>

          <!-- 문의 기본 정보 -->
          <div class="info-subtitle">문의 기본 정보</div>
          <v-card class="pa-3 mb-3">
            <v-row>
              <v-col cols="6">
                <p class="greyText"><span class="dot">▪</span> <span class="info-title">부문</span> <span
                    class="separator">&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;|</span> {{
                      inquiry.QA_SECTOR }}</p>
                <p class="greyText"><span class="dot">▪</span> <span class="info-title">문의구분</span> <span
                    class="separator">|</span> {{
                      inquiry.QA_TYPE }}</p>
              </v-col>
              <v-col cols="6">
                <p class="greyText"><span class="dot">▪</span> <span class="info-title">제품종류</span> <span
                    class="separator">|</span> </p>
                <p class="greyText"><span class="dot">▪</span> <span class="info-title">요청일</span> <span
                    class="separator">&nbsp;&nbsp;&nbsp;&nbsp;|</span> {{
                      inquiry.INSERT_DT }}</p>
              </v-col>
            </v-row>
          </v-card>
        </v-card>
      </v-col>

      <!-- 오른쪽: 문의 정보 관리 및 답변 -->
      <v-col cols="5">
        <v-card class="pa-4">
          <!-- 고객 문의 내용 제목 -->
          <v-card-title class="section-title">
            <!-- <v-icon class="mr-1 mb-1">mdi-account-question-outline</v-icon> 고객 문의 내용 -->
            <div class="info-title-after"></div>답변내용
          </v-card-title>

          <v-divider class="mb-3"></v-divider>

          <div class="info-subtitle">댓글</div>
          <v-card class="pa-3 mb-3">
            <v-list-item v-for="(comment, index) in comments" :key="index" class="comment-item">
              <div class="comment-content">
                <div class="comment-text">{{ comment.text }}</div>
                <div class="comment-timestamp">{{ comment.timestamp }}</div>
              </div>
            </v-list-item>


          </v-card>
          <v-card class="pa-3 mt-3">
            <v-textarea v-model="newComment" label="댓글 입력" class="mt-3"></v-textarea>
            <v-btn color="primary" @click="addComment">등록</v-btn>
          </v-card>
        </v-card>
      </v-col>
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
    },
  },
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
  /* 고객 문의 내용 제목 크기 키우기 */
  font-weight: bold;
}

.info-subtitle {
  font-size: 16px;
  /* "고객 정보", "문의 기본 정보" 크기 키우기 */
  font-weight: bold;
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
  color: #D3D3D3;
  margin-right: 5px;
}

.info-card {
  background-color: #f9f9f9;
  /* 흰색 박스 스타일 */
  border-radius: 8px;
  /* 둥근 모서리 */
  box-shadow: 0px 2px 5px rgba(0, 0, 0, 0.1);
  /* 가벼운 그림자 */
}

.greyText {
  color: #666666;
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

.comment-item {
  display: flex;
  width: 100%;
  padding: 10px;
}

.comment-content {
  display: flex;
  flex-direction: column;
  /* 세로 정렬 */
  width: 100%;
}

.comment-text {
  text-align: left;
  font-size: 16px;
  margin-bottom: 5px;
  /* 날짜와 간격 추가 */
}

.comment-timestamp {
  text-align: right;
  font-size: 12px;
  color: gray;
}
</style>