<template>
  <v-container fluid class="pr-0 pl-0 pt-4">
    <v-row>
      <v-col>
        <div style="margin-top:-10px;">
          <v-divider thickness="3" color="#578ADB"></v-divider>
        </div>
      </v-col>
    </v-row>
    <br>
    <v-row dense align="center" class="flex-wrap justify-end" style="gap:5px;">
      <!-- 제목 -->
      <v-col cols="12" sm="6" md="auto" class="d-flex align-center filter-col">
        <span class="filter-label">제목<span class="label-divider"></span></span>
        <v-text-field v-model="sub" @keydown.enter="fetchData" variant="outlined" density="compact" hide-details
          class="filter-input-sub" append-inner-icon="mdi-magnify" />
      </v-col>
    </v-row>

    <!-- 데이터 테이블 상단 버튼 영역 -->
    <v-row class="top-button-row mb-2">
      <v-col class="d-flex align-center">
        <span class="mx-3">
          <span class="text-subtitle-2 text-grey">총 </span>
          <span class="text-subtitle-2 font-weight-bold">{{ totalItems }}</span>
          <span class="text-subtitle-2 text-grey">건</span>
        </span>

        <v-spacer></v-spacer>

        <v-btn variant="flat" color="green darken-2" class="custom-btn white-text d-flex align-center" size="small"
          @click="$router.push({ name: 'CA_PostCreateForm' })">

          <v-icon size="default" class="mr-1">mdi-pencil</v-icon>
          게시글 작성
        </v-btn>

      </v-col>
    </v-row>

    <!-- 데이터 테이블 -->
    <v-row class="grid-table ma-0 pa-0">
      <v-col class="pa-0">
        <div class="table-container">
          <!-- 테이블 헤더 -->
          <div class="table-header">
            <div class="th-cell checkbox-cell">
              <v-checkbox hide-details density="compact" v-model="selectAll" @change="toggleSelectAll"></v-checkbox>
            </div>
            <div class="th-cell">접수번호</div>
            <div class="th-cell">요청일</div>
            <div class="th-cell">제목</div>
            <div class="th-cell">소속</div>
            <div class="th-cell">작성자</div>
          </div>

          <!-- 테이블 데이터 행 -->
          <div v-for="(item, index) in paginatedData" :key="index" class="table-row">
            <div class="td-cell checkbox-cell">
              <v-checkbox hide-details density="compact" v-model="item.selected"></v-checkbox>
            </div>
            <div class="td-cell">{{ item.seq }}</div>
            <div class="td-cell">{{ formatDate(item.requestDate) }}</div>
            <div class="td-cell title-cell">
              <router-link :to="{
                name: (item.saveFlag === 'Y' && item.processState === 'S')
                  ? 'CA_PostDetailSrForm'
                  : 'CA_PostDetailForm',
                params: { receivedSeq: item.seq }
              }" class="title-link" style="display: inline-flex; align-items: center;">
                {{ item.sub }}

                <span v-if="item.countComment > 0" style="color: #737577;">&nbsp;[{{ item.countComment }}]</span>

                <span v-if="item.new_yn === 'Y'">&nbsp;</span>
                <v-img v-if="item.new_yn === 'Y'" src="@/assets/new-icon.png" alt="new" width="22" height="22"
                  style="display: inline-block; vertical-align: middle;"></v-img>
              </router-link>
            </div>
            <div class="td-cell" style="text-align: center;">
              {{ item.division }}<br>
              {{ item.dpNm }}
            </div>
            <div class="td-cell">{{ item.uid }}</div>
            <div class="td-cell">{{ item.inquiryType }}</div>
          </div>
        </div>

        <!-- 로딩 표시 -->
        <div v-if="loading" class="loading-overlay">
          <v-progress-circular indeterminate color="primary"></v-progress-circular>
        </div>

        <!-- 데이터 없음 표시 -->
        <div v-if="!loading && tableData.length === 0" class="no-data">
          조회된 데이터가 없습니다.
        </div>

        <!-- 페이지네이션 -->
        <div class="pagination-container" v-if="tableData.length > 0">
          <v-btn icon="mdi-chevron-left" variant="text" size="small" :disabled="currentPage === 1"
            @click="currentPage--"></v-btn>

          <template v-if="totalPages <= 5">
            <v-btn v-for="page in totalPages" :key="page" size="small" :variant="currentPage === page ? 'flat' : 'text'"
              :color="currentPage === page ? 'primary' : ''" @click="currentPage = page">
              {{ page }}
            </v-btn>
          </template>

          <template v-else>
            <!-- 처음 페이지 -->
            <v-btn v-if="currentPage > 3" size="small" variant="text" @click="currentPage = 1">
              1
            </v-btn>

            <!-- 생략 표시 -->
            <span v-if="currentPage > 3" class="mx-1">...</span>

            <!-- 이전 페이지 -->
            <v-btn v-if="currentPage > 1" size="small" variant="text" @click="currentPage = currentPage - 1">
              {{ currentPage - 1 }}
            </v-btn>

            <!-- 현재 페이지 -->
            <v-btn size="small" variant="flat" color="primary">
              {{ currentPage }}
            </v-btn>

            <!-- 다음 페이지 -->
            <v-btn v-if="currentPage < totalPages" size="small" variant="text" @click="currentPage = currentPage + 1">
              {{ currentPage + 1 }}
            </v-btn>

            <!-- 생략 표시 -->
            <span v-if="currentPage < totalPages - 2" class="mx-1">...</span>

            <!-- 마지막 페이지 -->
            <v-btn v-if="currentPage < totalPages - 2" size="small" variant="text" @click="currentPage = totalPages">
              {{ totalPages }}
            </v-btn>
          </template>

          <v-btn icon="mdi-chevron-right" variant="text" size="small" :disabled="currentPage === totalPages"
            @click="currentPage++"></v-btn>
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
import { inject, onMounted } from 'vue';
import '@vuepic/vue-datepicker/dist/main.css';

