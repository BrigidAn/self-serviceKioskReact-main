import React, { useState } from "react";
import "./AccountPage.css";

function AccountsPage() {
  const [balance, setBalance] = useState(250); // example balance
  const [topUpAmount, setTopUpAmount] = useState("");

  const handleTopUp = () => {
    const value = parseFloat(topUpAmount);
    if (!isNaN(value) && value > 0) {
      setBalance(balance + value);
      setTopUpAmount("");
    }
  };

  return (
    <div className="acc-container">
      <button className="back-btn" onClick={() => window.history.back()}>
          ← Back
        </button>
      {/* Header */}
      <h1 className="acc-title">My Account</h1>
      <p className="acc-sub">Manage your balance and account actions</p>

      {/* Balance Card */}
      <div className="acc-card balance-card">
        <h2>Available Balance</h2>
        <div className="acc-balance">R {balance.toFixed(2)}</div>
      </div>

      {/* Top-Up Card */}
      <div className="acc-card topup-card">
        <h2>Add Funds</h2>

        <div className="topup-input-wrap">
          <input
            type="number"
            placeholder="Enter amount"
            className="topup-input"
            value={topUpAmount}
            onChange={(e) => setTopUpAmount(e.target.value)}
          />
          <button className="topup-btn" onClick={handleTopUp}>
            Top Up
          </button>
        </div>
      </div>

      {/* Optional History Section */}
      <div className="acc-card history-card">
        <h2>Transaction History</h2>

        <p className="acc-coming-soon">Coming Soon...</p>
      </div>
    </div>
  );
}

export default AccountsPage;