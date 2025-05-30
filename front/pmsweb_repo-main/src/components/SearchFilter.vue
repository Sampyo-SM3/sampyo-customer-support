<template>
      <v-row dense align="center" class="flex-wrap rounded-border sky-bg" style="gap:5px;">
  
        <!-- 요청기간 -->
        <v-col cols="12" sm="6" md="auto" class="d-flex align-center filter-col">
          <span class="filter-label">요청기간<span class="label-divider"></span></span>
          
          <!-- 시작일 입력 필드 -->
          <div class="start-date-wrapper">
            <VueDatePicker 
              class="date-picker" 
              :month-picker="false" 
              preview-format="yyyy-MM-dd" 
              v-model="localStartDate"
              :teleport="true" 
              position="bottom" 
              :enable-time-picker="false" 
              auto-apply 
              locale="ko" 
              format="yyyy-MM-dd"
              :week-start="1" 
              @update:model-value="onStartDateChange" 
              v-model:open="startDatePickerOpen"
              :clearable="false" 
              :text-input="false" 
            />
          </div>
          <span class="date-separator">~</span>
  
          <!-- 종료일 입력 필드 -->
          <div class="end-date-wrapper">
            <VueDatePicker 
              class="date-picker" 
              :month-picker="false" 
              preview-format="yyyy-MM-dd" 
              v-model="localEndDate"
              :teleport="true" 
              position="bottom" 
              :enable-time-picker="false" 
              auto-apply 
              locale="ko" 
              format="yyyy-MM-dd"
              :week-start="1" 
              @update:model-value="onEndDateChange" 
              v-model:open="endDatePickerOpen" 
              :clearable="false"
              :text-input="false" 
            />
          </div>
  
          <!-- 날짜 버튼 -->
          <div class="date-buttons mr-2">
            <div class="date-btn-container">
              <v-btn 
                value="today" 
                class="date-btn" 
                :class="{ 'active-date-btn': dateRange === 'today' }"
                @click="setDateRange('today')"
              >
                오늘
              </v-btn>
              <v-btn 
                value="week" 
                class="date-btn" 
                :class="{ 'active-date-btn': dateRange === 'week' }"
                @click="setDateRange('week')"
              >
                1주일
              </v-btn>
              <v-btn 
                value="15days" 
                class="date-btn" 
                :class="{ 'active-date-btn': dateRange === '15days' }"
                @click="setDateRange('15days')"
              >
                15일
              </v-btn>
              <v-btn 
                value="month" 
                class="date-btn" 
                :class="{ 'active-date-btn': dateRange === 'month' }"
                @click="setDateRange('month')"
              >
                1개월
              </v-btn>
              <v-btn 
                value="3months" 
                class="date-btn" 
                :class="{ 'active-date-btn': dateRange === '3months' }"
                @click="setDateRange('3months')"
              >
                3개월
              </v-btn>
            </div>
          </div>
        </v-col>
  
        <!-- 접수상태 -->
        <v-col cols="12" sm="6" md="auto" class="d-flex align-center filter-col">
          <span class="filter-label">접수상태<span class="label-divider"></span></span>
          <v-select 
            v-model="localSelectedStatus" 
            :items="statusOptions" 
            item-title="text" 
            item-value="value"
            variant="outlined" 
            density="compact" 
            hide-details 
            class="filter-input" 
          />
        </v-col>
  
        <!-- 담당자 -->
        <v-col cols="12" sm="6" md="auto" class="d-flex align-center filter-col">
          <span class="filter-label">담당자<span class="label-divider"></span></span>
          <v-text-field 
            v-model="localManager" 
            @keydown.enter="handleSearch()" 
            variant="outlined" 
            density="compact" 
            hide-details
            class="filter-input" 
          />
        </v-col>
  
        <!-- 제목 -->
        <v-col cols="12" sm="6" md="auto" class="d-flex align-center filter-col">
          <span class="filter-label">제목<span class="label-divider"></span></span>
          <v-text-field 
            v-model="localSubject" 
            @keydown.enter="handleSearch()" 
            variant="outlined" 
            density="compact" 
            hide-details
            class="filter-input-sub" 
          />
        </v-col>
  
        <!-- 검색 버튼 -->
        <v-col cols="12" sm="6" md="auto" class="d-flex justify-end">
          <v-btn 
            variant="flat" 
            color="primary" 
            class="custom-btn" 
            size="small" 
            @click="handleSearch()"
          >
            <v-icon size="default" class="mr-1">mdi-magnify</v-icon>
            조회
          </v-btn>
        </v-col>
  
      </v-row>    
  </template>
  
  <script>
  import VueDatePicker from '@vuepic/vue-datepicker';
  import '@vuepic/vue-datepicker/dist/main.css';
  
  export default {
    name: 'SearchFilter',
    components: {
      VueDatePicker
    },
    props: {
      // props는 필요 없음 - 컴포넌트 내부에서 모든 것을 처리
    },
    emits: ['search'],
    data() {
      return {
        startDatePickerOpen: false,
        endDatePickerOpen: false,
        localStartDate: new Date(),
        localEndDate: new Date(),
        localSelectedStatus: '%',
        localManager: '',
        localSubject: '',
        dateRange: 'month',
        statusOptions: [] // 상태 옵션도 컴포넌트 내부에서 관리
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
      // 기본 날짜 범위 설정 (1개월)
      this.setDateRange('month');
      // 상태 옵션 가져오기
      this.getStatus();

      this.handleSearch();
    },
    methods: {
      async getStatus() {
        try {
          // apiClient import 추가 필요
          const apiClient = (await import('@/api')).default;
          
          const statusList = await apiClient.get("/api/code/list", {
            params: {
              category: 'STATUS'
            }
          });
  
          // 상태 이름 리스트 저장
          this.statusOptions = statusList.data.map(status => ({
            text: status.codeName,
            value: status.codeId
          }));
  
          // '전체' 옵션을 맨 앞에 추가
          this.statusOptions.unshift({ text: '전체', value: '%' });
  
          // 초기값을 '전체'로 설정
          this.localSelectedStatus = '%';
  
        } catch (error) {
          console.error("❌ 상태 목록 조회 오류:", error);
          // 오류 발생 시 기본값 설정
          this.statusOptions = [{ text: '전체', value: '%' }];
          this.localSelectedStatus = '%';
        }
      },
      onStartDateChange(date) {
        this.localStartDate = date;
        this.startDatePickerOpen = false;
      },
  
      onEndDateChange(date) {
        this.localEndDate = date;
        this.endDatePickerOpen = false;
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
  
        this.localStartDate = start;
        this.localEndDate = today;
      },
  
      // 날짜 포맷팅 함수
      formattedDate(dateObj) {
        const year = dateObj.getFullYear();
        const month = String(dateObj.getMonth() + 1).padStart(2, '0');
        const day = String(dateObj.getDate()).padStart(2, '0');
        return `${year}-${month}-${day}`;
      },
  
      // 검색 이벤트 발생
      handleSearch() {
        const searchParams = {
          startDate: this.formattedDate(this.localStartDate) + ' 00:00:00',
          endDate: this.formattedDate(this.localEndDate) + ' 23:59:59',
          status: this.localSelectedStatus,
          manager: this.localManager,
          subject: this.localSubject
        };
  
        // 부모 컴포넌트에 검색 파라미터 전달
        this.$emit('search', searchParams);
      },
  
      // 검색 조건 초기화
      resetFilters() {
        this.setDateRange('month');
        this.localSelectedStatus = '%';
        this.localManager = '';
        this.localSubject = '';
      }
    }
  }
  </script>
  
  <style scoped>
  /* VueDatePicker 관련 추가 스타일 */
  .date-picker {
    width: auto;
    min-width: 0;
    padding: 0;
  }
  
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
  
  .custom-btn {
    font-size: 14px;
    height: 40px;
    border-radius: 10px;
  }
  
  .date-separator {
    margin-left: -15px;
    z-index: 100;
    font-size: 16px;
    color: #7A7A7A;
  }
  
  .start-date-wrapper {
    margin-left: -0px;
    position: relative;
    display: flex;
    align-items: center;
    width: 150px;
    color: #737577;
  }
  
  .end-date-wrapper {
    margin-left: 10px;
    position: relative;
    display: flex;
    align-items: center;
    width: 150px;
    color: #737577;
  }
  
  .date-buttons {
    margin-left: 5px;
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
    padding: 0 8px;
    font-size: 13px;
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