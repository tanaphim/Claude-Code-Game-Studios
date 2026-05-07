# Territory War System

> ## ⛔ SUPERSEDED — 2026-04-23
>
> ระบบนี้ถูก **deprecated** ตามมติ leadership meeting วันที่ 2026-04-23
> ([decision doc](../decisions/meeting-2026-04-23-tournament-pivot.md)).
>
> เนื้อหาด้านล่างถูกเก็บไว้เพื่อ historical reference / audit trail เท่านั้น
> **ห้ามใช้เป็น source of truth สำหรับการ implement หรือ design ใหม่**
>
> **ระบบที่มาแทน:**
>
> | สิ่งที่ Territory War เคยทำ | ระบบใหม่ที่มาแทน |
> |---|---|
> | World Map (1M+ cities, ซื้อด้วย Premium) | **FT12** World Map (10 Neutral Cities — socialize hub, ไม่มีเจ้าของ) |
> | Declare War + MOBA 1v1–25v25 | **FT13** Tournament System (Casual/Ranked, MOBA 5v5 หลัก) |
> | Conquest hierarchy / vassal | **FT11** Faction System (5–6 ฝ่าย, faction-shared progression) |
> | Long-term meta progression | **FT14** Fragment & Meta-Game (~100M Fragment → ring → กฎ Universe) |
> | Premium currency wagering | **FT15** Wager Mode (Fragment-based betting) |
> | FT10a Citizen System (sub-system) | ⛔ Removed — never authored |
> | FT10b Mercenary System (sub-system) | ⛔ Removed — never authored |
>
> ดูรายละเอียดผลกระทบต่อ ADRs ใน
> [change-impact-2026-04-23-tournament-pivot.md](../../docs/architecture/change-impact-2026-04-23-tournament-pivot.md)

> **Status**: ⛔ Superseded (was: In Review)
> **Author**: User + Claude Code agents
> **Last Updated**: 2026-04-04 *(superseded 2026-04-23)*
> **System ID**: FT10 *(deprecated)*
> **Implements Pillar**: แข่งขันจริงจัง (Competitive Core), ความลึกเชิงกลยุทธ์ (Strategic Depth)

## Overview

Territory War คือโหมด Meta-game ที่ซ้อนอยู่บน World Map ขนาดใหญ่ซึ่งประกอบด้วยเมืองกว่า 1,000,000 เมืองที่ถูก generate ไว้ล่วงหน้า ผู้เล่นใช้ Premium Currency ซื้อเมืองที่ยังไม่มีเจ้าของได้โดยตรง ไม่จำกัดจำนวนเมืองและไม่ต้องอยู่ติดกัน เมื่อเป็นเจ้าของเมืองแล้ว ผู้เล่นสามารถประกาศสงครามต่อเมืองของผู้เล่นคนอื่น และแก้ปัญหาข้อพิพาทด้วยการต่อสู้แบบ MOBA ตั้งแต่ 1v1 ถึง 25v25 ผู้แพ้ไม่สูญเสียความเป็นเจ้าของ แต่เมืองของตนจะกลายเป็น "เมืองขึ้น" ภายใต้ผู้ชนะ ก่อให้เกิด hierarchy ของอำนาจที่ขยายและเปลี่ยนแปลงตลอดเวลา

## Player Fantasy

ผู้เล่นรู้สึกเหมือน **ผู้ปกครองที่กำลังสร้างอาณาจักร** — เริ่มจากเมืองเล็กๆ เมืองเดียว ค่อยๆ ขยายอิทธิพลข้ามแผนที่โลกที่มีผู้เล่นอยู่จริงนับล้านคน การประกาศสงครามไม่ใช่แค่ตัวเลขใน ranking — มันคือการส่งกองทัพออกรบในนามของดินแดน ชัยชนะในสนาม MOBA สะท้อนกลับมาเป็นอำนาจบน World Map ส่วนความพ่ายแพ้ไม่ได้หมายความว่าจบ — มันเป็นจุดเริ่มต้นของการวางแผนแก้แค้น หรือสร้างพันธมิตรเพื่อต่อกรกับผู้พิชิต

## Detailed Design

### Core Rules

#### 3.1 World Map

