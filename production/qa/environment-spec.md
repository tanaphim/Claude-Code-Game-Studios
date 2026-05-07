# QA Environment Specification

> **Purpose:** Canonical environment baseline for performance + load tests.
> Performance ACs are not comparable across environments with different specs —
> every test result must reference this file's pinned values. Closes FT12 World
> Map blocker **qa-7** (directory + spec absent) + **qa-8** (PlayFab rate limit
> not pinned).
>
> **Status:** Initial stub authored 2026-05-05 as part of FT12 Phase 3 Cluster 3
> revision pass. Values marked **TBD** require producer / DevOps decisions before
> performance ACs (TR-WMS-034a/b/c, TR-035, TR-042, TR-055) can execute.
>
> **Owner:** DevOps Engineer (primary) + QA Lead (verification) + Producer
> (procurement decisions).

---

## Hardware Tier (load injector + server staging)

| Field | Value | Notes |
|---|---|---|
| Load injector CPU | **TBD** (recommend 8-core Xeon-class min) | Single-threaded request injector per TR-034 harness spec — must NOT be CPU-bound during 5000-req/10s injection |
| Load injector RAM | **TBD** (recommend 16 GB min) | Holds full request batch + response logs |
| Server staging CPU class | **TBD** | Match production tier ; must be documented |
| Server staging RAM | **TBD** | Match production tier |
| Network bandwidth ceiling (injector→server) | **TBD** (recommend ≥ 1 Gbps) | Must accommodate peak 500 RPS × avg payload ; bandwidth saturation invalidates latency measurements |

## Region & Co-location

| Field | Value | Notes |
|---|---|---|
| Azure region (staging) | **TBD** (recommend Southeast Asia matching production) | Cross-region latency invalidates Photon SLA per network-programmer round 2 review |
| PlayFab region | **TBD** (must match Azure region) | Required for R12 single-`UpdateUserData` atomicity contract — cross-region adds replication lag |
| Photon Fusion 2 region | **TBD** (must match Azure region) | Required for EC-08 M11 filter latency budget (≤150ms) — co-location constraint per network-programmer round 2 |

## PlayFab CCU Plan (closes qa-8)

| Field | Value | Notes |
|---|---|---|
| **PlayFab plan tier** | **"Indie Studio" tier (recommend)** — TBD pending producer approval | Default plan ~1000 calls/s ceiling is INSUFFICIENT for FT12 launch-spike per Cluster 3 #9 ; "Indie Studio" tier (~2500 calls/s) is the minimum that meets FT12 R12 + Scenario E budget |
| **API calls/s ceiling** | **2500** (Indie Studio tier minimum) | TR-WMS-034c asserts measured rate ≤ ceiling × 0.95 (5% headroom) |
| Rate-limit response | HTTP 429 + `Retry-After` header | Server must NOT silently retry past rate-limit ; surface to ops dashboard |
| Plan upgrade approval | **PENDING producer sign-off** | Cost differential vs default plan must be approved before launch ; ops playbook must include plan tier verification step |

## Photon Fusion 2 CCU Plan

| Field | Value | Notes |
|---|---|---|
| Photon plan tier | **TBD** | Must accommodate `LAUNCH_MODE_PREWARM_COUNT × CITY_INSTANCE_SOFT_CAP` = 35 × 150 = 5250 CCU minimum during launch mode |
| Concurrent room ceiling | **TBD** (recommend ≥ 50 rooms per starter city) | Covers prewarm + overflow + EC-04 idle-teardown grace |
| Bandwidth per room | **TBD** | Verify against R9 hard ceiling 180 players × per-player update rate (network-programmer round 2 recommended item) |

## Azure Functions Tier (closes Cluster 6 #14 — pinned per R13.1)

| Field | Value | Notes |
|---|---|---|
| **Functions tier** | **Premium (EP1 minimum) — PINNED per R13.1** | Closes Cluster 6 #14 + Cluster 1 #2 (same cold-start mechanism). Consumption tier explicitly forbidden by R13.1 binding constraint. Producer cost approval **PENDING** but design decision committed |
| Pre-warmed instance count | **3 (minimum) per region** | R13.1 binding ; covers D2 scheduler + R8 fan-out + general CRUD with no contention at steady state |
| Region (must match Azure region above) | **Same as Azure region row above** | Co-location constraint with PlayFab + Photon + Redis per R13.1 |
| Function timeout | **30s** matching `ENTERING_TIMEOUT_SECONDS` | Must not race EC-18 timeout |
| Scale-out limits | EP1 default (per Azure plan tier docs) | Verify against launch-mode peak load (5000 reqs/10s × 3 PlayFab calls + Redis CAS calls) |

## Redis Tier (closes Cluster 2 #5 + #6 — pinned per R13.1)

