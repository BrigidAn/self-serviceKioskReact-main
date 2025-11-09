import React from "react";
import "./LandingPage.css"
import { Link } from "react-router-dom";

function LandingPage() {
  return (
    <div className="landing-container">
      <div className="landing-left">
        <div className="landing-overlay">
          <h1 className="landing-title">Smart Kiosk System</h1>
          <p className="landing-subtitle">
            Fast • Secure • Convenient
          </p>
          <div className="landing-buttons">
            <Link to="/login" className="btn-primary">Login</Link>
            <Link to="/register" className="btn-outline">Register</Link>
          </div>
        </div>
      </div>

      <div className="landing-right">
        <div className="login-preview-card">
          <h2>Experience seamless shopping</h2>
          <p>
            Manage your cart, view your balance, and shop instantly from the kiosk interface.
          </p>
          <ul>
            <li>💳 Secure payments</li>
            <li>📊 Real-time balance updates</li>
            <li>🛒 Smooth checkout process</li>
          </ul>
        </div>
      </div>
    </div>
  );
}

export default LandingPage;