1. World Map ประกอบด้วยเมืองที่ถูก generate ล่วงหน้า **1,000,000 เมือง** แสดงผลเป็น 2D map
2. เมืองทุกเมืองเริ่มต้นที่ **City Level 1** และ Level เพิ่มขึ้นได้จากการ **อัพเกรด** โดยเจ้าของ
3. แต่ละเมืองมีสถานะ: **Unclaimed / Owned / At War / Vassal**
4. ผู้เล่นสามารถเลื่อนดู World Map และ inspect เมืองใดก็ได้

#### 3.2 City Ownership

1. ผู้เล่นซื้อเมืองที่มีสถานะ Unclaimed ได้ทันทีโดยใช้ **Premium Currency**
2. ราคาเมือง — ⚠️ Open Question
3. ผู้เล่นเป็นเจ้าของได้ไม่จำกัดจำนวน ไม่ต้องอยู่ติดกัน
4. เจ้าของเมืองสามารถ: **อัพเกรดเมือง** (ทรัพยากร ⚠️ TBD), ประกาศสงคราม, ตั้งชื่อเมือง, ปลด vassal, ขายเมืองคืน (ราคา TBD)

#### 3.3 War System

1. เจ้าของเมืองประกาศสงครามโจมตีเมือง Owned หรือ Vassal ของผู้เล่นอื่นได้
2. **Battle Scale ตาม City Level ของเมืองที่ถูกโจมตี**:

   | City Level | Battle Scale |
   |-----------|-------------|
   | 1 | 1v1 |
   | 2 | 3v3 |
   | 3 | 5v5 |
   | 4 | 25v25 |

3. **War Window**: เจ้าของเมืองกำหนดช่วงเวลาสะดวกในแต่ละวันล่วงหน้า ผู้โจมตีเลือกเวลาโจมตีภายใน War Window นั้น ทั้งสองฝ่ายทราบล่วงหน้า
4. เมื่อถึงเวลาสงคราม ช่องผู้เล่นที่ขาดจะถูกเติมด้วย **Bot** (FT5) อัตโนมัติ
   - หากฝ่ายโจมตีไม่มีผู้เล่นจริง**แม้แต่ 1 คน** → ฝ่ายป้องกันชนะทันที แม้ฝ่ายป้องกันจะไม่มีคนเช่นกัน
5. สงครามต่อสู้บนแผนที่ MOBA มาตรฐาน (1v1 ใช้ map พิเศษ, 25v25 ⚠️ TBD)
6. **ชนะ**: ทำลาย Core ของฝ่ายตรงข้าม
7. เมืองที่มีสถานะ At War ไม่สามารถถูกประกาศสงครามซ้ำได้

#### 3.4 Vassal System

1. เมืองที่แพ้สงครามกลายเป็น **Vassal** ของเมืองผู้ชนะ
2. เจ้าของยังเป็นเจ้าของเมืองนั้น แต่มีสถานะ Vassal แสดงบน map
3. **การหลุดพ้น**: (ก) ประกาศสงครามกลับ — ชนะ = อิสระ / แพ้ = vassal ต่อ　(ข) เจ้าผู้ปกครองปลดให้
4. ระบบ tribute — ⚠️ Open Question

### States and Transitions

#### สถานะเมือง (City State)

| State | เงื่อนไขเข้า | เงื่อนไขออก | พฤติกรรม |
|-------|-------------|------------|---------|
| **Unclaimed** | เริ่มต้น (generate) | ถูกซื้อ | ซื้อได้ทันที |
| **Owned** | ซื้อสำเร็จ / ชนะสงคราม (vassal หลุดพ้น) | ถูกประกาศสงคราม / กลายเป็น vassal | เจ้าของบริหารได้เต็ม |
| **At War** | ถูกประกาศสงครามและ war time ถูกกำหนดแล้ว | สงครามจบ (ชนะหรือแพ้) | ถูกโจมตีซ้ำไม่ได้ |
| **Vassal** | แพ้สงคราม | ชนะสงครามกลับ / ถูกปลด | แสดงชื่อผู้ปกครองบน map |

#### สถานะสงคราม (War State)

