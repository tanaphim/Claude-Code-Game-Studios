# Post-Launch Backlog

**Created**: 2026-05-15 (Sprint 006 S6-01, AI Bot Path B descope)
**Status**: Active backlog — re-prioritize per post-launch live-ops cadence
**Source**: User decisions that descope MVP scope to post-launch slot

วิธีใช้: รายการนี้เก็บงานที่ถูก **descope จาก MVP** เพื่อโฟกัส launch-critical path
แต่ละ entry มี estimate + rationale + re-evaluation trigger เพื่อให้ live-ops
ทีมหยิบกลับมา prioritize ได้

---

## Entries

### S4-09 — AI Bot item-buying

**Original sprint**: Sprint 002 (as S2-09) → S3-08 → S4-09 → S5-12 deferred → **descoped 2026-05-15 (S6-01 Path B)**
**Carryover count when descoped**: 4
**Estimate**: 2.0d (per Sprint 002 plan)
**Dependencies**:
- S5-11 R-21 Item Role Restriction decision (if Path A → bot priority logic is role-aware; if Path B → all items in candidate pool)
- CBS Item data schema stability

**What it adds**:
- Bot shop visit logic — when, how often, what triggers a purchase
- Item priority by role + situation (Tank vs Mage vs Carry profiles)
- Gold management — when to save for legendary vs. buy cheap components

**Why descoped**:
- Bot pipeline is MVP-functional without it (Training mode works, AFK replacement works)
- Item-buying is polish-tier, not core-loop
- Sprint 006 Phase 3 hero migration is the launch-critical path

**Re-evaluation trigger**: Player feedback flags AI bots as top-3 launch complaint OR Training mode bounce-rate data implicates basic-loop bots OR competitive parity becomes a hard requirement.

**See**: [S5-12 decision](../decisions/S5-12-ai-bot-fate.md), [AI Bot GDD §3.7](../../design/gdd/ai-bot-system.md)

---

### S4-10 — AI Bot Difficulty levels

**Original sprint**: Sprint 002 (as S2-10) → S3-09 → S4-10 → S5-12 deferred → **descoped 2026-05-15 (S6-01 Path B)**
**Carryover count when descoped**: 4
**Estimate**: 1.0d (per Sprint 002 plan)
**Dependencies**: S4-09 (item-buying — difficulty scaling tweaks item priority parameters)

**What it adds**:
- Easy / Normal / Hard difficulty slider for Training mode
- Parameter scaling — reaction time, target priority weights, item budget, skill use frequency
- Marketing line: "適切な相手 from beginner to veteran"

**Why descoped**:
- Hardcoded difficulty parameters today are "Normal" — playable for all skill levels
- Live-ops can ship difficulty slider as a Training mode enhancement patch
- 1.0d work doesn't fit S6 capacity once Phase 3 commits

**Re-evaluation trigger**: Same as S4-09 + specifically when Training mode is repositioned as the primary new-player onramp (live-ops content cycle decision).

**See**: [S5-12 decision](../decisions/S5-12-ai-bot-fate.md)

---

## Future candidates (not yet descoped — keep here for visibility)

These are AI Bot features mentioned in `ai-bot-system.md §3.7` as "Bot ยังไม่มี" but were never explicitly storied. If post-launch demand surfaces, file as stories under live-ops:

| Feature | Estimate (rough) | Trigger |
|---|---|---|
| Last-hit Minion farming | TBD (~0.5d?) | Player feedback / competitive parity |
| Bot↔Bot coordination (team-aware) | TBD | Endgame mode polish |
| Objective awareness (Boss/Tower priority) | TBD | Strategic-depth feedback |
| Gank / Roaming | TBD | Late-game pacing feedback |
| Collision Avoidance (code exists, commented) | ~0.5d | Anytime — small effort |

---

## Process notes

- **No further defer** on entries here means "don't move back to MVP sprint without explicit re-evaluation trigger event". Live-ops slots that pick these up are not "defers", they are intentional content cycles.
- **Reversibility**: any entry can be re-promoted to MVP by editing this file and adding a story to a sprint plan. The descope decision (S5-12 = Path B) does NOT lock these out permanently.
- **Carryover ledger closed for AI Bot**: post-launch home eliminates the 4-sprint carryover pattern that triggered Sprint 003 retro action #5.

---

## References

- [S5-12 decision: AI Bot Path B](../decisions/S5-12-ai-bot-fate.md)
- [S5-12 briefing](../sprint-006-prep/S5-12-ai-bot-fate-briefing.md)
- [AI Bot system GDD](../../design/gdd/ai-bot-system.md)
- [Sprint 003 retrospective](../retrospectives/sprint-003.md) — action #5 root-cause-review rule
- [Sprint 005 retrospective](../retrospectives/sprint-005.md) §7 — carryover ledger