export default {
  components: {

  },
  setup() {
    const extraBreadcrumb = inject('extraBreadcrumb', null);
    const listButtonLink = inject('listButtonLink', null);
    onMounted(() => {
      if (extraBreadcrumb) {
        extraBreadcrumb.value = null;
      }

      if (listButtonLink) {
        listButtonLink.value = null;
      }
    });

    return {};
  },
  unmounted() { // ❗ 컴포넌트가 언마운트될 때
    const listButtonLink = inject('listButtonLink', null);
    if (listButtonLink) {
      listButtonLink.value = null; // 🔥 페이지 벗어날 때 목록버튼 없애기
    }
  },
  data() {
    return {
      sub: '',
      countComment: 0,
      new_yn: 'n',
      dateRange: 'month',
      productType: 'test1',
      tableData: [],
      loading: false,
      selectAll: false,
      // 페이징 관련 변수
      currentPage: 1,
      itemsPerPage: 10,
      processState: '',
      errorMessages: [],
      showError: false,
      savedMidMenu: '',
      savedSubMenu: '',
    }
  },

  computed: {
    // 전체 페이지 수 계산
    totalPages() {
      return Math.ceil(this.tableData.length / this.itemsPerPage);
    },

    // 현재 페이지에 표시할 데이터
    paginatedData() {
      const start = (this.currentPage - 1) * this.itemsPerPage;
      const end = start + this.itemsPerPage;
      return this.tableData.slice(start, end);
    },

    // 전체 아이템 수
    totalItems() {
      return this.tableData.length;
    }
  },

  watch: {
    // API 파라미터가 변경되면 데이터 다시 로드
    startDate() {
      this.currentPage = 1; // 검색 조건 변경 시 첫 페이지로 리셋      
    },
    endDate() {
      this.currentPage = 1;
    },
  },

  mounted() {
    this.fetchData();
    this.checkLocalStorage();
  },

  methods: {
    checkLocalStorage() {
      const midMenuFromStorage = localStorage.getItem('midMenu');
      const subMenuFromStorage = localStorage.getItem('subMenu');

      this.savedMidMenu = midMenuFromStorage ? JSON.parse(midMenuFromStorage) : null;
      this.savedSubMenu = subMenuFromStorage ? JSON.parse(subMenuFromStorage) : null;
    },

    // API 호출하여 데이터 가져오기
    async fetchData() {
      this.loading = true;

      try {
        // 서버 측 페이징을 구현할 경우 페이지 관련 파라미터 추가
        const response = await apiClient.get('/api/require/search', {
          params: {
            manager: this.manager,
            sub: this.sub,
            status: this.selectedStatus,
            dpId: JSON.parse(localStorage.getItem("userInfo"))?.deptCd || null
          }
        });

        // API 응답 데이터 처리
        if (response.data && Array.isArray(response.data)) {


          this.tableData = response.data.map(item => {
            const requestDateTime = new Date(item.requestDateTime);
            const now = new Date();
            const diffTime = now - requestDateTime;
            const diffHours = diffTime / (1000 * 60 * 60);


            return {
              ...item,
              selected: false,
              // API에서 진행상태가 오지 않으면 임의로 설정
              status: item.processState === 'S'
                ? (item.statusNm + ' (' +
                  (item.srFlag === 'Y'
                    ? '상신완료'
                    : item.srFlag === 'F'
                      ? '반려'
                      : '상신 전'
                  ) + ')' || this.getRandomStatus())
                : (item.statusNm || this.getRandomStatus()),


              // 24시간 이내 여부에 따라 new_yn 설정
              new_yn: diffHours < 24 ? 'Y' : 'N',

              // 테이블에 표시할 데이터 매핑
              manager: item.manager || '-',  // 담당자 필드가 없어서 임시로 요청자 ID 사용
              memo: item.currentIssue || '-', // 메모 필드가 없어서 임시로 현재 이슈 사용              
            };
          });

          // 서버 측 페이징 구현시 전체 개수 설정 (API 응답에서 받아야 함)
          // this.totalItems = response.data.totalItems;
        } else {
          this.tableData = [];
        }



      } catch (error) {
        console.error('데이터 로드 중 오류 발생:', error);
      } finally {
        this.loading = false;
      }
    },
    // 날짜 포맷 함수 (ISO 문자열 -> YYYY-MM-DD 형식)
    formatDate(dateString) {
      if (!dateString) return '-';
      const date = new Date(dateString);
      if (isNaN(date.getTime())) return '-';

      const year = date.getFullYear();
      const month = String(date.getMonth() + 1).padStart(2, '0');
      const day = String(date.getDate()).padStart(2, '0');
      return `${year}-${month}-${day}`;
    },

    // 전체 선택/해제 토글
    toggleSelectAll() {
      // 현재 페이지의 항목만 선택/해제
      this.paginatedData.forEach(item => {
        item.selected = this.selectAll;
      });
    },
  }
}</script>

