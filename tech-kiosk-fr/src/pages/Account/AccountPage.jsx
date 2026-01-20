import React, { useEffect, useState } from "react";
import "./AccountPage.css";
import { toast } from "react-toastify";
import "react-toastify/dist/ReactToastify.css";
import ConfirmTopUpModal from "../../components/ConfirmTopUpModal";

const ACCOUNT_API = "https://localhost:5016/api/account";
const TX_API = "https://localhost:5016/api/Transaction";

function AccountsPage() {
  // 🔐 AUTH STATE (FIXED LOCATION)
  const [authReady, setAuthReady] = useState(false);
  const [authToken, setAuthToken] = useState(null);
  const [authUserId, setAuthUserId] = useState(null);

  // 💰 ACCOUNT STATE
  const [balance, setBalance] = useState(0);
  const [topUpAmount, setTopUpAmount] = useState("");
  const [transactions, setTransactions] = useState([]);
  const [loadingTx, setLoadingTx] = useState(true);
  const [popupMessage, setPopupMessage] = useState("");
  const [showConfirmTopUp, setShowConfirmTopUp] = useState(false);
  const [pendingAmount, setPendingAmount] = useState(0);

  /* ---------------- AUTH INITIALIZATION ---------------- */
  useEffect(() => {
    const token = localStorage.getItem("token");
    const userId = localStorage.getItem("userId");

    if (!token || !userId) {
      toast.error("Session expired. Please log in again.");
      return;
    }

    setAuthToken(token);
    setAuthUserId(userId);
    setAuthReady(true);
  }, []);

  /* ---------------- FETCH BALANCE ---------------- */
  const fetchBalance = async () => {
    try {
      const res = await fetch(`${ACCOUNT_API}/balance`, {
        headers: { Authorization: `Bearer ${authToken}` },
      });

      if (!res.ok) throw new Error("Failed to fetch balance");

      const data = await res.json();
      setBalance(data?.balance ?? 0);
    } catch (err) {
      toast.error(err.message);
      setBalance(0);
    }
  };

  /* ---------------- FETCH TRANSACTIONS ---------------- */
  const fetchTransactions = async () => {
    try {
      const res = await fetch(`${TX_API}/mytrasactions`, {
        headers: { Authorization: `Bearer ${authToken}` },
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

  /* ---------------- AUTO LOAD ON LOGIN ---------------- */
  useEffect(() => {
    if (!authReady) return;

    setBalance(0);
    setTransactions([]);
    setLoadingTx(true);

    fetchBalance();
    fetchTransactions();
  }, [authReady, authUserId]);

  /* ---------------- VALIDATE TOP UP ---------------- */
  const validateAmount = () => {
    if (topUpAmount.trim() === "") {
      setPopupMessage("Amount is required.");
      return;
    }

    const amount = parseFloat(topUpAmount);

    if (isNaN(amount) || amount <= 0) {
      setPopupMessage("Enter a valid amount.");
      return;
    }

    if (amount > 1500) {
      setPopupMessage("You cannot top up more than R1500 at once.");
      return;
    }

    setPendingAmount(amount);
    setShowConfirmTopUp(true);
  };

  /* ---------------- HANDLE TOP UP ---------------- */
  const handleTopUp = async (amount) => {
    try {
      const res = await fetch(`${ACCOUNT_API}/topup`, {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
          Authorization: `Bearer ${authToken}`,
        },
        body: JSON.stringify({ amount }),
      });

      const data = await res.json();
      if (!res.ok) throw new Error(data.message || "Top-up failed");

      setBalance(data?.newBalance ?? balance + amount);
      setTopUpAmount("");
      setPopupMessage(`Successfully topped up R${amount.toFixed(2)}`);

      fetchTransactions();
    } catch (err) {
      setPopupMessage(err.message);
    }
  };

  /* ---------------- UI ---------------- */
  return (
    <div className="acc-container">
      <button className="back-btn" onClick={() => window.history.back()}>
        ← Back
      </button>

      <h1 className="acc-title">My Account</h1>
      <p className="acc-sub">Manage your balance, top-ups & spending</p>

      <div className="acc-card balance-card">
        <h2>Available Balance</h2>
        <div
          className={`acc-balance ${
            balance < 0 ? "neg" : "pos"
          }`}
        >
          R {balance.toFixed(2)}
        </div>
      </div>

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
          <button className="topup-btn" onClick={validateAmount}>
            Top Up
          </button>
        </div>
      </div>

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
                    className={`tx-amount ${
                      tx.type?.toLowerCase() === "checkout"
                        ? "neg"
                        : tx.totalAmount < 0
                        ? "neg"
                        : "pos"
                    }`}
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

      <ConfirmTopUpModal
        show={showConfirmTopUp}
        amount={pendingAmount}
        onConfirm={() => {
          setShowConfirmTopUp(false);
          handleTopUp(pendingAmount);
        }}
        onCancel={() => setShowConfirmTopUp(false)}
      />

      {popupMessage && (
        <div className="popup-overlay" onClick={() => setPopupMessage("")}>
          <div className="popup-box">
            <p>{popupMessage}</p>
            <button
              className="btn primary"
              onClick={() => setPopupMessage("")}
            >
              OK
            </button>
          </div>
        </div>
      )}
    </div>
  );
}

export default AccountsPage;
