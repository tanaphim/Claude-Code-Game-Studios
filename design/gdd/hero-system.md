---
status: reverse-documented
source: C:\GitHub\delta-unity\Assets\GameScripts\Gameplays\Characters\Actors\ActorAvatar.cs
date: 2026-04-02
---

# Hero System (C2)

> **Note**: เอกสารนี้ reverse-engineer จาก codebase ที่มีอยู่
> บันทึกพฤติกรรมปัจจุบันและเจตนาการออกแบบที่ได้รับการยืนยัน

## 1. Overview

Hero System จัดการทุกอย่างที่ทำให้ฮีโร่แตกต่างจาก Actor ทั่วไป:
การเลือกฮีโร่ก่อนเกม, ระบบบทบาท 6 ประเภท, inventory ไอเทม, recall กลับฐาน,
cosmetics, และ skill architecture ที่เป็นพื้นฐานให้ฮีโร่ 25+ ตัว

## 2. Player Fantasy

ผู้เล่นรู้สึกว่า **ฮีโร่ที่เลือกคือตัวตนในเกม** — มี playstyle เฉพาะตัว,
สกิลที่ต้องเรียนรู้, และ mastery ที่สร้างได้จากการฝึก ฮีโร่ที่ซับซ้อนให้ความพึงพอใจ
เมื่อใช้งานได้อย่างชำนาญ ฮีโร่ที่ง่ายทำให้ผู้เล่นใหม่เข้าถึงได้ทันที

## 3. Detailed Rules

### 3.1 Hero Roles

| Role | บทบาท | จุดเด่น |
|------|--------|---------|
| **Tank** | ผู้นำทีม, รับ damage | HP สูง, Armor/MR สูง, Crowd Control |
| **Support** | ช่วยเหลือทีม | Heal, Buff/Debuff, Utility |
| **Carry** | Damage หลักในระยะยาว | Physical DPS, scaling สูง, late-game power |
| **Fighter** | Melee hybrid | Damage + Tankiness, sustained fights |
| **Assassin** | กำจัดเป้าหมายเดี่ยว | Burst damage, Mobility สูง |
| **Mage** | Magic damage + CC | AoE, Crowd Control, ability-based |

**Multi-role:** ฮีโร่สามารถมีได้มากกว่า 1 บทบาท เช่น Tank/Fighter, Mage/Support
บทบาทแรกใน array คือ Primary Role, ที่เหลือคือ Secondary

### 3.2 Lanes

| Lane | ผู้เล่นที่เหมาะสม | หมายเหตุ |
|------|-----------------|---------|
| **Top** | Tank, Fighter | Solo lane |
| **Mid** | Mage, Assassin | Solo lane กลางแผนที่ |
| **Bot** | Carry | Dual lane กับ Support |
| **Jungle** | Fighter, Assassin | Farm neutral camps, gank |
| **Support** | Support | Roam กับ Carry ที่ Bot |

> Lane assignment เป็นคำแนะนำ ไม่ใช่บังคับ — ผู้เล่นเลือก Primary + Secondary role
> ก่อนเกม แต่ทีมตกลงกันเองว่าใครไปเลนไหน

### 3.3 Difficulty System

| ระดับ | ความหมาย | ตัวอย่าง |
|-------|---------|---------|
| **Low** | เหมาะมือใหม่ — mechanics ตรงไปตรงมา | Athena (Tank) |
| **Moderate** | ต้องการความเข้าใจ timing และ positioning | Artemis (Carry) |
| **High** | Mechanics ซับซ้อน เช่น stance changes, stacking | Merlin (Mage) |

**Complexity Ratings** (0–10 ต่อด้าน สำหรับ Hero Select UI):

| Rating | ความหมาย |
|--------|---------|
| **Complexity** | ความยากรวมของ kit |
| **Attack** | ความสามารถรุก |
| **Defense** | ความสามารถรับ |
| **Crowdcontrol** | ความสามารถ CC ศัตรู |
| **Mobility** | ความคล่องตัว |
| **Utility** | ประโยชน์ต่อทีม |

### 3.4 Hero Selection (Draft)

**ขั้นตอน:**
```
1. Role Selection Phase
   - ผู้เล่นเลือก Primary Role + Secondary Role
   - บันทึกใน settings (KEY_ROLEFRIST, KEY_ROLESECOND)

2. Hero Selection Phase
   - ผู้เล่นเลือกฮีโร่ที่ match กับ role ที่เลือก
   - ดู stats, skills, difficulty ได้ใน hero select UI

3. Match Start
   - ฮีโร่ spawn ที่ base ของทีม
   - ทีมตกลง lane assignment กันเอง
```

