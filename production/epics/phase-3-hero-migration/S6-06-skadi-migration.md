# Story: S6-06 — Skadi Migration (Phase 3 batch 1, hero #4 — CONTROL CASE)

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

- ✅ S6-05 closed (sequential — completes 3 BUG-0001 cousin migrations before clean control case)
- ✅ All Phase 2 infrastructure landed (see EPIC.md)
- ⏳ Phase 2 soak green (ends 2026-05-21)
- ✅ CBS data: Skadi — verified Hero record exists; `.asset` file at `Assets/GameData/Heroes/Skadi.asset`

## Context

Skadi is the **control case** for Phase 3 batch 1:

- **5 abilities (Q W E R + I)** — same shape as Horus/Volund
- **NO BUG-0001 history** — not in PR #357 cousin list
- **Smallest combined codebase** of batch 1: ~160 lines total
- **NO R-23 AdditionalMoveSpeed usage** (verified via grep)
- **NO unusual ability variants** (no A like Mehmed, no 3-stance like Merlin, no sub-system like GuanYu)
- **Has `.asset` file** at `GameData/Heroes/Skadi.asset` — production data confirmed present

Code lives at:

- `Assets/GameScripts/Gameplays/Characters/Skadi/SkadiQAction.cs` (52 lines)
- `Assets/GameScripts/Gameplays/Characters/Skadi/SkadiWAction.cs` (22 lines)
- `Assets/GameScripts/Gameplays/Characters/Skadi/SkadiEAction.cs` (38 lines)
- `Assets/GameScripts/Gameplays/Characters/Skadi/SkadiRAction.cs` (21 lines)
- `Assets/GameScripts/Gameplays/Characters/Skadi/SkadiIAction.cs` (27 lines)

**Why control case matters**: Skadi has none of the complications of the 3 cousin heroes. If S6-03/04/05 take longer than estimated (BUG-0001 verification side-quests, GuanYu sub-systems), Skadi serves as a **clean velocity datapoint** — confirms the Phase 3 migration recipe works for a hero with no quirks. This baseline informs batch 2 sprint planning estimates.

## Acceptance Criteria

1. **Code audit complete**: scan all 5 Skadi action files. **Expect LOWEST hardcoded-SkillKey count** of batch 1 (smallest total LOC, no unusual abilities). Document findings; if findings differ significantly from expectation → investigate why.
2. **Pattern replacements applied** (Patterns A-D) where applicable. Dual-path fallback retained.
3. **`ActorCombat.OnStartup` BindSlot pipeline verified for Skadi**: 0 `BindSlot not registered` warnings.
4. **`StateReleaseSlot` routing verified**: 0 `slot=0` warnings. VFX/SFX correct on Q/W/E/R + I.
5. **Multipeer harness Pass #1-5 green** with Skadi loaded; bandwidth ≤65 B/s preserved.
6. **Manual playthrough**: 1-match Training with Skadi — all 5 abilities + Normal Attack + Recall + Item.
7. **Velocity baseline recorded**: actual time taken (start → close) logged in evidence doc. Compared to Horus/Volund/GuanYu times → produces a velocity datapoint for batch 2 estimation. If Skadi takes ≥0.6d (vs 0.5 estimate) → flag in S6 retrospective as estimate-needs-update signal.

## Out of Scope

- BUG-0001 cousin verification (Skadi NOT in cousin list — covered by S6-03/04/05)
- Anti-pattern discovery (Skadi is the clean baseline; any new patterns found should NOT block this story — file as Phase 3 follow-up if surfaced)
- Velocity comparison analytics beyond the 1-line baseline note (deeper analysis is Sprint 006 retrospective scope)

## Test Evidence

**Required**:
- `production/qa/evidence/sprint-006-phase-3-batch1.md` — Skadi section (append) with per-ability checklist + sign-off + **velocity baseline note** (actual time elapsed)
- `production/qa/evidence/S6-06-multipeer.txt` — multipeer Pass #1-5
- PR description: pattern counts (expect baseline-low) + velocity note

**Optional**:
- EditMode tests `Assets/UnitTests/TestEditMode/SkadiSlotBindingTests.cs`

## Performance Impact

Expected zero. Skadi has smallest LOC of batch 1 — performance variance from migration is negligible.

## Files to Modify (anticipated)

- 5 × Skadi action `.cs` files
- `production/qa/evidence/sprint-006-phase-3-batch1.md` (APPEND Skadi section with velocity note)

**Expected diff size**: smallest of batch 1 (Skadi total ~160 lines, replacement footprint should be <20 lines).

## Risks

- **R1 — Lower than expected**: Skadi's "clean baseline" assumption is wrong; hero has an undocumented quirk surfaced during audit. **Mitigation**: AC #1 surfaces this; if quirk found → document in `phase-2-lessons-learned.md`, may bump batch 2 estimates
- **R2** — Velocity datapoint surprise: Skadi takes >0.6d despite being the cleanest case. **Mitigation**: AC #7 logs this for retrospective; signal that batch 2 stories need re-estimation before Sprint 007 plan locks
- **R3** — `.asset` data divergence: `Skadi.asset` exists but CBS data doesn't match (offline `.asset` vs PlayFab CBS source of truth). **Mitigation**: BindSlot warning logs surface mismatch; data ticket if needed

## References

- [Epic: Phase 3 Hero Migration](EPIC.md)
- [S6-05 GuanYu migration](S6-05-guanyu-migration.md) — sequential predecessor
- [Phase 2 Lessons Learned](../../../docs/architecture/phase-2-lessons-learned.md)
- `Assets/GameData/Heroes/Skadi.asset` — production data file
