import Axios from "axios";

const PEOPLE_STATS_ENDPOINT = "/stats/api/personcategory?name={0}&sex={1}&education={2}&race={3}";
const PEOPLE_STATS_PERCITY_ENDPOINT = "/stats/api/percitycategory/cities/{0}/counter";
const PEOPLE_NAMECOUNTER_PERCITY_ENDPOINT = "/stats/api/percitycategory/counters/{0}";
const CITIES_ENDPOINT = "/stats/api/percitycategory/cities";

export function GetStats(filter) {
  const addr = PEOPLE_STATS_ENDPOINT.replace("{0}", filter.name ? filter.name : "")
    .replace("{1}", filter.sex ? filter.sex : "")
    .replace("{2}", filter.education ? filter.education : "")
    .replace("{3}", filter.race ? filter.race : "");

  return Axios.get(addr);
}

export function GetStatsPerCity(city) {
  const addr = PEOPLE_STATS_PERCITY_ENDPOINT.replace("{0}", city);
  return Axios.get(addr);
}

export function GetStatsPerNameCounter(name) {
  const addr = PEOPLE_NAMECOUNTER_PERCITY_ENDPOINT.replace("{0}", name);
  return Axios.get(addr);
}

export function GetCities() {
  return Axios.get(CITIES_ENDPOINT);
}
