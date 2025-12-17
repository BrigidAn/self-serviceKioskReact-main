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
    revenue: 0,
    pendingOrders: 0,
    paidOrders: 0,
  });

  const [recentOrders, setRecentOrders] = useState([]);
  const [lowStockProducts, setLowStockProducts] = useState([]);
  const [recentUsers, setRecentUsers] = useState([]);

  const token = localStorage.getItem("token");
  const isMounted = useRef(true);

  const fetchDashboardData = async () => {
    try {
      // ---------------- Products ----------------
      const resProducts = await fetch(`${API_URL}/product`, {
        headers: { Authorization: `Bearer ${token}` },
      });
      const products = await resProducts.json();

      const totalProducts = products.length;
      const lowStockList = products.filter(p => p.quantity <= 5);
      const lowStock = lowStockList.length;

      // ---------------- Users ----------------
      const resUsers = await fetch(`${API_URL}/admin/users?page=1&pageSize=50`, {
        headers: { Authorization: `Bearer ${token}` },
      });
      const usersData = await resUsers.json();

      const recentUsersList = (usersData.data || [])
        .sort((a, b) => new Date(b.createdAt) - new Date(a.createdAt))
        .slice(0, 5);

      // ---------------- Orders ----------------
      const resOrders = await fetch(`${API_URL}/admin/orders?page=1&pageSize=10`, {
        headers: { Authorization: `Bearer ${token}` },
      });
      const ordersData = await resOrders.json();
      const orders = ordersData.data || [];

      const revenue = orders.reduce((sum, o) => sum + o.totalAmount, 0);
      const pendingOrders = orders.filter(o => o.status === "Pending").length;
      const paidOrders = orders.filter(o => o.paymentStatus === "Paid").length;

      if (!isMounted.current) return;

      setStats({
        totalProducts,
        lowStock,
        totalUsers: usersData.total || usersData.data?.length || 0,
        totalOrders: ordersData.total || orders.length,
        revenue,
        pendingOrders,
        paidOrders,
      });

      setRecentOrders(orders);
      setLowStockProducts(lowStockList.slice(0, 5));
      setRecentUsers(recentUsersList);
    } catch (err) {
      console.error(err);
      toast.error("Failed to load dashboard data");
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

        {/* ================= Stats ================= */}
        <div className="stats-grid">
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
            <table>
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

        {/* ================= Low Stock ================= */}
        <div className="glass-section">
          <h3>Low Stock Alerts</h3>
          <ul className="simple-list">
            {lowStockProducts.length === 0
              ? <li>No low stock products</li>
              : lowStockProducts.map(p => (
                  <li key={p.productId}>
                    {p.name} — <strong>{p.quantity}</strong> left
                  </li>
                ))}
          </ul>
        </div>

        {/* ================= Recent Users ================= */}
        <div className="glass-section">
          <h3>Recent Users</h3>
          <ul className="simple-list">
            {recentUsers.length === 0
              ? <li>No recent signups</li>
              : recentUsers.map(u => (
                  <li key={u.id}>{u.name} ({u.email})</li>
                ))}
          </ul>
        </div>

      </div>
    </AdminLayout>
  );
}
