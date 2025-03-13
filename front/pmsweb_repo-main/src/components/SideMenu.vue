<template>
  <v-navigation-drawer class="custom-drawer" permanent width="270">
    <v-list class="tighter-menu-spacing">
      <template v-for="item in processedMenuItems" :key="item.M_CODE">
        <!-- 대메뉴 (LEV 2) -->
        <div v-if="item.LEV === 2" class="category-label">{{ item.M_NAME }}</div>

        <!-- 중메뉴 (LEV 3) -->
        <template v-else-if="item.LEV === 3">
          <v-list-item v-if="isClickable(item.M_CODE) && (!item.children || item.children.length === 0)"
            @click="activateMenuItem(item)" :class="['menu-item', 'no-submenu', { 'active-item': item.isActive }]">
            <template v-slot:prepend>
              <!-- <v-icon :icon="getMenuIcon(item.M_NAME)" class="custom-menu-icon"></v-icon>               -->
              <v-icon :icon=item.M_ICON class="custom-menu-icon"></v-icon>
            </template>
            <template v-slot:title>
              <span class="custom-menu-title">{{ item.M_NAME }}</span>
            </template>
          </v-list-item>

          <v-list-group v-else v-model="item.isOpen">

            <template v-slot:activator="{ props }">
              <!-- 중메뉴 부분 -->
              <!-- :prepend-icon="getMenuIcon(item.M_NAME)" -->
              <v-list-item v-bind="props" :prepend-icon=item.M_ICON
                :append-icon="item.children && item.children.length ? (item.isOpen ? 'mdi-menu-up' : 'mdi-menu-down') : ''"
                class="menu-item" @click="toggleMenu(item)">
                <template v-slot:title>
                  <span class="menu-item-text">{{ item.M_NAME }}</span>
                </template>
                <template v-slot:append>
                  <v-icon class="list-icon" :icon="item.isOpen ? 'mdi-menu-up' : 'mdi-menu-down'">
                  </v-icon>
                </template>
              </v-list-item>
            </template>

            <!-- 소메뉴 (LEV 4) -->
            <v-list-item v-for="subItem in item.children" :key="subItem.M_CODE" :title="subItem.M_NAME"
              @click="activateMenuItem(subItem)" :class="['sub-menu-item', { 'active-item': subItem.isActive }]">
              <!-- :to="{ name: subItem.M_CODE }" -->
              <template v-slot:title>
                <span class="sub-menu-text">{{ subItem.M_NAME }}</span>
              </template>
            </v-list-item>
          </v-list-group>
        </template>
      </template>
    </v-list>
  </v-navigation-drawer>
</template>

<script>
import { computed, defineComponent, onMounted, watch, ref } from 'vue';
import { storeToRefs } from 'pinia';
import { useMenuStore } from '@/store/menuStore';
import { useRouter } from 'vue-router';

export default defineComponent({
  name: 'SideMenu',
  emits: ['navigate'],
  setup() {
    const router = useRouter()
    const menuStore = useMenuStore()
    const { menuData, isLoading, error } = storeToRefs(menuStore);
    const auth = ref('CA');
    const id = ref('아이디!!아직 아이디별 권한 관리는 안됨');
    const isFirstLoad = ref(true);

    onMounted(async () => {
      await menuStore.fetchMenuData(auth.value, id.value)



      watch(menuData, (newValue) => {
        console.log('menuData changed:', newValue);

        // 첫 로드시에만 실행
        if (isFirstLoad.value && menuData.value && menuData.value.length > 0) {
          const firstClickableMenu = menuData.value.find(item =>
            item.LEV === 3 && isClickable(item.M_CODE)
          );

          if (firstClickableMenu) {
            activateMenuItem(firstClickableMenu);
            isFirstLoad.value = false;  // 플래그 변경
          }
        }
      });


    });

    const processedMenuItems = computed(() => {
      console.log('----------processedMenuItems()-----------')

      if (isLoading.value || menuData.value.length === 0) {
        return []
      }

      const result = []
      const level3Map = new Map()

      menuData.value.forEach(item => {
        console.log(item.M_NAME)
        if (item.LEV === 2) {
          result.push(item)
        } else if (item.LEV === 3) {
          item.children = []
          item.isOpen = false
          level3Map.set(item.M_CODE, item)
          result.push(item)
        } else if (item.LEV === 4) {
          const parentCode = item.M_CODE.substring(0, item.M_CODE.lastIndexOf('_'))
          if (level3Map.has(parentCode)) {
            level3Map.get(parentCode).children.push(item)
          }
        }
      })
      // console.log('지금 테스트!! -> ', result)
      return result
    })

    const activateMenuItem = (item) => {
      console.log(`Activate menu item: ${item.M_NAME}, Code: ${item.M_CODE}`)
      menuData.value.forEach(menuItem => {
        menuItem.isActive = false
        if (menuItem.children) {
          menuItem.children.forEach(child => child.isActive = false)
        }
      })
      item.isActive = true

      // 라우터를 통해 해당 컴포넌트로 이동
      if (item.M_CODE.includes('_')) {
        const path = `/views/${item.M_CODE.substring(0, 2)}/${item.M_CODE}`
        router.push(path)
      }
    }


    const isClickable = (mCode) => {
      return mCode.includes('_')
    }

    const toggleMenu = (item) => {
      console.log('----------toggleMenu----------')
      console.log('processedMenuItems.value[1] -> ', processedMenuItems.value[1])
      console.log('processedMenuItems.value[4] -> ', processedMenuItems.value[4])

      processedMenuItems.value.forEach(menuItem => {
        if (menuItem !== item) {
          menuItem.isOpen = false;
        }
      });

      console.log('클릭 후')
      console.log('processedMenuItems.value[1] -> ', processedMenuItems.value[1])
      console.log('processedMenuItems.value[4] -> ', processedMenuItems.value[4])

      item.isOpen = !item.isOpen;

      console.log('item.isOpen -> ', item.isOpen)
    }




    return {
      processedMenuItems,
      activateMenuItem,
      isClickable,
      toggleMenu,
      error,
      auth,
      id,
      menuData: computed(() => menuStore.menuData),
    }
  }
})
</script>

