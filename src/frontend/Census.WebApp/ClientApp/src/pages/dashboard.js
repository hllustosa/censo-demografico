import React, { useState, useEffect } from "react";
import withBasePage from "./base-page";
import CompleteTable, {
  TableHeaderCell,
  StyledTableCell,
} from "../components/Table";
import Grid from "@material-ui/core/Grid";
import TextField from "@material-ui/core/TextField";
import MenuItem from "@material-ui/core/MenuItem";
import { withStyles } from "@material-ui/core/styles";

import CategoryChart from "../components/CategoryChart";

const styles = (theme) => ({
  root: {
    width: "100%",
    maxWidth: "1300px",
    margin: "auto",
  },
  backdrop: {
    zIndex: 1001,
    color: "#fff",
  },
  link: {
    textDecoration: "none",
    fontFamily: "sans-serif",
  },
});

function Dashboard(props) {
  const [personFilter, setPersonFilter] = useState({});

  const onChange = (field) => {
    return (e) => {
      const newPersonFilter = Object.assign({}, personFilter, {});
      newPersonFilter[field] = e.target.value;
      setPersonFilter(newPersonFilter);
    };
  };

  const { classes } = props;
  return (
    <div className={classes.root}>
   
        <Grid style={{width: "80%", margin: "auto", paddingBottom: "15px"}} container spacing={2} direction="row">
          <Grid item lg={3} md={4} sm={6} xs={12}>
            <TextField
              fullWidth
              id="name"
              label="Nome"
              size="small"
              variant="outlined"
              value={personFilter.name}
              onChange={onChange("name")}
            />
          </Grid>
          <Grid item lg={3} md={4} sm={6} xs={12}>
            <TextField
              fullWidth
              select
              id="sex"
              label="Sexo"
              size="small"
              variant="outlined"
              value={personFilter.sex}
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
          <Grid item lg={3} md={4} sm={6} xs={12}>
            <TextField
              fullWidth
              select
              id="race"
              label="Raça"
              size="small"
              variant="outlined"
              value={personFilter.race}
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
          <Grid item lg={3} md={4} sm={6} xs={12}>
            <TextField
              fullWidth
              select
              id="education"
              label="Escolaridade"
              size="small"
              variant="outlined"
              value={personFilter.education}
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
        </Grid>
      <Grid container spacing={2} direction="row">
        <Grid item lg={4} md={4} sm={6} xs={12}>
          <CategoryChart />
        </Grid>
        <Grid item lg={4} md={4} sm={6} xs={12}>
          <CategoryChart />
        </Grid>
        <Grid item lg={4} md={4} sm={6} xs={12}>
          <CategoryChart />
        </Grid>
        <Grid item lg={4} md={4} sm={6} xs={12}>
          <CategoryChart />
        </Grid>
        <Grid item lg={4} md={4} sm={6} xs={12}>
          <CategoryChart />
        </Grid>
        <Grid item lg={4} md={4} sm={6} xs={12}>
          <CategoryChart />
        </Grid>
      </Grid>
    </div>
  );
}

export default withBasePage(withStyles(styles)(Dashboard));
