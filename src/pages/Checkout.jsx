import React, { useState, useEffect } from "react";
import { useLocation, useNavigate } from "react-router-dom";
import axios from "axios";

function Checkout({ cart: propCart = [], clearCart }) {
  const location = useLocation();
  const navigate = useNavigate();
  const cart = location.state?.cart || propCart;

  const [balance, setBalance] = useState(0);
  const [topUpAmount, setTopUpAmount] = useState("");
  const [step, setStep] = useState(1);
  const [loading, setLoading] = useState(false);
  const total = cart.reduce((sum, item) => sum + item.price, 0);

  // Fetch user balance on mount
  useEffect(() => {
    fetchBalance();
  }, []);

  const fetchBalance = async () => {
    try {
      const res = await axios.get("http://localhost:5000/api/account/balance", {
        headers: { Authorization: `Bearer ${localStorage.getItem("token")}` },
      });
      setBalance(res.data.balance);
    } catch (err) {
      console.error(err);
    }
  };

  // Proceed to payment
  const nextStep = () => setStep((prev) => prev + 1);
  const prevStep = () => setStep((prev) => prev - 1);

  // Top up user account
  const handleTopUp = async (e) => {
    e.preventDefault();
    if (!topUpAmount || parseFloat(topUpAmount) <= 0) return;

    setLoading(true);
    try {
      const res = await axios.post(
        "http://localhost:5000/api/account/topup",
        parseFloat(topUpAmount),
        { headers: { Authorization: `Bearer ${localStorage.getItem("token")}` } }
      );
      setBalance(res.data.balance);
      setTopUpAmount("");
      alert("Balance topped up successfully!");
    } catch (err) {
      console.error(err);
      alert("Failed to top up.");
    } finally {
      setLoading(false);
    }
  };

  // Confirm purchase using account balance
  const handlePurchase = async () => {
    if (balance < total) {
      alert("Insufficient balance. Please top up your account.");
      return;
    }

    try {
      // Call transaction API
      const userId = localStorage.getItem("userId");
      await axios.post(
        "http://localhost:5000/api/transaction",
        {
          accountId: userId, // Or fetch actual accountId from backend
          type: "debit",
          totalAmount: total,
          description: "Purchase from cart",
        },
        { headers: { Authorization: `Bearer ${localStorage.getItem("token")}` } }
      );

      // Update balance locally
      setBalance(balance - total);
      clearCart();
      setStep(3); // Payment success step
    } catch (err) {
      console.error(err);
      alert("Purchase failed.");
    }
  };

  return (
    <div className="container mt-5 mb-5">
      <h2 className="text-center mb-4">Checkout</h2>

      {/* Step 1: Review Cart */}
      {step === 1 && (
        <div className="text-center">
          {cart.length === 0 ? (
            <p>Your cart is empty.</p>
          ) : (
            <>
              <ul className="list-group mb-3">
                {cart.map((item, index) => (
                  <li
                    key={index}
                    className="list-group-item d-flex justify-content-between align-items-center"
                  >
                    {item.name} <span>R{item.price.toFixed(2)}</span>
                  </li>
                ))}
              </ul>
              <h5>Total: R{total.toFixed(2)}</h5>
              <button
                className="btn btn-primary mt-3"
                onClick={nextStep}
                disabled={cart.length === 0}
              >
                Proceed to Payment
              </button>
            </>
          )}
        </div>
      )}

      {/* Step 2: Payment with Account Balance */}
      {step === 2 && (
        <div className="col-md-6 mx-auto border p-4 rounded bg-light shadow-sm">
          <h4 className="mb-3 text-center">Payment Using Account Balance</h4>
          <p>Current Balance: <strong>R{balance.toFixed(2)}</strong></p>
          <p>Total Purchase: <strong>R{total.toFixed(2)}</strong></p>

          {balance < total && (
            <form onSubmit={handleTopUp} className="mb-3">
              <input
                type="number"
                placeholder="Top up amount"
                value={topUpAmount}
                onChange={(e) => setTopUpAmount(e.target.value)}
                className="form-control mb-2"
              />
              <button type="submit" className="btn btn-info" disabled={loading}>
                {loading ? "Processing..." : "Top Up"}
              </button>
            </form>
          )}

          <div className="d-flex justify-content-between">
            <button className="btn btn-secondary" onClick={prevStep}>Back</button>
            <button className="btn btn-success" onClick={handlePurchase} disabled={balance < total}>
              Confirm Purchase
            </button>
          </div>
        </div>
      )}

      {/* Step 3: Payment Success */}
      {step === 3 && (
        <div className="text-center mt-5">
          <h3>Payment Successful!</h3>
          <p className="lead">Thank you for your purchase.</p>
          <button className="btn btn-dark mt-3" onClick={() => navigate("/")}>
            Back To Home
          </button>
        </div>
      )}
    </div>
  );
}

export default Checkout;
