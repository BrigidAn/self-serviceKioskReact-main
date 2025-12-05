import React, { useState } from "react";
import { BrowserRouter as Router, Routes, Route } from "react-router-dom";
import LandingPage from "./pages/LandingPage/LandingPage";
import ProductsPage from "./pages/Products/ProductsPage";
import Cart from "./pages/Cart/CartPage";
import SupportPage from "./pages/Support/SupportPage";
import AccountPage from "./pages/Account/AccountPage";
import TransactionsPage from "./pages/TransactionsPage/TransactionsPage";
import AboutPage from "./pages/About/AboutPage";

function App() {
  const [user, setUser] = useState(null);

  return (
    <Router>
      <Routes>
        <Route path="/" element={<LandingPage  />} />
        <Route
          path="/products"
          element={<ProductsPage user={user} setUser={setUser} />}
        />
         <Route
          path="/cart"
          element={<Cart user={user} setUser={setUser} />}
        />
       {/* <Route path="/admin" element={<PrivateRoute adminOnly={true}><AdminPage /></PrivateRoute>} />
<Route path="/products" element={<PrivateRoute><ProductPage /></PrivateRoute>} /> */}

        <Route
          path="/support"
          element={<SupportPage />}
        />

        <Route
          path="/transaction"
          element={<TransactionsPage />}
        />
      <Route
          path="/account"
          element={<AccountPage />}
        /> 
         <Route
          path="/about"
          element={<AboutPage />}
        /> 

      </Routes>
    </Router>
  );
}

export default App;
