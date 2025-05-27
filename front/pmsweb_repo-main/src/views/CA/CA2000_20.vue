<template>
  <v-container fluid class="pr-0 pl-0 pt-4">
    <v-row class="user-row" align="center">
      <v-col sm="6" md="auto" class="d-flex align-center filter-col">
        <span class="filter-label">요청기간<span class="label-divider"></span></span>
        
        <!-- 시작일 입력 필드 -->
        <div class="start-date-wrapper">
          <VueDatePicker 
            class="date-picker" 
            :month-picker="false" 
            preview-format="yyyy-MM-dd" 
            v-model="dateStartDate"
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
            v-model="dateEndDate"
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
      
      <v-col sm="12" md="auto" class="d-flex justify-end">
        <v-btn 
          variant="flat" 
          color="primary" 
          class="custom-btn" 
          size="small" 
          @click="handleSearch"
        >
          <v-icon size="default" class="mr-1">mdi-magnify</v-icon>
          조회
        </v-btn>
      </v-col>      
    </v-row>
    
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

    <!-- 데이터 테이블 상단 버튼 영역 -->
    <v-row class="top-button-row">
      <v-col class="d-flex align-center pb-0">
        <v-btn-toggle v-model="selectedView" class="custom-btn-toggle" mandatory>
          <v-btn 
            value="my" 
            @click="handleViewChange('A')" 
            :class="{ 'selected-btn': selectedView === 'my'}"
            :disabled="loading"
          >
            나의 문의글
          </v-btn>
          <v-btn 
            value="dept" 
            @click="handleViewChange('B')" 
            :class="{ 'selected-btn': selectedView === 'dept' }"
            :disabled="loading"
          >
            부서 문의글
          </v-btn>
        </v-btn-toggle>
      </v-col>
    </v-row>

    <!-- 데이터 테이블 -->
    <v-row class="grid-table ma-0 pa-0">
      <v-col class="pa-0">
        <div class="table-container">
          <!-- 테이블 헤더 -->
          <div class="table-header">
            <div class="th-cell">접수번호</div>
            <div class="th-cell">요청일</div>
            <div class="th-cell">제목</div>
            <div class="th-cell">소속</div>
            <div class="th-cell">작성자</div>
            <div class="th-cell">문의유형</div>
            <div class="th-cell">진행상태</div>
            <div class="th-cell">완료일</div>
            <div class="th-cell">담당자</div>
            <div class="th-cell">소요시간</div>
          </div>

          <!-- 테이블 데이터 행 -->
          <div v-for="(item, index) in paginatedData" :key="index" class="table-row">
            <div class="td-cell">{{ item.seq }}</div>
            <div class="td-cell">{{ formatDate(item.requestDate) }}</div>
            <div class="td-cell title-cell">
              <router-link 
                :to="{
                  name: (item.saveFlag === 'Y' && item.processState === 'S')
                    ? 'CA_PostDetailSrForm'
                    : 'CA_PostDetailForm',
                  params: { receivedSeq: item.seq }
                }" 
                class="title-link" 
                style="display: inline-flex; align-items: center;"
              >
                {{ item.sub }}

                <span v-if="item.countComment > 0" style="color: #737577;">
                  &nbsp;[{{ item.countComment }}]
                </span>

                <span v-if="item.new_yn === 'Y'">&nbsp;</span>
                <v-img 
                  v-if="item.new_yn === 'Y'" 
                  src="@/assets/new-icon.png" 
                  alt="new" 
                  width="22" 
                  height="22"
                  style="display: inline-block; vertical-align: middle;"
                />
              </router-link>
            </div>
            <div class="td-cell" style="text-align: center;">
              {{ item.division }}<br>
              {{ item.dpNm }}
            </div>
            <div class="td-cell">{{ item.uid }}</div>
            <div class="td-cell">{{ item.inquiryType }}</div>
            <div class="td-cell">
              <span :class="['status-badge', 'status-' + item.processState]">
                {{ item.status }}
              </span>
            </div>
            <div class="td-cell">{{ formatDate(item.completeDt) }}</div>
            <div class="td-cell">{{ item.manager || '-' }}</div>
            <div class="td-cell">{{ calculateDuration(item.requestDate, item.completeDt) }}</div>
          </div>
        </div>

        <!-- 로딩 표시 -->
        <div v-if="loading" class="loading-overlay">
          <v-progress-circular indeterminate color="primary" />
        </div>

        <!-- 데이터 없음 표시 -->
        <div v-if="!loading && tableData.length === 0" class="no-data">
          조회된 데이터가 없습니다.
        </div>

        <!-- 페이지네이션 -->
        <div class="pagination-container" v-if="tableData.length > 0">
          <v-btn 
            icon="mdi-chevron-left" 
            variant="text" 
            size="small" 
            :disabled="currentPage === 1"
            @click="currentPage--"
          />

          <template v-if="totalPages <= 5">
            <v-btn 
              v-for="page in totalPages" 
              :key="page" 
              size="small" 
              :variant="currentPage === page ? 'flat' : 'text'"
              :color="currentPage === page ? 'primary' : ''" 
              @click="currentPage = page"
            >
              {{ page }}
            </v-btn>
          </template>

          <template v-else>
            <!-- 처음 페이지 -->
            <v-btn 
              v-if="currentPage > 3" 
              size="small" 
              variant="text" 
              @click="currentPage = 1"
            >
              1
            </v-btn>

            <!-- 생략 표시 -->
            <span v-if="currentPage > 3" class="mx-1">...</span>

            <!-- 이전 페이지 -->
            <v-btn 
              v-if="currentPage > 1" 
              size="small" 
              variant="text" 
              @click="currentPage = currentPage - 1"
            >
              {{ currentPage - 1 }}
            </v-btn>

            <!-- 현재 페이지 -->
            <v-btn size="small" variant="flat" color="primary">
              {{ currentPage }}
            </v-btn>

            <!-- 다음 페이지 -->
            <v-btn 
              v-if="currentPage < totalPages" 
              size="small" 
              variant="text" 
              @click="currentPage = currentPage + 1"
            >
              {{ currentPage + 1 }}
            </v-btn>

            <!-- 생략 표시 -->
            <span v-if="currentPage < totalPages - 2" class="mx-1">...</span>

            <!-- 마지막 페이지 -->
            <v-btn 
              v-if="currentPage < totalPages - 2" 
              size="small" 
              variant="text" 
              @click="currentPage = totalPages"
            >
              {{ totalPages }}
            </v-btn>
          </template>

          <v-btn 
            icon="mdi-chevron-right" 
            variant="text" 
            size="small" 
            :disabled="currentPage === totalPages"
            @click="currentPage++"
          />
        </div>
      </v-col>
    </v-row>
  </v-container>

  <!-- 스낵바로 오류 메시지 표시 -->
  <v-snackbar 
    v-model="showError" 
    color="warning" 
    timeout="5000" 
    location="center" 
    elevation="8" 
    variant="elevated"
  >
    {{ errorMessages[0] }}

    <template v-slot:actions>
      <v-btn variant="text" @click="showError = false">
        닫기
      </v-btn>
    </template>
  </v-snackbar>
