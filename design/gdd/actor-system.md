---
status: reverse-documented
source: C:\GitHub\delta-unity\Assets\GameScripts\Gameplays\Characters\Actors\
date: 2026-04-02
---

# Actor System (F1)

> **Note**: เอกสารนี้ reverse-engineer จาก codebase ที่มีอยู่
> บันทึกพฤติกรรมปัจจุบันและเจตนาการออกแบบที่ได้รับการยืนยัน

## 1. Overview

Actor System คือระบบ entity พื้นฐานของทุกสิ่งมีชีวิตและโครงสร้างในเกม Delta
ทุกอย่างที่มี HP, รับ/ให้ damage, หรือมีตำแหน่งบนแผนที่คือ Actor
ระบบนี้กำหนดโครงสร้าง stats, states, components, และ lifecycle ที่ระบบอื่นทั้งหมดพึ่งพา

## 2. Player Fantasy

ผู้เล่นรู้สึกว่าตัวละครในสนามมี **น้ำหนักและตัวตน** — ฮีโร่แต่ละตัวมี stats
ที่แตกต่าง, ตอบสนองต่อ crowd control อย่างชัดเจน, และเติบโตแข็งแกร่งขึ้นตามเลเวล
ผู้เล่นอ่านสถานการณ์ได้จากสถานะของ Actor (HP bar, status icons, animation)

## 3. Detailed Rules

### 3.1 Actor Hierarchy

```
Actor (abstract)
├── ActorAvatar        — ฮีโร่ผู้เล่น/บอท
├── ActorMonster       — มอนสเตอร์พื้นฐาน
│   ├── ActorBoss      — บอสใหญ่
│   ├── ActorMiniBoss  — มินิบอส
│   ├── ActorJungle    — มอนสเตอร์ป่า
│   └── ActorDungeon   — มอนสเตอร์โหมด Dungeon
├── ActorTower         — หอคอย (มี Tier + Order)
├── ActorDummy         — หุ่นซ้อม (Training mode)
├── ActorSumonner      — สิ่งที่ถูกเรียกออกมา (summon)
└── ActorSpectator     — ผู้ชม/replay
```

### 3.2 Component Structure

แต่ละ Actor ประกอบด้วย components ผ่าน ActorKernel:

| Component | หน้าที่ |
|-----------|---------|
| **ActorUI** | แสดง HP bar, เลเวล, cooldown ultimate |
| **ActorCombat** | จัดการสกิล Q/W/E/R, normal attack, passive |
| **ActorDamageable** | รับ damage, จัดการการตาย |
| **ActorTakeDamage** | ติดตามสถิติ damage ที่รับ |
| **NetworkTrait** | ระบบ stats/attributes, status effects, modifiers |
| **ActorAnim** | จัดการ animation state |
| **ActorDriver** | การเคลื่อนที่/pathfinding (NavMesh) |
| **ActorStatistic** | ติดตาม kill/death/streak |
| **ActorAFKHandle** | ตรวจจับ AFK |

**Lifecycle**: Preload → Spawned → Initialize → Update Loop → Despawned

### 3.3 Stats (24 ตัว)

#### Stats หลัก (มี per-level scaling)

| Stat | Base | Per Level | หมายเหตุ |
|------|------|-----------|----------|
| MaxHp | จาก config | MaxHpPerLevel | - |
| MaxMp | จาก config | MaxMpPerLevel | - |
| HpRegen | จาก config | HpRegenPerLevel | ฟื้นฟูต่อวินาที |
| MpRegen | จาก config | MpRegenPerLevel | ฟื้นฟูต่อวินาที |
| AttackDamage | จาก config | AttackDamagePerLevel | - |
| Armor | จาก config | ArmorPerLevel | ลด physical damage |
| MagicResist | จาก config | MagicResistPerLevel | ลด magic damage |
| AttackSpeed | จาก config | 0 | base เท่านั้น |
| BonusAttackSpeed | 0 | AttackSpeedPerLevel | โบนัสจากเลเวล + ไอเทม |
| MoveSpeed | จาก config / 100 | 0 | clamp: MinMoveSpeed–MaxMoveSpeed |
| AttackRange | จาก config / 100 | 0 | กำหนด Melee vs Ranged |
| SightRange | จาก config / 100 | 0 | ระยะมองเห็นใน Fog of War |
| GameplayRadius | จาก config / 100 | 0 | ขนาดตัวละคร |
| AcqRadius | จาก config / 100 | 0 | ระยะ auto-acquire เป้าหมาย |

