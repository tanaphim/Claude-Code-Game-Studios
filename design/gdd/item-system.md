# Item System — Game Design Document

**System ID**: FT1
**Version**: 0.2.0
**Status**: Draft — Partially Implemented
**Last Updated**: 2026-04-03
**Dependencies**: Actor System (F1), Combat & Skills System (C1), Gold Economy (C3), Data-Config System (F3)

---

## 1. Overview

ระบบไอเทมให้ผู้เล่นซื้อและจัดการอุปกรณ์ในเกมผ่านร้านค้าในแผนที่ สามารถซื้อได้เฉพาะเมื่ออยู่ที่ฐาน ไอเทมแบ่งเป็น 7 ระดับ (Starter → Mythic) มีระบบสูตรประกอบ (Recipe) ที่ช่วยลดราคาเมื่อมีชิ้นส่วนในคลัง ไอเทมแต่ละชิ้นมอบค่าสถิติและ Effect พิเศษแก่ Hero ผู้ถือ ทั้งแบบ Passive (ทำงานอัตโนมัติ) และ Active (ต้องกดใช้)

---

## 2. Player Fantasy

ผู้เล่นรู้สึกถึงการเติบโตอย่างต่อเนื่องตลอด Match — เริ่มต้นด้วยไอเทมราคาถูก สะสมทอง ประกอบสูตรทีละชิ้น จนครบ Build ที่วางแผนไว้ การเลือก Build ที่ตอบสนองต่อสถานการณ์ (ศัตรูสาย Physical / Magic / Hybrid) คือทักษะสำคัญที่แยกผู้เล่นระดับสูงออกจากระดับทั่วไป

---

## 3. Detailed Rules

### 3.1 โครงสร้าง Inventory

Hero มี Slot สำหรับไอเทมทั้งหมด **7 ช่อง**:

| Slot | ประเภท | รายละเอียด |
|------|--------|-----------|
| 0–5 | Regular Slot | ไอเทมทั่วไป 6 ช่อง |
| 6 | Special Slot | สำหรับไอเทม Stackable พิเศษ (Ward / Consumable) |

- Inventory "เต็ม" เมื่อ Slot 0–5 ครบทั้ง 6 ช่อง (Slot 6 ยังรับได้)
- Special Slot รับ Stack ได้สูงสุด **2** ชิ้น
- Item Skill ถูกสร้างอัตโนมัติเมื่อสวมใส่ไอเทม (7 Item Skill Slot รวม Special Slot)

### 3.2 ระดับไอเทม (ItemType)

| ระดับ | คำอธิบาย | กฎพิเศษ |
|-------|---------|---------|
| **Starter** | ไอเทมราคาถูกสุด เริ่มต้นเกม | — |
| **Potion** | Consumable ใช้แล้วหมด | ใช้ได้เฉพาะ**นอก**ฐาน; สูตร Refillable และ Non-refillable ไม่สามารถมีพร้อมกัน |
| **Boots** | เพิ่ม Move Speed | มีได้ **1 ชิ้น** เท่านั้น (รวม Tier 1+2); Tier 2 ต้องมี Tier 1 ในคลังก่อน |
| **Basic** | ไอเทมต่อสู้ทั่วไป | — |
| **Epic** | ไอเทม Rare | — |
| **Legendary** | ไอเทมหายากมาก | — |
| **Mythic** | สูงสุด มี Effect เฉพาะตัว | มีได้ **1 ชิ้น** ต่อ Hero เท่านั้น |

### 3.3 ระบบสูตรประกอบ (Recipe)

ไอเทมระดับสูงสามารถประกอบจากชิ้นส่วนที่มีใน Inventory:

```
ราคารวม (Composite) = ราคาไอเทมสำเร็จ - ราคาชิ้นส่วนที่มีอยู่
```

**ขั้นตอนการซื้อไอเทมที่มีสูตร**:
1. ระบบตรวจสอบว่ามีชิ้นส่วน Recipe ใน Inventory หรือไม่
2. ถ้ามี → ลบชิ้นส่วนออก, คิดแค่ส่วนต่าง
3. ถ้าไม่มี → คิดราคาเต็ม (ซื้อเป็น base item)
4. สูตรสามารถซ้อนได้ (ชิ้นส่วนที่ประกอบจากชิ้นส่วนอื่นอีกที)

