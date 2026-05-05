# World Map System (FT12) — Review Log

## Review — 2026-04-28 — Verdict: NEEDS REVISION
Scope signal: XL
Specialists: game-designer ✓, systems-designer ✓ — network-programmer / ux-designer / qa-lead / narrative-director / creative-director **unavailable (org monthly usage limit hit during spawn)**
Blocking items: 8 | Recommended: 6
Summary: First review of FT12. GDD is structurally complete (8/8 sections) with unusually thorough Open Questions, edge cases (27), and ACs (35). systems-designer found 4 algorithmic/data-integrity defects in D1 (missing in-algorithm hard ceiling, wrong freshness predicate, undefined Player Record atomicity, untestable G.5 fairness window) plus a knob safe-range that permits violating G.6 constraint. game-designer found 3 fantasy-delivery holes (R3 Anchor one-and-done, no plaza linger reason, no stranger-interaction affordance). Verdict NEEDS REVISION. Coverage gaps: network/ux/qa/narrative/creative-director did not run — recommend re-review in fresh session before R13 sharding ADR is authored.
Prior verdict resolved: First review

### Blocking items (resolved in same session 2026-04-28)
1. [systems-designer] EC-06 hard ceiling missing from D1 pseudocode → fixed inline in D1 step 1 with `hard_ceiling = floor(SOFT_CAP × HARD_CEILING_RATIO)` check + emit_signal on overflow
2. [systems-designer] G.6 knob constraint violable within safe ranges → narrowed `D2_SCHEDULER_TICK_SECONDS` safe range from 1–60 to 1–6; added CBS validation rule `lead ≥ 5 × tick` enforcement
3. [systems-designer] Player Record write atomicity unspecified → R12 expanded with atomic-transaction contract for 4 fields + `LookupInstance` read-repair behavior for stale `current_instance_id`
4. [systems-designer] EC-07 freshness predicate inverted → D1 step 1 gate strengthened to `state == InCity AND last_heartbeat_age ≤ 5s` (excludes `EnteringCity` transient instance_id)
5. [systems-designer] G.5 fairness index time window undefined → specified Gini of `unique_visitors_per_city` (10 cities incl. zeros), rolling 24h sample, daily UTC snapshot, 7 consecutive days trigger
6. [game-designer] R3 Anchor Content has no repeat-visit hook → added R3.1 Rotating Lore Vignette (pool ≥ 4 per city, default 7-day rotation) + 2 knobs in G.3 (`ANCHOR_VIGNETTE_ROTATION_DAYS`, `ANCHOR_VIGNETTE_POOL_MIN`)
7. [game-designer] No designed reason to linger in plaza between events → added R15 Plaza Ambient Social Affordances (≥ 4 categories: bench/co-sit, emote spots, fountain interaction, ambient gathering hooks); inherits R3 no-mechanical-reward rule
8. [game-designer] No stranger-interaction affordance → OQ-9 added (defer to UX spec); R15 affordances serve as scaffold; OQ-9 explicitly notes Section B framing limited to presence (not encounter) until UX resolves

### Recommended (NOT addressed this session — open for re-review)
9. [systems-designer] D1 first-fit-descending fragmentation at scale — no operational alert; consider `CITY_INSTANCE_FRAGMENTATION_ALERT` knob or defragmentation policy
10. [systems-designer] Hard ceiling 180 vs density chaos contradiction — G.1 calls high cap "density chaos / chat flood"; ceiling 180 already past tolerable. Lower ratio or document trade-off
11. [systems-designer] EC-19 750→1000 overage path untested — TR-WMS-034 jumps to 5000/10s; small-overflow case has no AC
12. [game-designer] R10 proximity-only chat + sharding kills city-wide collective Fragment Event moment — bell is the only cross-instance synchronous beat; consider non-chat collective-awareness cue
13. [game-designer] P3 cross-faction party vs P4 Fragment routing perverse incentive — high-skill players diluting their own faction pool unaddressed
14. [game-designer] First-visit beat conflates functional (where do I queue) and tone (sanctuary) onboarding — separate in UX spec

### Coverage gaps (specialists not consulted)
- network-programmer — R8 cross-instance broadcast budget, R12 hot-row scaling, EC-25 clock compensation tolerance, M11 sync filter latency, EC-16 graceful drain plan unreviewed; **recommend dedicated pass before R13 sharding ADR is authored**
- ux-designer — 17 UI surfaces, Galaxy Map dual-purpose mode, EC-09 priority hierarchy contradiction, OQ-7 input model, accessibility section "[TO BE CONFIGURED]" all unreviewed
- qa-lead — 35 ACs not validated for testability; R1–R14 → AC coverage matrix not built; 11 of 27 ECs without AC not flagged
- narrative-director — R3 "faction history hall" vs absolute neutrality, OQ-2 themes blocking art production, EC-11 Thai phrasing, tone-bible cross-system consistency unreviewed
- creative-director — no senior synthesis

### Next step
Re-review in fresh session: `/clear` then `/design-review design/gdd/world-map-system.md` to obtain full specialist coverage on the revised draft.

---

