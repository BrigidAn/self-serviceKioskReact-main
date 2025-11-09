import React, { createContext, useState, useContext } from 'react';

const AuthContext = createContext();

export const useAuth = () => useContext(AuthContext);

export const AuthProvider = ({ children }) => {
  const [user, setUser] = useState(null);
  const [balance, setBalance] = useState(0);

  const login = (userData) => {
    setUser(userData);
    setBalance(userData.balance || 0);
  };

  const logout = () => {
    setUser(null);
    setBalance(0);
  };

  return (
    <AuthContext.Provider value={{ user, balance, setBalance, login, logout }}>
      {children}
    </AuthContext.Provider>
  );
};
