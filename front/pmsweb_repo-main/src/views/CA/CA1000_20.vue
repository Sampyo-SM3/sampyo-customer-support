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
                <v-btn value="today" class="date-btn">오늘</v-btn>
                <v-btn value="week" class="date-btn">1주일</v-btn>
                <v-btn value="15days" class="date-btn">15일</v-btn>
                <v-btn value="month" class="date-btn">1개월</v-btn>
                <v-btn value="3months" class="date-btn">3개월</v-btn>
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
          <v-btn variant="flat" color="primary" class="mr-2 white-text d-flex align-center" size="small">
            <v-icon size="small" class="mr-1">mdi-account</v-icon>
            CS담당자 지정
          </v-btn>
          <span class="mx-3 text-subtitle-2">총 5 건 / 미처리: <span class="text-error">1</span></span>
          
          <v-spacer></v-spacer>
          
          <v-btn variant="flat" color="success" class="white-text d-flex align-center" size="small">
            <v-icon size="small" class="mr-1">mdi-file-excel</v-icon>
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
                <v-checkbox hide-details density="compact"></v-checkbox>
              </div>
              <div class="th-cell">접수번호</div>
              <div class="th-cell">요청일</div>
              <div class="th-cell">부문</div>
              <div class="th-cell">문의구분</div>
              <div class="th-cell">문의유형</div>
              <div class="th-cell">
                <div class="header-with-divider">
                  <span>회원</span>
                  <v-divider vertical class="mx-1 header-divider"></v-divider>
                  <span>거래처명</span>
                </div>
              </div>
              <div class="th-cell">제목</div>
              <div class="th-cell">진행상태</div>
              <div class="th-cell">종결일</div>
              <div class="th-cell">소요시간</div>
            </div>
            
            <!-- 테이블 데이터 행 -->
            <div v-for="(item, index) in tableData" :key="index" class="table-row">
              <div class="td-cell checkbox-cell">
                <v-checkbox hide-details density="compact" v-model="item.selected"></v-checkbox>
              </div>
              <div class="td-cell">{{ item.id }}</div>
              <div class="td-cell">{{ item.requestDate }}</div>
              <div class="td-cell">{{ item.department }}</div>
              <div class="td-cell">{{ item.inquiryType }}</div>
              <div class="td-cell">{{ item.inquiryCategory }}</div>
              <div class="td-cell">
                <span>{{ item.member }}</span>
                <span v-if="item.customer">
                  <v-divider vertical class="mx-1 d-inline-block" style="height: 12px;"></v-divider>
                  {{ item.customer }}
                </span>
              </div>
              <div class="td-cell title-cell">
                <a :href="`#${item.id}`" class="title-link">{{ item.title }}</a>
                <span v-if="item.file" class="file-indicator">[0]</span>
              </div>
              <div class="td-cell" :class="{'text-error': item.status === '미처리'}">{{ item.status }}</div>
              <div class="td-cell">{{ item.completionDate }}</div>
              <div class="td-cell">{{ item.duration }}</div>
            </div>
          </div>
          
          <!-- 페이지네이션 -->
          <div class="pagination-container">
            <v-btn icon="mdi-chevron-left" variant="text" size="small"></v-btn>
            <v-btn size="small" variant="flat" color="primary">1</v-btn>
            <v-btn icon="mdi-chevron-right" variant="text" size="small"></v-btn>
          </div>
        </v-col>
      </v-row>
    </v-container>
  </template>
  
  <script>
  export default {
    data() {
      return {
        startDate: '2025-03-03',
        endDate: '2025-03-10',
        dateRange: 'month',
        productType: 'test1',
        tableData: [
          {
            id: 'CS1488',
            selected: false,
            requestDate: '2025-03-10',
            department: '기타',
            inquiryType: '1:1문의',
            inquiryCategory: '',
            member: '회원',
            customer: '예스이푸씨/이주혁',
            title: '삼표 사이트에서 1종 밀크 사이트 Tone간 간격 차이의 색상이 Tone간 간격 [0]',
            status: '미처리',
            completionDate: '',
            duration: '',
            file: true
          },
          {
            id: 'CS1487',
            selected: false,
            requestDate: '2025-03-08',
            department: '기타',
            inquiryType: '제품/기술문의',
            inquiryCategory: '기타',
            member: '비회원',
            customer: '이진',
            title: '일도에서 시공업체 문의 [0]',
            status: '응답',
            completionDate: '2025-03-10',
            duration: '1일 21시간'
          },
          {
            id: 'CS1486',
            selected: false,
            requestDate: '2025-03-08',
            department: '몰탈',
            inquiryType: '자료요청',
            inquiryCategory: '',
            member: '비회원',
            customer: '권애슨',
            title: 'MSDS요청의 건 [0]',
            status: '응답',
            completionDate: '2025-03-10',
            duration: '1일 16시간'
          },
          {
            id: 'CS1485',
            selected: false,
            requestDate: '2025-03-08',
            department: '몰탈',
            inquiryType: '자료요청',
            inquiryCategory: '',
            member: '비회원',
            customer: '김재욱',
            title: '삼표 몰탈 일반마감용 MSDS [0]',
            status: '응답',
            completionDate: '2025-03-10',
            duration: '1일 23시간'
          },
          {
            id: 'CS1484',
            selected: false,
            requestDate: '2025-03-03',
            department: '몰탈',
            inquiryType: '자료요청',
            inquiryCategory: '',
            member: '회원',
            customer: '채하준',
            title: '건조시멘트모르타르(일반용) 자재시험성적서 요청건. [0]',
            status: '응답',
            completionDate: '2025-03-04',
            duration: '0일 16시간'
          }
        ]
      }
    }
  }
  </script>
  
  <style scoped>
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
    flex: 0 0 40px;
    justify-content: center;
  }
  
  .th-cell:nth-child(2), .td-cell:nth-child(2) { flex: 0 0 80px; } /* 접수번호 */
  .th-cell:nth-child(3), .td-cell:nth-child(3) { flex: 0 0 90px; } /* 요청일 */
  .th-cell:nth-child(4), .td-cell:nth-child(4) { flex: 0 0 70px; } /* 부문 */
  .th-cell:nth-child(5), .td-cell:nth-child(5) { flex: 0 0 90px; } /* 문의구분 */
  .th-cell:nth-child(6), .td-cell:nth-child(6) { flex: 0 0 90px; } /* 문의유형 */
  .th-cell:nth-child(7), .td-cell:nth-child(7) { flex: 0 0 150px; } /* 회원/거래처명 */
  .th-cell:nth-child(8), .td-cell:nth-child(8) { flex: 1; } /* 제목 */
  .th-cell:nth-child(9), .td-cell:nth-child(9) { flex: 0 0 80px; } /* 진행상태 */
  .th-cell:nth-child(10), .td-cell:nth-child(10) { flex: 0 0 90px; } /* 종결일 */
  .th-cell:nth-child(11), .td-cell:nth-child(11) { flex: 0 0 90px; } /* 소요시간 */
  
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
  
  .pagination-container {
    display: flex;
    justify-content: center;
    align-items: center;
    margin-top: 16px;
  }
  </style>