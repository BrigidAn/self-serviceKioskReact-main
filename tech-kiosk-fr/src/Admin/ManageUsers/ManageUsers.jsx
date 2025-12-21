import React, { useState, useEffect } from "react";
import AdminLayout from "../AdminLayout";
import "./ManageUsers.css";
import Popup from "../components/Popup";
import { toast } from "react-toastify";

const API_BASE = "https://localhost:5016/api/Admin";

function ManageUsers() {
  const [users, setUsers] = useState([]);
  const [loading, setLoading] = useState(true);
  const [amounts, setAmounts] = useState({});
  const [popup, setPopup] = useState({ show: false, userId: null, amount: 0, message: "" });
  const token = localStorage.getItem("token");

  const fetchUsers = async () => {
    setLoading(true);
    try {
      const res = await fetch(`${API_BASE}/users?page=1&pageSize=50`, {
        headers: { Authorization: `Bearer ${token}` },
      });
      if (!res.ok) throw new Error("Failed to fetch users");

      const data = await res.json();
      setUsers(data.data || []);
    } catch (err) {
      console.error(err);
      toast.error(err.message);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchUsers();
  }, []);

  const requestTopUp = (userId) => {
    const amount = Number(amounts[userId]);
      if (!amount) {
    setPopup({
      show: true,
      type: "error",
      message: "Please enter a valid amount.",
    });
    return;
  }
    if (amount > 1500) {
      setPopup({
        show: true,
        type: "error",
        userId,
        amount,
        message: `You cannot exceed R1500 per deposit`
      });

      return;
    }
    if (amount <0) {
      setPopup({
        show: true,
        type: "error",
        message: "Amount cannont be zero",
      });
      return;
    }

    setPopup({
      show: true,
      type: "confirm",
      userId,
      amount,
      message: `Are you sure you want to top up R${amount.toFixed(2)} for this user?`
    });

  };

  const confirmTopUp = async () => {
    try {
      const { userId, amount } = popup;
      const res = await fetch(`${API_BASE}/topup`, {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
          Authorization: `Bearer ${token}`,
        },
        body: JSON.stringify({ UserId: userId, Amount: amount }),
      });

      const data = await res.json();
      if (!res.ok) throw new Error(data.message || "Top-up failed");

      toast.success(data.message);
      setAmounts({ ...amounts, [userId]: "" });
      fetchUsers();
    } catch (err) {
      console.error(err);
      toast.error(err.message);
    } finally {
      setPopup({ show: false, userId: null, amount: 0, message: "" });
    }
  };

  const cancelTopUp = () => {
    setPopup({ show: false, userId: null, amount: 0, message: "" });
  };

  return (
    <AdminLayout>
      <h1>Manage Users</h1>

      {popup.show && (
        <Popup
          message={popup.message}
          type={popup.type}
          onConfirm={popup.type === "confirm" ? confirmTopUp : () => setPopup({show: false})}
          onCancel={popup.type === "confirm" ? () => setPopup({show: false}): undefined}
        />
      )}


      {loading ? (
        <p>Loading users...</p>
      ) : (
        <table className="admin-table">
          <thead>
            <tr>
              <th>User</th>
              <th>Email</th>
              <th>Roles</th>
              <th>Balance</th>
              <th>Add Funds</th>
            </tr>
          </thead>
          <tbody>
            {users.map((u) => (
              <tr key={u.id}>
                <td>{u.name}</td>
                <td>{u.email}</td>
                <td>{u.roles.join(", ")}</td>
                <td>R {u.balance?.toFixed(2) || 0}</td>
                <td>
                  <input
                    type="number"
                    placeholder="Amount"
                    value={amounts[u.id] || ""}
                    onChange={(e) =>
                      setAmounts({ ...amounts, [u.id]: e.target.value })
                    }
                  />
                  <button onClick={() => requestTopUp(u.id)}>Top Up</button>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      )}
    </AdminLayout>
  );
}

export default ManageUsers;
