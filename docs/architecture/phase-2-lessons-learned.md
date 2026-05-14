# Phase 2 Lessons Learned — Hercules Pilot Migration

> **Source**: Sprint 005 (2026-05-09 → 2026-05-22) — ADR-0006 Phase 2
> **Status**: Living document — append patterns as Phase 3 hero migrations surface new lessons
> **Audience**: gameplay-programmer, network-programmer, unity-specialist
> **Companion to**: ADR-0006 (Unified Ability System) Phases 1a/1b/2/3

---

## Overview

Phase 2 migrated **Hercules** end-to-end from `SkillKey` enum literals + `Combat.SkillN` direct property reads to slot-bound runtime (`BoundSlot` + `GetSlotAction(byte)` + `GetActiveSlot()` + slot-indexed `AbilityComponent.Slots` dictionary). This document captures the patterns that survived production playtest and the anti-patterns that bit us.

**Phase 3 will repeat this work for ~20 more heroes** (Anansi, Merlin, Garen, Cupid, GuanYu, Athena, HattoriHanzo, etc.). Apply these patterns by default; deviations require an ADR amendment.

---

## Pattern 1 — Dual-path retention with slot-or-legacy guard

**Origin**: S5-04 `IsActiveSlotOwner` + `ResolveLegacyOwnerBySkillKey`; S5-05 `IsInputSlotMatch` + `ResolveLegacySkillKeyButtonMatch`; S5-09 `BindSlot` with legacy `CreateSkill` fallback; S5-21 `SetActiveSlot` with `ResolveSlotFromSkillKey` fallback.

### Why
Migrated hero (Hercules) has `BoundSlot != 0` once `AbilityComponent.BindSlot` has populated the per-slot dictionary on `OnStartup`. Unmigrated heroes (every hero except the one currently being migrated) still have `BoundSlot == 0`. The same runtime code must serve both populations safely until Phase 3 drains the unmigrated set.

### Pattern

```csharp
// Slot-first, legacy-fallback:
var slot = BoundSlot != 0
    ? BoundSlot
    : ResolveSlotFromSkillKey(AbilityData.SkillKey);

if (slot != 0)
{
    // proceed via new slot-bound path
}
else
{
    // SkillKey.Item or .None — neither path applies, drop / no-op
}
```

### Concrete examples
- `ActorCombatAction.IsActiveSlotOwner()` — slot reference equality OR legacy SkillKey owner-guard
- `ActorCombatAction.IsInputSlotMatch()` — `input.PressedSlot == BoundSlot` OR legacy `Buttons.IsSet(...)`
- `ActorCombatAction.IsQuickCastForBoundSlot()` — `Actor.Combat.IsQuickCast(BoundSlot)` OR legacy `IsQuickQ/W/E/R`
- `ActorCombatAction.Progress.set` (S5-21) — `SetActiveSlot(BoundSlot)` OR `SetActiveSlot(ResolveSlotFromSkillKey(...))`
- `ActorCombat.BootstrapSlotBindings` (S5-09) — Hero path uses `BindSlot`, non-Hero falls through to legacy `CreateSkill`

### When to apply
**Always**, on any new `BoundSlot`-aware code path until Phase 3 deletes the `SkillKey` enum (per ADR-0006 §10). The legacy branch is removable only when `BoundSlot == 0` is impossible across all heroes.

### Phase 3 removal target
Migration of the last hero (currently Hercules is the only migrated hero). Schedule a `git grep "BoundSlot != 0"` sweep + drop the legacy branch in a single commit per call site.

---

## Pattern 2 — Static-helper extraction for NetworkBehaviour testability

**Origin**: S5-02/S5-03 retrospective seed — `ActorCombat` and `ActorCombatAction` are `NetworkBehaviour`; EditMode tests cannot instantiate Fusion types. Static helpers from S5-04 (`ResolveIsActiveSlotOwner`, `ResolveLegacyOwnerBySkillKey`), S5-05 (`ResolveLegacySkillKeyButtonMatch`, `ResolveLegacyQuickCast`), S5-06 (`TryResolveSlotRoute`), S5-21 (`ResolveSlotFromSkillKey`).

### Why
EditMode tests run without a `NetworkRunner`. Reading networked state (`[Networked]` properties) or constructing a `NetworkBehaviour` subclass NREs immediately. Pure functions sidestep the harness entirely — they run in ~1ms per case, no Fusion runtime required.

### Pattern

Extract the **decision logic** from an instance method into a `public static` helper that:
1. Takes only value types or interface references as parameters (no `NetworkBehaviour` types).
2. Has zero side effects — no logging, no state writes, no `Actor.*` calls.
3. Returns either a primitive (`bool`, `byte`) or uses `out` parameters for additional outputs.
4. Lives in the same file as the instance method that uses it (co-located, removable together in Phase 3).

