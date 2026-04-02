# ADR-0005: ItemAnimationType for Per-Item Animation Routing

## Status

Accepted

## Date

2026-04-03

## Decision Makers

tanaphim, Claude Sonnet 4.6

## Context

### Problem Statement

ไอเทม Active ใน Delta มีหลายประเภทที่ควรเล่น animation ท่าทางแตกต่างกัน:
ใบวาร์ปควรเล่น recall pose, ขวดยาควรเล่น consume/drink pose, คัมภีร์เวทย์ควรเล่น
cast pose, ดาบยาวควรเล่น attack pose

ก่อนหน้านี้ `ActorAnimation.RunSkill()` มี guard `if (key == SkillKey.Item) return;`
ซึ่ง skip animation ทุกประเภทสำหรับ item — ทำให้ไม่มี animation ใดๆ เลยเมื่อใช้ไอเทม

### Current State (ก่อนการเปลี่ยนแปลง)

```csharp
// ActorAnimation.cs — item animation ถูก skip ทั้งหมด
public override async void RunSkill(SkillKey key, SkillState state, float speed = 1)
{
    if (key == SkillKey.Item) return;  // ← bug: skip ทุก item animation
    // ...
}
```

### Constraints

- ต้องไม่ break API ของ `IActorAnimation.RunSkill(SkillKey, SkillState, float)`
- Item type ใหม่อาจเพิ่มในอนาคต — ต้องออกแบบให้ extensible
- Animation type ต้องกำหนดต่อ item (data-driven) ไม่ใช่ hardcode ในโค้ด
- ทุก item animation ต้อง trigger `StateRelease(SkillKey.Item, ...)` เหมือนกัน

### Requirements

- ไอเทมแต่ละชิ้นสามารถระบุ animation type ที่ต้องการ
- System เล่น animator state ที่ตรงกับ type นั้น
- เพิ่ม type ใหม่ได้โดยแก้ code น้อยที่สุด
- Default fallback เมื่อไม่ระบุ type (ไม่ error)

## Decision

เพิ่ม `ItemAnimationType` enum ใน `CBSAbility` และ routing logic ใน
`ActorAnimation.ResolveAnimStateHash()` โดยใช้ `CurrentItemAnimationType`
property บน `IActorAnimation` เป็น runtime state

### Architecture

```
CBS (PlayFab)
└── CBSAbility.ItemAnimationType = Recall | Consume | Spell | Attack | Default

ActorCombatAction.OnPerform()
    │ (ก่อน RunSkill)
    ├── Actor.Animation.Running.CurrentItemAnimationType = AbilityData.ItemAnimationType
    └── RunSkill(SkillKey.Item, SkillState.Perform)
            │
            ▼
    ActorAnimation.ResolveAnimStateHash(key, state)
        ├── Default  → hash("Item_Perform")
        ├── Recall   → hash("Item_Recall_Perform")
        ├── Consume  → hash("Item_Consume_Perform")
        ├── Spell    → hash("Item_Spell_Perform")
        └── Attack   → hash("Item_Attack_Perform")
            │
            ▼
    Animator.Play(hash)
            │
    Animation Event (on clip)
            │
    AnimationEvent.Skill_Item_{Type}_Perform(0)
            │
    StateRelease(SkillKey.Item, SkillState.Perform, 0)
            │
    ActorCombatAction.OnStateRelease() → item effect activates
```

### Key Interfaces

```csharp
// enum — เพิ่ม value ใหม่ที่นี่เมื่อต้องการ type ใหม่
public enum ItemAnimationType { Default, Recall, Consume, Spell, Attack }

// CBS data — ตั้งค่าต่อไอเทมใน PlayFab Dashboard
public class CBSAbility : CBSItemCustomData
{
    public ItemAnimationType ItemAnimationType; // default = Default (0)
}

// IActorAnimation — runtime state
public abstract class IActorAnimation : MonoBehaviour
{
    public ItemAnimationType CurrentItemAnimationType { get; set; }
}

// ActorAnimation — resolve ชื่อ state
private int ResolveAnimStateHash(SkillKey key, SkillState state)
{
    if (key == SkillKey.Item && CurrentItemAnimationType != ItemAnimationType.Default)
        return Animator.StringToHash($"Item_{CurrentItemAnimationType}_{state}");
    return m_KeyStateDict[key][state];
}

// AnimationEvent — ทุก variant route ไปที่ Item StateRelease
public void Skill_Item_Recall_Perform(int param)  => Skill_Item_Perform(param);
public void Skill_Item_Consume_Perform(int param) => Skill_Item_Perform(param);
public void Skill_Item_Spell_Perform(int param)   => Skill_Item_Perform(param);
public void Skill_Item_Attack_Perform(int param)  => Skill_Item_Perform(param);
```

### Implementation Guidelines

- `CurrentItemAnimationType` ต้องถูก set **ก่อน** `RunSkill()` เสมอ — ทำใน `OnPerform()`
- Animator state ทุกตัว (`Item_*_Perform`) ต้องมี Animation Event เรียก method ที่ตรงกัน
- เมื่อเพิ่ม type ใหม่: (1) เพิ่ม enum value, (2) เพิ่ม AnimationEvent method, (3) สร้าง Animator State ใน Editor
- `ItemAnimationType.Default` fallback ไปที่ `Item_Perform` เสมอ — ไม่ต้องสร้าง state ใหม่สำหรับไอเทมพื้นฐาน

