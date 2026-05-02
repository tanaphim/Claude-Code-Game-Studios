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

**Player POV:** New account, just completed FT11 faction selection. `faction_id = "faction_b"` committed to Player Record. No `last_city_id`, no equipment, no `ft12_migrated` flag, no Fragment history. State = `OutOfUniverse`. Provisional `STARTER_CITY_ID = "city_01"` (per OQ-1). No FT9 `last_town_state` (net-new account ; for migrated FT9 accounts EC-15 applies but UX is identical from this point).

**Time origin:** t=0 = login success after FT11 selection (FT11 emits `faction_committed` ; FT12 first session begins).

**Knob context:** `STARTER_CITY_INSTANCE_PREWARM_COUNT = 5`, `ENTERING_TIMEOUT_SECONDS = 30`, `MIN_AMBIENT_NPC_PER_CITY = 5`, EC-27 grace period = 0.5s after scene streaming complete.

**Platform assumption:** PC + mouse+keyboard (provisional per OQ-7 — every interaction below carries platform-revision risk per **BLOCKER #19**).

| t | Player input/state | Server | Client visual | Audio | UI surface | AC ref / blocker |
|---|---|---|---|---|---|---|
| **t < 0** (setup) | At FT11→FT12 handoff screen | FT11 server commits `faction_id` to Player Record ; emits `faction_committed` event to FT12 server-side listener | FT11 final screen ("ผู้ถือธงคนใหม่ของฝ่าย X") | FT11 commit cue | FT11 confirmation modal | FT11 ACs (out of FT12 scope) |
| **t = 0** (login success) | (no input — auto-transition) | FT12 server reads Player Record ; `last_city_id` is null → fall-through to `STARTER_CITY_ID` ; allocate instance via D1 with party_size=1, target=`city_01` ; D1 step 2 picks pre-warmed instance city_01_01 (or whichever has lowest pop among 5 pre-warmed) ; state transitions `OutOfUniverse` → `EnteringCity` ; `current_instance_id` written via single PlayFab `UpdateUserData` (R12 atomicity contract) | Loading screen fades in | Loading-screen ambient cue | Loading-screen UI (out-of-FT12-scope handoff) | TR-WMS-005 (R4 first-ever spawn) ; **BLOCKER #4** (R12 atomicity claim — TR-WMS-037 tests phantom failure mode here) ; **BLOCKER #5** (R12 conflates Player Record CAS with instance-population CAS — D1 step 2 does both in this row) |
| **t = 0 to t = T_load** (T_load typically 3–8s on PC) | (no input) | Scene streaming progresses ; `ENTERING_TIMEOUT_SECONDS = 30` countdown active per EC-18 | Loading-screen progress | Loading-screen ambient | Loading screen | TR-WMS-041 (EC-18 EnteringCity timeout — fires at 30s if scene loader hangs) |
| **t = T_load** (scene streaming complete) | (no input) | Server confirms instance assignment ; `state = InCity` written to Player Record ; presence broadcast to in-instance peers | Plaza render begins ; player avatar materializes at spawn point near Keeper NPC ; Faction Banner Racks visible (auto-flag with `faction_id` per R7 + EC-23) ; Monument visible (dormant) ; ≥5 ambient NPCs (`MIN_AMBIENT_NPC_PER_CITY`) | Ambient soundscape onset (theme-bound — **OQ-2 deliverable, undefined → narr-3**) | HUD minimal (no menus open yet) | TR-WMS-005 spawn destination ; TR-WMS-031 (FT11 read-only, badge renders for player A) ; **narr-3** (theme-bound soundscape) |
| **t = T_load + 0.5s** (EC-27 grace period) | (no input) | Server emits `first_visit_environmental_beat` trigger to client (one-shot, never refires) | First-visit environmental beat: **diegetic text fade-in + ambient lighting cue** per Section B (lines 90–93). Per Section B: "atmospheric only — ห้ามอธิบาย service / UI / wayfinding" | Bell ring (single soft strike — distinct from R8 Fragment Event bell) ? — **bell type/instrument theme-bound, undefined → narr-10 nice-to-have** | Non-popup text overlay, fade in/out (per UI Requirements line 959) ; **PLACEHOLDER TEXT in GDD: "คุณกำลังเข้าสู่ลานเมือง [City Name]..."** — **dev placeholder per narr-3** ; golden draft proposed: *"ลานแห่งนี้ไม่มีประตูกั้นฝ่าย — ระฆังจะบอกให้รู้เองว่าอะไรกำลังจะมา"* | Manual-WMS-01 (first-visit fantasy smoke check) ; **BLOCKER narr-3** (Section B sample line is dev placeholder) ; **narr-1** (per-city beat text from OQ-2 deliverable absent) ; EC-27 (texture pop-in grace — passes by waiting 0.5s) |
| **t ≈ T+5s** (player observes plaza) | Idle ; player surveys environment ; mouse-look around plaza | Heartbeat to Player Record continues (EC-07 freshness window — `last_heartbeat` updated every 5s) | Player avatar idle ; plaza camera shows Keeper NPC ~8 units from spawn (visible per Section B placement spec) ; Faction Banner Racks decorated with current instance's faction mix (proportional — flagged by **narr-6 nice-to-have** as art direction note) ; Monument dormant ; cross-faction strangers visible (not in proximity yet) | Plaza ambient ; ambient NPC chatter | HUD minimal ; cross-instance counter visible at plaza entrance ("ลานนี้ X / ลานร่วม Y") | TR-WMS-031 ; **narr-9** ("ลานร่วม Y" label ambiguity) ; **narr-6 nice-to-have** (Banner Racks display proportional vs plurality) |
| **t ≈ T+10s** (approaches Keeper) | Walk toward Keeper NPC | Server tracks proximity ; when player within Keeper interaction radius → emit `keeper_prompt_active` to client | Keeper NPC visible ; **interaction prompt highlights ("กดเพื่อพูดคุย" or similar — BUT the prompt phrasing has no narrative writer pass — narr-1)** | Footsteps ; Keeper has no voice line until interacted | Interaction prompt UI (button hint) appears | (no AC for Keeper interaction at GDD level) ; **BLOCKER #17** (Keeper info-dump anti-pattern — info-dump pattern is here, no rationale why this serves sanctuary tone) ; **BLOCKER narr-1** (Keeper has NO voice profile, NO demeanor brief, NO sample line at GDD level — writer fills the slot blind) |
| **t ≈ T+11s** (clicks Keeper) | Click Keeper / press interact key | Server logs `keeper_first_interact(player_id, city_id)` ; one-shot flag `keeper_first_visit_consumed = true` written to Player Record (atomicity? — currently NOT specified in R12's 4-field atomicity contract → **BLOCKER #4 + #5 implication**) | City Menu UI fade-in ; Keeper avatar plays "open arms" or "gestures to menu" — **animation undefined**, no narrative directive | UI open sound ; Keeper voice line — **NONE SPECIFIED** | **City Menu opens** (1 of 17 UI surfaces — see UI Requirements line 944) ; menu shows 8 Universal Services per R2 | TR-WMS-002 (R2 Universal Services list) ; **BLOCKER #17** confirmed live (Keeper functions purely as a menu opener — info-dump pattern delivered) ; **BLOCKER narr-1** confirmed live (Keeper has zero voice in this critical first encounter — writer cannot author what GDD does not brief) |
| **t ≈ T+12s** (browses City Menu) | Hover/scroll over service buttons | Server: no state mutation (read-only menu) | City Menu shows 8 service buttons: Hero/Skin, Equipment, Lobby Creation, Tournament Queue, Personal Dungeon, Party Formation, Social/Chat, Faction Profile | UI hover sounds | City Menu — 8 buttons | TR-WMS-002 (verify all 8 services available, no `available=false`) ; **qa-rec1** (TR-002 fragile count assertion — per qa-lead recommended) |
| **t ≈ T+30s** (picks Tournament Queue) | Click "Tournament Queue" | Server records menu navigation (telemetry) ; opens Tournament Queue UI subsystem | Tournament Queue UI fade-in ; City Menu may stay or transition (UX spec deferred) | UI open sound | Tournament Queue UI (1 of 17 surfaces) — Casual / Ranked / Custom selection | TR-WMS-032 (FT13 mock — applies once player confirms queue) |
| **t ≈ T+45s** (confirms Casual queue) | Click "Confirm Casual Queue" | Server sends `(player_id, queue_type=casual, origin_city_id="city_01")` to FT13 mock endpoint ; state transition `InCity` → `InCity+Queued` ; `origin_city_id = "city_01"` snapshot per EC-13 fallback chain | Tournament Queue UI confirms ; queue indicator appears on HUD ; player avatar idle | Confirmation cue | Queue indicator persistent on HUD | TR-WMS-032 ; ties to **BLOCKER #19** (OQ-7 — gamepad/touch confirmation requires different interaction model than this mouse-click flow) |
| **t ≈ T+~5min** (while waiting, types in friend-search field) | Open friend list, type in search box | M1 surface — friend search active ; FT12 doesn't track this directly | Friend list UI open ; text input focused ; cursor blinking | Typing sounds | Friend list UI (M1 surface, FT12 displays) ; text input has focus | (no FT12-specific AC) ; sets up **BLOCKER #18** for next row |
| **t ≈ T+~5min + ε** (FT13 emits match_found while text focused) | (no input — async event) | FT13 emits `match_found(player_id, match_id)` ; FT12 receives ; per EC-09: defer countdown popup up to 3s if input focused, but modal must fire by t = countdown_total − 8s (effective floor 8s) | Match-found countdown modal interrupts ; text input field auto-blurs per EC-09 | Match-found alert audio (interrupt-priority) | Match-found countdown modal (Priority 1 per UI hierarchy line 963) | **BLOCKER #18** (EC-09 auto-blur — what happens to text DRAFT in friend-search field? Discarded? Preserved? UNSPECIFIED in EC-09. Player loses unsent message with no recovery) ; TR for EC-09 exists but tests countdown timing, not draft fate |
| **t ≈ T+~5min + 8s** (player accepts match) | Click "Accept" | Server: state → `InMatch` ; FT13 owns from here ; `origin_city_id = "city_01"` retained | Match-found modal closes ; loading into match | Match-load cue | Match loading screen (out-of-FT12-scope) | TR-WMS-032 |
| **(t = match in progress — out of FT12 scope)** | — | — | — | — | — | — |
| **t = match_end** (FT13 emits match_end) | (no input — auto-transition) | FT12 receives `match_end(player_id, match_id, return_payload)` ; state → `ReturningFromMatch` ; `return_payload` includes Fragment Event city list (active events) per C3 FT13 OUT row | Post-match screen fade-in | Post-match cue | Post-match screen — Fragment award, stats, "เดินทางไป [Event City]" shortcut button if Fragment Event active | TR for state transition |
| **t = match_end + 30s** (auto-timeout to gohome) | Auto-timeout (per C3 ReturningFromMatch row: "auto-timeout (~30s)") | State → `EnteringCity` ; destination = `origin_city_id = "city_01"` ; allocate via D1 (likely same instance city_01_01 if still active) | Loading screen | Loading cue | Loading screen | TR-WMS-004 (R4 reconnect path — uses `last_city_id` which equals `origin_city_id` here) |
| **t = match_end + ~35s** (returns InCity, second visit to city_01) | (no input — auto-spawn) | State `InCity` ; `last_city_id = "city_01"` (unchanged) ; presence broadcast | Player avatar appears at spawn point ; **Keeper NPC visible BUT interaction prompt INACTIVE per Section B "active เฉพาะ first-visit"** ; Monument dormant ; plaza ambient | Plaza ambient | HUD minimal | TR-WMS-004 ; **BLOCKER #12 — CRITICAL EXPOSURE** (post-first-visit City Menu access path UNSPECIFIED — Keeper prompt is gone, no documented hotkey, no persistent HUD button. Player has NO documented path to access Universal Services. Section B does not specify post-first-visit affordance) |
| **t ≈ match_end + 40s** (player wants to queue again) | Looks for queue UI ; finds nothing obvious | Server: no relevant state | Player avatar idle ; no UI prompt ; player must walk to find some other landmark? — **WHICH landmark? GDD does not specify which of 8 fixed landmarks (line 891) is the menu entry post-first-visit** | — | **NO UI surface accessible without documentation** | **BLOCKER #12** confirmed load-bearing — second visit and beyond fail at this row. The GDD specifies 8 fixed landmarks per city (Tournament Queue Terminal, Personal Dungeon Gate, Equipment Alcove, Hero/Skin Shrine, etc.) but does not state whether each landmark opens its own service UI directly OR whether City Menu is required as a hub. If individual landmarks open their own service UI, player must memorize 8 spatial locations. If City Menu is required, no documented entry path exists post-first-visit |

### Empty-cell blockers exposed by Scenario A

| Blocker | Severity | Where exposed | What's missing in cell |
|---|---|---|---|
| **#4** R12 atomicity claim overstated | BLOCKING | t=0 (login spawn writes 4 fields atomically) | TR-WMS-037 tests "partial-success" response that PlayFab API does not emit ; AC is testing phantom failure mode ; first-visit is the FIRST place this contract is exercised |
| **#5** R12 conflates Player Record + instance-pop CAS | BLOCKING | t=0 (D1 step 2 does both) | First-visit hits both stores in same atomic-feeling operation but they are different stores ; implementer trap surfaces here |
| **#12** Post-first-visit City Menu access UNSPECIFIED | BLOCKING (pillar P5) | t=match_end+~35s through +40s | Keeper prompt gone ; no documented hotkey ; no persistent HUD button ; player cannot reach Universal Services on second visit and beyond ; **load-bearing for entire returning-player UX** |
| **#17** Keeper NPC info-dump anti-pattern | BLOCKING (pillar) | t=T+10s, T+11s | Keeper functions as menu opener at the moment Section B promises sanctuary tone ; design rationale why info-dump serves sanctuary absent |
| **#18** EC-09 auto-blur text draft fate UNSPECIFIED | BLOCKING (UX) | t=T+~5min+ε (match-found while typing) | Player's typed message in friend-search loses ; discard vs preserve not specified ; common UX failure |
| **#19** OQ-7 platform decision no gate | BLOCKING | All input rows (T+10s, T+11s, T+30s, T+45s, etc.) | Every interaction assumes mouse+keyboard ; gamepad/touch shift requires substantive redesign ; provisional commitment becomes silent commitment |
| **narr-1** Keeper voice profile absent | BLOCKING (pillar narrative) | t=T+10s, T+11s | At the moment most critical for sanctuary fantasy delivery, the only voiced character has no voice ; writer fills slot blind |
| **narr-3** OQ-2 theme spec deliverable absent | BLOCKING (production) | t=T_load (ambient soundscape), t=T+0.5s (per-city beat text), t=T+0.5s (bell instrument), t=T+5s (Banner Rack faction visuals) | 4 cells require theme-bound content with no deliverable spec ; cascade-blocks Art + Audio + Asset Spec phases |
| **qa-5** R5 Browse-mode no-travel no AC | BLOCKING (qa) | t=match_end (post-match shortcut "เดินทางไป Event City") | If Galaxy Map opens automatically post-match, Browse vs Travel mode rules apply but no AC verifies behavior |

### Scenario A complete-row summary

- **Total rows:** 17 (including out-of-scope match-in-progress placeholder)
- **Rows with all 5 cells filled, no blocker reference:** 1 (t=T_load + 0.5s ambient soundscape minor — but still references narr-3)
- **Rows with at least one empty/TBD/blocker-marked cell:** 16
- **Distinct BLOCKING blockers exposed:** 9 (2 of which are Scenario A primary: #17 Keeper info-dump, #12 post-first-visit access — the other 7 are touched but better-exposed in other scenarios)
- **AC coverage:** TR-WMS-002, 004, 005, 031, 032, 041 + Manual-WMS-01 — 7 ACs reference this scenario directly

### Implication for revision pass (Phase 3)

**Cluster 8 (UX/Onboarding Tone)** must be addressed in this scenario's terms:
- **Blocker #17 + narr-1** are the same gap viewed from two angles: the Keeper has a mechanical role (open menu) but no narrative role (sanctuary keeper). Fix requires both: (a) voice profile + sample line at GDD level (in Section B or new dedicated subsection), (b) reframe Keeper's purpose from "tutorial NPC who hands out menu" to "ambient sanctuary keeper whose menu-opening is one of several offered services." This must precede UX spec authoring.
- **Blocker #12** is structural and must be answered before Phase 3 even begins. Decision needed: do the 8 fixed landmarks each open their own service UI directly (no City Menu hub), OR does City Menu remain a hub with a persistent hotkey/HUD button? GDD currently implies the latter without specifying access. Recommend: explicit hotkey + persistent HUD pin, with Keeper as decorative-and-optional supplementary entry post-first-visit. This unblocks Scenario B which depends on this answer.
- **Blocker #18** (EC-09 draft fate) is a one-line addition to EC-09: "text field content at time of auto-blur is [discarded / preserved as draft pending modal resolution]." Decide and document.
- **Blocker #19** (OQ-7 platform gate) needs a producer decision before any UX spec phase begins, OR an explicit revision-pass budget for gamepad/touch deferred to a later sprint with cost.

**Production gate (narr-3):** First-visit cannot ship without per-city environmental beat text (10 cities × ~50-word beat = ~500 narrative words minimum, but each must be tonally precise per Section B caravanserai metaphor). Currently unstaffed and uncosted. Producer notification needed parallel to the existing 80-writer-day vignette estimate.

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

**Player POV (dual):** Player A — `faction_id = "faction_b"`, state = `InCity` at city_astra_01 (pop = 87/150). Player B — `faction_id = "faction_d"`, state = `InCity` at city_solis_02 (pop = 92/150). A and B are existing friends (M1 friend list). Both have already finished first-visit (Keeper prompt inactive — so this scenario also depends on **BLOCKER #12** resolution from Scenario A).

**Time origin:** t=0 = Player A clicks "Invite to Party" on Player B's friend list entry.

**Knob context:** `CITY_INSTANCE_SOFT_CAP = 150`, `CITY_INSTANCE_HARD_CEILING_RATIO = 1.2` (hard ceiling = 180), `R16_ACKNOWLEDGE_RADIUS_UNIT` and `R16_ACKNOWLEDGE_COOLDOWN_SECONDS = 60` per PR #17 G.5, `CROSS_FACTION_PARTY_MATCH_PCT_ALERT` per PR #17 R9.1 telemetry.

**Critical context:** This scenario is the canonical exposure of **OQ-10 / Cluster 5** — R9 explicitly allows cross-faction party but Fragment routing per `player.faction_id` punishes the social behavior R9 enables. PR #17 added R9.1 telemetry but does NOT close the design issue.

| t | Player input/state | Server | Client visual | Audio | UI surface | AC ref / blocker |
|---|---|---|---|---|---|---|
| **t < 0** (setup) | A `InCity` city_astra_01, B `InCity` city_solis_02 ; both via M1 friend list see each other online | Heartbeats to PlayFab (EC-07 freshness window) | Each in own plaza ; cross-faction strangers in their respective plazas | Plaza ambient (theme-bound — **narr-3**) | A's HUD: friend list visible (M1 surface) showing B online with city indicator | (no AC — setup) ; **BLOCKER #12** prerequisite (both A and B accessing service menus post-first-visit) |
| **t = 0** (A clicks "Invite to Party") | Click invite | M1 server processes party-create + invite ; party state: leader=A, members=[A, B], destination=null ; emits invite event to B's session | Friend list shows "invite sent" indicator | UI confirmation cue | A's friend list updated | (M1-owned AC, out-of-FT12-scope) ; **BLOCKER #19** (gamepad/touch invite flow differs) |
| **t ≈ +2s** (B receives invite) | (no input) | M1 server delivers invite event to B's client via SignalR / Photon | A invite banner slides in on B's HUD ; banner shows A's faction badge (faction_b) per R7 | Invite cue (M1-owned) | Party invite banner (1 of 17 surfaces — line 957, "30s timeout") | (M1-owned) ; banner is FT12 UI consumer surface |
| **t ≈ +5s** (B accepts) | Click accept on banner | M1 commits party formation ; both clients receive `party_formed(leader=A, members=[A,B])` event | Party UI on both HUDs (M1 surface) ; banner closes on B | UI accept cue | Party panel persistent on both HUDs | (M1-owned) ; FT12 reads party state for D1 step 1 co-location |
| **t ≈ +6s** (B opens Galaxy Map to travel) | Press Galaxy Map hotkey | Server: no state mutation (read-only browse) | Galaxy Map overlay opens for B | UI overlay open cue | Galaxy Map overlay (1 of 17 surfaces, line 943) — **opens in Browse mode by default per R5 (PR #17 R5.1 refines)** | TR-WMS-050 (R5 Browse mode does not trigger travel — needed per **qa-5** ; AC pending) ; TR-WMS-043 (PR #17 R5.1 dual-purpose interrupt) |
| **t ≈ +7s** (B toggles to Travel mode) | Click "เลือกเมืองปลายทาง" toggle button | Server: no state mutation | Mode toggle visible ; header reads "กำลังเลือกเมืองปลายทาง" | UI mode-change cue | Galaxy Map header changes ; city nodes now travel-targets | TR-WMS-051 (Travel mode requires explicit toggle — needed per **qa-5**) |
| **t ≈ +8s** (B clicks city_astra node) | Click city_astra | Server presents confirm dialog (R5 mandate) | Confirm dialog overlays Galaxy Map | UI dialog cue | Confirm dialog: "เดินทางไป city_astra ?" | (R5 confirm dialog AC implicit in TR-WMS-006) |
| **t ≈ +9s** (B confirms travel) | Click confirm | Server: state `InCity` → `EnteringCity` for B ; D1 runs for B with party_size=2 (party-aware co-location) ; Step 1 co-location: party member A is `state == InCity`, `last_heartbeat ≤ 5s`, `current_instance_city_id == city_astra`, `current_instance_id == city_astra_01` ✓ → target = city_astra_01 ; CAS check: `pop=87+1=88 ≤ hard_ceiling=180` ✓ → CAS success → assign city_astra_01 | Galaxy Map fades out ; loading screen | Loading cue + GALAXY_MAP_TRANSITION_SECONDS=2.5s | Loading screen | TR-WMS-006 (R5 transition timing) ; TR-WMS-010 (R9 co-location) ; **BLOCKER #15** lurking — at higher pop, D1 `break` (line 520 of GDD) conflates CAS-conflict retry with ceiling-exceeded ; in this row CAS succeeds so blocker not exposed but logic path is fragile |
| **t ≈ +11.5s** (B's scene loads) | (auto) | State `InCity` for B ; presence broadcast to city_astra_01 instance peers | B's avatar materializes in city_astra_01 near A | Plaza ambient soundscape (theme-bound — **narr-3**) | HUD shows B in city_astra ; party panel updates with co-location indicator | TR-WMS-010 verifies `current_instance_id` of B matches A |
| **t ≈ +13s** (A and B meet visually) | A and B are now in same instance ; not yet in proximity chat range | Server tracks proximity for chat eligibility | A's faction_b badge + B's faction_d badge both visible to each other and to in-instance peers ; Faction Banner Racks update to include both factions per R7 + EC-23 | — | Both badges render in-world | TR-WMS-031 (FT11 read-only) ; **narr-6 nice-to-have** (Banner Racks now show both factions — does this read as "neutrality" or as "two factions claiming space"?) |
| **t ≈ +15s** (a stranger of faction_a in plaza performs R16 toward A) | (no input from A or B — stranger does it) | M1-routed action ; M11 opt-out check ; if not opted out → emit `r16_acknowledge(source, target)` | Stranger plays acknowledge animation ; A receives directional indicator that stranger acknowledged her ; **animation working name "Traveler's Salute" — narr-8 flags military framing inappropriate ; correct verb register: นบ / วันทา / แสดงคารวะ ; Thai working name "ผ่านลาน" is correct** | Acknowledge audio cue (theme-bound — undefined) | R16 indicator on A's HUD | TR-WMS-044 (R16 primitive) ; TR-WMS-045 (R16 cooldown — Logic+Visual conflated per qa-rec) ; TR-WMS-049 (R16 opt-out passthrough) ; **narr-8** (R16 vocabulary: "Salute" → replace with bow/nod register before UX spec) |
| **t ≈ +20s** (A opens City Menu to queue Tournament) | Press City Menu hotkey ?? | Server: read-only menu open | City Menu UI (Tournament Queue button highlighted) | UI open cue | City Menu — 8 services | **BLOCKER #12** confirmed exposure — A presses "City Menu hotkey" but GDD does not specify any hotkey ; if Scenario A blocker #12 is unresolved, this row fails. Scenario D depends on Scenario A's #12 fix |
| **t ≈ +22s** (A picks Tournament Queue, selects party-2 Casual) | Click Tournament Queue → select Casual → "Include party (2)" toggle | Server: opens Tournament Queue UI ; party-aware queue mode | Tournament Queue UI ; party indicator | UI cue | Tournament Queue UI surface | TR-WMS-032 (FT13 mock — but party-aware queue may need separate AC ; coverage gap per qa-lead pattern) |
| **t ≈ +25s** (A confirms 2-person Casual queue) | Click confirm | Server sends `(player_id_A, player_id_B, queue_type=casual_party_2, origin_city_id="city_astra")` to FT13 mock ; both transition `InCity` → `InCity+Queued` ; both `origin_city_id = "city_astra"` snapshot (B's origin is now city_astra, NOT city_solis where B started) | Both HUDs show queue indicator | Confirmation cue | Queue indicator persistent on both HUDs | TR-WMS-032 ; **note origin_city_id semantics:** B's `last_city_id` was city_solis at session start, became city_astra after travel ; B's `origin_city_id` snapshot at queue = city_astra. After match end (t = match_end + 30s), B will return to city_astra, NOT city_solis |
| **t ≈ +5min** (match found) | (no input — async) | FT13 emits `match_found` for both ; same priority hierarchy as Scenario A ; if either was typing → EC-09 auto-blur applies → **BLOCKER #18** | Match-found modal on both HUDs | Match-found alert | Match-found countdown modal | TR for state transition ; **BLOCKER #18** (EC-09 auto-blur draft fate UNSPECIFIED — applies to both A and B independently if either was typing) |
| **t ≈ +5min+8s** (both accept) | Both click Accept | State → `InMatch` for both | Match loading | Match-load cue | Match loading screen | TR-WMS-032 |
| **(t = match in progress — out of FT12 scope)** | A and B on same Casual team ; A from faction_b, B from faction_d | — | — | — | — | — |
| **t = match_end** (FT13 emits match_end) | (no input) | FT12 receives `match_end` for both ; **CRITICAL — Fragment routing logic per OQ-10:** A's match contribution routes to faction_b pool, B's match contribution routes to faction_d pool. R9.1 telemetry (PR #17) increments `CROSS_FACTION_PARTY_MATCH_PCT_ALERT` counter. **OQ-10 DESIGN ISSUE LIVE: if A is high-skill, A's wins are diluting A's own faction_b Fragment pool because A is partying with faction_d B. R9 social behavior is mechanically punished by R9-Fragment-routing rule.** | Post-match screen | Post-match cue | Post-match screen — Fragment award per individual faction routing | TR for state transition ; **BLOCKER #13 — CRITICAL EXPOSURE** (OQ-10 Fragment routing perverse incentive — live in current rules) ; TR-WMS-047 (R9.1 telemetry — PR #17, but only telemetry not fix) |
| **t = match_end + 30s** (auto-return) | (no input — auto) | Both transition `ReturningFromMatch` → `EnteringCity` ; destination = `origin_city_id = "city_astra"` for BOTH (B's city_solis "home" is no longer the return city) ; both spawn in city_astra_01 | Loading screen for both | Loading cue | Loading screen | TR-WMS-004 ; note for production: B may be confused that they returned to A's city, not their own — **UX risk worth flagging in /ux-design post-match-screen** |
| **t = match_end + 35s** (both back at city_astra) | Both `InCity` city_astra_01 ; B is away from B's pre-party home city_solis | Server presence broadcast | Both back in plaza ; can re-queue or travel separately | Plaza ambient | HUD minimal | TR-WMS-004 ; the system has no UX affordance for "B is now far from home" ; if B wants to return to city_solis, B must Galaxy Map travel back manually |

### Empty-cell blockers exposed by Scenario D

| Blocker | Severity | Where exposed | What's missing in cell |
|---|---|---|---|
| **#12** Post-first-visit City Menu access (cascade from A) | BLOCKING | t=+20s (A opens City Menu) | Same as Scenario A — A's queue flow requires hotkey/HUD access that GDD does not specify |
| **#13** OQ-10 Fragment routing perverse incentive | BLOCKING (pillar P3 vs P4) | t=match_end | Live in current GDD rules ; R9 enables cross-faction Casual social behavior ; Fragment routing rule punishes that behavior ; PR #17 R9.1 telemetry monitors but does not fix ; FT13/FT14 design must close — but FT13/FT14 are undesigned |
| **#15** D1 break/CAS-conflict path (lurking, not directly hit here) | BLOCKING (latent) | t=+9s (D1 step 1 CAS) | In Scenario D's setup pop=87, CAS succeeds trivially. At higher pop near hard_ceiling 180, D1 `break` (line 520) conflates CAS-conflict retry with ceiling-exceeded ; co-location can silently fail at step 2 with no signal. Scenario D shows the happy path ; the failing path requires Scenario E aggregate exposure |
| **#18** EC-09 auto-blur text draft fate | BLOCKING (UX) | t=+5min (match-found while either typing) | Same gap as Scenario A — applies independently to both party members |
| **#19** OQ-7 platform decision | BLOCKING | All input rows from t=0 onward | Every interaction (invite click, banner accept, Galaxy Map toggle, hotkey for City Menu, queue confirm) assumes mouse+keyboard ; gamepad/touch shift requires substantial redesign |
| **narr-3** OQ-2 theme spec | BLOCKING (production) | t=+11.5s (city_astra plaza ambient), t=+15s (acknowledge audio cue), Banner Rack visuals | Theme-bound content for B's first arrival at A's city ; cross-city travel is the moment city-to-city tonal contrast is most visible to player |
| **narr-6** nice-to-have | NICE-TO-HAVE | t=+13s (Banner Racks show both factions) | Plurality vs proportional faction display matters most when 2+ factions co-present in small instance |
| **narr-8** R16 "Salute" military framing | RECOMMENDED | t=+15s (R16 acknowledge animation) | Working name "Traveler's Salute" violates Section B prohibition on military vocabulary ; replace with นบ/วันทา/แสดงคารวะ register before UX spec |
| **qa-5** R5 Browse-mode no-travel AC | BLOCKING (qa) | t=+6s, +7s, +8s (Galaxy Map mode toggle sequence) | Three rows of Galaxy Map mode behavior, no AC verifies Browse mode does NOT trigger travel ; needs TR-WMS-050/051 |

### Scenario D complete-row summary

- **Total rows:** 19 (including out-of-scope match-in-progress)
- **Rows with all 5 cells filled, no blocker reference:** 0
- **Rows with at least one empty/TBD/blocker-marked cell:** 19
- **Distinct BLOCKING blockers exposed:** 6 (Cluster 5 primary: #13 ; Cluster 8 + qa + narr secondary)
- **AC coverage:** TR-WMS-006, 010, 031, 032, 004, 044, 045, 047, 049 + TR-WMS-043 (PR #17 R5.1) — 10 ACs reference this scenario directly (denser AC coverage than C/A — reflects more cross-system interactions)

### Implication for revision pass (Phase 3)

**Cluster 5 (OQ-10 Fragment routing) must be addressed before R9 ships:**
- Decision required: (a) Cross-faction Casual parties get Fragment routing exception (split evenly or to leader's faction) — closes perverse incentive but adds FT13/FT14 design complexity ; (b) Cross-faction party formation restricted to non-Fragment-bearing modes — preserves pillar but limits R9 social value ; (c) Accept dilution as documented design trade-off — must be explicit in GDD with rationale (e.g., "P3 social value > P4 mechanical purity")
- Whatever decision: GDD R9 must state the rule explicitly. Currently R9 enables the behavior and OQ-10 defers the resolution to undesigned FT13/FT14 — this is a live contradiction in shipping rules.

**Cluster 7 #15 (D1 break path)** is latent in Scenario D but load-bearing in Scenario E. Fix the `break` to split CAS-conflict retry from ceiling-exceeded paths, with explicit signal `party_co_location_ceiling_exceeded` firing on the latter. (Carries to Scenario E.)

**narr-8 R16 vocabulary** is a one-line fix in R16 spec — replace working name "Traveler's Salute" with "Wayfarer's Nod" or "Courtyard Bow" (English) ; preserve "ผ่านลาน" (Thai). This must precede UX spec animation authoring.

**Production note:** B returning to A's city after match (not B's pre-party home) is a UX consequence of `origin_city_id` semantics. Not a blocker, but worth flagging to /ux-design post-match-screen — possible improvement: post-match dialog offers "stay at city_astra (default)" vs "return to my last city (city_solis)" as explicit choice. Currently silent default.

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
- [x] **Scenario A authored (2026-05-02)** — 17 rows, 9 distinct BLOCKING blockers exposed (primaries: #17 Keeper info-dump, #12 post-first-visit access)
- [x] **Scenario D authored (2026-05-02)** — 19 rows, 6 distinct BLOCKING blockers exposed (primary: #13 OQ-10 Fragment routing perverse incentive — pillar P3-vs-P4)
- [ ] Scenario B authored (depends on #12 resolution)
- [ ] Scenario E authored
- [ ] Blocker → Walkthrough Mapping verified against authored content
- [ ] Phase 2 complete; Phase 3 (cluster-by-cluster GDD revision) can begin

---

## Next session

Author **Scenario E — Launch-Spike** (server-aggregate view). Primary blocker
coverage: **Cluster 2 (R12 PlayFab CAS / atomicity)** + **Cluster 3 (capacity
math — EC-19 prewarm coverage, EC-14 ghost slots, PlayFab API rate limit)** +
Cluster 6 (D2 5s tick execution environment) + Cluster 7 #15 D1 break path
(load-bearing here, latent in D). Differs from Scenarios A/C/D in being aggregate
(5000 simultaneous players) rather than single-player slice — surfaces capacity
issues only visible at scale.

Then **Scenario B — Return-Visit-No-Event** last (depends on #12 resolution from
Scenario A — Scenario B's entire session lives in post-first-visit state, so
authoring before #12 has a proposed fix would produce a placeholder-heavy table).