```csharp
// Instance method — requires NetworkRunner, untestable in EditMode
private bool IsInputSlotMatch(NetworkButtons legacyButtons, byte inputSlot)
{
    if (BoundSlot != 0) return inputSlot != 0 && inputSlot == BoundSlot;
    return ResolveLegacySkillKeyButtonMatch(AbilityData.SkillKey, legacyButtons);
}

// Static helper — pure, EditMode-testable
public static bool ResolveLegacySkillKeyButtonMatch(SkillKey key, NetworkButtons buttons) =>
    key switch
    {
        SkillKey.Q => buttons.IsSet(Buttons.Q),
        SkillKey.W => buttons.IsSet(Buttons.W),
        // ...
        _ => false,
    };
```

### Test coverage rule of thumb
- **Instance methods that read networked state** → tested via multipeer harness + manual playthrough, not EditMode.
- **Static helpers** → cover every reachable branch (≥1 test per `switch` case + boundary inputs).
- **Test file naming**: `[InstanceClass]SlotMatchTests.cs`, `[InstanceClass]OwnerGuardTests.cs`, etc. — namespace `Radius.Tests.Characters`, NUnit, `[TestFixture]`, `[Test]`, `test_<scenario>_<expected>()`.

### When to apply
Whenever a `switch` / boolean-combinator / mapping function appears inside a `NetworkBehaviour` method and the same logic could be expressed without Fusion types. Don't extract for the sake of it — only when the logic has decision content worth testing.

---

## Pattern 3 — Central state-transition instrumentation

**Origin**: S5-21 v1→v2 fix (TD-006). v1 wired `Actor.Combat.SetActiveSlot()` into `OnPressButtons` (the obvious manual-press entry point). Unity playtest revealed Normal Attack auto-target sets `Progress = SkillState.Attack1` directly at `ActorCombatAction.cs:2693` and `:2770` — **bypassing `OnPressButtons` entirely**. Result: 30+ "slot=0" warnings per match + no Normal Attack damage. v2 moved the write into the `Progress` setter (line 402).

### Why
When N entry points trigger the same state transition, instrumenting at one of the entry points covers only that path. Future code that triggers the same transition from a new path will bypass the instrumentation silently. The correct location is the **single state-transition point** that all entry points must pass through.

### Pattern

```csharp
// ❌ WRONG — multiple entry points; some bypass this one
protected void OnPressButtons(InputMessage input)
{
    // SetActiveSlot here — Normal Attack auto-target doesn't call OnPressButtons,
    // so this fires for Q/W/E/R presses but never for Attack1/2/3 transitions.
    Actor.Combat.SetActiveSlot(slot);
    // ... existing body
}

// ✅ CORRECT — central transition point catches every path
public SkillState Progress
{
    set
    {
        if (Object == null || !Object.IsValid) return;
        if (m_Progress == value) return;
        if (Runner.IsServer)
        {
            m_Progress = value;

            // Catches all transition paths — manual press, auto-attack,
            // R-charge release, passive triggers — atomically.
            if (value != SkillState.None && AbilityData != null)
            {
                var slot = BoundSlot != 0
                    ? BoundSlot
                    : ResolveSlotFromSkillKey(AbilityData.SkillKey);
                if (slot != 0) Actor.Combat.SetActiveSlot(slot);
            }
        }
    }
}
```

### Identification rule
Before wiring a side-effect at an entry point, ask: **"What is the state transition I am observing?"** Then locate the `[Networked]` property setter (or equivalent state machine) that represents that transition. Wire the side-effect at the setter, not at one of the entry points.

### When to apply
Any time the side-effect is logically tied to "the ability is now active / casting / hitting / etc." rather than "the player pressed a button". Press handling is a separate concern from state observation.

---

## Pattern 4 — Server-authoritative networked writes

**Origin**: S5-04 `SetBoundSlot`, S5-21 `Progress` setter SetActiveSlot block, S5-09 `ActorCombat.SetActiveSlot` itself.

### Why
`[Networked]` properties in Fusion 2 are replicated via the tick-based state synchroniser. Writes must originate from the **state authority** (server in Shared/Hosted mode); client writes are silently discarded by Fusion and cause divergence between host and client state if attempted from gameplay code that wraps them in a setter expecting both sides to apply.

### Pattern

Every networked write must be guarded by **one** of:

```csharp
// Option A — explicit HasStateAuthority guard (preferred for public API)
public void SetActiveSlot(byte slot)
{
    if (Object == null || Object.IsValid == false) return;
    if (HasStateAuthority == false) return;   // ← guard
    m_ActiveSlot = slot;
}

// Option B — Runner.IsServer guard (preferred for internal setters)
public SkillState Progress
{
    set
    {
        // ...
        if (Runner.IsServer)   // ← guard
        {
            m_Progress = value;
            // side effects also inside the guard
        }
    }
}
```

### Forbidden
- ❌ Writing a `[Networked]` property from `Update()` or `FixedUpdateNetwork()` on the client peer without a guard.
- ❌ Calling a method that writes networked state from RPC handlers on the wrong authority side.
- ❌ Relying on "client predicts, server corrects" without explicit prediction code — Fusion does not auto-predict unguarded writes.

