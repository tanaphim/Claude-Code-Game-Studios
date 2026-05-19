# Story: S6-05 — Guan Yu Migration (Phase 3 batch 1, hero #3)

**Status**: Complete (2026-05-19)
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
- [Phase 2 Lessons Learned](../../../docs/architecture/phase-2-lessons-learned.md) — Pattern 6 confirmation log + AC #9 sub-pattern note
- [TD-011 — Pattern 6 dedicated tracking](../../../docs/tech-debt-register.md) — 3rd case GuanYuR added by S6-05
- BUG-0001 fix PR #357 — cousin bug list including GuanYuE (VERIFIED CLOSED-BY-SIDE-EFFECT 2026-05-19; batch 1 cousin closure cycle complete)
- [BUG-0010](../../qa/bugs/BUG-0010-guanyu-i-empower-na-chain-timing-race.md) — NEW pre-existing S3 surfaced during AC #8
- S5-21 — TD-006 SetActiveSlot wiring in `Progress` setter (Normal Attack auto-target path fix; GuanYuNAction inherits clean from MeleeNormalAttackAction)

---

## Completion Notes

**Completed**: 2026-05-19 (same-day close — 3rd batch 1 hero same-day-close in a row)
**Velocity**: ~0.5d actual vs 0.5d budget = on target
**Criteria**: 7/9 PASS + AC #6 compile-time PASS + AC #9 explicit PASS; AC #2 partially DEFERRED (Pattern 6 3rd case GuanYuR → TD-011 updated; 3-confirmation threshold sustained, scope expanded to include active abilities not just passives)
**Deviations** (all advisory, in-scope):
- Pattern 6 confirmation log gained 3rd case (GuanYuR active R ability) — first non-passive case; phase-2-lessons-learned.md and TD-011 both updated to reflect scope expansion
- AC #9 sub-system documentation added to phase-2-lessons-learned.md — GuanYu's W/+R/ sub-dirs contain benign helper files (RotateGO visual rotator, GuanYuWOnHit generic trigger detector); pattern documented for batch 2 planning awareness, not promoted to its own Pattern entry
- AC #6 TD-006 cousin gate PASSED at compile-time (GuanYuNAction body entirely commented out — inherits 100% from MeleeNormalAttackAction base; no GuanYu-specific NA override that could bypass `Progress` setter); no manual playtest verification needed for this AC
- BUG-0009 cousin audit on all 8 GuanYu files = clean (pre-emptive, no NRE during playthrough as predicted)
- **One pre-existing S3 bug surfaced**: BUG-0010 — GuanYu I (DETERMINATION lifesteal empower) does not trigger on continuous NA chain (timing race on Networked stack propagation); workaround = walk/cancel then resume; pre-existing, NOT a regression from Sprint 006; deferred to Sprint 007 (P2)
**Test Evidence**:
- Integration playtest documented at [production/qa/evidence/sprint-006-phase-3-batch1.md § S6-05](../../qa/evidence/sprint-006-phase-3-batch1.md)
- Multipeer harness log at [production/qa/evidence/S6-05-multipeer.txt](../../qa/evidence/S6-05-multipeer.txt)
- EditMode tests waived per AC + QA plan (optional in batch 1)
**Code Review**: N/A — no delta-unity code changes in S6-05 (audit-only refactor)
**BUG-0001 cousin verdict**: GuanYuE → CLOSED-BY-SIDE-EFFECT. **Batch 1 cousin closure cycle COMPLETE** (HorusE/R + VolundW + GuanYuE all resolved by S5-19 AnimatorStateSync fix)
**BUG filed**: BUG-0010 (S3 / P2 / deferred Sprint 007)

### Carry-forward (next migrations)

- **S6-06 Skadi** = batch 1 final hero, CONTROL CASE — clean baseline per story description (no BUG-0001 cousin, no R-23, no special variants); should be the cleanest of batch 1
- **Pattern 6 cross-hero scan** still pending — Anansi/Cupid/Merlin passives; can run before S6-06 or opportunistically during S6-06 audit
- **BUG-0010 fix bundling decision** — consider whether to include in Pattern 6 migration story (Sprint 007/008) or schedule separately
