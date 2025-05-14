// api/index.js
import axios from 'axios';

const apiClient = axios.create({
  // 조희재테스트
  baseURL: 'http://localhost:8080',
  //baseURL: process.env.VUE_APP_API_URL || '/csr/',
  timeout: 10000,
  headers: {
    'Content-Type': 'application/json'
  }
});

export default apiClient;