import React, { useEffect, useState } from "react";
import "./ShopforUsers.css";
import AdminLayout from "../AdminLayout";
import { ToastContainer, toast } from "react-toastify";

const API_URL = "https://localhost:5016/api";

export default function ShopForUser() {
  const [users, setUsers] = useState([]);
  const [products, setProducts] = useState([]);
  const [cart, setCart] = useState([]);
  const [selectedUserId, setSelectedUserId] = useState(null);
  const [search, setSearch] = useState("");
  const [quantityMap, setQuantityMap] = useState({});
  const [account, setAccount] = useState(null);
  const [checkoutLoading, setCheckoutLoading] = useState(false);


  const token = localStorage.getItem("token");

  useEffect(() => {
    fetch(`${API_URL}/admin/users?page=1&pageSize=50`, {
      headers: { Authorization: `Bearer ${token}` }
    })
      .then(r => r.json())
      .then(d => setUsers(d.data || []));

    fetch(`${API_URL}/Admin/products`, {
      headers: { Authorization: `Bearer ${token}` }
    })
      .then(r => r.json())
      .then(d => setProducts(d.data || []));
  }, []);

useEffect(() => {
  if (!selectedUserId) return;

  fetch(`${API_URL}/admin/cart/summary/${selectedUserId}`, {
    headers: { Authorization: `Bearer ${token}` }
  })
    .then(res => res.text())
    .then(txt => setCart(txt ? JSON.parse(txt).items || [] : []))
    .catch(err => console.error(err));

  fetch(`${API_URL}/user/account/${selectedUserId}`, {
    headers: { Authorization: `Bearer ${token}` }
  })
    .then(res => res.text())
    .then(txt => setAccount(txt ? JSON.parse(txt) : null))
    .catch(err => console.error(err));
}, [selectedUserId]);


  const filteredProducts = products.filter(p =>
    p.name.toLowerCase().includes(search.toLowerCase())
  );

  const addToCart = async (productId) => {
    if (!selectedUserId) return toast.warning("Select a user");

    const qty = quantityMap[productId] || 1;

    await fetch(`${API_URL}/admin/cart/add`, {
      method: "POST",
      headers: {
        Authorization: `Bearer ${token}`,
        "Content-Type": "application/json"
      },
      body: JSON.stringify({ userId: selectedUserId, productId, quantity: qty })
    });

    toast.success("Added to cart");
    setQuantityMap({ ...quantityMap, [productId]: 1 });
  };

  return (
    <AdminLayout>
      <div className="shop-users-container">
        <h1>🛒 Shop for Users</h1>

        {/* USER SELECT */}
        <select
          className="user-dropdown"
          value={selectedUserId || ""}
          onChange={(e) => setSelectedUserId(+e.target.value)}
        >
          <option value="">Select User</option>
          {users.map(u => (
            <option key={u.id} value={u.id}>
              {u.name} ({u.email})
            </option>
          ))}
        </select>

        {/* SEARCH */}
        <input
          className="search-input"
          placeholder="Search products..."
          value={search}
          onChange={(e) => setSearch(e.target.value)}
        />

          {selectedUserId && account && (
            <div className="checkout-panel">
              <p>
                <strong>{users.find(u => u.id === selectedUserId)?.name}'s Balance:</strong> R {account.balance.toFixed(2)}
              </p>
              <p>
                <strong>Cart Total:</strong> R {cart.reduce((sum, i) => sum + i.unitPrice * i.quantity, 0).toFixed(2)}
              </p>
              <button
                onClick={async () => {
                  if (!cart.length) return toast.warning("Cart is empty");
                  setCheckoutLoading(true);

                  const res = await fetch(`${API_URL}/checkout`, {
                    method: "POST",
                    headers: {
                      Authorization: `Bearer ${token}`,
                      "Content-Type": "application/json",
                    },
                    body: JSON.stringify({ userId: selectedUserId, deliveryMethod: "delivery" }),
                  });

                  const data = await res.json();
                  setCheckoutLoading(false);

                  if (res.ok) {
                    toast.success(data.message || "Checkout successful");
                    setCart([]);
                    setAccount(prev => ({ ...prev, balance: prev.balance - data.totalAmount }));
                  } else {
                    toast.error(data.message || "Checkout failed");
                  }
                }}
                disabled={checkoutLoading || !cart.length || account.balance <= 0}
              >
                {checkoutLoading ? "Processing..." : "Checkout for User"}
              </button>
            </div>
          )}

        <div className="shop-layout">
          {/* PRODUCTS */}
            <div className="products-table-wrapper">
              <table className="products-table">
                <thead>
                  <tr>
                    <th>Image</th>
                    <th>Name</th>
                    <th>Category</th>
                    <th>Price</th>
                    <th>Stock</th>
                    <th>Qty</th>
                    <th>Action</th>
                  </tr>
                </thead>

                <tbody>
                  {filteredProducts.length === 0 ? (
                    <tr>
                      <td colSpan="7" className="empty">
                        No products found
                      </td>
                    </tr>
                  ) : (
                    filteredProducts.map(p => (
                      <tr key={p.productId}>
                        <td>
                          <img
                            src={p.imageUrl}
                            alt={p.name}
                            className="table-img"
                          />
                        </td>

                        <td>{p.name}</td>
                        <td>{p.category}</td>
                        <td>R {p.price.toFixed(2)}</td>

                        <td className={p.quantity === 0 ? "out" : ""}>
                          {p.quantity === 0 ? "Out" : p.quantity}
                        </td>

                        <td>
                          <input
                            type="number"
                            min="1"
                            max={p.quantity}
                            value={quantityMap[p.productId] || 1}
                            onChange={(e) =>
                              setQuantityMap({
                                ...quantityMap,
                                [p.productId]: +e.target.value
                              })
                            }
                            disabled={p.quantity === 0}
                          />
                        </td>

                        <td>
                          <button
                            disabled={p.quantity === 0}
                            onClick={() => addToCart(p.productId)}
                          >
                            Add
                          </button>
                        </td>
                      </tr>
                    ))
                  )}
                </tbody>
              </table>
            </div>

          {/* CART */}
          <div className="cart-panel">
            <h2>🧾 Cart</h2>
            {cart.length === 0 ? (
              <p>No items</p>
            ) : (
              cart.map(i => (
                <div key={i.cartItemId} className="cart-item">
                  <span>{i.productName}</span>
                  <span>x{i.quantity}</span>
                </div>
              ))
            )}
          </div>
        </div>

        <ToastContainer />
      </div>
    </AdminLayout>
  );
}
