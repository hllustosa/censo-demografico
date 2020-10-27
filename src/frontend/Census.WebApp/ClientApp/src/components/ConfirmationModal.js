import React from "react";
import Button from "@material-ui/core/Button";
import Dialog from "@material-ui/core/Dialog";
import DialogActions from "@material-ui/core/DialogActions";
import DialogContent from "@material-ui/core/DialogContent";
import DialogContentText from "@material-ui/core/DialogContentText";
import DialogTitle from "@material-ui/core/DialogTitle";

export default function ConfirmationModal(props) {

  const onOkClick = () => {
    props.handleOk();
    props.handleClose();
  };

  const onCancelClick = () => {
    props.handleClose();
  };

  return (
    <Dialog
      open={props.open}
      onClose={props.handleClose}
      aria-labelledby="alert-dialog-title"
      aria-describedby="alert-dialog-description"
    >
      <DialogTitle id="alert-dialog-title">
        {"Tem certeza que deseja remover este item?"}
      </DialogTitle>
      <DialogContent>
        <DialogContentText id="alert-dialog-description">
          Este item será removido definitivamente e a operação não poderá ser
          desfeita. Deseja continuar?
        </DialogContentText>
      </DialogContent>
      <DialogActions>
        <Button onClick={onOkClick} color="primary">
          Ok
        </Button>
        <Button onClick={onCancelClick} color="primary" autoFocus>
          Cancelar
        </Button>
      </DialogActions>
    </Dialog>
  );
}
