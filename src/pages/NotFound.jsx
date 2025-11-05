import React from "react";
import { Link } from "react-router-dom";

function NotFound() {
  return (
    <div className="container mt-5 text-center">
      <h2>404 - Page Not Found</h2>
      <Link to="/" className="btn btn-dark mt-3">Go Home</Link>
    </div>
  );
}

export default NotFound;
