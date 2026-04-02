---
status: reverse-documented
source: C:\GitHub\delta-unity\Assets\GameScripts\Gameplays\Characters\Actors\Component\NetworkTrait.cs
date: 2026-04-02
---

# Level/XP System (C5)

> **Note**: เอกสารนี้ reverse-engineer จาก codebase ที่มีอยู่
> บันทึกพฤติกรรมปัจจุบันและเจตนาการออกแบบที่ได้รับการยืนยัน

## 1. Overview

Level/XP System จัดการการเติบโตของฮีโร่ระหว่างแมตช์ — ฮีโร่เพิ่มเลเวล
จากการสะสม XP, ได้ Skill Points ทุกเลเวล, และสามารถอัปเกรด skill ได้สูงสุดตาม
เลเวลปัจจุบัน ระบบนี้สร้าง power curve ที่กำหนดว่าใครแข็งแกร่งขึ้นเร็วกว่าในแมตช์

## 2. Player Fantasy

ผู้เล่นรู้สึกถึง **การเติบโตที่มีความหมาย** — แต่ละ level up เปิด skill rank ใหม่,
การเลือกว่าจะ max สกิลไหนก่อนเป็นการตัดสินใจเชิงกลยุทธ์, และการที่ทีมมีเลเวลสูงกว่า
ทำให้รู้สึกได้เปรียบอย่างเป็นรูปธรรม

## 3. Detailed Rules

### 3.1 เลเวลและ XP Table

| **MaxLv** | **18** |
|-----------|---------|
| **Initial Level** | จาก CBSConfigBattle.InitialLevel |

**XP ที่ต้องการต่อเลเวล:**

| Level | XP ต้องการ | XP สะสม |
|-------|-----------|---------|
| 1 → 2 | 42 | 42 |
| 2 → 3 | 114 | 156 |
| 3 → 4 | 144 | 300 |
| 4 → 5 | 174 | 474 |
| 5 → 6 | 204 | 678 |
| 6 → 7 | 234 | 912 |
| 7 → 8 | 308 | 1,220 |
| 8 → 9 | 392 | 1,612 |
| 9 → 10 | 486 | 2,098 |
| 10 → 11 | 590 | 2,688 |
| 11 → 12 | 640 | 3,328 |
| 12 → 13 | 690 | 4,018 |
| 13 → 14 | 740 | 4,758 |
| 14 → 15 | 790 | 5,548 |
| 15 → 16 | 840 | 6,388 |
| 16 → 17 | 690 | 7,078 |
| 17 → 18 | 940 | 8,018 |
| Total XP (lv 1→18) | | 8,018 |

> ⚠️ **Bug หรือ Intentional?** Level 16→17 ต้องการ 690 XP น้อยกว่า 15→16 (840 XP)
> ควรตรวจสอบว่าตั้งใจให้มี "XP dip" ที่ lv 16 หรือเป็น typo ในตาราง

### 3.2 Level Up Mechanics

**Automatic** — ระบบ level up อัตโนมัติเมื่อ XP ถึง threshold:

```
SetExp(gainedXP):
  CurrentExp += gainedXP

  while CurrentExp >= MaxExp AND Level < MaxLv:
    CurrentExp -= MaxExp
    Level += 1
    MaxExp = XPTable[Level]
    CurrentSpellPoint += 1
    isLevelup = true

  RpcGainExp(isLevelup, Level, gainedXP)  // broadcast ไปทุก client
```

**ผลของ Level Up:**
- Stats เพิ่มตาม PerLevel formula (ดู Actor System)
- Skill rank cap เพิ่ม
- +1 Skill Point
- Visual + sound effect broadcast

### 3.3 Skill Points

