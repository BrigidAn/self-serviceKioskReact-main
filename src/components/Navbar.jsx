import React from "react";
import { Link, useNavigate } from "react-router-dom";
import { FaShoppingCart, FaUserCircle } from "react-icons/fa";
import "./Navbar.css";

function Navbar({ cartCount }) {
  const navigate = useNavigate();

  const handleLogout = () => {
    localStorage.removeItem("user");
    navigate("/");
  };

  const handleAddAccount = () => {
    navigate("/account");
  };

  return (
    <nav className="navbar-glass navbar navbar-expand-lg navbar-dark px-4">
      <div className="container-fluid">
        {/* Brand */}
        <Link className="navbar-brand fw-bold" to="/">
          Tech Shack
        </Link>

        {/* Navbar Links */}
        <div className="collapse navbar-collapse">
          <ul className="navbar-nav me-auto mb-2 mb-lg-0">
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
        </div>

        {/* Right-side Icons */}
        <div className="navbar-icons d-flex align-items-center">
          {/* Cart */}
          <Link className="btn-glass" to="/cart">
            <FaShoppingCart size={18} />
            {cartCount > 0 && (
              <span className="position-absolute top-0 start-100 translate-middle badge rounded-pill bg-light text-dark" style={{ fontSize: "0.7rem" }}>
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
            <ul className="dropdown-menu dropdown-menu-end shadow " aria-labelledby="userDropdown">
              <li>
                <button className="dropdown-item" onClick={handleAddAccount}>
                  Add Account Balance
                </button>
              </li>
              <li>
                <hr className="dropdown-divider" />
              </li>
              <li>
                <button className="dropdown-item text-danger" onClick={handleLogout}>
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
