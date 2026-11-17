import React, { useEffect, useState } from "react";
import { useNavigate } from "react-router-dom";
import "./CheckoutPage.css";
import InsufficientBalanceModal from "../../components/InsufficientBalanceModal";
import { Button, Modal } from "../../components/ui";
import { useCartTimer } from "../../context/CartTimerContext";
import toast, { Toaster } from "react-hot-toast";
import { API_URL as BASE_API_URL } from "../../api/config";

const API_URL = `${BASE_API_URL}`;

export default function CheckoutPage() {
  const token = localStorage.getItem("token");
  const navigate = useNavigate();
  const { refreshCart } = useCartTimer();
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

      if (!res.ok) {
        const err = await res.json().catch(() => ({}));
        throw new Error(err.message || "Top-up failed.");
      }

      setBalance(prev => prev + Number(topUpAmount));
      setTopUpAmount("");

      if (balance + Number(topUpAmount) >= totalItemsPrice + deliveryFee) {
        setShowInsufficientModal(false);
      }

      toast.success("Top-up successful!");
    } catch (err) {
      console.error(err);
      toast.error(err.message);
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
      refreshCart(); // cart is checked out, so the floating timer disappears
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
      <Button variant="secondary" className="checkout-back" onClick={() => navigate("/products")}>
        ← Back
      </Button>

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

      <Button size="lg" block onClick={handleCheckout}>
        Confirm checkout
      </Button>

       <InsufficientBalanceModal
        show={showInsufficientModal}
        onClose={() => setShowInsufficientModal(false)}
        remainingAmount={amountNeeded}
      >
        <div className="checkout-topup">
          <input
            type="number"
            min="0"
            placeholder="Enter top-up amount"
            value={topUpAmount}
            onChange={(e) => setTopUpAmount(e.target.value)}
            className="ui-input"
            aria-label="Top-up amount"
          />
          <Button onClick={handleTopUp}>Top up</Button>
        </div>
      </InsufficientBalanceModal>

      <Modal
        open={Boolean(orderSummary)}
        onClose={() => navigate("/products")}
        title="🎉 Thank you for your order!"
        size="sm"
        closeOnOverlay={false}
        footer={
          <>
            <Button variant="secondary" onClick={() => navigate("/account")}>Top up wallet</Button>
            <Button onClick={() => navigate("/products")}>Continue shopping</Button>
          </>
        }
      >
        {orderSummary && (
          <>
            <p>Delivery fee: R {orderSummary.deliveryFee.toFixed(2)}</p>
            <p>Total paid: <strong>R {orderSummary.total.toFixed(2)}</strong></p>
            <p>Remaining balance: R {balance.toFixed(2)}</p>
          </>
        )}
      </Modal>
      </div>
    </>
  );
}
