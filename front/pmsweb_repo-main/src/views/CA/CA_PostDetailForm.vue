<template>
  <v-container fluid class="pr-5 pl-5 pt-7">

    <v-row>
      <v-col>
        <div class="d-flex align-center">
          <div class="title-div">문의 상세보기</div>
          <v-btn 
            variant="outlined" 
            color="primary" 
            class="goBack-btn ml-auto mr-2" 
            size="small"
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
      <v-row no-gutters class="status-row top-row bottom-row status-select-row"
        style="width: 220px; min-width: 220px; max-width: 220px;">
        <v-col class="search-col">
          <div class="label-box">접수상태</div>
          <v-select 
            v-model="selectedStatus" 
            :items="progressStatuses" 
            item-title="text" 
            item-value="value"
            density="compact" 
            variant="plain" 
            hide-details 
            class="status-select" />
        </v-col>
      </v-row>

      <v-btn variant="outlined" color="primary" size="small" class="save-status-btn ml-3" @click="saveStatus">
        저장
      </v-btn>
      <v-btn variant="outlined" color="green darken-2" class="save-status-btn ml-auto mr-2" size="small" @click="$router.push({
        name: 'CA_PostCreateSrForm',
        params: { receivedSeq: this.receivedSeq }
      })">
        SR요청서
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
        <div class="author-value content-textarea">{{ inquiry.etc }}</div>
      </v-col>
    </v-row>

    <br>
    <br>


    <!-- 하단: 댓글 섹션을 아래로 배치 -->
    <v-row>
      <v-col cols="12">
        <div class="section-title">
          <div class="info-title-after"></div>답변 내용
        </div>
        <!-- 댓글 섹션 -->
        <!-- <div class="info-subtitle">댓글 {{ commentTextLength }}</div> -->
        <div class="pa-3 mb-3" v-if="commentTextLength > 0" style="margin-top:-20px;">
          <comment-tree v-for="comment in topLevelComments" :key="comment.commentId" :comment="comment"
            :all-comments="comments" @refresh="fetchComments" />
        </div>

        <!-- 댓글 입력 -->
        <div class="comment-input-container" :class="{ 'mt-20': commentTextLength === 0 }">
          <v-textarea v-model="newComment.content"
            :label="replyTo ? `${replyTo.userId}님에게 답글 작성` : '댓글 입력'"></v-textarea>
          <div class="btn-container">
            <v-btn v-if="replyTo" text @click="cancelReply" class="mr-2">답글 취소</v-btn>
            <v-btn variant="outlined" color="primary" @click="addComment()">등록</v-btn>
          </div>
        </div>
      </v-col>
    </v-row>
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
import CommentTree from '@/components/CommentTree.vue';  // CommentTree 컴포넌트 import

export default {
  props: {
    receivedSeq: {
      type: [Number, String],
      required: false
    },
  },
  components: {
    CommentTree
  },
  data() {
    return {
      step: 1,
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
      comments: [],
      newComment: {
        content: "", // 댓글 내용
        postId: null, // 게시글 ID
        userId: "test_user", // 유저 ID
        parentId: null // 부모 댓글 ID
      },
      replyTo: null,
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
    this.fetchComments();
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

      // 상태 매핑 체크 후 기본값 설정
      this.step = this.statusMapping?.[processState] ?? 1;

      // 선택된 상태 반영
      const matchedStatus = this.progressStatuses.find(status => status.value === processState);
      this.selectedStatus = matchedStatus ? matchedStatus.value : "P";

      this.inquiry = {
        sub: response.data?.sub || "",
        etc: response.data?.etc || "",
        uid: response.data?.uid || "",
        processState: response.data?.processState || "P",
      };

      this.selectedStatus = this.inquiry.processState;
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
        userId: this.userId || "", // 유저 ID
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
    test() {
      console.log('--test--');

      try {
        // 폼 타입 결정
        let formType = ''
        formType = 'WF_FORM_LEGACY_FI_STATE_UNBAN'
        // formType = 'WF_FO.RM_SR'
        // formType = 'WF_FORM_SR_V0'

        // URL 및 파라미터 설정
        // const baseUrl = 'https://bluesam.sampyo.co.kr/WebSite/Approval/Forms/FormLinkForLEGACY.aspx'
        const baseUrl = 'https://bluesam.sampyo.co.kr/WebSite/Approval/Forms/FormLinkForLEGACY.aspx'
        const params = {
          key: 1,
          empno: 1,
          legacy_form: formType,
          datatype: 'xml',
          ip: '127.0.0.1',
          db: 'tttt'
        }

        // 쿼리 파라미터 문자열 생성
        const queryString = new URLSearchParams(params).toString()
        const fullUrl = `${baseUrl}?${queryString}`

        // 새 창에서 URL 열기
        window.open(fullUrl, '_blank')


      } catch (error) {
        console.error('상신 처리 중 오류 발생:', error)
      }
    },
  }
}
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
  /* 기본 회색 */
  top: 50%;
  left: 50%;
  transform: translateX(-50%);
  z-index: 1;
  transition: background-color 0.3s ease-in-out;
  /* 색상 변경 애니메이션 */
}

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

.btn-container {
  display: flex;
  justify-content: flex-end;
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

.status-row.top-row, status-row.bottom-row{
  overflow: hidden;
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
  margin-bottom: 15px;
  height: 42px;  
}


.save-status-btn {
  height: 42px;
  min-width: 60px;
  font-size: 14px;
  border-radius: 6px;
  margin-bottom: 15px;
}

.goBack-btn {
  height: 35px;
  min-width: 55px;
  font-size: 14px;
  border-radius: 6px;
  margin-bottom: 10px;
}

.info-title-after {
  content: "";
  display: inline-block;
  width: 6px;
  height: 17px;
  background-color: #B0CAE6;
  margin-right: 10px;
  margin-bottom: 3px;
  position: relative;
  top: 4px;
}

.section-title {
  font-size: 17px;
  margin-bottom: 15px;
  font-weight: 400;
}
</style>