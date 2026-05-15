# Retrospective: Sprint 005

**Period planned:** 2026-05-09 → 2026-05-22 (14 days)
**Period actual:** 2026-05-08 → 2026-05-14 (7 calendar days — closed 1 week early)
**Generated:** 2026-05-15
**Theme:** Phase 2 Hercules pilot + 3 design decisions

---

## 1. Metrics

| Metric | Planned | Actual | Delta |
|---|---|---|---|
| Must Have stories | 10 (incl. S5-21 added mid-sprint) | 10/10 PASS | **100%** ✅ |
| Should Have stories | 3 | 1 done + 2 deferred-w-briefing | 33% landed, 67% deferred-with-handoff |
| Nice to Have | 6 | 1 done (S5-22), 5 backlog | 17% |
| Bugs (filed in sprint) | — | 2/2 RESOLVED (S5-19, S5-20) | 100% |
| Estimated days (Must+Should+Bug) | 7.0d | ~6.0d actual | -1.0d (~14% under) |
| Calendar days used | 14 | 7 | -7 (50% under, closed early) |
| Commits (Delta-Project) | — | ~21 | — |
| Commits (delta-unity) | — | ~92 | — |
| TODO count (Assets/GameScripts) | ≤19 (S4 baseline 14 +5) | 13 | **-1 from S4 baseline ✅** |
| New tech debt surfaced | — | TD-006, TD-007, BUG-0003, BUG-0004 | All TD resolved in-sprint |
| ADRs amended/superseded | — | 1 (ADR-0006 §6.1 → ADR-0008 mid-sprint) | — |

## 2. Velocity Trend

| Sprint | Planned | Completed | Rate |
|---|---|---|---|
| 002 | 12.0d | partial (~50%) | <50% |
| 003 | 9.5d | Phase 1b Must (~9d) | ~95% |
| 004 | 10.0d | 8/8 Must (2.7d actual) | 80% scope, 27% est-burn |
| **005** | **7.0d (Must+Should+Bug)** | **6.0d actual, 10/10 Must + 1/3 Should + 2/2 Bug** | **~100% scope on critical path** |

**Trend:** Steady recovery — Sprint 005 is the first sprint where Must Have closed 100% with estimates within ~15% accuracy. Reverse-doc overestimation problem from S4 fully corrected (real implementation work this sprint, not investigation).

---

## 3. What Went Well

