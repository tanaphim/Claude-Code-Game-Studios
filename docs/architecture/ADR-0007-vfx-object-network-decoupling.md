# ADR-0007: vfxObject — NetworkObject → GameObject Decoupling

## Status

Proposed

## Date

2026-05-07

## Last Verified

2026-05-07

## Decision Makers

- User (project lead)
- technical-director (pending review)
- network-programmer (pending review)
- unity-specialist (pending review)

## Summary

ย้าย VFX prefabs ที่เป็น visual-only ออกจาก Photon Fusion `NetworkObject` ไปเป็น
plain Unity `GameObject` ที่ instantiate locally per-client เพื่อลด network bandwidth,
spawn cost และ replication overhead. VFX ที่ต้องการ network-replicated state
(เช่น damage zones ที่ persist ข้าม tick) ยังคงเป็น `NetworkObject`.

## Engine Compatibility

| Field | Value |
|-------|-------|
| **Engine** | Unity 2022.3.62f1 + Photon Fusion 2 |
| **Domain** | Networking + Rendering (VFX) |
| **Knowledge Risk** | LOW — Unity 2022.3 LTS, Fusion 2 stable APIs in training data |
| **References Consulted** | `docs/engine-reference/unity/VERSION.md` |
| **Post-Cutoff APIs Used** | None |
| **Verification Required** | (1) Multipeer harness Pass #4/#5 still green after migration; (2) VFX visible identically on Host + Client for converted prefabs |

## ADR Dependencies

| Field | Value |
|-------|-------|
| **Depends On** | [ADR-0002](ADR-0002-photon-fusion-2-networking.md) Photon Fusion 2 networking; [ADR-0006](ADR-0006-unified-ability-system.md) Unified Ability System (uses VFX hooks) |
| **Enables** | E-07 (AOE shapes — cleaner SkillObject decoupling); reduced replication cost for E-05 (Stat/StatusEffect NetworkStruct profiling baseline) |
| **Blocks** | None — additive migration |
| **Ordering Note** | Should ship after ADR-0006 Phase 2 (Sprint 005) closes to avoid concurrent ability + VFX refactor in same sprint |

## Context

### Problem Statement

`vfxObject` (visual effect prefabs spawned by skill cast/hit/perform animations)
ปัจจุบันเป็น Photon Fusion `NetworkObject`. ทุกครั้งที่ skill ทำงาน:

1. Server `Runner.Spawn()` → allocate NetworkObject
2. Replicate spawn event ไปทุก client
3. Replicate Transform updates (ถ้า moving) ทุก tick
4. Despawn replicate กลับไปทุก client
5. Server + clients pay GC + memory cost

ส่วนใหญ่ของ VFX ที่ใช้:
- ✦ **Particle bursts** (spell cast flash, hit spark) — visual only, fire-and-forget
- ✦ **Decals** (impact crater, slow zone footprint) — visual only, faded ตามเวลา
- ✦ **Trails** (projectile streak) — follow projectile owner ที่เป็น NetworkObject อยู่แล้ว

มีแค่ส่วนน้อยที่ต้องการ network state จริง:
- ⚠️ **Persistent damage zones** (เช่น Hercules R AOE pool ที่อยู่ 5 วินาที) — server
  ต้อง authoritative ว่าใครอยู่ในโซน
- ⚠️ **Status-applying volumes** ที่ใช้ proximity check → ต้องการ shared transform

### Current State

จาก audit pattern ใน codebase (Phase 1b verification):
- VFX spawn ผ่าน `Runner.Spawn(vfxPrefab, ...)` ใน `ActorCombatAction` Pattern A/B
- VFX prefab ทุกตัวที่ตรวจตอน Phase 1b มี `NetworkObject` component
- ยังไม่มี categorization ว่าตัวไหนต้อง state จริง vs visual-only

(ก่อน implement ต้อง audit จริง — รายละเอียดใน Migration Plan §1)

### Constraints

- **Photon Fusion 2** — Tick-rate-based replication; NetworkObject spawn เกี่ยวข้องกับ
  AOI (Area of Interest) calculations
- **Unity 2022.3 LTS** — particle systems, Animator events, prefab pooling stable
- **Existing animation event hooks** — VFX มัก spawn จาก `AnimationEvent` shim methods
  (`PerformQ`, `OnHit`, etc.) → ต้องเข้ากับ ADR-0006 Phase 2 §6.2 Option A
- **Multipeer harness regression** — ทุก hero ที่มี VFX ต้องผ่าน Pass #4/#5

