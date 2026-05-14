# S5-11 — R-21 Item Role Restriction Decision (Sprint 006 Day-1 Briefing)

> **Status**: Deferred from Sprint 005 to Sprint 006 Day-1.
> **Owner**: gameplay-programmer (`tanapol`) + game-designer
> **Estimate**: 1.0d (decision + implementation + ADR + tests)
> **Pre-reading time**: 5 minutes
> **Decide before or alongside S5-12** (AI Bot fate) — S5-11 Path A enables AI Bot role-aware item priority

---

## TL;DR — The decision in one sentence

Choose **A** (implement gating: `item.Positions.Contains(Hero.Role)` check + Shop UI filter), **B** (remove field: delete dead schema and commented UI), or **C** (soft hint: Shop UI badge only, no purchase gate).

---

## Carryover history

| Sprint | Story IDs | Outcome |
|--------|-----------|---------|
| Sprint 004 | R-21 surfaced (S4-05 reverse-doc) | Retro action #2: decide before Sprint 005 mid-point |
| Sprint 005 | S5-11 (the decision) | Deferred — Phase 2 closeout took capacity |
| **Sprint 006** | **DECIDE on Day-1 alongside S5-12** | Should not defer further — design ambiguity tax accrues |

---

## Current state (S4-05 reverse-doc, 2026-05-08)

**Field declared, unused at runtime:**

```csharp
// Assets/GameScripts/Resources/ItemObject.cs
public class ItemObject : ScriptableObject
{
    public StatValue[] Stats;
    public Role[] Positions;          // ← declared
    public ItemType ItemType;
    public ItemEffectPattern[] ItemEffectsPattern;
    public ItemEffectMythicPattern[] MythicItemEffect;
}

// Assets/GameScripts/Commons/Enums/Role.cs
public enum Role : byte { All, Tank, Support, Carry, Fighter, Assassin, Mage }
```

**CBS data state:** Sample item `.asset` shows `Positions: 00` (empty default). Designer has never filled this field for ~158 items in production CBS.

**`AvailableToPurchase()` gates** (`NetworkHeroInventory.cs:1045-1158`): money / mythic max-1 / boots tier rules / epic+legendary recipe / potion at-base / inventory-full with observer-ward exception. **No `Positions` check.**

**UI references** (`UIInGameShopView.cs`): 3 mentions all commented out (display only, never were purchase gates):
```csharp
// foreach (var pos in itemObject.Positions) { position += $"-{pos}"; }
```

**No other read sites** of `Positions` in `Assets/GameScripts/`. Field is dead code in production.

---

## Risk Register R-21

| Field | Value |
|-------|-------|
| Source | S4-05 reverse-doc finding 2026-05-08 |
| Probability | Realized (all roles can buy all items today) |
| Impact | Low — does not block gameplay, balance: no build constraints |
| Status | Open |
| Owner | gameplay-programmer + game-designer |
| Escalation trigger | Designer fills `Positions[]` expecting enforcement → playtest reveals it does nothing |

---

## Path A — IMPLEMENT gating (1.0d)

**Code work** (~0.5d):
1. `NetworkHeroInventory.AvailableToPurchase()` — add Position check:
   ```csharp
   // Role restriction: empty Positions = all roles allowed (backward compat)
   if (item.Positions != null && item.Positions.Length > 0 &&
       !item.Positions.Contains(Hero.Role) &&
       !item.Positions.Contains(Role.All))
   {
       return false;
   }
   ```
2. `UIInGameShopView.cs` — activate the 3 commented blocks:
   - Shop filter: "show only items for my role"
   - Item card: "recommended for [Role]" badge
   - Disabled state visual if role mismatch

**Data work** (~0.25d, designer task — can run parallel):
- Fill `Positions[]` for ~158 CBS items on PlayFab dashboard
- Backward-compat rule: empty `Positions[]` = all roles allowed

**Test work** (~0.25d):
- EditMode test `RoleRestrictionTests.cs`: matrix of {Role × Item.Positions} → expected bool
- Manual playtest: Tank tries to buy Mage-only item → blocked correctly

**Pros**:
- Honors original design intent (Positions was declared deliberately)
- **Unblocks S4-09 AI Bot item-buying with role-aware priority** (paths align)
- Strategic build depth (role identity matters)
- MOBA pattern parity (LoL / Mobile Legends gate or recommend by role)

**Cons**:
- Designer toil: fill 158 items
- Player friction risk: "why can't my Tank buy this item?"
- Adds another `AvailableToPurchase()` gate to maintain

## Path B — REMOVE field (0.35d) — *Sprint 005 closeout assistant recommendation*

**Code work** (~0.25d):
1. Delete `public Role[] Positions;` from `ItemObject.cs`
2. Delete 3 commented blocks in `UIInGameShopView.cs`
3. CBS dashboard schema migration ticket — drop `Positions` column

**Test work** (~0.1d):
- Smoke check: items still purchase normally; no serialization NRE

