import React, { createContext, useCallback, useContext, useEffect, useRef, useState } from "react";
import { useLocation } from "react-router-dom";
import { toast } from "react-toastify";
import { API_URL, authHeaders } from "../api/config";
import { getRole, isLoggedIn } from "../utils/auth";

/**
 * Tracks the user's cart reservation timer for the whole app, so the countdown
 * can be shown on every page (see CartTimerPill) and on the cart page itself.
 *
 * The API reserves cart items for a short time and resets the timer whenever an
 * item is added. Call `refreshCart()` after anything that changes the cart.
 */
const CartTimerContext = createContext(null);

// The API stores UTC times but serialises them without a "Z", which browsers would read as local time.
const parseUtc = (value) => {
  if (!value) return null;
  const hasZone = /[zZ]|[+-]\d{2}:?\d{2}$/.test(value);
  const ms = Date.parse(hasZone ? value : `${value}Z`);
  return Number.isNaN(ms) ? null : ms;
};

const isShopper = () => isLoggedIn() && getRole() !== "Admin";

export function CartTimerProvider({ children }) {
  const location = useLocation();
  const [expiresAt, setExpiresAt] = useState(null);
  const [itemCount, setItemCount] = useState(0);
  const [now, setNow] = useState(() => Date.now());
  const [expiredAt, setExpiredAt] = useState(null); // set each time the cart expires
  const expiringRef = useRef(false);

  const clear = () => {
    setExpiresAt(null);
    setItemCount(0);
  };

  const refreshCart = useCallback(async () => {
    if (!isShopper()) {
      clear();
      return;
    }

    try {
      const res = await fetch(`${API_URL}/cart`, { headers: authHeaders() });
      if (!res.ok) {
        // 404 = no active cart (or it already expired on the server)
        clear();
        return;
      }

      const data = await res.json();
      const count = (data.items || []).reduce((sum, i) => sum + (i.quantity || 0), 0);
      const expiry = parseUtc(data.expiresAt);

      setItemCount(count);
      setExpiresAt(count > 0 ? expiry : null);
      setNow(Date.now());
      expiringRef.current = false;
    } catch (err) {
      console.error("Failed to refresh cart timer", err);
    }
  }, []);

  // Re-sync on every page change (also covers login/logout).
  useEffect(() => {
    refreshCart();
  }, [location.pathname, refreshCart]);

  // Tick once per second while there is something to count down.
  useEffect(() => {
    if (!expiresAt) return undefined;
    const id = setInterval(() => setNow(Date.now()), 1000);
    return () => clearInterval(id);
  }, [expiresAt]);

  const remainingMs = expiresAt ? Math.max(0, expiresAt - now) : 0;

  // When the timer runs out, release the items exactly once.
  useEffect(() => {
    if (!expiresAt || remainingMs > 0 || expiringRef.current) return;
    expiringRef.current = true;

    (async () => {
      try {
        await fetch(`${API_URL}/cart/expire`, { method: "POST", headers: authHeaders() });
      } catch (err) {
        console.error("Failed to expire cart", err);
      }
      clear();
      setExpiredAt(Date.now());
      toast.info("Your cart reservation expired and the items were returned to stock.");
    })();
  }, [expiresAt, remainingMs]);

  const value = {
    itemCount,
    expiresAt,
    remainingMs,
    isActive: Boolean(expiresAt) && remainingMs > 0 && itemCount > 0,
    expiredAt,
    refreshCart,
  };

  return <CartTimerContext.Provider value={value}>{children}</CartTimerContext.Provider>;
}

export function useCartTimer() {
  const ctx = useContext(CartTimerContext);
  if (!ctx) throw new Error("useCartTimer must be used inside <CartTimerProvider>");
  return ctx;
}

export const formatCountdown = (ms) => {
  const totalSeconds = Math.ceil(ms / 1000);
  const minutes = Math.floor(totalSeconds / 60);
  const seconds = totalSeconds % 60;
  return `${String(minutes).padStart(2, "0")}:${String(seconds).padStart(2, "0")}`;
};
