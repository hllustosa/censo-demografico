import React, { useState, useEffect } from "react";
import withBasePage from "./base-page";
import CompleteTable, {
  TableHeaderCell,
  StyledTableCell,
} from "../components/Table";
import Grid from "@material-ui/core/Grid";
import TextField from "@material-ui/core/TextField";
import TableRow from "@material-ui/core/TableRow";
import TableHead from "@material-ui/core/TableHead";
import Button from "@material-ui/core/Button";
import Backdrop from "@material-ui/core/Backdrop";
import CircularProgress from "@material-ui/core/CircularProgress";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import { faPlus } from "@fortawesome/free-solid-svg-icons";
import { withStyles } from "@material-ui/core/styles";
import { GetPeople, DeletePerson } from "../data/People";
import { handleErrorResponse } from "../data/Utils";
import Title from "../components/Title";
import DetailsIcon from "../components/DetailsIcon";
import DeleteIcon from "../components/DeleteIcon";
import ConfirmationModal from "../components/ConfirmationModal";
import PersonForm from "../components/PersonForm";
import Tree from "../components/TreeModal";

const styles = () => ({
  root: {
    width: "100%",
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

function People(props) {
  const { classes } = props;
  const [rows, setRows] = useState([]);
  const [page, setPage] = useState(0);
  const [itensCount, setItensCount] = useState(0);
  const [search, setSearch] = useState("");
  const [selectedPerson, setSelectedPerson] = useState({});
  const [loading, setLoading] = useState(true);
  const [showConfirmation, setShowConfirmation] = useState(false);
  const [showForm, setShowForm] = useState(false);
  const [showTree, setShowTree] = useState(false);

  const RenderHeader = () => {
    return (
      <TableHead>
        <TableRow>
          <TableHeaderCell>Nome</TableHeaderCell>
          <TableHeaderCell>Sexo</TableHeaderCell>
          <TableHeaderCell>Raça</TableHeaderCell>
          <TableHeaderCell>Escolaridade</TableHeaderCell>
          <TableHeaderCell></TableHeaderCell>
          <TableHeaderCell></TableHeaderCell>
          <TableHeaderCell></TableHeaderCell>
        </TableRow>
      </TableHead>
    );
  };

  const RenderRow = (row, index) => {
    return (
      <TableRow hover key={`row-person-${index}`}>
        <StyledTableCell component="th" scope="row">
          <a
            className={classes.link}
            href="#"
            onClick={(e) => {
              e.preventDefault();
              setSelectedPerson(row);
            }}
          >
            {row.name}
          </a>
        </StyledTableCell>
        <StyledTableCell component="th" scope="row">
          {row.sex}
        </StyledTableCell>
        <StyledTableCell component="th" scope="row">
          {row.race}
        </StyledTableCell>
        <StyledTableCell component="th" scope="row">
          {row.education}
        </StyledTableCell>
        <StyledTableCell style={{ width: 30 }}>
          <DetailsIcon
            onClick={() => {
              setSelectedPerson(row);
              setShowTree(true);
            }}
          />
        </StyledTableCell>
        <StyledTableCell style={{ width: 30 }}>
          <DetailsIcon
            onClick={() => {
              setSelectedPerson(row);
              setShowForm(true);
            }}
          />
        </StyledTableCell>
        <StyledTableCell style={{ width: 30 }}>
          <DeleteIcon
            onClick={() => {
              setSelectedPerson(row);
              setShowConfirmation(true);
            }}
          />
        </StyledTableCell>
      </TableRow>
    );
  };

  useEffect(() => {
    setPage(0);
    loadPeople();
  }, [search]);

  useEffect(() => {
    loadPeople();
  }, [page]);

  const loadPeople = () => {
    setLoading(true);
    GetPeople(page + 1, search)
      .then((response) => {
        setRows(response.data.items);
        setItensCount(response.data.totalItems);
        setLoading(false);
      })
      .catch((e) => {
        handleErrorResponse(e);
        setLoading(false);
      });
  };

  const deletePeople = () => {
    setLoading(true);
    DeletePerson(selectedPerson)
      .then(() => {
        setPage(0);
        loadPeople();
      })
      .catch((e) => {
        handleErrorResponse(e);
        setLoading(false);
      });
  };

  const handleChangePage = (newPage) => {
    setPage(newPage);
  };

  const handleCloseForm = () => {
    setShowForm(false);
    loadPeople();
  };

  const handleCloseTreeForm = () => {
    setShowTree(false);
  };

  return (
    <Grid container className={classes.root} direction="column">
      <Backdrop className={classes.backdrop} open={loading}>
        <CircularProgress color="primary" />
      </Backdrop>
      <Grid container direction="row" justify="flex-start">
        <Title>{"Pessoas"}</Title>
      </Grid>
      <Grid
        container
        direction="row"
        justify="flex-end"
        alignItems="flex-end"
        spacing={2}
      >
        <Grid item>
          <TextField
            id="date"
            label="Buscar Pessoa"
            type="text"
            value={search}
            onChange={(e) => {
              setSearch(e.target.value);
            }}
            InputLabelProps={{
              shrink: true,
            }}
          />
        </Grid>
        <Grid item>
          <Button
            variant="contained"
            color="primary"
            onClick={() => {
              setSelectedPerson(null);
              setShowForm(true);
            }}
          >
            <FontAwesomeIcon style={{ marginRight: "5px" }} icon={faPlus} />{" "}
            {"Novo"}
          </Button>
        </Grid>
      </Grid>
      <Grid item style={{ paddingTop: "15px" }}>
        <CompleteTable
          rows={rows}
          page={page}
          totalItens={itensCount}
          renderHeader={RenderHeader}
          renderRow={RenderRow}
          handleChangePage={handleChangePage}
        />
      </Grid>
      <ConfirmationModal
        open={showConfirmation}
        handleOk={() => deletePeople()}
        handleClose={() => setShowConfirmation(false)}
      />
      {showForm && (
        <PersonForm
          open={showForm}
          handleClose={handleCloseForm}
          person={selectedPerson}
        />
      )}
      {showTree && (
        <Tree
          open={showTree}
          handleClose={handleCloseTreeForm}
          person={selectedPerson}
        />
      )}
    </Grid>
  );
}

export default withBasePage(withStyles(styles)(People));
