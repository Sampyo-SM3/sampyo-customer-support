<template>
  <v-container fluid class="pr-0 pl-0 pt-0">

    <!-- 진행 상태 표시 바 -->
    <v-row justify="center" class="mb-0 pt-0">
      <v-col cols="12" class="d-flex align-center justify-center">
        <div class="custom-stepper">
          <div
            v-for="(status, index) in progressStatuses"
            :key="index"
            class="step"
            :class="{
              active: step === index + 1,
              completed: step > index + 1
            }"
          >
            <div class="circle">{{ index + 1 }}</div>
            <div class="label">{{ status.text }}</div>
            <div v-if="index < progressStatuses.length - 1" class="line"></div>
          </div>
        </div>
      </v-col>
    </v-row>

    <br>
    <br>    

    <!-- 전체 래퍼: 접수상태 박스 + 버튼을 나란히 배치 -->
    <div class="d-flex align-center mb-4">
      <!-- 접수상태 박스 -->
      <!-- <v-row no-gutters class="search-row top-row bottom-row status-select-row"       -->
      <v-row no-gutters class="status-row status-select-row" style="width: 220px; 
        min-width: 220px; 
        max-width: 220px;">
        <v-col class="search-col">
          <div class="label-box">접수상태</div>
          <v-select v-model="selectedStatus" :items="progressStatuses" item-title="text" item-value="value"
            density="compact" variant="plain" hide-details class="status-select" />
        </v-col>
      </v-row>

      <v-btn variant="flat" color="#3A70B1" size="small" class="save-status-btn ml-3" @click="saveStatus">
        저장
      </v-btn>

      <!-- 오른쪽 정렬: 수정 + 상신 -->
      <div class="d-flex ml-auto">
        <v-btn v-if="inquiry.srFlag === 'N'" variant="flat" color="green darken-2"
          class="save-status-btn mr-2" size="small" @click="moveEidtSr">
          수정
        </v-btn>
        <v-btn v-if="inquiry.srFlag === 'N'" variant="flat" color="#F7A000"
          class="save-status-btn white-text" size="small" @click="approvalBtn">
          상신
        </v-btn>
        <v-btn v-if="inquiry.srFlag === 'Y'" variant="flat" color="#F7A000"
          class="save-status-btn white-text" size="small" @click="showSrBtn">
          SR요청서 보기
        </v-btn>
      </div>
    </div>


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
      <v-col class="search-col request-period">
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

    <!-- 첨부파일 -->
    <v-row no-gutters class="search-row bottom-row">
      <v-col class="search-col select-files file-attach d-flex align-center">
        <div class="label-box colNm">첨부파일</div>

        <div v-if="fetchedFiles.length > 0" class="ml-2 mt-2 mb-2" style="flex: 1;">
          <div class="d-flex flex-wrap" style="gap: 8px;">
            <div v-for="(file) in fetchedFiles" :key="file.seq" class="file-chip d-flex align-center px-3 py-2 fileBox"
              @click="downloadFile(file)">
              <span class="text-body-2" style="color:#1A5CA8">{{ file.fileName }}</span>
              <v-icon class="ml-2" size="20" style="color:#1A5CA8">mdi-download</v-icon>
            </div>
          </div>
        </div>
      </v-col>
    </v-row>



    <!-- 하단: 댓글 섹션을 아래로 배치 -->
    <v-row>
      <v-col cols="12">
        <div class="section-title">
          <div class="info-title-after"></div>답변 내용
        </div>
        
        <!-- 댓글 섹션 -->
        <div class="comments-container" v-if="commentTextLength > 0">
          <comment-tree v-for="comment in topLevelComments" :key="comment.commentId" :comment="comment"
            :all-comments="comments" @refresh="fetchComments" />
        </div>
        
        <!-- 댓글이 없을 때 메시지 -->
        <div v-else class="no-comments">
          <p>등록된 답변이 없습니다. 첫 번째 답변을 작성해보세요.</p>
        </div>
        
        <!-- 댓글 입력 -->
        <div class="comment-input-container" :class="{ 'mt-4': commentTextLength === 0 }">
          <v-textarea 
            auto-grow
            v-model="newComment.content" 
            :label="replyTo ? `${replyTo.userId}님에게 답글 작성` : '답변 입력'" 
            variant="outlined"
            density="comfortable"
            color="#3A70B1"
            rows="3"
            hide-details
            class="comment-textarea"></v-textarea>
          <div class="btn-container">
            <v-btn v-if="replyTo" variant="text" color="#666" class="cancel-btn mr-2" @click="cancelReply">            
              답글 취소
            </v-btn>
            <v-btn variant="flat" color="#3A70B1" class="white--text comment-submit-btn" @click="addComment()">
              답변 등록
            </v-btn>
          </div>
        </div>
      </v-col>
    </v-row>
  </v-container>

  <!-- 관리자 추가하기 팝업 -->
  <user-popup :show="showUserPopup" @manager-selected_sr_edit="editManager" @close="showUserPopup = false" />    
