---
title: Change Impact — Tournament & World Map Pivot
date: 2026-04-27
source_decision: design/decisions/meeting-2026-04-23-tournament-pivot.md
review_mode: lean
director_gates_skipped:
  - TD-CHANGE-IMPACT (lean mode)
status: deferred-resolutions
---

# Change Impact Report — Tournament & World Map Pivot

## Change Summary

Source: [Meeting 2026-04-23 — World Map & Tournament System Pivot](../../design/decisions/meeting-2026-04-23-tournament-pivot.md)

**Headline change:** Territory War (Declare War) + 1M-city world map ถูกแทนด้วย:
- **Tournament system** (5v5 หลัก, Casual/Ranked, Fragment reward)
- **World Map = 10 Neutral Cities** (planets, socialize hub)
- **Faction system** (5-6 ฝ่าย)
- **Fragment & Universe meta-game** (~100M → ring → กฎ Universe, reset ~3 ปี)
- **Wager Mode**, **Reputation/Anti-Toxicity**, **Manga Influence Loop**

**Game simplification mandate:** 5v5 จบใน 15 นาที, Max Level ใน 10 นาที, locked builds, simplified item stats, possibly remove last-hit mechanic.

**ADRs scanned:** 8 (ADR-0001 ถึง ADR-0006 + sub-files)

---

## Impact Classification

| ADR | Status | Action |
|-----|--------|--------|
| ADR-0001 Unity 2022.3.62f1 + URP + C# | ✅ Still Valid | None |
| ADR-0002 Photon Fusion 2 Networking | ⚠️ Needs Review | **Deferred** |
| ADR-0003 PlayFab CBS + Azure Functions | ⚠️ Needs Review | **Deferred** |
| ADR-0004 ActorCombatAction + SkillKey Pipeline | ✅ Still Valid | None |
| ADR-0005 ItemAnimationType Routing | ✅ Still Valid | None |
| ADR-0006 Unified Ability System | ⚠️ Needs Review | **Deferred** |
| ADR-0006-migration-audit | ✅ Still Valid | None |
| ADR-0006-phase-1a-interfaces | ✅ Still Valid | None |

---

## Detailed Impact

### ADR-0002: Photon Fusion 2 Networking

**Status:** ⚠️ Needs Review — **Deferred** until Tournament + Game Mode Manager GDDs revised

**What ADR assumed:**
- "ขยายถึง 50 คน (25v25) ในอนาคต" — hard scaling target
- Validation criteria items 4-7: 50-player desync test, AOI correctness, < 15 KB/s bandwidth at 25v25
- Performance budgets ระบุ tick rate และ bandwidth ต่อ mode สำหรับ 25v25
- Risk row: "Tick rate ไม่พอที่ 50 players" (ปานกลาง × สูง)

**What meeting decided:**
- "Standard = 5v5 / โหมด 25v25 ปล่อยตามธรรมชาติ" — 25v25 ลดเป็น nice-to-have
- Game length 15 นาที = bandwidth/cost budget ต่อแมตช์ลดลง (อาจ relax)
- 1v1 / 2v2 / 3v3 รองรับตาม Request — เพิ่ม mode permutations

**Why deferred:**
- ต้องรอ Game Mode Manager GDD (revise) เพื่อยืนยันว่า 25v25 ยัง schedule อยู่ใน roadmap หรือ deprioritize ทั้งหมด
- Tournament GDD จะกำหนด matchmaking + room shape ที่ส่งผลต่อ AOI requirement

**Update plan (when triggered):**
- Add status note: "25v25 deferred per 2026-04-23 leadership decision"
- Move 4 validation criteria items เป็น "Deferred (post-MVP)"
- Update Performance Implications table — ลบ 25v25 row หรือ mark deferred
- Update AOI risk priority

---

### ADR-0003: PlayFab CBS + Azure Functions

