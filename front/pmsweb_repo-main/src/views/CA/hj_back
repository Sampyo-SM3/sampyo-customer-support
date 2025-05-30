<template>
    <v-app>
      <v-main>
        <v-container fluid class="pa-6">
          <!-- 헤더 -->
          <div class="d-flex justify-space-between align-center mb-6">
            <div>
              <h1 class="text-h4 font-weight-bold primary--text">업무요청 통계 현황</h1>
              <p class="text-subtitle-1 grey--text">실시간 업무처리 현황 및 분석 리포트</p>
            </div>
            <v-chip color="success" text-color="white">
              <v-icon left>mdi-update</v-icon>
              실시간 업데이트
            </v-chip>
          </div>
  
          <!-- 필터 및 검색 영역 -->
          <v-card class="mb-6 elevation-2">
            <v-card-title class="pb-2">
              <v-icon left color="primary">mdi-filter-variant</v-icon>
              조회 조건
            </v-card-title>
            <v-card-text>
              <v-row>
                <v-col cols="12" md="2">
                  <v-select
                    v-model="selectedPeriod"
                    :items="periods"
                    label="기간"
                    outlined
                    dense
                    prepend-inner-icon="mdi-calendar"
                  />
                </v-col>
                <v-col cols="12" md="3">
                  <v-autocomplete
                    v-model="selectedCompanies"
                    :items="companies"
                    label="법인 선택"
                    multiple
                    chips
                    outlined
                    dense
                    prepend-inner-icon="mdi-office-building"
                  />
                </v-col>
                <v-col cols="12" md="2">
                  <v-select
                    v-model="selectedStatuses"
                    :items="statuses"
                    label="처리상태"
                    multiple
                    outlined
                    dense
                    prepend-inner-icon="mdi-clipboard-list"
                  />
                </v-col>
                <v-col cols="12" md="2">
                  <v-select
                    v-model="selectedCategories"
                    :items="categories"
                    label="문의유형"
                    multiple
                    outlined
                    dense
                    prepend-inner-icon="mdi-tag"
                  />
                </v-col>
                <v-col cols="12" md="2">
                  <v-select
                    v-model="selectedPriority"
                    :items="priorities"
                    label="우선순위"
                    multiple
                    outlined
                    dense
                    prepend-inner-icon="mdi-alert"
                  />
                </v-col>
                <v-col cols="12" md="1">
                  <v-btn color="primary" block @click="applyFilters">
                    <v-icon>mdi-magnify</v-icon>
                  </v-btn>
                </v-col>
              </v-row>
            </v-card-text>
          </v-card>
  
          <!-- 핵심 지표 카드 영역 -->
          <v-row class="mb-6">
            <v-col v-for="(metric, index) in keyMetrics" :key="index" cols="12" sm="6" md="3">
              <v-card class="text-center elevation-3" :color="metric.color" dark>
                <v-card-text>
                  <v-icon size="48" class="mb-2">{{ metric.icon }}</v-icon>
                  <div class="text-h3 font-weight-bold">{{ metric.value }}</div>
                  <div class="text-subtitle-1">{{ metric.title }}</div>
                  <div class="text-caption">
                    <v-icon small :color="metric.trend > 0 ? 'green' : 'red'">
                      {{ metric.trend > 0 ? 'mdi-trending-up' : 'mdi-trending-down' }}
                    </v-icon>
                    {{ Math.abs(metric.trend) }}% 전월 대비
                  </div>
                </v-card-text>
              </v-card>
            </v-col>
          </v-row>
  
          <!-- 실시간 알림 영역 -->
          <v-row class="mb-6">
            <v-col cols="12">
              <v-card class="elevation-2">
                <v-card-title class="pb-2">
                  <v-icon left color="warning">mdi-bell-ring</v-icon>
                  실시간 알림
                  <v-spacer></v-spacer>
                  <v-chip small color="error" text-color="white">{{ urgentCount }}건 긴급</v-chip>
                </v-card-title>
                <v-card-text>
                  <v-timeline dense>
                    <v-timeline-item
                      v-for="(alert, index) in realtimeAlerts"
                      :key="index"
                      :color="alert.color"
                      small
                    >
                      <v-row>
                        <v-col cols="8">
                          <strong>{{ alert.company }}</strong> - {{ alert.message }}
                        </v-col>
                        <v-col cols="4" class="text-right">
                          <small class="grey--text">{{ alert.time }}</small>
                        </v-col>
                      </v-row>
                    </v-timeline-item>
                  </v-timeline>
                </v-card-text>
              </v-card>
            </v-col>
          </v-row>
  
          <!-- 메인 차트 영역 -->
          <v-row class="mb-6">
            <v-col cols="12" md="8">
              <v-card class="elevation-3">
                <v-card-title>
                  <v-icon left color="primary">mdi-chart-line</v-icon>
                  월별 처리 트렌드
                </v-card-title>
                <v-card-text>
                  <canvas ref="trendChart" style="height: 400px;"></canvas>
                </v-card-text>
              </v-card>
            </v-col>
            <v-col cols="12" md="4">
              <v-card class="elevation-3 mb-4">
                <v-card-title>
                  <v-icon left color="success">mdi-chart-donut</v-icon>
                  법인별 점유율
                </v-card-title>
                <v-card-text>
                  <canvas ref="companyChart" style="height: 250px;"></canvas>
                </v-card-text>
              </v-card>
              <v-card class="elevation-3">
                <v-card-title>
                  <v-icon left color="info">mdi-speedometer</v-icon>
                  처리 성과 지표
                </v-card-title>
                <v-card-text>
                  <div v-for="(kpi, index) in performanceKPIs" :key="index" class="mb-3">
                    <div class="d-flex justify-space-between mb-1">
                      <span>{{ kpi.title }}</span>
                      <span class="font-weight-bold">{{ kpi.value }}%</span>
                    </div>
                    <v-progress-linear
                      :value="kpi.value"
                      :color="kpi.color"
                      height="8"
                      rounded
                    ></v-progress-linear>
                  </div>
                </v-card-text>
              </v-card>
            </v-col>
          </v-row>
  
          <!-- 상태별 분석 -->
          <v-row class="mb-6">
            <v-col cols="12" md="6">
              <v-card class="elevation-3">
                <v-card-title>
                  <v-icon left color="warning">mdi-chart-bar</v-icon>
                  처리상태별 현황
                </v-card-title>
                <v-card-text>
                  <canvas ref="statusChart" style="height: 300px;"></canvas>
                </v-card-text>
              </v-card>
            </v-col>
            <v-col cols="12" md="6">
              <v-card class="elevation-3">
                <v-card-title>
                  <v-icon left color="purple">mdi-tag-multiple</v-icon>
                  문의유형별 분포
                </v-card-title>
                <v-card-text>
                  <canvas ref="categoryChart" style="height: 300px;"></canvas>
                </v-card-text>
              </v-card>
            </v-col>
          </v-row>
  
          <!-- 처리시간 분석 -->
          <v-row class="mb-6">
            <v-col cols="12">
              <v-card class="elevation-3">
                <v-card-title>
                  <v-icon left color="deep-orange">mdi-clock-outline</v-icon>
                  처리시간 분석
                  <v-spacer></v-spacer>
                  <v-btn-toggle v-model="timeAnalysisView" mandatory>
                    <v-btn small>일별</v-btn>
                    <v-btn small>주별</v-btn>
                    <v-btn small>월별</v-btn>
                  </v-btn-toggle>
                </v-card-title>
                <v-card-text>
                  <v-row>
                    <v-col cols="12" md="8">
                      <canvas ref="processingTimeChart" style="height: 350px;"></canvas>
                    </v-col>
                    <v-col cols="12" md="4">
                      <v-list>
                        <v-list-item-group>
                          <v-list-item v-for="(time, index) in processingTimes" :key="index">
                            <v-list-item-icon>
                              <v-icon :color="time.color">{{ time.icon }}</v-icon>
                            </v-list-item-icon>
                            <v-list-item-content>
                              <v-list-item-title>{{ time.category }}</v-list-item-title>
                              <v-list-item-subtitle>평균 {{ time.avgTime }}시간</v-list-item-subtitle>
                            </v-list-item-content>
                            <v-list-item-action>
                              <v-chip small :color="time.performance">{{ time.grade }}</v-chip>
                            </v-list-item-action>
                          </v-list-item>
                        </v-list-item-group>
                      </v-list>
                    </v-col>
                  </v-row>
                </v-card-text>
              </v-card>
            </v-col>
          </v-row>
  
          <!-- 담당자별 성과 -->
          <v-row class="mb-6">
            <v-col cols="12">
              <v-card class="elevation-3">
                <v-card-title>
                  <v-icon left color="teal">mdi-account-group</v-icon>
                  담당자별 처리 성과
                </v-card-title>
                <v-card-text>
                  <v-data-table
                    :headers="performanceHeaders"
                    :items="performanceData"
                    :items-per-page="10"
                    class="elevation-1"
                  >
                    <template #[`item.performance`]="{ item }">
                      <v-progress-linear
                        :value="item.performance"
                        :color="getPerformanceColor(item.performance)"
                        height="20"
                        rounded
                      >
                        <strong>{{ item.performance }}%</strong>
                      </v-progress-linear>
                    </template>
                    <template #[`item.satisfaction`]="{ item }">
                      <v-rating
                        :value="item.satisfaction"
                        color="yellow darken-3"
                        background-color="grey darken-1"
                        half-increments
                        readonly
                        small
                      ></v-rating>
                    </template>
                    <template #[`item.trend`]="{ item }">
                      <v-chip
                        small
                        :color="item.trend > 0 ? 'success' : item.trend < 0 ? 'error' : 'warning'"
                        :text-color="item.trend === 0 ? 'black' : 'white'"
                      >
                        <v-icon small left>
                          {{ item.trend > 0 ? 'mdi-trending-up' : item.trend < 0 ? 'mdi-trending-down' : 'mdi-trending-neutral' }}
                        </v-icon>
                        {{ item.trend > 0 ? '+' : '' }}{{ item.trend }}%
                      </v-chip>
                    </template>
                  </v-data-table>
                </v-card-text>
              </v-card>
            </v-col>
          </v-row>
  
          <!-- 법인별 상세 현황 -->
          <v-row class="mb-6">
            <v-col cols="12">
              <v-card class="elevation-3">
                <v-card-title>
                  <v-icon left color="indigo">mdi-office-building</v-icon>
                  법인별 상세 현황
                  <v-spacer></v-spacer>
                  <v-text-field
                    v-model="companySearch"
                    append-icon="mdi-magnify"
                    label="법인 검색"
                    single-line
                    hide-details
                    dense
                    outlined
                    style="max-width: 300px;"
                  ></v-text-field>
                </v-card-title>
                <v-card-text>
                  <v-data-table
                    :headers="companyHeaders"
                    :items="companyData"
                    :search="companySearch"
                    :items-per-page="15"
                    class="elevation-1"
                  >
                    <template #[`item.completionRate`]="{ item }">
                      <div class="d-flex align-center">
                        <v-progress-circular
                          :value="item.completionRate"
                          :color="getCompletionColor(item.completionRate)"
                          size="40"
                          width="4"
                        >
                          <span class="caption">{{ item.completionRate }}%</span>
                        </v-progress-circular>
                      </div>
                    </template>
                    <template #[`item.urgentCount`]="{ item }">
                      <v-chip
                        small
                        :color="item.urgentCount > 5 ? 'error' : item.urgentCount > 2 ? 'warning' : 'success'"
                        :text-color="item.urgentCount <= 2 ? 'black' : 'white'"
                      >
                        {{ item.urgentCount }}건
                      </v-chip>
                    </template>
                    <template #[`item.avgProcessTime`]="{ item }">
                      <span :class="getProcessTimeClass(item.avgProcessTime)">
                        {{ item.avgProcessTime }}시간
                      </span>
                    </template>
                    <template #[`item.actions`]="{ item }">
                      <v-btn icon small @click="viewDetails(item)">
                        <v-icon small>mdi-eye</v-icon>
                      </v-btn>
                      <v-btn icon small @click="exportReport(item)">
                        <v-icon small>mdi-download</v-icon>
                      </v-btn>
                    </template>
                  </v-data-table>
                </v-card-text>
              </v-card>
            </v-col>
          </v-row>
  
          <!-- 예측 분석 -->
          <v-row class="mb-6">
            <v-col cols="12" md="8">
              <v-card class="elevation-3">
                <v-card-title>
                  <v-icon left color="cyan">mdi-crystal-ball</v-icon>
                  AI 예측 분석
                </v-card-title>
                <v-card-text>
                  <canvas ref="predictionChart" style="height: 350px;"></canvas>
                </v-card-text>
              </v-card>
            </v-col>
            <v-col cols="12" md="4">
              <v-card class="elevation-3 mb-4">
                <v-card-title>
                  <v-icon left color="amber">mdi-lightbulb</v-icon>
                  인사이트
                </v-card-title>
                <v-card-text>
                  <v-alert
                    v-for="(insight, index) in insights"
                    :key="index"
                    :type="insight.type"
                    text
                    dense
                    class="mb-2"
                  >
                    {{ insight.message }}
                  </v-alert>
                </v-card-text>
              </v-card>
              <v-card class="elevation-3">
                <v-card-title>
                  <v-icon left color="green">mdi-target</v-icon>
                  개선 제안
                </v-card-title>
                <v-card-text>
                  <v-list dense>
                    <v-list-item v-for="(suggestion, index) in suggestions" :key="index">
                      <v-list-item-icon>
                        <v-icon small color="success">mdi-check-circle</v-icon>
                      </v-list-item-icon>
                      <v-list-item-content>
                        <v-list-item-title class="caption">{{ suggestion }}</v-list-item-title>
                      </v-list-item-content>
                    </v-list-item>
                  </v-list>
                </v-card-text>
              </v-card>
            </v-col>
          </v-row>
  
          <!-- 액션 버튼 -->
          <v-row>
            <v-col cols="12" class="text-center">
              <v-btn color="primary" large class="mx-2" @click="exportFullReport">
                <v-icon left>mdi-file-excel</v-icon>
                전체 리포트 내보내기
              </v-btn>
              <v-btn color="success" large class="mx-2" @click="scheduleReport">
                <v-icon left>mdi-calendar-plus</v-icon>
                정기 리포트 설정
              </v-btn>
              <v-btn color="info" large class="mx-2" @click="setAlerts">
                <v-icon left>mdi-bell-plus</v-icon>
                알림 설정
              </v-btn>
            </v-col>
          </v-row>
        </v-container>
      </v-main>
    </v-app>
  </template>
  
  <script>
  export default {
    name: 'BusinessDashboard',
    data() {
      return {
        // 필터 옵션
        selectedPeriod: '최근 3개월',
        selectedCompanies: [],
        selectedStatuses: [],
        selectedCategories: [],
        selectedPriority: [],
        companySearch: '',
        timeAnalysisView: 0,
        
        periods: [
          '최근 1주일', '최근 1개월', '최근 3개월', 
          '최근 6개월', '최근 1년', '사용자 정의'
        ],
        
        companies: [
          '(주)삼표', '삼표시멘트', '삼표레미콘', '삼표이앤씨',
          '삼표산업', '삼표건설', '삼표물류', '기타'
        ],
        
        statuses: [
          '접수대기', '진행중', '검토중', '완료', '보류', '취소'
        ],
        
        categories: [
          'IT지원', '인사', '총무', '회계', '법무', '구매', '기타'
        ],
        
        priorities: [
          '긴급', '높음', '보통', '낮음'
        ],
  
        // 핵심 지표
        keyMetrics: [
          {
            title: '총 문의건수',
            value: '1,247',
            icon: 'mdi-file-document-multiple',
            color: 'primary',
            trend: 8.5
          },
          {
            title: '처리완료',
            value: '892',
            icon: 'mdi-check-circle',
            color: 'success',
            trend: 12.3
          },
          {
            title: '평균처리시간',
            value: '2.1일',
            icon: 'mdi-clock-fast',
            color: 'info',
            trend: -5.2
          },
          {
            title: '만족도',
            value: '4.2/5',
            icon: 'mdi-star',
            color: 'warning',
            trend: 3.1
          }
        ],
  
        // 실시간 알림
        urgentCount: 3,
        realtimeAlerts: [
          {
            company: '삼표시멘트',
            message: '긴급 IT지원 요청 접수',
            time: '2분 전',
            color: 'red'
          },
          {
            company: '삼표레미콘',
            message: '인사 관련 문의 처리 완료',
            time: '5분 전',
            color: 'green'
          },
          {
            company: '삼표이앤씨',
            message: '회계 승인 대기중',
            time: '8분 전',
            color: 'orange'
          }
        ],
  
        // 성과 지표
        performanceKPIs: [
          { title: '목표 달성률', value: 87, color: 'success' },
          { title: 'SLA 준수율', value: 92, color: 'primary' },
          { title: '고객 만족도', value: 84, color: 'warning' },
          { title: '재문의 비율', value: 15, color: 'error' }
        ],
  
        // 처리시간 분석
        processingTimes: [
          { category: 'IT지원', avgTime: 1.5, color: 'blue', icon: 'mdi-monitor', grade: 'A', performance: 'success' },
          { category: '인사', avgTime: 2.8, color: 'green', icon: 'mdi-account-group', grade: 'B', performance: 'warning' },
          { category: '총무', avgTime: 1.2, color: 'purple', icon: 'mdi-briefcase', grade: 'A+', performance: 'success' },
          { category: '회계', avgTime: 4.1, color: 'orange', icon: 'mdi-calculator', grade: 'C', performance: 'error' },
          { category: '법무', avgTime: 6.2, color: 'red', icon: 'mdi-gavel', grade: 'D', performance: 'error' }
        ],
  
        // 담당자 성과 테이블
        performanceHeaders: [
          { text: '담당자', value: 'name' },
          { text: '소속', value: 'department' },
          { text: '처리건수', value: 'processed' },
          { text: '완료율', value: 'performance' },
          { text: '평균처리시간', value: 'avgTime' },
          { text: '만족도', value: 'satisfaction' },
          { text: '성장률', value: 'trend' }
        ],
  
        performanceData: [
          {
            name: '김민수',
            department: 'IT팀',
            processed: 156,
            performance: 94,
            avgTime: '1.2일',
            satisfaction: 4.5,
            trend: 8
          },
          {
            name: '이영희',
            department: '인사팀',
            processed: 132,
            performance: 87,
            avgTime: '2.1일',
            satisfaction: 4.2,
            trend: -2
          },
          {
            name: '박철수',
            department: '총무팀',
            processed: 198,
            performance: 91,
            avgTime: '1.8일',
            satisfaction: 4.3,
            trend: 12
          }
        ],
  
        // 법인별 상세 현황
        companyHeaders: [
          { text: '법인명', value: 'name' },
          { text: '총 문의', value: 'totalRequests' },
          { text: '완료율', value: 'completionRate' },
          { text: '긴급건', value: 'urgentCount' },
          { text: '평균처리시간', value: 'avgProcessTime' },
          { text: '만족도', value: 'satisfaction' },
          { text: '액션', value: 'actions', sortable: false }
        ],
  
        companyData: [
          {
            name: '(주)삼표',
            totalRequests: 245,
            completionRate: 89,
            urgentCount: 3,
            avgProcessTime: 2.1,
            satisfaction: 4.3
          },
          {
            name: '삼표시멘트',
            totalRequests: 198,
            completionRate: 92,
            urgentCount: 1,
            avgProcessTime: 1.8,
            satisfaction: 4.5
          },
          {
            name: '삼표레미콘',
            totalRequests: 167,
            completionRate: 85,
            urgentCount: 5,
            avgProcessTime: 2.8,
            satisfaction: 4.1
          }
        ],
  
        // 인사이트
        insights: [
          {
            type: 'info',
            message: 'IT지원 요청이 전월 대비 15% 증가했습니다.'
          },
          {
            type: 'warning',
            message: '회계 관련 문의의 처리시간이 평균보다 길어지고 있습니다.'
          },
          {
            type: 'success',
            message: '고객 만족도가 지속적으로 향상되고 있습니다.'
          }
        ],
  
        // 개선 제안
        suggestions: [
          '회계팀 인력 충원을 통한 처리시간 단축',
          'IT지원 FAQ 개선으로 반복 문의 감소',
          '긴급 문의 전용 채널 신설',
          '담당자별 업무량 균등 배분',
          '고객 피드백 시스템 강화'
        ]
      }
    },
  
    methods: {
      applyFilters() {
        console.log('필터 적용');
      },
      
      getPerformanceColor(value) {
        if (value >= 90) return 'success';
        if (value >= 80) return 'warning';
        return 'error';
      },
      
      getCompletionColor(value) {
        if (value >= 90) return 'success';
        if (value >= 80) return 'primary';
        return 'warning';
      },
      
      getProcessTimeClass(time) {
        if (time <= 2) return 'success--text';
        if (time <= 3) return 'warning--text';
        return 'error--text';
      },
      
      viewDetails(item) {
        console.log('상세보기:', item);
      },
      
      exportReport(item) {
        console.log('리포트 내보내기:', item);
      },
      
      exportFullReport() {
        console.log('전체 리포트 내보내기');
      },
      
      scheduleReport() {
        console.log('정기 리포트 설정');
      },
      
      setAlerts() {
        console.log('알림 설정');
      }
    }
  }
  </script>
  
  <style scoped>
  .v-card {
    transition: all 0.3s ease;
  }
  
  .v-card:hover {
    transform: translateY(-2px);
  }
  
  .elevation-1 {
    box-shadow: 0 2px 4px rgba(0,0,0,0.1) !important;
  }
  
  .elevation-2 {
    box-shadow: 0 4px 8px rgba(0,0,0,0.12) !important;
  }
  
  .elevation-3 {
    box-shadow: 0 6px 12px rgba(0,0,0,0.15) !important;
  }
  </style>