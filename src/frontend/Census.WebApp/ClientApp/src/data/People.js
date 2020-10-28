import Axios from "axios";
import { DoPost, DoPut, DoDelete } from "./Utils";

const PEOPLE_BYNAME_ENDPOINT = "person/api/person?page={0}&name={1}";
const SAVE_ENDPOINT = "person/api/person/";
const UPDATE_ENDPOINT = "person/api/person/{0}";
const DELETE_ENDPOINT = "person/api/person/{0}";

export function GetPeople(page, name) {
  const addr = PEOPLE_BYNAME_ENDPOINT.replace("{0}", page)
    .replace("{1}", name ? name : "");

  return Axios.get(addr);
}

export function SavePerson(person) {
  return DoPost(SAVE_ENDPOINT, person);
}

export function UpdatePerson(person) {
  return DoPut(UPDATE_ENDPOINT.replace("{0}", person.id), person);
}

export function DeletePerson(person) {
  return DoDelete(DELETE_ENDPOINT.replace("{0}", person.id));
}
