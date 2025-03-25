<template>
  <v-container fluid class="pr-5 pl-5 pt-7">

    <v-row>
      <v-col>
        <div class="title-div">간단 폼 입력</div>
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
        <v-text-field v-model="title" placeholder="제목을 입력하세요" clearable hide-details density="compact"
          variant="outlined" class="manager-search"></v-text-field>
      </v-col>
    </v-row>

    <v-row no-gutters class="search-row bottom-row">
      <!-- 내용 텍스트필드 -->
      <v-col class="search-col content-field">
        <div class="label-box">내 용</div>
        <v-textarea v-model="content" placeholder="내용을 입력하세요" auto-grow rows="18" clearable hide-details
          density="compact" variant="outlined" class="content-textarea">
        </v-textarea>
      </v-col>
    </v-row>

    <br>
    <br>

    <div class="d-flex justify-center">
      <v-btn variant="flat" color="secondary" class="custom-btn mr-2 white-text d-flex align-center" size="large"
        @click="goBack">
        <v-icon size="default" class="mr-1">mdi-close</v-icon>
        취소
      </v-btn>
      &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;
      <v-btn variant="flat" color="primary" class="custom-btn mr-2 white-text d-flex align-center" size="large"
        @click="insertBoard()">
        <v-icon size="default" class="mr-1">mdi-check</v-icon>
        접수
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
  data() {
    return {
      loading: false,
      errorMessages: [],
      showError: false,
      userName: null,
      userId: null,
      title: '',
      content: ''
    }
  },

  computed: {

  },

  watch: {

  },

  mounted() {
    this.checkLocalStorage();
    this.getUserInfo();
  },

  created() {
    // localStorage에서 사용자 정보 불러오기
    this.getUserInfo();
  },

  methods: {
    validateBoard() {
      // 검증 초기화
      this.errorMessages = [];

      // 제목 검증
      if (!this.title || this.title.trim() === '') {
        this.errorMessages.push('제목을 입력해주세요.');
        this.showError = true;
        return false;
      }

      // 내용 검증
      if (!this.content || this.content.trim() === '') {
        this.errorMessages.push('내용을 입력해주세요.');
        this.showError = true;
        return false;
      }

      return true;
    },

    async insertBoard() {
      this.showError = false;

      if (!this.validateBoard()) {
        return; // 검증 실패 시 함수 종료
      }

      try {
        const boardData = {
          "projectName": this.title,
          "projectContent": this.content,
          "requesterId": this.userId,
          "requesterName": this.userName,
          "processState": "C",
          "businessSector": "시멘트"
        };

        const response = await apiClient.post("/api/require/insert", boardData);

        console.log("게시글이 성공적으로 등록되었습니다.", response.data);
        // 여기에 성공 후 처리할 로직 추가 (예: 목록 페이지로 이동, 알림 표시 등)
        this.$router.push({ name: 'CA1000_10' });


      } catch (error) {
        // 에러 처리
        console.error("게시글 등록 중 오류가 발생했습니다.", error);

        // 스낵바에 오류 메시지 표시
        this.errorMessages = ["게시글 등록 중 오류가 발생했습니다."];
        this.showError = true;
      }
    },

    checkLocalStorage() {
      const midMenuFromStorage = localStorage.getItem('midMenu');
      const subMenuFromStorage = localStorage.getItem('subMenu');

      this.savedMidMenu = midMenuFromStorage ? JSON.parse(midMenuFromStorage) : null;
      this.savedSubMenu = subMenuFromStorage ? JSON.parse(subMenuFromStorage) : null;

      console.log('메뉴 클릭 후 midMenu:', this.savedMidMenu);
      console.log('메뉴 클릭 후 subMenu:', this.savedSubMenu);
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