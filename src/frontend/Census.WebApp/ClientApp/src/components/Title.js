import React from "react";
import Typography from "@material-ui/core/Typography";

export default function Title(props) {
  return (
    <Typography
      variant="h4"
      style={{ color: "#566787", fontFamily: "Varela Round, Sans-serif" }}
    >
      {props.children}
    </Typography>
  );
}
