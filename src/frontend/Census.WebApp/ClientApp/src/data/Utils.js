import Axios from "axios";
import * as SignalR from "@aspnet/signalr";

export function GET_BASE_URL() {
  return process.env.NODE_ENV !== "production"
    ? "http://localhost:8080"
    : Axios.defaults.baseURL;
}

export function GET_BASE_SIGNAIR_URL() {
  return process.env.NODE_ENV !== "production"
    ? "http://localhost:8080/stats/signair/hubs/notification"
    : "/stats/signair/hubs/notification";
}

export function handleErrorResponse(error) {
  if (error && error.response.data) {
    const data = error.response.data;
    if (data.errors) {
      alert(data.errors[0].ErrorMsg);
    }
  } else {
    alert("Ocorreu um erro durante comunicação com o servidor");
  }
}

export function adjustDate(value) {
  if (value) {
    const dateStr = value.split("T")[0];
    const dataComponents = dateStr.split("-");
    return `${dataComponents[2]}/${dataComponents[1]}/${dataComponents[0]}`;
  }
  return "";
}

export function DoPost(addr, obj) {
  return Axios.post(addr, obj);
}

export function DoPut(addr, obj) {
  return Axios.put(addr, obj);
}

export function DoDelete(addr) {
  return Axios.delete(addr);
}

export async function SetUpSignair(callback) {
  const connection = new SignalR.HubConnectionBuilder()
    .withUrl(GET_BASE_SIGNAIR_URL())
    .configureLogging(SignalR.LogLevel.Information)
    .build();

  connection
    .start({ withCredentials: false })
    .then(() => {
      console.log("Signalr Connection Started Successfully");
    })
    .catch((err) => {
      console.log("Signalr Connection Error: \n"+err);
    });

  connection.on("Notify", (message) => {
    callback();
  });
}
