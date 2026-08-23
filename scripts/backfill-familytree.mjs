#!/usr/bin/env node
/**
 * One-shot Neo4j backfill from People API (cold graph repair).
 * Not used on the hot family-tree read path.
 *
 * Usage (stack running):
 *   CENSUS_BASE_URL=http://localhost:8080 node scripts/backfill-familytree.mjs
 */
import { createRequire } from 'module';

const base = process.env.CENSUS_BASE_URL || 'http://localhost:8080';
const email = process.env.CENSUS_ADMIN_EMAIL || 'admin@censo.local';
const password = process.env.CENSUS_ADMIN_PASSWORD || 'Admin@12345';

async function login() {
  const res = await fetch(`${base}/auth/api/v1/auth/login`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ email, password }),
  });
  if (!res.ok) throw new Error(`login failed: ${res.status}`);
  const body = await res.json();
  return body.accessToken || body.token || body.access_token;
}

async function listPeople(token, page) {
  const res = await fetch(`${base}/person/api/v1/person?page=${page}`, {
    headers: { Authorization: `Bearer ${token}` },
  });
  if (!res.ok) throw new Error(`list people failed: ${res.status}`);
  return res.json();
}

async function main() {
  console.log('This script lists people via the gateway for operators to re-publish events');
  console.log('or to verify People SoT after a Neo4j wipe. Prefer replaying outbox/events');
  console.log('over inventing a second write path.');
  const token = await login();
  let page = 1;
  let total = 0;
  for (;;) {
    const result = await listPeople(token, page);
    const items = result.items || result.Items || [];
    if (!items.length) break;
    total += items.length;
    console.log(`page ${page}: ${items.length} people`);
    page += 1;
    if (items.length < 10) break;
  }
  console.log(`Listed ${total} people from People SoT. To rebuild Neo4j, republish PersonCreated events or re-seed.`);
}

main().catch((err) => {
  console.error(err);
  process.exit(1);
});
