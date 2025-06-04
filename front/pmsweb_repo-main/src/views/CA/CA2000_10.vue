<template>
  <div>
    <v-divider thickness="3" color="#578ADB"></v-divider>
  </div>

  <v-container fluid>
    <br>
    <div class="d-flex">
      <div style="flex: 2; margin-right: 20px; padding-left: 70px;">
        <div class="d-flex align-center justify-end mb-2">
          <v-btn @click="handleAddUser" prepend-icon="mdi-plus" size="small" color="green darken-2"
            class="text-none mr-2">추가</v-btn>
          <v-btn @click="deleteUser" prepend-icon="mdi-delete" size="small" color="grey darken-2"
            class="text-none">삭제</v-btn>
        </div>
        <v-table density="compact" fixed-header class="table-style">
          <thead class="table-header">
            <tr>
              <th class="text-left" style="width: 70px;">선택</th>
              <th class="text-left">사용자ID</th>
              <th class="text-left">이름</th>
              <th class="text-left">직급</th>
              <th class="text-left">부서</th>
              <th class="text-left">회사</th>
              <th class="text-left" style="width: 150px;">권한</th>          
            </tr>
          </thead>
          <tbody>
            <tr v-for="user in users" :key="user.id" :class="{ 'selected-row': selectedUserId === user.id }"
              @click="selectUser(user.id);" style="cursor: pointer;">
              <td @click.stop>
                <v-icon @click="selectUser(user.id)" :color="selectedUserId === user.id ? 'primary' : '#aaa'">
                  {{ selectedUserId === user.id ? 'mdi-checkbox-marked' : 'mdi-checkbox-blank-outline' }}
                </v-icon>
              </td>
              <td>{{ user.id }}</td>
              <td>{{ user.name }}</td>
              <td>{{ user.rollPstnNm }}</td>
              <td>{{ user.deptNm }}</td>
              <td>{{ user.corpNm }}</td>
              <td @click.stop>
                <v-select
                  v-model="user.authLevel"
                  :items="authLevelOptions"
                  item-title="name"
                  item-value="code"
                  variant="outlined"
                  density="compact"
                  hide-details
                  class="auth-select"
                />
              </td>
            </tr>
          </tbody>
        </v-table>
      </div>
      <div style="flex: 1; padding-right: 70px;">
        <div class="d-flex justify-space-between align-center mb-1">
          <div class="text-h6">게시판 목록</div>
          <v-btn color="primary" class="ml-auto py-2 text-body-1" @click="savePermissions">저장</v-btn>
        </div>
        <v-card>
          <div class="height-scroll-container" ref="menuScrollContainer">
            <div v-for="(group, groupIdx) in menuGroups" :key="group.groupKey" class="mb-2">
              <div class="text-subtitle-1 font-weight-bold d-flex align-center">
                <v-checkbox v-model="group.checked" :label="group.groupLabel" hide-details density="compact"
                  class="my-1 main-label" @change="toggleGroup(groupIdx)"
                  :style="{ color: group.checked ? '#1867C0' : '#888888' }" />
              </div>
              <div v-for="(mid) in group.options" :key="mid.value" class="ml-6">
                <div class="font-weight-medium mb-1 d-flex align-center">
                  <v-checkbox :label="mid.label" :model-value="isMidChecked(group, mid)"
                    @update:model-value="checked => toggleMid(group, mid, checked)" hide-details density="compact"
                    class="my-1 checkbox-mid" :style="{
                      color: isMidChecked(group, mid) ? '#1867C0' : '#888888'
                    }" />
                </div>
                <div>
                  <v-checkbox v-for="sub in mid.children" :key="sub.value" :label="sub.label" :value="sub.value"
                    v-model="group.selected" hide-details density="compact" class="my-1 ml-7 sub-label"
                    @change="updateParentCheckStatus(group, mid)"
                    :style="{ color: isChecked(sub.value, group.selected) }" />

                </div>
              </div>
            </div>
          </div>
        </v-card>
      </div>
    </div>
  </v-container>

  <!-- user 추가하기 팝업 -->
  <user-popup :show="showUserPopup" @user-selected="onUserAdded" @close="showUserPopup = false" />

</template>

<script>
import { inject, onMounted } from 'vue';
import apiClient from '@/api';
import UserPopup from '@/components/UserPopup';

