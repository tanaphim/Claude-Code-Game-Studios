# Sprint 6 — 2026-05-15 to 2026-05-28

> **Sprint window shifted** (originally 2026-05-23 → 2026-06-05) — Sprint 005 closed 1 week early (10/10 Must Have @ 2026-05-14), so Sprint 006 starts immediately per Sprint 004 retro action "Sprint window flexibility". **Phase 2 soak (ends 2026-05-21) still gates S6-03..S6-06 Phase 3 hero migrations** — non-Phase-3 stories (S6-01, S6-02, S6-08, S6-09, S6-10) can begin now.

## Sprint Goal

**Day-1**: Execute S5-12 (AI Bot fate — **Path B: descope to post-launch backlog**, pre-decided 2026-05-15) + close S5-11 (R-21 Item Role) decision before any implementation begins.
**Main work**: Kick off **Phase 3 hero migration** (target 4-5 heroes using the dual-path pattern proven in Phase 2 Hercules pilot) + bundle 3 carryover process-polish items (S5-14/15/16).

**Theme:** "Phase 3 kickoff + design decision close + process debt sweep"

---

## Capacity

**Sprint window:** 14 calendar days (2026-05-15 → 2026-05-28)
**Phase 2 soak ends:** 2026-05-21 (mid-sprint gate for S6-03..06 only)

### Per-member capacity

| Handle | Role | Calendar days | Velocity × | Effective days |
|--------|------|---------------|------------|----------------|
| `tanapol` | gameplay-programmer (lead, senior) | 14 | 1.0× | 14.0d |
| **Team total (gross)** | | | | **14.0d** |

### Buffer

- Buffer (20% of gross): **2.8d** สำรองสำหรับงานที่ไม่ได้วางแผน + mid-sprint scope additions
- **Available effective: ~11.2d**

### Notes

- Solo sprint (same as S5). Other 11 roster members ใน `production/team.yaml` ยังไม่ได้ assign — เพิ่มกลางทางได้ตาม schema เดิม
- Sprint 005 estimate accuracy ~7/10 within ±20% → trust point estimates, but reserve 20% buffer for TD-006-style surprises
- **AI Bot Path B pre-decided** (2026-05-15) → S6-11 from draft removed; ~3.0d freed for Phase 3 / buffer

---

## Tasks

> **Day-1 protocol**: S6-01 + S6-02 MUST close before any other story starts. No implementation work begins until both decisions are documented per their briefings.

### Must Have (Critical Path)

| ID | Task | Owner (role) | Assignee | Est. Days | Dependencies | Acceptance Criteria |
|----|------|--------------|----------|-----------|-------------|-------------------|
| **S6-01** ✅ | **S5-12 AI Bot fate — execute Path B** (descope to post-launch). 4th carryover closed. See [briefing](../sprint-006-prep/S5-12-ai-bot-fate-briefing.md). **COMPLETE 2026-05-15** — decision doc + post-launch backlog + GDD §3.7 + sprint-003 action #5 all landed. | producer | `tanapol` | 0.5 | — | ✅ Decision doc at `production/decisions/S5-12-ai-bot-fate.md` (Path B); ✅ S4-09 + S4-10 moved to `production/backlog/post-launch.md`; ✅ Sprint 003 retro action #5 marked resolved; ✅ `design/gdd/ai-bot-system.md` §3.7 missing features labeled "Post-Launch" |
| **S6-02** ✅ | **S5-11 R-21 Item Role decision — execute Path B** (remove dead `Role[] Positions` field). **COMPLETE 2026-05-15** — code change landed in delta-unity + ADR-0009 Accepted + R-21 risk register Resolved + GDD updated. | game-designer + gameplay-programmer | `tanapol` | 0.35 (Path B actual; vs 1.0 worst-case budget) | S6-01 | ✅ Decision doc + ✅ ADR-0009 Accepted + ✅ Code: 1-line removal in `ItemObject.cs` + ✅ R-21 → Resolved + ✅ GDD `item-system.md` §Known Issues / §5 / §6 updated. Manual smoke check (Unity Editor) + CBS dashboard column drop = async pre-launch user-side tasks. |
| **S6-03** | **Phase 3 hero #1 migration** (next hero after Hercules) — apply dual-path pattern: ActorCombat.OnStartup BindSlot + ActorCombatAction patterns A-D | gameplay-programmer | `tanapol` | 0.5 | S6-01, S6-02; Phase 2 soak green | Hero playable end-to-end in `scene_game_map.unity`; 0 `BindSlot not registered` warnings; multipeer Pass #1-5 green; EditMode tests pass |
| **S6-04** | **Phase 3 hero #2 migration** | gameplay-programmer | `tanapol` | 0.5 | S6-03 | Same as S6-03 |
| **S6-05** | **Phase 3 hero #3 migration** | gameplay-programmer | `tanapol` | 0.5 | S6-04 | Same |
| **S6-06** | **Phase 3 hero #4 migration** | gameplay-programmer | `tanapol` | 0.5 | S6-05 | Same |
| **S6-07** | **Phase 3 mini-batch playtest gate** — 1-match Training with 1 of the 4 migrated heroes; multipeer harness across all 4; lessons-learned doc appended | gameplay-programmer + qa-tester | `tanapol` | 0.5 | S6-03..S6-06 | Evidence in `production/qa/evidence/sprint-006-phase-3-batch1.md`; lessons-learned doc updated if new patterns surface; bandwidth ≤65 B/s preserved |

