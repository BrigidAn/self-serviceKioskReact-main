import React, { useEffect, useState } from "react";
import AdminLayout from "../AdminLayout";
import { ToastContainer, toast } from "react-toastify";
import "react-toastify/dist/ReactToastify.css";
import "./AdminOrders.css";

const API_URL = "https://localhost:5016/api";
const PAGE_SIZE = 12;

export default function AdminOrders() {
  const [orders, setOrders] = useState([]);
  const [products, setProducts] = useState([]);
  const [loading, setLoading] = useState(true);
  const [filterStatus, setFilterStatus] = useState("All");
  const [currentPage, setCurrentPage] = useState(1);
  const [searchTerm, setSearchTerm] = useState("");

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

      const res = await fetch(`${API_URL}/Admin/orders?page=1&pageSize=100`, {
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
  }, []);

  const handleStatusChange = async (orderId, newStatus) => {
    setOrders((prev) =>
      prev.map((o) => (o.orderId === orderId ? { ...o, status: newStatus } : o))
    );
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
      toast.error(err.message || "Status update failed. Reverting back ...");
    }
  };

  const getProductName = (productId) => {
    const prod = products.find((p) => p.productId === productId);
    return prod?.name || `Unknown Product (#${productId})`;
  };

const filteredOrders = orders
  .filter((o) => {
    // Exclude orders with no items or items not in products
    if (!o.items || o.items.length === 0) return false;
    const allItemsExist = o.items.every((i) =>
      products.some((p) => p.productId === i.productId)
    );
    if (!allItemsExist) return false;

    // Filter by status
    if (filterStatus !== "All" && o.status !== filterStatus) return false;

    // Filter by search term
    if (!searchTerm) return true;
    const term = searchTerm.toLowerCase();

    if (o.orderId.toString().includes(term)) return true;
    if ((o.customer || "").toLowerCase().includes(term)) return true;
    if (
      o.items.some((i) =>
        getProductName(i.productId).toLowerCase().includes(term)
      )
    )
      return true;

    return false;
  });


  const totalPages = Math.ceil(filteredOrders.length / PAGE_SIZE);
  const paginatedOrders = filteredOrders.slice(
    (currentPage - 1) * PAGE_SIZE,
    currentPage * PAGE_SIZE
  );

  const handlePageChange = (page) => {
    if (page < 1 || page > totalPages) return;
    setCurrentPage(page);
  };

  return (
    <AdminLayout>
      <ToastContainer position="top-right" />
      <div className="admin-orders">
        <header className="dashboard-header">
          <h1>All Orders</h1>
          <div className="controls">
            {/* Filter */}
            <label className="o-filter">
              Filter
              <select
                value={filterStatus}
                onChange={(e) => {
                  setFilterStatus(e.target.value);
                  setCurrentPage(1);
                }}
              >
                <option value="All">All</option>
                <option value="Pending">Pending</option>
                <option value="Completed">Completed</option>
              </select>
            </label>

            <input
              type="text"
              className="o-search-input"
              placeholder="Search by user, order ID, or product..."
              value={searchTerm}
              onChange={(e) => {
                setSearchTerm(e.target.value);
                setCurrentPage(1);
              }}
            />
          </div>
        </header>

        {loading ? (
          <p>Loading orders...</p>
        ) : filteredOrders.length === 0 ? (
          <p>No orders found</p>
        ) : (
          <>
            <table className="orders-table">
              <thead>
                <tr>
                  <th>#</th>
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
              {paginatedOrders.map((order, idx) => (
                <tr key={order.orderId}>
                  <td>{(currentPage - 1) * PAGE_SIZE + idx + 1}</td>
                  <td>{order.orderId}</td>
                  <td>{order.customer}</td>
                  <td>
                    {order.items?.length > 0
                      ? order.items.map((i, index) => (
                          <div key={index}>
                            {getProductName(i.productId)} x{i.quantity} @
                            R{i.priceAtPurchase.toFixed(2)}
                          </div>
                        ))
                      : "No products"}
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

              {/* Placeholder rows to maintain table height */}
              {paginatedOrders.length < PAGE_SIZE &&
                Array.from({ length: PAGE_SIZE - paginatedOrders.length }).map((_, idx) => (
                  <tr key={`placeholder-${idx}`} className="placeholder-row">
                    {Array.from({ length: 8 }).map((_, col) => (
                      <td key={col}>&nbsp;</td>
                    ))}
                  </tr>
                ))}
            </tbody>
            </table>
            {totalPages > 1 && (
              <div className="o-pagination">
                <button onClick={() => handlePageChange(currentPage - 1)}>Prev</button>
                {[...Array(totalPages)].map((_, i) => (
                  <button
                    key={i}
                    className={currentPage === i + 1 ? "active" : ""}
                    onClick={() => handlePageChange(i + 1)}
                  >
                    {i + 1}
                  </button>
                ))}
                <button onClick={() => handlePageChange(currentPage + 1)}>Next</button>
              </div>
            )}
          </>
        )}
      </div>
    </AdminLayout>
  );
}
