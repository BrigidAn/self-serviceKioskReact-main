import React, { useState, useEffect } from "react";
import AdminLayout from "../AdminLayout";
import "./ManageUsers.css";
import Popup from "../components/Popup";
import { toast } from "react-toastify";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import { faEye, faEyeSlash } from "@fortawesome/free-solid-svg-icons";

const ADMIN_API = "https://localhost:5016/api/Admin";
const AUTH_API = "https://localhost:5016/api/auth";

function ManageUsers() {
  const token = localStorage.getItem("token");
const [showPassword, setShowPassword] = useState(false);

  // Data
  const [users, setUsers] = useState([]);
  const [loading, setLoading] = useState(true);

  // Pagination
  const [page, setPage] = useState(1);
  const [pageSize] = useState(10);
  const [total, setTotal] = useState(0);
  const [currentPage, setCurrentPage] = useState(1);

  // Funds
  const [amounts, setAmounts] = useState({});

  // Popups
  const [popup, setPopup] = useState(null);
  const [rolePopup, setRolePopup] = useState(null);
  const [createPopup, setCreatePopup] = useState(null);
  const [createErrors, setCreateErrors] = useState({});

  // Search
  const [search, setSearch] = useState("");

  /* ---------------- FETCH USERS ---------------- */
  const fetchUsers = async () => {
    setLoading(true);
    try {
      const res = await fetch(
        `${ADMIN_API}/users?page=${currentPage}&pageSize=${pageSize}`,
        { headers: { Authorization: `Bearer ${token}` } }
      );
      const data = await res.json();
      if (!res.ok) throw new Error("Failed to fetch users");

      setUsers(data.data || []);
      setTotal(data.total);
    } catch (err) {
      toast.error(err.message);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchUsers();
  }, [currentPage]);

  const handlePageChange = (page) => {
    if (page < 1 || page > totalPages) return;
    setCurrentPage(page);
  };

  /* ---------------- TOP UP ---------------- */
  const requestTopUp = (userId) => {
    const amount = Number(amounts[userId]);
    if (!amount || amount <= 0) {
      setPopup({
        show: true,
        type: "error",
        message: "Please enter a valid amount greater than 0",
      });
      return;
    }
    if (amount > 1500) {
      setPopup({
        show: true,
        type: "error",
        userId,
        amount,
        message: "You cannot exceed R1500 per deposit",
      });
      return;
    }
    setPopup({
      show: true,
      type: "confirm",
      userId,
      amount,
      message: `Are you sure you want to top up R${amount.toFixed(2)} for this user?`,
    });
  };

  const confirmTopUp = async () => {
    try {
      const res = await fetch(`${ADMIN_API}/topup`, {
        method: "POST",
        headers: { "Content-Type": "application/json", Authorization: `Bearer ${token}` },
        body: JSON.stringify({ UserId: popup.userId, Amount: popup.amount }),
      });

      const data = await res.json();
      if (!res.ok) throw new Error(data.message);

      toast.success("Balance updated");
      setAmounts({});
      fetchUsers();
    } catch (err) {
      toast.error(err.message);
    } finally {
      setPopup(null);
    }
  };

  /* ---------------- ASSIGN ROLE ---------------- */
  const confirmAssignRole = async () => {
    try {
      const res = await fetch(`${AUTH_API}/assign-role`, {
        method: "POST",
        headers: { "Content-Type": "application/json", Authorization: `Bearer ${token}` },
        body: JSON.stringify({ userId: rolePopup.user.id.toString(), role: rolePopup.role }),
      });

      const data = await res.json();
      if (!res.ok) throw new Error(data.message);

      toast.success("Role updated");
      fetchUsers();
    } catch (err) {
      toast.error(err.message);
    } finally {
      setRolePopup(null);
    }
  };

  /* ---------------- CREATE USER ---------------- */
  const confirmCreateUser = async () => {
    try {
      const { name, email, password, role } = createPopup;

      // Validation
      const errors = {};
      if (!name) errors.name = "Name is required";
      if (!email) errors.email = "Email is required";
      if (!password) errors.password = "Password is required";

      const emailExists = users.some(u => u.email.toLowerCase() === email?.toLowerCase());
      if (emailExists) errors.email = "Email already exists";

      setCreateErrors(errors);
      if (Object.keys(errors).length > 0) return;

      // Create user
      const res = await fetch(`${AUTH_API}/register`, {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ name, email, password }),
      });
      const data = await res.json();
      if (!res.ok) throw new Error(data.message || "Registration failed");

      // Assign role if not default
      if (role && role !== "User") {
        const roleRes = await fetch(`${AUTH_API}/assign-role`, {
          method: "POST",
          headers: { "Content-Type": "application/json", Authorization: `Bearer ${token}` },
          body: JSON.stringify({ userId: data.userId, role }),
        });
        if (!roleRes.ok) throw new Error("Role assignment failed");
      }

      toast.success("User created successfully");
      fetchUsers();
      setCreatePopup(null);
    } catch (err) {
      toast.error(err.message || "Something went wrong");
    }
  };

  const totalPages = Math.ceil(total / pageSize);

  // Filter users by search
  const filteredUsers = users.filter(
    u => u.name.toLowerCase().includes(search.toLowerCase()) || u.email.toLowerCase().includes(search.toLowerCase())
  );

  return (
    <AdminLayout>
      <div className="header-row">
        <h1>Manage Users</h1>
        <button onClick={() => { setCreatePopup({}); setCreateErrors({}); }}>+ Add User</button>
      </div>

      {/* SEARCH */}
      <input
        type="text"
        placeholder="Search users..."
        value={search}
        onChange={(e) => setSearch(e.target.value)}
        className="user-search"
      />

      {loading ? (
        <p>Loading users...</p>
      ) : (
        <>
          <table className="admin-table">
            <thead>
              <tr>
                <th>Name</th>
                <th>Email</th>
                <th>Role</th>
                <th>Balance</th>
                <th>Top Up</th>
                <th>Actions</th>
              </tr>
            </thead>
            <tbody>
              {filteredUsers.map(u => (
                <tr key={u.id}>
                  <td>{u.name}</td>
                  <td>{u.email}</td>
                  <td>{u.roles.join(", ")}</td>
                  <td>R {u.balance.toFixed(2)}</td>
                  <td>
                    <input
                      type="number"
                      value={amounts[u.id] || ""}
                      onChange={(e) => setAmounts({ ...amounts, [u.id]: e.target.value })}
                    />
                    <button onClick={() => requestTopUp(u.id)}>Top Up</button>
                  </td>
                  <td>
                    <button
                      onClick={() => setRolePopup({ user: u, role: u.roles[0] })}
                    >
                      Edit Role
                    </button>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>

          {/* PAGINATION */}
          {totalPages > 1 && (
            <div className="p-pagination">
              <button disabled={currentPage === 1} onClick={() => handlePageChange(currentPage - 1)}>Prev</button>
              {[...Array(totalPages)].map((_, i) => (
                <button
                  key={i}
                  className={currentPage === i + 1 ? "active" : ""}
                  onClick={() => handlePageChange(i + 1)}
                >
                  {i + 1}
                </button>
              ))}
              <button disabled={currentPage === totalPages} onClick={() => handlePageChange(currentPage + 1)}>Next</button>
            </div>
          )}
        </>
      )}

      {/* POPUPS */}
      {popup && <Popup type="confirm" message={popup.message} onConfirm={confirmTopUp} onCancel={() => setPopup(null)} />}

      {rolePopup && (
        <Popup
          type="confirm"
          message={
            <select value={rolePopup.role} onChange={(e) => setRolePopup({ ...rolePopup, role: e.target.value })}>
              <option value="User">User</option>
              <option value="Admin">Admin</option>
            </select>
          }
          onConfirm={confirmAssignRole}
          onCancel={() => setRolePopup(null)}
        />
      )}

      {createPopup && (
        <Popup
          type="confirm"
          disableConfirm={Object.values(createErrors).some(err => err)}
          message={
            <div className="form">
              <input placeholder="Name" onChange={(e) => {
                const name = e.target.value;
                setCreatePopup(p => ({ ...p, name }));
                setCreateErrors(prev => ({ ...prev, name: !name ? "Name is required" : "" }));
              }} />
              {createErrors.name && <span className="error">{createErrors.name}</span>}

              <input placeholder="Email" onChange={(e) => {
                const email = e.target.value;
                setCreatePopup(p => ({ ...p, email }));
                const emailExists = users.some(u => u.email.toLowerCase() === email.toLowerCase());
                setCreateErrors(prev => ({ ...prev, email: !email ? "Email is required" : emailExists ? "Email already exists" : "" }));
              }} />
              {createErrors.email && <span className="error">{createErrors.email}</span>}

             <div className="password-wrapper">
                  <input
                    type={showPassword ? "text" : "password"}
                    placeholder="Password"
                    onChange={(e) => setCreatePopup(p => ({ ...p, password: e.target.value }))}
                  />
                  <span
                    className="eye-icon"
                    onClick={() => setShowPassword(prev => !prev)}
                  >
                    <FontAwesomeIcon icon={showPassword ? faEyeSlash : faEye} />
                  </span>
                </div>


            {createErrors.password && (
              <span className="error">{createErrors.password}</span>
            )}

              <select onChange={(e) => setCreatePopup(p => ({ ...p, role: e.target.value }))}>
                <option value="User">User</option>
                <option value="Admin">Admin</option>
              </select>
            </div>
          }
          onConfirm={confirmCreateUser}
          onCancel={() => setCreatePopup(null)}
        />
      )}
    </AdminLayout>
  );
}

export default ManageUsers;