</template>

<script setup>
import { ref, reactive, computed, onMounted, onBeforeUnmount, nextTick, watch } from 'vue'

import apiClient from '@/api'
import VueDatePicker from '@vuepic/vue-datepicker'
import '@vuepic/vue-datepicker/dist/main.css'
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
} from 'chart.js'

Chart.register(
  DoughnutController,
  BarController,
  BarElement,
  ArcElement,
  Tooltip,
  Legend,
  CategoryScale,
  LinearScale
)

// Refs
const inquiryTypeChartCanvas = ref(null)
const statusChartCanvas = ref(null)
const monthlyChartCanvas = ref(null)

// Reactive data
const dateStartDate = ref(new Date())
const dateEndDate = ref(new Date())
const startDatePickerOpen = ref(false)
const endDatePickerOpen = ref(false)
const startDate = ref('')
const endDate = ref('')
const dateRange = ref('month')
const selectedView = ref('my')
const tableData = ref([])
const loading = ref(false)
const currentPage = ref(1)
const itemsPerPage = ref(10)
const errorMessages = ref([])
const showError = ref(false)
const progressStatuses = ref([])
const selectedStatus = ref('')
const userName = ref(null)
const userId = ref(null)
const userDeptCd = ref(null)
const aryInquiryRes = ref([])
const aryInquiryResCount = ref([])

