<template>
  <div class="gantt-chart-container">
    <!-- 메인 차트 영역 - 그리드와 SVG 조합 -->
    <div class="chart-main">           
      <div ref="containerRef" class="chart-container">
        <svg ref="svgRef"></svg>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted, watch, nextTick } from 'vue'
import * as d3 from 'd3'

// Props 정의
const props = defineProps({
  chartData: {
    type: Array
  },
  config: {
    type: Object   
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
      .attr('y', -11)
      .attr('width', endX - startX)
      .attr('height', 36)
      .attr('fill', phase.color)
      .attr('stroke', '#fff')
      .attr('stroke-width', 1)

    headerGroup.append('text')
      .attr('x', startX + (endX - startX) / 2)
      .attr('y', 12)
      .attr('text-anchor', 'middle')
      .attr('fill', 'white')
      .attr('font-size', '12px')
      .attr('font-weight', 'bold')
      .text(phase.name)
  })

// 시간축
  const timeAxis = d3.axisTop(xScale)
    .tickValues(d3.range(5, props.config.maxDuration + 1, 5)) // 0을 제외하고 5부터 시작
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

  // 행 배경 - 모든 행을 흰색으로 통일
  rows.append('rect')
    .attr('x', 0)
    .attr('y', 0)
    .attr('width', width)
    .attr('height', props.config.rowHeight)
    .attr('fill', 'white') // 모든 행을 흰색으로 통일
    .attr('stroke', '#eee')

  // 작업 바
  const bars = rows.selectAll('.task-bar')
    .data(d => d.tasks.map(task => ({...task, parentId: d.id})))
    .enter()
    .append('g')
    .attr('class', 'task-bar')

  bars.append('rect')
    .attr('x', d => xScale(d.start))
    .attr('y', (props.config.rowHeight - props.config.barHeight) / 2)
    .attr('width', d => xScale(d.duration))
    .attr('height', props.config.barHeight)
    .attr('fill', d => d.color)
    .attr('stroke', function(d, i, nodes) {
      // 같은 행(parentId)의 다른 막대들과 비교
      const currentParentId = d.parentId;
      const allBarsInRow = d3.selectAll(nodes).data();
      const barsInSameRow = allBarsInRow.filter(bar => bar.parentId === currentParentId);
      
      // 현재 막대의 끝점과 다른 막대의 시작점이 같은지 확인
      const currentEnd = d.start + d.duration;
      const hasConnectedBar = barsInSameRow.some(bar => 
        bar !== d && (bar.start === currentEnd || d.start === bar.start + bar.duration)
      );
      
      return hasConnectedBar ? 'none' : '#fff';
    })
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
    .attr('y', props.config.rowHeight / 2 + 4)
    .attr('text-anchor', 'middle')
    .attr('fill', 'white')
    .attr('font-size', '10px')
    .attr('font-weight', 'bold')
    .text(d => d.duration > 3 ? d.value : '')

  // 시작점과 끝점 표시 로직
  bars.each(function(d) {
    const bar = d3.select(this);
    const startX = xScale(d.start);
    const endX = xScale(d.start + d.duration);
    const showStart = d.start > 0 && d.duration >= 1;
    const showEnd = d.start + d.duration > 0;
    
    // 시작점과 끝점이 겹치는지 확인 (20px 이내면 겹친다고 판단)
    const isOverlapping = showStart && showEnd && Math.abs(endX - startX) < 20;
    
    if (showStart) {
      // 시작점 표시 - 겹치면 막대 위쪽, 안 겹치면 아래쪽
      bar.append('text')
        .attr('x', startX)
        .attr('y', isOverlapping ? 
          (props.config.rowHeight - props.config.barHeight) / 2 - 5 : // 막대 위쪽
          props.config.rowHeight / 2 + 25) // 막대 아래쪽
        .attr('text-anchor', 'middle')
        .attr('fill', 'red')
        .attr('font-size', '8px')
        .attr('font-weight', 'bold')
        .text(d.start);
    }
    
    if (showEnd) {
      // 끝점 표시 - 겹치면 막대 아래쪽, 안 겹치면 아래쪽
      bar.append('text')
        .attr('x', endX)
        .attr('y', isOverlapping ? 
          props.config.rowHeight / 2 + 25 : // 막대 아래쪽 (겹칠 때)
          props.config.rowHeight / 2 + 25) // 막대 아래쪽 (안 겹칠 때)
        .attr('text-anchor', 'middle')
        .attr('fill', 'red')
        .attr('font-size', '8px')
        .attr('font-weight', 'bold')
        .text(d.start + d.duration);
    }
  });
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
  // right margin을 0으로 설정하여 오른쪽 공간 완전히 제거
  const width = containerWidth - props.config.margin.left
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
    .padding(0)

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
  height: 100px; /* 그래프 하단에 공백 생기길래 강제로 줄임 */
  font-family: 'Arial', sans-serif;
}

/* 메인 차트 영역 - 간격 완전 제거 */
.chart-main {
  display: flex;
  overflow: hidden;
  background: white;
  border: none;
}

/* 오른쪽 차트 영역 */
.chart-container {
  flex: 1;
  overflow-x: auto;
  background: white;
  margin: 0;
  padding: 0;
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