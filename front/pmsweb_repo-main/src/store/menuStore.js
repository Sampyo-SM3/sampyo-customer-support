import { defineStore } from "pinia";
import axios from "axios";

export const useMenuStore = defineStore("menu", {
  state: () => ({
    menuData: [],
    isLoading: true,
    error: null,
  }),
  actions: {
    async fetchMenuData(auth, id) {
      console.log("-------store fetchMenuData ---------");
      try {
        this.isLoading = true;

        //  const response = await axios.get('http://:29000/pms/api/menuitem?auth=CA&id=javachohj')
        const response = await axios.get("http://localhost:8080/api/menuitem", {
          //  const response = await axios.get('http://localhost:29000/pms/api/menuitem?auth=CA&id=javachohj', {
          params: {
            auth: auth,
            id: id,
          },
        });

        if (response.data && Array.isArray(response.data.menuItems)) {
          this.menuData = response.data.menuItems.map(this.mapMenuItem);
          console.log("store this.menuData -> ", this.menuData);
        } else {
          throw new Error(
            "Invalid data structure or menuItems is not an array"
          );
        }
      } catch (error) {
        console.error("Error fetching menu data:", error);
        this.error = error;
        this.menuData = [];
      } finally {
        this.isLoading = false;
      }
    },
    mapMenuItem(apiItem) {
      return {
        LEV: parseInt(apiItem.mlev) || 0,
        M_NAME: apiItem.mname || "",
        M_GBN: apiItem.mGBN || "",
        M_CODE: apiItem.mcode || "",
        M_ICON: apiItem.micon || "",
      };
    },
  },
});
