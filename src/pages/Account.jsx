import React, { useState, useEffect } from "react";
import axios from "axios";
import "./Account.css";

function Account() {
  const [balance, setBalance] = useState(0);
  const [amount, setAmount] = useState("");
  const [transactions, setTransactions] = useState([]);
  const [loading, setLoading] = useState(false);
  const [message, setMessage] = useState("");

  // Fetch balance and transactions on mount
  useEffect(() => {
    fetchBalance();
    fetchTransactions();
  }, []);

  const fetchBalance = async () => {
    try {
      const res = await axios.get("http://localhost:5016/api/User/account/{userId}", {
        headers: { Authorization: `Bearer ${localStorage.getItem("token")}` },
      });
      setBalance(res.data.balance);
    } catch (err) {
      console.error(err);
      setMessage("Failed to fetch balance.");
    }
  };

  const fetchTransactions = async () => {
    try {
      const userId = localStorage.getItem("userId"); // assuming you save it on login
      const res = await axios.get(`http://localhost:5016/api/Transaction/user/${userId}`, {
        headers: { Authorization: `Bearer ${localStorage.getItem("token")}` },
      });
      setTransactions(res.data);
    } catch (err) {
      console.error(err);
      setMessage("Failed to fetch transactions.");
    }
  };

  const handleTopUp = async (e) => {
    e.preventDefault();
    if (!amount || isNaN(amount) || parseFloat(amount) <= 0) {
      setMessage("Enter a valid amount");
      return;
    }

    setLoading(true);
    try {
      const res = await axios.post(
        "http://localhost:5016/api/User/account/topup",
        parseFloat(amount),
        { headers: { Authorization: `Bearer ${localStorage.getItem("token")}` } }
      );
      setBalance(res.data.balance);
      setAmount("");
      setMessage("Balance topped up successfully!");
      fetchTransactions(); // refresh transaction list
    } catch (err) {
      console.error(err);
      setMessage("Top-up failed.");
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="account-container">
      <div className="account-card">
        <h2 className="account-title">Account</h2>
        <p className="account-balance">R {balance.toFixed(2)}</p>

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
