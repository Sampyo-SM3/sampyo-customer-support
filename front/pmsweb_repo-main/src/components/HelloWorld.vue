<template>
  <div class="d-flex">
    <!-- 아이콘 박스 -->
    <v-navigation-drawer permanent width="65" class="icon-bar">
      <v-list density="compact" nav>
        <v-list-item v-for="icon in icons" :key="icon" link>
          <template v-slot:prepend>
            <div>
              <v-icon>{{ icon }}</v-icon>
              <br> <p class="test">생산관리</p>
            </div>
            
          </template>
        </v-list-item>
      </v-list>
    </v-navigation-drawer>

    <!-- 기존 네비게이션 드로어 -->
    <v-navigation-drawer permanent width="256" class="main-drawer">
      <v-list density="compact" nav>
        <v-list-item class="test2" link>
          <template v-slot:prepend>
            <v-icon>mdi-home</v-icon>
          </template>
          <v-list-item-title>Home</v-list-item-title>
        </v-list-item>
        <v-list-group
          v-for="item in items"
          :key="item.title"
          :value="item.title"
        >
          <template v-slot:activator="{ props }">
            <v-list-item v-bind="props" :prepend-icon="item.icon" :title="item.title"></v-list-item>
          </template>
          <v-list-item
            v-for="subItem in item.items"
            :key="subItem.title"
            :title="subItem.title"
            link
          ></v-list-item>
        </v-list-group>
      </v-list>
    </v-navigation-drawer>
  </div>
</template>

<script>
import { ref, defineComponent } from 'vue'

export default defineComponent({
  name: 'HelloWorld',
  emits: ['navigate'],
  setup() {
    const icons = ref(['mdi-home', 'mdi-account', 'mdi-cog', 'mdi-help'])
    
    const items = ref([
      {
        title: '생산관리',
        // icon: 'mdi-view-dashboard',
        items: [
          { title: '생산계획' },
          { title: '생산실적' },
          { title: '공정관리' },
        ],
      },
      {
        title: '재고관리',
        // icon: 'mdi-cube',
        items: [
          { title: '재고현황' },
          { title: '입출고관리' },
        ],
      }  
    ])  

    return {
      icons,
      items
    }
  }
})
</script>

<style scoped>
.icon-bar {
  border-right: 1px solid rgba(0, 0, 0, 0.12);
}

.main-drawer {
  border-left: none;
}

.test {
  font-size: 12px;
  transform: translateX(-10px);
}

.test2 {
  background-color: red;  
}
</style>