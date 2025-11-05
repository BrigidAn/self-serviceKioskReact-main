import React from "react";
import {Link} from "react-router-dom"

function Cart({ cart, removeFromCart }) {
  const total = cart.reduce((sum, item) => sum + item.price, 0);

  return (
    <div className="container mt-5">
      <h2 className="text-center mb-4">Your Cart</h2>
      {cart.length === 0 ? (
        <div className="text-center">
        <p className="lead">No items in your cart.</p>
        <Link to="/" className="btn btn-primary mt-3">
        Continue Shopping
        </Link>
        </div>
      ) : (
        <>
          <div className="table-responsive">
          <table className="table table-boarded table-striped align-middle">
        <thead className="table-drak">
        <tr>
          <th>#</th>
          <th>Product</th>
          <th>Price</th>
          <th>Action</th>
        </tr>
        </thead>
        <tbody>
          {cart.map((item, index) => (
            <tr key={index}>
              <td>{index + 1}</td>
              <td>{item.name}</td>
              <td>{item.price.toFixed(2)}</td>

              <td>
                <button className="btn btn-danger btn-sm" onClick={() => removeFromCart(index)}>
                    Remove
                </button>
              </td>
            </tr>
          ))}
          </tbody>
          </table>
      </div>

          <div className="d-flex justify-content-between align-item-center mt-4">
            <h4>Total: R{total.toFixed(2)}</h4>
            <Link to="/checkout" className="btn btn-success btn-lg">
            Proceed to Checkout 
            </Link>
          </div>
        </>
      )}
    </div>
  );
}

export default Cart;