// 차트 업데이트 상태 관리 (사용하지 않는 변수들 제거)
// const isUpdatingCharts = ref(false)
// const pendingChartUpdate = ref(false)

// Chart instances
const chartInstances = reactive({
  inquiryType: null,
  status: null,
  monthly: null
})

// Computed
const totalPages = computed(() => {
  return Math.ceil(tableData.value.length / itemsPerPage.value)
})

const paginatedData = computed(() => {
  const start = (currentPage.value - 1) * itemsPerPage.value
  const end = start + itemsPerPage.value
  return tableData.value.slice(start, end)
})

// Watchers
watch([startDate, endDate], () => {
  currentPage.value = 1
})

// Methods
const onStartDateChange = (date) => {
  dateStartDate.value = date
  startDatePickerOpen.value = false
}

const onEndDateChange = (date) => {
  dateEndDate.value = date
  endDatePickerOpen.value = false
  if (date) {
    const formattedDate = new Date(date)
    const year = formattedDate.getFullYear()
    const month = String(formattedDate.getMonth() + 1).padStart(2, '0')
    const day = String(formattedDate.getDate()).padStart(2, '0')
    endDate.value = `${year}-${month}-${day}`
  }
}

const setDateRange = (range) => {
  dateRange.value = range
  const today = new Date()
  let start = new Date(today)

  switch (range) {
    case 'today':
      break
    case 'week':
      start.setDate(today.getDate() - 7)
      break
    case '15days':
      start.setDate(today.getDate() - 15)
      break
    case 'month':
      start.setMonth(today.getMonth() - 1)
      break
    case '3months':
      start.setMonth(today.getMonth() - 3)
      break
  }

  dateStartDate.value = start
  dateEndDate.value = today
}

const formattedDate = (dateObj) => {
  const year = dateObj.getFullYear()
  const month = String(dateObj.getMonth() + 1).padStart(2, '0')
  const day = String(dateObj.getDate()).padStart(2, '0')
  return `${year}-${month}-${day}`
}

const formatDate = (dateString) => {
  if (!dateString) return '-'
  const date = new Date(dateString)
  if (isNaN(date.getTime())) return '-'

  const year = date.getFullYear()
  const month = String(date.getMonth() + 1).padStart(2, '0')
  const day = String(date.getDate()).padStart(2, '0')
  return `${year}-${month}-${day}`
}

const calculateDuration = (startDate, endDate) => {
  if (!startDate || !endDate) return '-'

  const start = new Date(startDate)
  const end = new Date(endDate)
  if (isNaN(start.getTime()) || isNaN(end.getTime())) return '-'

  const diffTime = Math.abs(end - start)
  const diffDays = Math.floor(diffTime / (1000 * 60 * 60 * 24)) + 1
  return `${diffDays}일`
}

const getUserInfo = () => {
  const userInfo = JSON.parse(localStorage.getItem("userInfo")) || {}
  userName.value = userInfo.name || null
  userId.value = userInfo.id || null
  userDeptCd.value = userInfo.deptCd || null
}

const getStatus = async () => {
  try {
    const statusList = await apiClient.get("/api/code/list", {
      params: { category: 'STATUS' }
    })

    progressStatuses.value = statusList.data.map(status => ({
      text: status.codeName,
      value: status.codeId
    }))

    progressStatuses.value.unshift({ text: '전체', value: '%' })
    selectedStatus.value = '%'

  } catch (error) {
    console.error("❌ 상태 로드 오류:", error)
  }
}

