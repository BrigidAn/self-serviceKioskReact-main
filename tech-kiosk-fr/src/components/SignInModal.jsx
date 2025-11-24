import React from "react";
import "./SignInModal.css";

function SignInModal({ close }) {
  return (
    <div className="modal-overlay">
      <div className="modal-box">
        <h2>Sign In</h2>

        <input type="text" placeholder="Email" className="input" />
        <input type="password" placeholder="Password" className="input" />

        <button className="login-btn">Login</button>

        <p className="close-text" onClick={close}>Close</p>
      </div>
    </div>
  );
}

export default SignInModal;
