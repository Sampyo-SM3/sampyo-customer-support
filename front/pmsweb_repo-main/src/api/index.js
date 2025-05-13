// api/index.js
import axios from 'axios';

const apiClient = axios.create({
  //baseURL: 'http://10.50.10.10:29001',
  baseURL: 'http://localhost:8080',
  timeout: 10000,
  headers: {
    'Content-Type': 'application/json'
  }
});

export default apiClient;