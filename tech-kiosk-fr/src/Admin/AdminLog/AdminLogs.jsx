import React, { useState } from "react";
import AdminLayout from "../AdminLayout";
import "./AdminLogs.css";

export default function AdminLogs() {
  const [filter, setFilter] = useState("all");

  const logs = JSON.parse(localStorage.getItem("transactions") || "[]");

  const filtered = logs.filter((log) =>
    filter === "all" ? true : log.type === filter
  );

  return (
    <AdminLayout>
      <h1>Transaction Logs</h1>

      <div className="filter-buttons">
        <button onClick={() => setFilter("all")}>All</button>
        <button onClick={() => setFilter("purchase")}>Purchases</button>
        <button onClick={() => setFilter("deposit")}>User Deposits</button>
        <button onClick={() => setFilter("admin_topup")}>Admin Top-Ups</button>
      </div>

      <div className="logs-list">
        {filtered.map((log, index) => (
          <div className="log-item" key={index}>
            <p><strong>Type:</strong> {log.type}</p>
            <p><strong>Amount:</strong> R {log.amount}</p>
            <p><strong>Date:</strong> {new Date(log.date).toLocaleString()}</p>
            {log.userId && <p><strong>User ID:</strong> {log.userId}</p>}
          </div>
        ))}
      </div>
    </AdminLayout>
  );
}
