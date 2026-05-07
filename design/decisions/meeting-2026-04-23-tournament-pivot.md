---
title: World Map & Tournament System Pivot
date: 2026-04-23
status: approved
type: leadership-decision
supersedes:
  - design/gdd/territory-war.md (FT10)
  - design/gdd/systems-index.md entries FT10a, FT10b
impacts:
  new_systems:
    - tournament-system
    - world-map-10-cities
    - faction-system
    - fragment-meta-system
    - wager-mode
    - reputation-anti-toxicity
    - manga-influence-loop
  revised_systems:
    - game-concept.md
    - level-xp-system.md
    - item-system.md
    - gold-economy.md
    - game-mode-manager.md
    - dungeon-mode.md
    - hero-system.md
    - systems-index.md
  deprecated_systems:
    - territory-war.md (FT10)
    - citizen-system (FT10a — never authored)
    - mercenary-system (FT10b — never authored)
source_document: Google Docs — "สำเนาของ Meeting Summary 23/04/2026"
---

# สรุปการประชุม — World Map & Tournament System

**วันที่:** 23 เมษายน 2569 (2026-04-23)

> **หมายเหตุการตีความ (จากผู้ใช้ 2026-04-27):**
> - "Declare War" ในเอกสารต้นฉบับ = **Territory War** (FT10) ในระบบเดิม
> - **World map เปลี่ยนจาก 1 ล้านเมือง → 10 ดาว** (Neutral Cities)

---

## 1. ระบบ Tournament (แทน Declare War)

Tournament เข้ามาแทน Declare War (Territory War) โดยสมบูรณ์ มุ่งเน้นการแข่งขันเพื่อความสนุกและรางวัล แทนการทำสงครามระหว่างกลุ่ม

| หัวข้อ | รายละเอียด | ประเภท |
| ----- | ----- | :---: |
| **รูปแบบหลัก** | MOBA 5v5 เป็นหลัก / รองรับ 1v1, 2v2, 3v3 ตาม Request | **MOBA 5v5** |
| **โหมดการเล่น** | Casual — เล่นสนุก ไม่มี Stake | **Casual** |
|  | Ranked — แข่งจริงจัง มีผลต่อ Reward | **Ranked** |
| **Fragment Reward** | เฉพาะผู้เล่น Top 10% ของ Ranked เท่านั้นที่ได้ Fragment | **Top 10%** |
| **Fragment Drop** | Random ทุกวันตามดาวต่างๆ ทั่ว Universe — ทุกฝ่ายยกทีมไปแย่งชิงกัน | **Daily Random** |
| **Wager Mode** | Fragment วันนั้นหมดแล้ว → วาง Fragment ตัวเองเป็นเดิมพันชิงของคนอื่น | **Wager** |

---

## 2. World Map — 10 เมืองกลาง (Neutral City)

- 10 เมือง (ดาวเคราะห์) เป็นพื้นที่ Socialize ไม่มีเจ้าของ ทุกคนเข้าได้
- มี 5-6 ฝ่าย ผู้เล่นเลือกฝ่ายตั้งแต่แรก สามารถเปลี่ยนได้แต่มีเงื่อนไข
- Fragment Drop ตามดาวต่างๆ ทั่ว Universe — ทุกฝ่ายรู้และยกทีมไปแย่งชิงกัน
- Theme ของแต่ละเมืองมีธีมเฉพาะตัว — ยังอยู่ระหว่าง Design

---

## 3. ระบบ Fragment & Meta Game

| หัวข้อ | รายละเอียด |
| ----- | ----- |
| **วิธีสะสม** | ฝ่ายแต่ละฝ่ายสะสม Fragment ร่วมกัน / ถ้า Inactive นาน Fragment คืนกองกลาง |
| **เป้าหมายสุดท้าย** | สะสม ~100 ล้าน Fragment → รวมเป็นแหวน → ฝ่ายที่ชนะกำหนด "กฎของ Universe" |
| **เชื่อมโยงมังงะ** | ผลในเกม Influence Story ในมังงะ — ผู้เล่นเก่งอาจกลายเป็นตัวละครในมังงะ |
| **Reset Cycle** | กำลังพิจารณา Cycle ประมาณ 3 ปีต่อครั้ง |

---

## 4. ระบบ Dungeon

