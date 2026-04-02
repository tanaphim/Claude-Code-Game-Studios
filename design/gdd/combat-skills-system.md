---
status: reverse-documented
source: C:\GitHub\delta-unity\Assets\GameScripts\Gameplays\Cores\DamageSystem\
date: 2026-04-02
---

# Combat & Skills System (C1)

> **Note**: เอกสารนี้ reverse-engineer จาก codebase ที่มีอยู่
> บันทึกพฤติกรรมปัจจุบันและเจตนาการออกแบบที่ได้รับการยืนยัน

## 1. Overview

Combat & Skills System คือระบบที่จัดการทุกอย่างที่เกี่ยวกับการต่อสู้ใน Delta:
การใช้สกิล, การคำนวณ damage, การรับ damage, status effects, kill rewards
และ auto-attack ระบบนี้เป็นแก่นของ gameplay loop ทุกแมตช์

## 2. Player Fantasy

ผู้เล่นรู้สึกว่าทุกการโจมตีมี **น้ำหนักและผลลัพธ์ที่ชัดเจน** — เห็นตัวเลข damage,
รู้สึกถึงความแตกต่างระหว่าง crit กับ normal hit, รู้ว่า shield กำลังป้องกันอะไร
ชนะด้วยความเข้าใจ mechanics ไม่ใช่แค่กดปุ่มเร็ว

## 3. Detailed Rules

### 3.1 Damage Types

| ประเภท | ถูกลด damage โดย | หมายเหตุ |
|--------|-----------------|---------|
| **Physical** | Armor | auto-attack ส่วนใหญ่ |
| **Magic** | Magic Resist | สกิลส่วนใหญ่ |
| **True** | ไม่มี | ทะลุทุกการป้องกัน |

### 3.2 Spell Effects (ประเภทสกิล)

ใช้สำหรับกำหนด bonus damage และ life steal:

| ค่า | ความหมาย |
|-----|---------|
| **BasicAttack** | Auto-attack — ใช้ LifeSteal |
| **Spell** | สกิล single target — ใช้ AbilityLifeSteal |
| **Aoe** | สกิล area of effect — ใช้ AbilityLifeSteal |
| **Dot** | Damage over time |
| **AoeDot** | AoE damage over time |
| **Proc** | Effect ที่ trigger จากเงื่อนไข |
| **Heal** | การฟื้นฟู HP |
| **Shield** | การสร้าง shield |
| **SpellAoe** | AoE variant อีกประเภท |

### 3.3 Skill Keys & Types

**Hotkeys:**
| Key | สกิล |
|-----|------|
| **I** | Passive |
| **A** | Auto-attack |
| **Q** | Skill 1 |
| **W** | Skill 2 |
| **E** | Skill 3 |
| **R** | Skill 4 (Ultimate) |
| **Item** | ใช้ไอเทม active |
| **Recall** | เทเลพอร์ตกลับฐาน |

**Skill Types:**
- **Active**: ผู้เล่นกดเอง
- **Passive**: trigger อัตโนมัติจากเงื่อนไข
- **Buff**: โบนัส passive

### 3.4 Skill State Machine

ทุกสกิลใช้ Animator-driven state machine:

```
States:
Attack1, Attack2, Attack3   — auto-attack phases
StartCast                   — เริ่ม animation cast
Casting                     — กำลัง cast
Perform                     — execute หลัก
Perform2–Perform5           — execute เพิ่มเติม (สกิลซับซ้อน)
Empower                     — สถานะ empowered
Empower2, Empower3          — empower variants
```

**Lifecycle ของสกิลหนึ่งครั้ง:**

```
1. Input (กดปุ่ม)
   ↓
2. ตรวจ conditions: range, cooldown, mana, target type
   ↓
3. เริ่ม Animator state
   ↓
4. OnStateEnter() → SkillManager.CallStateEnter()
   ↓
5. Animation เล่น...
   ↓
6. Animation Event → OnStateRelease(id)
   → Hit detection (raycast/overlap sphere)
   → OnHit() → route by SkillState
   ↓
7. ApplyDamage() → DamageMessage → target.ApplyDamage()
   ↓
8. DamageHitFilter() → formula → reductions → shields → HP
   ↓
9. Life steal, status effects, statistics
   ↓
10. OnStateExit() → StartCooldown()
```