| State | เงื่อนไขเข้า | เงื่อนไขออก | พฤติกรรม |
|-------|-------------|------------|---------|
| **Declared** | ฝ่ายโจมตีประกาศสงคราม | เลือกเวลาใน War Window สำเร็จ | รอทั้งสองฝ่ายเลือกเวลา |
| **Scheduled** | เวลาสงครามถูกยืนยัน | ถึงเวลาสงคราม | ทั้งสองฝ่ายเตรียมทีม |
| **In Battle** | ถึงเวลา + ฝ่ายโจมตีมีคนอย่างน้อย 1 คน | MOBA จบ (Core ถูกทำลาย) | Bot เติมช่องที่ขาด |
| **Resolved** | MOBA จบ หรือ ฝ่ายโจมตีไม่มีคนเลย | — (terminal state) | อัพเดตสถานะเมือง |

### Interactions with Other Systems

| ระบบ | ทิศทาง | ข้อมูลที่ไหล |
|------|--------|------------|
| **Game Mode Manager (FT7)** | Territory War → FT7 | ส่ง battle request พร้อม scale (1v1/3v3/5v5/25v25) และ map type |
| **Matchmaking (FT6)** | Territory War → FT6 | สร้าง private room ให้ทั้งสองฝ่าย (ไม่ใช่ public matchmaking) |
| **AI Bot System (FT5)** | Territory War → FT5 | ขอ bot เติมช่องที่ขาด พร้อม difficulty TBD |
| **Account & Auth (M6)** | Territory War → M6 | อ่าน Player ID สำหรับ city ownership, war history |
| **Notification System (M4)** | Territory War → M4 | แจ้งเตือนเมื่อถูกประกาศสงคราม, ใกล้ถึงเวลาสงคราม |
| **Social System (M1)** | Territory War → M1 | เชิญเพื่อนร่วมทีมสงคราม (สำหรับ 3v3/5v5/25v25) |
| **Data/Config System (F4)** | F4 → Territory War | โหลดค่า city price, upgrade cost, war window config |
| **Combat & All Core Systems** | ใช้ตรงๆ | สงครามใช้ระบบ MOBA ปกติทั้งหมด — ไม่มี interface พิเศษ |

## Formulas

### Battle Scale Formula

```
Battle Scale = f(City Level ของเมืองที่ถูกโจมตี)
```

| City Level | Battle Scale | ผู้เล่นต่อทีม |
|-----------|-------------|------------|
| 1 | 1v1 | 1 |
| 2 | 3v3 | 3 |
| 3 | 5v5 | 5 |
| 4 | 25v25 | 25 |

### สูตรที่ยังไม่ได้กำหนด (Open Questions)

| สูตร | Candidate Options | สถานะ |
|------|------------------|-------|
| ราคาซื้อเมือง (Unclaimed) | — | ⚠️ TBD |
| กลไก City Level Upgrade | (ก) จ่ายทรัพยากรครั้งเดียว = +1 Level　(ข) สะสม City XP จากชัยชนะในสงคราม　(ค) ทั้งสองอย่าง — จ่ายทรัพยากร + ต้องชนะสงครามก่อนอัพได้ | ⚠️ TBD |
| ราคาอัพเกรดต่อ Level | — | ⚠️ TBD |
| ระบบ Tribute ของ Vassal | (ก) จ่าย Premium Currency ทุก X วัน　(ข) จ่าย % ของรายได้ที่เมืองสร้างได้　(ค) ไม่ต้องจ่าย — แพ้มีผลเชิง status เท่านั้น | ⚠️ TBD |
| ราคาขายเมืองคืน | — | ⚠️ TBD |

## Edge Cases

