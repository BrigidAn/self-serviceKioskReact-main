import React from "react";
import "./Button.css";

/**
 * Shared button.
 *
 * variant: "primary" | "secondary" | "danger" | "ghost"
 * size:    "sm" | "md" | "lg"
 * block:   stretch to full width
 * loading: shows a spinner and disables the button
 */
export default function Button({
  variant = "primary",
  size = "md",
  block = false,
  loading = false,
  disabled = false,
  type = "button",
  className = "",
  children,
  ...rest
}) {
  const classes = [
    "ui-btn",
    `ui-btn--${variant}`,
    `ui-btn--${size}`,
    block ? "ui-btn--block" : "",
    loading ? "ui-btn--loading" : "",
    className,
  ]
    .filter(Boolean)
    .join(" ");

  return (
    <button
      type={type}
      className={classes}
      disabled={disabled || loading}
      aria-busy={loading || undefined}
      {...rest}
    >
      {loading && <span className="ui-btn__spinner" aria-hidden="true" />}
      <span className="ui-btn__label">{children}</span>
    </button>
  );
}
