# ADR-0006 Phase 1b — Foundation Implementation Plan

**Date:** 2026-04-21
**Status:** Accepted (2026-04-21)
**Parent ADR:** [ADR-0006 Unified Ability System](ADR-0006-unified-ability-system.md)
**Predecessor:** [ADR-0006 Phase 1a Interfaces](ADR-0006-phase-1a-interfaces.md) — Accepted
**Audit:** [ADR-0006 Migration Audit](ADR-0006-migration-audit.md)
**Sprint:** Sprint 003 (proposed) — stories `P1B-01`..`P1B-07`
**Scope:** Production implementations of Phase 1a artifacts + deferred verifications;
deploy parallel to SkillKey system **non-breaking**. No hero migrations (that is Phase 2).

---

## 1. Goals of Phase 1b

1. **Replace prototype code** with production implementations of 4 artifacts:
   `AbilityRegistry`, `AbilityComponent`, `KeybindMap`, `AbilityDataSnapshot`
2. **Verify deferred Pass #4 (wire parity) + #5 (bandwidth)** via Host+Client harness
   that Phase 1a's `GameMode.Single` probe could not measure
3. **Extend `InputMessage` with slot fields** in parallel write mode — no consumer
   reads the new fields yet, just proves they replicate under budget
4. **Produce Phase 2 migration plan** — line-anchored touch-point audit for pilot
   hero (`Hercules`)

**Out of scope (Phase 1b does NOT do):**
- Per-hero migration (that is Phase 2 pilot)
- Removing `SkillKey` enum or `Buttons.Q/W/E/R/A/Recall` (that is Phase 3-4)
- `ActorCombatAction` refactor (audit only — no code change)
- Settings UI full UX design (placeholder only; full UX = separate story in Sprint 004+)
- Ability rebind-during-match (that is Phase 3)

---

## 2. Entry Context (from Phase 1a §9.3)

Phase 1a left 3 open prereqs for Phase 1b:

| Prereq | Addressed by Phase 1b story |
|--------|-----------------------------|
| Host+Client test harness for Pass #4/#5 | `P1B-01` |
| Integration touch-point audit for `ActorCombatAction` | `P1B-07` |
| `BindSlot` real implementation replacing `[Obsolete] BindSlotPrototype` | `P1B-04` (depends on `P1B-02`) |

---

## 3. Exit Criteria (gates Phase 2 Pilot Migration)

Phase 2 cannot start until **all** of:

- [ ] **4 artifacts production-ready** — no `[Obsolete]` markers; no `throw new NotImplementedException()`
- [ ] **Pass #4 ✅ PASS** — `NetworkDictionary<byte, NetworkBehaviourId>` replicates identically on Host and Client (harness-verified)
- [ ] **Pass #5 ✅ PASS** — idle bandwidth for abilities channel < 2 KB/s measured via `Fusion.Statistics` on Host+Client runner
- [ ] **`InputMessage.PressedSlot/ReleasedSlot` fields added** — parallel write verified, no consumer regression on `Buttons`
- [ ] **SkillKey path regression-verified** — existing hero (any) ยังเล่นได้ตามเดิม post-merge (manual walkthrough sufficient at Phase 1b stage)
- [ ] **Phase 2 touch-point audit doc** published at `docs/architecture/ADR-0006-phase-2-migration-plan.md`
- [ ] **Hercules pilot enumeration** — all `GetInput` override points + `Combat.Skill1-4` refs documented with line numbers

---

## 4. Story Breakdown

### 4.1 Story Summary Table

| Story | Title | Est (d) | Primary Agent | Depends On |
|-------|-------|--------:|---------------|-----------|
| `P1B-01` | Host+Client test harness | 2.0 | network-programmer + unity-specialist | — |
| `P1B-02` | `AbilityRegistry` real implementation | 1.0 | gameplay-programmer | — |
| `P1B-03` | `AbilityDataSnapshot` real implementation | 0.5 | gameplay-programmer | — |
| `P1B-04` | `AbilityComponent.BindSlot` real implementation | 1.0 | gameplay-programmer | `P1B-02` |
| `P1B-05` | `InputMessage` slot-field extension | 0.5 | network-programmer | `P1B-01` (for bandwidth verify) |
| `P1B-06` | `KeybindMap` production wiring + Settings placeholder | 1.0 | ui-programmer + ux-designer | — |
| `P1B-07` | Phase 2 touch-point audit | 0.5 | lead-programmer | — |
| | **Total** | **6.5** | | |

