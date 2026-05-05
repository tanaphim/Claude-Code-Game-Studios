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

## Azure Functions Tier (cross-references Cluster 6 #14)

| Field | Value | Notes |
|---|---|---|
| **Functions tier** | **TBD — Cluster 6 #14 unresolved** | Consumption tier has 200–800ms cold-start that violates D2 5s tick budget per Scenario E exposure ; recommend **Premium + pre-warmed** ; producer cost approval required ; binding decision needed before TR-035 + Scenario E perf ACs execute |
| Pre-warmed instance count | **TBD** (recommend ≥ 3 if Premium chosen) | Eliminates cold-start latency for D2 scheduler tick |
| Region (must match Azure region above) | **TBD** | Co-location constraint per network-programmer |
| Function timeout | **TBD** (recommend 30s matching `ENTERING_TIMEOUT_SECONDS`) | Must not race EC-18 timeout |

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
2. **Azure Functions Premium tier** (cost approval) — required by FT12 Cluster 6 #14 for D2 5s tick + R8 1s skew lower bound
3. **Photon Fusion 2 plan tier** (cost approval) — must accommodate launch-mode CCU
4. **Hardware tier for staging** — match production class so that TR-034 results are meaningful baseline
5. **Launch-mode playbook authoring** (ops) — codified switch from steady-state to `LAUNCH_MODE_PREWARM_COUNT=35` ; revert process ; verification checklist

## Change Log

| Date | Change | Trigger |
|---|---|---|
| 2026-05-05 | Initial stub authored | FT12 Phase 3 Cluster 3 revision (closes qa-7 + qa-8 partial — TBD fields must be filled by producer before perf ACs execute) |
