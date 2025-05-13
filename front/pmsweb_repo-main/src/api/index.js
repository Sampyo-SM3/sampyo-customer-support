// api/index.js
import axios from 'axios';

const apiClient = axios.create({
  baseURL: process.env.VUE_APP_API_URL || '/csr/',
  timeout: 10000,
  headers: {
    'Content-Type': 'application/json'
  }
});

export default apiClient;