<style scoped>
/* 대메뉴 스타일 */
.category-label {
  padding: 8px 16px;
  /* font-weight: normal; */
  font-weight: 500;
  color: #9e9e9e;
  /* 옅은 회색 */
  font-size: 12px;
  /* 더 작은 폰트 크기 */
  text-transform: uppercase;
}

:deep(.menu-item) {
  padding-inline-start: 28px !important;
}

:deep(.menu-item-text) {
  /* 중메뉴 폰트 굵게 */
  font-weight: 600;
  font-size: 13px;
  color: #003044;
  /* color: black; */
  line-height: 3 !important;
}

:deep(.menu-item .v-list-item__prepend) {
  /* 아이콘과 텍스트 사이 간격 줄임 */
  margin-right: -29px;
  /* transform: translateX(); */
}

:deep(.sub-menu-item) {
  /* 소메뉴 들여쓰기 */
  padding-left: 20px !important;
  margin-left: 10px;
  margin-right: 20px;
  border-radius: 8px !important;

}

/* 소메뉴 스타일 */
.sub-menu-text {
  padding-left: 10px;
  /* transform: translateY(-100px) !important;   */
  font-weight: 500;
  font-size: 13px;
  color: #5B737D;
  line-height: 3 !important;
}


/* Vuetify의 기본 스타일 덮어쓰기 */
:deep(.v-list-item__prepend > .v-icon) {
  margin-inline-end: 8px;
  /* 아이콘과 텍스트 사이 간격 추가 조정 */
}

.active-item {
  /* 활성 항목 배경색 (원하는 색상으로 변경 가능) */
  /* background-color: #e3f2fd;  */
  background-color: #00B0F0;

}

/* .active-item .menu-item-text, */
.active-item .sub-menu-text {
  color: white;
  /* 활성 항목 텍스트 색상 (원하는 색상으로 변경 가능) */

}

.active-item .menu-item-text {
  color: white;
  /* 활성 항목 텍스트 색상 (원하는 색상으로 변경 가능) */
}

/* 하위메뉴가 있는 중메뉴 클릭 후 스타일 */
:deep(.v-list-item--active) {
  /* Vuetify의 기본 활성 스타일 제거 */
  background-color: transparent !important;
}

:deep(.v-list-item--active:hover) {
  /* 활성 항목에 대한 호버 효과 */
  background-color: #e3f2fd !important;
}

.list-icon {
  font-size: 37px !important;
  color: black;
}

/* 하위메뉴가 없는 중메뉴 클릭 시 스타일 수정 */
:deep(.menu-item.no-submenu.active-item) {
  background-color: #00B0F0;
  font-weight: bold;
  border-radius: 8px !important;
  margin-left: 10px;
  margin-right: 10px;
  /* color: white !important;  */
}

:deep(.menu-item.no-submenu.active-item) * {
  color: white !important;
}

:deep(.custom-menu-title) {
  transition: padding-left 0.3s ease;
  font-weight: 600;
  font-size: 13px;
  color: #003044;
  line-height: 3 !important;
}

:deep(.menu-item.no-submenu.active-item .custom-menu-icon) {
  color: white;
  /* 아이콘 색상을 흰색으로 변경 */
}

/* :deep(.custom-menu-icon) {
  transition: margin-left 0.3s ease;
}

:deep(.custom-menu-title) {
  transition: padding-left 0.3s ease;
} */

:deep(.menu-item.no-submenu.active-item .custom-menu-icon) {
  margin-left: -10px;
  /* 활성 상태일 때 아이콘을 왼쪽으로 10px 이동 */
}

/* :deep(.menu-item.no-submenu.active-item .custom-menu-title) {
  padding-left: 10px; 
} */

.custom-drawer {
  border-right: 1px solid rgba(0, 0, 0, 0.08) !important;
}

:deep(.tighter-menu-spacing) {
  --v-list-item-min-height: 10px !important;
  --v-list-item-padding-top: 0 !important;
  --v-list-item-padding-bottom: 0 !important;
}

:deep(.tighter-menu-spacing .v-list-item) {
  min-height: var(--v-list-item-min-height) !important;
  padding-top: var(--v-list-item-padding-top) !important;
  padding-bottom: var(--v-list-item-padding-bottom) !important;
}

:deep(.tighter-menu-spacing .v-list-item__content) {
  padding: 2px 0 !important;
}
</style>