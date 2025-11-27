import React, { useEffect, useState } from "react";
import "./CartPage.css";

function CartPage() {
  const [cartItems, setCartItems] = useState([]);
  const [cartId, setCartId] = useState(null);
  const [user, setUser] = useState(null);
  const [deliveryMethod, setDeliveryMethod] = useState("Collection");

  const token = localStorage.getItem("token");

  const USER_API = "https://localhost:5016/api/account/me";
  const CART_API = "https://localhost:5016/api/cart";

  // Fetch logged-in user
  useEffect(() => {
    if (!token) return;

    fetch(USER_API, {
      headers: { Authorization: `Bearer ${token}` },
    })
      .then((res) => res.json())
      .then((data) => {
        const finalUser = data.user || data; // handle both shapes
        setUser(finalUser);
      })
      .catch(console.log);
  }, [token]);

  // Fetch cart when user is loaded
  useEffect(() => {
    if (user) fetchCart();
  }, [user]);

  const fetchCart = async () => {
    try {
      const userId = user.id || user.Id;
      const res = await fetch(`${CART_API}/${userId}`, {
        headers: { Authorization: `Bearer ${token}` },
      });
      const data = await res.json();

      if (!res.ok) {
        console.log("Failed to load cart:", data.message);
        setCartItems([]);
        return;
      }

      // Map cart items according to your backend
      setCartItems(
        (data.items || []).map((item) => ({
          cartItemId: item.cartItemId,
          product: { name: item.productName },
          priceAtPurchase: item.unitPrice,
          quantity: item.quantity,
        }))
      );

      setCartId(data.cartId);
    } catch (err) {
      console.log(err);
    }
  };

  // Remove item
  const handleRemove = async (cartItemId) => {
    try {
      const res = await fetch(`${CART_API}/item/${cartItemId}`, {
        method: "DELETE",
        headers: { Authorization: `Bearer ${token}` },
      });
      const data = await res.json();
      alert(data.message || "Item removed");
      fetchCart();
    } catch (err) {
      console.log(err);
    }
  };

  // Update quantity
  const handleQuantityChange = async (cartItemId, quantity) => {
    if (quantity < 1) return;

    try {
      const res = await fetch(`${CART_API}/item/${cartItemId}`, {
        method: "PUT",
        headers: {
          "Content-Type": "application/json",
          Authorization: `Bearer ${token}`,
        },
        body: JSON.stringify({ quantity }), // backend expects object
      });

      const data = await res.json();

      if (!res.ok) alert(data.message || "Failed to update quantity");
      fetchCart();
    } catch (err) {
      console.log(err);
    }
  };

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
        <>
          <table className="cart-table">
            <thead>
              <tr>
                <th>Product</th>
                <th>Price</th>
                <th>Quantity</th>
                <th>Total</th>
                <th>Action</th>
              </tr>
            </thead>
            <tbody>
              {cartItems.map((item) => (
                <tr key={item.cartItemId}>
                  <td>{item.product?.name}</td>
                  <td>R {item.priceAtPurchase.toFixed(2)}</td>
                  <td>
                    <input
                      type="number"
                      min="1"
                      value={item.quantity}
                      onChange={(e) =>
                        handleQuantityChange(
                          item.cartItemId,
                          parseInt(e.target.value)
                        )
                      }
                    />
                  </td>
                  <td>R {(item.priceAtPurchase * item.quantity).toFixed(2)}</td>
                  <td>
                    <button onClick={() => handleRemove(item.cartItemId)}>
                      Remove
                    </button>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>

          <div className="cart-total">Total: R {cartTotal.toFixed(2)}</div>

          <div className="delivery-method">
            <label>
              Delivery method:
              <select
                value={deliveryMethod}
                onChange={(e) => setDeliveryMethod(e.target.value)}
              >
                <option value="Collection">Collection</option>
                <option value="Delivery">Delivery</option>
              </select>
            </label>
          </div>
        </>
      )}
    </div>
  );
}

export default CartPage;