### Requirements

- VFX spawn cost ลดลงสำหรับ visual-only prefabs (target: ลด NetworkObject spawn ≥80%
  ใน steady-state combat)
- Visual fidelity ไม่ลด — VFX ปรากฏที่ตำแหน่ง + เวลาเดียวกันบน Host + Client (tolerance
  ±1 tick = ±33ms ที่ 30Hz)
- Migration เป็น additive — เก่าใช้ได้ระหว่าง migrate ทีละ prefab
- ไม่ต้องแก้ Animation Clip ใดๆ (เก็บ event method names เดิม)

## Decision

แบ่ง VFX เป็น 2 หมวดด้วย explicit categorization บน prefab:

### Category A — Local VFX (`GameObject` + `LocalVfxSpawner`)

**ลักษณะ:**
- Visual-only — ไม่ต้องการ shared state, ไม่กระทบ gameplay logic
- Fire-and-forget lifetime (auto-destroy ตาม particle duration)
- ไม่มี collider ที่ apply damage/status
- Examples: cast flash, hit spark, dust puff, impact decal

**Spawn path:**
- `Runner.IsServer || Runner.LocalPlayer` ทุก peer instantiate locally ตอน
  `AnimationEvent` หรือ ability lifecycle hook trigger
- ใช้ `Object.Instantiate()` (plain Unity), ไม่ใช่ `Runner.Spawn()`
- Position/rotation มาจาก spawn-time snapshot (anchor transform ของ Actor ที่
  เป็น NetworkObject อยู่แล้ว)

### Category B — Networked VFX (`NetworkObject`, ไม่เปลี่ยน)

**ลักษณะ:**
- มี gameplay-affecting state (damage tick, status apply, area query)
- Persistent across multiple ticks ที่ต้อง server authority
- Examples: persistent AOE damage zone, status-applying field, network-tracked
  projectile (ถ้ายังไม่มี separate `Projectile` NetworkObject)

**Spawn path:** เดิม (`Runner.Spawn()`)

### Architecture

```
SkillObject / AnimationEvent
        │
        ├─── visual-only? ──► LocalVfxSpawner.Spawn(prefab, anchor)
        │                          │
        │                          └─► Object.Instantiate (per-client)
        │                              auto-destroy on particle finish
        │
        └─── needs state? ───► Runner.Spawn(prefab, ...)  (unchanged)
                                   │
                                   └─► NetworkObject lifecycle (unchanged)
```

### Key Interfaces

```csharp
// New service — local VFX spawner (registered via DeltaService like AbilityRegistry)
public interface ILocalVfxSpawner
{
    GameObject Spawn(GameObject vfxPrefab, Vector3 position, Quaternion rotation,
                     Transform parent = null, float overrideDuration = -1f);
    void Despawn(GameObject instance);  // optional manual despawn
}

// On VFX prefab — categorization marker (ScriptableObject metadata or component)
[CreateAssetMenu(menuName = "VFX/Vfx Metadata")]
public class VfxMetadata : ScriptableObject
{
    public VfxCategory Category;        // Local | Networked
    public float DefaultLifetime;       // for Local; 0 = use particle duration
    public bool FollowAnchor;           // attach to anchor transform after spawn
}

public enum VfxCategory { Local, Networked }
```

### Implementation Guidelines

1. **Categorization is explicit**, not inferred from prefab contents — `VfxMetadata.Category`
   field ระบุชัดเจน. Default = `Local` (lighter path); designer flag `Networked`
   เมื่อจำเป็น
2. **`LocalVfxSpawner` is a `DeltaService`** — registered globally, accessed via
   `DeltaService.Get<ILocalVfxSpawner>()` (เข้ากับ AbilityRegistry pattern จาก S3-02)
3. **Anchor handling** — ถ้า `FollowAnchor = true`, parent ไปที่ Actor's anchor
   transform (NetworkBehaviour ที่อยู่บน Actor); position sync ผ่าน Unity's parent
   transform — ไม่ต้อง replicate
4. **Spawn timing tolerance** — local instantiate ที่ animation event tick → ทุก
   peer fire ที่ animation tick เดียวกัน (animation state replicate ผ่าน Fusion อยู่แล้ว)
   → tolerance ±1 tick acceptable
5. **No `OnSpawned` RPC needed** — ถ้า VFX เกิดจาก animation event, animation
   replication คือ sync mechanism

## Alternatives Considered

### Alternative 1: Keep ทุก vfxObject เป็น NetworkObject

