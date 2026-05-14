# S5-12 — AI Bot Fate Decision (Sprint 006 Day-1 Briefing)

> **Status**: Deferred from Sprint 005 with **hard commitment** to decide Day 1 of Sprint 006 (Sprint 003 retro action #5 — 4th carryover; further defer NOT acceptable).
> **Owner**: producer (`tanapol`) + creative-director
> **Estimate**: 0.5d (decision + sprint doc updates)
> **Pre-reading time**: 5 minutes
> **Sprint 006 plan must reference this briefing before kickoff**

---

## TL;DR — The decision in one sentence

Choose **A** (commit S6 full focus on AI Bot, ~3.0d), **B** (descope S4-09/S4-10 to post-launch backlog), or **C** (cherry-pick MVP-critical pieces only).

---

## Carryover history

| Sprint | Story IDs | Outcome |
|--------|-----------|---------|
| Sprint 002 | S2-09 (item-buying 2.0d) + S2-10 (Difficulty 1.0d) | Deferred — Phase 1b priority |
| Sprint 003 | S3-08 + S3-09 | Deferred — Phase 1b doc theme purity |
| Sprint 004 | S4-09 + S4-10 | Deferred — Phase 2 priority |
| Sprint 005 | S5-12 (the meta-decision) | Deferred — Phase 2 closeout took capacity |
| **Sprint 006** | **DECIDE — no further defer** | Must close per Sprint 003 retro action #5 |

---

## Current AI Bot state (working, in production)

From `design/gdd/ai-bot-system.md`:

- ✅ 11-state state machine (Idle, FindRoute, Walking, FoundTarget, HpLow, Recall, RegenHp, Return, AFK-related)
- ✅ Fuzzy Utility AI for skill priority (damage + AoE + heal + escape weights, range penalty)
- ✅ Targeting — 3.5u scan radius, 0.2s server tick
- ✅ Skill point auto-distribution (R → E → W → Q)
- ✅ AFK Detection — 180s warning, 300s kick, BotActor crops Avatar
- ✅ Server-Authoritative — all bot logic on server peer
- ✅ Recall at HP < 30%
- ✅ Bot Replacement when player AFK
- ✅ Skill priority queue R → Q → W → E

**Functional MVP**: Players can play full matches against bots, AFK replacement works, bots cast skills sensibly. No P0 bot bugs filed in Sprint 005.

## Missing features (what S4-09 + S4-10 would add)

| Feature | Sprint cost | Story |
|---------|------------:|-------|
| **Item buying** — bot shop logic, item priority by role | 2.0d | S4-09 (depends S5-11 R-21 decision) |
| **Difficulty levels** — easy/normal/hard parameter scaling | 1.0d | S4-10 (depends S4-09) |
| Last-hit minion farming | TBD | Not yet a story |
| Coordination between bots (team-aware) | TBD | Not yet a story |
| Objective awareness (Boss/Tower priority) | TBD | Not yet a story |
| Gank / Roaming | TBD | Not yet a story |
| Collision Avoidance (code exists, commented out) | ~0.5d | Not yet a story |

---

## Path A — COMMIT Sprint 006 full focus

**What ships**: S4-09 (item-buying, 2.0d) + S4-10 (Difficulty, 1.0d) = 3.0d of AI Bot work in Sprint 006.

**Pros**:
- Closes 3-sprint debt (now 4 carryovers — finally resolved)
- AFK bot replacement quality much improved → better player experience when teammates drop
- Training mode becomes a real onramp for new players, not "basic cast loop"
- Difficulty slider gives marketing a feature line ("適切な相手 from beginner to veteran")

**Cons**:
- 3.0d of Sprint 006 capacity locked → Phase 3 hero migration (20+ heroes) slows
- Phase 3 timeline → +1 sprint at minimum
- If launch deadline is fixed, this trades Phase 3 polish for AI Bot polish

**Prerequisites**:
- **S5-11 R-21 Item Role Restriction decision** must land first (still Should Have in Sprint 005; if not done by Sprint 005 close → Sprint 006 Day-1 decision too) — defines whether item-priority logic needs role-gating branches
- CBS Item data schema confirmed stable (R-21 might force a schema change)

**Sprint 006 implication**: Sprint 006 theme becomes "AI Bot pass + selected Phase 3 heroes" (not "Phase 3 full sweep"). Capacity table reshuffles.

## Path B — DESCOPE to post-launch (Recommended by AI assistant — context only)

**What ships**: Move S4-09 + S4-10 → `production/backlog/post-launch.md`. Close S5-12 in Sprint 005 retro as "Path B accepted".

**Pros**:
- Frees ~3.0d in Sprint 006 → Phase 3 hero migration on track (largest launch dependency)
- Bot pipeline already MVP-functional (per GDD §3.1) — no launch blocker
- Item-buying + difficulty are polish, not core loop
- Post-launch content cadence has natural slot for bot tuning patches

**Cons**:
- New-player onboarding may feel rougher than competitors (League / Mobile Legends both have item-buying bots since launch)
- AFK bot replacement stays "basic cast loop" — teammates of AFK player may complain
- 3+ sprints of historical sprint plans contained this; descoping is a process correction visible in retros

**Process effect**:
- Closes Sprint 003 retro action #5 cleanly
- Sets pattern for future deferral discipline (carryover ledger respected)

**Required updates if Path B**:
1. Move S4-09 + S4-10 → `production/backlog/post-launch.md`
2. Update `design/gdd/ai-bot-system.md` §3.7 — mark missing features as "Post-Launch"
3. Close Sprint 003 retro action #5 (mark resolved with "Path B descope")

## Path C — PARTIAL cherry-pick

**What ships**: Pick 1-2 specific bot improvements that are clearly launch-critical; descope the rest.

**Examples of candidates**:
- Last-hit minion farming (~0.5d) — closes a visible weakness without scope expansion
- Better AFK bot replacement than basic state machine (e.g., parameterised aggression curve) — small effort, big perceived improvement

**Pros**:
- Middle ground; addresses real gaps
- Smaller capacity footprint than Path A

**Cons**:
- Re-scoping has its own cost (new stories, new ACs, new estimates)
- High risk of trigger carryover #5 — "we picked X but it grew" is a familiar story
- Decision must be **very specific** ("ship only feature Y, nothing else") to avoid scope creep

---

## Briefing for creative-director — discussion prompts

1. **MVP launch experience question**: Is Training mode an onramp for new players, or a sandbox for veterans? (Defines whether bot quality is launch-critical or polish.)
2. **AFK replacement question**: How common is AFK in your target audience? (Defines whether basic bot replacement is acceptable.)
3. **Competitor benchmark question**: League / Mobile Legends ship with item-buying bots. Is this a parity expectation for our launch, or a differentiator delta we can afford?
4. **Post-launch content question**: Does the live-ops plan have headroom to add bot features in patches 1-3? (Defines whether Path B is reversible.)

If creative-director answers: "launch differentiator + Training mode is critical + competitor parity required" → **Path A**.
If answers: "MVP only + AFK is rare + we'll patch later" → **Path B**.
Mixed answers → **Path C**, requiring a specific feature pick.

---

## Decision deliverables (Day 1 of Sprint 006)

Whatever path is chosen, all of these must update before Sprint 006 kickoff:

1. **Decision document** at `production/decisions/S5-12-ai-bot-fate.md` (NEW) — records Path A / B / C + rationale
2. **`production/sprints/sprint-006.md`** — Sprint 006 plan reflects the decision (Must Have AI Bot if Path A; absent if Path B/C-with-other-pick)
3. **`production/sprint-status.yaml`** — close S5-12 status: `done` with decision + path noted
4. If **Path B**: `production/backlog/post-launch.md` (NEW or existing) — add S4-09 + S4-10 with cost estimates and rationale
5. **`design/gdd/ai-bot-system.md` §3.7** — update "สิ่งที่ Bot ยังไม่มี" with Phase 3 / Post-launch labels
6. **Sprint 003 retro action #5** — mark resolved in `production/retrospectives/sprint-003.md` (or append a 1-line resolution note)

---

## Sprint 005 close-out reference

This briefing is the artefact produced by deferring S5-12 from Sprint 005. The deferral was tracked in:
- `production/sprints/sprint-005.md` § Deferred to Sprint 006 (entry: "S5-12 AI Bot fate decision — Path A/B/C, see briefing")
- `production/sprint-status.yaml` (entry: S5-12 status `deferred-to-S6 (committed-decision)`)

**No further defer permitted** per Sprint 003 retro action #5 (>3 carryovers triggers root-cause review; we are at 4).

---

## Changelog

- **2026-05-14** — Initial briefing authored as part of Sprint 005 close-out (S5-12 deferred with hard commitment to decide Sprint 006 Day 1).
