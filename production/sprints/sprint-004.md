# Sprint 4 — 2026-05-16 to 2026-05-29

## Sprint Goal

ปิด **carryover batch** ที่ค้างจาก Sprint 002 (animator + bug investigations + docs) และ
Sprint 003 (AI Bot items + difficulty) ให้จบในสปรินท์เดียว เพื่อ unblock **Sprint 005 =
Phase 2 Hercules pilot migration** (focused ability sprint)

**Theme:** "Animator + Bug Hunting + AI Bot batch" — ไม่มี ability-system migration ใน
สปรินท์นี้ ตามแผน (Phase 2 ย้ายไป Sprint 005)

---

## Capacity

- Total days: 14 (2 สัปดาห์)
- Buffer (20%): 2.8 วัน สำรองสำหรับงานที่ไม่ได้วางแผน
- Available: ~11 วัน

---

## Tasks

### Must Have (Critical Path — close the long-running carryover)

Re-numbered from Sprint 002 carryover (`S2-01..S2-08`). Acceptance criteria carried
over verbatim — these stories never started.

| ID | Task | Agent/Owner | Est. Days | Dependencies | Acceptance Criteria |
|----|------|-------------|-----------|-------------|-------------------|
| S4-01 | สร้าง Animator States `Item_Recall_Perform`, `Item_Consume_Perform`, `Item_Spell_Perform`, `Item_Attack_Perform` ใน AnimatorController ต่อ Hero | unity-specialist | 2.0 | — | Item recall/consume/spell/attack เล่น clip ถูกต้องใน hero อย่างน้อย 1 ตัว |
| S4-02 | เพิ่ม `Item_Viable` bool parameter ใน Animator ต่อ Hero | unity-specialist | 0.5 | S4-01 | `GetViable(SkillKey.Item)` และ `SetViable(SkillKey.Item, true/false)` ทำงานโดยไม่ error |
| S4-03 | ตรวจสอบและ fix `attack_speed` / `move_speed` ÷100 bug | gameplay-programmer | 1.0 | — | ยืนยันว่าค่าใน CBS ตั้ง 0–100; stat apply กับ Actor ถูกต้อง |
| S4-04 | ระบุและ document Photon Fusion Tick Rate config | network-programmer | 0.5 | — | ค่า TickRate จริงถูกระบุใน `networking-core.md` |
| S4-05 | ตรวจสอบ `AvailableToPurchase()` Role Restriction | gameplay-programmer | 0.5 | — | Documented ว่า `Positions[]` enforce จริง หรือระบุว่าเป็น unimplemented |
| S4-06 | Reverse-document Mythic Passive Bonus formula จากโค้ด | game-designer | 1.0 | — | `item-system.md §4` มีสูตรและตัวแปรครบถ้วน |
| S4-07 | ตรวจสอบ `AdditionalMoveSpeed` override bug (หลาย buff พร้อมกัน) | gameplay-programmer | 1.0 | — | Bug status documented; `movement-navigation-system.md` อัพเดต |
| S4-08 | สร้าง Risk Register เริ่มต้น | producer | 0.5 | — | `production/risk-register/risk-register.md` พร้อมรายการ risk จาก ADRs + GDDs |

**Must Have Subtotal: 7.0 วัน**

### Should Have (AI Bot — carryover from Sprint 003)

| ID | Task | Agent/Owner | Est. Days | Dependencies | Acceptance Criteria |
|----|------|-------------|-----------|-------------|-------------------|
| S4-09 | AI Bot item-buying behavior (was S3-08 / S2-09) | ai-programmer + game-designer | 2.0 | — | Bot buys items ตาม role + gold threshold; อ้างอิง `ai-bot-system.md`; playtest 1 match ไม่มี crash |
| S4-10 | AI Bot Difficulty Level — Easy/Normal/Hard (was S3-09 / S2-10) | ai-programmer | 1.0 | S4-09 | 3 tiers ต่างกันที่ reaction time + item-buying priority; documented ใน `ai-bot-system.md §Difficulty` |

**Should Have Subtotal: 3.0 วัน**

### Nice to Have — Phase 1b polish backlog

ถ้า Must Have + Should Have เสร็จก่อนกำหนดและยังเหลือ buffer (~1.0 วัน) เลือกหยิบจาก
list นี้. **ทุกตัวไม่ block Phase 2** — defer ทั้งหมดไป Sprint 006+ ก็ได้ถ้า Sprint 004
แน่น

