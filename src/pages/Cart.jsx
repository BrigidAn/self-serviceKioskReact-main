import React, { useEffect, useState } from "react";
import { useNavigate, Link } from "react-router-dom";
import axios from "axios";

function Cart({ userId }) {
  const navigate = useNavigate();
  const [orderId, setOrderId] = useState(null);
  const [items, setItems] = useState([]);

  // Fetch the user's active order
  const loadOrder = async () => {
    try {
      const res = await axios.get(`https://localhost:5016/api/Order/${userId}`);
      const active = res.data.find((o) => o.Status === "Pending");

      if (active) {
        setOrderId(active.OrderId);
        loadCartItems(active.OrderId);
      } else {
        setItems([]);
        console.warn("No active order found for user.");
      }
    } catch (err) {
      console.error(err);
    }
  };

  // Fetch items from backend cart
  const loadCartItems = async (orderId) => {
    try {
      const res = await axios.get(`https://localhost:5016/api/OrderItem/order/${orderId}`);
      setItems(res.data.Items || []);
    } catch (err) {
      console.error(err);
    }
  };

  // Remove item using backend DELETE
  const removeItem = async (orderItemId) => {
    try {
      await axios.delete(`https://localhost:5016/api/OrderItem/${orderItemId}`);
      loadCartItems(orderId);
    } catch (err) {
      console.error(err);
    }
  };

  // Navigate to checkout
  const goToCheckout = () => {
    navigate("/checkout", { state: { orderId } });
  };

  // Countdown timer for item expiry
  useEffect(() => {
    const timer = setInterval(() => {
      setItems((prevItems) =>
        prevItems
          .map((item) => ({
            ...item,
            SecondsRemaining: Math.max(0, item.SecondsRemaining - 1),
          }))
          .filter((item) => item.SecondsRemaining > 0)
      );
    }, 1000);

    return () => clearInterval(timer);
  }, []);

  const total = items.reduce((sum, item) => sum + item.Total, 0);

  return (
    <div className="container mt-5">
      <h2 className="text-center mb-4">Your Cart</h2>

      {items.length === 0 ? (
        <div className="text-center">
          <p className="lead">No items in your cart.</p>
          <Link to="/home" className="btn btn-primary mt-3">
            Continue Shopping
          </Link>
        </div>
      ) : (
        <>
          <div className="table-responsive">
            <table className="table table-bordered table-striped align-middle">
              <thead className="table-dark">
                <tr>
                  <th>#</th>
                  <th>Product</th>
                  <th>Price</th>
                  <th>Quantity</th>
                  <th>Total</th>
                  <th>Expires In (sec)</th>
                  <th>Remove</th>
                </tr>
              </thead>
              <tbody>
                {items.map((item, index) => (
                  <tr key={item.OrderItemId}>
                    <td>{index + 1}</td>
                    <td>{item.Name}</td>
                    <td>R{item.PriceAtPurchase.toFixed(2)}</td>
                    <td>{item.Quantity}</td>
                    <td>R{item.Total.toFixed(2)}</td>
                    <td>{Math.floor(item.SecondsRemaining)}</td>
                    <td>
                      <button
                        className="btn btn-danger btn-sm"
                        onClick={() => removeItem(item.OrderItemId)}
                      >
                        Remove
                      </button>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>

          <div className="d-flex justify-content-between align-items-center mt-4">
            <h4>Total: R{total.toFixed(2)}</h4>
            <button onClick={goToCheckout} className="btn btn-success btn-lg">
              Proceed to Checkout
            </button>
          </div>
        </>
      )}
    </div>
  );
}

export default Cart;
