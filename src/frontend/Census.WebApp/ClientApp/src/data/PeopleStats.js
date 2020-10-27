import Axios from "axios";

const PEOPLE_STATS_ENDPOINT = "/stats/api/personcategory?name={0}&sex={1}&education={2}&race={3}";
const PEOPLE_STATS_PERCITY_ENDPOINT = "/stats/api/percitycategory/cities/{0}/counter";
const PEOPLE_NAMECOUNTER_PERCITY_ENDPOINT = "/stats/api/percitycategory/counters/{0}";
const CITIES_ENDPOINT = "/stats/api/percitycategory/cities";

export function GetStats(name, sex, education, race) {
  const addr = PEOPLE_STATS_ENDPOINT.replace("{0}", name)
    .replace("{0}", sex)
    .replace("{2}", education)
    .replace("{3}", race);

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
