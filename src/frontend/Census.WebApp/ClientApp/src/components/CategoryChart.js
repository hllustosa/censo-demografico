import React from "react";
import { withStyles } from "@material-ui/core/styles";
import Paper from "@material-ui/core/Paper";
import { ResponsiveContainer, PieChart, Pie, Cell, Tooltip } from "recharts";
import { Typography } from "@material-ui/core";

const styles = () => ({
  root: {
    width: "calc(100% - 20px)",
    height: "35vh",
    padding: "10px",
    outline: "none",
  },
});

const COLORS = [
  "#5d89a8",
  "#77ba99",
  "#ef9738",
  "#97d1fa",
  "#a45f6e",
  "#9d02d7",
];

const renderLabel = function (entry) {
  return entry.name;
};

function CategoryChart(props) {
  const { classes } = props;
  const colors = COLORS;

  return (
    <Paper className={classes.root}>
      <Typography>{props.title}</Typography>
      {props.data && (
        <ResponsiveContainer>
          <PieChart>
            <Pie
              dataKey="value"
              isAnimationActive={false}
              data={props.data}
              outerRadius={80}
              innerRadius={40}
              fill="#8884d8"
              label={renderLabel}
            >
              {props.data.map((entry, index) => (
                <Cell fill={colors[index % colors.length]} />
              ))}
            </Pie>
            <Tooltip />
          </PieChart>
        </ResponsiveContainer>
      )}
    </Paper>
  );
}

export default withStyles(styles)(CategoryChart);
