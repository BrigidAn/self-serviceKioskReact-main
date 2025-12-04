import React, { useState, useEffect } from "react";
import { useNavigate } from "react-router-dom";
import "./CheckoutPage.css";

export default function CheckoutPage() {
  const navigate = useNavigate();

  const cart = JSON.parse(localStorage.getItem("cart") || "[]");
  const [balance, setBalance] = useState(
    Number(localStorage.getItem("balance") || 0)
  );
  const [showTopup, setShowTopup] = useState(false);
  const [topupAmount, setTopupAmount] = useState("");

  const total = cart.reduce((a, b) => a + Number(b.price), 0);

  const checkout = () => {
    if (balance < total) {
      alert("❌ Insufficient funds — please top up.");
      setShowTopup(true);
      return;
    }

    // Deduct balance
    const newBalance = balance - total;
    localStorage.setItem("balance", newBalance);
    setBalance(newBalance);

    // Save order to history
    const history = JSON.parse(localStorage.getItem("orders") || "[]");
    history.push({
      items: cart,
      total,
      date: new Date().toISOString(),
    });
    localStorage.setItem("orders", JSON.stringify(history));

    // Clear cart
    localStorage.removeItem("cart");

    alert("✅ Purchase successful!");
    navigate("/history");
  };

  const handleTopup = () => {
    const amount = Number(topupAmount);
    if (amount <= 0) return;

    const newBal = balance + amount;
    localStorage.setItem("balance", newBal);
    setBalance(newBal);

    // Save transaction log
    const logs = JSON.parse(localStorage.getItem("transactions") || "[]");
    logs.push({
      amount,
      type: "deposit",
      date: new Date().toISOString(),
    });
    localStorage.setItem("transactions", JSON.stringify(logs));

    setShowTopup(false);
    setTopupAmount("");
    alert("💰 Balance topped up!");
  };

  return (
    <div className="checkout-page">
      <h2>Checkout</h2>

           <button className="back-btn" onClick={() => navigate("/cart")}>
          ← Back
        </button>


      <p>Balance: R {balance.toFixed(2)}</p>
      <p>Total Price: R {total.toFixed(2)}</p>

      <h3>Items</h3>

      {cart.map((i) => (
        <div key={i.id} className="checkout-item">
          <span>{i.name}</span>
          <span>R {i.price}</span>
        </div>
      ))}

      <button onClick={checkout} className="confirm-btn">
        Confirm Checkout
      </button>

      {showTopup && (
        <div className="topup-modal">
          <div className="topup-box">
            <h3>Top Up Account</h3>
            <input
              type="number"
              placeholder="Amount"
              value={topupAmount}
              onChange={(e) => setTopupAmount(e.target.value)}
            />

            <button onClick={handleTopup}>Add Funds</button>
            <button onClick={() => setShowTopup(false)}>Cancel</button>
          </div>
        </div>
      )}
    </div>
  );
}
