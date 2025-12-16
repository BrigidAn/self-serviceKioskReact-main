import React from "react";
import "./ConfirmTopUpModal.css"; // reuse same styles if you want

export default function DeleteConfirmPopup({
  show,
  message,
  onDelete,
  onCancel,
}) {
  if (!show) return null;

  return (
    <div className="popup-overlay">
      <div className="popup">
        <p>{message}</p>

        <div className="popup-actions">
          <button className="btn danger" onClick={onDelete}>
            Delete
          </button>

          <button className="btn" onClick={onCancel}>
            Cancel
          </button>
        </div>
      </div>
    </div>
  );
}
