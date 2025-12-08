import React, { useState, useEffect } from "react";
import AdminLayout from "../AdminLayout";
import "./ManageUsers.css";
import { toast } from "react-toastify";
import "react-toastify/dist/ReactToastify.css";

const API_BASE = "https://localhost:5016/api/Admin";

export default function ManageUsers() {
  const [users, setUsers] = useState([]);
  const [loading, setLoading] = useState(true);
  const [amounts, setAmounts] = useState({}); // track input for each user
  const token = localStorage.getItem("token");

  // ------------------- FETCH USERS -------------------
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

  // ------------------- TOP UP USER -------------------
  const topUp = async (userId) => {
    const amount = amounts[userId];
    if (!amount || amount <= 0) {
      toast.info("Enter a valid amount");
      return;
    }

    try {
      const res = await fetch(`${API_BASE}/topup`, {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
          Authorization: `Bearer ${token}`,
        },
        body: JSON.stringify({ UserId: userId, Amount: Number(amount) }),
      });

      const data = await res.json();
      if (!res.ok) throw new Error(data.message || "Top-up failed");

      toast.success(data.message);
      setAmounts({ ...amounts, [userId]: "" }); // clear input
      fetchUsers(); // refresh users with new balance
    } catch (err) {
      console.error(err);
      toast.error(err.message);
    }
  };

  return (
    <AdminLayout>
      <h1>Manage Users</h1>

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
              <th>Add Money</th>
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
                  <button onClick={() => topUp(u.id)}>Top Up</button>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      )}
    </AdminLayout>
  );
}
