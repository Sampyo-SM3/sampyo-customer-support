<template>
    <v-container fluid class="pa-10">
      <v-row no-gutters class="search-row top-row">
        <!-- 요청기간 -->
        <v-col class="search-col request-period">
          <div class="label-box">요청기간</div>
          <div class="input-container pt-2 pb-2">
            <div class="date-wrapper">
              <v-text-field
                v-model="startDate"
                class="date-input"
                density="compact"
                hide-details
                readonly
                variant="outlined"
              ></v-text-field>
              <div class="calendar-icon-container">
                <v-btn icon class="calendar-btn">
                  <v-icon size="small" color="#2196F3">mdi-calendar</v-icon>
                </v-btn>
              </div>
            </div>
            
            <span class="date-separator">~</span>
            
            <div class="date-wrapper">
              <v-text-field
                v-model="endDate"
                class="date-input"
                density="compact"
                hide-details
                readonly
                variant="outlined"
              ></v-text-field>
              <div class="calendar-icon-container">
                <v-btn icon class="calendar-btn">
                  <v-icon size="small" color="#2196F3">mdi-calendar</v-icon>
                </v-btn>
              </div>
            </div>
            
            <div class="date-buttons">
              <div class="date-btn-container">
                <v-btn value="today" class="date-btn" @click="setDateRange('today')">오늘</v-btn>
                <v-btn value="week" class="date-btn" @click="setDateRange('week')">1주일</v-btn>
                <v-btn value="15days" class="date-btn" @click="setDateRange('15days')">15일</v-btn>
                <v-btn value="month" class="date-btn" @click="setDateRange('month')">1개월</v-btn>
                <v-btn value="3months" class="date-btn" @click="setDateRange('3months')">3개월</v-btn>
              </div>
            </div>
          </div>
        </v-col>
      </v-row>
  
      <!-- 제품구분 행 추가 -->
      <v-row no-gutters class="search-row bottom-row">
        <v-col class="search-col product-category">
          <div class="label-box">제품구분</div>
          <div class="input-container pt-2 pb-2">
            <!-- 여기에 제품구분 내용 추가 -->
          </div>
        </v-col>
      </v-row>
  
      <br>
      <v-divider></v-divider>
      <br>
  
      <!-- 데이터 테이블 상단 버튼 영역 -->
      <v-row class="top-button-row mb-2">
        <v-col class="d-flex align-center">
          <v-btn variant="flat" color="primary" class="custom-btn mr-2 white-text d-flex align-center" size="small">
            <v-icon size="default" class="mr-1">mdi-account</v-icon>
            CS담당자 지정
          </v-btn>
          <span class="mx-3 text-subtitle-2">총 {{ tableData.length }} 건 / 미처리: <span class="text-error">{{ getUnprocessedCount() }}</span></span>
          
          <v-spacer></v-spacer>
          
          <v-btn variant="flat" color="success" class="custom-btn white-text d-flex align-center" size="small">
            <v-icon size="default" class="mr-1">mdi-file-excel</v-icon>
            엑셀 다운로드
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
              <div class="th-cell">메모</div>
            </div>
            
            <!-- 테이블 데이터 행 -->
            <div v-for="(item, index) in tableData" :key="index" class="table-row">
              <div class="td-cell checkbox-cell">
                <v-checkbox hide-details density="compact" v-model="item.selected"></v-checkbox>
              </div>
              <div class="td-cell">{{ item.seq }}</div>
              <div class="td-cell">{{ formatDate(item.insertDt) }}</div>
              <div class="td-cell title-cell">
                <a :href="`#${item.seq}`" class="title-link">{{ item.projectName }}</a>
                <span v-if="item.hasAttachment" class="file-indicator">[0]</span>
              </div>              
              <div class="td-cell">{{ item.businessSector }}</div>
              <div class="td-cell" :class="getStatusClass(item.status)">{{ item.status }}</div>
              <div class="td-cell">{{ formatDate(item.completeDt) }}</div>
              <div class="td-cell">{{ item.manager || '-' }}</div>
              <div class="td-cell">{{ calculateDuration(item.insertDt, item.completeDt) }}</div>
              <div class="td-cell">{{ item.memo || '-' }}</div>
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
            <v-btn icon="mdi-chevron-left" variant="text" size="small"></v-btn>
            <v-btn size="small" variant="flat" color="primary">1</v-btn>
            <v-btn icon="mdi-chevron-right" variant="text" size="small"></v-btn>
          </div>
        </v-col>
      </v-row>
    </v-container>
  </template>
  
  <script>
  import axios from 'axios';
  
  export default {
    data() {
      return {
        startDate: '',
        endDate: '',
        dateRange: 'month',
        productType: 'test1',
        tableData: [],
        loading: false,
        selectAll: false,
        // 상태값 목록 (실제 API에서 받아올 수 있음)
        statusList: ['미처리', '진행중', '보류중', '종결']
      }
    },
    
    mounted() {
      // 컴포넌트 마운트 시 기본 날짜 범위 설정
      this.setDateRange('month');
      // 데이터 로드
      this.fetchData();
    },
    
    methods: {
      // 날짜 범위 설정 함수
      setDateRange(range) {
        this.dateRange = range;
        const today = new Date();
        let start = new Date(today);
        
        switch(range) {
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
        
        // 날짜 변경 시 데이터 다시 로드
        // this.fetchData();
      },
      
      // API 호출하여 데이터 가져오기
      async fetchData() {
        this.loading = true;
        try {
          const response = await axios.get('http://localhost:8080/api/require/list', {
            params: {
              startDate: this.startDate,
              endDate: this.endDate
              // 필요한 경우 추가 파라미터
            }
          });

          console.log('게시판 데이터 리스트 조회!! -> ' + response.data);
          
          // API 응답 데이터 처리
          if (response.data && Array.isArray(response.data)) {
            this.tableData = response.data.map(item => ({
              ...item,
              selected: false,
              // API에서 진행상태가 오지 않으면 임의로 설정
              status: item.status || this.getRandomStatus(),
              // 첨부파일 여부 (임시로 랜덤하게 설정)
              hasAttachment: Math.random() > 0.5
            }));
          } else {
            this.tableData = [];
          }
        } catch (error) {
          console.error('데이터 로드 중 오류 발생:', error);
          // 오류 발생 시 테스트 데이터 로드 (개발용)
          this.loadTestData();
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
        
        // this.tableData = [
        //   {
        //     seq: 'REQ001',
        //     selected: false,
        //     insertDt: today.toISOString(),
        //     projectName: '시스템 유지보수 요청',
        //     businessSector: 'IT부문',
        //     status: '미처리',
        //     completeDt: null,
        //     manager: '김담당',
        //     memo: '긴급 처리 필요',
        //     hasAttachment: true
        //   },
        //   {
        //     seq: 'REQ002',
        //     selected: false,
        //     insertDt: yesterday.toISOString(),
        //     projectName: '데이터 복구 요청',
        //     businessSector: '관리부문',
        //     status: '진행중',
        //     completeDt: null,
        //     manager: '이매니저',
        //     memo: '',
        //     hasAttachment: false
        //   },
        //   {
        //     seq: 'REQ003',
        //     selected: false,
        //     insertDt: lastWeek.toISOString(),
        //     projectName: '기능 개선 요청',
        //     businessSector: '개발부문',
        //     status: '종결',
        //     completeDt: yesterday.toISOString(),
        //     manager: '박책임',
        //     memo: '추가 요청사항 확인 필요',
        //     hasAttachment: true
        //   },
        //   {
        //     seq: 'REQ004',
        //     selected: false,
        //     insertDt: lastWeek.toISOString(),
        //     projectName: '오류 수정 요청',
        //     businessSector: 'IT부문',
        //     status: '보류중',
        //     completeDt: null,
        //     manager: '최팀장',
        //     memo: '사용자 확인 대기중',
        //     hasAttachment: false
        //   },
        //   {
        //     seq: 'REQ005',
        //     selected: false,
        //     insertDt: lastWeek.toISOString(),
        //     projectName: '시스템 업데이트 요청',
        //     businessSector: '개발부문',
        //     status: '진행중',
        //     completeDt: null,
        //     manager: '정대리',
        //     memo: '',
        //     hasAttachment: true
        //   }
        // ];
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
      
      // 미처리 건수 계산
      getUnprocessedCount() {
        return this.tableData.filter(item => item.status === '미처리').length;
      },
      
      // 전체 선택/해제 토글
      toggleSelectAll() {
        this.tableData.forEach(item => {
          item.selected = this.selectAll;
        });
      },
      
      // 랜덤 상태값 반환 (API에서 상태값이 없을 경우 사용)
      getRandomStatus() {
        return this.statusList[Math.floor(Math.random() * this.statusList.length)];
      },
      
      // 상태에 따른 클래스 반환
      getStatusClass(status) {
        switch(status) {
          case '미처리':
            return 'text-error';
          case '진행중':
            return 'text-info';
          case '보류중':
            return 'text-warning';
          case '종결':
            return 'text-success';
          default:
            return '';
        }
      }
    }
  }
  </script>
  
  <style scoped>
  .custom-btn {
    font-size: 14px;
    height : 35px; 
  }
  
  .search-row {
    display: flex;
    align-items: stretch;
    min-height: 40px;
    border-top: 1px solid #e0e0e0;
    border-bottom: 0; /* 하단 테두리 제거 */
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
  }
  
  .request-period, .product-category {
    max-width: 550px;
    flex-grow: 0;
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
    color: #333;
    background-color: #f5f5f5;
    white-space: nowrap;
    padding: 0 4px;
    border-right: 1px solid #eaeaea;
  }
  
  .input-container {
    display: flex;
    align-items: center;
    flex: 1;
    padding: 0 16px;
  }
  
  .date-wrapper {
    position: relative;
    display: flex;
    align-items: center;
    width: 160px;
  }
  
  .date-input {
    width: 120px;
  }
  
  .date-input :deep(.v-field__input) {
    font-size: 15px;
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
    color: #757575;
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
    color: #333333;
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
  }
  
  .table-header {
    display: flex;
    background-color: #f5f5f5;
    font-weight: 500;
    border-bottom: 1px solid #e0e0e0;
  }
  
  .table-row {
    display: flex;
    border-bottom: 1px solid #e0e0e0;
  }
  
  .table-row:hover {
    background-color: #f9f9f9;
  }
  
  .th-cell, .td-cell {
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
  
  .th-cell:last-child, .td-cell:last-child {
    border-right: none;
  }
  
  .checkbox-cell {
    flex: 0 0 60px;
    justify-content: center;
  }
  
  .th-cell:nth-child(2), .td-cell:nth-child(2) { flex: 0 0 80px; justify-content: center; } /* 접수번호 */
  .th-cell:nth-child(3), .td-cell:nth-child(3) { flex: 0 0 100px; justify-content: center; } /* 요청일 */
  .th-cell:nth-child(4), .td-cell:nth-child(4) { flex: 1; } /* 제목 */  
  .th-cell:nth-child(5), .td-cell:nth-child(5) { flex: 0 0 100px; justify-content: center; } /* 사업부문 */
  .th-cell:nth-child(6), .td-cell:nth-child(6) { flex: 0 0 90px; justify-content: center; } /* 진행상태 */
  .th-cell:nth-child(7), .td-cell:nth-child(7) { flex: 0 0 100px; justify-content: center; } /* 완료일 */
  .th-cell:nth-child(8), .td-cell:nth-child(8) { flex: 0 0 90px; justify-content: center; } /* 담당자 */    
  .th-cell:nth-child(9), .td-cell:nth-child(9) { flex: 0 0 100px; justify-content: center; } /* 소요시간 */    
  .th-cell:nth-child(10), .td-cell:nth-child(10) { flex: 0 0 180px; } /* 메모 */    
  
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
    color: #f44336;
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
  </style>