### 3.5 Auto-Attack System

**เงื่อนไขที่ต้องเป็น true ทั้งหมด:**
- IsServer = true (server เท่านั้นตัดสิน)
- IsAutoAttack = true (toggle โดย UI)
- !IsCasting (ไม่กำลัง cast สกิล)

**Flow:**
```
1. FindTarget():
   - Search radius = max(Range × 2, AcqRadius)
   - Sort by distance (closest first)
   - เลือก enemy ที่ใกล้สุดและ alive

2. MoveToAction():
   - เดินเข้าหา target
   - โจมตีเมื่ออยู่ใน AttackRange

3. Attack loop:
   - โจมตีซ้ำ (delay ตาม AttackSpeed)
   - จนกว่า target ตาย หรือ IsAutoAttack = false
```

### 3.6 Damage Calculation Pipeline

**ขั้นตอนทั้งหมดตามลำดับ:**

```
STEP 1: คำนวณ base damage จาก formula
  damage = Evaluate(formula, caster, target, skillLv)

STEP 2: Critical Strike
  if Critical == Guarantee:  isCrit = true
  if Critical == Allow:
    roll random 0–1
    if random <= (CritChance / 100): isCrit = true
  if isCrit:
    damage *= (CriticalStrikeDamage / 100)

STEP 3: Defense Reduction (Physical/Magic เท่านั้น, True ข้ามขั้นตอนนี้)
  defense = target.Armor  OR  target.MagicResist
  defense -= source.ArmorPierceFlat  OR  MagicPierceFlat
  defense *= (1 - source.ArmorPiercePercent / 100)  OR  (1 - MagicPiercePercent / 100)

  if defense >= 0:
    damage *= 100 / (90 + defense)
  else:
    damage *= (1 + (-defense / 90))

STEP 4: Bonus Damage (จากไอเทม/บัฟ)
  bonus_flat    = Σ flat bonuses ที่ match SpellEffects, DamageType, UnitType
  bonus_percent = Σ % bonuses ที่ match
  damage += bonus_flat
  damage *= (1 + bonus_percent + ForcedBonusDamage%)

STEP 5: Damage Mitigation (จากไอเทม/บัฟบน target)
  reduce_flat    = Σ flat reduction ที่ match DamageType
  reduce_percent = Σ % reduction ที่ match
  damage -= reduce_flat
  damage *= (1 - reduce_percent)

STEP 6: Clamp
  damage = max(damage, 0)
  damage = min(damage, Capped)  // ถ้ามี damage cap
```

**Tower Damage Reduction (เพิ่มเติม):**
```
ถ้า target = Tower:
  ถ้า caster = Hero:    damage *= 0.5   // 50% reduction
  ถ้า caster = อื่นๆ:  damage *= 0.7   // 30% reduction
```

### 3.7 Defense Formula — ตารางอ้างอิง

| Armor/MR | Damage Reduction | % Damage ที่รับ |
|----------|-----------------|----------------|
| -50 | -55.6% (amplify) | 155.6% |
| 0 | 0% | 100% |
| 10 | 10% | 90% |
| 50 | 35.7% | 64.3% |
| 90 | 50% | 50% |
| 100 | 52.6% | 47.4% |
| 200 | 68.9% | 31.1% |
| 400 | 81.6% | 18.4% |

> **หมายเหตุ**: ไม่มี hard cap — armor สูงขึ้นเรื่อยๆ แต่ diminishing returns

### 3.8 Damage Absorption Order

```
1. Immortal check → ถ้า true: ไม่รับ damage ใดๆ
2. Barrier check:
   - ถ้ามี Barrier และ damage มาจากด้านหน้า (IsFront(caster) = true):
     Barrier รับก่อน
3. Type-Specific Shield:
   - Physical damage → หัก PhysicalShield ก่อน
   - Magic damage → หัก MagicShield ก่อน
4. Universal Shield:
   - ถ้า type-specific หมด → หัก UniversalShield
5. HP:
   - ถ้า Shield ทั้งหมดหมด → หัก HP
```