#### Stats ที่เริ่มจาก 0 (ได้จากไอเทม/บัฟเท่านั้น)

| Stat | หมายเหตุ |
|------|----------|
| AbilityPower | เพิ่มความเสียหายสกิล |
| CriticalStrikeChance | โอกาส crit |
| CriticalStrikeDamage | เริ่มจาก InitializeCriticalDamage (config) |
| CoolDownReduction | clamp: 0–MaxCoolDownReduction |
| LifeSteal | ดูดเลือดจาก auto attack |
| AbilityLifeSteal | ดูดเลือดจากสกิล |
| ArmorPiercePercent | เจาะเกราะ % |
| ArmorPierceFlat | เจาะเกราะ flat |
| MagicPiercePercent | เจาะ magic resist % |
| MagicPierceFlat | เจาะ magic resist flat |
| MindSync | [ใช้เฉพาะบางฮีโร่] |
| EnergyReduction | ลด energy cost |

#### Attributes (ค่าปัจจุบัน ผูกกับ stat max)

| Attribute | Max อ้างจาก | หมายเหตุ |
|-----------|------------|----------|
| Hp | MaxHp | เลือดปัจจุบัน |
| Mp | MaxMp | มานาปัจจุบัน |
| UniversalShields | MaxHp | โล่รับทุกประเภท |
| PhysicalShields | MaxHp | โล่รับ physical เท่านั้น |
| MagicShields | MaxHp | โล่รับ magic เท่านั้น |

### 3.4 States & Crowd Control

#### สถานะ Crowd Control

| สถานะ | ผลกระทบ |
|-------|---------|
| **Stun** | ห้ามเคลื่อนที่ + โจมตี + ใช้สกิล |
| **Root** | ห้ามเคลื่อนที่ (โจมตี + สกิลได้) |
| **Silence** | ห้ามใช้สกิล (เคลื่อนที่ + โจมตีได้) |
| **Sleep** | ห้ามทุกอย่าง (หลุดเมื่อรับ damage) |
| **KnockUp** | ลอยขึ้น ห้ามทุกอย่าง |
| **KnockBack** | ถูกผลักไป ห้ามเคลื่อนที่ |
| **Taunt** | ถูกบังคับให้โจมตีผู้ใช้ taunt |
| **Charm** | ถูกดึงดูดเข้าหาผู้ใช้ charm |
| **Fear** | วิ่งหนีอย่างควบคุมไม่ได้ |
| **MindControl** | ถูกควบคุมโดยศัตรู |
| **Hook** | ถูกดึง/ลาก |

#### ตาราง Action Permission

| Action | ต้อง **ไม่มี** สถานะเหล่านี้ |
|--------|------------------------------|
| **CanMove** | stun, root, sleep, knockback |
| **CanAttack** | stun, unarmed, sleep, fear, charm |
| **CanSkill** | stun, silence, sleep, mindcontrol, fear, charm |
| **CanDash** | root, dash (กำลัง dash อยู่), fear, charm |

#### สถานะบัฟ/ยูทิลิตี้

| สถานะ | ผลกระทบ |
|-------|---------|
| **Immortal** | ไม่รับ damage |
| **Invisibility** | ศัตรูมองไม่เห็น |
| **NotDeadYet** | รอดตายครั้งแรกที่ HP ถึง 0 |
| **CatchUp** | เพิ่มความเร็ว |
| **BlueBuff** | บัฟ stats จากป่าฝั่งน้ำเงิน |
| **RedBuff** | บัฟ damage จากป่าฝั่งแดง |
| **Burn** | DoT damage ต่อเนื่อง |
| **BurnMana** | ลด mana ต่อเนื่อง |
| **Revel** | ถูกเปิดเผยใน Fog of War |
| **TowerDisturbed** | หอคอยจะโจมตี |

#### สถานะเฉพาะฮีโร่

