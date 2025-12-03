import React, { useEffect, useState } from "react";
import "./LandingPage.css"
import { useNavigate } from "react-router-dom";
import Navbar from "../../components/Navbar";
import product1 from "../../assests/illustrations/dogrobot.png";
import product2 from "../../assests/illustrations/21.png";
import product3 from "../../assests/illustrations/robot-left.png";
import product4 from "../../assests/illustrations/robot-right.png";

function LandingPage() {
  const [animateCards, setAnimateCards] = useState(false);
  const [showForm, setShowForm] = useState(false); // Sign In modal
  const [activeTab, setActiveTab] = useState("signin"); // Signin / Register
  const navigate = useNavigate();

  useEffect(() => {
    setTimeout(() => setAnimateCards(true), 300);
  }, []);

  const products = [
    { id: 1, name: "Smart Robot", price: "$499", img: product1, style: { top: "10%", left: "15%", rotate: "-3deg" } },
    { id: 2, name: "AI Assistant", price: "$299", img: product2, style: { top: "30%", left: "55%", rotate: "2deg" } },
    { id: 3, name: "Smart Light", price: "$99", img: product3, style: { top: "55%", left: "25%", rotate: "-1deg" } },
    { id: 4, name: "Smart Speaker", price: "$149", img: product4, style: { top: "60%", left: "65%", rotate: "4deg" } },
  ];


  return (
    <div className="landing-page">
      <Navbar onSignInClick={() => setShowForm(true)} />

      {/* Floating Tech Shapes */}
      <div className="floating-bg">
        <div className="circle c1"></div>
        <div className="circle c2"></div>
        <div className="circle c3"></div>
        <div className="circle c4"></div>
      </div>

      {/* Hero Section */}
      <div className="hero-section">
        <div className="hero-text">
          <h1 className="title">Smart Tech for Modern Living</h1>
          <p className="subtitle">Discover our latest innovative products</p>
          <button className="view-products-btn" onClick={() => navigate("/products")}>View All Products</button>
        </div>

        {/* Floating Product Cards */}
        {products.map((p, idx) => (
          <div
            key={p.id}
            className={`product-card ${animateCards ? "fade-in" : ""}`}
            style={{
              top: p.style.top,
              left: p.style.left,
              transform: `rotate(${p.style.rotate})`,
              transitionDelay: `${idx * 0.3}s`,
            }}
          >
            <img src={p.img} alt={p.name} />
            <div className="product-info">
              <h3>{p.name}</h3>
              <p>{p.price}</p>
            </div>
          </div>
    ))}
  </div>

  {/* Sign In / Register Modal */}
      {showForm && (
        <div className="form-overlay">
          <div className="auth-form">
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

            <form className="form-body">
              {activeTab === "signin" ? (
                <>
                  <div className="input-group">
                    <input type="email" placeholder="Email" required />
                    <label>Email</label>
                  </div>
                  <div className="input-group">
                    <input type="password" placeholder="Password" required />
                    <label>Password</label>
                  </div>
                  <button type="submit" className="submit-btn">
                    Sign In
                  </button>
                </>
              ) : (
                <>
                  <div className="input-group">
                    <input type="text" placeholder="Full Name" required />
                    <label>Full Name</label>
                  </div>
                  <div className="input-group">
                    <input type="email" placeholder="Email" required />
                    <label>Email</label>
                  </div>
                  <div className="input-group">
                    <input type="password" placeholder="Password" required />
                    <label>Password</label>
                  </div>
                  <button type="submit" className="submit-btn">
                    Register
                  </button>
                </>
              )}
            </form>

            <button className="close-btn" onClick={() => setShowForm(false)}>
              ×
            </button>
          </div>
        </div>
      )}
    </div>
  );
}

export default LandingPage;
