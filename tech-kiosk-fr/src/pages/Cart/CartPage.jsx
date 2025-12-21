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

  const fetchCart = async () => {
    try {
      const res = await fetch(`${API_URL}/checkout/summary`, {
        headers: { Authorization: `Bearer ${token}` },
      });
      if (!res.ok) throw new Error("Failed to fetch cart");
      const data = await res.json();
      setCart(data.items || []);
      setTotal(data.items?.reduce((sum, i) => sum + i.unitPrice * i.quantity, 0) || 0);
    } catch (err) {
      console.error(err);
      setCart([]);
      setTotal(0);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchCart();
  }, []);

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

 if (loading) {
    return (
      <div className="cart-page empty-cart">
        <div className="cart-message">
          <h2>Loading your cart...</h2>
        </div>
        <div className="vp-hero-floating">
          <div className="float-shape fs1"></div>
          <div className="float-shape fs2"></div>
          <div className="float-shape fs3"></div>
        </div>
      </div>
    );
  }

  if (!cart || cart.length === 0) {
    return (
      <div className="cart-page empty-cart">
        <div className="cart-message">
          <h2>Your cart is empty 😔</h2>
          <button className="cart-back-btn" onClick={() => navigate("/products")}>
            ← Browse Products
          </button>
        </div>
        <div className="vp-hero-floating">
          <div className="float-shape fs1"></div>
          <div className="float-shape fs2"></div>
          <div className="float-shape fs3"></div>
        </div>
      </div>
    );
  }

  return (
    <div className="cart-page">

      <button className="cart-back-btn" onClick={() => navigate("/products")}>
        ← Back
      </button>

      <h2 className="cart-title">Your Cart</h2>

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
                  onClick={() =>
                    handleQuantityChange(item.cartItemId, item.quantity - 1)
                  }
                  className="qty-btn"
                >
                  -
                </button>
                <span className="qty-number">{item.quantity}</span>
                <button
                  onClick={() =>
                    handleQuantityChange(item.cartItemId, item.quantity + 1)
                  }
                  className="qty-btn"
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

            <div className="vp-hero-floating">
                <div className="float-shape fs1"></div>
                <div className="float-shape fs2"></div>
                <div className="float-shape fs3"></div>
              </div>
    </div>
  );
}