export default {
  components: {
    UserPopup
  },
  setup() {
    // const extraBreadcrumb = inject('extraBreadcrumb', null);
    const listButtonLink = inject('listButtonLink', null);
    onMounted(() => {
      // if (extraBreadcrumb) extraBreadcrumb.value = '권한등록';
      if (listButtonLink) listButtonLink.value = null;
    });
    return {};
  },
  data() {
    return {
      selectedUserId: null,
      users: [],
      menuGroups: [],
      showUserPopup: false,
      selectedUser: null,
      // 권한 레벨 옵션
      authLevelOptions: [
        { code: 10, name: '일반' },
        { code: 20, name: 'IT관리자' },
        { code: 30, name: '마스터' }
      ]
    };
  },
  mounted() {
    this.getAuthUser();
    this.fetchMenuGroups();
  },
  methods: {
    async getAuthUser() {
      const res = await apiClient.get('/api/userAuth/list');
      this.users = res.data.map(item => ({
        selected: false,
        auth: item.auth,
        id: item.id,
        name: item.name || '',
        rollPstnNm: item.rollPstnNm || '',
        deptNm: item.deptNm || '',
        corpNm: item.corpNm || '',
        authLevel: this.convertAuthLevelToCode(item.authLevel), // 한글 → code 변환
        isNew: false
      }));
    },

    // 한글 권한명을 code로 변환하는 함수 추가
    convertAuthLevelToCode(authLevelName) {
      const mapping = {
        '일반': 10,
        'IT관리자': 20,
        '마스터': 30
      };
      return mapping[authLevelName] || 10; // 기본값 10
    },    
      
    selectUser(userId) {
      if (this.selectedUserId === userId) {
        // 이미 선택된 사용자 다시 클릭 → 해제 처리
        this.selectedUserId = null;
        this.menuGroups.forEach(group => {
          group.checked = false;
          group.selected = [];
        });
        this.fetchUserAuth(null); // 체크 해제 시 이 함수 호출
      } else {
        // 새 사용자 선택
        this.selectedUserId = userId;
        this.menuGroups.forEach(group => {
          group.checked = false;
          group.selected = [];
        });
        this.$nextTick(() => {
          const container = this.$refs.menuScrollContainer;
          if (container?.scrollTo) container.scrollTo({ top: 0, behavior: 'smooth' });
          else if (container) container.scrollTop = 0;
        });
        this.fetchUserAuth(userId);
      }
    },
    async fetchUserAuth(userId) {
      const res = await apiClient.get(`/api/userAuth/detailList`, { params: { userId } });

      this.menuGroups.forEach(group => {
        const matched = res.data.filter(auth => auth.mcode.startsWith(group.groupKey));

        const availableCodes = group.options.flatMap(mid => {
          const codes = mid.children.length > 0 ? mid.children.map(sub => sub.value) : [];
          return [...codes, mid.value];  // <- 중메뉴도 추가
        });

        group.selected = matched
          .map(auth => auth.mcode)
          .filter(code => availableCodes.includes(code));

        // 초기 상태에서 하위 메뉴 상태에 따라 그룹 체크 상태 설정.
        group.checked = this.hasAnySelected(group);
      });

      // 권한 로드 후 모든 메뉴 체크 상태 업데이트
      this.updateAllCheckStatus();
    },
    // 해당 그룹에 선택된 항목이 있는지 확인
    hasAnySelected(group) {
      return group.selected.length > 0;
    },
    // 중메뉴 체크 상태 확인 - 하위에 하나라도 체크되면 체크된 상태
    isMidChecked(group, mid) {
      if (mid.children && mid.children.length > 0) {
        return mid.children.some(sub => group.selected.includes(sub.value)) || group.selected.includes(mid.value);
      }
      return group.selected.includes(mid.value);
    },
    toggleGroup(index) {
      const group = this.menuGroups[index];
      if (group.checked) {
        // 대메뉴 체크: 모든 하위 메뉴 체크
        const allCodes = [group.groupKey];
        group.options.forEach(mid => {
          allCodes.push(mid.value);
          if (mid.children && mid.children.length > 0) {
            mid.children.forEach(sub => allCodes.push(sub.value));
          }
        });
        group.selected = allCodes;
      } else {
        // 대메뉴 해제: 모든 하위 메뉴 해제
        group.selected = [];
      }
    },
    toggleMid(group, mid, isChecked) {
      const hasChildren = mid.children && mid.children.length > 0;
      const selected = new Set(group.selected);

      if (isChecked) {
        selected.add(mid.value);

        if (hasChildren) {
          mid.children.forEach(sub => selected.add(sub.value));
        }
        group.checked = true; // 중메뉴가 체크되면 대메뉴도 체크
      } else {
        selected.delete(mid.value);
        if (hasChildren) {
          mid.children.forEach(sub => selected.delete(sub.value));
        }

        // ✅ 중메뉴 해제 후 대메뉴 체크 상태 다시 계산
        const stillChecked = group.options.some(opt =>
          selected.has(opt.value) ||
          (opt.children && opt.children.some(sub => selected.has(sub.value)))
        );
        group.checked = stillChecked;
      }
      group.selected = Array.from(selected);
    },
    // 하위 메뉴 변경 시 상위 메뉴 상태 업데이트
    updateParentCheckStatus(group, mid) {
      const isAnyChecked = mid.children.some(sub => group.selected.includes(sub.value));
      const selected = new Set(group.selected);

      if (!isAnyChecked) {
        //selected.delete(mid.value); // 하위 항목이 모두 해제되면 중메뉴도 해제
      } else {
        selected.add(mid.value); // 하나라도 선택되면 중메뉴는 체크 유지
      }

      group.selected = Array.from(selected);

      // 그룹 체크 상태도 갱신
      group.checked = this.hasAnySelected(group);
    },
    // 모든 메뉴의 체크 상태 업데이트
    updateAllCheckStatus() {
      this.menuGroups.forEach(group => {
        // 그룹 체크 상태 업데이트 - 하나라도 선택됐으면 체크
        group.checked = this.hasAnySelected(group);
      });
    },
    isChecked(value, selected) {
      return selected.includes(value) ? '#1867C0' : '#888888';
    },
    async fetchMenuGroups() {
      const res = await apiClient.get('/api/menuitem/all-menu');

      this.menuGroups = res.data.map(group => ({
        groupLabel: group.groupLabel,
        groupKey: group.groupKey,
        checked: false,
        selected: [],
        options: group.options
      }));
    },
    async deleteUser() {
      const selectedUser = this.users.find(user => user.id === this.selectedUserId);

      if (!selectedUser) {
        alert('삭제할 사용자를 선택해주세요.');
        return;
      }

      const isNew = selectedUser.isNew === true;
      const isSaved = selectedUser.isNew === false;

      // 1. 추가된 사용자만 선택한 경우 → 바로 삭제
      if (isNew) {
        this.users = this.users.filter(user => user.id !== selectedUser.id);
        this.selectedUserId = null;
        return;
      }

      // 2. 저장된 사용자만 선택한 경우 → confirm 후 삭제
      if (isSaved) {
        const confirmed = confirm("해당 사용자의 권한을 제거하시겠습니까?");

        if (confirmed) {
          await apiClient.post('/api/userAuth/deleteUser', {
            id: this.selectedUserId
          }, {});

          this.getAuthUser()
          this.fetchUserAuth(null);
          alert('삭제하였습니다.');
        }

        return;
      }
    },
    async savePermissions() {
      // console.log('--savePermissions--');
      if (!this.selectedUserId)
        return alert('사용자를 먼저 선택해주세요.');

      const userInfo = JSON.parse(localStorage.getItem('userInfo'));
      const insertUser = userInfo?.id || 'system';        

      
      const selectedUser = this.users.find(user => user.id === this.selectedUserId);      
      // console.log('selectedAuthLevel');
      // console.log(this.users);
      // console.log(selectedUser.authLevel);
      
      try {
        // 1. 선택된 사용자의 권한 레벨 업데이트
        const authLevelPayload = {
          id: selectedUser.id,
          authLevel: selectedUser.authLevel,
          insertUser: insertUser,
          updateUser: insertUser,          
        };

        // console.log(authLevelPayload);

         
        await apiClient.post('/api/userAuth/update-auth-level', authLevelPayload);

        // 2. 기존 권한 조회
        const res = await apiClient.get(`/api/userAuth/detailList`, { params: { userId: this.selectedUserId } });

        const existingCodes = res.data.map(auth => auth.mcode);
        const selectedCodes = [];

        this.menuGroups.forEach(group => {
          if (group.checked) selectedCodes.push(group.groupKey);
          selectedCodes.push(...group.selected);
        });

        if (selectedCodes.length === 0) {
          alert('권한을 부여할 게시판을 하나 이상 선택해주세요.');
          return;
        }

        const toInsert = selectedCodes.filter(c => !existingCodes.includes(c));
        const toDelete = existingCodes.filter(c => !selectedCodes.includes(c));
                
        // 3. 메뉴 권한 추가
        for (const menuCd of toInsert) {
          const payload = {
            id: this.selectedUserId,
            menuCode: menuCd,
            auth: 31,
            insertUser: insertUser,
            updateUser: insertUser,
          };

          await apiClient.post('/api/userAuth/save', payload);
        }

        // 4. 메뉴 권한 제거
        for (const menuCd of toDelete) {
          const payload = {
            id: this.selectedUserId,
            menuCode: menuCd,
            auth: 0,
            insertUser: insertUser,
            updateUser: insertUser
          };

          await apiClient.post('/api/userAuth/save', payload);
        }

        this.fetchUserAuth(this.selectedUserId);
        alert('권한이 저장되었습니다.');

      } catch (error) {
        console.error('권한 저장 오류:', error);
        alert('권한 저장 중 오류가 발생했습니다.');
      }
    },
    handleAddUser() {
      const hasUnsavedUser = this.users.some(user => user.isNew);
      if (hasUnsavedUser) {
        alert('기존 추가된 사용자를 먼저 저장해주세요.');
        return;
      }

      this.showUserPopup = true; // 조건 만족 시 팝업 오픈
    },
    onUserAdded(selectedUser) {
      if (!selectedUser) return;

      const exists = this.users.some(user => user.id === selectedUser.usrId);
      if (exists) {
        alert('이미 등록된 사용자입니다.');
        return;
      }

      this.users.push({
        id: selectedUser.usrId,
        name: selectedUser.name,
        rollPstnNm: selectedUser.rollPstnNm || '',
        deptNm: selectedUser.deptNm || '',
        corpNm: selectedUser.corpNm || '',
        authLevel: 10, // 새 사용자 기본값: 일반
        isNew: true // 새로 추가된 유저 표시
      });

      this.selectedUserId = selectedUser.usrId; // 체크 표시용 선택

      this.fetchUserAuth(null);
    },
  }
};
</script>

