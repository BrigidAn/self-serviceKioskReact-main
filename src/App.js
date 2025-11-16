import React, { useEffect, useState } from "react";
import { BrowserRouter as Router, Routes, Route } from "react-router-dom";
import Navbar from "./components/Navbar";
import Home from "./pages/Home";
import { AuthProvider } from './context/AuthContext';
import Cart from "./pages/Cart";
import Checkout from "./pages/Checkout";
import LandingPage from './pages/LandingPage';
import NotFound from "./pages/NotFound";
import Account from "./pages/Account";
import ProtectedRoute from "./components/ProtectedRoute";
import "./App.css";

function App() {
  const [cart, setCart] = useState(() => {
    const savedCart = localStorage.getItem("cart");
    return savedCart ? JSON.parse(savedCart) : [];
  });

  useEffect(() => {
    localStorage.setItem("cart", JSON.stringify(cart));
  }, [cart]);

  const addToCart = (product) => setCart([...cart, product]);

  const removeFromCart = (index) => {
    const newCart = [...cart];
    newCart.splice(index, 1);
    setCart(newCart);
  };

  const clearCart = () => setCart([]);

  return (
    <AuthProvider>
      <Router>
        <Navbar cartCount={cart.length} />
        <div className="container mt-3">
          <Routes>
            <Route path="/" element={<LandingPage />} />
            <Route path="/account" element={<Account/>} />
            <Route path="/home" element={<Home addToCart={addToCart} />} />
            <Route path="/cart" element={<ProtectedRoute><Cart cart={cart} removeFromCart={removeFromCart} /></ProtectedRoute>} />
            <Route path="/checkout" element={<ProtectedRoute><Checkout cart={cart} clearCart={clearCart} /></ProtectedRoute>} />
            <Route path="*" element={<NotFound />} />
          </Routes>
        </div>
      </Router>
    </AuthProvider>
  );
}

export default App;
