import React, { useState } from "react";
import { Link, useNavigate } from "react-router-dom";
import "./HamburgerMenu.css";

function HamburgerMenu() {
  const [open, setOpen] = useState(false);
  const navigate = useNavigate();

  const handleLogout = () => {
    localStorage.removeItem("token");
    navigate("/login");
  };

  return (
    <>
      {/* HAMBURGER ICON */}
      <div
        className={`hamburger ${open ? "open" : ""}`}
        onClick={() => setOpen(!open)}
      >
        <span></span>
        <span></span>
        <span></span>
      </div>

      {/* SIDE MENU */}
      <div className={`side-menu ${open ? "show" : ""}`}>
        <ul>
          <li>
            <Link to="/account" onClick={() => setOpen(false)}>
              My Account
            </Link>
          </li>

          <li>
            <Link to="/orders" onClick={() => setOpen(false)}>
              Order History
            </Link>
          </li>

          <li>
            <Link to="/transactions" onClick={() => setOpen(false)}>
              Transactions
            </Link>
          </li>

          <li className="logout" onClick={handleLogout}>
            Logout
          </li>
        </ul>
      </div>
    </>
  );
}

export default HamburgerMenu;