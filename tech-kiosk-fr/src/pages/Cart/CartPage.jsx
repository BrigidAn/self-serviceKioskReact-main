import React, { useEffect, useState } from "react";
import { useNavigate } from "react-router-dom";

const API_URL = "https://localhost:5016/api";
const token = localStorage.getItem("token");

export default function CartPage() {
  const navigate = useNavigate();
  const [cart, setCart] = useState([]);
  const [loading, setLoading] = useState(true);
  const [total, setTotal] = useState(0);

  // Fetch cart from backend
  const fetchCart = async () => {
    try {
      const res = await fetch(`${API_URL}/cart`, {
        headers: { Authorization: `Bearer ${token}` },
      });
      if (!res.ok) throw new Error("Failed to fetch cart");
      const data = await res.json();
      setCart(data.items || []);
      setTotal(data.items?.reduce((sum, i) => sum + i.unitPrice * i.quantity, 0) || 0);
    } catch (err) {
      console.error(err);
      setCart([]);
      setTotal(0);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchCart();
  }, []);

  const handleRemove = async (cartItemId) => {
    try {
      const res = await fetch(`${API_URL}/cart/item/${cartItemId}`, {
        method: "DELETE",
        headers: { Authorization: `Bearer ${token}` },
      });
      if (!res.ok) throw new Error("Failed to remove item");
      await fetchCart();
    } catch (err) {
      console.error(err);
      alert(err.message);
    }
  };

  const handleCheckout = () => {
    navigate("/checkout");
  };

  if (loading) return <p>Loading cart...</p>;

  if (cart.length === 0) return <p>Your cart is empty.</p>;

  return (
    <div className="p-4">
      <h2 className="text-2xl font-bold mb-4">Your Cart</h2>
      <div className="space-y-2">
        {cart.map((item) => (
          <div key={item.cartItemId} className="flex justify-between items-center border p-2 rounded">
            <div>
              {item.productName} x {item.quantity}
            </div>
            <div>R {(item.unitPrice * item.quantity).toFixed(2)}</div>
            <button
              className="text-red-500 ml-4"
              onClick={() => handleRemove(item.cartItemId)}
            >
              Remove
            </button>
          </div>
        ))}
      </div>
      <p className="mt-4 font-bold">Total: R {total.toFixed(2)}</p>
      <button
        className="mt-4 px-4 py-2 bg-blue-600 text-white rounded"
        onClick={handleCheckout}
      >
        Checkout
      </button>
    </div>
  );
}
