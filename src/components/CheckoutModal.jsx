import React, { useState } from 'react';
import { Modal, Button, Form } from 'react-bootstrap';
import { useAuth } from '../context/AuthContext';
import api from "../api";

function CheckoutModal({ show, onHide, total, onConfirm }) {
  const { balance, setBalance, refreshBalance } = useAuth();
  const [topUpAmount, setTopUpAmount] = useState("");
  const [showTopUp, setShowTopUp] = useState(false);
  const [loading, setLoading] = useState(false);

  const handleTopUp = async () => {
    const amount = parseFloat(topUpAmount);
    if (isNaN(amount) || amount <= 0) return;
    setLoading(true);
    try {
      const res = await api.post("/Account/topup", amount);
      setBalance(res.data.balance ?? (balance + amount));
      setTopUpAmount("");
      setShowTopUp(false);
      await refreshBalance();
    } catch (err) {
      console.error(err);
      alert(err.response?.data?.message || "Top-up failed");
    } finally {
      setLoading(false);
    }
  };

  const handleConfirm = () => {
    if (balance >= total) {
      onConfirm();
    } else {
      setShowTopUp(true);
    }
  };

  return (
    <Modal show={show} onHide={onHide} centered>
      <Modal.Header closeButton>
        <Modal.Title>Confirm Purchase</Modal.Title>
      </Modal.Header>

      <Modal.Body>
        <p><strong>Cart Total:</strong> R{Number(total).toFixed(2)}</p>
        <p><strong>Available Balance:</strong> R{Number(balance).toFixed(2)}</p>

        {balance < total && !showTopUp && (
          <p className="text-danger mt-3">
            Insufficient balance! You need R{Math.abs(balance - total).toFixed(2)} more.
          </p>
        )}

        {showTopUp && (
          <div className="mt-3">
            <Form.Label>Enter Top-Up Amount:</Form.Label>
            <Form.Control
              type="number"
              placeholder="Enter amount"
              value={topUpAmount}
              onChange={(e) => setTopUpAmount(e.target.value)}
            />
            <Button variant="success" className="mt-2 w-100" onClick={handleTopUp} disabled={loading}>
              {loading ? "Processing..." : "Add Funds"}
            </Button>
          </div>
        )}
      </Modal.Body>

      <Modal.Footer>
        {!showTopUp ? (
          <>
            <Button variant="secondary" onClick={onHide}>Cancel</Button>
            <Button variant="primary" onClick={handleConfirm}>
              {balance >= total ? 'Confirm Purchase' : 'Add Funds' }
            </Button>
          </>
        ) : (
          <Button variant="outline-secondary" onClick={() => setShowTopUp(false)}>Back</Button>
        )}
      </Modal.Footer>
    </Modal>
  );
}

export default CheckoutModal;