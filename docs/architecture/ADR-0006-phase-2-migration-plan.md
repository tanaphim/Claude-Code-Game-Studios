# ADR-0006 Phase 2 — Pilot Hero Migration Plan (Hercules)

**Date:** 2026-04-21
**Status:** Accepted
**Predecessor:** [ADR-0006 Phase 1b Implementation Plan](ADR-0006-phase-1b-implementation.md) — Closed (S3-01..S3-07)
**Source audit:** [ADR-0006 Migration Audit](ADR-0006-migration-audit.md) — 2026-04-19 (line numbers refreshed against HEAD `1022c87dbb` 2026-04-21)
**Sprint:** Sprint 004 (proposed) — first migration story to ship under unified ability runtime

---

## 1. Goals of Phase 2

1. **Migrate one hero (Hercules) end-to-end** off `SkillKey` literals + `Combat.SkillN` properties → `AbilityComponent.Slots` + `PressedSlot` reads.
2. **Prove the Phase 1b foundation in a real match scene** — not just the multipeer harness.
3. **Establish the per-hero refactor recipe** — first migration is exploratory; remaining heroes (~12 today, 25+ on roadmap) follow the codified pattern in Phase 3.
4. **Land the missing infrastructure** that the audit identified but Phase 1b deferred:
   - `CBSAbility.Slot` (or equivalent slot-source) field
   - `AnimationEvent` Option A wiring (active-slot resolver, no clip changes)
   - `ActorCombatAction` Pattern-A/B helper extraction (refactor under unified path)

