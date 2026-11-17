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
import AdminDashboard from "./Admin/AdminDashboard/AdminDashboard";
import ManageUsers from "./Admin/ManageUsers/ManageUsers";
import ManageProducts from "./Admin/ManageProducts/ManageProducts";
import AdminLogs from "./Admin/AdminLog/AdminLogs";
import UserRoute from "./components/UserRoute";
import AdminRoute from "./components/AdminRoute";
import AdminOrders from "./Admin/AdminOrders/AdminOrders";
import ShopForUser from "./Admin/ShopforUsers/ShopforUsers";
import { ToastContainer } from "react-toastify";
import { CartTimerProvider } from "./context/CartTimerContext";
import CartTimerPill from "./components/CartTimerPill/CartTimerPill";

function App() {
  return (
    <Router>
      <CartTimerProvider>
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
      <CartTimerPill />
      </CartTimerProvider>
      <ToastContainer position="top-right" autoClose={3500} newestOnTop />
    </Router>
  );
}

export default App;
