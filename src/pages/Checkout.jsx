import React, { useState } from "react";
import { useLocation } from "react-router-dom";
import CheckoutModal from '../components/CheckoutModal';

function Checkout({ cart: propCart = [], clearCart }) {
  const location = useLocation();
  const cart = location.state?.cart || propCart;

  const [step, setStep] = useState(1);
  const [showModal, setShowModal] = useState(false);
  const [formData, setFormData] = useState({
    name: "",
    cardNumber: "",
    expiry: "",
    cvv: "",
  });

  const total = cart.reduce((sum, item) => sum + item.price, 0);

  const handleChange = (e) => {
    setFormData({ ...formData, [e.target.name]: e.target.value });
  };

  const handleConfirmPurchase = () => {
    clearCart();
    setShowModal(false);
    setStep(3);
  };

  const nextStep = () => setStep((prev) => prev + 1);
  const prevStep = () => setStep((prev) => prev - 1);

  const handleSubmit = (e) => {
    e.preventDefault();
    setShowModal(true); // trigger the modal on submit
  };

  return (
    <div className="container mt-5 mb-5">
      <h2 className="text-center mb-4">Checkout</h2>

      {cart.length === 0 && step === 1 && (
        <p className="text-center">Your cart is empty.</p>
      )}

      {step === 1 && cart.length > 0 && (
        <div className="text-center">
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
        </div>
      )}

      {step === 2 && (
        <form
          onSubmit={handleSubmit}
          className="col-md-6 mx-auto border p-4 rounded bg-light shadow-sm"
        >
          <h4 className="mb-3 text-center">Payment Information</h4>

          <div className="mb-3">
            <label className="form-label">Full Name on Card</label>
            <input
              type="text"
              name="name"
              className="form-control"
              required
              value={formData.name}
              onChange={handleChange}
            />
          </div>

          <div className="mb-3">
            <label className="form-label">Card Number</label>
            <input
              type="text"
              name="cardNumber"
              className="form-control"
              maxLength="16"
              required
              value={formData.cardNumber}
              onChange={handleChange}
            />
          </div>

          <div className="row">
            <div className="col-md-6 mb-3">
              <label className="form-label">Expiry Date</label>
              <input
                type="text"
                name="expiry"
                className="form-control"
                placeholder="MM/YY"
                maxLength="5"
                required
                value={formData.expiry}
                onChange={handleChange}
              />
            </div>

            <div className="col-md-6 mb-3">
              <label className="form-label">CVV</label>
              <input
                type="password"
                name="cvv"
                className="form-control"
                placeholder="***"
                maxLength="3"
                required
                value={formData.cvv}
                onChange={handleChange}
              />
            </div>
          </div>

          <div className="d-flex justify-content-between">
            <button
              type="button"
              className="btn btn-secondary"
              onClick={prevStep}
            >
              Back
            </button>
            <button type="submit" className="btn btn-success">
              Confirm Payment
            </button>
          </div>
        </form>
      )}

      {step === 3 && (
        <div className="text-center mt-5">
          <h3>Payment Successful!</h3>
          <p className="lead">Thank you for your purchase.</p>
          <button className="btn btn-dark mt-3" onClick={() => setStep(1)}>
            Back To Home
          </button>
        </div>
      )}

      <CheckoutModal
        show={showModal}
        onHide={() => setShowModal(false)}
        total={total}
        onConfirm={handleConfirmPurchase}
      />
    </div>
  );
}

export default Checkout;