**Out of scope:**
- All other heroes (`AnansiQAction`..`MerlinXAction` etc.) — these wait for Phase 3.
- Removing `SkillKey` enum or `Buttons.Q/W/E/R/A/Recall` — these stay parallel through Phase 3 (deletion is Phase 4).
- Rebind-during-match (`RebindRequestSlot`) — Phase 3.
- Stat/StatusEffect → NetworkStruct (epic backlog item #5; not in 0006 scope).

---

## 2. Entry Context (Phase 1b → Phase 2 handover)

| Phase 1b artifact | Status at handover | Phase 2 consumer |
|-------------------|--------------------|------------------|
| `AbilityRegistry` (DeltaService-registered) | ✅ S3-02 | `AbilityComponent.BindSlot` lookup target |
| `AbilityRegistry.CreateAction(string, NetworkBehaviour anchor)` overload | ✅ S3-04 | spawns Hercules action prefabs at slot bind time |
| `AbilityComponent.BindSlot(byte, string)` production path | ✅ S3-04 | Hercules avatar's `ActorCombat.OnStartup` calls this 4× (Q/W/E/R slots 1–4) |
| `AbilityComponent.BindSlotForTestHarness` | retained, renamed | unchanged — still serves multipeer harness; removed when TestActor retires |
| `AbilityDataSnapshot` + `AbilitySnapshotService` | ✅ S3-03 | Match-start broadcast; Hercules reads frozen values via `Get(abilityId)` |
| `InputMessage.PressedSlot/ReleasedSlot` | ✅ S3-05 (PressedSlot only) | Hercules `GetInput` overrides switch from `Buttons.IsSet(Buttons.X)` to `input.PressedSlot == AbilityData.Slot` |
| `KeybindMap` service | ✅ S3-06 | Slot index resolution for input writer (already wired in `NetworkRunnerInput`) |
| `KeybindMap` runtime UI | ⏸ deferred Sprint 004 polish | Phase 2 uses Editor inspector for QA rebind; gameplay accepts default Q→1/W→2/E→3/R→4 |
| `ReleasedSlot` writer | ⏸ deferred | Hercules R uses `released.IsSet(Buttons.R)` (line 241 commented; falls through to `IsSet(Buttons.R)` line 246) — keep `Buttons` read on release path until S3-06+ adds the production release event |
| Multipeer harness Pass #4/#5 | ✅ — verified S3-01 + retained S3-04 (re-run) | regression gate; re-run after Hercules migration |

---

## 3. Phase 2 Exit Criteria

Phase 2 closes when **all** of:

- [ ] **Hercules plays end-to-end** in a real match scene using the unified path: spawn → `AbilityComponent.BindSlot` × 4 → press Q/W/E/R → `input.PressedSlot` triggers correct ability → cast/charge/cancel flows work identically to pre-migration.
- [ ] **No `SkillKey.{Q,W,E,R,Recall}` literal in any Hercules file** under `Assets/GameScripts/Gameplays/Characters/Hercules/`. (`SkillKey.Recall` may stay until Recall is migrated.)
- [ ] **No `Combat.Skill1/2/3/4/SkillRecall` direct property read** in Hercules action files. Accesses go through `Combat.GetSlotAction(byte)` or `AbilityComponent.GetSlotAction(byte)`.
- [ ] **`ActorCombatAction` Pattern-A/B helpers extracted** — at least these two get a single replacement implementation each (see §5.2). Other heroes still call into the legacy ifs (compatibility shims) until Phase 3.
- [ ] **`CBSAbility.Slot` (or surrogate) source-of-truth** wired so Hercules' Q/W/E/R abilities know their default slot without `SkillKey` literal.
- [ ] **AnimationEvent Option A confirmed** — `PerformQ`/`ReleaseQ`/etc. shim methods route through `Actor.Combat.GetActiveSlot()` instead of literal `SkillKey.Q`. No animator clip rebinds.
- [ ] **Multipeer harness regression** — Pass #1–#5 from S3-01 still green after Hercules merge.
- [ ] **Manual playtest** — 1 match (Training mode, 1 player, Hercules) spawning all 4 skills successfully + recall + normal attack.

---

## 4. Pilot enumeration — Hercules touch points (HEAD `1022c87dbb`)

### 4.1 Hercules action files

| File | Line | Current pattern | Phase 2 replacement |
|------|------|-----------------|---------------------|
| `HerculesQAction.cs` | 68 | `AbilityData.SkillKey == SkillKey.Recall` (used in CastEvent ctor) | leave — `SkillKey.Recall` still valid at this seam (see Out of scope) |
| `HerculesQAction.cs` | 128 | same | same |
| `HerculesRAction.cs` | 67 | same | same |
| `HerculesRAction.cs` | **231–254** | `public override void GetInput(InputMessage input)` body — checks `input.Buttons.IsSet(Buttons.R)` (line 246) and `released.IsSet(Buttons.R)` commented at line 241 | **REWRITE:** `if (input.PressedSlot == AbilityData.Slot)` for press; `released` path picks up `ReleasedSlot` once S3-06+ wires the production release event |
| `HerculesWAction.cs` | 25, 33, 35, 77, 92 | `Actor.Combat.Skill3.{CanUseSkillWhileanotheruse \| IsPerform \| Progress}` — direct property reads of slot-3 (E ability) sibling | **REWRITE:** `Actor.Combat.GetSlotAction(3)?.{...}` — go through facade so the read is slot-indexed, not enum-property-named |
| `HerculesWAction.cs` | 28 | `SkillKey.Recall` ctor arg | leave |
| `HerculesEAction.cs` | — | (no SkillKey/Buttons/SkillN refs in current file) | no Phase 2 changes |
| `HerculesPassiveAction.cs` | — | (no refs) | no Phase 2 changes |
| `HerculesNAction.cs` | — | (no refs) | no Phase 2 changes — Normal-attack stays on `Buttons.A` until Phase 3 |

**Total Hercules edits (Phase 2 scope):** **2 files** — `HerculesRAction.cs` (1 method body) + `HerculesWAction.cs` (5 sibling reads).

### 4.2 ActorCombat (sibling property facade)

`Actor.Combat.Skill1..Skill4 / NormalAttack / SkillRecall / Passive` properties stay as legacy facades through Phase 3, but **need a slot-indexed accessor added** so Phase 2 callers can stop reading by enum-property name:

```csharp
// ActorCombat.cs — new in Phase 2 (additive):
public ActorCombatAction GetSlotAction(byte slot) =>
    AbilityComponent.GetSlotAction(slot);  // delegates to S3-04 read API
```

This is a **3-line addition**, not a refactor. Existing `Skill1..4` properties remain (used by ~50 non-Hercules call sites; Phase 3 drains them).

---

## 5. ActorCombatAction patterns — line refresh

The audit (2026-04-19) listed Pattern A/B/C/D against an older HEAD. Here are the **current line numbers** at HEAD `1022c87dbb`:

### 5.1 Pattern A — Owner guard (5 occurrences, each block is 7 lines)

| Block | Line range (old audit said) | Line range (HEAD) | Method context |
|-------|------------------------------|-------------------|----------------|
| #1 | 866-872 | **867–873** | (call site near `OnSkillOpened` cluster) |
| #2 | 884-890 | **885–891** | |
| #3 | 902-908 | **903–909** | (ranges 909+ continue into the same cluster) |
| #4 | 974-980 | **981–987** | |
| #5 | 996-1002 | **1009–1015** | |

Each block is structurally identical (audit §2.2.1 quoted full block once). Phase 2 extracts this into:

```csharp
private bool IsActiveSlotOwner(byte slot)
{
    var slotAction = Actor.Combat.GetSlotAction(slot);
    return slotAction != null && slotAction.Id == Id;
}
// Caller (replaces all 5 blocks):
if (!IsActiveSlotOwner(AbilityData.Slot)) return;
```

**Net delta:** −35 lines × 5 sites = **−170 lines**, +1 helper (≈10 lines). Estimated **~−155 lines** for Pattern A alone.

### 5.2 Pattern B — Input-to-slot binding (4 occurrences in HEAD, each 6 lines)

| Site | Line | Context |
|------|------|---------|
| B1 | **1749–1754** | press path (cooldown + state guard cluster) |
| B2 | **1764–1769** | release path |
| B3 | **1781–1786** | press path (charge variant) |
| B4 | **1909** | (single-line condition) — `input.Buttons.IsSet(Buttons.{Q,W,E,R,Recall})` matched against `AbilityData.SkillKey` |

Phase 2 replacement (one-liner for each):

```csharp
// Press:    if (input.PressedSlot != 0 && input.PressedSlot == AbilityData.Slot) { ... }
// Release:  if (input.ReleasedSlot != 0 && input.ReleasedSlot == AbilityData.Slot) { ... }
```

Note: line 1909's `input.Buttons.IsSet(Buttons.Q)` etc. is a **single condition line**, not a multi-line block — easier rewrite.

### 5.3 Pattern C — Rank-up exclusions

Audit referenced lines 2115/2125/2133 — these are **unchanged** at HEAD (file grew but pattern lives in stable section). Phase 2 strategy unchanged: rely on `AbilityDataSnapshot.IsLeveled` (already populated by S3-03 from `cbs.MaxRank > 1`). Refactor:

```csharp
// Old: if (SkillKey.A || SkillKey.Item || SkillKey.Recall) return;
// New: if (!IsLeveled) return;
```

### 5.4 Pattern D — Quick-cast settings

| Site | Line | Pattern |
|------|------|---------|
| D1 | **1790–1795** | `Actor.Combat.IsQuickQ && SkillKey.Q \|\| IsQuickW && SkillKey.W \|\| IsQuickE && SkillKey.E \|\| IsQuickR && SkillKey.R` |

Phase 2 replacement uses a slot-indexed quick-cast lookup on `ActorCombat`:

```csharp
public bool IsQuickCast(byte slot) =>
    slot switch { 1 => IsQuickQ, 2 => IsQuickW, 3 => IsQuickE, 4 => IsQuickR, _ => false };
// Caller: if (Actor.Combat.IsQuickCast(AbilityData.Slot)) { ... }
```

(Phase 3 cleanup: replace `IsQuickQ/W/E/R` fields with `bool[] m_QuickCastBySlot`. Phase 2 only adds the read-side facade.)

---

## 6. Schema & infrastructure additions

### 6.1 `CBSAbility.Slot` field

**Audit said** (§5.2 blocker): `CBSAbility.SkillKey` (enum) is the live source-of-truth on PlayFab CBS dashboard. **HEAD confirms** `CBSAbility.cs` has `public SkillKey SkillKey;` (line 8) but **no `Slot` field**.

**Phase 2 strategy — additive, non-breaking:**

```csharp
// CBSAbility.cs — append:
public byte Slot;   // 0 = unassigned (legacy), 1-7 = slot index
```

- New field defaults to 0 on existing CBS records → consumers must derive from `SkillKey` when `Slot == 0` (compatibility shim during migration).
- Compatibility shim: `AbilityData.EffectiveSlot => Slot != 0 ? Slot : SkillKeyToSlot(SkillKey)` where the mapping is `Q→1, W→2, E→3, R→4, A→5, I→6, Recall→7`.
- Phase 2 only **reads** `EffectiveSlot`; populating `Slot` on each CBS record is a designer task (one-time migration via CBS dashboard) tracked separately.

### 6.2 AnimationEvent.cs — Option A confirmation

**Audit §2.2.2** picked Option A: keep all 42 method names as shims, swap internal implementation to use active-slot resolver.

**Phase 2 status:** **CONFIRMED**. Concrete change:

```csharp
// Before:
public void PerformQ(int param) => StateRelease(SkillKey.Q, SkillState.Perform, param);
// After (Phase 2):
public void PerformQ(int param) => StateReleaseSlot(Actor.Combat.GetActiveSlot(), SkillState.Perform, param);
```

`Actor.Combat.GetActiveSlot()` is new — returns the slot index of the most-recently-cast ability (already tracked internally by current `m_Skill1..4` lists). Implementation: ~15 lines on `ActorCombat`.

**Animator clip touches:** **zero**. All 25+ hero clips keep their existing event method-name bindings. This was the deciding factor for Option A.

### 6.3 Resource dictionaries (deferred to Phase 3)

Audit §2.6 lists `SkillObjectDictionary`, `SkillVfxDictionary`, `SkinObject` — all keyed by `SkillKey`. **Phase 2 does NOT migrate these.** Hercules' SkinObject lookups stay on the legacy `SkillKey`-keyed path; this works because Hercules still **has** a `SkillKey` value on `CBSAbility` (we don't remove it). Phase 3 introduces an editor migration tool to flip ~25 SkinObject assets in one batch.