| ID | Task | Agent/Owner | Est. Days | Dependencies | Acceptance Criteria |
|----|------|-------------|-----------|-------------|-------------------|
| S4-P1 | `AbilityMultipeerRunner` duplicate-Start guard (peer scene clone fix) | network-programmer | 0.5 | — | Console clean ตอน multipeer harness run — ไม่มี GameIsFull cascade |
| S4-P2 | `AbilityRegistry` boot-time optimization (Resources.LoadAll → build-time manifest หรือ Addressables) | unity-addressables-specialist | 1.5 | — | Cold-start scan < 100 ms / 158 prefabs (ปัจจุบัน 511.9 ms warm / 1079 ms cold) |
| S4-P3 | `KeybindMap` runtime UGUI Settings panel (`Assets/Scenes/Settings/Controls.unity`) + UX sign-off | ui-programmer + ux-designer | 1.5 | — | Player remap Q→X จาก in-game settings; persist ข้าม session; UX designer sign-off "non-blocking for Phase 2" |
| S4-P4 | `CBSKeybindDefaults` schema + `KeybindMap.TryLoadDefaultsFromCBS()` body | gameplay-programmer + game-designer | 0.5 | designer ใส่ field ใน CBS dashboard | KeybindMap load defaults จาก CBS เมื่อ schema มี; fallback เดิมยังทำงาน |
| S4-P5 | PeerMode toggle automation (Multipeer ↔ Production) — Editor menu OR scene-bound override OR per-scene config asset | tools-programmer หรือ network-programmer | 0.5 | — | Toggle PeerMode ได้โดยไม่ต้องแก้ `NetworkProjectConfig.fusion` ตรงๆ; ลืม revert ไม่ทำให้ production playtest พัง. ดู `docs/dev-workflow/peer-mode-toggle.md` |

**Nice to Have Subtotal: 4.5 วัน** (ทั้งหมด — ไม่ได้คาดว่าจะหยิบครบใน Sprint 004)

**Total Estimated: 10.0 วัน core + เลือก 1 polish ได้ตาม buffer** (ภายใน capacity 11d)

---

## Carryover from Previous Sprints

### Accepted into Sprint 004

| Task | เหตุผล | From Sprint | Estimate |
|------|--------|-------------|----------|
| S4-01 (was S2-01) | ค้างจาก Sprint 002, ไม่ได้เริ่ม 2 sprints ติด | 002 | 2.0 วัน |
| S4-02 (was S2-02) | depends on S4-01 | 002 | 0.5 วัน |
| S4-03 (was S2-03) | bug investigation, ค้างมาตั้งแต่ Sprint 001 | 002 | 1.0 วัน |
| S4-04 (was S2-04) | networking documentation | 002 | 0.5 วัน |
| S4-05 (was S2-05) | role restriction documentation | 002 | 0.5 วัน |
| S4-06 (was S2-06) | mythic passive formula doc | 002 | 1.0 วัน |
| S4-07 (was S2-07) | AdditionalMoveSpeed bug | 002 | 1.0 วัน |
| S4-08 (was S2-08) | risk register init | 002 | 0.5 วัน |
| S4-09 (was S3-08) | AI Bot item-buying — deferred 2026-04-21 by user | 003 | 2.0 วัน |
| S4-10 (was S3-09) | AI Bot difficulty — deferred 2026-04-21 by user | 003 | 1.0 วัน |

### Deferred from Sprint 004 to Sprint 005+

| Task | เหตุผล |
|------|--------|
| Phase 2 P2-01..P2-10 (Hercules pilot, 3.75d) | Sprint 005 = focused ability migration sprint (separation of concerns; AI/animation/bugs ไม่ปนกับ ability refactor) |
| Phase 1b polish (S4-P1..S4-P4) | flagged as Nice to Have; pulled into Sprint 004 only if buffer allows |

---

## Risks

| Risk | Probability | Impact | Mitigation |
|------|------------|--------|-----------|
| S4-01/S4-02 animator work expands beyond 1 hero (per-hero AnimatorController) | Medium | Medium | Acceptance criteria scoped to "1 hero proof-of-concept"; full hero rollout = separate Sprint 005+ story per hero |
| S4-03/S4-07 bug investigation reveals deeper architecture issue | Medium | High | Time-box at estimate; if root cause requires refactor → spawn ADR + new story for Sprint 005, document finding here |
| S4-09/S4-10 AI Bot baseline unclear (no playtest history) | Medium | Low | Use MOBA reference values (DotA2/LoL) as default tier values; tune post-Sprint via playtest |
| Sprint 002 carryover slips for 3rd consecutive sprint | Low (with this plan) | High | These stories now Must Have priority; if any slips again, escalate to producer for capacity rebalance and root-cause why animator + bug work keeps deferring |
| User direction shifts to Phase 2 mid-sprint | Medium | Medium | Sprint goal explicit ("no ability-system migration this sprint"); Phase 2 readiness already locked in via `ADR-0006-phase-2-migration-plan.md` — can pull P2-01..P2-10 into Sprint 005 plan instantly |

