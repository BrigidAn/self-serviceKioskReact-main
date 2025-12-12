import React, { useEffect, useState } from "react";
import "./ShopForUsers.css";
import AdminLayout from "../AdminLayout";
import { ToastContainer, toast } from "react-toastify";
import "react-toastify/dist/ReactToastify.css";

const API_URL = "https://localhost:5016/api";

export default function ShopForUser() {
  const [users, setUsers] = useState([]);
  const [products, setProducts] = useState([]);
  const [selectedUserId, setSelectedUserId] = useState(null);
  const [cart, setCart] = useState([]);
  const [quantityMap, setQuantityMap] = useState({});
  const [loadingUsers, setLoadingUsers] = useState(true);
  const [loadingProducts, setLoadingProducts] = useState(true);

  const getToken = () => localStorage.getItem("token");

  // Fetch users
  const fetchUsers = async () => {
    const token = getToken();
    if (!token) return;

    try {
      const res = await fetch(`${API_URL}/admin/users?page=1&pageSize=50`, {
        headers: { Authorization: `Bearer ${token}` },
      });
      const data = await res.json();
      setUsers(Array.isArray(data.data) ? data.data : []);
    } catch (err) {
      console.error("Error fetching users:", err);
      setUsers([]);
    } finally {
      setLoadingUsers(false);
    }
  };

  // Fetch products
  const fetchProducts = async () => {
    const token = getToken();
    if (!token) return;

    try {
      const res = await fetch(`${API_URL}/admin/products`, {
        headers: { Authorization: `Bearer ${token}` },
      });
      const data = await res.json();
      setProducts(Array.isArray(data.data) ? data.data : []);
    } catch (err) {
      console.error("Error fetching products:", err);
      setProducts([]);
    } finally {
      setLoadingProducts(false);
    }
  };

  // Fetch user's cart
  const fetchUserCart = async (userId) => {
    const token = getToken();
    if (!token || !userId) return;

    try {
      const res = await fetch(`${API_URL}/admin/cart/summary/${userId}`, {
        headers: { Authorization: `Bearer ${token}` },
      });

      if (!res.ok) {
        setCart([]);
        return;
      }

      const data = await res.json();
      const items = data.items?.map((i) => ({
        cartItemId: i.cartItemId, // important for deletion
        productId: i.productId,
        name: i.productName,
        quantity: i.quantity,
      })) || [];
      setCart(items);
    } catch (err) {
      console.error("Error fetching cart:", err);
      setCart([]);
    }
  };

  useEffect(() => {
    fetchUsers();
    fetchProducts();
  }, []);

  useEffect(() => {
    if (selectedUserId) fetchUserCart(selectedUserId);
  }, [selectedUserId]);

  const handleQuantityChange = (productId, value) => {
    setQuantityMap((prev) => ({
      ...prev,
      [productId]: parseInt(value),
    }));
  };

  const handleAddToCart = async (productId) => {
    if (!selectedUserId) {
      toast.warning("Select a user first");
      return;
    }

    const token = getToken();
    const quantity = quantityMap[productId] || 1;

    try {
      const res = await fetch(`${API_URL}/admin/cart/add`, {
        method: "POST",
        headers: {
          Authorization: `Bearer ${token}`,
          "Content-Type": "application/json",
        },
        body: JSON.stringify({
          userId: selectedUserId,
          productId,
          quantity,
        }),
      });

      if (!res.ok) {
        const err = await res.json();
        throw new Error(err.message || "Failed to add to cart");
      }

      fetchUserCart(selectedUserId);
      toast.success("Item added to cart");
    } catch (err) {
      console.error("Add to cart error:", err);
      toast.error("Failed to add to cart");
    }
  };

  const handleRemoveCartItem = async (cartItemId) => {
    const token = getToken();
    if (!token || !cartItemId) return;

    try {
      const res = await fetch(`${API_URL}/cart/item/${cartItemId}`, {
        method: "DELETE",
        headers: {
          Authorization: `Bearer ${token}`,
        },
      });

      if (!res.ok) {
        const err = await res.json();
        throw new Error(err.message || "Failed to remove item");
      }

      fetchUserCart(selectedUserId);
      toast.success("Item removed from cart");
    } catch (err) {
      console.error("Remove cart error:", err);
      toast.error("Failed to remove item");
    }
  };


  const handleUpdateCartQuantity = async (cartItemId, newQty) => {
  const token = getToken();
  if (!token || !cartItemId) return;

  const item = cart.find((ci) => ci.cartItemId === cartItemId);
  if (!item) return;

  const diff = newQty - item.quantity; // positive if increasing, negative if decreasing
  const product = products.find((p) => p.productId === item.productId);

  if (!product || newQty < 1 || newQty > (product.quantity + item.quantity)) {
    toast.warning("Invalid quantity");
    return;
  }

  try {
    const res = await fetch(`${API_URL}/cart/item/${cartItemId}`, {
      method: "POST",
      headers: {
        Authorization: `Bearer ${token}`,
        "Content-Type": "application/json",
      },
      body: JSON.stringify({ quantity: newQty }),
    });

    if (!res.ok) {
      const err = await res.json();
      throw new Error(err.message || "Failed to update quantity");
    }

    // Optimistic UI update
    setCart((prev) =>
      prev.map((ci) =>
        ci.cartItemId === cartItemId ? { ...ci, quantity: newQty } : ci
      )
    );

    setProducts((prev) =>
      prev.map((p) =>
        p.productId === item.productId
          ? { ...p, quantity: p.quantity - diff }
          : p
      )
    );

    toast.success("Quantity updated");
  } catch (err) {
    console.error("Update quantity error:", err);
    toast.error("Failed to update quantity");
  }
};



  const handleCheckout = async () => {
    if (!selectedUserId) {
      toast.warning("Select a user first");
      return;
    }

    const token = getToken();
    try {
      const res = await fetch(`${API_URL}/admin/cart/checkout/${selectedUserId}`, {
        method: "POST",
        headers: { Authorization: `Bearer ${token}` },
      });

      if (!res.ok) {
        const err = await res.json();
        throw new Error(err.message || "Checkout failed");
      }

      const data = await res.json();
      toast.success(`Checkout successful! Order ID: ${data.orderId}`);

      fetchUserCart(selectedUserId);
      fetchProducts();
    } catch (err) {
      console.error("Checkout error:", err);
      toast.error("Checkout failed");
    }
  };

  return (
    <AdminLayout>
      <div className="shop-container">
        <h1>Admin Shop for Users</h1>

        {/* User Selection */}
        <div className="user-select">
          {loadingUsers ? (
            <p>Loading users...</p>
          ) : (
            <select
              value={selectedUserId || ""}
              onChange={(e) => setSelectedUserId(parseInt(e.target.value))}
            >
              <option value="">-- Select User --</option>
              {users.map((u) => (
                <option key={u.id} value={u.id}>
                  {u.name} ({u.email})
                </option>
              ))}
            </select>
          )}
        </div>

        {/* Products Table */}
        <div className="products-table">
          {loadingProducts ? (
            <p>Loading products...</p>
          ) : (
            <table>
              <thead>
                <tr>
                  <th>Name</th>
                  <th>Category</th>
                  <th>Price</th>
                  <th>Stock</th>
                  <th>Quantity</th>
                  <th>Action</th>
                </tr>
              </thead>
              <tbody>
                {products.map((p) => (
                  <tr key={p.productId}>
                    <td>{p.name}</td>
                    <td>{p.category}</td>
                    <td>${p.price.toFixed(2)}</td>
                    <td>{p.quantity}</td>
                    <td>
                      <input
                        type="number"
                        min="1"
                        max={p.quantity || 1}
                        value={quantityMap[p.productId] || 1}
                        onChange={(e) =>
                          handleQuantityChange(p.productId, e.target.value)
                        }
                      />
                    </td>
                    <td>
                      <button onClick={() => handleAddToCart(p.productId)}>Add</button>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          )}
        </div>

        {/* User Cart */}
        <div className="user-cart">
          <h2>Cart for {users.find((u) => u.id === selectedUserId)?.name}</h2>
          {cart.length === 0 ? (
            <p>No items in cart</p>
          ) : (
            <>
              <ul>
                {cart.map((item) => (
                  <li key={item.cartItemId}>
                    {item.name} x{" "}
                    <input
                      type="number"
                      min="1"
                      max={
                        (products.find((p) => p.productId === item.productId)?.quantity || 0) +
                        item.quantity
                      }
                      value={item.quantity}
                      onChange={(e) =>
                        handleUpdateCartQuantity(item.cartItemId, parseInt(e.target.value))
                      }
                    />{" "}
                    <button onClick={() => handleRemoveCartItem(item.cartItemId)}>Remove</button>
                  </li>
                ))}
              </ul>

              <button onClick={handleCheckout}>Checkout</button>
            </>
          )}
        </div>

        <ToastContainer position="top-right" autoClose={3000} />
      </div>
    </AdminLayout>
  );
}
