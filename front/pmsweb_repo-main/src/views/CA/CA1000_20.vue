<template>
  <v-container fluid>
    <v-row>
      <v-col>
        <div class="mt-2">
          <v-divider thickness="3" color="#578ADB"></v-divider>
        </div>
      </v-col>
    </v-row>

    <br>

    <div class="d-flex ">
      <!-- 왼쪽 테이블 영역 -->
      <div style="flex: 2; margin-right: 20px; padding-left: 70px;">
        <div class="d-flex align-center justify-end mb-2">
          <v-btn prepend-icon="mdi-plus" size="small" color="primary" class="text-none mr-2">
            추가
          </v-btn>
          <v-btn prepend-icon="mdi-plus" size="small" color="grey darken-2" class="text-none">
            삭제
          </v-btn>
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
            <tr v-for="user in users" :key="user.userId" :class="{ 'selected-row': selectedUserId === user.userId }"
              @click="selectUser(user.userId);" style="cursor: pointer;">
              <td @click.stop>
                <v-icon @click="selectUser(user.userId)" :color="selectedUserId === user.userId ? 'primary' : '#aaa'">
                  {{ selectedUserId === user.userId ? 'mdi-checkbox-marked' : 'mdi-checkbox-blank-outline' }}
                </v-icon>
              </td>
              <td>{{ user.userId }}</td>
              <td>{{ user.name }}</td>
              <td>{{ user.position }}</td>
              <td>{{ user.department }}</td>
              <td>{{ user.company }}</td>
            </tr>
          </tbody>

        </v-table>
      </div>

      <!-- 오른쪽 게시판 목록 영역 -->
      <div style="flex: 1; padding-right: 70px;">
        <div class="text-h6 mb-1">게시판 목록</div>

        <v-card>
          <div class="height-scroll-container">
            <div v-for="(group, index) in menuGroups" :key="group.groupKey" class="mb-6">
              <div class="text-subtitle-1 font-weight-bold mb-1 d-flex align-center">
                <v-checkbox v-model="group.checked" :label="group.groupLabel" hide-details density="compact"
                  class="my-1 main-label" @update:model-value="toggleGroup(index)"
                  :style="{ color: group.checked ? '#1867C0' : '#888888' }" />
              </div>
              <div>
                <v-checkbox v-for="opt in group.options" :key="opt.value" :label="opt.label" :value="opt.value"
                  v-model="group.selected" hide-details density="compact" class="my-1 ml-5 sub-label"
                  :style="{ color: isChecked(opt.value, group.selected) }" />
              </div>
            </div>
          </div>


        </v-card>
      </div>
    </div>
  </v-container>
</template>

<script>
import { inject, onMounted } from 'vue';
import apiClient from '@/api';