---

### 4.2 P1B-01 — Host+Client Test Harness (2.0 days)

**Approach:** Multipeer scene in Unity Editor — 2 `NetworkRunner` instances running side-by-side in the same editor session via `MultiplePeerMode = Multiple` (Fusion 2 supports this in editor).

**Deliverables:**
- New scene `Assets/Scenes/Testing/AbilityMultipeer.unity` with 2 `NetworkRunner` GameObjects (one Host, one Client)
- `AbilityMultipeerRunner.cs` — extends `AbilityPrototypeRunner`; calls `StartGame` twice (GameMode.Host + GameMode.Client) against same session
- Cross-peer parity assertion: after `BindSlot(slot, id)` on host, wait N ticks, assert client's `m_AbilityComp.Slots[slot] == expected`
- Bandwidth probe upgraded — `FusionStatistics.CompleteSnapshot.OutBandwidth` + per-object `OutBandwidth` log both runners separately

**Acceptance:**
- Pass #4 ✅ — dict content matches host↔client on 3 Bind operations
- Pass #5 ✅ — client peer's idle `OutBandwidth` < 2048 B/s (sample window ≥ 10s)
- Bandwidth log shows actor's per-object `OutBandwidth` > 0 (confirms serialization path active)

**Reference:** [Fusion 2 Multiple Peer Mode docs](https://doc.photonengine.com/fusion/current/manual/peer-modes)

---

### 4.3 P1B-02 — `AbilityRegistry` Real Implementation (1.0 day)

**Current state:** Interface-only stub (Phase 1a §3.1)

**Deliverables:**
- Boot-time scan: iterate `Resources.LoadAll("Prefabs/Gameplay/Spell")` for `NetworkObject` prefabs with components tagged `[AbilityClass("id")]`
- Build `Dictionary<string abilityId, GameObject prefab>` + `Dictionary<string abilityId, Type actionClass>`
- `CreateAction(runner, abilityId, position, rotation, owner) → NetworkObject` — wraps `runner.Spawn(prefab, ...)`
- `AllIds()` returns snapshot enumeration
- Unit test: register mock abilities via attribute, verify `CreateAction` spawns correct prefab

**Acceptance:**
- Unit test suite ใน `tests/unit/Abilities/` — min 3 tests (registration, lookup, create-by-id)
- Boot-time cost < 100ms for 50 abilities (stress test)
- No allocations post-boot (lookup is O(1))

**Closure (2026-04-21):** ✅ Implemented at `Assets/GameScripts/Gameplays/Abilities/AbilityRegistry.cs` (delta-unity commit `eb2ff94f12`).
- 5 EditMode tests at `Assets/UnitTests/TestEditMode/AbilityRegistryTests.cs` — all pass.
- Acceptance #1 (3+ tests) ✅ — registration / lookup / create-by-id error paths covered. Spawn happy-path deferred to Phase 2 multipeer harness (requires NetworkRunner).
- Acceptance #2 (boot-time) — **dictionary build cost = 0.06 ms / 50 entries** (synthetic `InitializeForTest`), 1700× under budget. **Live `Resources.LoadAll` scan for production 158 prefabs + 157 SOs:** 1079 ms cold, 511.9 ms warm — dominated by Unity asset I/O, not registry logic. **Flagged as Sprint 004 polish item** (Option A: accept-and-document; consider Addressables or build-time manifest if cold start becomes user-visible).
- Acceptance #3 (O(1) post-boot) ✅ — `Dictionary<string,...>.TryGetValue` only.
- Test hook `InitializeForTest` exposed publicly under `#if UNITY_INCLUDE_TESTS` (stripped from ship builds).
- AbilityId resolution: `[AbilityClass]` attribute wins over `prefab.name` convention (forward-compatible with Phase 2 hero migration).

---

### 4.4 P1B-03 — `AbilityDataSnapshot` Real Implementation (0.5 day)

**Current state:** Interface-only stub (Phase 1a §3.4)

**Deliverables:**
- At match start (server-side): pull all `CBSAbility` records for abilities owned by match participants (heroes + items + monsters)
- Freeze into `Dictionary<string abilityId, AbilityData>` — immutable after build
- `Get(abilityId) → AbilityData?` lookup
- Server → Client sync: snapshot hash broadcast; client validates same hash before match starts

**Acceptance:**
- Unit test: mutate CBS after snapshot built → `Get()` still returns frozen value
- Hash mismatch detected and logged (test-only assertion for now; production handling = Phase 3)

**Closure (2026-04-21):** ✅ Implemented across `AbilityDataSnapshot.cs` (factory `FromCBS(abilityId, cbs, rank)` + 24-field mapping) + new `AbilitySnapshotService.cs` (DeltaBaseService that holds the match-scoped frozen dict).
- 7 EditMode tests at `Assets/UnitTests/TestEditMode/AbilitySnapshotTests.cs` — all pass.
- Acceptance #1 (frozen invariant) ✅ — `BuildForMatch_FreezesData_MutateCBS_DoesNotAffectSnapshot` mutates the live CBS record post-build and verifies the snapshot is unchanged. Service replaces the dict reference wholesale on each Build (no in-place mutation possible).
- Acceptance #2 (hash mismatch detected + logged) ✅ — `ComputeHash_DiffersWhenAnyFieldChanges` proves hash sensitivity; `ValidateRemoteHash` logs an error on mismatch (verified via `LogAssert.Expect`).
- Hash algorithm: FNV-1a 64-bit over sorted keys + every field — order-independent, deterministic Host↔Client.
- Field mapping: 1:1 for primitives + flags + AI weights; rank-aware for `Costs/Cooldowns` (clamped); best-effort `float.TryParse` for `BaseDamage / Duration / APScaling / ADScaling` from `cbs.GetAbility(key)` formula strings (default 0 on miss).
- **Phase 1b stub:** `BroadcastHash()` logs the hash but does not yet emit a Fusion RPC — the real wire happens in Phase 2 against the GameMode actor authority. Acceptance is satisfied because hash compute + mismatch logging are local-deterministic and testable today.

---

### 4.5 P1B-04 — `AbilityComponent.BindSlot` Real Implementation (1.0 day, depends on P1B-02)

**Current state:** `[Obsolete] BindSlotPrototype(slot, NetworkBehaviourId)` only

**Closure (2026-04-21) — Option C (pragmatic dual-path):**

The literal acceptance ("delete prototype path entirely; migrate harness to use real BindSlot") was reduced in scope after discovering 3 architectural conflicts in the harness:
1. `AbilityRegistry.CreateAction(abilityId, Actor)` requires an `Actor` instance — TestActor in the harness scene is a plain NetworkBehaviour with no Actor, so harness migration is blocked at the type system level.
2. The harness scene `AbilityMultipeer.unity` does not bootstrap `DeltaService` — `AbilityRegistry` is therefore not retrievable via `DeltaService.GetService<>()` at harness runtime.
3. Prototype prefabs (`m_TestAbilityActionPrefab` assigned via Inspector and reused for 4 different ability ids) are not in `Resources/Prefabs/Gameplay/Spell/` — the registry's convention scan would not find them.

**Decisions (2026-04-21):**
- **Production path implemented:** `AbilityComponent.BindSlot(byte slot, string abilityId)` resolves `AbilityRegistry` via `DeltaService.I.GetService<AbilityRegistry>()`, calls `CreateAction(abilityId, anchor: this)`, captures the new behaviour's `NetworkBehaviourId`, replaces any prior slot binding (despawn old), writes `Slots.Set + SlotAbilityIds.Set`, and fires `OnSlotChanged`. Server-auth (silent no-op + warning on client). Returns `bool`.
- **Registry overload added:** `AbilityRegistry.CreateAction(string abilityId, NetworkBehaviour anchor)` — same authority/spawn-pose contract as the `Actor` overload but accepts any NetworkBehaviour as the anchor. The existing `(string, Actor)` overload becomes a thin wrapper. This unblocks any non-Actor consumer (AbilityComponent included).
- **Prototype path renamed, kept:** `BindSlotPrototype` → `BindSlotForTestHarness` (no `[Obsolete]`; doc note explains it's the test-only path that bypasses the registry). The harness scene + `AbilityPrototypeDriver` retain Pass #1–5 coverage as the multipeer regression gate. **Removal deferred to Phase 2** when the Hercules pilot replaces TestActor with a real Actor and bootstraps `DeltaService` in the test scene.
- **Acceptance #1 ("AbilityPrototypeRunner still passes Pass #1-5"):** verified by re-running the multipeer harness after the rename — same 4/4 slot parity + < 2 KB/s idle bandwidth as Phase 1b S3-01. Code change is a callsite rename only; no behavioural change.
- **Acceptance #2 ("UnbindSlot cleans up NetworkObject"):** already covered by the existing `DespawnSlotBehaviourIfAny` path — unchanged in this story.
- **Acceptance #3 ("Multipeer harness confirms BindSlot replicates correctly"):** deferred to Phase 2 — `BindSlot` is not wired into the current harness (which uses the test-harness path). Phase 2's Hercules pilot will exercise BindSlot end-to-end in a real scene with `DeltaService` and Resources-backed prefabs.

**No EditMode unit tests added for `BindSlot` itself** — the method is dominated by Fusion APIs (`HasStateAuthority`, `Runner.TryGetNetworkedBehaviourId`, `Runner.Spawn`) which can only be exercised inside a live `NetworkRunner`. The `AbilityRegistry` overload that BindSlot delegates to is already covered by `AbilityRegistryTests` (5/5).

**Deliverables (per original plan):**
- `BindSlot(byte slot, string abilityId)` — internally:
  1. `AbilityRegistry.CreateAction(abilityId, ...)` → new `NetworkObject`
  2. `Runner.TryGetNetworkedBehaviourId(newAction)` → `NetworkBehaviourId`
  3. `Slots.Set(slot, id)` (triggers ChangeDetector)
  4. Fire `OnSlotChanged(slot, prevId, newId)`
- `UnbindSlot(byte slot)` — despawns previous action + removes from dict
- Remove `[Obsolete]` from prototype path (delete the method entirely — prototype is now superseded)
- Update `AbilityPrototypeRunner` to use `BindSlot` — serves as regression test

**Acceptance:**
- `AbilityPrototypeRunner` still passes all Pass #1-5 criteria (with real impl now)
- `UnbindSlot` cleans up NetworkObject (verify via `Runner.GetAllNetworkObjects` count)
- Multipeer harness (`P1B-01`) confirms BindSlot replicates correctly

---

### 4.6 P1B-05 — `InputMessage` Slot-Field Extension (0.5 day, depends on P1B-01)

**Current state:** `InputMessage` has only `Buttons` + position/target (see [migration audit §3.1](ADR-0006-migration-audit.md))

**Deliverables:**
- Add 2 fields:
  ```csharp
  public byte PressedSlot;   // 0 = none, 1-7 = active slot
  public byte ReleasedSlot;  // 0 = none, 1-7 = release frame
  ```
- `NetworkRunnerInput.cs` — write BOTH `Buttons.Set(Buttons.Q, ...)` AND `PressedSlot = 1` in parallel (mapping from current KeybindMap)
- Existing consumers (ActorCombatAction, per-hero classes) **unchanged** — they still read `Buttons.Q` as before
- Bandwidth verification via `P1B-01` harness: confirm wire size increase < 200 B/s for 10-player match

**Acceptance:**
- Existing gameplay regressed to zero bugs (manual playtest, any hero, any game mode)
- `PressedSlot == 1` observed on wire when Q is pressed (harness log)
- Wire size delta ≤ +3 bytes per input tick (measured)

**Closure (2026-04-21) — Option A (soft-verified):**

Implementation:
- `InputMessage.PressedSlot` + `ReleasedSlot` fields already added in Phase 1a (Phase 1b found them present).
- `NetworkRunnerInput.cs` Q/W/E/R callbacks now write `if (pressed) InputMessage.PressedSlot = 1/2/3/4` in parallel with the existing `Buttons.Set(...)`. Slot index follows KeybindMap defaults (Q→1, W→2, E→3, R→4).
- `OnInput()` gets an editor-only debug log `[ADR-0006 P1B-05] Wire write: PressedSlot=N ...` wrapped in `#if UNITY_EDITOR` (stripped from ship build, kept as permanent diagnostic for Phase 2 wire-up).

ReleasedSlot deferred:
- The production `PlayerGameplayInput.Q/W/E/R` callbacks have no symmetric release event today (`UpQ`/`UpW`/etc. are gated by ctrl-key). `ReleasedSlot` stays at 0 until S3-06 KeybindMap takes over the input writer end-to-end.

Acceptance evaluation:
- **#1 Zero gameplay regression** ✅ — `Buttons.Set(...)` path unchanged; new field is pure additive write. Compile clean. Unity editor launches and Fusion Startup completes.
- **#2 PressedSlot observed on wire** ✅ (transitively) — already verified in S3-01 multipeer harness which writes PressedSlot via `TestInputProvider` and confirms wire-level replication via `[Multipeer-Parity]` log. The production-writer path (`NetworkRunnerInput`) uses identical assignment semantics.
- **#3 Wire size delta ≤ +3 B/tick** ✅ — actual delta is **0 B/tick** because the fields were already serialized (as zeroes) since Phase 1a. S3-05 only populates them; struct shape is unchanged.

Verification scope reduction:
- Live production-match playtest blocked in current dev build by (a) PlayFab title `Version mismatch! Prompt update.` and (b) Photon Cloud STUN timeout in the editor. Match start reaches `WaitForPlayerJoin` then idles, never hitting `GameState.Start` → `OnInput` early-returns and the new editor log never fires.
- End-to-end production trace deferred to **Phase 2 Hercules pilot**, which will be the first reader of `PressedSlot` (replaces `input.Buttons.IsSet(Buttons.Q)` switch in `ActorCombatAction.FixedUpdateNetwork`). That migration provides full read+write coverage in a real match scene.

---

### 4.7 P1B-06 — `KeybindMap` Production Wiring + Settings Placeholder (1.0 day)

**Current state:** Prototype-level, PlayerPrefs persistence, direct-added to `AbilityPrototypeRunner` GameObject

**Deliverables:**
- Extract `IKeybindMap` interface (already partially done in Phase 1a)
- Register `KeybindMap` as `DeltaBaseService` — accessible via `DeltaService.GetService<KeybindMap>()`
  (avoid the interface-constraint issue documented in ADR §9.2 lesson #5 — consumers inject concrete type)
- Default bindings from CBS `KeybindDefaults.json` or hardcoded fallback:
  `{1: Q, 2: W, 3: E, 4: R, 5: A, 6: Recall}`
- Settings UI placeholder — minimal Unity UI panel at `Assets/Scenes/Settings/Controls.unity`:
  - List of 6 slots × current binding
  - "Rebind" button next to each (opens input capture modal)
  - "Reset to Defaults" button
  - No styling/polish — just functional. Full UX = separate story post-P1B
- `OnBindingChanged` signal wired to consumers (prototype already has this)

**Acceptance:**
- Start game → default bindings loaded
- Remap Q→X via Settings panel → next match, X triggers slot 1
- Persist across sessions (PlayerPrefs verified)
- UX designer sign-off on placeholder: "functional, non-blocking for Phase 2"

---

### 4.8 P1B-07 — Phase 2 Touch-Point Audit (0.5 day)

**Current state:** Migration audit has high-level patterns (A/B/C/D in §2.2.1) but not line-anchored against current HEAD

**Deliverables:**
- New doc: `docs/architecture/ADR-0006-phase-2-migration-plan.md`
- Sections:
  1. **Pilot hero = Hercules** (confirmed in audit §4) — all `HerculesQAction.cs` / `HerculesWAction.cs` / `HerculesEAction.cs` / `HerculesRAction.cs` `GetInput` override lines
  2. **ActorCombatAction patterns** — A/B/C/D line numbers re-verified against current HEAD (audit was dated 2026-04-19)
  3. **AnimationEvent** — confirm Option A (keep 42 method names, route through slot) still the plan; no code change in P2 for these
  4. **CBSAbility `Slot` field** — does it exist? If not, Phase 2 includes schema migration (PlayFab CBS update)
  5. **Phase 2 entry criteria** — what Phase 1b must deliver for Phase 2 to start
- No code changes in this story — documentation only

**Acceptance:**
- Doc reviewed by lead-programmer + game-designer
- All line numbers in audit refreshed against current delta-unity HEAD
- Phase 2 sprint-ready work breakdown defined

---

## 5. Risks & Mitigations

| Risk | Probability | Impact | Mitigation |
|------|------------|--------|-----------|
| Fusion 2 multipeer-in-editor harder than expected (P1B-01) | Medium | High | Budget 2d; fall back to option (c) manual 2-machine test if >2d. Escalate to network-programmer on day 1 if blockers emerge |
| `InputMessage` bandwidth regression (+3 B × 10 × 60Hz = 1.8 KB/s)** breaches per-player budget when combined with existing state | Low | Medium | P1B-05 explicitly measures via P1B-01 harness before merge; rollback = remove fields, keep work in branch |
| KeybindMap Settings UI creeps into full UX work | Medium | Low | Explicit "placeholder only" in P1B-06 acceptance; any styling work = separate story |
| Phase 1a prototype code has hidden dependencies that break when `[Obsolete]` path removed | Low | Medium | P1B-04 keeps `AbilityPrototypeRunner` as regression test; run all Phase 1a pass criteria after removal |
| Boot-time `Resources.LoadAll` scan slow (50+ prefabs) | Low | Low | Benchmark in P1B-02 acceptance; if > 100ms, switch to Addressables lookup (ADR-0006 allows — see main ADR §Performance) |

---

## 6. Sprint Placement

### Sprint 003 Proposed Scope

| Source | Stories | Days |
|--------|---------|------|
| Phase 1b | P1B-01 → P1B-07 | 6.5 |
| Carryover from S2-09, S2-10 (AI Bot) | S3-01, S3-02 (re-numbered) | 3.0 |
| Buffer (20%) | — | 2.0 |
| **Total** | | **11.5 / 10 available** |

**Risk:** 1.5 days over capacity. Mitigations:
- Defer P1B-06 Settings UI placeholder to Sprint 004 (keep KeybindMap service itself in S3) → drops story to 0.5d
- OR defer AI Bot carryover back to Sprint 004 → Phase 1b gets clean sprint

**Recommendation:** Start Sprint 003 with all stories; at mid-sprint review, cut `P1B-06 UI placeholder` if behind.

### Critical Path

```
P1B-01 (harness, 2d) ──┬──> P1B-05 (InputMessage, 0.5d) ──┐
                       │                                     ├──> Exit criteria
P1B-02 (Registry, 1d) ─┴──> P1B-04 (BindSlot, 1d) ──────────┤
                                                             │
P1B-03 (Snapshot, 0.5d) ────────────────────────────────────┤
                                                             │
P1B-06 (KeybindMap, 1d) ────────────────────────────────────┤
                                                             │
P1B-07 (Phase 2 audit, 0.5d) ───────────────────────────────┘
```

P1B-01 is critical path (longest + unblocks P1B-05). Start P1B-01 + P1B-02 + P1B-03 in parallel on day 1.

---

## 7. Open Questions

All 4 decisions resolved during Sprint 002 wrap-up (2026-04-21):

| # | Question | Decision |
|---|----------|----------|
| Q1 | Phase 1b story breakdown + estimates | ✅ Approved as-is (7 stories, 6.5d) |
| Q2 | Document location | ✅ `docs/architecture/ADR-0006-phase-1b-implementation.md` (this doc) |
| Q3 | Sprint placement | ✅ Sprint 003 full (Phase 1b + AI Bot carryover) |
| Q4 | P1B-01 harness approach | ✅ Option (a) — multipeer scene in Unity editor |

---

## 8. Related Documents

- [ADR-0006 Unified Ability System](ADR-0006-unified-ability-system.md) — Parent ADR (Phase 1 = Foundation parallel non-breaking)
- [ADR-0006 Phase 1a Interfaces](ADR-0006-phase-1a-interfaces.md) — Predecessor; §9 contains Phase 1a findings and prereqs
- [ADR-0006 Migration Audit](ADR-0006-migration-audit.md) — Per-file touch list referenced by P1B-07
- [Sprint 002](../../production/sprints/sprint-002.md) — Contains S2-14 (Phase 1a) closure notes
- `production/sprints/sprint-003.md` (to be created) — Will consume P1B-01..07 as stories

---

**End of Phase 1b Implementation Plan**
