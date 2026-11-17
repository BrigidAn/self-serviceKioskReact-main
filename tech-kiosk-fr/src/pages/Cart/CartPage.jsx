import React, { useCallback, useEffect, useRef, useState } from "react";
import { useNavigate } from "react-router-dom";
import { toast } from "react-toastify";
import "./CartPage.css";
import { API_URL, authHeaders } from "../../api/config";
import { Button, Modal } from "../../components/ui";
import { formatCountdown, useCartTimer } from "../../context/CartTimerContext";

const PLACEHOLDER = "https://via.placeholder.com/100";

export default function CartPage() {
  const navigate = useNavigate();
  const { isActive, remainingMs, expiredAt, refreshCart } = useCartTimer();

  const [cart, setCart] = useState([]);
  const [loading, setLoading] = useState(true);
  const [expiredMessage, setExpiredMessage] = useState("");
  const [showClearConfirm, setShowClearConfirm] = useState(false);
  const [clearing, setClearing] = useState(false);
  const mountedAt = useRef(Date.now());

  const total = cart.reduce((sum, i) => sum + i.unitPrice * i.quantity, 0);

  /* ---------------- FETCH CART ---------------- */
  const fetchCart = useCallback(async () => {
    try {
      const res = await fetch(`${API_URL}/checkout/summary`, { headers: authHeaders() });
      if (!res.ok) throw new Error("Failed to fetch cart");
      const data = await res.json();
      setCart(data.items || []);
    } catch (err) {
      console.error(err);
      setCart([]);
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    fetchCart();
  }, [fetchCart]);

  /* ---------------- EXPIRY (timer lives in CartTimerContext) ---------------- */
  useEffect(() => {
    if (!expiredAt || expiredAt < mountedAt.current) return undefined;

    setCart([]);
    setExpiredMessage("⏰ Your cart has expired. Redirecting to products...");
    const id = setTimeout(() => navigate("/products"), 3000);
    return () => clearTimeout(id);
  }, [expiredAt, navigate]);

  /* ---------------- CART CHANGES ---------------- */
  const afterChange = async () => {
    await fetchCart();
    await refreshCart(); // keeps the floating timer and item count in sync
  };

  const handleRemove = async (cartItemId) => {
    try {
      const res = await fetch(`${API_URL}/cart/item/${cartItemId}`, {
        method: "DELETE",
        headers: authHeaders(),
      });
      if (!res.ok) throw new Error("Failed to remove item");
      await afterChange();
    } catch (err) {
      console.error(err);
      toast.error(err.message);
    }
  };

  const handleClearCart = async () => {
    setClearing(true);
    try {
      await Promise.all(
        cart.map((item) =>
          fetch(`${API_URL}/cart/item/${item.cartItemId}`, {
            method: "DELETE",
            headers: authHeaders(),
          })
        )
      );
      await afterChange();
      toast.success("Cart cleared");
    } catch (err) {
      console.error("Failed to clear cart:", err);
      toast.error("Failed to clear cart");
    } finally {
      setClearing(false);
      setShowClearConfirm(false);
    }
  };

  const handleQuantityChange = async (cartItemId, newQuantity) => {
    if (newQuantity < 1) return;

    try {
      const res = await fetch(`${API_URL}/cart/item/${cartItemId}`, {
        method: "POST",
        headers: { "Content-Type": "application/json", ...authHeaders() },
        body: JSON.stringify({ quantity: newQuantity }),
      });
      if (!res.ok) throw new Error("Failed to update quantity");
      await afterChange();
    } catch (err) {
      console.error(err);
      toast.error(err.message);
    }
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
          {expiredMessage ? (
            <div className="cart-expired-message">{expiredMessage}</div>
          ) : (
            <h2>Your cart is empty 😔</h2>
          )}
          <Button variant="secondary" onClick={() => navigate("/products")}>
            ← Browse products
          </Button>
        </div>
      </div>
    );
  }

  /* ---------------- UI ---------------- */
  const warning = isActive && remainingMs <= 30 * 1000;

  return (
    <div className="cart-page">
      <div className="cart-container">
        <h2 className="cart-title">Your Cart</h2>

        {isActive && (
          <div className={`cart-expiry ${warning ? "cart-expiry--warning" : ""}`} role="timer">
            ⏳ Items are reserved for <span>{formatCountdown(remainingMs)}</span>
            <small>Adding an item restarts the timer.</small>
          </div>
        )}

        <div className="cart-actions">
          <Button variant="secondary" onClick={() => navigate("/products")}>
            ← Back
          </Button>
          <Button variant="danger" onClick={() => setShowClearConfirm(true)}>
            Clear all
          </Button>
        </div>

        <div className="cart-items">
          {cart.map((item) => (
            <div key={item.cartItemId} className="cart-item-card">
              <img
                src={item.imageUrl || PLACEHOLDER}
                alt={item.productName}
                className="cart-item-image"
              />

              <div className="cart-item-info">
                <div className="cart-item-name">{item.productName}</div>

                <div className="cart-item-qty-controls">
                  <Button
                    variant="secondary"
                    size="sm"
                    aria-label={`Decrease ${item.productName} quantity`}
                    disabled={item.quantity <= 1}
                    onClick={() => handleQuantityChange(item.cartItemId, item.quantity - 1)}
                  >
                    −
                  </Button>

                  <span className="qty-number">{item.quantity}</span>

                  <Button
                    variant="secondary"
                    size="sm"
                    aria-label={`Increase ${item.productName} quantity`}
                    onClick={() => handleQuantityChange(item.cartItemId, item.quantity + 1)}
                  >
                    +
                  </Button>
                </div>
              </div>

              <div className="cart-item-right">
                <div className="cart-item-price">
                  R {(item.unitPrice * item.quantity).toFixed(2)}
                </div>

                <Button variant="ghost" size="sm" onClick={() => handleRemove(item.cartItemId)}>
                  Remove
                </Button>
              </div>
            </div>
          ))}
        </div>

        <div className="cart-summary">
          <span>Total:</span>
          <span>R {total.toFixed(2)}</span>
        </div>

        <Button size="lg" block onClick={() => navigate("/checkout")}>
          Checkout
        </Button>
      </div>

      <Modal
        open={showClearConfirm}
        onClose={() => setShowClearConfirm(false)}
        title="Clear cart?"
        tone="danger"
        size="sm"
        footer={
          <>
            <Button variant="secondary" onClick={() => setShowClearConfirm(false)}>
              Cancel
            </Button>
            <Button variant="danger" loading={clearing} onClick={handleClearCart}>
              Clear cart
            </Button>
          </>
        }
      >
        <p>All items will be removed from your cart and returned to stock.</p>
      </Modal>
    </div>
  );
}
