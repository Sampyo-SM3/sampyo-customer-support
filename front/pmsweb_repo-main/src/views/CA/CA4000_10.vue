<template>
  <div class="entry-box">
       
    <!-- 범례 -->
    <div class="legend">      
      <div class="legend-items">
        <div 
          v-for="item in legendItems" 
          :key="item.brickTypeCd"
          class="legend-item"
        >
          <div 
            class="legend-color" 
            :style="{ backgroundColor: item.brickColor }"
          ></div>
          <span>{{ item.brickTypeCd }}</span>
        </div>
      </div>
    </div>
        
    <!-- 메인 차트 영역 - 그리드와 SVG 조합 -->
    <div class="main-box">
      <!-- 왼쪽 정보 그리드 -->
      <div class="info-grid">
        <!-- 헤더 -->
        <div class="info-header">
          <!-- <div class="info-cell header">선택</div>
          <div class="info-cell header">보수시작</div>
          <div class="info-cell header">보수종료</div>
          <div class="info-cell header">가동일</div>
          <div class="info-cell header">총구간</div> -->
          <div class="info-cell">선택</div>
          <div class="info-cell">보수시작</div>
          <div class="info-cell">보수종료</div>
          <div class="info-cell">가동일</div>
          <div class="info-cell">총구간</div>          
        </div>
        
        <!-- 데이터 행들 -->
        <div 
          v-for="item in chartData" 
          :key="item.id" 
          class="info-row"
          :class="{ selected: selectedItems.includes(item.id) }"
        >
          <div class="info-cell checkbox-cell">
            <input 
              type="checkbox" 
              :checked="selectedItems.includes(item.id)"
              @change="handleRowSelect(item.id)"
              class="row-checkbox"
            />
          </div>
          <div class="info-cell name-cell">
            {{ item.repairFrDt }}
          </div>
          <div class="info-cell duration-cell">
            {{ item.repairToDt }}
          </div>
          <div class="info-cell length-cell">
            {{ item.repairDay }}
          </div>
          <div class="info-cell length-cell">
            {{ item.repairMeter }}
          </div>          
        </div>
      </div>

      <GanttChart
        :chartData="chartData"
        :config="config"      
      />      
    </div>



    <!-- 선택된 항목 표시 -->
    <!-- <div v-if="selectedItems.length > 0" class="selected-info">
      <h3>선택된 항목</h3>
      <div>선택된 행: {{ selectedItems.join(', ') }}</div>
    </div> -->
  </div>
</template>

<script setup>
import apiClient from '@/api';
import { onMounted, ref } from 'vue'
import GanttChart from '@/components/GanttChart';

// Reactive 상태    
const selectedItems = ref([])
const chartData = ref([])
const legendItems = ref([]) 

// api호출해서 다시 세팅함
const config = ref({
  margin: { top: 50, right: 30, bottom: 30, left: 0 },
  rowHeight: 50,
  barHeight: 30,
  maxDuration: 0,
  phases: [
    { name: 'test', color: '#87CEEB', range: [0, 3] },
    { name: 'test', color: '#87CEEB', range: [3, 9] },
    { name: 'test', color: '#90EE90', range: [9, 25] },
    { name: 'test', color: '#FFA07A', range: [25, 40] },
    { name: 'test', color: '#90EE90', range: [40, 60] },
    { name: 'test', color: '#40GE90', range: [60, 87] }
  ]
})

// 라이프사이클 훅
onMounted(() => {
  selectChartData();
})