### 3.4 ระบบ ItemAction

| Action | พฤติกรรม |
|--------|---------|
| **Passive** | ทำงานอัตโนมัติตลอดเวลาที่ถือครอง |
| **Active** | ต้องกดใช้; มี Cooldown |
| **Replace** | วางไอเทมบนพื้นที่แผนที่ (เช่น Ward) |
| **None** | ไม่มี Action พิเศษ |

### 3.5 กฎการซื้อ/ขาย

**ซื้อ (Buy)**:
- ต้องอยู่ที่ฐานเท่านั้น
- ตรวจสอบทองเพียงพอ → Validation → หักทอง → เพิ่มใน Slot
- หาก Inventory เต็ม (Slot 0–5 ครบ) → รับเฉพาะไอเทม IsSpecialSlot

**ขาย (Sell)**:
- ต้องอยู่ที่ฐานเท่านั้น
- ราคาขาย = `floor(ราคาซื้อ × SellPricePercent / 100)`
- ไอเทม Stack: ลด Stack 1 ชิ้น (ไม่ Clear Slot)
- ไอเทมทั่วไป: Clear Slot ทั้งหมด

**Undo**:
- ระบบเก็บประวัติการซื้อ/ขายไว้ (History)
- กด Undo → ย้อนรายการล่าสุด 1 รายการ
- Undo Buy → ขายคืน, Undo Sell → ซื้อคืน
- History ถูกล้างเมื่อออกจากฐาน

### 3.6 ระบบ Stack & Refill

- **IsStack**: ไอเทมสะสมในช่องเดียวได้ (เช่น Potion)
- **IsRefillable**: Charge คืนค่าอัตโนมัติ
  - ที่ฐาน: Refill ทันที
  - ในสนาม: มี `RefillCoolDown` (วินาที) ระหว่าง Charge
- Special Slot: Stack สูงสุด 2 เสมอ

### 3.7 Mythic Passive Bonus

> ⚠️ **สถานะ: Schema-only / Unimplemented** (ตรวจสอบ 2026-05-08, S4-06)
> Schema มีจริงใน `ItemObject.cs` แต่ **ไม่มี runtime path ใดอ่านค่า** — grep
> ทั้ง `Assets/GameScripts` พบเฉพาะการ declare ของ field, ไม่มี read site.
> ต้องสร้าง story ใน Sprint 005+ เพื่อ wire-up ก่อนใช้งานได้จริง

#### Schema (`ItemObject.cs`)

```csharp
// ItemObject.cs:24
public ItemEffectMythicPattern[] MythicItemEffect;

// ItemObject.cs:67-72
public struct ItemEffectMythicPattern
{
    public ItemType  ItemType;          // trigger condition: นับไอเทมประเภทนี้ใน inventory
    public TargetType TargetType;        // เป้าหมายที่จะ apply bonus (Hero / Monster / Tower / ...)
    public StatValue[] MythicItemEffect; // Stats ที่จะให้เป็น bonus
}

public struct StatValue
{
    public StatKey      Key;            // เช่น attack_damage, hp, attack_speed
    public ModifierType ModifierType;   // Flat | Percent
    public float        Value;
}
```

#### `TargetType` enum (`Commons/Enums/TargetType.cs`)

| Value | Meaning |
|-------|---------|
| `None`(0), `Hero`(1), `Monster`(2), `Tower`(4), `Dummy`(5), `Summoner`(6), `Item`(7), `AFK` | object class หรือ context filter |

#### Intended semantics (จากชื่อ field — ยังต้องยืนยันเมื่อ implement)

`ItemEffectMythicPattern` หนึ่งรายการ = "เมื่อมีไอเทมประเภท `ItemType` อยู่ใน
inventory + (filter ตาม `TargetType` ของ damage target หรือ owner role) → apply
`MythicItemEffect[]` เป็น bonus stats เพิ่มเติม"