// 뷰 변경 핸들러 - 중복 클릭 방지
const handleViewChange = async (paraType) => {
  if (loading.value) return
  
  // 먼저 모든 차트 완전히 제거
  destroyAllCharts()
  
  // selectedView 값 업데이트
  if (paraType === 'A') {
    selectedView.value = 'my'
  } else if (paraType === 'B') {
    selectedView.value = 'dept'
  }
  
  await fetchData(paraType)
}

const fetchData = async (paraType) => {
  // 이미 로딩 중이면 무시
  if (loading.value) return
  
  loading.value = true

  try {
    let response = ''
    
    if (paraType === 'A') {
      response = await apiClient.get('/api/require/search', {
        params: {
          startDate: formattedDate(dateStartDate.value) + ' 00:00:00',
          endDate: formattedDate(dateEndDate.value) + ' 23:59:59',
          writerId: userId.value,
        }
      })
    } else if (paraType === 'B') {    
      response = await apiClient.get('/api/require/search', {
        params: {
          startDate: formattedDate(dateStartDate.value) + ' 00:00:00',
          endDate: formattedDate(dateEndDate.value) + ' 23:59:59',
          writerDp: userId.value,
        }
      })
    }

    if (response.data && Array.isArray(response.data)) {
      tableData.value = response.data.map(item => {
        const requestDateTime = new Date(item.requestDateTime)
        const now = new Date()
        const diffTime = now - requestDateTime
        const diffHours = diffTime / (1000 * 60 * 60)

        return {
          ...item,
          selected: false,
          status: item.processState === 'S'
            ? (item.statusNm + ' (' +
              (item.srFlag === 'Y'
                ? '상신완료'
                : item.srFlag === 'F'
                  ? '반려'
                  : '상신 전'
              ) + ')' || getRandomStatus())
            : (item.statusNm || getRandomStatus()),
          new_yn: diffHours < 24 ? 'Y' : 'N',
          manager: item.manager || '-',
          memo: item.currentIssue || '-',
        }
      })
    } else {
      tableData.value = []
    }

    // 문의 유형별 count 데이터 가져오기
    const response2 = await apiClient.post('/api/code/count', {
      startDate: formattedDate(dateStartDate.value) + ' 00:00:00',
      endDate: formattedDate(dateEndDate.value) + ' 23:59:59',
      writerId: paraType === 'A' ? userId.value : '',
      writerDp: paraType === 'B' ? userId.value : ''
    })

    aryInquiryRes.value = response2.data.map(item => item.codeName)
    aryInquiryResCount.value = response2.data.map(item => item.cnt)

  } catch (error) {
    console.error('데이터 로드 중 오류 발생:', error)
    showError.value = true
    errorMessages.value = ['데이터를 불러오는 중 오류가 발생했습니다.']
  } finally {
    loading.value = false
  }
  
  // 로딩 완료 후 별도로 차트 업데이트
  await nextTick()
  setTimeout(() => {
    if (!loading.value) { // 로딩이 완전히 끝난 후에만 실행
      updateAllCharts()
    }
  }, 300)
}

const getRandomStatus = () => {
  const statusList = ['미처리', '진행중', '보류중', '종결']
  return statusList[Math.floor(Math.random() * statusList.length)]
}

// Chart methods - 더 안전한 차트 관리
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

const updateAllCharts = () => {
  try {
    // 먼저 모든 차트 정리
    destroyAllCharts()
    
    // DOM이 준비되었는지 확인
    if (!inquiryTypeChartCanvas.value || !statusChartCanvas.value || !monthlyChartCanvas.value) {
      console.warn('차트 캔버스 요소가 아직 준비되지 않았습니다.')
      return
    }
    
    // 순차적으로 차트 생성
    setTimeout(() => updateInquiryTypeChart(), 50)
    setTimeout(() => updateStatusChart(), 100)
    setTimeout(() => updateMonthlyChart(), 150)
    
  } catch (error) {
    console.error('차트 업데이트 중 오류:', error)
  }
}

