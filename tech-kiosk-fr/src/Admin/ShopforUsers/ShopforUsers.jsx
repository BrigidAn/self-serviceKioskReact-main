import React, { useEffect, useState } from "react";
import "./ShopforUsers.css";
import AdminLayout from "../AdminLayout";
import { ToastContainer, toast } from "react-toastify";
import "react-toastify/dist/ReactToastify.css";

const API_URL = "https://localhost:5016/api";
const PAGE_SIZE = 6;

export default function ShopForUser() {
  const [users, setUsers] = useState([]);
  const [products, setProducts] = useState([]);
  const [cart, setCart] = useState([]);
  const [account, setAccount] = useState(null);

  const [selectedUserId, setSelectedUserId] = useState(null);
  const [search, setSearch] = useState("");
  const [quantityMap, setQuantityMap] = useState({});
  const [currentPage, setCurrentPage] = useState(1);
  const [checkoutLoading, setCheckoutLoading] = useState(false);
  const [loadingProducts, setLoadingProducts] = useState(true);

  const token = localStorage.getItem("token");

  const fetchUsers = async () => {
    const res = await fetch(`${API_URL}/admin/users?page=1&pageSize=60`, {
      headers: { Authorization: `Bearer ${token}` },
    });
    const data = await res.json();
    setUsers(data.data || []);
  };

  const fetchProducts = async () => {
    setLoadingProducts(true);
    try {
      const res = await fetch(`${API_URL}/Product`, {
        headers: { Authorization: `Bearer ${token}` },
      });
      const data = await res.json();
      setProducts(Array.isArray(data) ? data : data.data || []);
    } catch {
      toast.error("Failed to load products");
    } finally {
      setLoadingProducts(false);
    }
  };

  const fetchCartAndAccount = async (userId) => {
    if (!userId) return;

    const cartRes = await fetch(`${API_URL}/admin/cart/summary/${userId}`, {
      headers: { Authorization: `Bearer ${token}` },
    });
    const cartTxt = await cartRes.text();
    setCart(cartTxt ? JSON.parse(cartTxt).items || [] : []);

    const accRes = await fetch(`${API_URL}/user/account/${userId}`, {
      headers: { Authorization: `Bearer ${token}` },
    });
    const accTxt = await accRes.text();
    setAccount(accTxt ? JSON.parse(accTxt) : null);
  };

  useEffect(() => {
    fetchUsers();
    fetchProducts();
  }, []);

  useEffect(() => {
    if (selectedUserId) fetchCartAndAccount(selectedUserId);
  }, [selectedUserId]);

  const filteredProducts = products.filter((p) =>
    p.name.toLowerCase().includes(search.toLowerCase())
  );

  const totalPages = Math.ceil(filteredProducts.length / PAGE_SIZE);
  const paginatedProducts = filteredProducts.slice(
    (currentPage - 1) * PAGE_SIZE,
    currentPage * PAGE_SIZE
  );

  const handlePageChange = (page) => {
    if (page < 1 || page > totalPages) return;
    setCurrentPage(page);
  };

  const addToCart = async (productId) => {
    if (!selectedUserId) return toast.warning("Select a user first");

    const product = products.find((p) => p.productId === productId);
    if (!product || product.quantity === 0)
      return toast.error("Product out of stock");

    const qty = quantityMap[productId] || 1;
    if (qty > product.quantity) return toast.error("Exceeds stock");

    await fetch(`${API_URL}/admin/cart/add`, {
      method: "POST",
      headers: {
        Authorization: `Bearer ${token}`,
        "Content-Type": "application/json",
      },
      body: JSON.stringify({ userId: selectedUserId, productId, quantity: qty }),
    });

    toast.success("Added to cart");
    setQuantityMap((prev) => ({ ...prev, [productId]: 1 }));
    fetchCartAndAccount(selectedUserId);
    fetchProducts(); // update stock
  };

const removeFromCart = async (cartItemId) => {
  if (!selectedUserId) return;
  try {
    await fetch(`${API_URL}/admin/cart/remove/${cartItemId}`, {
      method: "DELETE",
      headers: { Authorization: `Bearer ${token}` },
    });
    toast.info("Item removed from cart");
    fetchCartAndAccount(selectedUserId);
    fetchProducts(); // optional: update stock if needed
  } catch {
    toast.error("Failed to remove item");
  }
};


  const handleCheckout = async () => {
    if (!cart.length || checkoutLoading) return;

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
      fetchCartAndAccount(selectedUserId);
      fetchProducts();
    } else {
      toast.error(data.message || "Checkout failed");
    }
  };

  const cartTotal = cart.reduce((sum, i) => sum + i.unitPrice * i.quantity, 0);

  return (
    <AdminLayout>
      <ToastContainer position="top-right" />

      <div className="shop-users-container">
        <h1>🛒 Shop for Users</h1>

        <div className="toolbar">
          <select
            className="user-dropdown"
            value={selectedUserId || ""}
            onChange={(e) => setSelectedUserId(+e.target.value)}
          >
            <option value="">Select User</option>
            {users.map((u) => (
              <option key={u.id} value={u.id}>
                {u.name} ({u.email})
              </option>
            ))}
          </select>

          <button
            className="refresh-btn"
            onClick={() => {
              fetchUsers();
              fetchProducts();
              if (selectedUserId) fetchCartAndAccount(selectedUserId);
              toast.info("Data refreshed");
            }}
          >
            Refresh
          </button>
        </div>

        <input
          className="search-input"
          placeholder="Search products..."
          value={search}
          onChange={(e) => {
            setSearch(e.target.value);
            setCurrentPage(1);
          }}
        />

        {selectedUserId && account && (
          <div className="checkout-panel">
            <p>
              <strong>
                {users.find((u) => u.id === selectedUserId)?.name}'s Balance:
              </strong>{" "}
              R {account.balance.toFixed(2)}
            </p>

            <p>
              <strong>Cart Total:</strong> R {cartTotal.toFixed(2)}
            </p>

            <button
              disabled={checkoutLoading || !cart.length || account.balance < cartTotal}
              onClick={handleCheckout}
            >
              {checkoutLoading ? "Processing..." : "Checkout for User"}
            </button>
          </div>
        )}

        <div className="shop-layout">
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
                {loadingProducts ? (
                  <tr><td colSpan="7">Loading products...</td></tr>
                ) : paginatedProducts.length === 0 ? (
                  <tr><td colSpan="7">No products found</td></tr>
                ) : (
                  paginatedProducts.map((p) => (
                    <tr key={p.productId}>
                      <td>
                        <img
                          src={p.imageUrl || "https://via.placeholder.com/60"}
                          alt={p.name}
                          className="table-img"
                        />
                      </td>
                      <td>{p.name}</td>
                      <td>{p.category}</td>
                      <td>R {p.price.toFixed(2)}</td>
                      <td>{p.quantity === 0 ? "Out" : p.quantity}</td>
                      <td>
                        <input
                          type="number"
                          min="1"
                          max={p.quantity}
                          value={quantityMap[p.productId] || 1}
                          disabled={p.quantity === 0}
                          onChange={(e) =>
                            setQuantityMap((prev) => ({
                              ...prev,
                              [p.productId]: Math.min(
                                Math.max(+e.target.value, 1),
                                p.quantity
                              ),
                            }))
                          }
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

          <div className="cart-panel">
            <h2>🧾 Cart</h2>
            {cart.length === 0 ? (
              <p>No items</p>
            ) : (
              cart.map((i) => (
                <div key={i.cartItemId} className="cart-item">
                  <span>
                    {i.productName} x{i.quantity}
                  </span>
                  <button onClick={() => removeFromCart(i.cartItemId)}>❌</button>
                </div>
              ))
            )}
            <p><strong>Total:</strong> R {cartTotal.toFixed(2)}</p>
          </div>
        </div>

        <ToastContainer />
            {totalPages > 1 && (
              <div className="o-pagination">
                <button onClick={() => handlePageChange(currentPage - 1)}>
                  Prev
                </button>
                {[...Array(totalPages)].map((_, i) => (
                  <button
                    key={i}
                    className={currentPage === i + 1 ? "active" : ""}
                    onClick={() => handlePageChange(i + 1)}
                  >
                    {i + 1}
                  </button>
                ))}
                <button onClick={() => handlePageChange(currentPage + 1)}>
                  Next
                </button>
              </div>
            )}
      </div>
    </AdminLayout>
  );
}
