<template>
  <v-app-bar class="custom-elevation">
    <v-container fluid class="d-flex align-center pl-4">

      <!-- 로고 및 회사명 -->
      <v-img src="@/assets/sam_logo.jpg" max-height="260" max-width="260" class="mr-2">
      </v-img>

      <!-- 메뉴 아이템들 -->
      <v-tabs v-model="activeTab">
        <v-tab class="custom-tab" :ripple="false" @click="handleMenuClick(item)" v-for="item in menuItems" :key="item"
          :value="item">
          <p class="tab-text">{{ item.m_name }}</p>
        </v-tab>

        <!-- Business 탭 추가 -->
        <!-- <v-btn
          class="business-btn"
          variant="text"
        >
          <p class="business-text">Business</p>
          <v-icon right>mdi-open-in-new</v-icon>
        </v-btn>    -->

      </v-tabs>

      <v-spacer></v-spacer>

      <!-- 검색바 -->
      <!-- <v-text-field 
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
        class="custom-searchbar"/>            -->

      <!-- 언어 선택 드롭다운 -->
      <!-- <v-menu offset-y>
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
  <v-list-item v-for="(language, index) in languages" :key="index">
    <v-list-item-title>{{ language }}</v-list-item-title>
  </v-list-item>
</v-list>
</v-menu> -->

      <!-- 알림 아이콘 -->
      <!-- <v-btn icon class="mr-4">
        <v-icon>mdi-bell-outline</v-icon>        
      </v-btn>       -->

      <div class="pr-5">
        <v-icon>
          <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 100 100" width="24" height="24">
            <circle cx="50" cy="30" r="20" :fill="iconColor" />
            <circle cx="50" cy="30" r="15" fill="#f8f9fa" />
            <path d="M30,80 C30,65 70,65 70,80" :fill="iconColor" :stroke="iconColor" stroke-width="1" />
          </svg>
        </v-icon>
        {{ userNameDisplay }}
      </div>


      <!-- 로그인 버튼 -->

      <v-btn class="login-btn" @click="handleLoginLogout">
        <p class="login-text">{{ userLoginStatus ? '로그아웃' : '로그인' }}</p>
      </v-btn>


    </v-container>
  </v-app-bar>
</template>


<script>
import { ref, defineComponent, onMounted, watch, provide } from 'vue';
import { storeToRefs } from 'pinia';
import { useAuthStore } from '@/store/auth';
import { useMenuStore } from '@/store/menuStore';
import { useRouter } from 'vue-router';  // useRouter 임포트
import apiClient from '@/api';



export default defineComponent({
  name: 'HeaderBar',
  emits: ['navigate', 'menuSelected'],
  setup(props, { emit }) {
    const menuStore = useMenuStore()
    const { isLoading, error } = storeToRefs(menuStore);
    const activeTab = ref(null)
    const menuItems = ref([])

    const userIdDisplay = ref('');
    const userNameDisplay = ref('');
    const userLoginStatus = ref('');

    // auth 스토어 사용
    const authStore = useAuthStore();

    // 라우터 사용
    const router = useRouter();

    // 로그인/로그아웃 처리 함수
    const handleLoginLogout = () => {
      if (userLoginStatus.value) {
        // 로그인 상태일 경우 로그아웃
        authStore.logout();  // Pinia의 logout 호출
        userLoginStatus.value = false;  // 로그인 상태 반영

        router.push({ name: 'Login' });
      } else {
        // 로그인 상태가 아닐 경우 로그인 (로그인 로직은 추가적으로 구현 필요)
        // userLoginStatus.value = true;
        // authStore.login();  // Pinia의 login 호출
      }
    };

    const showSideMenu = (item) => {
      // console.log('--------showSideMenu--------')
      // console.log('item-> ', item)

      // 로컬 저장소에서 user_id 가져오기
      const userId = localStorage.getItem('user_id');
      if (userId) {
        menuStore.fetchMenuData(item.m_code, userId);  // user_id를 사용하여 메뉴 데이터 가져오기
      } else {
        console.error('User ID is not found in localStorage');
      }
    }

    const fetchMenuData = async (id) => {
      // console.log('----------fetchMenuData2----------')
      try {
        isLoading.value = true
        const response = await apiClient.get('/api/menuitem', {
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
          // console.log('menuItems.value -> ', menuItems.value)
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
      // 로컬 저장소에서 user_id 가져오기
      const userId = localStorage.getItem('user_id');
      if (userId) {
        fetchMenuData(userId)  // user_id로 메뉴 데이터 가져오기
      } else {
        console.error('User ID is not found in localStorage');
      }

      userIdDisplay.value = localStorage.getItem('user_id');
      userLoginStatus.value = localStorage.getItem('isAuthenticated');

      const retrievedData = localStorage.getItem('userInfo');
      const userInfo = JSON.parse(retrievedData);
      userNameDisplay.value = userInfo.name; // '관리자'                           
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





    // 사이드메뉴 참조를 위한 설정
    const sideMenuRef = ref(null);
    provide('sideMenuRef', sideMenuRef); // 자식 컴포넌트에서 접근 가능하도록 provide    

    // 메뉴 클릭 핸들러 수정
    const handleMenuClick = (item) => {
      // console.log('--------handleMenuClick--------');
      // console.log('item-> ', item);

      // 기존 사이드메뉴 데이터 로드 함수 호출
      showSideMenu(item);

      // 헤더 메뉴 클릭 이벤트 발생 - 부모 컴포넌트에 알림
      emit('menuSelected', item.m_code);
    };




    return {
      error,
      menuItems,
      activeTab,
      handleMenuClick,
      sideMenuRef,
      TextDecoderStream,
      userIdDisplay,
      userNameDisplay,
      userLoginStatus,
      handleLoginLogout,
    }
  }
})
</script>




<style scoped>
.login-btn {
  background-color: #1867C0;
  border-radius: 10px !important;
  color: white;
  width: 80px;
  /* 원하는 길이로 조정 */
}

.login-text {
  transform: translateY(-1px);
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
.custom-tab.v-tab--selected {
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
  font-size: 14px;
  color: #737577;

}

/* 버튼 오버레이 효과 제거 */
::v-deep.custom-tab .v-btn__overlay {
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
  color: #00B0F3 !important;
  /* Business 텍스트 색상 */
}

.business-btn .v-icon {
  margin-left: 7px;
  font-size: 18px;
}

.business-text {
  letter-spacing: 0px
}
</style>