> **Barrier**: directional defense (ป้องด้านหน้า) — damage จากด้านหลังข้าม Barrier
> UI ควรแสดง indicator ทิศทางของ Barrier ให้ชัดเจน

### 3.9 Life Steal

```
BasicAttack damage:
  heal = (LifeSteal / 100) × damage
  caster.HP += heal

Spell/Aoe/SpellAoe/AoeDot damage:
  heal = (AbilityLifeSteal / 100) × damage
  caster.HP += heal
```

| Stat | ใช้กับ |
|------|--------|
| **LifeSteal** | BasicAttack เท่านั้น |
| **AbilityLifeSteal** | Spell, Aoe, SpellAoe, AoeDot |

### 3.10 Critical Strike

| Critical enum | พฤติกรรม |
|--------------|---------|
| **None** | crit ไม่สามารถเกิดได้ |
| **Allow** | roll ตาม CriticalStrikeChance% |
| **Guarantee** | crit ทุกครั้ง |

```
damage หลัง crit = damage × (CriticalStrikeDamage / 100)
// CriticalStrikeDamage = 200 → damage × 2.0 (double)
```

### 3.11 Cooldown System

แต่ละสกิลมี **2 cooldown** ที่รันพร้อมกันได้:

| Cooldown | ใช้สำหรับ |
|----------|----------|
| **MainCooldown** | cooldown หลัก แสดงบน UI |
| **SubCooldown** | cooldown รอง สำหรับสกิลที่มี phase หลายช่วง |

```
ActualCooldown = BaseCooldown × (1 - CoolDownReduction / 100)
ActualCost     = BaseCost     × (1 - EnergyReduction / 100)
```

### 3.12 Status Effect Application

```
RunStatusEffect(target, effectKey, valueKey, durationKey):
  1. Boss/MiniBoss → return null (immune ทุก status effects)
  2. คำนวณ value = GetAbilityData(valueKey) หรือ Parse(valueKey)
  3. คำนวณ duration = GetAbilityData(durationKey) หรือ Parse(durationKey)
  4. ตรวจ max stack → ถ้าเต็มและ duration <= 0: return null
  5. CreateStatusEffect(effectKey, target, value, duration)
```

**Stack System:**
- Status effect เดียวกันสามารถ stack ได้หลายครั้ง
- แต่ละ stack มี duration แยกกัน
- ถ้า `HasMaxStack = true` จะ cap ที่ MaxStack

### 3.13 Modifier System

Status effects ปรับ stats ผ่าน ModifierWay:

| ModifierWay | ผลกระทบ |
|------------|---------|
| **Stat** | ปรับ stat โดยตรง เช่น -10% Armor |
| **BonusSpellEffects** | เพิ่ม damage ตามประเภทสกิล เช่น +20% Spell |
| **BonusDamageType** | เพิ่ม damage ตาม type เช่น +15% Physical |
| **BonusDamageUnitType** | เพิ่ม damage ต่อ unit type เช่น +25% vs Monsters |
| **ReduceDamageType** | ลด damage ที่รับ เช่น -30% Magic |

### 3.14 Mind Control Kill Credit

```
ถ้าเหยื่อมีสถานะ MindControl:
  killer = GetStatusEffect("mindcontrol").Source.Actor  // ผู้ใช้ mind control
  ไม่ใช่ผู้ที่กดโจมตีครั้งสุดท้าย
```

## 4. Formulas

### 4.1 Damage Formula Syntax

```
[source.stat[ABILITY_POWER]]      — AP ของผู้ใช้
[source.stat[ATTACK_DAMAGE]]      — AD ของผู้ใช้
[target.stat[ARMOR]]              — Armor ของเป้าหมาย
[skill_lv]                        — rank ของสกิลปัจจุบัน
[source.var[VAR_NAME]]            — ตัวแปร custom
[source.base[STAT]]               — ค่า base stat
[source.initial[STAT]]            — ค่า initial stat
[source.growth[STAT]]             — ค่า growth stat

Operators: +, -, *, /, ^, (, )
```

