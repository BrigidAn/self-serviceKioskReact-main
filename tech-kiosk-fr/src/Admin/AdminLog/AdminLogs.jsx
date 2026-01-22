import React, { useState, useEffect } from "react";
import AdminLayout from "../AdminLayout";
import "./AdminLogs.css";

const API_URL = "https://localhost:5016/api/admin/transactions?page=1&pageSize=100";

export default function AdminLogs() {
  const [filter, setFilter] = useState("all");
  const [transactions, setTransactions] = useState([]);
  const [loading, setLoading] = useState(true);
  const [currentPage, setCurrentPage] = useState(1);
  const itemsPerPage = 15;
  const token = localStorage.getItem("token");
  const [searchTerm, setSearchTerm] = useState("");
  const fetchTransactions = async () => {
    try {
      setLoading(true);
      const res = await fetch(API_URL, {
        headers: { Authorization: `Bearer ${token}` },
      });
      if (!res.ok) throw new Error("Failed to fetch transactions");
      const data = await res.json();
      const sorted = (data.data || []).sort(
        (a, b) => new Date(b.createdAt) - new Date(a.createdAt)
      );
      setTransactions(sorted);
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

const filtered = transactions.filter((t) => {

  let matchesFilter;
  switch (filter) {
    case "credit":
      matchesFilter = t.type.toLowerCase() === "deposit";
      break;
    case "topup":
      matchesFilter = t.type.toLowerCase() === "topup";
      break;
    case "checkout":
      matchesFilter = t.type.toLowerCase() === "checkout" || t.type.toLowerCase() === "purchase";
      break;
    case "admin_topup":
      matchesFilter = t.type.toLowerCase() === "admin_topup";
      break;
    default:
      matchesFilter = true;
  }

  const matchesSearch = t.accountOwner?.toLowerCase().includes(searchTerm.toLowerCase());

  return matchesFilter && (!searchTerm || matchesSearch);
});

  // --- PAGINATION LOGIC ---
  const totalPages = Math.ceil(filtered.length / itemsPerPage);
  const startIndex = (currentPage - 1) * itemsPerPage;
  const currentTransactions = filtered.slice(startIndex, startIndex + itemsPerPage);

  const handlePageChange = (page) => {
    if (page < 1 || page > totalPages) return;
    setCurrentPage(page);
  };

  return (
    <AdminLayout>
      <h1>Transaction Logs</h1>

    <div className="search-bar">
      <input
        type="text"
        placeholder="Search by user..."
        value={searchTerm}
        onChange={(e) => { setSearchTerm(e.target.value); setCurrentPage(1); }}
      />
    </div>


      <div className="filter-buttons">
        <button onClick={() => { setFilter("all"); setCurrentPage(1); }} className={filter === "all" ? "active" : ""}>
          All
        </button>
        <button onClick={() => { setFilter("topup"); setCurrentPage(1); }} className={filter === "topup" ? "active" : ""}>
          Top-Up
        </button>
        <button onClick={() => { setFilter("checkout"); setCurrentPage(1); }} className={filter === "checkout" ? "active" : ""}>
          Checkout
        </button>
        <button onClick={() => { setFilter("Admin_topup"); setCurrentPage(1); }} className={filter === "admin_topup" ? "active" : ""}>
          Admin Top-Up
        </button>
      </div>

      {loading ? (
        <p>Loading transactions...</p>
      ) : currentTransactions.length === 0 ? (
        <p>No transactions found.</p>
      ) : (
        <>
          <div className="logs-list">
            {currentTransactions.map((t) => (
              <div className="log-item" key={t.transactionId}>
                <p><strong>Type:</strong> {t.type}</p>
                <p>
                  <strong>Amount:</strong>{" "}
                  <span className={t.totalAmount < 0 ? "neg-amount" : "pos-amount"}>
                    R {t.totalAmount.toFixed(2)}
                  </span>
                </p>
                <p><strong>Date:</strong> {new Date(t.createdAt).toLocaleString()}</p>
                {t.accountOwner && <p><strong>User:</strong> {t.accountOwner}</p>}
                {t.description && <p><strong>Description:</strong> {t.description}</p>}
              </div>
            ))}
          </div>

          {/* Pagination Controls */}
          <div className="pagination">
            <button onClick={() => handlePageChange(currentPage - 1)} disabled={currentPage === 1}>
              &laquo; Prev
            </button>
            {[...Array(totalPages)].map((_, idx) => (
              <button
                key={idx + 1}
                className={currentPage === idx + 1 ? "active" : ""}
                onClick={() => handlePageChange(idx + 1)}
              >
                {idx + 1}
              </button>
            ))}
            <button onClick={() => handlePageChange(currentPage + 1)} disabled={currentPage === totalPages}>
              Next &raquo;
            </button>
          </div>
        </>
      )}
    </AdminLayout>
  );
}