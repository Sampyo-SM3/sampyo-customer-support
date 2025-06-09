<template>
  <div class="gantt-chart-container">
    <div class="chart-header">
      <h2>공정 진행 현황 (D3.js)</h2>
      <div class="chart-info">Ø5.5mm-87mL</div>
      <div class="usage-info">
        • 마우스 오버: 상세 정보 | • 클릭: 작업 선택 | • 체크박스: 행 선택
      </div>
    </div>
    
    <!-- 메인 차트 영역 - 그리드와 SVG 조합 -->
    <div class="chart-main">
      <!-- 왼쪽 정보 그리드 -->
      <div class="info-grid">
        <!-- 헤더 -->
        <div class="info-header">
          <div class="info-cell header">선택</div>
          <div class="info-cell header">기간</div>
          <div class="info-cell header">일수</div>
          <div class="info-cell header">길이</div>
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
            {{ item.name }}
          </div>
          <div class="info-cell duration-cell">
            {{ item.duration }}일
          </div>
          <div class="info-cell length-cell">
            {{ item.length }}
          </div>
        </div>
      </div>
      
      <!-- 오른쪽 차트 영역 -->
      <div ref="containerRef" class="chart-container">
        <svg ref="svgRef"></svg>
      </div>
    </div>

    <!-- 범례 -->
    <div class="legend">
      <h3>범례</h3>
      <div class="legend-items">
        <div class="legend-item">
          <div class="legend-color out"></div>
          <span>OUT / L.T.Z</span>
        </div>
        <div class="legend-item">
          <div class="legend-color coating"></div>
          <span>COATING</span>
        </div>
        <div class="legend-item">
          <div class="legend-color utz"></div>
          <span>U.T.Z</span>
        </div>
        <div class="legend-item">
          <div class="legend-color safety"></div>
          <span>SAFETY</span>
        </div>
      </div>
    </div>

    <!-- 선택된 항목 표시 -->
    <div v-if="selectedItems.length > 0" class="selected-info">
      <h3>선택된 항목</h3>
      <div>선택된 행: {{ selectedItems.join(', ') }}</div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted, watch, nextTick } from 'vue'
import * as d3 from 'd3'

// Props 정의
const props = defineProps({
  chartData: {
    type: Array,
    default: () => [
      {
        id: 1,
        name: '25-01-22 ~ 25-03-09',
        duration: 129,
        length: '32.4m (162R)',
        tasks: [
          { name: 'OUT', start: 0, duration: 1.5, color: '#87CEEB', value: '' },
          { name: 'L.T.Z', start: 5, duration: 4, color: '#87CEEB', value: '' },
          { name: 'COATING', start: 9, duration: 20, color: '#FFD700', value: '' },
          { name: 'U.T.Z', start: 52, duration: 5, color: '#FFA500', value: '8.1' },          
        ]
      },
      {
        id: 2,
        name: '24-09-14 ~ 24-09-15',
        duration: 8,
        length: '0.4m (2R)',
        tasks: [
          { name: 'OUT', start: 31, duration: 1, color: '#87CEEB', value: '0.4' }
        ]
      },
    ]
  },
  config: {
    type: Object,
    default: () => ({
      margin: { top: 60, right: 30, bottom: 30, left: 50 },
      rowHeight: 50,
      barHeight: 30,
      maxDuration: 87,
      phases: [
        { name: 'OUT', color: '#87CEEB', range: [0, 3] },
        { name: 'L.T.Z', color: '#87CEEB', range: [3, 9] },
        { name: 'COATING ZONE', color: '#90EE90', range: [9, 25] },
        { name: 'U.T.Z', color: '#FFA07A', range: [25, 40] },
        { name: 'SAFETY ZONE', color: '#90EE90', range: [40, 60] },
        { name: 'CALCINING ZONE', color: '#40GE90', range: [60, 87] }
      ]
    })
  }
})

// Emits 정의
const emit = defineEmits(['task-clicked', 'row-selected', 'chart-updated'])

// Reactive 상태
const svgRef = ref(null)
const containerRef = ref(null)
const selectedItems = ref([])
const hoveredTask = ref(null)

// 헤더 그리기 함수
const drawHeader = (svg, xScale) => {
  const headerGroup = svg.append('g')
    .attr('transform', `translate(${props.config.margin.left}, 10)`)

  // 단계별 헤더
  props.config.phases.forEach(phase => {
    const startX = xScale(phase.range[0])
    const endX = xScale(phase.range[1])
    
    headerGroup.append('rect')
      .attr('x', startX)
      .attr('y', 0)
      .attr('width', endX - startX)
      .attr('height', 25)
      .attr('fill', phase.color)
      .attr('stroke', '#fff')
      .attr('stroke-width', 1)

    headerGroup.append('text')
      .attr('x', startX + (endX - startX) / 2)
      .attr('y', 17)
      .attr('text-anchor', 'middle')
      .attr('fill', 'white')
      .attr('font-size', '12px')
      .attr('font-weight', 'bold')
      .text(phase.name)
  })

  // 시간축
  const timeAxis = d3.axisTop(xScale)
    .tickValues(d3.range(0, props.config.maxDuration + 1, 5))
    .tickSize(-props.chartData.length * props.config.rowHeight)

  headerGroup.append('g')
    .attr('transform', 'translate(0, 40)')
    .call(timeAxis)
    .selectAll('text')
    .attr('font-size', '10px')
}