ตัวอย่างที่ผู้ออกแบบน่าจะตั้งใจ (ตีความจาก schema):
- "+10 attack_damage ต่อ ไอเทม Epic ที่ถืออยู่" → `ItemType=Epic`, `TargetType=Hero`,
  `MythicItemEffect=[{attack_damage, Flat, +10}]`
- "+5% damage ต่อ Monster เมื่อมีไอเทม Legendary 2 ชิ้นขึ้นไป" → ต้อง
  threshold logic เพิ่ม (schema ยังไม่มี field สำหรับ threshold)

> ⚠️ Schema ปัจจุบัน**ไม่มี** field สำหรับ threshold/scaling — ตอน wire-up
> ต้องตัดสินใจว่าจะนับเป็น "ต่อชิ้น" (linear) หรือ "ตั้งแต่ N ชิ้น" (step) แล้ว
> บันทึกใน ADR

#### Hardcoded constraint ที่ implement แล้ว

`NetworkHeroInventory.cs:208-210, 1054-1056`
```csharp
public bool HasMythic
    => GetItems.Count(x => x.itemType == ItemType.Mythic) > 0;

// AvailableToPurchase()
case ItemType.Mythic: return !HasMythic;
```

= "1 Mythic ต่อ Hero" enforce แล้ว (ผ่าน purchase gate) — แต่ Bonus path ยังไม่ wire

### 3.8 Item Animation System

ไอเทม Active แต่ละชิ้นสามารถกำหนด Animation ที่ Hero เล่นขณะใช้งานได้ ผ่าน field `ItemAnimationType` ใน `CBSAbility`

#### ItemAnimationType Enum

| ค่า | Animator State | ตัวอย่างไอเทม |
|-----|---------------|-------------|
| `Default` | `Item_Perform` | ไอเทมทั่วไปที่ไม่มีท่าพิเศษ |
| `Recall` | `Item_Recall_Perform` | ใบวาร์ป (Teleport Scroll) |
| `Consume` | `Item_Consume_Perform` | ขวดยา / Potion |
| `Spell` | `Item_Spell_Perform` | คัมภีร์เวทย์ / Spellbook |
| `Attack` | `Item_Attack_Perform` | ดาบยาว / Active Weapon |

#### Animation Flow

```
OnPerform() ใน ActorCombatAction
  → Actor.Animation.Running.CurrentItemAnimationType = AbilityData.ItemAnimationType
  → RunSkill(SkillKey.Item, SkillState.Perform)
      → ResolveAnimStateHash(key, state)
          ถ้า Default  → hash("Item_Perform")
          ถ้าไม่ใช่   → hash("Item_{AnimationType}_Perform")
      → Animator.Play(hash)
  → Animation Clip เล่นท่าทาง
  → Animation Event: Skill_Item_{Type}_Perform(0)
      → StateRelease(SkillKey.Item, SkillState.Perform)
  → OnStateRelease() → ผล Effect ของไอเทมทำงาน
```

#### Animation Event Methods (AnimationEvent.cs)

แต่ละ animator state ต้องมี Animation Event เรียก method ที่ตรงกัน:

| Animator State | Animation Event Method |
|---------------|----------------------|
| `Item_Perform` | `Skill_Item_Perform(0)` |
| `Item_Recall_Perform` | `Skill_Item_Recall_Perform(0)` |
| `Item_Consume_Perform` | `Skill_Item_Consume_Perform(0)` |
| `Item_Spell_Perform` | `Skill_Item_Spell_Perform(0)` |
| `Item_Attack_Perform` | `Skill_Item_Attack_Perform(0)` |

> ทุก variant method route ไปที่ `StateRelease(SkillKey.Item, SkillState.Perform)` เหมือนกัน

#### การเพิ่ม Type ใหม่ในอนาคต

1. เพิ่ม value ใน `ItemAnimationType` enum (`ItemType.cs`)
2. เพิ่ม method ใน `AnimationEvent.cs`: `Skill_Item_{New}_Perform(int param)`
3. สร้าง Animator State `Item_{New}_Perform` ใน AnimatorController ของแต่ละ Hero
4. ตั้ง `ItemAnimationType` ใน CBS Ability data ของไอเทม

---

## 4. Formulas

### ราคาขาย
```
SellGold = floor(PurchasePrice × SellPricePercent / 100)
```

