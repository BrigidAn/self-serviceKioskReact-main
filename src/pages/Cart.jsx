import React, { useEffect, useState, useCallback } from "react";
import { useNavigate, Link } from "react-router-dom";

function Cart() {
  const navigate = useNavigate();

  // State
  const [cartItems, setCartItems] = useState([]); // items in cart
  const [timeLeft, setTimeLeft] = useState(""); // countdown display
  const [loading, setLoading] = useState(true); // loading state

  // Fetch cart items from backend
  const fetchCartItems = useCallback(() => {
    fetch("/api/cart", { credentials: "include" }) // session-based fetch
      .then((res) => {
        if (res.status === 401) navigate("/login"); // redirect if not logged in
        return res.json();
      })
      .then((data) => {
        setCartItems(data.items || []);
        setLoading(false);

        if (data.items.length > 0) {
          // Start countdown for the earliest expiring cart item
          const soonestExpiry = data.items.reduce((prev, curr) =>
            new Date(prev.reservedUntil) < new Date(curr.reservedUntil) ? prev : curr
          );
          startCountdown(soonestExpiry.reservedUntil);
        }
      });
  }, [navigate]);

  // Remove expired items (backend cleanup)
  const cleanupCart = useCallback(() => {
    fetch("/api/cart/cleanup", { method: "POST", credentials: "include" });
  }, []);

  // Load cart on mount
  useEffect(() => {
    cleanupCart();
    fetchCartItems();
  }, [cleanupCart, fetchCartItems]);

  // Countdown timer for reservations
  const startCountdown = useCallback((expiresAt) => {
    const target = new Date(expiresAt);

    const interval = setInterval(() => {
      const now = new Date();
      const diff = target - now;

      if (diff <= 0) {
        setTimeLeft("Reservation Expired");
        clearInterval(interval);
        fetchCartItems(); // refresh cart after expiration
        return;
      }

      const hrs = Math.floor(diff / (1000 * 60 * 60));
      const mins = Math.floor((diff % (1000 * 60 * 60)) / (1000 * 60));
      const secs = Math.floor((diff % (1000 * 60)) / 1000);

      setTimeLeft(`${hrs}h ${mins}m ${secs}s`);
    }, 1000);
  }, [fetchCartItems]);

  // Remove item from cart
  const removeFromCart = async (cartItemId) => {
    await fetch(`/api/cart/remove/${cartItemId}`, {
      method: "DELETE",
      credentials: "include",
    });

    setCartItems(cartItems.filter((c) => c.cartItemId !== cartItemId));
  };

  // Calculate total price
  const total = cartItems.reduce(
    (sum, item) => sum + item.product.price * item.quantity,
    0
  );

  const goToCheckout = () => navigate("/checkout");

  if (loading) return <h3 className="text-center mt-5">Loading cart...</h3>;

  return (
    <div className="container mt-5">
      <h2 className="text-center mb-4">Your Cart</h2>

      {cartItems.length === 0 ? (
        <div className="text-center">
          <p className="lead">Your cart is empty or expired.</p>
          <Link to="/home" className="btn btn-primary mt-3">
            Continue Shopping
          </Link>
        </div>
      ) : (
        <>
          {/* Countdown timer */}
          <p className="text-center text-danger">
            ⏳ Reservation expires in: <b>{timeLeft}</b>
          </p>

          <div className="table-responsive">
            <table className="table table-bordered table-striped align-middle">
              <thead className="table-dark">
                <tr>
                  <th>#</th>
                  <th>Product</th>
                  <th>Qty</th>
                  <th>Price</th>
                  <th>Action</th>
                </tr>
              </thead>
              <tbody>
                {cartItems.map((item, index) => (
                  <tr key={item.cartItemId}>
                    <td>{index + 1}</td>
                    <td>{item.product.name}</td>
                    <td>{item.quantity}</td>
                    <td>R{(item.product.price * item.quantity).toFixed(2)}</td>
                    <td>
                      <button
                        className="btn btn-danger btn-sm"
                        onClick={() => removeFromCart(item.cartItemId)}
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
