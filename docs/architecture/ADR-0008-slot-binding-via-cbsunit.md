# ADR-0008: Slot Binding Source-of-Truth — CBSUnit (supersedes ADR-0006 §6.1)

## Status

Accepted

## Date

2026-05-08

## Last Verified

2026-05-08

## Decision Makers

- User (creative-director / game-designer)
- gameplay-programmer (S5-01 implementation)
- ADR-0006 Phase 2 plan author

## Summary

Ability slot binding is sourced from `CBSUnit` (per-hero slot composition), not
`CBSAbility` (per-ability slot self-declaration). C# accesses the legacy CBS
field names (`Skill0/Skill1..4/SkillI/SkillA`) through new read-only property
aliases (`SlotQ/W/E/R/A/I`) — zero CBS dashboard migration required.

## Engine Compatibility

| Field | Value |
|-------|-------|
| **Engine** | Unity 2022.3.62f1 |
| **Domain** | Core / Scripting |
| **Knowledge Risk** | LOW — pure C# data access pattern |
| **References Consulted** | `Assets/CBS/Scripts/Core/SharedData/CBSBaseCustomData.cs` (reflection serializer) |
| **Post-Cutoff APIs Used** | None |
| **Verification Required** | EditMode unit tests on alias properties; Hercules bootstrap E2E (S5-09 → S5-10) |

## ADR Dependencies

| Field | Value |
|-------|-------|
| **Depends On** | ADR-0006 (Unified Ability System), ADR-0006 Phase 2 §1–§5 (still applicable) |
| **Supersedes** | ADR-0006 Phase 2 §6.1 (`CBSAbility.Slot` field decision) |
| **Enables** | Phase 3 cross-slot ability swap; runtime variant rebind (Merlin-style elemental swap) |
| **Blocks** | Sprint 005 stories S5-01 (revised), S5-09 (revised) |
| **Ordering Note** | Must be Accepted before /dev-story S5-01 resumes |

## Context

### Problem Statement

ADR-0006 Phase 2 §6.1 proposed adding `CBSAbility.Slot` as the source-of-truth
for which slot an ability defaults to. Implementation began in Sprint 005 S5-01
(committed as a4d… reverted 2026-05-08).

User design review surfaced that **slot binding is a property of "what kit
this hero has", not "what slot does this ability prefer"**. Multiple heroes
may reuse the same ability in different slots (rune systems, talent trees,
ability shops); a per-ability slot declaration imposes a 1:1 ability↔slot
constraint that the design does not want.

### Current State

- `CBSUnit` already exposes per-slot ability composition:
  - `Skill0` (legacy passive list — duplicate of `SkillI`)
  - `Skill1, Skill2, Skill3, Skill4` (Q/W/E/R variant pools — `List<string>`)
  - `SkillI` (single passive id, legacy)
  - `SkillA` (single normal-attack id)
- Each `Skill1..4` list represents a **variant pool** (e.g. Merlin
  `Skill1 = [merlin_q_fire, merlin_q_ice, merlin_q_dark]` — element 0 is the
  starting variant, others swap in via Phase 3 RebindSlot mechanism).
- Existing reads (`ActorCombat.cs:202–218`, `UIAvatarDetail.cs:254`,
  `UIStatObjectView.cs:407`) use field names directly — only 3 sites.
- `CBSBaseCustomData.ToDictionary()` uses reflection on **public fields only**
  → renaming fields breaks PlayFab serialization for every existing hero record.

### Constraints

- **CBS dashboard is hard to use** — manual rename across ~25 hero records
  is high-risk + high-effort (programmer task today, designer-led eventually).
- **Existing CBS records cannot be invalidated** — rename `Skill1` → `SlotQ`
  in C# breaks deserialization of every hero JSON in PlayFab.
- **CBS reflection serializer ignores properties** — only `GetFields()` is
  iterated, so read-only property aliases are safe to add without affecting
  upload/download payloads.
- **Sprint 005 critical path tight** — must not exceed 3.75d Must Have budget.

### Requirements

- Designer can change per-hero kit by editing CBSUnit on PlayFab dashboard
  (no code change, no redeploy).
- Code accesses slot bindings through a single uniform API regardless of
  underlying field naming history.
- Variant pool semantics preserved — slot can hold N abilities, runtime
  picks one as "active" (Phase 3 RebindSlot).
- Slot is type-agnostic — passive can be placed in slot Q; active can be
  placed in slot I. "Type" is a property of the ability, not the slot.