### ราคาประกอบ (Composite)
```
CraftPrice = ItemFullPrice - Σ(ComponentPrice สำหรับชิ้นส่วนที่มีใน Inventory)
```
- ถ้า CraftPrice = 0 → ไม่มีสูตร (base item)
- ถ้า CraftPrice > 0 → มีสูตร, ลบชิ้นส่วนออกก่อนวาง

### การแปลงค่าสถิติ

| Stat | ModifierType = Flat | ModifierType = Percent |
|------|---------------------|----------------------|
| attack_speed | ÷ 100 (เสมอ) | ÷ 100 (เสมอ) |
| move_speed | ÷ 100 (เสมอ) | ÷ 100 (เสมอ) |
| hp_regen, mp_regen | ใช้ตรงๆ | ÷ 100 |
| สถิติอื่นๆ | ใช้ตรงๆ | ÷ 100 |

> ⚠️ **หมายเหตุ**: attack_speed และ move_speed ถูก Hardcode ให้ ÷100 เสมอ โดยไม่คำนึง ModifierType ในโค้ด

### Mythic Passive Bonus (proposed — unimplemented)

> ⚠️ Schema ตามนี้มีอยู่จริง แต่ runtime ยังไม่อ่าน ดู §3.7 Status

ตัวแปร:
- `M` = Mythic item ที่ Hero ถืออยู่ (อย่างมาก 1 ชิ้นต่อ Hero)
- `M.MythicItemEffect[]` = list of `ItemEffectMythicPattern`
- ต่อ pattern `p`:
  - `p.ItemType` = ประเภทที่ใช้นับ (เช่น `Epic`, `Legendary`)
  - `count(p.ItemType)` = จำนวนไอเทมประเภทนั้นใน inventory ของ Hero
  - `p.TargetType` = filter ของ apply context
  - `p.MythicItemEffect[]` = stats ที่จะให้

สูตร (proposed — linear interpretation):

```
สำหรับ Hero ที่ถือ Mythic ตัว M:
  สำหรับ pattern p ใน M.MythicItemEffect:
    n = count(items with itemType == p.ItemType in Hero.Inventory)
    เมื่อ context.TargetType ตรงกับ p.TargetType:
      สำหรับ stat s ใน p.MythicItemEffect:
        bonus = s.Value × n   ถ้า s.ModifierType == Flat
              = s.Value × n%  ถ้า s.ModifierType == Percent
        Actor.Trait.ApplyStat(s.Key, bonus)
```

ค่าตัวอย่าง (ปัจจุบันยังไม่มีข้อมูล CBS เนื่องจากยังไม่ wire — ใส่เพื่ออ้างอิง):

| Pattern | Result |
|---------|--------|
| `ItemType=Epic, TargetType=Hero, [{attack_damage, Flat, +10}]` × Hero ถือ Epic 3 ชิ้น | +30 attack_damage |
| `ItemType=Legendary, TargetType=Monster, [{damage, Percent, +5}]` × Hero ถือ Legendary 2 ชิ้น, ตี Monster | +10% damage ต่อ Monster |

**Open questions ก่อน implement** (ต้องตอบใน ADR Sprint 005+):

1. นับ "ต่อชิ้น" (linear) หรือ "ตั้งแต่ N ชิ้น" (step / threshold)?
2. `TargetType=Hero` หมายถึง self (Hero ที่ถือ Mythic) หรือ target ของ damage?
3. Bonus apply ตอนไหน — equip time (static) หรือ per-attack context (dynamic)?
4. Bonus stack ระหว่างหลาย Mythic pattern หรือไม่ (1 Mythic อาจมีหลาย pattern)?

---

## 5. Edge Cases