| กฎ | รายละเอียด |
|----|-----------|
| **ได้รับ** | +1 ต่อ level up |
| **เริ่มต้น** | = InitialLevel (มี point เท่ากับเลเวลเริ่ม) |
| **Banking** | ❌ ไม่สามารถเก็บสะสมข้ามเลเวลได้ — ต้องใช้ก่อน level ถัดไป |
| **Respec** | ❌ ไม่สามารถยกเลิกหลังจาก allocate แล้ว |
| **Validation** | Server ตรวจสอบทุก rank-up request |

### 3.4 Skill Rank System

**Max Rank ที่ปลดล็อคตามเลเวล:**

| Skill | Formula | ตัวอย่าง (Level 6) | Max ที่เป็นไปได้ |
|-------|---------|------------------|----------------|
| Q, W, E | `ceil(Level / 2)` | ceil(6/2) = 3 | 5 (lv 10+) |
| R (Ult) | `floor(Level / 6)` | floor(6/6) = 1 | 3 (lv 18) |

**ตาราง Skill Rank ที่ปลดล็อคตามเลเวล:**

| Level | Max Q/W/E | Max R |
|-------|-----------|-------|
| 1–2 | 1 | 0 |
| 3–4 | 2 | 0 |
| 5–6 | 3 | 1 |
| 7–8 | 4 | 1 |
| 9–10 | 5 | 1 |
| 11–12 | 5 | 2 |
| 13–14 | 5 | 2 |
| 15–18 | 5 | 3 |

> **R ล็อคที่ lv 1–4** — ผู้เล่นลงทุน skill points ไปก่อน แล้วจึงได้ Ultimate

**Skill Upgrade Rules:**
- มี `CurrentSpellPoint > 0`
- Skill rank ต่ำกว่า max สำหรับ level ปัจจุบัน
- Skill rank ต่ำกว่า `AbilityData.MaxRank`
- 1 point = +1 rank (ใช้ได้หลาย rank ต่อ level-up ถ้ามี point)

### 3.5 XP Sources

#### ก. Hero Kills Hero

**Base XP จากตาราง (ตาม level เหยื่อ):**

| Level เหยื่อ | Base XP |
|-------------|---------|
| 1 | 42 |
| 2 | 114 |
| 3 | 144 |
| 4 | 174 |
| 5 | 204 |
| 6 | 234 |
| 7 | 308 |
| 8 | 392 |
| 9 | 486 |
| 10 | 590 |
| 11 | 640 |
| 12 | 690 |
| 13 | 740 |
| 14 | 790 |
| 15 | 840 |
| 16 | 690 |
| 17 | 940 |
| 18 | 990 |

**Level Difference Bonus** (เมื่อเหยื่อมีเลเวลสูงกว่า killer):

| Level เหยื่อ − Level killer | XP Bonus |
|---------------------------|---------|
| ≤ 0 | +0% |
| +1 | +16% |
| +2 | +32% |
| +3 | +48% |
| ≥ +4 | +60% |

**XP Split:** หารเท่ากันระหว่าง damagers ทุกคน (ไม่ cap เหมือน gold assist)

#### ข. Minion / Creep XP

**การแบ่ง XP ตามประเภท:**

| ประเภท | แบ่งให้ใคร |
|--------|----------|
| **Boss / Mini-Boss** | ทีมทั้งหมด (ทุกคนเต็มจำนวน) |
| **Tower** | ทีมทั้งหมด |
| **Lane Creep / Jungle Camp / Super Creep** | เฉพาะฮีโร่ที่อยู่ใกล้ (radius 1,600 units) |

**Nearby XP Sharing (เมื่อมีหลายคนในรัศมี):**
```
ถ้า heroesInRadius > 1:
  perHero = floor((baseXP + 26.73) / heroesInRadius)
ถ้า heroesInRadius == 1:
  perHero = baseXP
```

**Jungle Creep XP Bonus:** base XP + ค่า scaling จาก CBSConfigBattle
(CreepJungleIncHp + CreepJungleIncAtk + CreepJungleIncDamage)

#### ค. เมื่อ Objective ฆ่า Hero

