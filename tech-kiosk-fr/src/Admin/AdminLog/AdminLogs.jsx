import React, { useState, useEffect } from "react";
import AdminLayout from "../AdminLayout";
import "./AdminLogs.css";

const API_URL = "https://localhost:5016/api/admin/transactions";

export default function AdminLogs() {
  const [filter, setFilter] = useState("all");
  const [transactions, setTransactions] = useState([]);
  const [loading, setLoading] = useState(true);
  const token = localStorage.getItem("token"); // JWT token

  // ---------------- FETCH TRANSACTIONS ----------------
  const fetchTransactions = async () => {
    try {
      setLoading(true);
      const res = await fetch(`${API_URL}`, {
        headers: { Authorization: `Bearer ${token}` },
      });
      if (!res.ok) throw new Error("Failed to fetch transactions");
      const data = await res.json();
      // Assuming your API returns { total, page, pageSize, data: [...] }
      setTransactions(data.data || []);
      setLoading(false);
    } catch (err) {
      console.error(err);
      alert(err.message);
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchTransactions();
  }, []);

  // ---------------- FILTERED TRANSACTIONS ----------------
  const filtered = transactions.filter((t) => {
    if (filter === "all") return true;
    // Map filter names to your API types
    if (filter === "purchase") return t.type === "Purchase";
    if (filter === "deposit") return t.type === "Deposit";
    if (filter === "admin_topup") return t.type === "TopUp";
    return true;
  });

  return (
    <AdminLayout>
      <h1>Transaction Logs</h1>

      <div className="filter-buttons">
        <button onClick={() => setFilter("all")}>All</button>
        <button onClick={() => setFilter("purchase")}>Purchases</button>
        <button onClick={() => setFilter("deposit")}>User Deposits</button>
        <button onClick={() => setFilter("admin_topup")}>Admin Top-Ups</button>
      </div>

      {loading ? (
        <p>Loading transactions...</p>
      ) : filtered.length === 0 ? (
        <p>No transactions found.</p>
      ) : (
        <div className="logs-list">
          {filtered.map((t) => (
            <div className="log-item" key={t.transactionId}>
              <p><strong>Type:</strong> {t.type}</p>
              <p><strong>Amount:</strong> R {t.totalAmount.toFixed(2)}</p>
              <p><strong>Date:</strong> {new Date(t.createdAt).toLocaleString()}</p>
              {t.accountOwner && <p><strong>User:</strong> {t.accountOwner}</p>}
              {t.description && <p><strong>Description:</strong> {t.description}</p>}
            </div>
          ))}
        </div>
      )}
    </AdminLayout>
  );
}
