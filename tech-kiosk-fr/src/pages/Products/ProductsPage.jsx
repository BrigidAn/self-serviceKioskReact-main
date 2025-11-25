import React, { useEffect, useState } from "react";
import { FaShoppingCart } from "react-icons/fa";
import "./ProductsPage.css";

function ProductsPage({ user, setUser }) {
  const [products, setProducts] = useState([]);
  const [cart, setCart] = useState([]);
  const [flipped, setFlipped] = useState({});
  const [hamburgerOpen, setHamburgerOpen] = useState(false);

  const balance = user?.balance || 0;
  const [userBalance, setUserBalance] = useState(balance);

  // Fetch products from API
  useEffect(() => {
    fetch("http://localhost:5000/products")
      .then((res) => res.json())
      .then((data) => setProducts(data))
      .catch((err) => console.log(err));
  }, []);

  // Add to cart
  const addToCart = (product) => {
    if (product.quantity <= 0) return;

    setProducts((prev) =>
      prev.map((p) =>
        p.id === product.id ? { ...p, quantity: p.quantity - 1 } : p
      )
    );

    const exists = cart.find((p) => p.id === product.id);
    if (exists) {
      setCart(cart.map((p) => p.id === product.id ? {...p, quantity: p.quantity + 1} : p));
    } else {
      setCart([...cart, { ...product, quantity: 1 }]);
    }
  };

  // Remove from cart
  const removeFromCart = (product) => {
    setCart(cart.filter((item) => item.id !== product.id));

    setProducts((prev) =>
      prev.map((p) =>
        p.id === product.id ? { ...p, quantity: p.quantity + product.quantity } : p
      )
    );
  };

  // Toggle flip
  const toggleFlip = (id) => {
    setFlipped((prev) => ({ ...prev, [id]: !prev[id] }));
  };

  // Logout
  const handleLogout = () => {
    setUser(null);
    window.location.href = "/";
  };

  return (
    <div className="product-page p-6 min-h-screen bg-gray-100">
      {/* NAVBAR */}
      <nav className="navbar flex justify-between items-center mb-6">
        <div className="logo font-bold text-xl">Tech Shack</div>

        <ul className="nav-links hidden md:flex gap-4">
          <li>Products</li>
          <li>About</li>
          <li>Contact</li>
        </ul>

        {/* Hamburger Menu */}
        <div className="relative">
          <button
            className="hamburger-btn md:hidden text-2xl"
            onClick={() => setHamburgerOpen(!hamburgerOpen)}
          >
            ☰
          </button>
          {hamburgerOpen && (
            <ul className="absolute right-0 top-10 bg-white shadow p-3 rounded">
              <li onClick={() => alert("Account")}>Account</li>
              <li onClick={() => alert("Transactions")}>Transactions</li>
              <li onClick={() => alert("Settings")}>Settings</li>
              <li onClick={handleLogout}>Logout</li>
            </ul>
          )}
        </div>

        {/* Cart Icon */}
        <div className="relative ml-4">
          <FaShoppingCart className="w-8 h-8 cursor-pointer" />
          {cart.length > 0 && (
            <span className="absolute -top-2 -right-2 bg-red-600 text-white text-sm px-2 py-1 rounded-full">
              {cart.length}
            </span>
          )}
        </div>
      </nav>

      {/* PRODUCTS GRID */}
      <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-6">
        {products.map((product) => (
          <div
            key={product.id}
            className={`relative w-full h-60 rounded-xl shadow-lg transform transition-all duration-500 ${
              product.quantity === 0 ? "bg-gray-400 cursor-not-allowed" : "hover:scale-105 hover:shadow-xl cursor-pointer"
            }`}
            style={{
              backgroundColor:
                product.quantity === 0
                  ? "#c5c5c5"
                  : "rgba(174, 207, 184, 0.51)", // AECFB8 @ 51%
            }}
            onClick={() => toggleFlip(product.id)}
          >
            {/* Flip card */}
            <div
              className={`relative w-full h-full transition-transform duration-500 ${
                flipped[product.id] ? "rotate-y-180" : ""
              }`}
              style={{ transformStyle: "preserve-3d" }}
            >
              {/* FRONT */}
              <div
                className="absolute inset-0 flex flex-col justify-center items-center p-4 text-center"
                style={{ backfaceVisibility: "hidden" }}
              >
                <h2 className="text-xl font-semibold mb-2">{product.name}</h2>
                <p className="text-lg">Qty: {product.quantity}</p>
                <p className="font-bold text-lg mt-2">R {product.price}</p>

                {product.quantity > 0 && (
                  <button
                    className="mt-3 bg-green-600 text-white px-3 py-1 rounded hover:bg-green-700"
                    onClick={(e) => {
                      e.stopPropagation();
                      addToCart(product);
                    }}
                  >
                    Add to Cart
                  </button>
                )}
              </div>

              {/* BACK */}
              <div
                className="absolute inset-0 flex flex-col justify-center items-center p-4 text-center bg-white rounded-xl shadow-lg"
                style={{
                  transform: "rotateY(180deg)",
                  backfaceVisibility: "hidden",
                }}
              >
                <h2 className="text-xl font-bold mb-2">Description</h2>
                <p className="text-gray-700">{product.description}</p>
              </div>
            </div>

            {/* Out of stock ribbon */}
            {product.quantity === 0 && (
              <span className="absolute top-3 right-3 bg-red-600 text-white px-2 py-1 text-xs font-bold rounded">
                Unavailable
              </span>
            )}
          </div>
        ))}
      </div>
    </div>
  );
}

export default ProductsPage;
