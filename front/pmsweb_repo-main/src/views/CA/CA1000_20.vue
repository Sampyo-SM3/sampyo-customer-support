<template>
  <v-container>
    <v-card class="mx-auto">
      <v-card-title class="text-h5 font-weight-bold">
        완료함
        <v-spacer></v-spacer>
        <v-btn icon variant="text" to="/home">
          <v-icon>mdi-home</v-icon>
        </v-btn>
        <span class="text-caption ms-2">전자결재</span>
      </v-card-title>

      <!-- 탭 메뉴 -->
      <v-tabs v-model="activeTab" bg-color="transparent">
        <v-tab value="new">새로고침</v-tab>
        <v-tab value="history">복사</v-tab>
        <v-tab value="category">일괄칼럼</v-tab>
        <v-tab value="setting">엑셀저장</v-tab>
      </v-tabs>

      <!-- 검색 필터 영역 -->
      <v-card-text>
        <v-row align="center">
          <v-col cols="12" sm="6" md="3">
            <v-select v-model="searchType" :items="searchTypes" label="제목" variant="outlined" density="compact"
              hide-details class="mb-2"></v-select>
          </v-col>

          <v-col cols="12" sm="6" md="5">
            <v-text-field v-model="searchText" label="검색어를 입력하세요" variant="outlined" density="compact" hide-details
              append-inner-icon="mdi-magnify" class="mb-2"></v-text-field>
          </v-col>

          <v-col cols="auto">
            <v-btn color="primary" size="small" class="mb-2">검색</v-btn>
          </v-col>

          <v-col cols="auto">
            <v-checkbox v-model="searchInTitle" label="상세검색" hide-details density="compact" class="mb-2"></v-checkbox>
          </v-col>

          <v-spacer></v-spacer>

          <v-col cols="12" sm="6" md="3">
            <div class="d-flex align-center">
              <v-select v-model="selectFilter" :items="filterOptions" label="전체" variant="outlined" density="compact"
                hide-details class="mb-2 me-2"></v-select>

              <v-checkbox v-model="dateFilter" label="기간" hide-details density="compact" class="mb-2"></v-checkbox>

              <div class="text-caption ms-4">
                <span>7일</span>
                <v-slider v-model="days" :min="1" :max="30" :step="1" class="mx-2" thumb-label dense hide-details
                  style="max-width: 120px"></v-slider>
                <span>30일</span>
                <span class="ml-2">전체</span>
              </div>
            </div>
          </v-col>
        </v-row>
      </v-card-text>

      <!-- 테이블 영역 -->
      <v-data-table :headers="headers" :items="items" :items-per-page="itemsPerPage" :footer-props="{
        'items-per-page-options': [10, 15, 20, 30]
      }" item-value="id" class="elevation-1" show-select density="compact">
        <template #[`item.category`]="{ item }">
          <v-btn size="small" variant="text" class="text-caption">
            {{ item.columns.category }}
          </v-btn>
        </template>

        <template #[`item.title`]="{ item }">
          <div class="d-flex align-center">
            <v-btn size="small" variant="text" class="text-caption text-start">
              {{ item.columns.title }}
            </v-btn>
          </div>
        </template>

        <template #[`item.attachments`]="{ item }">
          <v-icon v-if="item.columns.attachments" size="small">mdi-paperclip</v-icon>
        </template>

        <template #[`item.content`]="{ item }">
          <v-icon v-if="item.columns.content === '시내교통비 지출결의서'">mdi-file-document-outline</v-icon>
          <span v-else>{{ item.columns.content }}</span>
        </template>

        <template #[`item.actions`]="{ item }">
          <v-icon v-if="item.columns.comment" size="small" color="blue">mdi-comment-outline</v-icon>
          <v-icon v-if="item.columns.edit" size="small" color="green" class="ms-2">mdi-pencil</v-icon>
        </template>
      </v-data-table>
    </v-card>
  </v-container>
</template>

<script setup>
import { ref } from 'vue';

// 탭 관련
const activeTab = ref('new');

// 검색 관련
const searchType = ref('title');
const searchTypes = ref(['제목', '내용', '작성자', '문서번호']);
const searchText = ref('');
const searchInTitle = ref(false);

// 필터 관련
const selectFilter = ref('all');
const filterOptions = ref(['전체', '대기', '완료', '거절']);
const dateFilter = ref(true);
const days = ref(7);