<style scoped>
:deep(.dp__input) {
  border: none;
  box-shadow: none;
  color: #7a7a7a;
}

:deep(.dp__main) {
  font-family: inherit;
  border-radius: 8px;
  z-index: 100;
}

:deep(.dp__theme_light) {
  --dp-primary-color: #2196F3;
  --dp-border-radius: 8px;
}

:deep(.dp__overlay_cell_active) {
  background-color: var(--dp-primary-color);
  color: white;
}

.breadcrumb-div {
  font-size: 12px;
  color: #A1A6A6;
}

.title-search {
  padding-block: 10px;
  padding-left: 10px;
  width: 800px;
  font-weight: 400;
}

.custom-btn {
  font-size: 14px;
  height: 40px;
  border-radius: 10px;
}

.date-btn {
  min-width: 48px;
  padding: 0 12px;
  height: 32px;
  letter-spacing: -0.5px;
  border: 1px solid #eaeaea;
  border-radius: 0;
  background-color: #ffffff;
  color: #7A7A7A;
  box-shadow: none;
  margin: 0;
}

.v-col.pa-0 {
  height: 100%;
}

.top-button-row {
  margin-bottom: 8px;
}

.white-text {
  color: white !important;
}

.table-container {
  border: 1px solid #e0e0e0;
  width: 100%;
  position: relative;
  border-radius: 10px;
  overflow: hidden;
}

