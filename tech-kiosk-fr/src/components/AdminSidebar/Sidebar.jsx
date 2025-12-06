import React, { useState } from "react";
import { NavLink } from "react-router-dom";
import { FaUsers, FaBoxOpen, FaHome, FaBars, FaClipboardList } from "react-icons/fa";
import "./Sidebar.css";

export default function Sidebar() {
  const [collapsed, setCollapsed] = useState(false);

  const menuItems = [
    { name: "Dashboard", icon: <FaHome />, path: "/admin/dashboard" },
    { name: "Users", icon: <FaUsers />, path: "/admin/users" },
    { name: "Products", icon: <FaBoxOpen />, path: "/admin/products" },
    { name: "Logs", icon: <FaClipboardList />, path: "/admin/logs" },
  ];

  return (
    <aside className={`sidebar ${collapsed ? "collapsed" : ""}`}>
      <div className="sidebar-header">
        <h2 className={`logo ${collapsed ? "hidden" : ""}`}>My Admin</h2>
        <button className="collapse-btn" onClick={() => setCollapsed(!collapsed)}>
          <FaBars />
        </button>
      </div>

      <nav className="sidebar-nav">
        {menuItems.map((item, index) => (
          <NavLink
            key={index}
            to={item.path}
            className="nav-item"
            title={collapsed ? item.name : ""}
          >
            <span className="icon">{item.icon}</span>
            {!collapsed && <span className="text">{item.name}</span>}
          </NavLink>
        ))}
      </nav>
    </aside>
  );
}