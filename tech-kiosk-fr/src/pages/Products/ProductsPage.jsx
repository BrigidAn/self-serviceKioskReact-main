import React, { useEffect, useState } from "react";
import "./ProductsPage.css";
import { useNavigate } from "react-router-dom";

function ProductsPage() {
  const navigate = useNavigate();

  const PRODUCTS_API = "https://localhost:5016/api/product";
  const USER_API = "https://localhost:5016/api/auth/me";

  const [products, setProducts] = useState([]);
  const [user, setUser] = useState(null);
  const [menuOpen, setMenuOpen] = useState(false);
  const [cart, setCart] = useState([]);
  const [toastMessage, setToastMessage] = useState("");
  const [showToast, setShowToast] = useState(false);

  const triggerToast = (msg) => {
    setToastMessage(msg);
    setShowToast(true);
    setTimeout(() => setShowToast(false), 3000);
  }

  // --- Load reserved items & auto-expire ---
  useEffect(() => {
    const savedCart = JSON.parse(localStorage.getItem("cart")) || [];
    const now = Date.now();

    const validCart = savedCart.filter((item) => now - item.timestamp < 18 * 60 * 60 * 1000);

    if (validCart.length !== savedCart.length) {
      localStorage.setItem("cart", JSON.stringify(validCart));
    }

    setCart(validCart);
  }, []);

  // Fetch products
  useEffect(() => {
    fetch(PRODUCTS_API)
      .then((res) => res.json())
      .then((data) => setProducts(data))
      .catch(() => console.log("Failed to load products"));
  }, []);

  // Fetch user balance
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

  // Add to cart & reduce quantity
  const handleAdd = (product) => {
    if (product.quantity === 0) return;

    const updatedProducts = products.map((p) =>
      p.id === product.id ? { ...p, quantity: p.quantity - 1 } : p
    );

    setProducts(updatedProducts);

    const newCartItem = {
      ...product,
      timestamp: Date.now(),
    };

    const updatedCart = [...cart, newCartItem];

    setCart(updatedCart);
    localStorage.setItem("cart", JSON.stringify(updatedCart));

    triggerToast(`${product.name} added to cart`);
  };

  const handleLogout = () => {
    localStorage.removeItem("token");
    navigate("/");
  };

  return (
    <div className="product-page">

      {/* NAVBAR */}
      <nav className="navbar">
        {/* HAMBURGER ICON */}
        <div className="hamburger" onClick={() => setMenuOpen(true)}>
          <span></span>
          <span></span>
          <span></span>
        </div>

        {/* CENTER NAV LINKS */}
        <ul className="nav-links">
          <li onClick={() => navigate("/about")}>About</li>
          <li className="active" onClick={() => navigate("/products")}>Products</li>
          <li onClick={() => navigate("/contact")}>Contact</li>
        </ul>

        {/* CART ICON */}
        <div className="cart-icon" onClick={() => navigate("/cart")}>
          🛒 <span className="cart-count">{cart.length}</span>
        </div>
      </nav>

      {/* SIDE MENU */}
      <div className={`side-menu ${menuOpen ? "open" : ""}`}>
        <div className="close-btn" onClick={() => setMenuOpen(false)}>✕</div>

        <ul>
          <li onClick={() => { navigate("/account"); setMenuOpen(false); }}>Account</li>
          <li onClick={() => { navigate("/categories"); setMenuOpen(false); }}>Categories</li>
          <li onClick={() => { navigate("/transactions"); setMenuOpen(false); }}>Transactions</li>
          <li className="side-balance">Balance: R {user?.balance || 0}</li>
          <li onClick={handleLogout}>Logout</li>
        </ul>
      </div>

      {/* PRODUCT GRID */}
      <div className="product-grid">
        {products.length > 0 ? (
          products.map((item) => (
            <div className="product-card" key={item.id}>

              {/* FLIP CARD INNER */}
              <div className="card-inner">

                {/* FRONT SIDE */}
                <div className="card-front">
                  <img src={item.imageUrl} alt={item.name} className="product-img" />
                  <h3>{item.name}</h3>

                  <div className="product-footer">
                    <span className="product-price">R {item.price}</span>
                    <span className="qty">Qty: {item.quantity}</span>
                  </div>
                </div>

                {/* BACK SIDE */}
                <div className="card-back">
                  <p className="product-description">{item.description}</p>
                  <button className="add-btn" onClick={() => handleAdd(item)}>
                    Add
                  </button>
                  {showToast && (
                  <div className="toast">
                    {toastMessage}
                  </div>
                )}

                </div>
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
