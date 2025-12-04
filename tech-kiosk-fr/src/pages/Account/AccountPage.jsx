import React, { useState, useEffect } from "react";
import { useNavigate } from "react-router-dom";
import NavBar from "../../components/Navbar";
import "./AccountPage.css";
import {toast } from "react-toastify";
import "react-toastify/dist/ReactToastify.css";


export default function AccountPage() {
  const navigate = useNavigate();
  const [balance, setBalance] = useState(1500); // Example starting balance
  const [topupAmount, setTopupAmount] = useState("");
  const [transactions, setTransactions] = useState([
    { id: 1, type: "Deposit", amount: 500, date: "2025-11-30" },
    { id: 2, type: "Purchase", amount: 200, date: "2025-12-01" },
  ]);

  // Handle top-up
  const handleTopup = () => {
    const amt = Number(topupAmount);
    if (amt <= 0) {
      toast.error("Enter a valid amount");
      return;
    }
    setBalance(balance + amt);
    setTransactions([
      { id: Date.now(), type: "Deposit", amount: amt, date: new Date().toISOString().slice(0, 10) },
      ...transactions,
    ]);
    setTopupAmount("");
    toast.success(`R ${amt.toFixed(2)} added to your account!`);
  };

  return (
    <div className="velvety-account-page2">
      <NavBar cartCount={0} />

        <button className="back-btn" onClick={() => navigate("/products")}>
          ← Back 
        </button>


      <div className="velvety-account-page">
        <h2>Account Overview</h2>

        {/* Current Balance */}
        <div className="account-balance-card">
          <h3>Current Balance</h3>
          <p>R {balance.toFixed(2)}</p>
        </div>

        {/* Top-up Section */}
        <div className="topup-card">
          <h3>Top-up Account</h3>
          <div className="topup-input-group">
            <input
              type="number"
              placeholder="Enter amount"
              value={topupAmount}
              onChange={(e) => setTopupAmount(e.target.value)}
            />
            <button onClick={handleTopup}>Top-up</button>
          </div>
        </div>

        {/* Transactions History */}
        <div className="transactions-card">
          <h3>Transaction History</h3>
          {transactions.length === 0 ? (
            <p>No transactions yet.</p>
          ) : (
            <table>
              <thead>
                <tr>
                  <th>Date</th>
                  <th>Type</th>
                  <th>Amount (R)</th>
                </tr>
              </thead>
              <tbody>
                {transactions.map((t) => (
                  <tr key={t.id}>
                    <td>{t.date}</td>
                    <td>{t.type}</td>
                    <td className={t.type === "Deposit" ? "deposit" : "purchase"}>
                      {t.amount.toFixed(2)}
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          )}
        </div>
      </div>
    </div>
  );
}