</template>


<script>
import apiClient from '@/api';
import CommentTree from '@/components/CommentTree.vue';  // CommentTree 컴포넌트 import
import { inject, onMounted } from 'vue';
import { useKakaoStore } from '@/store/kakao';
import { useAuthStore } from '@/store/auth';
import userPopup from '@/components/userPopup.vue';

export default {
  // props 정의 추가
  props: {
    receivedSeq: {
      type: [Number, String],
      required: false
    },
    userId: JSON.parse(localStorage.getItem("userInfo"))?.id || null
  },
  setup() {
    // 스토어 초기화
    const kakaoStore = useKakaoStore();
    const authStore = useAuthStore();

    const extraBreadcrumb = inject('extraBreadcrumb', null);
    const listButtonLink = inject('listButtonLink', null);

    onMounted(() => {
      if (extraBreadcrumb) {
        extraBreadcrumb.value = 'SR 요청';  // 🔥 추가하고 싶은 값
      }

      if (listButtonLink) {
        listButtonLink.value = '/views/CA/CA1000_10';  // 🔥 현재 페이지에 맞는 "목록" 경로 설정
      }
    });

    // 이 컴포넌트의 다른 메서드에서 사용할 수 있도록 반환
    return {
      kakaoStore,
      authStore
    }
  },
  components: {
    CommentTree,
    userPopup
  },
  unmounted() { // ❗ 컴포넌트가 언마운트될 때
    const listButtonLink = inject('listButtonLink', null);
    if (listButtonLink) {
      listButtonLink.value = null; // 페이지 벗어날 때 목록버튼 없애기
    }

    const extraBreadcrumb = inject('extraBreadcrumb', null);
    if (extraBreadcrumb) {
      extraBreadcrumb.value = null; // 페이지 벗어날 때 목록버튼 없애기
    }
  },
  data() {
    return {
      userInfo: null,       //사용자 ID

      step: 1,
      selectedStatus: '', // 추가된 상태 변수
      oldStatus: '', // 추가된 상태 변수
      inquiry: {
        sub: "",
        context: "",
        uId: "",
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
        processState: "",
        srFlag: "",
        docNum: ""
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
      fetchedFiles: [],
    };
  },
  methods: {
    async editManager(selectedManager) {    
      try {
        this.loading = true;

        const boardData = {
          "seq": this.receivedSeq,
          "manager": selectedManager.name,
          "managerId": selectedManager.usrId,
          "managerTel": selectedManager.handTelNo,
          "managerEmail": selectedManager.emailAddr
        };            

        // 게시글 등록 및 seq 값 반환
        await apiClient.post("/api/require/updateForm", boardData);      
      } catch (error) {
        console.error("관리자 수정 중 오류:", error);
        this.errorMessages = [error.message || "관리자 수정 중 오류가 발생했습니다."];
        this.showError = true;
      } finally {
        this.loading = false;
      }

      // 수정 성공 후 페이지 새로고침
      window.location.reload();      
    },      
    async getDetailInquiry() {
      const response = await apiClient.get("/api/require/detail", {
        params: { seq: this.receivedSeq }
      });

      const processState = response.data?.processState || "P"; // 기본값 설정
      this.selectedStatus = processState;
      this.step = this.statusMapping?.[this.selectedStatus] ?? 1;

      // 3. 나머지 데이터 매핑
      this.inquiry = {
        sub: response.data?.sub || "",
        etc: response.data?.etc || "",
        uid: response.data?.uid || "",
        writerId: response.data?.writerId || "",
        manager: response.data?.manager || "",
        srFlag: response.data?.srFlag || "",
        processState: processState,
      };

      this.inquiry = {
        sub: response.data?.sub || "",
        etc: response.data?.etc || "",
        uid: response.data?.uid || "",
        writerId: response.data?.writerId || "",
        manager: response.data?.manager || "",
        srFlag: response.data?.srFlag || "",
        processState: response.data?.processState || "P",
      };

      // response.data.writerId

      this.selectedStatus = this.inquiry.processState;

      //첨부파일 리스트 불러오기
      try {
        const fileList = await apiClient.get("/api/file-attach/fileList", {
          params: { seq: this.receivedSeq }
        });

        this.fetchedFiles = Array.isArray(fileList.data)
          ? fileList.data.filter(file => file && file.fileName)
          : [];

      } catch (error) {
        console.error("❌ 오류 발생:", error);
      }
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
          srFlag: response.data?.srFlag || "",
          docNum: response.data?.docNum || "",
          management: {
            PROGRESS: processState
          }
        };
      } catch (error) {
        console.error("❌ 오류 발생:", error);
      }

      //첨부파일 리스트 불러오기
      try {
        const fileList = await apiClient.get("/api/file-attach/fileList", {
          params: { seq: this.receivedSeq }
        });

        this.fetchedFiles = Array.isArray(fileList.data)
          ? fileList.data.filter(file => file && file.fileName)
          : [];
      } catch (error) {
        console.error("❌ 오류 발생:", error);
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
      //   c 종결
      // H 보류중
      // I 접수
      // P 미처리
      // S SR
      try {
        const userInfoString = localStorage.getItem('userInfo');
        const phone = JSON.parse(userInfoString).phone;

        const prevStatusName = this.getStatusName(this.oldStatus);
        // 이전 상태값이 false, null, undefined, 빈 문자열인 경우 알림톡 발송 중단
        if (!prevStatusName) {
          console.log('이전 상태값이 없어 알림톡 발송을 중단합니다.');
          alert("접수상태가 변경되지 않았습니다.");
          return;
        }

        // 이전 상태가 P(미처리)가 아니고, 선택된 상태가 P(미처리)인 경우 변경 불가
        if (this.oldStatus !== 'P' && this.selectedStatus === 'P') {
          alert("처리가 시작된 이후에는 미처리 상태로 돌아갈 수 없습니다.");
          // 선택된 상태를 이전 상태로 되돌림
          this.selectedStatus = this.oldStatus;
          return;
        }

        const statusData = {
          seq: this.receivedSeq,
          processState: this.selectedStatus
        };
        // API 요청: 댓글 DB에 저장
        await apiClient.post("/api/updateStatus", statusData);
        alert("접수상태가 저장되었습니다.");
        // 상태변경
        this.kakaoStore.sendAlimtalk(this.receivedSeq, this.getStatusName(this.oldStatus), this.getStatusName(this.selectedStatus), phone);
        // 상세정보 새로고침
        this.fetchRequireDetail();
        //this.management.PROGRESS = this.selectedStatus;
      } catch (error) {
        console.error("상태 저장 실패");
        this.fetchRequireDetail();
      }
    },
    moveEidtSr() {
      if (this.inquiry.srFlag === 'Y') {
        alert('상신 후에는 수정을 할 수 없습니다.');
        return;
      }

      this.$router.push({
        name: 'CA_PostEditSrForm',
        params: { receivedSeq: this.receivedSeq }
      })

      this.fetchRequireDetail();
    },
    async downloadFile(file) {
      try {
        console.log(file);

        const response = await apiClient.get("/api/download", {
          params: { filename: file.fileName },
          responseType: 'blob',
        });

        const blob = new Blob([response.data]);
        const url = window.URL.createObjectURL(blob);
        const link = document.createElement("a");
        link.href = url;
        link.setAttribute("download", file.fileName);
        document.body.appendChild(link);
        link.click();
        link.remove();
        window.URL.revokeObjectURL(url);
      } catch (error) {
        console.error("파일 다운로드 중 오류:", error);
      }
    },
    approvalBtn() {
      if (!confirm('상신 후에는 수정을 할 수 없습니다.')) {
        return;
      }

      try {
        // 폼 타입 결정        
        const baseUrl = 'https://bluesam.sampyo.co.kr/WebSite/Approval/Forms/FormLinkForLEGACY.aspx'
        const params = {
          key: this.receivedSeq,  // board seq번호
          empno: 'SPH221342320005', // 사원번호
          legacy_form: 'WF_FORM_SRTEST',
          datatype: 'xml',  // 데이터 타입          
          ip: '10.50.20.71', // 프로시저 호출되는 ip          
          db: 'SPC_TEST',     // 프로시저 호출되는 db
        };

        // 쿼리 파라미터 문자열 생성
        const queryString = new URLSearchParams(params).toString()
        const fullUrl = `${baseUrl}?${queryString}`

        const popupWidth = 800;
        const popupHeight = 900;

        // 현재 브라우저 창 기준 (멀티모니터에서도 정확히 뜸)
        const dualScreenLeft = window.screenLeft !== undefined ? window.screenLeft : window.screenX;
        const dualScreenTop = window.screenTop !== undefined ? window.screenTop : window.screenY;

        const windowWidth = window.innerWidth || document.documentElement.clientWidth || screen.width;
        const windowHeight = window.innerHeight || document.documentElement.clientHeight || screen.height;

        const left = dualScreenLeft + (windowWidth - popupWidth) / 2;
        const top = dualScreenTop + (windowHeight - popupHeight) / 2;

        const options = `width=${popupWidth},height=${popupHeight},top=${top},left=${left},location=no`;

        // 새 창에서 URL 열기
        const popupWindow = window.open(fullUrl, '_blank', options);

        // 팝업 창 닫힘 감지를 위한 타이머 설정
        if (popupWindow) {
          const checkPopupClosed = setInterval(() => {
            if (popupWindow.closed) {
              clearInterval(checkPopupClosed);
              // 팝업이 닫히면 현재 페이지 새로고침
              window.location.reload();
            }
          }, 500); // 500ms 간격으로 체크
        }
      } catch (error) {
        console.error('상신 처리 중 오류 발생:', error)
      }
    },
    showSrBtn() {
      try {
        // 폼 타입 결정        
        const baseUrl = 'https://bluesam.sampyo.co.kr/WebSite/Approval/Forms/Form.aspx'

        const params = {
          mode: '',  // board seq번호
          piid: this.inquiry.docNum,
          usid: '&usid=srbyeon@sampyo.co.kr'
        };

        const queryString = new URLSearchParams(params).toString()
        const fullUrl = `${baseUrl}?${queryString}`

        const popupWidth = 800;
        const popupHeight = 900;

        // 현재 브라우저 창 기준 (멀티모니터에서도 정확히 뜸)
        const dualScreenLeft = window.screenLeft !== undefined ? window.screenLeft : window.screenX;
        const dualScreenTop = window.screenTop !== undefined ? window.screenTop : window.screenY;

        const windowWidth = window.innerWidth || document.documentElement.clientWidth || screen.width;
        const windowHeight = window.innerHeight || document.documentElement.clientHeight || screen.height;

        const left = dualScreenLeft + (windowWidth - popupWidth) / 2;
        const top = dualScreenTop + (windowHeight - popupHeight) / 2;

        const options = `width=${popupWidth},height=${popupHeight},top=${top},left=${left},location=no`;

        // 새 창에서 URL 열기
        window.open(fullUrl, '_blank', options)
      } catch (error) {
        console.error('오류 발생:', error)
      }
    }
  },
  computed: {
    topLevelComments() {
      return Array.isArray(this.comments) ? this.comments.filter(comment => !comment.parentId) : [];
    },
    commentTextLength() {
      return Array.isArray(this.comments) ? this.comments.length : 0;
    },
    // 코드값으로 상태명을 반환하는 함수
    getStatusName() {
      return (statusCode) => {
        if (!statusCode || !this.progressStatuses.length) return '';

        const foundStatus = this.progressStatuses.find(status => status.value === statusCode);
        return foundStatus ? foundStatus.text : '';
      };
    },

    // 현재 선택된 상태명
    currentStatusName() {
      return this.getStatusName(this.selectedStatus);
    },

    // 이전 상태명
    previousStatusName() {
      return this.getStatusName(this.oldStatus);
    }
  },
  created() {
    // 초기화 시 현재 상태 설정
    this.selectedStatus = this.management.PROGRESS;

    this.userInfo = JSON.parse(localStorage.getItem("userInfo"));
  },
  mounted() {
    //접수상태 리스트 가져오기
    this.getStatus().then(() => {
      this.fetchRequireDetail();  // 상세 데이터 호출
    });

    // 댓글 데이터 가져오기
    this.fetchComments();

  },
  watch: {
    receivedSeq: {
      immediate: true  // 컴포넌트 생성 시점에도 즉시 실행
    },
    selectedStatus(newVal, oldVal) {
      // console.log(`📌 상태 변경: ${oldVal} → ${newVal}`);
      this.oldStatus = oldVal;
    }
  }
};
</script>

