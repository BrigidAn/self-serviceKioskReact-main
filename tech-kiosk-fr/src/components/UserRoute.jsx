import { Navigate } from "react-router-dom";
import { isLoggedIn, getRole } from "../utils/auth";

export default function UserRoute({ children }) {
  if (!isLoggedIn()) return <Navigate to="/" replace />;

  if (getRole() === "Admin") return <Navigate to="/admin/dashboard" replace />;

  return children;
}