| สถานการณ์ | พฤติกรรม |
|-----------|---------|
| ซื้อไอเทมนอกฐาน | ไม่อนุญาต → ไม่มีผลใดๆ |
| Hero Role ไม่ตรงกับ `ItemObject.Positions` | **ไม่มีผล** (unimplemented) — ทุก Role ซื้อได้ทุกไอเทม; ดู §Known Issues / S4-05 |
| ใช้ Potion ที่ฐาน | ไม่อนุญาต |
| ซื้อ Mythic ชิ้นที่ 2 | ถูกบล็อกโดย AvailableToPurchase() |
| Inventory เต็ม + ซื้อ Ward | Ward ไปที่ Special Slot (Slot 6) ได้ถ้ายังไม่เต็ม |
| Observer Ward (ID: `itemingame_Observward`) | มีกฎ Hardcode — สามารถเข้า Special Slot ได้แม้ Inventory เต็ม |
| ซื้อ Boots ชิ้นที่ 2 | ถูกบล็อก ยกเว้นเป็นการ Upgrade Tier 1 → Tier 2 ที่มีอยู่ |
| Potion Refillable + Non-refillable พร้อมกัน | ถูกบล็อก — ไม่อนุญาตให้มีทั้งสองแบบพร้อมกัน |
| Recipe ชิ้นส่วนอยู่ใน Special Slot | ระบบนับ Special Slot เมื่อคำนวณ Component |
| Undo หลังออกจากฐาน | ไม่สามารถ Undo ได้ — History ถูกล้างแล้ว |
| Stack สูงสุดใน Special Slot | 2 ชิ้น Hardcode — ไม่สามารถเปลี่ยนผ่าน Config ได้ |

---

## 6. Dependencies

| ระบบ | ความสัมพันธ์ |
|------|------------|
| **Actor System (F1)** | `Actor.Trait.ApplyStat()` ใช้เพิ่ม/ลบค่า Stats จากไอเทม |
| **Combat & Skills System (C1)** | ItemEffect หลายตัวทำงานผ่าน `ActorCombatAction`; Damage pipeline ใช้ Item stats (Lethality, Pierce ฯลฯ); `OnPerform()` set `CurrentItemAnimationType` ก่อน `RunSkill()` |
| **Gold Economy (C3)** | ใช้ `Withdraw()` / `Deposit()` ระบบทอง; ราคาไอเทมมาจาก CBS |
| **Data-Config System (F3)** | ข้อมูลไอเทมทั้งหมดมาจาก `CBSItemInGame` ผ่าน MetadataService |
| **Level/XP System (C5)** | ไม่มีการ Lock ไอเทมตาม Level (ซื้อได้ทุก Tier ตั้งแต่ต้น) |
| **Hero System (C2)** | ⚠️ **Role Restriction unimplemented** — `ItemObject.Positions : Role[]` field มี แต่ `AvailableToPurchase()` ไม่ check (ดู §Known Issues / S4-05). ปัจจุบัน Hero ทุก Role ซื้อไอเทมใดก็ได้ |
| **Photon Fusion (F5)** | `NetworkHeroInventory` ใช้ NetworkArray + NetworkBehaviourId สำหรับ Sync ทุก Client |

---

## 7. Tuning Knobs

| ค่า | ที่อยู่ | ค่าปัจจุบัน | หมายเหตุ |
|-----|--------|------------|---------|
| ราคาไอเทม (Price) | CBS → CBSItemInGame.Price | ตามการตั้งค่า | หน่วยภายใน (แสดงผล ÷ 1000) |
| เปอร์เซ็นต์ขายคืน (SellPricePercent) | CBS → CBSItemInGame.SellPricePercent | 1–100 | ตั้งต่อไอเทม |
| Cooldown Refill | CBS → CBSItemInGame.RefillCoolDown | ตามการตั้งค่า | หน่วย: วินาที |
| Special Slot Max Stack | Hardcode | 2 | เปลี่ยนได้เฉพาะในโค้ด |
| จำนวน Inventory Slot | Hardcode | 7 (6+1) | เปลี่ยนได้เฉพาะในโค้ด |
| สถิติไอเทม (Key/Value/ModifierType) | CBS → CBSItemInGame lists | ตามการตั้งค่า | แก้ได้ผ่าน Dashboard |
| สูตรประกอบ (Recipe) | CBS → CBSItemInGame.Recipe | ตามการตั้งค่า | List of Component IDs |
| Effect ของไอเทม | CBS → CBSItemInGame.Effect | ตามการตั้งค่า | ≥168 Effect ที่รองรับ |
| ไอเทม Mythic ต่อ Hero | Hardcode | 1 | เปลี่ยนได้เฉพาะในโค้ด |
| Boots ต่อ Hero | Hardcode | 1 | เปลี่ยนได้เฉพาะในโค้ด |
| ItemAnimationType | CBS → CBSAbility.ItemAnimationType | Default | กำหนดต่อ Ability; ไม่ต้องตั้งถ้าใช้ท่า Default |
| Mythic Passive Bonus pattern | `ItemObject.MythicItemEffect` (SO array) | **schema-only** | ดู §3.7 — ยังไม่ wire runtime |