// 테이블 관련
const itemsPerPage = ref(10);
const headers = ref([
  { title: '', key: 'select', sortable: false },
  { title: '구분', key: 'category', sortable: true },
  { title: '제목', key: 'title', sortable: true },
  { title: '기안부서', key: 'department', sortable: true },
  { title: '기안자', key: 'author', sortable: true },
  { title: '파일', key: 'attachments', sortable: false, align: 'center' },
  { title: '양식명', key: 'content', sortable: true },
  { title: '문서번호', key: 'docNumber', sortable: true },
  { title: '일자', key: 'date', sortable: true },
  { title: '의견', key: 'comment', sortable: false, align: 'center' },
  { title: '수정', key: 'actions', sortable: false, align: 'center' },
]);

// 테이블 데이터
const items = ref([
  {
    id: 1,
    columns: {
      category: '일람',
      title: '25년 03월 SMS팀 시내교통비',
      department: 'SMS팀(삼표산업)',
      author: '변세현',
      attachments: true,
      content: '시내교통비 지출결의서',
      docNumber: 'SMS팀(삼표산업)25-061',
      date: '2025-04-01 17:06',
      comment: false,
      edit: false
    }
  },
  {
    id: 2,
    columns: {
      category: '일람',
      title: 'test',
      department: 'SMS팀(삼표산업)',
      author: '변세현',
      attachments: false,
      content: '업무협조전',
      docNumber: 'SMS팀(삼표산업)25-051',
      date: '2025-03-21 10:59',
      comment: true,
      edit: false
    }
  },
  {
    id: 3,
    columns: {
      category: '일람',
      title: '사무용 전화기 신청의 건',
      department: 'WEB팀(삼표산업)',
      author: '변세현',
      attachments: false,
      content: '사무용 전화기 신청서',
      docNumber: 'CORE개발팀(삼표산업)-2',
      date: '2025-03-17 08:45',
      comment: false,
      edit: true
    }
  },
  {
    id: 4,
    columns: {
      category: '일람',
      title: '삼표산업 PC사용자 변경 신청의 件',
      department: 'SMS팀(삼표산업)',
      author: '변세현',
      attachments: false,
      content: '업무협조전',
      docNumber: 'SMS팀(삼표산업)25-049',
      date: '2025-03-13 15:54',
      comment: false,
      edit: false
    }
  },
  {
    id: 5,
    columns: {
      category: '일람',
      title: '25년 02월 SMS팀 시내교통비',
      department: 'SMS팀(삼표산업)',
      author: '변세현',
      attachments: true,
      content: '시내교통비 지출결의서',
      docNumber: 'SMS팀(삼표산업)25-041',
      date: '2025-03-07 13:47',
      comment: false,
      edit: false
    }
  },
  {
    id: 6,
    columns: {
      category: '일람',
      title: 'SMS팀 삼표시멘트 삼척공장 출장결과 보고',
      department: 'SMS팀(삼표산업)',
      author: '변세현',
      attachments: true,
      content: '국내출장 결과 보고',
      docNumber: 'SMS팀(삼표산업)25-039',
      date: '2025-03-05 18:10',
      comment: false,
      edit: false
    }
  },
  {
    id: 7,
    columns: {
      category: '일람',
      title: '신규 PC 신청의 건',
      department: 'SMS팀(삼표산업)',
      author: '변세현',
      attachments: false,
      content: 'PC신청서',
      docNumber: 'SMS팀(삼표산업)-2025-0',
      date: '2025-02-18 17:42',
      comment: true,
      edit: true
    }
  },
  {
    id: 8,
    columns: {
      category: '일람',
      title: '사무용 가구 신청의 件',
      department: 'WEB팀(삼표산업)',
      author: '변세현',
      attachments: false,
      content: '신규가구 신청서',
      docNumber: 'CORE개발팀(삼표산업)-2',
      date: '2025-02-13 13:02',
      comment: true,
      edit: true
    }
  },
  {
    id: 9,
    columns: {
      category: '일람',
      title: '노후 모니터 교체 신청의 건',
      department: 'WEB팀(삼표산업)',
      author: '변세현',
      attachments: false,
      content: 'PC신청서',
      docNumber: 'CORE개발팀(삼표산업)-2',
      date: '2025-02-13 08:54',
      comment: true,
      edit: true
    }
  }
]);
</script>

<style scoped>
.v-data-table :deep(.v-data-table__th) {
  background-color: #f5f5f5;
  font-weight: bold;
  color: rgba(0, 0, 0, 0.87);
}

.v-data-table :deep(.v-data-table-header__sort-badge) {
  background-color: #1976d2;
  color: white;
}

.v-slider {
  display: inline-block;
}
</style>