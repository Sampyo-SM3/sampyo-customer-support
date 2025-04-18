<template>
  <v-container fluid>
    <v-row>
      <v-col>
        <div class="title-div">권한 등록</div>
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
              @click="selectUser(user.userId)" style="cursor: pointer;">
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
          <div style="padding:15px;">
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
export default {
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
          groupLabel: '채널S',
          groupKey: 'channelS',
          selected: [],
          checked: false,
          options: [
            { label: '최근게시', value: '최근게시' },
            { label: '심포스토리', value: '심포스토리' },
            { label: '심포생활백서', value: '심포생활백서' },
            { label: '이벤트', value: '이벤트' },
            { label: '심포블로그', value: '심포블로그' }
          ]
        },
        {
          groupLabel: '공지사항',
          groupKey: 'notice',
          selected: [],  //체크된 애는 여기에 넣으면
          checked: false,
          options: [
            { label: '그룹공지', value: '그룹공지' },
            { label: '심포공지', value: '심포공지' },
            { label: 'IT공지', value: 'IT공지' },
            { label: '인사소식', value: '인사소식' },
            { label: '경조소식', value: '경조소식' },
            { label: '복리후생소식', value: '복리후생소식' },
            { label: '회계/재무일람', value: '회계/재무일람' }
          ]
        },
      ]

    };
  },
  methods: {
    selectUser(userId) {
      this.selectedUserId = this.selectedUserId === userId ? null : userId;

      // 🔥 추가: 모든 메뉴 그룹 체크 해제
      this.menuGroups.forEach(group => {
        group.checked = false;
        group.selected = [];
      });
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
      // 체크됐으면 파란색, 아니면 연회색
    }
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
</style>
