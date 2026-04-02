# ADR-0004: ActorCombatAction + SkillKey Pipeline for Skill System

## Status

Accepted

## Date

2024-01-01 (reconstructed from codebase)

## Decision Makers

Core development team

## Context

### Problem Statement

Delta มีฮีโร่ 25+ ตัว แต่ละตัวมีสกิล Q/W/E/R/Passive/Attack และ Item skills รวม 7+ skills
ต่อตัวละคร ต้องออกแบบ architecture ที่: (1) ทุก skill ใช้ lifecycle เดียวกัน,
(2) ตัวละครต่างกันสามารถ override behavior ได้, (3) networking-aware (server authority)

### Current State

ไม่มี skill system ก่อนหน้า — เป็นการตัดสินใจออกแบบระบบใหม่

### Constraints

- ทุก skill ต้องทำงานบน Photon Fusion 2 (server-authoritative)
- Animation, VFX, damage calculation ต้องแยกออกจากกัน (testability)
- Hero แต่ละตัวต้องสามารถ override lifecycle phases ได้
- ต้องรองรับ: channeled skills, instant skills, charged skills, passive skills

### Requirements

- Unified lifecycle สำหรับทุก skill type (Enter → Casting → Perform → Exit)
- Server-authoritative damage calculation
- Animation และ VFX ทำงานอัตโนมัติตาม lifecycle
- Cooldown, cost, และ state management แยกออกจาก logic
- รองรับ skill interruption (Stun, Silence, Recall cancel)

## Decision

ใช้ **`ActorCombatAction` base class + `SkillKey` enum** เป็น foundation ของ skill system
ทุก skill (รวมถึง item skills) เป็น subclass ของ `ActorCombatAction`

### Architecture

```
SkillKey enum: Q | W | E | R | I(Passive) | A(Attack) | Recall | Item
SkillState enum: Enter | Casting | Progress(Perform) | Exit | Empower | ...

ActorCombatAction (base class)
├── Lifecycle (override points)
│   ├── Enter(SkillStateMessage)     — skill เริ่มต้น
│   ├── OnCasting()                  — ระหว่าง cast time
│   ├── OnPerform()                  — trigger animation + VFX
│   ├── OnStateRelease(id)           — animation event fires → effect
│   └── Exit()                       — cleanup
│
├── Cross-cutting concerns (handled by base)
│   ├── RunSkill(key, state)         — trigger animation
│   ├── CreateSkillVfx(clipState)    — spawn VFX
│   ├── StartMainCooldown()          — cooldown timer
│   ├── ReceiveDamage()              — interruption hook
│   └── IsStop = true               — request exit
│
└── Specializations
    ├── RecallAction                 — interrupt on damage, teleport on release
    ├── ItemCombatAction (base)      — item-specific behavior
    └── [HeroName][Skill]Action      — per-hero skill implementations

SkillStateMachine (Animator StateMachineBehaviour)
    → fires Enter/Release/Exit callbacks → ActorCombatAction lifecycle
```

### Key Interfaces

```csharp
// ทุก skill implement อย่างน้อย OnPerform หรือ OnStateRelease
public class MySkill : ActorCombatAction
{
    // ข้อมูล skill จาก CBS
    public override int Rank => 1;

    // เริ่มต้น skill (ตรวจสอบ target, set direction)
    protected override void Enter(SkillStateMessage msg) { base.Enter(msg); }

    // animation event fires → trigger effect
    protected override void OnStateRelease() { base.OnStateRelease();
        // spawn projectile, apply damage, teleport, etc.
    }

    // ถูก interrupt (damage, stun)
    protected override void ReceiveDamage(NetworkDamageFeedback fb)
    { base.ReceiveDamage(fb); IsStop = true; }
}

// Animation → Skill bridge
// AnimationEvent.cs method (called from animation clip event)
public void Skill_Q_Perform(int param) =>
    StateRelease(SkillKey.Q, SkillState.Perform, param);

// SkillKey.Item animation routing
Actor.Animation.Running.CurrentItemAnimationType = AbilityData.ItemAnimationType;
Actor.Animation.Running.RunSkill(SkillKey.Item, SkillState.Perform);
```

### Implementation Guidelines