| สถานะ | ผลกระทบ |
|-------|---------|
| **IsAtBase** | อยู่ที่ฐาน (regen เร็วขึ้น) |
| **IsRecalling** | กำลังเทเลพอร์ตกลับฐาน |
| **IsAttackingHero** | กำลังต่อสู้กับฮีโร่ศัตรู |
| **IsBeingAttacked** | ถูกโจมตีใน 1.5 วินาทีล่าสุด |

### 3.5 Shield Damage Order

เมื่อ Actor รับ damage ลำดับการหักคือ:

```
1. ตรวจ Immortal → ถ้ามี ไม่รับ damage
2. ตรวจ NotDeadYet → ถ้า HP จะถึง 0 และยังไม่เคยใช้ → HP = 1
3. หัก Type-Specific Shield ก่อน (Physical Shield สำหรับ physical damage, Magic Shield สำหรับ magic damage)
4. ถ้า Type-Specific Shield หมด → หัก Universal Shield เป็น fallback
5. ถ้า Shield ทั้งหมดหมด → หัก HP
```

**เหตุผล**: ให้ค่ากับการซื้อไอเทม shield เฉพาะทาง — ถ้าซื้อ Magic Shield มาเจอ
magic damage ควรใช้ของนั้นก่อน, Universal เก็บไว้รับ damage ประเภทอื่นที่ไม่มี shield เฉพาะ

### 3.6 Networking

ทุก Actor เป็น Photon Fusion NetworkBehaviour:

**Networked Properties หลัก:**
- PlayfabId, PlayerName, ObjectId, SkinId, SkinWeaponId
- Team, ObjectType, IsBot, IsDead
- IsUseSkill, IsCastingItem, IsAFK

**Stat Sync:**
- ใช้ `NetworkDictionary<NetworkString<_32>, float>` สำหรับค่าตัวเลข
- ใช้ `NetworkDictionary<NetworkString<_32>, NetworkBool>` สำหรับ flags
- Status effects sync ผ่าน NetworkTrait

**RPCs:**
- `RPC_Hit(NetworkDamageFeedback)` — แจ้ง damage ไปทุก client
- `RpcGainExp(isLevelUp, level, exp)` — แจ้ง exp/levelup
- `RpcUpdateSpell(level)` — แจ้งอัปเดต spell rank

## 4. Formulas

### 4.1 Stat Calculation

```
FinalStat = (BaseValue + PerLevelValue × Level + FlatModifiers) × (1 + PercentModifiers)
```

| ตัวแปร | คำอธิบาย |
|--------|---------|
| BaseValue | ค่าเริ่มต้นจาก config ของแต่ละ hero/monster |
| PerLevelValue | ค่าที่เพิ่มต่อเลเวล จาก config |
| Level | เลเวลปัจจุบันของ Actor |
| FlatModifiers | ผลรวม flat bonus จากไอเทม/บัฟ |
| PercentModifiers | ผลรวม % bonus จากไอเทม/บัฟ |

### 4.2 Level & Experience

```
เริ่มต้น:
  Level = InitialLevel (จาก config, ปกติ = 1)
  MaxExp = CBSBattleLevelExp[Level]
  SpellPoint = Level

เลเวลอัป:
  while CurrentExp >= MaxExp AND Level < MaxLv:
    CurrentExp -= MaxExp
    Level += 1
    SpellPoint += 1
    MaxExp = CBSBattleLevelExp[Level]
```

### 4.3 Skill Rank

```
สกิล Q/W/E:  MaxRank = ceil(Level / 2)
สกิล R (ult): MaxRank = floor(Level / 6)   // สูงสุด 3
```

| ตัวแปร | คำอธิบาย | ช่วงค่า |
|--------|---------|---------|
| Level | เลเวลปัจจุบัน | 1–MaxLv |
| MaxRank | rank สูงสุดที่ปลดล็อค | Q/W/E: 1–9, R: 0–3 |

### 4.4 Kill Bounty (Gold)

