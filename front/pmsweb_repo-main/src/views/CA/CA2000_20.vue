<template>
  <div class="centered-message">
    <v-icon size="48" color="primary">mdi-alert-circle-outline</v-icon>
    <div class="mt-4 text-h6 font-weight-medium">
      관리자에게 권한요청을 해주세요.
    </div>
  </div>
</template>

<style scoped>
.centered-message {
  position: fixed;
  top: 50%;
  left: 55%;
  transform: translate(-50%, -50%);
  text-align: center;
}
</style>