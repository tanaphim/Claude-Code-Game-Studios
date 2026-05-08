# Retrospective: Sprint 004

**Period planned:** 2026-05-16 → 2026-05-29 (14 days)
**Period actual:** 2026-05-07 → 2026-05-08 (2 calendar days)
**Generated:** 2026-05-08
**Theme:** Animator + Bug Hunting + AI Bot batch (carryover from Sprint 002+003)

---

## 1. Metrics

| Metric | Planned | Actual | Delta |
|---|---|---|---|
| Must Have stories | 8 | 8 | 0 (100%) |
| Should Have stories | 2 | 0 | -2 (deferred) |
| Nice to Have | 5 (S4-P1..P5) | 1 (S4-P5) | -4 (deferred) |
| Estimated days | 10.0 (Must 7.0 + Should 3.0) | ~2.7 actual | -7.3 (~73% under) |
| Calendar days used | 14 | 2 | -12 (88% under) |
| Commits | — | 10 | — |
| Risks added to register | — | 4 (R-20..R-23) | — |
| Bugs found | — | 3 documented (R-22, R-23, Garen gap) | — |
| Bugs fixed in code | — | 0 (all defer to Phase 2) | — |

## 2. Velocity Trend

| Sprint | Planned | Completed | Rate |
|---|---|---|---|
| 001 | 10.0d | partial — S1 stories carried | <100% |
| 002 | 12.0d | partial — S2-01..S2-08 carried | <50% |
| 003 | 9.5d | Phase 1b Must (~9d) | ~95% |
| **004** | **10.0d (est)** | **8/8 Must Have (2.7d actual)** | **80% scope, ~27% est-burn** |

**Trend:** Estimation accuracy collapsing for reverse-doc work — Sprint 004
estimates inherited from Sprint 002 plan when stories were assumed to be
implementation work. Actual nature was investigation/documentation of existing
code → 60-100% overestimation per story.

---

## 3. What Went Well

- **Carryover backlog finally closed** — S4-01..S4-08 ค้างมา 3 sprints (since
  Sprint 002 plan). All 8 closed in 2 days. Sprint 005 Phase 2 planning unblocked
  from phantom dependencies.
- **Investigation pattern became repeatable** — 6 of 8 stories followed
  identical workflow: grep targets → read code → confirm/refute hypothesis →
  document with file:line citations → add risk register entry. Pattern is now
  worth codifying as a skill or template.
- **Risk register grew with concrete entries** — R-20 (Mythic schema-only),
  R-21 (Role Restriction unimpl), R-22 (stat /100 designer trap),
  R-23 (AdditionalMoveSpeed override fragile). Every entry has file:line +
  mitigation path. Sprint 005+ has a queue of design decisions to land.
- **Pre-sprint blocker resolved** — Sprint 003 retro action #1 (Photon
  PeerMode root cause) closed in commit `238e3e3` before sprint start →
  no blocker cascade.
- **S4-01 + S4-02 verified pre-existing** — Saved 2.5d of estimated animator
  work. Shared base controller `RadiusBasicLocomotion.controller` had item
  states + `Item_Viable` parameter all along.
- **Documentation density up** — `item-system.md`, `movement-navigation-system.md`,
  `networking-core.md` ทั้ง 3 ได้ §Known Issues + §Tuning Knobs ที่อ้าง file:line
  — designers จะอ่าน code path ตามได้

---

## 4. What Went Poorly

- **Stories carried 3 sprints before anyone verified** — S4-01/S4-02 stat:
  estimated 2.5d, actual 0 work needed. Cost: 3 sprints of false planning
  weight + Phase 2 (Sprint 005) start delay risk. Root cause: Sprint 002
  plan inherited from initial scope estimate without code-existence audit.
- **Bug investigation outcome = "documented, defer Phase 2"** — S4-03, S4-07
  both found real architectural issues but not currently exploitable.
  No code fix possible without balance pass / breaking change → defer ภาระ
  ไป Phase 2 หรือ post-launch. Sprint 004 acceptance "documented" was met
  but no behavior changed.