.table-header,
.table-row {
  display: grid;
  grid-template-columns: 60px 80px 100px 1fr 100px 100px 100px 120px 100px 90px 100px;
}

.table-header {
  background-color: #D0DFF1;
  font-weight: 500;
  border-bottom: 1px solid #e0e0e0;
  color: #3E4B5B !important;
}

.table-row {
  border-bottom: 1px solid #e0e0e0;
  height: 54px;
  color: #5B5D60;
  font-size: 15px;
}

.table-row:hover {
  background-color: #f9f9f9;
}

.th-cell,
.td-cell {
  padding: 8px 12px;
  border-right: none;
  display: flex;
  align-items: center;
  font-size: 14px;
}

.th-cell {
  justify-content: center;
  font-weight: 500;
  white-space: nowrap;
  font-size: 14px;
}

.checkbox-cell {
  flex: 0 0 40px;
  justify-content: center;
}

.title-cell {
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
  padding-left: 12px;
}

.title-link {
  color: #1976d2;
  text-decoration: none;
}

.title-link:hover {
  text-decoration: underline;
}

.pagination-container {
  display: flex;
  justify-content: center;
  align-items: center;
  margin-top: 10px;
}

.loading-overlay {
  position: absolute;
  top: 0;
  left: 0;
  width: 100%;
  height: 100%;
  background-color: rgba(255, 255, 255, 0.7);
  display: flex;
  justify-content: center;
  align-items: center;
  z-index: 5;
}

.no-data {
  padding: 30px;
  text-align: center;
  color: #757575;
  font-size: 14px;
}


.filter-label {
  font-size: 14.5px;
  min-width: 45px;
  font-weight: 500;
  color: #005bac;
  margin-left: 10px;
  margin-right: 0px;
}


.filter-label::after {
  content: "";
  height: 16px;
  width: 1px;
  background: #ddd;
  margin-top: 13px;
  margin-left: 11px;
}

.filter-input-sub {
  width: 220px;
  margin-right: 6px;
  color: #5271C1;
}

.date-btn {
  font-size: 12px;
  height: 32px;
  min-width: 56px;
}

.search-btn {
  color: white;
  font-weight: 500;
  height: 36px;
  min-width: 64px;
}

.v-btn.date-btn {
  margin-top: 2px;
  /* 버튼 살짝 내려서 정렬 */
  padding: 0 8px;
  font-size: 13px;
}

.v-btn.search-btn {
  margin-top: 2px;
  /* 검색 버튼도 아래 요소와 정렬 */
}

.filter-col {
  height: 50px;
  border: 1.5px solid #D0DFF1;
  border-radius: 8px;
  background-color: white;
}

.rounded-border {
  display: flex;
  flex-wrap: wrap;
  gap: 12px;
  padding: 12px;
  border: 1px solid #D0DFF1;
  border-radius: 8px;
  background-color: rgba(208, 223, 241, 0.5);
  height: auto;
  max-width: 450px;
}

.label-divider {
  display: inline-block;
  height: 18px;
  background-color: #bbb;
  margin-left: 10px;
  margin-bottom: 2px;
  border-radius: 1px;
  vertical-align: middle;
  width: 2px;
  background-color: #B0CAE6;
}
</style>