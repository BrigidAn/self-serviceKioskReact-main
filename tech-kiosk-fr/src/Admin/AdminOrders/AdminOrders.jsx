import React, { useEffect, useState } from "react";
import AdminLayout from "../AdminLayout";
import { ToastContainer, toast } from "react-toastify";
import "react-toastify/dist/ReactToastify.css";
import "./AdminOrders.css"; // optional for styling

const API_URL = "https://localhost:5016/api";

export default function AdminOrders() {
  const [orders, setOrders] = useState([]);
  const [loading, setLoading] = useState(true);

  const token = localStorage.getItem("token");

  const fetchOrders = async () => {
    setLoading(true);
    try {
      const res = await fetch(`${API_URL}/order`, {
        headers: token ? { Authorization: `Bearer ${token}` } : {},
      });
      if (!res.ok) throw new Error("Failed to fetch orders");
      const data = await res.json();
      setOrders(Array.isArray(data) ? data : data?.data ?? []);
    } catch (err) {
      console.error(err);
      toast.error(err.message || "Failed to load orders");
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchOrders();
    // eslint-disable-next-line
  }, []);

  const handleStatusChange = async (orderId, newStatus) => {
    try {
      const res = await fetch(`${API_URL}/order/${orderId}/status`, {
        method: "PUT",
        headers: {
          "Content-Type": "application/json",
          ...(token ? { Authorization: `Bearer ${token}` } : {}),
        },
        body: JSON.stringify({ status: newStatus }),
      });

      if (!res.ok) throw new Error("Failed to update status");
      toast.success("Order status updated");
      fetchOrders();
    } catch (err) {
      console.error(err);
      toast.error(err.message || "Status update failed");
    }
  };

  return (
    <AdminLayout>
      <ToastContainer position="top-right" />
      <div className="admin-orders">
        <header className="dashboard-header">
          <h1>All Orders</h1>
          <button className="btn" onClick={fetchOrders}>Refresh</button>
        </header>

        {loading ? (
          <p>Loading orders...</p>
        ) : orders.length === 0 ? (
          <p>No orders found</p>
        ) : (
          <table className="orders-table">
            <thead>
              <tr>
                <th>#</th>
                <th>Order ID</th>
                <th>User</th>
                <th>Products</th>
                <th>Total (R)</th>
                <th>Status</th>
                <th>Date</th>
                <th>Actions</th>
              </tr>
            </thead>
            <tbody>
              {orders.map((order, idx) => (
                <tr key={order.id}>
                  <td>{idx + 1}</td>
                  <td>{order.id}</td>
                  <td>{order.userName || order.userEmail}</td>
                  <td>
                    {order.items?.map((i) => (
                      <div key={i.productId}>{i.name} x{i.quantity}</div>
                    ))}
                  </td>
                  <td>R {Number(order.total).toFixed(2)}</td>
                  <td>{order.status}</td>
                  <td>{new Date(order.createdAt).toLocaleString()}</td>
                  <td>
                    {order.status !== "Completed" && (
                      <button
                        className="btn small"
                        onClick={() => handleStatusChange(order.id, "Completed")}
                      >
                        Mark Completed
                      </button>
                    )}
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        )}
      </div>
    </AdminLayout>
  );
}