```
ถ้า killStreak >= 8:  bounty = 1000 + ((killStreak - 7) × 100)
ถ้า killStreak == 7:  bounty = 1000
ถ้า killStreak == 6:  bounty = 900
ถ้า killStreak == 5:  bounty = 800
ถ้า killStreak == 4:  bounty = 700
ถ้า killStreak == 3:  bounty = 600
ถ้า killStreak == 2:  bounty = 450
ถ้า killStreak == 1:  bounty = 400
ถ้า killStreak == 0:  bounty = 300

ถ้า deathStreak (ตายติดต่อ):
  -1: 270,  -2: 220,  -3: 170,  -4: 150,  -5+: 120
```

| ตัวแปร | คำอธิบาย | ช่วงค่า |
|--------|---------|---------|
| killStreak | จำนวนฆ่าติดต่อไม่ตาย | 0–∞ |
| deathStreak | จำนวนตายติดต่อไม่ฆ่า | 0 ถึง -5+ |
| bounty | ทองที่ได้จากการฆ่า | 120–1000+ |

### 4.5 Assist Gold

```
AssistGold = (KillBounty / 2) / AssistPlayerCount

Consecutive Kill Bonus:
  BonusGold = max(0, (ConsecutiveCount - 1) × 60)
  // Window: 12 วินาที ระหว่างการฆ่า
```

| ตัวแปร | คำอธิบาย | ช่วงค่า |
|--------|---------|---------|
| KillBounty | ค่าหัวเป้าหมาย | 120–1000+ |
| AssistPlayerCount | จำนวนคนช่วย | 1–4 |
| ConsecutiveCount | จำนวนฆ่าใน 12 วินาที | 1–∞ |

### 4.6 Monster Level Scaling

```
MonsterLevel = 1 + max(AverageHeroLevel, MinimumMonsterLevel)
Boss/MiniBoss minimum level = 6
```

### 4.7 Hero Experience from Enemy Kill

| Enemy Level | EXP |
|-------------|-----|
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

## 5. Edge Cases

| สถานการณ์ | พฤติกรรม |
|-----------|---------|
| HP ถึง 0 + มี NotDeadYet | HP = 1, ใช้ได้ครั้งเดียวต่อชีวิต |
| Damage ขณะ Immortal | ไม่รับ damage ใดๆ |
| Stun + Root พร้อมกัน | ทั้งสองมีผล — CanMove = false (จาก root), CanAttack = false (จาก stun) |
| Sleep + ถูกตี | Sleep หลุด (ตรวจจาก IsBeingAttacked) |
| Shield เหลือ 50 + damage 100 | Shield หมด → damage ที่เหลือ 50 หักจาก HP |
| Recall + ถูกตี | Recall ยกเลิก (IsCancelRecall = true) |
| Actor ตาย + มี killer | เรียก OnDead(victim, caster) → ActorStatistic อัปเดต streak ทั้งสองฝั่ง |
| ตาย + ไม่มี killer (เช่น ตกน้ำ) | caster = null, ไม่ให้ bounty ใคร |
| Disconnect ขณะมีชีวิต | IsAFK = true → ระบบ AFK อาจแทนที่ด้วยบอท |
| Level 16 → EXP จาก kill | EXP = 690 (ไม่ใช่ 840 — ดูตาราง, อาจเป็น bug หรือ intentional) |
| MoveSpeed ต่ำ/สูงเกินไป | Clamp ที่ MinMoveSpeed–MaxMoveSpeed จาก config |
| CDR สูงเกินไป | Clamp ที่ 0–MaxCoolDownReduction จาก config |

## 6. Dependencies

| ระบบ | ความสัมพันธ์ |
|------|-------------|
| **Networking Core (F2)** | Actor ใช้ Photon Fusion NetworkBehaviour, RPCs, NetworkDictionary |
| **Input System (F3)** | ActorDriver รับ input สำหรับการเคลื่อนที่ |
| **Data/Config System (F4)** | Stats base values โหลดจาก CBS config |
| **Combat & Skills (C1)** | ← พึ่งพา Actor สำหรับ stats, states, damage pipeline |
| **Hero System (C2)** | ← พึ่งพา ActorAvatar เป็น base class |
| **Gold Economy (C4)** | ← พึ่งพา ActorStatistic สำหรับ kill bounty |
| **Level/XP (C5)** | ← พึ่งพา NetworkTrait สำหรับ level/exp tracking |
| **Item System (FT1)** | ← พึ่งพา stat modifier system |
| **Fog of War (FT4)** | ← พึ่งพา Actor สำหรับ visibility/SightRange |

