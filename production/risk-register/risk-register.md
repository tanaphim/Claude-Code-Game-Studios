# Risk Register

**Owner:** producer
**Created:** 2026-05-07 (S4-08, Sprint 004)
**Last full review:** 2026-05-07
**Review cadence:** Sprint boundary (every 2 weeks) + on ADR acceptance

---

## Purpose

รวบรวม risks ระดับโปรเจกต์จากแหล่งกระจาย (ADRs, GDDs, sprint retros, incidents)
มาที่จุดเดียว เพื่อให้ producer และ leads เห็น exposure ทั้งหมด ตัดสินใจจัด
mitigation และ schedule re-review ได้

**Scope:** เฉพาะ risk ระดับ project/architecture/process ที่ persist ข้าม sprint
Sprint-local risks ยังอยู่ใน `production/sprints/sprint-NNN.md §Risks` ตามเดิม

**ไม่รวม:** Bug list (ใช้ `production/qa/bugs/`), tech debt (ใช้ `/tech-debt`),
known limitations ในแต่ละ ADR (ดูใน ADR ตรงๆ)

---

## Status legend

| Status | ความหมาย |
|--------|----------|
| **Open** | ยังไม่มี mitigation หรือ mitigation ยังไม่ effective |
| **Mitigating** | mitigation กำลังทำงาน — มีหลักฐานว่าลด exposure ได้บางส่วน |
| **Monitoring** | mitigation พร้อม — เฝ้า trigger conditions |
| **Realized** | risk เกิดจริง — ดู Notes ว่าจัดการอย่างไร |
| **Closed** | risk หายไป (decision changed / system removed / no longer applicable) |

## Probability × Impact rubric

- **Probability:** Low (<10%), Medium (10–40%), High (>40%)
- **Impact:** Low (sprint-level, recoverable), Medium (multi-sprint setback),
  High (release/quality at risk), Critical (project-level)

---

## R-01..R-09: Technical / Architecture

### R-01 — Photon Cloud downtime หรือ price hike
- **Source:** ADR-0002 §Risks
- **Probability:** Low (downtime) / Medium (price)
- **Impact:** High
- **Status:** Mitigating
- **Owner:** network-programmer
- **Mitigation:** Reconnect logic + monitor SLA; networking layer abstract สำหรับ
  swap provider; cost model ก่อน 25v25 launch
- **Trigger to escalate:** Photon outage > 1 hr ใน production, หรือ
  ประกาศ pricing change > 30%

### R-02 — Tick rate / bandwidth ไม่พอที่ 25v25 (50 players)
- **Source:** ADR-0002 §Risks; `design/gdd/networking-core.md`
- **Probability:** Medium
- **Impact:** High
- **Status:** Open (not yet benchmarked at scale)
- **Owner:** network-programmer
- **Mitigation:** AOI + per-mode tick rate; benchmark gate ก่อน 25v25 launch
- **Trigger to escalate:** เริ่มงาน 25v25 mode → spawn benchmark story

### R-03 — PlayFab API deprecation / outage / schema migration pain
- **Source:** ADR-0003 §Risks
- **Probability:** Low (outage) / Medium (schema)
- **Impact:** High (outage) / Medium (schema)
- **Status:** Mitigating
- **Owner:** backend (TBD) + game-designer
- **Mitigation:** Abstract ผ่าน MetadataService; version CBS models;
  local read-only cache fallback; backward-compatible field changes
- **Trigger to escalate:** PlayFab announces breaking change; CBS designer
  reports schema friction (linked to E-09 in `architecture-epics.md`)

### R-04 — Skill state desync (combat pipeline)
- **Source:** ADR-0004 §Risks
- **Probability:** Low
- **Impact:** High
- **Status:** Mitigating
- **Owner:** gameplay-programmer + network-programmer
- **Mitigation:** Server-authoritative state; Fusion replication;
  unit tests on lifecycle transitions (target ≥80% coverage — ยังไม่ครบ)
- **Trigger to escalate:** Repeat desync bug ใน playtest 2 ครั้งติด

