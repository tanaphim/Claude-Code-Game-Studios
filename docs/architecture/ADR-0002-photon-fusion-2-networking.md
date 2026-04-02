# ADR-0002: Photon Fusion 2 for Multiplayer Networking

## Status

Accepted

## Date

2024-01-01 (reconstructed from codebase)

## Decision Makers

Core development team

## Context

### Problem Statement

Delta เป็น real-time MOBA ที่รองรับหลาย mode — เริ่มต้นด้วย 5v5 และมีแผน scale ขึ้นถึง 25v25
ต้องการ networking ที่มีความหน่วงต่ำ, server-authoritative state replication, client-side prediction,
และรองรับ dedicated server สำหรับ fairness และ scale ของ large-scale modes

### Current State

ไม่มี networking layer ก่อนหน้า — เป็นการตัดสินใจเริ่มต้นโปรเจกต์

### Constraints

- Engine ต้องเป็น Unity (ตาม ADR-0001)
- รองรับผู้เล่นพร้อมกัน: 10 คน (5v5) ในปัจจุบัน, ขยายถึง 50 คน (25v25) ในอนาคต
- Latency target: < 100ms สำหรับ gameplay actions
- Host model: **Dedicated Server** — ต้องการ server authority เต็มรูปแบบ, ไม่มี host advantage
- ทีมไม่มีประสบการณ์เขียน netcode จากศูนย์

### Requirements

- State replication อัตโนมัติสำหรับตัวละคร, projectile, และ game state
- Client-side prediction + server reconciliation สำหรับ player movement
- RPC support สำหรับ game events (damage, skill use, death)
- Interest management (AOI) สำหรับ performance — จำเป็นมากเมื่อ player count ขึ้นถึง 50
- Dedicated server: ไม่มี host advantage, server authority เต็มรูปแบบ
- Scale ได้ถึง 50 concurrent players ต่อ room (25v25)

## Decision

ใช้ **Photon Fusion 2** (Server Mode + Dedicated Server) เป็น networking framework หลัก

### Architecture

```
Photon Cloud (Relay / Matchmaking)
        │
        ▼
Dedicated Server (Photon Fusion 2 — Server Mode)
├── Server Mode
│   ├── Dedicated Server: Authority เต็มรูปแบบ, ไม่มี client มี authority
│   └── Clients: Input → Predict locally → Reconcile กับ server state
│
├── NetworkObject — ทุก entity ที่ sync กัน
│   ├── NetworkBehaviour — component ที่มี networked state
│   └── [Networked] properties — auto-replicated fields
│
├── RPC Methods — one-shot events
│   ├── RPC_Hit(damage, target)
│   ├── RPC_UseSkill(key, state)
│   └── RPC_Death(actorId)
│
├── INetworkInput — player input struct (polled every tick)
│   └── InputMessage { MoveDirection, SkillKey, TargetPosition }
│
└── Interest Management (AOI)
    └── จำเป็นสำหรับ 25v25 — ลด bandwidth โดย replicate เฉพาะ entity ที่อยู่ใกล้
```

### Key Interfaces

```csharp
// Network entity — ทุก actor ใช้
public class Actor : NetworkBehaviour
{
    [Networked] public float Health { get; set; }
    [Networked] public NetworkBool IsAlive { get; set; }

    public override void FixedUpdateNetwork()
    {
        if (GetInput(out InputMessage input))
            ProcessInput(input);
    }
}

// Input struct — poll ทุก tick
public struct InputMessage : INetworkInput
{
    public Vector3 MoveDirection;
    public SkillKey SkillKey;
    public Vector3 TargetPosition;
}

// Authority check — ใช้ก่อนทุก server-side operation
if (!IsServer) return;
if (!HasInputAuthority) return;
```

### Implementation Guidelines

- ใช้ `[Networked]` สำหรับ state ที่ client ทุกคนต้องเห็นตรงกัน
- ใช้ RPC สำหรับ one-shot events (damage, effects) — ไม่ใช้กับ state ที่เปลี่ยนบ่อย
- `IsServer` check ก่อนทุก authoritative action — ห้าม client เปลี่ยน game state ตรงๆ
- `HasInputAuthority` check สำหรับ local player input processing
- ห้าม allocate ใน `FixedUpdateNetwork()` — ใช้ object pooling

## Alternatives Considered

### Alternative 1: Unity Netcode for GameObjects (NGO)

