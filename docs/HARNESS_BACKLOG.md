# Harness Backlog

Use this file when an agent discovers a missing harness capability but should
not change the operating model immediately.

## Items

---

### Kafka KRaft Listener Configuration

**Discovered while:** US-002 Docker Compose, US-006 Debezium connector

**Current pain:** `confluentinc/cp-kafka` rejects `localhost` and `0.0.0.0` in
`KAFKA_ADVERTISED_LISTENERS` when using KRaft mode. Working config requires
`CONTROLLER://kafka:9093` (hostname, not `0.0.0.0`) and single PLAINTEXT listener
advertising `kafka:9092`. Finding this took several failed attempts and container
recreations.

**Suggested improvement:** Document the exact working KRaft config in
`docs/ARCHITECTURE.md` or `scripts/README.md` so future agents do not repeat
the discovery. Consider a healthcheck-based `depends_on` for Debezium.

**Risk:** tiny

**Status:** proposed

---

### Debezium Schema Wrapper

**Discovered while:** US-010 Worker E2E integration

**Current pain:** Debezium emits `{"schema":{...},"payload":{...}}` by default.
The consumer had to be updated to extract `payload` before deserialization.
This was not visible until the Worker ran against a real Kafka topic.

**Suggested improvement:** Document the envelope format in `docs/product/pipeline.md`
and add a note in the consumer that the `payload` extraction is mandatory unless
`CONNECT_VALUE_CONVERTER_SCHEMAS_ENABLE=false` is set on the Debezium service.

**Risk:** tiny

**Status:** proposed

---

### ClickHouse Authentication in Latest Image

**Discovered while:** US-010 Worker E2E integration

**Current pain:** `clickhouse/clickhouse-server:latest` (v26+) requires a password
via `CLICKHOUSE_PASSWORD` env var. Empty password fails with `REQUIRED_PASSWORD`.
This is a breaking change from older versions that accepted no password.

**Suggested improvement:** Pin ClickHouse image to a specific version in
`docker-compose.yml`, or document the required env vars in `scripts/README.md`.

**Risk:** tiny

**Status:** proposed

---

### Worker Hostname Resolution on Windows Host

**Discovered while:** US-010 Worker E2E integration

**Current pain:** The .NET Worker runs on the Windows host but Kafka advertises
`kafka:9092` (container hostname). The Worker cannot resolve `kafka` without a
hosts file entry. This is a one-time manual step that is easy to miss.

**Suggested improvement:** Add a prerequisite check or setup note to
`scripts/README.md`. Consider a `docker compose` profile that also runs the
Worker as a container to avoid the hosts file dependency entirely.

**Risk:** tiny

**Status:** proposed

---

### Debezium Decimal Handling Mode

**Discovered while:** US-010 Worker E2E integration

**Current pain:** Default decimal mode sends `DECIMAL` columns as base64-encoded
bytes. Worker deserialization fails silently (NullReferenceException in handler).
Required `decimal.handling.mode=double` in the connector config.

**Suggested improvement:** Document this requirement in the Debezium connector
registration command in `scripts/README.md` and in `docs/product/pipeline.md`.

**Risk:** tiny

**Status:** proposed
