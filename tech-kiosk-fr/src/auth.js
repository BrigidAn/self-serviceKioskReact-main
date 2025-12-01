export const saveAuthData = (user, token) => {
  localStorage.setItem("token", token);
  localStorage.setItem("userId", user.userId);
  localStorage.setItem("role", user.role); // for admin redirect
  localStorage.setItem("name", user.name); // optional
};
 
// Get JWT token
export const getToken = () => {
  return localStorage.getItem("token");
};

// Get userId
export const getUserId = () => {
  return localStorage.getItem("userId");
};

// Get role
export const getRole = () => {
  return localStorage.getItem("role");
};

// Get user name
export const getUserName = () => {
  return localStorage.getItem("name");
};

// Check if user is logged in
export const isLoggedIn = () => {
  return !!localStorage.getItem("token");
};

// Check if user is admin
export const isAdmin = () => {
  return getRole() === "Admin";
};

// Logout user
export const logout = () => {
  localStorage.removeItem("token");
  localStorage.removeItem("userId");
  localStorage.removeItem("role");
  localStorage.removeItem("name");
  window.location.href = "/"; // redirect to landing page
};

// Fetch wrapper to include JWT automatically
export const authFetch = async (url, options = {}) => {
  const token = getToken();
  const headers = {
    "Content-Type": "application/json",
    ...(options.headers || {}),
    ...(token ? { Authorization: `Bearer ${token}` } : {}),
  };

  const fetchOptions = {
    ...options,
    headers,
  };

  try {
    const res = await fetch(url, fetchOptions);
    if (res.status === 401) {
      logout(); // token invalid or expired
      return null;
    }
    return res;
  } catch (err) {
    console.error("Error in authFetch:", err);
    return null;
  }
};

// Example usage:
// import { saveAuthData, logout, getToken } from './auth';
// saveAuthData(user, token);
// const token = getToken();