**Ban Phase:** ยังไม่มีในปัจจุบัน — อาจเพิ่มใน Ranked Mode ในอนาคต

### 3.5 Hero Skill Architecture

แต่ละฮีโร่มี **6 skill slots** + **Recall**:

| Slot | Key | ประเภท | หมายเหตุ |
|------|-----|--------|---------|
| Passive | I | Passive/Trigger | auto-trigger ตามเงื่อนไข |
| Normal Attack | A | BasicAttack | auto-attack, loop |
| Skill 1 | Q | Active | หลักๆ ใช้บ่อย |
| Skill 2 | W | Active | |
| Skill 3 | E | Active | |
| Skill 4 | R | Ultimate | cooldown สูง, powerful |
| Recall | — | Special | teleport กลับฐาน |

**Skill Rank:** แต่ละสกิลมี rank แยกกัน — ดูสูตรใน Actor System (C1 link)

### 3.6 Skill Patterns (จากการวิเคราะห์โค้ด)

ฮีโร่แต่ละตัวใช้ pattern หนึ่งหรือหลาย pattern รวมกัน:

#### Pattern 1: Simple Damage + Effect (Athena)
```
กด Q → Animation → Release → ApplyDamage + RunStatusEffect
```
- เหมาะกับฮีโร่ Low difficulty
- ตรงไปตรงมา ไม่มี state เพิ่มเติม

#### Pattern 2: Stack System (Artemis Q)
```
Normal attack hit → Stack +1 (max N stacks)
→ Stack ค้างอยู่ T วินาที แล้ว decay
→ กด Q เมื่อ max stack → Empowered release (bonus damage + effect)
```
- สร้าง rhythm ระหว่าง auto-attack กับ skill usage
- Stacks แต่ละอันมี duration แยกกัน

#### Pattern 3: Charge System (Cupid Q)
```
กด Q ค้าง → Range เพิ่มขึ้นตามเวลา (จนถึง max)
→ MoveSpeed ลดระหว่าง charge
→ ปล่อย Q → ยิง projectile ตาม range ที่ charge ได้
```
- Tradeoff: อยู่นิ่งนานขึ้น ได้ range มากขึ้น
- Visual indicator แสดง range ปัจจุบัน

#### Pattern 4: Stance System (Merlin)
```
ฮีโร่มีหลาย "stance" (Fire/Ice/Dark)
→ แต่ละ stance เปลี่ยน behavior ของ Q/W/R
→ Passive นับจำนวน cast → ทุก 3 cast = bonus damage
```
- ความซับซ้อนสูง — ผู้เล่นต้องเลือก stance ให้เหมาะสถานการณ์
- High difficulty

#### Pattern 5: Invulnerability Window (Hattori Hanzo E)
```
กด E → Apply Immortal + Freeze (CC immune) บน self
→ Multi-hit AoE ซ้ำ N ครั้งใน duration
→ Immortal หมด → กลับสู่ปกติ
```
- Window ที่ใช้รับ damage ไม่ได้ = reward for timing
- Hit count dictionary ป้องกัน hit เดิมซ้ำ

### 3.7 Resource Types (CostType)

| Type | คำอธิบาย |
|------|---------|
| **Mana** | resource พื้นฐาน regen ช้า |
| **Energy** | regen เร็ว cap ต่ำ ไม่ต้องซื้อ item เพิ่ม |
| **Health** | ใช้ HP แทน resource |

> CostType กำหนดว่า `Cost` ของสกิลหักจาก resource ไหน

### 3.8 Inventory System

ActorAvatar มี `NetworkHeroInventory` ที่ไม่มีใน ActorMonster:

**Slots:**
- **6 item slots** ปกติ
- **1 special slot** (stackable สูงสุด 2)

**Gold:**
- เก็บไว้ใน `CurrentGold`
- แสดงในรูปแบบ "X.XXk" ถ้า ≥ 1000

**Item Actions:**
| Action | เงื่อนไข |
|--------|---------|
| **Buy** | `IsAtBase = true`, มีทองพอ, มีช่องว่าง |
| **Sell** | มีไอเทมในช่อง → รับทองคืน SellPricePercent% |
| **Undo** | ยกเลิก purchase ล่าสุด (ระหว่างอยู่ที่ shop) |
| **Use** | Active items — trigger item skill |

