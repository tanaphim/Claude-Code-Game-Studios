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