**Must Have Subtotal: 4.0d**

**Critical path:** S6-01 → S6-02 → S6-03 → S6-04 → S6-05 → S6-06 → S6-07

### Should Have

| ID | Task | Owner (role) | Assignee | Est. Days | Dependencies | Acceptance Criteria |
|----|------|--------------|----------|-----------|-------------|-------------------|
| **S6-08** ✅ | **Process polish bundle** — S5-14 (5-min existence check rule) + S5-15 (scene-naming convention) + S5-16 (origin/dev merge cadence metric). **COMPLETE 2026-05-15** — 3 rules landed in coordination-rules.md + coding-standards.md; S3-retro #3 + #4, S4-retro #1, S5-retro recurring items all marked closed. | producer | `tanapol` | 0.75 | — | ✅ 5-min check rule in coordination-rules.md § Sprint Planning Rules; ✅ Scene naming in coding-standards.md § Asset Naming Conventions; ✅ Branch hygiene in coordination-rules.md § Branch Hygiene; ✅ retro action closures landed |
| **S6-09** ✅ | **API+caller pair audit gate** for `/story-readiness` skill. **COMPLETE 2026-05-15** — checklist item added under Architecture Completeness with full procedure + 5 test cases covering BLOCKED/PASS/AUTO-PASS scenarios; phase-2-lessons-learned.md Pattern #5 marked "promoted to gate". | lead-programmer | `tanapol` | 0.5 | — | ✅ `/story-readiness` SKILL.md § Architecture Completeness — API+Caller Pair Audit checklist item added; ✅ `.claude/skills/story-readiness/tests/api-caller-audit-test.md` NEW (5 test cases); ✅ phase-2-lessons-learned.md Pattern #5 marked promoted |
| **S6-10** ✅ | **BUG-0003 root-cause investigation**. **COMPLETE 2026-05-15 — Path B accepted** (defensive null-conditional). Root cause = 3 distinct Fusion lifecycle nullable windows (pre-Behavior spawn, mid-respawn, pre-BindSlot wiring); `?.` is correct shape; alternatives (input-readiness gate, post-spawn subscription) are >0.5d work with no runtime benefit. | network-programmer | `tanapol` | 0.5 | — | ✅ BUG-0003 file created at `production/qa/bugs/BUG-0003-network-runner-input-nre.md` (was previously referenced but never filed); ✅ Inline comment in `NetworkRunnerInput.cs` Q() updated (drop "BAND-AID:" + follow-up prefix, replace with concise lifecycle rationale); ✅ Residual concern (DeltaService leading expression unguarded) documented as future-surveillance candidate |

**Should Have Subtotal: 1.75d**

### Nice to Have

