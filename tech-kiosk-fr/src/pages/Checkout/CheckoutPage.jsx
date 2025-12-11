import React, { useEffect, useState } from "react";
import { useNavigate } from "react-router-dom";

const API_URL = "https://localhost:5016/api";
const token = localStorage.getItem("token");

export default function CheckoutPage() {
  const navigate = useNavigate();
  const [cart, setCart] = useState([]);
  const [balance, setBalance] = useState(0);
  const [orderSummary, setOrderSummary] = useState(null);
  const [loading, setLoading] = useState(true);

  const fetchCart = async () => {
    try {
      const res = await fetch(`${API_URL}/cart`, {
        headers: { Authorization: `Bearer ${token}` },
      });
      if (!res.ok) throw new Error("Failed to fetch cart");
      const data = await res.json();
      setCart(data.items || []);
      setBalance(data.accountBalance || 0); // if you want to fetch user's balance
    } catch (err) {
      console.error(err);
      setCart([]);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchCart();
  }, []);

  const totalItemsPrice = cart.reduce(
    (sum, item) => sum + item.unitPrice * item.quantity,
    0
  );

  const handleCheckout = async (deliveryMethod = "pickup") => {
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

      setOrderSummary({
        items: cart,
        total: data.totalAmount,
        deliveryFee: data.deliveryFee,
      });

      setCart([]);
      setBalance(prev => prev - data.totalAmount);
    } catch (err) {
      console.error(err);
      alert(err.message);
    }
  };

  if (loading) return <p>Loading checkout...</p>;
  if (cart.length === 0) return <p>Your cart is empty.</p>;

  return (
    <div className="p-4">
      <h2 className="text-2xl font-bold mb-4">Checkout</h2>
      <p>Balance: R {balance.toFixed(2)}</p>
      <p>Total: R {totalItemsPrice.toFixed(2)}</p>

      <div className="space-y-2 mt-4">
        {cart.map((i) => (
          <div key={i.cartItemId} className="flex justify-between">
            <div>{i.productName} x {i.quantity}</div>
            <div>R {(i.unitPrice * i.quantity).toFixed(2)}</div>
          </div>
        ))}
      </div>

      <button
        className="mt-4 px-4 py-2 bg-green-600 text-white rounded"
        onClick={() => handleCheckout("pickup")}
      >
        Confirm Checkout
      </button>

      {orderSummary && (
        <div className="mt-6 p-4 border rounded bg-gray-100">
          <h3 className="font-bold text-lg">Thank You!</h3>
          <p>Delivery Fee: R {orderSummary.deliveryFee.toFixed(2)}</p>
          <p>Total Paid: R {orderSummary.total.toFixed(2)}</p>
        </div>
      )}
    </div>
  );
}
