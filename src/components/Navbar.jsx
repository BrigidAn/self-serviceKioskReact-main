import React from "react";
import { Link, useNavigate } from "react-router-dom";
import { FaShoppingCart, FaUserCircle } from "react-icons/fa";

function Navbar({ cartCount }) {
  const navigate = useNavigate();

  const handleLogout = () => {
    // Example: remove auth data from local storage (you can adjust this)
    localStorage.removeItem("user");
    navigate("/login");
  };

  const handleAddAccount = () => {
    // redirect to account balance page
    navigate("/account");
  };

  return (
    <nav
      className="navbar navbar-expand-lg p-3 mb-4"
      style={{
        background:
          "linear-gradient(90deg, rgba(55,153,191,1) 0%, rgba(87,199,133,1) 84%)",
        color: "white",
      }}
    >
      <div className="container-fluid d-flex justify-content-between align-items-center">
        <Link
          className="navbar-brand text-white fw-bold"
          to="/"
          style={{ textDecoration: "none" }}
        >
          Tech Stack
        </Link>

        <div className="d-flex align-items-center">
          {/* Cart Button */}
          <Link className="btn btn-outline-light mx-2 position-relative" to="/cart">
            <FaShoppingCart size={18} />
            {cartCount > 0 && (
              <span
                className="position-absolute top-0 start-100 translate-middle badge rounded-pill bg-light text-dark"
                style={{ fontSize: "0.7rem" }}
              >
                {cartCount}
              </span>
            )}
          </Link>

          {/* User Dropdown */}
          <div className="dropdown">
            <button
              className="btn btn-outline-light dropdown-toggle d-flex align-items-center"
              type="button"
              id="userDropdown"
              data-bs-toggle="dropdown"
              aria-expanded="false"
            >
              <FaUserCircle size={20} className="me-2" />
              Account
            </button>

            <ul
              className="dropdown-menu dropdown-menu-end shadow"
              aria-labelledby="userDropdown"
            >
              <li>
                <button
                  className="dropdown-item"
                  onClick={handleAddAccount}
                >
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
