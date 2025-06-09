<template>
  <v-container fluid class="pr-0 pl-0 pt-4">
    <!-- 모든 필터 표시 -->
    <DynamicSearchFilter
      :showManagerFilter="false"
      :showStatusFilter="false"
      :showTitleFilter="false"
      :showCompanyFilter="true"
      @search="onSearch"
    />       

    <!-- 핵심 지표 카드 영역 -->
    <v-row class="mb-6">
      <v-col v-for="(metric, index) in keyMetrics" :key="index" cols="12" sm="6" md="3">
        <v-card class="text-center elevation-3" :color="metric.color" dark>
          <v-card-text>
            <v-icon size="48" class="mb-2">{{ metric.icon }}</v-icon>
            <div class="text-h3 font-weight-bold">{{ metric.value }}</div>
            <div class="text-subtitle-1">{{ metric.title }}</div>
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
            <canvas ref="trendChart" style="height: 500px;"></canvas>
          </v-card-text>
        </v-card>
      </v-col>

      <v-col cols="12" md="4" class="text-center">
        개발 중
      </v-col> 

      <!-- <v-col cols="12" md="4">
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
      </v-col> -->
    </v-row>

    <!-- 상태별 분석 -->
    <!-- <v-row class="mb-6">
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
    </v-row> -->

    <!-- 처리시간 분석 -->
    <!-- <v-row class="mb-6">
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
    </v-row> -->

    <!-- 법인별 상세 현황 -->
    <!-- <v-row class="mb-6">
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
    </v-row>       -->

    <!-- 액션 버튼 -->
    <!-- <v-row>
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

      
    </v-row> -->

    
  </v-container>
</template>

<script>
import qs from 'qs'
import { ref, reactive, onMounted, onUnmounted, nextTick } from 'vue'
import DynamicSearchFilter from '@/components/DynamicSearchFilter'
import apiClient from '@/api'
import {      
  Chart,
  DoughnutController,
  BarController,
  LineController,
  PointElement,
  LineElement,
  BarElement,
  ArcElement,
  Tooltip,
  Legend,
  CategoryScale,
  LinearScale
} from 'chart.js'

Chart.register(
  DoughnutController,
  BarController,
  LineController,
  PointElement,
  LineElement,
  BarElement,
  ArcElement,
  Tooltip,
  Legend,
  CategoryScale,
  LinearScale
)