| สถานการณ์ | พฤติกรรม | เหตุผล |
|-----------|---------|--------|
| เมือง Vassal ถูกโจมตีโดยบุคคลที่ 3 | สงครามเกิดขึ้นได้ตามปกติ เจ้าผู้ปกครองไม่มีสิทธิ์แทรกแซง | Vassal ยังเป็นเจ้าของเมืองนั้น รับผิดชอบป้องกันเอง |
| เจ้าผู้ปกครองถูกยึดเมืองหลักโดยคนอื่น | เมืองขึ้นทั้งหมดของเมืองนั้นหลุดพ้นเป็นอิสระทันที | ผู้ปกครองที่ไม่มีเมืองหลักไม่สามารถเป็นเจ้าผู้ปกครองได้ |
| ผู้โจมตีประกาศสงคราม แต่ War Window ของเป้าหมายไม่มีช่วงที่ตัวเองสะดวก | ไม่สามารถประกาศสงครามได้ จนกว่าจะมี War Window ที่ตรงกัน | ทั้งสองฝ่ายต้องสะดวก |
| เจ้าของเมืองไม่ได้ตั้ง War Window | เมืองนั้นไม่สามารถถูกโจมตีได้ — แต่ก็ไม่สามารถประกาศสงครามโจมตีคนอื่นได้เช่นกัน | สิทธิ์สงครามต้องการ War Window สองทาง |
| ฝ่ายโจมตีส่งคนมา 1 คน (จาก 25) ใน 25v25 | Bot เติม 24 ช่องที่เหลือ สงครามดำเนินต่อ | กฎ minimum 1 คนครบ |
| ฝ่ายป้องกัน Level 4 ไม่มีคนมาเลย ฝ่ายโจมตีมา 1 คน | Bot เติมฝ่ายป้องกัน 25 ช่อง สงครามดำเนินต่อ | ฝ่ายโจมตีมีคน = สงครามเกิด |
| ผู้เล่นไม่มีเมืองเลย | ไม่สามารถประกาศสงครามได้ | สงครามผูกกับ city ownership |
| เมืองถูกประกาศสงครามขณะกำลังอัพเกรด | การอัพเกรดหยุดชั่วคราว / ยกเลิก — ⚠️ TBD | ต้องกำหนดในช่วง balance |

## Dependencies

| ระบบ | ทิศทาง | ประเภท | Interface |
|------|--------|--------|----------|
| **Game Mode Manager (FT7)** | Territory War → FT7 | Hard | ส่ง `TerritoryWarBattleRequest` พร้อม scale, map type, team rosters |
| **Matchmaking (FT6)** | Territory War → FT6 | Hard | สร้าง private room โดยไม่ผ่าน public queue |
| **AI Bot System (FT5)** | Territory War → FT5 | Hard | ขอ bot จำนวน N ตัวเติมฝ่ายที่ขาด |
| **Account & Auth (M6)** | Territory War → M6 | Hard | อ่าน/เขียน city ownership, war history ต่อ account |
| **Notification System (M4)** | Territory War → M4 | Hard | ส่ง event: ถูกประกาศสงคราม, สงครามใกล้เริ่ม, ผลสงคราม |
| **Social System (M1)** | Territory War → M1 | Hard | เชิญ **พลเมือง** (ผู้เล่นที่สมัครอยู่ในเมือง) ออกรบผ่าน notification/invite |
| **Citizen System (FT10a)** | Sub-system ภายใน | Hard | จัดการการสมัคร/ออกจากเมือง, รายชื่อพลเมืองต่อเมือง |
| **Mercenary System (FT10b)** | Sub-system ภายใน | Soft | ตลาดจ้างทหารรับจ้าง — ผู้เล่นลงทะเบียนรับจ้าง, เจ้าเมืองจ้างมาเสริมทีม |
| **Data/Config System (F4)** | F4 → Territory War | Hard | โหลด config: war window limits, bot difficulty, city price (TBD) |
| **Statistics & History (M5)** | Territory War → M5 | Soft | บันทึก war history, win/loss record ต่อเมือง |

## Tuning Knobs

| Parameter | ค่าปัจจุบัน | Safe Range | เพิ่มมากเกิน | ลดมากเกิน |
|-----------|------------|------------|-------------|----------|
| จำนวนเมืองบน World Map | 1,000,000 | 100,000–∞ | Server/DB load สูง | Map ดูโล่ง, ขาดความรู้สึก scale |
| City Level สูงสุด | 4 | 3–6 | 25v25 ขึ้นไปยาก balance | ไม่มี end-game content |
| War Window ขั้นต่ำต่อวัน (ชั่วโมง) | ⚠️ TBD | 1–4 ชม. | ป้องกันยาก (ถูกโจมตีตลอด) | โจมตีได้ยากมาก |
| War Window สูงสุดต่อวัน (ชั่วโมง) | ⚠️ TBD | 4–12 ชม. | เหมือนไม่มี window | — |
| เวลารอหลังประกาศสงคราม (Cool-down) | ⚠️ TBD | 1–24 ชม. | โจมตีซ้ำไม่ได้นาน | spam สงครามได้ |
| Bot Difficulty ใน Territory War | ⚠️ TBD | Easy–Hard | ฝ่ายที่มีคนน้อยได้เปรียบมาก | Bot ไม่มีความหมาย |
| จำนวนพลเมืองสูงสุดต่อเมือง (FT10a) | ⚠️ TBD | 10–500 | ยากบริหาร | ทีมเล็กเกินไป |

