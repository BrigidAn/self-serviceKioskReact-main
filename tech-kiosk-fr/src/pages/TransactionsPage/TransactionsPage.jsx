import React, { useEffect, useState } from "react";
import "./TransactionsPage.css";
import axios from "axios";
import { toast } from "react-toastify";

const ITEMS_PER_PAGE = 8;

function TransactionsPage() {
  const [search, setSearch] = useState("");
  const [transactions, setTransactions] = useState([]);
  const [loading, setLoading] = useState(true);
  const [currentPage, setCurrentPage] = useState(1);

  const token = localStorage.getItem("token");

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

  /* ---------------- Filter ---------------- */
  const filtered = transactions.filter((t) =>
    t.type.toLowerCase().includes(search.toLowerCase())
  );

  /* ---------------- Pagination ---------------- */
  const totalPages = Math.ceil(filtered.length / ITEMS_PER_PAGE);
  const startIndex = (currentPage - 1) * ITEMS_PER_PAGE;
  const paginated = filtered.slice(
    startIndex,
    startIndex + ITEMS_PER_PAGE
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
          onChange={(e) => {
            setSearch(e.target.value);
            setCurrentPage(1); // reset page on search
          }}
        />
      </div>

      {loading ? (
        <p>Loading transactions...</p>
      ) : filtered.length === 0 ? (
        <p className="tx-empty">No matching transactions...</p>
      ) : (
        <>
          <div className="tx-table">
            <div className="tx-header">
              <span>Type</span>
              <span>Amount</span>
              <span>Date</span>
              <span>Status</span>
            </div>

            {paginated.map((t) => {
              const isCheckout =
                t.type.toLowerCase() === "checkout" ||
                t.type.toLowerCase() === "debit";

              return (
                <div key={t.transactionId} className="tx-row">
                  <span className="tx-type">{t.type}</span>

                  <span
                    className={`tx-amount ${
                      isCheckout ? "neg" : "pos"
                    }`}
                  >
                    {isCheckout
                      ? `- R${t.totalAmount.toFixed(2)}`
                      : `+ R${t.totalAmount.toFixed(2)}`}
                  </span>

                  <span>
                    {new Date(t.createdAt).toLocaleDateString()}
                  </span>

                  <span
                    className={`tx-status ${
                      isCheckout ? "failed" : "completed"
                    }`}
                  >
                    {isCheckout ? "Checkout" : "Top-up"}
                  </span>
                </div>
              );
            })}
          </div>

          {/* ---------------- Pagination Controls ---------------- */}
          <div className="tx-pagination">
            <button
              disabled={currentPage === 1}
              onClick={() => setCurrentPage((p) => p - 1)}
            >
              Previous
            </button>

            <span>
              Page {currentPage} of {totalPages}
            </span>

            <button
              disabled={currentPage === totalPages}
              onClick={() => setCurrentPage((p) => p + 1)}
            >
              Next
            </button>
          </div>
        </>
      )}
    </div>
  );
}

export default TransactionsPage;