- Recall is universal — does not need per-hero CBSUnit field; bound globally
  at slot 7 by ActorCombat.

## Decision

**Slot binding source-of-truth = `CBSUnit`** with **additive read-only property
aliases** for canonical access. No CBS field renames.

### Architecture

```
PlayFab CBS Dashboard
  │
  │  (designer authors per-hero kit composition)
  ▼
CBSUnit (JSON)
  ├── Skill0  : List<string>   ← legacy passive pool (duplicate of SkillI)
  ├── Skill1  : List<string>   ← Q variant pool
  ├── Skill2  : List<string>   ← W variant pool
  ├── Skill3  : List<string>   ← E variant pool
  ├── Skill4  : List<string>   ← R variant pool
  ├── SkillI  : string         ← passive (single, legacy)
  └── SkillA  : string         ← normal attack (single)
  │
  │  (CBSBaseCustomData.ToDictionary reflects fields only — properties bypass)
  ▼
CBSUnit C# class
  ├── [legacy fields kept verbatim — DO NOT RENAME]
  └── [new read-only property aliases — canonical access pattern]
        SlotQ → Skill1
        SlotW → Skill2
        SlotE → Skill3
        SlotR → Skill4
        SlotA → wrap(SkillA) as List<string>
        SlotI → Skill0 ?? wrap(SkillI) as List<string>
  │
  │  (S5-09 Hercules bootstrap)
  ▼
ActorCombat.OnStartup
  └── for each slot in 1..6:
        AbilityComponent.BindSlot(slot, unit.Slot{Q,W,E,R,A,I}[0])
  └── slot 7 (Recall): bound by ActorCombat globally — universal ability id
```

### Key Interfaces

```csharp
// CBSUnit.cs — additive aliases (no field rename)
public class CBSUnit : CBSItemCustomData
{
    // ===== Legacy fields (DO NOT RENAME — CBS reflection serializer keys on field names) =====
    public List<string> Skill0;
    public List<string> Skill1, Skill2, Skill3, Skill4;
    public string SkillI;
    public string SkillA;
    // ... rest unchanged ...

    // ===== Slot aliases (ADR-0008 — canonical access pattern) =====
    public List<string> SlotQ => Skill1 ?? EmptyList;
    public List<string> SlotW => Skill2 ?? EmptyList;
    public List<string> SlotE => Skill3 ?? EmptyList;
    public List<string> SlotR => Skill4 ?? EmptyList;

    public List<string> SlotA => string.IsNullOrEmpty(SkillA)
        ? EmptyList : new List<string> { SkillA };

    public List<string> SlotI => (Skill0 != null && Skill0.Count > 0)
        ? Skill0
        : (string.IsNullOrEmpty(SkillI) ? EmptyList : new List<string> { SkillI });

    private static readonly List<string> EmptyList = new List<string>();
}
```

```csharp
// S5-09 Hercules bootstrap usage
var unit = metadataService.GetCustomData<CBSUnit>(Actor.ObjectId.Value);

if (unit.SlotQ.Count > 0) abilityComponent.BindSlot(1, unit.SlotQ[0]);
if (unit.SlotW.Count > 0) abilityComponent.BindSlot(2, unit.SlotW[0]);
if (unit.SlotE.Count > 0) abilityComponent.BindSlot(3, unit.SlotE[0]);
if (unit.SlotR.Count > 0) abilityComponent.BindSlot(4, unit.SlotR[0]);
if (unit.SlotA.Count > 0) abilityComponent.BindSlot(5, unit.SlotA[0]);
if (unit.SlotI.Count > 0) abilityComponent.BindSlot(6, unit.SlotI[0]);

// Slot 7 (Recall) bound globally elsewhere — universal ability, not in CBSUnit
```

### Implementation Guidelines

1. **Legacy field names are immutable** — never rename `Skill0/Skill1..4/SkillI/SkillA`.
   Add a comment in CBSUnit.cs: `// DO NOT RENAME — CBS reflection serializer
   keys on field names; rename breaks PlayFab deserialization.`
2. **All new code reads via aliases** — `unit.SlotQ`, never `unit.Skill1`.
3. **Existing reads (3 sites) may keep legacy names** — no forced refactor in
   Phase 2; opportunistic migration is fine.
4. **Empty pool = no ability bound** — caller checks `Count > 0` before
   `BindSlot`. Aliases never throw; they return `EmptyList` on null source.
