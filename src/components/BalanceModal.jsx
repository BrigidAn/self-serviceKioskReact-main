import React from 'react';
import { Modal} from 'react-bootstrap';
import { Link } from 'react-router-dom';

 function BalanceModal({ show, onHide, balance }) {
  const {balance} = useAuth();

  return (
    <Modal show={show} onHide={onHide} centered>
      <Modal.Header closeButton>
        <Modal.Title>Your Account Balance</Modal.Title>
      </Modal.Header>
      <Modal.Body>
        <p className="fs-5">Current Balance: <strong>R{Number(balance).toFixed(2)}</strong></p>
      </Modal.Body>
      <Modal.Footer>
        <Link className="btn btn-primary" onClick={onHide} to="/account">Manage Account</Link>
      </Modal.Footer>
    </Modal>
  );
}

export default BalanceModal;