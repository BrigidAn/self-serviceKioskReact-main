import React, { useState } from "react";
import "./AdminAddProduct.css";

function AdminAddProduct() {
  const [formData, setFormData] = useState({
    name: "",
    description: "",
    price: "",
    category: "",
    supplierId: "",
  });

  const [image, setImage] = useState(null);
  const [message, setMessage] = useState("");

  const handleChange = (e) => {
    setFormData({ ...formData, [e.target.name]: e.target.value });
  };

  const handleImageUpload = (e) => {
    setImage(e.target.files[0]);
  };

  const handleSubmit = async (e) => {
    e.preventDefault();

    const data = new FormData();
    data.append("Name", formData.name);
    data.append("Description", formData.description);
    data.append("Price", formData.price);
    data.append("Category", formData.category);
    data.append("SupplierId", formData.supplierId);
    data.append("Image", image);

    try {
      const response = await fetch("https://localhost:5016/api/Product", {
        method: "POST",
        body: data,
      });

      if (response.ok) {
        setMessage("Product uploaded successfully!");
      } else {
        setMessage("Failed to upload product.");
      }
    } catch (error) {
      console.error(error);
      setMessage("Error uploading product.");
    }
  };

  return (
    <div className="admin-container">
      <h2>Add New Product</h2>

      {message && <p className="message">{message}</p>}

      <form onSubmit={handleSubmit} className="admin-form">
        <label>Product Name</label>
        <input
          type="text"
          name="name"
          onChange={handleChange}
          required
        />

        <label>Description</label>
        <textarea
          name="description"
          rows="4"
          onChange={handleChange}
          required
        />

        <label>Price</label>
        <input
          type="number"
          name="price"
          onChange={handleChange}
          required
        />

        <label>Category</label>
        <input
          type="text"
          name="category"
          onChange={handleChange}
          required
        />

        <label>Supplier ID</label>
        <input
          type="number"
          name="supplierId"
          onChange={handleChange}
          required
        />

        <label>Upload Image</label>
        <input type="file" onChange={handleImageUpload} required />

        <button type="submit" className="btn-submit">
          Upload Product
        </button>
      </form>
    </div>
  );
}

export default AdminAddProduct;