**ตัวอย่าง:**
```
"[source.stat[ATTACK_DAMAGE]] * 1.5 + [source.stat[ABILITY_POWER]] * 0.6"
→ (caster AD × 1.5) + (caster AP × 0.6)

"100 + 20 * [skill_lv]"
→ 100 + (20 × rank)
```

### 4.2 Defense Reduction Formula

```
ถ้า defense >= 0:  damage_multiplier = 100 / (90 + defense)
ถ้า defense < 0:   damage_multiplier = 1 + (-defense / 90)
```

| ตัวแปร | คำอธิบาย |
|--------|---------|
| defense | Armor หรือ MagicResist หลังหัก pierce |
| damage_multiplier | ตัวคูณ damage สุดท้าย |

### 4.3 Kill EXP (Hero ฆ่า Hero)

```
base_exp = EXPTable[victim.level]     // ดูตารางใน Actor System

level_diff = caster.level - victim.level
if level_diff == 1:  base_exp *= 1.16
if level_diff == 2:  base_exp *= 1.32
if level_diff == 3:  base_exp *= 1.48
if level_diff >= 4:  base_exp *= 1.60

final_exp = base_exp / nearby_damager_count
            (แบ่งให้ผู้เล่นที่ deal damage ทุกคน)
```

### 4.4 Kill Gold

```
kill_gold  = GetKillBounty(killstreak, deathstreak)  // ดูตารางใน Actor System
first_blood_bonus = +100 gold (ถ้าเป็น kill แรกของเกม)
consecutive_bonus = max(0, (count - 1) × 60)         // window: 12 วินาที

assist_gold = (kill_gold / 2) / assist_count          // max 4 คน assist
```

### 4.5 Cooldown with CDR

```
actual_cooldown = base_cooldown × (1 - CDR / 100)
actual_cost     = base_cost     × (1 - EnergyReduction / 100)
```

## 5. Edge Cases

| สถานการณ์ | พฤติกรรม |
|-----------|---------|
| Damage ถึง Immortal | ไม่รับ damage ใดๆ ทุกประเภท |
| True damage + Barrier | True damage ข้าม defense แต่ยัง hit Barrier |
| True damage + Shield | True damage ยัง hit Shield → ลด HP |
| Crit + Shield | Crit damage คำนวณก่อน แล้วค่อยหักออกจาก Shield |
| LifeSteal จาก True damage | LifeSteal ยังคำนวณจาก damage ที่ deal จริง |
| Mind control kill | Kill credit → ผู้ใช้ mind control ไม่ใช่ผู้ตีสุดท้าย |
| Boss/MiniBoss + status effect | Immune ทุก status — RunStatusEffect return null |
| Status stack เต็ม + duration <= 0 | ไม่ apply เพิ่ม — stack cap มีผล |
| CDR ทำให้ cooldown ติดลบ | Clamp ที่ 0 (cooldown ขั้นต่ำ = 0) |
| Barrier hit จากด้านหลัง | ข้าม Barrier ไปหัก Shield โดยตรง |
| Hero ตีหอคอย | damage × 0.5 (50% reduction) |
| Minion ตีหอคอย | damage × 0.7 (30% reduction) |
| Armor ติดลบ | damage amplification = 1 + (-armor / 90) |
| Assist count > 4 | คำนวณแบ่งจาก 4 คน (cap ที่ 4) |
| SkillState ไม่ match OnHit handler | ไม่ deal damage (miss routing) |

## 6. Dependencies

