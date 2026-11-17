import React from "react";
import { Button, Modal } from "../../components/ui";

/**
 * Admin confirm / message popup.
 * type: "confirm" shows Yes/No, "error" shows a red title, anything else shows OK.
 */
export default function Popup({ message, onConfirm, onCancel, type, title, disableConfirm = false }) {
  const isConfirm = type === "confirm";
  const isError = type === "error";

  return (
    <Modal
      open
      onClose={isConfirm ? onCancel : onConfirm}
      title={title || (isError ? "Something went wrong" : isConfirm ? "Please confirm" : "Notice")}
      tone={isError ? "danger" : "default"}
      closeOnOverlay={false}
      footer={
        isConfirm ? (
          <>
            <Button variant="secondary" onClick={onCancel}>No</Button>
            <Button onClick={onConfirm} disabled={disableConfirm}>Yes</Button>
          </>
        ) : (
          <Button onClick={onConfirm}>OK</Button>
        )
      }
    >
      {typeof message === "string" ? <p>{message}</p> : message}
    </Modal>
  );
}