// 격자선 그리기 함수
const drawGridlines = (g, xScale) => {
  const gridlines = g.append('g')
    .attr('class', 'gridlines')

  const tickValues = d3.range(0, props.config.maxDuration + 1, 5)
  
  gridlines.selectAll('.grid-line')
    .data(tickValues)
    .enter()
    .append('line')
    .attr('class', 'grid-line')
    .attr('x1', d => xScale(d))
    .attr('x2', d => xScale(d))
    .attr('y1', 0)
    .attr('y2', props.chartData.length * props.config.rowHeight)
    .attr('stroke', '#e5e7eb')
    .attr('stroke-width', 1)
    .attr('stroke-dasharray', '2,2')
}

// 데이터 행 그리기 함수  
const drawDataRows = (g, xScale, yScale, width) => {
  const rows = g.selectAll('.row')
    .data(props.chartData)
    .enter()
    .append('g')
    .attr('class', 'row')
    .attr('transform', d => `translate(0, ${yScale(d.id)})`)

  // 행 배경
  rows.append('rect')
    .attr('x', 0)
    .attr('y', 0)
    .attr('width', width)
    .attr('height', yScale.bandwidth())
    .attr('fill', '#f9f9f9')
    .attr('stroke', '#eee')

  // 작업 바
  const bars = rows.selectAll('.task-bar')
    .data(d => d.tasks.map(task => ({...task, parentId: d.id})))
    .enter()
    .append('g')
    .attr('class', 'task-bar')

  bars.append('rect')
    .attr('x', d => xScale(d.start))
    .attr('y', (yScale.bandwidth() - props.config.barHeight) / 2)
    .attr('width', d => xScale(d.duration))
    .attr('height', props.config.barHeight)
    .attr('fill', d => d.color)
    .attr('stroke', '#fff')
    .attr('stroke-width', 1)
    .attr('rx', 3)
    .style('cursor', 'pointer')
    .style('opacity', 0.8)
    .on('mouseover', handleTaskHover)
    .on('mouseout', handleTaskLeave)
    .on('click', handleTaskClick)

  // 바 안의 텍스트
  bars.append('text')
    .attr('x', d => xScale(d.start) + xScale(d.duration) / 2)
    .attr('y', yScale.bandwidth() / 2 + 4)
    .attr('text-anchor', 'middle')
    .attr('fill', 'white')
    .attr('font-size', '10px')
    .attr('font-weight', 'bold')
    .text(d => d.duration > 3 ? d.value : '')

  // 시작점과 끝점 표시 (빨간색 텍스트)
  bars.append('text')
    .attr('x', d => xScale(d.start))
    .attr('y', yScale.bandwidth() / 2 + 25)
    .attr('text-anchor', 'middle')
    .attr('fill', 'red')
    .attr('font-size', '10px')
    .attr('font-weight', 'bold')
    .text(d => d.start)

  bars.append('text')
    .attr('x', d => xScale(d.start + d.duration))
    .attr('y', yScale.bandwidth() / 2 + 25)
    .attr('text-anchor', 'middle')
    .attr('fill', 'red')
    .attr('font-size', '10px')
    .attr('font-weight', 'bold')
    .text(d => d.start + d.duration)
}

// 이벤트 핸들러들
const handleTaskHover = (event, d) => {
  hoveredTask.value = d
  d3.select(event.target)
    .style('opacity', 1)
    .attr('stroke-width', 2)
  
  showTooltip(event, d)
}

const handleTaskLeave = (event) => {
  hoveredTask.value = null
  d3.select(event.target)
    .style('opacity', 0.8)
    .attr('stroke-width', 1)
  
  hideTooltip()
}

const handleTaskClick = (event, d) => {
  emit('task-clicked', d)
}

const handleRowSelect = (rowId) => {
  const index = selectedItems.value.indexOf(rowId)
  if (index > -1) {
    selectedItems.value.splice(index, 1)
  } else {
    selectedItems.value.push(rowId)
  }
  emit('row-selected', selectedItems.value)
}

// 툴팁 함수들
const showTooltip = (event, d) => {
  const tooltip = d3.select('body').append('div')
    .attr('class', 'gantt-tooltip')
    .style('position', 'absolute')
    .style('background', 'rgba(0,0,0,0.8)')
    .style('color', 'white')
    .style('padding', '8px')
    .style('border-radius', '4px')
    .style('font-size', '12px')
    .style('pointer-events', 'none')
    .style('z-index', '1000')
    .style('opacity', 0)

  tooltip.html(`
    <strong>${d.name}</strong><br/>
    시작: ${d.start}<br/>
    기간: ${d.duration}<br/>
    값: ${d.value}
  `)
    .style('left', (event.pageX + 10) + 'px')
    .style('top', (event.pageY - 10) + 'px')
    .transition()
    .duration(200)
    .style('opacity', 1)
}

