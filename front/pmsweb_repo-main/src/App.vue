<template>
  <v-app>
    <v-main>
      <router-view></router-view>
    </v-main>
  </v-app>
</template>

<script>
export default {
  name: 'App',
}
</script>

<style>
/* 전역 스타일이 필요하다면 여기에 추가하세요 */
body {
  overflow: hidden;
  /* 기본은 스크롤 막음 */
}
</style>