---

## 7. Phase 2 sprint-ready work breakdown

| Story | Title | Est | Depends on |
|-------|-------|----:|------------|
| `P2-01` | `ActorCombat.GetSlotAction` + `AbilityComponent.GetSlotAction` facade additions | 0.25 | S3-04 ✅ |
| `P2-02` | `ActorCombat.GetActiveSlot` + `IsQuickCast(byte slot)` accessor | 0.25 | S3-04 ✅ |
| `P2-03` | `CBSAbility.Slot` field + `AbilityData.EffectiveSlot` shim + `SkillKeyToSlot` mapper | 0.25 | — |
| `P2-04` | `ActorCombatAction` Pattern-A helper (`IsActiveSlotOwner`) — replace 5 blocks | 0.5 | P2-01, P2-03 |
| `P2-05` | `ActorCombatAction` Pattern-B/C/D one-liner replacements (4 sites) | 0.5 | P2-01..P2-03 |
| `P2-06` | `AnimationEvent` Option A — wire 42 shim methods through `GetActiveSlot()` | 0.5 | P2-02 |
| `P2-07` | `HerculesRAction.GetInput` rewrite (PressedSlot path) | 0.25 | P2-03 |
| `P2-08` | `HerculesWAction` slot-indexed sibling reads (5 sites) | 0.25 | P2-01 |
| `P2-09` | `Hercules` avatar bootstrap — `ActorCombat.OnStartup` calls `AbilityComponent.BindSlot(slot, abilityId)` × 4 | 0.5 | P2-03 |
| `P2-10` | Manual playtest checklist + 1-match Training playthrough verification | 0.5 | all above |
| | **Total** | **3.75** | |

