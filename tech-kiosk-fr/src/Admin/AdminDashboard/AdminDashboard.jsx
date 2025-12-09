import React, { useEffect, useState } from "react";
import AdminLayout from "../AdminLayout";
import ManageProducts from "../ManageProducts/ManageProducts";
import toast, { Toaster } from "react-hot-toast";
import "./AdminDashboard.css"

const API_URL = "https://localhost:5016/api";

export default function AdminDashboard() {
  const [stats, setStats] = useState({
    totalProducts: 0,
    lowStock: 0,
    totalUsers: 0,
    totalOrders: 0,
  });

  const [recentProducts, setRecentProducts] = useState([]);
  const token = localStorage.getItem("token");

  // ---------------- Fetch Dashboard Data ----------------
  const fetchDashboardData = async () => {
    try {
      const resProducts = await fetch(`${API_URL}/product`, {
        headers: { Authorization: `Bearer ${token}` },
      });
      const productsData = await resProducts.json();

      const totalProducts = productsData.length;
      const lowStock = productsData.filter((p) => p.quantity <= 5).length;

      const resUsers = await fetch(`https://localhost:5016/api/Admin/users`, {
        headers: { Authorization: `Bearer ${token}` },
      });
      const usersData = await resUsers.json();

      const resOrders = await fetch(`${API_URL}/order`, {
        headers: { Authorization: `Bearer ${token}` },
      });
      const ordersData = await resOrders.json();

      setStats({
        totalProducts,
        lowStock,
        totalUsers: usersData.length,
        totalOrders: ordersData.length,
      });

      // Recent Products
      const recent = productsData
        .sort((a, b) => b.productId - a.productId)
        .slice(0, 5);

      setRecentProducts(recent);
    } catch (err) {
      console.error(err);
      toast.error("Failed to fetch dashboard data");
    }
  };

  useEffect(() => {
    fetchDashboardData();
  }, []);

  return (
    <AdminLayout>
      <Toaster position="top-right" />

      <div className="dashboard-container">
        {/* ---------------- Stats Cards ---------------- */}
        <div className="stats-grid">
          <div className="stat-card">
            <h2>{stats.totalProducts}</h2>
            <p>Total Products</p>
          </div>

          <div className="stat-card">
            <h2>{stats.lowStock}</h2>
            <p>Low Stock</p>
          </div>

          <div className="stat-card">
            <h2>{stats.totalUsers}</h2>
            <p>Total Users</p>
          </div>

          <div className="stat-card">
            <h2>{stats.totalOrders}</h2>
            <p>Total Orders</p>
          </div>
        </div>

        {/* ---------------- Recent Products ---------------- */}
        <div className="recent-products">
          <div className="recent-header">
            <h3>Recent Products</h3>
            <button
              className="add-btn"
              onClick={() => toast.success("Open Add Product Modal")}
            >
              + Add Product
            </button>
          </div>

          <div className="table-wrapper">
            <table>
              <thead>
                <tr>
                  <th>#</th>
                  <th>Name</th>
                  <th>Category</th>
                  <th>Price</th>
                  <th>Qty</th>
                  <th>Actions</th>
                </tr>
              </thead>

              <tbody>
                {recentProducts.map((p, index) => (
                  <tr key={p.productId}>
                    <td>{index + 1}</td>
                    <td>{p.name}</td>
                    <td>{p.category}</td>
                    <td>R {p.price.toFixed(2)}</td>
                    <td>{p.quantity}</td>
                    <td className="action-buttons">
                      <button
                        className="edit-btn"
                        onClick={() => toast("Open Edit Modal")}
                      >
                        Edit
                      </button>

                      <button
                        className="delete-btn"
                        onClick={() => toast.error("Deleted Product")}
                      >
                        Delete
                      </button>
                    </td>
                  </tr>
                ))}
              </tbody>

            </table>
          </div>
        </div>
      </div>
    </AdminLayout>
  );
}
