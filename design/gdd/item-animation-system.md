# Item Animation System

**System ID**: FT1-ANIM
**Version**: 1.0.0
**Status**: Implemented
**Last Updated**: 2026-04-16
**Dependencies**: Item System (FT1), Actor System (F1), Combat & Skills System (C1)

---

## 1. Overview

ระบบ Item Animation ให้ไอเทม Active แต่ละชิ้นเล่น animation ท่าทางที่แตกต่างกันเมื่อ Hero ใช้งาน
โดยใช้ Animator Layer "Item" แยกออกมาจาก Base Layer ทำให้ animation ไอเทมไม่กระทบ locomotion
และใช้ AnimatorOverrideController ให้แต่ละ Hero/Weapon สามารถ assign clip ของตัวเองได้โดยไม่ต้องแก้ base state machine

---

## 2. Player Fantasy

Hero แสดงท่าทางที่สมจริงและเหมาะสมกับไอเทมที่ใช้ — ดื่มยาด้วยท่า consume, ใช้ใบวาร์ปด้วยท่า recall,
ฟาดดาบด้วยท่า attack — ทำให้ผู้เล่นรู้สึกว่าไอเทมแต่ละชิ้นมีน้ำหนักและ identity ของตัวเอง

---

## 3. Detailed Rules

### 3.1 Architecture Overview

```
RadiusBasicLocomotion.controller  (base, shared ทุก Hero)
  └── Layer: "Item" (Override, weight=1)
        ├── Empty              ← default state
        ├── Item_StartCast     ← cast windup (ถ้า CastTime > 0)
        ├── Item_Casting       ← loop ระหว่าง cast (ถ้ามี clip)
        ├── Item_Perform       ← default perform
        ├── Item_Recall_Perform
        ├── Item_Consume_Perform
        ├── Item_Spell_Perform
        └── Item_Attack_Perform

{Hero}CompleteLocomotion.overrideController  (per-hero)
  └── override: Item_* states → hero-specific .anim clips

{Hero}_{Weapon}.overrideController  (per-weapon, optional)
  └── override: Item_* states → weapon-specific .anim clips
```

### 3.2 ItemAnimationType Enum

กำหนดใน `ItemType.cs` — ระบุว่าไอเทมชิ้นนี้จะเล่น state ไหนใน Animator:

| Value | Animator State (Perform) | ตัวอย่างการใช้งาน |
|-------|--------------------------|-----------------|
| `Default` | `Item_Perform` | ไอเทมทั่วไปที่ไม่มีท่าพิเศษ |
| `Recall` | `Item_Recall_Perform` | ใบวาร์ป (Teleport Scroll) |
| `Consume` | `Item_Consume_Perform` | ขวดยา, Potion |
| `Spell` | `Item_Spell_Perform` | คัมภีร์เวทย์, Spellbook |
| `Attack` | `Item_Attack_Perform` | ดาบ Active, อาวุธ Active |

> **StartCast และ Casting** ใช้ state เดิมทุก type (`Item_StartCast`, `Item_Casting`) — ไม่มี type prefix

### 3.3 Animation Flow

```
CBS Dashboard: CBSAbility.ItemAnimationType = Recall

ActorCombatAction.OnPerform()
  → Actor.Animation.Running.CurrentItemAnimationType = AbilityData.ItemAnimationType
  → RunSkill(SkillKey.Item, SkillState.Perform)
      → ResolveAnimStateHash(Item, Perform)
          → Animator.StringToHash("Item_Recall_Perform")
      → Animator.Play(hash, layer: "Item")
      → WeaponAnimator.Play(hash, layer: "Item")   ← weapon animator ด้วย

Animation Clip เล่น
  → Animation Event fires: Skill_Item_Recall_Perform(0)
      → StateRelease(SkillKey.Item, SkillState.Perform)
  → ActorCombatAction.OnStateRelease()
      → Item effect ทำงาน
```

### 3.4 State Transitions

| State | Exit Condition |
|-------|---------------|
| `Item_StartCast` | exit-time (clip จบ) → `Item_Casting` |
| `Item_Casting` | loop — exit ผ่าน code (`Animator.Play`) เมื่อ CastTime หมด |
| `Item_*_Perform` | exit-time (clip จบ) → `Empty` |
| `Item_*_Perform` | `Moving=true` → `Empty` ทันที (interrupt) |

### 3.5 Animator Parameter