<style scoped>
.step {
  position: relative;
  text-align: center;
  flex: 1;
}

.custom-stepper {
  display: flex;
  align-items: center;
  justify-content: center;
  width: 100%;
  max-width: 750px;
  position: relative;
}

.circle {
  width: 44px;
  height: 44px;
  border-radius: 50%;
  background-color: #d5dce6;
  color: white;
  font-weight: bold;
  font-size: 16px;
  display: flex;
  align-items: center;
  justify-content: center;
  margin: 0 auto;
  z-index: 2;
  transition: all 0.3s ease;
  position: relative;  
}

.label {
  margin-top: 8px;
  font-size: 14px;
  color: #333;
}

.line {
  position: absolute;
  top: 22px;
  left: 50%;
  width: 100%;
  height: 4px;
  background-color: #d5dce6;
  z-index: 1;
}

.step:not(:last-child)::after {
  content: '';
  position: absolute;
  top: 22px;
  left: 50%;
  width: 100%;
  height: 4px;
  background-color: #d5dce6;
  z-index: 0;
}

.step.completed:not(:last-child)::after {
  background-color: #5b9bd5;
}

.step.completed .line {
  background-color: #5b9bd5;
}

.step.completed .circle {
  background-color: #5b9bd5;
}

.step.active .circle {
  background-color: #1867c0;
  box-shadow: 0 0 0 4px rgba(24, 103, 192, 0.2);
  font-size: 18px;
}

