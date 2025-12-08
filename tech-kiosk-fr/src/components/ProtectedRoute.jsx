import React from "react";
import { Navigate } from "react-router-dom";
import { isLoggedIn } from "../utils/auth";

function ProtectedRoute({ children }) {
  if (!isLoggedIn()) {   // FIXED
    return <Navigate to="/" replace />;
  }
  return children;
}

export default ProtectedRoute;
