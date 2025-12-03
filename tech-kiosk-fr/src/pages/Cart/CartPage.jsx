import React from "react";
import NavBar from "../../components/Navbar";
import "./CartPage.css";

export default function CartPage() {
  const items = JSON.parse(localStorage.getItem("cart") || "[]");

  return (
    <div>
      <NavBar cartCount={items.length} />

      <div className="velvety-cart-page">
        <h2>Shopping Cart</h2>

        {items.length === 0 ? (
          <p className="empty-cart">Your cart is empty.</p>
        ) : (
          <div className="cart-list">
            {items.map((p) => (
              <div className="cart-item" key={p.id}>
                <img src={p.imageUrl} alt={p.name} />
                <div>
                  <h4>{p.name}</h4>
                  <p>R {Number(p.price).toFixed(2)}</p>
                </div>
              </div>
            ))}
          </div>
        )}
      </div>
    </div>
  );
}
