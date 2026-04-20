# Sprint 2 — 2026-04-18 to 2026-05-01

## Sprint Goal

ปิด Sprint 001 carryover ทั้งหมด (Item animation, combat/networking bugs, risk register)
และเริ่มออกแบบระบบ AI Bot + กำหนด Gold Economy values

## Capacity

- Total days: 14 (2 สัปดาห์)
- Buffer (20%): 2.8 วัน สำรองสำหรับงานที่ไม่ได้วางแผน
- Available: ~11 วัน

---

## Tasks

### Must Have (Critical Path)

| ID | Task | Agent/Owner | Est. Days | Dependencies | Acceptance Criteria |
|----|------|-------------|-----------|-------------|-------------------|
| S2-01 | สร้าง Animator States `Item_Recall_Perform`, `Item_Consume_Perform`, `Item_Spell_Perform`, `Item_Attack_Perform` ใน AnimatorController ต่อ Hero | unity-specialist | 2.0 | — | Item recall/consume/spell/attack เล่น clip ถูกต้องใน hero อย่างน้อย 1 ตัว |
| S2-02 | เพิ่ม `Item_Viable` bool parameter ใน Animator ต่อ Hero | unity-specialist | 0.5 | S2-01 | `GetViable(SkillKey.Item)` และ `SetViable(SkillKey.Item, true/false)` ทำงานโดยไม่ error |
| S2-03 | ตรวจสอบและ fix `attack_speed` / `move_speed` ÷100 bug | gameplay-programmer | 1.0 | — | ยืนยันว่าค่าใน CBS ตั้ง 0–100; stat apply กับ Actor ถูกต้อง |
| S2-04 | ระบุและ document Photon Fusion Tick Rate config | network-programmer | 0.5 | — | ค่า TickRate จริงถูกระบุใน `networking-core.md` |
| S2-05 | ตรวจสอบ `AvailableToPurchase()` Role Restriction | gameplay-programmer | 0.5 | — | Documented ว่า `Positions[]` enforce จริง หรือระบุว่าเป็น unimplemented |
| S2-06 | Reverse-document Mythic Passive Bonus formula จากโค้ด | game-designer | 1.0 | — | `item-system.md §4` มีสูตรและตัวแปรครบถ้วน |
| S2-07 | ตรวจสอบ `AdditionalMoveSpeed` override bug (หลาย buff พร้อมกัน) | gameplay-programmer | 1.0 | — | Bug status documented; `movement-navigation-system.md` อัพเดต |
| S2-08 | สร้าง Risk Register เริ่มต้น | producer | 0.5 | — | `production/risk-register/risk-register.md` พร้อมรายการ risk จาก ADRs + GDDs |

**Must Have Subtotal: 7.0 วัน**

### Should Have

| ID | Task | Agent/Owner | Est. Days | Dependencies | Acceptance Criteria |
|----|------|-------------|-----------|-------------|-------------------|
| S2-11 | กำหนด Gold Economy values — minion gold ทุกประเภท + Buyback cost formula | game-designer | 1.0 | — | `gold-economy.md` ไม่มี ⚠️ TODO เหลือสำหรับ minion gold และ buyback |
| S2-14 | ✅ **ADR-0006 Phase 1a** — ออกแบบ interface signature + data contract ของ `AbilityRegistry`, `AbilityComponent`, `KeybindMap`, `AbilityDataSnapshot` + dummy test-scene prototype (ไม่แตะ hero เก่า / SkillKey) | lead-programmer + unity-specialist | 3.0 | — | **DONE 2026-04-21** — ADR §9.1: Pass #1-3 ✅, #4 🟡 PARTIAL (argument), #5 ⏸ DEFERRED→Phase 1b. Prototype Q/W/E/R → 4 slots Execute fired. Gate: Phase 1b unblocked |

**Should Have Subtotal: 4.0 วัน**

> **Deferred to Sprint 003:** S2-09 (AI Bot item-buying, 2d) และ S2-10 (AI Difficulty
> Level, 1d) ถูกดันไป Sprint 003 เพื่อเปิดทางให้ S2-14 (ADR-0006 Phase 1a).

### Nice to Have

| ID | Task | Agent/Owner | Est. Days | Dependencies | Acceptance Criteria |
|----|------|-------------|-----------|-------------|-------------------|
| S2-12 | สืบสวน Surrender Reject Vote logic (ถูก comment out) | gameplay-programmer | 0.5 | — | Documented ว่าเป็น bug หรือ design intent; `surrender-system.md` อัพเดต |
| S2-13 | สืบสวน AFK Detection RPC stubs ว่าง | gameplay-programmer | 0.5 | — | Documented ว่า stubs ต้องการ implement หรือเป็น placeholder; `afk-detection.md` อัพเดต |

**Nice to Have Subtotal: 1.0 วัน**

**Total Estimated: 12.0 วัน** (fits within available capacity, 2 วันเหลือเป็น buffer)

---

## Carryover from Previous Sprint

| Task | เหตุผล | Estimate ใหม่ |
|------|--------|--------------|
| S1-01 → S2-01 | ไม่ได้เริ่ม | 2.0 วัน |
| S1-02 → S2-02 | ไม่ได้เริ่ม | 0.5 วัน |
| S1-03 → S2-03 | ไม่ได้เริ่ม | 1.0 วัน |
| S1-04 → S2-04 | ไม่ได้เริ่ม | 0.5 วัน |
| S1-05 → S2-05 | ไม่ได้เริ่ม | 0.5 วัน |
| S1-06 → S2-06 | ไม่ได้เริ่ม | 1.0 วัน |
| S1-07 → S2-07 | ไม่ได้เริ่ม | 1.0 วัน |
| S1-08 → S2-08 | ไม่ได้เริ่ม | 0.5 วัน |
| S1-09 → S2-09 | ไม่ได้เริ่ม | 2.0 วัน |
| S1-10 → S2-10 | ไม่ได้เริ่ม | 1.0 วัน |

