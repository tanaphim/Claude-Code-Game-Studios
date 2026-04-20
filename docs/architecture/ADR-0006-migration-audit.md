# ADR-0006 Migration Audit — Unified Ability System

**Date:** 2026-04-19
**Status:** Draft (awaiting user approval)
**Target ADR:** [ADR-0006 Unified Ability System](ADR-0006-unified-ability-system.md)
**Scope:** สำรวจ coupling ปัจจุบันของ `SkillKey` / `Buttons` / `Combat.Skill1-4`
เพื่อวางแผน migration เป็น slot-indexed + ability-id binding

---

## 1. สรุประดับสูง (Executive Summary)

ระบบปัจจุบันมี **3 axes ที่ถูกมัดรวมเป็นหนึ่ง**:

| Axis | Type | ตัวอย่าง |
|------|------|----------|
| **Input key** | `Buttons` enum | `Buttons.Q / W / E / R / A / S / B / Recall` |
| **Slot identity** | `SkillKey` enum | `SkillKey.Q / W / E / R / I / A / Recall / Item` |
| **Actor storage** | `Combat.SkillN` | `Combat.Skill1 / 2 / 3 / 4` |

ทั้ง 3 axes ถูกผูกกันแบบ 1-to-1 ด้วย **string-match** (UISkill.cs:176 —
`Enum.TryParse(ability.SkillKey.ToString(), out Buttons)`) และ **hardcoded
if-chain** (ActorCombatAction.cs กระจาย 85 refs)

**ข่าวดี:**
- Data layer (CBSAbility) เป็น string-id อยู่แล้ว — รองรับ N abilities ต่อ hero
- `SkillObject : NetworkBehaviour` มี `[Networked] NetworkString<_64> AbilityId` อยู่แล้ว
- FuzzySkillAI อ่าน weights จาก CBS อยู่แล้ว (damageWeight / aoeWeight / ฯลฯ)
- การ replicate `ActorCombatAction` ผ่าน `NetworkBehaviourId` (ไม่ใช่ enum) — รองรับ swap
- Dictionaries (SkillObjectDictionary / SkillVfxDictionary / SkinObject) เป็นแค่
  asset lookup — ย้ายจาก key `SkillKey` → key `string abilityId` ได้ตรง ๆ

**ข่าวร้าย:**
- `InputMessage.Buttons` (NetworkButtons — bitfield ขนาดคงที่) ไม่มี slot index —
  ต้องเพิ่ม field ใหม่ (ตาม ADR-0006 `byte PressedSlot`)
- `AnimationEvent.cs` มี method ต่อ (SkillKey × SkillState × ReleaseId) = **42 methods** —
  ต้องเลิกผูก animation event กับ SkillKey แล้วใช้ slot index แทน
- `ActorCombatAction` มี guard-block `if (SkillKey.A && Combat.NormalAttack.Id != Id)` ซ้ำ
  5+ ครั้ง — ต้อง refactor เป็น helper method เดียว (ถ้าไม่เปลี่ยน slot ก็ซ่อนความยุ่งได้)
- Per-hero class (HerculesRAction, CupidQAction, ฯลฯ) override `GetInput()` แล้วเช็ค
  `Buttons.R` ตรง ๆ — แต่ละ hero ต้องแก้ให้อ่านจาก slot แทน

**Blocker เดียวจริง ๆ:** `SkillKey` enum — เป็นจุดผูกมัดที่ลาม 15+ ไฟล์ 223 refs

---

## 2. Per-File Refactor Checklist

### 2.1 Input / Binding Layer (Client → Server)

