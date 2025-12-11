import React, { useEffect, useState } from "react";
import "./AccountPage.css";
import { toast } from "react-toastify";
import "react-toastify/dist/ReactToastify.css";

const ACCOUNT_API = "https://localhost:5016/api/account";
const TX_API = "https://localhost:5016/api/Transaction";
const token = localStorage.getItem("token");

function AccountsPage() {
  const [balance, setBalance] = useState(0);
  const [topUpAmount, setTopUpAmount] = useState("");
  const [transactions, setTransactions] = useState([]);
  const [loadingTx, setLoadingTx] = useState(true);
  const [popupMessage, setPopupMessage] = useState("");

  const userId = localStorage.getItem("userId"); // stored after login

  // Fetch balance
  const fetchBalance = async () => {
    try {
      const res = await fetch(`${ACCOUNT_API}/balance`, {
        headers: { Authorization: `Bearer ${token}` },
      });
      if (!res.ok) throw new Error("Failed to fetch balance");

      const data = await res.json();
      setBalance(data?.balance ?? 0);
    } catch (err) {
      toast.error(err.message);
      setBalance(0);
    }
  };

  // Fetch transaction history
  const fetchTransactions = async () => {
    try {
      const res = await fetch(`${TX_API}/mytrasactions`, {
        headers: { Authorization: `Bearer ${token}` },
      });

      if (!res.ok) throw new Error("Could not load transactions");

      const data = await res.json();
      setTransactions(data);
    } catch (err) {
      toast.error(err.message);
    } finally {
      setLoadingTx(false);
    }
  };

  useEffect(() => {
    fetchBalance();
    fetchTransactions(); // load on mount
  }, []);

  // Validation function
  const validateAmount = () => {
    if (topUpAmount.trim() === "") {
      setPopupMessage("Amount is required.");
      return false;
    }

    if (/[^0-9.]/.test(topUpAmount)) {
      setPopupMessage("Amount cannot contain letters.");
      return false;
    }

    const amount = parseFloat(topUpAmount);

    if (isNaN(amount)) {
      setPopupMessage("Invalid amount entered.");
      return false;
    }

    if (amount <= 0) {
      setPopupMessage("Amount must be greater than zero.");
      return false;
    }

    if (amount > 1500) {
      setPopupMessage("You cannot top up more than R1500 at once.");
      return false;
    }


    return true;
  };

  // Top-up handler
  const handleTopUp = async () => {
    if (!validateAmount()) return;

    const amount = parseFloat(topUpAmount);

    try {
      const res = await fetch(`${ACCOUNT_API}/topup`, {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
          Authorization: `Bearer ${token}`,
        },
        body: JSON.stringify({ amount }),
      });

      const data = await res.json();
      if (!res.ok) throw new Error(data.message || "Top-up failed");

      setBalance(data?.newBalance ?? balance + amount);
      setTopUpAmount("");
      setPopupMessage(`Successfully topped up R${amount.toFixed(2)}`);

      // Refresh transactions after top-up
      fetchTransactions();
    } catch (err) {
      setPopupMessage(err.message);
    }
  };

  return (
    <div className="acc-container">
      <button className="back-btn" onClick={() => window.history.back()}>
        ← Back
      </button>

      <h1 className="acc-title">My Account</h1>
      <p className="acc-sub">Manage your balance, top-ups & spending</p>

      {/* BALANCE */}
      <div className="acc-card balance-card">
        <h2>Available Balance</h2>
        <div className="acc-balance">R {balance.toFixed(2)}</div>
      </div>

      {/* TOP-UP */}
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

      {/* TRANSACTION HISTORY */}
      <div className="acc-card history-card">
        <h2>Transaction History</h2>

        {loadingTx ? (
          <p>Loading transactions...</p>
        ) : transactions.length === 0 ? (
          <p className="acc-empty">No transactions yet...</p>
        ) : (
          <div className="tx-list">
            {transactions.slice(0, 4).map((tx) => (
              <div key={tx.transactionId} className="tx-item">
                <div className="tx-left">
                  <span className="tx-type">{tx.type}</span>
                  <span className="tx-date">
                    {new Date(tx.createdAt).toLocaleDateString()}
                  </span>
                </div>

                <span
                  className={
                    tx.totalAmount < 0 ? "tx-amount neg" : "tx-amount pos"
                  }
                >
                  {tx.totalAmount < 0
                    ? `- R${Math.abs(tx.totalAmount).toFixed(2)}`
                    : `+ R${tx.totalAmount.toFixed(2)}`}
                </span>
              </div>
            ))}

            <button
              className="tx-view-all"
              onClick={() => (window.location.href = "/transactions")}
            >
              View Full History →
            </button>
          </div>
        )}
      </div>

      {/* Popup */}
      {popupMessage && (
        <div className="popup-overlay" onClick={() => setPopupMessage("")}>
          <div className="popup-box">
            <p>{popupMessage}</p>
            <button className="btn primary" onClick={() => setPopupMessage("")}>
              OK
            </button>
          </div>
        </div>
      )}
    </div>
  );
}

export default AccountsPage;
