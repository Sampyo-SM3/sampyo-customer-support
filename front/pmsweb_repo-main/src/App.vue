<template>
  <v-app>
    <v-main class="no-scroll">
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
/* html, body {
  overflow: hidden;
  height: 100vh;
  margin: 0;
  padding: 0;
}

.no-scroll {
  overflow: hidden;
} */
</style>