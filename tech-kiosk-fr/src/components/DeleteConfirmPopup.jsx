import React from "react";
import { Button, Modal } from "./ui";

export default function DeleteConfirmPopup({ show, message, onDelete, onCancel }) {
  return (
    <Modal
      open={show}
      onClose={onCancel}
      title="Delete item?"
      tone="danger"
      size="sm"
      footer={
        <>
          <Button variant="secondary" onClick={onCancel}>Cancel</Button>
          <Button variant="danger" onClick={onDelete}>Delete</Button>
        </>
      }
    >
      <p>{message}</p>
    </Modal>
  );
}
