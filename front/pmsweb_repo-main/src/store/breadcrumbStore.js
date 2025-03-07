// src/store/breadcrumbStore.js
import { defineStore } from 'pinia'
import { ref } from 'vue'

export const useBreadcrumbStore = defineStore('breadcrumb', () => {
  // 브레드크럼 아이템 상태
  const breadcrumbs = ref([
    { title: '안전관광시', path: '/' }
  ])
  
  // 브레드크럼 업데이트 함수
  const updateBreadcrumbs = (newItems) => {
    // 첫 번째 아이템(홈)은 유지하고 나머지 아이템을 업데이트
    breadcrumbs.value = [
      { title: '안전관광시', path: '/' },
      ...newItems
    ]
  }
  
  // 단일 브레드크럼 추가 함수
  const addBreadcrumb = (title, path) => {
    // 이미 같은 경로가 있는지 확인
    const existingIndex = breadcrumbs.value.findIndex(item => item.path === path)
    
    if (existingIndex > 0) {
      // 이미 있는 경로라면 그 뒤의 항목들을 삭제
      breadcrumbs.value = breadcrumbs.value.slice(0, existingIndex + 1)
    } else if (existingIndex === -1) {
      // 새 항목 추가
      breadcrumbs.value.push({ title, path })
    }
  }
  
  // 브레드크럼 초기화 함수 (홈만 남김)
  const resetBreadcrumbs = () => {
    breadcrumbs.value = [{ title: '안전관광시', path: '/' }]
  }
  
  return {
    breadcrumbs,
    updateBreadcrumbs,
    addBreadcrumb,
    resetBreadcrumbs
  }
})