| ระบบ | ความสัมพันธ์ |
|------|-------------|
| **Actor System (F1)** | Stats (AD, AP, Armor, LifeSteal ฯลฯ), States (Stun, Silence ฯลฯ), Shield attributes |
| **Data/Config System (F4)** | CBSAbility formulas, cooldowns, costs |
| **Networking Core (F2)** | RPC_Hit, server-authoritative damage calculation |
| **Input System (F3)** | รับ key press → trigger skill |
| **Hero System (C2)** | ← พึ่งพา C1 สำหรับ skill execution framework |
| **Item System (FT1)** | ← ไอเทม active ใช้ pipeline เดียวกับ skill; `CBSAbility.ItemAnimationType` กำหนด animator state ที่เล่นขณะใช้ไอเทม (ดู FT1 §3.8) |
| **HUD & In-Game UI (P1)** | ← แสดง damage numbers, cooldown, shield bar |
| **AI/Bot System (FT5)** | ← ใช้ CBSAbility.AI weights สำหรับ skill decisions |
| **Statistics & History (M5)** | ← รับ damage events สำหรับ tracking |

## 7. Tuning Knobs

| ค่า | ที่เก็บ | ผลกระทบ |
|-----|--------|---------|
| Defense formula base (90) | Code constant | เปลี่ยน 90 → สูงกว่า = defense มีค่าน้อยลง |
| Tower Hero damage reduction (50%) | Code constant | ยิ่งสูง tower ยิ่งอยู่นาน |
| Tower Objective damage reduction (30%) | Code constant | - |
| Level diff EXP multiplier (1.16/level) | Code constant | สูงกว่า = snowball มากกว่า |
| Life steal (per hero/item) | CBS PlayFab | สมดุล sustain |
| CDR cap | CBSConfigBattle | ป้องกัน spam |
| Barrier direction check | Code logic | เปลี่ยนเป็น omnidirectional ได้ |
| Consecutive kill window (12s) | Code constant | กว้างกว่า = bonus บ่อยกว่า |
| First blood bonus (100 gold) | Code constant | - |
| Status effect stack limits | per-effect config | - |

## 8. Acceptance Criteria

| # | เกณฑ์ | วิธีทดสอบ |
|---|-------|----------|
| 1 | Physical damage ลดตาม Armor formula | Unit test: 100 dmg + 100 armor → 47.4 dmg |
| 2 | Magic damage ลดตาม MagicResist formula | Unit test: 100 dmg + 90 MR → 50 dmg |
| 3 | True damage ไม่ถูกลด | Unit test: 100 true dmg + 999 armor → 100 dmg |
| 4 | Armor pierce ลด defense ก่อนคำนวณ | Unit test: 100 armor - 50 pierce → ใช้ 50 armor |
| 5 | Crit chance roll ถูกต้อง | Unit test: 10,000 samples @ 50% → ~50% crit rate |
| 6 | Crit damage multiplier ถูกต้อง | Unit test: 100 dmg, CritDmg=200 → 200 dmg |
| 7 | Life steal heal ถูกต้อง | Unit test: 100 dmg, 20% LS → +20 HP |
| 8 | Damage absorption order ถูก | Unit test: 50 shield + 100 dmg → shield=0, HP-50 |
| 9 | Barrier block damage จากด้านหน้า | Integration test: โจมตีจากด้านหน้า → Barrier ลดก่อน |
| 10 | Barrier ไม่ block damage จากด้านหลัง | Integration test: โจมตีจากด้านหลัง → Shield ลดโดยตรง |
| 11 | Tower ลด damage 50% จากฮีโร่ | Integration test: 100 dmg → Tower รับ 50 |
| 12 | Status effect immune บน Boss | Unit test: RunStatusEffect บน Boss → return null |
| 13 | Mind control kill credit ถูก | Integration test: MC kill → killer = MC user |
| 14 | CDR ลด cooldown ถูก | Unit test: 10s CD, 30% CDR → 7s actual |
| 15 | Kill EXP level diff bonus ถูก | Unit test: kill lv+4 → EXP × 1.60 |
| 16 | Assist gold แบ่งถูก | Unit test: 3 assists → ทองแต่ละคน = (kill_gold/2)/3 |
| 17 | Formula parser คำนวณถูก | Unit test: "[source.stat[ATTACK_DAMAGE]] * 2" → AD×2 |
| 18 | Skill cooldown เริ่มหลัง state exit | Integration test: สกิล → ตรวจ cooldown เริ่มหลัง animation จบ |
