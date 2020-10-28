import React, { useState } from "react";
//Components
import AppBar from "@material-ui/core/AppBar";
import Divider from "@material-ui/core/Divider";
import IconButton from "@material-ui/core/IconButton";
import MenuIcon from "@material-ui/icons/Menu";
import Grid from "@material-ui/core/Grid";
import Drawer from "@material-ui/core/Drawer";
import List from "@material-ui/core/List";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import {
  faUsers,
  faChartArea,
} from "@fortawesome/free-solid-svg-icons";
import ListItem from "@material-ui/core/ListItem";
import ListItemIcon from "@material-ui/core/ListItemIcon";
import ListItemText from "@material-ui/core/ListItemText";
import { withStyles } from "@material-ui/core/styles";
import Typography from "@material-ui/core/Typography";
import Paths from "../paths.json";
import store from "../redux/store";
import Logo from "../static/logo.png"

const styles = () => ({
  root: {
    height: "45px",
    paddingLeft: "20px",
    paddingRight: "20px",
  },
  title: {
    flexGrow: 1,
  },
  titleText: {
    fontWeight: "550",
  },
  appName: {
    fontWeight: "550",
  },
  side: {
    paddingTop: "25px",
    paddingLeft: "5px",
    paddingRight: "5px",
    minWidth: "220px",
    color: "#566787",
  },
  listItem: {
    color: "#566787",
  },
  logo: {
    height: "110px"
  }
});

const menuEntries = [
  { path: Paths.dashboard, icon: faChartArea, text: "Dashboard" },
  { path: Paths.people, icon: faUsers, text: "Pessoas" },
];

const APP_NAME = "Censo 2020";

function SideList(props) {
  const { classes, toggleDrawer, goTo } = props;

  return (
    <div
      className={classes.list}
      onClick={toggleDrawer(false)}
      onKeyDown={toggleDrawer(false)}
    >
      <Grid
        container
        direction="column"
        justify="center"
        alignItems="center"
        className={classes.side}
      >
        <img src={Logo} alt="Logo" className={classes.logo} />
        <Typography variant="h6" className={classes.appName}>
          {APP_NAME}
        </Typography>
      </Grid>
      <List>
        <Divider />
        {menuEntries
          .filter((entry) => {
            if (entry.text === "Amigos") {
              var role = store.getState().user.roles[0];
              if (role !== "admin") return false;
            }
            return true;
          })
          .map((entry, index) => (
            <ListItem
              button
              key={`menu-entry-${index}`}
              id={entry.path}
              onClick={goTo}
              className={classes.listItem}
            >
              <ListItemIcon>
                <FontAwesomeIcon icon={entry.icon} />
              </ListItemIcon>
              <ListItemText primary={entry.text} />
            </ListItem>
          ))}
        <Divider />
      </List>
    </div>
  );
}

function MenuAppBar(props) {
  const { classes } = props;
  const [open, setOpen] = useState(false);

  const toggleDrawer = (open) => (event) => {
    if (
      event.type === "keydown" &&
      (event.key === "Tab" || event.key === "Shift")
    ) {
      return;
    }

    setOpen(open);
  };

  const goTo = (evt) => {
    props.history.push(evt.currentTarget.id);
  };

  return (
    <AppBar position="static" className={classes.root}>
      <Grid
        container
        direction="row"
        justify="flex-start"
        alignItems="center"
      >
        <Grid item>
          <IconButton
            edge="start"
            color="inherit"
            aria-label="Menu"
            onClick={toggleDrawer(true)}
          >
            <MenuIcon />
          </IconButton>
        </Grid>
        <Grid item>
          <Typography style={{ marginLeft: "10px" }}>{APP_NAME}</Typography>
        </Grid>
      </Grid>
      <Drawer
        className={classes.drawer}
        open={open}
        onClose={toggleDrawer(false)}
      >
        <SideList {...props} goTo={goTo} toggleDrawer={toggleDrawer} />
      </Drawer>
    </AppBar>
  );
}

export default withStyles(styles)(MenuAppBar);
