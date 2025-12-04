import React from "react";
import { Link } from "react-router-dom";
import { FiShoppingCart } from "react-icons/fi";
import "./Navbar.css";
import HamburgerMenu from "../components/HamburgerMenu";

export default function NavBar({ cartCount = 0 }) {
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

        <HamburgerMenu/>
        
      </div>
    </nav>
  );
}