**Status:** ⚠️ Needs Review — **Deferred** until new-system GDDs authored

**What ADR assumed:**
- รายการ CBS models: `CBSItemInGame`, `CBSAbility`, `CBSUnit`, `CBSSkin`
- Custom logic: `MatchResult()`, `BattlePassReward()`, `AntiCheat()`, `CraftItem()`
- Battle pass เป็น progression layer หลัก
- Item / inventory เป็น core economy

**What meeting decided:**
- ระบบใหม่ที่ต้องใช้ backend หนัก:
  - Tournament (Casual/Ranked + matchmaking + bracket)
  - Fragment (daily drops, faction-shared pool, decay)
  - Faction (membership, switch rules)
  - Wager (bet validation, escrow)
  - Reputation/Anti-Toxicity (chat moderation, ranked lock)
  - Universe cycle (~3-year reset, 100M aggregation)
- Battle pass อยู่ใน revise list — ความสัมพันธ์กับ Fragment/Faction ยังไม่ชัด
- Item simplification → CBS schema สำหรับ Item อาจปรับ

**Why deferred:**
- รายการ CBS models ใหม่ที่ต้องเพิ่ม ขึ้นกับ GDD ใหม่ที่ยังไม่เขียน
- Azure Functions ใหม่ขึ้นกับ Tournament/Fragment/Wager mechanics ที่ยัง [Open Question]
- Battle pass revise อาจ collapse เข้ากับ Fragment system → ลด CBS model ที่ต้องเพิ่ม

**Update plan (when triggered):**
- เพิ่มรายการ CBS models: `CBSFaction`, `CBSTournament`, `CBSFragment`, `CBSReputation`, `CBSWager` (ตามจริงหลัง GDD เสร็จ)
- เพิ่ม Azure Functions: `MatchmakingTournament()`, `FragmentDrop()`, `FactionTransfer()`, `WagerEscrow()`, `ReputationDecay()`, `UniverseCycleReset()` (ตามจริง)
- Update Performance Implications: PlayFab API call budget ต่อ session (Tournament + Fragment เพิ่ม load)
- Re-evaluate Validation Criteria item 4 (live balance update)

---

### ADR-0006: Unified Ability System

**Status:** ⚠️ Needs Review — **Deferred** until item-system + dungeon-mode + hero-system revised

**What ADR assumed (GDD Requirements Addressed table):**
- `dungeon-mode.md` — Boss encounters with phase-based ability swap → justifies `AbilityComponent.SwapPhase()`
- `item-system.md` — "Active items / spell stealer items ให้ ability ชั่วคราว" → justifies `AbilityComponent.SetSlot(ItemSlot, stolenAbilityId)`
- `hero-system.md` — Scalability to 25+ heroes
- Future: `ability-draft-mode.md` — pool-based ability picking

**What meeting decided:**
- Dungeon: "Personal/Side Quest 1-5 + bot, MOBA combat" — multi-phase boss encounters? **[Open Question]**
- Item: build ล็อคต่อ hero, simplify item stats, คง consumable + skill item — "spell stealer" item อาจไม่อยู่ใน new direction
- Hero: build-lock ไม่กระทบ ability slot model

**Why deferred:**
- Architecture decision (data-driven binding) **ยังถูกต้อง** — เพราะ variable slot count + keybind remap + Ability Draft ยังเป็น valid future requirements
- แต่ justification linkage ใน "GDD Requirements Addressed" table อาจ stale หลัง GDD revise
- Phase 1a (interface design) ดำเนินต่อได้โดยไม่ต้องแก้ ADR

**Update plan (when triggered):**
- Update "GDD Requirements Addressed" table:
  - `dungeon-mode.md` row: ตรวจว่า boss phase swap ยังเป็น requirement หรือไม่
  - `item-system.md` row: ลบ "spell stealer" ถ้าไม่ใช่ direction แล้ว, หรือ keep เป็น future capability