**5 Item Skill Slots:** สำหรับ consumable/active items แยกจาก item slots

### 3.9 Recall System

| ค่า | รายละเอียด |
|-----|-----------|
| **Cast time** | 8 วินาที |
| **Cooldown** | TickTimer (รีเซ็ตเมื่อ recall สำเร็จ) |
| **Destination** | random spawn point ที่ base ของทีม |
| **VFX** | particle ที่จุด start + particle ที่ base |
| **UI** | countdown timer แสดงขณะ recall |

**Recall จะถูกยกเลิกเมื่อ:**
- รับ damage (SetGetHit)
- เริ่มใช้ item
- โดน Mind Control หรือ Fear
- กด right-click เพื่อเดิน

### 3.10 Buyback System

| ค่า | รายละเอียด |
|-----|-----------|
| **ราคา** | 10 ทอง (placeholder — ยังไม่ได้ออกแบบสูตรจริง) |
| **Cooldown** | `BuyBackCooldown` timer |
| **เงื่อนไข** | ตายแล้ว + BuyBackCooldown หมด + มีทองพอ |

> ⚠️ **TODO**: ออกแบบสูตร Buyback cost จริง (โดยทั่วไป MOBA คิดตาม gold ที่หาได้ หรือ flat rate ตาม time)

### 3.11 Cosmetics System

**Skin Tiers:**
| Tier | ระดับ |
|------|-------|
| **Common** | ฟรี หรือราคาต่ำ |
| **Rare** | Mid-tier |
| **Ultimate** | High-tier visual overhaul |
| **Legendary** | สูงสุด |

**แต่ละ Skin ประกอบด้วย:**
- Model 3D แยก (rigged)
- VFX ทุก skill ทุก state (Enter/Release/Hit)
- Projectile models
- Sound overrides (optional)

**SkinWeapon:** skin อาวุธแยกจาก body skin — เลือก combination ได้อิสระ

### 3.12 Hero vs Monster Differences

| ความสามารถ | ActorAvatar (Hero) | ActorMonster |
|------------|-------------------|-------------|
| Inventory | ✅ 6 slots + special | ❌ |
| Recall | ✅ 8 วินาที | ❌ |
| Buyback | ✅ | ❌ |
| IsAtBase detection | ✅ | ❌ |
| Skin system | ✅ full cosmetics | ❌ static model |
| Draft/role selection | ✅ | ❌ |
| Tower aggro tracking | ✅ IsTowerAttacking | ❌ |
| Hero combat tracking | ✅ IsAttackingHero | ❌ |
| HP/MP regen (RegenerateAttributes) | ✅ | simplified |

## 4. Formulas

### 4.1 Recall

```
recall_duration = 8 วินาที (hardcoded)
cancel_conditions = {damage_taken, item_use, mind_control, fear, movement_command}
destination = random(base_spawn_points[team])
```

### 4.2 Buyback Cost

```
// ปัจจุบัน (placeholder)
buyback_cost = 10

// TODO: สูตรจริงยังไม่ได้ออกแบบ
// ตัวอย่างแนวทาง: buyback_cost = base_cost + (gold_earned × ratio)
```

### 4.3 Skill Rank Cap (จาก Actor System)

```
Q/W/E max rank = ceil(level / 2)
R max rank     = floor(level / 6)  // สูงสุด 3
```

### 4.4 Stack Decay (Artemis pattern)

```
stack_duration = T วินาที (จาก AbilityData)
แต่ละ stack มี TickTimer แยกกัน
stack ตาย = TickTimer expired → CurrentStack -= 1
max_stack → ArtemisQState.MaxStack → trigger empower ได้
```

## 5. Edge Cases

| สถานการณ์ | พฤติกรรม |
|-----------|---------|
| Recall + รับ damage | Recall ยกเลิก (IsCancelRecall = true) |
| Recall + Mind Control | Recall ยกเลิก ทันที |
| Recall สำเร็จ → spawn ที่ base | random spawn point ของทีม |
| Inventory เต็ม (6 slots) + ซื้อไอเทม | ซื้อไม่ได้ — IsInventoryFull = true |
| Special slot เต็ม (2 stacks) | ซื้อไม่ได้ — IsSpecialSlotFull = true |
| Undo หลังออกจาก base | ไม่สามารถ undo ได้ |
| Buyback cooldown ยังไม่หมด | ปุ่ม buyback disabled |
| ฮีโร่ secondary role | ใช้สำหรับ filter ใน hero select UI, ไม่มีผลต่อ gameplay |
| IsEnable = false ใน CBSUnit | ไม่แสดงใน hero select, ใช้ในแมตช์ไม่ได้ |
| Skin ID ไม่พบ | ใช้ default skin (GetSkinDefault()) |
| สกิลถูก cancel ระหว่าง charge | SubCooldown รีเซ็ต, animation หยุด |

