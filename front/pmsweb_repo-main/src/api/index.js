import axios from 'axios';

const apiClient = axios.create({
  // baseURL: 'http://localhost:8080',  
  // baseURL: process.env.VUE_APP_API_URL || '/csr/',  
  baseURL: process.env.VUE_APP_API_URL,
  timeout: 10000,
  headers: {
    'Content-Type': 'application/json'
  }
});

export default apiClient;