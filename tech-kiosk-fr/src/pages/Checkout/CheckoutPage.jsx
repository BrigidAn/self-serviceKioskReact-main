import React, { useEffect, useState } from "react";
import { useNavigate } from "react-router-dom";
import "./CheckoutPage.css";
import InsufficientBalanceModal from "../../components/InsufficientBalanceModal";
import toast, { Toaster } from "react-hot-toast";

const API_URL = "https://localhost:5016/api";
const token = localStorage.getItem("token");

export default function CheckoutPage() {
  const navigate = useNavigate();
  const [cart, setCart] = useState([]);
  const [balance, setBalance] = useState(0);
  const [orderSummary, setOrderSummary] = useState(null);
  const [loading, setLoading] = useState(true);
  const [deliveryMethod, setDeliveryMethod] = useState("pickup");
  const [deliveryFee, setDeliveryFee] = useState(0);

  const [showInsufficientModal, setShowInsufficientModal] = useState(false);
  const [amountNeeded, setAmountNeeded] = useState(0);

  const [topUpAmount, setTopUpAmount] = useState("");

  const fetchCart = async () => {
    try {
      const res = await fetch(`${API_URL}/cart`, {
        headers: { Authorization: `Bearer ${token}` },
      });
      if (!res.ok) throw new Error("Failed to fetch cart");

      const data = await res.json();
      setCart(data.items || []);
    } catch (err) {
      console.error(err);
      setCart([]);
    }
  };

  const fetchBalance = async () => {
    try {
      const res = await fetch(`${API_URL}/Account/balance`, {
        headers: { Authorization: `Bearer ${token}` },
      });

      if (!res.ok) throw new Error("Failed to fetch balance");

      const balanceData = await res.json();
      setBalance(balanceData.balance ?? 0);
    } catch (err) {
      console.error(err);
      setBalance(0);
    }
  };

  useEffect(() => {
    const loadData = async () => {
      await fetchCart();
      await fetchBalance();
      setLoading(false);
    };
    loadData();
  }, []);

  const totalItemsPrice = cart.reduce(
    (sum, item) => sum + item.unitPrice * item.quantity,
    0
  );

  useEffect(() => {
    setDeliveryFee(deliveryMethod === "delivery" ? 80 : 0);
  }, [deliveryMethod]);

  const handleTopUp = async () => {
    if (!topUpAmount || topUpAmount <= 0) return toast("Enter a valid amount");

    try {
      const res = await fetch(`${API_URL}/Account/topup`, {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
          Authorization: `Bearer ${token}`,
        },
        body: JSON.stringify({ amount: Number(topUpAmount) }),
      });

      if (!res.ok) throw new Error("Failed to top up");

      const data = await res.json();
      setBalance(prev => prev + Number(topUpAmount));
      setTopUpAmount("");

      if (balance + Number(topUpAmount) >= totalItemsPrice + deliveryFee) {
        setShowInsufficientModal(false);
      }

      toast.success("Top-up successful!");
    } catch (err) {
      console.error(err);
      toast.error("Top-up failed.");
    }
  };


  const handleCheckout = async () => {

  const totalCost = totalItemsPrice + deliveryFee;

    if (balance < totalCost) {
      setAmountNeeded(totalCost - balance);
      setShowInsufficientModal(true);
      return;
    }

     const toastId = toast.loading("Processing checkout...");

    try {
      const res = await fetch(`${API_URL}/checkout`, {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
          Authorization: `Bearer ${token}`,
        },
        body: JSON.stringify({ deliveryMethod }),
      });

      const data = await res.json();
      if (!res.ok) throw new Error(data.message || "Checkout failed");

    toast.success("Order placed successfully!", { id: toastId });

      setOrderSummary({
        items: cart,
        total: data.totalAmount,
        deliveryFee: data.deliveryFee,
      });

      setCart([]);
      setBalance((prev) => prev - data.totalAmount);
    } catch (err) {
      console.error(err);
      toast.error(err.message);
    }
  };

  return (
    <>
    <Toaster toastOptions={{
    duration: 4000,}}
  className="toaster"
/>

    <div className="checkout-container">
      <button
        className="checkout-back-btn"
        onClick={() => navigate("/products")}
      >
        ← Back
      </button>

      <div className="vp-hero-floating">
                <div className="float-shape fs1"></div>
                <div className="float-shape fs2"></div>
                <div className="float-shape fs3"></div>
              </div>

      <h2 className="checkout-title">Checkout</h2>

      <p className="checkout-balance">Balance: R {balance.toFixed(2)}</p>

      <p className="checkout-total">
        Total Items: R {totalItemsPrice.toFixed(2)}
      </p>

      <p className="checkout-total">
        Total with Delivery: R {(totalItemsPrice + deliveryFee).toFixed(2)}
      </p>

      <div className="checkout-delivery">
        <label>
          <input
            type="radio"
            value="pickup"
            checked={deliveryMethod === "pickup"}
            onChange={(e) => setDeliveryMethod(e.target.value)}
          />
          Pickup (Free)
        </label>

        <label>
          <input
            type="radio"
            value="delivery"
            checked={deliveryMethod === "delivery"}
            onChange={(e) => setDeliveryMethod(e.target.value)}
          />
          Delivery (R80)
        </label>
      </div>

      <div className="checkout-items">
        {cart.map((i) => (
          <div key={i.cartItemId} className="checkout-item-row">
            <div>
              {i.productName} x {i.quantity}
            </div>
            <div>R {(i.unitPrice * i.quantity).toFixed(2)}</div>
          </div>
        ))}
      </div>

      <button className="checkout-confirm-btn" onClick={handleCheckout}>
        Confirm Checkout
      </button>

       <InsufficientBalanceModal
        show={showInsufficientModal}
        onClose={() => setShowInsufficientModal(false)}
        remainingAmount={amountNeeded}
      >
        <div className="topup-container">
          <input
            type="number"
            placeholder="Enter top-up amount"
            value={topUpAmount}
            onChange={(e) => setTopUpAmount(e.target.value)}
            className="modal-input"
          />
          <button className="modal-topup-btn" onClick={handleTopUp}>
            Top Up
          </button>
        </div>
      </InsufficientBalanceModal>

      {orderSummary && (
        <div className="checkout-popup">
          <h3 className="checkout-popup-title">🎉 Thank You!</h3>

          <p>Delivery Fee: R {orderSummary.deliveryFee.toFixed(2)}</p>
          <p>Total Paid: R {orderSummary.total.toFixed(2)}</p>

          <p className="checkout-remaining">
            Remaining Balance: R {balance.toFixed(2)}
          </p>

          <div className="checkout-popup-buttons">
            <button
              className="checkout-popup-btn"
              onClick={() => navigate("/account")}
            >
              Top Up
            </button>

            <button
              className="checkout-popup-btn"
              onClick={() => navigate("/products")}
            >
              Exit
            </button>
          </div>
        </div>
      )}
      </div>
    </>
  );
}
