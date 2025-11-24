import React from "react";
import "./Button.css";

const Button = ({ text, variant = "primary", onClick }) => {
  return (
    <button className={`btn ${variant}`} onClick={onClick}>
      {text}
    </button>
  );
};

export default Button;