const selectChartData = async (para_params) => {  
  console.log('--selectChartData--');  
  const payload = {
    ...para_params,    
  };
  console.log('최종 파라미터:', payload);
        
  try {
    // 차트 데이터, config 데이터, length 데이터를 병렬로 조회
    const [chartResponse, configResponse, lengthResponse] = await Promise.all([
      apiClient.get('/api/naehwa/select-chartdata', { params: payload }),
      apiClient.get('/api/naehwa/select-config', { params: payload }),
      apiClient.get('/api/naehwa/select-length', { params: payload })
    ]);
    
    console.log('차트 response.data -> ', chartResponse.data);
    console.log('config response.data -> ', configResponse.data);
    console.log('length response.data -> ', lengthResponse.data);
    
    // 차트 데이터 변환
    if (chartResponse.data && Array.isArray(chartResponse.data)) {
      chartData.value = transformApiDataToChartData(chartResponse.data);
      
      // 범례 데이터 생성
      legendItems.value = generateLegendItems(chartResponse.data);
      
      console.log('변환된 chartData -> ', chartData.value);
      console.log('생성된 legendItems -> ', legendItems.value);
    }
    
    // config 데이터 변환 (length API 결과를 maxDuration으로 사용)
    if (configResponse.data && Array.isArray(configResponse.data)) {
      const maxDuration = lengthResponse.data || 87; // API 결과가 없으면 기본값 87 사용
      config.value = transformApiDataToConfig(configResponse.data, maxDuration);
      console.log('변환된 config -> ', config.value);
    }
    
  } catch (error) {
    console.error('API 조회 중 오류:', error);
  }
}

// 범례 아이템 생성 함수
const generateLegendItems = (apiData) => {
  const uniqueItems = new Map();
  
  apiData.forEach(item => {
    if (item.brickTypeCd && item.brickColor) {
      // brickTypeCd를 키로 사용하여 중복 제거
      if (!uniqueItems.has(item.brickTypeCd)) {
        uniqueItems.set(item.brickTypeCd, {
          brickTypeCd: item.brickTypeCd,
          brickColor: item.brickColor
        });
      }
    }
  });
  
  return Array.from(uniqueItems.values());
}

// API 데이터를 config로 변환하는 함수 (maxDuration 매개변수 추가)
const transformApiDataToConfig = (apiConfigData, maxDuration) => {
  // 기본 색상 배열 (wcZoneColor가 없을 경우 대비)
  const defaultColors = [
    '#87CEEB', '#87CEEB', '#87CEEB', '#87CEEB',
    '#87CEEB', '#87CEEB', '#87CEEB', '#87CEEB',
  ];
  
  const phases = apiConfigData.map((item, index) => ({
    name: item.wcZoneNm,
    color: item.wcZoneColor || defaultColors[index % defaultColors.length], // wcZoneColor 우선 사용
    range: [item.wcZoneSt, item.wcZoneCl]
  }));
  
  return {
    margin: { top: 50, right: 30, bottom: 30, left: 0 },
    rowHeight: 50,
    barHeight: 30,
    maxDuration: maxDuration, // API에서 받은 값으로 설정
    phases: phases
  };
}

// API 데이터를 차트 데이터로 변환하는 함수
const transformApiDataToChartData = (apiData) => {
  // reportNo별로 그룹화
  const groupedData = apiData.reduce((acc, item) => {
    if (!acc[item.reportNo]) {
      acc[item.reportNo] = [];
    }
    acc[item.reportNo].push(item);
    return acc;
  }, {});
  
  // 그룹별로 차트 데이터 생성
  return Object.keys(groupedData).map((reportNo, index) => {
    const group = groupedData[reportNo];
    const firstItem = group[0]; // 공통 정보는 첫 번째 아이템에서 가져오기
    
    // 날짜 포맷팅 함수 (2025-06-01T00:00:00 -> 25-06-01)
    const formatDate = (dateString) => {
      const date = new Date(dateString);
      const year = date.getFullYear().toString().slice(-2);
      const month = String(date.getMonth() + 1).padStart(2, '0');
      const day = String(date.getDate()).padStart(2, '0');
      return `${year}-${month}-${day}`;
    };
    
    // API 데이터를 기반으로 tasks 생성
    const tasks = group.map(item => ({
      name: item.brickTypeCd,
      start: item.repairMeterSt,
      duration: item.repairMeterCl - item.repairMeterSt,
      color: item.brickColor,
      value: '',
    }));
    
    return {
      id: index + 1, // 1부터 순차적 ID
      reportNo: reportNo,
      repairFrDt: formatDate(firstItem.repairFrDt),
      repairToDt: formatDate(firstItem.repairToDt),
      repairDay: firstItem.repairDay,
      repairMeter: `${firstItem.repairMeter}m`,
      tasks: tasks
    };
  });
};

const handleRowSelect = (rowId) => {
  const index = selectedItems.value.indexOf(rowId)
  if (index > -1) {
    selectedItems.value.splice(index, 1)
  } else {
    selectedItems.value.push(rowId)
  }    
}
  
