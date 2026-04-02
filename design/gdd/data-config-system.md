---
status: reverse-documented
source: C:\GitHub\delta-unity\Assets\GameScripts\Datas\, C:\GitHub\delta-unity\Assets\CBS\
date: 2026-04-02
---

# Data/Config System (F4)

> **Note**: เอกสารนี้ reverse-engineer จาก codebase ที่มีอยู่
> บันทึกพฤติกรรมปัจจุบันและเจตนาการออกแบบที่ได้รับการยืนยัน

## 1. Overview

Data/Config System คือระบบที่กำหนดว่าค่าสมดุลและข้อมูลเกมทั้งหมดเก็บที่ไหน
โหลดอย่างไร และเข้าถึงอย่างไรในขณะ runtime ระบบนี้เป็นพื้นฐานที่ทุกระบบพึ่งพา
เพราะทุก stats, ทุก ability, ทุก item ล้วนต้องการข้อมูลจากระบบนี้

## 2. Player Fantasy

ระบบนี้ไม่มี player-facing experience โดยตรง แต่รองรับ **เจตนาของ designer**:
- ปรับ balance ได้โดยไม่ต้อง build เกมใหม่
- เพิ่มฮีโร่/ไอเทม/โหมดใหม่ผ่าน dashboard
- ค่าทุกตัวมีแหล่งที่มาชัดเจน ไม่กระจัดกระจายในโค้ด

## 3. Detailed Rules

### 3.1 สถาปัตยกรรมหลัก (3 ชั้น)

```
ชั้น 1: PlayFab CBS (Source of Truth)
  └─ Hero stats, Ability specs, Item data, Battle config, Creep scaling

ชั้น 2: Unity ScriptableObjects (Editor-baked assets)
  └─ Prefabs, Sprites, Sounds, Game mode config, Visual references

ชั้น 3: Local JSON — Deprecated
  └─ ไม่ได้ใช้งานแล้ว (เดิมเคยใช้สำหรับ level progression, hero catalog)
```

### 3.2 PlayFab CBS Layer

**การโหลดข้อมูล:**

```
Game Start
  ↓
CBSTitleDataModule.GetAllTitleData()
  ↓
ExecuteFunction("GetAllTitleData") → Azure Function
  ↓
JSON (compressed) → JsonPlugin.FromJsonDecompress<CBSTitleData>()
  ↓
TitleDataContainer (Dictionary<string, CBSTitleData>)
  ↓
MetadataService cache (m_CustomDataCached)
  ↓
แต่ละระบบเรียก MetadataService.GetCustomData<T>(objectId)
```

**Data Classes หลัก:**

| Class | ประเภท | ข้อมูล |
|-------|--------|--------|
| `CBSUnit` | TitleCustomData | Hero base stats ทั้งหมด (HP, MP, Armor, ฯลฯ) |
| `CBSAbility` | TitleCustomData | Ability spec (range, cooldown, cost, formula) |
| `CBSItemInGame` | CBSItemCustomData | Item stats, recipe, effects |
| `CBSConfigBattle` | TitleCustomData | Battle timers, economy config, AFK config |
| `CBSConfigJungle` | TitleCustomData | Spawn cooldowns (creep wave, boss, jungle) |
| `CBSCreepAmount` | TitleCustomData | Creep scaling per game minute |
| `CBSLevelSpell` | TitleCustomData | Spell level scaling table |
| `CBSSkin` | CBSItemCustomData | Hero skin visual variants |
| `CBSSkinWeapon` | CBSItemCustomData | Weapon skin visual variants |

**Hierarchy ของ Data Classes:**

```
CBSBaseCustomData
├── TitleCustomData (server-level config)
│   ├── CBSConfigBattle
│   ├── CBSConfigJungle
│   ├── CBSCreepAmount
│   ├── CBSLevelSpell
│   └── CBSUnit
└── CBSItemCustomData (item catalog)
    ├── CBSAbility
    ├── CBSItemInGame
    ├── CBSSkin
    └── CBSSkinWeapon
```

### 3.3 CBSConfigBattle — ค่า Battle Balance หลัก

