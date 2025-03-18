import { defineStore } from "pinia";
import axios from "axios";

export const useMenuStore = defineStore("menu", {
  state: () => ({
    menuData: [],
    isLoading: true,
    error: null,
    // 로그인 정보 관련 상태 추가
    userId: null,
    userInfo: null,
  }),
  actions: {
    async fetchMenuData(auth, id) {
      console.log("-------fetchMenuData ---------");
      try {
        this.isLoading = true;
        
        const response = await axios.get("http://localhost:8080/api/menuitem", {        
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
    // 로그인 시 사용자 ID 저장
    setUserId(id) {
      this.userId = id;
    },
    // 로그인이 성공했을 때 회원 정보를 가져오는 함수
    async fetchUserInfo() {
      try {
        this.isLoading = true;
        
        // 회원 정보를 가져오는 API 호출
        /*
        const response = await axios.get("http://localhost:8080/api/user", {
          params: {
            id: this.userId
          }
        });
        
        if (response.data) {
          this.userInfo = response.data;
          console.log("User info loaded:", this.userInfo);
        } else {
          throw new Error("Failed to fetch user information");
        }
        */
        
      } catch (error) {
        console.error("Error fetching user info:", error);
        this.error = error;
      } finally {
        this.isLoading = false;
      }
    }
  },
});