**Critical path:** P2-03 (CBS schema) → P2-04 (Pattern-A helper) → P2-09 (Hercules bootstrap) → P2-10 (verify). ~2.0 days serial.

**Parallelizable:** P2-01/P2-02 (independent facades), P2-06 (AnimationEvent shim), P2-07/P2-08 (Hercules-only files) can run alongside the critical path.

---

## 8. Risks & mitigations

| Risk | Probability | Impact | Mitigation |
|------|------------|--------|-----------|
| `CBSAbility.Slot` requires PlayFab CBS dashboard update — designer dependency | Medium | High | EffectiveSlot shim derives from `SkillKey` when `Slot==0` → unblocks code path; designer fills `Slot` on-deck without code waiting |
| `AnimationEvent.GetActiveSlot()` returns wrong slot during multi-skill chains | Low | High | Add unit tests on `ActorCombat.GetActiveSlot` that simulate Q→W→E presses; assert returned slot matches latest `OnPressButtons` call |
| Hercules R charge path uses `Buttons.R` on release (S3-05 ReleasedSlot deferred) | Medium | Medium | Phase 2 keeps `released.IsSet(Buttons.R)` for release until KeybindMap S3-06+ wires production release; document as known limitation; full removal in Phase 3 |
| Pattern-A helper extraction misses an edge-case branch (e.g. SkillKey.Item → no slot) | Low | Medium | Run multipeer harness Pass #1-5 + manual Hercules playthrough as regression gate |
| 50+ non-Hercules call sites of `Combat.SkillN` keep working through facade | High (by design) | Low | Phase 2 doesn't refactor them; Phase 3 drains. Facade pattern documented in P2-01 |
| Bandwidth regression from `EffectiveSlot` derivation (per-tick computation) | Very Low | Negligible | `Slot` is read-only after match start (`AbilityDataSnapshot` frozen); cache on snapshot build, no per-tick math |

