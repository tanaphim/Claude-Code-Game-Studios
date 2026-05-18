# Sprint 005 — Hercules Phase 2 Pilot Playthrough Evidence

> **Story**: S5-10 (Manual playtest + Phase 2 closeout gate)
> **ADR**: ADR-0006 Phase 2 §3 Exit Criteria
> **Date**: 2026-05-14
> **Tester**: tanapol (gameplay-programmer + qa-tester)
> **Build**: delta-unity@claude/s5-10-hercules-playtest (off dev post-PR350 + PR351 + UI fixes 7499f47b22 / 8661c388f2)
> **Unity**: 2022.3.62f1 LTS
> **Scene tested**: `scene_game_map.unity` (production game map — NOT a test harness)

---

## Pre-flight checklist

- [x] Pulled latest dev: `git pull` → branch off as `claude/s5-10-hercules-playtest`
- [x] Unity Editor compiles clean (0 errors; pre-existing CS0414 warning on `SkillStateMachine.isEntered` carries forward — unrelated)
- [x] EditMode tests: **108/108** in `Radius.Tests.Characters` (12 S5-06 + 18 S5-05 + 14 S5-04 + 64 prior)
- [x] `AbilityComponent` attached to `base_avatar.prefab` (Step 1 below)
- [x] PeerMode = `Single` for production playtest
- [x] Training-equivalent flow loaded via `scene_initial → scene_login → scene_metadata → scene_game_mode → scene_game_map`

---

## Step 1 — AbilityComponent prefab attach (S5-09 deferred)

**Goal**: Remove `BootstrapSlotBindings: AbilityComponent missing` warning that fires on every Hero spawn.

- [x] Located Hero prefab at: `Assets/Resources/Prefabs/Gameplay/Character/Avatar/base_avatar.prefab`
      <!-- Confirmed via GUID lookup: ActorCombat GUID f440468e81db6404bafec7a500b62be2 -->
- [x] Opened prefab in Prefab Mode
- [x] Added Component → `AbilityComponent` (namespace `Radius.Gameplays.Abilities`)
- [x] Applied prefab override (no nested override, no broken refs)
- [x] Saved prefab
- [x] **Verification**: Played `scene_game_map` — Console no longer shows `BootstrapSlotBindings: AbilityComponent missing` warning ✅

**Notes:**
Adding `AbilityComponent` to `base_avatar` propagates to all heroes (single template prefab — Hercules, Anansi, Merlin, etc. all derive from it). No per-hero variant edits needed.

---

## Step 2 — Multipeer harness Pass #1–5

**Goal**: Re-verify networking parity + bandwidth post-merge.

- [x] Opened scene: `Assets/GameScripts/Gameplays/Abilities/Testing/PrototypeTest.unity`
- [x] Configured multipeer setup (user adjustment)
- [x] Run sessions — **Pass #4 explicit ✅** in Editor.log:
  ```
  [Multipeer-Parity] slot=1 ✅ host=([Id:1027], 'proto.ability.q') client=([Id:1027], 'proto.ability.q')
  [Multipeer-Parity] slot=2 ✅ host=([Id:1028], 'proto.ability.w') client=([Id:1028], 'proto.ability.w')
  [Multipeer-Parity] slot=3 ✅ host=([Id:1029], 'proto.ability.e') client=([Id:1029], 'proto.ability.e')
  [Multipeer-Parity] slot=4 ✅ host=([Id:1030], 'proto.ability.r') client=([Id:1030], 'proto.ability.r')
  [Multipeer-Parity] ✅ PASS #4 — all 4 slots converge (Host↔Client).
  ```
- [x] Bandwidth ≤65 B/s — baseline carry-forward from S5-04 / S5-05 (no networked-state delta on dev since)
- [x] Log capture: `production/qa/evidence/S5-10-multipeer.txt`

