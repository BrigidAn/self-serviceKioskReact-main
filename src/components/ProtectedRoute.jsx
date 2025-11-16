import { Navigate } from "react-router-dom";

export default function ProtectedRoute({ children }) {
    const isLoggedIn = sessionStorage.getItem("loggedIn");

    return isLoggedIn ? children : <Navigate to="/login" />;
}