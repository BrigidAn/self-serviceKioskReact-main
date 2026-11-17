import React, { useEffect, useId, useRef } from "react";
import { createPortal } from "react-dom";
import "./Modal.css";

/**
 * Shared modal dialog. All popups in the app should be built on this.
 *
 * open:            whether the modal is shown
 * onClose:         called on Escape, overlay click or the close (×) button
 * title:           heading text
 * tone:            "default" | "danger" (colors the title)
 * size:            "sm" | "md" | "lg"
 * footer:          action buttons, rendered right-aligned at the bottom
 * closeOnOverlay:  set false for forms where an accidental click would lose input
 */
export default function Modal({
  open,
  onClose,
  title,
  tone = "default",
  size = "md",
  footer,
  closeOnOverlay = true,
  children,
}) {
  const panelRef = useRef(null);
  const titleId = useId();

  useEffect(() => {
    if (!open) return undefined;

    const previouslyFocused = document.activeElement;
    const previousOverflow = document.body.style.overflow;
    document.body.style.overflow = "hidden";
    panelRef.current?.focus();

    const onKeyDown = (e) => {
      if (e.key === "Escape" && onClose) onClose();
    };
    document.addEventListener("keydown", onKeyDown);

    return () => {
      document.removeEventListener("keydown", onKeyDown);
      document.body.style.overflow = previousOverflow;
      if (previouslyFocused instanceof HTMLElement) previouslyFocused.focus();
    };
  }, [open, onClose]);

  if (!open) return null;

  const handleOverlayMouseDown = (e) => {
    if (closeOnOverlay && onClose && e.target === e.currentTarget) onClose();
  };

  return createPortal(
    <div className="ui-modal-overlay" onMouseDown={handleOverlayMouseDown}>
      <div
        ref={panelRef}
        className={`ui-modal ui-modal--${size} ui-modal--${tone}`}
        role="dialog"
        aria-modal="true"
        aria-labelledby={title ? titleId : undefined}
        tabIndex={-1}
      >
        {(title || onClose) && (
          <div className="ui-modal__header">
            {title && (
              <h2 id={titleId} className="ui-modal__title">
                {title}
              </h2>
            )}
            {onClose && (
              <button
                type="button"
                className="ui-modal__close"
                onClick={onClose}
                aria-label="Close"
              >
                ×
              </button>
            )}
          </div>
        )}

        <div className="ui-modal__body">{children}</div>

        {footer && <div className="ui-modal__footer">{footer}</div>}
      </div>
    </div>,
    document.body
  );
}
