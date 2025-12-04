import React from "react"
import { useNavigate } from "react-router-dom";
import NavBar from "../../components/Navbar";

function TransactionsPage() {
  const logs = JSON.parse(localStorage.getItem("transactions") || "[]");

  const navigate = useNavigate();

  return (
    <div className="transaction-page">
      <NavBar />

      <button className="back-btn" onClick={() => navigate("/products")}>
          ← Back
        </button>

      <h2>Deposit Transactions</h2>

      {logs.length === 0 ? (
        <p>No deposit history</p>
      ) : (
        logs.map((t, i) => (
          <div key={i} className="transaction-row">
            <span>{t.type.toUpperCase()}</span>
            <span>R {t.amount}</span>
            <span>{new Date(t.date).toLocaleString()}</span>
          </div>
        ))
      )}
    </div>
  );
}
export default TransactionsPage;