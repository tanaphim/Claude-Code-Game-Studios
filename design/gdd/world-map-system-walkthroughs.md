# FT12 World Map — Player Journey Walkthroughs

> **Purpose:** Top-down walkthrough-driven analysis of FT12 World Map per third-review
> creative-director synthesis (2026-04-30). Each scenario is a moment-by-moment
> player journey. Every row must cover all 5 layers (server / client visual /
> audio / UI surface / AC ref). **Empty cells in any row = a blocker from the
> review log** — those are what GDD revision must close before mark-Approved.
>
> **Status:** In Progress — Phase 2 of FT12 revision plan
> **Author:** User + Claude Code agents
> **Created:** 2026-05-02
> **Companion docs:**
> - GDD: `design/gdd/world-map-system.md`
> - Review log: `design/gdd/reviews/world-map-system-review-log.md`
> - Source design: `design/decisions/meeting-2026-04-23-tournament-pivot.md`

---

## Methodology

### Why walkthrough-driven

Round 2 (2026-04-29) revision applied a bottom-up patch model: each blocker fixed
in isolation, structural completeness verified per-rule. Round 3 review identified
a pattern: **"added the noun, not the verb"** — mechanisms specified (R8 bell
sequence, R3.1 vignette rotation, R15 affordances, etc.) without the player-facing
or implementation-facing affordances that make those mechanisms deliver.

The walkthrough methodology forces every mechanism to be evaluated as an *integrated
moment in a player journey*. If a row in any scenario table has an empty cell, the
mechanism does not deliver at that moment for at least one of the 5 layers. That
empty cell is the blocker — not the rule itself.

### How to walk a scenario

1. Pick a representative player POV (state + intent at t=0)
2. Step time forward in observable beats — UI input, server event, audio cue,
   visual change. Step size varies (0.1s for animation cues, minutes for state)
