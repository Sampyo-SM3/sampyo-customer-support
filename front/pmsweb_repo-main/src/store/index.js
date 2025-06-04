import { createStore } from 'vuex';

export default createStore({
  state: {
    menuItems: []
  },
  mutations: {
    SET_MENU_ITEMS(state, items) {
      state.menuItems = items;
    }
  },
  actions: {
  
  },
  getters: {
    // processedMenuItems: (state) => {
      
    // }
  }
});