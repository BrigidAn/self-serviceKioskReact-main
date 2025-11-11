import React from "react";
import { useNavigate, Link } from "react-router-dom";

function Cart({ cart, removeFromCart }) {
  const navigate = useNavigate();

  const total = cart.reduce((sum, item) => sum + item.price, 0);

  const goToCheckout = () => {
    navigate("/checkout", { state: { cart } });
  };

  return (
    <div className="container mt-5">
      <h2 className="text-center mb-4">Your Cart</h2>

      {cart.length === 0 ? (
        <div className="text-center">
          <p className="lead">No items in your cart.</p>
          <Link to="/home" className="btn btn-primary mt-3">
            Continue Shopping
          </Link>
        </div>
      ) : (
        <>
          <div className="table-responsive">
            <table className="table table-bordered table-striped align-middle">
              <thead className="table-dark">
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
                    <td>R{item.price.toFixed(2)}</td>
                    <td>
                      <button
                        className="btn btn-danger btn-sm"
                        onClick={() => removeFromCart(index)}
                      >
                        Remove
                      </button>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>

          <div className="d-flex justify-content-between align-items-center mt-4">
            <h4>Total: R{total.toFixed(2)}</h4>
            <button onClick={goToCheckout} className="btn btn-success btn-lg">
              Proceed to Checkout
            </button>
          </div>
        </>
      )}
    </div>
  );
}

export default Cart;
