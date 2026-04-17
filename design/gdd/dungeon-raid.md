# Dungeon Raid — Game Design Document

**System ID**: FT8-B
**Version**: 0.1.0
**Status**: Draft
**Last Updated**: 2026-04-06
**Dependencies**: Game Mode Manager (FT7), AI/Bot System (FT5), Combat & Skills System (C1), Map & Objectives (FT2), Gold Economy (EC1), Territory War (TW1)

---

## 1. Overview

Dungeon Raid เป็นโหมด PvE สำหรับทีม 5 คน ที่ใช้แผนที่ MOBA เดิมแต่ฝั่งศัตรูเป็น Monster AI ทั้งหมด แทนที่จะเป็นผู้เล่น Monster มีพฤติกรรมดัน lane, ถอย, และรุมตีเหมือน Hero จริงๆ เป้าหมายคือทำลาย Monster Core กลางแผนที่ ชนะแล้วได้ทรัพยากรสำหรับโหมด Territory War และ Currency กลางที่ใช้ได้ทั้งเกม โหมดนี้ออกแบบมาเพื่อให้ผู้เล่นที่ไม่ต้องการเข้า Ranked ยังมี content ที่มีความหมายและให้ผลตอบแทนอย่างต่อเนื่อง

## 2. Player Fantasy

ผู้เล่นรู้สึกเหมือน **นักรบที่บุกโจมตีป้อมปราการของสัตว์ประหลาด** — ฝ่าแนว Monster ที่ฉลาดพอจะถอยและรุม, รื้อ Tower ที่ปกป้องด้วยยักษ์ใหญ่, แล้วโค่น Core Boss ที่อยู่ด้านใน ความรู้สึกต่างจาก Ranked ตรงที่ทีมทุกคนมุ่งไปทิศทางเดียวกัน ไม่มีความกดดันจาก Rank Point — แต่ชัยชนะมีน้ำหนักเพราะ Resource ที่ได้ไปขับเคลื่อน Territory War จริงๆ

## 3. Detailed Rules

### 3.1 Match Basics

| ค่า | รายละเอียด |
|-----|-----------|
| ผู้เล่น | 5 คน (Team ผู้เล่น) |
| ศัตรู | Monster AI (ไม่มี Hero ฝั่งศัตรู) |
| แผนที่ | แผนที่ MOBA เดิม (3 Lane + Jungle) |
| เวลาเฉลี่ย | 20–30 นาทีต่อ match |
| Difficulty | Auto-scale ตามจำนวนผู้เล่นและ level ฮีโร่ |
| ระบบ Gold / Item | เหมือน MOBA ปกติ (ซื้อ Item ระหว่าง match) |
### 3.2 Monster System

Monster ฝั่งศัตรูมี 4 ประเภท แต่ละประเภทมีบทบาทต่างกันในแผนที่:

| ประเภท | บทบาท | สกิล | ตำแหน่ง |
|--------|--------|------|---------|
| **Lane Walker** | ดัน lane เหมือน Hero ทั่วไป มี Minion เป็นแนวหน้า | Auto-attack เท่านั้น | Lane ทั้ง 3 |
| **Elite Lane Walker** | Lane Walker ที่แข็งแกร่งกว่า มีการตัดสินใจฉลาดขึ้น | มีสกิลพิเศษ 1–2 อย่าง (เช่น Stun, AoE) | Lane ทั้ง 3 (สุ่ม spawn) |
| **Tower Guardian** | ยึดติดอยู่กับ Tower เสริมพลัง Tower และโจมตีผู้เล่นที่เข้าใกล้ | มี Buff aura ให้ Tower | หน้า Tower ทุกตัว |
| **Raid Boss** | ปกป้อง Monster Core มี Pattern AI ซับซ้อน ต้องอ่าน pattern ให้ได้ | Pattern-based (เหมือน Boss ใน Dungeon Mode เดิม) | Monster Core |

**Monster Minion Wave**: สปอนจาก Monster Base ตามเลน เหมือน Minion ใน MOBA แต่มี skin/model เป็น Monster
### 3.3 Monster AI Behavior

Lane Walker และ Elite Lane Walker มี Decision Logic ดังนี้:

**สถานะหลัก (States)**

| State | เงื่อนไขเข้า | พฤติกรรม |
|-------|-------------|----------|
| **Push** | HP > 50% และไม่มีศัตรูมากกว่า 2 คนในระยะ | เดินหน้าตาม lane โจมตี Minion / Tower / ผู้เล่น |
| **Fight** | มีผู้เล่นเข้ามาในระยะ | หยุดดัน หันมาสู้กับผู้เล่น |
| **Retreat** | HP < 30% หรือ ผู้เล่นมากกว่า Monster ในพื้นที่ | ถอยกลับฐาน รอ HP ฟื้น |
| **Group** | ได้ยิน Signal จาก Elite Walker ใกล้เคียง | รวมกลุ่มกับ Monster อื่นก่อนรุก |