## Visual/Audio Requirements

[To be designed]

## UI Requirements

[To be designed]

## Acceptance Criteria

- [ ] ผู้เล่นซื้อเมือง Unclaimed ด้วย Premium Currency ได้ และเมืองเปลี่ยนสถานะเป็น Owned ทันที
- [ ] เจ้าเมืองตั้ง War Window ได้ และผู้โจมตีเห็น War Window ของเมืองเป้าหมาย
- [ ] ผู้โจมตีไม่สามารถประกาศสงครามนอก War Window ของเป้าหมายได้
- [ ] Battle Scale ถูกกำหนดจาก City Level ของเมืองที่ถูกโจมตีตรงตาม table (1→1v1, 2→3v3, 3→5v5, 4→25v25)
- [ ] เมื่อถึงเวลาสงคราม Bot เติมช่องที่ขาดโดยอัตโนมัติ
- [ ] หากฝ่ายโจมตีไม่มีผู้เล่นจริงแม้แต่ 1 คน → ฝ่ายป้องกันชนะทันทีโดยไม่ต้องเล่น
- [ ] เมืองที่แพ้สงครามเปลี่ยนสถานะเป็น Vassal และแสดงชื่อผู้ปกครองบน map
- [ ] เมือง Vassal ที่ชนะสงครามกลับหลุดพ้นเป็น Owned ทันที
- [ ] เจ้าผู้ปกครองปลด Vassal ได้ทุกเมื่อ → เมืองกลับเป็น Owned
- [ ] เมืองที่มีสถานะ At War ไม่สามารถถูกประกาศสงครามซ้ำได้
- [ ] เมืองที่เจ้าผู้ปกครองแพ้สงคราม → Vassal ทั้งหมดหลุดพ้นเป็นอิสระทันที
- [ ] เจ้าเมืองที่ไม่ได้ตั้ง War Window ไม่สามารถประกาศสงครามโจมตีคนอื่นได้
- [ ] World Map แสดงเมือง 1,000,000 เมืองได้โดยไม่มี performance drop ที่ client
- [ ] Network Sync: สถานะเมืองทุกเมือง (Owned/At War/Vassal) ถูก sync ถูกต้องทุก client

## Open Questions

| คำถาม | Candidate Options | Owner | สถานะ |
|-------|------------------|-------|-------|
| ชื่อ Premium Currency คืออะไร? | — | Designer | ⚠️ TBD |
| ราคาซื้อเมือง (Unclaimed) เท่าไหร่? | — | Economy Designer | ⚠️ TBD |
| กลไก City Level Upgrade ทำงานอย่างไร? | (ก) จ่ายทรัพยากรครั้งเดียว (ข) สะสม City XP จากสงคราม (ค) ทั้งสองอย่าง | Designer | ⚠️ TBD |
| ทรัพยากรสำหรับอัพเกรดเมืองคืออะไร? | Premium Currency / In-game Gold / ทั้งสองอย่าง | Designer | ⚠️ TBD |
| ราคาอัพเกรดต่อ Level เท่าไหร่? | — | Economy Designer | ⚠️ TBD |
| ระบบ Tribute ของ Vassal ทำงานอย่างไร? | (ก) จ่าย Premium Currency ทุก X วัน (ข) จ่าย % รายได้ (ค) ไม่ต้องจ่าย | Designer | ⚠️ TBD |
| ราคาขายเมืองคืนเท่าไหร่? | — | Economy Designer | ⚠️ TBD |
| War Window ขั้นต่ำ/สูงสุดต่อวันเท่าไหร่? | — | Designer | ⚠️ TBD |
| War declaration cool-down นานแค่ไหน? | — | Designer | ⚠️ TBD |
| Bot Difficulty ใน Territory War ระดับไหน? | — | Designer | ⚠️ TBD |
| จำนวนพลเมืองสูงสุดต่อเมือง? | — | Designer (FT10a) | ⚠️ TBD |
| เมืองถูกโจมตีขณะอัพเกรด — หยุดหรือยกเลิก? | — | Designer | ⚠️ TBD |
| 25v25 map design และ performance budget? | — | Technical Director | ⚠️ TBD |
