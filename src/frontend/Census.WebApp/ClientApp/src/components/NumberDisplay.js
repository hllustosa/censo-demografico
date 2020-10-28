import React from "react";
import { withStyles } from "@material-ui/core/styles";
import Paper from "@material-ui/core/Paper";
import { Typography, Grid } from "@material-ui/core";

const styles = () => ({
  root: {
    width: "calc(100% - 20px)",
    height: "35vh",
    padding: "10px",
    //marginLeft: "10px",
    //marginRight: "10px",
    outline: "none",
  },
  display: {
    textAlign: "center",
    fontWeight: "600",
    fontSize: "70px",
    verticalAlign: "middle",
  },
  subdisplay: {
    textAlign: "center",
    verticalAlign: "middle",
  },
});

function NumberDisplay(props) {
  const { classes } = props;

  return (
    <Paper className={classes.root}>
      <Typography>{"Número de Pessoas"}</Typography>
      <Grid container style={{height:"calc(100% - 45px)"}} direction="column" justify="center" alignItems="center" alignContent="center">
        <Grid item>
          <Typography className={classes.display}>
            {props.data}
          </Typography>
        </Grid>
        <Grid item>
          <Typography className={classes.subdisplay}>
            {"Pessoa(s)"}
          </Typography>
        </Grid>
      </Grid>
    </Paper>
  );
}

export default withStyles(styles)(NumberDisplay);