.step.active .label {
  color: #1867c0;
}

.status-row {
  overflow: hidden;
}

.status-select-row {
  border: 1px solid #e0e0e0;
  border-radius: 8px;
  overflow: hidden;
  /* height: 45px; */
  margin-bottom: 15px;
  /* height: 42px;   */
}

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
  /* align-items: center; */
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
  /* margin-bottom: 5px; */
  color: #333333 !important;
  background-color: #e6eef8 !important;
}

.white-text {
  color: white !important;
}

.status-select {
  margin-left: 15px;
  margin-bottom: 10px;
}

.status-select-row {
  border: 1px solid #e0e0e0;
  border-radius: 8px;
  overflow: hidden;
  height: 45px;
}

.status-select>>>.v-select__selection {
  font-size: 14.5px !important;
  margin-bottom: 2px;
  /* 원하는 크기로 조정 */
}

.mdi-menu-down::before {
  margin-right: 10px;
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
  display: flex;
  align-items: center;
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
  background-color: #B0CAE6;
  margin-right: 10px;
  position: relative;
}

.comments-container {
  margin-bottom: 20px;
  background-color: #f9fbfd;
  border-radius: 8px;
  padding: 10px 15px;
  border: 1px solid #E6EEF8;
}

.no-comments {
  padding: 20px;
  text-align: center;
  color: #666;
  background-color: #f9fbfd;
  border-radius: 8px;
  border: 1px solid #E6EEF8;
  margin-bottom: 20px;
}