---

## 9. What this plan does NOT change (intentionally)

To keep Phase 2 scope tight and reversible, **all of the following stay exactly as today**:

- `SkillKey` enum definition
- `Buttons.Q/W/E/R/A/Recall` enum entries
- `InputMessage.Buttons` field (still serialized, still read by ~50 sites)
- All non-Hercules hero action files
- `SkinObject` / `SkillObjectDictionary` / `SkillVfxDictionary` — still SkillKey-keyed
- Animator state machine names (`Q_Perform`, `R_Empower`, etc.)
- `UISkill.cs` line 176 `Enum.TryParse(SkillKey, out Buttons)` bridge
- Any Editor migration tool for SkinObject assets

If Hercules works after Phase 2, Phase 3 codifies the recipe and drains the rest. If Hercules **breaks**, rollback is `git revert` of ~10 small commits — no data migration to undo.

---

## 10. Phase 2 → Phase 3 handover criteria

Phase 3 starts when Phase 2 has shipped **and**:

1. Hercules has been live in `dev` branch for ≥1 week with no slot-related bugs raised by QA.
2. `ActorCombat.GetSlotAction` facade has at least 1 non-Hercules call site (proof the API generalizes).
3. `AbilityDataSnapshot.EffectiveSlot` has been verified to return correct values for all CBS records in the current title (audit script in `tests/integration/abilities/`).
4. The Pattern-A helper (`IsActiveSlotOwner`) has been peer-reviewed and is the documented preferred pattern in `control-manifest.md`.

---

## 11. References

- [ADR-0006 Unified Ability System](ADR-0006-unified-ability-system.md) — parent ADR
- [ADR-0006 Migration Audit](ADR-0006-migration-audit.md) — original line-level survey (2026-04-19)
- [ADR-0006 Phase 1a Interfaces](ADR-0006-phase-1a-interfaces.md) — interface contracts + Pass #1-5
- [ADR-0006 Phase 1b Implementation Plan](ADR-0006-phase-1b-implementation.md) — closed; this Phase 2 plan picks up where 1b left off
- delta-unity HEAD at audit time: `1022c87dbb` (S3-06 close)
