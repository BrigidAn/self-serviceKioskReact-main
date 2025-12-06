import React, { useState, useRef } from "react";
import AdminLayout from "../AdminLayout";
import "./ManageProducts.css";

export default function ManageProducts() {
  const [products, setProducts] = useState(
    JSON.parse(localStorage.getItem("products") || "[]")
  );

  const [form, setForm] = useState({
    id: null,
    name: "",
    description: "",
    price: "",
    category: "",
    image: null,
    quantity: "",
    supplierId: "",
  });

  const [isEditing, setIsEditing] = useState(false);
  const [showForm, setShowForm] = useState(false);
  const [imagePreview, setImagePreview] = useState(null);
  const fileInputRef = useRef(null);

  const updateLocal = (list) => {
    localStorage.setItem("products", JSON.stringify(list));
    setProducts(list);
  };

  const submitProduct = () => {
    if (!form.name || !form.price || !form.category || !form.image) {
      alert("Please fill all required fields!");
      return;
    }

    let updated;
    const imageUrl = imagePreview; // store base64 preview as "image URL"

    if (isEditing) {
      updated = products.map((p) =>
        p.id === form.id
          ? { ...form, price: Number(form.price), quantity: Number(form.quantity), imageUrl }
          : p
      );
      alert("Product updated!");
    } else {
      updated = [
        ...products,
        { ...form, id: Date.now(), price: Number(form.price), quantity: Number(form.quantity), imageUrl },
      ];
      alert("Product added!");
    }

    updateLocal(updated);
    resetForm();
    setShowForm(false);
  };

  const deleteProduct = (id) => {
    const confirmed = window.confirm("Delete this product?");
    if (!confirmed) return;

    const updated = products.filter((p) => p.id !== id);
    updateLocal(updated);
  };

  const beginEdit = (p) => {
    setForm(p);
    setImagePreview(p.imageUrl || null);
    setIsEditing(true);
    setShowForm(true);
  };

  const resetForm = () => {
    setForm({
      id: null,
      name: "",
      description: "",
      price: "",
      category: "",
      image: null,
      quantity: "",
      supplierId: "",
    });
    setImagePreview(null);
    setIsEditing(false);
  };

  // Drag & drop handlers
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
    setForm({ ...form, image: file });
    const reader = new FileReader();
    reader.onloadend = () => {
      setImagePreview(reader.result);
    };
    reader.readAsDataURL(file);
  };

  return (
    <AdminLayout>
      <div className="manage-products-header">
        <h1>Manage Products</h1>
        <button className="add-product-btn" onClick={() => setShowForm(true)}>
          + Add Product
        </button>
      </div>

      {/* Popup Form */}
      {showForm && (
        <div className="product-modal">
          <div className="product-modal-content">
            <h2>{isEditing ? "Edit Product" : "Add Product"}</h2>

            <input
              type="text"
              placeholder="Product Name"
              value={form.name}
              onChange={(e) => setForm({ ...form, name: e.target.value })}
            />
            <textarea
              placeholder="Description"
              value={form.description}
              onChange={(e) => setForm({ ...form, description: e.target.value })}
            />
            <input
              type="number"
              placeholder="Price"
              value={form.price}
              onChange={(e) => setForm({ ...form, price: e.target.value })}
            />
            <input
              type="text"
              placeholder="Category"
              value={form.category}
              onChange={(e) => setForm({ ...form, category: e.target.value })}
            />
            <input
              type="number"
              placeholder="Quantity"
              value={form.quantity}
              onChange={(e) => setForm({ ...form, quantity: e.target.value })}
            />
            <input
              type="text"
              placeholder="Supplier ID"
              value={form.supplierId}
              onChange={(e) => setForm({ ...form, supplierId: e.target.value })}
            />

            {/* Drag & Drop Image */}
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
              <button onClick={submitProduct}>
                {isEditing ? "Update" : "Add"} Product
              </button>
              <button
                className="cancel-btn"
                onClick={() => {
                  resetForm();
                  setShowForm(false);
                }}
              >
                Cancel
              </button>
            </div>
          </div>
        </div>
      )}

      {/* Product Grid */}
      <div className="product-grid">
        {products.map((p) => (
          <div key={p.id} className="product-card">
            <img src={p.imageUrl} alt={p.name} />
            <h4>{p.name}</h4>
            <p>R {p.price.toFixed(2)}</p>
            <p>Qty: {p.quantity}</p>
            <p>Category: {p.category}</p>

            <div className="product-actions">
              <button onClick={() => beginEdit(p)}>Edit</button>
              <button className="delete-btn" onClick={() => deleteProduct(p.id)}>
                Delete
              </button>
            </div>
          </div>
        ))}
      </div>
    </AdminLayout>
  );
}
