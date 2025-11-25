import React, { useEffect, useState } from "react";
import "./LandingPage.css";
import robotImg from "../../assests/illustrations/roboImg.png"
import logo from "../../assests/illustrations/logo.png"
import { useNavigate } from "react-router-dom";

function LandingPage() {
  const navigate = useNavigate();

  // State to trigger robot image animation on mount
  const [animateRobot, setAnimateRobot] = useState(false);
  const [animateTags, setAnimateTags] = useState(false);

  const [showForm, setShowForm] = useState(false); // show/hide login/register
  const [activeTab, setActiveTab] = useState("signin"); // 'signin' or 'register'

  const Login_API = "https://localhost:5016/api/auth/login";
  const Register_API = "https://localhost:5016/api/auth/register";

    // States for Sign In / Register
  const [signinEmail, setSigninEmail] = useState("");
  const [signinPassword, setSigninPassword] = useState("");
  const [registerName, setRegisterName] = useState("");
  const [registerEmail, setRegisterEmail] = useState("");
  const [registerPassword, setRegisterPassword] = useState("");

// State for feedback messages
  const [authMessage, setAuthMessage] = useState("");


  useEffect(() => {

    setTimeout(() => {
    setAnimateRobot(true); // triggers slide-up animation
    setAnimateTags(true);
    }, 200)
  }, []);

  // Handle Sign In
const handleSignIn = async (e) => {
  e.preventDefault();
  setAuthMessage("");
  try {
    const res = await fetch(Login_API, {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({
        email: signinEmail,
        password: signinPassword
      })
    });

    const data = await res.json();

    if (res.ok) {
      setAuthMessage("Sign In successful!");
      setShowForm(false); // close popup

      navigate("/products")
    } else {
      setAuthMessage(data.message || "Sign In failed");
    }
  } catch (error) {
    setAuthMessage("Error connecting to API");
  }
};

// Handle Register
const handleRegister = async (e) => {
  e.preventDefault();
  setAuthMessage("");
  try {
    const res = await fetch(Register_API, {   // fixed API variable
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({
        name: registerName,
        email: registerEmail,
        password: registerPassword
      })
    });

    const data = await res.json();

    if (res.ok) {
      setAuthMessage("Registration successful! Please Sign In");
      setActiveTab("signin"); // switch tab after register
    } else {
      setAuthMessage(data.message || "Registration failed");
    }
  } catch (error) {
    setAuthMessage("Error connecting to API");
  }
};


  return (
    <div className="landing-page">

      {/* Navigation Bar */}
      <nav className="navbar">
        <div className="logo">
          <img src={logo} alt="Tech Shack Logo" className="logo-img"/>
        </div>
        <ul className="nav-links">
          <li>Products</li>
          <li>About</li>
          <li>Contact</li>
        </ul>
        <button className="signin-btn" onClick={() => setShowForm(!showForm)}
        >Sign In</button>
      </nav>

      {/* Main Landing Section */}
      <div className="landing-container">

        {/* LEFT SIDE */}
        <div className="left-section">
          <h1 className="title">Smart Tech for Smart living</h1>
          <p className="subtitle">Reliable Tech at an affordable price</p>

          <p className={`tag tag1 ${animateTags ? "fade-in order1" : ""}`}>
            Powerful CPU & SSD
          </p>

          <p className={`tag tag2 ${animateTags ? "fade-in order2" : ""}`}>
            Restaurant Robot
          </p>
        </div>

        {/* CENTER IMAGE */}
        <div className="center-image">
          <img
            src={robotImg}
            alt="Robot"
            className={`robot-img ${animateRobot ? "slide-up" : ""}`}
          />
        </div>

        {/* RIGHT SIDE */}
        <div className="right-section">
          <p className={`tag t-right1 ${animateTags ? "fade-in order1" : ""}`}>
            AI Assistant
          </p>

          <p className={`tag t-right2 ${animateTags ? "fade-in order2" : ""}`}>
            Wide Range of Devices
          </p>

          <p className={`tag t-right3 ${animateTags ? "fade-in order3" : ""}`}>
            Smart House System
          </p>
        </div>

      </div>

      {/* sign in */}
      {showForm && (
        <div className="form-overlay">
          <div className="auth-form">
            {/* Tabs */}
            <div className="form-tabs">
              <button
                className={activeTab === "signin" ? "active-tab" : ""}
                onClick={() => setActiveTab("signin")}
              >
                Sign In
              </button>
              <button
                className={activeTab === "register" ? "active-tab" : ""}
                onClick={() => setActiveTab("register")}
              >
                Register
              </button>
            </div>

            {/* Form Body */}
            {activeTab === "signin" ?  (
              <form className="form-body" onSubmit={handleSignIn}>
                <div className="input-group">
                  <input type="email" placeholder="Email" value={signinEmail}
                    onChange={(e) => setSigninEmail(e.target.value)} required />
                  {authMessage && <p className="auth-message">{authMessage}</p>}
                  <label>Email</label>
                </div>
                <div className="input-group">
                  <input type="password" placeholder="Password" value={signinPassword}
                    onChange={(e) => setSigninPassword(e.target.value)} required />
                  {authMessage && <p className="auth-message">{authMessage}</p>}
                <label>Password</label>
                </div>
                <button type="submit" className="submit-btn">
                  {activeTab === "signin" ? "Sign In" : "Register"}
                </button>
              </form>
            ) : (
                <form className="form-body" onSubmit={handleRegister}>
                  <div className="input-group">
                    <input type="text" placeholder="Full Name" value={registerName}
                      onChange={(e) => setRegisterName(e.target.value)} required />
                    {authMessage && <p className="auth-message">{authMessage}</p>}
                    <label>FullName</label>
                  </div>
                  <div className="input-group">
                    <input type="email" placeholder="Email" value={registerEmail}
                      onChange={(e) => setRegisterEmail(e.target.value)} required />
                    {authMessage && <p className="auth-message">{authMessage}</p>}
                    <label>Email</label>
                  </div>
                  <div className="input-group">
                    <input type="password" placeholder="Password" value={registerPassword}
                      onChange={(e) => setRegisterPassword(e.target.value)} required />
                    {authMessage && <p className="auth-message">{authMessage}</p>}
                    <label>Password</label>
                  </div>

                <button type="submit" className="submit-btn">
                    {activeTab === "signin" ? "Sign In" : "Register"}
                </button>
              </form>
            )}

            <button
              className="close-btn"
              onClick={() => setShowForm(false)}
            >
              ×
            </button>
          </div>
        </div>
      )}
    </div>
  );
}

export default LandingPage;