3. For each beat, fill all 5 columns:
   - **Server**: what server-side state changed, what API was called, what
     persisted to PlayFab/CosmosDB
   - **Client visual**: what render changed on the player's screen — avatars,
     environment, particles
   - **Audio**: what sound the player hears (or doesn't)
   - **UI surface**: which of the 17 UI surfaces appears, updates, or hides
   - **AC ref / blocker**: which TR-WMS-NNN AC tests this row — OR which review
     log blocker this row reveals if no AC exists
4. If any cell is empty: cite the blocker ID from the review log (or note as new
   finding). Do NOT fabricate cell content to fill the table.
5. After full scenario walk, list all empty-cell-derived blockers in the
   per-scenario summary

### What "complete" means

A row is complete when:
- All 5 columns have content (or explicit "n/a" with reason)
- AC ref exists for any state-mutation or player-observable behavior
- Server / client / audio layers reference specific GDD rules (R1, R8, EC-XX, etc.)
  — not generic claims

A scenario is complete when:
- Every observable beat in the player journey has a row
- The summary matrix maps every empty cell to a review log blocker
- No "TBD" cells remain (TBD = blocker)

---

## Scope — 5 Scenarios

| # | Scenario | Player POV | Primary blocker coverage |
|---|---|---|---|
| A | First-Visit | New player, just completed FT11 faction selection, entering World Map for first time | Cluster 8 (Keeper info-dump), Section B onboarding, EC-15 migration, EC-19 launch flood |
| B | Return-Visit-No-Event | Returning player, no Fragment Event active anywhere, casual session start | Cluster 4 (R3.1 vignette rotation, R15 zero pull), Cluster 7 (D1 algorithm), post-first-visit City Menu access |
| C | Return-Visit-With-Event | Player already InCity when Fragment Event announces at same city | **Cluster 1 (R8 bell collective moment)**, Cluster 4 (R15 secondary linger), R3.1 freshness signal, EC-26 sequencing |
| D | Cross-Faction-Party Formation | Player invites friend of different faction, parties up, queues Casual Tournament | **Cluster 5 (OQ-10 perverse incentive)**, R9 co-location, R16 Stranger Acknowledge (PR #17), Fragment routing |
| E | Launch-Spike | Server-side: 5000 new players hit STARTER_CITY in 10s window | **Cluster 3 (capacity math)**, EC-19, EC-14 ghost slots, PlayFab API rate limit, D2 scheduler |

---

## Scenario A — First-Visit

**Player POV:** New account, just completed FT11 faction selection (state = `OutOfUniverse` → transition to `EnteringCity`). Has no `last_city_id`, no equipment, no Fragment history. Default `STARTER_CITY_ID = city_01`.

**Time origin:** t=0 = login success after FT11 selection.

| t | Player input/state | Server | Client visual | Audio | UI surface | AC ref / blocker |
|---|---|---|---|---|---|---|
| _TBD — to be authored in Scenario A session_ |

**Empty-cell blockers (to be filled during authoring):** TBD

---

## Scenario B — Return-Visit-No-Event

**Player POV:** Existing account, `last_city_id = "city_solis"` (set previous session). Logs in. No Fragment Event active anywhere in the universe. Wants to: check faction Anchor for any updates, then queue Casual Tournament.

**Time origin:** t=0 = login success.

| t | Player input/state | Server | Client visual | Audio | UI surface | AC ref / blocker |
|---|---|---|---|---|---|---|
| _TBD — to be authored in Scenario B session_ |

**Empty-cell blockers (to be filled during authoring):** TBD

---

## Scenario C — Return-Visit-With-Event

**Player POV:** Existing account, already `InCity` at city_nova for ~2 minutes browsing services. FT14 announces Fragment Event at city_nova. Player is in the same city as the announce.

**Time origin:** t=0 = FT14 emits `fragment_event_started(city_nova, evt_001)`.

| t | Player input/state | Server | Client visual | Audio | UI surface | AC ref / blocker |
|---|---|---|---|---|---|---|
| _TBD — to be authored in Scenario C session (next, per recommended order)_ |

**Empty-cell blockers (to be filled during authoring):** TBD

---

## Scenario D — Cross-Faction-Party Formation

**Player POV:** Player A (`faction_id = "faction_b"`) is `InCity` at city_astra. Friend B (`faction_id = "faction_d"`) is online in city_solis. Player A invites Player B to party, B accepts and travels to city_astra. They form a 2-person Casual Tournament queue.

**Time origin:** t=0 = Player A clicks "Invite to Party" on Player B's friend list entry.

| t | Player input/state | Server | Client visual | Audio | UI surface | AC ref / blocker |
|---|---|---|---|---|---|---|
| _TBD — to be authored in Scenario D session_ |

**Empty-cell blockers (to be filled during authoring):** TBD

---

## Scenario E — Launch-Spike

**Player POV:** Server-side aggregate view (not single-player) — 5,000 new accounts complete FT11 selection in a 10-second window after a marketing push. All target `STARTER_CITY_ID = city_01`. Pre-warm = 5 instances × 150 cap = 750 capacity.

**Time origin:** t=0 = first TravelRequest of the spike arrives at FT12 server.

| t | Aggregate state | Server | Client visual (representative player) | Audio | UI surface | AC ref / blocker |
|---|---|---|---|---|---|---|
| _TBD — to be authored in Scenario E session_ |

**Empty-cell blockers (to be filled during authoring):** TBD

---

## Blocker → Walkthrough Mapping (to be populated as scenarios complete)

| Blocker # | Cluster | Title (short) | Exposed in Scenario |
|---|---|---|---|
| 1 | 1 | R8 bell collective moment | C |
| 2 | 1 | R8 skew lower bound 1s | C, E |
| 3 | 1 | R8 cross-channel ordering | C |
| 4 | 2 | R12 atomicity claim overstated | E |
| 5 | 2 | R12 conflates two CAS systems | E |
| 6 | 2 | R12 cache layer constraint | E |
| 7 | 3 | EC-19 prewarm 15% coverage | E |
| 8 | 3 | EC-14 ghost slots phantom occupancy | E |
| 9 | 3 | PlayFab API rate budget | E |
| 10 | 4 | R3.1 vignette freshness signal | B (primary), C (secondary) |
| 11 | 4 | R15 zero pull mechanics | B (primary), C |
| 12 | 4 | Post-first-visit City Menu access | A (introduces), B (exposes) |
| 13 | 5 | OQ-10 Fragment routing perverse incentive | D |
| 14 | 6 | D2 5s tick execution environment | E |
| 15 | 7 | D1 break/CAS-conflict path | D, E |
| 16 | 7 | G.6 LEAD min jitter headroom | E |
| 17 | 8 | Keeper NPC info-dump | A |
| 18 | 8 | EC-09 auto-blur draft fate | A, D |
| 19 | 8 | OQ-7 platform decision gate | A (introduces in onboarding) |
| qa-1 | qa | TR-034 step-3 rate AC rewrite | E |
| qa-2 | qa | TR-037 partial-success AC rewrite | E |
| qa-3 | qa | TR-038 self-contradicting AC | D, E |
| qa-4 | qa | R12 read-repair / cache TTL no AC | E |
| qa-5 | qa | R5 Browse-mode no-travel no AC | A, B, C |
| qa-6 | qa | R8 cross-instance skew no AC | C |
| qa-7 | qa | production/qa/ directory absent | E |
| qa-8 | qa | PlayFab rate limit env-spec | E |
| qa-9 | qa | TR-048 instrumentation spec | C |
| narr-1 | narr | Keeper voice profile | A |
| narr-2 | narr | EC-11 ban broadcast intent | A (touches), D (touches) |
| narr-3 | narr | OQ-2 deliverable spec | A, B, C (all theme-dependent) |

---

## Status / Progress

- [x] Skeleton created (2026-05-02)
- [ ] Scenario C authored
- [ ] Scenario A authored
- [ ] Scenario B authored
- [ ] Scenario D authored
- [ ] Scenario E authored
- [ ] Blocker → Walkthrough Mapping verified against authored content
- [ ] Phase 2 complete; Phase 3 (cluster-by-cluster GDD revision) can begin

---

## Next session

Author **Scenario C — Return-Visit-With-Event** (highest blocker density: Cluster 1
R8 bell collective moment + R3.1 freshness + R15 secondary linger + EC-26 sequencing).