| ID | Task | Owner (role) | Assignee | Est. Days | Dependencies | Acceptance Criteria |
|----|------|--------------|----------|-----------|-------------|-------------------|
| **S6-11** | **Phase 3 hero #5 (stretch)** — pull when Must + Should close on schedule | gameplay-programmer | `tanapol` | 0.5 | S6-06 | Same as S6-03 |
| **S6-12** | **Phase 3 hero #6 (stretch)** — second stretch hero, only if velocity ≥1.0× hero/day proven | gameplay-programmer | `tanapol` | 0.5 | S6-11 | Same |
| **S6-13** ✅ | **PlayMode test framework spike**. **COMPLETE 2026-05-15 — WORKING BRANCH** (Unity Test Runner: 4/4 smoke tests PASS). Sprint 007 follow-up filed (S7-PROPOSED-PLAYMODE-SCALING) to scale into first domain NetworkBehaviour coverage. 3 asmdef config rounds documented as lesson #6 (Fusion.Runtime.dll + Fusion.Common.dll + Fusion.Log.dll + `using Assert = NUnit.Framework.Assert;` alias). Burned ~1.0d total (research + authoring + 3 compile-error iterations) vs 1.5d budget. | lead-programmer + gameplay-programmer | `tanapol` | 1.5 (budget) / ~1.0 (used) | — | ✅ NetworkRunnerFixture.cs + NetworkRunnerSmokeTest.cs (4 tests covering IsRunning, LocalPlayer, IsServer, TickOnce — all PASS); ✅ UnitTestPlayMode.asmdef updated (3 Fusion DLLs added); ✅ Decision doc updated with working-branch closure + Sprint 007 follow-up; ✅ 6 risks documented in decision doc (#6 = asmdef gotchas with 3-round chronicle for future reference) |
| **S6-14** | **S5-17** AbilityMultipeerRunner duplicate-Start guard (was S4-P1) | network-programmer | `tanapol` | 0.5 | — | Multipeer harness Console clean (no GameIsFull cascade); guard logic + 1 EditMode test |
| **S6-15** ✅ | **R-22 / R-23 schedule call**. **COMPLETE 2026-05-15 — DEFERRED TO SPRINT 008 BALANCE PASS** for both. Phase 3 (S6+) focuses on slot binding, not item balance / field rename. Sprint 008 bundles both → amortizes playtest re-tune cost. Hold-the-line rule stays for R-23 through Sprint 007. | technical-director + game-designer | `tanapol` | 0.25 | — | ✅ Risk register R-22 + R-23 status updated with target sprint 008 + schedule rationale + pre-launch escalation trigger; ✅ Hold-the-line enforcement note added to R-23 (Phase 3 hero migrations must not write `AdditionalMoveSpeed`) |
| **S6-B1** ✅ | **BUG-0004** — typo `msg`→`msg2`. **COMPLETE 2026-05-15** — fixed at both `BotActor.cs:716` AND newly-discovered cousin `ActorDungeon.cs:401` (S5-05 found only the first; S6-B1 investigation surfaced the copy-pasted twin). | gameplay-programmer | `tanapol` | 0.25 | — | ✅ 2 × 1-char fix (msg→msg2) in delta-unity; ✅ NEW production/qa/bugs/BUG-0004-bot-withtarget-msg-typo.md filed; ✅ no EditMode test added (over-engineering for 1-char fix — flagged as future tooling story candidate) |

**Nice to Have Subtotal: 3.5d** (pull only ถ้า Must + Should เสร็จเร็ว)