| Parameter | Type | ใช้ทำอะไร |
|-----------|------|----------|
| `Item_Viable` | bool | enable/disable Item skill slot ใน UI และ ability validation |

### 3.6 WeaponAnimator Sync

เมื่อ `RunSkill` ถูกเรียก จะ play hash เดิมบน `WeaponAnimator` ด้วยเสมอ
ทำให้ weapon mesh เล่น animation ตรงกับ body — **weapon override controller ต้องมี Item_* entries ด้วย**

---

## 4. Formulas

ไม่มีสูตรคำนวณ — ระบบนี้เป็น lookup และ string hash:

```
animStateHash = Animator.StringToHash(stateName)

stateName =
  ถ้า StartCast/Casting               → "Item_{state}"
  ถ้า Perform + type != Default        → "Item_{ItemAnimationType}_{state}"
  ถ้า Perform + type == Default        → m_KeyStateDict[Item][state]  (= "Item_Perform")
```

---

## 5. Edge Cases

| สถานการณ์ | พฤติกรรม |
|-----------|---------|
| `ItemAnimationType` ไม่ได้ตั้งใน CBS | ค่า default = `Default` → เล่น `Item_Perform` |
| Override controller ไม่มี Item clip | Animator.Play() ล้มเหลว silently — ไม่ crash, ไม่มี animation |
| Hero เคลื่อนที่ระหว่าง Perform | `Moving=true` → transition ออกทันที, effect ยังทำงานตามปกติ (state release จาก code ไม่ใช่ event) |
| `CastTime = 0` | ข้าม StartCast/Casting ไปที่ Perform โดยตรง |
| `CastTime > 0` แต่ clip `Item_Casting` ไม่มี | ข้าม Casting → ไปที่ Perform โดยตรง |
| Weapon controller ไม่มี Item entries | Weapon animator ค้างที่ state ก่อนหน้า — ไม่ crash |

---

## 6. Dependencies

| ระบบ | ความสัมพันธ์ |
|------|------------|
| **Item System (FT1)** | `CBSAbility.ItemAnimationType` เป็น field ใน item data — ระบบนี้ consume ค่านั้น |
| **Actor System (F1)** | `IActorAnimation.CurrentItemAnimationType` เป็น property บน animation component |
| **Combat & Skills (C1)** | `ActorCombatAction.OnPerform()` set CurrentItemAnimationType ก่อน RunSkill ทุกครั้ง |
| **Photon Fusion (F5)** | `AnimatorState.RequestSync()` sync animator state ผ่าน network เมื่อ HasStateAuthority |

---

## 7. Tuning Knobs

| ค่า | ที่อยู่ | ค่าปัจจุบัน | หมายเหตุ |
|-----|--------|------------|---------|
| `ItemAnimationType` | CBS → CBSAbility | `Default` | กำหนดต่อ ability ผ่าน PlayFab Dashboard |
| Item Layer weight | `RadiusBasicLocomotion.controller` → Item layer | 1.0 | Override layer — 1.0 = full override |
| exit-time ของ Perform states | Animator transition | 1.0 (normalized) | clip จบพอดี → กลับ Empty |
| Moving interrupt | Animator transition condition | `Moving = true` | ปรับ threshold ได้ใน Animator |

---

## 8. Acceptance Criteria

- [ ] ไอเทม Active ที่ตั้ง `ItemAnimationType = Recall` เล่น `Item_Recall_Perform` ไม่ใช่ `Item_Perform`
- [ ] ไอเทม Active ที่ไม่ตั้ง type เล่น `Item_Perform` โดยไม่ error
- [ ] Weapon animator เล่น clip เดียวกับ body animator พร้อมกัน
- [ ] Animation Event fires → item effect ทำงาน
- [ ] Hero เคลื่อนที่ระหว่าง Perform → animation หยุดทันที
- [ ] `Item_StartCast` → `Item_Casting` (loop) → `Item_Perform` ทำงานตามลำดับเมื่อ `CastTime > 0`

---

## Guide: Programmer — ตั้งค่า ItemAnimationType ใน CBS

### ขั้นตอน

1. เข้า PlayFab Dashboard → Catalog → Items
2. เลือกไอเทมที่ต้องการ → แก้ `CustomData` → `CBSAbility`
3. ตั้ง field `ItemAnimationType`:

```json
{
  "ItemAnimationType": 1
}
```

| ค่า int | enum value |
|---------|-----------|
| 0 | Default |
| 1 | Recall |
| 2 | Consume |
| 3 | Spell |
| 4 | Attack |

