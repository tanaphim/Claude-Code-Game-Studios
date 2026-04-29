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

## Review — 2026-04-30 — Verdict: MAJOR REVISION NEEDED (resolved in same session)
Scope signal: XL
Specialists: game-designer ✓, systems-designer ✓, network-programmer ✓, ux-designer ✓, qa-lead ✓, narrative-director ✓, creative-director ✓ (synthesis) — full coverage achieved
Blocking items: 16 clusters (27 individual items) | Recommended: 15
Summary: Second review with full specialist coverage. Three convergent BLOCKING clusters: (A) R12 atomicity fiction — 3 specialists (systems/network/qa) independently flagged "single transactional Azure Function call" as impossible on PlayFab + internally contradictory with read-repair pattern; (B) Section B fantasy/sharding dissonance — 3 specialists (game/ux/narrative) flagged "ทุกฝ่ายมาเจอกัน" claim cannot be delivered under R10 proximity chat + R13 instance sharding; (C) OQ-2/3/9 treated as deferrable details when they are load-bearing design pillars. Re-review failure pattern: items 9, 10, 11, 13, 14 from first review remained unaddressed (item 11 elevated from Recommended to Blocking due to TR-WMS-034 mechanical math error). Pre-approval gate: Accessibility section was `[TO BE CONFIGURED]`. Verdict MAJOR REVISION; all 27 items resolved in same session per user direction.
Prior verdict resolved: Yes — items 9 (D1 fragmentation), 10 (hard ceiling vs density chaos), 11 (EC-19 750→1000 boundary), 13 (cross-faction Fragment perverse incentive), 14 (first-visit beat tone+functional split) all addressed this session.

### Blocking clusters resolved (same session 2026-04-30)
1. [systems G + network F2 + qa D4] R12 atomicity rewritten to single-document JSON blob model (PlayFab Entity Object key `ft12_player_state`) with idempotency key + `IfChangedFromDataVersion` conditional write. Native PlayFab atomicity replaces fictional multi-key transaction.
2. [systems A] D1 step 1 TOCTOU race fixed via `TryIncrementPopulation` atomic check-and-set primitive; D1 step 2 also migrated to same pattern.
3. [systems D + qa D11] TR-WMS-034 prewarm math fixed (PREWARM=34 for 5,000 player test); added TR-WMS-034b for 750→1000 boundary case.
4. [qa D9 + systems I] TR-WMS-035 reclassified BLOCKING (was ADVISORY); CBS constraint forbids tick > 5 in TR-WMS-035 context.
5. [game C3 + ux F6 + narrative N5] Section B reframed: "ทุกฝ่าย" → "ผู้เล่นหลากฝ่าย"; added per-instance sharding note in Overview; added R10 fantasy-gap acknowledgment paragraph.
6. [game I3 + ux F8] OQ-9 converted to **R16 Cross-Faction Stranger Acknowledge Primitive** (1-button salute, no reward, cross-faction visually distinct, 60s cooldown).
7. [narrative N1] R3 example list scrubbed; added R1 alignment rule + neutrality test ("Anchor นี้มีความหมายเท่าเทียมกันสำหรับผู้เล่นที่เพิ่งเปลี่ยนฝ่ายเมื่อวานหรือไม่?").
8. [narrative N4] EC-11 Thai phrasing fixed: "โดยกำเนิด" → "ได้ออกจากลาน [City Name] แล้ว".
9. [narrative N3 + qa D10] `ANCHOR_VIGNETTE_POOL_MIN` raised 4 → 8 (2-month exhaustion floor); G.6 lifetime constraint added.
10. [game C2 + qa D10] R15 added category 5 (Monument ambient countdown — ongoing watchable that competes with City Menu pull); 3 ACs added (TR-WMS-039/040/041).
11. [systems C] `HARD_CEILING_RATIO` max 1.5 → 1.1 (ceiling 165, prevents G.1 density chaos contradiction).
12. [ux F2] EC-09 rewritten: auto-blur + auto-save (no defer); resolves priority hierarchy contradiction.
13. [ux F5] R13 cross-instance counter relabeled "ลานร่วม Y" → "เมืองนี้ทั้งหมด Y คน"; informational note added.
14. [ux F1] R5.1 added — Galaxy Map dual-purpose interrupt rule (passive event update, no selection interrupt).
15. [ux F7] EC-26 sequencing rule added (bell t=0, indicator t=+0.3s, Galaxy passive).
16. [ux pre-approval gate] Accessibility section configured with baseline (PC+kb/mouse default per OQ-7, color/text/audio/input commitments).

