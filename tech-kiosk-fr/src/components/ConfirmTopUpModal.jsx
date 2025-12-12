import "./ConfirmTopUpModal.css";

export default function ConfirmTopUpModal({ show, amount, onConfirm, onCancel }) {
  if (!show) return null;

  return (
    <div className="modal-overlay">
      <div className="modal-content">

        <h2 className="modal-title">Confirm Top-Up</h2>

        <p className="modal-message">
          Are you sure you want to add <strong>R {amount.toFixed(2)}</strong> to your account?
        </p>

        <div className="modal-btn-row">
          <button className="modal-confirm-btn" onClick={onConfirm}>Yes</button>
          <button className="modal-cancel-btn" onClick={onCancel}>No</button>
        </div>

      </div>
    </div>
  );
}