### ตัวอย่าง: ใบวาร์ป

```json
{
  "SkillKey": "Item",
  "CastTime": 3.0,
  "ItemAnimationType": 1
}
```

> `CastTime = 3.0` → system เล่น `Item_StartCast` → `Item_Casting` (loop 3 วินาที) → `Item_Recall_Perform`

### ตัวอย่าง: ขวดยา

```json
{
  "SkillKey": "Item",
  "CastTime": 0,
  "ItemAnimationType": 2
}
```

> `CastTime = 0` → ข้าม cast phase → เล่น `Item_Consume_Perform` ทันที

---

## Guide: Programmer — เพิ่ม ItemAnimationType ใหม่

1. เพิ่ม value ใน `ItemType.cs`:

```csharp
public enum ItemAnimationType
{
    Default, Recall, Consume, Spell, Attack,
    Throw,  // ← ใหม่
}
```

2. เพิ่ม Animation Event method ใน `AnimationEvent.cs`:

```csharp
public void Skill_Item_Throw_Perform(int param) { Skill_Item_Perform(param); }
```

3. สร้าง Animator State `Item_Throw_Perform` ใน `RadiusBasicLocomotion.controller`
   ผ่าน menu: **Delta → Hero Animator Setup → Add Item States to Base Controller**
   (ต้องเพิ่ม state name ใน `HeroAnimatorSetup.cs → ItemStateNames` ก่อน)

4. สร้าง clip และ override entries ผ่าน script (ดู Guide: Animator ด้านล่าง)

---

## Guide: Animator — Asset Structure

### Hero Body Controller

```
Assets/Animations/Hero/
  {Hero}CompleteLocomotion.overrideController   ← hero body override
  
Assets/Game_Asset/Character/Avatar/{Hero}/Animation/
  Item_Perform.anim
  Item_Recall_Perform.anim
  Item_Consume_Perform.anim
  Item_Spell_Perform.anim
  Item_Attack_Perform.anim
  Item_StartCast.anim
  Item_Casting.anim
```

### Hero Weapon Controller

```
Assets/Animations/Weapon/
  {Hero}_{Weapon}.overrideController            ← weapon override

Assets/Game_Asset/Character/Avatar/{Hero}/Animation/{WeaponFolder}/
  Item_Perform.anim
  Item_Recall_Perform.anim
  ... (7 clips เช่นกัน)
```

### Heroes ที่มี override controllers

**Body** (`Assets/Animations/Hero/`):

| Hero | Controller file |
|------|----------------|
| Anansi | `AnansiCompleteLocomotion.overrideController` |
| Aphrodite | `AphroditeCompleteLocomotion.overrideController` |
| Artemis | `ArtemisCompleteLocomotion.overrideController` |
| Athena | `AthenaCompleteLocomotion.overrideController` |
| Cupid | `CupidCompleteLocomotion.overrideController` |
| GuanYu | `GuanYuCompleteLocomotion.overrideController` |
| Hattori | `HattoriCompleteLocomotion.overrideController` |
| Hercules | `HerculesCompleteLocomotion.overrideController` |
| Horus | `HorusCompleteLocomotion.overrideController` |
| Hypnos | `HypnosCompleteLocomotion.overrideController` |
| KingArthur | `KingArthurCompleteLocomotion.overrideController` |
| Koschei | `KoscheiCompleteLocomotion.overrideController` |
| Lanceslot | `LanceslotCompleteLocomotion.overrideController` |
| Mehmed | `MehmedCompleteLocomotion.overrideController` |
| Merlin | `MerlinCompleteLocomotion.overrideController` |
| Napoleon | `NapoleonCompleteLocomotion.overrideController` |
| Skadi | `SkadiCompleteLocomotion.overrideController` |
| Volund | `VolundCompleteLocomotion.overrideController` |
| WildBill | `WildBillCompleteLocomotion.overrideController` |
| WooChi | `WooChiCompleteLocomotion.overrideController` |

**Weapon** (`Assets/Animations/Weapon/`):
Anansi, Aphrodite, Artemis, Athena, Cupid, GuanYu, Hattori, Hercules, Hypnos, KingArthur, Lancelot, Merlin, Napoleon, Skadi, Volund, WildBill, WooChi

---

## Guide: Animator — สร้าง Item Clip สำหรับ Hero ใหม่

### เงื่อนไข
Hero ที่จะ join ระบบต้องใช้ `RadiusBasicLocomotion.controller` เป็น base ใน override controller

