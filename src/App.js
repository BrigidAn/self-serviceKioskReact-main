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
import AdminDashboard from "./admin/AdminDashboard";
import ManageProducts from "./admin/ManageProducts";
import ManageOrders from "./admin/ManageOrders";
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
            <Route path="/admin-dashboard" element={<AdminDashboard />} /> 
            <Route path="/home" element={<Home addToCart={addToCart} />} />
            <Route path="/admin/products" element={<ManageProducts />} />
            <Route path="/admin/orders" element={<ManageOrders />} />
            <Route path="/cart" element={<Cart cart={cart} removeFromCart={removeFromCart} />} />
            <Route path="/checkout" element={<Checkout cart={cart} clearCart={clearCart} />} />
            <Route path="*" element={<NotFound />} />
          </Routes>
        </div>
      </Router>
    </AuthProvider>
  );
}

export default App;
