import React, { useState } from "react";
import { BrowserRouter as Router, Routes, Route } from "react-router-dom";
import LandingPage from "./pages/LandingPage/LandingPage";
import ProductsPage from "./pages/Products/ProductsPage";
import Cart from "./pages/Cart/CartPage";
import SupportPage from "./pages/Support/SupportPage";
import AccountPage from "./pages/Account/AccountPage";
import TransactionsPage from "./pages/TransactionsPage/TransactionsPage";
import AboutPage from "./pages/About/AboutPage";
import HistoryPage from "./pages/HistoryPage/HistoryPage";
import Auth from "./pages/Auth/Auth";
import CheckoutPage from "./pages/Checkout/CheckoutPage";
import AdminDashboard from "./admin/AdminDashboard/AdminDashboard";
import ManageUsers from "./admin/ManageUsers/ManageUsers";
import ManageProducts from "./admin/ManageProducts/ManageProducts";
import AdminLogs from "./admin/AdminLog/AdminLogs";

function App() {
  const [user, setUser] = useState(null);

  return (
    <Router>
      <Routes>
        <Route path="/" element={<Auth/>} />
        <Route path="/landing" element={<LandingPage  />} />
        <Route path="/products" element={<ProductsPage user={user} setUser={setUser} />} />
        <Route path="/cart" element={<Cart user={user} setUser={setUser} />} />
        <Route path="/support" element={<SupportPage />} />
        <Route path="/transaction" element={<TransactionsPage />} />
        <Route path="/account" element={<AccountPage />} /> 
        <Route path="/about" element={<AboutPage />} /> 
        <Route path="/orders" element={<HistoryPage/>} />
        <Route path="/transactions" element={<TransactionsPage/>} />
        <Route path="/checkout" element={<CheckoutPage/>} />
        <Route path="/admin/dashboard" element={<AdminDashboard />} />
<Route path="/admin/users" element={<ManageUsers />} />
<Route path="/admin/products" element={<ManageProducts />} />
<Route path="/admin/logs" element={<AdminLogs />} />

      </Routes>
    </Router>
  );
}

export default App;
