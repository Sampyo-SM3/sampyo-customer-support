<template>
  <v-container fluid>
    <v-row>
      <v-col>
        <div style="margin-top:-10px;">
          <v-divider thickness="3" color="#578ADB"></v-divider>
        </div>
      </v-col>
    </v-row>
    <br>
    <div class="d-flex">
      <div style="flex: 2; margin-right: 20px; padding-left: 70px;">
        <div class="d-flex align-center justify-end mb-2">
          <v-btn @click="showManagerPopup = true" prepend-icon="mdi-plus" size="small" color="green darken-2"
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
  <user-popup :show="showUserPopup" @manager-selected="onAdminAdded" @close="showUserPopup = false" />
</template>

<script>
import { inject, onMounted } from 'vue';
import apiClient from '@/api';
import userPopup from '@/components/userPopup.vue';

export default {
  components: {
    userPopup
  },
  setup() {
    const extraBreadcrumb = inject('extraBreadcrumb', null);
    const listButtonLink = inject('listButtonLink', null);
    onMounted(() => {
      if (extraBreadcrumb) extraBreadcrumb.value = '권한등록';
      if (listButtonLink) listButtonLink.value = null;
    });
    return {};
  },
  data() {
    return {
      selectedUserId: null,
      users: [],
      menuGroups: []
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
        corpNm: item.corpNm || ''
      }));
    },
    selectUser(userId) {
      this.selectedUserId = this.selectedUserId === userId ? null : userId;
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

        // 초기 상태에서 하위 메뉴 상태에 따라 그룹 체크 상태 설정
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

      // 중메뉴 자체의 체크 상태 변경
      if (isChecked) {
        selected.add(mid.value);
        // 중메뉴 체크 시 모든 하위 메뉴 체크
        if (hasChildren) {
          mid.children.forEach(sub => selected.add(sub.value));
        }
      } else {
        selected.delete(mid.value);
        // 중메뉴 해제 시 모든 하위 메뉴 해제
        if (hasChildren) {
          mid.children.forEach(sub => selected.delete(sub.value));
        }
      }

      group.selected = Array.from(selected);

      // 그룹 체크 상태 업데이트 - 하나라도 선택됐으면 체크
      //group.checked = this.hasAnySelected(group);
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
    insertUser() {
      alert('추가');
    },
    deleteUser() { alert('삭제'); },
    async savePermissions() {
      if (!this.selectedUserId)
        return alert('사용자를 먼저 선택해주세요.');

      const res = await apiClient.get(`/api/userAuth/detailList`, { params: { userId: this.selectedUserId } });

      const existingCodes = res.data.map(auth => auth.mcode);
      const selectedCodes = [];

      this.menuGroups.forEach(group => {
        if (group.checked) selectedCodes.push(group.groupKey);
        selectedCodes.push(...group.selected);
      });

      const toInsert = selectedCodes.filter(c => !existingCodes.includes(c));
      const toDelete = existingCodes.filter(c => !selectedCodes.includes(c));

      console.log('추가할 권한:', toInsert);
      console.log('삭제할 권한:', toDelete);

      // 여기에 권한 저장 API 호출 로직 추가 필요
      // await apiClient.post('/api/userAuth/save', { userId: this.selectedUserId, toInsert, toDelete });

      alert('권한이 저장되었습니다.');

      this.fetchUserAuth(this.selectedUserId);
    }
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
  background-color: #D0DFF1 !important;
  font-weight: 500;
  border-bottom: 1px solid #e0e0e0 !important;
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
</style>