## Review — 2026-04-29 — Verdict: NEEDS REVISION
Scope signal: XL (unchanged)
Specialists: game-designer ✓, systems-designer ✓, network-programmer ✓, ux-designer ✓, qa-lead ✓, narrative-director ✓, creative-director ✓ (full coverage achieved)
Blocking items: 22 | Recommended: 18 | Nice-to-have: 4
Summary: Second review of FT12 with full specialist coverage. systems-designer verified 5 first-pass fixes but found 3 prose-vs-architecture gaps (R12 atomicity overpromised PlayFab/Azure Functions stack; EC-06 hard ceiling has TOCTOU race; STARTER_CITY_ID null-crashes EC-13 fallback). game-designer found 3 unresolved fantasy holes from first review's recommended items (R15 set-dressing without pull mechanism; R8 Anchor moment instance-scoped not city-scoped; P3/P4 cross-faction Fragment routing perverse incentive). network-programmer raised 4 R13-ADR-blocking items (R8 fan-out mechanism, R12 PlayFab co-location constraint, LookupInstance hot-row, EC-08 sync filter SLA). ux-designer raised 5 blockers (Accessibility section absent, OQ-7 platform deferral, EC-09 silent decision window, Galaxy Map mode confusion, first-visit beat conflation). qa-lead found 5 AC gaps (R3.1, R12, R15, EC-14, EC-18 + performance staging spec undefined). narrative-director raised 4 (faction history hall vs neutrality, OQ-2 ownership, 80 writer-day vignette scope uncosted, EC-11 broken Thai). creative-director synthesis: NEEDS REVISION — architecture recoverable, no fundamental rework required, 22 blockers cluster around 4-5 root causes.

Prior verdict resolved: Yes — first review's 8 blockers verified by systems-designer; first review's coverage gaps (network/ux/qa/narrative/creative-director) all closed this pass.

### Blocking items (resolved in same session 2026-04-29)
**Cluster A — R12 Atomicity Architecture (3 voices)**
1. [systems-designer] R12 atomicity overpromises platform → PlayFab single `UpdateUserData` blob constraint added; multi-call pattern explicitly forbidden
2. [network-programmer] R12 architectural constraint → all 4 fields must stay co-located in one PlayFab user-data partition (stated as constraint, not implementation note)
3. [qa-lead] R12 rollback no AC → TR-WMS-037 added (fault-injection test for partial-write rollback)

**Cluster B — R8 Cross-Instance Anchor Moment (2 voices, pillar-level)**
4. [game-designer] R8 + R10 + R13 = instance-scoped Anchor moment → R8 reframed as 30-second bell-as-linger sequence with cross-instance presence flash; R15 demoted to secondary linger layer
5. [network-programmer] R8 fan-out unspecified → `R8_CROSS_INSTANCE_SKEW_BUDGET_SECONDS` knob (default 3, range 1-5) + behavioral contract; mechanism deferred to R13 ADR with named constraint

**Cluster C — Sanctuary Fantasy / Pillar Integrity**
6. [game-designer + narrative-director] R3 "faction history hall" violates R1 → replaced with chronicle-of-the-war + neutral examples; faction-privilege Anchor pattern explicitly banned
7. [game-designer] R15 affordances are prop list → R15 reframed as secondary; bell sequence is primary linger
8. [game-designer] P3/P4 perverse incentive → OQ-10 added flagging FT13/FT14 to address Fragment routing exception for cross-faction Casual

**Cluster D — Data Integrity / Race Conditions**
9. [systems-designer] EC-06 TOCTOU race → CAS via PlayFab version token / CosmosDB ETag stated; D1 step 1 pseudocode updated to `TryClaimSlots`; TR-WMS-038 added
10. [systems-designer] STARTER_CITY_ID null-crash → provisional `city_01` default + server-startup config validator (refuse boot if invalid); OQ-1 + EC-13 + G.1 updated
11. [network-programmer] LookupInstance hot-row → read-through cache (TTL ≤ 5s) constraint added to R12
12. [network-programmer] EC-08 filter latency → `M11_FILTER_LATENCY_BUDGET_MS` (default 150) + 500ms timeout + degraded-mode fallback (broadcast + flag pending async review)

**Cluster E — UX / Onboarding / Accessibility**
13. [ux-designer] Accessibility section [TO BE CONFIGURED] → 4 mandatory minimums populated (subtitle for diegetic bell, colorblind-safe faction differentiation, input remapping, text scale 100-150%)
14. [ux-designer] OQ-7 platform deferral → provisional PC + mouse+keyboard documented + revision-pass budget required for any platform change
15. [ux-designer] EC-09 deferred countdown silently reduces decision window → effective floor 8s mandated; popup fires no later than t=2s (countdown_total − 8s) regardless of input focus
16. [ux-designer] Galaxy Map dual-purpose mode confusion → mandatory Browse mode (default, detail panel only) vs Travel mode (explicit toggle, confirm dialog) separation
17. [game-designer + ux-designer] First-visit beat conflates tone + functional → Section B explicitly splits: environmental beat = tone-only (atmospheric); Keeper NPC = default functional orientation mechanism

**Cluster F — AC Coverage Gaps**
18. [qa-lead] R3.1 vignette rotation no AC → TR-WMS-036 added
19. [qa-lead] R15 affordances no AC → TR-WMS-039 added (no-reward verification)
20. [qa-lead] EC-14 + EC-18 state-machine ACs missing → TR-WMS-040 (network-drop retry → OutOfUniverse), TR-WMS-041 (EnteringCity timeout 30s) added
21. [qa-lead] Performance ACs not reproducible → staging environment spec mandate added (`production/qa/environment-spec.md` reference); single-threaded harness specified for TR-034; TR-WMS-042 added for 751-999 mid-range overflow gap

**Cluster G — Narrative**
22. [narrative-director] EC-11 broken Thai phrasing "ออกจากลานเมืองโดยกำเนิด" → replaced with "ออกจากลานแล้ว" (neutral, dignity-preserving)
+ [narrative-director] OQ-2 owner switched from Art-led → Narrative-led (lead) + World Builder + Art Director (co-own); cascade-block on OQ-3 + R3.1 made explicit
+ [narrative-director] OQ-3 producer notification added: 40-vignette MVP load = ~80 narrative writer-days; explicit producer flag before sprint commit

