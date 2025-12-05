import React, { useState } from "react";
import "./TransactionsPage.css";

function TransactionsPage() {
  const [search, setSearch] = useState("");

  // Example transaction data
  const transactions = [
    { id: 1, type: "Top-Up", amount: 150, date: "2025-01-12", status: "Completed" },
    { id: 2, type: "Purchase", amount: -45.99, date: "2025-01-14", status: "Completed" },
    { id: 3, type: "Purchase", amount: -89.5, date: "2025-01-16", status: "Failed" },
    { id: 4, type: "Top-Up", amount: 200, date: "2025-01-19", status: "Pending" },
  ];

  const filtered = transactions.filter((t) =>
    t.type.toLowerCase().includes(search.toLowerCase())
  );

  return (
    <div className="tx-container">
      <h1 className="tx-title">Transaction History</h1>
      <p className="tx-sub">View all your purchases & top-ups</p>

      {/* Search Bar */}
      <div className="tx-search-wrap">
        <input
          className="tx-search"
          placeholder="Search transactions..."
          value={search}
          onChange={(e) => setSearch(e.target.value)}
        />
      </div>

      {/* Table */}
      <div className="tx-table">
        <div className="tx-header">
          <span>Type</span>
          <span>Amount</span>
          <span>Date</span>
          <span>Status</span>
        </div>

        {filtered.length === 0 ? (
          <p className="tx-empty">No matching transactions...</p>
        ) : (
          filtered.map((t) => (
            <div key={t.id} className="tx-row">
              <span className="tx-type">{t.type}</span>
              <span className={`tx-amount ${t.amount < 0 ? "neg" : "pos"}`}>
                {t.amount < 0 ? `- R${Math.abs(t.amount).toFixed(2)}` : `+ R${t.amount.toFixed(2)}`}
              </span>
              <span>{t.date}</span>
              <span className={`tx-status ${t.status.toLowerCase()}`}>
                {t.status}
              </span>
            </div>
          ))
        )}
      </div>
    </div>
  );
}
export default TransactionsPage;