---

## Risks

| Risk | Probability | Impact | Mitigation |
|------|------------|--------|-----------|
| Animator States ต่อ hero หลาย ตัว ใช้เวลานานกว่าคาด | สูง | ปานกลาง | เริ่มจาก hero ต้นแบบ 1 ตัว; สร้าง template ให้ hero อื่น copy |
| `attack_speed` ÷100 bug กระทบ balance data ที่มีอยู่ | ปานกลาง | สูง | ทดสอบ hero sample ก่อน fix; บันทึก ADR ถ้าต้องเปลี่ยน CBS format |
| Tick Rate config ไม่ได้ตั้งค่าไว้จริง | ปานกลาง | ปานกลาง | ถ้าใช้ Fusion default — document และ test ว่า default รองรับ 5v5 |
| Sprint 001 carryover ทั้งหมด 10 วัน อาจ overload ถ้ามี blockers | ปานกลาง | สูง | Should Have เป็น optional; cut S2-09/S2-10 ถ้า Must Have ล่าช้า |
| Gold Economy values ต้องการ playtesting ก่อนกำหนด | ต่ำ | ปานกลาง | ใช้ค่าจาก MOBA reference (DotA2/LoL) เป็น baseline ก่อน |

---

## Dependencies on External Factors

- Unity Editor เข้าถึง `delta-unity` AnimatorControllers ได้ (S2-01, S2-02)
- Photon Fusion Project Settings เข้าถึงได้ (S2-04)
- PlayFab CBS Dashboard เข้าถึงได้สำหรับตรวจ stat values (S2-03)

---

## Definition of Done for this Sprint

- [ ] S2-01: Item animation เล่นถูก clip สำหรับ hero อย่างน้อย 1 ตัว (proof of concept)
- [ ] S2-02: `Item_Viable` parameter ทำงานบน hero ที่ทำ S2-01
- [ ] S2-03: `attack_speed`/`move_speed` behavior documented และ fix ถ้าเป็น bug
- [ ] S2-04: Tick Rate ถูกระบุใน `networking-core.md`
- [ ] S2-05: Role Restriction status documented
- [ ] S2-06: Mythic Passive formula อยู่ใน `item-system.md §4`
- [ ] S2-07: AdditionalMoveSpeed bug status documented
- [ ] S2-08: Risk Register สร้างแล้วที่ `production/risk-register/risk-register.md`
- [x] S2-14: ADR-0006 Phase 1a interface document approved + dummy prototype พิสูจน์ input→slot→registry chain ✅ (see `docs/architecture/ADR-0006-phase-1a-interfaces.md` §9)
- [ ] GDD ที่เกี่ยวข้องอัพเดตสำหรับทุก finding
- [ ] ไม่มี S1/S2 bugs ใหม่จาก tasks ในสปรินท์นี้

---

## Progress

### S2-14 — ADR-0006 Phase 1a (Closed 2026-04-21)

**Deliverables:**
- `docs/architecture/ADR-0006-phase-1a-interfaces.md` — Accepted (interfaces §1-8 + findings §9)
- `delta-unity` prototype: `AbilityPrototypeRunner` + `AbilityPrototypeDriver` + `TestInputProvider` + `TestAbilityAction` + `AbilityComponent` (with `[Obsolete] BindSlotPrototype` marker)
- `delta-unity` commits: `b77d382812` (scaffold), `6938a52c2d` (Day 2 wrap-up), `a218eddfaa` (Day 3-Lite state-write probe)

**Pass Criteria:**
| # | Status | Notes |
|---|---|---|
| 1-3 | ✅ PASS | Keybind→slot→registry→Execute chain พิสูจน์ได้ใน `GameMode.Single` |
| 4 | 🟡 PARTIAL | ChangeDetector path argument from Pass #3; wire parity defer ไป Phase 1b |
| 5 | ⏸ DEFERRED (hard) | Single mode elide state serialization → probe ใช้งานไม่ได้; Host+Client harness คือ Phase 1b entry criterion |

**Lessons captured (ADR §9.2):** 7 items — รวม `allowUnsafeCode` asmdef constraint, `AssembliesToWeave` registration, Fusion Statistics zero-counter behavior

**Day count:** Estimate 3.0 วัน → actual ~3 วัน (Day 1-2 core prototype, Day 3-Lite bandwidth probe) — on-budget

**Phase 1b entry criteria (see ADR §9.3):**
1. Host+Client test harness → verify Pass #4/#5 at wire level
2. Integration touch-point audit (`ActorCombatAction` boundary)
3. Real `BindSlot(slot, abilityId)` impl replacing `BindSlotPrototype`

---

## Source

สร้างจาก carryover Sprint 001 และ GDD scan ของ `/project-stage-detect` (2026-04-17):
- `design/gdd/item-system.md`
- `design/gdd/item-animation-system.md`
- `design/gdd/networking-core.md`
- `design/gdd/movement-navigation-system.md`
- `design/gdd/ai-bot-system.md`
- `design/gdd/gold-economy.md`
- `design/gdd/surrender-system.md`
- `design/gdd/afk-detection.md`