### Recommended (NOT all addressed this session)
- [systems-designer] D1 first-fit-descending fragmentation alert/knob — defer to R13 ADR
- [systems-designer] G.1 SOFT_CAP=300 vs HARD_CEILING_RATIO=1.2 contradiction — accept as documented trade-off
- [systems-designer] G.6 validator formal predicate — defer to CBS validator implementation
- [network-programmer] D2 5s scheduler tick infeasible on PlayFab CloudScript native — defer to R13 ADR (execution environment selection)
- [network-programmer] EC-25 client-side compensation model — defer to FT14 schema
- [network-programmer] EC-16 graceful drain timeout knob — defer to ops playbook
- [ux-designer] Travel-Queue dialog should display queue type + elapsed wait — defer to UX spec
- [ux-designer] 17 surfaces consolidation review — defer to /ux-design city-menu
- [ux-designer] "ลานร่วม Y" label ambiguous — defer to UX spec
- [ux-designer] EC-26 cardinal direction camera-rotation-unsafe — defer to UX spec
- [qa-lead] TR-008 static-analysis tool unspecified — defer to CI setup
- [qa-lead] Manual ACs no sign-off owner — defer to QA process doc
- [qa-lead] EC-09/12/13 incorrectly deferred — partial: EC-13 fix included; EC-09/12 still in H.7
- [qa-lead] Mock M11 latency budget absent — partial: M11_FILTER_LATENCY_BUDGET_MS knob added but mock contract not pinned
- [game-designer] R3.1 28-day repeat cycle thin — narrative team to address ahead-of-rotation pool
- [narrative-director] FT11/FT12 onboarding line underperforms tone — defer to UX spec narrative writer pass
- [narrative-director] Ambient NPCs need theme/behavior spec — defer to Asset Spec
- [narrative-director] FT11 OQ-5 (faction identity) is FT12 pre-production gate — flag to producer

### Nice-to-have (deferred)
- [game-designer] G.5 Gini=0.3 worked example
- [game-designer] EC-22 ambient NPCs cluster near Monument
- [narrative-director] 10 unique bell sounds — flag to audio-director
- [systems-designer] EC-19 751–999 player gap — partially addressed via TR-WMS-042

### Coverage gaps
None this pass — all 6 specialists + senior synthesis ran successfully.

### Next step
Re-review in fresh session: `/clear` then `/design-review design/gdd/world-map-system.md` to verify revisions hold under cold-context specialist re-evaluation. Validation criteria: all 22 Required items closed with architecture-backed (not prose-backed) resolutions; STARTER_CITY_ID provisional confirmed; accessibility minimums confirmed; 7 new ACs (TR-036 through TR-042) confirmed testable.

---