| File | SkillKey Usage | Buttons Usage | Refactor Action | Phase | Risk |
|------|----------------|---------------|-----------------|-------|------|
| [`Commons/Enums/SkillKey.cs`](../../../../delta-unity/Assets/GameScripts/Commons/Enums/SkillKey.cs) | enum definition | — | **Deprecate** (คง enum ไว้ระหว่าง migration; mark `[Obsolete]` เมื่อทุก consumer ย้ายแล้ว) | P4 | H |
| [`Gameplays/Cores/Input/InputMessage.cs`](../../../../delta-unity/Assets/GameScripts/Gameplays/Cores/Input/InputMessage.cs) | — | `Buttons` enum (10 entries) | **ADD** `public byte PressedSlot` field + `public byte ReleasedSlot` (ตาม ADR-0006). คง `Buttons.LeftClick/RightClick/S/B` ไว้สำหรับ move/stop/base; Q/W/E/R/A/Recall กลายเป็น slot number | P1 | H |
| [`Networkings/NetworkRunnerInput.cs`](../../../../delta-unity/Assets/GameScripts/Networkings/NetworkRunnerInput.cs) (lines 58-146) | — | subscribes `PlayerGameplayInput.Q/W/E/R/A/S/T/Recall` → `InputMessage.Buttons.Set(Buttons.X, ...)` | **REPLACE:** subscribe ผ่าน `KeybindMap` (new) ที่อ่าน user-defined remap; emit slot index แทน ปุ่ม | P2 | H |
| [`UI/GameViews/Core/UISkill.cs:176`](../../../../delta-unity/Assets/GameScripts/UI/GameViews/Core/UISkill.cs) | อ่าน `m_Ability.SkillKey` | `Enum.TryParse(..., out Buttons)` — **bridge ปัจจุบันระหว่าง data และ input** | **REPLACE:** `KeybindMap.TriggerSlot(abilityId)` — ไม่แปลง string แล้ว | P2 | M |
| [`UI/GameViews/Core/UIMinimapView.cs:210`](../../../../delta-unity/Assets/GameScripts/UI/GameViews/Core/UIMinimapView.cs) | — | `Buttons.RightClick` | ไม่ต้องแตะ (move input คง Buttons ไว้) | — | L |
| [`Networkings/NetworkRunnerManager.cs`](../../../../delta-unity/Assets/GameScripts/Networkings/NetworkRunnerManager.cs) | — | Buttons ref | ตรวจสอบ subscribe/dispose flow — ไม่คาดว่าต้องแก้ | — | L |
| [`Miscellaneous/FusionCallbacksHandler.cs`](../../../../delta-unity/Assets/GameScripts/Miscellaneous/FusionCallbacksHandler.cs) | — | Buttons ref | ตรวจสอบ input injection flow | — | L |
| [`Networkings/Network/NetworkGameObjectBehavior.cs`](../../../../delta-unity/Assets/GameScripts/Networkings/Network/NetworkGameObjectBehavior.cs) | — | Buttons ref | ตรวจสอบ | — | L |

**Key finding:** Gameplay input (Q/W/E/R/A/Recall) ถูกแยก event กันใน
`PlayerGameplayInput` อยู่แล้ว — ไม่ใช่ single "OnAnyKey" handler —
`KeybindMap` จะแทนที่ event subscription layer นี้ได้โดยไม่กระทบ UI

---

### 2.2 Dispatch Layer (Server-side skill matching)

| File | SkillKey Refs | Refactor Action | Phase | Risk |
|------|---------------|-----------------|-------|------|
| [`Gameplays/Characters/ActorCombatAction.cs`](../../../../delta-unity/Assets/GameScripts/Gameplays/Characters/ActorCombatAction.cs) | **85** | รายละเอียดด้านล่าง | P2-P3 | **H** |
| [`Gameplays/Characters/AnimationEvent.cs`](../../../../delta-unity/Assets/GameScripts/Gameplays/Characters/AnimationEvent.cs) | **43** | รายละเอียดด้านล่าง | P2 | H |
| [`Gameplays/Characters/ActorAnimation.cs`](../../../../delta-unity/Assets/GameScripts/Gameplays/Characters/ActorAnimation.cs) | 37 | `RunSkill(SkillKey, SkillState)` → `RunSkill(byte slot, SkillState)` + internal mapping to animator params | P2 | M |
| [`Gameplays/Characters/ActorCombat.cs`](../../../../delta-unity/Assets/GameScripts/Gameplays/Characters/ActorCombat.cs) | 7 | ราย `Skill1/Skill2/Skill3/Skill4` → `NetworkDictionary<byte, NetworkBehaviourId> Slots` (ADR-0006 ข้อ 3); คง property `Skill1`-`Skill4` เป็น facade ระหว่าง migration | P3 | H |
| [`Gameplays/Characters/ActorBakeController.cs`](../../../../delta-unity/Assets/GameScripts/Gameplays/Characters/ActorBakeController.cs) | 20 | bake system (prewarm) — ส่ง slot list แทน SkillKey list | P3 | M |
| [`Gameplays/Characters/ActorBakeAnimation.cs`](../../../../delta-unity/Assets/GameScripts/Gameplays/Characters/ActorBakeAnimation.cs) | 7 | เหมือน ActorBakeController | P3 | L |