### Test coverage
Cannot be EditMode-tested (no `NetworkRunner`). Verification path:
1. Multipeer harness (`PrototypeTest.unity`) — assert parity between host & client peers after a write.
2. Manual playthrough in `scene_game_map.unity` — confirm replicated state visible on both peers.
3. Optional: extend `AbilityMultipeerRunner` parity check with per-property assertion (1 line per check).

---

## Pattern 5 — API + caller pair audit (story-readiness gate)

**Origin**: 4 surfacings in Sprint 005 alone — TD-006 (S5-03 added `SetActiveSlot` API, no caller until S5-21), TD-007 (S5-09 added `BindSlot` caller, no `AbilityRegistry` registration until S5-10), S5-06 attempted shim migration without the upstream caller, S5-21 v1 wired the caller at the wrong entry point.

### Why
ADRs frequently specify a contract as "**an API X exists, called by Y when Z happens**". Implementation stories tend to land the **API** half cleanly (it's a single file change), but the **caller** half is invisible in the API's own diff — it lives in a different file, often a different layer. Story-readiness must explicitly check both halves.

### Pattern

**For every story that adds a new `public` method, [Networked] property, or service**, the story-readiness gate must verify:

1. **At least one caller exists in production code** (not just in tests). Use `grep -rn "\.NewMethod(" Assets/GameScripts/`.
2. **The caller is reachable from production scene boot** — registered service, attached prefab component, wired animator event, etc. (TD-007 was about an API registered as `DeltaBaseService` but never added to `DeltaConfiguration.Services` list.)
3. **The caller fires at the right time** — not too early (state not yet initialised), not too late (downstream observer missed the write).

### Anti-patterns this catches
- "API exists with no caller" → story is incomplete; caller belongs in this story or a paired story.
- "Caller exists but service not registered" → registration is part of the story, not a follow-up.
- "API + caller both exist but caller is at wrong entry point" → see Pattern 3.

### Story-readiness checklist addendum

When a story adds a new method / property / service, the story-readiness verdict must explicitly state:

```markdown
## API ↔ caller audit
- [ ] New API: `ClassName.NewMethod(args)` — file:line
- [ ] Production caller exists: file:line (`grep -rn ...` confirms)
- [ ] Caller fires from production scene boot: [describe path]
- [ ] Caller fires at correct lifecycle moment: [describe]
```

If any item is unchecked → story is **NEEDS WORK**, not READY.

---

## Cross-cutting: when to escalate

| Situation | Escalation path |
|-----------|-----------------|
| New `NetworkBehaviour` in a hero — uncertain about lifecycle | unity-specialist + network-programmer |
| State machine transition has > 2 entry points | Apply Pattern 3 (central instrumentation); review with lead-programmer if non-obvious central point |
| Need to test logic that touches both `NetworkRunner` and gameplay state | Apply Pattern 2 (static-helper extraction); if logic can't be extracted, schedule PlayMode test framework (Sprint 006 candidate) |
| Hero migration broke a non-Hercules hero | Apply Pattern 1 (verify dual-path is correct); if no dual-path, the migration scope was wrong |
| Manual playtest finds animation event drops / VFX missing / no damage | Check Pattern 3 — `Actor.Combat.GetActiveSlot()` returns 0 means a state-transition path isn't writing slot |

---

## Phase 3 application checklist (per-hero migration)

For each remaining hero (Anansi, Merlin, Garen, Cupid, GuanYu, Athena, HattoriHanzo, ...):

1. **Pre-flight** — confirm the hero's `BoundSlot` will be populated on `OnStartup` via `AbilityComponent.BindSlot` (S5-09 path). Add a test that asserts `Slots.Count == 7` post-Spawned.
2. **Action file refactor** — replace `SkillKey.Q/W/E/R/Recall` literals + `Combat.Skill1/2/3/4` direct reads with slot-bound calls. Use dual-path (Pattern 1) until the hero is fully on slot-binding.
3. **Animation event verification** — confirm `Progress` setter writes slot for every state this hero enters (Pattern 3). Watch Console for `[S5-06] StateReleaseSlot ... slot=0` warnings during the per-ability playthrough.
4. **Multipeer Pass #1–5** — re-run `PrototypeTest.unity` with the new hero. Capture log.
5. **Soak** — let the hero ride dev for ≥3 days before considering the migration "done" per ADR-0006 §10.

When the last hero migrates, schedule a sweep to remove all Pattern 1 legacy branches (one PR per call site or one large refactor PR — discuss with lead-programmer).

---

## References

- ADR-0006 — Unified Ability System (parent)
- ADR-0006 Phase 1a / 1b / 2 — implementation plans
- ADR-0008 — Slot Binding via CBSUnit
- Sprint 005 stories: S5-01 through S5-10 + S5-21
- Tech debt register: TD-001 through TD-007 (Phase 2 surfaced)

## Changelog

- **2026-05-14** — Initial version with 5 patterns (Sprint 005 retro carry-forward).
