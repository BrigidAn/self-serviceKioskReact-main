import React from "react";
import "./HistoryPage.css";
import NavBar from "../../components/Navbar";
import { useNavigate } from "react-router-dom"; // <-- useNavigate instead of useNavigation

function HistoryPage() {
  const orders = JSON.parse(localStorage.getItem("orders") || "[]");
  const navigate = useNavigate(); // <-- correct hook

  return (
    <div className="history-page">
      <NavBar />
      <button className="back-btn" onClick={() => navigate("/products")}>
        ← Back
      </button>

      <h2>Order History</h2>

      {orders.length === 0 ? (
        <p>No past orders yet.</p>
      ) : (
        orders.map((o, index) => (
          <div className="order-card" key={index}>
            <p>Date: {new Date(o.date).toLocaleString()}</p>
            <p>Total: R {o.total.toFixed(2)}</p>

            <div className="order-items">
              {o.items.map((i) => (
                <p key={i.id}>
                  {i.name} — R {Number(i.price).toFixed(2)}
                </p>
              ))}
            </div>
          </div>
        ))
      )}
    </div>
  );
}

export default HistoryPage;