#### 2.2.1 ActorCombatAction.cs — รายละเอียด 85 refs

**Pattern A — Owner guard (ซ้ำ 5+ ครั้ง)** ที่ lines 866-872, 884-890, 902-908, 974-980, 996-1002:

```csharp
if (AbilityData.SkillKey == SkillKey.A && Actor.Combat.NormalAttack.Id != Id) return;
if (AbilityData.SkillKey == SkillKey.I && Actor.Combat.Passive.Id != Id) return;
if (AbilityData.SkillKey == SkillKey.Q && Actor.Combat.Skill1.Id != Id) return;
if (AbilityData.SkillKey == SkillKey.W && Actor.Combat.Skill2.Id != Id) return;
if (AbilityData.SkillKey == SkillKey.E && Actor.Combat.Skill3.Id != Id) return;
if (AbilityData.SkillKey == SkillKey.R && Actor.Combat.Skill4.Id != Id) return;
if (AbilityData.SkillKey == SkillKey.Recall && Actor.Combat.SkillRecall.Id != Id) return;
```

→ **Refactor:** หลัง migration จะกลายเป็น 1 บรรทัด:

```csharp
if (Actor.Combat.GetSlotAction(AbilityData.Slot)?.Id != Id) return;
```

**Pattern B — Input-to-slot binding (ที่ซ้ำหนักที่สุด)** lines 1730, 1741, 1754, 1877 — เช็ค
`pressed.IsSet(Buttons.X)` ต่อ SkillKey:

```csharp
if (AbilityData.SkillKey == SkillKey.A && pressed.IsSet(Buttons.A)
 || AbilityData.SkillKey == SkillKey.Q && pressed.IsSet(Buttons.Q)
 || AbilityData.SkillKey == SkillKey.W && pressed.IsSet(Buttons.W)
 ...) { ... }
```

→ **Refactor:** เมื่อ InputMessage มี `PressedSlot`:

```csharp
if (input.PressedSlot == AbilityData.Slot && input.PressedSlot != 0) { ... }
```

**Pattern C — Rank-up / leveling exclusions** lines 2115, 2125, 2133:
`if (SkillKey.A || SkillKey.Item || SkillKey.Recall) return;` — คัดเฉพาะ
"leveled skills" ออกจาก "non-leveled"

→ **Refactor:** เพิ่ม `AbilityData.IsLeveled` (bool ใน CBS) — ไม่ต้องผูก slot identity

**Pattern D — Quick-cast settings** line 1764:
`DeltaService.RuntimeSettings.QuickQ && SkillKey.Q || QuickW && SkillKey.W || ...`

→ **Refactor:** `RuntimeSettings.QuickCastSlots[]` (ตาม slot index แทน SkillKey)

**สรุป ActorCombatAction refactor:**
- Pattern A → 1 helper `IsActiveOwner(byte slot)` แทน 5 ชุด if-chain = **-35 บรรทัด**
- Pattern B → 1 condition per phase × 4 phases = **-40 บรรทัด**
- Pattern C/D → ย้าย data ไป CBS / Settings ตาม slot = **-10 บรรทัด**
- **Net: ActorCombatAction จะลดลง ~85-100 บรรทัด** — ลดทั้ง complexity และ bug surface

#### 2.2.2 AnimationEvent.cs — 42 bridge methods

ปัจจุบันมี **42 methods ใน AnimationEvent.cs** ที่ animator clip เรียก:

```csharp
public void PerformQ(int param) => StateRelease(SkillKey.Q, SkillState.Perform, param);
public void PerformQ2(int param) => StateRelease(SkillKey.Q, SkillState.Perform2, param);
...
public void EmpowerI3(int param) => StateRelease(SkillKey.I, SkillState.Empower3, param);
```