.comment-input-container {
  margin-bottom: 40px;
  padding: 15px;
  background-color: #f9fbfd;
  border-radius: 8px;
  border: 1px solid #E6EEF8;
}

.comment-textarea {
  margin-bottom: 10px;
  background-color: white;
  border-radius: 4px;
  font-size: 14px;
}

.info-card {
  background-color: #f9f9f9;
  border-radius: 0;
  box-shadow: none;
  border: 1px solid #ddd;
  padding-top: 0 !important;
}

.colNm {
  width: 140px;
}

.vertical-label {
  height: 100%;
  background-color: #f1f5fb;
  display: flex;
  align-items: center;
  justify-content: center;
  color: #333;
  font-size: 14px;
  font-weight: 500;
  text-align: center;
  line-height: 1.5;
  border-right: 1px solid #ddd;
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
  margin-top: 10px;
}

.comment-submit-btn {
  font-size: 14px;
  text-transform: none;
  border-radius: 4px;
  height: 36px;
  color: white !important;
}

.cancel-btn {
  font-size: 14px;
  text-transform: none;
  border: 1px solid #ddd;
  border-radius: 4px;
  height: 36px;
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

.fileBox {
  background-color: rgba(26, 92, 168, 0.08);
  border: 1px solid #d0dff1;
  border-radius: 6px;
  cursor: pointer;
  user-select: none;
}

.commentBtn {
  border: 1px solid #888A8D !important;
  color: #5A5C5F !important;
  border-radius: 4px;
  padding: 4px 12px;
  background-color: white;
}
</style>