// AdminLayout.jsx
import React from "react";
import "./AdminLayout.css";
import Sidebar from "../components/AdminSidebar/Sidebar";

function AdminLayout({ children }) {
  return (
    <div className="admin-wrapper">
      {/* Use only the new collapsible Sidebar */}
      <Sidebar />

      {/* Main content */}
      <main className="admin-main">
        {children}
      </main>
    </div>
  );
}

export default AdminLayout;