### R-05 — `ActorCombatAction` base class โตเกินไป (godclass)
- **Source:** ADR-0004 §Risks ("เกิดแล้ว"); architecture-epics E-03
- **Probability:** Realized (ongoing)
- **Impact:** Medium
- **Status:** Mitigating (planned via E-03 component split, T2 next-up)
- **Owner:** lead-programmer
- **Mitigation:** Split เป็น component หลัง Phase 2 ปิด (ADR-0006 Phase 3 scope)
- **Trigger to escalate:** ถ้า bug fix touch base class > 3 ครั้งใน 1 sprint

### R-06 — Phase 2 migration: `CBSAbility.Slot` designer dependency
- **Source:** ADR-0006 Phase 2 §Risks
- **Probability:** Medium
- **Impact:** High
- **Status:** Mitigating
- **Owner:** producer + game-designer
- **Mitigation:** EffectiveSlot shim ทำให้ code path unblocked; designer
  fill `Slot` on-deck แบบ async
- **Trigger to escalate:** Sprint 005 kickoff โดยที่ designer ยังไม่ commit field

### R-07 — Phase 2 migration: AnimationEvent slot resolution wrong on multi-skill chain
- **Source:** ADR-0006 Phase 2 §Risks
- **Probability:** Low
- **Impact:** High
- **Status:** Open (test ยังไม่เขียน)
- **Owner:** gameplay-programmer
- **Mitigation:** Unit tests on `ActorCombat.GetActiveSlot` simulating Q→W→E
  presses (P2-09 in ADR-0006 Phase 2 plan)
- **Trigger to escalate:** Hercules pilot (Sprint 005) แสดง wrong-slot bug

### R-08 — Phase 2 migration: 50+ non-Hercules call sites หลุดถูกแตะใน Phase 2
- **Source:** ADR-0006 Phase 2 §Risks (intentional, by design)
- **Probability:** High
- **Impact:** Low (facade absorbs them)
- **Status:** Monitoring (intentional, drained in Phase 3)
- **Owner:** lead-programmer
- **Mitigation:** Facade pattern P2-01; CI grep rule
  (`SkillObjectDictionary<SkillKey` = fail) ใน Phase 3 — ยังไม่บังคับใน Phase 2
- **Trigger to escalate:** ถ้ามี Phase 2 commit แตะ non-Hercules action file

### R-09 — vfxObject NetworkObject overhead (E-06 / ADR-0007)
- **Source:** ADR-0007 §Risks; architecture-epics E-06
- **Probability:** Realized (ongoing perf cost)
- **Impact:** Medium
- **Status:** Open (ADR-0007 = Proposed, not yet Accepted)
- **Owner:** technical-director (sign-off pending) + network-programmer
- **Mitigation:** ADR-0007 migration plan Sprint 006–007; Editor validation rule
  สำหรับ damage-layer prefab → must be Networked
- **Trigger to escalate:** ADR-0007 Accepted → schedule Sprint 006 audit story

---

## R-10..R-13: Networking / Multipeer

### R-10 — PeerMode leak (Multipeer harness → production scene)
- **Source:** Sprint 003 retro action #1 (resolved 2026-05-07)
- **Probability:** Realized
- **Impact:** High (production playtest blocked)
- **Status:** Mitigating
- **Owner:** network-programmer + tools-programmer
- **Mitigation:** Manual revert workflow ตาม `docs/dev-workflow/peer-mode-toggle.md`;
  S4-P5 polish ticket = automation (toggle without editing
  `NetworkProjectConfig.fusion`)
- **Trigger to escalate:** เกิดซ้ำหลังจาก S4-P5 ลง

### R-11 — `AbilityMultipeerRunner` duplicate-Start cascade
- **Source:** Sprint 004 §Polish backlog S4-P1
- **Probability:** Realized (intermittent)
- **Impact:** Low (console noise; harness still works)
- **Status:** Open
- **Owner:** network-programmer
- **Mitigation:** S4-P1 (duplicate-Start guard, 0.5d) — Nice to Have
- **Trigger to escalate:** ถ้า GameIsFull cascade ทำให้ harness fail run

