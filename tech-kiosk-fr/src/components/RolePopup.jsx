import React, { useState } from "react";
import "./RolePopup.css";

export default function RolePopup({ user, token, onClose, onSaveRole }) {
  const [selectedRole, setSelectedRole] = useState(user.roles?.[0] || "");
  const [saving, setSaving] = useState(false);

  const handleSave = async () => {
    if (!selectedRole) return;

    setSaving(true);
    await onSaveRole(user.id, selectedRole);
    setSaving(false);
  };

  return (
    <div className="role-popup-overlay">
      <div className="role-popup">
        <h2>Edit User Role</h2>

        <div className="role-popup-info">
          <p><strong>Name:</strong> {user.name}</p>
          <p><strong>Email:</strong> {user.email}</p>
          <p><strong>Current Role:</strong> {user.roles.join(", ")}</p>
        </div>

        <label className="role-label">Assign New Role</label>
        <select
          value={selectedRole}
          onChange={(e) => setSelectedRole(e.target.value)}
          className="role-select"
        >
          <option value="">Select role</option>
          <option value="User">User</option>
          <option value="Admin">Admin</option>
        </select>

        <div className="role-popup-actions">
          <button className="btn cancel" onClick={onClose}>
            Cancel
          </button>
          <button
            className="btn save"
            onClick={handleSave}
            disabled={saving || !selectedRole}
          >
            {saving ? "Saving..." : "Save Role"}
          </button>
        </div>
      </div>
    </div>
  );
}