**Elite Lane Walker เพิ่มเติม**
- สามารถส่ง "Group Signal" ให้ Monster ใกล้เคียงรวมกลุ่มได้
- เลือกใช้สกิลตาม HP ของผู้เล่น (เช่น ใช้ Stun เมื่อผู้เล่น HP ต่ำ)
- ถ้า Tower ถูกโจมตี จะ Retreat กลับมาช่วย Tower ก่อน

**Raid Boss**
- ใช้ Pattern AI เหมือนระบบ Boss เดิม (Sequential / Random Pattern)
- Enrage เมื่อ HP < 25%: เพิ่ม attack speed และเปลี่ยน pattern
### 3.4 Win / Lose Conditions

**ชนะ**
- ทำลาย Monster Core ได้สำเร็จ → แสดง "DUNGEON RAID CLEAR!" + Summary หน้าจอ

**แพ้**
- Monster ทำลาย Nexus ของผู้เล่นได้สำเร็จ

**Death & Respawn**
- เมื่อ Hero ตาย → รอ Respawn timer แล้วกลับมาสู้ได้ (เหมือน MOBA ปกติ)
- ทุกครั้งที่ตาย → เสีย Gold ในเกม (in-match gold) บางส่วน — กระทบความสามารถในการออก Item แต่ไม่กระทบ Resource ที่ได้หลัง match
- Respawn timer ยาวขึ้นตาม level ฮีโร่

**ไม่มี Time Limit** — match จบเมื่อมีฝ่ายแพ้เท่านั้น
### 3.6 Reward System

Resource ที่ได้จาก Dungeon Raid แบ่งเป็น 2 ส่วน:

**1. In-match Gold** (ใช้ภายใน match เท่านั้น)
- ได้จาก: ฆ่า Monster Minion, ฆ่า Lane Walker / Elite Walker, ทำลาย Tower
- ใช้: ซื้อ Item ใน Shop ระหว่าง match
- หายไปเมื่อ match จบ

**2. Post-match Resource** (เก็บถาวร ใช้นอก match)

| เหตุการณ์ | Resource ที่ได้ |
|-----------|----------------|
| ฆ่า Lane Walker | Dungeon Token (เล็กน้อย) |
| ฆ่า Elite Lane Walker | Dungeon Token (ปานกลาง) |
| ทำลาย Tower | Dungeon Token (Bonus) + Global Currency |
| ทำลาย Monster Core (ชนะ) | Dungeon Token (ใหญ่) + Global Currency |

- **Dungeon Token** → ใช้ใน Territory War
- **Global Currency** → ใช้ได้ทุกโหมดในเกม (ซื้อ Cosmetic ฯลฯ)
- Resource ได้เต็มจำนวนโดยไม่ขึ้นกับการตายระหว่าง match

## 4. Formulas

### Monster Auto-scale
```
MonsterHP = BaseHP × DifficultyMultiplier × PlayerCountFactor
MonsterDamage = BaseDamage × DifficultyMultiplier × PlayerCountFactor

DifficultyMultiplier = 1 + (AvgHeroLevel × 0.05)
  ตัวอย่าง: ฮีโร่ level 10 เฉลี่ย → ×1.50

PlayerCountFactor = 0.6 + (PlayerCount × 0.08)
  ตัวอย่าง: 5 คน → ×1.00 | 3 คน → ×0.84 | 1 คน → ×0.68
```

### Gold Penalty เมื่อตาย
```
GoldLost = CurrentGold × DeathPenaltyRate
DeathPenaltyRate = 0.10 (10% ของ Gold ที่ถืออยู่)
  ตัวอย่าง: มี 500 Gold → เสีย 50 Gold
```

### Post-match Resource
```
TotalDungeonToken = KillToken + TowerToken + ClearBonus

KillToken       = (LaneWalkerKills × 2) + (EliteKills × 8)
TowerToken      = TowersDestroyed × 20
ClearBonus      = 100 (ถ้าชนะ) หรือ 0 (ถ้าแพ้)

GlobalCurrency  = (TowersDestroyed × 5) + (ClearBonus > 0 ? 30 : 0)
```

*ตัวเลขทั้งหมดเป็น baseline — ปรับได้ผ่าน Tuning Knobs (Section 7)*

## 5. Edge Cases

| สถานการณ์ | พฤติกรรม |
|-----------|---------|
| ผู้เล่น 1 คน Disconnect | 4 คนที่เหลือเล่นต่อได้ — Monster auto-scale ปรับลดตาม PlayerCount ใหม่ |
| ผู้เล่นทุกคนตายพร้อมกัน | แต่ละคนรอ Respawn timer ของตัวเอง — match ยังดำเนินต่อ Monster อาจ Push เข้า Nexus ระหว่างนี้ |
| Monster Walker ถูก Kite ออกนอก lane | ถอยกลับ lane เดิมถ้าไม่มีผู้เล่นในระยะ (Leash mechanic เหมือน Jungle creep) |
| Elite Walker ส่ง Group Signal แต่ไม่มี Monster ใกล้เคียง | Signal หมดอายุใน 5 วินาที Elite Walker ดำเนินการ Push ต่อตามลำพัง |
| ผู้เล่น 1 คนเข้า match ไม่ทัน (Loading นาน) | รอ Grace Period 60 วินาที ถ้ายังไม่เข้า match เริ่มโดยไม่มีผู้เล่นคนนั้น |
| Raid Boss ถูก Kite ออกจาก Core | Boss Teleport กลับ Core ทันทีถ้าระยะห่างเกิน 12 หน่วย และ Heal 20% HP |