**ปัญหา:** animator clip อ้างอิง method name ด้วย string — เปลี่ยน signature = clip แตก

**กลยุทธ์:**
- **Option A (แนะนำ):** คง method names เดิมทั้ง 42 ไว้ แต่เปลี่ยน internal implementation:
  ```csharp
  public void PerformQ(int param) => StateReleaseSlot(Actor.Combat.GetActiveSlot(), SkillState.Perform, param);
  ```
  (animator event ไม่รู้ว่าเป็น SkillKey อะไร มันรู้แค่ว่า `Actor.Combat` มี skill
  ที่ active อยู่ตอนนี้ คือ skill ไหน — ส่ง slot ของ active skill เข้าไป)

- **Option B:** ลบ 42 methods + ให้ animator clip ทุกตัว rebind ใช้ `PerformSlot(int slot, int releaseId)` method เดียว
  → **ต้นทุน:** ต้อง re-author animator ทุกตัว (25+ heroes × N clips) = **ปฏิเสธ** — cost สูงมาก

**Resolution:** ใช้ Option A; 42 methods คงไว้เป็น shim, mark `[Obsolete]` ภายใน

#### 2.2.3 ActorAnimation.cs — 37 refs

`RunSkill(SkillKey, SkillState)` เป็น entry point หลักที่ ActorCombatAction เรียก:
- Trigger animator parameter (เช่น `Q_Perform`, `R_Empower`)
- Spawn VFX ผ่าน SkinObject

**Refactor:** เพิ่ม method overload `RunSkill(byte slot, SkillState)`:
- slot → SkillKey mapping ดึงจาก `SkinObject.SlotToSkillKey[slot]` (per-hero asset)
- คง `RunSkill(SkillKey, SkillState)` เป็น facade ระหว่าง migration

---

### 2.3 AI Layer

| File | Usage | Refactor Action | Phase | Risk |
|------|-------|-----------------|-------|------|
| [`Gameplays/Characters/Actors/AIRule/FuzzySkillAI.cs`](../../../../delta-unity/Assets/GameScripts/Gameplays/Characters/Actors/AIRule/FuzzySkillAI.cs) | `SkillData.keys` = SkillKey | **REPLACE:** `SkillData.slot = byte`; algorithm ไม่แตะ (weights อ่านจาก CBS อยู่แล้ว) | P2 | L |
| [`Gameplays/Characters/Actors/ActorBoss.cs`](../../../../delta-unity/Assets/GameScripts/Gameplays/Characters/Actors/ActorBoss.cs) | `Dictionary<Buttons, float> m_SkillCD` (Q/W/E/R) + `AddIfReady(Skill1, Buttons.Q)` | **REPLACE:** `Dictionary<byte, float> m_SlotCD` + iterate slots จาก `Combat.Slots` | P2 | M |
| [`Gameplays/Characters/Actors/ActorMiniBoss.cs`](../../../../delta-unity/Assets/GameScripts/Gameplays/Characters/Actors/ActorMiniBoss.cs) | เหมือน ActorBoss | เหมือน ActorBoss | P2 | M |
| [`Gameplays/Characters/Actors/ActorMonster.cs`](../../../../delta-unity/Assets/GameScripts/Gameplays/Characters/Actors/ActorMonster.cs) | `Buttons.RightClick` เท่านั้น (move) | ไม่ต้องแตะ | — | L |
| [`Gameplays/Characters/Actors/ActorJungle.cs`](../../../../delta-unity/Assets/GameScripts/Gameplays/Characters/Actors/ActorJungle.cs) | `Buttons.RightClick` เท่านั้น | ไม่ต้องแตะ | — | L |
| [`Gameplays/Characters/Actors/ActorDungeon.cs`](../../../../delta-unity/Assets/GameScripts/Gameplays/Characters/Actors/ActorDungeon.cs) | 7 × SkillKey + `DoActivateSkill(Combat.Skill1, Buttons.Q, ...)` × 4 | iterate slots: `foreach var (slot, action) in Combat.Slots` | P2 | M |

