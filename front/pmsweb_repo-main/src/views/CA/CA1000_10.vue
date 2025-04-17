<template>
  <v-container fluid class="pr-5 pl-5 pt-7">

    <v-row>
      <v-col>
        <div class="title-div">작업요청</div>
        <div class="mt-2">
          <v-divider thickness="3" color="#578ADB"></v-divider>
        </div>
      </v-col>
    </v-row>

    <br>


    <v-row dense align="center" class="flex-wrap rounded-border sky-bg" style="gap: 12px;">

      <!-- 요청기간 -->
      <v-col cols="auto" class="d-flex align-center filter-col ml-2">
        <span class="filter-label">요청기간<span class="label-divider"></span></span>
        <div class="date-wrapper">
          <!-- 시작일 입력 필드 -->
          <v-menu v-model="startDateMenu" :close-on-content-click="false" transition="scale-transition" offset-y
            min-width="auto">
            <template v-slot:activator="{ props }">
              <div class="date-field-wrapper" v-bind="props">
                <v-text-field v-model="startDate" class="date-input" density="compact" hide-details readonly
                  variant="plain"></v-text-field>
                <div class="calendar-icon-container">
                  <v-btn icon class="calendar-btn">
                    <v-icon size="small" color="#7A7A7A">mdi-calendar-search</v-icon>
                  </v-btn>
                </div>
              </div>
            </template>
            <v-date-picker v-model="Date_startDate" @update:model-value="startDateMenu = false" locale="ko-KR"
              elevation="1" color="blue" width="290" first-day-of-week="1" show-adjacent-months scrollable
              :allowed-dates="allowedDates"></v-date-picker>
          </v-menu>
        </div>
        <span class="date-separator">~</span>

        <div class="date-wrapper">
          <!-- 종료일 입력 필드 -->
          <v-menu v-model="endDateMenu" :close-on-content-click="false" transition="scale-transition" offset-y
            min-width="auto">
            <template v-slot:activator="{ props }">
              <div class="date-field-wrapper" v-bind="props">
                <v-text-field v-model="endDate" class="date-input" density="compact" hide-details readonly
                  variant="plain"></v-text-field>
                <div class="calendar-icon-container">
                  <v-btn icon class="calendar-btn">
                    <v-icon size="small" color="#7A7A7A">mdi-calendar-search</v-icon>
                  </v-btn>
                </div>
              </div>
            </template>
            <v-date-picker v-model="Date_endDate" @update:model-value="endDateMenu = false" locale="ko-KR" elevation="1"
              color="blue" width="290" first-day-of-week="1" show-adjacent-months scrollable
              :allowed-dates="allowedDates"></v-date-picker>
          </v-menu>
        </div>


        <!-- 날짜 버튼 -->
        <div class="date-buttons mr-2">
          <div class="date-btn-container">
            <v-btn value="today" class="date-btn" :class="{ 'active-date-btn': dateRange === 'today' }"
              @click="setDateRange('today')">오늘</v-btn>
            <v-btn value="week" class="date-btn" :class="{ 'active-date-btn': dateRange === 'week' }"
              @click="setDateRange('week')">1주일</v-btn>
            <v-btn value="15days" class="date-btn" :class="{ 'active-date-btn': dateRange === '15days' }"
              @click="setDateRange('15days')">15일</v-btn>
            <v-btn value="month" class="date-btn" :class="{ 'active-date-btn': dateRange === 'month' }"
              @click="setDateRange('month')">1개월</v-btn>
            <v-btn value="3months" class="date-btn" :class="{ 'active-date-btn': dateRange === '3months' }"
              @click="setDateRange('3months')">3개월</v-btn>
          </div>
        </div>
      </v-col>

      <!-- 접수상태 -->
      <v-col cols="auto" class="d-flex align-center filter-col">
        <span class="filter-label">접수상태<span class="label-divider"></span></span>
        <v-select v-model="selectedStatus" :items="progressStatuses" item-title="text" item-value="value"
          variant="outlined" density="compact" hide-details class="filter-input" />
      </v-col>

      <!-- 담당자 -->
      <v-col cols="auto" class="d-flex align-center filter-col">
        <span class="filter-label">담당자<span class="label-divider"></span></span>
        <v-text-field v-model="manager" variant="outlined" density="compact" hide-details class="filter-input" />
      </v-col>

      <!-- 제목 -->
      <v-col cols="auto" class="d-flex align-center filter-col">
        <span class="filter-label">제목<span class="label-divider"></span></span>
        <v-text-field v-model="sub" variant="outlined" density="compact" hide-details class="filter-input-sub" />
      </v-col>

      <!-- 검색 버튼 -->
      <v-col cols="auto">
        <v-btn variant="flat" color="primary" class="custom-btn mr-2 d-flex align-center" size="small"
          @click="fetchData()">
          <v-icon size="default" class="mr-1">mdi-magnify</v-icon>
          조회
        </v-btn>
      </v-col>

    </v-row>

    <br>
    <br>

    <!-- 데이터 테이블 상단 버튼 영역 -->
    <v-row class="top-button-row mb-2">
      <v-col class="d-flex align-center">
        <!-- <v-btn variant="flat" color="primary" class="custom-btn mr-2 white-text d-flex align-center" size="small">
          <v-icon size="default" class="mr-1">mdi-account</v-icon>
          test
        </v-btn> -->
        <span class="mx-3">
          <span class="text-subtitle-2 text-grey">총 </span>
          <span class="text-subtitle-2 font-weight-bold">{{ totalItems }}</span>
          <!-- <span class="text-subtitle-2 text-grey">건</span> -->
          <span class="text-subtitle-2 text-grey"> / 미처리: </span>
          <span class="text-subtitle-2 font-weight-bold text-red"><span :class="getStatusClass('P')">{{ getUnprocessedCount('P') }}</span></span>
          <span class="text-subtitle-2 text-grey ml-2"> 진행: </span>
          <span class="text-subtitle-2 font-weight-bold text-blue"><span :class="getStatusClass('I')">{{ getUnprocessedCount('I') }}</span></span>
          <span class="text-subtitle-2 text-grey ml-2"> 보류: </span>
          <span class="text-subtitle-2 font-weight-bold text-blue"><span :class="getStatusClass('H')">{{ getUnprocessedCount('H') }}</span></span>
          <span class="text-subtitle-2 text-grey ml-2"> SR: </span>
          <span class="text-subtitle-2 font-weight-bold text-blue"><span :class="getStatusClass('S')">{{ getUnprocessedCount('S') }}</span></span>
          <span class="text-subtitle-2 text-grey ml-2"> 종결: </span>
          <span class="text-subtitle-2 font-weight-bold"><span :class="getStatusClass('C')">{{ getUnprocessedCount('C') }}</span></span>

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
            <div class="th-cell">사업부문</div>
            <div class="th-cell">진행상태</div>
            <div class="th-cell">완료일</div>
            <div class="th-cell">담당자</div>
            <div class="th-cell">소요시간</div>
            <!-- <div class="th-cell">메모</div> -->
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
                  ? 'CA_PostDetailSrForm' : 'CA_PostDetailForm', params: { receivedSeq: item.seq }
              }"
                class="title-link">{{ item.sub }}</router-link>
            </div>
            <div class="td-cell">{{ item.division }}</div>
            <div class="td-cell" :class="getStatusClass(item.processState)">{{ item.status }}</div>
            <div class="td-cell">{{ formatDate(item.completeDate) }}</div>
            <div class="td-cell">{{ item.manager || '-' }}</div>
            <div class="td-cell">{{ calculateDuration(item.requestDate, item.completeDate) }}</div>
            <!-- <div class="td-cell">{{ item.memo || '-' }}</div> -->
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

