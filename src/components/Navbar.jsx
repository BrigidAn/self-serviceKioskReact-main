import { Link } from "react-router-dom";
import React from "react";
import { FaShoppingCart } from "react-icons/fa"; // Install react-icons: npm install react-icons

function Navbar({ cartCount }) {
  return (
    <nav className="navbar navbar-dark bg-dark p-3 mb-4">
      <Link className="navbar-brand" to="/">Kiosk Shop</Link>
      <div>
        <Link className="btn btn-outline-light mx-2" to="/cart">
          <FaShoppingCart /> Cart{" "}
          <span className="badge bg-light text-dark">{cartCount}</span>
        </Link>
        <Link className="btn btn-outline-light" to="/checkout">Checkout</Link>
      </div>
    </nav>
  );
}

export default Navbar;
