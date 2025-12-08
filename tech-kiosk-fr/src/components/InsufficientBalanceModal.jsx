// InsufficientBalanceModal.jsx
import React from "react";
import "./InsufficientBalanceModal.css";

export default function InsufficientBalanceModal({ show, onClose, remainingAmount, deductedAmount }) {
  if (!show) return null;

  return (
    <div className="modal-overlay">
      <div className="modal-content">
        <h2>Insufficient Balance</h2>
        <p>
          Your balance is not enough to complete this purchase.
        </p>
        <p>
          Deducted Amount: R {deductedAmount.toFixed(2)}
        </p>
        <p>
          Remaining Amount Needed: R {remainingAmount.toFixed(2)}
        </p>
        <button onClick={onClose} className="modal-close-btn">Close</button>
      </div>
    </div>
  );
}