- **Description**: Unity's official networking solution + Relay service
- **Pros**: Official support, integrate กับ Unity Gaming Services
- **Cons**: ตอนตัดสินใจ NGO ยังอยู่ใน early stage, feature ไม่ครบ, performance ต่ำกว่า Fusion 2 สำหรับ real-time action game
- **Rejection Reason**: ยัง immature, ขาด client prediction ที่แข็งแกร่ง

### Alternative 2: Mirror Networking

- **Description**: Open-source HLAPI networking สำหรับ Unity
- **Pros**: Free, community ใหญ่, flexible
- **Cons**: ต้องเขียน prediction และ reconciliation เอง, ไม่มี cloud relay built-in, ต้องจัดการ server infrastructure เอง
- **Rejection Reason**: effort สูงมากในการ implement prediction + dedicated server

### Alternative 3: Photon Fusion 1

- **Description**: รุ่นก่อนหน้าของ Photon Fusion
- **Pros**: มี documentation และ examples มากกว่า
- **Cons**: Fusion 2 มี API ที่ clean กว่า, performance ดีกว่า, Fusion 1 อยู่ใน maintenance mode
- **Rejection Reason**: Fusion 2 เป็น current recommended version

## Consequences

### Positive

- State replication อัตโนมัติ — ลด boilerplate code มาก
- Built-in client prediction ลด perceived latency
- Server Mode รองรับ dedicated server — ไม่มี host advantage, fair สำหรับทุก client
- Photon Fusion 2 รองรับ player count สูงพร้อม AOI — scale ถึง 25v25 ได้

### Negative

- ค่าใช้จ่าย Photon Cloud + Dedicated Server infrastructure เพิ่มตาม DAU
- ต้องดูแล dedicated server deployment และ scaling
- API เปลี่ยนบ่อยใน minor versions — ต้องติดตาม release notes
- 25v25 ต้องการ AOI tuning อย่างระมัดระวัง — replicate ผิดจะ desync

### Neutral

- ทุก networked entity ต้อง inherit `NetworkBehaviour` — เพิ่ม coupling กับ Fusion API

## Risks

| Risk | Probability | Impact | Mitigation |
|------|------------|--------|-----------|
| Photon Cloud downtime | ต่ำ | สูง | Implement reconnect logic, monitor SLA |
| Photon ปรับราคา | ปานกลาง | ปานกลาง | ออกแบบ networking layer ให้ swap ได้ |
| Dedicated server cost scale ที่ 25v25 | ปานกลาง | ปานกลาง | วาง cost model ก่อน launch large-scale mode |
| AOI misconfiguration ทำให้ desync | ปานกลาง | สูง | Test AOI อย่างละเอียดก่อน launch 25v25 |
| Tick rate ไม่พอที่ 50 players | ปานกลาง | สูง | Benchmark ที่ player count สูงสุด; ปรับ tick rate ต่อ mode |

## Performance Implications

| Metric | Target |
|--------|--------|
| Tick rate | 30–60 Hz (ปรับต่อ mode) |
| Bandwidth per player | < 20 KB/s (5v5), < 15 KB/s (25v25 with AOI) |
| Latency (dedicated server) | < 100ms ระหว่าง region |
| Max concurrent players per room | 10 (5v5) / 50 (25v25) |

## Validation Criteria

- [x] 10 players sync state ได้พร้อมกันโดยไม่มี desync (5v5)
- [x] Player movement มี client prediction — รู้สึก responsive แม้ latency 100ms
- [x] Damage calculation server-authoritative — client ไม่สามารถ cheat ค่า damage
- [ ] Dedicated server deploy และ run ได้บน cloud infrastructure
- [ ] 50 players sync state ได้พร้อมกันโดยไม่มี desync (25v25)
- [ ] AOI ทำงานถูกต้อง — replicate เฉพาะ entity ในรัศมีที่กำหนด
- [ ] Bandwidth < 15 KB/s per player ที่ 25v25 + 60 Hz

## Related

- [ADR-0001: Unity 2022.3.62f1 engine](ADR-0001-unity-engine-urp-csharp.md)
- [ADR-0004: ActorCombatAction skill pipeline](ADR-0004-actor-combat-action-skill-pipeline.md)
- `design/gdd/networking-core.md`
- `Assets/GameScripts/Gameplays/Characters/` — NetworkBehaviour implementations
