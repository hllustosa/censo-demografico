#!/usr/bin/env node
/**
 * Seed ~100 people via the People API (through the gateway by default).
 *
 * Usage:
 *   node scripts/seed-people.mjs
 *   CENSUS_COUNT=50 node scripts/seed-people.mjs
 *   make seed-people
 *
 * Env:
 *   CENSUS_BASE_URL   default http://localhost:8080
 *   CENSUS_EMAIL      default admin@censo.local
 *   CENSUS_PASSWORD   default Admin@12345
 *   CENSUS_COUNT      default 100
 *   CENSUS_REQUEST_MS delay between creates (default 700; stays under API rate limit)
 */

const BASE_URL = (process.env.CENSUS_BASE_URL ?? "http://localhost:8080").replace(/\/$/, "");
const EMAIL = process.env.CENSUS_EMAIL ?? "admin@censo.local";
const PASSWORD = process.env.CENSUS_PASSWORD ?? "Admin@12345";
const TOTAL = Math.max(1, Number.parseInt(process.env.CENSUS_COUNT ?? "100", 10));
const REQUEST_GAP_MS = Math.max(0, Number.parseInt(process.env.CENSUS_REQUEST_MS ?? "700", 10));
const MAX_RETRIES = 8;

const sleep = (ms) => new Promise((resolve) => setTimeout(resolve, ms));

const SEX = ["M", "F"];
const RACES = ["Branco(a)", "Pardo(a)", "Negro(a)", "Amarelo(a)", "Indígena", "Não Informada"];
const EDUCATIONS = [
  "Analfabeto(a)",
  "Alfabetizado(a)",
  "Ensino Fundamental",
  "Ensino Médio",
  "Ensino Superior",
  "Pós-Graduação",
];

const FIRST_NAMES_M = [
  "João", "Pedro", "Carlos", "Lucas", "Mateus", "Rafael", "Bruno", "Felipe",
  "Gustavo", "Henrique", "Marcos", "Paulo", "Ricardo", "Roberto", "Tiago",
  "André", "Daniel", "Eduardo", "Fernando", "Gabriel",
];

const FIRST_NAMES_F = [
  "Maria", "Ana", "Juliana", "Fernanda", "Patricia", "Camila", "Amanda",
  "Beatriz", "Carla", "Daniela", "Helena", "Isabela", "Larissa", "Mariana",
  "Natália", "Paula", "Renata", "Sandra", "Tatiana", "Vera",
];

const LAST_NAMES = [
  "Silva", "Santos", "Oliveira", "Souza", "Lima", "Pereira", "Costa", "Rodrigues",
  "Almeida", "Nascimento", "Carvalho", "Ribeiro", "Martins", "Araújo", "Barbosa",
  "Ferreira", "Gomes", "Rocha", "Dias", "Melo",
];

const CITIES = [
  { city: "São Paulo", state: "SP", burrow: "Centro" },
  { city: "Rio de Janeiro", state: "RJ", burrow: "Copacabana" },
  { city: "Belo Horizonte", state: "MG", burrow: "Savassi" },
  { city: "Curitiba", state: "PR", burrow: "Batel" },
  { city: "Porto Alegre", state: "RS", burrow: "Moinhos de Vento" },
  { city: "Salvador", state: "BA", burrow: "Barra" },
  { city: "Recife", state: "PE", burrow: "Boa Viagem" },
  { city: "Fortaleza", state: "CE", burrow: "Aldeota" },
  { city: "Brasília", state: "DF", burrow: "Asa Sul" },
  { city: "Campinas", state: "SP", burrow: "Cambuí" },
];

function pick(list) {
  return list[Math.floor(Math.random() * list.length)];
}

function pickDifferent(list, exclude) {
  const filtered = list.filter((item) => item !== exclude);
  return pick(filtered.length ? filtered : list);
}

function buildName(sex, index) {
  const first = sex === "M" ? pick(FIRST_NAMES_M) : pick(FIRST_NAMES_F);
  const last = pick(LAST_NAMES);
  return `${first} ${last} ${index + 1}`;
}

function buildAddress() {
  const place = pick(CITIES);
  const number = 100 + Math.floor(Math.random() * 900);
  return {
    zipCode: `${10000 + Math.floor(Math.random() * 89999)}-${100 + Math.floor(Math.random() * 899)}`,
    addressDesc: `Rua ${pick(LAST_NAMES)}, ${number}`,
    complement: Math.random() > 0.6 ? `Apto ${Math.floor(Math.random() * 200)}` : "",
    burrow: place.burrow,
    city: place.city,
    state: place.state,
  };
}

function buildPersonPayload(name, sex, fatherId, motherId) {
  const body = {
    name,
    sex,
    race: pick(RACES),
    education: pick(EDUCATIONS),
    address: buildAddress(),
  };
  if (fatherId) body.fatherId = fatherId;
  if (motherId) body.motherId = motherId;
  return body;
}