| Field | Value | Notes |
|---|---|---|
| **Redis SKU** | **Azure Cache for Redis Premium P1 (minimum) — PINNED per R13.1** | Closes Cluster 2 #5 (instance population CAS store named) + #6 (cache layer named single-shared instance). Standard SKU has insufficient throughput for 500 RPS CAS; Premium provides clustering + replication for production resilience |
| Region (must match Azure region above) | **Same as Azure region row above** | Co-location constraint with Functions Premium per R13.1 — sub-ms latency depends on it |
| Keyspace separation | `inst-pop:*` (no TTL, CAS) + `cache:*` (TTL ≤ 5s, read-through) | Per R12 + R13.1 ; eviction policy `volatile-lru` (only TTL'd keys evict) |
| Lua-script CAS pattern | EVAL with `KEYS[1]` = pop key, `ARGV[1]` = party_size, `ARGV[2]` = hard_ceiling | Atomic read-check-write within single Redis op ; verified by TR-WMS-038a/b |
| Producer approval | **PENDING** (cost) | P1 SKU price differential must be approved ; ops playbook must include Redis health-check |

## Network Bandwidth & Connection Limits

| Field | Value | Notes |
|---|---|---|
| Server inbound bandwidth ceiling | **TBD** (must exceed peak 500 RPS × avg payload + Photon room traffic) | |
| Server outbound bandwidth ceiling | **TBD** (must exceed peak presence broadcast × 5250 CCU) | |
| Concurrent TCP connection ceiling | **TBD** (≥ 5500 to cover spike + Photon overhead) | |
| Azure Functions max concurrent execution | **TBD** | Consumption tier has soft limits ; Premium has plan-tier limits — verify against R12 D1 invocation rate |

## Load Harness Specification

| Field | Value | Notes |
|---|---|---|
| Harness type | Single-threaded request injector | Per TR-WMS-034 family — deterministic ordering required |
| Request distribution | Uniform 500 RPS over 10s window for TR-034a/b/c | Matches Scenario E spike profile |
| Mock services | PlayFab mock OR live "Indie Studio" tier ; FT13 mock per TR-WMS-032 ; M11 mock per TR-WMS-033 | Mock contract pinned in `tests/mocks/` (TBD — qa-lead recommended item) |
| Test data isolation | Each TR-WMS-034 run uses fresh Player Records ; cleanup between runs | Prevents cross-run state pollution |
| Disconnect injection (TR-055) | Mock network layer drops 10% of EnteringCity sessions at random | Reproduces EC-14 ghost-slot scenario |

## Acceptance Criteria Mapping

| AC | Required env-spec field |
|---|---|
| TR-WMS-034a (prewarm cohort) | Hardware tier + region + Functions tier + Load harness + PlayFab plan |
| TR-WMS-034b (overflow cohort) | All above + steady-state `STARTER_CITY_INSTANCE_PREWARM_COUNT=5` config |
| TR-WMS-034c (PlayFab rate-limit) | PlayFab plan tier + measured rate telemetry hook |
| TR-WMS-035 (Fragment Event spike) | All above + `EVENT_ANNOUNCE_LEAD_SECONDS` + `D2_SCHEDULER_TICK_SECONDS` config |
| TR-WMS-042 (mid-range overflow) | All above + `pre-warm=5 (steady)` config |
| TR-WMS-055 (ghost-slot accounting) | All above + disconnect injection harness + `ENTERING_HOLD_SECONDS=30` config |

## Producer Decisions Outstanding

1. **PlayFab plan tier upgrade** (cost approval) — minimum "Indie Studio" tier (~2500 calls/s) per FT12 Cluster 3 #9
2. ~~Azure Functions Premium tier~~ **PINNED per R13.1 (Phase 3 Cluster 6 closure 2026-05-05)** — cost approval still pending but design decision committed
3. ~~Redis Premium SKU~~ **PINNED per R13.1 (Phase 3 Cluster 2 closure 2026-05-05)** — cost approval still pending but design decision committed
4. **Photon Fusion 2 plan tier** (cost approval) — must accommodate launch-mode CCU
5. **Hardware tier for staging** — match production class so that TR-034 results are meaningful baseline
6. **Launch-mode playbook authoring** (ops) — codified switch from steady-state to `LAUNCH_MODE_PREWARM_COUNT=35` ; revert process ; verification checklist

## Change Log

| Date | Change | Trigger |
|---|---|---|
| 2026-05-05 | Initial stub authored | FT12 Phase 3 Cluster 3 revision (closes qa-7 + qa-8 partial — TBD fields must be filled by producer before perf ACs execute) |
| 2026-05-05 | Functions tier + Redis SKU pinned (Premium for both) | FT12 Phase 3 Cluster 6 + Cluster 2 paired pass — R13.1 binding constraint authored ; producer cost approval still required but design choice committed |
