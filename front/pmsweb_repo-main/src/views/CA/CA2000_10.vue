<template>
    <v-container fluid class="pr-5 pl-5 pt-7">
  
      <!-- test -->
      <v-btn @click="test1()">SR요청서 띄우기 테스트</v-btn>
      <br>
      <br>      
      <v-btn @click="test2()">알림톡테스트</v-btn>
      <br>
      <br>      
      <v-btn @click="test3()">메일테스트</v-btn>
      <br>
      <br>
      
  
  

  
      
      
      
    </v-container>

  </template>
  
  <script>

  
  export default {
    data() {
      return {
     
      }
    },
  
  
    methods: {
      test () {
        console.log('--test--');
  
            
      },
   
      },
  

  }</script>
  
  <style scoped>
  .breadcrumb-div {
    font-size: 12px;
    color: #A1A6A6;
  }
  
  .title-div {
    font-size: 25px;
  }
  
  .manager-search  {
    padding-block: 10px;
    padding-left: 10px;
    width: 800px;
    font-weight: 400;
  }
  
  .title-search{
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
    height: 35px;
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
    color: #7A5344;
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
    color: #7A5344;
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
    align-items: stretch;
    min-height: 40px;
    border-top: 1px solid #e0e0e0;
    border-bottom: 0;
    /* 하단 테두리 제거 */
  }
  
  /* 테두리 라운드 처리를 위한 스타일 */
  .search-row.top-row {
    border-top: 2px solid #e0e0e0;
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
  
  .search-row.bottom-row .search-col:first-child {
    border-bottom-left-radius: 8px;
  }
  
  /* 마지막 열에 오른쪽 라운드 적용 */
  .search-row.top-row .search-col:last-child {
    border-top-right-radius: 8px;
  }
  
  .search-row.bottom-row .search-col:last-child {
    border-bottom-right-radius: 8px;
  }
  
  /* 전체 검색 영역에 그림자 효과 추가 (선택사항) */
  .search-row {
    display: flex;
    align-items: stretch;
    min-height: 40px;
    border-top: 1px solid #e0e0e0;
    border-bottom: 0;
    border-left: 1px solid #e0e0e0;
    border-right: 1px solid #e0e0e0;
  }
  
  
  /* 첫 번째 열에 왼쪽 라운드 적용 */
  .search-row.top-row .search-col:first-child {
    border-top-left-radius: 8px;
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
  
  /* 수정된 스타일 */
  .label-box {
    width: 80px;
    flex-shrink: 0;
    height: 100%;
    display: flex;
    align-items: center;
    justify-content: center;
    font-size: 14px;
    font-weight: 500;
    color: #333333;
    background-color: #e6eef8;
    /* 그리드 헤더와 같은 색상으로 변경 */
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
  
  /*
  .th-cell:last-child,
  .td-cell:last-child {
    border-right: none;
  }
  */
  
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
  
  /* 완료일 */
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