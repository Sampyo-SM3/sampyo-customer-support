<template>
  <component :is="dynamicComponent" v-if="dynamicComponent" />
  <div v-else>Loading component...</div>
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