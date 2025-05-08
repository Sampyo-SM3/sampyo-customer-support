<template>
  <div class="dynamic-loader">
    <template v-if="dynamicComponent">
      <component :is="dynamicComponent" />
    </template>
    <template v-else>
      <div class="loading-container">
        <div class="spinner"></div>
        <div class="loading-text">잠시만 기다려주세요...</div>
      </div>
    </template>
  </div>
</template>

<script>
import { defineComponent, ref, watch } from 'vue'
import { useRoute } from 'vue-router'

export default defineComponent({
  name: 'DynamicComponentLoader',
  setup() {
    const route = useRoute()
    const dynamicComponent = ref(null)

    const loadComponent = async () => {
      const { folder, file } = route.params
      try {
        const component = await import(`@/views/${folder}/${file}.vue`)
        dynamicComponent.value = component.default
      } catch (error) {
        console.error('Failed to load component:', error)
        dynamicComponent.value = null
      }
    }

    watch(() => route.params, loadComponent, { immediate: true })

    return {
      dynamicComponent
    }
  }
})
</script>

<style scoped>
.dynamic-loader {
  width: 100%;
  height: 100%;
  position: relative;
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
