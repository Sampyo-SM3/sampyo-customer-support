<template>
  <v-app>
    <HeaderBar />
    <v-container fluid class="pa-0">
      <v-row no-gutters>
        <!-- 사이드 메뉴 -->
        <v-col cols="auto" class="side-menu-col">
          <SideMenu />
        </v-col>

        <v-col>
          <!-- 라우터 뷰 (실제 콘텐츠) -->
          <v-main>
            <router-view></router-view>
          </v-main>
        </v-col>


      </v-row>
    </v-container>
  </v-app>
</template>

<script>
import SideMenu from '@/components/SideMenu.vue';
import HeaderBar from '@/components/HeaderBar.vue';

export default {
  name: 'MainPage',
  components: {
    SideMenu,
    HeaderBar
  }
}
</script>

<style scoped>
.v-main {
  /* v-main의 기본 패딩 제거 */
  /* padding: 0 !important; */
}

.breadcrumb-container {
  /* width: 100%; */
  border-bottom: 1px solid #eaeaea;
}

.breadcrumb-container {
  width: 100%;
  border-bottom: 1px solid #eaeaea;
  background-color: #f9f9f9;
  /* 연한 회색 배경 */
  font-family: 'Noto Sans KR', sans-serif;
  /* 한글 웹폰트 */
}

.breadcrumb-container span {
  font-size: 13px;
  color: #555;
  /* 글자색 */
}

.breadcrumb-container span.font-weight-medium {
  color: #000;
  /* 현재 위치(마지막 항목)는 더 진한 색상 */
  font-weight: 500;
}

.breadcrumb-container .v-icon {
  color: #999;
  /* 화살표 아이콘 색상 */
}
</style>