---

## Dependencies on External Factors

- Unity Editor + AnimatorController access to delta-unity heroes (S4-01, S4-02)
- Photon Fusion Project Settings access (S4-04)
- PlayFab CBS Dashboard access สำหรับตรวจ stat values (S4-03)
- `ai-bot-system.md` GDD baseline (S4-09, S4-10) — ต้อง available ก่อน Sprint start

---

## Definition of Done for this Sprint

### Per-Task Gates

- [ ] S4-01: Item animation เล่นถูก clip สำหรับ hero อย่างน้อย 1 ตัว
- [ ] S4-02: `Item_Viable` parameter ทำงานบน hero ที่ทำ S4-01
- [ ] S4-03: `attack_speed`/`move_speed` behavior documented + fix ถ้าเป็น bug
- [ ] S4-04: Tick Rate ถูกระบุใน `networking-core.md`
- [ ] S4-05: Role Restriction status documented
- [ ] S4-06: Mythic Passive formula อยู่ใน `item-system.md §4`
- [ ] S4-07: AdditionalMoveSpeed bug status documented
- [ ] S4-08: Risk Register สร้างแล้วที่ `production/risk-register/risk-register.md`
- [ ] S4-09: AI Bot buys items ใน 1 match (no crash)
- [ ] S4-10: 3 difficulty tiers documented + tested

### Cross-cutting

- [ ] ไม่มี S1/S2/S3 bugs ใหม่จาก tasks ในสปรินท์นี้
- [ ] GDD ที่เกี่ยวข้องอัพเดตสำหรับทุก finding
- [ ] Sprint 002 carryover backlog = **0** หลังสปรินท์นี้ปิด (S2-01..S2-08 ปิดครบ)
- [ ] Sprint 005 plan สร้างแล้ว — Phase 2 P2-01..P2-10 + remaining polish

---

## Progress

- **2026-05-08** — S4-01 + S4-02 Item Animation **VERIFIED DONE** (no work
  needed; pre-existing). All 5 item states (`Item_Recall_Perform`,
  `Item_Consume_Perform`, `Item_Spell_Perform`, `Item_Attack_Perform`,
  `Item_Perform`) **และ** `Item_Viable` bool parameter มีอยู่แล้วใน shared
  base controller `Assets/Animations/RadiusBasicLocomotion.controller`
  (GUID `d5cada5dadda5f44db70f1faa1c641fc`). Hero override controllers
  ทั้ง 22/25 inherit states อัตโนมัติ. Acceptance ผ่านทั้งคู่. Story เหล่านี้
  ค้างมาตั้งแต่ Sprint 002 เพราะไม่มีใคร verify — ตอน reverse-doc พบว่าเสร็จไป
  นานแล้ว. **Gap ที่พบ:** 3 Garen variant override controllers
  (`GarenCompleteLocomotion`, `GarenButKingArthur*`, `GarenButXinZhoa*`)
  ใช้ `GameCreator/CompleteLocomotion.controller` เป็น base ซึ่งไม่มี
  item states — น่าจะเป็น legacy test prototypes; defer Sprint 005+ เพื่อ
  confirm + ลบหรือ swap base. อัพเดต `item-system.md` §Known Issues.
- **2026-05-08** — S4-07 AdditionalMoveSpeed override **DONE** (documented, no
  code change). Field คือ single `float` ใน `NetworkVariable.cs:40-44` (ไม่มี
  stack). **Misnamed**: semantic จริงคือ `OverrideMoveSpeed` — เมื่อ > 0 จะ
  bypass `move_speed` stat ทั้งหมด (รวม item bonus + SLOW). Write sites ปัจจุบัน
  เฉพาะ `CupidQAction.cs:33,119` + `HerculesRAction.cs:70,204` (charge ramp);
  เขียนบน *self* และคนละ hero → **ไม่ collide ในทางปฏิบัติตอนนี้**. Bug taxonomy
  (Last-write-wins, Cleanup hardcoded 0, Base stat bypass) จะ realized เมื่อมี
  ability ที่ 3 ใช้ field นี้. อัพเดต `movement-navigation-system.md` §4
  (write sites table + naming caveat) §Known Issues. เพิ่ม **R-23** (Medium /
  Open). Code rename + stack design defer to Phase 2 (rename กระทบ
  `NetworkVariable` dict key serialization).