## Additive Follow-up — 2026-04-30 — Verdict: ADDITIVE PATCH (no review re-run)
Scope signal: M (additive only — no contradictions to 2026-04-29 design)
Specialists: parallel /design-review session (6 specialists + creative-director synthesis) ran on a separate branch before merge of #15 ; this patch applies only the items that do NOT conflict with the 2026-04-29 revisions
Items: 7 additive (5 new specs + 7 new ACs + 2 new G.5 knobs)
Summary: Parallel review session conducted on separate branch produced overlapping findings with the 2026-04-29 review (PR #15) but using different design choices in some areas (R15 cat 5 vs R15 demote; EC-09 auto-blur vs 8s floor; OQ-9 → R16 vs OQ-9 left open). PR #16 was reopened against main with only the truly additive items extracted — no contradictions to PR #15. The 2026-04-29 design decisions (R8 bell-as-linger, R15 secondary-linger, EC-09 8s floor, OQ-10 FT13/FT14 deferral) are preserved in full.
Prior verdict resolved: Yes for OQ-9 (now R16) ; partial for prior-log items 9 (D1 fragmentation — now has CITY_INSTANCE_FRAGMENTATION_ALERT knob) and 13 (cross-faction Fragment perverse incentive — now has CROSS_FACTION_PARTY_MATCH_PCT_ALERT telemetry as interim while OQ-10 awaits FT13/FT14)

### Additive items applied (2026-04-30)
1. **R5.1 — Galaxy Map dual-purpose interrupt rule** — refines R5 mode separation from /design-review #2: defines passive update behavior when Fragment Event arrives during active Travel-mode selection
2. **R9.1 — Cross-faction Fragment routing telemetry position** — interim metric while OQ-10 awaits FT13/FT14 design decision; closes prior-log item 13 at the telemetry layer (not the design layer)
3. **R16 — Cross-Faction Stranger Acknowledge Primitive** — resolves OQ-9 by committing 1-button acknowledge action with no mechanical reward, cross-faction visual emphasis, 60s per-target cooldown, M11-routed opt-out
4. **EC-26 sequencing rule** — bell t=0, indicator t=+0.3s, Galaxy Map passive
5. **G.5 `CITY_INSTANCE_FRAGMENTATION_ALERT`** — closes prior-log item 9
6. **G.5 `CROSS_FACTION_PARTY_MATCH_PCT_ALERT`** — backs R9.1 metric exposure
7. **G knobs `R16_ACKNOWLEDGE_RADIUS_UNIT` + `R16_ACKNOWLEDGE_COOLDOWN_SECONDS`** — back R16 contract
8. **7 new ACs (TR-WMS-043 to 049)** — cover R5.1, R9.1 telemetry, R16 (×3 covering primitive/cooldown/opt-out passthrough), EC-26 sequencing, G.5 fragmentation alert

### Items NOT brought into this patch (would conflict with 2026-04-29 design)
- R15 cat 5 Monument ambient countdown — conflicts with 2026-04-29 R15 demotion (bell sequence is primary linger ; R15 is secondary)
- EC-09 auto-blur — conflicts with 2026-04-29 EC-09 8s countdown floor approach
- Section B "ผู้เล่นหลากฝ่าย" rewording — already addressed via 2026-04-29 R8 bell-as-linger reframe
- TR numbering (parallel session used TR-WMS-036 to 051) — conflicts with 2026-04-29 numbering (TR-WMS-036 to 042) ; this patch uses TR-WMS-043 onwards to avoid collision

### Coverage gaps
None this pass — additive only.

### Next step
Re-review in fresh session: `/clear` then `/design-review design/gdd/world-map-system.md` to verify the combined post-#15 + additive state holds. Particular focus areas for verification: (a) R16 design contract completeness vs 2026-04-29 R8 bell-as-linger interaction, (b) R9.1 telemetry sufficiency given OQ-10 still open, (c) AC coverage for new R5.1/R9.1/R16/EC-26 sequencing/G.5 fragmentation hooks, (d) absence of contradictions between R15 secondary-linger framing and any plaza R16 acknowledgment overlap.

---

## Review — 2026-04-30 — Verdict: MAJOR REVISION NEEDED
Scope signal: XL (unchanged)
Specialists: game-designer ✓, systems-designer ✓, network-programmer ✓, ux-designer ✓, creative-director ✓ (synthesis) — **qa-lead ✗, narrative-director ✗ (org monthly usage limit hit during spawn)**
Blocking items: 19 | Recommended: 19 | Nice-to-have: 2

> **GDD snapshot caveat:** This review ran on a branch cut **before** PR #17 (ad9a442) was merged. PR #17 closes recommended-list **OQ-9 stranger interaction** via R16 and prior-log item 9 (D1 fragmentation alert) via G.5 `CITY_INSTANCE_FRAGMENTATION_ALERT`. Those items in this review's Recommended/prior-log scope are now CLOSED. PR #17 is purely additive and does NOT touch any of the 19 blockers below (R8 bell, R12 atomicity, R3.1 vignette, R15, R9 Fragment routing, capacity math, D1 algorithm). PR #17's R9.1 adds telemetry for cluster 5 blocker 13 but does NOT resolve the design issue (OQ-10 still open, perverse incentive still live).

Summary: Third cold-context review of FT12. Round-2 fixes structurally hold but cold-context adversarial re-read surfaces deeper issues across 8 root-cause clusters. Pattern identified by creative-director: round 2 added mechanisms (bell sequence, vignette rotation, R15 demotion, D1 simplification) **without adding the player-facing or implementation-facing affordances that make those mechanisms deliver** — "added the noun, not the verb" failure mode. Three pillar-threats live in current GDD (P3 collective moment via R8 bell-as-counter, P5 repeat-visit via invisible vignette rotation + zero-pull R15, P3-vs-P4 conflict via R9 Fragment routing perverse incentive). Methodology failure on capacity math (3 independent arithmetic errors all under-counting load: EC-19 prewarm 15% coverage, EC-14 ghost slots 67% phantom occupancy, D1 PlayFab API rate budget exceeded). R12 mutually inconsistent with own tests (TR-037 partial-success) and PlayFab API reality (atomicity claim overstated; CAS conflates per-key user-data version with instance population CAS — different stores). Coverage gap material: qa-lead would likely escalate AC contradictions across TR-034/037/038; narrative-director would likely escalate Keeper info-dump tone + bell collective-moment delivery to pillar-threat. Verdict cannot be locked without re-running both gates.

Prior verdict resolved: Partial — round 2's 22 blockers structurally addressed, but several were "noun added, verb missing" (bell mechanism present, behavioral cue missing; vignette rotation present, freshness signal missing; R15 demoted, primary linger now bell-only-during-events; D1 simplified, CAS-conflict path silently merged with ceiling-exceeded path).

### Blocking items (NOT addressed this session — open for revision pass)

**Cluster 1 — R8 Bell Collective Moment (P3 pillar-threat)**
1. [game-designer] R8 delivers density counter not collective moment — no avatar behavioral cue spec; "ทุกคนหันมามองกัน" undeliverable
2. [network-programmer] R8 skew range lower bound 1s unachievable on Azure Function Consumption (cold-start 200–800ms)
3. [network-programmer] R8 bell + RadiusChat cross-channel ordering undefined — hybrid Photon+SignalR fragments collective moment within skew budget

**Cluster 2 — R12 PlayFab CAS / Test Reality (implementer trap)**
4. [systems-designer + network-programmer] R12 atomicity claim overstated — PlayFab UpdateUserData has no "partial-success" response; TR-WMS-037 tests non-existent failure mode
5. [systems-designer] R12 conflates two CAS systems — PlayFab user-data per-key version (Player Record) vs instance population CAS (CosmosDB/Redis/lock — different store)
6. [network-programmer] R12 cache layer: distributed cache (Redis) required, not "any cache"; named constraint missing

**Cluster 3 — Capacity Math Internally Inconsistent (methodology failure)**
7. [systems-designer] EC-19 prewarm 750/5000 = 15%; TR-WMS-034 "<10% step-3 invoke rate" mathematically impossible (real ~85%)
8. [network-programmer] EC-14 ghost slots 60s @ 10% disconnect = 67% phantom occupancy of pre-warmed launch capacity; uncounted in EC-19
9. [network-programmer] D1 ~4 PlayFab calls/req × 500 RPS = ~2000 calls/s exceeds default PlayFab rate limits; not in TR-034 / staging spec

**Cluster 4 — Linger / Repeat-Visit Hook Broken (P5 pillar-threat)**
10. [game-designer + ux-designer] R3.1 vignette rotation locked server-clock + no per-player seen-state + no player-facing freshness signal → habitual visitors see same vignette indefinitely
11. [game-designer] R15 affordances zero pull mechanics; demotion to "secondary" makes it the only linger when no Fragment Event
12. [ux-designer] Post-first-visit City Menu entry path UNSPECIFIED — Keeper interaction prompt first-visit only; no documented hotkey/HUD button for returning players

**Cluster 5 — OQ-10 Live Perverse Incentive (P3 vs P4 pillar-threat)**
13. [game-designer] R9 cross-faction party + Fragment routing per `player.faction_id` = high-skill players punish own faction Fragment pool. Live in FT12 rules today, not deferred. P3 actively undermined by P4 in current R9

**Cluster 6 — Execution Environment Pinning (deferred-to-ADR but blocks GDD)**
14. [network-programmer] D2 5s tick requires Azure Functions Premium + pre-warmed; Consumption tier infeasible. Must be pinned in GDD as binding constraint (not deferred to R13 ADR) because timing budgets are pillar-load-bearing for P3

**Cluster 7 — D1 Algorithm Defects (implementer trap)**
15. [systems-designer] D1 `break` (line 520) conflates CAS-conflict retry with ceiling-exceeded; co-location silently fails at step 2; signal `party_co_location_ceiling_exceeded` does not fire on CAS-conflict path
16. [systems-designer] G.6 LEAD min=10s + tick=2s = exactly 5 ticks zero jitter headroom; formula `floor(LEAD/5)` permits fragile boundary

**Cluster 8 — UX / Onboarding Tone Conflict**
17. [game-designer] Keeper NPC at spawn = classic info-dump anti-pattern; chosen as default functional orientation without justifying why this serves sanctuary tone (narrative-director not consulted — likely pillar-threat escalation)
18. [ux-designer] EC-09 auto-blur on Priority-1 modal: text draft fate (discard vs preserve) UNSPECIFIED
19. [ux-designer] OQ-7 platform decision has no gate blocking `/ux-design galaxy-map` start; provisional assumption silent commitment

### Recommended (NOT addressed this session)
- [systems-designer] Slots in (SOFT_CAP, hard_ceiling] permanently dead for step 2; undocumented operational invisibility
- [systems-designer] G.5 Gini=0.3 baseline under intended Fragment Event distribution = ~0.42 (computed); threshold fires routinely; recalibrate
- [systems-designer] TR-WMS-038 self-contradicts in-line ("actually:" parenthetical correction); rewrite as clean Given-When-Then
- [systems-designer] PARTY_MAX unenforceable — not in entities.yaml; CBS validator cannot enforce G.6 constraint
- [network-programmer] EC-08 SLA 150/500ms achievable only when Photon + M11 co-located in same Azure region; constraint missing
- [network-programmer] R9 hard ceiling 180 not validated against Photon Fusion 2 NetworkObject bandwidth at 150–180 players
- [network-programmer] M1 friend-list ghost-presence not covered by "syncing presence..." mitigation (party-only)
- [game-designer] 9 cities mostly irrelevant on non-Fragment-Event days; R3 Anchor strips mechanical weight; P4 pillar claim inconsistent
- [game-designer] P5 Anti-Toxicity single-point-of-failure on undesigned M11; degraded mode = unfiltered broadcast (not explicitly acknowledged)
- ~~[game-designer] OQ-9 stranger interaction affordance must be in Detailed Rules~~ **CLOSED by PR #17 (ad9a442) R16 Cross-Faction Stranger Acknowledge Primitive** — merged after this review's GDD snapshot
- [ux-designer] 17-surface navigation load — tournament-queue critical path needs to be named as UX spec constraint
- [ux-designer] Galaxy Map Browse-default inverts majority-case travel intent; consider mode-by-state
- [ux-designer] Keeper NPC walk-past fallback floor unspecified
- [ux-designer] Motor (hold-to-confirm) + cognitive accessibility absent from mandatory minimums
- [ux-designer] Cross-instance Y counter intent (ambient social proof vs decision-support) UNSPECIFIED
- [ux-designer] Travel-Queue dialog omits queue type identification
- [ux-designer] EC-03 toast auto-dismiss + retry affordance UNSPECIFIED
- [ux-designer] R15 sightline-from-spawn layout minimum missing
- [ux-designer] Manual-WMS-01 criterion 3 tester-subjective; needs golden-text ref or vocabulary constraint

### Nice-to-have
- [systems-designer + game-designer] R12 read-repair paragraph duplicated (lines ~275–282 = ~296–303); copy-paste artifact
- [game-designer] R3 Anchor examples all solo-consumption types; no relational/social Anchor types in cross-faction-encounter hub

### Coverage gaps (CRITICAL)
- **qa-lead** did not run — would likely escalate AC contradictions across TR-034/037/038 (three independent test/architecture inconsistencies); high confidence additional BLOCKING AC items
- **narrative-director** did not run — would likely escalate Keeper info-dump tone, bell collective moment, vignette content scope, 9-cities worldbuilding-payoff vs mechanical-irrelevance to pillar-threat severity

### Senior synthesis [creative-director]
Pattern: round 2 added nouns (mechanisms) without verbs (player-facing/implementation-facing affordances). Three pillar-threats live + methodology failure on capacity math + R12 mutually inconsistent with tests and PlayFab API. Not "two more weeks of polish" — needs "one more design pass with a different methodology" (top-down, walkthrough-driven).

Required for next revision pass:
- Pillar-delivery walkthroughs for 4 scenarios (first-visit, return-visit-no-event, return-visit-with-event, cross-faction-party, launch-spike) — moment-by-moment table: trigger → server → client → audio → visual → AC
- Top-down launch-spike capacity model with explicit ghost-slot accounting + PlayFab rate-limit headroom
- R12 re-authored with second CAS store named (Redis/Cosmos/lock); TR-WMS-037 removed or rewritten to test what architecture actually does
- Execution-environment binding constraint section (Functions Premium, Redis, region co-location)
- OQ-10 closed before R9 ships, or R9 explicitly gated behind OQ-10
- Re-run qa-lead and narrative-director gates before next pass

### Next step
**Do NOT lock verdict yet.** Revision must happen in a separate session (out of scope for this review) and must:
1. Re-run qa-lead + narrative-director after monthly usage limit resets to confirm coverage
2. Apply methodology change (top-down walkthrough-driven) per creative-director synthesis
3. Re-review with `/design-review design/gdd/world-map-system.md` (full mode) after revision

This session ends with findings logged and FT12 status flagged MAJOR REVISION NEEDED in systems-index.

---

## Coverage Closure Pass — 2026-04-30 — qa-lead independent assessment
Specialist: qa-lead ✓ (closes coverage gap from third review)
Blocking items: 9 (3 confirm prior + rewrite specs ; 6 net-new) | Recommended: 10
Prior verdict resolved: No — third review's 19 blockers all stand; this pass adds 6 net-new blockers. Total now: **25 BLOCKING / 29 RECOMMENDED**.

Summary: qa-lead ran independently after main session re-spawn (rate limit was burst-throttle on parallel spawn, not monthly quota). Independent AC audit covered all 49 ACs (TR-WMS-001 to 042 + TR-043 to 049 from PR #17). Confirms 3 prior blockers (TR-034/037/038) with specific rewrite guidance, and surfaces 6 net-new BLOCKING items not visible to other specialists: 3 missing AC sets (R12 read-repair/cache TTL, R5 Browse-mode no-travel, R8 cross-instance skew), 1 process gap (`production/qa/` directory does not exist — blocks performance + manual ACs from execution), 1 GDD-level gap (PlayFab rate limit must be AC + env-spec), 1 untestable AC (TR-048 timing tolerance without Unity instrumentation hook spec).

### Blocking items (NOT addressed this session)

**Confirms prior + rewrite specs**
1. TR-WMS-034 "<10% step-3 invoke rate" mathematically impossible at EC-19 5000/750 capacity → rewrite to separate pre-warm cohort (assert step-3 = 0%) from overflow cohort (assert count-ceiling for ~85% overflow rate)
2. TR-WMS-037 "mock PlayFab partial-success response" tests phantom failure mode → rewrite to test HTTP 500 total failure; remove "partial-success" language
3. TR-WMS-038 self-contradicting Given-When-Then → split into TR-038a (170+12 both-reject) and TR-038b (156+12 sequential CAS success)

**Net-new from qa-lead independent audit**
4. R12 read-repair behavior + cache TTL (≤5s) have no ACs → add TR-053 (null-instance fallthrough verification) + TR-054 (cache TTL boundary)
5. R5 Browse-mode no-travel rule + Travel-mode explicit-toggle requirement have no ACs → add TR-050 (Browse mode click does NOT trigger travel) + TR-051 (Travel mode requires explicit toggle)
6. R8 cross-instance skew budget (3s) has no AC → add TR-052 (multi-instance bell timestamp within 3s budget)
7. `production/qa/` directory does not exist on disk; `environment-spec.md` referenced by TR-034/035/042 absent → directory + stub must be created before any performance or manual test can execute
8. PlayFab API rate limit (~2000 calls/s at 500 RPS D1 path vs ~1000 calls/s default plan) → add rate-limit assertion to TR-034/035; spec PlayFab CCU plan in env-spec.md
9. TR-048 timing tolerance untestable without instrumentation spec → downgrade to Manual/Visual OR specify Unity instrumentation hook for ±50ms client-side signal-ordering measurement

### Recommended (NOT addressed this session)
- TR-WMS-002 fragile count assertion ("response includes all 8 services") — refactor to assert specific service-IDs to prevent silent regression on future service additions
- EC-09 partial deferral inconsistent — H.7 lists EC-09 as deferred but TR for input-focus deferral exists; reconcile
- TR-WMS-013 ambiguous precondition ("client sends request bypassing validation") — what mechanism? Forged HTTP header? Direct PlayFab call? Specify
- TR-WMS-036 vignette rotation verification non-terminating — "verify next visit shows different vignette" requires unbounded simulation; specify max iterations or rotation cycle assertion
- 6 ECs silently uncovered: EC-04 (idle teardown), EC-16 (server restart soft kick), EC-17 (account deletion mid-instance), EC-21 (Anchor concentration alert), EC-24 (clock skew display), EC-27 (texture pop-in grace) — flag in H.7 as deferred or add ACs
- TR-WMS-045 (R16 acknowledge) conflates Logic + Visual story types — split
- Manual AC sign-off ownership absent (Manual-WMS-01, 02) — process gap
- Integration test infrastructure gap — no documented mock harness for PlayFab, Photon Fusion, M11 endpoints; integration ACs untestable until provided
- AC story-type ratio: Logic 28 / Integration 13 / Performance 3 / Manual 2 — Integration count low for a system with 7 cross-system handoffs; mocking may be overused
- TR-WMS-029 EC-23 faction switch live update tolerance "within 2s" — verify against actual presence broadcast SLA (R12 read-through cache TTL ≤ 5s); 2s may be tighter than infrastructure delivers

### Coverage gap remaining
- **narrative-director** still has not run — pending for next session

### Next step
- Append narrative-director coverage closure pass in next session (independent run; same pattern as this entry)
- Then proceed to Phase 2 walkthrough authoring per third-review creative-director synthesis
- Update systems-index FT12 blocker count from 19 to 25 to reflect this entry

---

## Coverage Closure Pass — 2026-04-30 — narrative-director independent assessment
Specialist: narrative-director ✓ (closes final coverage gap from third review)
Blocking items: 3 net-new | Recommended: 4 net-new | Nice-to-have: 3 (1 escalated from prior nice-to-have)
Prior verdict resolved: No — third review's 19 + qa-lead's 6 still stand. Total now: **28 BLOCKING / 33 RECOMMENDED**.

Summary: narrative-director ran independently same session as qa-lead after burst rate-limit cleared. Confirms Cluster 1 (R8 bell) + Cluster 8 (Keeper) blockers from tone side and **upgrades Cluster 8 Keeper to pillar-threat severity** (Keeper is the only character with a voice in the system; no voice profile = sanctuary fantasy fails at first impression). Surfaces 3 net-new BLOCKING items all clustering around a single root cause: **narrative production infrastructure is underprepared for the scope FT12 commits to** — Keeper has no voice, Section B sample line is dev placeholder, OQ-2 has no deliverable spec, vignette scope is unplanned. GDD authoring is sound; the gap is production briefs not yet authored.

### Blocking items (NOT addressed this session)

10. [narrative-director] Keeper NPC has no voice profile, demeanor brief, or sample line at GDD level — writer receiving this slot will default to tutorial NPC, puncturing sanctuary tone at first impression. **Pillar-threat upgrade from third review Cluster 8 blocker #17.** Required: one-paragraph Keeper voice profile + one golden sample line embedded in Section B; remove "active only on first visit" framing in favor of "most prominent on first visit"
11. [narrative-director] EC-11 ban broadcast "ออกจากลานแล้ว" is indistinguishable from voluntary leave/disconnect — design rationale undocumented. Two valid philosophies (invisible moderation OR diegetic keeper ejection) not chosen. Required: document intent OR adopt Keeper-voiced phrasing "ผู้ดูแลลานได้ขอให้ผู้ถือธงท่านหนึ่งออกจากลาน" (instance-broadcast only) + distinct phrasing for voluntary leave ("ออกเดินทางแล้ว") and disconnect ("ขาดการติดต่อชั่วคราว")
12. [narrative-director] OQ-2 deliverable spec missing — cascade blocks Art + Audio + Asset Spec phases (3 production departments on hard-gate). Required additions: (a) 10 city identities with distinct cultural/thematic registers, (b) one Anchor Content type per city respecting R3 neutrality, (c) per-city ritual instrument designation, (d) per-city first-visit beat text. Producer notification needed: 80 writer-days (OQ-3) is vignette-only — theme spec itself is separate staffing requirement currently unplanned

### Recommended (NOT addressed this session)

- [narrative-director] Section B sample line "คุณกำลังเข้าสู่ลานเมือง [City Name]..." is dev placeholder — violates own instruction (line 93: "Phrasing ต้องผ่าน narrative writer pass ไม่ใช่ developer placeholder"). **Golden draft proposed:** *"ลานแห่งนี้ไม่มีประตูกั้นฝ่าย — ระฆังจะบอกให้รู้เองว่าอะไรกำลังจะมา"* (diegetic environmental observation, no system-speak, foreshadows R8 bell mechanic). Replace template token with note that city-specific variants are OQ-2 deliverable
- [narrative-director] R3 Anchor Content all solo-consumption types (lore archive, observatory, etc.) — tonal mismatch for cross-faction encounter hub. **Escalated from third review nice-to-have to RECOMMENDED.** OQ-3 brief must include ≥2 shared-witness/relational Anchor types (faction-count-aware narration, proximity-triggered relics, cooperative observatory triggers — leverage existing R12/R13 presence state, no new mechanic)
- [narrative-director] R16 action verb "Salute" is militarily coded — Section B explicitly prohibits military vocabulary (line 53–55). Thai working name "ผ่านลาน" is correct; English "Traveler's Salute" should be "Wayfarer's Nod" or "Courtyard Bow." Vocabulary note required in R16 before UX spec authoring (use นบ/วันทา/แสดงคารวะ/ทักทาย; avoid military registers)
- [narrative-director] "ลาน" vocabulary overloading: (a) "ลานร่วม Y" cross-instance counter UI label is spatially ambiguous → propose "ผู้ถือธงทั้งเมือง Y" ; (b) "ลานชุมนุม" in R15 has political-protest connotation → use "ลานกลาง" or simply "ลาน" ; (c) "ลานเมือง" in Section B sample line is bureaucratic vs. sacred "ลานวิหาร" — golden draft above sidesteps via standalone "ลาน"

### Nice-to-have

- [narrative-director] R7 Banner Racks display proportional faction colors → art direction note: design for *plurality* (always show ≥1 of each represented faction) over *majority* (proportional to population). Prevents "ลานวิหาร" reading as faction base when 120/150 instance is one faction
- [narrative-director] 10 cities × instrument designation: add to OQ-2 deliverable list. Bell defaults to identical sound type unless narrative specifies per-city ritual instrument (e.g., maritime city = ship's bell/conch; observatory city = singing bowl/glass harmonica; forge city = anvil strike/gong)
- [narrative-director] Caravanserai metaphor exists only in FT12 (Section B lines 59–63) but not in game-concept.md or FT11 — propagate back to shared world tone vocabulary in next revision pass

### Severity adjustments to prior findings

- Third review Cluster 1 blocker #1 (R8 bell density counter): CONFIRMED from tone side. R8 does not operationalize Section B Anchor moment ("ทุกคนหันมามองกันครู่หนึ่ง") into production brief — animation/audio/dialogue departments will fill 30s window independently without coordinated beat. Add 1-sentence behavioral cue to R8 rationale paragraph
- Third review Cluster 8 blocker #17 (Keeper info-dump): CONFIRMED and UPGRADED to pillar-threat (see Blocking #10 above)
- Third review nice-to-have (R3 solo-consumption Anchors): ESCALATED to RECOMMENDED (see Recommended #2 above)

### Coverage gap remaining
- **NONE.** All 7 specialists from third review now have findings on record (game/systems/network/ux qa-lead/narrative-director + creative-director synthesis). Coverage closure complete for third review.

### Next step
- **Phase 1 (coverage closure) is COMPLETE.** Total blocker landscape locked: 28 BLOCKING / 33 RECOMMENDED across 8 root-cause clusters + 1 production-infrastructure cluster (narrative briefs)
- Update systems-index FT12 blocker count from 25 to 28
- Proceed to Phase 2 walkthrough authoring per creative-director synthesis (5 scenarios: first-visit, return-no-event, return-with-event, cross-faction-party, launch-spike) — methodology change to top-down walkthrough-driven before any GDD edits begin
- Phase 2 should NOT spawn specialists (single-session main thread); reserves spawning quota for Phase 3 cluster-by-cluster revision (8-10 sessions, 1-2 specialists per cluster)

---

## Phase 3 Cluster 4 Partial — 2026-05-05 — Blocker #12 resolution
Specialist: main session (no specialist spawn — single-document patch per Phase 3 protocol for narrow blockers)
Blockers closed: 1 (#12) + partial close of #17 (Keeper info-dump anti-pattern reduced from BLOCKING to NEEDS-NARRATIVE-PASS)
Items: 4 GDD edits + 1 net-new AC + 1 walkthrough status update

Summary: Phase 3 begins with narrow Cluster 4 partial — close blocker #12 (post-first-visit City Menu access UNSPECIFIED) before Scenario B authoring can proceed. Decision: **hybrid 3-path access** — landmark-direct (primary spatial), City Menu hotkey + persistent HUD pin (quick-access), Keeper NPC (ambient supplementary). Closes #12 cleanly + reduces #17 surface area (Keeper no longer functions purely as tutorial info-dump ; functional role overlaps with hotkey/pin so Keeper becomes ambient sanctuary keeper, awaiting narrative writer pass per narr-1).

### Decision rationale
Three options evaluated:
- (a) **Landmark-only** (each landmark opens own UI directly, no hub) — preserves spatial discovery + sanctuary fantasy but forces players to memorize 8 spatial locations ; queue-prone players experience friction
- (b) **City-Menu-only** (hub required, hotkey or Keeper to open) — preserves quick-access but landmarks become decorative without behavioral pull ; regresses Section B fantasy
- (c) **Hybrid** (both landmark-direct + hotkey/pin in parallel, Keeper as ambient option) — chosen ; preserves both fantasy and accessibility ; Keeper gets purpose tonally consistent with sanctuary

Hybrid (c) chosen because it delivers P5 sanctuary tone (landmarks have presence) AND P4 mechanical access (hotkey for queue-prone players) AND closes the blocker without forcing a single-path commitment that subsequent UX spec phase would have to walk back.

### Patch applied (2026-05-05)
1. **R2.1 added** to `design/gdd/world-map-system.md` — codifies 3 parallel access paths with explicit "ห้าม design choice ใดๆ ที่ทำให้ Path 1 หรือ Path 2 หายไปหลัง first-visit" constraint
2. **Section B onboarding text patched** — Keeper interaction prompt framing changed from "active เฉพาะ first-visit" to "most prominent on first-visit, active ทุก visit per R2.1 Path 3" ; alternative mechanisms reframed from "alternative-instead-of" to "available in parallel"
3. **UI Requirements table updated** — City Menu row updated with 3 paths; new row added for "8 service landmarks (in-world)" as separate UI surface
4. **TR-WMS-056 added** — Integration AC verifies all 3 paths reachable from any plaza spawn, hotkey/pin active in `InCity+Queued`, Keeper prompt active post-first-visit ; AC suite count 49 → 50
5. **Walkthrough Status updated** — `design/gdd/world-map-system-walkthroughs.md` Status section: Scenario B unblocked

### Cascading effects on Phase 2 walkthroughs
- **Scenario A blocker #12** — RESOLVED (Keeper post-first-visit framing fixed, hotkey/pin path explicit)
- **Scenario A blocker #17 (Keeper info-dump)** — REDUCED ; Keeper is no longer "menu opener that disappears after first-visit" but "ambient sanctuary keeper that overlaps hotkey functionality" ; full close still requires narr-1 voice profile
- **Scenario D blocker #12 cascade** — RESOLVED (A's #12 fix propagates to D's t≈+20s row "A presses City Menu hotkey")
- **Scenario E** — unaffected (server-aggregate scope does not exercise UI access paths)
- **Scenario B** — UNBLOCKED ; can now author per Phase 2 plan with explicit hotkey/pin/landmark/Keeper paths

### Items NOT in scope of this patch
- Hotkey default key (`M` chosen as PC default) is provisional pending OQ-7 platform decision (#19)
- HUD pin visual treatment + position defer to `/ux-design hud`
- Keeper voice profile + ambient lines remain narr-1 (BLOCKING for Cluster 8 narrative production)
- Discoverability + tutorial explanation of 3-path access defer to `/ux-design first-visit-onboarding`

### Updated blocker landscape
- Total now: **27 BLOCKING / 33 RECOMMENDED** (was 28 — #12 closed)
- Cluster 4 still has #10 (R3.1 freshness signal) + #11 (R15 zero pull) open ; #12 was the load-bearing one for Scenario B unblocking
- Cluster 8 still has #17 (reduced), #18 (EC-09 draft fate), #19 (OQ-7 platform) open

### Next step
Author Scenario B (Return-Visit-No-Event) per Phase 2 plan now that #12 is resolved. Then proceed to Phase 3 Cluster 3 (capacity math) — densest blocker exposure per Scenario E findings.