**Observation:** AI layer ไม่มี hardcoded hero behaviour — อ่าน `SkillData` list แล้ว
evaluate weights. ย้าย key ไป slot = **refactor ตรงไปตรงมา**

---

### 2.4 Merlin Variants + Boss Phase Swap

#### Merlin (Elemental Forms)

12 ไฟล์: `MerlinQ[Fire|Ice|Dark]Action`, `MerlinW[Fire|Ice|Dark]Action`,
`MerlinR[Fire|Ice|Dark]Action`, `MerlinEAction` (form switch), `MerlinNAction`,
`MerlinIAction`

**Pattern ปัจจุบัน:** Hero-specific classes. `MerlinEAction` (E key) สลับ form
ของ Merlin — form นี้เป็น form ระดับ Actor (ไม่ใช่ระดับ slot)

**Migration strategy:**
- Form switching ไม่ใช่ ability-swap — เป็นการ hot-swap Q/W/R actions บน slots
  [1][2][3] (ยังคงอยู่ slot เดิม แค่เปลี่ยน ability ที่ bind)
- `MerlinEAction` ไปเรียก `Actor.Combat.RebindSlot(slot: 1, abilityId: "MERLIN_Q_FIRE")`
- Animation / VFX ยังคง slot เดิม — แค่ SkinObject lookup ใหม่ตาม ability

**ความเสี่ยง:** MEDIUM — pattern ตรงกับ Rubick ability steal ที่ ADR-0006 มุ่งสนับสนุนอยู่แล้ว
(rebind slot ระหว่าง match) — ถือเป็น test case ของ system

#### Boss Phase Swap

Ice Dragon มี prefab แยก phase1 / phase2 — swap ทั้ง prefab เมื่อ HP threshold

**Migration strategy:**
- Phase 1 prefab = AbilityComponent พร้อม `[abilityId:1, abilityId:2, abilityId:3]`
- Phase 2 prefab = AbilityComponent พร้อม `[abilityId:4, abilityId:5, abilityId:6]`
- ถ้าต้องการ swap skillset โดยไม่สวอป prefab: `Actor.Combat.RebindSlots(new[] {id4, id5, id6})`
  → ถูกกว่าในแง่ network overhead และไม่ต้อง re-parent attachments

**ความเสี่ยง:** LOW — existing prefab swap ยัง work, ADR-0006 แค่ **เปิดทาง** rebind
in-place เป็นทางเลือกใหม่

---

### 2.5 UI Layer

| File | SkillKey / Buttons Usage | Refactor Action | Phase | Risk |
|------|--------------------------|-----------------|-------|------|
| [`UI/GameViews/Core/UISkill.cs`](../../../../delta-unity/Assets/GameScripts/UI/GameViews/Core/UISkill.cs) | `m_Ability.SkillKey.ToString()` → `Enum.Parse<Buttons>` (**bridge**) | **REPLACE:** bind ด้วย abilityId โดยตรง (line 66 `Setup(skillId)`); trigger ผ่าน `KeybindMap.TriggerSlot` | P2 | M |
| `UI/GameViews/Core/UICharacterConsoleView.cs` | 5 SkillKey refs | Display panel — replace by slot index + display name จาก CBS | P3 | L |
| `UI/GameViews/Core/UITrainingMenuView.cs` | SkillKey ref | Training mode display | P3 | L |
| `UI/GameViews/Core/Skillshowinfo.cs` | SkillKey ref | Tooltip panel | P3 | L |

**Key finding:** UI แสดง `m_SkillID.Split("_")[1]` (ดึง "Q"/"W"/"E"/"R" จาก abilityId
เช่น `"MERLIN_Q_FIRE"` → "Q") — เป็น display only — ไม่ใช่ logic. Display ใหม่จะมาจาก
`AbilityData.DisplayKeyLabel` ใน CBS (สอดคล้อง remap setting)

---

### 2.6 Resource Dictionaries

