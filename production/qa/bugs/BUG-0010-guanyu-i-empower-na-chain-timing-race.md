# BUG-0010 — GuanYu I empower (DETERMINATION lifesteal) does not trigger on continuous NA chain

**Filed**: 2026-05-19 (Sprint 006 Day 5, surfaced during S6-05 GuanYu migration AC #8 manual playthrough)
**Status**: 🔴 OPEN
**Severity**: **S3** (workaround = walk/cancel then resume NA; mechanic still triggers correctly via workaround; ability is functional, just timing-sensitive)
**Priority**: **P2** (defer Sprint 007; consider bundling with Pattern 6 migration if root cause turns out to share refactor)
**Owner**: gameplay-programmer (TBD assignee)
**Affected hero**: Guan Yu (canonical: "Guan Yu"; code: `GuanYu`)
**Pre-existing**: YES — predates Sprint 006 work; S6-03/04/05 did not modify GuanYu code

---

## Symptom

GuanYu's I passive (DETERMINATION lifesteal):

1. Design intent: every 3rd Normal Attack hit should be **empowered** — apply BONUS_PHYSICAL_DAMAGE + lifesteal heal + reset stack
2. **Expected behavior**:
   - NA hit #1 → +1 DETERMINATION stack
   - NA hit #2 → +1 DETERMINATION stack (total = 2)
   - NA hit #3 → `IsEmpowered()` true → route to `OnEmpowerHit` → lifesteal fires, stacks reset
3. **Actual behavior — continuous NA chain**:
   - Player auto-attacks continuously without breaking
   - NA hit #3 does NOT trigger lifesteal — fires as a normal hit, stacks may not reset
   - Mechanic effectively broken under continuous chain conditions
4. **Workaround that does work — interrupted NA chain**:
   - Player attacks 1-2 times
   - Walks or otherwise cancels the attack queue
   - Returns and presses attack again
   - 3rd hit now correctly fires as empowered (lifesteal applies)

## Likely root cause hypothesis

**Timing race** between Fusion `[Networked]` state propagation and NA hit dispatch.

Relevant code at `Assets/GameScripts/Gameplays/Characters/GuanYu/GuanYuIAction.cs:32-66`:

```csharp
public override void Initialize()
{
    base.Initialize();
    if (IsServer == false) return;
    Actor.Combat.NormalAttack.OnHitTarget += OnNormalAttackHitTarget;
    m_maxstack = 2;
}

private void OnNormalAttackHitTarget(SkillMessage skillMessage)
{
    if (m_currentstack == m_maxstack) return;
    EmpowerStatusEffect = RunStatusEffect(Actor.Trait, "DETERMINATION", string.Empty, string.Empty);
    m_DeterminationEffectId = EmpowerStatusEffect.ID;
    m_currentstack++;
}

protected override bool IsEmpowered()
{
    return Actor.Trait.GetStatusEffectStack(m_DeterminationEffectId) >= 2;
}

protected override bool OnEmpowerHit(SkillMessage skillMessage)
{
    // ... applies bonus damage + lifesteal + resets m_currentstack = 0 ...
}
```

### Hypothesised race condition

Under continuous NA chain, the NA system dispatches hits at the animation cadence (fast). Each hit:

1. NA fires `OnHitTarget` event → `OnNormalAttackHitTarget` runs (subscriber) → calls `RunStatusEffect("DETERMINATION", ...)` to apply stack
2. NA dispatcher checks `IsEmpowered()` to decide whether to route the next hit to `OnHit` or `OnEmpowerHit`

Both operations involve Fusion-replicated state (`m_currentstack`, `m_DeterminationEffectId`, the StatusEffect's internal stack). With continuous chains:

- The stack-application from hit #2 (writing `DETERMINATION` stack) may not have propagated server-side / be readable by hit #3's `IsEmpowered()` check
- `IsEmpowered()` reads `GetStatusEffectStack` which depends on the StatusEffect's tick-replicated state
- Hit #3 sees stack still = 1 (or 0) → `IsEmpowered()` returns false → routes as normal hit
- The mechanic appears to "miss" the empower trigger

With interrupted chains, the idle ticks between attacks give the networked state time to propagate; `IsEmpowered()` correctly reads stack ≥ 2 on the resumed hit.

### Alternative hypothesis to test

- `RunStatusEffect` may not increment stack on repeated calls with the same effect ID if the status effect is still active — it may "refresh duration" instead. If that's the case, the stack stays at 1 across multiple NA hits and `IsEmpowered()` never returns true unless the first stack expires.
- Distinguishable from the timing-race hypothesis: if RunStatusEffect refreshes rather than stacks, even the interrupted-chain workaround should not work (but it does). So the timing-race hypothesis is more likely.

## Pre-existing — NOT regression from Sprint 006

S6-03 Horus / S6-04 Volund / S6-05 GuanYu migrations did NOT modify GuanYu code at all (audit-only refactors for batch 1). The user-reporter described the workaround from prior knowledge, indicating the bug predates the Sprint 006 verification window.

## Reproduction

1. `scene_game_map.unity` Training match with GuanYu
2. Find a target dummy or enemy you can hit repeatedly
3. **Reproduce broken behavior**: Press attack and let GuanYu auto-attack continuously (do not move, do not cancel)
   - Observe hits 1-3 visually — hit #3 fires without empower VFX, no lifesteal heal applied to GuanYu HP
4. **Reproduce working behavior**: Press attack 1-2 times, walk away to cancel queue, walk back, press attack
   - Observe hit #3 (resumed) — fires WITH empower VFX, lifesteal heal applied to GuanYu HP

## Investigation candidates

- `Assets/GameScripts/Gameplays/Characters/GuanYu/GuanYuIAction.cs` — primary; review whether `m_currentstack` (local field) and `GetStatusEffectStack(m_DeterminationEffectId)` (replicated read) can diverge under chain pressure
- `Assets/GameScripts/Gameplays/Cores/Stat/StatusEffect.cs` (or wherever `RunStatusEffect` lives) — does repeated `RunStatusEffect` with same ID increment stack or refresh duration?
- NA hit dispatcher — find where `IsEmpowered()` is called relative to `OnHitTarget` subscriber dispatch; if the check happens BEFORE subscribers run, that's the race
- Compare to working empower flows:
  - **HorusI** (cooldown reduction, no stack-based empower) — different mechanism entirely
  - **VolundI** (stack-based, applies to TARGET not self) — also uses `GetStatusEffectStack` threshold; could share root cause but user did NOT report Volund issue (may not have been chain-tested, or Volund's threshold = 4 stacks is less commonly reached)
- BUG-0006 fix at delta-unity `4ed9a04dda` introduced eager init for `SkillStateMachine.m_Animator`/`m_Actor` — orthogonal change, not expected to affect this. Mentioned only as a recently-touched area near ability state machines.

## Possible Pattern 6 overlap

The `GetSkillBySlot(Slot)` API proposed in [TD-011](../bugs/../../../docs/tech-debt-register.md) is for sibling-skill lookup, not directly related to this bug. However, the broader Pattern 6 migration story may include an audit of `OnEmpowerHit` / `OnHitTarget` dual-subscription patterns — if so, this bug could be bundled into the same refactor.

If the root cause is **timing-race on networked state read**, the fix shape is independent of Pattern 6:
- Option A: Use local `m_currentstack` as authoritative (already incremented synchronously) — change `IsEmpowered()` to `return m_currentstack >= m_maxstack;`
- Option B: Force a sync barrier between `OnNormalAttackHitTarget` and `IsEmpowered()` check
- Option C: Move the empower-trigger logic INTO `OnNormalAttackHitTarget` (when currentstack hits maxstack → apply empower effect immediately, don't rely on next-hit check)

Recommended for fix story: A (cheapest, fixes the read-side desync without changing event flow).

## Phase 3 / cousin pattern relevance

Other heroes with similar empower-on-Nth-hit mechanics (TBD audit during batch 2 planning):

- Anansi — TBD
- Cupid — TBD
- Merlin — TBD
- Mehmed — TBD
- Various ranged heroes

Worth grepping for `RunStatusEffect.*"DETERMINATION"` or similar empower-status patterns to identify cousins before the Pattern 6 / NA-chain-empower migration story is authored.

## References

- [Story S6-05](../../epics/phase-3-hero-migration/S6-05-guanyu-migration.md) — AC #8 surfacing window
- [S6-05 evidence](../evidence/sprint-006-phase-3-batch1.md) — playthrough sign-off with this bug noted
- [TD-011 — Pattern 6](../../../docs/tech-debt-register.md) — possible bundle candidate for Sprint 007/008 migration story
- [GuanYuIAction.cs](../../../../delta-unity/Assets/GameScripts/Gameplays/Characters/GuanYu/GuanYuIAction.cs) — primary code surface