---

## 8. Acceptance Criteria

- [ ] ซื้อไอเทมได้เฉพาะเมื่ออยู่ที่ฐาน; นอกฐานกด Buy ไม่มีผล
- [ ] ไอเทมที่มีสูตร → ลบชิ้นส่วนออกและคิดราคาส่วนต่างถูกต้อง
- [ ] Undo ย้อนรายการล่าสุด 1 รายการ; History หายเมื่อออกจากฐาน
- [ ] Stats ของไอเทม (Flat/Percent) ถูกใช้กับ Actor ทันทีเมื่อสวมใส่/ถอด
- [x] Mythic: ซื้อชิ้นที่ 2 ถูกบล็อก (`AvailableToPurchase()` enforce)
- [ ] ⚠️ **BLOCKED** Mythic Passive Bonus คำนวณถูกต้อง — ปัจจุบัน schema-only, runtime ยังไม่ wire (ดู §3.7 / §4 Mythic Passive Bonus); ต้อง implement ใน Sprint 005+ ก่อน acceptance ผ่าน
- [ ] Boots: มีได้ 1 ชิ้น; Upgrade Tier 1→Tier 2 ได้ถ้ามี Tier 1 อยู่
- [ ] Special Slot รับ Stack สูงสุด 2; Ward ไปที่ Special Slot ได้แม้ Slot 0–5 เต็ม
- [ ] Potion ใช้ได้เฉพาะนอกฐาน; Refillable ไม่สามารถมีพร้อมกับ Non-refillable
- [ ] Item Skill ถูกสร้าง/ทำลายตาม Equip/Unequip อัตโนมัติ
- [ ] ราคาขายคืนถูกคำนวณตาม `SellPricePercent` จาก CBS
- [ ] ร้านค้าแสดงไอเทมแยกตาม Tab Type และ Role Filter ถูกต้อง
- [ ] Network Sync: การเปลี่ยน Inventory สะท้อนทุก Client ผ่าน NetworkArray
- [ ] ไอเทม Active เล่น Animation ท่าทางตาม `ItemAnimationType` ที่กำหนดใน CBS
- [ ] ใบวาร์ป (Recall) → เล่น `Item_Recall_Perform`; ขวดยา → `Item_Consume_Perform`
- [ ] Animation เสร็จ → `StateRelease` trigger Effect ของไอเทมทำงาน
- [ ] ไอเทมที่ไม่มี animation type ตก default ไปที่ `Item_Perform` โดยไม่ error

---

## Known Issues / TODO

- ⚠️ **Role Restriction** (S4-05, 2026-05-08): `ItemObject.Positions : Role[]`
  declared ใน `ItemObject.cs:21` แต่ **unimplemented**. หลักฐาน:
  (a) `NetworkHeroInventory.AvailableToPurchase()` (ll. 1045–1158) ไม่อ้าง
  `Positions` เลย — gate ใช้แค่ Money / Mythic / Boots / Epic-Legendary recipe
  / Potion / Inventory full;
  (b) UI references ทั้งหมดใน `UIInGameShopView.cs:521-526, 750, 792` ถูก
  comment-out (display only, ไม่ใช่ purchase gate);
  (c) ไม่มี read site อื่นใน `Assets/GameScripts`. ถ้าจะ enforce จริงต้อง
  Sprint 005+ story: เพิ่ม `case` ตรวจ `item.Positions.Contains(Hero.Role)` ใน
  `AvailableToPurchase()` และ surface ใน Shop UI (recommend filter)
