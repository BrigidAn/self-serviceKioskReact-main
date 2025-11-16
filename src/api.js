import axios from 'axios';

//this is so that we don't have to repeat the base URL and withCredentials in every request
const api = axios.create({
  baseURL: 'http://localhost:5016/api',
  withCredentials: true, // important for session-based auth
  headers: {
    'Content-Type': 'application/json',
  },
});

export default api;