| หัวข้อ | รายละเอียด |
| ----- | ----- |
| **รูปแบบ** | Personal / Side Quest — เล่นคนเดียวหรือกลุ่ม 1-5 คน มีบอท |
| **รูปแบบการต่อสู้** | เหมือน MOBA เดิม |
| **Item Drop** | ⏳ ยังไม่ได้สรุปว่าเอาไปใช้ทำอะไร — รอ Design เพิ่มเติม |

---

## 5. Game Simplification — ทำให้เกมจบเร็ว

**เป้าหมาย:** 5v5 จบภายใน 15 นาที และถึง Max Level ภายใน 10 นาที

| หัวข้อ | แนวทาง | ทีม |
| ----- | ----- | ----- |
| **เวลาจบเกม** | 5v5 จบภายใน 15 นาที | Dev |
| **Level Cap** | ถึง Max Level ภายใน 10 นาที | Dev |
| **XP System** | ปรับให้ขึ้นเร็วขึ้น หรือลด XP ที่ต้องใช้ต่อเลเวล | Dev |
| **Item Build** | ล็อค Build ต่อตัวละคร — ลด Overwhelm ผู้เล่น | Design |
| **Last Hit** | พิจารณาตัดออก หรือปรับให้ได้เงินน้อยลงถ้า Last Hit ไม่ได้ | Design |
| **Item Stat** | พิจารณาตัด/Simplify — คง Consumable & Skill Item | Design |
| **Base Mode** | Standard = 5v5 / โหมด 25v25 ปล่อยตามธรรมชาติ | Design |

---

## 6. ระบบ Anti-Toxicity

| มาตรการ | รายละเอียด |
| ----- | ----- |
| **เป้าหมาย** | Toxicity ต้องน้อยกว่าทุก MOBA ในตลาด — เป็น Key Feature ของเกม |
| **Reputation System** | หักคะแนนเมื่อพิมพ์คำหยาบ/ด่า → ถูกล็อคจาก Ranked Mode |
| **Filter คำหยาบ** | Block คำ + แสดงเป็น *** — ระบบมีอยู่แล้ว รอ integrate เพิ่มเติม |

---

## 7. Action Items

**นัดประชุมครั้งต่อไป:** ยังไม่ได้นัดวันเวลา — เก็บ Requirement เพิ่มเติม

| ผู้รับผิดชอบ | Task | หมายเหตุ | Due Date |
| :---: | ----- | ----- | ----- |
| **อัพ** | หา Reference รูป Planet / World Map เพิ่ม | ส่งใน Line | 2026-04-24 |
| **ทีม MOBA** | ทำ MOBA ให้สะอาด ไม่มี Bug ก่อน Test | โฟกัส Simplify | - |
| **เอิธ - พจน์** | หา Solution ทำให้เกมจบ 15 นาที และ Level Up ใน 10 นาที | นำเสนอทีม | 2026-05-07 |
| **เอิธ - พจน์** | หา Solution เรื่อง Anti-Toxicity มาเสนอ | นำเสนอทีม | 2026-05-07 |
| **พจน์** | แก้ Board ใน Figma — เพิ่ม Task เกี่ยวกับ MOBA Optimization | อัพเดต Board | 2026-05-01 |

— จบสรุปการประชุม —

---

## Open Questions (ติด `[Open Question]` ใน GDD ใหม่ทุกตัว)

1. **Theme เฉพาะตัวของแต่ละเมือง 10 ดาว** — ยังไม่กำหนด
2. **จำนวนฝ่ายที่แน่นอน** — 5 หรือ 6
3. **เงื่อนไขการเปลี่ยนฝ่าย** — ยังไม่กำหนด
4. **Item Drop ของ Dungeon** — เอาไปใช้ทำอะไร
5. **Reset Cycle ของ Universe** — 3 ปีหรือไม่
6. **Solution Last Hit** — ตัดออก หรือลดเงิน (รอ 2026-05-07)
7. **Solution ให้เกมจบ 15 นาที + Max Level 10 นาที** — รอ proposal (2026-05-07)
8. **Solution Anti-Toxicity เชิงระบบ** — รอ proposal (2026-05-07)
9. **Manga Influence Loop** — กลไกที่แน่นอน, criteria ที่ผู้เล่น "เก่งพอ" จะกลายเป็นตัวละคร
10. **โหมด 25v25** — "ปล่อยตามธรรมชาติ" หมายถึงอะไรในเชิง implementation
