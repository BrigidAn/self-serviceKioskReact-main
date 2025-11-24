import React, { createContext, useContext, useEffect, useState } from "react";
import api from "../api";

const AuthContext = createContext();

export function AuthProvider({ children }) {
  const [user, setUser] = useState(null); // optional user info from login
  const [balance, setBalance] = useState(0);
  const [loadingBalance, setLoadingBalance] = useState(false);

 const refreshBalance = async () => {
  if (!user) return;
  try {
    const res = await api.get("/Account/balance");
    setBalance(res.data.balance ?? 0);
  } catch {
    setBalance(0);
  }
};

  useEffect(() => {
    refreshBalance();
  }, []);

  return (
    <AuthContext.Provider
      value={{
        user,
        setUser,
        balance,
        setBalance,
        refreshBalance,
        loadingBalance,
      }}
    >
      {children}
    </AuthContext.Provider>
  );
}

export function useAuth() {
  return useContext(AuthContext);
}