**S5-18** (AbilityRegistry → Addressables, 1.5d) — **deferred to Sprint 007** (pairs with content-loading theme, doesn't fit S6 Phase 3 hero theme).

---

## Capacity Reconciliation

| Bucket | Days |
|---|---:|
| Must Have (S6-01..07) | 4.0d |
| Should Have (S6-08..10) | 1.75d |
| **Committed total** | **5.75d** |
| Available effective | 11.2d |
| **Slack for Nice / buffer** | **~5.45d** |

Plenty of headroom to pull S6-11/S6-12 stretch heroes if velocity holds. Path B descope frees capacity for Phase 3 depth rather than AI Bot polish — matches Sprint 005 retro recommendation.

---

## Carryover from Previous Sprint

### Accepted into Sprint 006

| Task | Reason | New Estimate |
|------|--------|--------------|
| **S5-11 R-21 Item Role decision** (1st carryover, briefing prepared) | Phase 2 closeout consumed S5 Should Have capacity | 1.0d (Path A worst-case) → S6-02 |
| **S5-12 AI Bot fate decision** (4th carryover, hard-commit) — **Path B pre-decided 2026-05-15** | Same | 0.5d → S6-01 (execute Path B deliverables) |
| **S5-14** 5-min existence check rule (S4-retro #1, 1st carryover) | S5 Nice deprioritized vs Phase 2 | 0.25d → S6-08 (bundled) |
| **S5-15** Scene-naming convention (S3-retro #3, 2nd carryover) | Same | 0.25d → S6-08 (bundled) |
| **S5-16** origin/dev merge cadence metric (S3-retro #4, 2nd carryover) | Same | 0.25d → S6-08 (bundled) |
| **S5-17** AbilityMultipeerRunner guard (S4-P1, 1st carryover) | Lower priority than Phase 2 | 0.5d → S6-14 (Nice) |

### Descoped to post-launch backlog (S6-01 Path B)

| Task | Original Sprint | Carryover count | Disposition |
|------|------|---:|------|
| **S4-09** AI Bot item-buying | S2-09 → S3-08 → S4-09 → S5-12 → **post-launch** | 4 | `production/backlog/post-launch.md` |
| **S4-10** AI Bot Difficulty levels | S2-10 → S3-09 → S4-10 → S5-12 → **post-launch** | 4 | `production/backlog/post-launch.md` |

### Deferred to Sprint 007+

| Task | Reason |
|------|--------|
| **S5-18** AbilityRegistry → Addressables (1.5d) | Pairs with content-loading theme; doesn't fit S6 Phase 3 hero theme |
| **R-22 stat /100** + **R-23 AdditionalMoveSpeed rename** (implementation only — schedule call in S6-15) | Dedicated balance pass slot needed |

---

## Risks

| Risk | Probability | Impact | Mitigation |
|------|-------------|--------|------------|
| Phase 3 hero migration surfaces new TD-006-style API+caller gaps | Medium | Medium | S6-09 (API+caller audit gate) lands early in sprint; phase-2-lessons-learned pattern 5 explicitly screens for this |
| Phase 2 soak (ends 2026-05-21) surfaces Hercules regression | Low | High | Soak window 7 days; if regression filed → S6 plan absorbs as P0 bug (pre-empts Phase 3 kickoff) |
| S6-02 designer call diverges from briefing recommendation (Path A chosen instead of Path B) | Low | Medium | With AI Bot descoped, Path A loses its strongest justification (role-aware bot priority no longer needed). Worst-case 1.0d budgeted; designer call documented either way |
| S6-13 PlayMode framework spike eats Nice-to-Have budget | Medium | Low | Time-box at 1.5d; if no working prototype by half, abort with "static-helper canonical" decision |
| BUG-0003 root cause more complex than band-aid suggests | Medium | Low | Time-box S6-10 at 0.5d; if root cause expands → "accept band-aid + file follow-up" decision |
| Per-hero velocity slower than 0.5d (Phase 2 surprise compounding) | Medium | Medium | S6-07 batch playtest gate after 4 heroes catches systemic issues; if avg >0.6d/hero → reduce S6-11/12 stretch goals, log in retro |
| Player feedback in post-launch criticizes basic AI bots (Path B consequence) | Medium | Low | Process-level: documented in Path B decision; live-ops post-launch slot reserved for bot tuning patches |

---

## Dependencies on External Factors

- **Phase 2 soak verdict (2026-05-21)** — Hercules must complete 1-week soak with no slot-related regressions before Phase 3 kickoff per ADR-0006 §10
- **creative-director / game-designer availability on Day-1** — S6-02 requires designer input (S6-01 Path B already user-decided)
- **PlayFab CBS dashboard access** — if S6-02 chooses Path A, designer fills 158 items × Positions[] async
- **Multipeer harness** — runs after each Phase 3 hero migration as regression check (same pattern as S5)

---

## Definition of Done for this Sprint

### Per-Task Gates

- [ ] All Must Have tasks (S6-01..S6-07) completed
- [ ] S6-01 Path B executed: S4-09/S4-10 in post-launch backlog; Sprint 003 retro action #5 closed; GDD §3.7 updated
- [ ] S6-02 ADR-0009 status=Accepted; R-21 risk register closed
- [ ] Phase 3 batch 1 (4 heroes) all playable in production scene
- [ ] All tasks pass acceptance criteria
- [ ] **QA plan exists** (`production/qa/qa-plan-sprint-006.md`) — required for Phase 3 production work
- [ ] Logic stories have passing EditMode tests
- [ ] Phase 3 batch playtest evidence in `production/qa/evidence/sprint-006-phase-3-batch1.md`
- [ ] Smoke check passed (`/smoke-check sprint`)
- [ ] QA sign-off report: APPROVED or APPROVED WITH CONDITIONS
- [ ] No S1 or S2 bugs in migrated heroes' core abilities
- [ ] Code reviewed and merged

### Cross-cutting

- [ ] No new TODO/FIXME accumulation > +5 from S5 baseline (current: 13)
- [ ] Risk register updated if Phase 3 reveals new risks
- [ ] Sprint 006 retrospective generated at sprint end
- [ ] Phase 2 lessons-learned doc appended if new patterns emerge (S6-07)
- [ ] All 7 Sprint 005 retro action items addressed (closed, deferred, or in-progress)

---

## Source

- [Sprint 005 retrospective](../retrospectives/sprint-005.md) — 7 action items
- [S5-11 briefing](../sprint-006-prep/S5-11-r-21-item-role-restriction-briefing.md)
- [S5-12 briefing](../sprint-006-prep/S5-12-ai-bot-fate-briefing.md) — Path B pre-decided 2026-05-15
- [Phase 2 lessons learned](../../docs/architecture/phase-2-lessons-learned.md) — 5 patterns for Phase 3 prep
- [ADR-0006 Phase 2 Migration Plan §10](../../docs/architecture/ADR-0006-phase-2-migration-plan.md) — Phase 3 entry gate
- [Risk Register](../risk-register/risk-register.md) — R-21..R-23

## Pre-Sprint Decisions

**2026-05-15 — S5-12 AI Bot fate: Path B (descope to post-launch)** — User-decided during Sprint 006 planning. S4-09 + S4-10 → `production/backlog/post-launch.md`. Frees ~3.0d Sprint 006 capacity for Phase 3 hero migration depth. Closes 4-carryover ledger (S2→S3→S4→S5→S6) per Sprint 003 retro action #5. Full execution + retroactive doc updates land in S6-01 on Day-1 of sprint.
