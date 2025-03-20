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
        <v-text-field v-model="requesterId" placeholder="제목을 입력하세요" clearable hide-details density="compact"
          variant="outlined" class="manager-search"></v-text-field>        
      </v-col>   
    </v-row>

    <v-row no-gutters class="search-row bottom-row">
      <!-- 내용 텍스트필드 -->
      <v-col class="search-col content-field">
        <div class="label-box">내 용</div>
        <v-textarea 
          v-model="content" 
          placeholder="내용용을 입력하세요" 
          auto-grow
          rows="8"
          clearable 
          hide-details 
          density="compact"
          variant="outlined" 
          class="content-textarea">
        </v-textarea>       
      </v-col>
    </v-row>

    <br>

    <div class="d-flex justify-center">      
      <v-btn variant="flat" color="primary" class="custom-btn mr-2 white-text d-flex align-center" size="small" @click="fetchData()">
          <v-icon size="default" class="mr-1">mdi-close</v-icon>
          취소
      </v-btn>                    

      <v-btn variant="flat" color="primary" class="custom-btn mr-2 white-text d-flex align-center" size="small" @click="fetchData()">
          <v-icon size="default" class="mr-1">mdi-check</v-icon>
          접수
      </v-btn>                          
    </div>
    <br>
    <br>

    <br>
    <v-divider></v-divider>
    <br>

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
// import apiClient from '@/api';

export default {
  data() {
    return {
      loading: false,      
      errorMessages: [],
      showError: false,
      userName: null,
      requesterId: '',
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
    }
  }
}
</script>

<style scoped>  
.product-category {
  display: flex;
  flex-direction: row; /* 가로 방향으로 배치 */
  align-items: center;
  flex-wrap: nowrap; /* 줄바꿈 방지 */
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

.manager-search, .content-textarea {
  padding-block: 10px;
  padding-left: 10px;
  width: 800px;
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
  /* 하단 테두리 제거 */
}

.search-row.top-row {
  border-top: 3px solid #e0e0e0;
}

.search-row.bottom-row {
  border-bottom: 2px solid #e0e0e0;
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