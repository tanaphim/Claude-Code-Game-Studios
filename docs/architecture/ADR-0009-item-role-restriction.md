# ADR-0009 — Item Role Restriction: Remove Dead Field

**Date**: 2026-05-15
**Status**: Accepted
**Supersedes**: none
**Superseded by**: none
**Related**: ADR-0005 (Item Animation Type Routing), [S5-11 decision](../../production/decisions/S5-11-r-21-item-role-restriction.md)
**Risk register**: closes R-21

---

## Context

`ItemObject` (`Assets/GameScripts/Resources/ItemObject.cs`) declared a `Role[] Positions` field intended to restrict which hero roles could purchase each item. Reverse-doc audit S4-05 (2026-05-08) revealed the field was **declared but never enforced**:

- `NetworkHeroInventory.AvailableToPurchase()` (`.../NetworkHeroInventory.cs:1045-1158`) gates purchases on Money, Mythic max-1, Boots tier, Epic-Legendary recipe, Potion at-base, and inventory-full rules — but never checks `Positions`.
- Three UI references in `Assets/GameScripts/UI/GameViews/UIInGameShopView.cs` (lines 521-526, 750, 792) all commented out — display only, never purchase gates.
- No other read sites in `Assets/GameScripts`.
- Production CBS data: `Positions: 00` (empty default) across ~158 items — designer never populated it.

The field was effectively a phantom API: declared without a working caller. This anti-pattern is what Phase 2 Lessons Learned Pattern #5 (API+caller pair audit) was created to catch going forward.

R-21 was filed (Sprint 004) to force a decision before designers populated `Positions[]` expecting enforcement and then discovered playtest does nothing.

## Decision

**Remove the `Role[] Positions` field from `ItemObject`.** No purchase-time role restriction, no Shop UI role filter, no role-aware item priority. All hero roles can purchase all items — matches current de-facto behavior (no behavioral change for players or designers).

## Consequences

### Positive

- **Eliminates a known phantom API** — directly resolves R-21
- **No designer toil** — avoided ~158 items × manual Positions[] fill in PlayFab CBS dashboard
- **Cleaner code path** — `AvailableToPurchase()` keeps current 5 gates instead of 6
- **MVP launch friendly** — fewer moving parts; matches "ship what works" philosophy
- **Reversible** — re-adding the field + gate + UI badge is ~0.5d if post-launch feedback demands it

### Negative

- **Lost build-constraint design depth** — role-locked item builds (a depth lever some MOBAs use) is not a launch feature
- **Sprint 006 S6-02 capacity spent on the cleanup** rather than feature work, but Path B (0.35d) is the lowest-cost path among the 3 options
- **Asset warnings on first load** — existing `ItemObject` `.asset` files in production may have serialized `Positions: [...]` arrays; Unity logs "missing field 'Positions' on ItemObject" warning on next load then re-saves cleanly. **No NRE risk** (Unity skips unknown fields by default).
- **CBS dashboard cleanup pending** — `CBSItem` schema in PlayFab dashboard may still have a `Positions` column. Non-blocking (CBS deserializer tolerates extra fields) but should be dropped before launch.

### Neutral

- No impact on networking, replication, or save/load (purchase decisions are server-authoritative; no `Positions` state was ever transmitted)
- No impact on AI Bot logic (S6-01 Path B descoped item-buying entirely)
- No impact on Hero System's `Role` enum — that field stays in `Hero` for other uses (UI display, matchmaking role tags, future analytics)

## Alternatives considered

### Path A — Implement gating

Add `AvailableToPurchase()` check: `item.Positions.Length == 0 || item.Positions.Contains(hero.Role) || item.Positions.Contains(Role.All)`. Activate the 3 commented Shop UI blocks for badges and filtering. Designer fills `Positions[]` on all 158 production items.

**Estimate**: 1.0d code + 0.25d designer toil = ~1.25d
**Rejected because**: AI Bot descope (S6-01 Path B) removed the strongest justification (role-aware bot priority). Adds maintenance burden + player friction without compensating gameplay value at MVP.

### Path C — Soft hint only

Keep `Positions` field, no purchase gate, activate Shop UI as "recommended for [Role]" badge only.

**Estimate**: 0.5d
**Rejected because**: leaves field with ambiguous semantics (declared but soft) — invites re-decision in a future sprint as designers ask "does this actually do anything?". High carryover-#2 risk; doesn't fix R-21 root cause (phantom API).

## Implementation

**Single source-level change** in `delta-unity` repo at commit boundary 2026-05-15 (S6-02):

```diff
 public class ItemObject : BaseResource
 {
     public StatValue[] Stats;
-    public Role[] Positions;
     public ItemType ItemType;
 }
```

**Out-of-scope cleanup** (left as-is for future tech-debt sweep):

- 3 commented blocks in `UIInGameShopView.cs` that reference `itemObject.Positions` remain inert (they've been commented since pre-Sprint-002; removing them is broader scope creep into "dead UI code cleanup" theme).

**Manual smoke test** (user-side, Unity Editor):

1. Open Unity project, observe Console on initial asset reimport — expect warnings ("missing field 'Positions' on ItemObject"), no errors
2. Open Shop UI in play mode — verify all items still purchase normally
3. Trigger asset re-save (touch a `.asset` file, save scene) — warnings should clear after one cycle

**CBS migration** (async, before launch):

- PlayFab CBS dashboard: drop `Positions` column from `CBSItem` schema
- Owner: tanapol
- Blocking? No — runtime tolerates extra CBS fields

## References

- [S5-11 decision document](../../production/decisions/S5-11-r-21-item-role-restriction.md)
- [S5-11 briefing](../../production/sprint-006-prep/S5-11-r-21-item-role-restriction-briefing.md)
- [Risk Register R-21](../../production/risk-register/risk-register.md)
- [Item System GDD](../../design/gdd/item-system.md) §Known Issues / §6
- [Phase 2 Lessons Learned](phase-2-lessons-learned.md) — Pattern #5 (API+caller pair audit)
- [ADR-0005 — Item Animation Type Routing](ADR-0005-item-animation-type-routing.md) — related item system ADR

## Changelog

- **2026-05-15** — Accepted. Code change landed in delta-unity (1 line removal in `ItemObject.cs`). R-21 closed. CBS migration noted as async pre-launch task.