const hideTooltip = () => {
  d3.selectAll('.gantt-tooltip').remove()
}

// D3 차트 그리기 함수 (SVG 부분만)
const drawChart = () => {
  if (!svgRef.value || !containerRef.value) return

  // 이전 차트 클리어
  d3.select(svgRef.value).selectAll("*").remove()

  const containerWidth = containerRef.value.offsetWidth
  const width = containerWidth - props.config.margin.left - props.config.margin.right
  const height = props.chartData.length * props.config.rowHeight + props.config.margin.top + props.config.margin.bottom

  // SVG 설정
  const svg = d3.select(svgRef.value)
    .attr('width', containerWidth)
    .attr('height', height)

  // 메인 그룹
  const g = svg.append('g')
    .attr('transform', `translate(${props.config.margin.left}, ${props.config.margin.top})`)

  // 스케일 설정
  const xScale = d3.scaleLinear()
    .domain([0, props.config.maxDuration])
    .range([0, width])

  const yScale = d3.scaleBand()
    .domain(props.chartData.map(d => d.id))
    .range([0, props.chartData.length * props.config.rowHeight])
    .padding(0.1)

  // 헤더 그리기
  drawHeader(svg, xScale)
  
  // 격자선 그리기
  drawGridlines(g, xScale)
  
  // 데이터 행 그리기 (바 차트 부분만)
  drawDataRows(g, xScale, yScale, width)
}

// 차트 업데이트 함수
const updateChart = () => {
  nextTick(() => {
    drawChart()
    emit('chart-updated')
  })
}

// 리사이즈 핸들러
const handleResize = () => {
  updateChart()
}

// 라이프사이클 훅
onMounted(() => {
  drawChart()
  window.addEventListener('resize', handleResize)
})

// 데이터 변경 감지
watch(() => props.chartData, updateChart, { deep: true })
watch(() => props.config, updateChart, { deep: true })

// 외부에서 사용할 수 있는 메서드들
const exportChart = () => {
  const svgElement = svgRef.value
  const serializer = new XMLSerializer()
  const svgString = serializer.serializeToString(svgElement)
  return svgString
}

const selectAllRows = () => {
  selectedItems.value = props.chartData.map(d => d.id)
}

const clearSelection = () => {
  selectedItems.value = []
}

// 외부에서 접근 가능한 메서드들 expose
defineExpose({
  updateChart,
  exportChart,
  selectAllRows,
  clearSelection,
  selectedItems,
  handleRowSelect
})
</script>

<style scoped>
.gantt-chart-container {
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

/* 메인 차트 영역 - 그리드 레이아웃 */
.chart-main {
  display: flex;
  border: 1px solid #e5e7eb;
  border-radius: 8px;
  overflow: hidden;
  background: white;
}

/* 왼쪽 정보 그리드 */
.info-grid {
  min-width: 350px;
  border-right: 2px solid #ddd;
  background: #fafafa;
}

.info-header {
  display: grid;
  grid-template-columns: 60px 1fr 80px 120px;
  background: #e9ecef;
  border-bottom: 2px solid #ddd;
  font-weight: bold;
}

.info-row {
  display: grid;
  grid-template-columns: 60px 1fr 80px 120px;
  border-bottom: 1px solid #e5e7eb;
  transition: background-color 0.2s;
  height: 50px; /* 차트의 rowHeight와 동일하게 */
}

.info-row:hover {
  background-color: #f1f3f4;
}

.info-row.selected {
  background-color: #e3f2fd;
  border-left: 4px solid #2196f3;
}

.info-cell {
  padding: 8px 12px;
  display: flex;
  align-items: center;
  font-size: 11px;
  border-right: 1px solid #e5e7eb;
}

.info-cell.header {
  background: #e9ecef;
  font-weight: bold;
  font-size: 12px;
  text-align: center;
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

/* 오른쪽 차트 영역 */
.chart-container {
  flex: 1;
  overflow-x: auto;
  background: white;
}

.legend {
  margin-top: 1rem;
  padding: 1rem;
  background-color: #f8f9fa;
  border-radius: 8px;
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
}

.legend-color.out {
  background-color: #87CEEB;
}

.legend-color.coating {
  background-color: #FFD700;
}

.legend-color.utz {
  background-color: #FFA500;
}

.legend-color.safety {
  background-color: #191970;
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

/* D3 관련 스타일 */
:deep(.tick line) {
  stroke: #ddd;
  stroke-dasharray: 2,2;
}

:deep(.tick text) {
  fill: #666;
}

:deep(.domain) {
  stroke: #333;
}
</style>