const selectAllRows = () => {
  selectedItems.value = chartData.value.map(d => d.id)
}

const clearSelection = () => {
  selectedItems.value = []
}

// 외부에서 접근 가능한 메서드들 expose
defineExpose({
  selectAllRows,
  clearSelection,
  selectedItems,
  handleRowSelect,
  chartData,
  config,
  selectChartData
})
</script>

<style scoped>
.entry-box {
  width: 100%;
  font-family: 'Arial', sans-serif;
}

.chart-header {
  margin-bottom: 1rem;
  padding: 1rem;
  background-color: #f8f9fa;
  border-radius: 8px;
}

.chart-header h2 {
  font-size: 1.25rem;
  font-weight: bold;
  margin-bottom: 0.5rem;
  color: #333;
}

.chart-info {
  font-size: 0.875rem;
  color: #666;
  margin-bottom: 0.5rem;
}

.usage-info {
  font-size: 0.75rem;
  color: #888;
}

/* 메인 차트 영역 - 간격 완전 제거 */
.main-box {
  display: flex;
  border: 1px solid #e5e7eb;
  /* border-radius: 8px; */
  overflow: hidden;
  background: white;    
  width: 100%;
  gap: 0; /* 간격 완전 제거 */
}

/* 왼쪽 정보 그리드 */
.info-grid {
  
  min-width: 392px; /* min-width에서 width로 변경 */
  flex-shrink: 0; /* 크기 축소 방지 */
  /* border-right: 1px solid #e5e7eb;  */
  background: white; /* 차트와 동일한 배경색 */
}



.info-row {
  display: grid;
  grid-template-columns: 60px 1fr 80px 60px 120px;
  border-bottom: 1px solid #e5e7eb;
  transition: background-color 0.2s;
  height: 50px; /* 차트의 rowHeight와 동일하게 */
  align-items: center; /* 세로 중앙 정렬 */
  background: white; /* 모든 행을 흰색으로 통일 */
}

.info-row:hover {
  background-color: #f1f3f4 !important; /* hover 시 우선순위 적용 */
}

/* .info-row.selected {
  background-color: #e3f2fd !important; 
  border-left: 4px solid #2196f3;
} */

.info-cell {
  padding: 8px 12px;
  display: flex;  
  font-size: 11px;
  border-right: 1px solid #e5e7eb;
  height: 100%; /* 부모 높이에 맞춤 */  
  align-items: center;  
  justify-content: center;  
}

/* 마지막 셀의 오른쪽 테두리 제거하여 차트와 연결 */
.info-cell:last-child {
  border-right: none;
}

.info-header {
  display: grid;
  grid-template-columns: 60px 1fr 80px 60px 120px;
  background: #e9ecef;
  border-bottom: 1px solid #ddd;
  /* font-weight: bold; */
  height: 50px; 
  align-items: center;
  justify-content: center;
}


.checkbox-cell {
  justify-content: center;
}

.row-checkbox {
  width: 16px;
  height: 16px;
  cursor: pointer;
}

.name-cell {
  font-weight: 500;
  color: #333;
}

.duration-cell {
  justify-content: center;
  color: #666;
}

.length-cell {
  color: #666;
  font-size: 10px;
}

.legend {
  margin-top: 1rem;
  margin-bottom: 4rem;
  padding: 1rem;
  background-color: #f8f9fa;
  /* border-radius: 8px; */
}

.legend h3 {
  font-weight: 600;
  margin-bottom: 0.5rem;
  color: #333;
}

.legend-items {
  display: flex;
  flex-wrap: wrap;
  gap: 1rem;
}

.legend-item {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  font-size: 0.875rem;
}

.legend-color {
  width: 1rem;
  height: 1rem;
  border-radius: 2px;
  border: 1px solid #ddd;
}

.selected-info {
  margin-top: 1rem;
  padding: 1rem;
  background-color: #eff6ff;
  border-radius: 8px;
  border-left: 4px solid #3b82f6;
}

.selected-info h3 {
  font-weight: 600;
  margin-bottom: 0.5rem;
  color: #1e40af;
}

.selected-info div {
  font-size: 0.875rem;
  color: #1e40af;
}


</style>