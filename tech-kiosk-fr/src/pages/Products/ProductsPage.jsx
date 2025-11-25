import React, { useEffect, useState } from "react";
import "./ProductsPage.css";
import { useNavigate } from "react-router-dom";

function ProductsPage() {
  const navigate = useNavigate();

  const PRODUCTS_API = "https://localhost:5016/api/products";
  const USER_API = "https://localhost:5016/api/auth/me";

  const [products, setProducts] = useState([]);
  const [user, setUser] = useState(null);
  const [menuOpen, setMenuOpen] = useState(false);
  const [cart, setCart] = useState([]);
  const [cartOpen, setCartOpen] = useState(false);

  // Fetch products
  useEffect(() => {
    fetch(PRODUCTS_API)
      .then((res) => res.json())
      .then((data) => setProducts(data))
      .catch(() => console.log("Failed to load products"));
  }, []);

  // Fetch user info
  useEffect(() => {
    const token = localStorage.getItem("token");
    if (!token) return;

    fetch(USER_API, {
      headers: { Authorization: `Bearer ${token}` },
    })
      .then((res) => res.json())
      .then((data) => setUser(data))
      .catch(() => console.log("Failed to load user details"));
  }, []);

  // Logout
  const handleLogout = () => {
    localStorage.removeItem("token");
    navigate("/");
  };

  // Add to cart
  const handleAddToCart = (product) => {
    setCart([...cart, product]);
  };

  // Remove from cart
  const handleRemoveFromCart = (index) => {
    const newCart = [...cart];
    newCart.splice(index, 1);
    setCart(newCart);
  };

  const cartTotal = cart.reduce((total, item) => total + item.price, 0);

  return (
    <div className="product-page">

      {/* NAVBAR */}
      <nav className="navbar">
        {/* Hamburger left */}
        <div
          className={`hamburger ${menuOpen ? "open" : ""}`}
          onClick={() => setMenuOpen(!menuOpen)}
        >
          <span></span>
          <span></span>
          <span></span>
        </div>

        {/* Center links */}
        <ul className="nav-links">
          <li onClick={() => navigate("/about")}>About</li>
          <li className="active" onClick={() => navigate("/products")}>Products</li>
          <li onClick={() => navigate("/contact")}>Contact</li>
        </ul>

        {/* Cart icon right */}
        <div className="cart-icon" onClick={() => setCartOpen(!cartOpen)}>
          🛒
          {cart.length > 0 && <span className="cart-count">{cart.length}</span>}
        </div>
      </nav>

      {/* Slide-in Hamburger Menu */}
      <div className={`side-menu ${menuOpen ? "open" : ""}`}>
        <ul>
          <li onClick={() => { navigate("/account"); setMenuOpen(false); }}>Account</li>
          <li onClick={() => { navigate("/categories"); setMenuOpen(false); }}>Categories</li>
          <li onClick={() => { navigate("/transactions"); setMenuOpen(false); }}>Transactions</li>
          <li>Balance: R {user?.balance || 0}</li>
          <li onClick={handleLogout}>Logout</li>
        </ul>
      </div>

      {/* Cart Dropdown */}
      {cartOpen && (
        <div className="cart-dropdown">
          {cart.length === 0 ? (
            <p className="empty-cart">Your cart is empty</p>
          ) : (
            <>
              <ul>
                {cart.map((item, index) => (
                  <li key={index} className="cart-item">
                    <img src={item.imageUrl} alt={item.name} className="cart-item-img" />
                    <div className="cart-item-info">
                      <p>{item.name}</p>
                      <span>R {item.price}</span>
                    </div>
                    <button className="remove-btn" onClick={() => handleRemoveFromCart(index)}>✕</button>
                  </li>
                ))}
              </ul>
              <div className="cart-total">
                Total: R {cartTotal.toFixed(2)}
              </div>
            </>
          )}
        </div>
      )}

      {/* PRODUCT GRID */}
      <div className="product-grid">
        {products.length > 0 ? (
          products.map((item) => (
            <div className="product-card" key={item.id}>
              <img src={item.imageUrl} alt={item.name} className="product-img" />
              <h3 className="product-name">{item.name}</h3>
              <p className="product-description">{item.description}</p>
              <div className="product-footer">
                <span className="product-price">R {item.price}</span>
                <button className="add-btn" onClick={() => handleAddToCart(item)}>Add</button>
              </div>
            </div>
          ))
        ) : (
          <p className="loading-msg">Loading products...</p>
        )}
      </div>
    </div>
  );
}

export default ProductsPage;
