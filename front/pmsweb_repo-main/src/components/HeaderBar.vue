<template>
  <v-app-bar class="custom-elevation">
    <v-container fluid class="d-flex align-center pl-4">
      <v-img src="@/assets/SAMPYO.png" max-height="130" max-width="130" class="mr-2" />

      <v-tabs v-model="activeTab">
        <v-tab class="custom-tab" :ripple="false" @click="handleMenuClick(item)" v-for="item in menuItems"
          :key="item.m_code" :value="item">
          <p class="tab-text">{{ item.m_name }}</p>
        </v-tab>
      </v-tabs>

      <v-spacer></v-spacer>

      <v-menu>
        <template #activator="{ props }">
          <v-btn variant="text" v-bind="props" class="d-flex align-center user-btn">
            <v-icon size="25" class="mr-1" style="color: #1867C0; margin-top:2px;">mdi-account-circle-outline</v-icon>
            <span class="user-name-text">{{ userNameDisplay }}</span>
            <v-icon size="18" class="ml-1">mdi-chevron-down</v-icon>
          </v-btn>
        </template>

        <v-list density="compact">
          <v-list-item @click="goToChangePassword" class="custom-list-item">
            <v-list-item-title class="list-item-text">비밀번호 변경</v-list-item-title>
          </v-list-item>

          <v-list-item @click="handleLoginLogout" class="custom-list-item">
            <v-list-item-title class="list-item-text">
              {{ userLoginStatus ? '로그아웃' : '로그인' }}
            </v-list-item-title>
          </v-list-item>
        </v-list>
      </v-menu>
    </v-container>
  </v-app-bar>

  <!-- 비밀번호 변경 다이얼로그 -->
  <v-dialog v-model="showPasswordDialog" width="400" max-width="800" transition="dialog-bottom-transition">
    <v-card class="pa-6" style="width: 100%;">
      <v-card-title class=" text-h6 font-weight-bold" style="pointer-events: auto;">비밀번호 변경</v-card-title>
      <v-card-text style="pointer-events: auto;">
        <v-text-field v-model="newPassword" :type="showNewPassword ? 'text' : 'password'" label="새 비밀번호"
          variant="outlined" color="primary" :append-inner-icon="showNewPassword ? 'mdi-eye' : 'mdi-eye-off'"
          @click:append-inner="showNewPassword = !showNewPassword" hide-details="auto"></v-text-field>
      </v-card-text>
      <v-card-actions class="justify-end" style="pointer-events: auto;">
        <v-btn variant="outlined" color="grey" @click="showPasswordDialog = false">취소</v-btn>
        <v-btn variant="flat" color="primary" @click="confirmReset">변경</v-btn>
      </v-card-actions>
    </v-card>
  </v-dialog>

  <!-- 성공 스낵바 -->
  <v-snackbar v-model="showSuccess" timeout="2000" color="success" elevation="2" class="center-snackbar">
    {{ successMessage }}
  </v-snackbar>

  <!-- 실패 스낵바 -->
  <v-snackbar v-model="showFail" timeout="2000" color="error" elevation="2" class="center-snackbar">
    {{ errorMessage }}
  </v-snackbar>
</template>
<script>
import { ref, defineComponent, onMounted, watch, provide } from 'vue';
import { storeToRefs } from 'pinia';
import { useAuthStore } from '@/store/auth';
import { useMenuStore } from '@/store/menuStore';
import { useRouter } from 'vue-router';
import apiClient from '@/api';

