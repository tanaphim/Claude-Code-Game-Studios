# Sprint 1 — 2026-04-03 to 2026-04-17

## Sprint Goal

ปิด known issues ที่ block gameplay loop หลัก: Item animation ใน Unity Editor,
ตรวจสอบ bug ที่ระบุใน GDD, และ verify core combat/networking ทำงานถูกต้อง

## Capacity

- Total days: 14 (2 สัปดาห์)
- Buffer (20%): 2.8 วัน สำรองสำหรับงานที่ไม่ได้วางแผน
- Available: ~10 วัน

---

## Tasks

### Must Have (Critical Path)

| ID | Task | Agent/Owner | Est. Days | Dependencies | Acceptance Criteria |
|----|------|-------------|-----------|-------------|-------------------|
| S1-01 | สร้าง Animator States `Item_Recall_Perform`, `Item_Consume_Perform`, `Item_Spell_Perform`, `Item_Attack_Perform` ใน AnimatorController ต่อ Hero | unity-specialist | 2.0 | Animation code (merged) | ใบวาร์ปเล่น recall pose, ขวดยาเล่น consume pose ถูกต้อง |
| S1-02 | เพิ่ม `Item_Viable` bool parameter ใน Animator ต่อ Hero | unity-specialist | 0.5 | S1-01 | `GetViable(SkillKey.Item)` และ `SetViable(SkillKey.Item, true/false)` ทำงานโดยไม่ error |
| S1-03 | ตรวจสอบและ fix `attack_speed` / `move_speed` ÷100 bug | gameplay-programmer | 1.0 | — | ยืนยันว่าค่าใน CBS ตั้ง 0–100; stat apply กับ Actor ถูกต้อง |
| S1-04 | ตรวจสอบ Photon Fusion Tick Rate config | network-programmer | 0.5 | — | ระบุค่า TickRate จริงใน Fusion Project Settings; อัพเดต networking-core.md |
| S1-05 | ตรวจสอบ `AvailableToPurchase()` Role Restriction | gameplay-programmer | 0.5 | — | ยืนยันว่า `Positions[]` บังคับใช้จริง หรือ document ว่าเป็น unimplemented |

**Must Have Subtotal: 4.5 วัน**

### Should Have

| ID | Task | Agent/Owner | Est. Days | Dependencies | Acceptance Criteria |
|----|------|-------------|-----------|-------------|-------------------|
| S1-06 | Reverse-document Mythic Passive Bonus formula จากโค้ด | game-designer | 1.0 | — | อัพเดต `item-system.md §4` ด้วยสูตรและตัวแปรที่แท้จริง |
| S1-07 | ตรวจสอบ `AdditionalMoveSpeed` override bug (หลาย buff พร้อมกัน) | gameplay-programmer | 1.0 | — | หลาย speed buff stack ถูกต้อง; อัพเดต movement-navigation-system.md |
| S1-08 | สร้าง Risk Register เริ่มต้น | producer | 0.5 | — | `production/risk-register/risk-register.md` พร้อมรายการ risk จาก ADRs + GDDs |

**Should Have Subtotal: 2.5 วัน**

### Nice to Have

| ID | Task | Agent/Owner | Est. Days | Dependencies | Acceptance Criteria |
|----|------|-------------|-----------|-------------|-------------------|
| S1-09 | ออกแบบ AI Bot item-buying system | game-designer | 2.0 | — | GDD update `ai-bot-system.md` พร้อม logic design ที่ implementable |
| S1-10 | ออกแบบ AI Difficulty Level system | game-designer | 1.0 | S1-09 | Difficulty multiplier (Easy/Normal/Hard) defined ใน GDD พร้อม tuning knobs |

**Nice to Have Subtotal: 3.0 วัน**

**Total Estimated: 10.0 วัน** (fits within available capacity)

---

## Carryover from Previous Sprint

ไม่มี — Sprint แรก

---

## Risks

| Risk | Probability | Impact | Mitigation |
|------|------------|--------|-----------|
| Animator States ต่อ hero ใช้เวลานาน (25+ heroes) | สูง | ปานกลาง | เริ่มจาก hero ที่ใช้ item บ่อย; สร้าง base Animator template ให้ hero อื่น copy |
| `attack_speed` ÷100 bug กระทบ balance ที่มีอยู่ | ปานกลาง | สูง | ทดสอบกับ hero sample ก่อน fix; บันทึกใน ADR ถ้าต้องเปลี่ยน CBS data format |
| Tick Rate config ไม่ได้ตั้งค่าไว้ | ปานกลาง | ปานกลาง | ถ้าใช้ Fusion default — document และ test ว่า default พอสำหรับ 5v5 |
| Mythic Passive formula ซับซ้อนกว่าที่คาด | ต่ำ | ต่ำ | Document ตามโค้ดที่มี ไม่ต้อง redesign |

---

## Dependencies on External Factors

- Unity Editor เข้าถึง `delta-unity` AnimatorControllers ได้ (S1-01, S1-02)
- Photon Fusion Project Settings เข้าถึงได้ (S1-04)
- PlayFab CBS Dashboard เข้าถึงได้สำหรับตรวจ stat values (S1-03)

---

## Definition of Done for this Sprint

- [ ] S1-01: Item animation เล่นถูก clip สำหรับ hero อย่างน้อย 1 ตัว (proof of concept)
- [ ] S1-02: `Item_Viable` parameter ทำงานบน hero ที่ทำ S1-01
- [ ] S1-03: `attack_speed`/`move_speed` behavior documented และ fix ถ้าเป็น bug
- [ ] S1-04: Tick Rate ถูกระบุใน networking-core.md
- [ ] S1-05: Role Restriction status documented
- [ ] S1-06: Mythic Passive formula อยู่ใน item-system.md §4
- [ ] S1-07: AdditionalMoveSpeed bug status documented
- [ ] S1-08: Risk Register สร้างแล้ว
- [ ] GDD ที่เกี่ยวข้องอัพเดตสำหรับทุก finding
- [ ] ไม่มี S1/S2 bugs ใหม่จาก tasks ในสปรินท์นี้

---

## Source

สร้างจาก known issues ใน GDDs ต่อไปนี้:
- `design/gdd/item-system.md` — 5 ⚠️ issues
- `design/gdd/networking-core.md` — 4 ⚠️ issues
- `design/gdd/movement-navigation-system.md` — 4 ⚠️ issues
- `design/gdd/ai-bot-system.md` — 5 ⚠️ issues