export default {
  data() {
    return {
      Date_startDate: new Date(),
      Date_endDate: new Date(),
      startDate: '',
      endDate: '',
      startDateMenu: false,  // 시작일 날짜 선택기 메뉴 표시 여부
      endDateMenu: false,    // 종료일 날짜 선택기 메뉴 표시 여부
      selectedStatus: '',
      progressStatuses: [],
      manager: '',
      sub: '',
      dateRange: 'month',
      productType: 'test1',
      tableData: [],
      loading: false,
      selectAll: false,
      // 페이징 관련 변수
      currentPage: 1,
      itemsPerPage: 10,
      // 상태값 목록 (실제 API에서 받아올 수 있음)
      //statusList: ['미처리', '진행중', '보류중', '종결'],
      processState: '',
      errorMessages: [],
      showError: false,
      savedMidMenu: '',
      savedSubMenu: '',
      countStatus: []
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
    Date_startDate(newValue) {
      if (newValue) {
        // Date 객체를 'YYYY-MM-DD' 형식의 문자열로 변환
        const date = new Date(newValue);
        const year = date.getFullYear();
        const month = String(date.getMonth() + 1).padStart(2, '0');
        const day = String(date.getDate()).padStart(2, '0');

        // 형식화된 문자열을 startDate에 할당
        this.startDate = `${year}-${month}-${day}`;
      } else {
        this.startDate = '';
      }
    },
    Date_endDate(newValue) {
      if (newValue) {
        // Date 객체를 'YYYY-MM-DD' 형식의 문자열로 변환
        const date = new Date(newValue);
        const year = date.getFullYear();
        const month = String(date.getMonth() + 1).padStart(2, '0');
        const day = String(date.getDate()).padStart(2, '0');

        // 형식화된 문자열을 startDate에 할당
        this.endDate = `${year}-${month}-${day}`;
      } else {
        this.endDate = '';
      }
    },
    selectedStatus(newVal, oldVal) {
      console.log(`📌 상태 변경: ${oldVal} → ${newVal}`);
    }
  },

  mounted() {
    this.setDateRange('month');
    this.getStatus();
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

    isValidDate(options = {}) {
      const errors = [];

      // 기본 옵션 설정
      const {
        maxDays = null,
        allowFutureDates = true,
        allowPastDates = true,
        minDate = null,
        maxDate = null,
      } = options;

      // 1. 기본 입력 검사
      if (!this.startDate || !this.endDate) {
        errors.push('시작일과 종료일을 모두 입력해주세요.');
        return { isValid: false, errors };
      }

      // 2. 날짜 형식 검사 (YYYY-MM-DD)
      const dateFormatRegex = /^\d{4}-\d{2}-\d{2}$/;
      if (!dateFormatRegex.test(this.startDate)) {
        errors.push('시작일의 형식이 올바르지 않습니다. (YYYY-MM-DD)');
      }
      if (!dateFormatRegex.test(this.endDate)) {
        errors.push('종료일의 형식이 올바르지 않습니다. (YYYY-MM-DD)');
      }

      // 형식이 올바르지 않으면 여기서 중단
      if (errors.length > 0) {
        return { isValid: false, errors };
      }

      // 3. 유효한 날짜인지 검사
      const isValidDateObj = (dateStr) => {
        const [year, month, day] = dateStr.split('-').map(Number);
        const date = new Date(year, month - 1, day);
        return (
          date.getFullYear() === year &&
          date.getMonth() === month - 1 &&
          date.getDate() === day
        );
      };

      if (!isValidDateObj(this.startDate)) {
        errors.push('시작일이 유효한 날짜가 아닙니다.');
      }
      if (!isValidDateObj(this.endDate)) {
        errors.push('종료일이 유효한 날짜가 아닙니다.');
      }

      // 유효한 날짜가 아니면 여기서 중단
      if (errors.length > 0) {
        return { isValid: false, errors };
      }

      // 날짜 객체 생성
      const startDate = new Date(this.startDate);
      const endDate = new Date(this.endDate);
      startDate.setHours(0, 0, 0, 0);
      endDate.setHours(23, 59, 59, 999);

      // 4. 날짜 범위 검사 (시작일 <= 종료일)
      if (startDate > endDate) {
        errors.push('시작일이 종료일보다 나중일 수 없습니다.');
      }

      // 5. 최대 기간 검사
      if (maxDays) {
        const diffTime = Math.abs(endDate - startDate);
        const diffDays = Math.ceil(diffTime / (1000 * 60 * 60 * 24));

        if (diffDays > maxDays) {
          errors.push(`조회 기간은 최대 ${maxDays}일까지 가능합니다.`);
        }
      }

      // 7. 현재 날짜와 비교
      const today = new Date();
      today.setHours(0, 0, 0, 0); // 시간 부분 제거

      // 미래 날짜 검사
      if (!allowFutureDates && startDate > today) {
        errors.push('시작일은 오늘 이후일 수 없습니다.');
      }

      // 과거 날짜 검사
      if (!allowPastDates && endDate < today) {
        errors.push('종료일은 오늘 이전일 수 없습니다.');
      }

      // 8. 허용된 날짜 범위 검사
      if (minDate) {
        const minDateObj = new Date(minDate);
        if (startDate < minDateObj) {
          errors.push(`시작일은 ${minDate} 이후여야 합니다.`);
        }
      }

      if (maxDate) {
        const maxDateObj = new Date(maxDate);
        if (endDate > maxDateObj) {
          errors.push(`종료일은 ${maxDate} 이전이어야 합니다.`);
        }
      }

      // 최종 결과 반환
      return {
        isValid: errors.length === 0,
        errors
      };
    },

    // 날짜 범위 설정 함수
    setDateRange(range) {
      this.dateRange = range;
      const today = new Date();
      let start = new Date(today);

      switch (range) {
        case 'today':
          // 오늘 날짜로 시작일과 종료일 모두 설정
          break;
        case 'week':
          // 1주일 전
          start.setDate(today.getDate() - 7);
          break;
        case '15days':
          // 15일 전
          start.setDate(today.getDate() - 15);
          break;
        case 'month':
          // 1개월 전
          start.setMonth(today.getMonth() - 1);
          break;
        case '3months':
          // 3개월 전
          start.setMonth(today.getMonth() - 3);
          break;
      }

      this.startDate = this.formatDateForInput(start);
      this.endDate = this.formatDateForInput(today);

    },

    // API 호출하여 데이터 가져오기
    async fetchData() {

      this.loading = true;
      try {
        // 서버 측 페이징을 구현할 경우 페이지 관련 파라미터 추가
        const response = await apiClient.get('/api/require/search', {
          params: {
            startDate: this.startDate + ' 00:00:00',
            endDate: this.endDate + ' 23:59:59',
            manager: this.manager,
            sub: this.sub,
            status: this.selectedStatus
          }
        });

        // API 응답 데이터 처리
        if (response.data && Array.isArray(response.data)) {
          this.tableData = response.data.map(item => ({
            ...item,
            selected: false,
            // API에서 진행상태가 오지 않으면 임의로 설정
            status: item.processState === 'S'
              ? (item.statusNm + ' (' + item.srFlag + ')' || this.getRandomStatus())
              : (item.statusNm || this.getRandomStatus()),

            // 테이블에 표시할 데이터 매핑
            manager: item.manager || '-',  // 담당자 필드가 없어서 임시로 요청자 ID 사용
            memo: item.currentIssue || '-'     // 메모 필드가 없어서 임시로 현재 이슈 사용          
          }));

          // 서버 측 페이징 구현시 전체 개수 설정 (API 응답에서 받아야 함)
          // this.totalItems = response.data.totalItems;
        } else {
          this.tableData = [];
        }
      } catch (error) {
        console.error('데이터 로드 중 오류 발생:', error);
        // 오류 발생 시 테스트 데이터 로드 (개발용)
        //   this.loadTestData();
      } finally {
        this.loading = false;
      }
    },

    // 테스트 데이터 로드 (API 연결 전이나 오류 시 사용)
    loadTestData() {
      const today = new Date();
      const yesterday = new Date(today);
      yesterday.setDate(today.getDate() - 1);
      const lastWeek = new Date(today);
      lastWeek.setDate(today.getDate() - 7);

      // 20개의 테스트 데이터 생성
      this.tableData = Array.from({ length: 20 }, (_, i) => {
        const insertDate = new Date(today);
        insertDate.setDate(today.getDate() - (i * 3));

        const completeDate = i % 4 === 0 ? null : new Date(insertDate);
        if (completeDate) {
          completeDate.setDate(insertDate.getDate() + (i % 10) + 1);
        }

        const statusIndex = i % 4;

        return {
          seq: `REQ${(1000 + i).toString().padStart(3, '0')}`,
          selected: false,
          insertDt: insertDate.toISOString(),
          projectName: `프로젝트 요청 ${i + 1}`,
          businessSector: ['IT부문', '영업부문', '개발부문', '마케팅부문', '생산부문'][i % 5],
          status: this.statusList[statusIndex],
          completeDt: completeDate?.toISOString() || null,
          manager: [`김담당`, `이매니저`, `박책임`, `최팀장`, `정대리`][i % 5],
          memo: statusIndex === 0 ? '긴급 처리 필요' : statusIndex === 1 ? '일정 조정 중' : statusIndex === 2 ? '진행 보류 요청' : '정상 처리 완료'
        };
      });
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

    // 입력용 날짜 포맷 함수
    formatDateForInput(date) {
      const year = date.getFullYear();
      const month = String(date.getMonth() + 1).padStart(2, '0');
      const day = String(date.getDate()).padStart(2, '0');
      return `${year}-${month}-${day}`;
    },

    // 소요시간 계산 함수
    calculateDuration(startDate, endDate) {
      if (!startDate || !endDate) return '-';

      const start = new Date(startDate);
      const end = new Date(endDate);
      if (isNaN(start.getTime()) || isNaN(end.getTime())) return '-';

      const diffTime = Math.abs(end - start);
      const diffDays = Math.floor(diffTime / (1000 * 60 * 60 * 24));
      const diffHours = Math.floor((diffTime % (1000 * 60 * 60 * 24)) / (1000 * 60 * 60));

      return `${diffDays}일 ${diffHours}시간`;
    },

    // 건수 계산
    getUnprocessedCount(statusCnt) {
      this.countStatus[statusCnt] = this.tableData.filter(item => item.processState === statusCnt).length;
      return this.countStatus[statusCnt];
    },

    // 전체 선택/해제 토글
    toggleSelectAll() {
      // 현재 페이지의 항목만 선택/해제
      this.paginatedData.forEach(item => {
        item.selected = this.selectAll;
      });
    },

    // 랜덤 상태값 반환 (API에서 상태값이 없을 경우 사용)
    getRandomStatus() {
      return this.statusList[Math.floor(Math.random() * this.statusList.length)];
    },

    // 상태에 따른 클래스 반환
    getStatusClass(status) {
      switch (status) {
        case 'P':
          return 'text-error';
        case 'I':
          return 'text-info';
        case 'H':
          return 'text-warning';
        case 'S':
          return 'text-info';
        case 'C':
          return 'text-success';
        default:
          return '';
      }
    },
    async getStatus() {
      try {
        const statusList = await apiClient.get("/api/status/list");

        // 상태 이름 리스트 저장
        this.progressStatuses = statusList.data.map(status => ({
          text: status.codeName,
          value: status.codeId
        }));

        this.progressStatuses.unshift({ text: '전체', value: '%' });

        this.selectedStatus = '%';

      } catch (error) {
        console.error("❌ 오류 발생:", error);
      }
    },
  }
}</script>

<style scoped>
.breadcrumb-div {
  font-size: 12px;
  color: #A1A6A6;
}

.title-div {
  font-size: 25px;
}

.manager-search {
  padding-block: 10px;
  padding-left: 10px;
  width: 800px;
  font-weight: 400;
}

.title-search {
  padding-block: 10px;
  padding-left: 10px;
  width: 800px;
  font-weight: 400;
}

.select-btn {
  color: white;
  background-color: #23BBF5 !important;
}

.custom-btn {
  font-size: 14px;
  height: 40px;
  border-radius: 10px;
}

/* 날짜 선택 관련 스타일 */
.date-field-wrapper {
  display: flex;
  align-items: center;
  cursor: pointer;
  width: 100%;

}

.date-wrapper {
  position: relative;
  display: flex;
  align-items: center;
  width: 125px;
  color: #737577;
}

.date-input {
  width: 120px;
  align-items: center;
  /* 수직 가운데 정렬 */
}

.date-input :deep(.v-field__input) {
  font-size: 15px;
  padding-top: 0;
  padding-bottom: 0;
}

.calendar-icon-container {
  display: flex;
  align-items: center;
  margin-left: 12px;
}

.calendar-btn {
  width: 28px;
  height: 28px;
  min-width: 28px;
  z-index: 1;
}

.date-separator {
  margin: 0 10px;
  font-size: 16px;
  color: #7A7A7A;
}

.date-buttons {
  margin-left: 20px;
}

.date-btn-container {
  display: flex;
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

.date-btn:not(:first-child) {
  border-left: none;
}

.date-btn:hover {
  background-color: #f9f9f9;
}

.date-btn.active {
  background-color: #e8f4fd;
  color: #2196F3;
}

.active-date-btn {
  background-color: #e8f4fd !important;
  color: #2196F3 !important;
  border-color: #2196F3 !important;
  font-weight: 500;
  border-left: 1px solid #2196F3 !important;
}

.search-row {
  display: flex;
  flex-direction: column;
  /* 세로 정렬 */
  border: 1px solid #ccc;
  border-radius: 8px;
  background-color: white;
  padding: 12px;
  /* 여백 주기 */
  gap: 12px;
  /* 두 줄 사이 간격 */
}

.search-col {
  display: flex;
  align-items: center;
  padding: 0;
  border-left: 1px solid #e0e0e0;
}

.request-period,
.product-category,
.title-category {
  max-width: 550px;
  flex-grow: 0;
}

.label-box {
  height: 40px;
  min-width: 80px;
  display: flex;
  align-items: center;
  justify-content: center;
  background-color: #e3ecf8;
  font-weight: 500;
  padding: 0 12px;
  white-space: nowrap;
  border-right: 1px solid #ccc;
}

.v-col.pa-0 {
  height: 100%;
  /* v-col도 확실하게 높이 채우기 */
}

.input-container {
  display: flex;
  align-items: center;
  flex: 1;
  padding: 0 16px;
}

/* 상단 버튼 행 스타일 */
.top-button-row {
  margin-bottom: 8px;
}

.white-text {
  color: white !important;
}

/* 테이블 스타일 */
.table-container {
  border: 1px solid #e0e0e0;
  width: 100%;
  position: relative;
  border-radius: 10px;
  /* 모서리 라운드 처리 */
  overflow: hidden;
  /* 내부 요소가 라운드 처리된 모서리를 벗어나지 않도록 함 */
}

/* 1페이지의 1행만 열 간격이 틀어지는 현상이 있어서 강제로 사이즈를 지정함 */
.table-header,
.table-row {
  display: grid;
  grid-template-columns: 60px 80px 100px 1fr 100px 90px 100px 90px 100px;
}

.table-header {
  background-color: #D0DFF1;
  font-weight: 500;
  border-bottom: 1px solid #e0e0e0;
}

.table-row {
  border-bottom: 1px solid #e0e0e0;
  height: 54px;  
  color: #5B5D60;
}

.table-row:hover {
  background-color: #f9f9f9;
}

.th-cell,
.td-cell {
  padding: 8px 12px;
  border-right: 1px solid #e0e0e0;
  display: flex;
  align-items: center;
  font-size: 13px;
}

.th-cell {
  justify-content: center;
  font-weight: 500;
  white-space: nowrap;
}

.th-cell,
.td-cell {
  padding: 8px 12px;
  border-right: none;
  /* 오른쪽 테두리 제거 */
  display: flex;
  align-items: center;
  font-size: 13px;
}

.checkbox-cell {
  flex: 0 0 60px;
  justify-content: center;
}

.th-cell:nth-child(2),
.td-cell:nth-child(2) {
  flex: 0 0 80px;
  justify-content: center;
}

/* 접수번호 */
.th-cell:nth-child(3),
.td-cell:nth-child(3) {
  flex: 0 0 100px;
  justify-content: center;
}

/* 요청일 */
.th-cell:nth-child(4),
.td-cell:nth-child(4) {
  flex: 1;
}

/* 제목 */
.th-cell:nth-child(5),
.td-cell:nth-child(5) {
  flex: 0 0 100px;
  justify-content: center;
}

/* 사업부문 */
.th-cell:nth-child(6),
.td-cell:nth-child(6) {
  flex: 0 0 90px;
  justify-content: center;
}

/* 진행상태 */
.th-cell:nth-child(7),
.td-cell:nth-child(7) {
  flex: 0 0 100px;
  justify-content: center;
}

.th-cell:nth-child(8),
.td-cell:nth-child(8) {
  flex: 0 0 90px;
  justify-content: center;
}

/* 담당자 */
.th-cell:nth-child(9),
.td-cell:nth-child(9) {
  flex: 0 0 100px;
  justify-content: center;
}

/* 소요시간 */
/* .th-cell:nth-child(10), .td-cell:nth-child(10) { flex: 0 0 180px; }  */

.header-with-divider {
  display: flex;
  align-items: center;
  justify-content: center;
  width: 100%;
}

.header-divider {
  height: 16px !important;
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

.file-indicator {
  color: #2196F3;
  margin-left: 4px;
}

.text-error {
  color: #E6373A;
}

.text-info {
  color: #2196F3;
}

.text-warning {
  color: #FB8C00;
}

.text-success {
  color: #4CAF50;
}

.pagination-container {
  display: flex;
  justify-content: center;
  align-items: center;
  margin-top: 16px;
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
  margin-right: 10px;
}


.filter-label::after {
  content: "";
  height: 16px;
  width: 1px;
  background: #ddd;
  margin-top: 13px;
  margin-left: 11px;
}

.filter-input {
  width: 120px;
  margin-right: 6px;
  color: #5271C1;
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

.v-text-field.filter-input :deep(.v-input__control) {
  min-height: 36px;
  padding-top: 0;
  padding-bottom: 0;
  align-items: center;
}

.v-select.filter-input :deep(.v-input__control) {
  min-height: 36px;
  padding-top: 0;
  padding-bottom: 0;
  align-items: center;
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
  height: 70px;
  border: 1px solid #D0DFF1;
  border-radius: 8px;
  overflow: hidden;
  background-color: rgba(208, 223, 241, 0.5);
}

.label-divider {
  display: inline-block;
  height: 18px;
  width: 1px;
  background-color: #bbb;
  margin-left: 10px;
  margin-bottom: 2px;
  border-radius: 1px;
  vertical-align: middle;
  width: 2px;
  background-color: #B0CAE6;
}
</style>