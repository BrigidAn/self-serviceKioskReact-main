import React, { useState, useEffect } from "react";
import AdminLayout from "../AdminLayout";
import "./ShopforUsers.css";

export default function ShopForUser() {
  const [users, setUsers] = useState([]);
  const [selectedUserId, setSelectedUserId] = useState(null);
  const [products, setProducts] = useState([]);
  const [cart, setCart] = useState(null);
  const [loading, setLoading] = useState(false);
  const token = localStorage.getItem("token");

  // Fetch all users (admin only)
  const fetchUsers = async () => {
    try {
      const res = await fetch("https://localhost:5016/api/admin/users", {
        headers: { Authorization: `Bearer ${token}` },
      });
      if (!res.ok) throw new Error("Failed to fetch users");
      const data = await res.json();
      setUsers(data);
    } catch (err) {
      console.error(err.message);
    }
  };

  // Fetch all products
  const fetchProducts = async () => {
    try {
      const res = await fetch("https://localhost:5016/api/product", {
        headers: { Authorization: `Bearer ${token}` },
      });
      if (!res.ok) throw new Error("Failed to fetch products");
      const data = await res.json();
      setProducts(data);
    } catch (err) {
      console.error(err.message);
    }
  };

  // Fetch selected user's cart
  const fetchCartForUser = async (userId) => {
    try {
      const res = await fetch(`https://localhost:5016/api/cart/user/${userId}`, {
        headers: { Authorization: `Bearer ${token}` },
      });
      if (!res.ok) setCart(null); // if no cart yet
      else {
        const data = await res.json();
        setCart(data);
      }
    } catch (err) {
      console.error(err.message);
    }
  };

  const addToUserCart = async (productId, quantity) => {
    if (!selectedUserId) return alert("Select a user first");

    try {
      const res = await fetch(`https://localhost:5016/api/admin/cart/add`, {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
          Authorization: `Bearer ${token}`,
        },
        body: JSON.stringify({ userId: selectedUserId, productId, quantity }),
      });

      if (!res.ok) {
        const errorData = await res.json();
        throw new Error(errorData.message || "Failed to add product");
      }

      const data = await res.json();
      console.log(data.message);
      fetchCartForUser(selectedUserId); // refresh cart
    } catch (err) {
      console.error(err.message);
    }
  };

  useEffect(() => {
    fetchUsers();
    fetchProducts();
  }, []);

  useEffect(() => {
    if (selectedUserId) fetchCartForUser(selectedUserId);
  }, [selectedUserId]);

  return (
    <div>
      <h2>Shop For User</h2>

      <div>
        <label>Select User: </label>
        <select
          value={selectedUserId || ""}
          onChange={(e) => setSelectedUserId(e.target.value)}
        >
          <option value="">-- Select a User --</option>
          {users.map((u) => (
            <option key={u.id} value={u.id}>
              {u.name} ({u.email})
            </option>
          ))}
        </select>
      </div>

      <h3>Products</h3>
      <ul>
        {products.map((p) => (
          <li key={p.productId}>
            {p.name} - ${p.price} - Stock: {p.quantity}
            <button onClick={() => addToUserCart(p.productId, 1)}>Add 1</button>
          </li>
        ))}
      </ul>

      {cart && cart.items.length > 0 && (
        <div>
          <h3>User Cart</h3>
          <ul>
            {cart.items.map((item) => (
              <li key={item.cartItemId}>
                {item.productName} x {item.quantity} = $
                {item.unitPrice * item.quantity}
              </li>
            ))}
          </ul>
          <h4>Total: ${cart.totalAmount}</h4>
        </div>
      )}
    </div>
  );
}
