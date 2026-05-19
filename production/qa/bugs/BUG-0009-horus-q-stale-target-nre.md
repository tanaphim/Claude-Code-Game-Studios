# BUG-0009 — HorusQAction.DashEnd NullReferenceException on stale `Target` base-property reference

**Filed**: 2026-05-18 (Sprint 006 Day 4, surfaced during S6-03 Horus migration AC #7 manual playthrough)
**Status**: ✅ **RESOLVED in-line by S6-03**
**Severity**: S2 (NRE during ability cast; ability fails mid-execution; would block S6-03 AC #7 "No S1/S2 bugs surfaced")
**Priority**: P1 (fix-inline to unblock S6-03 close)
**Owner**: gameplay-programmer
**Assignee**: tanapol
**Affected**: Horus Q ability (`HorusQAction.cs`)
**Pre-existing**: YES — bug shape predates Sprint 006; surfaced now because S6-03 AC #7 includes formal cast-Q test in Training match

---

## Symptom

`NullReferenceException` thrown from `HorusQAction.DashEnd()` during Q ability cast in Training match:

```
NullReferenceException: Object reference not set to an instance of an object
Radius.Gameplays.Characters.HorusQAction.DashEnd ()
  at Assets/GameScripts/Gameplays/Characters/Horus/HorusQAction.cs:34
Radius.Gameplays.Cores.NetworkTrait.Dash (System.Single range, System.Single speed,
  System.Action callback, System.Boolean blockObstacle)
  at Assets/GameScripts/Gameplays/Cores/Stat/NetworkStatusEffect.cs:864
System.Runtime.CompilerServices.AsyncMethodBuilderCore+<>c.<ThrowAsync>b__7_0 (...)
UnityEngine.UnitySynchronizationContext+WorkRequest.Invoke ()
UnityEngine.UnitySynchronizationContext.Exec ()
UnityEngine.UnitySynchronizationContext.ExecuteTasks ()
```

## Root cause

`HorusQAction.DashEnd()` mixed two references to "the Q target":

- `target` — private field cached at `OnPerformEnter()` (`target = Target;` on line 18); stable for the lifetime of the cast
- `Target` — base-class property on `ActorCombatAction`; refreshed by the skill state machine and may be reset to `null` by the time the dash callback fires

Before the fix, `DashEnd()` null-checked `target` (line 26) but then dereferenced `Target.transform.position` (line 34) when invoking `target.Trait.Knock(...)`. Because the dash completion callback fires asynchronously after the dash duration, the state machine has often transitioned out of the "Perform" state by the time `DashEnd` runs — clearing `Target` while the cached `target` field is still valid. Dereferencing the cleared base property produced the NRE.

This was masked in most plays because short-range Q casts complete before any state transition resets `Target`. Long-range casts, casts on moving targets, or casts during state-machine timing pressure (multipeer tick variance, AnimationEvent vs. StateMachineBehaviour race per BUG-0006 lessons) expose the gap.

## Why surfaced now

S6-03 AC #7 is the first manual playthrough since the Phase 2 migration that explicitly tests Horus Q in Training match as a sign-off gate (rather than incidental play). The cast-on-target → dash-completes path was exercised under sign-off scrutiny rather than playtest-and-move-on conditions.

Not caused by:
- Phase 2 migration work (S5-09..S5-21)
- BUG-0006 fix at delta-unity `4ed9a04dda` (touches `SkillStateMachine`, not `HorusQAction`)
- Sprint 006 prior commits

## Fix

1-line edit at [HorusQAction.cs:34](../../../delta-unity/Assets/GameScripts/Gameplays/Characters/Horus/HorusQAction.cs):

```diff
-       target.Trait.Knock(Target.transform.position + Actor.transform.forward * 2, AbilityData.Speed / 100f, 0f);
+       target.Trait.Knock(target.transform.position + Actor.transform.forward * 2, AbilityData.Speed / 100f, 0f);
```

Use the cached `target` reference for the position read instead of the base-class `Target` property. The line-26 null-check already covers cached `target` (including Unity's fake-null for destroyed `GameObject`s), so no additional guard is needed.

Verification: re-cast Q in Training match → no NRE; Knock effect applies at target's last-known position.

## Cousin audit needed

Similar `target` (cached) / `Target` (base) pattern may exist in other Horus actions or other hero actions that:

- Cache `Target` into a private field during `OnPerformEnter` / `OnPerformRelease`, AND
- Reference base-class `Target` from an async callback (dash callback, animation event, status-effect-end callback) instead of the cached field

**Grep candidates** (run during follow-up):

```bash
grep -rn "private Actor target" Assets/GameScripts/Gameplays/Characters/
grep -rn "Target\.transform\|Target\.Trait\|Target\.IsDead" Assets/GameScripts/Gameplays/Characters/
```

HorusR is the immediate cousin candidate (also has dash callback `DashEnd`). Verified in S6-03 audit: HorusR uses `Position` (cached from `Target.transform.position` at cast time, line 33), not `Target.transform`, so it does NOT have this bug shape.

Add cousin findings to this bug or file follow-up bugs as they surface during S6-04..06 playthroughs.

## References

- [S6-03 Horus Migration story](../../epics/phase-3-hero-migration/S6-03-horus-migration.md)
- [S6-03 evidence doc](../evidence/sprint-006-phase-3-batch1.md)
- [HorusQAction.cs](../../../../delta-unity/Assets/GameScripts/Gameplays/Characters/Horus/HorusQAction.cs)
- Stack pointer: `NetworkStatusEffect.cs:864` (Dash callback dispatch — unrelated to root cause; just the call site)
