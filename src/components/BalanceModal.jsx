import React from 'react';
import { Modal, Button } from 'react-bootstrap';

 function BalanceModal({ show, onHide, balance }) {
  return (
    <Modal show={show} onHide={onHide} centered>
      <Modal.Header closeButton>
        <Modal.Title>Your Account Balance</Modal.Title>
      </Modal.Header>
      <Modal.Body>
        <p className="fs-5">Current Balance: <strong>${balance.toFixed(2)}</strong></p>
      </Modal.Body>
      <Modal.Footer>
        <Button variant="primary" onClick={onHide}>Continue</Button>
      </Modal.Footer>
    </Modal>
  );
}

export default BalanceModal;