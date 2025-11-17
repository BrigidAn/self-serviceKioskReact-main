import React, { useEffect, useState } from "react";
import "./Home.css";

function Home({ addToCart }) {
  const [products, setProducts] = useState([]);

  // Fetch products from ASP.NET backend on load
  useEffect(() => {
    fetch("https://localhost:5016/api/Product") // <-- your API endpoint
      .then((res) => res.json())
      .then((data) => {
        setProducts(data);
      })
      .catch((error) => console.error("Failed to load products:", error));
  }, []);

  return (
    <div className="container mt-5">
      <h2 className="text-center mb-4 text-light fw-bold">Kiosk</h2>

      <div className="row justify-content-center">
        {products.map((p) => {
          const isAvailable = p.quantity > 0;

          return (
            <div
              key={p.productId}
              className="col-md-4 mb-4 d-flex justify-content-center"
            >
              <div
                className="product-card text-center"
                style={{
                  width: "18rem",
                  opacity: isAvailable ? 1 : 0.5,
                  filter: isAvailable ? "none" : "grayscale(100%)",
                  pointerEvents: isAvailable ? "auto" : "none",
                  boxShadow: "0 6px 15px rgba(0, 0, 0, 0.2)",
                  borderRadius: "15px",
                  transition: "all 0.3s ease",
                }}
                onMouseEnter={(e) => {
                  if (isAvailable) {
                    e.currentTarget.style.transform = "translateY(-8px)";
                    e.currentTarget.style.boxShadow =
                      "0 10px 20px rgba(21, 158, 236, 0.25)";
                  }
                }}
                onMouseLeave={(e) => {
                  e.currentTarget.style.transform = "translateY(0)";
                  e.currentTarget.style.boxShadow =
                    "0 4px 10px rgba(0, 0, 0, 0.15)";
                }}
              >
                {/* --------------------------
                    Product Image
                    If backend image URL missing → use fallback image
                --------------------------- */}
                <img
                  src={
                    p.imageUrl && p.imageUrl.trim() !== ""
                      ? p.imageUrl
                      : "https://via.placeholder.com/300x200?text=No+Image"
                  }
                  className="card-img-top"
                  alt={p.name}
                  style={{
                    height: "200px",
                    objectFit: "cover",
                    borderRadius: "20px",
                  }}
                />

                <div className="card-body">
                  <h5 className="card-title">{p.name}</h5>
                  <p className="card-text">R{p.price}</p>

                  <p className="text-info fw-bold">
                    {isAvailable ? `In Stock: ${p.quantity}` : "Out of Stock"}
                  </p>

                  {isAvailable ? (
                    <button
                      className="btn"
                      style={{
                        width: "40%",
                        padding: "0.8rem 0",
                        background: "rgba(0, 217, 255, 0.1)",
                        color: "#00d9ff",
                        border: "1px solid rgba(0, 217, 255, 0.5)",
                        borderRadius: "12px",
                        fontWeight: "bold",
                        letterSpacing: "0.5px",
                        transition: "all 0.3s ease",
                      }}
                      onMouseEnter={(e) => {
                        e.currentTarget.style.background = "#00d9ff";
                        e.currentTarget.style.color = "#0f2027";
                        e.currentTarget.style.boxShadow =
                          "0 0 20px rgba(0, 217, 255, 0.6)";
                      }}
                      onMouseLeave={(e) => {
                        e.currentTarget.style.background =
                          "rgba(0, 217, 255, 0.1)";
                        e.currentTarget.style.color = "#00d9ff";
                        e.currentTarget.style.boxShadow = "none";
                      }}
                      onClick={() => addToCart(p)}
                    >
                      Add to Cart
                    </button>
                  ) : (
                    <button className="ribbon" disabled>
                      Not Available
                    </button>
                  )}
                </div>
              </div>
            </div>
          );
        })}
      </div>
    </div>
  );
}

export default Home;
