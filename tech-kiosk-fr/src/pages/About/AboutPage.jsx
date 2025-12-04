import React from "react";
import NavBar from "../../components/Navbar";
import "./AboutPage.css";

function AboutPage() {
  return (
    <div className="about-page">
      <NavBar />

      <section className="about-hero">
        <div className="hero-content">
          <h1>Welcome to Tech Shack</h1>
          <p>
            Your one-stop shop for the latest tech gadgets and smart accessories.
            From high-quality earbuds and headsets to cutting-edge robots and VR headsets,
            we bring innovation right to your doorstep.
          </p>
        </div>
        <div className="hero-image">
          <img
            src="https://via.placeholder.com/500x400?text=Tech+Shack"
            alt="Tech Shack"
          />
        </div>
      </section>

      <section className="about-products">
        <h2>Our Products</h2>
        <div className="product-grid">
          <div className="product-card">
            <img src="https://via.placeholder.com/250?text=Earbuds" alt="Earbuds" />
            <h3>Earbuds</h3>
            <p>High-quality wireless earbuds for music and calls with crystal-clear sound.</p>
          </div>

          <div className="product-card">
            <img src="https://via.placeholder.com/250?text=Robots" alt="Robots" />
            <h3>Robots</h3>
            <p>Smart robots for restaurants, delivery, and more, bringing automation to life.</p>
          </div>

          <div className="product-card">
            <img src="https://via.placeholder.com/250?text=Headsets" alt="Headsets" />
            <h3>Headsets</h3>
            <p>Comfortable gaming and professional headsets for immersive audio experiences.</p>
          </div>

          <div className="product-card">
            <img src="https://via.placeholder.com/250?text=VR+Headsets" alt="VR Headsets" />
            <h3>VR Headsets</h3>
            <p>Step into virtual worlds with our latest VR headsets for gaming and simulations.</p>
          </div>
        </div>
      </section>

      <section className="about-mission">
        <h2>Our Mission</h2>
        <p>
          At Tech Shack, we aim to make advanced technology accessible and exciting for everyone.
          We are committed to providing quality products, excellent customer service, and innovation that keeps you ahead of the curve.
        </p>
      </section>
    </div>
  );
}

export default AboutPage;