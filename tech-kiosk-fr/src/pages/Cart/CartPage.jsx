import React, { useEffect, useState } from "react";
import NavBar from "../../components/Navbar";
import "./CartPage.css";
import { useNavigate } from "react-router-dom";
import { toast } from "react-toastify";
import "react-toastify/dist/ReactToastify.css";

const CART_API = "https://localhost:5016/api/cart";
const CHECKOUT_API = "https://localhost:5016/api/checkout";
const PLACEHOLDER =
  "https://images.unsplash.com/photo-1703584449021-4cfdfe9650dd?w=500&auto=format&fit=crop&q=60";

export default function CartPage() {
  const [cartItems, setCartItems] = useState([]);
  const [cartCount, setCartCount] = useState(0);
  const [loading, setLoading] = useState(true);
  const [deliveryMethod, setDeliveryMethod] = useState("pickup");
  const navigate = useNavigate();
  const token = localStorage.getItem("token");
  const [showModal, setShowModal] = useState(false);
  const [remainingAmount, setRemainingAmount] = useState(0);
  const [deductedAmount, setDeductedAmount] = useState(0);
  const CART_EXPIRY_MINUTES = 10;
  const [timeLeft, setTimeLeft] = useState(null);
const [countdown, setCountdown] = useState(0);
const [isPaying, setIsPaying] = useState(false);

  // ------------------- FETCH CART -------------------
  const fetchCart = async () => {
    setLoading(true);
    try {
      const res = await fetch(CART_API, {
        headers: { Authorization: `Bearer ${token}` },
      });
      if (!res.ok) throw new Error("Failed to fetch cart");

      // Start expiry timer if not started
      let expiry = localStorage.getItem("cartExpiry");

      if (!expiry) {
        const newExpiry = Date.now() + CART_EXPIRY_MINUTES * 60 * 1000;
        localStorage.setItem("cartExpiry", newExpiry);
        expiry = newExpiry;
      }

      startCountdown(expiry);


      const data = await res.json();
      setCartItems(data.items || []);
      setCartCount(data.items?.length || 0);
    } catch (err) {
      console.error(err);
      toast.error(err.message);
    } finally {
      setLoading(false);
    }
  };

  // COUNTDOWN WHEN PAYMENT STARTS
const startCountdown = () => {
  setCountdown(30); // 30 seconds
  setIsPaying(true);

  const timer = setInterval(() => {
    setCountdown((prev) => {
      if (prev <= 1) {
        clearInterval(timer);
        setIsPaying(false);
        return 0;
      }
      return prev - 1;
    });
  }, 1000);
};


  useEffect(() => {
    fetchCart();
  }, []);

  // ------------------- UPDATE QUANTITY -------------------
  const handleUpdateQuantity = async (itemId, quantity) => {
    if (quantity < 1) return;
    try {
      const res = await fetch(`${CART_API}/item/${itemId}`, {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
          Authorization: `Bearer ${token}`,
        },
        body: JSON.stringify({ quantity }),
      });
      if (!res.ok) {
        const err = await res.json();
        throw new Error(err.message || "Failed to update quantity");
      }
      fetchCart();
    } catch (err) {
      console.error(err);
      toast.error(err.message);
    }
  };


    const handleRemoveItem = async (itemId) => {
      try {
        const res = await fetch(`${CART_API}/item/${itemId}`, {
          method: "DELETE",
          headers: { Authorization: `Bearer ${token}` },
        });
        if (!res.ok) throw new Error("Failed to remove item");
        toast.success("Item removed from cart");
        fetchCart(); // refresh cart
      } catch (err) {
        console.error(err);
        toast.error(err.message);
      }
    };


  // ------------------- CHECKOUT -------------------
  const handleCheckout = async () => {
    if (!cartItems.length) return toast.info("Cart is empty");

    try {
      const res = await fetch(CHECKOUT_API, {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
          Authorization: `Bearer ${token}`,
        },
        body: JSON.stringify({
          UserId: 1, // Replace with logged-in user's ID
          DeliveryMethod: deliveryMethod,
        }),
      });

      const data = await res.json();

      if (!res.ok) {
        if (data.message?.toLowerCase().includes("insufficient")) {
          setRemainingAmount(data.remainingAmount || 0);
          setDeductedAmount(data.deductedAmount || 0);
          setShowModal(true);
        } else {
          throw new Error(data.message || "Checkout failed");
        }
      } else {
        toast.success(data.message);
        fetchCart();
        navigate("/checkout");
      }
    } catch (err) {
      console.error(err);
      toast.error(err.message);
    }
  };

  // ------------------- TOTALS -------------------
  const totalItemsAmount = cartItems.reduce(
    (sum, item) => sum + item.unitPrice * item.quantity,
    0
  );
  const deliveryFee = deliveryMethod === "delivery" ? 80 : 0;
  const grandTotal = totalItemsAmount + deliveryFee;

  const handleImgError = (e) => {
    e.target.src = PLACEHOLDER;
  };

  // ------------------- MODAL COMPONENT -------------------
  const InsufficientBalanceModal = ({ show, onClose, remainingAmount, deductedAmount }) => {
    if (!show) return null;


    const startCountdown = (expiryTime) => {
    const interval = setInterval(() => {
      const now = Date.now();
      const diff = expiryTime - now;

      if (diff <= 0) {
        clearInterval(interval);
        setTimeLeft("Expired");
        expireCart();
        return;
      }

      // Convert milliseconds to MM:SS
      const minutes = Math.floor(diff / 60000);
      const seconds = Math.floor((diff % 60000) / 1000);

      setTimeLeft(`${minutes}:${seconds < 10 ? "0" : ""}${seconds}`);
    }, 1000);
    };

    const expireCart = async () => {
      localStorage.removeItem("cartExpiry");

      try {
        await fetch(`${CART_API}/expire`, {
          method: "POST",
          headers: { Authorization: `Bearer ${token}` },
        });

        toast.info("Cart expired. Items returned to stock.");
        fetchCart();
      } catch (err) {
        console.error(err);
      }
    };



    return (
      <div className="modal-overlay">
        <div className="modal-content">
          <h2>Insufficient Balance</h2>
          <p>Your balance is not enough to complete this purchase.</p>
          <p>Deducted Amount: R {deductedAmount.toFixed(2)}</p>
          <p>Remaining Amount Needed: R {remainingAmount.toFixed(2)}</p>
          <button onClick={onClose} className="modal-close-btn">
            Close
          </button>
        </div>
      </div>
    );
  };

  return (
    <div>
      <NavBar cartCount={cartCount} />

      <button className="cart-back-btn" onClick={() => navigate("/products")}>
        ← Back
      </button>

      <div className="velvety-cart-page">
        <h2>Shopping Cart</h2>

        {loading ? (
          <p>Loading cart...</p>
        ) : cartItems.length === 0 ? (
          <p className="empty-cart">Your cart is empty.</p>
        ) : (
              <>
                {timeLeft && (
                <div className="cart-expiry-banner">
                  🕒 Cart expires in <strong>{timeLeft}</strong>
                </div>
              )}

            <div className="cart-list">
              {cartItems.map((item) => (
                <div className="cart-item" key={item.cartItemId}>
                  <img
                    src={item.product?.imageUrl || PLACEHOLDER}
                    alt={item.productName}
                    onError={handleImgError}
                  />
                  <div className="cart-item-info">
                    <h4>{item.productName}</h4>
                    <p>R {Number(item.unitPrice).toFixed(2)}</p>

                    <div className="cart-quantity">
                      <button
                        onClick={() =>
                          handleUpdateQuantity(item.cartItemId, item.quantity - 1)
                        }
                        disabled={item.quantity <= 1}
                      >
                        -
                      </button>
                      <span>{item.quantity}</span>
                      <button
                        onClick={() =>
                          handleUpdateQuantity(item.cartItemId, item.quantity + 1)
                        }
                      >
                        +
                      </button>
                    </div>

                    <button
                      className="remove-btn"
                      onClick={() => handleRemoveItem(item.cartItemId)}
                    >
                      Remove
                    </button>
                  </div>
                </div>
              ))}
            </div>

            {/* Delivery options */}
            <div className="delivery-options">
              <label htmlFor="delivery">Delivery Method:</label>
              <select
                id="delivery"
                value={deliveryMethod}
                onChange={(e) => setDeliveryMethod(e.target.value)}
              >
                <option value="pickup">Pickup (No Fee)</option>
                <option value="delivery">Delivery (R 80)</option>
              </select>
            </div>

            <div className="cart-total">
              <h3>Items Total: R {totalItemsAmount.toFixed(2)}</h3>
              <h3>Delivery Fee: R {deliveryFee}</h3>
              <h2>Grand Total: R {grandTotal.toFixed(2)}</h2>
            </div>

            <button className="checkout-btn" onClick={handleCheckout}>
              Checkout
            </button>

            <InsufficientBalanceModal
              show={showModal}
              onClose={() => setShowModal(false)}
              remainingAmount={remainingAmount}
              deductedAmount={deductedAmount}
            />
          </>
        )}
      </div>
    </div>
  );
}
