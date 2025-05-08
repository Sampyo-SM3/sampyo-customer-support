<template>
  <v-navigation-drawer class="custom-drawer" permanent width="270">
    <v-list class="tighter-menu-spacing">
      <template v-for="item in processedMenuItems" :key="item.M_CODE">
        <!-- 대메뉴 -->
        <div v-if="item.LEV === 2" class="category-label">{{ item.M_NAME }}</div>

        <!-- 중메뉴 -->
        <template v-else-if="item.LEV === 3">
          <v-list-item v-if="isClickable(item.M_CODE) && (!item.children || item.children.length === 0)"
            @click="activateMenuItem(item)" :class="['menu-item', 'no-submenu', { 'active-item': item.isActive }]">
            <template v-slot:prepend>
              <v-icon :icon="item.M_ICON" class="custom-menu-icon"></v-icon>
            </template>
            <template v-slot:title>
              <span class="custom-menu-title">{{ item.M_NAME }}</span>
            </template>
          </v-list-item>

          <v-list-group v-else v-model="item.isOpen">
            <template v-slot:activator="{ props }">
              <v-list-item v-bind="props" :prepend-icon="item.M_ICON"
                :append-icon="item.children.length ? (item.isOpen ? 'mdi-menu-up' : 'mdi-menu-down') : ''"
                class="menu-item" @click="toggleMenu(item)">
                <template v-slot:title>
                  <span class="menu-item-text">{{ item.M_NAME }}</span>
                </template>
                <template v-slot:append>
                  <v-icon class="list-icon" :icon="item.isOpen ? 'mdi-menu-up' : 'mdi-menu-down'"></v-icon>
                </template>
              </v-list-item>
            </template>

            <!-- 소메뉴 -->
            <v-list-item v-for="subItem in item.children" :key="subItem.M_CODE" @click="activateMenuItem(subItem)"
              :class="['sub-menu-item', { 'active-item': subItem.isActive }]">
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
import { useBreadcrumbStore } from '@/store/breadcrumbStore';

export default defineComponent({
  name: 'SideMenu',
  emits: ['navigate'],
  setup(props, { emit }) {
    const router = useRouter();
    const menuStore = useMenuStore();
    const { menuData, isLoading, error } = storeToRefs(menuStore);
    const auth = ref('CA');
    const id = ref('아이디!!');
    const isFirstLoad = ref(true);

    onMounted(async () => {
      await menuStore.fetchMenuData(auth.value, id.value);

      watch(menuData, () => {
        if (isFirstLoad.value && menuData.value.length > 0) {
          const savedSubMenu = localStorage.getItem('subMenu');
          const cleanSubMenu = savedSubMenu ? savedSubMenu.replace(/^"|"$/g, '') : null;

          if (cleanSubMenu) {
            console.log('✅ Saved subMenu from localStorage:', cleanSubMenu);

            menuData.value.forEach(menuItem => {
              menuItem.isActive = false;
              if (menuItem.children && menuItem.children.length > 0) {
                menuItem.children.forEach(child => {
                  child.isActive = (child.M_NAME === cleanSubMenu);
                });
              } else if (menuItem.LEV === 3) {
                menuItem.isActive = (menuItem.M_NAME === cleanSubMenu);
              }
            });
          }

          isFirstLoad.value = false;
        }
      });
    });

    const processedMenuItems = computed(() => {
      if (isLoading.value || menuData.value.length === 0) return [];
      const result = [];
      const level3Map = new Map();

      menuData.value.forEach(item => {
        if (item.LEV === 2) {
          result.push(item);
        } else if (item.LEV === 3) {
          item.children = [];
          item.isOpen = false;
          level3Map.set(item.M_CODE, item);
          result.push(item);
        } else if (item.LEV === 4) {
          const parentCode = item.M_CODE.substring(0, item.M_CODE.lastIndexOf('_'));
          if (level3Map.has(parentCode)) {
            level3Map.get(parentCode).children.push(item);
          }
        }
      });

      return result;
    });

    const activateMenuItem = (item) => {
      const level3Name = item.M_NAME;
      const level2Name = getLevel2MenuName(item.M_CODE);

      const breadcrumbStore = useBreadcrumbStore();
      breadcrumbStore.setMenuPath(level2Name, level3Name);

      menuData.value.forEach(menuItem => {
        menuItem.isActive = false;
        if (menuItem.children) {
          menuItem.children.forEach(child => child.isActive = false);
        }
      });

      item.isActive = true;

      if (item.M_CODE.includes('_')) {
        const path = `/views/${item.M_CODE.substring(0, 2)}/${item.M_CODE}`;
        router.push(path);
      }

      emit('menu-clicked', item);
    };

    const getLevel2MenuName = (mCode) => {
      if (!mCode || typeof mCode !== 'string') return '';
      const parentCode = mCode.substring(0, 4);
      const level2Item = menuData.value.find(item => item.LEV === 2 && item.M_CODE === parentCode);
      return level2Item ? level2Item.M_NAME : '';
    };

    const isClickable = (mCode) => mCode.includes('_');

    const toggleMenu = (item) => {
      processedMenuItems.value.forEach(menuItem => {
        if (menuItem !== item) menuItem.isOpen = false;
      });
      item.isOpen = !item.isOpen;
    };

    const activateFirstSubmenuByHeader = () => {
      if (!menuData.value.length) return;
      const targetMenu = menuData.value.find(item => (item.LEV === 3 || item.LEV === 4) && isClickable(item.M_CODE));
      if (targetMenu) activateMenuItem(targetMenu);
    };

    return {
      processedMenuItems,
      activateMenuItem,
      getLevel2MenuName,
      isClickable,
      toggleMenu,
      error,
      auth,
      id,
      menuData: computed(() => menuStore.menuData),
      activateFirstSubmenuByHeader,
    };
  }
});
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
  /* text-transform: uppercase; */
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
  background-color: #B0CAE6;

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
  background-color: #B0CAE6;
  font-weight: bold;
  border-radius: 8px !important;
  margin-left: 10px;
  margin-right: 10px;
  /* color: white !important;  */
}

:deep(.menu-item.no-submenu.active-item) * {
  color: #1867C0 !important;
}

:deep(.custom-menu-title) {
  color: #5A5C5F !important;
  transition: padding-left 0.3s ease;
  font-weight: 600;
  font-size: 13px;
  color: #003044;
  line-height: 3 !important;
  padding-left: 20px;
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
  background-color: #F0F4F8;
  /* background-color: red; */
  height: 100vh !important;
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