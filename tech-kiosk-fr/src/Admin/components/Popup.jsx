import React from "react";
import "./Popup.css";

export default function Popup({ message, onConfirm, onCancel, type }) {
  return (
    <div className="popup-overlay">
      <div className={`popup ${type === "error" ? "popup-error" : ""}`}>
        <p>{message}</p>

        <div className="popup-actions">
          {type === "confirm" ? (
            <>
              <button className="btn primary" onClick={onConfirm}>Yes</button>
              <button className="btn" onClick={onCancel}>No</button>
            </>
          ) : (
            <button className="btn primary" onClick={onConfirm}>OK</button>
          )}
        </div>
      </div>
    </div>
  );
}