async function request(path, options = {}) {
  for (let attempt = 1; attempt <= MAX_RETRIES; attempt += 1) {
    const response = await fetch(`${BASE_URL}${path}`, options);
    const text = await response.text();
    let data;
    try {
      data = text ? JSON.parse(text) : null;
    } catch {
      data = text;
    }

    if (response.ok) return data;

    if (response.status === 429 && attempt < MAX_RETRIES) {
      const retryAfterHeader = Number.parseInt(response.headers.get("retry-after") ?? "", 10);
      const waitMs = Number.isFinite(retryAfterHeader) && retryAfterHeader > 0
        ? retryAfterHeader * 1000
        : Math.min(60_000, 2_000 * 2 ** (attempt - 1));
      process.stdout.write(
        `\nRate limited (429). Waiting ${Math.ceil(waitMs / 1000)}s before retry ${attempt}/${MAX_RETRIES - 1}...`
      );
      await sleep(waitMs);
      continue;
    }

    const detail = data?.detail ?? data?.title ?? text ?? response.statusText;
    throw new Error(`${response.status} ${path}: ${detail}`);
  }

  throw new Error(`429 ${path}: exceeded retries after rate limiting`);
}

async function login() {
  const data = await request("/auth/api/v1/auth/login", {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify({ email: EMAIL, password: PASSWORD }),
  });
  const token = data.accessToken ?? data.AccessToken;
  if (!token) throw new Error("Login succeeded but no access token was returned.");
  return token;
}

async function createPerson(token, payload) {
  const data = await request("/person/api/v1/person/", {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
      Authorization: `Bearer ${token}`,
    },
    body: JSON.stringify(payload),
  });
  if (REQUEST_GAP_MS > 0) await sleep(REQUEST_GAP_MS);
  return data.id ?? data.Id;
}

function planGenerations(total) {
  if (total <= 2) {
    return { grandparents: total, parents: 0, children: 0 };
  }

  let grandparents = Math.max(2, Math.round(total * 0.3));
  let parents = Math.max(2, Math.round(total * 0.4));
  let children = total - grandparents - parents;

  if (children < 1) {
    const trim = 1 - children;
    parents = Math.max(2, parents - trim);
    children = total - grandparents - parents;
  }

  while (grandparents + parents + children > total) {
    if (parents > 2) parents -= 1;
    else if (grandparents > 2) grandparents -= 1;
    else if (children > 1) children -= 1;
    else break;
  }

  while (grandparents + parents + children < total) {
    children += 1;
  }

  return { grandparents, parents, children };
}

async function main() {
  console.log(`Seeding ${TOTAL} people via ${BASE_URL} ...`);
  if (REQUEST_GAP_MS > 0) {
    console.log(`Pacing: ${REQUEST_GAP_MS}ms between creates (override with CENSUS_REQUEST_MS).`);
  }

  const token = await login();
  console.log("Authenticated.");

  const { grandparents, parents, children } = planGenerations(TOTAL);
  const grandpaIds = [];
  const grandmaIds = [];
  const parentMaleIds = [];
  const parentFemaleIds = [];

  let created = 0;
  let index = 0;

  const grandpaTarget = Math.ceil(grandparents / 2);
  const grandmaTarget = grandparents - grandpaTarget;

  for (let i = 0; i < grandpaTarget; i += 1) {
    const id = await createPerson(token, buildPersonPayload(buildName("M", index), "M"));
    grandpaIds.push(id);
    created += 1;
    index += 1;
    process.stdout.write(`\rCreated ${created}/${TOTAL}`);
  }

  for (let i = 0; i < grandmaTarget; i += 1) {
    const id = await createPerson(token, buildPersonPayload(buildName("F", index), "F"));
    grandmaIds.push(id);
    created += 1;
    index += 1;
    process.stdout.write(`\rCreated ${created}/${TOTAL}`);
  }

  const parentMaleTarget = Math.ceil(parents / 2);
  const parentFemaleTarget = parents - parentMaleTarget;

  for (let i = 0; i < parentMaleTarget; i += 1) {
    const id = await createPerson(
      token,
      buildPersonPayload(buildName("M", index), "M", pick(grandpaIds), pick(grandmaIds))
    );
    parentMaleIds.push(id);
    created += 1;
    index += 1;
    process.stdout.write(`\rCreated ${created}/${TOTAL}`);
  }

  for (let i = 0; i < parentFemaleTarget; i += 1) {
    const id = await createPerson(
      token,
      buildPersonPayload(buildName("F", index), "F", pick(grandpaIds), pick(grandmaIds))
    );
    parentFemaleIds.push(id);
    created += 1;
    index += 1;
    process.stdout.write(`\rCreated ${created}/${TOTAL}`);
  }

  for (let i = 0; i < children; i += 1) {
    const fatherId = pick(parentMaleIds);
    const motherId = pickDifferent(parentFemaleIds, fatherId);
    const sex = pick(SEX);
    await createPerson(
      token,
      buildPersonPayload(buildName(sex, index), sex, fatherId, motherId)
    );
    created += 1;
    index += 1;
    process.stdout.write(`\rCreated ${created}/${TOTAL}`);
  }

  console.log("\nDone.");
  console.log(`  Grandparents: ${grandparents} (${grandpaTarget}M + ${grandmaTarget}F)`);
  console.log(`  Parents:      ${parents} (${parentMaleTarget}M + ${parentFemaleTarget}F)`);
  console.log(`  Children:     ${children}`);
  console.log("Events were published via outbox — stats and family tree sync in a few seconds.");
}

main().catch((error) => {
  console.error("\nSeed failed:", error.message);
  process.exit(1);
});
