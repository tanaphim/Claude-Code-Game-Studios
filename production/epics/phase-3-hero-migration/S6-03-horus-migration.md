# Story: S6-03 — Horus Migration (Phase 3 batch 1, hero #1)

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

- ✅ **All Phase 2 infrastructure landed** (S5-01..S5-21 — see EPIC.md dependency list)
- ⏳ **Phase 2 soak green** (ends 2026-05-21) — Hercules clean, no slot-related regressions
- ✅ **CBS data: Horus** — verified Hero record exists in production; SlotQ..SlotI fill status verified async (user-side or pre-merge spot-check)

## Context

Horus is a 5-ability hero (Q W E R + I). Code lives at:

- `Assets/GameScripts/Gameplays/Characters/Horus/HorusQAction.cs` (54 lines)
- `Assets/GameScripts/Gameplays/Characters/Horus/HorusWAction.cs` (63 lines)
- `Assets/GameScripts/Gameplays/Characters/Horus/HorusEAction.cs` (26 lines)
- `Assets/GameScripts/Gameplays/Characters/Horus/HorusRAction.cs` (42 lines)
- `Assets/GameScripts/Gameplays/Characters/Horus/HorusIAction.cs` (90 lines)

**BUG-0001 cousin flagged**: Sprint 005 PR #357 commit message identified `HorusE` + `HorusR` as likely benefiting from the `AnimatorStateSync` stale-hash fix. **This story is the verification window** — migrate + playtest, confirm whether the cousin bugs are closed-by-side-effect or need their own fix.

## Acceptance Criteria

1. **Code audit complete**: scan all 5 Horus action files for hardcoded `SkillKey.X` references, hardcoded slot literals (1..6), or sibling-skill reads via `Actor.Combat.Skill1..4` direct properties. Document findings in PR description (count per pattern type).
2. **Pattern replacements applied** (if any code shape matches Patterns A-D from Phase 2): replacements use proven helpers (`IsActiveSlotOwner`, `IsInputSlotMatch`, `IsQuickCastForBoundSlot`, `MaxRank <= 1`). Dual-path fallback retained where `BoundSlot == 0` is reachable (legacy/respawn windows).
3. **`ActorCombat.OnStartup` BindSlot pipeline verified for Horus**: in `scene_game_map.unity` Console, Horus spawns with **0** `BindSlot not registered` warnings (the TD-007 regression guard).
4. **`StateReleaseSlot` routing verified for Horus animation events**: 0 `[S5-06] StateReleaseSlot ... slot=0` warnings during cast (TD-006 regression guard). VFX/SFX fire correctly on Q/W/E/R + I.
5. **BUG-0001 cousin verification (HorusE + HorusR)**: cast Horus E and Horus R; observe `AnimatorStateSync` behavior post-cast. If locomotion re-engages cleanly without stuck-animation residual → mark BUG-0001 cousin (HorusE/HorusR) as **CLOSED-BY-SIDE-EFFECT** in BUG-0001.md addendum. If residual bug visible → file new bug (BUG-0005 candidate).
6. **Multipeer harness Pass #1-5 green** with Horus loaded: parity + bandwidth ≤65 B/s preserved.
7. **Manual playthrough**: 1-match Training match with Horus — Q/W/E/R + I + Normal Attack + Recall + Item all functional. No S1/S2 bugs surfaced.
8. **EditMode tests** (optional in batch 1; consider for batch 2 onward): `HorusSlotBindingTests.cs` if a per-hero test class pattern is adopted; otherwise covered transitively by existing 116-test `Radius.Tests.Characters` assembly.

## Out of Scope

- **Anansi / Merlin / Cupid** code touches — different stories / different sprints
- **R-23 AdditionalMoveSpeed redesign** — Cupid + Hercules use this; Horus does NOT (verified via grep), so no hold-the-line concern
- **CBS designer fill for SlotQ..SlotI** — if missing, this story files a CBS data ticket; does not block migration code work
- **Loc/UX work for "Horus" canonical name** — code uses "Horus" which matches canonical (no divergence flag)
- **New Phase 3 ADR authoring** — Phase 3 reuses ADR-0006 patterns; no new ADR needed for batch 1

## Test Evidence

**Required**:
- `production/qa/evidence/sprint-006-phase-3-batch1.md` — Horus per-ability checklist + screenshot + BUG-0001 cousin verification note + sign-off
- `production/qa/evidence/S6-03-multipeer.txt` — multipeer Pass #1-5 log capture
- PR description: code audit findings (Pattern A-D counts, hardcoded SkillKey count pre/post)

**Optional**:
- EditMode tests `Assets/UnitTests/TestEditMode/HorusSlotBindingTests.cs` (NEW) if first-mover for per-hero test convention
- Screen recording of cast sequence (especially HorusE + HorusR for BUG-0001 cousin verification)

## Performance Impact

Expected zero — refactor is pure code transformation. No new networked state added (all `BoundSlot` traffic already accounted for in Phase 2 bandwidth budget).

## Files to Modify (anticipated)

- `Assets/GameScripts/Gameplays/Characters/Horus/HorusQAction.cs` (audit + Pattern A-D replacements if applicable)
- `Assets/GameScripts/Gameplays/Characters/Horus/HorusWAction.cs` (same)
- `Assets/GameScripts/Gameplays/Characters/Horus/HorusEAction.cs` (same)
- `Assets/GameScripts/Gameplays/Characters/Horus/HorusRAction.cs` (same)
- `Assets/GameScripts/Gameplays/Characters/Horus/HorusIAction.cs` (same — Skill_I_* path retained on legacy SkillKey per S5-21 F2)
- `production/qa/evidence/sprint-006-phase-3-batch1.md` (NEW or APPEND)
- `production/qa/bugs/BUG-0001-recall-locomotion-stuck.md` (APPEND cousin verification result)

**No new files expected** in delta-unity (helpers, if needed, reuse existing `ActorCombatAction` static helpers from S5-04/S5-05/S5-21).

## Risks

- **R1** — Horus has ability shape not seen in Hercules pilot (e.g., HorusI is 90 lines = largest of the 5; may have unique passive trigger path). **Mitigation**: code audit AC #1 surfaces this early; if unique pattern → document as Pattern 6 candidate in `phase-2-lessons-learned.md` before applying
- **R2** — BUG-0001 cousin (HorusE/HorusR) still reproduces despite AnimatorStateSync fix. **Mitigation**: AC #5 gates this; if reproduces → file BUG-0005, decide block S6-04..06 vs continue
- **R3** — CBS Hero "Horus" record missing SlotQ..SlotI values. **Mitigation**: BindSlot warning logs surface this; story closes with "code ready, awaiting CBS data fill" note if blocked

## References

- [Epic: Phase 3 Hero Migration](EPIC.md) — shared context + pattern shape
- [S5-09 Hercules bootstrap](../../sprints/sprint-005.md) — template migration story
- [Phase 2 Lessons Learned](../../../docs/architecture/phase-2-lessons-learned.md) — 5 patterns
- [BUG-0001 fix PR #357](../../../production/sprint-006-prep/) (delta-unity) — cousin bug list
