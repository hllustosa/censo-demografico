import React from "react";
import { faTrashAlt } from "@fortawesome/free-solid-svg-icons";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";

export default function DeleteIcon(props) {

  const clickHandler = (e) => {
    e.preventDefault();
    props.onClick();
  };

  return (
    <a style={{color: "#CD5C5C" }} href="/#" onClick={clickHandler}>
      <FontAwesomeIcon icon={faTrashAlt} />
    </a>
  );
}