import React, { useEffect, useState } from "react";
import "./LandingPage.css";

function LandingPage() {
  // State to trigger robot image animation on mount
  const [animateRobot, setAnimateRobot] = useState(false);

  useEffect(() => {
    setAnimateRobot(true); // triggers slide-up animation
  }, []);

  return (
    <div className="landing-page">
      
      {/* Navigation Bar */}
      <nav className="navbar">
        <div className="logo">Tech Shack</div>
        <ul className="nav-links">
          <li>Home</li>
          <li>Products</li>
          <li>About</li>
          <li>Contact</li>
        </ul>
        <button className="signin-btn">Sign In</button>
      </nav>

      {/* Main Landing Section */}
      <div className="landing-container">
        
        {/* Left Section */}
        <div className="left-section">
          <h1 className="title">Tech Shack</h1>
          <p className="subtitle">Reliable Tech at an affordable price</p>

          {/* Tags on left */}
          <p className="tag tag1">Powerful CPU & SSD</p>
          <p className="tag tag2">Restaurant Robot</p>
        </div>

        {/* Center Image with slide-up animation */}
        <div className="center-image">
          <img
            src="https://via.placeholder.com/400x400"
            alt="Robot"
            className={`robot-img ${animateRobot ? "slide-up" : ""}`}
          />
        </div>

        {/* Right Section */}
        <div className="right-section">
          <p className="tag t-right1">AI Assistant</p>
          <p className="tag t-right2">Wide Range of Devices</p>
          <p className="tag t-right3">Smart House System</p>
        </div>

      </div>
    </div>
  );
}

export default LandingPage;