- ⚠️ **Mythic Passive Formula** (S4-06, 2026-05-08): Schema documented ใน §3.7 + proposed formula §4. **Schema-only / unimplemented** — `ItemObject.MythicItemEffect` ไม่มี read site ใน `Assets/GameScripts`. ต้อง: (a) Sprint 005+ story สำหรับ wire-up `ApplyMythicBonus()` ใน `NetworkHeroInventory` หรือ `Actor.Trait`, (b) ADR ตอบ Open Questions §4, (c) CBS dashboard editor support สำหรับ `ItemEffectMythicPattern[]` field
- ⚠️ **attack_speed / move_speed item-bonus /100 unconditional** (S4-03, 2026-05-08):
  `NetworkHeroInventory.cs:1299-1304` หาร item bonus /100 เสมอ ไม่ว่า
  `ModifierType` จะเป็น Flat หรือ Percent → **ทั้งสองให้ผลเหมือนกัน**
  สำหรับ 2 stats นี้ (ต่างจาก stats อื่นที่ /100 เฉพาะ Percent).
  CBS scale conventions:
  - `CBSUnit.MoveSpeed` int 200..500 → /100 ใน `NetworkStat.cs:169` → game units 2.0..5.0
  - `CBSUnit.AttackSpeed` float ~0.5..2.0 (raw multiplier, **NO /100** ใน base init `NetworkStat.cs:179`)
  - Item bonus values ใน CBS เก็บเป็น ×100 ของ runtime delta (เช่น item "+30" → +0.30 runtime)
  - **Designer trap**: ค่า "30" ใน item อาจคิดว่า "+30%" แต่จริงๆ คือ +0.30 runtime
    → บน MoveSpeed base 3.5 = ~+8.6%, บน AttackSpeed base 1.0 = +30% (ตรงโดยบังเอิญ)
  - **`ModifierType.Flat` กับ `Percent` ให้ผลเหมือนกัน** สำหรับ 2 stats นี้ → label เลือกให้ตรง intent แต่ผลเหมือนกัน
  Status: **documented, not fixed** — code change กระทบ balance ของทุก item ที่ตั้งค่าไว้แล้ว;
  defer to Phase 2 หรือ balance pass. ดู R-22 ใน risk register
- ✅ **Item Animation States — VERIFIED DONE** (S4-01, 2026-05-08):
  States `Item_Recall_Perform`, `Item_Consume_Perform`, `Item_Spell_Perform`,
  `Item_Attack_Perform`, `Item_Perform` (default) ทั้ง 5 states มีอยู่แล้วใน
  shared base controller `Assets/Animations/RadiusBasicLocomotion.controller`
  (GUID `d5cada5dadda5f44db70f1faa1c641fc`). Hero override controllers ทั้งหมด
  เป็น `AnimatorOverrideController` ที่ point ไปที่ base นี้ → **inherit
  states อัตโนมัติ**. Coverage: 22/25 hero override controllers ใช้ base
  ที่ถูกต้อง
- ✅ **Item_Viable parameter — VERIFIED DONE** (S4-02, 2026-05-08):
  Bool parameter `Item_Viable` มีอยู่แล้วใน `RadiusBasicLocomotion.controller`.
  `GetViable(SkillKey.Item)` / `SetViable(SkillKey.Item, true/false)` ทำงาน
  โดยไม่ error สำหรับทุก hero ที่ override จาก base นี้
- ⚠️ **Garen variant controllers gap** (S4-01, 2026-05-08): 3 override
  controllers ใช้ `GameCreator/CompleteLocomotion.controller` (plugin) เป็น
  base — **ไม่มี item states และไม่มี `Item_Viable` parameter**:
  `GarenCompleteLocomotion`, `GarenButKingArthurCompleteLocomotion`,
  `GarenButXinZhoaCompleteLocomotion`. ชื่อแบบ "GarenBut*" สื่อว่าเป็น
  hybrid prototypes (legacy test). Action: (a) confirm ว่าไม่ใช่ production
  hero แล้วพิจารณาลบ; (b) ถ้ายังใช้ → swap base เป็น `RadiusBasicLocomotion`
  หรือ port states เข้า `CompleteLocomotion.controller`. Defer ไป Sprint 005+