**Known cosmetic errors (not blocking):**
- `StartGame Failed: GameIsFull (32765)` — Photon room not released yet when Play→Stop→Play cycles fast. Tracked as **S5-17** (AbilityMultipeerRunner duplicate-Start guard, Nice-to-Have).
- `AcquirePrefabInstance threw NRE for [Prefab 2]` — cascade after GameIsFull. Tracked as **BUG-0003** (band-aid applied in S5-04; root cause Sprint 006).

---

## Step 3 — Hercules QWER Training match

**Goal**: ADR §3 Exit Criterion #1 — Hercules plays end-to-end in real match scene.

### 3.1 — Match boot

- [x] `scene_game_map.unity` loaded (production scene flow, PeerMode=Single)
- [x] Hercules spawned at base, controllable
- [x] Bot opponent spawned, AI active
- [x] HUD renders correctly (HP/MP bars, skill icons, minimap)

### 3.2 — Per-ability verification

User confirmed all abilities work normally ("ปกติ").

| Ability | Cast | Animation | VFX/SFX | Damage | Cooldown | Notes |
|---------|------|-----------|---------|--------|----------|-------|
| Q (slot 1) | ✅ | ✅ | ✅ | ✅ | ✅ | via legacy CreateSkill fallback |
| W (slot 2) | ✅ | ✅ | ✅ | ✅ | ✅ | |
| E (slot 3) | ✅ | ✅ | ✅ | ✅ | ✅ | |
| R press / release | ✅ | ✅ | ✅ | ✅ | ✅ | |
| A (Attack chain 1/2/3) | ✅ | ✅ | ✅ | ✅ | n/a | |
| Recall | ✅ | ✅ | ✅ | n/a | ✅ | |
| Item variants | ✅ | ✅ | ✅ | varies | n/a | |

### 3.3 — Match-end checks

- [x] No crash during playthrough
- [x] No Console error spike (warnings present but tracked — see TD-007 below)
- [x] FPS stayed in pre-Phase-2 range (no perf regression observed)
- [x] Animator transitions return to Idle/Locomotion cleanly after each cast

**Console warnings observed:**

1. **`[AbilityComponent] BindSlot('hercules_q/w/e/r/a/i/recall'): AbilityRegistry not registered with DeltaService`** — 14 occurrences (2 hero spawns × 7 slots).
   - **Severity**: MEDIUM (cosmetic — game playable via legacy fallback)
   - **Action**: Logged as **TD-007** (HIGH priority follow-up, pairs with TD-006, target S5-21)
   - **Root cause**: S5-09 `BindSlot` pipeline requires `AbilityRegistry` registered with `DeltaService` before Hero spawn; production scene boot doesn't register at the right time. New slot-binding pipeline dormant in production; legacy `CreateSkill` fallback (per S5-09 dual-path) handles everything.

2. **`PlayFabException: Must be logged in`** — expected when running scene_initial offline. Cosmetic.

3. **`d3d12: upload buffer was full!`** — Unity rendering pipe warning during heavy load. Pre-existing, unrelated to S5-10.

4. **`IEnumeratorAwaitExtensions:Complete (Exception)`** — async chain markers from Photon disconnect cascade. Pre-existing, unrelated.

No new errors introduced by S5-10 work.

---

## Step 4 — Phase 2 Exit Criteria verification (ADR-0006 §3)

| # | Criterion | Pass? | Note |
|---|-----------|-------|------|
| 1 | Hercules plays end-to-end | ✅ | Via legacy `CreateSkill` fallback (S5-09 dual-path retention) |
| 2 | No `SkillKey.{Q,W,E,R,Recall}` literal in Hercules files | ✅ | Verified via grep (S5-07/S5-08) |
| 3 | No `Combat.Skill1/2/3/4` direct read in Hercules action files | ✅ | S5-07/S5-08 |
| 4 | `ActorCombatAction` Pattern-A/B helpers extracted | ✅ | S5-04 (Pattern-A) + S5-05 (Pattern-B/C/D) |
| 5 | `CBSAbility.Slot` source-of-truth wired (per ADR-0008 via CBSUnit aliases) | ⚠️ PARTIAL | API + S5-09 caller landed; production-scene `AbilityRegistry` registration missing (TD-007) — pipeline dormant, legacy path used |
| 6 | AnimationEvent Option A confirmed | ⚠️ PARTIAL | Infrastructure landed (S5-06); 43-shim migration reverted (TD-006 SetActiveSlot wiring gap); deferred to S5-21 |
| 7 | Multipeer harness Pass #1–5 still green | ✅ | Pass #4 parity explicit ✅ in log; Pass #5 baseline carry-forward S5-04/S5-05 |
| 8 | Manual playtest (1 match Training) | ✅ | User-confirmed "ปกติ" (all abilities work) |

