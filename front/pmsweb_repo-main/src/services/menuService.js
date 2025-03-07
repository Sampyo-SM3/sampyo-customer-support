import axios from 'axios';

const API_URL = 'http://your-api-url.com/api'; // 실제 API URL로 변경하세요

export default {
  async getMenuItems() {
    try {
      const response = await axios.get(`${API_URL}/menu-items`);
      return response.data;
    } catch (error) {
      console.error('Error fetching menu items:', error);
      throw error;
    }
  }
};