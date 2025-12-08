import React, { useEffect, useState } from "react";
import AdminLayout from "../AdminLayout";
import ManageProducts from "../ManageProducts/ManageProducts"; // popup modal will come from here
import toast, { Toaster } from "react-hot-toast"; // make sure to install react-hot-toast

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
      // Total Products & Low Stock
      const resProducts = await fetch(`${API_URL}/product`, {
        headers: { Authorization: `Bearer ${token}` },
      });
      const productsData = await resProducts.json();

      const totalProducts = productsData.length;
      const lowStock = productsData.filter(p => p.quantity <= 5).length;

      // Total Users (if you have user endpoint)
      const resUsers = await fetch(`${API_URL}/user`, {
        headers: { Authorization: `Bearer ${token}` },
      });
      const usersData = await resUsers.json();
      const totalUsers = usersData.length;

      // Total Orders (if implemented)
      const resOrders = await fetch(`${API_URL}/order`, {
        headers: { Authorization: `Bearer ${token}` },
      });
      const ordersData = await resOrders.json();
      const totalOrders = ordersData.length;

      setStats({ totalProducts, lowStock, totalUsers, totalOrders });

      // Recent Products (last 5)
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
      <div className="dashboard-container p-6 space-y-8">

        {/* ---------------- Stats Cards ---------------- */}
        <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-6">
          <div className="bg-white/10 backdrop-blur-md rounded-xl p-5 text-center text-white shadow-lg">
            <h2 className="text-2xl font-bold">{stats.totalProducts}</h2>
            <p>Total Products</p>
          </div>
          <div className="bg-white/10 backdrop-blur-md rounded-xl p-5 text-center text-white shadow-lg">
            <h2 className="text-2xl font-bold">{stats.lowStock}</h2>
            <p>Low Stock</p>
          </div>
          <div className="bg-white/10 backdrop-blur-md rounded-xl p-5 text-center text-white shadow-lg">
            <h2 className="text-2xl font-bold">{stats.totalUsers}</h2>
            <p>Total Users</p>
          </div>
          <div className="bg-white/10 backdrop-blur-md rounded-xl p-5 text-center text-white shadow-lg">
            <h2 className="text-2xl font-bold">{stats.totalOrders}</h2>
            <p>Total Orders</p>
          </div>
        </div>

        {/* ---------------- Recent Products ---------------- */}
        <div className="recent-products bg-white/10 backdrop-blur-md p-6 rounded-xl shadow-lg text-white">
          <div className="flex justify-between items-center mb-4">
            <h3 className="text-xl font-semibold">Recent Products</h3>
            <button
              className="bg-blue-500 hover:bg-blue-600 px-4 py-2 rounded-lg text-white font-semibold"
              onClick={() => toast.success("Open Add Product Modal")}
            >
              + Add Product
            </button>
          </div>

          <div className="overflow-x-auto">
            <table className="min-w-full text-left border-collapse">
              <thead>
                <tr>
                  <th className="px-4 py-2 border-b border-white/30">#</th>
                  <th className="px-4 py-2 border-b border-white/30">Name</th>
                  <th className="px-4 py-2 border-b border-white/30">Category</th>
                  <th className="px-4 py-2 border-b border-white/30">Price</th>
                  <th className="px-4 py-2 border-b border-white/30">Qty</th>
                  <th className="px-4 py-2 border-b border-white/30">Actions</th>
                </tr>
              </thead>
              <tbody>
                {recentProducts.map((p, index) => (
                  <tr key={p.productId} className="hover:bg-white/10">
                    <td className="px-4 py-2">{index + 1}</td>
                    <td className="px-4 py-2">{p.name}</td>
                    <td className="px-4 py-2">{p.category}</td>
                    <td className="px-4 py-2">R {p.price.toFixed(2)}</td>
                    <td className="px-4 py-2">{p.quantity}</td>
                    <td className="px-4 py-2 space-x-2">
                      <button
                        className="bg-yellow-500 hover:bg-yellow-600 px-3 py-1 rounded"
                        onClick={() => toast("Open Edit Modal")}
                      >
                        Edit
                      </button>
                      <button
                        className="bg-red-500 hover:bg-red-600 px-3 py-1 rounded"
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