export default {
  setup() {
    const extraBreadcrumb = inject('extraBreadcrumb', null);
    const listButtonLink = inject('listButtonLink', null);
    onMounted(() => {
      if (extraBreadcrumb) {
        extraBreadcrumb.value = '권한등록';  // 🔥 추가하고 싶은 값
      }

      if (listButtonLink) {
        listButtonLink.value = null;  // 🔥 현재 페이지에 맞는 "목록" 경로 설정
      }
    });

    return {};
  },
  unmounted() { // ❗ 컴포넌트가 언마운트될 때
    const listButtonLink = inject('listButtonLink', null);
    if (listButtonLink) {
      listButtonLink.value = null; // 🔥 페이지 벗어날 때 목록버튼 없애기
    }
  },
  data() {
    return {
      selectedUserId: null,
      users: [
        { selected: false, role: '계시판관리자', userId: '1004631', name: '나영찬', position: '수석', department: '환경팀', company: '(주)삼포시멘트' },
        { selected: false, role: '계시판관리자', userId: '1005508', name: '오길식', position: '수석', department: '서울영업소', company: '(주)삼포시멘트' },
        { selected: false, role: '계시판관리자', userId: '1005628', name: '이미숙', position: '수석', department: '화계팀', company: '(주)삼포시멘트' },
        { selected: false, role: '계시판관리자', userId: '1005637', name: '최진호', position: '수석', department: '법무팀', company: '(주)삼포시멘트' },
        { selected: false, role: '계시판관리자', userId: '1005820', name: '배대송', position: '수석', department: '화계팀', company: '(주)삼포시멘트' },
        { selected: false, role: '기획', userId: '1005901', name: '임주희', position: '책임', department: '재무팀', company: '(주)삼포시멘트' },
        { selected: false, role: '계시판관리자', userId: '1005984', name: '김정희', position: '매니저', department: '인사팀', company: '(주)삼포시멘트' },
        { selected: false, role: '기획', userId: '1006970', name: '이명규', position: '책임', department: '총무팀', company: '(주)삼포시멘트' },
        { selected: false, role: '계시판관리자', userId: '1007245', name: '박선아', position: '매니저', department: '인사팀', company: '(주)삼포시멘트' },
        { selected: false, role: '기획', userId: '1007428', name: '김국주', position: '책임', department: '해무팀', company: '(주)삼포시멘트' },
        { selected: false, role: '슈퍼관리자', userId: 'flynow', name: '이민주', position: '수석', department: 'CORE개발팀', company: '(주)심포산업' },
      ],
      channelSGroupChecked: false, // 상위 체크박스 상태
      channelSSelected: [], // 하위 체크 항목들
      menuGroups: [
        {
          // groupLabel: '채널S',
          // groupKey: 'channelS',
          // selected: [],
          // checked: false,
          // options: [
          //   { label: '최근게시', value: '최근게시' },
          //   { label: '심포스토리', value: '심포스토리' },
          //   { label: '심포생활백서', value: '심포생활백서' },
          //   { label: '이벤트', value: '이벤트' },
          //   { label: '심포블로그', value: '심포블로그' }
          // ]
        },
      ]

    };
  },
  mounted() {
    this.fetchMenuGroups();
  },
  methods: {
    selectUser(userId) {
      this.selectedUserId = this.selectedUserId === userId ? null : userId;

      // 🔥 추가: 모든 메뉴 그룹 체크 해제
      this.menuGroups.forEach(group => {
        group.checked = false;
        group.selected = [];
      });

      //선택된 user의 권한 체크
      this.fetchUserAuth(userId);
    },
    async fetchUserAuth(userId) {
      try {
        const response = await apiClient.get(`/api/userAuth/detailList`, {
          params: { userId }
        });

        const authList = response.data; // 서버에서 받은 권한 데이터

        // groupKey로 그룹 매칭
        this.menuGroups.forEach(group => {
          const matchedCodes = authList.filter(auth => auth.mcode.startsWith(group.groupKey));

          if (matchedCodes.length > 0) {
            group.checked = true; // 그룹 활성화
            group.selected = [];  // 초기화

            matchedCodes.forEach(auth => {
              // 옵션 안에 존재하는 mcode만 추가
              const exists = group.options.some(opt => opt.value === auth.mcode);
              if (exists) {
                group.selected.push(auth.mcode);
              }
            });
          } else {
            group.checked = false;
            group.selected = [];
          }
        });

      } catch (err) {
        console.error('❌ 사용자 권한 정보 로딩 실패:', err);
      }
    },
    toggleGroup(index) {
      const group = this.menuGroups[index];
      if (group.checked) {
        group.selected = group.options.map(o => o.value);
      } else {
        group.selected = [];
      }
    },
    isChecked(value, selectedList) {
      return selectedList.includes(value) ? '#1867C0' : '#888888';
    },
    async fetchMenuGroups() {
      try {
        const res = await apiClient.get('/api/menuitem/all-menu');

        console.log(res);

        this.menuGroups = res.data.map(group => ({
          groupLabel: group.groupLabel,
          groupKey: group.groupKey,
          checked: false,
          selected: [],
          options: group.options
        }));
      } catch (err) {
        console.error('메뉴 불러오기 실패:', err);
      }
    },
  },
  watch: {
    menuGroups: {
      handler(newVal) {
        newVal.forEach((group, index) => {
          const anySelected = group.selected.length > 0; // 하나라도 체크됐으면 true
          this.menuGroups[index].checked = anySelected;
        });
      },
      deep: true
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

::v-deep(.sub-label .v-label) {
  color: #5A5C5F !important;
  /* Vuetify primary */
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