### R-12 — Bandwidth regression จาก ability migration (string hash lookup)
- **Source:** ADR-0006 §Risks ("Performance regression")
- **Probability:** Low
- **Impact:** Low
- **Status:** Mitigating
- **Owner:** gameplay-programmer
- **Mitigation:** Cache `AbilityDefinition` ต่อ slot หลัง first resolve;
  benchmark gate ก่อน Sprint 005 ปิด
- **Trigger to escalate:** Benchmark > 1ms dispatch latency

### R-13 — `InputMessage` shape change breaks mid-match
- **Source:** ADR-0006 §Risks
- **Probability:** Low
- **Impact:** Critical
- **Status:** Monitoring
- **Owner:** network-programmer
- **Mitigation:** Forced client update; bump protocol version on any
  `InputMessage` change
- **Trigger to escalate:** Phase 3 plan touches `InputMessage` (Phase 2 ตั้งใจ
  ไม่แตะ — ดู ADR-0006 Phase 2 §9)

---

## R-14..R-17: Process / Production

### R-14 — Sprint 002 carryover slips เป็นรอบที่ 3
- **Source:** Sprint 004 §Risks
- **Probability:** Low (with current Must Have priority)
- **Impact:** High
- **Status:** Mitigating
- **Owner:** producer
- **Mitigation:** S4-01..S4-08 ติดธง Must Have; ถ้าหลุดอีก escalate หา
  root cause why animator + bug work keep deferring
- **Trigger to escalate:** Sprint 004 retro หลุด carryover → spawn ADR หรือ
  process change

### R-15 — User direction shifts ไป Phase 2 mid-Sprint 004
- **Source:** Sprint 004 §Risks
- **Probability:** Medium
- **Impact:** Medium
- **Status:** Mitigating
- **Owner:** producer
- **Mitigation:** Sprint 004 goal explicit ("no ability-system migration");
  Phase 2 plan locked → pull P2-01..P2-10 เป็น Sprint 005 ได้ทันที
- **Trigger to escalate:** User request mid-sprint

### R-16 — ADR-0007 sign-off ค้าง
- **Source:** Active session state 2026-05-07; ADR-0007 §Status
- **Probability:** Realized (Status: Proposed)
- **Impact:** Medium (E-06 work ปลด-block ไม่ได้จนกว่าจะ Accepted)
- **Status:** Open
- **Owner:** producer (route to technical-director / network-programmer / unity-specialist)
- **Mitigation:** Schedule sign-off review session
- **Trigger to escalate:** Sprint 005 kickoff โดย ADR ยัง Proposed

### R-17 — E-09 (CBS replacement) audit ยังไม่จัด
- **Source:** Architecture epics §T4 Discuss; active session §Open questions
- **Probability:** Realized (audit not scheduled)
- **Impact:** Medium (long-term productivity)
- **Status:** Open
- **Owner:** producer + game-designer
- **Mitigation:** Pain-point audit session กับ designers ก่อนวาง scope plan
- **Trigger to escalate:** ถ้า designer report CBS friction ครั้งที่ 3 ใน sprint

---

## R-18..R-19: Phase 1b hangovers

### R-18 — `AbilityRegistry` boot-time scan ช้า
- **Source:** Sprint 004 §Polish backlog S4-P2; ADR-0006 Phase 1b §Closure
- **Probability:** Realized (511.9 ms warm / 1079 ms cold)
- **Impact:** Low (boot-time only, > 100ms target)
- **Status:** Open
- **Owner:** unity-addressables-specialist
- **Mitigation:** S4-P2 (build-time manifest หรือ Addressables, 1.5d)
- **Trigger to escalate:** ถ้า cold-start > 2s หลังเพิ่ม prefab batch

