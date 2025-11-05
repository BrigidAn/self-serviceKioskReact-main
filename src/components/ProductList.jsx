import { useEffect, useState } from "react";
import axios from "axios";

function ProductList({ addToCart }) {
  const [products, setProducts] = useState([]);

  useEffect(() => {
    axios.get("https://localhost:5001/api/products") // from backend API
      .then(res => setProducts(res.data))
      .catch(err => console.error("Error loading products", err));
  }, []);

  return (
    <div className="container mt-4">
      <h2>Available Products</h2>
      <div className="row">
        {products.map(p => (
          <div key={p.id} className="col-md-4 mb-3">
            <div className="card">
              <div className="card-body">
                <h5>{p.name}</h5>
                <p>{p.description}</p>
                <p>R{p.price}</p>
                <button className="btn btn-primary" onClick={() => addToCart(p)}>
                  Add to Cart
                </button>
              </div>
            </div>
          </div>
        ))}
      </div>
    </div>
  );
}
export default ProductList;
