# ADR-0003: PlayFab (CBS) + Azure Functions as Live Backend

## Status

Accepted

## Date

2024-01-01 (reconstructed from codebase)

## Decision Makers

Core development team

## Context

### Problem Statement

Delta ต้องการ live backend สำหรับ: account management, item/inventory, battle pass,
leaderboard, matchmaking metadata, และ server-side game logic เช่น Azure Functions
ต้องเลือก BaaS ที่รองรับ game-specific features และ scale ได้โดยไม่ต้องดูแล infrastructure เอง

### Current State

ไม่มี backend ก่อนหน้า — เป็นการตัดสินใจเริ่มต้นโปรเจกต์

### Constraints

- ทีมเล็ก — ไม่ต้องการดูแล server infrastructure เอง
- ต้องการ out-of-the-box: auth, inventory, leaderboard, economy
- Game data (item stats, ability values) ต้องปรับได้โดยไม่ต้อง redeploy client
- Custom server logic สำหรับ anti-cheat และ complex operations

### Requirements

- Account system: register, login, link Steam/social
- Inventory management: items, stacks, currencies
- Economy: virtual currency, store, receipts
- Battle pass: season progression, rewards
- Custom data: item stats, ability configs, hero stats ที่ live-tunable
- Server-side validation สำหรับ purchases และ rewards
- Azure Functions สำหรับ custom logic ที่ซับซ้อน

## Decision

ใช้ **PlayFab** (ผ่าน CBS — Cloud Backend System wrapper) เป็น BaaS หลัก
และ **Azure Functions** สำหรับ custom server-side logic

### Architecture

```
Client (Unity)
    │
    ├── MetadataService ──► PlayFab Title Data
    │                        (item configs, ability values, hero stats)
    │
    ├── CBSModule ──────────► PlayFab Player Data + Catalog
    │   ├── CBSItemInGame       (inventory, economy, purchases)
    │   ├── CBSAbility          (skill configs)
    │   ├── CBSUnit             (hero/monster stats)
    │   └── CBSSkin             (cosmetic data)
    │
    └── Azure Functions ────► Custom Logic
        ├── MatchResult()       (คำนวณ XP/gold หลัง match)
        ├── BattlePassReward()  (ให้รางวัล season pass)
        ├── AntiCheat()         (validate game events)
        └── CraftItem()         (server-side recipe validation)

PlayFab Title Data = Live-tunable game configs (no client redeploy)
PlayFab Catalog   = Item definitions + pricing
PlayFab CloudScript → Azure Functions (next-gen CloudScript)
```

### Key Interfaces

```csharp
// ดึง config ไอเทมจาก CBS
var item = DeltaService.I
    .GetService<MetadataService>()
    .GetCustomData<CBSItemInGame>(itemId);

// ดึง ability config
var ability = DeltaService.I
    .GetService<MetadataService>()
    .GetCustomData<CBSAbility>(abilityId);

// CBS data models (deserialized จาก PlayFab JSON)
public class CBSItemInGame : CBSItemCustomData
{
    public string ID;
    public int Price;
    public ItemType itemType;
    public ItemAnimationType AnimationType;
    // ...
}
```

### Implementation Guidelines

- **ห้าม hardcode** ค่า game balance ในโค้ด — ทุกอย่างต้องมาจาก CBS/MetadataService
- CBS data models (`CBSItemInGame`, `CBSAbility`, `CBSUnit`) คือ source of truth สำหรับ game data
- Azure Functions ใช้สำหรับ operations ที่ต้องการ server authority — ไม่ใช้ client-side เพียงอย่างเดียว
- Cache CBS data ใน MetadataService — ไม่ fetch PlayFab ทุก frame
- PlayFab Title Data update = live patch โดยไม่ต้อง redeploy app

## Alternatives Considered

### Alternative 1: Firebase (Google)

- **Description**: Firebase Auth + Firestore + Cloud Functions
- **Pros**: ค่าใช้จ่ายต่ำ, Firestore real-time sync ดีมาก, community ใหญ่
- **Cons**: ไม่มี game-specific features (inventory, economy, leaderboard), ต้องสร้างทุกอย่างเอง, ขาด PlayFab Catalog/Economy system
- **Rejection Reason**: ต้องพัฒนา game economy layer เองทั้งหมด — effort สูงมาก

### Alternative 2: Custom Backend (Node.js + PostgreSQL)

- **Description**: สร้าง backend เอง บน cloud (AWS/GCP/Azure)
- **Pros**: ยืดหยุ่นสูงสุด, ไม่มี vendor lock-in
- **Cons**: ต้องดูแล infrastructure, security, scaling เอง — ทีมเล็กไม่มี capacity
- **Rejection Reason**: overhead ด้าน DevOps สูงเกินไปสำหรับทีมขนาดนี้

### Alternative 3: GameSparks (AWS)

- **Description**: AWS GameSparks BaaS
- **Pros**: AWS infrastructure, scalable
- **Cons**: ยังอยู่ใน preview ช่วงที่ตัดสินใจ, documentation น้อย, community เล็ก
- **Rejection Reason**: immaturity และ uncertainty เรื่อง roadmap

## Consequences

### Positive

- Out-of-the-box: auth, inventory, economy, leaderboard — ลด dev time มาก
- Live-tunable configs ผ่าน PlayFab Dashboard — ปรับ balance โดยไม่ต้อง update app
- Azure Functions integrate กับ PlayFab ได้โดยตรง
- Microsoft/Azure infrastructure — SLA และ uptime สูง

### Negative

- Vendor lock-in กับ PlayFab — migration ยากถ้าต้องการเปลี่ยนในอนาคต
- CBS wrapper layer เพิ่ม abstraction — debug ยากขึ้นเล็กน้อย
- PlayFab pricing scale ตาม MAU — ต้องวางแผน cost

### Neutral

- ทุก game data model ต้อง serialize/deserialize เป็น JSON ตาม PlayFab format

## Risks

| Risk | Probability | Impact | Mitigation |
|------|------------|--------|-----------|
| PlayFab API deprecation | ต่ำ | สูง | Abstract ผ่าน MetadataService layer |
| Azure Functions cold start | ปานกลาง | ต่ำ | Keep-alive ping, async UX |
| Data schema migration | ปานกลาง | ปานกลาง | Version CBS models, backward-compatible changes |
| PlayFab outage | ต่ำ | สูง | Local cache fallback สำหรับ read-only data |

## Performance Implications

| Metric | Target |
|--------|--------|
| CBS data load (startup) | < 3s |
| Azure Function latency (p95) | < 500ms |
| PlayFab API calls per session | < 50 |

## Validation Criteria

- [x] Login, register, และ link Steam ทำงานผ่าน PlayFab Auth
- [x] Item stats โหลดจาก CBS และ apply กับ Actor ถูกต้อง
- [x] Azure Functions validate และ grant rewards หลัง match
- [ ] Live balance update ผ่าน Dashboard มีผลกับ client ภายใน 1 session

## Related

- [ADR-0001: Unity 2022.3.62f1 engine](ADR-0001-unity-engine-urp-csharp.md)
- `design/gdd/account-auth-system.md`
- `design/gdd/item-system.md`
- `design/gdd/battle-pass.md`
- `Assets/GameScripts/Datas/DataModel/Metadata/CBS/`
