<template>
  <v-container fluid class="pr-0 pl-0 pt-0">
    <v-row>
      <v-col>
        <v-divider thickness="3" color="#578ADB" class="mb-6"></v-divider>
      </v-col>
    </v-row>

    <v-row no-gutters class="search-row top-row">
      <!-- 제목 필드 -->
      <v-col class="search-col">
        <div class="label-box">제 목</div>
        <div class="author-value">{{ title }}</div>
      </v-col>
    </v-row>

    <v-row no-gutters class="search-row middle-row">
      <!-- 내용 텍스트필드 -->
      <v-col class="search-col content-field">
        <div class="label-box">내 용</div>
        <div class="author-value content-textarea">{{ content }}</div>
      </v-col>
    </v-row>

    <!-- 첨부파일 -->
    <v-row no-gutters class="search-row bottom-row">
      <v-col class="search-col d-flex align-center">
        <div class="label-box">첨부파일</div>

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

    <div class="mt-4 d-flex align-center justify-center">
      <v-btn variant="flat" color="#1E88E5" class="custom-btn mr-2" size="small" @click="moveEdit">
        수정
      </v-btn>
      <v-btn variant="flat" color="#f44336" class="custom-btn mr-2" size="small" @click="moveDelete">
        삭제
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

  <v-dialog v-model="showConfirm" persistent max-width="600" transition="dialog-bottom-transition">
    <v-card class="pa-6 rounded-lg" elevation="10">
      <div class="text-center mb-2">
        <v-icon color="warning" size="50">mdi-alert-circle-outline</v-icon>
      </div>

      <div class="text-h6 font-weight-bold text-center mb-2">
        게시글을 삭제하시겠습니까?
      </div>

      <div class="text-body-2 text-grey text-center mb-6">
        삭제하시면 다시 복구하실 수 없습니다.
      </div>

      <!-- 버튼 -->
      <v-card-actions class="justify-center">
        <v-btn variant="outlined" color="#1E88E5" class="mr-2" @click="showConfirm = false">
          취소
        </v-btn>

        <v-btn variant="flat" color="#1E88E5" class="white--text" @click="confirmDelete">
          삭제
        </v-btn>
      </v-card-actions>

    </v-card>
  </v-dialog>



</template>

<script>
import apiClient from '@/api';
import { inject, onMounted } from 'vue';
import { useAuthStore } from '@/store/auth';

export default {
  props: {
    receivedSeq: {
      type: [Number, String],
      required: false
    },
  },
  setup() {
    const authStore = useAuthStore();

    const extraBreadcrumb = inject('extraBreadcrumb', null);
    const listButtonLink = inject('listButtonLink', null);

    onMounted(() => {
      if (extraBreadcrumb) {
        extraBreadcrumb.value = '상세보기';  // 🔥 추가하고 싶은 값
      }

      if (listButtonLink) {
        listButtonLink.value = '/views/CA/CA2000_10';  // 🔥 현재 페이지에 맞는 "목록" 경로 설정
      }
    });

    // 이 컴포넌트의 다른 메서드에서 사용할 수 있도록 반환
    return {
      authStore
    }
  },
  components: {
  },
  unmounted() { // ❗ 컴포넌트가 언마운트될 때
    const listButtonLink = inject('listButtonLink', null);
    if (listButtonLink) {
      listButtonLink.value = null; // 🔥 페이지 벗어날 때 목록버튼 없애기
    }

    const extraBreadcrumb = inject('extraBreadcrumb', null);
    if (extraBreadcrumb) {
      extraBreadcrumb.value = null; // 🔥 페이지 벗어날 때 목록버튼 없애기
    }
  },
  data() {
    return {
      loading: false,
      errorMessages: [],
      fetchedFiles: [],
      showError: false,
      title: '',
      content: '',
      vuewCount: '',
      showConfirm: false,
    }
  },
  computed: {
  },
  watch: {
    receivedSeq: {
      immediate: true  // 컴포넌트 생성 시점에도 즉시 실행
    },
    selectedStatus(newVal, oldVal) {
      // console.log(`📌 상태 변경: ${oldVal} → ${newVal}`);
      this.oldStatus = oldVal;
    }
  },
  mounted() {
    this.checkLocalStorage();
    this.getUserInfo();
    this.getDetailLibrary();

  },
  created() {
    // localStorage에서 사용자 정보 불러오기
    this.getUserInfo();

  },
  methods: {
    async getDetailLibrary() {
      const response = await apiClient.get("/api/library/detail", {
        params: { seq: this.receivedSeq }
      });

      this.title = response.data?.title || "";
      this.content = response.data?.content || "";

      //첨부파일 리스트 불러오기
      try {
        const fileList = await apiClient.get("/api/file-attach/fileList", {
          params: { seq: this.receivedSeq, boardType: 'CA2000_10' }
        });

        this.fetchedFiles = Array.isArray(fileList.data)
          ? fileList.data.filter(file => file && file.fileName)
          : [];

      } catch (error) {
        console.error("❌ 오류 발생:", error);
      }
    },

    moveDelete() {
      this.showConfirm = true; // 다이얼로그 표시
    },
    async confirmDelete() {
      try {
        // 실제 삭제 로직
        await apiClient.post('/api/library/delete', { seq: this.receivedSeq });
        this.showConfirm = false;
        this.$router.push({ name: 'CA2000_10' });
      } catch (err) {
        console.error("삭제 오류:", err);
        alert("삭제에 실패했습니다.");
      }

      this.showConfirm = false;
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
      this.userPhone = JSON.parse(localStorage.getItem("userInfo"))?.phone || null;
      this.userDeptCd = JSON.parse(localStorage.getItem("userInfo"))?.deptCd || null;
    },

    goBack() {
      this.$router.go(-1);
    },

    moveEdit() {
      this.$router.push({
        name: 'CA_LibraryEditForm',
        params: { receivedSeq: this.receivedSeq }
      })

    },

    async downloadFile(file) {
      try {
        // console.log(file);

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
    }
  }
}
</script>

<style scoped>
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
  min-height: 50px;
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

.label-box {
  width: 100px;
  flex-shrink: 0;
  height: 100%;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 14px;
  font-weight: 500;
  background-color: #e6eef8 !important;
  color: #333333 !important;
  white-space: nowrap;
  padding: 0 4px;
  border-right: 1px solid #eaeaea;
  /* margin-bottom: 5px; */
}

.white-text {
  color: white !important;
}

.custom-btn {
  height: 45px;
  min-width: 70px;
  font-size: 15px;
  border-radius: 6px;
  margin-bottom: 15px;
  border-width: 1px;
}

.goBack-btn {
  height: 35px;
  min-width: 55px;
  font-size: 14px;
  border-radius: 6px;
  margin-bottom: 10px;
}


.fileBox {
  border: 1px solid #B0CAE6;
  border-radius: 6px;
  background-color: rgba(231, 239, 248, 0.6);
  cursor: pointer;
  user-select: none;
}
</style>