- **ทุก skill logic** ต้องอยู่ใน subclass ของ `ActorCombatAction` — ไม่เขียน skill logic ใน MonoBehaviour อื่น
- **Server authority**: ทุก damage/effect calculation ต้องมี `if (!IsServer) return;`
- **Animation event** ต้องมีใน AnimationEvent.cs ทุก skill state ที่ใช้ `OnStateRelease`
- **`IsStop = true`** คือวิธีเดียวในการ exit skill จากภายใน — ไม่ call Exit() ตรงๆ
- **CBS AbilityData** คือ source of truth สำหรับ cooldown, cost, range — ห้าม hardcode

## Alternatives Considered

### Alternative 1: Behavior Tree per Skill

- **Description**: แต่ละ skill เป็น Behavior Tree node graph
- **Pros**: Visual editing, non-programmer friendly
- **Cons**: Overhead สูง, debugging ยาก, ไม่เหมาะกับ real-time combat ที่ต้องการ performance
- **Rejection Reason**: Performance และ complexity สูงเกินไปสำหรับ 25+ heroes × 7 skills

### Alternative 2: Data-Driven Skill (ScriptableObject only)

- **Description**: ทุก skill เป็นแค่ data (ScriptableObject) + generic executor
- **Pros**: ไม่ต้องเขียน code ต่อ skill, designer สร้างได้เอง
- **Cons**: Hero-specific behavior (เช่น skill ที่เปลี่ยน form) ทำใน data ได้ยาก, workaround ซับซ้อน
- **Rejection Reason**: MOBA skills มีความซับซ้อนมากเกินไปสำหรับ pure data approach

### Alternative 3: Component per Skill Phase

- **Description**: แต่ละ phase (Cast, Perform, Exit) เป็น Component ที่ attach/detach
- **Pros**: Composable, reusable phases
- **Cons**: Runtime component add/remove มี GC pressure, ยาก debug state transitions
- **Rejection Reason**: GC overhead และ complexity ของ state management

## Consequences

### Positive

- Hero ใหม่ทำได้โดย subclass + override method ที่ต้องการ
- Lifecycle ที่ชัดเจน — ง่ายต่อการ debug และ test
- Animation + VFX + damage แยกออกจากกัน — test แต่ละส่วนได้

### Negative

- ทุก skill เป็น C# class — designer ไม่สามารถสร้าง skill ใหม่โดยไม่มี programmer
- Base class ขนาดใหญ่ (`ActorCombatAction.cs` 3000+ lines) — ยาก navigate

### Neutral

- AnimationEvent.cs ต้องมี method ต่อ skill state — boilerplate แต่ explicit

## Risks

| Risk | Probability | Impact | Mitigation |
|------|------------|--------|-----------|
| Base class ใหญ่เกินไป | เกิดแล้ว | ปานกลาง | Refactor เป็น partial class หรือ component ใน อนาคต |
| Animation event ลืมสร้าง | ปานกลาง | ต่ำ | Skill ยังทำงานได้ — แค่ไม่มี animation |
| Skill state desync | ต่ำ | สูง | Server-authoritative state + Fusion replication |

## Performance Implications

| Metric | Target |
|--------|--------|
| Skill execution latency | < 1 frame (16ms) |
| Active skills per frame | สูงสุด 50 (10 players × 5 skills) |
| GC allocation per skill | 0 bytes (pooling) |

## Validation Criteria

- [x] Q/W/E/R/Recall/Item skills ทุกตัวใช้ lifecycle เดียวกัน
- [x] Damage calculation เกิดบน server เท่านั้น
- [x] Skill interrupt (damage → IsStop) ทำงานถูกต้อง
- [x] Animation event → OnStateRelease chain ทำงานถูกต้อง
- [ ] Unit test coverage ≥ 80% สำหรับ lifecycle transitions

## Related

- [ADR-0002: Photon Fusion 2 networking](ADR-0002-photon-fusion-2-networking.md)
- [ADR-0005: ItemAnimationType animation routing](ADR-0005-item-animation-type-routing.md)
- `design/gdd/combat-skills-system.md`
- `Assets/GameScripts/Gameplays/Characters/ActorCombatAction.cs`
- `Assets/GameScripts/Gameplays/Characters/AnimationEvent.cs`
