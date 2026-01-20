import React, { useEffect, useState } from "react";
import { useNavigate } from "react-router-dom";
import "./CartPage.css";

const API_URL = "https://localhost:5016/api";
const token = localStorage.getItem("token");

export default function CartPage() {
  const navigate = useNavigate();

  const [cart, setCart] = useState([]);
  const [loading, setLoading] = useState(true);
  const [total, setTotal] = useState(0);
  const [expiredMessage, setExpiredMessage] = useState("");

  // ✅ NEW: expiry + countdown
  const [expiresAt, setExpiresAt] = useState(null);
  const [timeLeft, setTimeLeft] = useState("");
  const [expired, setExpired] = useState(false);


  /* ---------------- FETCH CART ---------------- */
  const fetchCart = async () => {
    try {
      const res = await fetch(`${API_URL}/checkout/summary`, {
        headers: { Authorization: `Bearer ${token}` },
      });

      if (!res.ok) throw new Error("Failed to fetch cart");

      const data = await res.json();

      setCart(data.items || []);
      const expiry =
      data.expiresAt ??
      new Date(Date.now() + 1 * 60 * 1000).toISOString(); // 15 minutes fallback

      setExpiresAt(expiry);


      setTotal(
        data.items?.reduce(
          (sum, i) => sum + i.unitPrice * i.quantity,
          0
        ) || 0
      );
    } catch (err) {
      console.error(err);
      setCart([]);
      setTotal(0);
    } finally {
      setLoading(false);
    }
  };

  /* ---------------- INITIAL LOAD ---------------- */
  useEffect(() => {
    fetchCart();
  }, []);

  /* ---------------- COUNTDOWN TIMER ---------------- */
  useEffect(() => {
    if (!expiresAt) return;

    const interval = setInterval(async () => {
      const now = new Date();
      const expiry = Date.parse(expiresAt);
      if (isNaN(expiry)) return;

      const diff = expiry - Date.now();

                if (diff <= 0 && !expired) {
            setExpired(true);

            await fetch(`${API_URL}/cart/expire`, {
              method: "POST",
              headers: { Authorization: `Bearer ${token}` },
            });

            setCart([]);
            setTotal(0);
            setTimeLeft("");
            navigate("/products");
          }


      if (diff <= 0) {
        clearInterval(interval);

        await fetch(`${API_URL}/cart/expire`, {
          method: "POST",
          headers: { Authorization: `Bearer ${token}` },
        });

        setCart([]);
        setTotal(0);
        setTimeLeft("");

      setExpiredMessage("⏰ Your cart has expired. Redirecting to products...");
        setCart([]);
        setTotal(0);
        setTimeLeft("");

        setTimeout(() => {
          navigate("/products");
        }, 3000);

        return;

      }

      const minutes = Math.floor(diff / 60000);
      const seconds = Math.floor((diff % 60000) / 1000);

      setTimeLeft(
        `${String(minutes).padStart(2, "0")}:${String(seconds).padStart(2, "0")}`
      );
    }, 1000);

    return () => clearInterval(interval);
  }, [expiresAt, navigate]);

  /* ---------------- REMOVE ITEM ---------------- */
  const handleRemove = async (cartItemId) => {
    try {
      const res = await fetch(`${API_URL}/cart/item/${cartItemId}`, {
        method: "DELETE",
        headers: { Authorization: `Bearer ${token}` },
      });

      if (!res.ok) throw new Error("Failed to remove item");
      await fetchCart();
    } catch (err) {
      console.error(err);
      alert(err.message);
    }
  };

  /* ---------------- UPDATE QUANTITY ---------------- */
  const handleQuantityChange = async (cartItemId, newQuantity) => {
    if (newQuantity < 1) return;

    try {
      const res = await fetch(`${API_URL}/cart/item/${cartItemId}`, {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
          Authorization: `Bearer ${token}`,
        },
        body: JSON.stringify({ quantity: newQuantity }),
      });

      if (!res.ok) throw new Error("Failed to update quantity");
      await fetchCart();
    } catch (err) {
      console.error(err);
      alert(err.message);
    }
  };

  const handleCheckout = () => {
    navigate("/checkout");
  };

  /* ---------------- LOADING ---------------- */
  if (loading) {
    return (
      <div className="cart-page empty-cart">
        <div className="cart-container">
          <div className="cart-message">
            <h2>Loading your cart...</h2>
          </div>
        </div>
      </div>
    );
  }

  /* ---------------- EMPTY CART ---------------- */
  if (!cart || cart.length === 0) {
    return (
      <div className="cart-page empty-cart">
        <div className="cart-message">
          <h2>Your cart is empty 😔</h2>
          <button className="cart-back-btn" onClick={() => navigate("/products")}>
            ← Browse Products
          </button>
        </div>
      </div>
    );
  }

  /* ---------------- UI ---------------- */
  return (
    <div className="cart-page">
      <div className="cart-container">
        <button className="cart-back-btn" onClick={() => navigate("/products")}>
          ← Back
        </button>

        <h2 className="cart-title">Your Cart</h2>

        {expiredMessage && (
          <div className="cart-expired-message">
            {expiredMessage}
          </div>
        )}


        {/* ✅ COUNTDOWN DISPLAY */}
        {timeLeft && (
          <div className="cart-expiry">
            ⏳ Cart expires in <span>{timeLeft}</span>
          </div>
        )}

        <div className="cart-items">
          {cart.map((item) => (
            <div key={item.cartItemId} className="cart-item-card">
              <img
                src={item.imageUrl || "https://via.placeholder.com/100"}
                alt={item.productName}
                className="cart-item-image"
              />

              <div className="cart-item-info">
                <div className="cart-item-name">{item.productName}</div>

                <div className="cart-item-qty-controls">
                  <button
                    className="qty-btn"
                    onClick={() =>
                      handleQuantityChange(item.cartItemId, item.quantity - 1)
                    }
                  >
                    -
                  </button>

                  <span className="qty-number">{item.quantity}</span>

                  <button
                    className="qty-btn"
                    onClick={() =>
                      handleQuantityChange(item.cartItemId, item.quantity + 1)
                    }
                  >
                    +
                  </button>
                </div>
              </div>

              <div className="cart-item-right">
                <div className="cart-item-price">
                  R {(item.unitPrice * item.quantity).toFixed(2)}
                </div>

                <button
                  className="cart-item-remove-btn"
                  onClick={() => handleRemove(item.cartItemId)}
                >
                  Remove
                </button>
              </div>
            </div>
          ))}
        </div>

        <div className="cart-summary">
          <span>Total:</span>
          <span>R {total.toFixed(2)}</span>
        </div>

        <button className="cart-checkout-btn" onClick={handleCheckout}>
          Checkout
        </button>
      </div>
    </div>
  );
}
