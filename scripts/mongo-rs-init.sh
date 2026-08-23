#!/usr/bin/env bash
# Initialize a single-node MongoDB replica set (required for multi-document transactions).
set -euo pipefail

HOST="${MONGO_HOST:-mongo}"
NOAUTH="${MONGO_NOAUTH:-false}"

mongosh_cmd() {
  if [ "${NOAUTH}" = "true" ]; then
    mongosh --host "${HOST}" "$@"
  else
    USER="${MONGO_INITDB_ROOT_USERNAME:-guest}"
    PASS="${MONGO_INITDB_ROOT_PASSWORD:-guest}"
    mongosh --host "${HOST}" -u "${USER}" -p "${PASS}" --authenticationDatabase admin "$@"
  fi
}

echo "Waiting for MongoDB at ${HOST}..."
until mongosh_cmd --quiet --eval 'db.adminCommand({ ping: 1 })' >/dev/null 2>&1; do
  sleep 2
done

STATUS="$(mongosh_cmd --quiet --eval 'try { rs.status().ok } catch (e) { 0 }' || true)"
if [ "${STATUS}" = "1" ]; then
  echo "Replica set already initialized."
  exit 0
fi

echo "Initiating replica set rs0..."
mongosh_cmd --eval 'rs.initiate({_id: "rs0", members: [{ _id: 0, host: "mongo:27017" }]})'

echo "Waiting for PRIMARY..."
until mongosh_cmd --quiet --eval 'db.hello().isWritablePrimary' | grep -q true; do
  sleep 2
done

echo "Replica set ready."