5. **Variant pool index 0 is always the "default" / starting variant** —
   higher indices are runtime swap targets (Phase 3 RebindSlot).
6. **Recall is universal** — not in CBSUnit; bound by ActorCombat at slot 7
   from a global config or hardcoded id.

## Alternatives Considered

### Alternative 1: `CBSAbility.Slot` field (original ADR-0006 §6.1)

- **Description**: Each ability declares its preferred slot via a new `byte
  Slot` field on `CBSAbility`. Hero bootstrap reads ability list from CBSUnit
  and uses each ability's authored slot.
- **Pros**: Slot info travels with the ability; cross-hero ability reuse
  carries the slot automatically.
- **Cons**: Constrains ability to a single slot identity; rune/talent systems
  that swap an ability into a different slot fight the schema; designers
  must populate Slot for every CBS record (~hundreds of abilities) with no
  automated path.
- **Estimated Effort**: 0.25d code + indeterminate designer time
- **Rejection Reason**: User design review (2026-05-08) — slot is a property
  of "what hero gets which abilities", not of the ability itself. Per-ability
  slot declaration prevents future MOBA-style ability swap mechanics.

### Alternative 2: Rename CBSUnit fields directly (Skill1 → SlotQ)

- **Description**: Rename C# fields to match the canonical convention. Use a
  PlayFab Admin migration script to copy old field values to new field names
  in every existing CBS record.
- **Pros**: Single clean schema; no alias indirection.
- **Cons**: High-risk operation against production CBS records; requires
  PlayFab Admin API credentials, staging test, rollback plan; 0.75d work for
  what is fundamentally a cosmetic improvement.
- **Estimated Effort**: 0.75d
- **Rejection Reason**: Risk:value ratio unfavourable. Aliases achieve the
  same code-level cleanliness for 0d migration work. Defer to a Phase 4
  cleanup epic if deemed worthwhile post-launch.

### Alternative 3: `[JsonProperty("Skill1")] public List<string> SlotQ`

- **Description**: Use Newtonsoft.Json attribute to map JSON key "Skill1" to
  C# property "SlotQ".
- **Pros**: Single canonical name in code; JSON unchanged.
- **Cons**: CBSBaseCustomData uses reflection-on-fields, NOT Newtonsoft.Json.
  Attribute would have no effect — would silently break serialization.
- **Estimated Effort**: 0.1d
- **Rejection Reason**: Incompatible with CBS serializer implementation.

## Consequences

### Positive

- **Zero CBS migration risk** — existing hero records keep working unchanged.
- **Clean code access pattern** — `unit.SlotQ` everywhere new code is written.
- **Reversible** — aliases can be removed in Phase 4 cleanup epic when (and
  if) CBS field rename migration is performed.
- **Unblocks Phase 2 immediately** — S5-01 scope shrinks to "add aliases +
  tests" (0.25d), S5-09 scope unchanged (reads from CBSUnit either way).
- **Future-proof** — Phase 3 ability swap mechanics work natively; slot is
  not coupled to ability identity.

### Negative

- **Schema dual-naming** — readers of `CBSUnit.cs` see both legacy and new
  names. Mitigation: comment block explaining why; control-manifest entry
  forbidding new use of legacy field names in new code.
- **Aliases allocate** — `SlotA` / `SlotI` wrap single strings in a new list
  per call. Not measurable for typical bootstrap (called ~6× per actor at
  match start). Cache locally if hot-path use emerges.
- **Phase 4 cleanup debt** — eventually want canonical schema; deferred
  rather than eliminated.

### Neutral

- `Skill0` and `SkillI` consolidation happens at the alias layer (`SlotI`
  resolves both); underlying fields stay separate until Phase 4.

## Risks

| Risk | Probability | Impact | Mitigation |
|------|------------|--------|-----------|
| Designer authors new field on CBS dashboard with name `SlotQ` (confused by code), breaks deserialization | Low | High | Document in `CBSUnit.cs` header + control-manifest: "designer-facing names are Skill0..4/SkillI/SkillA; canonical code aliases are SlotQ..R/SlotI/SlotA — these MUST stay in sync" |
| `SlotA` / `SlotI` allocate-per-call cause GC pressure if used in tight loop | Low | Low | Aliases meant for bootstrap (cold path); profiling will catch if misused |
| Phase 4 cleanup never happens; dual-naming becomes permanent technical debt | Medium | Low | Acceptable — code reads cleanly via aliases; cleanup is cosmetic |
| Variant pool index 0 convention not enforced — designer accidentally puts non-default at [0] | Medium | Medium | Add validation in S5-01 (or later) — log warning if `SlotX[0]` doesn't match expected naming pattern; document convention in CBS authoring guide |

