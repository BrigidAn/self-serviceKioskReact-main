import React from "react";
import mouseImg from "../assets/mouse.png";
import keyboardImg from "../assets/keyboard.png";
import monitorImg from "../assets/monitor.png";

function Home({ addToCart }) {
  const products = [
    { id: 1, name: "Wireless Mouse", price: 199, image: mouseImg },
    { id: 2, name: "USB Keyboard", price: 299, image: keyboardImg },
    { id: 3, name: "HD Monitor", price: 1599, image: monitorImg },
  ];

  return (
    <div className="container mt-5">
      <h2 className="text-center mb-4">Available Products</h2>
      <div className="row justify-content-center">
        {products.map((p) => (
          <div key={p.id} className="col-md-4 mb-4 d-flex justify-content-center">
            <div className="card text-center p-3" style={{ width: "18rem" }}>
              <img src={p.image} className="card-img-top" alt={p.name} style={{ height: "200px", objectFit: "cover", borderRadius: "15px" }}/>
              <div className="card-body">
                <h5 className="card-title">{p.name}</h5>
                <p className="card-text">R{p.price}</p>
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

export default Home;
