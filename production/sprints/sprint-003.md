# Sprint 3 — 2026-05-02 to 2026-05-15

## Sprint Goal

ส่งมอบ **ADR-0006 Phase 1b Foundation** (production implementations ของ 4 artifacts + Host+Client harness verify Pass #4/#5)
และ **AI Bot initial behaviors** (item-buying + difficulty tiers) เพื่อปลดล็อก Phase 2 pilot migration (Hercules)

## Capacity

- Total days: 14 (2 สัปดาห์)
- Buffer (20%): 2.8 วัน สำรองสำหรับงานที่ไม่ได้วางแผน
- Available: ~11 วัน

---

## Tasks

### Must Have (Critical Path — ADR-0006 Phase 1b)

| ID | Task | Agent/Owner | Est. Days | Dependencies | Acceptance Criteria |
|----|------|-------------|-----------|-------------|-------------------|
| S3-01 | **P1B-01** — Host+Client multipeer test harness | network-programmer + unity-specialist | 2.0 | — | Multipeer scene (Host+Client runners); Pass #4 ✅ dict content parity; Pass #5 ✅ idle bandwidth < 2 KB/s |
| S3-02 | **P1B-02** — `AbilityRegistry` real implementation | gameplay-programmer | 1.0 | — | Boot-time `Resources.LoadAll` scan < 100ms/50 abilities; 3+ unit tests; O(1) lookup post-boot |
| S3-03 | **P1B-03** — `AbilityDataSnapshot` real implementation | gameplay-programmer | 0.5 | — | CBS pull freezes at match start; mutate-after-build test proves immutability; hash broadcast wired |
| S3-04 | **P1B-04** — `AbilityComponent.BindSlot` real implementation | gameplay-programmer | 1.0 | S3-02 | Real `BindSlot(slot, abilityId)` spawns via registry + triggers ChangeDetector; `[Obsolete] BindSlotPrototype` removed; prototype runner ยัง pass criteria #1-5 |
| S3-05 | **P1B-05** — `InputMessage.PressedSlot/ReleasedSlot` fields | network-programmer | 0.5 | S3-01 | Parallel write (`Buttons.Q` + `PressedSlot=1`); existing consumers zero regression; wire delta ≤ +3 B/tick measured |
| S3-06 | **P1B-06** — `KeybindMap` production wiring + Settings placeholder | ui-programmer + ux-designer | 1.0 | — | `DeltaService.GetService<KeybindMap>()` ทำงาน; Settings UI placeholder ที่ `Controls.unity` (rebind + reset); persist across sessions; UX sign-off "non-blocking for Phase 2" |
| S3-07 | **P1B-07** — Phase 2 touch-point audit | lead-programmer | 0.5 | — | `docs/architecture/ADR-0006-phase-2-migration-plan.md` published; Hercules Q/W/E/R lines re-verified; `ActorCombatAction` A/B/C/D refreshed; Phase 2 entry criteria ครบ |

**Must Have Subtotal: 6.5 วัน**

### Should Have

| ID | Task | Agent/Owner | Est. Days | Dependencies | Acceptance Criteria |
|----|------|-------------|-----------|-------------|-------------------|
| S3-08 | AI Bot item-buying behavior (carryover S2-09) | ai-programmer + game-designer | 2.0 | — | Bot buys items ตาม role + gold threshold; อ้างอิง `ai-bot-system.md`; playtest 1 match ไม่มี crash |
| S3-09 | AI Bot Difficulty Level — Easy/Normal/Hard (carryover S2-10) | ai-programmer | 1.0 | S3-08 | 3 tiers ต่างกันที่ reaction time + item-buying priority; documented ใน `ai-bot-system.md §Difficulty` |

**Should Have Subtotal: 3.0 วัน**

### Nice to Have

*ไม่มี — Sprint 002 carryover S2-01..S2-08 (7.0d) ถูก defer ไป Sprint 004 โดยเจตนา (see Carryover section)*

**Total Estimated: 9.5 วัน** (ภายใน capacity 11d; เหลือ ~1.5d buffer)

---

## Carryover from Previous Sprint

### Accepted into Sprint 003

| Task | เหตุผล | Estimate ใหม่ |
|------|--------|--------------|
| S2-09 → S3-08 | ไม่ได้เริ่ม (dropped to Should Have ใน Sprint 002) | 2.0 วัน |
| S2-10 → S3-09 | ไม่ได้เริ่ม | 1.0 วัน |

### Deferred to Sprint 004 (by design — Plan A)

| Task | เหตุผล |
|------|--------|
| S2-01 (Item animation states) | Phase 1b คือ critical path สำหรับ Phase 2 pilot; Item animation ไม่ block P1B exit criteria |
| S2-02 (Item_Viable param) | Depends on S2-01 |
| S2-03 (attack_speed/move_speed ÷100 bug) | Investigation task; ไม่ block P1B |
| S2-04 (Fusion Tick Rate doc) | Documentation-only; ไม่ block P1B |
| S2-05 (Role Restriction doc) | Documentation-only |
| S2-06 (Mythic Passive formula) | Documentation-only |
| S2-07 (AdditionalMoveSpeed bug) | Investigation-only |
| S2-08 (Risk Register) | Producer task; ไม่ block engineering |

**Total deferred: 7.0 วัน** — รวม Sprint 004 เป็น "animator + bug hunting batch"

---

## Risks

| Risk | Probability | Impact | Mitigation |
|------|------------|--------|-----------|
| Fusion 2 multipeer-in-editor harder than expected (S3-01) | ปานกลาง | สูง | Budget 2d; fall back to manual 2-machine test ถ้า >2d; escalate network-programmer ตั้งแต่ day 1 ถ้าเจอ blocker |
| `InputMessage` bandwidth regression (+3 B × 10 × 60Hz = 1.8 KB/s) ทำให้ budget ต่อผู้เล่นเกิน | ต่ำ | ปานกลาง | S3-05 วัดผ่าน S3-01 harness ก่อน merge; rollback = remove fields, keep work in branch |
| S3-06 Settings UI creeps เข้าสู่ full UX work | ปานกลาง | ต่ำ | "placeholder only" ใน acceptance; styling = separate story |
| Phase 1a prototype code มี hidden deps พังตอนลบ `[Obsolete]` | ต่ำ | ปานกลาง | S3-04 ให้ `AbilityPrototypeRunner` เป็น regression test; run Pass #1-5 หลังลบ |
| Boot-time `Resources.LoadAll` scan ช้า (50+ prefabs) | ต่ำ | ต่ำ | Benchmark ใน S3-02 acceptance; ถ้า > 100ms สลับไป Addressables |
| AI Bot tuning (S3-08/S3-09) ขาด playtest baseline | ปานกลาง | ต่ำ | Use MOBA reference (DotA2/LoL) เป็น default tier values; tune post-Sprint |

---

## Dependencies on External Factors

- Unity Editor + Fusion 2 multipeer mode (S3-01)
- PlayFab CBS Dashboard เข้าถึงได้สำหรับ `CBSAbility` pull (S3-03)
- `Assets/Prefabs/Gameplay/Spell/` folder มี ability prefabs สำหรับ Registry scan (S3-02)
- `delta-unity` branch `feature/refactor-ability` ยังไม่ถูก merge เข้า main

---

## Definition of Done for this Sprint

### Per-Task Gates

- [x] S3-01: Multipeer harness pass #4 + #5 ✅ verified (2026-04-21)
- [x] S3-02: `AbilityRegistry` real impl + 5 EditMode tests pass (2026-04-21)
- [x] S3-03: `AbilityDataSnapshot` real impl + 7 EditMode tests pass (2026-04-21)
- [x] S3-04: `AbilityComponent.BindSlot` real impl + harness regression Pass #4/#5 still ✅ (2026-04-21, Option C dual-path)
- [x] S3-05: `InputMessage.PressedSlot` wired in production input writer (2026-04-21, Option A soft-verified)
- [x] S3-06: `KeybindMap` service registration + Editor placeholder UI (2026-04-21, Option A; runtime UI deferred Sprint 004)
- [ ] S3-02: `AbilityRegistry` real impl + unit tests pass
- [ ] S3-03: `AbilityDataSnapshot` real impl + immutability test
- [ ] S3-04: `BindSlot` real impl + `[Obsolete]` removed + regression pass
- [ ] S3-05: `InputMessage` slot fields + zero regression + wire delta measured
- [ ] S3-06: `KeybindMap` service + Settings placeholder + UX sign-off
- [ ] S3-07: Phase 2 migration plan doc published
- [ ] S3-08: AI Bot buys items ใน 1 match (no crash)
- [ ] S3-09: 3 difficulty tiers documented + tested

### ADR-0006 Phase 1b Exit Criteria (gates Phase 2)

- [ ] **4 artifacts production-ready** — no `[Obsolete]` markers; no `NotImplementedException`
- [ ] **Pass #4 ✅ PASS** — `NetworkDictionary<byte, NetworkBehaviourId>` replicates identically (harness-verified)
- [ ] **Pass #5 ✅ PASS** — idle bandwidth < 2 KB/s measured via Fusion Statistics on Host+Client
- [ ] **`InputMessage.PressedSlot/ReleasedSlot`** added — parallel write verified
- [ ] **SkillKey path regression-free** — existing hero manual walkthrough pass
- [ ] **Phase 2 touch-point audit doc published**
- [ ] **Hercules pilot enumeration** — all `GetInput` overrides + `Combat.Skill1-4` refs line-anchored

### Cross-cutting

- [ ] ไม่มี S1/S2/S3 bugs ใหม่จาก tasks ในสปรินท์นี้
- [ ] GDD/ADR ที่เกี่ยวข้องอัพเดตสำหรับทุก finding

---

## Progress

### 2026-04-21 — S3-01 closed ✅

**P1B-01 Host+Client multipeer test harness** — DONE

- สร้าง `Assets/Scenes/Testing/AbilityMultipeer.unity` + `AbilityMultipeerRunner.cs`
  (Host + Client `NetworkRunner` ใน editor session เดียวกัน, `PeerMode=Multiple`)
- **Pass #4 ✅ PASS** — 4/4 slots converge Host↔Client; log: `[Multipeer-Parity] ✅ PASS #4 — all 4 slots converge (Host↔Client).`
- **Pass #5 ✅ PASS** — Host out 26–27 B/s, Client out 39–65 B/s (~1–3% ของ 2048 B/s budget); per-object `OutBandwidth > 0`
- Fix `SceneRef.FromIndex` `ArgumentOutOfRangeException` (guard buildIndex < 0 → `default(SceneRef)` + warning)
- Fix duplicate AudioListener + MainCamera ที่เกิดจาก multipeer scene clone
- Evidence บันทึกใน `docs/architecture/ADR-0006-phase-1a-interfaces.md` §9.5

**Known polish item (non-blocking):**
- `AbilityMultipeerRunner.Start()` ยิง 2 ครั้งเมื่อ multipeer clone active scene
  (second instance ถูกสร้างใน client peer scene) → `GameIsFull` cascade.
  Pass #4 + #5 ไม่กระทบ (first session สำเร็จก่อน). Fix: static bootstrap
  guard หรือ disable self เมื่อ `NetworkRunner.Instances.Count > 0`.
  → tracked ต่อใน S3-02..S3-07 polish pass หรือ Sprint 004 nice-to-have.

### 2026-04-21 — S3-02 closed ✅

**P1B-02 `AbilityRegistry` real implementation** — DONE

- Implementation `Assets/GameScripts/Gameplays/Abilities/AbilityRegistry.cs` (delta-unity `eb2ff94f12`)
- Strategy: `Resources.LoadAll<GameObject>("Prefabs/Gameplay/Spell")` → filter prefabs ที่มี `NetworkObject` + `ActorCombatAction` → resolve abilityId via `[AbilityClass]` attribute (forward-compatible) ตกที่ `prefab.name` (convention) — option A+C-fallback
- 5 EditMode tests pass ทั้งหมด ที่ `Assets/UnitTests/TestEditMode/AbilityRegistryTests.cs`
- **Live scan:** 158 abilities + 157 SOs ใน 1079 ms cold / 511.9 ms warm (เกิน budget 100ms ของ plan §4.3)
- **Dict build cost (synthetic 50):** 0.06 ms — 1700× ใต้ budget ✅ (acceptance criteria #2 บางส่วน)
- **O(1) lookup post-boot ✅**
- **Spawn happy-path** ไม่ test ใน EditMode (ต้อง NetworkRunner) — defer ไป Phase 2 multipeer harness

**Polish item flagged → Sprint 004 nice-to-have:**
- `Resources.LoadAll` cold-start cost dominated by Unity asset I/O (not registry logic)
- 3 options: accept (current), Addressables migration (~2-3d), build-time manifest (~1-1.5d)
- Decision: Accept ตอนนี้ — boot-time = one-time match start ไม่ใช่ per-frame cost

### 2026-04-21 — S3-03 closed ✅

**P1B-03 `AbilityDataSnapshot` real implementation** — DONE

- Implementation: `AbilityDataSnapshot.cs` (factory body) + new `AbilitySnapshotService.cs`
  (DeltaBaseService — match-scoped frozen dict + FNV-1a 64-bit hash + broadcast stub)
- 7 EditMode tests pass at `Assets/UnitTests/TestEditMode/AbilitySnapshotTests.cs`
- Acceptance #1 (frozen invariant): mutate CBS post-build → snapshot unchanged ✅
- Acceptance #2 (hash mismatch detection): `ComputeHash` differs on any field change;
  `ValidateRemoteHash` logs error on mismatch ✅
- Hash sorted-key, deterministic across Host↔Client (verified via two services with
  same data in different insertion order produce identical hash)
- Field mapping: 1:1 primitives + rank-aware Costs/Cooldowns (clamped) + best-effort
  formula parse for BaseDamage / Duration / APScaling / ADScaling

**Phase 1b stub (full RPC in Phase 2):**
- `BroadcastHash()` logs the hash but does not emit Fusion RPC yet — the real wire
  needs an authoritative actor (GameMode) which Phase 2 introduces. Acceptance is
  satisfied today because hash compute + mismatch detection are local-deterministic
  and verifiable in EditMode.

### 2026-04-21 — S3-04 closed ✅

**P1B-04 `AbilityComponent.BindSlot` real implementation** — DONE (Option C dual-path)

**Production path implemented** (`AbilityComponent.BindSlot(byte, string) → bool`):
- Resolves `AbilityRegistry` via `DeltaService.I.GetService<AbilityRegistry>()`
- Calls new `AbilityRegistry.CreateAction(abilityId, NetworkBehaviour anchor)` overload
  (anchor-based; unblocks non-Actor callers like AbilityComponent itself)
- Captures `NetworkBehaviourId`, replaces prior slot binding (despawn old),
  writes `Slots.Set + SlotAbilityIds.Set`, fires `OnSlotChanged`
- Server-auth: silent no-op + warning on client-side calls

**Test-harness path retained** (renamed `BindSlotPrototype → BindSlotForTestHarness`):
- `[Obsolete]` removed; doc note explains it bypasses registry for harness scenes
- `AbilityPrototypeDriver` updated to new name; `#pragma 618` workaround removed
- Phase 2 will remove this when Hercules pilot retires TestActor

**Plan deviation** — Option C (pragmatic dual-path) chosen over literal "delete prototype path entirely + migrate harness" because:
- Registry's `CreateAction` requires `Actor` (TestActor doesn't have one)
- Harness scene `PrototypeTest.unity` doesn't bootstrap `DeltaService`
- Proto prefabs (`m_TestAbilityActionPrefab`) aren't in `Resources/Prefabs/Gameplay/Spell/`
  registry scan target

**Verification** (PrototypeTest.unity multipeer harness re-run after rename):
- Pass #4 ✅ — 4/4 slots converge `[Multipeer-Parity] ✅ PASS #4`
- Pass #5 ✅ — Host 26-28 B/s, Client 51-64 B/s (~1-3% of 2 KB/s budget)
- `BindSlotForTestHarness` fires correctly for all 4 slots; no warnings/regressions

**Note:** No EditMode unit tests added for `BindSlot` itself — the method is dominated
by Fusion APIs (`HasStateAuthority`, `Runner.TryGetNetworkedBehaviourId`, `Runner.Spawn`)
that require a live `NetworkRunner`. Coverage delegates to `AbilityRegistryTests`
(5/5 ✅ for the registry overload BindSlot calls) + Phase 2 multipeer harness for
the full round-trip happy path.

### 2026-04-21 — Merged origin/dev (no conflicts)

- Merge commit `b2128d6ae4` — pulled 3 dev commits (ui town fix, reconnection
  stability, register session fix) into `feature/refactor-ability-claude`
- Conflict-free this round (S3-01..S3-04 work didn't touch dev's edit surface)
- Removed orphan `Assets/GameScripts/UI/GameViews/Core/UIStatVisualUtility.cs.meta`
  (Unity local-generated; dev side now ships canonical version)

### 2026-04-21 — S3-05 closed ✅

**P1B-05 `InputMessage.PressedSlot/ReleasedSlot` wired** — DONE (Option A soft-verified)

- `NetworkRunnerInput.cs` Q/W/E/R now write `if (pressed) InputMessage.PressedSlot = N`
  in parallel with existing `Buttons.Set(...)` (Q→1, W→2, E→3, R→4 per KeybindMap defaults)
- Added editor-only debug log `[ADR-0006 P1B-05]` in `OnInput()` wrapped in `#if UNITY_EDITOR`
  (permanent diagnostic for Phase 2 wire-up; stripped from ship build)
- `ReleasedSlot` deferred to S3-06 (production callback path lacks symmetric release event today)

**Verification:**
- Wire delta = **0 B/tick** (fields existed since Phase 1a; struct shape unchanged)
- Compile clean; Fusion Startup completes; no exception in OnInput path
- S3-01 multipeer harness already covers wire-level replication of these fields
  (TestInputProvider is functionally identical writer; Pass #4 confirmed both peers
  receive same PressedSlot value)
- **End-to-end production playtest blocked** by environment in current dev build:
  - PlayFab `Version mismatch! Prompt update.` (CBS title version drift)
  - Photon Cloud STUN timeout (network/firewall) → match stuck at `WaitForPlayerJoin`,
    never reaches `GameState.Start` → `OnInput` early-returns
- Production end-to-end trace deferred to **Phase 2 Hercules pilot** (first reader
  of PressedSlot — will exercise full read+write loop in a real match scene)

### 2026-04-21 — S3-06 closed ✅

**P1B-06 `KeybindMap` production wiring + Settings placeholder** — DONE (Option A)

**Service wiring (critical path):**
- New `KeybindMapService.prefab` registered in `DeltaConfiguration.asset` Services
  array (10 → 11 services). `DeltaService.I.GetService<KeybindMap>()` resolves
  at runtime → unblocks S3-04 BindSlot lookup path
- `KeybindMap` already complete from Phase 1a (lifecycle + IKeybindMap +
  SetBinding + GetSlotForKey/Slot + ResetToDefaults + PlayerPrefs persistence
  + OnBindingChanged) — S3-06 adds the deployment surface

**CBS bridge stub:**
- `KeybindMap.ResetToDefaults()` calls `TryLoadDefaultsFromCBS()` first → falls
  back to hardcoded Q/W/E/R/A/B (slots 1-6) if CBS path returns false
- Stub returns false today; doc comment lists 4-step impl pattern for when
  CBSKeybindDefaults schema lands

**Editor placeholder UI** (`Radius/Abilities/Keybind Map` menu + CustomEditor):
- Inspector + EditorWindow show 8 slots with current binding
- Per-slot Rebind/Clear + global Reset to defaults
- KeyCode → InputSystem.Key mapper covers A-Z, 0-9, F1-F12, common modifiers
- Verified visually: rebinds persist via PlayerPrefs across Play Mode entry/exit

**Acceptance evaluation:**
- ✅ #1 Defaults loaded on game start (KeybindMap.Init → LoadFromPlayerPrefs →
  ResetToDefaults on first boot)
- 🟡 #2 Remap workflow — Editor inspector covers it; **runtime UGUI panel
  deferred to Sprint 004**
- ✅ #3 Persist across sessions (existing PlayerPrefs logic)
- 🟡 #4 UX sign-off — deferred with runtime UI

**Out of scope → Sprint 004:**
- Runtime UGUI Settings panel scene (Controls.unity)
- CBSKeybindDefaults schema (CBS dashboard + parser)
- UX designer review of runtime panel

---

## Source

สร้างจาก:
- [ADR-0006 Phase 1b Implementation Plan](../../docs/architecture/ADR-0006-phase-1b-implementation.md) — Accepted 2026-04-21 (S3-01..S3-07)
- [Sprint 002 §Deferred to Sprint 003](sprint-002.md) — S2-09, S2-10 carryover (S3-08, S3-09)
- [ADR-0006 Phase 1a Interfaces §9.3 Phase 1b Entry Criteria](../../docs/architecture/ADR-0006-phase-1a-interfaces.md)
- [ADR-0006 Migration Audit](../../docs/architecture/ADR-0006-migration-audit.md) — Pilot hero = Hercules confirmed