- **AI Bot deferred 3rd consecutive sprint** — S2-09/10 → S3-08/09 → S4-09/10
  → Sprint 005. Each defer was correct in isolation (Phase 1b priority,
  doc theme purity) but pattern signals AI Bot is structurally
  under-prioritized. Must commit a sprint to it or formally descope.
- **3 Garen variant controllers gap discovered late** — Found during S4-01
  verification that 3 of 25 hero controllers use a different base lacking
  item system. Not added to risk register (judged cosmetic at the time —
  may revise).
- **Sprint window mismatch** — Plan said 2026-05-16 → 05-29. Actual work
  done 2026-05-07 → 05-08, BEFORE the planned start. Suggests the plan
  was written too far in advance for stories that were primarily desk-work.

---

## 5. Blockers Encountered

| Blocker | Duration | Resolution | Prevention |
|---|---|---|---|
| (none in-sprint) | — | — | — |
| Photon PeerMode (carryover from Sprint 003 retro) | resolved 2026-05-07 | `NetworkProjectConfig.fusion` reverted `PeerMode=Multiple → Single`; workflow doc + S4-P5 polish | Add CI check or pre-commit hook flagging PeerMode change |

---

## 6. Estimation Accuracy

| Story | Estimated | Actual | Variance | Likely Cause |
|---|---|---|---|---|
| S4-01 Animator States | 2.0d | ~0.05d | -97% | Already implemented in shared base controller; no one had verified |
| S4-02 Item_Viable | 0.5d | ~0.02d | -96% | Same — parameter pre-existed in base controller |
| S4-06 Mythic reverse-doc | 1.0d | ~0.5d | -50% | Schema reading went faster than worst-case; clear file structure |
| S4-04 Photon Tick Rate | 0.5d | ~0.2d | -60% | One file lookup; convention well-named |
| S4-07 AdditionalMoveSpeed | 1.0d | ~0.7d | -30% | Closest-to-estimate; needed to verify all hero abilities |
| S4-05 Role Restriction | 0.5d | ~0.3d | -40% | Already partially traced in S1 carryover |
| S4-03 stat /100 | 1.0d | ~0.5d | -50% | Stat init code straightforward |
| S4-08 Risk Register | 0.5d | ~0.4d | -20% | Closest to estimate among new authoring |

**Overall:** 0/8 stories within ±20% of estimate. **All overestimated** —
average ~62% under-burn. Pattern: Sprint 002 estimates assumed "implementation"
but actual work was "verify or document existing code." Reverse-doc is
significantly cheaper than greenfield implementation, and the plan didn't
distinguish.

---

## 7. Carryover Analysis

| Story | Original Sprint | Times Carried | Reason | Action |
|---|---|---|---|---|
| S4-01..S4-08 (was S2-01..S2-08) | 002 | 2 (002→003 deferred→004) | Sprint 002 stalled; Sprint 003 prioritized Phase 1b | ✅ Closed Sprint 004 |
| S4-09 (AI Bot item-buy) | 002 (S2-09) | 3 (002→003→004→005) | Always lower priority than Phase 1b/carryover doc | Sprint 005 — commit to it or formally descope |
| S4-10 (AI Bot Difficulty) | 002 (S2-10) | 3 | Depends S4-09 | Same |

