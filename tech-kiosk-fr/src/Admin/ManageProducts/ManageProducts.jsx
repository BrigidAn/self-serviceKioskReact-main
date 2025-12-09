import React, { useEffect, useState, useRef } from "react";
import AdminLayout from "../AdminLayout";
import "./ManageProducts.css";
import { ToastContainer, toast } from "react-toastify";
import "react-toastify/dist/ReactToastify.css";

const API_URL = "https://localhost:5016/api";

export default function AdminDashboard() {
  const [stats, setStats] = useState({
    totalProducts: 0,
    lowStock: 0,
    totalUsers: 0,
    totalOrders: 0,
  });
  const [products, setProducts] = useState([]);
  const [loading, setLoading] = useState(true);

  // Modal / form state
  const [showModal, setShowModal] = useState(false);
  const [isEditing, setIsEditing] = useState(false);
  const [editingId, setEditingId] = useState(null);
  const [form, setForm] = useState({
    name: "",
    description: "",
    price: "",
    category: "",
    quantity: 0,
    supplierId: 1,
    file: null,
  });
  const [imagePreview, setImagePreview] = useState(null);
  const fileRef = useRef(null);

  const token = localStorage.getItem("token");

  const fetchDashboardData = async () => {
    setLoading(true);
    try {
      const resProducts = await fetch(`${API_URL}/product`, {
        headers: token ? { Authorization: `Bearer ${token}` } : {},
      });
      if (!resProducts.ok) throw new Error("Failed to load products");
      const data = await resProducts.json();
      const productList = Array.isArray(data) ? data : data?.data ?? [];
      productList.sort((a, b) => (a.productId ?? a.id ?? 0) - (b.productId ?? b.id ?? 0));
      setProducts(productList);

      // Stats
      const totalProducts = productList.length;
      const lowStock = productList.filter((p) => Number(p.quantity) <= 5).length;
      setStats((prev) => ({ ...prev, totalProducts, lowStock }));
    } catch (err) {
      console.error(err);
      toast.error("Failed to fetch products");
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchDashboardData();
    // eslint-disable-next-line
  }, []);

  const openAdd = () => {
    setIsEditing(false);
    setEditingId(null);
    setForm({
      name: "",
      description: "",
      price: "",
      category: "",
      quantity: 0,
      supplierId: 1,
      file: null,
    });
    setImagePreview(null);
    setShowModal(true);
  };

  const openEdit = (p) => {
    setIsEditing(true);
    setEditingId(p.productId ?? p.id);
    setForm({
      name: p.name || "",
      description: p.description || "",
      price: p.price ?? "",
      category: p.category || "",
      quantity: p.quantity ?? 0,
      supplierId: p.supplierId ?? 1,
      file: null,
    });
    setImagePreview(p.imageUrl || p.image || null);
    setShowModal(true);
  };

  const handleFormChange = (e) => {
    const { name, value } = e.target;
    setForm((f) => ({ ...f, [name]: value }));
  };

  const handleFileSelect = (e) => {
    const file = e.target.files[0];
    if (!file) return;
    setForm((f) => ({ ...f, file }));
    const reader = new FileReader();
    reader.onloadend = () => setImagePreview(reader.result);
    reader.readAsDataURL(file);
  };

  const handleSubmit = async (e) => {
    e.preventDefault();

    if (!form.name || !form.price || !form.category) {
      toast.error("Please fill name, price and category");
      return;
    }

    if (!form.file && !isEditing) {
      toast.error("Please select a product image");
      return;
    }

    try {
      const fd = new FormData();
      fd.append("Name", form.name);
      fd.append("Description", form.description || "");
      fd.append("Price", String(form.price));
      fd.append("Category", form.category);
      fd.append("Quantity", String(form.quantity ?? 0));
      fd.append("SupplierId", String(form.supplierId ?? 1));

      if (form.file) fd.append("File", form.file); // <-- must match DTO exactly

      const url = isEditing && editingId ? `${API_URL}/product/${editingId}` : `${API_URL}/product`;
      const method = isEditing ? "PUT" : "POST";

      const res = await fetch(url, {
        method,
        headers: token ? { Authorization: `Bearer ${token}` } : {},
        body: fd,
      });

      if (!res.ok) {
        const data = await res.json().catch(() => ({}));
        throw new Error(data?.message || "Failed to save product");
      }

      toast.success(isEditing ? "Product updated" : "Product added");
      setShowModal(false);
      setForm({
        name: "",
        description: "",
        price: "",
        category: "",
        quantity: 0,
        supplierId: 1,
        file: null,
      });
      setImagePreview(null);
      fetchDashboardData();
    } catch (err) {
      console.error(err);
      toast.error(err.message || "Save failed");
    }
  };

  const handleDelete = async (productId) => {
    if (!window.confirm("Delete this product?")) return;
    try {
      const res = await fetch(`${API_URL}/product/${productId}`, {
        method: "DELETE",
        headers: token ? { Authorization: `Bearer ${token}` } : {},
      });
      if (!res.ok) throw new Error("Delete failed");
      toast.success("Product deleted");
      fetchDashboardData();
    } catch (err) {
      console.error(err);
      toast.error(err.message || "Delete failed");
    }
  };

  return (
    <AdminLayout>
      <ToastContainer position="top-right" />
      <div className="admin-dashboard">
        {/* HEADER */}
        <header className="dashboard-header">
          <h1>Admin Dashboard</h1>
          <div className="header-actions">
            <button className="btn primary" onClick={openAdd}>+ Add Product</button>
            <button className="btn" onClick={fetchDashboardData}>Refresh</button>
          </div>
        </header>

        {/* STATS */}
        <section className="stats-grid">
          <div className="stat-card">
            <div className="stat-num">{loading ? "—" : stats.totalProducts}</div>
            <div className="stat-label">Total Products</div>
          </div>
          <div className="stat-card">
            <div className="stat-num">{loading ? "—" : stats.lowStock}</div>
            <div className="stat-label">Low Stock (&le; 5)</div>
          </div>
        </section>

        {/* CARDS */}
        <section className="recent-products-section">
          <div className="card-grid">
            {products.length === 0 ? (
              <div className="empty">No products found</div>
            ) : (
              products.map((p, idx) => (
                <div key={p.productId ?? p.id ?? idx} className="product-card">
                  <div className="product-media">
                    <img
                      src={p.imageUrl || p.image || "https://via.placeholder.com/240x160?text=No+Image"}
                      alt={p.name}
                      onError={(e) => (e.target.src = "https://via.placeholder.com/240x160?text=No+Image")}
                    />
                  </div>
                  <div className="product-body">
                    <div className="product-title">
                      <span className="index">{String((p.productId ?? p.id ?? 0)).padStart(3, "0")}</span>
                      <h3>{p.name}</h3>
                    </div>
                    <p className="product-cat">{p.category}</p>
                    <div className="product-meta">
                      <div>R {Number(p.price).toFixed(2)}</div>
                      <div>Qty: {p.quantity ?? 0}</div>
                    </div>
                    <div className="product-actions">
                      <button className="btn small" onClick={() => openEdit(p)}>Edit</button>
                      <button className="btn danger small" onClick={() => handleDelete(p.productId ?? p.id)}>Delete</button>
                    </div>
                  </div>
                </div>
              ))
            )}
          </div>
        </section>

        {/* TABLE */}
        <section className="product-table-section">
          <table className="product-table">
            <thead>
              <tr>
                <th>#</th>
                <th>Image</th>
                <th>Name</th>
                <th>Category</th>
                <th>Price (R)</th>
                <th>Quantity</th>
                <th>Supplier</th>
                <th>Actions</th>
              </tr>
            </thead>
            <tbody>
              {products.length === 0 ? (
                <tr>
                  <td colSpan={8} style={{ textAlign: "center" }}>No products found</td>
                </tr>
              ) : (
                products.map((p, idx) => (
                  <tr key={p.productId ?? p.id ?? idx}>
                    <td>{idx + 1}</td>
                    <td>
                      <img
                        src={p.imageUrl || p.image || "https://via.placeholder.com/50"}
                        alt={p.name}
                        style={{ width: "50px", height: "50px", objectFit: "cover" }}
                      />
                    </td>
                    <td>{p.name}</td>
                    <td>{p.category}</td>
                    <td>{Number(p.price).toFixed(2)}</td>
                    <td>{p.quantity ?? 0}</td>
                    <td>{p.supplierId}</td>
                    <td>
                      <button className="btn small" onClick={() => openEdit(p)}>Edit</button>
                      <button className="btn danger small" onClick={() => handleDelete(p.productId ?? p.id)}>Delete</button>
                    </td>
                  </tr>
                ))
              )}
            </tbody>
          </table>
        </section>

        {/* MODAL */}
        {showModal && (
          <div className="modal-overlay">
            <div className="modal">
              <header className="modal-header">
                <h3>{isEditing ? "Edit Product" : "Add Product"}</h3>
                <button className="close" onClick={() => setShowModal(false)}>✕</button>
              </header>

              <form className="modal-form" onSubmit={handleSubmit}>
                <label>
                  Name
                  <input name="name" value={form.name} onChange={handleFormChange} />
                </label>
                <label>
                  Description
                  <textarea name="description" value={form.description} onChange={handleFormChange} />
                </label>
                <div className="row">
                  <label>
                    Price
                    <input name="price" type="number" step="0.01" value={form.price} onChange={handleFormChange} />
                  </label>
                  <label>
                    Quantity
                    <input name="quantity" type="number" value={form.quantity} onChange={handleFormChange} />
                  </label>
                </div>
                <label>
                  Category
                  <input name="category" value={form.category} onChange={handleFormChange} />
                </label>
                <label>
                  Supplier
                  <select
                    name="supplierId"
                    value={form.supplierId}
                    onChange={(e) => setForm({ ...form, supplierId: Number(e.target.value) })}
                  >
                    <option value={1}>Becy</option>
                    <option value={2}>Stacy</option>
                    <option value={3}>Garfield</option>
                    <option value={4}>Marie</option>
                    <option value={5}>Ebuka</option>
                  </select>
                </label>
                <label className="file-label">
                  Product Image (optional)
                  <input ref={fileRef} type="file" accept="image/*" onChange={handleFileSelect} />
                </label>
                {imagePreview && (
                  <div className="preview-wrap">
                    <img src={imagePreview} alt="preview" />
                  </div>
                )}
                <div className="modal-actions">
                  <button type="submit" className="btn primary">{isEditing ? "Update" : "Add Product"}</button>
                  <button type="button" className="btn" onClick={() => setShowModal(false)}>Cancel</button>
                </div>
              </form>
            </div>
          </div>
        )}
      </div>
    </AdminLayout>
  );
}