| หมวด | ค่า | คำอธิบาย |
|------|-----|---------|
| **Respawn** | `CooldownAvatarRespawn` | เวลา respawn ของฮีโร่ |
| **Economy** | `InitialMoney` | ทองเริ่มต้นต่อผู้เล่น |
| | `GoldStartTime` | เวลาเริ่มให้ passive gold |
| | `GoldRepeatTime` | interval ของ passive gold |
| | `GoldIncreaseValue` | ทองต่อ tick |
| **Tower** | `TowerIncreaseAtkStartTime` | เวลาเริ่ม buff หอคอย |
| | `TowerIncreaseAtkValue` | ค่า ATK ที่เพิ่มต่อ buff |
| **Creep Lane** | `CreepLaneIncreaseStatTime` | เวลาเริ่ม buff minion เลน |
| | `CreepLaneIncreaseAtkValue` | ATK ที่เพิ่ม |
| | `CreepLaneIncreaseHPValue` | HP ที่เพิ่ม |
| **Creep Jungle** | `CreepJungleIncreaseStatTime` | เวลาเริ่ม buff jungle camp |
| | `CreepJungleIncreaseAtkValue` | ATK ที่เพิ่ม |
| | `CreepJungleIncreaseDamageValue` | Damage ที่เพิ่ม |
| **Mini-Boss** | `MiniBossIncreaseStatTime` | เวลาเริ่ม buff mini-boss |
| | `MiniBossIncreaseAtkValue` | ATK ที่เพิ่ม |
| **Timers** | `WaitForPlayerJoinTime` | เวลารอผู้เล่นเข้า |
| | `PickupAvatarTime` | เวลา hero select phase |
| | `PreparingBeforeGameStartTime` | countdown ก่อนเกมเริ่ม |
| | `EndTime` | เวลา end screen |
| **AFK** | `AFKWarningTime` | วินาทีก่อนเตือน AFK |
| | `AFKKickTime` | วินาทีก่อนเปลี่ยนเป็นบอท |
| **Surrender** | `SurrenderVoteTime` | เวลาให้โหวต |
| | `SurrenderReduceSecond` | cooldown ระหว่างการโหวต |

### 3.4 CBSUnit — Hero Stats Schema

| กลุ่ม | Fields | หมายเหตุ |
|-------|--------|---------|
| **Identity** | `IsEnable`, `CBSRole`, `CBSLane` | เปิด/ปิด, บทบาท, เลน |
| **Economy** | `Exp`, `Gold` | ค่า exp/gold เมื่อถูกฆ่า |
| **HP** | `MaxHp`, `MaxHpPerLevel`, `HpRegen`, `HpRegenPerLevel` | |
| **MP** | `MaxMp`, `MaxMpPerLevel`, `MpRegen`, `MpRegenPerLevel` | |
| **Armor** | `Armor`, `ArmorPerLevel` | |
| **Magic Resist** | `MagicResist`, `MagicResistPerLevel` | |
| **Attack** | `AttackDamage`, `AttackDamagePerLevel`, `AttackSpeed`, `AttackSpeedPerLevel` | |
| **Ability Power** | `MagicalDamage`, `MagicalDamagePerLevel` | |
| **Special** | `LifeSteal`, `CritChange`, `CritDamage`, `CoolDownReduce` | ไม่ scale per level |
| **Geometry** | `AttackRange`, `MoveSpeed`, `GameplayRadius`, `SelectionRadius`, `PathingRadius`, `AcqRadius`, `SightRange` | หน่วย: ค่า/100 |
| **Vision** | `DayVisionRange`, `NightVisionRange` | |
| **Skills** | `Skill0`–`Skill4`, `SkillI`, `SkillA` | List<string> ของ ability IDs |
| **UI Tags** | `Complexity`, `Attack`, `Defense`, `Crowdcontrol`, `Mobility`, `Utility` | 1–10 สำหรับ hero select UI |

### 3.5 CBSAbility — Ability Schema

| กลุ่ม | Fields | หมายเหตุ |
|-------|--------|---------|
| **Type** | `SkillKey`, `TargetingInput`, `SpellEffects`, `DamageType`, `CooldownStart` | |
| **Casting** | `CastTime`, `HasCharging`, `CanUseWhileMoving`, `WaitFinishCast` | |
| **Geometry** | `Range`, `TargetRange`, `AttackRange`, `EffectRadius`, `Width`, `Speed` | |
| **Progression** | `MaxRank`, `Costs` (List<float>), `Cooldowns` (List<float>) | index = rank-1 |
| **Formulas** | `Abilitys` (List<string>) | "key:value" เช่น "damage:100+AP*0.6" |
| **AI Weights** | `damageWeight`, `aoeWeight`, `escapeWeight`, `healWeight`, `executeWeight`, `buffWeight` | สำหรับ AI Bot |

