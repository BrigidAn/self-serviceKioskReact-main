import React from "react";
import "./InsufficientBalanceModal.css";

export default function InsufficientBalanceModal({ show, onClose, remainingAmount, children }) {
  if (!show) return null;

  return (
    <div className="modal-overlay">
      <div className="modal-content">
        <h2>Insufficient Balance</h2>
        <p>Your balance is not enough to complete this purchase.</p>

        <p>Amount Needed: R {remainingAmount.toFixed(2)}</p>

        {children} {/* Top-up UI goes here */}

        <button onClick={onClose} className="modal-close-btn">Close</button>
      </div>
    </div>
  );
}