### Additional resolved items (Recommended → addressed)
17. [ux F4 + narrative N9] First-visit beat split into tone job + functional job (2 separate beats); provisional string committed.
18. [narrative N2/N3] OQ-2 + OQ-3 converted from open questions to MILESTONES with Narrative Director as named primary owner, deliverable specs (City Theme Brief, 80 vignettes), exit criteria.
19. [narrative N8] Keeper NPC archetype added — one-sentence brief + provisional first-line.
20. [qa D4] R12 atomic rollback test added (TR-WMS-044 failure-injection) + idempotency test (TR-WMS-045).
21. [qa D11] EC-14, EC-18, EC-24 silent drops resolved with TR-WMS-047/048/049.
22. [qa D1/D2/D6/D7] TR-WMS-006/009/027/029 SLA sources defined (server-side timestamp anchors).
23. [systems B / item 9 prior log] `CITY_INSTANCE_FRAGMENTATION_ALERT` knob added.
24. [game I1 / item 13 prior log] R9.1 cross-faction Fragment routing trade-off position + `CROSS_FACTION_PARTY_MATCH_PCT_ALERT` metric added.
25. [systems E] D1 heartbeat boundary clarified: `< 5` (exclusive at 5.0s).
26. [systems H] PARTY_MAX placeholder=5 + entity registry note.
27. [systems F] G.5 Gini zero-bucket sensitivity documented in knob description.

### Items added (new specs)
- **R5.1** Galaxy Map dual-purpose interrupt rule
- **R9.1** Cross-faction Fragment routing trade-off
- **R15 category 5** Monument ambient countdown
- **R16** Cross-Faction Stranger Acknowledge Primitive (resolves OQ-9)
- **16 new ACs** TR-WMS-036 through TR-WMS-051
- **4 new knobs** R16_ACKNOWLEDGE_RADIUS_UNIT, R16_ACKNOWLEDGE_COOLDOWN_SECONDS, CITY_INSTANCE_FRAGMENTATION_ALERT, CROSS_FACTION_PARTY_MATCH_PCT_ALERT

### Items NOT addressed in this session (open for re-review)
- Specific narrative team staffing decision for 80 vignettes (OQ-3 milestone exit criteria — requires Producer + Narrative Director allocation discussion)
- WCAG conformance level (AA vs AAA) — defer to UX spec phase
- Cross-region clock drift tolerance value (EC-25) — defer to ADR
- R8 cross-instance broadcast bus selection — defer to R13 sharding ADR (network-programmer F1/F7/F9)

### Coverage gaps (addressed this round)
- network-programmer ✓ (was unavailable in first review) — 10 findings, 5 blocking for ADR; identified that R13 ADR scope must include broadcast bus + LookupInstance caching + M11 RTT budget
- ux-designer ✓ (was unavailable) — 9 findings; resolved EC-09, R5.1, R13 counter, accessibility baseline
- qa-lead ✓ (was unavailable) — 11 findings; resolved AC SLA defects, added 16 new ACs
- narrative-director ✓ (was unavailable) — 9 findings; resolved R3 violation, EC-11 Thai, OQ-2/3 milestone conversion, Keeper NPC
- creative-director ✓ — synthesis confirmed MAJOR REVISION verdict pre-fix; recommended GDD-level fixes before ADR

### Next step
Re-review in fresh session: `/clear` then `/design-review design/gdd/world-map-system.md` to verify all 27 fixes hold against fresh specialist pass on the revised draft. Expected verdict if fixes hold: APPROVED or NEEDS REVISION (small items only). Watch for: R12 single-doc model implementability (network re-validate), Section B fantasy reframe sufficiency (game/narrative re-validate), R16 design contract completeness (ux re-validate), AC coverage of new R5.1/R9.1/R15.5 (qa re-validate).
