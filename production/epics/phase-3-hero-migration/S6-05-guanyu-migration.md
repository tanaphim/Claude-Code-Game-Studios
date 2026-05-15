# Story: S6-05 — Guan Yu Migration (Phase 3 batch 1, hero #3)

**Type**: Logic + Integration
**ADR**: ADR-0006 Phase 2 Migration Plan (Phase 3 uses identical patterns §5-6); ADR-0008 Slot Binding via CBSUnit
**Manifest Version**: N/A (control-manifest.md not yet created)
**Estimate**: 0.5d
**Priority**: Must Have
**Epic**: [Phase 3 Hero Migration](EPIC.md)
**Owner (role)**: gameplay-programmer
**Assignee**: tanapol

## Dependencies

- ✅ S6-04 closed (sequential — Volund migration validates batch pattern before more complex GuanYu)
- ✅ All Phase 2 infrastructure landed (see EPIC.md)
- ⏳ Phase 2 soak green (ends 2026-05-21)
- ✅ CBS data: GuanYu — verified Hero record exists

## Context

Guan Yu (canonical: "Guan Yu", code: `GuanYu`) is a **6-ability hero** (Q W E R + I + N) — first batch 1 hero with the I + N pair. Code lives at:

- `Assets/GameScripts/Gameplays/Characters/GuanYu/GuanYuQAction.cs` (22 lines)
- `Assets/GameScripts/Gameplays/Characters/GuanYu/W/GuanYuWAction.cs` + `GuanYuWOnHit.cs` (**sub-system**)
- `Assets/GameScripts/Gameplays/Characters/GuanYu/GuanYuEAction.cs` (41 lines)
- `Assets/GameScripts/Gameplays/Characters/GuanYu/R/GuanYuRAction.cs` + `RotateGO.cs` (**sub-system**)
- `Assets/GameScripts/Gameplays/Characters/GuanYu/GuanYuIAction.cs` (67 lines)
- `Assets/GameScripts/Gameplays/Characters/GuanYu/GuanYuNAction.cs` (64 lines)

**GuanYu is more structurally complex** than Horus/Volund because W and R have their own sub-directories with companion classes (`GuanYuWOnHit.cs`, `RotateGO.cs`). These companion files are likely NOT NetworkBehaviour ability-action subclasses but rather helpers/event-listeners that may also reference `SkillKey` or `Combat.Skill*`.

**BUG-0001 cousin flagged**: PR #357 specifically called out `GuanYuE`. This story's BUG-0001 cousin verification target is **GuanYuE**.

## Acceptance Criteria

1. **Code audit complete**: scan all 6 GuanYu action files **PLUS** the 2 companion files (`GuanYuWOnHit.cs`, `RotateGO.cs`) for hardcoded `SkillKey.X` references, hardcoded slot literals, or sibling-skill reads. Document findings in PR description. **Companion files are in-scope for audit** even if they don't end in `Action.cs`.
2. **Pattern replacements applied** in action + companion files where applicable. If companion files need their own pattern (e.g., they read `Actor.Combat.SkillW` directly to check charge state) → document as a Phase 3 sub-pattern.
3. **`ActorCombat.OnStartup` BindSlot pipeline verified for GuanYu**: 0 `BindSlot not registered` warnings.
4. **`StateReleaseSlot` routing verified**: 0 `slot=0` warnings. VFX/SFX correct on Q/W/E/R + I + N (6 abilities).
5. **BUG-0001 cousin verification (GuanYuE specifically)**: cast GuanYu E → observe animation behavior. Mark CLOSED or file BUG-0005 candidate.
6. **N-ability path verified**: `GuanYuNAction` (Normal Attack variant) routes correctly through `StateReleaseSlot`. Specifically verify that Normal Attack auto-target flow (S5-21 v2 fix in `Progress` setter) doesn't bypass slot binding for GuanYu — this is the exact case that surfaced TD-006 in Hercules.
7. **Multipeer harness Pass #1-5 green** with GuanYu loaded; bandwidth ≤65 B/s preserved.
8. **Manual playthrough**: 1-match Training with GuanYu — all 6 abilities + Normal Attack + Recall + Item.
9. **Sub-system documentation**: append a 1-paragraph note to `phase-2-lessons-learned.md` if companion-file pattern (W/, R/ sub-dirs) is unique to GuanYu or recurs in batch 2 candidates (worth a Phase 3 sub-pattern entry?).

## Out of Scope

- Refactoring W/R sub-system structure (companion files are pre-existing design; not Phase 3 scope)
- BUG-0001 cousin verification for non-GuanYu heroes (covered in S6-03/04)
- R-23 AdditionalMoveSpeed — GuanYu does NOT use this field (verified)
- Loc/UX work for "Guan Yu" vs `GuanYu` naming divergence — flag for future loc story (canonical has space, code does not)

## Test Evidence

**Required**:
- `production/qa/evidence/sprint-006-phase-3-batch1.md` — GuanYu section (append) including 6-ability + sub-system + BUG-0001 cousin (GuanYuE) verification
- `production/qa/evidence/S6-05-multipeer.txt` — multipeer Pass #1-5
- PR description: action+companion file audit findings; explicit N-action path verification (TD-006 cousin guard)

**Optional**:
- EditMode tests `Assets/UnitTests/TestEditMode/GuanYuSlotBindingTests.cs`
- Screen recording of GuanYuE cast + GuanYu N (Normal Attack) sequence

## Performance Impact

Expected zero. Sub-system files (GuanYuWOnHit, RotateGO) are runtime objects (possibly pooled) — refactor is pure code, no allocation change.

## Files to Modify (anticipated)

- 4 × GuanYu root action `.cs` files (Q E I N)
- 2 × W/ sub-system files (`GuanYuWAction.cs`, `GuanYuWOnHit.cs`)
- 2 × R/ sub-system files (`GuanYuRAction.cs`, `RotateGO.cs`)
- `production/qa/evidence/sprint-006-phase-3-batch1.md` (APPEND)
- `production/qa/bugs/BUG-0001-recall-locomotion-stuck.md` (APPEND GuanYuE cousin result)
- `docs/architecture/phase-2-lessons-learned.md` (APPEND if sub-system pattern noted)

## Risks

- **R1** — Companion files (`GuanYuWOnHit`, `RotateGO`) read networked state in patterns not covered by Phase 2 helpers. **Mitigation**: AC #1 + #2 surface this; document new pattern before applying
- **R2** — `GuanYuNAction` (Normal Attack variant) bypasses `Progress` setter (the exact TD-006 case fixed for Hercules in S5-21). If GuanYu has a different NA entry-point → re-investigate `SetActiveSlot` wiring for this hero specifically. **Mitigation**: AC #6 explicit gate
- **R3** — BUG-0001 cousin GuanYuE still reproduces. **Mitigation**: AC #5; file BUG-0005
- **R4** — Sub-pattern discovered (e.g., companion-file pattern) suggests batch 2 heroes (Anansi N, KingArthur P, etc.) need pre-audit before their stories are estimated. **Mitigation**: AC #9 documents finding for future planning

## References

- [Epic: Phase 3 Hero Migration](EPIC.md)
- [S6-04 Volund migration](S6-04-volund-migration.md) — sequential predecessor
- [Phase 2 Lessons Learned](../../../docs/architecture/phase-2-lessons-learned.md)
- BUG-0001 fix PR #357 — cousin bug list including GuanYuE
- S5-21 — TD-006 SetActiveSlot wiring in `Progress` setter (Normal Attack auto-target path fix)