## 6. Dependencies

| ระบบ | ความสัมพันธ์ |
|------|-------------|
| **Actor System (F1)** | ActorAvatar extends Actor — stats, states, networking |
| **Combat & Skills (C1)** | ← ฮีโร่ใช้ damage pipeline, skill state machine ของ C1 |
| **Data/Config (F4)** | CBSUnit (stats), AvatarObject (prefab/visual) |
| **Gold Economy (C4)** | ← Inventory ใช้ CurrentGold สำหรับ buy/sell/buyback |
| **Level/XP System (C5)** | ← Skill rank unlock ขึ้นกับ level |
| **Item System (FT1)** | ← NetworkHeroInventory เก็บและใช้ไอเทม |
| **Matchmaking (FT6)** | ← ใช้ hero/role ที่เลือกสำหรับ team composition |
| **Fog of War (FT4)** | ← SightRange, DayVisionRange, NightVisionRange |
| **Customization (M3)** | ← SkinSystem โหลด skin ตาม SkinId/SkinWeaponId |

## 7. Tuning Knobs

| ค่า | ที่เก็บ | ผลกระทบ |
|-----|--------|---------|
| ค่า stats ทุกตัวต่อฮีโร่ | CBSUnit บน PlayFab | สมดุลรายตัว |
| Recall duration (8s) | Code constant | ยิ่งสั้น recall ยิ่งแข็งแกร่ง |
| Buyback cost (10) | Code constant | ต้องออกแบบสูตรจริง |
| Max inventory slots (6) | Code constant | เพิ่ม = ไอเทมได้มากขึ้น |
| Stack duration ต่อ pattern | CBSAbility config | สมดุล stack-based heroes |
| Charge max time ต่อสกิล | CBSAbility config | สมดุล charge-based skills |
| Skin tier (เกี่ยวกับ monetization) | SkinObject config | ไม่กระทบ gameplay |
| Complexity ratings | CBSUnit | แค่ UI — ไม่กระทบ mechanics |

## 8. Acceptance Criteria

| # | เกณฑ์ | วิธีทดสอบ |
|---|-------|----------|
| 1 | ฮีโร่ spawn พร้อม stats ถูกต้องจาก CBSUnit | Integration: spawn → เทียบ HP/AD/Armor กับ config |
| 2 | Recall ใช้เวลา 8 วินาที | Integration: กด recall → จับเวลา |
| 3 | Recall ยกเลิกเมื่อรับ damage | Integration: recall → ตี → recall หยุด |
| 4 | Inventory block buy เมื่อเต็ม | Unit: fill 6 slots → buy → reject |
| 5 | Buy/Sell/Undo อัปเดต gold ถูกต้อง | Unit: buy 100g item → gold -100, sell → gold +SellPricePercent% |
| 6 | Skill rank unlock ตาม level | Integration: level up → ตรวจ MaxRank Q/R เพิ่ม |
| 7 | Skin โหลดถูกต้องตาม SkinId | Integration: spawn hero + skin → ตรวจ model/VFX |
| 8 | Skin default โหลดเมื่อ SkinId ไม่พบ | Unit: invalid skinId → GetSkinDefault() |
| 9 | Role selection บันทึก Primary + Secondary | UI test: เลือก role → ตรวจ KEY_ROLEFRIST / KEY_ROLESECOND |
| 10 | IsEnable = false → ไม่แสดงใน hero select | Integration: disable hero → ตรวจ hero select list |
| 11 | Stack decay ทำงานถูกต้อง | Integration: สร้าง stack → รอ → ตรวจ stack ลด |
| 12 | Charge range scale ตามเวลา | Integration: hold charge → ตรวจ RangeCharge เพิ่ม |
| 13 | Immortal window ระหว่างสกิล Hattori E | Integration: cast E → ตี → ตรวจไม่รับ damage |
| 14 | Buyback ใช้งานได้เมื่อ cooldown หมด | Integration: ตาย → รอ cooldown → buyback |
| 15 | Network sync hero state ถูกต้อง | Integration: เปลี่ยน state ฝั่ง server → client เห็นตรง |
