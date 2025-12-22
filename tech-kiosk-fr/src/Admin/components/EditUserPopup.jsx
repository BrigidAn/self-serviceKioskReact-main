import React, { useState } from "react";
import { toast } from "react-toastify";

export default function EditUserPopup({ user, token, onClose, onSaveRole, onTopUp }) {
  const [role, setRole] = useState(user.roles[0] || "");
  const [amount, setAmount] = useState("");

  const handleSave = () => {
    if (!role) {
      toast.error("Please select a role");
      return;
    }
    onSaveRole(user.id, role);
    onClose();
  };

  const handleTopUp = () => {
    const val = Number(amount);
    if (!val || val <= 0) {
      toast.error("Enter a valid amount");
      return;
    }
    onTopUp(user.id, val);
    setAmount("");
  };

  return (
    <div className="popup-overlay">
      <div className="popup-content">
        <h2>Edit User</h2>
        <p><strong>Name:</strong> {user.name}</p>
        <p><strong>Email:</strong> {user.email}</p>
        <p><strong>Current Role:</strong> {user.roles.join(", ")}</p>
        <div>
          <label>Assign Role:</label>
          <select value={role} onChange={(e) => setRole(e.target.value)}>
            <option value="">Select role</option>
            <option value="Admin">Admin</option>
            <option value="User">User</option>
          </select>
        </div>
        <div style={{ marginTop: "10px" }}>
          <label>Top-up:</label>
          <input
            type="number"
            placeholder="Amount"
            value={amount}
            onChange={(e) => setAmount(e.target.value)}
          />
          <button onClick={handleTopUp}>Top Up</button>
        </div>
        <div className="popup-actions">
          <button onClick={handleSave}>Save Role</button>
          <button onClick={onClose}>Cancel</button>
        </div>
      </div>
    </div>
  );
}
