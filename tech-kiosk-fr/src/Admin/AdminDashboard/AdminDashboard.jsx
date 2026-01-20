import React, { useEffect, useState } from "react";
import AdminLayout from "../AdminLayout";
import toast, { Toaster } from "react-hot-toast";
import {
  FaBox,
  FaExclamationTriangle,
  FaUsers,
  FaShoppingCart,
} from "react-icons/fa";
import {
  LineChart,
  Line,
  XAxis,
  YAxis,
  Tooltip,
  ResponsiveContainer,
  CartesianGrid,
} from "recharts";
import "./AdminDashboard.css";

const API_URL = "https://localhost:5016/api";

export default function AdminDashboard() {
  const [stats, setStats] = useState({
    totalProducts: 0,
    lowStock: 0,
    totalUsers: 0,
    totalOrders: 0,
  });

  const [allOrders, setAllOrders] = useState([]);
  const [ordersChart, setOrdersChart] = useState([]);
  const [orderFilter, setOrderFilter] = useState("recent");
  const [activeTab, setActiveTab] = useState("recent");

  const token = localStorage.getItem("token");

  const fetchDashboardData = async () => {
    try {
      // PRODUCTS
      const resProducts = await fetch(`${API_URL}/Product?page=1&pageSize=50`, {
        headers: { Authorization: `Bearer ${token}` },
      });
      if (!resProducts.ok) throw new Error("Failed to fetch products");
      const productsResponse = await resProducts.json();
      const products = productsResponse.data || [];
      const totalProducts = productsResponse.total ?? products.length;
      const lowStock = products.filter(p => p.quantity <= 5).length;

      // USERS
      const resUsers = await fetch(`${API_URL}/admin/users?page=1&pageSize=50`, {
        headers: { Authorization: `Bearer ${token}` },
      });
      const usersResponse = await resUsers.json();
      const totalUsers = usersResponse.total ?? usersResponse.data?.length ?? 0;

      // ORDERS
      const resOrders = await fetch(`${API_URL}/admin/orders?page=1&pageSize=100`, {
        headers: { Authorization: `Bearer ${token}` },
      });
      const ordersResponse = await resOrders.json();
      const orders = ordersResponse.data || [];
      const totalOrders = ordersResponse.total ?? orders.length;

      setStats({ totalProducts, lowStock, totalUsers, totalOrders });
      setAllOrders(orders.sort((a, b) => new Date(b.orderDate) - new Date(a.orderDate)));

      const grouped = {};
      orders.forEach(o => {
        const date = new Date(o.orderDate).toLocaleDateString();
        grouped[date] = (grouped[date] || 0) + 1;
      });
      const chartData = Object.keys(grouped)
        .slice(-7)
        .map(d => ({ date: d, orders: grouped[d] }));
      setOrdersChart(chartData);
    } catch (error) {
      console.error(error);
      toast.error("Failed to load dashboard data");
    }
  };

  useEffect(() => {
    fetchDashboardData();
    const interval = setInterval(fetchDashboardData, 10000);
    return () => clearInterval(interval);
  }, []);

  const filteredOrders = allOrders
    .filter(o => {
      if (orderFilter === "recent") return true;
      if (orderFilter === "pending") return o.status.toLowerCase() === "pending";
      if (orderFilter === "completed") return o.status.toLowerCase() === "completed";
      return true;
    })
    .sort((a, b) => new Date(b.orderDate) - new Date(a.orderDate))
    .slice(0, 6);

  return (
    <AdminLayout>
      <Toaster position="top-right" />
      <div className="dashboard-container">

        {/* ================= ACTION CARDS ================= */}
        <div className="action-cards-grid">
          <div className="action-card">
            <FaShoppingCart className="action-icon" />
            <p>Recent Orders</p>
          </div>
          <div className="action-card">
            <FaBox className="action-icon" />
            <p>Manage Products</p>
          </div>
          <div className="action-card">
            <FaUsers className="action-icon" />
            <p>Customers</p>
          </div>
          <div className="action-card">
            <FaExclamationTriangle className="action-icon" />
            <p>Alerts</p>
          </div>
        </div>

        {/* ================= TABS ================= */}
        <div className="dashboard-tabs">
          <button
            className={activeTab === "recent" ? "active" : ""}
            onClick={() => setActiveTab("recent")}
          >
            Recent Orders
          </button>
          <button
            className={activeTab === "graph" ? "active" : ""}
            onClick={() => setActiveTab("graph")}
          >
            Orders (Graph)
          </button>
        </div>

        {/* ================= TAB CONTENT ================= */}
        <div className="tab-content-wrapper">

          {activeTab === "recent" && (
            <div className="glass-section orders-section">
              <h3>Recent Orders</h3>
              <div className="order-filters">
                <button
                  className={orderFilter === "recent" ? "active" : ""}
                  onClick={() => setOrderFilter("recent")}
                >
                  All
                </button>
                <button
                  className={orderFilter === "pending" ? "active" : ""}
                  onClick={() => setOrderFilter("pending")}
                >
                  Pending
                </button>
                <button
                  className={orderFilter === "completed" ? "active" : ""}
                  onClick={() => setOrderFilter("completed")}
                >
                  Completed
                </button>
              </div>

              <div className="table-wrapper">
                <table className="glass-table">
                  <thead>
                    <tr>
                      <th>ID</th>
                      <th>User</th>
                      <th>Total</th>
                      <th>Status</th>
                      <th>Payment</th>
                      <th>Date</th>
                    </tr>
                  </thead>
                  <tbody>
                    {filteredOrders.length === 0 ? (
                      <tr><td colSpan="6">No orders found</td></tr>
                    ) : (
                      filteredOrders.map(o => (
                        <tr key={o.orderId}>
                          <td>{o.orderId}</td>
                          <td>{o.customer}</td>
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
          )}

          {activeTab === "graph" && (
            <div className="glass-section chart-section">
              <h3>Orders (Last 7 Days)</h3>
              <ResponsiveContainer width="100%" height={300}>
                <LineChart data={ordersChart}>
                  <CartesianGrid strokeDasharray="3 3" stroke="rgba(255,255,255,0.15)" />
                  <XAxis dataKey="date" stroke="#e5e7eb" />
                  <YAxis stroke="#e5e7eb" allowDecimals={false} />
                  <Tooltip />
                  <Line type="monotone" dataKey="orders" stroke="#4f46e5" strokeWidth={3} />
                </LineChart>
              </ResponsiveContainer>
            </div>
          )}

        </div>
      </div>
    </AdminLayout>
  );
}
