import React, { useState } from "react";
import { Link, useNavigate } from "react-router-dom";
import { FiShoppingCart } from "react-icons/fi";
import "./Navbar.css";

export default function NavBar({ cartCount = 0 }) {
  const [menuOpen, setMenuOpen] = useState(false);
  const navigate = useNavigate();

  const handleLogout = () => {
    localStorage.removeItem("token");
    navigate("/login");
    setMenuOpen(false);
  };

  return (
    <nav className="neo-nav">
      <div className="nav-shell">

        {/* LOGO */}
        <Link to="/landing" className="nav-logo">
          Tech<span className="highlight">Shack</span>
        </Link>

        {/* DESKTOP LINKS */}
        <div className="nav-links">
          <Link to="/products" className="nav-item">Products</Link>
          <Link to="/about" className="nav-item">About</Link>
          <Link to="/support" className="nav-item">Support</Link>
        </div>

        {/* CART */}
        <Link to="/cart" className="nav-cart">
          <FiShoppingCart size={23} />
          {cartCount > 0 && <span className="nav-cart-count">{cartCount}</span>}
        </Link>

        {/* HAMBURGER */}
        <div
          className={`hamburger ${menuOpen ? "open" : ""}`}
          onClick={() => setMenuOpen(!menuOpen)}
        >
          <span></span>
          <span></span>
          <span></span>
        </div>

        {/* MOBILE SIDE MENU */}
        {menuOpen && <div className="menu-overlay" onClick={() => setMenuOpen(false)}></div>}
        <div className={`side-menu ${menuOpen ? "show" : ""}`}>
          <div className="side-menu-title">Menu</div>
          <ul>
            <li>
              <Link to="/account" onClick={() => setMenuOpen(false)}>My Account</Link>
            </li>
            <li>
              <Link to="/orders" onClick={() => setMenuOpen(false)}>Order History</Link>
            </li>
            <li>
              <Link to="/transactions" onClick={() => setMenuOpen(false)}>Transactions</Link>
            </li>
            <li className="logout" onClick={handleLogout}>Logout</li>
          </ul>
        </div>

      </div>
    </nav>
  );
}