### ขั้นตอน

**1. เตรียม clip ต้นแบบ**

Duplicate clip `Skill_A_Attack1` ของ hero (อยู่ใน `Game_Asset/Character/Avatar/{Hero}/Animation/`) 7 ครั้ง ตั้งชื่อตามนี้:

| ชื่อ clip | ต้นแบบ | Animation Event | Loop |
|-----------|--------|----------------|------|
| `Item_Attack_Perform` | Skill_A_Attack1 | `Skill_Item_Attack_Perform` | ไม่ |
| `Item_Consume_Perform` | Skill_A_Attack1 | `Skill_Item_Consume_Perform` | ไม่ |
| `Item_Perform` | Skill_A_Attack1 | `Skill_Item_Perform` | ไม่ |
| `Item_Recall_Perform` | Skill_A_Attack1 | `Skill_Item_Recall_Perform` | ไม่ |
| `Item_Spell_Perform` | Skill_A_Attack1 | `Skill_Item_Spell_Perform` | ไม่ |
| `Item_StartCast` | Skill_A_Attack1 | `Skill_Item_StartCast` | ไม่ |
| `Item_Casting` | Idle | — (ไม่มี event) | **ใช่** |

> Placeholder เหล่านี้ทำให้ระบบทำงานได้ทันที Hero จะแสดงท่า attack ชั่วคราวระหว่างรอ clip จริง

**2. ตั้ง Animation Event บน Perform clips**

เปิด clip ใน Animation window → เพิ่ม Event ที่ตำแหน่งที่ต้องการ trigger effect:

```
Function: Skill_Item_Perform        (สำหรับ Item_Perform)
Function: Skill_Item_Recall_Perform (สำหรับ Item_Recall_Perform)
Function: Skill_Item_Consume_Perform (สำหรับ Item_Consume_Perform)
Function: Skill_Item_Spell_Perform  (สำหรับ Item_Spell_Perform)
Function: Skill_Item_Attack_Perform (สำหรับ Item_Attack_Perform)
Int: 0
```

> `Item_StartCast` และ `Item_Casting` ไม่ต้องมี event

**3. ตั้ง Loop Time บน Item_Casting**

เลือก `Item_Casting.anim` → Inspector → Animation → เปิด **Loop Time** ✓

**4. เพิ่ม clips เข้า Override Controller**

เปิด `{Hero}CompleteLocomotion.overrideController` → Overrides section:

| Original Clip | Override Clip |
|--------------|--------------|
| `Item_Perform` (base) | `Item_Perform` (hero) |
| `Item_Recall_Perform` (base) | `Item_Recall_Perform` (hero) |
| `Item_Consume_Perform` (base) | `Item_Consume_Perform` (hero) |
| `Item_Spell_Perform` (base) | `Item_Spell_Perform` (hero) |
| `Item_Attack_Perform` (base) | `Item_Attack_Perform` (hero) |
| `Item_StartCast` (base) | `Item_StartCast` (hero) |
| `Item_Casting` (base) | `Item_Casting` (hero) |

**5. ทำซ้ำสำหรับ Weapon Override Controller** (ถ้ามี)

ใส่ clips จาก weapon animation folder แทน

---

## Guide: Animator — ใส่ clip จริงแทน Placeholder

เมื่อ animator ทำ clip จริงสำหรับไอเทมแล้ว:

1. ตั้ง Animation Event ที่ตำแหน่งที่ effect ควร trigger (ไม่ต้องอยู่ท้าย clip)
2. ตรวจว่า function name ถูกต้อง (ดูตารางด้านบน)
3. เปิด override controller → drag clip ใหม่เข้า Override slot ที่ตรงกัน
4. ทดสอบใน Play Mode — ดู Console ว่า `StateRelease` fire ถูกจุด

---

## Known Issues / TODO

- ⚠️ **Aphrodite weapon path**: clips อยู่ใน `Animation/old/New Folder/New Folder/` — ควร reorganize folder
- ⚠️ **Placeholder clips**: Hero ทั้งหมดยังใช้ท่า Skill_A_Attack1 เป็น placeholder สำหรับ Item_* — รอ animator ทำ clip จริง
- ⚠️ **Item_StartCast event**: base clip ไม่มี animation event — ถ้าต้องการ trigger effect ที่ StartCast ต้องเพิ่ม method ใน `AnimationEvent.cs` และ event ใน clip