- **Description**: ไม่เปลี่ยนอะไร, ยอม cost
- **Pros**: zero migration cost, no risk
- **Cons**: bandwidth waste (~80%+ ของ VFX spawn เป็น visual-only), GC pressure,
  AOI calculations ขยายไม่จำเป็น
- **Estimated Effort**: 0d
- **Rejection Reason**: ไม่แก้ปัญหาที่ user ระบุ; cost compound เมื่อ skill count + hero count เพิ่ม

### Alternative 2: ใช้ Fusion `Runner.Spawn` flag `IsNetworked = false`

- **Description**: Fusion มี option ให้ spawn NetworkObject แต่ skip replication
- **Pros**: minimal API change
- **Cons**: ยังมี NetworkObject overhead (allocation, AOI registration); ยัง
  serialize เข้า scene state; misleading API (ดูเหมือน networked แต่ไม่ใช่)
- **Estimated Effort**: 1d
- **Rejection Reason**: ไม่ใช่ root-cause fix — ยังเป็น NetworkObject เพียงแค่ปิด replicate

### Alternative 3: RPC-driven local spawn (server fires `RPC_SpawnLocalVfx` → all clients)

- **Description**: Server authority decide เมื่อ VFX spawn, RPC broadcast ไป client
- **Pros**: server authoritative timing
- **Cons**: extra RPC bandwidth (มากกว่า animation replication ที่มีอยู่แล้ว);
  redundant — animation event ทำหน้าที่เดียวกันได้แล้ว; เพิ่ม latency 1 round-trip
- **Estimated Effort**: 2d
- **Rejection Reason**: ขัดกับหลัก "ใช้กลไก replication ที่มีอยู่แล้ว"; animation event
  เป็น natural sync point

## Consequences

### Positive

- ลด NetworkObject spawn ใน steady combat ≥80% (ประมาณการ — verify ตอน profiling)
- ลด AOI calculations + GC churn
- ลด replicate bandwidth สำหรับ visual-only effects
- VFX prefab spawn pool ทำง่ายขึ้น (Object Pool บน plain GameObject ดีกว่า NetworkObject)
- E-07 (AOE shapes) ทำง่ายขึ้น เพราะ shape preview/visual แยกจาก authority shape

### Negative

- Designer ต้องเข้าใจ category — VfxMetadata field ใหม่บน prefab ทุกตัว
- ถ้า miscategorize Networked → Local จะ break gameplay (damage zone หาย); ต้องการ
  validation rule ใน editor
- Migration ต้อง audit prefab ทุกตัว (~? VFX prefabs — ยังไม่นับ)
- VFX ที่ spawn เร็วมาก (1 frame) อาจ desync ±1 tick ระหว่าง peers — acceptable
  สำหรับ visual แต่ต้อง playtest ยืนยัน

### Neutral

- Local VFX ไม่ปรากฏใน Fusion replay/recording (ถ้ามีระบบ replay) — แต่ animation
  event reproducible อยู่แล้ว → reconstructible
- Spawn pool strategy ต้องเลือก (Unity built-in ObjectPool vs custom) — แยก decision

## Risks

| Risk | Probability | Impact | Mitigation |
|------|------------|--------|-----------|
| Miscategorized prefab (Networked → Local) breaks damage application | Medium | High | Editor validation rule: prefab with `Collider` ที่ใช้ damage layer must be Networked; CI check |
| Animation event timing ไม่ตรงกัน Host vs Client → VFX desync visible | Low | Low | Visual-only acceptable ±1 tick; playtest กับ 100+ms latency before merge |
| Existing `Runner.Spawn(vfxPrefab)` call sites ต้อง refactor manual ทีละจุด | High (by design) | Low | Phased migration — keep both paths during migrate, ADR enforces new VFX uses Local by default |
| `LocalVfxSpawner` lifetime management leak (instance ไม่ถูก destroy) | Medium | Medium | Default behavior: auto-destroy on particle finish via coroutine; explicit `Despawn` API for edge cases; tests |
| VFX prefab pooling แตกต่างระหว่าง Local + Networked ทำให้ memory pattern เปลี่ยน | Low | Medium | Profile before/after migration; budget memory increase ≤5MB per match |

## Performance Implications

| Metric | Before (estimated) | Expected After | Budget |
|--------|--------|---------------|--------|
| NetworkObject spawn rate (steady combat) | TBD ops/sec | ≤20% of before | measured baseline -80% |
| Bandwidth (VFX-related) | TBD KB/s | ≤30% of before | measured baseline -70% |
| GC alloc per VFX spawn | NetworkObject overhead | Plain GameObject + particle | -30% per spawn |
| Memory (VFX prefabs in scene) | NetworkObject scene state | Plain GameObject | similar or +5MB pooled |

