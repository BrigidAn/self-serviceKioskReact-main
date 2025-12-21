import React from "react";
import "./AboutPage.css";
import NavBar from "../../components/Navbar";

function AboutPage() {
  return (
    <>
      <NavBar />

      <div className="about-container">

        <div className="about-floating c1"></div>
        <div className="about-floating c2"></div>
        <div className="about-floating c3"></div>
        <div className="about-floating c4"></div>

        <div className="about-inner">
          <h1 className="about-title">About Us</h1>
          <p className="about-subtitle">
            Empowering seamless, futuristic self-service experiences.
          </p>

          <section className="about-section">
            <h2 className="section-title">Our Mission</h2>
            <p className="section-text">
              We aim to redefine the modern self-service experience...
            </p>
          </section>

          <section className="about-section">
            <h2 className="section-title">Our Story</h2>
            <p className="section-text">
              What started as a simple automated kiosk idea...
            </p>
          </section>

          <section className="about-section">
            <h2 className="section-title">Why Choose Us?</h2>

            <div className="features-grid">

              <div className="feature-card">
                <h3 className="feature-title">🔒 Secure System</h3>
                <p className="feature-text">
                  Built on ASP.NET Identity + JWT roles for powerful authentication.
                </p>
              </div>

              <div className="feature-card">
                <h3 className="feature-title">⚡ Fast & Smooth</h3>
                <p className="feature-text">
                  Optimized React frontend, fast browsing & checkout.
                </p>
              </div>

              <div className="feature-card">
                <h3 className="feature-title">🎨 Futuristic UI</h3>
                <p className="feature-text">
                  Neon gradients + glow animations.
                </p>
              </div>

              <div className="feature-card">
                <h3 className="feature-title">📊 Transparent Tracking</h3>
                <p className="feature-text">
                  Real-time balances, top-ups & activity.
                </p>
              </div>

            </div>
          </section>
        </div>
      </div>
    </>
  );
}

export default AboutPage;