| File | Key Type | Migration |
|------|----------|-----------|
| [`Resources/SkillObjectDictionary.cs`](../../../../delta-unity/Assets/GameScripts/Resources/SkillObjectDictionary.cs) | `UnitySerializedDictionary<SkillKey, StateObjectDictionary>` | เปลี่ยน key type → `string abilityId` (ใช้ CBS id ตรง ๆ) |
| [`Resources/StateObjectDictionary.cs`](../../../../delta-unity/Assets/GameScripts/Resources/StateObjectDictionary.cs) | `<SkillState, ReleaseObjectDictionary>` | ไม่แตะ (SkillState ไม่เกี่ยวกับ binding) |
| `Resources/SkillVfxDictionary.cs` | SkillKey-keyed | เปลี่ยน key → abilityId |
| `Resources/ProjectileDictionary.cs` | SkillKey-keyed | เปลี่ยน key → abilityId |
| [`Resources/SkinObject.cs`](../../../../delta-unity/Assets/GameScripts/Resources/SkinObject.cs) | ใช้ 4 dict (EnterVfx / ReleaseVfx / HitVfx / SkillObject) + method `GetSkillObject(SkillKey, SkillState, ...)` | **REPLACE signatures** → `GetSkillObject(string abilityId, SkillState, ...)` |

**Risk:** HIGH เพราะ SkinObject เป็น ScriptableObject asset — ทุก hero มี 1 asset =
25+ assets ต้อง re-migrate dictionary (เขียน editor migration tool จะทุ่นเวลาได้มาก)

**คำถามถึง user:** อยากให้ **tools-programmer** เขียน editor migration script ที่:
- อ่านทุก SkinObject
- แปลง key SkillKey → abilityId ตาม mapping table
- เซฟ asset ทั้งหมด
= ทำได้ใน P3, ลดความเสี่ยง hand-migration ของ 25+ heroes

---

### 2.7 Per-Hero Action Files — Refactor Pattern

ตัวอย่างจาก HerculesRAction.cs:

```csharp
// ปัจจุบัน (line 231-254)
public override void GetInput(InputMessage input)
{
    base.GetInput(input);
    ...
    if (input.Buttons.IsSet(Buttons.R))  // ← hardcoded button
    {
        m_Position = input.Position;
        ChangeDirection();
    }
}
```

**หลัง migration:**

```csharp
public override void GetInput(InputMessage input)
{
    base.GetInput(input);
    ...
    if (input.PressedSlot == AbilityData.Slot)  // ← ผ่าน slot แทน
    {
        m_Position = input.Position;
        ChangeDirection();
    }
}
```

**Count:** ทุก per-hero file ที่ override `GetInput` + เช็ค `Buttons.X` ต้องแก้ —
estimated **~15-20 files** ในโฟลเดอร์ `Gameplays/Characters/*/`

**Good news:** ส่วนใหญ่เป็นการเปลี่ยน **1 บรรทัด** ต่อไฟล์ — เขียน regex + manual review
เพียงพอ ไม่ต้อง re-architect per-hero logic

---

## 3. InputMessage Migration Strategy

### 3.1 Current shape

```csharp
public struct InputMessage : INetworkInput
{
    public NetworkButtons Buttons;          // bitfield, fixed slots
    public Vector3Compressed Position;
    public NetworkBehaviourId Target;
    public Vector2Compressed MouseScreenPosition;
}
```

### 3.2 Target shape (ตาม ADR-0006)

```csharp
public struct InputMessage : INetworkInput
{
    public NetworkButtons Buttons;          // คง — ใช้สำหรับ LeftClick/RightClick/S/B
    public Vector3Compressed Position;
    public NetworkBehaviourId Target;
    public Vector2Compressed MouseScreenPosition;

    public byte PressedSlot;                // ADDED: 0 = none, 1-7 = active slot
    public byte ReleasedSlot;               // ADDED: release frame signal
    public byte RebindRequestSlot;          // ADDED (optional, Phase 3): ability swap during runtime
}
```

**Wire cost:** +3 bytes per tick per player (10 players × 60Hz = 1.8 KB/s per match — negligible)

### 3.3 Migration path

- **P1:** เพิ่ม `PressedSlot/ReleasedSlot` เข้า InputMessage (คง Buttons เดิม — ไม่ break)
- **P1:** `NetworkRunnerInput` เขียนทั้ง 2 pattern พร้อมกัน (`Buttons.Set(Buttons.Q, ...)` + `InputMessage.PressedSlot = 1`)
- **P2:** ActorCombatAction และ per-hero classes ย้ายไปอ่าน `PressedSlot`
- **P3:** ลบ Q/W/E/R/A/Recall ออกจาก `Buttons` enum (คง LeftClick/RightClick/S/B ไว้)
- **P4:** ลบ `SkillKey` enum

