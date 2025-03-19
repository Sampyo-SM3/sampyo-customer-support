// breadcrumbStore.js
import { defineStore } from 'pinia';

export const useBreadcrumbStore = defineStore('breadcrumb', {
  state: () => ({
    // 현재 활성화된 메뉴 경로를 저장 (중메뉴, 소메뉴만 저장)
    currentMidMenu: null,
    currentSubMenu: null,
  }),

  getters: {
    // 현재 브레드크럼 경로 가져오기 (배열 형태)
    currentBreadcrumbs: (state) => {
      const result = [];
      if (state.currentMidMenu) result.push(state.currentMidMenu);
      if (state.currentSubMenu) result.push(state.currentSubMenu);
      return result;
    },
    
    // 중메뉴 가져오기
    midMenu: (state) => state.currentMidMenu,
    
    // 소메뉴 가져오기
    subMenu: (state) => state.currentSubMenu,
  },

  actions: {
    // 메뉴 선택 시 브레드크럼 설정 (사이드메뉴 클릭 시 호출)
    setMenuPath(midMenu, subMenu = null) {
      this.currentMidMenu = midMenu;
      this.currentSubMenu = subMenu;
      
      // 로컬 스토리지에 저장
      this.saveToLocalStorage();
    },
    
    // 중메뉴만 설정
    setMidMenu(midMenu) {
      this.currentMidMenu = midMenu;
      
      // 로컬 스토리지에 저장
      this.saveToLocalStorage();
    },
    
    // 소메뉴만 설정
    setSubMenu(subMenu) {
      this.currentSubMenu = subMenu;
      
      // 로컬 스토리지에 저장
      this.saveToLocalStorage();
    },
    
    // 브레드크럼 초기화
    clearBreadcrumbs() {
      this.currentMidMenu = null;
      this.currentSubMenu = null;
      this.saveToLocalStorage();
    },
    
    // 로컬 스토리지에 저장
    saveToLocalStorage() {
      localStorage.setItem('midMenu', JSON.stringify(this.currentMidMenu));
      localStorage.setItem('subMenu', JSON.stringify(this.currentSubMenu));
    },
    
    // 로컬 스토리지에서 불러오기
    loadFromLocalStorage() {
      const savedMidMenu = localStorage.getItem('midMenu');
      const savedSubMenu = localStorage.getItem('subMenu');
      
      if (savedMidMenu) {
        this.currentMidMenu = JSON.parse(savedMidMenu);
      }
      
      if (savedSubMenu) {
        this.currentSubMenu = JSON.parse(savedSubMenu);
      }
    },
  },
});