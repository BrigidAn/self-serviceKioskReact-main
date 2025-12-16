import React, { useEffect, useState } from "react";
import AdminLayout from "../AdminLayout";
import { ToastContainer, toast } from "react-toastify";
import "react-toastify/dist/ReactToastify.css";
import "./AdminOrders.css"; // optional for styling

const API_URL = "https://localhost:5016/api";

export default function AdminOrders() {
  const [orders, setOrders] = useState([]);
  const [products, setProducts] = useState([]);
  const [loading, setLoading] = useState(true);

  const token = localStorage.getItem("token");

  const fetchProducts = async () => {
    try {
      const res = await fetch(`${API_URL}/admin/products`, {
        headers: token ? { Authorization: `Bearer ${token}` } : {},
      });
      if (!res.ok) throw new Error("Failed to fetch products");
      const data = await res.json();
      return Array.isArray(data) ? data : data?.data ?? [];
    } catch (err) {
      console.error(err);
      toast.error(err.message || "Failed to load products");
      return [];
    }
  };

  const fetchOrders = async () => {
    setLoading(true);
    try {
      const prodList = await fetchProducts();
      setProducts(prodList);

      const res = await fetch(`${API_URL}/admin/orders`, {
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
      const res = await fetch(`${API_URL}/order/${orderId}`, {
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

  const getProductName = (productId) => {
    const prod = products.find(p => p.productId === productId);
    return prod ? prod.name : `Product #${productId}`;
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
                <th>Id</th>
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
                <tr key={order.orderId}>
                  <td>{idx + 1}</td>
                  <td>{order.orderId}</td>
                  <td>{order.customer}</td>
                  <td>
                    {order.items?.map((i, index) => (
                      <div key={index}>
                        {getProductName(i.productId)} x{i.quantity} @ R{i.priceAtPurchase.toFixed(2)}
                      </div>
                    ))}
                  </td>
                  <td>R {Number(order.totalAmount).toFixed(2)}</td>
                  <td>{order.status}</td>
                  <td>{new Date(order.orderDate).toLocaleString()}</td>
                  <td>
                    {order.status !== "Completed" && (
                      <button
                        className="btn small"
                        onClick={() => handleStatusChange(order.orderId, "Completed")}
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