export default defineComponent({
  name: 'HeaderBar',
  emits: ['navigate', 'menuSelected'],
  setup(props, { emit }) {
    const menuStore = useMenuStore();
    const { isLoading, error } = storeToRefs(menuStore);
    const activeTab = ref(null);
    const menuItems = ref([]);

    const userIdDisplay = ref('');
    const userNameDisplay = ref('');
    const userLoginStatus = ref('');

    const showPasswordDialog = ref(false);
    const newPassword = ref('');
    const showNewPassword = ref(false);

    const showSuccess = ref(false);
    const successMessage = ref('');
    const showFail = ref(false);
    const errorMessage = ref('');

    const authStore = useAuthStore();
    const router = useRouter();

    const handleLoginLogout = () => {
      if (userLoginStatus.value) {
        authStore.logout();
        userLoginStatus.value = false;
        router.push({ name: 'Login' });
      }
    };

    const showSideMenu = (item) => {
      const userId = localStorage.getItem('user_id');
      if (userId) {
        menuStore.fetchMenuData(item.m_code, userId);
      }
    };

    const fetchMenuData = async (id) => {
      try {
        isLoading.value = true;
        const response = await apiClient.get('/api/menuitem', {
          params: { auth: '', id }
        });
        if (response.data && Array.isArray(response.data.menuItems)) {
          menuItems.value = response.data.menuItems.map(item => ({
            m_code: item.mcode,
            m_name: item.mname
          }));
        } else {
          throw new Error('Invalid data structure');
        }
      } catch (error) {
        console.error('Error fetching menu data:', error);
        menuItems.value = [];
      } finally {
        isLoading.value = false;
      }
    };

    const goToChangePassword = () => {
      showPasswordDialog.value = true;
    };

    const confirmReset = async () => {
      try {
        await apiClient.post("/api/resetPassword", {
          id: userIdDisplay.value,
          newPw: newPassword.value
        });

        successMessage.value = '비밀번호가 변경되었습니다.';
        showSuccess.value = true;
      } catch (error) {
        errorMessage.value = '비밀번호 변경에 실패했습니다. 관리자에게 문의하세요.';
        showFail.value = true;
      } finally {
        showPasswordDialog.value = false;
      }
    };

    onMounted(() => {
      const userId = localStorage.getItem('user_id');
      if (userId) fetchMenuData(userId);

      userIdDisplay.value = userId;
      userLoginStatus.value = localStorage.getItem('isAuthenticated');
      const userInfo = JSON.parse(localStorage.getItem('userInfo'));
      userNameDisplay.value = userInfo?.name || '';
    });

    watch(menuItems, (newMenuItems) => {
      if (newMenuItems.length > 0) {
        activeTab.value = newMenuItems[0];
        showSideMenu(newMenuItems[0]);
      }
    }, { immediate: true });

    const sideMenuRef = ref(null);
    provide('sideMenuRef', sideMenuRef);

    const handleMenuClick = (item) => {
      showSideMenu(item);
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
      showPasswordDialog,
      goToChangePassword,
      confirmReset,
      newPassword,
      showNewPassword,
      showSuccess,
      successMessage,
      showFail,
      errorMessage
    };
  }
});
</script>

<style scoped>
.login-btn {
  background-color: #1867C0;
  border-radius: 10px !important;
  color: white;
  width: 80px;
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
  color: #1976D2 !important;
  background-color: transparent !important;
}

.custom-tab::before,
.custom-tab::after {
  opacity: 0 !important;
  background-color: transparent !important;
}

.custom-tab {
  font-weight: 600;
  font-size: 14px;
  color: #737577;
}

.tab-text {
  font-size: 17px;
}

::v-deep.custom-tab .v-btn__overlay {
  opacity: 0 !important;
}

::v-deep.custom-tab .v-tab__slider {
  display: none !important;
}

.custom-elevation {
  box-shadow: 0 1px 1px rgba(0, 0, 0, 0.08) !important;
}

.business-btn {
  margin-top: 14px;
  text-transform: none;
  font-weight: 600;
  color: #00B0F3 !important;
}

.business-btn .v-icon {
  margin-left: 7px;
  font-size: 18px;
}

.business-text {
  letter-spacing: 0px;
}

.user-name-text {
  font-size: 16px;
  font-weight: 500;
}

.custom-list-item {
  min-height: 30px;
  padding-top: 0;
  padding-bottom: 0;
  cursor: pointer;
}

.custom-list-item:hover {
  background-color: #f5f5f5;
}

.list-item-text {
  font-size: 14px;
}

.center-snackbar {
  bottom: 30px;
}
</style>
