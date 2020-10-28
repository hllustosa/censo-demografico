import React from "react";
import Graph from "react-graph-vis";

export default function FamilyTree(props) {
  const graph = props.graph;

  const options = {
    layout: {
      hierarchical: {
        enabled: true,
        sortMethod: "directed",
      },
    },
    edges: {
      color: "#000000",
    },
    nodes: {
      color: "#3f51b5",
      font: {
        color: "#f4f4f4",
      },
    },
    physics: {
      enabled: false,
    },
    height: "500px",
  };

  return (
    <Graph
      graph={graph}
      options={options}
      getNetwork={(network) => {}}
    />
  );
}
