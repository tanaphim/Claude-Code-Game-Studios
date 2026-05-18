# BUG-0007 — Woochi Skill E: manual detonation press blocked after initial cast

**Filed**: 2026-05-18 (team report, Sprint 006 Day 4)
**Status**: 🔴 OPEN
**Severity**: S3 (workaround = wait for auto-detonate; ability completes but player loses manual timing control)
**Priority**: P2 (fix during Phase 3 batch 2 Woochi migration, or sooner if cousin pattern surfaces)
**Owner**: gameplay-programmer (TBD assignee)
**Affected hero**: Woochi (canonical: "Jeon Woo-chi"; code: `WhoChi`)

---

## Symptom

1. Player presses E → Woochi throws the skill projectile + animation plays + cooldown UI starts immediately
2. **Expected**: pressing E again (within active window before auto-detonate) should manually detonate the placed projectile
3. **Actual**: second E press does nothing — cooldown UI does not respond, no detonate event fires from the manual press
4. The projectile **DOES** auto-detonate on its internal timer eventually (ability completes correctly via fallback path)

## Pattern classification

2-step ability where:
- Step 1 (place): works
- Step 2 (detonate via manual press): **broken** — manual press not routed
- Step 2 fallback (auto-detonate via timer): works

Player retains the strategic value of the skill (damage on detonate) but loses **tactical timing control** (cannot detonate at chosen moment).

## Pre-existing — NOT regression from BUG-0006 fix

Team reported this bug **before 2026-05-18** (per operator confirmation). BUG-0006 fix (delta-unity `4ed9a04dda`) eagerly initializes `m_Animator` + `m_Actor` on `SkillStateMachine` at preload time — that change should not affect this multi-press 2-step pattern, and the bug pre-dates the fix.

## Likely root cause hypothesis

2-step ability state machine has a press-routing gap:
- Initial cast sets `BoundSlot`/`SkillState` to a "casting" state
- After initial cast resolves (`OnPerformRelease` fires → projectile spawned + CD started), the state machine transitions OUT of the "expecting input" state
- Second press goes through `OnPressButtons` / `ActorCombatAction.Progress` setter but doesn't route back to the same active projectile instance because the cooldown already started

Worth investigating:
- `WhoChiEAction.cs` — check if there's a 2-state cast pattern (Perform → Perform2 or Perform → Empower)
- Whether the manual detonate path was lost during S5-04/S5-05 Pattern A-D refactor (Hercules pilot)
- Whether the projectile object exposes a method like `Detonate()` that should be invoked by the second press but isn't

## Reproduction

1. `scene_game_map.unity` Training match with Woochi
2. Cast E (throws projectile) → projectile travels + CD starts immediately
3. Within projectile's active window (before auto-detonate timer expires), press E again
4. **Expected**: projectile detonates at current position
5. **Actual**: nothing happens; projectile continues until auto-detonate timer fires

## Phase 3 batch 2 link

Woochi is in Phase 3 **batch 2** (Sprint 007+) migration target per [EPIC](../../epics/phase-3-hero-migration/EPIC.md) line 31. Worth investigating during that migration — may share root cause with multi-state ability pattern that affects other 2-step heroes (e.g. Mehmed A variant, Merlin 3-stance Q/W/R in batch 3).

## Investigation candidates

- `Assets/GameScripts/Gameplays/Characters/WooChi/WhoChiEAction.cs` — primary
- `Assets/GameScripts/Gameplays/Characters/WooChi/WhoChiIAction.cs` — passive, may interact
- Compare to working 2-step patterns: Anansi W (place + detonate), Mehmed A (place + activate)
- Audit how `OnPressButtons` routes second press when the action is already past `OnPerformRelease`

## References

- [EPIC: Phase 3 Hero Migration](../../epics/phase-3-hero-migration/EPIC.md) line 31 — Woochi batch 2 slot
- Cousin pattern: Anansi W 2-step detonate ([AnansiWAction.cs](../../../../delta-unity/Assets/GameScripts/Gameplays/Characters/Anansi/AnansiWAction.cs))
