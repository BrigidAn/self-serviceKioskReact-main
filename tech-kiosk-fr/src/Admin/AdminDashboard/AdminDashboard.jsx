import React from "react";
import AdminLayout from "../AdminLayout";
import "./AdminDashboard.css"


function AdminDashboard() {
  const users = JSON.parse(localStorage.getItem("users") || "[]");
  const products = JSON.parse(localStorage.getItem("products") || "[]");
  const logs = JSON.parse(localStorage.getItem("transactions") || "[]");

  return (
    <AdminLayout>
      <h1>Dashboard</h1>

      <div className="admin-stats">
        <div className="stat-card">
          <h3>Total Users</h3>
          <p>{users.length}</p>
        </div>

        <div className="stat-card">
          <h3>Total Products</h3>
          <p>{products.length}</p>
        </div>

        <div className="stat-card">
          <h3>Transactions</h3>
          <p>{logs.length}</p>
        </div>
      </div>
    </AdminLayout>
  );
}

export default AdminDashboard;