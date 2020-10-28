import React, { useState } from "react";
import withBasePage from "./base-page";
import Grid from "@material-ui/core/Grid";
import TextField from "@material-ui/core/TextField";
import MenuItem from "@material-ui/core/MenuItem";
import { withStyles } from "@material-ui/core/styles";
import CategoryChart from "../components/CategoryChart";
import NumberDisplay from "../components/NumberDisplay";
import CityCategoriesChart from "../components/CityCategoriesChart";
import { GetStats, GetCities, GetStatsPerCity } from "../data/PeopleStats";
import { SetUpSignair } from "../data/Utils";

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

function processData(data, field) {
  const counter = {};
  for (const category of data) {
    if (!counter[category[field]]) {
      counter[category[field]] = 0;
    }

    let catCount = 0;
    const nameCounter = category.personNameCounters;
    for (const key of Object.keys(nameCounter)) {
      catCount = catCount + nameCounter[key].count;
    }

    counter[category[field]] += catCount;
  }

  const processedData = [];
  for (const key of Object.keys(counter)) {
    processedData.push({
      name: key,
      value: counter[key],
    });
  }

  return processedData;
}

function count(data) {
  let catCount = 0;
  for (const category of data) {
    const nameCounter = category.personNameCounters;
    for (const key of Object.keys(nameCounter)) {
      catCount = catCount + nameCounter[key].count;
    }
  }
  return catCount;
}

function processPerCityData(data) {
  const perCityData = [];
  const nameCounters = data.personNameCounters;

  for (const key of Object.keys(nameCounters)) {
    perCityData.push({ name: key, value: nameCounters[key].count });
  }
  return perCityData;
}

function Dashboard(props) {
  const [personFilter, setPersonFilter] = useState({});
  const [sexData, setSexData] = useState(false);
  const [raceData, setRaceData] = useState(false);
  const [educationData, setEducationData] = useState(false);
  const [cities, setCities] = useState([]);
  const [selectedCity, setSelectedCity] = useState("");
  const [perCityData, setPerCityData] = useState([]);
  const [peopleCount, setPeopleCount] = useState(0);

  const onChange = (field) => {
    return (e) => {
      const newPersonFilter = Object.assign({}, personFilter, {});
      newPersonFilter[field] = e.target.value;
      setPersonFilter(newPersonFilter);
    };
  };

  const onCityChange = (city) => {
    setSelectedCity(city);
  };

  const setCategoryData = (response) => {
    setSexData(processData(response.data, "sex"));
    setRaceData(processData(response.data, "race"));
    setEducationData(processData(response.data, "schoolLevel"));
    setPeopleCount(count(response.data));
  };

  function getData() {
    GetStats(personFilter)
      .then((response) => {
        setCategoryData(response);
        return GetCities();
      })
      .then((response) => {
        setCities(response.data);
      });
  };

  React.useEffect(() => {
    async function Initialize() {
      await SetUpSignair(getData);
      getData();
    }
    Initialize();
  }, []);

  React.useEffect(() => {
    GetStatsPerCity(selectedCity).then((response) => {
      setPerCityData(processPerCityData(response.data));
    });
  }, [selectedCity]);

  React.useEffect(() => {
    GetStats(personFilter).then((response) => {
      setCategoryData(response);
    });
  }, [personFilter]);

  const { classes } = props;

  return (
    <div className={classes.root}>
      <Grid
        style={{ width: "80%", margin: "auto", paddingBottom: "15px" }}
        container
        spacing={2}
        direction="row"
        id="filters"
      >
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
            <MenuItem key={3} value={""}>
              {"Sem Filtro"}
            </MenuItem>
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
            <MenuItem key={3} value={""}>
              {"Sem Filtro"}
            </MenuItem>
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
            <MenuItem key={3} value={""}>
              {"Sem Filtro"}
            </MenuItem>
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
      <div id="main" class="grid-container">
        <div id="cat-charts" class="main">
          <Grid container spacing={1} direction="row">
            <Grid item lg={6} md={6} sm={12} xs={12}>
              <NumberDisplay data={peopleCount} />
            </Grid>
            <Grid item lg={6} md={6} sm={12} xs={12}>
              <CategoryChart title="Pessoas por Raça" data={raceData} />
            </Grid>
            <Grid item lg={6} md={6} sm={12} xs={12}>
              <CategoryChart
                title="Pessoas por Escolaridade"
                data={educationData}
              />
            </Grid>
            <Grid item lg={6} md={6} sm={12} xs={12}>
              <CategoryChart title="Pessoas por Sexo" data={sexData} />
            </Grid>
          </Grid>
        </div>
        <div id="city-charts" class="right">
          <CityCategoriesChart
            cities={cities}
            data={perCityData}
            onCityChange={onCityChange}
          />
        </div>
      </div>
    </div>
  );
}

export default withBasePage(withStyles(styles)(Dashboard));
