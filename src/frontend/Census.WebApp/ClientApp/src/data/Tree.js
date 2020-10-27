import Axios from "axios";

const TREE_ENDPOINT = "/family/api/familytree/{0}?level={1}";

export function GetFamilyTree(personId, level) {
  const addr = TREE_ENDPOINT.replace("{0}", personId)
                .replace("{1}", level);

  return Axios.get(addr);
}
