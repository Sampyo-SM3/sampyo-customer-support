// api/index.js
import axios from 'axios';

const apiClient = axios.create({
  // baseURL: process.env.VUE_APP_API_URL || 'http://10.50.10.10:29001/',
  baseURL: process.env.VUE_APP_API_URL || '/csr/',
  
  timeout: 10000,
  headers: {
    'Content-Type': 'application/json'
  }
});

export default apiClient;