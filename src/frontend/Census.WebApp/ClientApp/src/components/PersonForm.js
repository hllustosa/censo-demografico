import React, { useState } from "react";
import { withStyles } from "@material-ui/core/styles";
import Modal from "@material-ui/core/Modal";
import Paper from "@material-ui/core/Paper";
import Grid from "@material-ui/core/Grid";
import Button from "@material-ui/core/Button";
import Divider from "@material-ui/core/Divider";
import Title from "./Title";
import { TextField, MenuItem } from "@material-ui/core";
import PeopleAutoComplete from "../components/PeopleAutoComplete";
import { GetPerson, SavePerson, UpdatePerson } from "../data/People";
import { handleErrorResponse } from "../data/Utils";

const styles = () => ({
  root: {
    position: "absolute",
    width: "450px",
    height: "600px",
    padding: "20px",
    outline: "none",
    top: "calc(50vh - (640px / 2))",
    left: "calc(50vw - (450px / 2))",
  },
});

function PersonForm(props) {
  const { classes } = props;
  const newPerson = props.person ? false : true;
  const [person, setPerson] = useState(newPerson ? { name: "", address: {} } : props.person);
  const [father, setFather] = useState({});
  const [mother, setMother] = useState({});
  const [loading, setLoading] = useState(true);

  React.useEffect(() => {
    GetPerson(person.fatherId).then((response) => {
      setFather(response.data);
      return GetPerson(person.motherId);
    })
    .then((response) => {
      setMother(response.data);
      setLoading(false);
    })
  }, []);


  const handleClose = () => {
    if (props.handleClose) props.handleClose();
  };

  const onChange = (field) => {
    return (e) => {
      const newPerson = Object.assign({}, person, {});
      newPerson[field] = e.target.value;
      setPerson(newPerson);
    };
  };

  const onChangeAddress = (field) => {
    return (e) => {
      const newPerson = Object.assign({}, person, {});
      newPerson["address"][field] = e.target.value;
      setPerson(newPerson);
    };
  };

  const onFatherChange = (father) => {
    const newPerson = Object.assign({}, person, {});
    newPerson["fatherId"] = father.id;
    setPerson(newPerson);
  }

  const onMotherChange = (mother) => {
    const newPerson = Object.assign({}, person, {});
    newPerson["motherId"] = mother.id;
    setPerson(newPerson);
  }

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
            <Title>Cadastro de Pessoa</Title>
          </Grid>
          <Divider style={{ marginBottom: "20px" }} />
          <Grid item>
            <TextField
              fullWidth
              id="name"
              label="Nome"
              size="small"
              variant="outlined"
              value={person.name}
              onChange={onChange("name")}
            />
          </Grid>
          <Grid item>
            <TextField
              fullWidth
              select
              id="sex"
              label="Sexo"
              size="small"
              variant="outlined"
              value={person.sex}
              onChange={onChange("sex")}
            >
              <MenuItem key={0} value={"M"}>
                {"Masculino"}
              </MenuItem>
              <MenuItem key={1} value={"F"}>
                {"Feminino"}
              </MenuItem>
              <MenuItem key={2} value={"I"}>
                {"Indefinido"}
              </MenuItem>
            </TextField>
          </Grid>
          <Grid item>
            <TextField
              fullWidth
              select
              id="race"
              label="Raça"
              size="small"
              variant="outlined"
              value={person.race}
              onChange={onChange("race")}
            >
              <MenuItem key={0} value={"Branco(a)"}>
                {"Branco(a)"}
              </MenuItem>
              <MenuItem key={1} value={"Pardo(a)"}>
                {"Pardo(a)"}
              </MenuItem>
              <MenuItem key={2} value={"Negro(a)"}>
                {"Negro(a)"}
              </MenuItem>
              <MenuItem key={2} value={"Amarelo(a)"}>
                {"Amarelo(a)"}
              </MenuItem>
              <MenuItem key={2} value={"Indígena"}>
                {"Indígena"}
              </MenuItem>
              <MenuItem key={2} value={"Não Informada"}>
                {"Não Informada"}
              </MenuItem>
            </TextField>
          </Grid>
          <Grid item>
            <TextField
              fullWidth
              select
              id="education"
              label="Escolaridade"
              size="small"
              variant="outlined"
              value={person.education}
              onChange={onChange("education")}
            >
              <MenuItem key={0} value={"Analfabeto(a)"}>
                {"Analfabeto(a)"}
              </MenuItem>
              <MenuItem key={1} value={"Alfabetizado(a)"}>
                {"Alfabetizado(a)"}
              </MenuItem>
              <MenuItem key={2} value={"Ensino Fundamental"}>
                {"Ensino Fundamental"}
              </MenuItem>
              <MenuItem key={2} value={"Ensino Médio"}>
                {"Ensino Médio"}
              </MenuItem>
              <MenuItem key={2} value={"Ensino Superior"}>
                {"Ensino Superior"}
              </MenuItem>
              <MenuItem key={2} value={"Pós-Graduação"}>
                {"Pós-Graduação"}
              </MenuItem>
            </TextField>
          </Grid>
          <Divider style={{ marginBottom: "5px", marginTop: "10px" }} />
          {!loading && <Grid item container spacing={2}>
            <Grid item xs={6}>
              <PeopleAutoComplete
                label={"Pai"}
                defaultValue={father}
                handleOnChange={onFatherChange}
              />
            </Grid>
            <Grid item xs={6}>
              <PeopleAutoComplete
                label={"Mãe"}
                defaultValue={mother}
                handleOnChange={onMotherChange}
              />
            </Grid>
          </Grid>}
          <Divider style={{ marginBottom: "10px", marginTop: "5px" }} />
          <Grid item>
            <TextField
              fullWidth
              id="name"
              label="Endereço"
              size="small"
              variant="outlined"
              value={person.address.addressDesc}
              onChange={onChangeAddress("addressDesc")}
            />
          </Grid>
          <Grid item container spacing={2}>
            <Grid item xs={6}>
              <TextField
                fullWidth
                id="name"
                label="Bairro"
                size="small"
                variant="outlined"
                value={person.address.burrow}
                onChange={onChangeAddress("burrow")}
              />
            </Grid>
            <Grid item xs={6}>
              <TextField
                fullWidth
                id="name"
                label="Cidade"
                size="small"
                variant="outlined"
                value={person.address.city}
                onChange={onChangeAddress("city")}
              />
            </Grid>
          </Grid>
          <Grid item container spacing={2}>
            <Grid item xs={8}>
              <TextField
                fullWidth
                id="name"
                label="CEP"
                size="small"
                variant="outlined"
                value={person.address.zipCode}
                onChange={onChangeAddress("zipCode")}
              />
            </Grid>
            <Grid item xs={4}>
              <TextField
                fullWidth
                id="name"
                label="Estado"
                size="small"
                variant="outlined"
                value={person.address.state}
                onChange={onChangeAddress("state")}
              />
            </Grid>
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
