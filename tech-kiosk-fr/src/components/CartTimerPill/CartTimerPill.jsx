import React from "react";
import { Link, useLocation } from "react-router-dom";
import { FiClock, FiShoppingCart } from "react-icons/fi";
import { formatCountdown, useCartTimer } from "../../context/CartTimerContext";
import "./CartTimerPill.css";

const WARNING_MS = 30 * 1000;
const HIDDEN_ON = ["/"]; // login page

/** Floating countdown shown on every shopper page while the cart has reserved items. */
export default function CartTimerPill() {
  const { isActive, remainingMs, itemCount } = useCartTimer();
  const { pathname } = useLocation();

  if (!isActive || HIDDEN_ON.includes(pathname) || pathname.startsWith("/admin")) return null;

  const warning = remainingMs <= WARNING_MS;
  const onCart = pathname === "/cart";
  const time = formatCountdown(remainingMs);
  const itemsLabel = `${itemCount} item${itemCount === 1 ? "" : "s"}`;

  return (
    <div
      className={`cart-timer-pill ${warning ? "cart-timer-pill--warning" : ""}`}
      role="timer"
      aria-label={`${itemsLabel} reserved in your cart for ${time}`}
    >
      <FiShoppingCart className="cart-timer-pill__icon" aria-hidden="true" />
      <div className="cart-timer-pill__text">
        <span className="cart-timer-pill__label">
          {warning ? "Hurry! Cart expires in" : `${itemsLabel} reserved for`}
        </span>
        <span className="cart-timer-pill__time">
          <FiClock aria-hidden="true" /> {time}
        </span>
      </div>
      {!onCart && (
        <Link to="/cart" className="cart-timer-pill__link">
          View cart
        </Link>
      )}
    </div>
  );
}
