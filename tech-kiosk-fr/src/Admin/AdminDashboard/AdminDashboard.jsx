import React, { useEffect, useState, useRef } from "react";
import AdminLayout from "../AdminLayout";
import toast, { Toaster } from "react-hot-toast";
import {
  FaBox,
  FaExclamationTriangle,
  FaUsers,
  FaShoppingCart,
} from "react-icons/fa";
import "./AdminDashboard.css";

const API_URL = "https://localhost:5016/api";

export default function AdminDashboard() {
  const [stats, setStats] = useState({
    totalProducts: 0,
    lowStock: 0,
    totalUsers: 0,
    totalOrders: 0,
  });

  const [recentOrders, setRecentOrders] = useState([]);
  const token = localStorage.getItem("token");
  const isMounted = useRef(true);

const fetchDashboardData = async () => {
  try {
    // products
    const resProducts = await fetch(`${API_URL}/Product?page=1&pageSize=50`, {
      headers: { Authorization: `Bearer ${token}` },
    });

            if (!resProducts.ok) {
          throw new Error("Failed to fetch products");
        }

    const productsResponse = await resProducts.json();
    const products = Array.isArray(productsResponse) ? productsResponse : productsResponse.data || [];

    const totalProducts = productsResponse.total ?? products.length;
    const lowStock = products.filter(p => p.quantity <= 5).length;

    //users
    const resUsers = await fetch(`${API_URL}/admin/users?page=1&pageSize=50`, {
      headers: { Authorization: `Bearer ${token}` },
    });

    const usersResponse = await resUsers.json();
    const totalUsers = usersResponse.total ?? usersResponse.data?.length ?? 0;

    //orders
    const resOrders = await fetch(`${API_URL}/admin/orders?page=1&pageSize=10`, {
      headers: { Authorization: `Bearer ${token}` },
    });

    const ordersResponse = await resOrders.json();
    const orders = ordersResponse.data || [];
    const totalOrders = ordersResponse.total ?? orders.length;

    setStats({
      totalProducts,
      lowStock,
      totalUsers,
      totalOrders,
    });

    setRecentOrders(orders);
  } catch (error) {
    console.error(error);
  }
};

  useEffect(() => {
    fetchDashboardData();
    const interval = setInterval(fetchDashboardData, 10000);

    return () => {
      isMounted.current = false;
      clearInterval(interval);
    };
  }, []);

  return (
    <AdminLayout>
      <Toaster position="top-right" />

      <div className="dashboard-container">

        <div className="stats-grid glass">
          <div className="i-stat-card">
            <FaBox className="stat-icon" />
            <h2>{stats.totalProducts}</h2>
            <p>Total Products</p>
          </div>

          <div className="i-stat-card">
            <FaExclamationTriangle className="stat-icon warning" />
            <h2>{stats.lowStock}</h2>
            <p>Low Stock</p>
          </div>

          <div className="i-stat-card">
            <FaUsers className="stat-icon" />
            <h2>{stats.totalUsers}</h2>
            <p>Total Users</p>
          </div>

          <div className="i-stat-card">
            <FaShoppingCart className="stat-icon" />
            <h2>{stats.totalOrders}</h2>
            <p>Total Orders</p>
          </div>
        </div>

        {/* ================= Orders Table ================= */}
        <div className="glass-section">
          <h3>Recent Orders</h3>
          <div className="table-wrapper">
            <table className="glass-table">
              <thead>
                <tr>
                  <th>ID</th>
                  <th>User</th>
                  <th>Items</th>
                  <th>Total</th>
                  <th>Status</th>
                  <th>Payment</th>
                  <th>Date</th>
                </tr>
              </thead>
              <tbody>
                {recentOrders.length === 0 ? (
                  <tr><td colSpan="7">No orders found</td></tr>
                ) : (
                  recentOrders.map(o => (
                    <tr key={o.orderId}>
                      <td>{o.orderId}</td>
                      <td>{o.customer}</td>
                      <td>
                        {o.items?.map(i => (
                          <div key={i.productId}>
                            {i.productId} x{i.quantity}
                          </div>
                        ))}
                      </td>
                      <td>R {o.totalAmount.toFixed(2)}</td>
                      <td>{o.status}</td>
                      <td>{o.paymentStatus}</td>
                      <td>{new Date(o.orderDate).toLocaleString()}</td>
                    </tr>
                  ))
                )}
              </tbody>
            </table>
          </div>
        </div>

      </div>
    </AdminLayout>
  );
}