export default {
  name: 'BusinessDashboard',
  components: {
    DynamicSearchFilter,
  },
  setup() {
    // 반응형 상태 정의
    const loading = ref(false)
    // const trendChartInstance = ref(null)
    const trendChart = ref(null)
    const companyChart = ref(null)
    const statusChart = ref(null)
    const categoryChart = ref(null)
    const processingTimeChart = ref(null)

    // 차트 인스턴스들을 관리하는 reactive 객체
    const chartInstances = reactive({
      trend: null,
      company: null,
      status: null,
      category: null,
      processingTime: null
    })

    // 필터 옵션
    const selectedPeriod = ref('최근 3개월')
    const selectedCompanies = ref([])
    const selectedStatuses = ref([])
    const selectedCategories = ref([])
    const selectedPriority = ref([])
    const companySearch = ref('')
    const timeAnalysisView = ref(0)

    const periods = ref([
      '최근 1주일', '최근 1개월', '최근 3개월', 
      '최근 6개월', '최근 1년', '사용자 정의'
    ])

    const companies = ref([
      '(주)삼표', '삼표시멘트', '삼표레미콘', '삼표이앤씨',
      '삼표산업', '삼표건설', '삼표물류', '기타'
    ])

    const statuses = ref([
      '접수대기', '진행중', '검토중', '완료', '보류', '취소'
    ])

    const categories = ref([
      'IT지원', '인사', '총무', '회계', '법무', '구매', '기타'
    ])

    const priorities = ref([
      '긴급', '높음', '보통', '낮음'
    ])

    // 핵심 지표
    const keyMetrics = reactive([
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
        title: '처리완료율',
        value: '4.2/5',
        icon: 'mdi-progress-check',
        color: 'warning',
        trend: 3.1
      },
      {
        title: '평균처리시간',
        value: '2.1일',
        icon: 'mdi-clock-fast',
        color: 'info',
        trend: -5.2
      }          
    ])

    // 월별 트렌드 차트 데이터
    const monthlyTrendData = reactive({
      labels: ['1월', '2월', '3월', '4월', '5월', '6월', '7월', '8월', '9월', '10월', '11월', '12월'],
      datasets: [
        {
          label: '총 문의건수',
          data: [120, 98, 145, 167, 189, 234, 198, 156, 178, 203, 189, 247],
          borderColor: '#1976D2',
          backgroundColor: 'rgba(25, 118, 210, 0.1)',
          borderWidth: 3,
          fill: true,
          tension: 0.4,
          pointBackgroundColor: '#1976D2',
          pointBorderColor: '#fff',
          pointBorderWidth: 2,
          pointRadius: 6,
          pointHoverRadius: 8
        },
        {
          label: '처리완료',
          data: [98, 89, 134, 145, 167, 198, 178, 134, 156, 189, 167, 198],
          borderColor: '#388E3C',
          backgroundColor: 'rgba(56, 142, 60, 0.1)',
          borderWidth: 3,
          fill: true,
          tension: 0.4,
          pointBackgroundColor: '#388E3C',
          pointBorderColor: '#fff',
          pointBorderWidth: 2,
          pointRadius: 6,
          pointHoverRadius: 8
        },
        {
          label: '진행중',
          data: [15, 6, 8, 16, 15, 25, 12, 15, 14, 8, 15, 32],
          borderColor: '#F57C00',
          backgroundColor: 'rgba(245, 124, 0, 0.1)',
          borderWidth: 3,
          fill: true,
          tension: 0.4,
          pointBackgroundColor: '#F57C00',
          pointBorderColor: '#fff',
          pointBorderWidth: 2,
          pointRadius: 6,
          pointHoverRadius: 8
        },
        {
          label: '보류',
          data: [7, 3, 3, 6, 7, 11, 8, 7, 8, 6, 7, 17],
          borderColor: '#D32F2F',
          backgroundColor: 'rgba(211, 47, 47, 0.1)',
          borderWidth: 3,
          fill: true,
          tension: 0.4,
          pointBackgroundColor: '#D32F2F',
          pointBorderColor: '#fff',
          pointBorderWidth: 2,
          pointRadius: 6,
          pointHoverRadius: 8
        }
      ]
    })

    // 실시간 알림
    const urgentCount = ref(3)
    const realtimeAlerts = reactive([
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
    ])

    // 성과 지표
    const performanceKPIs = reactive([
      { title: '목표 달성률', value: 87, color: 'success' },
      { title: 'SLA 준수율', value: 92, color: 'primary' },
      { title: '고객 만족도', value: 84, color: 'warning' },
      { title: '재문의 비율', value: 15, color: 'error' }
    ])

    // 처리시간 분석
    const processingTimes = reactive([
      { category: 'IT지원', avgTime: 1.5, color: 'blue', icon: 'mdi-monitor', grade: 'A', performance: 'success' },
      { category: '인사', avgTime: 2.8, color: 'green', icon: 'mdi-account-group', grade: 'B', performance: 'warning' },
      { category: '총무', avgTime: 1.2, color: 'purple', icon: 'mdi-briefcase', grade: 'A+', performance: 'success' },
      { category: '회계', avgTime: 4.1, color: 'orange', icon: 'mdi-calculator', grade: 'C', performance: 'error' },
      { category: '법무', avgTime: 6.2, color: 'red', icon: 'mdi-gavel', grade: 'D', performance: 'error' }
    ])

    // 담당자 성과 테이블
    const performanceHeaders = reactive([
      { text: '담당자', value: 'name' },
      { text: '소속', value: 'department' },
      { text: '처리건수', value: 'processed' },
      { text: '완료율', value: 'performance' },
      { text: '평균처리시간', value: 'avgTime' },
      { text: '만족도', value: 'satisfaction' },
      { text: '성장률', value: 'trend' }
    ])

    const performanceData = reactive([
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
    ])

    // 법인별 상세 현황
    const companyHeaders = reactive([
      { text: '법인명', value: 'name' },
      { text: '총 문의', value: 'totalRequests' },
      { text: '완료율', value: 'completionRate' },
      { text: '긴급건', value: 'urgentCount' },
      { text: '평균처리시간', value: 'avgProcessTime' },
      { text: '만족도', value: 'satisfaction' },
      { text: '액션', value: 'actions', sortable: false }
    ])

    const companyData = reactive([
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
    ])

    // 인사이트
    const insights = reactive([
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
    ])

    // 개선 제안
    const suggestions = reactive([
      '회계팀 인력 충원을 통한 처리시간 단축',
      'IT지원 FAQ 개선으로 반복 문의 감소',
      '긴급 문의 전용 채널 신설',
      '담당자별 업무량 균등 배분',
      '고객 피드백 시스템 강화'
    ])
    
    const onSearch = async (searchParams) => {
      // 이미 로딩 중이면 무시 (중복 실행 방지)
      if (loading.value) return
      
      // console.log('--onSearch--')
      // console.log(searchParams)

      await fetchDashboardData(searchParams)
      
      // 로딩 완료 후 별도로 차트 업데이트 (searchParams 전달)
      await nextTick()
      setTimeout(() => {
        if (!loading.value) { // 로딩이 완전히 끝난 후에만 실행
          updateAllCharts(searchParams)
        }
      }, 300)
    }

    const fetchDashboardData = async (searchParams) => {
      loading.value = true
        
      // 최종 전달될 파라미터 확인
      const finalParams = {
        ...searchParams,
        // dpId: JSON.parse(localStorage.getItem("userInfo"))?.deptCd || null
      }
      // console.log('최종 파라미터:', finalParams)
            
      try {
        // 서버 측 페이징을 구현할 경우 페이지 관련 파라미터 추가        
        const response = await apiClient.get('/api/require/dashboard/card', {
          params: finalParams
        })
        // console.log(response)
        
        // API 응답 데이터가 있는 경우 keyMetrics 업데이트
        if (response.data && response.data.length > 0) {
          const stats = response.data[0]
          
          // keyMetrics 값 업데이트
          keyMetrics[0].value = stats.totalCount?.toLocaleString() || '0'
          keyMetrics[1].value = stats.completeCount?.toLocaleString() || '0'
          keyMetrics[2].value = stats.completeRate ? `${stats.completeRate}%` : '0%'
          keyMetrics[3].value = stats.avgProcessDays ? `${stats.avgProcessDays}일` : '0일'

          // 만약 API에서 월별 트렌드 데이터도 받아온다면
          // if (stats.monthlyTrend) {
          //   Object.assign(monthlyTrendData, stats.monthlyTrend)
          // }
        }
        
      } catch (error) {
        console.error('데이터 로드 중 오류 발생:', error)
      } finally {
        loading.value = false
      }
    }    

    // 월별 트렌드 데이터를 가져오는 함수
    const fetchMonthlyTrendData = async (searchParams) => {
      loading.value = true
        
      try {
        // 파라미터 설정        
        const finalParams = {
          ...searchParams,          
        }
        
        const finalParams_c = {
          ...searchParams,
          status: 'C'          
        }
        
        const finalParams_p = {
          ...searchParams,
          inStatus: ['I', 'P', 'S'] 
        }

        const finalParams_h = {
          ...searchParams,
          status: 'H'          
        }        
              
        // 세 API를 병렬로 호출
        const [response, response2, response3, response4] = await Promise.all([          
          apiClient.get('/api/require/dashboard/monthly-total', { params: finalParams }),
          apiClient.get('/api/require/dashboard/monthly-total', { params: finalParams_c }),
          apiClient.get('/api/require/dashboard/monthly-total', {
            params: finalParams_p,
            paramsSerializer: params => qs.stringify(params, { indices: false })
          }),
          apiClient.get('/api/require/dashboard/monthly-total', { params: finalParams_h }),
        ])
        
        // console.log('월별 트렌드 데이터:', response)
        // console.log('월별 트렌드 데이터_처리완료:', response2)
        // console.log('월별 트렌드 데이터_진행중:', response3)
        // console.log('월별 트렌드 데이터_보류류:', response4)

        // 데이터 처리
        if (response.data && response.data.length > 0) {
          const trendData = response.data
          
          // 월별 라벨 생성 (month_name 사용)
          const labels = trendData.map(item => item.month_name)
          
          // 총 문의건수 데이터 생성 (total_count 사용)
          const totalRequests = trendData.map(item => item.total_count || 0)
          
          // 처리완료 데이터 매핑 (월별로 매칭)
          const completedMap = new Map()
          if (response2.data && response2.data.length > 0) {
            response2.data.forEach(item => {
              completedMap.set(item.month_name, item.total_count || 0)
            })
          }
          
          // 진행중 데이터 매핑 (월별로 매칭)
          const progressMap = new Map()
          if (response3.data && response3.data.length > 0) {
            response3.data.forEach(item => {
              progressMap.set(item.month_name, item.total_count || 0)
            })
          }

          // 보류류 데이터 매핑 (월별로 매칭)
          const pendingMap = new Map()
          if (response4.data && response4.data.length > 0) {
            response4.data.forEach(item => {
              pendingMap.set(item.month_name, item.total_count || 0)
            })
          }          
          
          // 전체 데이터와 동일한 순서로 데이터 생성
          const completeRequests = labels.map(monthName => {
            return completedMap.get(monthName) || 0
          })
          
          const progressRequests = labels.map(monthName => {
            return progressMap.get(monthName) || 0
          })

          const pendingRequests = labels.map(monthName => {
            return pendingMap.get(monthName) || 0
          })          
          
          // monthlyTrendData 업데이트
          monthlyTrendData.labels = labels
          monthlyTrendData.datasets[0].data = totalRequests
          monthlyTrendData.datasets[1].data = completeRequests
          monthlyTrendData.datasets[2].data = progressRequests
          monthlyTrendData.datasets[3].data = pendingRequests                                        
        }
        
      } catch (error) {
        console.error('월별 트렌드 데이터 로드 중 오류 발생:', error)
      } finally {
        loading.value = false
      }
    }

    // 모든 차트 안전하게 제거하는 함수
    const destroyAllCharts = () => {
      try {
        Object.keys(chartInstances).forEach(key => {
          if (chartInstances[key] && chartInstances[key].destroy) {
            try {
              // 차트가 아직 활성 상태인지 확인
              if (!chartInstances[key].destroyed) {
                chartInstances[key].destroy()
              }
            } catch (e) {
              console.warn(`차트 ${key} 정리 중 경고:`, e)
            }
            chartInstances[key] = null
          }
        })
      } catch (error) {
        console.error('차트 제거 중 오류:', error)
      }
    }

    // 모든 차트 업데이트
    const updateAllCharts = (searchParams = {}) => {
      try {
        // 먼저 모든 차트 정리
        destroyAllCharts()
        
        // DOM이 준비되었는지 확인
        if (!trendChart.value) {
          console.warn('차트 캔버스 요소가 아직 준비되지 않았습니다.')
          return
        }
        
        // 순차적으로 차트 생성 (타이밍 분산)
        setTimeout(() => initTrendChart(searchParams), 50)
        
      } catch (error) {
        console.error('차트 업데이트 중 오류:', error)
      }
    }

    // 월별 트렌드 차트 초기화
    const initTrendChart = async (searchParams = {}) => {
      try {
        // 먼저 월별 트렌드 데이터 가져오기
        await fetchMonthlyTrendData(searchParams)
        
        // canvas 요소와 컨텍스트가 유효한지 확인
        if (!trendChart.value) {
          console.warn('Canvas element not found')
          return
        }

        const ctx = trendChart.value.getContext('2d')
        if (!ctx) {
          console.warn('Canvas context not available')
          return
        }

        // 기존 차트가 있다면 제거
        if (chartInstances.trend) {
          try {
            if (!chartInstances.trend.destroyed) {
              chartInstances.trend.destroy()
            }
          } catch (e) {
            console.warn('기존 트렌드 차트 정리 중 경고:', e)
          }
          chartInstances.trend = null
        }

        // Canvas 크기 재설정 (Chart.js 버그 방지)
        trendChart.value.width = trendChart.value.offsetWidth
        trendChart.value.height = trendChart.value.offsetHeight

        chartInstances.trend = new Chart(ctx, {
          type: 'line',
          data: monthlyTrendData, // 업데이트된 데이터 사용
          options: {
            // ... 기존 옵션들 그대로 유지
            responsive: true,
            maintainAspectRatio: false,
            animation: false,
            plugins: {
              legend: {
                position: 'top',
                labels: {
                  usePointStyle: true,
                  padding: 20,
                  font: {
                    size: 12,
                    weight: '500'
                  }
                }
              },
              tooltip: {
                mode: 'index',
                intersect: false,
                backgroundColor: 'rgba(0, 0, 0, 0.8)',
                titleColor: '#fff',
                bodyColor: '#fff',
                borderColor: 'rgba(255, 255, 255, 0.2)',
                borderWidth: 1,
                cornerRadius: 8,
                displayColors: true,
                callbacks: {
                  title: function(context) {
                    return context[0].label + ' 처리현황'
                  },
                  label: function(context) {
                    return context.dataset.label + ': ' + context.parsed.y + '건'
                  },
                  afterLabel: function(context) {
                    if (context.datasetIndex === 0) {
                      const total = context.parsed.y
                      const completed = context.chart.data.datasets[1].data[context.dataIndex]
                      const rate = total > 0 ? Math.round((completed / total) * 100) : 0
                      return `완료율: ${rate}%`
                    }
                    return ''
                  }
                }
              }
            },
            scales: {
              x: {
                grid: {
                  display: true,
                  color: 'rgba(0, 0, 0, 0.05)'
                },
                ticks: {
                  font: {
                    size: 11
                  }
                }
              },
              y: {
                beginAtZero: true,
                grid: {
                  display: true,
                  color: 'rgba(0, 0, 0, 0.05)'
                },
                ticks: {
                  font: {
                    size: 11
                  },
                  callback: function(value) {
                    return value + '건'
                  }
                }
              }
            },
            interaction: {
              mode: 'nearest',
              axis: 'x',
              intersect: false
            },
            elements: {
              point: {
                hoverBorderWidth: 3
              }
            }
          }
        })
      } catch (error) {
        console.error('Chart initialization failed:', error)
      }
    }


    // 차트 데이터 업데이트 함수
    const updateTrendChart = (newData) => {
      if (chartInstances.trend && newData) {
        try {
          chartInstances.trend.data = newData
          chartInstances.trend.update('active')
        } catch (error) {
          console.error('차트 업데이트 중 오류:', error)
        }
      }
    }

    const applyFilters = () => {
      console.log('필터 적용')
    }
    
    const getPerformanceColor = (value) => {
      if (value >= 90) return 'success'
      if (value >= 80) return 'warning'
      return 'error'
    }
    
    const getCompletionColor = (value) => {
      if (value >= 90) return 'success'
      if (value >= 80) return 'primary'
      return 'warning'
    }
    
    const getProcessTimeClass = (time) => {
      if (time <= 2) return 'success--text'
      if (time <= 3) return 'warning--text'
      return 'error--text'
    }
    
    const viewDetails = (item) => {
      console.log('상세보기:', item)
    }
    
    const exportReport = (item) => {
      console.log('리포트 내보내기:', item)
    }
    
    const exportFullReport = () => {
      console.log('전체 리포트 내보내기')
    }
    
    const scheduleReport = () => {
      console.log('정기 리포트 설정')
    }
    
    const setAlerts = () => {
      console.log('알림 설정')
    }

    // 생명주기 훅
    onMounted(() => {
      onSearch()
    })

    onUnmounted(() => {
      destroyAllCharts()
    })

    return {
      // refs
      loading,
      trendChart,
      companyChart,
      statusChart,
      categoryChart,
      processingTimeChart,
      
      // 필터 관련
      selectedPeriod,
      selectedCompanies,
      selectedStatuses,
      selectedCategories,
      selectedPriority,
      companySearch,
      timeAnalysisView,
      periods,
      companies,
      statuses,
      categories,
      priorities,
      
      // 데이터
      keyMetrics,
      monthlyTrendData,
      urgentCount,
      realtimeAlerts,
      performanceKPIs,
      processingTimes,
      performanceHeaders,
      performanceData,
      companyHeaders,
      companyData,
      insights,
      suggestions,
      
      // 메소드
      onSearch,
      // fetchDashboardData,
      destroyAllCharts,
      updateAllCharts,
      initTrendChart,
      fetchMonthlyTrendData,
      updateTrendChart,
      applyFilters,
      getPerformanceColor,
      getCompletionColor,
      getProcessTimeClass,
      viewDetails,
      exportReport,
      exportFullReport,
      scheduleReport,
      setAlerts
    }
  }
}
</script>

<style scoped>
.v-card {
  transition: all 0.3s ease;
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