import React from "react";
import { faProjectDiagram } from "@fortawesome/free-solid-svg-icons";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";

export default function DeleteIcon(props) {

  const clickHandler = (e) => {
    e.preventDefault();
    props.onClick();
  };

  return (
    <a style={{color: "#566787" }} href="#" onClick={clickHandler}>
      <FontAwesomeIcon icon={faProjectDiagram} />
    </a>
  );
}