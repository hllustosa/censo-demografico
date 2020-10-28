import Axios from "axios";

export function handleErrorResponse(error) {
  if (error.response.data) {
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

export function shuffle(array) {
  var currentIndex = array.length, temporaryValue, randomIndex;
  while (0 !== currentIndex) {
    randomIndex = Math.floor(Math.random() * currentIndex);
    currentIndex -= 1;
    temporaryValue = array[currentIndex];
    array[currentIndex] = array[randomIndex];
    array[randomIndex] = temporaryValue;
  }

  return array;
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