- **2026-05-08** — S4-03 attack_speed / move_speed ÷100 **DONE** (documented, no code change).
  Findings: (1) `NetworkStat.cs:165-184` — base init ใช้ /100 สำหรับ
  `MoveSpeed`, `AttackRange`, `GameplayRadius`, `AcqRadius`, `SightRange`
  (CBSUnit เก็บเป็น int ×100 ของ runtime); `AttackSpeed` **NOT** /100
  (raw multiplier 0.5..2.0). (2) `NetworkHeroInventory.cs:1299-1304` หาร
  item bonus /100 **เสมอ** สำหรับ `attack_speed` + `move_speed` ไม่ว่า
  `ModifierType` เป็น Flat หรือ Percent — ต่างจาก stats อื่นที่ /100
  เฉพาะ Percent. ผลคือ Flat กับ Percent ให้ค่าเหมือนกันสำหรับ 2 stats
  นี้ และ designer trap: item "+30 move_speed" → +0.30 runtime ≈ +8.6%
  บน base 3.5. Acceptance "ค่าใน CBS ตั้ง 0–100" **FALSE** สำหรับ
  MoveSpeed (200..500). อัพเดต `movement-navigation-system.md` §4 (เพิ่ม
  CBS-to-runtime convention + Item Modifier Conversion) §Known Issues;
  `item-system.md` §Known Issues. เพิ่ม **R-22** ใน risk register
  (Medium / Open). Code fix defer to Phase 2 balance pass.
- **2026-05-08** — S4-04 Photon Tick Rate doc **DONE**. ค่าจริงจาก
  `Assets/Photon/Fusion/Resources/NetworkProjectConfig.fusion`:
  TickRate **64 Hz** (`Simulation.TickRateSelection.Client`), Send rates
  Server/Client = **0 → match tick rate**, PlayerCount **10**,
  SimulationUpdateTimeMode 0 (Engine update), ConnectionTimeout **120 s**,
  ConnectionShutdownTime 1.0 s. อัพเดต `networking-core.md` §4 Formulas
  (60→64 tps), §7 Tuning Knobs (เพิ่ม 6 แถว Photon config), §Known Issues
  (resolved warning #1). Acceptance criteria ผ่าน: ค่า TickRate จริงระบุใน doc.
- **2026-05-08** — S4-05 Role Restriction **DONE**. Confirmed unimplemented:
  `ItemObject.Positions : Role[]` declared (`ItemObject.cs:21`) แต่
  `AvailableToPurchase()` ไม่ check และ UI references ใน `UIInGameShopView.cs`
  (ll. 521–526, 750, 792) ถูก comment-out ทั้งหมด. อัพเดต `item-system.md`
  §5 Edge Cases, §6 Dependencies (Hero), §Known Issues. เพิ่ม R-21 (Low/Open).
- **2026-05-08** — S4-06 Mythic reverse-doc **DONE**. Updated `item-system.md`
  §3.7 (schema + status), §4 (proposed formula + 4 open questions), §7 (tuning
  knob), §8 (acceptance gated), §Known Issues. **Key finding:** `MythicItemEffect`
  field declared ใน `ItemObject.cs:24,67-72` แต่**ไม่มี read site**ในทั้ง
  `Assets/GameScripts` → schema-only / unimplemented. เพิ่ม R-20 ใน risk register
  สำหรับ wire-up Sprint 005+. `HasMythic` purchase gate (1 Mythic / Hero) ทำงานปกติ.
- **2026-05-07** — S4-08 Risk Register **DONE**. สร้าง
  `production/risk-register/risk-register.md` 19 risks (R-01..R-19) ดึงจาก
  ADR-0001..0007 + Sprint 003 retro + Phase 1b polish backlog. Categorized
  Technical (9) / Networking (4) / Process (4) / Phase 1b hangover (2).
  Severity summary: 0 Critical / 4 High / 9 Medium / 6 Low.

---

## Source

สร้างจาก:
- [Sprint 003 §Carryover](sprint-003.md) — S3-08, S3-09 deferred 2026-04-21 by user
- [Sprint 002 §Tasks](sprint-002.md) — S2-01..S2-08 carried since Sprint 002
- [ADR-0006 Phase 1b §4 Closure notes](../../docs/architecture/ADR-0006-phase-1b-implementation.md) — Phase 1b polish backlog flagged for Sprint 004 nice-to-have
- [ADR-0006 Phase 2 Migration Plan](../../docs/architecture/ADR-0006-phase-2-migration-plan.md) — Phase 2 work breakdown (deferred to Sprint 005)
