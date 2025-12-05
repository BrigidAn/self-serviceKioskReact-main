import React from "react";
import "./AboutPage.css";

function AboutPage() {
  return (
    <div className="about-container">

      {/* Floating Background Shapes */}
      <div className="about-floating c1"></div>
      <div className="about-floating c2"></div>
      <div className="about-floating c3"></div>
      <div className="about-floating c4"></div>

      <div className="about-inner">
        {/* Header */}
        <h1 className="about-title">About Us</h1>
        <p className="about-subtitle">
          Empowering seamless, futuristic self-service experiences.
        </p>

        {/* Mission Section */}
        <section className="about-section">
          <h2 className="section-title">Our Mission</h2>
          <p className="section-text">
            We aim to redefine the modern self-service experience by blending 
            intuitive design, real-time processing, and immersive interfaces. 
            Our kiosk ecosystem ensures effortless interaction, fast checkout, 
            and a visually engaging environment that feels futuristic yet familiar.
          </p>
        </section>

        {/* Story Section */}
        <section className="about-section">
          <h2 className="section-title">Our Story</h2>
          <p className="section-text">
            What started as a simple automated kiosk idea evolved into a full 
            end-to-end digital ecosystem.
          </p>
        </section>

        {/* Features Grid */}
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
                Optimized React frontend, instant product browsing & frictionless checkout.
              </p>
            </div>

            <div className="feature-card">
              <h3 className="feature-title">🎨 Futuristic UI</h3>
              <p className="feature-text">
                Neon gradients, floating shapes, ambient glow animations across all pages.
              </p>
            </div>

            <div className="feature-card">
              <h3 className="feature-title">📊 Transparent Tracking</h3>
              <p className="feature-text">
                Users can view balances, top-ups, transactions & activity in real-time.
              </p>
            </div>

          </div>
        </section>
      </div>
    </div>
  );
}
export default AboutPage;