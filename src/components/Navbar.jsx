import React, { useState, useRef, useEffect } from "react";
import { Link, useNavigate } from "react-router-dom";
import { FiShoppingCart } from "react-icons/fi";
import "./Navbar.css";

export default function NavBar({ cartCount = 0 }) {
  const [menuOpen, setMenuOpen] = useState(false);
  const menuRef = useRef(null);
  const navigate = useNavigate();

  const handleLogout = () => {
    localStorage.removeItem("token");
    navigate("/login");
    setMenuOpen(false);
  };

  // Close menu when clicking outside
  useEffect(() => {
    const handleClickOutside = (event) => {
      if (menuRef.current && !menuRef.current.contains(event.target)) {
        setMenuOpen(false);
      }
    };
    document.addEventListener("mousedown", handleClickOutside);
    return () => document.removeEventListener("mousedown", handleClickOutside);
  }, []);

  return (
    <nav className="velvety-nav">
      <div className="nav-inner">
        {/* LOGO */}
        <Link to="/" className="nav-logo">
          <span>Tech Shack</span>
        </Link>

        {/* LINKS */}
        <div className="nav-links">
          <Link to="/products">Products</Link>
          <Link to="/about">About</Link>
          <Link to="/support">Support</Link>
        </div>

        {/* CART */}
        <Link to="/cart" className="nav-cart">
          <FiShoppingCart size={22} />
          {cartCount > 0 && <span className="nav-cart-count">{cartCount}</span>}
        </Link>

        {/* HAMBURGER ICON */}
        <div
          className={`hamburger ${menuOpen ? "open" : ""}`}
          onClick={() => setMenuOpen(!menuOpen)}
        >
          <span></span>
          <span></span>
          <span></span>
        </div>

        {/* SIDE MENU */}
        <div ref={menuRef} className={`side-menu ${menuOpen ? "show" : ""}`}>
          <h2 className="side-menu-title">Menu</h2>
          <ul>
            <li>
              <Link to="/account" onClick={() => setMenuOpen(false)}>
                My Account
              </Link>
            </li>
            <li>
              <Link to="/orders" onClick={() => setMenuOpen(false)}>
                Order History
              </Link>
            </li>
            <li>
              <Link to="/transactions" onClick={() => setMenuOpen(false)}>
                Transactions
              </Link>
            </li>
            <li className="logout" onClick={handleLogout}>
              Logout
            </li>
          </ul>
        </div>

        {/* Overlay */}
        {menuOpen && <div className="menu-overlay" onClick={() => setMenuOpen(false)}></div>}
      </div>
    </nav>
  );
}
