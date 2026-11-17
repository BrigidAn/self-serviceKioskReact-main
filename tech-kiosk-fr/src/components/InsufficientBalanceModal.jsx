import React from "react";
import { Button, Modal } from "./ui";

export default function InsufficientBalanceModal({ show, onClose, remainingAmount, children }) {
  return (
    <Modal
      open={show}
      onClose={onClose}
      title="Insufficient balance"
      tone="danger"
      footer={<Button variant="secondary" onClick={onClose}>Close</Button>}
    >
      <p>Your balance is not enough to complete this purchase.</p>
      <p>
        Amount needed: <strong>R {Number(remainingAmount || 0).toFixed(2)}</strong>
      </p>
      {children}
    </Modal>
  );
}
