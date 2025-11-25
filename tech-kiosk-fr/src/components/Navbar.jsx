import React, { useState } from "react";

export default function Navbar({
  setShowLogin,
  setShowRegister,
  setPage,
  cartCount,
  isLoggedIn,
}) {
  const [menuOpen, setMenuOpen] = useState(false);

  return (
    <nav className="navbar">
      <h2 className="logo">RoboStore</h2>

      <ul className="nav-links">
        <li onClick={() => setPage("home")}>Home</li>
        <li onClick={() => setPage("products")}>Products</li>
      </ul>

      <div className="cart-icon">
        🛒
        {cartCount > 0 && <span className="cart-count">{cartCount}</span>}
      </div>

      {!isLoggedIn ? (
        <>
          <button onClick={() => setShowLogin(true)}>Login</button>
          <button onClick={() => setShowRegister(true)}>Register</button>
        </>
      ) : (
        <span className="logged-in-msg">Welcome!</span>
      )}

      <button className="hamburger-btn" onClick={() => setMenuOpen(!menuOpen)}>
        ☰
      </button>

      {menuOpen && (
        <ul className="hamburger-dropdown">
          <li onClick={() => setPage("home")}>Home</li>
          <li onClick={() => setPage("products")}>Products</li>
        </ul>
      )}
    </nav>
  );
}
