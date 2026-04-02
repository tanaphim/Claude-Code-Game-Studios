# Dungeon Mode — Game Design Document

**System ID**: FT8
**Version**: 0.1.0
**Status**: Draft — Reverse-documented from source
**Last Updated**: 2026-04-02
**Dependencies**: Game Mode Manager (FT7), AI/Bot System (FT5), Combat & Skills System (C1), Map & Objectives (FT2)

---

## 1. Overview

Dungeon Mode เป็น PvE Co-op สำหรับผู้เล่น 1–4 คน ผจญภัยผ่าน Stage แบบ Wave-based ต่อสู้กับ Monster และ Boss ที่มี AI แบบ Pattern-based ความแตกต่างหลักจาก MOBA คือผู้เล่นเลือก Equipment Loadout ก่อนเข้าด่าน และชนะได้ด้วยการกวาดล้าง Monster ทุกตัวในทุก Stage

---

## 2. Player Fantasy

ผู้เล่นรู้สึกถึงการเดินทางผจญภัยร่วมกับเพื่อน — เอาชีวิตรอดจากคลื่น Monster แล้วโค่น Boss ขนาดใหญ่ที่มีรูปแบบการโจมตีพิเศษ ความรู้สึกสะสมพลังจาก Equipment ช่วยให้เนื้อหา Dungeon ต่างจาก MOBA ที่ต้องสร้างพลังใหม่ทุกแมตช์

---

## 3. Detailed Rules

### 3.1 พื้นฐาน

| ค่า | รายละเอียด |
|-----|-----------|
| ผู้เล่นสูงสุด | 4 คน (Co-op) |
| ทีม | Team1 (ผู้เล่น) vs Team2 (Monster) |
| แผนที่ | `scene_dungeon_map.unity` |
| ระดับความยาก | Easy / Medium / Hard |

### 3.2 Stage Structure

- แต่ละ Dungeon มีหลาย Stage เรียงต่อกัน
- Stage มี 2 รูปแบบ:
  - **Wave-based**: Monster สปอนเป็น Wave ทีละคลื่น
  - **Single-Spawn**: Monster ทั้งหมดสปอนพร้อมกัน
- **ชนะ Stage**: ฆ่า Monster ทุกตัว (`RemainingMonsters == 0`)
- **ชนะ Dungeon**: ผ่านทุก Stage สำเร็จ → แสดง "DUNGEON CLEAR!"

### 3.3 Monster Types

ใน Dungeon สามารถสปอน:
- MeleeCreep, RangeCreep, SuperCreep, SeigeCreep
- JungleCreep
- **Boss** (Main Stage Boss)
- **MiniBoss** (Sub-encounter)

Monster แต่ละตัวมี: จำนวน (Count), Spawn Chance, HP Multiplier, Spawn Delay

### 3.4 Boss AI

Boss ใช้ **Pattern-based AI** เหมือนใน MOBA Map (ดู Creep & Minion System FT3):
- เลือก Pattern แบบสุ่มหรือ Sequential
- Patterns: [Attack, Skill], [Attack, Attack, Skill], [Skill, Attack], [Attack, Skill, Attack]
- มี Post-cast Idle, Attack Idle, Charge Time

### 3.5 Equipment Loadout

ผู้เล่นเลือก Equipment ก่อนเข้าด่านใน Hero Select Phase:

**6 Slot Equipment** (TownEquipmentType):

| Slot | ประเภท |
|------|--------|
| 1 | Weapon |
| 2 | Helmet |
| 3 | Body Armor |
| 4 | Pants |
| 5 | Gloves |
| 6 | Boots |

**Stats ที่ Equipment มอบได้**: attack_damage, ability_power, attack_speed, move_speed, max_hp, max_mp, hp_regen, mp_regen, armor, magic_resist, life_steal, critical_strike_chance, cooldown_reduction ฯลฯ

**การใช้งาน**: Stats ถูก Apply ผ่าน `Actor.Trait.ApplyStat(stat)` เมื่อเริ่ม Match

