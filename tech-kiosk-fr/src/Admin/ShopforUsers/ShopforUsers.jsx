import { useEffect, useState } from "react";
import AdminLayout from "../layout/AdminLayout";
import "./ShopforUsers.css";

export default function ShopForUser() {
  const [users, setUsers] = useState([]);
  const [products, setProducts] = useState([]);
  const [selectedUserId, setSelectedUserId] = useState("");
  const [cart, setCart] = useState(null);
  const [search, setSearch] = useState("");

  const token = localStorage.getItem("token");

  // ------------ FETCH USERS ------------
  const fetchUsers = async () => {
    const res = await fetch(`https://localhost:5016/api/account`, {
      headers: {
        Authorization: `Bearer ${token}`,
      },
    });
    const data = await res.json();
    setUsers(data);
  };

  // ------------ FETCH PRODUCTS ------------
  const fetchProducts = async () => {
    const res = await fetch(`https://localhost:5016/api/product`);
    const data = await res.json();
    setProducts(data);
  };

  // ------------ FETCH CART ------------
  const fetchCartForUser = async (userId) => {
    const res = await fetch(
      `https://localhost:5016/api/cart/user/${userId}`,
      {
        headers: {
          Authorization: `Bearer ${token}`,
        },
      }
    );
    const data = await res.json();
    setCart(data);
  };

  // ------------ ADD TO CART ------------
  const addToCart = async (productId) => {
    const res = await fetch(`https://localhost:5016/api/cart/add`, {
      method: "POST",
      headers: {
        "Content-Type": "application/json",
        Authorization: `Bearer ${token}`,
      },
      body: JSON.stringify({
        userId: selectedUserId,
        productId: productId,
        quantity: 1,
      }),
    });

    if (res.ok) {
      fetchCartForUser(selectedUserId);
    }
  };

  // ------------ ADMIN CHECKOUT ------------
  const checkoutForUser = async () => {
    const res = await fetch(`https://localhost:5016/api/admin/cart/checkout`, {
      method: "POST",
      headers: {
        "Content-Type": "application/json",
        Authorization: `Bearer ${token}`,
      },
      body: JSON.stringify({
        userId: selectedUserId,
      }),
    });

    const data = await res.json();
    alert(data.message);

    fetchCartForUser(selectedUserId);
  };

  // LOAD DATA
  useEffect(() => {
    fetchUsers();
    fetchProducts();
  }, []);

  return (
    <AdminLayout>
      <div className="shop-container">
        <h1 className="page-title">Shop For User</h1>

        {/* USER SELECTOR */}
        <div className="card">
          <label>Select User</label>
          <select
            value={selectedUserId}
            onChange={(e) => {
              setSelectedUserId(e.target.value);
              fetchCartForUser(e.target.value);
            }}
          >
            <option value="">-- Select a User --</option>
            {users.map((u) => (
              <option key={u.id} value={u.id}>
                {u.firstName} {u.lastName}
              </option>
            ))}
          </select>
        </div>

        {/* SEARCH */}
        <div className="card">
          <input
            type="text"
            placeholder="Search products..."
            className="search-input"
            value={search}
            onChange={(e) => setSearch(e.target.value)}
          />
        </div>

        {/* PRODUCT GRID */}
        <div className="products-grid">
          {products
            .filter((p) =>
              p.productName.toLowerCase().includes(search.toLowerCase())
            )
            .map((p) => (
              <div key={p.productId} className="product-card">
                <img src={p.imageUrl} alt="" />
                <h3>{p.productName}</h3>
                <p className="price">R {p.price}</p>
                <button
                  disabled={!selectedUserId}
                  onClick={() => addToCart(p.productId)}
                >
                  Add to Cart
                </button>
              </div>
            ))}
        </div>

        {/* CART PANEL */}
        {cart && cart.items.length > 0 && (
          <div className="card cart-card">
            <h2>User Cart</h2>
            <ul>
              {cart.items.map((item) => (
                <li key={item.cartItemId}>
                  {item.productName} x {item.quantity} = R{" "}
                  {item.totalPrice}
                </li>
              ))}
            </ul>

            <h3 className="total">Total: R {cart.totalAmount}</h3>

            <button className="checkout-btn" onClick={checkoutForUser}>
              Checkout for User
            </button>
          </div>
        )}
      </div>
    </AdminLayout>
  );
}
