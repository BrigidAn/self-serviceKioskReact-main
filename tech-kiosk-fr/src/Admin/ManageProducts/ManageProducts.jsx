import React, { useState, useEffect, useRef } from "react";
import AdminLayout from "../AdminLayout";
import { toast, ToastContainer } from "react-toastify";
import "react-toastify/dist/ReactToastify.css";
import "./ManageProducts.css";

const API_URL = "https://localhost:5016/api/product";

export default function ManageProducts() {
  const [products, setProducts] = useState([]);
  const [form, setForm] = useState({
    name: "",
    description: "",
    price: "",
    category: "",
    quantity: 0,
    imageFile: null,
    imageUrl: "",
  });
  const [isEditing, setIsEditing] = useState(false);
  const [editId, setEditId] = useState(null);
  const [showForm, setShowForm] = useState(false);
  const [imagePreview, setImagePreview] = useState(null);
  const fileInputRef = useRef(null);
  const token = localStorage.getItem("token");

  const fetchProducts = async () => {
    try {
      const res = await fetch(API_URL, {
        headers: { Authorization: `Bearer ${token}` },
      });
      const data = await res.json();
      setProducts(data.sort((a, b) => a.productId - b.productId));
    } catch (err) {
      console.error(err);
      toast.error("Failed to fetch products");
    }
  };

  useEffect(() => {
    fetchProducts();
  }, []);

  const handleChange = (e) => {
    const { name, value } = e.target;
    setForm({ ...form, [name]: value });
  };

  // Handle image drag/drop
  const handleDrop = (e) => {
    e.preventDefault();
    const file = e.dataTransfer.files[0];
    previewFile(file);
  };

  const handleFileChange = (e) => {
    const file = e.target.files[0];
    previewFile(file);
  };

  const previewFile = (file) => {
    if (!file) return;
    setForm({ ...form, imageFile: file });
    const reader = new FileReader();
    reader.onloadend = () => setImagePreview(reader.result);
    reader.readAsDataURL(file);
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    if (!form.name || !form.price || !form.category) {
      toast.error("Please fill in all required fields");
      return;
    }

    try {
      const formData = new FormData();
      formData.append("name", form.name);
      formData.append("description", form.description);
      formData.append("price", form.price);
      formData.append("category", form.category);
      formData.append("quantity", form.quantity);
      if (form.imageFile) formData.append("image", form.imageFile);
      else if (form.imageUrl) formData.append("imageUrl", form.imageUrl);

      const url = isEditing ? `${API_URL}/${editId}` : API_URL;
      const method = isEditing ? "PUT" : "POST";

      const res = await fetch(url, {
        method,
        headers: { Authorization: `Bearer ${token}` },
        body: formData,
      });

      const data = await res.json();
      if (!res.ok) throw new Error(data.message || "Error saving product");

      toast.success(isEditing ? "Product updated!" : "Product added!");
      fetchProducts();
      resetForm();
    } catch (err) {
      console.error(err);
      toast.error(err.message);
    }
  };

  const handleEdit = (p) => {
    setForm({
      name: p.name,
      description: p.description,
      price: p.price,
      category: p.category,
      quantity: p.quantity,
      imageFile: null,
      imageUrl: p.imageUrl || "",
    });
    setImagePreview(p.imageUrl || null);
    setEditId(p.productId);
    setIsEditing(true);
    setShowForm(true);
  };

  const handleDelete = async (id) => {
    if (!window.confirm("Are you sure you want to delete this product?")) return;
    try {
      const res = await fetch(`${API_URL}/${id}`, {
        method: "DELETE",
        headers: { Authorization: `Bearer ${token}` },
      });
      const data = await res.json();
      if (!res.ok) throw new Error(data.message || "Failed to delete");
      toast.success("Product deleted!");
      fetchProducts();
    } catch (err) {
      console.error(err);
      toast.error(err.message);
    }
  };

  const resetForm = () => {
    setForm({
      name: "",
      description: "",
      price: "",
      category: "",
      quantity: 0,
      imageFile: null,
      imageUrl: "",
    });
    setImagePreview(null);
    setIsEditing(false);
    setEditId(null);
    setShowForm(false);
  };

  return (
    <AdminLayout>
      <div className="manage-products-header">
        <h1>Manage Products</h1>
        <button className="add-product-btn" onClick={() => setShowForm(true)}>
          + Add Product
        </button>
      </div>

      {/* Modal */}
      {showForm && (
        <div className="product-modal">
          <div className="product-modal-content">
            <h2>{isEditing ? "Edit Product" : "Add Product"}</h2>
            <input
              type="text"
              name="name"
              placeholder="Name"
              value={form.name}
              onChange={handleChange}
            />
            <textarea
              name="description"
              placeholder="Description"
              value={form.description}
              onChange={handleChange}
            />
            <input
              type="number"
              name="price"
              placeholder="Price"
              value={form.price}
              onChange={handleChange}
            />
            <input
              type="text"
              name="category"
              placeholder="Category"
              value={form.category}
              onChange={handleChange}
            />
            <input
              type="number"
              name="quantity"
              placeholder="Quantity"
              value={form.quantity}
              onChange={handleChange}
            />

            {/* Drag & Drop */}
            <div
              className="image-drop-zone"
              onDrop={handleDrop}
              onDragOver={(e) => e.preventDefault()}
              onClick={() => fileInputRef.current.click()}
            >
              {imagePreview ? (
                <img src={imagePreview} alt="Preview" className="image-preview" />
              ) : (
                <p>Drag & drop image here or click to upload</p>
              )}
              <input
                type="file"
                ref={fileInputRef}
                style={{ display: "none" }}
                accept="image/*"
                onChange={handleFileChange}
              />
            </div>

            <div className="modal-actions">
              <button onClick={handleSubmit}>
                {isEditing ? "Update Product" : "Add Product"}
              </button>
              <button className="cancel-btn" onClick={resetForm}>
                Cancel
              </button>
            </div>
          </div>
        </div>
      )}

      {/* Product Table */}
      <table className="product-table">
        <thead>
          <tr>
            <th>#</th>
            <th>Image</th>
            <th>Name</th>
            <th>Category</th>
            <th>Price (R)</th>
            <th>Quantity</th>
            <th>Actions</th>
          </tr>
        </thead>
        <tbody>
          {products.map((p, idx) => (
            <tr key={p.productId}>
              <td>{idx + 1}</td>
              <td>
                <img
                  src={p.imageUrl || "https://via.placeholder.com/50"}
                  alt={p.name}
                  className="table-image"
                />
              </td>
              <td>{p.name}</td>
              <td>{p.category}</td>
              <td>{p.price.toFixed(2)}</td>
              <td>{p.quantity}</td>
              <td>
                <button onClick={() => handleEdit(p)}>Edit</button>
                <button
                  className="delete-btn"
                  onClick={() => handleDelete(p.productId)}
                >
                  Delete
                </button>
              </td>
            </tr>
          ))}
        </tbody>
      </table>

      <ToastContainer position="top-right" autoClose={2000} />
    </AdminLayout>
  );
}
