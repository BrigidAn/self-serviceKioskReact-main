import React, { useState, useEffect } from "react";
import api from "../api";
import { useAuth } from "../context/AuthContext";
import "./Account.css";

function Account() {
const { balance, setBalance, refreshBalance } = useAuth();
  const [amount, setAmount] = useState("");
  const [transactions, setTransactions] = useState([]);
  const [loading, setLoading] = useState(false);
  const [message, setMessage] = useState("");

  useEffect(() => {
    (async () => {
      await refreshBalance();
      await fetchTransactions();
    })();
    // eslint-disable-next-line
  }, []);

  const fetchTransactions = async () => {
    try {
      const res = await api.get("/Account/transactions");
      setTransactions(res.data || []);
    } catch (err) {
      console.error("fetchTransactions", err);
      setMessage("Failed to fetch transactions.");
    }
  };

  const handleTopUp = async (e) => {
    e.preventDefault();
    const val = parseFloat(amount);
    if (!val || val <= 0) {
      setMessage("Enter a valid amount");
      return;
    }
    setLoading(true);
    try {
      const res = await api.post("/Account/topup");
      setBalance(res.data.balance ?? 0);
      setAmount("");
      setMessage("Balance topped up successfully!");
      await fetchTransactions();
    } catch (err) {
      console.error("topup", err);
      setMessage(err.response?.data?.message || "Top-up failed.");
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="account-container">
      <div className="account-card">
        <h2 className="account-title">Account</h2>
        <p className="account-balance">R {Number(balance).toFixed(2)}</p>

        <form onSubmit={handleTopUp}>
          <input
            type="number"
            placeholder="Add funds"
            value={amount}
            onChange={(e) => setAmount(e.target.value)}
            className="account-input"
          />
          <button type="submit" className="account-btn" disabled={loading}>
            {loading ? "Processing..." : "Add Balance"}
          </button>
        </form>

        {message && <p className="account-message">{message}</p>}



        <h3 className="transactions-title">Transaction History</h3>  {/* shows the transaction history from db */}
        <div className="transaction-list">
          {transactions.length === 0 && <p>No transactions yet.</p>}
          {transactions.map((tx) => (
            <div key={tx.transactionId} className={`transaction-item ${tx.type}`}>
              <span>{tx.description}</span>
              <span>{tx.totalAmount.toFixed(2)}</span>
              <span>{new Date(tx.createdAt).toLocaleString()}</span>
            </div>
          ))}
        </div>
      </div>
    </div>
  );
}

export default Account;