**Concern:** AI Bot batch passing 3 carryover thresholds. By process rule
(Sprint 003 retro action #5), root-cause review required before Sprint 005
plans include it. **Decision needed:** is AI Bot actually MVP-required, or
should it move to post-launch?

---

## 8. Technical Debt Status

- **TODO count (Assets/GameScripts):** 14 (no prior baseline tracked)
- **FIXME count:** 0
- **HACK count:** 0
- **Trend:** Establish 14 as Sprint 004 baseline; track in Sprint 005 retro.
- **Risk register:** 23 risks total, 12 Open, 12 Medium severity, 4 High.
  Growing healthy (more visible, not more existing).

---

## 9. Previous Action Items Follow-Up

| Action (from Sprint 003 retro) | Status | Notes |
|---|---|---|
| 1. Photon STUN/PeerMode investigation | ✅ Done pre-sprint (`238e3e3`) | Workflow doc + S4-P5 polish closed |
| 2. AbilityMultipeerRunner duplicate-Start (S4-P1) | Not started | Was Nice-to-Have; defer Sprint 005 if buffer |
| 3. Scene-naming convention doc | Not started | No coordination-rules update — carry forward |
| 4. Frequent `origin/dev` merge cadence | Not tracked | No metric in place — establish in Sprint 005 |
| 5. Sprint 002 carryover root-cause (if slipped again) | ✅ Did not slip — closed | Action complete by virtue of completion |

3 of 5 closed. Action 3 (scene-naming) and Action 4 (merge cadence) carry forward.

---

## 10. Action Items for Sprint 005

| # | Action | Owner | Priority | Deadline |
|---|---|---|---|---|
| 1 | **Before estimating reverse-doc stories, run 5-min existence check** (grep target classes/files; spot-verify implementation status). If found pre-existing → close as "verified" without sprint allocation. | producer + lead-programmer | High | Sprint 005 planning |
| 2 | **Decide on Item Role Restriction** (R-21) — implement gating, or formally remove `Positions[]` from `ItemObject`. Blocks S4-09 AI Bot item priority design. | game-designer + gameplay-programmer | High | Before Sprint 005 mid-point |
| 3 | **Decide AI Bot fate** — commit Sprint 005 or 006 to S4-09/S4-10 with full focus, OR descope to post-launch. 3-sprint carryover requires explicit call. | producer + creative-director | High | Sprint 005 planning |
| 4 | **Schedule R-22 (stat /100) + R-23 (AdditionalMoveSpeed) in Phase 2 plan** — both touch ability migration; align rename/refactor with existing Phase 2 work. | technical-director + game-designer | Medium | During Sprint 005 |
| 5 | **Confirm 3 Garen variant controllers** — production heroes or legacy test? If legacy → delete; if production → swap base or port states. | art-director + lead-programmer | Low | Sprint 005 buffer |
| 6 | **Carry forward from S3-retro:** scene-naming convention doc + `origin/dev` merge cadence metric | producer | Medium | Sprint 005 |

---

## 11. Process Improvements

- **Reverse-doc story template** — codify the grep→read→confirm→doc+risk
  pattern that worked 6× this sprint. Estimation default = 0.5d unless
  greenfield design needed. (Possibly a `/reverse-document` skill enhancement.)
- **Existence check as planning gate** — every story imported from
  pre-existing carryover plan must have a 5-min existence audit before being
  re-estimated. Codify in `/sprint-plan` skill or `coordination-rules.md`.
- **Sprint window flexibility** — when Must Have closes early (>50% calendar
  remaining), trigger automatic retro + Sprint N+1 plan instead of waiting
  for nominal end date. Sprint 004 closed day 2 of planned 14 — 12 wasted
  calendar days if we had waited.

---

## 12. Summary

Sprint 004 was a **bookkeeping success and a planning warning**. We closed
3 sprints of carryover in 2 days because most stories were verifications,
not implementations — but that means the original Sprint 002 estimates were
fundamentally wrong, and we propagated those wrong estimates for 3 sprints.
The single most important change for Sprint 005: **never re-estimate carryover
stories without a 5-minute code existence check first.** The risk register
is the real product of this sprint — Phase 2 + balance pass now have a
queue of file:line-cited targets.

---

## 13. References

- [Sprint 004 plan](../sprints/sprint-004.md)
- [Sprint 003 retrospective](sprint-003.md)
- [Risk Register](../risk-register/risk-register.md)
- ADR-0006 Phase 2 Migration Plan: `docs/architecture/ADR-0006-phase-2-migration-plan.md`
- Commits: `62ef027..0b822a3` (Sprint 004 window)