const updateInquiryTypeChart = () => {
  try {
    const canvas = inquiryTypeChartCanvas.value
    if (!canvas) {
      console.warn('inquiryTypeChartCanvas가 준비되지 않았습니다.')
      return
    }

    const ctx = canvas.getContext('2d')
    if (!ctx) {
      console.warn('Canvas context를 가져올 수 없습니다.')
      return
    }

    // 데이터가 있는지 확인
    if (!aryInquiryRes.value || !aryInquiryRes.value.length || !aryInquiryResCount.value || !aryInquiryResCount.value.length) {
      console.warn('문의유형 차트 데이터가 없습니다.')
      return
    }

    // Canvas 크기 재설정 (Chart.js 버그 방지)
    canvas.width = canvas.offsetWidth
    canvas.height = canvas.offsetHeight

    chartInstances.inquiryType = new Chart(ctx, {
      type: 'doughnut',
      data: {
        labels: aryInquiryRes.value,
        datasets: [{
          data: aryInquiryResCount.value,
          backgroundColor: ['#FF9F40', '#4BC0C0', '#9966FF', '#FF6384', '#36A2EB']
        }]
      },
      options: {
        responsive: true,
        maintainAspectRatio: false,
        animation: false, // 애니메이션 비활성화로 안정성 향상
        plugins: {
          legend: {
            position: 'top',
            align: 'start'
          },
          tooltip: {
            callbacks: {
              label: ctx => `${ctx.label}: ${ctx.parsed}건`
            }
          }
        }
      }
    })
  } catch (error) {
    console.error('문의유형 차트 생성 중 오류:', error)
  }
}

