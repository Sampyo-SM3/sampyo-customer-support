<template>
  <div class="dynamic-loader">
    <div class="loading-container">
      <div class="spinner"></div>
      <div class="loading-text">잠시만 기다려주세요...</div>
    </div>
  </div>
</template>

<style scoped>
.dynamic-loader {
  width: 100%;
  height: 100vh;
  /* ✅ 뷰포트 기준으로 고정 */
  display: flex;
  /* ✅ flex로 정확히 가운데 정렬 */
  justify-content: center;
  align-items: center;
  position: relative;
  background-color: white;
  /* 필요하면 배경 깔끔하게 */
}

.loading-container {
  display: flex;
  flex-direction: column;
  align-items: center;
  text-align: center;
  margin-bottom: 160px;
}

.spinner {
  width: 80px;
  height: 80px;
  border: 8px solid #ccc;
  border-top: 8px solid #1976d2;
  border-radius: 50%;
  animation: spin 1s linear infinite;
}

.loading-text {
  margin-top: 20px;
  font-size: 20px;
  color: #555;
  animation: fade 1.5s infinite;
}

@keyframes spin {
  0% {
    transform: rotate(0deg);
  }

  100% {
    transform: rotate(360deg);
  }
}

@keyframes fade {

  0%,
  100% {
    opacity: 0.5;
  }

  50% {
    opacity: 1;
  }
}
</style>
