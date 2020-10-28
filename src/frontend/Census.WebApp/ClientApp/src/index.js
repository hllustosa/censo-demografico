import React from "react";
import ReactDOM from "react-dom";
import Axios from "axios";
import { BrowserRouter, Switch, Route} from "react-router-dom";
import { Provider } from "react-redux";
import store from "./redux/store";
import Paths from "./paths.json";
import Dashboard from "./pages/dashboard";
import People from "./pages/people";
import { GET_BASE_URL } from "./data/Utils";

const baseUrl = document.getElementsByTagName("base")[0].getAttribute("href");
const rootElement = document.getElementById("root");
Axios.defaults.baseURL = GET_BASE_URL();

function App(props) {
  return (
    <BrowserRouter basename={baseUrl}>
      <Switch>
        <Route path={Paths.dashboard} exact component={Dashboard} />
        <Route path={Paths.people} exact component={People} />
        <Route component={Dashboard} />
      </Switch>
    </BrowserRouter>
  );
}

ReactDOM.render(
  <Provider store={store}>
    <App />
  </Provider>,
  rootElement
);