### 3.6 ScriptableObject Layer

ใช้สำหรับ **asset references** ที่ต้องผูกกับ Unity objects:

| ScriptableObject | ข้อมูล |
|----------------|--------|
| `AvatarObject` | Hero prefab, sprites, skins, voice lines, skill combos |
| `MonsterObject` | Monster prefab, sounds, skin variants |
| `ItemObject` | Item icon, effect patterns, mythic effects |
| `DeltaConfiguration` | Game mode prefabs, services list, terrain |
| `RoleDataObject` | Role icon, banner, description |
| `RankObject` | Rank tier definition |

**กฎ**: ScriptableObjects **ไม่เก็บ balance values** — เก็บเฉพาะ asset references
Balance ทั้งหมดอยู่บน CBS/PlayFab

### 3.7 Runtime Data Access Pattern

```csharp
// ตัวอย่างการเข้าถึง hero stats
CBSUnit heroStats = MetadataService.GetCustomData<CBSUnit>("hero_garen");

// ตัวอย่างการเข้าถึง battle config
CBSConfigBattle battleConfig = MetadataService.GetCustomData<CBSConfigBattle>("ConfigBattle");

// ตัวอย่างการเข้าถึง item
CBSItemInGame item = MetadataService.GetCustomData<CBSItemInGame>("item_sword_01");
```

**MetadataService caching rules:**
- ข้อมูลถูก cache ใน `m_CustomDataCached` หลังโหลดครั้งแรก
- แต่ละ call return **cloned instance** (MemberwiseClone) เพื่อป้องกัน mutation
- Cache อยู่ตลอด session — ไม่ refresh ระหว่างแมตช์

### 3.8 การแก้ไข Balance

| เครื่องมือ | ใช้แก้ไขอะไร |
|-----------|-------------|
| **PlayFab Dashboard** | ดู/แก้ title data โดยตรง (JSON raw) |
| **Custom Update Tool** | เครื่องมือ built-in สำหรับ update batch values |
| **Azure Functions** | process การ validate และ deploy ค่าใหม่ |

การแก้ไขค่า **ไม่ต้อง build เกมใหม่** — ผู้เล่นรับค่าใหม่เมื่อ login ครั้งถัดไป

## 4. Formulas

### 4.1 Ability Cost/Cooldown by Rank

```
Cost[rank]     = CBSAbility.Costs[rank - 1]
Cooldown[rank] = CBSAbility.Cooldowns[rank - 1]
```

| ตัวแปร | คำอธิบาย |
|--------|---------|
| rank | rank ปัจจุบันของ ability (1 = rank แรก) |
| Costs | List<float> ของ cost ต่อ rank |
| Cooldowns | List<float> ของ cooldown ต่อ rank |

### 4.2 Creep Scaling (CBSCreepAmount)

```
SpawnCreepAmount = List<string> ของ "minute:amount"
GetAmount(currentMinute) → ดูว่า minute ใดที่ <= currentMinute มากสุด
                         → return amount ของ minute นั้น
```

ตัวอย่าง: `["0:3", "5:4", "10:5", "15:6"]`
- นาทีที่ 7 → ใช้ค่าของนาที 5 = 4 creeps/wave

### 4.3 Ability Formula Parsing

```
Abilitys = ["damage:100", "ap_ratio:0.6", "ad_ratio:0.4"]
GetAbility("damage")   → 100
GetAbility("ap_ratio") → 0.6
Format: "key:value" (string pair)
```

## 5. Edge Cases

