import React from "react";

function AdminNavbar() {
  return (
    <div style={{
      background: "#333",
      padding: "14px",
      color: "white",
      display: "flex",
      justifyContent: "space-between"
    }}>
      <h3>Admin Dashboard</h3>

      <nav>
        <a href="/admin/products" style={{ color: "#fff", marginRight: 15 }}>Products</a>
        <a href="/admin/add-product" style={{ color: "#fff" }}>Add Product</a>
      </nav>
    </div>
  );
}

export default AdminNavbar;
