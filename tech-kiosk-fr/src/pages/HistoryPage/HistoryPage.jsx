import React, { useEffect, useState } from "react";
import "./HistoryPage.css";
import { useNavigate } from "react-router-dom";

const ORDERS_PER_PAGE = 6;

function HistoryPage() {
  const [orders, setOrders] = useState([]);
  const [loading, setLoading] = useState(true);
  const [currentPage, setCurrentPage] = useState(1);
  const [sortOrder, setSortOrder] = useState("desc"); // asc | desc
  const navigate = useNavigate();

  useEffect(() => {
    const token = localStorage.getItem("token");

    if (!token) {
      navigate("/login");
      return;
    }

    fetch("https://localhost:5016/api/order/myOrders", {
      headers: {
        Authorization: `Bearer ${token}`,
        "Content-Type": "application/json",
      },
    })
      .then((res) => res.json())
      .then((data) => setOrders(data))
      .catch(() => setOrders([]))
      .finally(() => setLoading(false));
  }, [navigate]);

  if (loading) {
    return <p className="loading">Loading order history...</p>;
  }

  /* ---------------- SORTING ---------------- */
  const sortedOrders = [...orders].sort((a, b) =>
    sortOrder === "asc"
      ? new Date(a.orderDate) - new Date(b.orderDate)
      : new Date(b.orderDate) - new Date(a.orderDate)
  );

  /* ---------------- PAGINATION ---------------- */
  const totalPages = Math.ceil(sortedOrders.length / ORDERS_PER_PAGE);
  const startIndex = (currentPage - 1) * ORDERS_PER_PAGE;
  const paginatedOrders = sortedOrders.slice(
    startIndex,
    startIndex + ORDERS_PER_PAGE
  );

  return (
    <div className="history-page">
      <button className="back-btn" onClick={() => navigate("/products")}>
        ← Back
      </button>

      <h2>Order History</h2>

      {orders.length === 0 ? (
        <p>No orders yet.</p>
      ) : (
        <>
          {/* 🔹 SORT CONTROLS */}
          <div className="orders-sort">
            <span>Sort by date:</span>
            <button
              className={sortOrder === "desc" ? "active" : ""}
              onClick={() => {
                setSortOrder("desc");
                setCurrentPage(1);
              }}
            >
              Newest
            </button>
            <button
              className={sortOrder === "asc" ? "active" : ""}
              onClick={() => {
                setSortOrder("asc");
                setCurrentPage(1);
              }}
            >
              Oldest
            </button>
          </div>

          {/* 🔹 ORDERS GRID */}
          <div className="orders-grid">
            {paginatedOrders.map((o) => (
              <div className="order-card" key={o.orderId}>
                <div className="order-header">
                  <h3>Order #{o.orderId}</h3>
                  <span className="order-date">
                    {new Date(o.orderDate).toLocaleString()}
                  </span>
                </div>

                <div className="order-status">
                  <span>Status: {o.status}</span>
                  <span>Payment: {o.paymentStatus}</span>
                </div>

                <div className="order-items">
                  {o.items.map((i) => (
                    <div className="order-item-row" key={i.productId}>
                      <div className="order-item-details">
                        <h4>{i.productName}</h4>
                        <p>Qty: {i.quantity}</p>
                        <p>Price: R {Number(i.priceAtPurchase).toFixed(2)}</p>
                        <p className="subtotal">
                          Subtotal: R{" "}
                          {(Number(i.priceAtPurchase) * i.quantity).toFixed(2)}
                        </p>
                      </div>
                    </div>
                  ))}
                </div>

                <div className="order-total">
                  <strong>Total:</strong>
                  <span>R {Number(o.totalAmount).toFixed(2)}</span>
                </div>
              </div>
            ))}
          </div>

          {/* 🔹 PAGINATION */}
          <div className="orders-pagination">
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

export default HistoryPage;
