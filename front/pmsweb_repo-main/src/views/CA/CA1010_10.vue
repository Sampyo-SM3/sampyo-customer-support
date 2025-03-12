<template>
  <v-container fluid class="pa-0">
    <!-- 타이틀 영역 -->
    <div class="title-area">
      <h2>고객 문의 내역</h2>
      <v-divider></v-divider>
    </div>

    <!-- 검색 영역 -->
    <div class="search-area">
      <div class="search-row">
        <div class="search-item">
          <div class="search-label">요청기간</div>
          <div class="search-input-group">
            <v-text-field v-model="searchParams.startDate" hide-details density="compact" class="date-input"
              readonly></v-text-field>
            <v-btn icon size="small" variant="text" color="primary" class="date-btn">
              <v-icon>mdi-calendar</v-icon>
            </v-btn>
            <span class="mx-1">-</span>
            <v-text-field v-model="searchParams.endDate" hide-details density="compact" class="date-input"
              readonly></v-text-field>
            <v-btn icon size="small" variant="text" color="primary" class="date-btn">
              <v-icon>mdi-calendar</v-icon>
            </v-btn>
          </div>
        </div>

        <div class="search-item">
          <v-btn-toggle v-model="searchParams.period" density="compact" mandatory class="period-toggle">
            <v-btn value="today">오늘</v-btn>
            <v-btn value="week">1주일</v-btn>
            <v-btn value="month">1개월</v-btn>
            <v-btn value="all">전체</v-btn>
          </v-btn-toggle>
        </div>

        <div class="search-item">
          <v-select v-model="searchParams.progressStatus" :items="progressStatuses" label="진행상태" hide-details
            density="compact" class="status-select" variant="outlined"></v-select>
        </div>

        <div class="search-item">
          <v-select v-model="searchParams.inquiryType" :items="inquiryTypes" label="진행상태" hide-details density="compact"
            class="status-select" variant="outlined"></v-select>
        </div>

        <div class="search-item flex-grow-1">
          <v-select v-model="searchParams.searchType" :items="searchTypes" label="전체" hide-details density="compact"
            class="search-type-select" variant="outlined"></v-select>
          <v-text-field v-model="searchParams.keyword" hide-details density="compact" placeholder="검색어 입력"
            class="keyword-input" variant="outlined"></v-text-field>
        </div>

        <div class="search-item">
          <v-btn color="primary" class="search-btn">검색</v-btn>
        </div>
      </div>
    </div>

    <!-- 툴바 영역 -->
    <div class="toolbar-area">
      <div class="left-tools">
        <v-btn prepend-icon="mdi-account-check" variant="text" class="tool-btn">
          CS담당자 지정
        </v-btn>
        <v-divider vertical class="mx-2"></v-divider>
        <v-btn prepend-icon="mdi-email-outline" variant="text" class="tool-btn">
          고객회답도착 전송하기
        </v-btn>
        <span class="count-info">총 4 건 / 미처리: 0</span>
      </div>
      <div class="right-tools">
        <v-btn prepend-icon="mdi-microsoft-excel" color="success" class="excel-btn">
          엑셀 다운로드
        </v-btn>
      </div>
    </div>

    <!-- 테이블 영역 -->
    <v-table density="compact" class="inquiry-table">
      <thead>
        <tr>
          <th class="text-center" style="width: 40px">
            <v-checkbox v-model="selectAll" hide-details density="compact" @click="toggleSelectAll"></v-checkbox>
          </th>
          <th class="text-center" style="width: 80px">접수번호</th>
          <th class="text-center" style="width: 100px">요청일</th>
          <th class="text-center" style="min-width: 300px">제목</th>
          <th class="text-center" style="width: 100px">작성자</th>
          <th class="text-center" style="width: 100px">진행상태</th>
          <th class="text-center" style="width: 100px">종결일</th>
          <th class="text-center" style="width: 100px">소요시간</th>
        </tr>
      </thead>
      <tbody>
        <tr v-for="item in inquiryItems" :key="item.id" @click="viewDetail(item)" style="cursor: pointer;">
          <td class="text-center" @click.stop>
            <v-checkbox v-model="item.selected" hide-details density="compact"></v-checkbox>
          </td>
          <td class="text-center">{{ item.id }}</td>
          <td class="text-center">{{ item.requestDate }}</td>
          <td>{{ item.title }}</td>
          <td class="text-center">{{ item.writer }}</td>
          <td class="text-center">
            <v-chip size="small" :color="getStatusColor(item.status)" text-color="white" class="status-chip">
              {{ item.status }}
            </v-chip>
          </td>
          <td class="text-center">{{ item.completionDate || '-' }}</td>
          <td class="text-center">{{ item.elapsedTime }}</td>
        </tr>
      </tbody>
    </v-table>

    <!-- 페이지네이션 -->
    <div class="pagination-area">
      <v-pagination v-model="page" :length="totalPages" :total-visible="7"></v-pagination>
    </div>
  </v-container>
