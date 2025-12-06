import React, { useState } from "react";
import AdminLayout from "../AdminLayout";
import "./ManageUsers.css"

export default function ManageUsers() {
  const [users, setUsers] = useState(
    JSON.parse(localStorage.getItem("users") || "[]")
  );
  const [amount, setAmount] = useState("");

  const topUp = (id) => {
    if (!amount) return;

    const updated = users.map((u) =>
      u.id === id ? { ...u, balance: (u.balance || 0) + Number(amount) } : u
    );

    localStorage.setItem("users", JSON.stringify(updated));
    setUsers(updated);

    // log transaction
    const logs = JSON.parse(localStorage.getItem("transactions") || "[]");
    logs.push({
      type: "admin_topup",
      amount: Number(amount),
      userId: id,
      date: new Date().toISOString(),
    });
    localStorage.setItem("transactions", JSON.stringify(logs));

    alert("Balance added!");
    setAmount("");
  };

  return (
    <AdminLayout>
      <h1>Manage Users</h1>

      <table className="admin-table">
        <thead>
          <tr>
            <th>User</th>
            <th>Balance</th>
            <th>Add Money</th>
          </tr>
        </thead>

        <tbody>
          {users.map((u) => (
            <tr key={u.id}>
              <td>{u.name}</td>
              <td>R {u.balance?.toFixed(2) || 0}</td>
              <td>
                <input
                  type="number"
                  placeholder="Amount"
                  value={amount}
                  onChange={(e) => setAmount(e.target.value)}
                />
                <button onClick={() => topUp(u.id)}>Top Up</button>
              </td>
            </tr>
          ))}
        </tbody>
      </table>
    </AdminLayout>
  );
}