- **Phase 2 Hercules pilot SHIPPED end-to-end** — 10/10 Must Have PASS, 1-week soak in progress. ADR-0006 Phase 2 closed, Phase 3 unblocked. This is the largest single milestone since Sprint 003.
- **Dual-path migration pattern proved robust** — `BoundSlot != 0 ? slot : SkillKey-fallback` shipped across 4 stories (S5-04/05/09/21) with zero legacy regressions. Pattern codified in `phase-2-lessons-learned.md`.
- **Static-helper extraction unlocked EditMode coverage of NetworkBehaviour logic** — 4 pure static helpers (`ResolveIsActiveSlotOwner`, `ResolveLegacySkillKeyButtonMatch`, `ResolveLegacyQuickCast`, `ResolveSlotFromSkillKey`) — 116/116 tests green by sprint end (started ~80).
- **Tech debt caught + resolved in-sprint, not deferred** — TD-006 (SetActiveSlot wiring) and TD-007 (AbilityRegistry service missing) both surfaced *and* resolved within the sprint window. Contrast to S4 where R-20..R-23 were "documented, defer Phase 2."
- **BUG-0001 root-caused at AnimatorStateSync** — forensic diagnosis PR landed before fix PR (#44 → #45 pattern). Likely closes cousin bugs (GuanYuE/HorusE/HorusR/VolundW) — single source-of-truth fix at root.
- **S5-13 Garen variants closed cleanly** — 8 orphan files deleted with 3-evidence verification (no production refs, wrong base, suspicious git history). 4-sprint Known Issues entry resolved.
- **Estimation accuracy substantially improved** — most stories within ±20% of estimate (vs S4: 0/8 within ±20%).

---

## 4. What Went Poorly

- **S5-06 revert + S5-21 v1→v2 fork = lost ~0.5d to rework** — `AnimationEvent` 40-shim migration shipped, broke VFX in playtest (TD-006: SetActiveSlot caller never wired in S5-03), full revert + re-implementation under S5-21 (added mid-sprint, 0.25d est). Within S5-21, v1 placed SetActiveSlot in `OnPressButtons` but Normal Attack auto-target path bypassed it → 30+ slot=0 warnings → v2 moved to `Progress` setter. Two regressions caught only by manual playtest, not by tests.
- **Should Have 2/3 deferred (S5-11 + S5-12)** — Phase 2 closeout (S5-21+S5-22 mid-sprint scope expansion) consumed Should Have capacity. S5-12 is **4th carryover** of AI Bot fate decision (S2→S3→S4→S5→S6) — process rule from Sprint 003 retro #5 now strictly binding.
- **ADR-0006 §6.1 mid-sprint supersession (→ ADR-0008)** — design pivot landed 2026-05-08 (day 0). Forced S5-01 + S5-09 scope rewrite before sprint started. Net cost low (~0.25d sunk on initial CBSAbility.Slot impl), but signals ADR review depth was insufficient before sprint planning.
- **Pre-existing BUG-0003 (NetworkRunnerInput NRE) only got band-aid** — 8 sites null-conditional patched in S5-04 to unblock playtest. Root-cause investigation deferred to Sprint 006. New BUG-0004 (BotActor.WithTarget typo) filed out-of-scope.
- **Carry-forward process items still not done** — S5-14 (5-min existence check rule, sprint 004 retro action #1 → S5 retro action), S5-15 (scene-naming, S3-retro #3), S5-16 (origin/dev merge cadence, S3-retro #4). All Nice-to-Have, all backlog. This is **3rd defer for S3-retro items**.
- **No PlayMode test framework** — NetworkBehaviour-state behaviour (Fusion `[Networked]` writes) still uncovered by automated tests; falls to multipeer harness + manual playtest. Listed in Sprint 005 plan retro-seeds as Sprint 006 candidate.

---

## 5. Blockers Encountered

| Blocker | Duration | Resolution | Prevention |
|---|---|---|---|
| ADR-0006 §6.1 design issue → ADR-0008 supersession | ~0.25d (S5-01 rework day 0) | ADR-0008 authored; S5-01/S5-09 scope updated; reverted code stays on worktree | Stronger ADR review before sprint planning; user-design-review checkpoint in `/sprint-plan` |
| TD-006 (SetActiveSlot caller missing) | ~1.5d (S5-06 revert + S5-21 retry) | S5-21 added mid-sprint; landed Progress-setter fix | API+caller pair audit (codified as lesson #5 in phase-2-lessons-learned.md) |
| TD-007 (AbilityRegistry not in DeltaService.Services) | ~0.5d (caught in S5-10) | Pre-merge fix: created service prefab + added to DeltaConfiguration (12→13 services) | Manifest pattern check before story-readiness gate |
| BUG-0003 (pre-existing NetworkRunnerInput NRE) | blocked S5-04 playtest verification (~0.1d) | 8-site null-conditional band-aid | Root-cause investigation in S6; CI guard candidate |

---

## 6. Estimation Accuracy

| Story | Estimated | Actual | Variance | Likely Cause |
|---|---|---|---|---|
| S5-01..03 (CBSUnit + facades) | 0.75d total | ~0.8d | +7% | On-target; helper extraction added some scope |
| S5-04 Pattern-A | 0.5d | ~0.5d | 0% | On-target |
| S5-05 Pattern-B/C/D | 0.5d | ~0.6d | +20% | Code-review fix (BotActor PressedSlot) added scope |
| S5-06 AnimationEvent | 0.5d | ~1.5d total (incl. revert + S5-21 retry) | **+200%** | Hidden API+caller gap (TD-006) only surfaced in playtest |
| S5-07/08 Hercules R+W rewrites | 0.5d | ~0.4d | -20% | Smaller diff than estimated |
| S5-09 Hercules bootstrap | 0.5d | ~0.6d | +20% | `ISlotBinder` interface added to break asmdef cycle |
| S5-10 Playtest gate | 0.5d | ~0.5d | 0% | On-target; TD-007 fix landed within budget |
| S5-21 TD-006 wiring | 0.25d | ~0.4d | +60% | v1 OnPressButtons → v2 Progress setter rework |
| S5-13 Garen variants | 0.5d | ~0.3d | -40% | 3-evidence verification went fast |
| S5-19 BUG-0001 | 0.5d | ~0.3d | -40% | Forensic diagnosis pre-work made fix targeted |

**Overall:** ~7/10 stories within ±20% of estimate (vs S4: 0/8). **S5-06+S5-21 paired = +60% over** (the dominant outlier). Without the TD-006 surprise, sprint would have been ~5d actual / 5.5d planned = on-target.

---

## 7. Carryover Analysis

| Story | Original Sprint | Times Carried | Reason | Action |
|---|---|---|---|---|
| S5-11 R-21 Item Role | 004 retro action #2 → S5 → S6 | 1 (S5→S6) | Phase 2 closeout consumed Should Have capacity | **S6 Day-1 hard-commit** (briefing prepared, 3-path decision) |
| S5-12 AI Bot fate | S2-09/10 → S3-08/09 → S4-09/10 → S5-12 → S6 | **4 (S2→S3→S4→S5→S6)** | Same as S5-11 | **S6 Day-1 hard-commit** (briefing prepared; S3-retro #5 binding — no further defer) |
| S5-14 (5-min existence check rule) | S4-retro #1 → S5 Nice → backlog | 1 | Nice-to-Have priority, deprioritized vs Phase 2 | S6 process slot |
| S5-15 (scene-naming convention) | S3-retro #3 → S4 → S5 Nice → backlog | 2 | Same | S6 process slot |
| S5-16 (origin/dev merge cadence) | S3-retro #4 → S4 → S5 Nice → backlog | 2 | Same | S6 process slot |
| S5-17 (AbilityMultipeerRunner guard) | S4-P1 → S5 Nice → backlog | 1 | — | S6 candidate |
| S5-18 (AbilityRegistry → Addressables) | S4-P2 → S5 Nice → backlog | 1 | Larger 1.5d scope | S6 candidate |

**Concern:** S5-12 hits the formal no-further-defer threshold. S5-15/S5-16 hit 2-carryover for the second time — process docs structurally deprioritized. Consider bundling all 3 (S5-14/15/16) into a single S6 "process polish" half-day.

---

## 8. Technical Debt Status

- **TODO count (Assets/GameScripts):** 13 (Sprint 004 baseline: 14) — **-1, slight improvement** ✅
- **FIXME count:** 0 (unchanged)
- **HACK count:** 0 (unchanged)
- **TD-006 (SetActiveSlot wiring):** RESOLVED in S5-21
- **TD-007 (AbilityRegistry service):** RESOLVED in S5-10 pre-merge
- **TD-003 (single-slot tracker race):** still open — Phase 3 candidate per S5-21 risk note
- **BUG-0003 root cause:** Sprint 006 investigation
- **BUG-0004 (BotActor msg→msg2 typo):** filed, out-of-scope
- **Risk register:** R-21 still open (S5-11 decision pending); R-22/R-23 ADR-only still pending Phase 2 follow-up; R-20 unchanged

**Trend:** Tech debt healthy — surfaced 4 items, resolved 2 in-sprint, 2 remain tracked.

---

## 9. Previous Action Items Follow-Up (from Sprint 004 retro)

| Action (from S4 retro) | Status | Notes |
|---|---|---|
| 1. 5-min existence check rule codification | **Not started** | S5-14 (Nice-to-Have, backlog). Action item recurs for S6. |
| 2. Item Role Restriction decision (R-21) | **Deferred to S6 Day-1** | Briefing prepared at `production/sprint-006-prep/S5-11-...md` |
| 3. AI Bot fate decision | **Deferred to S6 Day-1 (hard-commit)** | Briefing prepared at `production/sprint-006-prep/S5-12-...md`. 4th carryover. |
| 4. R-22/R-23 schedule in Phase 2 plan | **Partial** — not explicit in S5 plan, but Phase 2 work completed without R-22/R-23 surfacing as blocker | Schedule formally in S6 or Phase 3 prep |
| 5. Confirm 3 Garen variant controllers | ✅ **Done** (S5-13) | Deleted; Known Issues resolved |
| 6. S3-retro carry-forward (scene-naming + origin/dev cadence) | **Not started** | S5-15, S5-16 (Nice-to-Have, backlog). Recurs for S6. |

**3 of 6 closed** (2 with hard-commit-to-S6 briefings). Recurring unaddressed: 5-min existence rule, scene-naming, origin/dev cadence — bundle for S6.

> **Update 2026-05-15** — Sprint 006 S6-08 process polish bundle landed all 3 recurring items:
> - 5-min existence check → [coordination-rules.md § Sprint Planning Rules](../../.claude/docs/coordination-rules.md) (closes S4-retro #1)
> - Scene-naming convention → [coding-standards.md § Asset Naming Conventions](../../.claude/docs/coding-standards.md) (closes S3-retro #3)
> - origin/dev merge cadence → [coordination-rules.md § Branch Hygiene](../../.claude/docs/coordination-rules.md) (closes S3-retro #4)
>
> All 3 doc-only landings totaled 0.75d (within budget). 2-3 sprint carryover pattern broken.

---

## 10. Action Items for Sprint 006

| # | Action | Owner | Priority | Deadline |
|---|---|---|---|---|
| 1 | **S5-12 AI Bot fate decision** — close 4-carryover. No further defer permitted. Briefing ready. | producer + creative-director + tanapol | **CRITICAL** | S6 Day-1 (before any other work) |
| 2 | **S5-11 R-21 Item Role decision** — Path A/B/C per briefing. Decide alongside or before S5-12 (Bot logic depends). | game-designer + gameplay-programmer | High | S6 Day-1 |
| 3 | **Bundle process-polish micro-stories** (S5-14 5-min check rule + S5-15 scene-naming + S5-16 origin/dev cadence) into a single 0.75d slot — break 2-3 sprint carryover pattern. | producer | Medium | S6 first week |
| 4 | **API+caller pair audit pattern** — codify lesson #5 from phase-2-lessons-learned.md as a `/story-readiness` gate (block stories whose AC promises a caller that doesn't exist yet). | lead-programmer | Medium | S6 mid-sprint |
| 5 | **BUG-0003 root-cause investigation** (NetworkRunnerInput NRE — band-aid present in 8 sites). | network-programmer | Medium | S6 buffer |
| 6 | **PlayMode test framework spike** (~1-2d) — minimal NetworkRunner fixture for unit-level NetworkBehaviour coverage. If too expensive, formally adopt static-helper extraction as canonical pattern. | lead-programmer + gameplay-programmer | Low | S6 Nice-to-Have |
| 7 | **Schedule R-22 (stat /100) + R-23 (AdditionalMoveSpeed rename) in Phase 3 plan or dedicated balance pass.** | technical-director + game-designer | Medium | Before Phase 3 kickoff |

---

## 11. Process Improvements

- **Mid-sprint scope insertion protocol** — S5-21 was added mid-sprint without `/sprint-plan` update; documented in commit only. Codify: any mid-sprint story addition requires a 1-paragraph note in the sprint plan + sprint-status.yaml entry. (Already mostly happening — formalize.)
- **ADR design-review gate before sprint planning** — ADR-0006 §6.1 → ADR-0008 supersession on day 0 cost ~0.25d. For any ADR a sprint depends on, run `/architecture-review` (or equivalent design pass) within the week prior to sprint planning.
- **Lessons-learned doc as living artifact** — `phase-2-lessons-learned.md` (S5-22) is the right shape. Convention: append per-sprint patterns at the end of each retro; promote to architecture/lessons docs when 2+ sprints validate a pattern.

---

## 12. Summary

Sprint 005 was a **production milestone success with a sharp tech-debt instructive moment**. Phase 2 Hercules pilot shipped end-to-end with 10/10 Must Have PASS, the dual-path migration pattern is now battle-tested, and estimation accuracy returned to within ±20% on most stories. The single most important lesson — codified in S5-22 as "API+caller pair audit" — is that any story whose AC promises a caller in another module must have that caller existence-verified before story-readiness signoff. The single most important Sprint 006 action: **close S5-12 (AI Bot fate) on Day-1, full stop.** Four-sprint carryover ends now.

---

## 13. References

- [Sprint 005 plan](../sprints/sprint-005.md)
- [Sprint 004 retrospective](sprint-004.md)
- [Phase 2 Lessons Learned](../../docs/architecture/phase-2-lessons-learned.md) — 5 patterns codified
- [S5-11 briefing](../sprint-006-prep/S5-11-r-21-item-role-restriction-briefing.md)
- [S5-12 briefing](../sprint-006-prep/S5-12-ai-bot-fate-briefing.md)
- [Risk Register](../risk-register/risk-register.md)
- ADR-0006 (Phase 2 Migration Plan) — Phase 2 §3 Exit Criteria all PASS
- ADR-0008 (Slot Binding via CBSUnit) — supersedes ADR-0006 §6.1
- Commits: `f883e33..a039d2b` (Sprint 005 window)
