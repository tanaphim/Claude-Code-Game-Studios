# Story: S6-04 — Volund Migration (Phase 3 batch 1, hero #2)

**Type**: Logic + Integration
**ADR**: ADR-0006 Phase 2 Migration Plan (Phase 3 uses identical patterns §5-6); ADR-0008 Slot Binding via CBSUnit
**GDD**: N/A — refactor story; hero-specific ability design lives in CBSUnit data + per-ability `*.cs` action files, not in any GDD
**Engine notes**: N/A — Unity 2022.3 LTS (within LLM training data per `docs/engine-reference/unity/VERSION.md`); no post-cutoff API usage expected
**Manifest Version**: N/A (control-manifest.md not yet created)
**Estimate**: 0.5d
**Priority**: Must Have
**Epic**: [Phase 3 Hero Migration](EPIC.md)
**Owner (role)**: gameplay-programmer
**Assignee**: tanapol

## Dependencies

- ✅ S6-03 closed (sequential — uses Horus migration to validate batch pattern before Volund)
- ✅ All Phase 2 infrastructure landed (see EPIC.md)
- ⏳ Phase 2 soak green (ends 2026-05-21)
- ✅ CBS data: Volund — verified Hero record exists

## Context

Volund is a 5-ability hero (Q W E R + I) with a notably **large W action** (118 lines, biggest of the 4 batch 1 heroes). Code lives at:

- `Assets/GameScripts/Gameplays/Characters/Volund/VolundQAction.cs` (68 lines)
- `Assets/GameScripts/Gameplays/Characters/Volund/VolundWAction.cs` (118 lines) — largest W in batch
- `Assets/GameScripts/Gameplays/Characters/Volund/VolundEAction.cs` (31 lines)
- `Assets/GameScripts/Gameplays/Characters/Volund/VolundRAction.cs` (42 lines)
- `Assets/GameScripts/Gameplays/Characters/Volund/VolundIAction.cs` (48 lines)

**BUG-0001 cousin flagged**: PR #357 commit specifically called out `VolundW` as likely benefiting from the `AnimatorStateSync` stale-hash fix. Since VolundW is also the largest action file in batch 1, **VolundW is the primary BUG-0001 cousin verification target** — both for code audit (largest surface) and for animation-stuck regression check.

## Acceptance Criteria

1. **Code audit complete**: scan all 5 Volund action files for hardcoded `SkillKey.X` references, hardcoded slot literals, or sibling-skill reads via direct properties. Document findings in PR description. Given VolundW's size (118 lines), expect **highest hardcoded-SkillKey count** of batch 1 — confirm or refute.
2. **Pattern replacements applied** (Patterns A-D from Phase 2) where code shapes match. Dual-path fallback retained.
3. **`ActorCombat.OnStartup` BindSlot pipeline verified for Volund**: 0 `BindSlot not registered` warnings.
4. **`StateReleaseSlot` routing verified**: 0 `slot=0` warnings during cast. VFX/SFX correct on Q/W/E/R + I.
5. **BUG-0001 cousin verification (VolundW specifically)**: cast Volund W → observe animation behavior. If locomotion re-engages cleanly post-cast → mark BUG-0001 cousin (VolundW) as **CLOSED-BY-SIDE-EFFECT** in BUG-0001.md addendum. If reproduces → file BUG-0005 candidate.
6. **Multipeer harness Pass #1-5 green** with Volund loaded; bandwidth ≤65 B/s preserved.
7. **Manual playthrough**: 1-match Training with Volund — all 5 abilities functional + Normal Attack + Recall + Item.
8. **VolundW-specific gate**: VolundW (largest action file) does not introduce new patterns beyond A-D. If a new pattern surfaces → document as Pattern 6 candidate in `phase-2-lessons-learned.md` before merging.

## Out of Scope

- Refactoring VolundW's size for maintainability (118 lines is large but functional; not Phase 3 scope)
- BUG-0001 cousin verification for HorusE/HorusR — covered in S6-03
- R-23 AdditionalMoveSpeed — Volund does NOT use this field (verified via grep), no hold-the-line concern
- Loc/UX work for "Volund" canonical name — matches code (no divergence)

## Test Evidence

**Required**:
- `production/qa/evidence/sprint-006-phase-3-batch1.md` — Volund per-ability checklist + BUG-0001 cousin (VolundW) verification + sign-off (append to S6-03 evidence doc)
- `production/qa/evidence/S6-04-multipeer.txt` — multipeer Pass #1-5
- PR description: pattern counts pre/post, especially VolundW size analysis

**Optional**:
- EditMode tests `Assets/UnitTests/TestEditMode/VolundSlotBindingTests.cs`
- Screen recording of VolundW cast for BUG-0001 cousin verification

## Performance Impact

Expected zero. VolundW size is implementation detail; refactor is pure transformation.

## Files to Modify (anticipated)

- 5 × Volund action `.cs` files (audit + Pattern A-D where applicable)
- `production/qa/evidence/sprint-006-phase-3-batch1.md` (APPEND Volund section)
- `production/qa/bugs/BUG-0001-recall-locomotion-stuck.md` (APPEND VolundW cousin result)

## Risks

- **R1** — VolundW's 118-line complexity hides a pattern not seen in Hercules pilot. **Mitigation**: AC #1 + AC #8 surface this; document new pattern before applying
- **R2** — BUG-0001 cousin VolundW still reproduces. **Mitigation**: AC #5; file BUG-0005 if needed
- **R3** — VolundW reads networked state in ways that don't replicate cleanly on multipeer client. **Mitigation**: multipeer Pass #4 (parity) AC #6 gates this

## References

- [Epic: Phase 3 Hero Migration](EPIC.md)
- [S6-03 Horus migration](S6-03-horus-migration.md) — sequential predecessor; pattern reuse
- [Phase 2 Lessons Learned](../../../docs/architecture/phase-2-lessons-learned.md)
- BUG-0001 fix PR #357 — cousin bug list including VolundW
