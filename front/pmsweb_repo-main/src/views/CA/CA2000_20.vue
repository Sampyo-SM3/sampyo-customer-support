<template>
  <v-container fluid>
    <!-- 차트 상단 영역 -->
    <v-row>
      <v-col cols="auto" style="width: 280px;">
        <v-card class="pa-4" elevation="2">
          <h3 class="mb-4">문의유형 분포</h3>
          <div class="chart-container">
            <canvas ref="inquiryTypeChartCanvas"></canvas>
          </div>
        </v-card>
      </v-col>

      <v-col cols="auto" style="width: 400px;">
        <v-card class="pa-4" elevation="2">
          <h3 class="mb-4">진행상태 분포</h3>
          <div class="chart-container">
            <canvas ref="statusChartCanvas"></canvas>
          </div>
        </v-card>
      </v-col>

      <v-col cols="auto" style="width: 600px;">
        <v-card class="pa-4" elevation="2">
          <h3 class="mb-4">월별 문의글 건수</h3>
          <div class="chart-container">
            <canvas ref="monthlyChartCanvas"></canvas>
          </div>
        </v-card>
      </v-col>
    </v-row>

    <!-- 🔽 필터 버튼 -->
    <v-row class="mt-4">
      <v-col cols="12">
        <v-btn-toggle v-model="selectedView" class="custom-btn-toggle" mandatory>
          <v-btn value="my" :class="{ 'selected-btn': selectedView === 'my' }">
            나의 문의글
          </v-btn>
          <v-btn value="dept" :class="{ 'selected-btn': selectedView === 'dept' }">
            부서 문의글
          </v-btn>
        </v-btn-toggle>
      </v-col>
    </v-row>

    <!-- 하단 그리드 -->
    <v-row>
      <v-col cols="12">
        <v-data-table :headers="tableHeaders" :items="filteredItems" class="elevation-1" items-per-page="5">
          <template #item="{ item }">
            <tr>
              <td>{{ item.id }}</td>
              <td>{{ item.title }}</td>
              <td>{{ item.type }}</td>
              <td>{{ item.author }}</td>
              <td>{{ item.status }}</td>
              <td>{{ item.date }}</td>
            </tr>
          </template>
        </v-data-table>
      </v-col>
    </v-row>
  </v-container>
</template>

<script>
import {
  Chart,
  DoughnutController,
  BarController,
  BarElement,
  ArcElement,
  Tooltip,
  Legend,
  CategoryScale,
  LinearScale
} from 'chart.js';

Chart.register(
  DoughnutController,
  BarController,
  BarElement,
  ArcElement,
  Tooltip,
  Legend,
  CategoryScale,
  LinearScale
);

export default {
  name: 'StatisticsDashboard',
  data() {
    return {
      selectedView: 'my',
      currentUser: '김철수',
      currentDept: '경영지원팀',
      tableHeaders: [
        { text: '번호', value: 'id' },
        { text: '제목', value: 'title' },
        { text: '문의유형', value: 'type' },
        { text: '작성자', value: 'author' },
        { text: '진행상태', value: 'status' },
        { text: '작성일', value: 'date' },
      ],
      tableItems: [
        { id: 1, title: '로그인 오류', type: '단순문의', author: '김철수', dept: '경영지원팀', status: '미처리', date: '2025-05-01' },
        { id: 2, title: '데이터 누락', type: '데이터 수정', author: '이영희', dept: '생산팀', status: '진행중', date: '2025-05-02' },
        { id: 3, title: '화면 깨짐', type: '프로그램 수정', author: '박민수', dept: '생산팀', status: '보류', date: '2025-05-03' },
        { id: 4, title: '버튼 작동안함', type: '프로그램 수정', author: '최지훈', dept: '경영지원팀', status: '종결', date: '2025-05-04' },
        { id: 5, title: 'SR 문의사항', type: '단순문의', author: '정예은', dept: '경영지원팀', status: 'SR', date: '2025-05-05' }
      ]
    };
  },
  computed: {
    filteredItems() {
      return this.selectedView === 'my'
        ? this.tableItems.filter(item => item.author === this.currentUser)
        : this.tableItems.filter(item => item.dept === this.currentDept);
    }
  },
  mounted() {
    this.drawInquiryTypeChart();
    this.drawStatusChart();
    this.drawMonthlyChart();
  },
  methods: {
    drawInquiryTypeChart() {
      const ctx = this.$refs.inquiryTypeChartCanvas.getContext('2d');
      new Chart(ctx, {
        type: 'doughnut',
        data: {
          labels: ['단순문의', '데이터 수정', '프로그램 수정'],
          datasets: [{
            data: [12, 18, 9],
            backgroundColor: ['#FF9F40', '#4BC0C0', '#9966FF']
          }]
        },
        options: {
          responsive: true,
          maintainAspectRatio: false,
          plugins: {
            legend: {
              position: 'top',
              align: 'start' // 👈 범례 오른쪽 정렬
            },
            tooltip: {
              callbacks: {
                label: ctx => `${ctx.label}: ${ctx.parsed}건`
              }
            }
          }
        }
      });
    },
    drawStatusChart() {
      const ctx = this.$refs.statusChartCanvas.getContext('2d');
      new Chart(ctx, {
        type: 'bar',
        data: {
          labels: ['미처리', '진행중', '보류', 'SR', '종결'],
          datasets: [{
            label: '건수',
            data: [15, 25, 10, 8, 42],
            backgroundColor: '#42A5F5'
          }]
        },
        options: {
          responsive: true,
          maintainAspectRatio: false,
          scales: { y: { beginAtZero: true } },
          plugins: {
            legend: { display: false },
            tooltip: {
              callbacks: {
                label: ctx => `${ctx.label}: ${ctx.parsed.y}건`
              }
            }
          }
        }
      });
    },
    drawMonthlyChart() {
      const ctx = this.$refs.monthlyChartCanvas.getContext('2d');
      new Chart(ctx, {
        type: 'bar',
        data: {
          labels: ['1월', '2월', '3월', '4월', '5월', '6월', '7월', '8월', '9월', '10월', '11월', '12월'],
          datasets: [{
            label: '작성 건수',
            data: [5, 8, 12, 20, 14, 10, 7, 6, 9, 11, 4, 3],
            backgroundColor: '#66BB6A'
          }]
        },
        options: {
          responsive: true,
          maintainAspectRatio: false,
          scales: { y: { beginAtZero: true } },
          plugins: {
            legend: { display: false },
            tooltip: {
              callbacks: {
                label: ctx => `${ctx.label}: ${ctx.parsed.y}건`
              }
            }
          }
        }
      });
    },
    editItem(item) {
      alert(`"${item.title}" 항목을 수정합니다.`);
    }
  }
};
</script>

<style scoped>
.chart-container {
  position: relative;
  height: 240px;
  display: flex;
  justify-content: center;
  align-items: center;
}

.custom-btn-toggle {
  display: inline-flex;
  border-radius: 6px;
  overflow: hidden;
}

.custom-btn-toggle .v-btn {
  background-color: rgba(25, 118, 210, 0.07);
  color: #1976D2;
  border-radius: 0;
  min-width: 120px;
  font-weight: 500;
  box-shadow: none !important;
}

.custom-btn-toggle .v-btn.selected-btn {
  background-color: #1976D2;
  color: white;
}
</style>
