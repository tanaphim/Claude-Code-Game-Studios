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
