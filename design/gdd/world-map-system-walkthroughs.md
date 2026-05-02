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

**Player POV:** Player A — `faction_id = "faction_b"`, `state = InCity`, in city_nova_01 (1 of 3 active instances of city_nova; pop = 87/150). Has visited city_nova twice before; last Anchor vignette seen = V2 (5 days ago). Currently browsing Hero/Skin Shrine UI (Universal Service R2). Two cross-faction strangers within `CITY_CHAT_RADIUS_UNIT = 15` units. No Fragment Event active anywhere at session start.

**Time origin:** t=0 = FT14 server-authoritative timestamp at which `fragment_event_started(city_id="city_nova", event_id="evt_001")` is emitted.

**Knob context:** `BELL_SEQUENCE_DURATION_SECONDS = 30`, `R8_CROSS_INSTANCE_SKEW_BUDGET_SECONDS = 3`, `EVENT_ANNOUNCE_LEAD_SECONDS = 30`, `M11_FILTER_LATENCY_BUDGET_MS = 150`.

| t | Player input/state | Server | Client visual | Audio | UI surface | AC ref / blocker |
|---|---|---|---|---|---|---|
| **t < 0** (setup) | `InCity`, browsing Hero/Skin Shrine | Heartbeat to PlayFab Player Record (`current_instance_id = nova_01`, `last_heartbeat`) every 5s per EC-07 freshness window | Plaza ambient render; player avatar idle near Hero/Skin Shrine landmark; 2 strangers within 15u; ambient NPCs (≥5 per `MIN_AMBIENT_NPC_PER_CITY`) | Plaza ambient soundscape (theme-bound — **OQ-2 deliverable, currently undefined** → **narr-3**) | Hero/Skin Shrine UI open (1 of 8 Universal Services); cross-instance counter "ลานนี้ 87 / ลานร่วม 240" visible at plaza entrance | TR-WMS-002 (R2 Universal Services) ; **narr-3** (city theme spec) ; **narr-9** ("ลานร่วม Y" label ambiguous) |
| **t = 0** | (no input) | FT14 emits `fragment_event_started(city_nova, evt_001)` to FT12 event-bus listener (Azure Function or Photon sidecar — **deferred to R13 ADR**) | (no change yet — propagation in flight) | (no audio yet) | (no UI change yet) | OQ-4 (FT14 schema TBD) ; OQ-5 (network pattern ADR) |
| **t = 0 + δ₁** (δ₁ = Azure Function cold-start to first instance, ~200–800ms) | (no input) | FT12 fan-out begins: SignalR / Photon mgmt API push to all 3 instances of city_nova. **Mechanism deferred to R13 ADR.** Read-through cache (≤5s TTL) on instance roster → enumerate nova_01, nova_02, nova_03 | (no change on player A's screen yet — instance not yet notified) | — | — | **BLOCKER #2** (R8 skew lower bound 1s unachievable on Function Consumption due to cold-start) ; **qa-7** (production/qa env-spec absent) |
| **t = T** (T ∈ [δ₁, 3s] per skew budget; representative T ≈ 0.6s for nova_01) | (no input) | nova_01 instance receives event ; server-side trigger of bell + Monument transform ; instance broadcasts bell to all 87 in-instance clients via Photon Fusion room channel | Monument transforms (particle + glow + height per Visual touch-points line 894) ; **Section B prescribes "ทุกคนในจัตุรัสหันมามองกัน" — NO avatar behavioral cue specified in R8** | Bell audio fires (diegetic 360-degree, **theme-bound bell sound — OQ-2 + audio direction undefined** → **narr-3, nice-to-have narr-10 instrument designation**) | Cross-instance counter highlights + animates ; Galaxy Map indicator (if open) flips to "active" | TR-WMS-009 (R8 — but **AC asserts ≤500ms not ≤3s skew**, see qa-6) ; **BLOCKER #1** (R8 collective moment undeliverable — counter ≠ moment, no avatar cue) ; **qa-6** (R8 cross-instance skew has no AC at 3s budget) ; **narr-1+10** (no instrument designation, no orchestration of moment) |
| **t = T to T+0.3s** | (no input) | (no further state mutation) | Monument continues transform animation (visual prominence ramping) | Bell continues (30-degree sound profile per EC-26) | Cross-instance counter pulse animation in progress | EC-26 (per PR #17 sequencing rule: bell t=0, indicator t=+0.3s, Galaxy Map passive) ; TR-WMS-046 (PR #17 EC-26 sequencing AC) |
| **t = T+0.3s** | (no input) | — | Galaxy Map indicator passive update (if Galaxy Map closed: indicator queued for next open) | — | Galaxy Map indicator badge for city_nova flips to "active" state per EC-26 sequencing | TR-WMS-046 |
| **t = T to T+30s** (bell sequence window) | Player A may: (a) keep browsing service menu, (b) close menu and watch plaza, (c) try to chat | Photon Fusion room channel delivers Monument-prominence-active state ; if player chats: client → Photon host → Azure Function chat handler → M11 filter (≤150ms SLA) → broadcast to in-radius peers | Monument prominence stays high ; **other players' avatars: no specified behavioral change — players who would "look" at Monument have no animation directive** ; in-instance peer chat messages render in proximity bubble | Bell audio continues full duration (default 30s) ; ambient soundscape volume ducked? — **not specified** | Service menu still open if player did not close ; chat composer bottom of screen | TR-WMS-033 (M11 RadiusChat forward) ; **BLOCKER #1** (collective moment) ; **BLOCKER #3** (R8 SignalR bell vs Photon RadiusChat cross-channel ordering — chat referencing bell may arrive on peer screens before bell does) |
| **t ≈ T+10s** (player A closes Hero/Skin menu, walks toward Monument) | Click "Close" on service menu ; walk input forward | Movement input handled by C3 Movement (out-of-FT12-scope) ; presence delta broadcast to in-instance peers | Service menu fades out ; player avatar walks toward plaza center ; Monument visible foreground | Footsteps + ambient ; bell still ringing | Service menu hides ; HUD minimal | (no AC — generic movement) ; **BLOCKER #11** (R15 secondary linger has zero pull — player walks toward Monument because of bell, not because R15 affordances drew them) |
| **t ≈ T+15s** (player A passes ambient bench R15, ignores it) | Walk past bench (R15 affordance #1) | Server registers proximity to affordance ; no state mutation since R15 has no rewards (TR-WMS-039) | Bench renders (cosmetic only) ; no interaction prompt unless player explicitly stops | Footsteps continue | None | TR-WMS-039 (R15 no-reward) ; **BLOCKER #11** (no pull — bench has no discoverability signal during bell window per ux-designer rec #11) |
| **t ≈ T+22s** (player A stands at Monument, looks up at it) | Idle at Monument ; no input | (no server state — Monument is one-way display per R8 boundary) | Monument visual prominence at peak ; player avatar idle | Bell audio still active (last ~8s of window) | None — Monument is not interactable | **BLOCKER #1** (no designed shared-witness moment — player A standing alone reading prominence ; "ทุกคนหันมามอง" requires others doing the same simultaneously, but no cue commands their avatars to do so) |
| **t = T+30s** (bell sequence end) | Idle | Server emits internal `bell_sequence_complete` (no FT14 round-trip — local end of fan-out window) | Monument retains "active visual" (no further transform — stays glowing per R8 step 3) | Bell audio fades out ; ambient soundscape returns to normal volume | Cross-instance counter pulse animation stops | TR-WMS-009 partial coverage (asserts visual active) ; **qa-6** still applies — no AC for "Monument stays after bell" timing |
| **t = T+30s + ~5s** (player A approaches Faction Anchor — chronicle of war for city_nova) | Walk to Anchor landmark | Client requests current Anchor vignette ID from server (per R3.1 — server-clock UTC rotation) | Anchor visual present ; vignette panel UI opens with current text (= V3 if rotation advanced past V2 ; could still be V2 if within current 7-day window) | Anchor ambient (theme-bound — undefined) | Anchor vignette panel | TR-WMS-036 (R3.1 rotation) ; **BLOCKER #10** (no per-player seen-state ; if last visit was 5 days ago and rotation is 7 days, player sees same V2 — no freshness signal indicates whether content changed) ; **narr-3** (vignette content scope OQ-3 unstaffed) |
| **t ≈ T+45s** (player A interacts with Anchor — reads vignette V2 again) | Click Anchor read | Server logs `anchor_interacted(player_id, city_id, vignette_id)` for telemetry | Vignette text panel renders ; lore content displays | Voiceover? — **not specified** ; ambient | Vignette panel persistent until close | TR-WMS-036 ; **BLOCKER #10** confirmed (player has no signal that vignette is unchanged from prior visit ; repeat-visit hook fails silently) |
| **t ≈ T+90s** (player A closes Anchor, decides to queue Tournament) | Click Tournament Queue terminal landmark → opens Tournament Queue UI | Player UI request → FT13 endpoint forecast (FT13 undesigned — mock response). Confirm queue → state transition `InCity` → `InCity+Queued` ; `origin_city_id = "city_nova"` snapshot | Service menu opens (Tournament Queue UI subset) ; queue confirmation dialog | Standard UI sounds | Tournament Queue UI surface ; subsequent confirmation modal | TR-WMS-032 (FT13 queue handoff mock) ; **qa-5** (R5 Browse-mode no-travel has no AC — but doesn't apply here since player queueing not travelling) |
| **t = T+90s+** (player remains InCity+Queued, Monument still glowing) | Idle in Tournament queue | Heartbeat continues ; if FT13 emits `match_found` → state → `InMatch` | Plaza render continues ; Monument retains active visual ; queue indicator on HUD | Ambient ; queue ambient cue if any | Queue indicator persistent | TR-WMS-032 ; future FT13 lifecycle |
| **t = T + ~3600s** (event ends per FT14 duration — out of FT12 control) | (player may be in match by now ; if still InCity+Queued, observes event end) | FT14 emits `fragment_event_ended(city_nova, evt_001)` ; FT12 broadcasts to all instances ; Monument resets | Monument visual returns to dormant state (no particle, no glow) | Reset cue? — **not specified** ; could be subtle bell-fade or silence | Galaxy Map indicator clears for city_nova ; cross-instance counter no special state | TR-WMS-009 partial (Monument reset) ; **qa-9** (TR-048 timing tolerance untestable without instrumentation hook spec) ; OQ-4 (FT14 ended-event schema) |

### Empty-cell blockers exposed by Scenario C

This scenario walk surfaces the following review-log blockers as load-bearing — i.e., the GDD as-written does not deliver the moment without them being closed:

| Blocker | Severity | Where exposed | What's missing in cell |
|---|---|---|---|
| **#1** R8 collective moment undeliverable | BLOCKING (pillar P3) | t=T (bell), t=T+22s (player at Monument) | Avatar behavioral cue at bell time ("ทุกคนหันมามองกัน" has no animation directive) ; Monument has no shared-witness mechanic (player standing alone reading prominence) |
| **#2** R8 skew lower bound 1s unachievable | BLOCKING | t=0+δ₁ (Function fan-out) | Cold-start budget exceeds 1s lower bound ; ADR must constrain to mechanism-feasible range |
| **#3** R8 cross-channel ordering undefined | BLOCKING | t=T to T+30s (chat about bell) | If SignalR bell + Photon chat use different real-time channels, peer Y sees chat referencing bell before peer Y receives bell event ; no constraint preventing this |
| **#10** R3.1 freshness signal absent | BLOCKING (pillar P5) | t=T+30s+5s (Anchor approach) | Habitual visitor sees same vignette ; no UI indicator distinguishes "new since last visit" from "unchanged" — repeat-visit hook fails silently |
| **#11** R15 zero pull mechanics | BLOCKING (pillar P5) | t=T+15s (walks past bench) | No discoverability signal during bell window ; bench passed without acknowledgment ; Section B "linger" intent has no behavioral incentive |
| **qa-6** R8 cross-instance skew no AC | BLOCKING (qa) | t=T (bell fire) | TR-WMS-009 asserts ≤500ms intra-instance ; no AC asserts ≤3s cross-instance skew budget |
| **qa-9** TR-048 timing tolerance untestable | BLOCKING (qa) | t=T+0.3s (sequencing per EC-26), t = event end | EC-26 specifies bell→indicator t=+0.3s sequencing but no instrumentation hook to verify ±50ms client-side |
| **narr-1** Keeper voice profile (peripheral here) | BLOCKING (pillar narrative) | t<0 (ambient) — Keeper as ambient persistent presence has no voice spec | Even non-first-visit, Keeper is observable in plaza ; voice profile absence means writers cannot author ambient lines |
| **narr-3** OQ-2 theme spec deliverable absent | BLOCKING (production) | t<0 (ambient soundscape), t=T (bell sound), t=T+30s+5s (Anchor ambient), t=T+~3600s (event-end cue) | 4 separate cells require theme-bound audio/visual content that does not yet have a deliverable spec ; 3 production departments (audio, art, narrative) blocked |

### Scenario C complete-row summary

- **Total rows:** 16
- **Rows with all 5 cells filled, no blocker reference:** 0 (every row references at least one blocker, AC, or OQ — expected for a system in MAJOR REVISION state)
- **Rows with at least one empty/TBD/blocker-marked cell:** 16
- **Distinct BLOCKING blockers exposed:** 9 (cluster spread: 1, 4, 8 + qa + narr)
- **AC coverage:** TR-WMS-009 (R8), TR-WMS-033 (M11), TR-WMS-039 (R15), TR-WMS-036 (R3.1), TR-WMS-046 (EC-26), TR-WMS-032 (FT13 mock) — **6 of 49 ACs reference this scenario directly** ; revised AC suite from qa-lead would add TR-052 (cross-instance skew) for full coverage

### Implication for revision pass (Phase 3)

To close Scenario C, Cluster 1 (R8) must address all three sub-items: (1) avatar behavioral cue specification at bell time — small addition to R8 step 1 ; (2) skew range mechanism-bound — ADR constraint plus knob range adjustment ; (3) cross-channel ordering constraint — ADR constraint with naming.

Cluster 4 (linger/repeat-visit) must address Blockers #10 + #11 with player-facing affordances, not just internal mechanisms — freshness UI signal for vignette change ; pull mechanic for R15 (e.g., line-of-sight environment placement minimum from spawn point per ux-designer rec #11).

Production-side narr-3 (OQ-2) must produce theme spec including: per-city ambient soundscape, per-city ritual instrument for bell, per-city Anchor ambient, per-city event-end cue. This is 4 distinct audio briefs per city × 10 cities = 40 audio direction specs as a derived deliverable from OQ-2 — currently unstaffed and uncosted.

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
- [x] **Scenario C authored (2026-05-02)** — 16 rows, 9 distinct BLOCKING blockers exposed (Clusters 1, 4, 8 + qa + narr)
- [ ] Scenario A authored
- [ ] Scenario B authored
- [ ] Scenario D authored
- [ ] Scenario E authored
- [ ] Blocker → Walkthrough Mapping verified against authored content
- [ ] Phase 2 complete; Phase 3 (cluster-by-cluster GDD revision) can begin

---

## Next session

Author **Scenario A — First-Visit** (post-FT11 selection). Primary blocker coverage:
Cluster 8 (Keeper info-dump tone — narr-1 voice profile gap), Section B onboarding
beat (narr-3 theme spec dependency), EC-15 FT9 migration, EC-19 launch flood scenario
(distinct from launch-spike Scenario E — single-player slice, not aggregate).
