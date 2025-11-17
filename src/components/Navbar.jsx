import React from "react";
import { Link, useNavigate } from "react-router-dom";
import { FaShoppingCart, FaUserCircle, FaBars } from "react-icons/fa";
import { useAuth } from "../context/AuthContext";
import "./Navbar.css";

function Navbar({ cartCount }) {
  const navigate = useNavigate();
  const { balance } = useAuth();

  const [open, setOpen] = React.useState(false);

  const handleLogout = () => {
    localStorage.removeItem("user");
    navigate("/");
  };

  const handleAddAccount = () => {
    navigate("/account");
  };

  return (
    <>
      {/* NAVBAR */}
      <nav className="navbar-glass">
        <div className="navbar-content">

          {/* HAMBURGER MENU BUTTON */}
          <button className="hamburger-btn" onClick={() => setOpen(true)}>
            <FaBars size={22} />
          </button>

          {/*BRAND */}
          <Link className="navbar-brand" to="/home">
            Tech Shack
          </Link>

          {/* RIGHT SIDE SECTION */}
          <div className="navbar-icons">

            {/*BALANCE */}
            <span className="balance-display">
              R{Number(balance).toFixed(2)}
            </span>

            {/*CART BUTTON */}
            <Link className="btn-glass" to="/cart">
              <FaShoppingCart size={18} />
              {cartCount > 0 && (
                <span className="cart-badge">
                  {cartCount}
                </span>
              )}
            </Link>

            {/* USER MENU */}
            <div className="dropdown">
              <button
                className="btn-glass dropdown-toggle ms-3"
                type="button"
                id="userDropdown"
                data-bs-toggle="dropdown"
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

                <li><hr className="dropdown-divider" /></li>

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

      {/* 🟦 SIDE MENU OVERLAY */}
      <div className={`side-overlay ${open ? "show" : ""}`} onClick={() => setOpen(false)}></div>

      {/* 🟦 SIDE MENU */}
      <div className={`side-menu ${open ? "open" : ""}`}>
        <button className="close-btn" onClick={() => setOpen(false)}>
          &times;
        </button>

        <h4 className="menu-title">Menu</h4>

        <Link className="side-link" to="/home" onClick={() => setOpen(false)}>
          Home
        </Link>

        <Link className="side-link" to="/category/laptops" onClick={() => setOpen(false)}>
          Laptops
        </Link>

        <Link className="side-link" to="/category/phones" onClick={() => setOpen(false)}>
          Phones
        </Link>

        <Link className="side-link" to="/category/accessories" onClick={() => setOpen(false)}>
          Accessories
        </Link>

        <Link className="side-link" to="/orders" onClick={() => setOpen(false)}>
          Orders
        </Link>

        <Link className="side-link" to="/account" onClick={() => setOpen(false)}>
          My Account
        </Link>
      </div>
    </>
  );
}

export default Navbar;
