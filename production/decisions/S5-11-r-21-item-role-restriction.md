# Decision: S5-11 — R-21 Item Role Restriction (Path B: Remove Dead Field)

**Date**: 2026-05-15
**Sprint**: 006 (S6-02, Day-1 alongside S6-01)
**Decision-makers**: tanapol (gameplay-programmer + project owner)
**Status**: ✅ ACCEPTED
**Risk closure**: R-21 → Resolved

---

## Decision

**Path B** — Remove the dead `Role[] Positions` field from `ItemObject` permanently. No `AvailableToPurchase()` gate is added; commented-out UI references in `UIInGameShopView.cs` are left as-is (already inert, can be cleaned in future tech-debt sweep — Path B scope is the field itself, not the dead UI loops that happen to reference it).

## Rationale

1. **Field was dead code in production** — S4-05 reverse-doc (2026-05-08) confirmed:
   - `NetworkHeroInventory.AvailableToPurchase()` (`Assets/GameScripts/.../NetworkHeroInventory.cs:1045-1158`) never checks `Positions`
   - UI references in `UIInGameShopView.cs` (lines 521-526, 750, 792) were all commented out (display only, never were purchase gates)
   - No other read sites of `Positions` in `Assets/GameScripts`
   - CBS data confirmed empty (`Positions: 00` default across ~158 production items — designer never filled it)

2. **AI Bot descope (S6-01 Path B) eliminates the strongest case for Path A** — the briefing's #1 Path A justification was "unblocks S4-09 AI Bot item-buying with role-aware priority". With AI Bot descoped to post-launch, role-aware bot logic is no longer needed → Path B becomes the strict-superior choice.

3. **Cleanest match for MVP launch** — fewer moving parts, no designer toil (158 items × Positions[] fill avoided), no player friction risk ("why can't my Tank buy this item?"), no maintenance overhead of another `AvailableToPurchase()` gate.

4. **Aligns with phase-2-lessons-learned Pattern #5 (API+caller pair audit)** — Positions[] was a field declared without a working caller (the "phantom API" anti-pattern). Removing it codifies the lesson.

5. **Reversible** — if post-launch feedback demands build constraints by role, the field can be re-added (1 line + AvailableToPurchase gate + UI badge ≈ 0.5d). Not a one-way door.

## What changed in code

**Single change** in `delta-unity` repo:

- `Assets/GameScripts/Resources/ItemObject.cs:21` — removed `public Role[] Positions;`

**Explicitly NOT changed** (out of Path B scope, deferred to future tech-debt sweep):

- `Assets/GameScripts/UI/GameViews/UIInGameShopView.cs` commented blocks at lines 506-535, 744-759, 786-800 — still reference the now-removed `Positions` field, but the blocks have been commented since pre-Sprint-002. They're inert dead code; leaving them as-is keeps the diff minimal. Future cleanup ticket can sweep them.

## Trade-offs accepted

- **Loses build-constraint design depth** — role-locked item builds are not a launch feature. If post-launch player feedback or competitive analysis demands them, re-add via new ADR.
- **Existing `.asset` serialized data becomes orphan** — `ItemObject` `.asset` files in `Assets/Resources/Settings/.../` may have serialized `Positions: [...]` arrays. Unity will log warnings on next asset load ("missing field 'Positions' on ItemObject"), then re-save the asset clean. **No NRE risk** (Unity skips unknown fields by default).
- **CBS schema** — PlayFab dashboard `CBSItem` may still have a `Positions` column. **Schema migration ticket** noted: drop `Positions` column from `CBSItem` in PlayFab CBS dashboard. Non-blocking for code (CBS deserialization tolerates extra fields), but should be done before launch to avoid designer confusion. **Owner**: tanapol async, before launch.

## Alternatives considered

- **Path A** (implement gating, 1.0d + 158-item designer fill) — **Rejected**: AI Bot descope removes the strongest Path A justification; designer toil + new gate maintenance > MVP value.
- **Path C** (soft hint badge, 0.5d) — **Rejected**: leaves field with ambiguous semantics → invites future re-decision; risk of carryover #2.

See [S5-11 briefing](../sprint-006-prep/S5-11-r-21-item-role-restriction-briefing.md) for full Path discussion.

## Execution deliverables (S6-02)

| # | Item | Status |
|---|------|--------|
| 1 | This decision document | ✅ Created |
| 2 | ADR-0009 (Status: Accepted) | ✅ Created at `docs/architecture/ADR-0009-item-role-restriction.md` |
| 3 | Code change: remove `Role[] Positions` from `ItemObject.cs` | ✅ Landed in delta-unity (1 line) |
| 4 | Risk Register R-21 → Resolved | ✅ Updated |
| 5 | `design/gdd/item-system.md` §Known Issues + §6 — mark R-21 resolved | ✅ Updated |
| 6 | `production/sprints/sprint-006.md` + `sprint-status.yaml` — S6-02 → done | ✅ Updated |
| 7 | Smoke check (manual — Unity Editor) | ⚠️ User-side: confirm items still purchase normally + no serialization NRE on `.asset` reload |
| 8 | CBS schema migration ticket — drop `Positions` column | ⚠️ Async — tanapol before launch (non-blocking) |

## Re-evaluation triggers

Re-open this decision if any of the following happen:
- Player feedback flags "all roles buy all items" as a top complaint
- Competitive analysis shows role-locked items become a hard parity expectation
- Live-ops content demands role-specific build paths

---

## References

- [S5-11 briefing](../sprint-006-prep/S5-11-r-21-item-role-restriction-briefing.md)
- [ADR-0009](../../docs/architecture/ADR-0009-item-role-restriction.md)
- [S5-12 decision (AI Bot Path B)](S5-12-ai-bot-fate.md) — sibling decision; descope removed Path A justification
- [Sprint 006 plan](../sprints/sprint-006.md) — S6-02 execution story
- [Item System GDD](../../design/gdd/item-system.md) §Known Issues — R-21 marked resolved
- [Risk Register R-21](../risk-register/risk-register.md)
- [Phase 2 Lessons Learned](../../docs/architecture/phase-2-lessons-learned.md) — Pattern #5 (API+caller pair audit)
- S4-05 reverse-doc finding (2026-05-08) — original evidence Positions is dead code
