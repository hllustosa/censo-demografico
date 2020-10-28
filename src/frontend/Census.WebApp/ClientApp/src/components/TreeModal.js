import React, { useState } from "react";
import { withStyles } from "@material-ui/core/styles";
import Modal from "@material-ui/core/Modal";
import Paper from "@material-ui/core/Paper";
import Grid from "@material-ui/core/Grid";
import Divider from "@material-ui/core/Divider";
import Title from "./Title";
import { GetFamilyTree } from "../data/Tree";
import { TextField } from "@material-ui/core";
import FamilyTree from "../components/FamilyTree";

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

function CreateTreeData(data) {
  const graph = {
    nodes: [],
    edges: [],
  };

  for (const node of data.nodes) {
    graph.nodes.push({
      id: node.id,
      label: node.name,
    });
  }

  for (const relationship of data.relationships) {
    graph.edges.push({
      from: relationship.parentId,
      to: relationship.childId,
    });
  }

  return graph;
}

function TreeModal(props) {
  const { classes } = props;
  const person = props.person;
  const [level, setLevel] = useState(1);
  const [treeData, setTreeData] = useState(false);

  React.useEffect(() => {
    GetFamilyTree(person.id, level).then((response) => {
      setTreeData(CreateTreeData(response.data));
    });
  }, [level, person]);

  const handleClose = () => {
    if (props.handleClose) props.handleClose();
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
            <Title>Árvore Genealógica</Title>
          </Grid>
          <Divider style={{ marginBottom: "20px" }} />
          <Grid container style={{ height: "480px" }}>
            {treeData && <FamilyTree graph={treeData} />}
          </Grid>
          <Divider style={{ marginBottom: "20px" }} />
          <Grid container direction="row" justify="flex-end">
            <TextField
              id="level"
              size="small"
              label="Nível"
              variant="outlined"
              type="number"
              min={"1"}
              value={level}
              onChange={(e) => {
                if (e.target.value >= 1) setLevel(e.target.value);
              }}
            />
          </Grid>
        </Grid>
      </Paper>
    </Modal>
  );
}

export default withStyles(styles)(TreeModal);