## Alternatives Considered

### Alternative 1: SkillKey ต่อ ItemType

- **Description**: เพิ่ม SkillKey variants เช่น `ItemRecall`, `ItemConsume`, `ItemSpell`
- **Pros**: ใช้ระบบ routing เดิมได้ทันที ไม่ต้องเพิ่ม property ใหม่
- **Cons**: SkillKey enum โตมาก; ทุก switch statement ทุกที่ใน codebase ต้องอัพเดต; `NetworkHeroInventory` mapping ซับซ้อน
- **Rejection Reason**: Cascading changes ทั่ว codebase สูงเกินไป

### Alternative 2: String clip name ใน CBSAbility

- **Description**: `CBSAbility.AnimationClipName = "Item_Consume_Perform"` เป็น raw string
- **Pros**: ยืดหยุ่นสูงสุด — ใส่ชื่อ state อะไรก็ได้
- **Cons**: Typo-prone, ไม่มี compile-time safety, ยาก discover ว่ามี state อะไรบ้าง
- **Rejection Reason**: Fragile และ ยาก maintain

### Alternative 3: Reuse existing SkillKey states (เช่น Recall_Perform สำหรับ warp item)

- **Description**: ใบวาร์ปเล่น `Recall_Perform` ตรงๆ โดยเปลี่ยน key เป็น `SkillKey.Recall`
- **Pros**: ไม่ต้องสร้าง animator state ใหม่
- **Cons**: Animation Event `Skill_Recall_Perform` → `StateRelease(SkillKey.Recall, ...)` — triggers callback ผิด key; item skill ไม่ถูก resolve
- **Rejection Reason**: Incorrect state release routing

## Consequences

### Positive

- เพิ่ม item animation type ใหม่ใน 4 ขั้นตอนที่ชัดเจน
- ไม่ break API ของ `RunSkill()` — signature เดิม
- Default fallback ป้องกัน error สำหรับไอเทมที่ไม่ระบุ type
- Data-driven — ปรับ animation ต่อไอเทมผ่าน CBS ได้โดยไม่ต้อง rebuild

### Negative

- `CurrentItemAnimationType` เป็น mutable state บน animation component — ต้อง set ก่อน RunSkill ทุกครั้ง (implicit ordering dependency)
- Animator States (`Item_*_Perform`) ต้องสร้างใน Editor ต่อ hero ทุกตัว — manual work

### Neutral

- AnimationEvent methods เพิ่มขึ้นตามจำนวน type — boilerplate แต่ explicit และ traceable

## Risks

| Risk | Probability | Impact | Mitigation |
|------|------------|--------|-----------|
| ลืม set `CurrentItemAnimationType` ก่อน RunSkill | ต่ำ | ต่ำ | Fallback เป็น Default — ไม่ crash |
| Animator State ขาดหายสำหรับบาง Hero | สูง (early) | ต่ำ | Animator.Play() ล้มเหลว silently — ไม่ crash |
| Type ใหม่ลืม add AnimationEvent method | ปานกลาง | ต่ำ | Animation เล่นได้แต่ OnStateRelease ไม่ fire |

## Performance Implications

| Metric | Before | After |
|--------|--------|-------|
| `Animator.StringToHash()` calls | 0 (skipped) | 1 ต่อ item use |
| GC allocation | 0 | 0 (hash เป็น int) |
| Extra field per ability | — | 4 bytes (enum) |

## Migration Plan

1. อัพเดต CBS Ability data ใน PlayFab Dashboard: เพิ่ม `ItemAnimationType` field ต่อไอเทม Active
2. สร้าง Animator States (`Item_Recall_Perform` ฯลฯ) ใน AnimatorController ของแต่ละ Hero
3. เพิ่ม `Item_Viable` bool parameter ใน Animator ของแต่ละ Hero (สำหรับ GetViable/SetViable)
4. ทดสอบต่อ hero ว่า animation เล่นถูกต้องและ effect ทำงานหลัง animation จบ

**Rollback**: เพิ่ม `if (key == SkillKey.Item) return;` กลับใน `RunSkill()` — item animations กลับเป็น skip ทั้งหมดเหมือนเดิม

## Validation Criteria

- [ ] ใบวาร์ป: เล่น `Item_Recall_Perform`, warp ทำงานหลัง animation จบ
- [ ] ขวดยา: เล่น `Item_Consume_Perform`, heal ทำงานหลัง animation จบ
- [ ] ไอเทมไม่มี type: fallback เล่น `Item_Perform` โดยไม่ error
- [ ] ไม่มี Animator State: `Animator.Play()` ล้มเหลว silently — ไม่ crash game

## Related

- [ADR-0004: ActorCombatAction skill pipeline](ADR-0004-actor-combat-action-skill-pipeline.md)
- `design/gdd/item-system.md` — §3.8 Item Animation System
- `Assets/GameScripts/Gameplays/Characters/ActorAnimation.cs`
- `Assets/GameScripts/Gameplays/Characters/AnimationEvent.cs`
- `Assets/GameScripts/Commons/Enums/ItemType.cs`
- `Assets/GameScripts/Datas/DataModel/Metadata/CBS/CBSAbility.cs`
