import axios from "axios";

const API = "http://localhost:5016/api/Auth"; // your backend port

// Save token to localStorage
const saveToken = (token) => {
  localStorage.setItem("token", token);
};

// Get token from localStorage
const getToken = () => {
  return localStorage.getItem("token");
};

// Remove token
const removeToken = () => {
  localStorage.removeItem("token");
};

// Attach token to axios automatically
axios.interceptors.request.use(
  (config) => {
    const token = getToken();
    if (token) {
      config.headers.Authorization = `Bearer ${token}`;
    }
    return config;
  },
  (err) => Promise.reject(err)
);

// REGISTER USER
export const registerUser = async (data) => {
  try {
    const res = await axios.post(`${API}/register`, data);

    if (res.data.token) {
      saveToken(res.data.token);
    }

    return res.data;
  } catch (error) {
    throw error.response?.data || error;
  }
};

// LOGIN USER
export const loginUser = async (data) => {
  try {
    const res = await axios.post(`${API}/login`, data);

    if (res.data.token) {
      saveToken(res.data.token);
    }

    return res.data;
  } catch (error) {
    throw error.response?.data || error;
  }
};

// LOGOUT USER
export const logoutUser = async () => {
  try {
    await axios.post(`${API}/logout`);
    removeToken();
  } catch (error) {
    removeToken();
  }
};

// GET CURRENT LOGGED-IN USER (decode token)
export const getCurrentUser = () => {
  const token = getToken();
  if (!token) return null;

  // decode JWT payload
  const payload = JSON.parse(atob(token.split(".")[1]));

  return {
    id: payload["nameid"],
    email: payload["email"],
    name: payload["unique_name"],
    roles: payload["role"] instanceof Array ? payload["role"] : [payload["role"]],
  };
};

// CHECK IF USER IS ADMIN
export const isAdmin = () => {
  const user = getCurrentUser();
  return user?.roles?.includes("Admin");
};

// CHECK IF USER IS LOGGED IN
export const isLoggedIn = () => {
  return !!getToken();
};
