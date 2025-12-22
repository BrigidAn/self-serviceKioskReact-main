export function isLoggedIn() {
  const token = localStorage.getItem("token");
  if (!token) return false;

  try {
    const payload = JSON.parse(atob(token.split(".")[1]));
    const exp = payload.exp;
    const now = Math.floor(Date.now() / 1000);

    return exp > now;
  } catch (err) {
    return false;
  }
}

export function getRole() {
  const role = localStorage.getItem("role");
  return role || null;
}

export function logout() {
  localStorage.removeItem("token");
  localStorage.removeItem("role");
}
