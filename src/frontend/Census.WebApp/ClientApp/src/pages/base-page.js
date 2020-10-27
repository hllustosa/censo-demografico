import React from "react";
//Components
import Navbar from "../components/Navbar";
import { withStyles } from "@material-ui/core/styles";

const styles = () => ({
  main: {
    display: "flex",
    flex: "1 0 auto",
    width: "calc(100% - 40px)",
    padding: "20px",
  },
});

function BasePage(props) {
  const { classes } = props;
  return [
    <Navbar key="header" {...props} />,
    <main key="main" className={classes.main}>
      {props.children}
    </main>,
  ];
}

const StyledBasePage = withStyles(styles)(BasePage);

function withBasePage(Component) {
  return (props) => (
    <StyledBasePage {...props}>
      <Component />
    </StyledBasePage>
  );
}

export default withBasePage;
