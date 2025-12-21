import React, { useState } from "react";
import { useNavigate } from "react-router-dom";
import { ToastContainer, toast } from "react-toastify";
import "react-toastify/dist/ReactToastify.css";
import "./Auth.css";

export default function Auth() {
  const [isLogin, setIsLogin] = useState(true);
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [fullName, setFullName] = useState("");

  const navigate = useNavigate();

  const toggleMode = () => setIsLogin(!isLogin);

  const handleSubmit = async (e) => {
    e.preventDefault();

    const url = isLogin
      ? "https://localhost:5016/api/Auth/login"
      : "https://localhost:5016/api/Auth/register";

    const body = isLogin
      ? { email, password }
      : { fullName, email, password };

    try {
      const response = await fetch(url, {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify(body),
      });

      if (!response.ok) throw new Error("Authentication failed");

      const data = await response.json();

      localStorage.setItem("token", data.token);
      const role = data.user.roles[0];
      localStorage.setItem("role", role);

      toast.success(
        isLogin ? "Login successful!" : "Account created successfully!",
        { autoClose: 2000 }
      );

      setTimeout(() => {
      if (role === "Admin") {
        navigate("/admin/dashboard", { replace: true });
      } else {
        navigate("/landing", { replace: true });
      }
    }, 2200);

  } catch (err) {
    toast.error(err.message || "Something went wrong");
  }
  };

  return (
    <div className="auth-container">
      <ToastContainer />

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

        <form className="auth-inputs" onSubmit={handleSubmit}>
          {!isLogin && (
            <input
              type="text"
              placeholder="Full Name"
              value={fullName}
              onChange={(e) => setFullName(e.target.value)}
              required
            />
          )}

          <input
            type="email"
            placeholder="Email Address"
            value={email}
            onChange={(e) => setEmail(e.target.value)}
            required
          />

          <input
            type="password"
            placeholder="Password"
            minLength="6"
            value={password}
            onChange={(e) => setPassword(e.target.value)}
            required
          />

          <button type="submit" className="auth-btn">
            {isLogin ? "Login" : "Register"}
          </button>
        </form>

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
