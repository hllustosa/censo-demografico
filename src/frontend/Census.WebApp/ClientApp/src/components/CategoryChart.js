import React from "react";
import { withStyles } from "@material-ui/core/styles";
import Paper from "@material-ui/core/Paper";
import { ResponsiveContainer, PieChart, Pie, Legend, Tooltip } from "recharts";

const styles = () => ({
  root: {
    width: "calc(100% - 20px)",
    height: "35vh",
    padding: "10px",
    marginLeft: "10px",
    marginRight: "10px",
    outline: "none",
  },
});

const data01 = [
    { name: 'Group A', value: 400 }, { name: 'Group B', value: 300 },
    { name: 'Group C', value: 300 }, { name: 'Group D', value: 200 },
    { name: 'Group E', value: 278 }, { name: 'Group F', value: 189 },
  ];
  
  const data02 = [
    { name: 'Group A', value: 2400 }, { name: 'Group B', value: 4567 },
    { name: 'Group C', value: 1398 }, { name: 'Group D', value: 9800 },
    { name: 'Group E', value: 3908 }, { name: 'Group F', value: 4800 },
  ];

function CategoryChart(props) {
  const { classes } = props;

  return (
    <Paper className={classes.root}>
      <ResponsiveContainer>
        <PieChart>
          <Pie
            dataKey="value"
            isAnimationActive={false}
            data={data01}
            outerRadius={80}
            innerRadius={40}
            fill="#8884d8"
            label
          />
          <Tooltip />
        </PieChart>
      </ResponsiveContainer>
    </Paper>
  );
}

export default withStyles(styles)(CategoryChart);