**Pros**:
- Cleanest — kills dead code permanently
- No designer toil
- All-role buying is the current actual behavior (no bug reports)
- MVP-launch friendly — fewer moving parts
- Matches "API exists with no caller" anti-pattern lesson from `phase-2-lessons-learned.md` Pattern 5

**Cons**:
- Loses build-constraint design depth (irreversible without re-adding field)
- S4-09 AI Bot item-buying becomes "all roles can buy any item" priority logic
- Drops a designed feature even if unused

## Path C — SOFT hint only (0.5d)

**Code work**:
1. Keep `Positions` field, don't add `AvailableToPurchase()` gate
2. `UIInGameShopView.cs`: activate the 3 commented blocks **as recommendation badge only**, not as gate
3. AI Bot may still ignore field or use as soft preference

**Pros**:
- Middle ground — design freedom + UI value
- No player friction
- Lower designer toil than Path A

**Cons**:
- Field continues without strict semantics → ongoing tech debt
- Ambiguous → S4-09 Bot item priority design still has to decide
- Carryover risk: "ambiguous decision triggers re-decision next sprint"

---

## Dependencies + downstream

**Unblocks:**
- **S4-09 AI Bot item-buying** — Bot priority logic differs per S5-11 path:
  - Path A → Bot uses role-aware priority (only buys items matching its Hero.Role)
  - Path B → Bot priority by stat/effect only, all items in candidate pool
  - Path C → Bot may treat Positions as soft preference (50/50 ambiguous)
- **S5-12 AI Bot fate** Path A — requires S5-11 to be Path A or C; if S5-12 chooses Path B (descope), S5-11 path matters less but still cleans up R-21

**Cross-cuts:**
- Hero System (C2): `Hero.Role` field already populated per hero (no work)
- Shop UI: Path A/C activates dead code, Path B deletes it
- CBS dashboard: Path A needs 158 items filled, Path B needs schema drop

---

## Stakeholder discussion prompts (for game-designer)

1. **Was role restriction intentional design or accidental schema?** Was `Positions[]` added because the design vision was "tanks build defensive, mages build damage", or was it added speculatively and never used?
2. **MVP launch question:** Should build flexibility be high (any role buys any item — current state) or constrained (role-locked items)? Constraint adds depth but may frustrate experimenters.
3. **Bot question:** If `S4-09 AI Bot item-buying` ships (Path A in S5-12), do bots need role-aware priority? If yes → S5-11 Path A or C required.
4. **Effort budget question:** If designer time is limited, is filling 158 items × Positions[] worth the design depth gained, or better spent on hero balance / content tuning?

If designer answers:
- "Intentional + want constraint + want bot role-aware" → **Path A**
- "Accidental + don't need it + want clean code" → **Path B** (recommended for MVP)
- "Intentional but soft + flexibility matters" → **Path C** (risk of further re-decision)

---

## Decision deliverables (Day 1 of Sprint 006)

Whatever path is chosen:

1. **Decision document** at `production/decisions/S5-11-r-21-item-role-restriction.md` (NEW)
2. **ADR-0009** at `docs/architecture/ADR-0009-item-role-restriction.md` (NEW) — Status: Accepted
3. **Risk register** — close R-21 with status: Resolved + reference ADR-0009
4. **GDD update** — `design/gdd/item-system.md` §Known Issues: mark R-21 resolved
5. **Sprint 005 retro action #2** — mark closed in `production/retrospectives/sprint-005.md` (when retro is generated)
6. **Sprint 006 plan** — reflect the chosen path (Path A: include implementation story; Path B: include cleanup story; Path C: include UI activation story)

**If Path A:**
- 7. `NetworkHeroInventory.AvailableToPurchase()` Position check (code)
- 8. `UIInGameShopView.cs` activate commented blocks
- 9. EditMode test `RoleRestrictionTests.cs`
- 10. CBS designer ticket: fill 158 items × Positions[]

**If Path B:**
- 7. Delete `Positions` field
- 8. Delete commented UI blocks
- 9. CBS schema migration ticket

**If Path C:**
- 7. UI activation as badge/recommendation only (no gate)
- 8. GDD note: "Positions = soft recommendation, not enforced"

---

## Sprint 005 close-out reference

This briefing is the artefact produced by deferring S5-11 from Sprint 005. The deferral was tracked in:
- `production/sprints/sprint-005.md` § Deferred to Sprint 006 (entry: "S5-11 R-21 Item Role Restriction — Path A/B/C, see briefing")
- `production/sprint-status.yaml` (entry: S5-11 status `deferred-to-S6`)

**Coordinate with S5-12 decision** — both belong to Sprint 006 Day-1 decision batch. S5-11 should be decided **first** (or in same conversation) because S5-12 path selection depends on S5-11 outcome.

---

## Changelog

- **2026-05-14** — Initial briefing authored as part of Sprint 005 close-out (S5-11 deferred alongside S5-12 to Sprint 006 Day-1).
