import React, { useEffect, useState } from "react";
import "./HistoryPage.css";
import { useNavigate } from "react-router-dom";

function HistoryPage() {
  const [orders, setOrders] = useState([]);
  const [loading, setLoading] = useState(true);
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
      .then(async (res) => res.json())
      .then((data) => setOrders(data))
      .catch(() => setOrders([]))
      .finally(() => setLoading(false));
  }, [navigate]);

  if (loading) return <p className="loading">Loading order history...</p>;

  return (
    <div className="history-page">
      <button className="back-btn" onClick={() => navigate("/products")}>
        ← Back
      </button>

      <h2>Order History</h2>

      {orders.length === 0 ? (
        <p>No orders yet.</p>
      ) : (
        orders.map((o) => (
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
                  <img
                    src={i.productImageUrl || "/images/no-image.png"}
                    alt={i.productName}
                    className="order-img"
                  />

                  <div className="order-item-details">
                    <h4>{i.productName}</h4>
                    <p>Quantity: {i.quantity}</p>
                    <p>Price Each: R {Number(i.priceAtPurchase).toFixed(2)}</p>
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
        ))
      )}
    </div>
  );
}

export default HistoryPage;