### R-19 — KeybindMap ยังไม่มี runtime UGUI Settings panel
- **Source:** Sprint 004 §Polish backlog S4-P3; ADR-0006 Phase 1b
- **Probability:** Realized
- **Impact:** Low (Editor placeholder ทำงาน, player remap ยังต้องผ่าน CBS)
- **Status:** Open
- **Owner:** ui-programmer + ux-designer
- **Mitigation:** S4-P3 (1.5d) — Nice to Have; non-blocking สำหรับ Phase 2
- **Trigger to escalate:** UX/QA flag ว่า player-facing remap ต้องมีก่อน beta

---

## R-20..R-23: Item system / Stats / Movement

### R-20 — Mythic Passive Bonus = schema-only (unimplemented)
- **Source:** S4-06 reverse-doc finding (2026-05-08); `design/gdd/item-system.md §3.7`
- **Probability:** Realized
- **Impact:** Medium (advertised mechanic missing; affects Mythic item value perception)
- **Status:** Open
- **Owner:** gameplay-programmer + game-designer
- **Mitigation:** GDD §3.7 + §4 documented schema + proposed formula + open
  questions; spawn Sprint 005+ wire-up story (ApplyMythicBonus in
  `NetworkHeroInventory` or `Actor.Trait`); ADR to answer Open Questions §4.1–4.4
- **Trigger to escalate:** Mythic items ship to playtest while bonus path
  ยังไม่ wire — players จะมอง Mythic เป็น "broken" item

### R-21 — Item Role Restriction = unimplemented
- **Source:** S4-05 reverse-doc finding (2026-05-08); `design/gdd/item-system.md §6 / §Known Issues`
- **Probability:** Realized
- **Impact:** Low (balance: ทุก Role ซื้อทุกไอเทมได้; ไม่ block gameplay แต่
  ทำให้ระบบ build ไม่มี constraint ตามที่ออกแบบ)
- **Status:** Open
- **Owner:** gameplay-programmer + game-designer
- **Mitigation:** Sprint 005+ story: เพิ่ม case ใน `AvailableToPurchase()`
  ตรวจ `item.Positions.Contains(Hero.Role)` + Shop UI recommend filter;
  หรือ explicit decision ว่าจะลบ field `Positions` ทิ้ง
- **Trigger to escalate:** Designer สร้าง CBS item ที่ตั้ง `Positions[]` แล้ว
  คาดว่าระบบจะ enforce → playtest พบว่าไม่ทำงาน

### R-22 — attack_speed / move_speed item-bonus /100 unconditional
- **Source:** S4-03 investigation (2026-05-08); `NetworkHeroInventory.cs:1299-1304`;
  `design/gdd/item-system.md §Known Issues`; `design/gdd/movement-navigation-system.md §4 / §Known Issues`
- **Probability:** Realized (code in production since pre-Sprint 002)
- **Impact:** Medium (designer trap: item "+30 move_speed" Flat ให้ผล +0.30 runtime
  ≈ +8.6% บน base 3.5, ไม่ใช่ +30% ที่อาจคิด; `ModifierType.Flat` กับ `Percent`
  ให้ผลเหมือนกันสำหรับ 2 stats นี้ → label เลือกผิดไม่ส่งผลแต่อาจหลอกการอ่าน data)
- **Status:** Open
- **Owner:** gameplay-programmer + game-designer (balance pass)
- **Mitigation:**
  (a) **Documentation done** — `item-system.md` + `movement-navigation-system.md`
      ระบุ scale convention และ /100 trap แล้ว (S4-03);
  (b) **Phase 2 / balance pass:** ตัดสินใจว่าจะ honor `ModifierType` (เปลี่ยน
      `NetworkHeroInventory.cs:1302` ให้ /100 เฉพาะเมื่อ `Percent`) หรือ rename
      stats ให้ชัด (`move_speed_centi`) — ทั้งสอง path ต้อง re-tune ทุก item ที่มีอยู่
- **Trigger to escalate:** (i) Designer ใหม่เข้ามา tune item แล้วรายงานว่า bonus
  ไม่ตรงกับที่ตั้ง; (ii) Balance pass พบ build dominant strategy ที่อาศัย scale
  mismatch นี้