<style scoped>
.v-table th {
  font-weight: bold;
  background-color: #f5f5f5;
}

.title-div {
  font-size: 25px;
}

.table-style {
  min-height: auto;
  border: 1px solid #e0e0e0;
  width: 100%;
  position: relative;
  border-radius: 10px;
  overflow: hidden;
}

::v-deep(.table-header) {
  height: 56px;
}

::v-deep(.table-header th) {
  /* background-color: #D0DFF1 !important;
  font-weight: 500;
  border-bottom: 1px solid #e0e0e0 !important; */
  background-color: #D0DFF1 !important;
  color: #3E4B5B !important;
  font-weight: 500 !important;
  font-size: 14px !important;
  border-bottom: 1px solid #e0e0e0 !important;
  text-align: center;
}

.v-table tbody tr {
  height: 40px;
}

.v-table tbody td {
  padding-top: 4px;
  padding-bottom: 4px;
  height: 40px;
  vertical-align: middle;
}

.lickable-icon {
  cursor: pointer;
  font-size: 22px;
}

.selected-row {
  background-color: #FAF9F1;
  transition: background-color 0.3s;
}

::v-deep(.main-label .v-label) {
  color: black !important;
  font-weight: 500;
  opacity: 1 !important;
}

::v-deep(.checkbox-mid .v-label) {
  color: #5A5C5F !important;
  font-weight: 500;
  opacity: 1 !important;
}

::v-deep(.sub-label .v-label) {
  color: #5A5C5F !important;
  font-weight: 500;
  opacity: 1 !important;
}

.height-scroll-container {
  padding: 15px;
  max-height: 700px;
  overflow-y: auto;
  border: 1px solid #ddd;
  border-radius: 8px;
}

/* 권한 콤보박스 스타일 */
.auth-select {
  min-width: 120px;
}

.auth-select :deep(.v-input__control) {
  min-height: 32px;
  height: 32px;
}

.auth-select :deep(.v-field__input) {
  padding-top: 0;
  padding-bottom: 0;
  min-height: 32px;
  font-size: 13px;
}

.auth-select :deep(.v-field) {
  border-radius: 4px;
}
</style>