## 7. Tuning Knobs

| ค่า | ปัจจุบัน | ช่วงปลอดภัย | ผลกระทบ |
|-----|---------|------------|---------|
| InitialLevel | 1 | 1–3 | เลเวลเริ่มต้น ยิ่งสูงยิ่งข้ามช่วง early game |
| MaxLv | จาก config | 15–25 | เลเวลสูงสุด ยิ่งสูงเกมยิ่งยาว |
| InitializeCriticalDamage | จาก config | 1.5–2.5 | base crit multiplier |
| MinMoveSpeed | จาก config | 0.5–2.0 | ป้องกัน perma-slow |
| MaxMoveSpeed | จาก config | 5.0–10.0 | ป้องกันความเร็วเกิน |
| MaxCoolDownReduction | จาก config | 30%–50% | ป้องกัน spam สกิล |
| Kill Bounty (base 300) | 300 | 200–500 | ค่าหัวพื้นฐาน ยิ่งสูงยิ่ง snowball |
| Streak Bonus Step | 100 | 50–200 | ทองเพิ่มต่อ streak ยิ่งสูงยิ่งลงโทษผู้เล่นที่ feed |
| Assist Gold Ratio | 50% | 30%–70% | ทอง assist ยิ่งสูงยิ่งให้ค่า support |
| Consecutive Kill Window | 12s | 8–20s | เวลาสำหรับ consecutive bonus |
| Consecutive Kill Bonus | 60 | 30–100 | ทองเพิ่มต่อ consecutive kill |
| IsBeingAttacked Window | 1.5s | 1.0–3.0s | เวลาที่ถือว่า "กำลังถูกตี" |
| Boss Min Level | 6 | 4–10 | เลเวลต่ำสุดบอส ยิ่งสูงบอสยิ่งแข็ง early |

## 8. Acceptance Criteria

| # | เกณฑ์ | วิธีทดสอบ |
|---|-------|----------|
| 1 | Actor spawn แล้ว stats ตรงกับ config | Unit test: เทียบ stat values กับ config table |
| 2 | Stat scaling ถูกต้องทุกเลเวล | Unit test: level 1→MaxLv ตรวจสูตร Base + PerLevel × Lv |
| 3 | Modifier (flat + %) คำนวณถูก | Unit test: เพิ่ม modifier → ตรวจ FinalStat |
| 4 | Crowd control บล็อก action ตามตาราง | Unit test: ใส่ stun → CanMove/CanAttack/CanSkill ตรง |
| 5 | Shield หักตามลำดับ Universal → Type → HP | Unit test: ให้ shield 50 + damage 100 → ตรวจ HP ลดถูก |
| 6 | Kill bounty ตรงกับ streak | Unit test: streak 0–8+ → bounty ตรงตาราง |
| 7 | Assist gold แบ่งถูกต้อง | Unit test: 3 assist → ตรวจทองที่แต่ละคนได้ |
| 8 | Levelup trigger เมื่อ exp ถึง threshold | Unit test: ให้ exp → ตรวจ level + spell point |
| 9 | SkillRank cap ตรงกับสูตร | Unit test: level 1–18 → ตรวจ MaxRank Q/W/E/R |
| 10 | Network sync ถูกต้อง | Integration test: เปลี่ยน stat ฝั่ง server → client เห็นค่าตรง |
| 11 | OnDead event fire เมื่อ HP = 0 | Integration test: damage จนตาย → OnDead ถูกเรียก |
| 12 | NotDeadYet ทำงานครั้งเดียว | Unit test: HP→0 ครั้งแรก = รอด, ครั้งที่ 2 = ตาย |
| 13 | MoveSpeed clamp ทำงาน | Unit test: set MoveSpeed เกิน max → ตรวจว่า clamp |
| 14 | Monster level scale ตาม average hero level | Integration test: hero avg lv 5 → monster lv 6 |
| 15 | Actor despawn cleanup ไม่ leak | Integration test: spawn→despawn → ตรวจ event listeners = 0 |
