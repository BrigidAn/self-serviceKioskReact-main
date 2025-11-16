import React from "react";
import { Link, useNavigate } from "react-router-dom";
import { FaShoppingCart, FaUserCircle } from "react-icons/fa";
import { useAuth } from "../context/AuthContext";
import "./Navbar.css";

function Navbar({ cartCount }) {
  const navigate = useNavigate();
  const { balance } = useAuth();


  const handleLogout = () => {
    localStorage.removeItem("user");
    navigate("/");
  };

  const handleAddAccount = () => {
    navigate("/account");
  };

  return (
    <nav className="navbar-glass">
      <div className="navbar-content">
        {/* Brand */}
        <Link className="navbar-brand" to="/home">
          Tech Shack
        </Link>

        {/* Navbar Links */}
        <ul className="navbar-links">
          <li className="nav-item">
            <Link className="nav-link" to="/products">
              Products
            </Link>
          </li>
          <li className="nav-item">
            <Link className="nav-link" to="/about">
              About
            </Link>
          </li>
        </ul>

        {/* Right-side Icons */}
        <div className="navbar-icons">
           <span style={{ color: "#00d9ff", marginRight: 12 }}>R{Number(balance).toFixed(2)}</span>
          {/* Cart */}
          <Link className="btn-glass" to="/cart">
            <FaShoppingCart size={18} />
            {cartCount > 0 && (
              <span className="cart-badge" style={{ fontSize: "0.7rem" }}>
                {cartCount}
              </span>
            )}
          </Link>

          {/* User Dropdown */}
          <div className="dropdown">
            <button
              className="btn-glass dropdown-toggle ms-3"
              type="button"
              id="userDropdown"
              data-bs-toggle="dropdown"
              aria-expanded="false"
            >
              <FaUserCircle size={20} className="me-2" />
              Account
            </button>
            <ul className="dropdown-menu dropdown-menu-end">
              <li>
                <button className="dropdown-item" onClick={handleAddAccount}>
                  Add Account Balance
                </button>
              </li>
              <li>
                <hr className="dropdown-divider" />
              </li>
              <li>
                <button
                  className="dropdown-item text-danger"
                  onClick={handleLogout}
                >
                  Logout
                </button>
              </li>
            </ul>
          </div>
        </div>
      </div>
    </nav>
  );
}

export default Navbar;
