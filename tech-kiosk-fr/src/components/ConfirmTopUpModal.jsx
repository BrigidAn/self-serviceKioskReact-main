import { Button, Modal } from "./ui";

export default function ConfirmTopUpModal({ show, amount, onConfirm, onCancel }) {
  return (
    <Modal
      open={show}
      onClose={onCancel}
      title="Confirm top-up"
      size="sm"
      footer={
        <>
          <Button variant="secondary" onClick={onCancel}>Cancel</Button>
          <Button onClick={onConfirm}>Top up</Button>
        </>
      }
    >
      <p>
        Are you sure you want to add <strong>R {Number(amount || 0).toFixed(2)}</strong> to your account?
      </p>
    </Modal>
  );
}