### R-23 — AdditionalMoveSpeed = misnamed override, no stack
- **Source:** S4-07 investigation (2026-05-08); `NetworkVariable.cs:40-44`,
  `ActorDriver.cs:271`, `CupidQAction.cs:33,119`, `HerculesRAction.cs:70,204`;
  `design/gdd/movement-navigation-system.md §4 / §Known Issues`
- **Probability:** Medium (จะ realized เมื่อมี ability ที่ 3 ใช้ field นี้)
- **Impact:** Medium (collision = silent move-speed bugs ระหว่าง ability charging /
  slow; SLOW status effect บน hero ที่กำลัง override จะไม่มีผล → balance escape)
- **Status:** Open (documented, not currently exploitable)
- **Owner:** gameplay-programmer + technical-director (semantics decision)
- **Mitigation:**
  (a) **Documentation done** — `movement-navigation-system.md` §4 + §Known Issues
      ระบุ semantic ที่แท้จริง + write sites + bug taxonomy (S4-07);
  (b) **Hold the line** — ห้าม ability ใหม่เขียน `AdditionalMoveSpeed` จนกว่าจะ
      redesign field (use `move_speed` stat with ModifierType.Percent แทน);
  (c) **Phase 2 redesign:** เลือก one of (1) rename → `OverrideMoveSpeed` +
      เพิ่ม API `Push/Pop` พร้อม priority stack, หรือ (2) ลบ field แล้วย้าย
      Cupid Q + Hercules R ไปใช้ `move_speed` stat modifier (ModifierType
      override / temp-effect). Both paths ต้อง playtest re-tune
- **Trigger to escalate:** (i) ออกแบบ ability ใหม่ที่ต้อง override move speed →
  เลือกทางไหน; (ii) Bug report จาก playtest ว่า SLOW ไม่มีผลกับ Cupid ตอน Q
  หรือ Hercules ตอน R

---

## Summary

| Severity (Prob × Impact) | Count | IDs |
|--------------------------|-------|-----|
| Critical (High prob × Critical impact) | 0 | — |
| High (Medium+ prob × High+ impact) | 4 | R-02, R-03, R-06, R-09 |
| Medium | 12 | R-01, R-04, R-05, R-07, R-10, R-15, R-16, R-17, R-18, R-20, R-22, R-23 |
| Low | 7 | R-08, R-11, R-12, R-13, R-14, R-19, R-21 |

**By status:**
- Open: 12 (R-02, R-07, R-09, R-11, R-16, R-17, R-18, R-19, R-20, R-21, R-22, R-23)
- Mitigating: 8 (R-01, R-03, R-04, R-05, R-06, R-10, R-12, R-14, R-15)
- Monitoring: 2 (R-08, R-13)
- Realized (active): 6 (R-05, R-09, R-10, R-11, R-16, R-17, R-18, R-19) — overlap กับ Open/Mitigating

---

## How to use

**Adding a risk:**
1. ตอน ADR ใหม่ Accepted → ดึง §Risks ของ ADR มาใส่ที่นี่ + อ้าง source
2. ตอน sprint retro พบ pattern → spawn risk ใหม่
3. ID เรียงต่อ (R-NN); ห้าม renumber

**Reviewing:**
1. ทุก sprint boundary: producer อ่าน register, อัพเดต Status ต่อ row,
   เลื่อน "Last full review" ที่หัวไฟล์
2. Trigger conditions ในแต่ละ row คือ alarm — ถ้า trigger fire ให้ escalate
   (spawn story / ADR / RCA)

**Closing a risk:**
- เปลี่ยน Status เป็น `Closed` + เพิ่ม "Closed: YYYY-MM-DD — reason" ใน row
- ห้ามลบ row (audit trail)

---

## Source documents

- `docs/architecture/ADR-0001` … `ADR-0007` — §Risks ของแต่ละ ADR
- `production/sprints/sprint-004.md` §Risks
- `production/retrospectives/sprint-003.md` §Action items
- `production/backlog/architecture-epics.md` — long-term refactor exposure
- `production/session-state/active.md` — Open questions