const updateStatusChart = () => {
  try {
    const canvas = statusChartCanvas.value
    if (!canvas) {
      console.warn('statusChartCanvas가 준비되지 않았습니다.')
      return
    }

    const ctx = canvas.getContext('2d')
    if (!ctx) {
      console.warn('Canvas context를 가져올 수 없습니다.')
      return
    }

    const statusData = getStatusData()
    
    // 데이터가 있는지 확인
    if (!statusData.labels.length || !statusData.data.length) {
      console.warn('상태 차트 데이터가 없습니다.')
      return
    }

    // Canvas 크기 재설정
    canvas.width = canvas.offsetWidth
    canvas.height = canvas.offsetHeight

    chartInstances.status = new Chart(ctx, {
      type: 'bar',
      data: {
        labels: statusData.labels,
        datasets: [{
          label: '건수',
          data: statusData.data,
          backgroundColor: '#42A5F5'
        }]
      },
      options: {
        responsive: true,
        maintainAspectRatio: false,
        animation: false, // 애니메이션 비활성화
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
    })
  } catch (error) {
    console.error('상태 차트 생성 중 오류:', error)
  }
}

const updateMonthlyChart = () => {
  try {
    const canvas = monthlyChartCanvas.value
    if (!canvas) {
      console.warn('monthlyChartCanvas가 준비되지 않았습니다.')
      return
    }

    const ctx = canvas.getContext('2d')
    if (!ctx) {
      console.warn('Canvas context를 가져올 수 없습니다.')
      return
    }

    const monthlyData = getMonthlyData()

    // 전체 월 라벨 배열
    const allMonthLabels = ['1월', '2월', '3월', '4월', '5월', '6월', '7월', '8월', '9월', '10월', '11월', '12월']
    
    // 데이터가 있는 월만 필터링
    const filteredData = []
    const filteredLabels = []
    
    monthlyData.forEach((count, index) => {
      if (count > 0) {
        filteredData.push(count)
        filteredLabels.push(allMonthLabels[index])
      }
    })

    // Canvas 크기 재설정
    canvas.width = canvas.offsetWidth
    canvas.height = canvas.offsetHeight

    chartInstances.monthly = new Chart(ctx, {
      type: 'bar',
      data: {
        labels: filteredLabels,
        datasets: [{
          label: '작성 건수',
          data: filteredData,
          backgroundColor: '#66BB6A'
        }]
      },
      options: {
        responsive: true,
        maintainAspectRatio: false,
        animation: false, // 애니메이션 비활성화
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
    })
  } catch (error) {
    console.error('월별 차트 생성 중 오류:', error)
  }
}

const getStatusData = () => {
  const statusCount = {}
  
  tableData.value.forEach(item => {
    const status = item.processState || 'unknown'
    statusCount[status] = (statusCount[status] || 0) + 1
  })

  const statusMapping = {
    'P': '미처리',
    'I': '진행중', 
    'H': '보류',
    'S': 'SR',
    'C': '종결'
  }

  const labels = Object.keys(statusCount).map(key => statusMapping[key] || key)
  const data = Object.values(statusCount)

  return { labels, data }
}

const getMonthlyData = () => {
  const monthlyCount = new Array(12).fill(0)
  
  tableData.value.forEach(item => {
    if (item.requestDate) {
      const month = new Date(item.requestDate).getMonth()
      monthlyCount[month]++
    }
  })

  return monthlyCount
}

const handleSearch = async () => {
  const viewType = selectedView.value === 'my' ? 'A' : 'B'
  await fetchData(viewType)
}

// Lifecycle
onMounted(async () => {
  setDateRange('month')
  getUserInfo()
  await getStatus()
  await fetchData('A')
})

onBeforeUnmount(() => {
  destroyAllCharts()
})
</script>
    
    <style scoped>
      .user-row {
          transform: translateX(10px);
      }
  
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
    
  
    
    /* 날짜 선택 관련 스타일 */
    .date-field-wrapper {
      display: flex;
      align-items: center;
      cursor: pointer;
      width: 100%;
    
    }
    
    .calendar-icon-container {
      display: flex;
      align-items: center;
      margin-left: 12px;
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
      margin-bottom: 0px;
    }
    
    .white-text {
      color: white !important;
    }
    
    /* 테이블 스타일 */
    .table-container {
      border: 1px solid #e0e0e0;
      width: 100%;
      position: relative;
      border-radius: 0px;
      /* 모서리 라운드 처리 */
      overflow: hidden;
      /* 내부 요소가 라운드 처리된 모서리를 벗어나지 않도록 함 */
    }
    
  
    
    .table-header {
      background-color: #D0DFF1;
      font-weight: 500;
      border-bottom: 1px solid #e0e0e0;
      color: #3E4B5B !important;
    }
    
    .table-row {
      border-bottom: 1px solid #e0e0e0;
      height: 54px;
      color: #5B5D60;
      font-size: 15px;
    }
    
    .table-row:hover {
      background-color: #f9f9f9;
    }
    
    
    .th-cell,
    .td-cell {
      padding: 8px 12px;
      border-right: none;
      display: flex;
      align-items: center;
      font-size: 14px;
    
    }
    
    .th-cell {
      justify-content: center;
      font-weight: 500;
      white-space: nowrap;
      font-size: 14px;
    }
    
  
    /* 1페이지의 1행만 열 간격이 틀어지는 현상이 있어서 강제로 사이즈를 지정함 */
    .table-header,
    .table-row {
      display: grid;
      grid-template-columns: 80px 100px 1fr 120px 100px 120px 120px 100px 90px 100px;
    }
        
    .th-cell:not(:nth-child(0)):not(:nth-child(3)),
    .td-cell:not(:nth-child(0)):not(:nth-child(3)) {
      justify-content: center;
    }
  
    .th-cell:nth-child(3),
    .td-cell:nth-child(3) {
      flex: 1;
    }
  
     
    
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
      color: #f34a4d;
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
      margin-top: 10px;
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
    
    .status-badge {
      padding: 4px 10px;
      border-radius: 12px;
      font-size: 12px;
      font-weight: 600;
      display: inline-block;
    }
      
    .chart-container {
    position: relative;
    height: 240px;
    display: flex;
    justify-content: center;
    align-items: center;
  }
  
  
  .custom-btn-toggle {
    display: inline-flex;
    border-radius: 0px;
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
  
  .custom-btn {
    font-size: 14px;
    height: 40px;
    border-radius: 10px;
  }
  
    </style>
