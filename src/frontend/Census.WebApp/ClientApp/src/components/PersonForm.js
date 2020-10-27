import React, { useState } from "react";
import { withStyles } from "@material-ui/core/styles";
import Modal from "@material-ui/core/Modal";
import Paper from "@material-ui/core/Paper";
import Grid from "@material-ui/core/Grid";
import Button from "@material-ui/core/Button";
import Divider from "@material-ui/core/Divider";
import Title from "./Title";
import { TextField, MenuItem } from "@material-ui/core";
import { SavePerson, UpdatePerson } from "../data/People";
import { handleErrorResponse } from "../data/Utils";

const styles = () => ({
  root: {
    position: "absolute",
    width: "450px",
    height: "330px",
    padding: "20px",
    outline: "none",
    top: "calc(50vh - (330px / 2))",
    left: "calc(50vw - (450px / 2))",
  },
});

function PersonForm(props) {
  const { classes } = props;
  const newPerson = props.person ? false : true;
  const [person, setPerson] = useState(newPerson ? { name: "" } : props.person);

  const handleClose = () => {
    if (props.handleClose) props.handleClose();
  };

  const onNameChange = (e) => {
    const newPerson = Object.assign({}, person, {});
    newPerson.name = e.target.value;
    setPerson(newPerson);
  };

  const save = () => {
    if (newPerson) {
      SavePerson(person)
        .then(() => {
          handleClose();
        })
        .catch((e) => {
          handleErrorResponse(e);
        });
    } else {
      UpdatePerson(person)
        .then(() => {
          handleClose();
        })
        .catch((e) => {
          handleErrorResponse(e);
        });
    }
  };

  return (
    <Modal
      open={props.open}
      onClose={handleClose}
      aria-labelledby="simple-modal-title"
      aria-describedby="simple-modal-description"
    >
      <Paper className={classes.root}>
        <Grid container direction="column" spacing={2}>
          <Grid item>
            <Title>Cadastro de Amigo</Title>
          </Grid>
          <Divider style={{ marginBottom: "20px" }} />
          <Grid item>
            <TextField
              fullWidth
              id="name"
              label="Nome"
              variant="outlined"
              value={person.name}
              onChange={onNameChange}
            />
          </Grid>
          <Divider style={{ marginBottom: "20px" }} />
          <Grid container direction="row" justify="flex-end">
            <Button color="primary" variant="contained" onClick={save}>
              Salvar
            </Button>
          </Grid>
        </Grid>
      </Paper>
    </Modal>
  );
}

export default withStyles(styles)(PersonForm);
