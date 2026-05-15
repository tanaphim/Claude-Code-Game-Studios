# Decision: S5-12 — AI Bot Fate (Path B: Descope to Post-Launch)

**Date**: 2026-05-15
**Sprint**: 006 (Day-1 decision, pre-decided during sprint planning 2026-05-15)
**Decision-makers**: tanapol (producer + project owner)
**Status**: ✅ ACCEPTED
**Carryover ledger closure**: S2-09/10 → S3-08/09 → S4-09/10 → S5-12 → **Post-Launch backlog** (4-carryover chain ends here)

---

## Decision

**Path B** — Descope AI Bot enhancement work (S4-09 item-buying, S4-10 Difficulty levels) to the post-launch backlog. The current MVP bot pipeline (11-state machine + Fuzzy Utility AI + AFK replacement) ships as-is for launch.

## Rationale

1. **Bot pipeline is MVP-functional today** (per `design/gdd/ai-bot-system.md` §3.1) — Training mode is playable, AFK replacement works, bots cast skills sensibly. No P0 bot bugs filed in Sprint 005.
2. **Sprint 006 Phase 3 hero migration is the launch-critical path** — descope frees ~3.0d that goes directly to Phase 3 depth (potential 4 → 6 heroes in S6, faster Phase 3 completion overall).
3. **Item-buying + difficulty levels are polish, not core loop** — they enhance perceived quality but the loop works without them.
4. **Post-launch live-ops slot is the natural home** for bot tuning patches; cadence already accommodates this kind of iterative improvement.
5. **Honors Sprint 003 retro action #5 spirit** — explicit descope decision is exactly the "root-cause review" the rule called for. Pattern: when a story carries past 3 sprints, force a yes/no decision rather than a 4th defer.

## Trade-offs accepted

- **New-player onboarding may feel rougher** than League / Mobile Legends, both of which ship with item-buying bots. Mitigation: Training mode is already an onramp; live-ops can patch this within the first 1-3 post-launch patches.
- **AFK bot replacement stays "basic cast loop"** — teammates of AFK players may notice. Mitigation: same as above (live-ops slot reserved).
- **3+ sprints of historical sprint plans contained this work** — descoping is a process correction visible in retros. Accepted; this is the correct call and the retros document why.

## Alternatives considered

- **Path A** (commit S6 full focus, 3.0d) — locks Phase 3 to slower pace; trades Phase 3 polish for AI Bot polish. **Rejected**: Phase 3 is the higher launch-blocker.
- **Path C** (cherry-pick 1-2 features like last-hit farming) — middle ground but high risk of scope creep ("we picked X but it grew"). **Rejected**: too easy to slip into 5th carryover with a different name.

See [S5-12 briefing](../sprint-006-prep/S5-12-ai-bot-fate-briefing.md) for the full discussion structure that preceded this call.

## Execution deliverables (S6-01)

| # | Item | Status |
|---|------|--------|
| 1 | This decision document | ✅ Created |
| 2 | S4-09 + S4-10 moved to `production/backlog/post-launch.md` | ✅ Created |
| 3 | `design/gdd/ai-bot-system.md` §3.7 — missing features labeled "Post-Launch" | ✅ Updated |
| 4 | `production/retrospectives/sprint-003.md` action #5 → resolved (1-line note) | ✅ Updated |
| 5 | `production/sprints/sprint-006.md` + `sprint-status.yaml` — S6-01 status → done | ✅ Updated |

## Reversibility

**Reversible at any post-launch patch**. S4-09 + S4-10 specs in `post-launch.md` retain estimates + dependency notes. Re-prioritization just requires a sprint-plan inclusion.

## Re-evaluation triggers

Re-open this decision if any of the following happen post-launch:
- Player feedback flags AI bots as a top-3 launch complaint
- Competitive analysis shows item-buying bots become a hard parity expectation
- Training mode usage data shows new players bouncing off the basic-loop bots specifically
- Live-ops cadence finds an idle sprint slot where bot work fits without displacing higher-priority content

---

## References

- [S5-12 briefing](../sprint-006-prep/S5-12-ai-bot-fate-briefing.md)
- [Sprint 003 retrospective](../retrospectives/sprint-003.md) — action #5 (root-cause review rule)
- [Sprint 004 retrospective](../retrospectives/sprint-004.md) §7 — carryover analysis (3-sprint AI Bot pattern flagged)
- [Sprint 005 retrospective](../retrospectives/sprint-005.md) §7 — 4-carryover hard-commit note
- [Sprint 006 plan](../sprints/sprint-006.md) — S6-01 execution story
- [AI Bot system GDD](../../design/gdd/ai-bot-system.md) §3.7 — feature labels updated
- [Post-launch backlog](../backlog/post-launch.md) — S4-09 + S4-10 home
