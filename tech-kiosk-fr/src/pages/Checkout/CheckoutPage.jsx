import React, { useState, useEffect } from "react";
import { useNavigate } from "react-router-dom";
import "./CheckoutPage.css";

const API_URL = "https://localhost:5016/api";
const token = localStorage.getItem("token");

export default function CheckoutPage() {
  const navigate = useNavigate();

  const [cart, setCart] = useState(
    JSON.parse(localStorage.getItem("cart") || "[]")
  );
  const [balance, setBalance] = useState(Number(localStorage.getItem("balance") || 0));
  const [showTopup, setShowTopup] = useState(false);
  const [topupAmount, setTopupAmount] = useState("");
  const [showThankYou, setShowThankYou] = useState(false);
  const [orderSummary, setOrderSummary] = useState({ items: [], total: 0, deliveryFee: 0 });

  // Calculate total items price based on quantity
  const totalItemsPrice = cart.reduce(
    (sum, item) => sum + Number(item.price) * (item.quantity || 1),
    0
  );

  const checkout = async (deliveryMethod = "pickup") => {
    try {
      const res = await fetch(`${API_URL}/checkout`, {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
          Authorization: `Bearer ${token}`,
        },
        body: JSON.stringify({ DeliveryMethod: deliveryMethod }),
      });

      const data = await res.json();

      if (!res.ok) {
        if (data.remainingAmount) {
          // Insufficient funds
          setShowTopup(true);
          return;
        }
        throw new Error(data.message || "Checkout failed");
      }

      // Backend grand total (includes delivery fee)
      const grandTotal = data.totalAmount ?? totalItemsPrice + (data.deliveryFee ?? 0);

      // Deduct balance and store
      const newBalance = balance - grandTotal;
      setBalance(newBalance);
      localStorage.setItem("balance", newBalance);

      // Clear local cart
      localStorage.removeItem("cart");
      setCart([]);

      // Show thank-you popup with correct quantities and totals
      setOrderSummary({
        items: cart.map((i) => ({ ...i })), // preserve name, price, quantity
        total: grandTotal,
        deliveryFee: data.deliveryFee ?? 0,
      });
      setShowThankYou(true);
    } catch (err) {
      console.error(err);
      alert(err.message || "Checkout failed");
    }
  };

  const handleTopup = () => {
    const amount = parseFloat(topupAmount);
    if (isNaN(amount) || amount <= 0) return;

    const newBal = balance + amount;
    setBalance(newBal);
    localStorage.setItem("balance", newBal);
    setTopupAmount("");
    setShowTopup(false);

    alert(`💰 Top-up successful. Current balance: R ${newBal.toFixed(2)}`);
  };

  return (
    <div className="checkout-page">
      <h2>Checkout</h2>
      <button className="checkout-back-btn" onClick={() => navigate("/cart")}>
        ← Back
      </button>

      <p>Balance: R {balance.toFixed(2)}</p>
      <p>Total Price: R {totalItemsPrice.toFixed(2)}</p>

      <h3>Items</h3>
      {cart.map((i) => (
        <div key={i.id} className="checkout-item">
          <span>
            {i.name} x {i.quantity || 1}
          </span>
          <span>R {(Number(i.price) * (i.quantity || 1)).toFixed(2)}</span>
        </div>
      ))}

      <button onClick={() => checkout("pickup")} className="confirm-btn">
        Confirm Checkout
      </button>

      {/* TOP-UP POPUP */}
      {showTopup && (
        <div className="popup-overlay">
          <div className="popup-box">
            <h2>❌ Insufficient Balance</h2>
            <p>Your balance is R {balance.toFixed(2)}. Please top up to complete the purchase.</p>
            <input
              type="number"
              placeholder="Enter top-up amount"
              value={topupAmount}
              onChange={(e) => setTopupAmount(e.target.value)}
            />
            <div className="popup-actions">
              <button className="popup-confirm" onClick={handleTopup}>
                Top Up
              </button>
              <button className="popup-cancel" onClick={() => setShowTopup(false)}>
                Cancel
              </button>
            </div>
          </div>
        </div>
      )}

      {/* THANK YOU POPUP */}
      {showThankYou && (
        <div className="popup-overlay">
          <div className="popup-box">
            <h2>🎉 Thank You!</h2>
            <p>Your purchase was successful.</p>
            <h4>Items Purchased:</h4>
            <ul style={{ textAlign: "left", marginBottom: "10px" }}>
              {orderSummary.items.map((i, idx) => (
                <li key={idx}>
                  {i.name} x {i.quantity || 1} — R {(Number(i.price) * (i.quantity || 1)).toFixed(2)}
                </li>
              ))}
            </ul>
            <p>Total Spent: R {orderSummary.total.toFixed(2)}</p>
            <p>Delivery Fee: R {orderSummary.deliveryFee.toFixed(2)}</p>
            <p>Current Balance: R {balance.toFixed(2)}</p>
            <div className="popup-actions">
              <button className="popup-confirm" onClick={() => setShowThankYou(false)}>
                Close
              </button>
              <button className="popup-cancel" onClick={() => {
                setShowThankYou(false);
                setShowTopup(true);
              }}>
                Top Up
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}
