import React, { useEffect, useState } from "react";
import "./ProductsPage.css";
import { useNavigate } from "react-router-dom";

function ProductsPage() {
  const navigate = useNavigate();

  const PRODUCTS_API = "https://localhost:5016/api/product";
  const USER_API = "https://localhost:5016/api/account/me";
  const CART_ADD_API = "https://localhost:5016/api/Cart/add";

  const [products, setProducts] = useState([]);
  const [user, setUser] = useState(null);
  const [menuOpen, setMenuOpen] = useState(false);
  const [cartCount, setCartCount] = useState(0);
  const [toastMessage, setToastMessage] = useState("");
  const [showToast, setShowToast] = useState(false);

  const token = localStorage.getItem("token");

  const triggerToast = (msg) => {
    setToastMessage(msg);
    setShowToast(true);
    setTimeout(() => setShowToast(false), 3000);
  };

  // Fetch user balance
  useEffect(() => {
    if (!token) return;

    fetch(USER_API, {
      headers: { Authorization: `Bearer ${token}` },
    })
      .then((res) => res.json())
      .then((data) => {
        const finalUser = data.user || data;
        setUser(finalUser);
        if (finalUser.cartCount !== undefined) setCartCount(finalUser.cartCount);
      })
      .catch(() => console.log("Failed to load user details"));
  }, [token]);

  // Fetch products
  useEffect(() => {
    fetch(PRODUCTS_API)
      .then((res) => res.json())
      .then((data) => setProducts(data))
      .catch(() => console.log("Failed to load products"));
  }, []);

  // Add product to backend cart
  const handleAddToCart = async (product) => {
    const token = localStorage.getItem("token");
    const userId = localStorage.getItem("userId");

  if (!token || !userId) {
    triggerToast("You must be logged in to add items to cart");
    return;
  }

  try {
    const res = await fetch(CART_ADD_API, {
      method: "POST",
      headers: {
        "Content-Type": "application/json",
        Authorization: `Bearer ${token}`,
      },
      body: JSON.stringify({
        userId: parseInt(userId),
        productId: product.productId ?? product.id ?? product.ProductId,
        quantity: 1, // always add 1 item at a time
      }),
    });

    if (!res.ok) {
      triggerToast(`${product.name} added to cart`);
      setCartCount((prev) => prev + 1);
    } else {
      const data = await res.json().catch(() => null);
      console.log("Backend add-to-cart failed:", data.message);
      triggerToast("Failed to add item to cart");
    }
  } catch (err) {
    console.log("Error contacting backend:", err);
    triggerToast("Error adding item to cart");
  }
};


  const handleLogout = () => {
    localStorage.removeItem("token");
    navigate("/");
  };

  return (
    <div className="product-page">
      {/* NAVBAR */}
      <nav className="navbar">
        <div className="hamburger" onClick={() => setMenuOpen(true)}>
          <span></span>
          <span></span>
          <span></span>
        </div>

        <ul className="nav-links">
          <li onClick={() => navigate("/about")}>About</li>
          <li className="active" onClick={() => navigate("/products")}>Products</li>
          <li onClick={() => navigate("/contact")}>Contact</li>
        </ul>

        <div className="cart-icon" onClick={() => navigate("/cart")}>
          🛒 <span className="cart-count">{cartCount}</span>
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
        {showToast && <div className="toast">{toastMessage}</div>}

        {products.length > 0 ? (
          products.map((item) => (
            <div className="product-card" key={item.id}>
              <div className="card-inner">
                <div className="card-front">
                  <img src={item.imageUrl} alt={item.name} className="product-img" />
                  <h3>{item.name}</h3>
                  <div className="product-footer">
                    <span className="product-price">R {item.price}</span>
                    <span className="qty">Qty: {item.quantity}</span>
                  </div>
                </div>

                <div className="card-back">
                  <p className="product-description">{item.description}</p>
                  <button className="add-btn" onClick={() => handleAddToCart(item)}>
                    Add
                  </button>
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
