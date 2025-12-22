import React, { useEffect, useState } from "react";
import "./TransactionsPage.css";
import axios from "axios";
import { toast } from "react-toastify";

function TransactionsPage() {
  const [search, setSearch] = useState("");
  const [transactions, setTransactions] = useState([]);
  const [loading, setLoading] = useState(true);

  const token = localStorage.getItem("token");
  const userId = localStorage.getItem("userId");

  useEffect(() => {
    const fetchTransactions = async () => {
      try {
        const res = await axios.get(
          `https://localhost:5016/api/Transaction/mytrasactions`,
          {
            headers: { Authorization: `Bearer ${token}` },
          }
        );
        
        const sorted = res.data.sort(
          (a, b) => new Date(b.createdAt) - new Date(a.createdAt)
        );

        setTransactions(sorted);
      } catch (err) {
        console.error(err);
        toast.error("Could not load transactions");
      } finally {
        setLoading(false);
      }
    };

    fetchTransactions();
  }, []);

  const filtered = transactions.filter((t) =>
    t.type.toLowerCase().includes(search.toLowerCase())
  );

  return (
    <div className="tx-container">
      <button className="tx-back-btn" onClick={() => window.history.back()}>
        ← Back
      </button>

      <h1 className="tx-title">Transaction History</h1>
      <p className="tx-sub">View all your purchases & top-ups</p>

      <div className="tx-search-wrap">
        <input
          className="tx-search"
          placeholder="Search transactions..."
          value={search}
          onChange={(e) => setSearch(e.target.value)}
        />
      </div>

      {loading ? (
        <p>Loading transactions...</p>
      ) : filtered.length === 0 ? (
        <p className="tx-empty">No matching transactions...</p>
      ) : (
        <div className="tx-table">
          <div className="tx-header">
            <span>Type</span>
            <span>Amount</span>
            <span>Date</span>
            <span>Status</span>
          </div>

          {filtered.map((t) => (
            <div key={t.transactionId} className="tx-row">
              <span className="tx-type">{t.type}</span>

              <span
                className={`tx-amount ${
                  t.type.toLowerCase() === "debit" ? "neg" : "pos"
                }`}
              >
                {t.type.toLowerCase() === "debit"
                  ? `- R${t.totalAmount.toFixed(2)}`
                  : `+ R${t.totalAmount.toFixed(2)}`}
              </span>

              <span>{new Date(t.createdAt).toLocaleDateString()}</span>

              <span
                className={`tx-status ${
                  t.type.toLowerCase() === "debit" ? "failed" : "completed"
                }`}
              >
                {t.type.toLowerCase() === "debit"
                  ? "Payment"
                  : "Top-up"}
              </span>
            </div>
          ))}
        </div>
      )}
    </div>
  );
}

export default TransactionsPage;