- ไม่ต้องแก้ Decision/Architecture/Implementation Guidelines — ยังถูกต้อง

---

## Follow-Up Trigger Map

| Triggering Event | Re-open ADR |
|------------------|-------------|
| Tournament GDD authored | ADR-0003 |
| Game Mode Manager GDD revised (25v25 status) | ADR-0002 |
| Fragment + Wager GDDs authored | ADR-0003 |
| Faction + Reputation GDDs authored | ADR-0003 |
| Dungeon Mode GDD revised (boss model) | ADR-0006 |
| Item System GDD revised (spell stealer / active items) | ADR-0006 |
| Battle Pass GDD revised (Fragment relationship) | ADR-0003 |

**Re-run command:** เมื่อ trigger เกิดขึ้น ให้รัน `/propagate-design-change <revised-gdd-path>` เพื่อตรวจ impact ต่อ ADR ที่ deferred ไว้

---

## GDD Work Tracker

### New GDDs to author (in dependency order)

1. **Faction System** — foundation ของหลายระบบใหม่ (5-6 ฝ่าย, switch rules)
2. **World Map — 10 Neutral Cities** — uses Faction
3. **Tournament System** — core meta loop (Casual/Ranked, MOBA 5v5/1v1/2v2/3v3)
4. **Fragment & Meta-Game System** — uses Tournament + Faction; goal 100M → ring
5. **Wager Mode** — sub-system ของ Fragment
6. **Reputation / Anti-Toxicity System** — Reputation score, ranked lock, chat filter
7. **Manga Influence Loop** — narrative layer (อาจ defer; รอ proposal)

### Existing GDDs to revise (retrofit mode)

- `design/gdd/game-concept.md` — pillars/scope/visual identity ตามทิศทางใหม่
- `design/gdd/level-xp-system.md` — Max Level ภายใน 10 นาที
- `design/gdd/item-system.md` — build lock per hero, simplify stats, คง consumable + skill item
- `design/gdd/gold-economy.md` — last-hit reward (ตัด/ลด)
- `design/gdd/game-mode-manager.md` — 5v5 = standard, 25v25 deferred
- `design/gdd/dungeon-mode.md` — Personal/Side Quest 1-5 + bot
- `design/gdd/hero-system.md` — build lock per hero
- `design/gdd/battle-pass.md` — relationship กับ Fragment system
- `design/gdd/systems-index.md` — deprecate FT10/FT10a/FT10b, เพิ่ม 7 ระบบใหม่

### Deprecate

- `design/gdd/territory-war.md` (FT10) — mark Status: Superseded by Tournament + Faction + Fragment systems
- FT10a Citizen System (never authored) — remove จาก systems-index
- FT10b Mercenary System (never authored) — remove จาก systems-index

---

## Director Gates

| Gate | Status |
|------|--------|
| TD-CHANGE-IMPACT | Skipped — Lean mode |

---

## Recommended Next Steps

1. **Update game-concept.md** ก่อน — pillars/scope ใหม่จะเป็น source สำหรับ GDD ใหม่ทุกตัว
2. **Update systems-index.md** — deprecate FT10/FT10a/FT10b, เพิ่ม slot 7 ระบบใหม่
3. **Author new GDDs ตามลำดับ** ใน "GDD Work Tracker" ข้างบน — ใช้ `/design-system <name>`
4. **Retrofit existing GDDs** — ใช้ `/design-system retrofit design/gdd/<name>.md`
5. **เมื่อ GDD แต่ละตัวเสร็จ** ให้รัน `/propagate-design-change` เพื่อ trigger ADR review ตาม "Follow-Up Trigger Map"
6. **หลัง GDD ใหม่ + revise เสร็จ** รัน `/consistency-check` + `/review-all-gdds`
7. **สุดท้าย** รัน `/architecture-review` เพื่อ verify traceability matrix ยังครบหลัง ADR updates

— จบ change impact report —
