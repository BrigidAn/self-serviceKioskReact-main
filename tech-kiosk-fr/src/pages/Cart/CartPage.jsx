import React, { useEffect, useState } from "react";
import "./CartPage.css";

function CartPage() {
  const [cartItems, setCartItems] = useState([]);
  const [cartId, setCartId] = useState(null);
  const [deliveryMethod, setDeliveryMethod] = useState("Collection");
  const [loading, setLoading] = useState(true);

  const token = localStorage.getItem("token");
  const userId = localStorage.getItem("userId");
  const CART_API = "https://localhost:5016/api/cart";

  // Move fetchCart outside so it can be called anywhere
  const fetchCart = async () => {
    if (!token || !userId) return;

    try {
      const res = await fetch(`${CART_API}/${userId}`, {
        headers: { Authorization: `Bearer ${token}` },
      });

      if (!res.ok) {
        console.log("Failed to load cart");
        setCartItems([]);
        setLoading(false);
        return;
      }

      const data = await res.json();
      setCartItems(data.items || []);
      setCartId(data.cartId || null);
      setLoading(false);
    } catch (err) {
      console.log("Error fetching cart:", err);
      setLoading(false);
    }
  };

  // Fetch cart on page load
  useEffect(() => {
    fetchCart();
  }, [token, userId]);

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
        body: JSON.stringify({ quantity }),
      });

      if (!res.ok) {
        const data = await res.json();
        alert(data.message || "Failed to update quantity");
      } else {
        fetchCart();
      }
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

  // Cart total
  const cartTotal = cartItems.reduce(
    (total, item) => total + item.unitPrice * item.quantity,
    0
  );

  // Redirect if not logged in
  if (!token || !userId) {
    return <p>Please log in to view your cart.</p>;
  }

  return (
    <div className="cart-page">
      <h2>My Cart</h2>

      {loading ? (
        <p>Loading cart...</p>
      ) : cartItems.length === 0 ? (
        <p>Your cart is empty</p>
      ) : (
        <>
          <table className="cart-table">
            <thead>
              <tr>
                <th>Product</th>
                <th>Price</th>
                <th>Qty</th>
                <th>Total</th>
                <th></th>
              </tr>
            </thead>
            <tbody>
              {cartItems.map((item) => (
                <tr key={item.cartItemId}>
                  <td>{item.productName}</td>
                  <td>R {item.unitPrice.toFixed(2)}</td>
                  <td>
                    <input
                      type="number"
                      min="1"
                      value={item.quantity}
                      onChange={(e) =>
                        handleQuantityChange(item.cartItemId, parseInt(e.target.value))
                      }
                    />
                  </td>
                  <td>R {(item.unitPrice * item.quantity).toFixed(2)}</td>
                  <td>
                    <button onClick={() => handleRemove(item.cartItemId)}>Remove</button>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>

          <div className="cart-total">
            Total: <strong>R {cartTotal.toFixed(2)}</strong>
          </div>

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
