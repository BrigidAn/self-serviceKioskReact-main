export function isLoggedIn() {
  const token = localStorage.getItem("token");
  if (!token) return false;

  try {
    const payload = JSON.parse(atob(token.split(".")[1]));
    const exp = payload.exp;
    const now = Math.floor(Date.now() / 1000);

    // Token expired?
    return exp > now;
  } catch (err) {
    return false;
  }
}

// Get the user's role
export function getRole() {
  const role = localStorage.getItem("role");
  return role || null; // returns "Admin", "User", or null
}

// Optional: clear auth (for logout)
export function logout() {
  localStorage.removeItem("token");
  localStorage.removeItem("role");
}
