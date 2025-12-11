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
import UserRoute from "./components/UserRoute";
import AdminRoute from "./components/AdminRoute";
import AdminLayout from "./admin/AdminLayout";
import AdminOrders from "./admin/AdminOrders/AdminOrders";
import ShopForUser from "./admin/ShopforUsers/ShopforUsers";

function App() {
  return (
    <Router>
      <Routes>
        <Route path="/" element={<Auth/>} />
        <Route path="/landing" element={<UserRoute> <LandingPage/> </UserRoute>} />
        <Route path="/products" element={<UserRoute> <ProductsPage /> </UserRoute>} />
        <Route path="/cart" element={<UserRoute> <Cart/> </UserRoute>} />
        <Route path="/support" element={<UserRoute> <SupportPage /> </UserRoute>} />
        <Route path="/transaction" element={<UserRoute> <TransactionsPage /> </UserRoute>} />
        <Route path="/account" element={<UserRoute> <AccountPage /> </UserRoute>} />
        <Route path="/about" element={<UserRoute> <AboutPage /> </UserRoute>} />
        <Route path="/orders" element={<UserRoute> <HistoryPage/> </UserRoute>} />
        <Route path="/transactions" element={<UserRoute> <TransactionsPage/> </UserRoute>} />
        <Route path="/checkout" element={<UserRoute> <CheckoutPage /> </UserRoute>} />
        <Route path="/admin/dashboard" element={<AdminRoute> <AdminDashboard /> </AdminRoute>} />
        <Route path="/admin/users" element={<AdminRoute> <ManageUsers /> </AdminRoute>} />
        <Route path="/admin/products" element={<AdminRoute> <ManageProducts /> </AdminRoute>} />
        <Route path="/admin/logs" element={<AdminRoute> <AdminLogs /> </AdminRoute>} />
        <Route path="/admin/orders" element={<AdminRoute> <AdminOrders /> </AdminRoute>} />
        <Route path="/admin/shop" element={<AdminRoute> <ShopForUser /> </AdminRoute>} />

      </Routes>
    </Router>
  );
}

export default App;
