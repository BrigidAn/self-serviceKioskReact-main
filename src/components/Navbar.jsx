import { Link } from "react-router-dom";
import React from "react";
import { FaShoppingCart } from "react-icons/fa";

function Navbar({ cartCount }) {
  return (
    <nav
      className="navbar p-3 mb-4"
      style={{
        background: "linear-gradient(90deg, rgba(55,153,191,1) 0%, rgba(87,199,133,1) 84%)",
        color: "white",
      }}
    >
      <Link
        className="navbar-brand text-white fw-bold"
        to="/"
        style={{ textDecoration: "none" }}
      >
        Kiosk 
      </Link>

      <div>
        <Link className="btn btn-outline-light mx-2" to="/cart">
          <FaShoppingCart />{" "}
          <span className="badge bg-light text-dark">{cartCount}</span>
        </Link>
        <Link className="btn btn-outline-light mx-2" to="/cart">
          <FaShoppingCart/>{" "}
        </Link>
      </div>
    </nav>
  );
}

export default Navbar;
