import { Navigate } from "react-router-dom";
import { isLoggedIn, getRole } from "../utils/auth";

export default function AdminRoute({ children }) {
  if (!isLoggedIn()) return <Navigate to="/" replace />;
  if (getRole() !== "Admin") return <Navigate to="/landing" replace />;

  return children;
}
