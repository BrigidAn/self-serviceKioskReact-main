import React, { useState, useEffect } from "react";
import { useLocation, useNavigate } from "react-router-dom";
import api from "../api";
import { useAuth } from "../context/AuthContext";

function Checkout({ cart: propCart = [], clearCart }) {
 const location = useLocation();
  const navigate = useNavigate();
  const cart = location.state?.cart || propCart;
  const total = cart.reduce((sum, item) => sum + item.price, 0);

  const { balance, setBalance, refreshBalance } = useAuth();
  const [topUpAmount, setTopUpAmount] = useState("");
  const [step, setStep] = useState(1);
  const [loading, setLoading] = useState(false);

  useEffect(() => {
    refreshBalance();
    // eslint-disable-next-line
  }, []);

  const nextStep = () => setStep((s) => s + 1);
  const prevStep = () => setStep((s) => s - 1);

  const handleTopUp = async (e) => {
    e.preventDefault();
    const val = parseFloat(topUpAmount);
    if (!val || val <= 0) return;
    setLoading(true);
    try {
      const res = await api.post("/Account/topup", val);
      setBalance(res.data.balance ?? 0);
      setTopUpAmount("");
      alert("Balance topped up successfully!");
    } catch (err) {
      console.error(err);
      alert(err.response?.data?.message || "Top-up failed.");
    } finally {
      setLoading(false);
    }
  };

  const handlePurchase = async () => {
    if (balance < total) {
      alert("Insufficient balance. Please top up your account.");
      return;
    }
    setLoading(true);
    try {
      // server will resolve account from session
      const res = await api.post("/Transaction", {
        type: "debit",
        totalAmount: total,
        description: "Purchase from cart",
      });
      setBalance(res.data.balance ?? balance - total);
      clearCart();
      setStep(3);
    } catch (err) {
      console.error(err);
      alert(err.response?.data?.message || "Purchase failed.");
    } finally {
      setLoading(false);
      refreshBalance();
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
