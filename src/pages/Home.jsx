import React from "react";
import mouseImg from "../assets/mouse.png";
import keyboardImg from "../assets/keyboard.png";
import monitorImg from "../assets/monitor.png";
import adapterImg from "../assets/adapter.png";
import routerImg from "../assets/router.png";
import cpuImage from "../assets/cpu.png";
import hostingImg from "../assets/hosting.png";

function Home({ addToCart }) {
  const products = [
    { id: 1, name: "Wireless Mouse", price: 199, image: mouseImg, avaliable: true },
    { id: 2, name: "USB Keyboard", price: 299, image: keyboardImg, avaliable: true },
    { id: 3, name: "HD Monitor", price: 1599, image: monitorImg, avaliable: false },
    { id: 4, name: "Adapter", price: 299, image: adapterImg, avaliable: false },
    { id: 5, name: "Router", price: 1350, image: routerImg, avaliable: true },
    { id: 6, name: "CPU Unit", price: 35000, image: cpuImage, avaliable: true },
    { id: 7, name: "Web Hosting Service", price: 20000, image: hostingImg, avaliable: false },
  ];

  return (
    <div className="container mt-5">
      <h2 className="text-center mb-4">Self-Service Kiosk</h2>
      <div className="row justify-content-center">
        {products.map((p) => (
          <div key={p.id} className="col-md-4 mb-4 d-flex justify-content-center">

            <div className="card text-center p-3" style={{ width: "18rem", opacity: p.avaliable ? 1 : 0.5, 
              pointerEvents: p.avaliable ? "auto" : "none", filter : p.avaliable ? "none" : "grayscale(100%)", boxShadow: "0 6px 15px rgba(0, 0, 0, 0.2)", // 👈 floating shadow
              borderRadius: "15px",transition: "all 0.3s ease", }}onMouseEnter={(e) => {if (p.available)e.currentTarget.style.transform = "translateY(-8px)";
                e.currentTarget.style.boxShadow = "0 10px 20px rgba(0, 0, 0, 0.25)";
            }}onMouseLeave={(e) => {if (p.available)
                e.currentTarget.style.transform = "translateY(0)";
                e.currentTarget.style.boxShadow = "0 4px 10px rgba(0, 0, 0, 0.15)";
            }}>
                
              <img src={p.image} className="card-img-top" alt={p.name} style={{ height: "200px", objectFit: "cover", borderRadius: "15px"}}/>
              <div className="card-body">
                <h5 className="card-title">{p.name}</h5>
                <p className="card-text">R{p.price}</p>
                {p.avaliable ? (<button className="btn btn-primary" onClick={() => addToCart(p)}>
                      Add to Cart
                </button>
                ) : (
                  <button className="ribbon" disabled>
                    Not Avaliable
                  </button>
                )}
              </div>
            </div>
          </div>
        ))}
      </div>
    </div>
  );
}

export default Home;