---

## 4. Pilot Hero Selection

**ข้อเสนอ:** ใช้ **Hercules** เป็น pilot — เหตุผล:

| Factor | Hercules | ทางเลือก (Merlin) | ทางเลือก (Cupid) |
|--------|----------|-------------------|-------------------|
| Skill count | Q/W/E/R + Attack + Passive + Recall = 7 (มาตรฐาน) | 3 forms × 3 = complex | 4 มาตรฐาน |
| Code complexity | ปานกลาง (R มี channeled + dash) | สูง (form swap) | ปานกลาง |
| ใช้ feature ของ ADR-0006 | Charged skill (R) — test `HasCharging` path | Rebind (form swap) | มาตรฐาน |
| Risk | ปานกลาง — หลาย code path | สูง — form swap ต้องใช้ rebind API ที่ยังไม่มี | ต่ำ |

Hercules ครอบคลุม "standard MOBA skill patterns" ได้ดี แต่ **ไม่** ไปกระทบ rebind API
ซึ่งเป็น P3. Merlin ไว้ test rebind ใน Phase 3 ต่อไป

**Alternative suggestion:** เริ่มที่ hero ใหม่เลย 1 ตัว (Hero N ที่ยังไม่มี) ที่ถูกสร้างบน
Unified Ability System ตั้งแต่แรก — ได้ end-to-end proof-of-concept โดยไม่เสี่ยงแก้ hero เก่า

---

## 5. Open Questions & Blockers

### 5.1 Open Questions สำหรับ user

1. **Editor migration tool** — อยากให้ tools-programmer เขียน script แปลง SkinObject /
   SkillObjectDictionary assets (25+ heroes) หรือทำ manual?
2. **Pilot hero** — เลือก Hercules, Merlin, หรือ hero ใหม่?
3. **Phase 1 scope** — ADR-0006 Phase 1 แค่เพิ่ม `AbilityRegistry` + `AbilityComponent`
   parallel กับระบบเดิม (ยังไม่แตะ SkillKey) — ยืนยันให้เริ่มที่นี่?
4. **Keybind UI** — UX Designer ต้องออกแบบหน้า remap (Settings → Controls) ก่อน P2
   หรือทำ placeholder ไปก่อน?
5. **AnimationEvent 42 methods** — ยืนยัน Option A (keep names, route through slot) —
   ห้ามแตะ animator clips ของ hero เดิมใช่หรือไม่?
6. **NetworkDictionary ใน Photon Fusion** — `[Networked] NetworkDictionary<byte, NetworkBehaviourId>`
   รองรับไหม? ต้องตรวจ Fusion 2 docs — ถ้าไม่รองรับ ต้องใช้ fixed array size + manual indexing

### 5.2 Technical Blockers

| Blocker | Severity | Resolution |
|---------|----------|-----------|
| Fusion 2 `NetworkDictionary<byte, NetworkBehaviourId>` support uncertain | H | ตรวจสอบ docs ก่อน P3; fallback = `[Networked, Capacity(8)] NetworkArray<NetworkBehaviourId> Slots` |
| `CBSAbility.SkillKey` (enum field) เป็น serialized data — เปลี่ยน schema = break ข้อมูล live บน CBS dashboard | H | คง field ไว้ + deprecate; เพิ่ม `byte DefaultSlot` แยกต่างหาก |
| Animator state machine parameters ใช้ name เช่น `Q_Perform` — ผูกกับ SkillKey | H | Animator ยังคงใช้ SkillKey name ภายใน; mapping layer (ActorAnimation) แปลง slot → animator param |
| Unit test infrastructure ของ ActorCombatAction ยังไม่มี | M | เสี่ยง regression — ขอให้ qa-lead วาง baseline playtest checklist ก่อน P2 |

---

## 6. Phase Breakdown (ตาม ADR-0006)