เฉพาะ ally ที่อยู่ในรัศมี 1,600 units ได้ XP:
```
baseXP = GetEnemyAvatarExpAtLevel(victimLevel)
ถ้า heroesInRadius > 1:
  perHero = floor((baseXP + 26.73) / heroesInRadius)
```

### 3.6 Monster Level Scaling

Monster เลเวลขึ้นตาม average hero level:

```
monsterLevel = 1 + max(1, averageHeroLevel)
Boss/MiniBoss minimum level = 6
```

### 3.7 Networking

**NetworkVariable keys:**
| Key | ค่า |
|-----|-----|
| `"level"` | เลเวลปัจจุบัน |
| `"max_level"` | เลเวลสูงสุด |
| `"current_exp"` | XP ปัจจุบันในเลเวลนี้ |
| `"max_exp"` | XP ที่ต้องการสำหรับเลเวลถัดไป |
| `"current_spell_point"` | Skill points ที่ยังไม่ได้ใช้ |

**RPCs:**
- `RpcGainExp(isLevelUp, level, exp)` — broadcast level up + visual
- `RpcUpdateSpell(level)` — อัปเดต UI skill rank availability

## 4. Formulas

### 4.1 XP Gain (Hero Kill)

```
baseXP = XPTable[victimLevel]

diff = victimLevel − killerLevel
bonus = {0: 0, 1: 0.16, 2: 0.32, 3: 0.48, ≥4: 0.60}[max(diff, 0)]

totalXP = floor(baseXP × (1 + bonus))
perPlayer = floor(totalXP / damagerCount)
```

| ตัวแปร | คำอธิบาย |
|--------|---------|
| victimLevel | เลเวลของฮีโร่ที่ตาย |
| killerLevel | เลเวลของ killer |
| damagerCount | จำนวนผู้เล่นที่ deal damage (ไม่ cap) |

### 4.2 XP Sharing (หลายคนในรัศมี)

```
perHero = floor((baseXP + 26.73) / heroesInRadius)
// ใช้เมื่อ heroesInRadius > 1
```

| ตัวแปร | คำอธิบาย |
|--------|---------|
| baseXP | XP ของ unit ที่ตาย |
| heroesInRadius | จำนวนฮีโร่ ally ที่มีชีวิตในรัศมี 1,600 units |
| 26.73 | ค่าคงที่สำหรับ balance การแบ่ง XP |

### 4.3 Max Skill Rank

```
maxRank_QWE = clamp(ceil(level / 2), 1, 5)
maxRank_R   = clamp(floor(level / 6), 0, 3)
```

## 5. Edge Cases

| สถานการณ์ | พฤติกรรม |
|-----------|---------|
| Level = MaxLv (18) + ได้ XP | XP หาย, level ไม่เพิ่ม |
| Level up หลายเลเวลจาก XP เดียว | Cascade: loop จนหมด XP หรือถึง MaxLv |
| Skill point + MaxRank ถึงแล้ว | ปุ่ม upgrade ปิด, point ค้างอยู่ |
| Spell point = 0 + กด upgrade | ไม่สามารถ rank up ได้ |
| Bot hero + level up | Auto-upgrade สกิลผ่าน UpdateRankSpell() |
| หลายคน deal damage → damagerCount > 1 | XP หารเท่า ไม่ cap เหมือน gold |
| ตายนอกรัศมีทุกคน | XP หาย (ไม่มีใครได้) |
| Boss/MiniBoss ตาย | ทีมทั้งหมดได้ XP เต็ม ไม่หาร |
| Minion ตายจาก Tower | ไม่มี player เป็น killer → XP กระจายตาม nearby radius |
| Level diff ติดลบ (kill ศัตรูเลเวลต่ำกว่า) | ไม่มี bonus, ได้แค่ base XP |

## 6. Dependencies