| สถานการณ์ | พฤติกรรม |
|-----------|---------|
| CBS โหลดไม่สำเร็จ (offline/error) | แสดง error — เกมไม่สามารถเริ่มได้ |
| GetCustomData<T> สำหรับ ID ที่ไม่มี | return null — ต้อง null-check ทุกที่ |
| Cache hit | return MemberwiseClone (ไม่ดึงจาก server ซ้ำ) |
| แก้ไขค่าบน PlayFab ขณะมีเกมกำลังเล่น | ไม่กระทบเกมที่กำลังเล่น — มีผลเกมถัดไป |
| ผู้เล่น 2 คน login ต่างเวลา | อาจได้ config version ต่างกัน — ควร sync ที่ game start |
| `IsEnable = false` บน CBSUnit | Hero นั้นไม่ปรากฏใน hero select |
| Ability rank เกิน MaxRank | ไม่ควรเกิด — ถูก cap โดย Level/XP System |

## 6. Dependencies

| ระบบ | ความสัมพันธ์ |
|------|-------------|
| **Networking Core (F2)** | CBS ใช้ PlayFab API ซึ่งต้องการ internet/auth |
| **Actor System (F1)** | ← พึ่งพา F4 สำหรับ base stats ของทุก Actor |
| **Combat & Skills (C1)** | ← พึ่งพา CBSAbility สำหรับ ability specs |
| **Hero System (C2)** | ← พึ่งพา CBSUnit สำหรับ hero stats |
| **Gold Economy (C4)** | ← พึ่งพา CBSConfigBattle สำหรับ gold rates |
| **Item System (FT1)** | ← พึ่งพา CBSItemInGame สำหรับ item definitions |
| **Matchmaking (FT6)** | ← พึ่งพา CBSConfigBattle สำหรับ lobby timers |
| **AI/Bot System (FT5)** | ← พึ่งพา CBSAbility.AI weights สำหรับ bot decisions |

## 7. Tuning Knobs

| ค่า | ที่เก็บ | ผลกระทบ |
|-----|--------|---------|
| ค่า stat ทุกตัวของฮีโร่ | CBSUnit บน PlayFab | สมดุลฮีโร่ทั้งหมด |
| Ability cooldowns/costs | CBSAbility บน PlayFab | สมดุลสกิล |
| Item prices/stats | CBSItemInGame บน PlayFab | สมดุลเศรษฐกิจ |
| GoldIncreaseValue | CBSConfigBattle | ความเร็วเกม (ยิ่งมากเกมเร็ว) |
| CooldownAvatarRespawn | CBSConfigBattle | ความรุนแรงของการตาย |
| CreepLaneIncreaseAtkValue | CBSConfigBattle | ความแข็งของ minion ปลายเกม |
| AFKWarningTime / AFKKickTime | CBSConfigBattle | ความเร็วในการตรวจ/เปลี่ยนบอท |
| SurrenderVoteTime | CBSConfigBattle | เวลาตัดสินใจยอมแพ้ |
| Cache duration | MetadataService (code) | ความถี่ในการ refresh ข้อมูล |

## 8. Acceptance Criteria

| # | เกณฑ์ | วิธีทดสอบ |
|---|-------|----------|
| 1 | CBS โหลดสำเร็จที่ game start | Integration test: login → ตรวจ cache ไม่ว่าง |
| 2 | GetCustomData<CBSUnit> return ค่าถูกต้อง | Unit test: เทียบกับค่าที่ตั้งบน PlayFab |
| 3 | GetCustomData return clone (ไม่ใช่ reference เดิม) | Unit test: แก้ค่าที่ return → cache ไม่เปลี่ยน |
| 4 | GetCustomData ที่ไม่มี ID → return null ไม่ throw | Unit test: GetCustomData<CBSUnit>("invalid_id") |
| 5 | CBSConfigBattle โหลดค่า battle timers ถูก | Integration test: เทียบ GoldRepeatTime กับ Dashboard |
| 6 | ScriptableObject อ้างอิง prefab/sprite ถูกต้อง | Editor test: validate references ไม่ null |
| 7 | แก้ค่าบน PlayFab → เกมใหม่รับค่าอัปเดต | Integration test: อัปเดต Dashboard → restart → ตรวจ |
| 8 | เกมที่กำลังเล่นไม่ได้รับผลจากการอัปเดต mid-game | Integration test: อัปเดต Dashboard ระหว่างเกม → ตรวจ |
| 9 | `IsEnable = false` บน CBSUnit → ไม่โชว์ใน hero select | Integration test: ปิดฮีโร่ → ตรวจ hero select |
| 10 | Ability formula parsing ถูกต้อง | Unit test: GetAbility("damage") → ค่าตรง |