> **Note:** Concrete baselines ต้อง profile ก่อน implement (Phase 1 ของ migration plan)

## Migration Plan

### Phase 1 — Audit + categorize (Sprint 006, ~0.5d)

1. List ทุก prefab ที่ spawn ผ่าน `Runner.Spawn` ใน ability/skill code paths
2. Categorize แต่ละตัว: Local vs Networked + rationale
3. Profile baseline match (10 min, 2 player) — record VFX spawn rate, bandwidth, GC
4. Output: audit doc `docs/architecture/ADR-0007-vfx-audit.md` (สร้างตอน implement)

### Phase 2 — Build infrastructure (Sprint 006, ~0.5d)

5. Implement `ILocalVfxSpawner` + default impl + `DeltaService` registration
6. Add `VfxMetadata` ScriptableObject + editor inspector for prefabs
7. Editor validation rule: damage-layer collider → must be Networked
8. Unit tests: spawn/despawn lifecycle, parent attachment, auto-cleanup

### Phase 3 — Migrate Hercules VFX (Sprint 007, ~0.5d)

9. หลัง Phase 2 (ADR-0006) ปิด → Hercules VFX prefab list (1 hero) เป็น pilot
10. Categorize + flip metadata + update spawn call sites
11. Multipeer harness Pass #4/#5 regression check
12. Manual playtest 1 match (Training, 1 player, Hercules)

### Phase 4 — Migrate remaining heroes (Sprint 008+, ~1d)

13. ทยอย hero ละ batch ตาม Phase 3 recipe
14. Re-profile หลังทุก batch — track delta

### Phase 5 — Cleanup (Sprint 009+, ~0.25d)

15. Remove deprecated NetworkObject components from Local-categorized prefabs
16. Update control manifest: "VFX prefabs default to Local; Networked requires `VfxMetadata.Networked` + reason in ADR"

**Rollback plan:** Each phase commits separately. Rollback = `git revert` ของ commits
ที่กระทบ; `VfxMetadata.Category = Networked` ทุกตัวกลับเป็น `Runner.Spawn` path
(infrastructure คงอยู่แต่ไม่มี caller).

## Validation Criteria

- [ ] Audit doc lists ทุก VFX prefab + category + rationale
- [ ] Baseline profile (before) recorded → measured-after profile shows ≥70% bandwidth
      reduction in VFX-related traffic
- [ ] Multipeer harness Pass #4 + Pass #5 ยังผ่าน หลัง Hercules migration
- [ ] Manual playtest 1 match: Hercules Q/W/E/R + N + Recall ทุก VFX ปรากฏถูกต้อง
      ทั้ง Host + Client
- [ ] ไม่มี `Runner.Spawn(vfxPrefab)` ใน Hercules ability/skill code paths หลัง Phase 3
- [ ] Editor validation rule trigger เมื่อ misconfigure prefab (test fixture)
- [ ] No regression: GC alloc per VFX spawn ≤ baseline; memory steady-state ≤ baseline +5MB

## GDD Requirements Addressed

| GDD Document | System | Requirement | How This ADR Satisfies It |
|-------------|--------|-------------|--------------------------|
| `design/gdd/networking-core.md` (assumed) | Networking | "Bandwidth budget per match must stay within target" | Reduces VFX-driven replication; preserves headroom for gameplay state |
| `design/gdd/combat.md` (assumed) | Combat | "Visual feedback must match server-authoritative timing" | Animation event sync replaces explicit RPC; maintains parity |

> Foundational performance/architecture decision. If GDD docs missing budget figures,
> add them as part of Phase 1 audit (concrete baseline numbers).

## Related

- [ADR-0002 Photon Fusion 2 Networking](ADR-0002-photon-fusion-2-networking.md) — networking foundation
- [ADR-0006 Unified Ability System](ADR-0006-unified-ability-system.md) — ability stack that owns VFX hooks
- [ADR-0006 Phase 2 Migration Plan](ADR-0006-phase-2-migration-plan.md) §6.2 AnimationEvent Option A — VFX trigger path
- [Architecture Epics Backlog E-06](../../production/backlog/architecture-epics.md) — epic source
- [Architecture Epics Backlog E-05](../../production/backlog/architecture-epics.md) — Stat/StatusEffect NetworkStruct (related but separate decision)