| ระบบ | ความสัมพันธ์ |
|------|-------------|
| **Actor System (F1)** | NetworkTrait เก็บ level/XP, stat scaling ตาม level |
| **Data/Config (F4)** | XPTable จาก config, InitialLevel จาก CBSConfigBattle |
| **Networking Core (F2)** | RpcGainExp, RpcUpdateSpell broadcast |
| **Combat & Skills (C1)** | Kill event → trigger XP distribution |
| **Hero System (C2)** | ← skill rank unlock ขึ้นกับ level |
| **Gold Economy (C4)** | parallel system — XP ≠ gold, แยกกัน |
| **Creep/Minion System (FT3)** | ← Minion XP ใช้ nearby sharing |
| **HUD & In-Game UI (P1)** | ← แสดง XP bar, level, skill rank availability |

## 7. Tuning Knobs

| ค่า | ที่เก็บ | ผลกระทบ |
|-----|--------|---------|
| XP Table (42→990) | Config/Code | Power curve ทั้งเกม |
| MaxLv (18) | Config | ระยะเวลาเกม |
| InitialLevel | CBSConfigBattle | ข้าม early game |
| Level diff bonus (16%/lv) | Code constant | Snowball prevention หรือ catch-up |
| Nearby XP radius (1,600) | Code constant | ยิ่งกว้าง → share บ่อยขึ้น → เลเวลใกล้กัน |
| Share constant (26.73) | Code constant | ยิ่งสูง → penalize multi-hero sharing น้อยลง |
| Skill point per level (1) | Code | เพิ่ม = upgrade เร็วขึ้น |
| Max Q/W/E rank (5) | Code | สกิลสูงสุดของ regular skills |
| Max R rank (3) | Code | สกิลสูงสุดของ Ultimate |

## 8. Acceptance Criteria

| # | เกณฑ์ | วิธีทดสอบ |
|---|-------|----------|
| 1 | Level up อัตโนมัติเมื่อ XP ถึง threshold | Unit: SetExp(42) at lv1 → Level = 2 |
| 2 | Cascade level up ทำงาน | Unit: SetExp(1000) at lv1 → Level เพิ่มหลายครั้ง |
| 3 | XP ไม่เพิ่มหลัง MaxLv | Unit: lv18 + XP → Level ยังคง 18 |
| 4 | +1 SpellPoint ต่อ level up | Unit: level up → SpellPoint++ |
| 5 | Skill rank unlock ตาม formula | Unit: lv6 → R max = 1, Q max = 3 |
| 6 | Upgrade block เมื่อ point = 0 | Unit: SpellPoint=0 → upgrade rejected |
| 7 | Upgrade block เมื่อ max rank ถึง | Unit: Q rank=5 → upgrade rejected |
| 8 | XP table ตรงกับค่าที่กำหนด | Unit: ตรวจ MaxExp ทุก level ตรงตาราง |
| 9 | Level diff bonus ถูกต้อง | Unit: kill lv+4 → XP × 1.60 |
| 10 | XP split ระหว่าง damagers | Unit: 3 damagers → perPlayer = floor(total/3) |
| 11 | Nearby XP sharing radius | Integration: 2 heroes ≤ 1600u → แบ่ง XP |
| 12 | Boss XP ไปทีมทั้งหมด | Integration: kill boss → ทีม 5 คนได้ XP ทุกคน |
| 13 | Bot auto-upgrade สกิล | Integration: level up bot → spell rank เพิ่ม |
| 14 | RpcGainExp broadcast ไปทุก client | Integration: level up → ทุก client เห็น level ใหม่ |
| 15 | InitialLevel ถูกต้องตาม config | Integration: spawn hero → level = InitialLevel |

## 9. Known Issues / TODO

| # | ปัญหา | ความสำคัญ |
|---|-------|----------|
| 1 | **XP dip ที่ level 16** (690 XP < level 15's 840 XP) — อาจเป็น typo | กลาง |
| 2 | **Banking spell points** ไม่ได้ — ถ้าไม่กด upgrade ก่อน level ถัดไป point หาย | ควรตรวจสอบ behavior นี้ |
