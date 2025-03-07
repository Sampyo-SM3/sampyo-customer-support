<template>  
  <v-app-bar class="custom-elevation">            
      <v-container fluid class="d-flex align-center pl-4">
        
      <!-- 로고 및 회사명 -->
      <v-img 
        src="@/assets/sam_logo.jpg" 
        max-height="260" 
        max-width="260" 
        class="mr-2">        
      </v-img>        

      <!-- 메뉴 아이템들 -->
      <v-tabs v-model="activeTab">
        <v-tab class="custom-tab" :ripple="false" @click="showSideMenu(item)" v-for="item in menuItems" :key="item" :value="item" >
          <p class="tab-text">{{ item.m_name }}</p>          
        </v-tab>

        <!-- Business 탭 추가 -->
        <v-btn
          class="business-btn"
          variant="text"
        >
          <p class="business-text">Business</p>
          <v-icon right>mdi-open-in-new</v-icon>
        </v-btn>   
        
      </v-tabs>

      <v-spacer></v-spacer>

      <!-- 검색바 -->               
      <v-text-field 
        rounded="lg"        
        v-model="search"
        placeholder="검색"
        prepend-inner-icon="mdi-magnify"
        filled        
        dense
        clearable     
        hide-details
        variant="outlined"                   
        density="compact"
        class="custom-searchbar"/>           
                
      <!-- 언어 선택 드롭다운 -->
      <v-menu offset-y>
        <template v-slot:activator="{ props }">
          <v-btn
            v-bind="props"
            text
            class="language-select px-0 mr-2"
          >
            <p class="lang-text">한국어</p>
            <v-icon right>mdi-chevron-down</v-icon>
          </v-btn>
        </template>
        <v-list>
          <v-list-item
            v-for="(language, index) in languages"
            :key="index"            
          >
            <v-list-item-title>{{ language }}</v-list-item-title>
          </v-list-item>
        </v-list>
      </v-menu>

      <!-- 알림 아이콘 -->
      <v-btn icon class="mr-4">
        <v-icon>mdi-bell-outline</v-icon>
        
      </v-btn>      

      <!-- 로그인 버튼 -->
      <v-btn class="login-btn">
        <p class="login-text">로그인</p>
      </v-btn>
    </v-container>
  </v-app-bar>
</template>


<script>
import { ref, defineComponent, onMounted, watch } from 'vue';
import { storeToRefs } from 'pinia';
import { useMenuStore } from '@/store/menuStore';
import axios from 'axios'

export default defineComponent({
  name: 'HeaderBar',
  emits: ['navigate'],
  setup() {          
    const menuStore = useMenuStore()    
    const { isLoading, error } = storeToRefs(menuStore);  
    const activeTab = ref(null)        
    const menuItems = ref([]) 

    const showSideMenu = (item) => {
      console.log('--------showSideMenu--------')
      console.log('item-> ', item)
   
      menuStore.fetchMenuData(item.m_code, 'javachohj')
    }    

    const fetchMenuData = async (id) => {
      console.log('----------fetchMenuData2----------')
      try {        
        isLoading.value = true        
        const response = await axios.get('http://localhost:8080/api/menuitem', {
          params: {
            auth: '',
            id: id
          }
        });        

        if (response.data && Array.isArray(response.data.menuItems)) {
          menuItems.value = response.data.menuItems.map(item => ({
            m_code: item.mcode,
            m_name: item.mname
          }))
          console.log('menuItems.value -> ', menuItems.value)
        } else {
          throw new Error('Invalid data structure or menuItems is not an array')
        }                
      } catch (error) {
        console.error('Error fetching menu data:', error)
        menuItems.value = []
      } finally {
        isLoading.value = false
      }
    }    
    
    onMounted(() => {            
      fetchMenuData('javachohj')
    })

    // 메뉴 아이템이 로드된 후 첫 번째 항목의 클릭 이벤트 트리거
    watch(menuItems, (newMenuItems) => {
      if (newMenuItems.length > 0) {
        // 첫 번째 메뉴 아이템 선택
        activeTab.value = newMenuItems[0]
        // 첫 번째 메뉴 아이템의 showSideMenu 함수 호출
        showSideMenu(newMenuItems[0])
      }
    }, { immediate: true })

    return {      
      error,  
      menuItems,
      activeTab,
      showSideMenu,
    }
  }
})
</script>




<style scoped>

.login-btn {  
  background-color: #00B0F3;
  color: white;  
  border-radius: 10px !important; /* 원하는 수치값으로 조정하세요 */     
  width: 10px;   
}

.login-text {
  transform: translateY(-1px);
}

.login-btn {
  border-radius: 8px !important;
}

.language-select {
  text-transform: none !important;
  letter-spacing: normal !important;
  font-weight: normal !important;
}

.language-select ::v-deep .v-btn__content {
  justify-content: space-between;
  width: 100%;
}

.language-select .v-icon {
  margin-left: 4px;  
}

.lang-text {
  font-weight: 600;
}


::v-deep.custom-searchbar {
  margin-right: 30px;
  /* color: #52555a !important; */
  color: #52555a !important;
  max-width: 320px;    
  
}

::v-deep.v-text-field input {
    font-size: 12px;    
  }

.custom-tab:hover,
.custom-tab:active,
.custom-tab:focus,
.custom-tab.v-tab--selected
{
  /* 대메뉴 폰트 색상 */
  color: #1976D2 !important;
  background-color: transparent !important;
}

/* 배경 효과를 생성하는 가상 요소들 제거 */
.custom-tab::before,
.custom-tab::after {  
  opacity: 0 !important;
  background-color: transparent !important;
}

/* 헤더 메뉴 스타일 */
.custom-tab {  
  font-weight: 600;     
  font-size: 13px;
  color: #003044;  

}

/* 버튼 오버레이 효과 제거 */
::v-deep.custom-tab .v-btn__overlay
{
  opacity: 0 !important;
}

/* 탭 슬라이더(하단 막대) 제거 */
::v-deep.custom-tab .v-tab__slider {
  display: none !important;
}

.custom-elevation {
  box-shadow: 0 1px 1px rgba(0, 0, 0, 0.08) !important  
} 

.business-btn {
  margin-top: 14px;  
  text-transform: none;
  font-weight: 600;
  color: #00B0F3 !important; /* Business 텍스트 색상 */
}

.business-btn .v-icon {
  margin-left: 7px;  
  font-size: 18px;
}  

.business-text {
  letter-spacing: 0px
}




</style>