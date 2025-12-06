import React, { useState } from "react";
import { useNavigate } from "react-router-dom";
import { ToastContainer, toast } from "react-toastify";
import "react-toastify/dist/ReactToastify.css";
import "./Auth.css";

export default function Auth() {
  const [isLogin, setIsLogin] = useState(true);
  const navigate = useNavigate();

  const toggleMode = () => setIsLogin(!isLogin);

  const handleSubmit = (e) => {
    e.preventDefault();

    // Simulate login/register success
    const action = isLogin ? "logged in" : "registered";
    
    toast.success(`You have successfully ${action}!`, {
      position: "top-right",
      autoClose: 2500,
      hideProgressBar: false,
      closeOnClick: true,
      pauseOnHover: true,
      draggable: true,
      theme: "colored",
    });

    // Redirect after a short delay
    setTimeout(() => {
      navigate("/landing"); // replace "/" with your landing page route
    }, 2600);
  };

  return (
    <div className="auth-container">
      {/* Toast container */}
      <ToastContainer />

      {/* Background Shapes */}
      <div className="auth-floating c1"></div>
      <div className="auth-floating c2"></div>
      <div className="auth-floating c3"></div>
      <div className="auth-floating c4"></div>

      <div className="auth-content">
        <h1>{isLogin ? "Welcome Back" : "Join Us"}</h1>
        <p className="auth-sub">
          {isLogin
            ? "Sign in to continue to your dashboard"
            : "Create an account to get started"}
        </p>

        <div className="auth-inputs">
          {!isLogin && <input type="text" placeholder="Full Name" required />}
          <input type="email" placeholder="Email Address" required />
          <input type="password" placeholder="Password" required />
        </div>

        <button className="auth-btn" onClick={handleSubmit}>
          {isLogin ? "Login" : "Register"}
        </button>

        <p className="auth-toggle">
          {isLogin ? "Don't have an account?" : "Already have an account?"}{" "}
          <span onClick={toggleMode}>
            {isLogin ? " Register" : " Login"}
          </span>
        </p>
      </div>
    </div>
  );
}
