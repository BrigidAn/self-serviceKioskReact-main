import React, { useEffect, useState, useRef } from "react";
import AdminLayout from "../AdminLayout";
import "./ManageProducts.css";
import { ToastContainer, toast } from "react-toastify";
import "react-toastify/dist/ReactToastify.css";
import DeleteConfirmPopup from "../../components/DeleteConfirmPopup";

const API_URL = "https://localhost:5016/api";

export default function ManageProducts() {
  const [stats, setStats] = useState({ totalProducts: 0, lowStock: 0 });
  const [products, setProducts] = useState([]);
  const [filteredProducts, setFilteredProducts] = useState([]);
  const [loading, setLoading] = useState(true);
  const [errors, setErrors] = useState({});
  const [currentPage, setCurrentPage] = useState(1);
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

  const [popup, setPopup] = useState({ show: false, productId: null, productName: "", onConfirm: null });
  const [filterLowStock, setFilterLowStock] = useState(false);
const PAGE_SIZE = 10;

  const fetchDashboardData = async () => {
    setLoading(true);
    try {
      const res = await fetch(`${API_URL}/product`, { headers: token ? { Authorization: `Bearer ${token}` } : {} });
      if (!res.ok) throw new Error("Failed to fetch products");
      const data = await res.json();
      const productList = Array.isArray(data) ? data : data?.data ?? [];
      productList.sort((a, b) => (a.productId ?? a.id ?? 0) - (b.productId ?? b.id ?? 0));
      setProducts(productList);

      const lowStockCount = productList.filter((p) => Number(p.quantity) <= 5).length;
      setStats({ totalProducts: productList.length, lowStock: lowStockCount });

      setFilteredProducts(filterLowStock ? productList.filter((p) => Number(p.quantity) <= 5) : productList);
    } catch (err) {
      console.error(err);
      toast.error(err.message || "Failed to fetch products");
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchDashboardData();
  }, [filterLowStock]);

  const toggleLowStockFilter = () => setFilterLowStock((prev) => !prev);

  const validateForm = () => {
  const newErrors = {};

  if (!form.name.trim()) newErrors.name = "Product name is required";
  if (!form.description.trim()) newErrors.description = "Description is required";

  if (!form.price) newErrors.price = "Price is required";
  else if (form.price <= 0) newErrors.price = "Price must be greater than 0";

  if (!form.quantity) newErrors.quantity = "Quantity is required";
  else if (form.quantity < 0) newErrors.quantity = "Quantity cannot be negative";

  if (!form.category || form.category === "")
    newErrors.category = "Please select a category";

  if (!form.supplierId)
    newErrors.supplierId = "Please select a supplier";

  if (!isEditing && !fileRef.current?.files?.length)
    newErrors.image = "Product image is required";

  setErrors(newErrors);
  return Object.keys(newErrors).length === 0; 
};

  const openAdd = () => {
    setIsEditing(false);
    setEditingId(null);
    setForm({ name: "", description: "", price: "", category: "", quantity: 0, supplierId: 1, file: null });
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


    if (!validateForm()) {
      return;
    }

    if (!form.file && !isEditing) return toast.error("Please select a product image");

    try {
      const fd = new FormData();
      fd.append("Name", form.name);
      fd.append("Description", form.description || "");
      fd.append("Price", String(form.price));
      fd.append("Category", form.category);
      fd.append("Quantity", String(form.quantity ?? 0));
      fd.append("SupplierId", String(form.supplierId ?? 1));
      if (form.file) fd.append("File", form.file);

      const url = isEditing && editingId ? `${API_URL}/product/${editingId}` : `${API_URL}/product`;
      const method = isEditing ? "PUT" : "POST";

      const res = await fetch(url, { method, headers: token ? { Authorization: `Bearer ${token}` } : {}, body: fd });
      if (!res.ok) {
        const data = await res.json().catch(() => ({}));
        throw new Error(data?.message || "Failed to save product");
      }

      toast.success(isEditing ? "Product updated" : "Product added");
      setShowModal(false);
      setForm({ name: "", description: "", price: "", category: "", quantity: 0, supplierId: 1, file: null });
      setImagePreview(null);
      fetchDashboardData();
    } catch (err) {
      console.error(err);
      toast.error(err.message || "Save failed");
    }
  };

 const requestDelete = (productId, productName) => {
  if (!productId) return toast.error("Invalid product");
  setPopup({
    show: true,
    productId,
    productName,
    onConfirm: () => handleConfirmDelete(productId, productName)
  });
};


const handleConfirmDelete = async (productId, productName) => {
  try {
    const res = await fetch(`${API_URL}/product/${productId}`, {
      method: "DELETE",
      headers: token ? { Authorization: `Bearer ${token}` } : {}
    });

    if (!res.ok) {
      const data = await res.json().catch(() => ({}));
      throw new Error(data?.message || "Delete failed");
    }

    toast.success(`Product "${productName}" deleted successfully`);
    setPopup({ show: false, productId: null, productName: "", onConfirm: null });
    fetchDashboardData();
  } catch (err) {
    console.error(err);
    toast.error(err.message || "Delete failed");
  }
};

const totalPages = Math.ceil(filteredProducts.length / PAGE_SIZE);

const paginatedProducts = filteredProducts.slice(
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
      <div className="admin-dashboard">
        <header className="dashboard-header">
          <h1>Manage Products</h1>
          <div className="header-actions">
            <button className="btn primary" onClick={openAdd}>+ Add Product</button>
            <button className="btn" onClick={fetchDashboardData}>Refresh</button>
          </div>
        </header>

        <section className="stats-grid">
          <div className="stat-card">
            <div className="stat-num">{loading ? "—" : stats.totalProducts}</div>
            <div className="stat-label">Total Products</div>
          </div>
          <div className={`stat-card clickable ${filterLowStock ? "active" : ""}`} onClick={toggleLowStockFilter}>
            <div className="stat-num">{loading ? "—" : stats.lowStock}</div>
            <div className="stat-label">Low Stock (≤5)</div>
          </div>
        </section>

        <section className="product-table-section">
          <table className="product-table">
            <thead>
              <tr>
                <th>Id</th>
                <th>Image</th>
                <th>Name</th>
                <th>Category</th>
                <th>Price</th>
                <th>Quantity</th>
                <th>Supplier</th>
                <th>Actions</th>
              </tr>
            </thead>
            <tbody>
              {paginatedProducts.length === 0 ? (
                <tr>
                  <td colSpan={8} className="empty">No products found</td>
                </tr>
              ) : (
              paginatedProducts.map((p, idx) => (
                <tr key={p.productId ?? p.id ?? idx}>
                  <td>{(currentPage - 1) * PAGE_SIZE + idx + 1}</td>
                  <td><img src={p.imageUrl || p.image || "/placeholder.png"} alt={p.name} /></td>
                  <td>{p.name}</td>
                  <td>{p.category}</td>
                  <td>R {Number(p.price).toFixed(2)}</td>
                  <td>{p.quantity ?? 0}</td>
                  <td>{p.supplierId}</td>
                  <td>
                    <button className="btn small" onClick={() => openEdit(p)}>Edit</button>
                    <button className="btn danger small" onClick={() => requestDelete(p.productId ?? p.id, p.name)}>Delete</button>
                  </td>
                </tr>
                ))
              )}
            </tbody>
          </table>
        </section>

                 {totalPages > 1 && (
              <div className="p-pagination">
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
                <input
                  name="name"
                  value={form.name}
                  onChange={handleFormChange}
                />
                {errors.name && <span className="error">{errors.name}</span>}
              </label>
 
              <label>
                Description
                <textarea
                  name="description"
                  value={form.description}
                  onChange={handleFormChange}
                />
                {errors.description && <span className="error">{errors.description}</span>}
              </label>

              <label>
                Price
                <input
                  name="price"
                  type="number"
                  step="0.01"
                  value={form.price}
                  onChange={handleFormChange}
                />
                {errors.price && <span className="error">{errors.price}</span>}
              </label>

              <label>
                Quantity
                <input
                  name="quantity"
                  type="number"
                  value={form.quantity}
                  onChange={handleFormChange}
                />
                {errors.quantity && <span className="error">{errors.quantity}</span>}
              </label>
              <label>
                Category
                <select
                  className="c-form-select"
                  name="category"
                  value={form.category}
                  onChange={(e) => setForm({ ...form, category: e.target.value })}
                >
                  <option value="">Select Category</option>
                  <option value="Accessories">Accessories</option>
                  <option value="VR">Virtual Reality</option>
                  <option value="Robots">Robots</option>
                  <option value="Headsets">Headsets</option>
                  <option value="Earbuds">Earbuds</option>
                </select>
                {errors.category && <span className="error">{errors.category}</span>}
              </label>
              <label>
                Supplier
                <select
                  className="c-form-select"
                  name="supplierId"
                  value={form.supplierId}
                  onChange={(e) =>
                    setForm({ ...form, supplierId: Number(e.target.value) })
                  }
                >
                  <option value={1}>Becy</option>
                  <option value={2}>Stacy</option>
                  <option value={3}>Garfield</option>
                  <option value={4}>Marie</option>
                  <option value={5}>Ebuka</option>
                </select>
                {errors.supplierId && <span className="error">{errors.supplierId}</span>}
              </label>

              <label>
                Product Image
                <input
                  ref={fileRef}
                  type="file"
                  accept="image/*"
                  onChange={handleFileSelect}
                />
                {errors.image && <span className="error">{errors.image}</span>}
              </label>

              {imagePreview && (
                <div className="preview-wrap">
                  <img src={imagePreview} alt="preview" />
                </div>
              )}

              <div className="modal-actions">
                <button type="submit" className="btn primary">
                  {isEditing ? "Update" : "Add Product"}
                </button>
                <button type="button" className="btn" onClick={() => setShowModal(false)}>
                  Cancel
                </button>
              </div>
              </form>

          </div>
        </div>
      )}

       <DeleteConfirmPopup
       show={popup.show}
       message={`Are you sure you want to delete "${popup.productName}"?`}
       onDelete={popup.onConfirm} onCancel={() =>
        setPopup({
          show: false, productId: null, productName: "", onConfirm: null }) }
          />
          </div>
          </AdminLayout>
        );
      }