### Phase 1 — Foundation (2-3 weeks)
- สร้าง `AbilityRegistry`, `AbilityComponent`, `KeybindMap`, `AbilityDataSnapshot`
- เพิ่ม `PressedSlot/ReleasedSlot` ใน InputMessage (parallel — ยังไม่ลบของเก่า)
- NetworkRunnerInput emit ทั้ง 2 pattern
- **ยังไม่แตะ ActorCombatAction / SkillKey**

### Phase 2 — Pilot Hero (2-3 weeks)
- Hercules ย้ายมาใช้ Unified system ทั้งตัว
- AnimationEvent.cs route through slot (Option A)
- FuzzySkillAI อ่าน slot
- ActorBoss / ActorMiniBoss AI iterate slots
- UISkill / UIMinimapView รองรับ slot + keybind remap UI

### Phase 3 — Mass Migration (4-6 weeks)
- 24 heroes ที่เหลือ (parallel หลาย programmer)
- Boss phase swap demo (Ice Dragon)
- Merlin form swap (rebind API)
- Editor migration script สำหรับ SkinObjects

### Phase 4 — Cleanup (1-2 weeks)
- ลบ `SkillKey` enum + Q/W/E/R/A/Recall จาก `Buttons` enum
- ลบ `Combat.Skill1/2/3/4/SkillRecall` properties
- AnimationEvent legacy 42 methods → `[Obsolete]`
- Ability Draft mode (เปิดทางไว้ — implement หรือไม่ implement ก็ได้)

**Total estimate:** 9-14 weeks (~2-3.5 months) — scope-dependent

---

## 7. Recommended Next Step

หลัง user อนุมัติ audit นี้:

1. อัปเดต ADR-0006 จาก `Proposed` → `Accepted`
2. สร้าง Sprint 003 (ถ้า Sprint 002 ใกล้จบ) หรือเพิ่ม epic เข้า Sprint 002
3. **Phase 1 kickoff** — lead-programmer + unity-specialist ออกแบบ `AbilityRegistry`
   interface ก่อน coding
4. Parallel: ux-designer + accessibility-specialist ออกแบบ remap UI
5. Parallel: tools-programmer วางแผน editor migration script

**Gate check ก่อน P2:** demo `AbilityRegistry` + `AbilityComponent` ทำงานได้จริง
(hero ตัวใหม่ที่ไม่ใช่ in-match hero) ก่อนจะแตะ Hercules

---

## 8. Appendix — Files Discovered

### 8.1 All SkillKey consumers (15 files, 223 refs)

| File | Refs |
|------|------|
| ActorCombatAction.cs | 85 |
| AnimationEvent.cs | 43 |
| ActorAnimation.cs | 37 |
| ActorBakeController.cs | 20 |
| ActorDungeon.cs | 7 |
| ActorCombat.cs | 7 |
| ActorBakeAnimation.cs | 7 |
| UICharacterConsoleView.cs | 5 |
| FuzzySkillAI.cs (SkillData.keys) | 1 |
| SkillObjectDictionary.cs (type param) | 1 |
| SkillVfxDictionary.cs (type param) | 1 |
| ProjectileDictionary.cs (type param) | 1 |
| SkinObject.cs (methods) | ~5 |
| UISkill.cs (via CBSAbility.SkillKey) | 1 |
| CBSAbility.cs (field) | 1 |

### 8.2 Hero action files ที่ต้อง per-file refactor

โฟลเดอร์ `Gameplays/Characters/*/` — สำรวจเพิ่มใน P2 แต่ละ hero เมื่อมาถึง. Estimated
~15-20 files ที่ override `GetInput` และเช็ค `Buttons.Q/W/E/R` ตรง ๆ

### 8.3 Files ที่ **ไม่** ต้องแก้

- `ActorMonster.cs`, `ActorJungle.cs` — ใช้แค่ `Buttons.RightClick` (move), ไม่มี skill
- `UIMinimapView.cs` — ใช้แค่ `Buttons.RightClick` (minimap click → move)
- `NetworkStatusEffect.cs` — ใช้แค่ `Buttons.RightClick` ใน mind control
- Files ที่อ้าง `SkillState` เท่านั้น (ไม่มี SkillKey)

---

**End of Audit**