## 6. Dependencies

| ระบบ | ความสัมพันธ์ |
|------|------------|
| **Game Mode Manager (FT7)** | Dungeon Raid เป็น subclass ของ NetworkGameModeBase เหมือน Dungeon Mode เดิม |
| **AI/Bot System (FT5)** | Lane Walker ใช้ FuzzySkillAI เป็น base แต่ต้องเพิ่ม Push/Retreat state ใหม่ |
| **Combat & Skills System (C1)** | Damage pipeline เดิมใช้ได้ทันที ทั้ง Hero → Monster และ Monster → Hero |
| **Map & Objectives (FT2)** | ใช้แผนที่ MOBA เดิม Tower / Barracks / Nexus ยังคงอยู่ แค่เปลี่ยน faction ศัตรู |
| **Gold Economy (EC1)** | In-match Gold ใช้ระบบเดิม Death penalty (GoldLost) ต้องเพิ่ม hook ใหม่ |
| **Hero System (C2)** | ผู้เล่นเลือกและเล่น Hero เหมือน MOBA ทุกอย่าง |
| **Data-Config System (F3)** | Monster stats, Token reward values, ScaleMultiplier ทั้งหมดเก็บใน CBS config |
| **Territory War (TW1)** | รับ Dungeon Token จาก Dungeon Raid — TW1 ต้องอ่าน Token balance จาก backend |

## 7. Tuning Knobs

| ค่า | CBS Key | ค่า Default | ช่วงที่ปลอดภัย | ผลกระทบ |
|-----|---------|------------|----------------|---------|
| Monster Base HP | `DungeonRaid.MonsterBaseHP` | 500 | 300–1000 | ความทนทานโดยรวมของ Monster |
| Monster Base Damage | `DungeonRaid.MonsterBaseDamage` | 80 | 50–150 | ความกดดันต่อผู้เล่น |
| DifficultyMultiplier Rate | `DungeonRaid.DiffMultRate` | 0.05 | 0.02–0.10 | ความเร็วที่ Monster แข็งขึ้นตาม level |
| PlayerCountFactor Base | `DungeonRaid.PlayerCountBase` | 0.6 | 0.4–0.8 | ความยากเมื่อเล่นคนเดียว |
| Death Gold Penalty | `DungeonRaid.DeathPenaltyRate` | 0.10 | 0.05–0.20 | ความหนักของ penalty เมื่อตาย |
| LaneWalker Kill Token | `DungeonRaid.KillTokenWalker` | 2 | 1–5 | อัตรา farm Token จาก Walker |
| Elite Kill Token | `DungeonRaid.KillTokenElite` | 8 | 5–15 | อัตรา farm Token จาก Elite |
| Tower Destroy Token | `DungeonRaid.TowerToken` | 20 | 10–40 | Bonus จากการทำลาย Tower |
| Clear Bonus Token | `DungeonRaid.ClearBonus` | 100 | 50–200 | แรงจูงใจในการชนะ |
| Raid Boss Enrage HP% | `DungeonRaid.BossEnrageHP` | 0.25 | 0.10–0.40 | จุดที่ Boss เข้าสู่ Enrage |
| Boss Leash Range | `DungeonRaid.BossLeashRange` | 12 | 8–20 | ระยะ Kite Boss ได้สูงสุด |

## 8. Acceptance Criteria

- [ ] ทีม 1–5 คนเข้า Dungeon Raid ได้ — Monster auto-scale ตามจำนวนผู้เล่นและ level ฮีโร่
- [ ] Lane Walker เดิน Push lane ได้เอง และถอยเมื่อ HP ต่ำกว่า 30%
- [ ] Elite Lane Walker ใช้สกิลพิเศษได้และส่ง Group Signal ให้ Monster ใกล้เคียง
- [ ] Tower Guardian ปรากฏหน้า Tower และมี Buff aura ให้ Tower
- [ ] ทำลาย Tower ได้ต้องฆ่า Tower Guardian ก่อน
- [ ] Raid Boss ใช้ Pattern AI และ Enrage เมื่อ HP < 25%
- [ ] ทำลาย Monster Core → แสดง "DUNGEON RAID CLEAR!" + Summary
- [ ] Monster ทำลาย Nexus → Match จบ ผู้เล่นแพ้
- [ ] Hero ตาย → รอ Respawn timer แล้วกลับมาได้ เสีย Gold 10% ของที่ถืออยู่
- [ ] Post-match Resource คำนวณถูกต้องตาม Kill / Tower / Clear bonus
- [ ] Dungeon Token ถูก credit เข้า account ผู้เล่นหลัง match จบ
- [ ] Boss ถูก Kite เกิน 12 หน่วย → Teleport กลับ Core และ Heal 20%
