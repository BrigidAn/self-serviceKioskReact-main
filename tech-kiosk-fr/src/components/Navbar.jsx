import React from "react";
import { Link } from "react-router-dom";
import "./Navbar.css";

function Navbar({ onSignIn }) {
  return (
    <nav className="nav">
      <div className="nav-left">
        <Link to="/" className="brand">Tech Shack</Link>
      </div>

      <div className="nav-right">
        <Link to="/">Home</Link>
        <Link to="/home">About</Link>
        <Link to="/home">Services</Link>
        <button className="signin-btn" onClick={onSignIn}>SignIn</button>
      </div>
    </nav>
  );
}

export default Navbar;