## Performance Implications

| Metric | Before | Expected After | Budget |
|--------|--------|---------------|--------|
| CPU (frame time) | 0ms | 0ms | 0ms (aliases called only at match start) |
| Memory | 0 | +1 alias allocation per SlotA/SlotI call | <1KB per actor |
| Load Time | n/a | n/a | n/a |
| Network | n/a | n/a | n/a (aliases bypass serialization) |

## Migration Plan

### From ADR-0006 §6.1 (CBSAbility.Slot path)

1. **Revert S5-01 implementation** — remove `CBSAbility.Slot`,
   `AbilityDataSnapshot.{Slot, EffectiveSlot, SkillKeyToSlot}`,
   `AbilitySlotTests.cs`. ✅ Done 2026-05-08.
2. **Implement S5-01 (revised)** — add alias properties to `CBSUnit.cs`,
   add EditMode tests covering: empty pool returns empty list; non-empty
   pool returns underlying field; consolidation rule for `SlotI`.
3. **Implement S5-09 (revised)** — Hercules bootstrap reads from
   `unit.Slot{Q,W,E,R,A,I}[0]` (with empty-check) instead of hardcoded ids.
4. **No CBS dashboard work required** — existing records continue serving.

### Phase 4 cleanup (post-launch, deferred)

1. Write PlayFab Admin migration script: copy `Skill0..4/SkillI/SkillA` →
   new canonical field names in every CBSUnit record.
2. Stage-verify against test environment.
3. Run on production CBS.
4. Rename C# fields; remove aliases.
5. Update `ActorCombat.cs` / `UIAvatarDetail.cs` / `UIStatObjectView.cs` to
   new names.

**Rollback plan**: Aliases are read-only properties — removing them is a
single Edit. Existing legacy field reads unaffected.

## Validation Criteria

- [ ] `CBSUnit.cs` compiles with alias properties added; existing CBS
      deserialization for all 25 heroes still produces valid records
      (regression check via existing AbilitySnapshotTests + manual load).
- [ ] EditMode tests for `SlotQ/W/E/R/A/I` aliases pass (S5-01 revised).
- [ ] Hercules bootstrap (S5-09) successfully binds 6 slots from CBSUnit
      with no hardcoded ability ids in C#.
- [ ] Hercules end-to-end Training playthrough (S5-10) plays Q/W/E/R + A
      + Recall correctly.
- [ ] No new `cbsUnit.Skill1..4` reads in code authored after this ADR
      (control-manifest enforcement, opportunistic only).

## GDD Requirements Addressed

| GDD Document | System | Requirement | How This ADR Satisfies It |
|-------------|--------|-------------|--------------------------|
| (foundational) | Ability System | Designer must be able to compose per-hero ability kit on CBS dashboard without code change | CBSUnit slot fields are the authoritative composition; bootstrap reads them via aliases |
| (foundational) | Ability System | Variant pool per slot (Merlin elemental swap) | `Skill1..4` are `List<string>`; alias preserves list semantics; index 0 is starting variant |
| (foundational) | Ability System | Slots are type-agnostic — any ability category may be placed in any slot | Decision keeps slot semantics in `CBSUnit` (composition), separates from ability behavior properties |

## Related

- [ADR-0006 Phase 2 Migration Plan](ADR-0006-phase-2-migration-plan.md) —
  §6.1 superseded by this ADR; §1–§5 (work breakdown, Hercules touch points,
  Pattern A/B/C/D refactor) remain in force.
- [ADR-0006 Unified Ability System](ADR-0006-unified-ability-system.md) —
  parent; slot-based dispatch architecture unchanged.
- Sprint 005 plan: `production/sprints/sprint-005.md` — S5-01 + S5-09
  scope updated to reflect this decision.
- Code touchpoints (after S5-01 revised + S5-09 revised land):
  - `Assets/GameScripts/Datas/DataModel/Metadata/CBS/CBSUnit.cs` (aliases)
  - `Assets/GameScripts/Gameplays/Characters/Hercules/Hercules.cs` (bootstrap)
  - `Assets/UnitTests/TestEditMode/CBSUnitSlotAliasTests.cs` (new)
