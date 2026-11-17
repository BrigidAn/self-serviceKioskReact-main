// Single place for the API base URL. Override with REACT_APP_API_URL in a .env file.
export const API_URL = process.env.REACT_APP_API_URL || "https://localhost:5016/api";

export const authHeaders = () => {
  const token = localStorage.getItem("token");
  return token ? { Authorization: `Bearer ${token}` } : {};
};
