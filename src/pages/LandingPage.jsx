import React, { useState } from "react";
import { useNavigate } from "react-router-dom";
import axios from "axios";
import "./LandingPage.css";

const API_BASE_URL = "https://localhost:5016/api/Auth";

function LandingPage() {
  const [isLogin, setIsLogin] = useState(true); // toggle login/register
  const [loginData, setLoginData] = useState({ email: "", password: "" });
  const [registerData, setRegisterData] = useState({
    name: "",
    email: "",
    password: "",
    confirmPassword: "",
  });
  const [error, setError] = useState("");
  const navigate = useNavigate();

  // Handle Login
  const handleLogin = async (e) => {
    e.preventDefault();
    setError("");
    try {
      const response = await axios.post(`${API_BASE_URL}/login`, loginData);
      localStorage.setItem("user", JSON.stringify(response.data.user));
      navigate("/home");
    } catch (err) {
      setError("Invalid email or password.");
    }
  };

  // Handle Register
  const handleRegister = async (e) => {
    e.preventDefault();
    setError("");
    if (registerData.password !== registerData.confirmPassword) {
      setError("Passwords do not match.");
      return;
    }
    try {
      const response = await axios.post(`${API_BASE_URL}/register`, registerData);
      alert(response.data.message || "Registration successful!");
      setIsLogin(true);
    } catch (err) {
      setError(err.response?.data?.message || "Registration failed.");
    }
  };

  return (
    <div className="landing-container">
      <div className="split-card">
        {/* Left: Form */}
        <div className={`form-section ${isLogin ? "" : "shift-left"}`}>
          <h1>Tech Shack</h1>
          <p>Powering Your Tech Experience</p>

          <div className="auth-wrapper">
            {error && <div className="auth-error">{error}</div>}

            {isLogin ? (
              <form className="auth-form" onSubmit={handleLogin}>
                <h2>Login</h2>
                <input
                  type="email"
                  placeholder="Email"
                  value={loginData.email}
                  onChange={(e) => setLoginData({ ...loginData, email: e.target.value })}
                  required
                />
                <input
                  type="password"
                  placeholder="Password"
                  value={loginData.password}
                  onChange={(e) => setLoginData({ ...loginData, password: e.target.value })}
                  required
                />
                <button type="submit">Login</button>
                <p>
                  Don’t have an account?{" "}
                  <span className="toggle-link" onClick={() => setIsLogin(false)}>
                    Register
                  </span>
                </p>
              </form>
            ) : (
              <form className="auth-form" onSubmit={handleRegister}>
                <h2>Register</h2>
                <input
                  type="text"
                  placeholder="Full Name"
                  value={registerData.name}
                  onChange={(e) => setRegisterData({ ...registerData, name: e.target.value })}
                  required
                />
                <input
                  type="email"
                  placeholder="Email"
                  value={registerData.email}
                  onChange={(e) => setRegisterData({ ...registerData, email: e.target.value })}
                  required
                />
                <input
                  type="password"
                  placeholder="Password"
                  value={registerData.password}
                  onChange={(e) => setRegisterData({ ...registerData, password: e.target.value })}
                  required
                />
                <input
                  type="password"
                  placeholder="Confirm Password"
                  value={registerData.confirmPassword}
                  onChange={(e) =>
                    setRegisterData({ ...registerData, confirmPassword: e.target.value })
                  }
                  required
                />
                <button type="submit">Register</button>
                <p>
                  Already have an account?{" "}
                  <span className="toggle-link" onClick={() => setIsLogin(true)}>
                    Login
                  </span>
                </p>
              </form>
            )}
          </div>
        </div>

        <div className="image-section">
          <img
            src="https://plus.unsplash.com/premium_vector-1711987763521-2327571e0987?w=500&auto=format&fit=crop&q=60&ixlib=rb-4.1.0&ixid=M3wxMjA3fDB8MHxwaG90by1yZWxhdGVkfDIwM3x8fGVufDB8fHx8fA%3D%3D"
            alt="Abstract tech"
          />
        </div>
      </div>
    </div>
  );
}

export default LandingPage;