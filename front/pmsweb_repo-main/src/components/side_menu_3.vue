<template>
  <v-container>
    <v-row>
      <!-- 왼쪽: 고객 문의 내용 -->
      <v-col cols="6">
        <v-card class="pa-4">
          <v-card-title>고객 문의 내용</v-card-title>
          
          <v-card class="pa-3 mt-3">
            <v-card-subtitle>고객 정보</v-card-subtitle>
            <v-list>
              <v-list-item>
                <v-list-item-title>거래처명: <b>{{ customer.USER_NM }}</b></v-list-item-title>
              </v-list-item>
              <v-list-item>
                <v-list-item-title>연락처: <b>{{ customer.MOBILE_NO }}</b></v-list-item-title>
              </v-list-item>
              <v-list-item>
                <v-list-item-title>이메일: <b>{{ customer.EMAIL }}</b></v-list-item-title>
              </v-list-item>
              <v-list-item>
                <v-list-item-title>현장명: <b>{{ customer.siteNm }}</b></v-list-item-title>
              </v-list-item>
            </v-list>
          </v-card>
          
          <v-card class="pa-3 mt-3">
            <v-card-subtitle>문의 기본 정보</v-card-subtitle>
            <v-list>
              <v-list-item>
                <v-list-item-title>부문: <b>{{ inquiry.QA_SECTOR }}</b></v-list-item-title>
              </v-list-item>
              <v-list-item>
                <v-list-item-title>문의구분: <b>{{ inquiry.QA_TYPE }}</b></v-list-item-title>
              </v-list-item>
              <v-list-item>
                <v-list-item-title>요청일: <b>{{ inquiry.INSERT_DT }}</b></v-list-item-title>
              </v-list-item>
            </v-list>
          </v-card>
          
          <v-card class="pa-3 mt-3">
            <v-card-subtitle>고객 작성 내용</v-card-subtitle>
            <v-table>
              <thead>
                <tr>
                  <th colspan="2">{{ inquiry.TITLE }}</th>
                </tr>
              </thead>
              <tbody>
                <tr>
                  <td colspan="2" class="board-cont">{{ inquiry.CONTENT }}</td>
                </tr>
              </tbody>
            </v-table>
          </v-card>
        </v-card>
      </v-col>
      
      <!-- 오른쪽: 문의 정보 관리 및 답변 -->
      <v-col cols="6">
        <v-card class="pa-3 mt-3">
          <v-card-subtitle>문의 정보 관리</v-card-subtitle>
          <v-form>
            <v-row>
              <v-col cols="6">
                <v-select label="부문" v-model="management.SECTOR" :items="sectors"></v-select>
              </v-col>
              <v-col cols="6">
                <v-text-field label="CS 담당자" v-model="management.CS_MANAGER"></v-text-field>
              </v-col>
              <v-col cols="6">
                <v-select label="진행 상태" v-model="management.PROGRESS" :items="progressStatuses"></v-select>
              </v-col>
              <v-col cols="6">
                <v-select label="문의 구분" v-model="management.QA_TYPE" :items="qaTypes"></v-select>
              </v-col>
              <v-col cols="6">
                <v-text-field label="소속 부서" v-model="management.DEPARTMENT"></v-text-field>
              </v-col>
              <v-col cols="6">
                <v-select label="접수 경로" v-model="management.RECEIPT_PATH" :items="receiptPaths"></v-select>
              </v-col>
            </v-row>
            <v-btn color="primary" class="mt-3">저장</v-btn>
          </v-form>
        </v-card>

        <v-card class="pa-3 mt-3">
          <v-card-subtitle>문의 답변</v-card-subtitle>
          <v-textarea v-model="answer" label="문의 답변"></v-textarea>
          <v-btn color="primary" class="mt-3">답변 등록</v-btn>
        </v-card>
      </v-col>
    </v-row>
  </v-container>
</template>

<script>
export default {
  data() {
    return {
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
        TITLE: "건조시멘트모르타르(일반용) 자체시험성적서 요청건.",
        CONTENT: "몰탈자체시험성적서,성과대비표-건조시멘트모르타르(일반미장용)/포항자이디오션/삼표산업(김해몰탈공장)/2025-02-07",
      },
      management: {
        SECTOR: "몰탈",
        CS_MANAGER: "",
        PROGRESS: "종결",
        QA_TYPE: "자료요청",
        DEPARTMENT: "",
        RECEIPT_PATH: "WEB",
      },
      answer: "안녕하십니까, 삼표산업입니다.\n'hjbae@gsenc.com' 메일로 요청파일 송부드렸습니다.\n다른 문의사항 있으시면 연락 부탁드립니다.\n감사합니다.",
      sectors: ["시멘트", "분체", "골재", "몰탈", "레미콘", "기타"],
      progressStatuses: ["미처리", "진행", "보류중", "종결"],
      qaTypes: ["제품/기술문의", "배차문의", "불편사항", "자료요청", "1:1문의"],
      receiptPaths: ["WEB", "KAKAO", "CALL", "CRM", "SIDP"],
    };
  },
};
</script>

<style scoped>
.board-cont {
  white-space: pre-wrap;
}
</style>