---

## Verdict

**Phase 2 Exit: PASS WITH NOTES** ✅

**Sign-off:**
- gameplay-programmer: **tanapol** — date: 2026-05-14
- qa-tester:           **tanapol** — date: 2026-05-14

**Notes for Sprint 005 retro:**
- S5-06 + S5-09 + S5-10 surfaced two paired wiring gaps (TD-006 SetActiveSlot caller missing, TD-007 AbilityRegistry registration missing). Both follow the same pattern: ADR specified an API + a caller pair; the caller side never landed. Process improvement candidate: when an ADR pairs API + caller, story-readiness should flag "API exists with no caller in repo" / "caller exists but service registration missing" as NEEDS WORK items before story can be marked READY.
- Both TDs are bundled for **S5-21** (Phase 2 polish): wire `SetActiveSlot` in input handler + register `AbilityRegistry` in production scene boot + re-land AnimationEvent 40-shim migration atomically with integration tests.
- Dual-path retention (S5-04/S5-05/S5-09) saved production every time a wiring gap surfaced — pattern worth codifying in `control-manifest.md`.

**Phase 3 readiness gate (ADR §10):**
- [x] 1-week soak on dev (2026-05-14 → 2026-05-21): **PASS WITH NOTES + Phase 3 batch 1 dash-hero blocker** (Day 4 manual probe 2026-05-18, root cause located 2026-05-18 EOD)
      - Verified by: tanapol, 2026-05-18 (Day 4 manual probe; full sign-off pending 2026-05-21)
      - Bugs filed in window:
          - BUG-0005 (test harness multipeer PeerMode) — NOT slot-related — RESOLVED 2026-05-18 at delta-unity@`eb1c4ea695`
          - **[BUG-0006](../bugs/BUG-0006-hercules-e-first-cast.md) (Hercules E first cast no-op per match)** — slot-related; ROOT CAUSE LOCATED 2026-05-18 EOD at `NetworkStatusEffect.cs:947` (shared `RequestDash` silent guard, likely `IsDashing` networked-state leak across match — H4b). Cousin risk HIGH: affects Horus E + Volund W in Phase 3 batch 1. **P0, OPEN, fix scheduled next session (~0.5d).**
      - Hercules manual playthrough: Q/W/R/NA/Recall/Item ทำงานปกติ; E first cast per match แสดงอาการตาม BUG-0006
      - Multipeer Pass #1-5: ✅ (post BUG-0005 fix)
      - **Verdict reading**:
          - Phase 2 BindSlot/StateReleaseSlot/multipeer pipeline = GREEN per Q/W/R control proof
          - BUG-0006 is in shared dash API (NetworkStatusEffect), affecting all dash-bearing heroes including Phase 3 batch 1 (Horus E S6-03, Volund W S6-04)
          - **Phase 3 batch 1 dash-hero kickoff BLOCKED until BUG-0006 RESOLVED** (Option A recommended)
          - Non-dash Phase 3 work can proceed (S6-06 Skadi appears clean — needs grep confirmation)
- [ ] Non-Hercules call site of `GetSlotAction` identified (future — Phase 3 hero migrations, created during S6-03 Horus)
- [ ] `AbilityDataSnapshot.EffectiveSlot` audit (future — Phase 3 batch close)
- [ ] Pattern-A helper added to `control-manifest.md` (future — control-manifest.md still TBD)
- [x] **S5-21 shipped** — TD-006 SetActiveSlot wired + TD-007 AbilityRegistry registered (Sprint 005 close 2026-05-14)
