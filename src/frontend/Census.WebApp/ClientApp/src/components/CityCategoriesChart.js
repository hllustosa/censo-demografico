import React from "react";
import { withStyles } from "@material-ui/core/styles";
import Paper from "@material-ui/core/Paper";
import {
  ResponsiveContainer,
  BarChart,
  Bar,
  XAxis,
  YAxis,
  Tooltip
} from "recharts";
import { shuffle } from "../data/Utils";
import { Typography, TextField, MenuItem } from "@material-ui/core";

const styles = () => ({
  root: {
    width: "calc(100% - 40px)",
    height: "calc(100% - 20px)",
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

function CityCategoriesChart(props) {
  const { classes } = props;
  const colors = shuffle(COLORS);

  const height =
    props.data.length < 6 ? props.data.length * 45 : props.data.length * 25;

  return (
    <Paper className={classes.root}>
      <Typography>{"Nomes por Cidade"}</Typography>

      <TextField
        fullWidth
        select
        style={{marginTop: "10px", marginBottom: "25px"}}
        id="city"
        label="Cidade"
        size="small"
        variant="outlined"
        onChange={e => {
            props.onCityChange(e.target.value);
        }}
      >
        {props.cities.map((city, index) => (
            <MenuItem key={`city-${index}`} value={city}>
                {city}
            </MenuItem>
        ))}
      </TextField>

      {props.data && (
        <div style={{ height: `${height}px` }}>
          <ResponsiveContainer>
            <BarChart
              data={props.data}
              layout="vertical"
              margin={{
                top: 5,
                left: 15,
                right: 5,
                bottom: 10,
              }}
            >
              <Bar
                dataKey="value"
                fill={"#77ba99"}
                radius={3}
                isAnimationActive={false}
              ></Bar>
              <XAxis hide={true} type="number" dataKey="value" />
              <YAxis
                width={80}
                hide={false}
                axisLine={false}
                interval={0}
                type="category"
                dataKey="name"
                tickLine={false}
              />
               <Tooltip />
            </BarChart>
          </ResponsiveContainer>
        </div>
      )}
    </Paper>
  );
}

export default withStyles(styles)(CityCategoriesChart);