### 3.6 ความแตกต่างจาก MOBA

| ด้าน | Dungeon | MOBA |
|------|---------|------|
| เป้าหมาย | กวาดล้าง Monster | ทำลาย Core ศัตรู |
| ศัตรู | AI Monster + Boss | ผู้เล่น + Minion |
| Mode | Co-op PvE | Competitive PvP |
| Equipment | เลือกก่อนเกม (Persistent) | ซื้อระหว่างเกม |
| ผู้เล่น | 1–4 คน | 5v5 |

---

## 4. Formulas

### Stage Clear
```
StageClear = (RemainingMonsters == 0)
DungeonClear = (AllStagesCleared == true)
```

### Equipment Stat Application
```
เหมือน Item System — Flat/Percent Modifier ผ่าน Actor.Trait.ApplyStat()
```

---

## 5. Edge Cases

| สถานการณ์ | พฤติกรรม |
|-----------|---------|
| ผู้เล่น 1 คน Disconnect | 3 คนที่เหลือเล่นต่อได้ |
| ผู้เล่นทุกคนตาย | Dungeon Fail (ต้องตรวจสอบ Implementation) |
| Boss หลุด Leash | กลับ Spawn Point (8 หน่วย) |
| Wave ยังไม่หมด → Timer หมด | ⚠️ ยังไม่พบ Time Limit Mechanic |

---

## 6. Dependencies

| ระบบ | ความสัมพันธ์ |
|------|------------|
| **Game Mode Manager (FT7)** | NetworkDungeonMode เป็น Subclass ของ NetworkGameModeBase |
| **AI/Bot System (FT5)** | Monster AI ใช้ FuzzySkillAI เดียวกับ Bot |
| **Combat & Skills System (C1)** | ความเสียหาย Hero กับ Monster ผ่าน Damage Pipeline เดิม |
| **Data-Config System (F3)** | CBSStage กำหนด Stage Config, Monster Stats |
| **Hero System (C2)** | ผู้เล่นเล่นเป็น Hero เหมือน MOBA |

---

## 7. Tuning Knobs

| ค่า | CBS Key | ผลกระทบ |
|-----|---------|---------|
| Monster Count/Wave | CBSStage.MonsterSpawn | ความหนาแน่น |
| Monster HP Multiplier | CBSStage.HPMultiplier | ความทนทาน |
| Difficulty Scale | CBSStage.Difficulty | Easy/Med/Hard |
| Boss Pattern | ActorBoss.m_RandomizePattern | สุ่ม vs Sequential |
| Max Players | NetworkDungeonMode | 4 คน (Hardcode) |

---

## 8. Acceptance Criteria

- [ ] ผู้เล่น 1–4 คนเข้า Dungeon พร้อมกันได้
- [ ] Equipment Loadout ถูก Apply ก่อนเกมเริ่ม
- [ ] Wave สปอน Monster ตาม CBSStage Config
- [ ] ฆ่า Monster ครบ Stage → ขึ้น Stage ถัดไป
- [ ] ผ่านทุก Stage → "DUNGEON CLEAR!" + แสดง Summary
- [ ] Boss ใช้ Pattern AI ไม่ใช่ Auto-Attack ธรรมดา
- [ ] Easy / Medium / Hard ให้ Monster แตกต่างกันชัดเจน

---

## Known Issues / TODO

- ⚠️ **Dungeon Fail Condition**: ยังไม่พบ Code ที่ Handle ผู้เล่นทุกคนตาย → ต้องตรวจสอบ
- ⚠️ **Time Limit**: ไม่พบ Stage Time Limit — Dungeon อาจไม่มี Timeout
- ⚠️ **Reward System**: ระบบ Reward หลัง Dungeon ยังไม่ได้ Document ตัวเลข
- ⚠️ **Equipment Persistence**: Equipment ในฐาน Town ต่างจาก Item Shop ใน MOBA — ต้องแยก Flow ให้ชัด
