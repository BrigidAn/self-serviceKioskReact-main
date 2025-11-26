import React, { useEffect, useState } from "react";
import { useNavigate } from "react-router-dom";
import "./CartPage.css";

function CartPage() {
  const navigate = useNavigate();
  const [cartItems, setCartItems] = useState([]);
  const [user, setUser] = useState(null);
  const [deliveryMethod, setDeliveryMethod] = useState("Collection");
  const token = localStorage.getItem("token");

  const USER_API = "https://localhost:5016/api/auth/me";

  // ----------------------------------------------------
  // 1️⃣ Fetch User Info
  // ----------------------------------------------------
  useEffect(() => {
    if (!token) return;

    fetch(USER_API, {
      headers: { Authorization: `Bearer ${token}` }
    })
      .then(res => res.json())
      .then(data => {
        setUser(data);
      })
      .catch(err => console.log(err));
  }, [token]);

  // ----------------------------------------------------
  // 2️⃣ Fetch Cart Only After User Is Loaded
  // ----------------------------------------------------
  useEffect(() => {
    if (user && user.currentOrderId) {
      fetchCart(user.currentOrderId);
    }
  }, [user]);

  // ----------------------------------------------------
  // 3️⃣ Load Cart Items
  // ----------------------------------------------------
  const fetchCart = async (orderId) => {
    try {
      const res = await fetch(
        `https://localhost:5016/api/orderItem/order/${orderId}`,
        {
          headers: { Authorization: `Bearer ${token}` }
        }
      );

      const data = await res.json();

      if (!res.ok) {
        console.log("Failed to fetch cart");
        setCartItems([]);
        return;
      }

      setCartItems(data.items || []);

    } catch (err) {
      console.log(err);
    }
  };

  // ----------------------------------------------------
  // 4️⃣ Remove Item
  // ----------------------------------------------------
  const handleRemove = async (orderItemId) => {
    try {
      const res = await fetch(
        `https://localhost:5016/api/orderItem/${orderItemId}`,
        {
          method: "DELETE",
          headers: { Authorization: `Bearer ${token}` }
        }
      );

      const data = await res.json();
      alert(data.message || "Item removed");

      fetchCart(user.currentOrderId);

    } catch (err) {
      console.log(err);
    }
  };

  // ----------------------------------------------------
  // 5️⃣ Update Quantity
  // ----------------------------------------------------
  const handleQuantityChange = async (orderItemId, quantity) => {
    if (quantity < 1) return;

    try {
      const res = await fetch(
        `https://localhost:5016/api/orderItem/${orderItemId}`,
        {
          method: "PUT",
          headers: {
            "Content-Type": "application/json",
            Authorization: `Bearer ${token}`
          },
          body: JSON.stringify({ quantity })
        }
      );

      const data = await res.json();

      if (!res.ok) {
        alert(data.message || "Failed to update quantity");
      }

      fetchCart(user.currentOrderId);

    } catch (err) {
      console.log(err);
    }
  };

  // ----------------------------------------------------
  // 6️⃣ Checkout
  // ----------------------------------------------------
  const handleCheckout = async () => {
    if (!user) return;

    try {
      const res = await fetch(
        `https://localhost:5016/api/orderItem/checkout/${user.currentOrderId}`,
        {
          method: "POST",
          headers: {
            "Content-Type": "application/json",
            Authorization: `Bearer ${token}`
          },
          body: JSON.stringify({ deliveryMethod })
        }
      );

      const data = await res.json();

      if (res.ok) {
        alert(`Checkout successful! Total: R ${data.order.totalAmount}`);
        setCartItems([]);
      } else {
        alert(data.message || "Checkout failed");
      }

    } catch (err) {
      console.log(err);
    }
  };

  // Total price
  const cartTotal = cartItems.reduce(
    (total, item) => total + item.priceAtPurchase * item.quantity,
    0
  );

  return (
    <div className="cart-page">
      <h2>My Cart</h2>

      {cartItems.length === 0 ? (
        <p>Your cart is empty</p>
      ) : (
        <table className="cart-table">
          <thead>
            <tr>
              <th>Product</th>
              <th>Price</th>
              <th>Qty</th>
              <th>Total</th>
              <th>Time Left</th>
              <th>Action</th>
            </tr>
          </thead>

          <tbody>
            {cartItems.map(item => (
              <tr key={item.orderItemId}>
                <td>{item.product?.name}</td>
                <td>R {item.priceAtPurchase}</td>

                <td>
                  <input
                    type="number"
                    value={item.quantity}
                    min="1"
                    disabled={item.secondsRemaining <= 0}
                    onChange={(e) =>
                      handleQuantityChange(item.orderItemId, parseInt(e.target.value))
                    }
                  />
                </td>

                <td>R {(item.priceAtPurchase * item.quantity).toFixed(2)}</td>

                <td>
                  {item.secondsRemaining > 0
                    ? `${Math.floor(item.secondsRemaining / 60)} min`
                    : "Expired"}
                </td>

                <td>
                  <button onClick={() => handleRemove(item.orderItemId)}>Remove</button>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      )}

      <div className="cart-total">
        Total: R {cartTotal.toFixed(2)}
      </div>

      <button
        className="checkout-btn"
        disabled={cartItems.length === 0}
        onClick={handleCheckout}
      >
        Checkout
      </button>
    </div>
  );
}

export default CartPage;
