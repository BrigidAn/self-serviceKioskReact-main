import React, { useState } from "react";
import { BrowserRouter as Router, Routes, Route } from "react-router-dom";
import LandingPage from "./pages/LandingPage/LandingPage";
import ProductsPage from "./pages/Products/ProductsPage";
import Cart from "./pages/Cart/CartPage";

function App() {
  const [user, setUser] = useState(null);

  return (
    <Router>
      <Routes>
        <Route path="/" element={<LandingPage setUser={setUser} />} />
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

      </Routes>
    </Router>
  );
}

export default App;
