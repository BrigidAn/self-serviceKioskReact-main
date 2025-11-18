import { useEffect, useState } from "react";
import AdminNavbar from "./AdminNavbar";
import api from "../api";
import { Table, Button } from "react-bootstrap";

function ManageOrders() {
  const [orders, setOrders] = useState([]);

  useEffect(() => {
    fetchOrders();
  }, []);

  const fetchOrders = async () => {
    try {
      const res = await api.get("/Orders");
      setOrders(res.data);
    } catch (err) {
      console.error(err);
    }
  };

  const handleUpdateStatus = async (order, status) => {
    try {
      await api.put(`/Orders/${order.id}`, { ...order, status });
      fetchOrders();
    } catch (err) {
      console.error(err);
    }
  };

  return (
    <div>
      <AdminNavbar />
      <div className="container mt-4">
        <h2>Manage Orders</h2>
        <Table striped bordered hover>
          <thead>
            <tr>
              <th>ID</th>
              <th>User</th>
              <th>Total</th>
              <th>Status</th>
              <th>Actions</th>
            </tr>
          </thead>
          <tbody>
            {orders.map(o => (
              <tr key={o.id}>
                <td>{o.id}</td>
                <td>{o.userEmail}</td>
                <td>R{o.total}</td>
                <td>{o.status}</td>
                <td>
                  <Button variant="success" size="sm" className="me-2" onClick={() => handleUpdateStatus(o, "Completed")}>Complete</Button>
                  <Button variant="danger" size="sm" onClick={() => handleUpdateStatus(o, "Cancelled")}>Cancel</Button>
                </td>
              </tr>
            ))}
          </tbody>
        </Table>
      </div>
    </div>
  );
}

export default ManageOrders;
