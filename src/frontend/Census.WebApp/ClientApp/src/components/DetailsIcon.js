import React from "react";
import { faEdit } from "@fortawesome/free-solid-svg-icons";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";

export default function DetailsIcon(props) {
  
  const clickHandler = (e) => {
    e.preventDefault();
    props.onClick();
  };

  return (
    <a style={{color: "#566787" }} href="/#" onClick={clickHandler}>
      <FontAwesomeIcon icon={faEdit} />
    </a>
  );
}
