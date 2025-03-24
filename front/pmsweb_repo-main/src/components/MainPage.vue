<template>
  <v-app>    
    <HeaderBar @menuSelected="handleMenuSelection" />
    <v-container fluid class="pt-0 pr-0 pl-0">
      <v-row no-gutters style="height: 100vh;">

        <!-- 사이드 메뉴 -->
        <v-col cols="auto" class="side-menu-col" style="height: 100vh;">
          <SideMenu ref="sideMenu" @menu-clicked="handleMenuClick" />
        </v-col>

        <!-- 실제 콘텐츠 -->
        <v-col style="height: 100vh; overflow-y: auto;">
          <!-- 브레드크럼 및 제목 영역 css잘몰라서 강제로 위치맞춤..-->          
          <div class="breadcrum-div d-flex align-center text-body-2 ml-16 pl-10 pt-15">          

            <br><br><br><br>

            <v-icon size="small" class="mx-1">mdi-chevron-right</v-icon>          
            <span class="menu-text"> &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;
              {{ savedMidMenu }} </span>             
            <span class="menu-text"> <v-icon size="small" class="mx-1">mdi-chevron-right</v-icon> </span>
            <span class="menu-text"> {{ savedSubMenu }} </span>
          </div>                                  
                    
          <!-- 메인 컨텐츠 영역 -->
          <v-main class="main">
            <router-view></router-view>
          </v-main>
        </v-col>
      </v-row>
    </v-container>
  </v-app>
</template>
<script>
import { ref, onMounted } from 'vue';
import SideMenu from '@/components/SideMenu.vue';
import HeaderBar from '@/components/HeaderBar.vue';

export default {
  name: 'MainPage',
  components: {
    SideMenu,
    HeaderBar
  },
  setup() {
    const sideMenu = ref(null);
    // 반응형 변수로 선언
    const savedMidMenu = ref(null);
    const savedSubMenu = ref(null);
    
    // 로컬 스토리지 값을 확인하고 반응형 변수에 저장하는 함수
    const checkLocalStorage = () => {
      const midMenuFromStorage = localStorage.getItem('midMenu');
      const subMenuFromStorage = localStorage.getItem('subMenu');
      
      savedMidMenu.value = midMenuFromStorage ? JSON.parse(midMenuFromStorage) : null;
      savedSubMenu.value = subMenuFromStorage ? JSON.parse(subMenuFromStorage) : null;
      
      console.log('메뉴 클릭 후 midMenu:', savedMidMenu.value);
      console.log('메뉴 클릭 후 subMenu:', savedSubMenu.value);
    };
    
    // 초기 로드 시에도 값 확인
    onMounted(() => {
      checkLocalStorage();
    });
    
    const handleMenuClick = () => {
      // 메뉴 클릭 후 약간의 지연을 주어 로컬 스토리지가 업데이트될 시간을 확보
      setTimeout(() => {
        checkLocalStorage();
      }, 100);
    };
    
    const handleMenuSelection = (headerCode) => {
      console.log('Header menu selected:', headerCode);
      setTimeout(() => {
        if (sideMenu.value) {
          sideMenu.value.activateFirstSubmenuByHeader(headerCode);
        }
      }, 300);
    };
    
    // 템플릿에서 사용할 수 있도록 변수 반환
    return {
      sideMenu,
      handleMenuSelection,
      handleMenuClick,
      savedMidMenu,  // 템플릿에서 사용하기 위해 반환
      savedSubMenu   // 템플릿에서 사용하기 위해 반환
    };
  }
}
</script>

<style scoped>

  .main {
    padding-top: 0px;
    margin-top: -40px;
  }
  
  .menu-text {
    font-size: 12px;
    color: #A1A6A6;
  }

  .menu-text {
    font-size: 12px;
    color: #A1A6A6;
  }  
</style>