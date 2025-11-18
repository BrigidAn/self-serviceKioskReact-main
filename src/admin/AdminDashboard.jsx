import AdminNavbar from "./AdminNavbar";

function AdminDashboard() {
  return (
    <div>
      <AdminNavbar />
      <div className="container mt-4">
        <h1>Welcome, Admin!</h1>
        <div className="row mt-4">
          <div className="col-md-4">
            <div className="card text-white bg-primary mb-3">
              <div className="card-body">
                <h5 className="card-title">Total Users</h5>
                <p className="card-text">123</p>
              </div>
            </div>
          </div>
          <div className="col-md-4">
            <div className="card text-white bg-success mb-3">
              <div className="card-body">
                <h5 className="card-title">Total Orders</h5>
                <p className="card-text">45</p>
              </div>
            </div>
          </div>
          <div className="col-md-4">
            <div className="card text-white bg-warning mb-3">
              <div className="card-body">
                <h5 className="card-title">Products</h5>
                <p className="card-text">67</p>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
}

export default AdminDashboard;