</template>

<script>
export default {
  data() {
    return {
      // 검색 파라미터
      searchParams: {
        startDate: '2025-03-01',
        endDate: '2025-03-10',
        period: 'month',
        progressStatus: '전체',
        inquiryType: '전체',
        searchType: '전체',
        keyword: ''
      },

      // 선택 관련
      selectAll: false,

      // 페이지네이션
      page: 1,
      totalPages: 1,

      // 조회 결과 데이터
      inquiryItems: [
        {
          id: 'CS1487',
          selected: false,
          requestDate: '2025-03-08',
          title: '안전에서 사업협력 문의',
          writer: '이진',
          status: '비처리',
          completionDate: '2025-03-10',
          elapsedTime: '1일 21시간'
        },
        {
          id: 'CS1486',
          selected: false,
          requestDate: '2025-03-08',
          title: 'MSDS요청건',
          writer: '김예은',
          status: '비처리',
          completionDate: '2025-03-10',
          elapsedTime: '1일 16시간'
        },
        {
          id: 'CS1485',
          selected: false,
          requestDate: '2025-03-08',
          title: '삼표 몰탈 일반마감용 MSDS',
          writer: '김계욱',
          status: '비처리',
          completionDate: '2025-03-10',
          elapsedTime: '1일 23시간'
        },
        {
          id: 'CS1484',
          selected: false,
          requestDate: '2025-03-03',
          title: '건조시멘트모르타르(일반용) 지재시험성적서 요청건',
          writer: '세려준',
          status: '처리',
          completionDate: '2025-03-04',
          elapsedTime: '0일 16시간'
        }
      ],

      // 셀렉트 옵션들
      progressStatuses: ['전체', '미처리', '처리중', '완료', '보류'],
      inquiryTypes: ['전체', '제품/기술문의', '배차문의', '불편사항', '자료요청', '1:1문의'],
      searchTypes: ['전체', '제목', '내용', '작성자', '접수번호']
    };
  },
  methods: {
    toggleSelectAll() {
      this.inquiryItems.forEach(item => {
        item.selected = this.selectAll;
      });
    },
    viewDetail(item) {
      console.log('View detail for:', item.id);
      // 상세 페이지로 이동하는 로직
    },
    getStatusColor(status) {
      const colorMap = {
        '비처리': 'error',
        '처리중': 'warning',
        '처리': 'success',
        '보류': 'grey'
      };
      return colorMap[status] || 'primary';
    }
  },
  computed: {
    selectedCount() {
      return this.inquiryItems.filter(item => item.selected).length;
    }
  },
  watch: {
    inquiryItems: {
      handler() {
        this.selectAll = this.inquiryItems.length > 0 &&
          this.inquiryItems.every(item => item.selected);
      },
      deep: true
    }
  }
};
</script>

<style scoped>
.title-area {
  margin-bottom: 20px;
}

.title-area h2 {
  font-size: 18px;
  font-weight: 500;
  margin-bottom: 10px;
  color: #333;
}

.search-area {
  background-color: #f5f5f5;
  padding: 15px;
  margin-bottom: 15px;
  border: 1px solid #e0e0e0;
}

.search-row {
  display: flex;
  flex-wrap: wrap;
  gap: 10px;
  align-items: center;
}

.search-item {
  display: flex;
  align-items: center;
}

.search-label {
  min-width: 70px;
  font-size: 14px;
  color: #555;
}

.search-input-group {
  display: flex;
  align-items: center;
}

.date-input {
  width: 110px;
}

.date-btn {
  margin: 0 2px;
}

.period-toggle {
  height: 36px;
  margin-left: 5px;
}

.status-select {
  width: 150px;
}

.search-type-select {
  width: 100px;
  margin-right: 8px;
}

.keyword-input {
  width: 200px;
}

.search-btn {
  height: 36px;
  min-width: 80px;
}

.toolbar-area {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 10px;
  padding: 0 10px;
}

.left-tools,
.right-tools {
  display: flex;
  align-items: center;
}

.tool-btn {
  text-transform: none;
  letter-spacing: 0;
  font-weight: normal;
}

.count-info {
  margin-left: 10px;
  font-size: 14px;
  color: #666;
}

.excel-btn {
  text-transform: none;
  letter-spacing: 0;
}

.inquiry-table {
  border: 1px solid #e0e0e0;
}

.inquiry-table th {
  background-color: #f5f5f5;
  color: #333;
  font-weight: 500;
  white-space: nowrap;
}

.status-chip {
  font-size: 12px;
  height: 24px;
}

.pagination-area {
  display: flex;
  justify-content: center;
  margin: 20px 0;
}
</style>