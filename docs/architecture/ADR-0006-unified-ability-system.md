# ADR-0006: Unified Ability System (Data-Driven Binding)

## Status

Accepted

## Date

2026-04-19

## Last Verified

2026-04-20

## Decision Makers

Core development team. Migration audit reviewed and approved 2026-04-20
(see [ADR-0006-migration-audit.md](ADR-0006-migration-audit.md)).

## Summary

แก้ `SkillKey` enum binding (Q/W/E/R/I/A/Item/Recall) ที่ผูกตายตัวใน 30+ ไฟล์
ให้เป็น string `AbilityId` lookup ผ่าน `AbilityRegistry` + `AbilityComponent`
เพื่อปลดล็อก variable slot count, runtime skill swap, boss phase ability swap,
และโหมด Ability Draft ในอนาคต โดยไม่รื้อ `ActorCombatAction` pipeline เดิมหรือ
SO/CBS layer ที่มีอยู่แล้ว

## Engine Compatibility

| Field | Value |
|-------|-------|
| **Engine** | Unity 2022.3.62f1 |
| **Domain** | Scripting / Networking / Core |
| **Knowledge Risk** | LOW — Unity 2022.3 LTS, well within LLM training data |
| **References Consulted** | `docs/engine-reference/unity/VERSION.md`, ADR-0002, ADR-0003, ADR-0004 |
| **Post-Cutoff APIs Used** | None |
| **Verification Required** | Fusion 2 `NetworkString<_64>` hashing cost under load (10 bots × 5 abilities × 60Hz); dictionary lookup of ability prefabs vs current enum-keyed dictionary |

## ADR Dependencies

| Field | Value |
|-------|-------|
| **Depends On** | ADR-0002 (Photon Fusion 2), ADR-0003 (PlayFab CBS backend) |
| **Enables** | Epic: Ability Draft mode; Epic: Hero Bot AI v2 (tag-based scoring); Story: Keybind Remap in Settings; Story: Boss Phase-Aware Ability Set |
| **Blocks** | Any new hero work that would add more hero-specific `ActorCombatAction` subclasses bound to `SkillKey` (to avoid growing migration debt) |
| **Ordering Note** | Partially updates ADR-0004 — `ActorCombatAction` lifecycle is preserved; only the binding/lookup layer changes. ADR-0004 stays Accepted; its Alternative 2 rejection rationale is revisited in this ADR |

## Context

### Problem Statement

Delta มี roster 20+ heroes (Aphrodite..WooChi) แต่ละ hero + monster + boss + item
ผูกอยู่กับ enum `SkillKey { I, A, Q, W, E, R, Item, Recall }` ที่ hardcoded ใน 30+
ไฟล์ ทำให้:

1. **จำนวนสกิลต่อ entity ตายตัว** — ไม่รองรับ hero ใหม่ที่อาจมี 5-6 abilities
   หรือ boss ที่มีสกิลเยอะเปลี่ยนตาม phase
2. **ปุ่มผูกกับ ability** — player ไม่สามารถ remap Q↔W ใน settings ได้ถ้าไม่
   refactor input layer
3. **Ability swap เป็นไปไม่ได้** — ไม่สามารถเอาสกิล hero อื่นมาใส่ช่องอื่น
   (Rubick-style steal, Ability Draft mode, spell stealer item)
4. **Boss phase swap ต้อง workaround** — ตอนนี้ใช้ prefab naming convention
   (`boss_icedragon_phase1_q`, `phase2_q`) แต่ยังผูก SkillKey อยู่
5. **Bot AI tuning ต่อ ability ยุ่งยาก** — `FuzzySkillAI` อ่าน weights จาก
   `CBSAbility` ได้ดี แต่ priority queue ยัง hardcode `R > Q > W > E`

การเพิ่ม hero ใหม่ทุกตัวทำให้หนี้เพิ่ม — ต้องตัดสินใจก่อนลงทุน hero/content เพิ่มเติม

### Current State

โปรเจคมี **3-layer architecture ที่ออกแบบถูกต้อง** อยู่แล้ว:

```
┌─────────────────────────────────────────────────────────────────────┐
│ 1. CBSAbility (design data — PlayFab CBS)                           │
│    key: "woochi_q" → TargetingInput, DamageType, Range, Cooldowns, │
│                      Costs, Abilitys[] formulas,                    │
│                      AI weights (damage/aoe/escape/heal/execute/buff)│
│                                                                     │
│ 2. SpellObject (ScriptableObject — assets only + LayerMask)        │
│    TargetMask (LayerMask, CBS ไม่รองรับ), FactionType,              │
│    Indicator, NetworkIndicator, StatusEffectDictionary, Sounds      │
│                                                                     │
│ 3. SkillObject (NetworkBehaviour prefab per ability)                │
│    Assets/Resources/Prefabs/Gameplay/Spell/{Hero}/{hero}_{slot}.prefab│
│    - Lookup CBSAbility runtime by AbilityId (NetworkString<_64>)   │
│    - Already supports variants: merlin_q_fire/ice/dark             │
│    - Already supports phase: boss_icedragon_phase1_q / phase2_q    │
└─────────────────────────────────────────────────────────────────────┘

Binding layer (the blocker):
  SkillObjectDictionary : UnitySerializedDictionary<SkillKey, ...>
  Input(Q) → SkillKey.Q → Dictionary[Q] → prefab
  CBSAbility.SkillKey field → enum binding baked into design data too
```

กล่าวอีกนัยหนึ่ง: infrastructure string-based lookup **มีอยู่แล้ว** (AbilityId
เป็น `NetworkString<_64>` บน `SkillObject`) แต่ entry point ยังถูก enum บังคับ

### Constraints

- **Fusion 2 determinism**: ค่า ability ต้องไม่เปลี่ยนกลางแมตช์ (desync)
- **CBS tool limitation**: ไม่รองรับ `LayerMask` — `TargetMask` ต้องอยู่ SO
- **Backward compatibility**: 20+ heroes shipping; ห้าม break ระหว่าง migration
- **ActorCombatAction ใหญ่ 3000+ บรรทัด** (ADR-0004 tech debt) — ห้ามแตะ lifecycle
  ใน ADR นี้ จำกัด scope ไว้ที่ binding layer
- **Photon Fusion input struct** `InputMessage.Buttons` ใช้ SkillKey enum →
  ต้องมี migration ที่ไม่ break replication

### Requirements

- **Variable slot count (0..N)** ต่อ entity — monster = 0 สกิล, boss = หลาย phase
  ละหลายสกิล, hero อนาคต 5-6 สกิล
- **Runtime mutable ability list** — boss เปลี่ยน abilities ตาม phase, item เติม
  ability ชั่วคราว, Rubick-style steal
- **Keybind remap** ใน settings — key → slotIndex เป็นการ binding แยกชั้น
- **Unified model** ใช้กับ hero / monster / miniboss / boss / item active เหมือนกัน
- **Designer-tunable AI** ผ่าน CBS weights ต่อ ability (มีอยู่แล้วใน `CBSAbility`
  weight fields) ไม่ต้องแตะโค้ด
- **ต้องรองรับ Merlin element variants** post-migration (q_fire/ice/dark)
- **Performance**: ability dispatch ต้องไม่ช้ากว่า enum lookup เดิมเกิน 1 frame
  (< 1ms per cast at 10 bots × 60Hz)

## Decision

ใช้ **`AbilityId` (string) เป็น primary binding key** แทน `SkillKey` enum
โดยเพิ่ม 3 ชั้น indirection ที่ decouple input/slot/ability ออกจากกัน และ
**ปล่อย `SkillKey` enum ให้เป็น "display hint / category" เท่านั้น**

### Architecture

```
┌───────────────────────────────────────────────────────────────────┐
│ INPUT LAYER                                                       │
│                                                                   │
│  Keyboard/Gamepad press                                           │
│        │                                                          │
│        ▼                                                          │
│  KeybindMap (player settings, client-side)                        │
│    KeyCode → slotIndex                                            │
│    e.g. Q → 2, W → 3, E → 4, R → 5                               │
│        │                                                          │
│        ▼                                                          │
│  InputMessage.SlotIndex (byte, replaces SkillKey flag)           │
└────────────────────────────────┬──────────────────────────────────┘
                                 │
                                 ▼ Fusion OnInput (server)
┌───────────────────────────────────────────────────────────────────┐
│ SLOT LAYER (per entity)                                           │
│                                                                   │
│  AbilityComponent (NetworkBehaviour on every Actor)              │
│    [Networked] AbilitySlot[] Slots    (variable length 0..N)    │
│                                                                   │
│  AbilitySlot { NetworkString<_32> AbilityId; byte Rank; ... }    │
│                                                                   │
│  Cast(slotIndex):                                                 │
│    abilityId = Slots[slotIndex].AbilityId                         │
│    → AbilityRegistry lookup                                       │
│    → fire ActorCombatAction (unchanged pipeline from ADR-0004)    │
└────────────────────────────────┬──────────────────────────────────┘
                                 │
                                 ▼
┌───────────────────────────────────────────────────────────────────┐
│ RESOLUTION LAYER                                                  │
│                                                                   │
│  AbilityRegistry (global, loaded once at boot)                    │
│    Dictionary<string, AbilityDefinition> by AbilityId             │
│                                                                   │
│  AbilityDefinition {                                              │
│    string AbilityId;            // "merlin_q_fire"               │
│    SpellObject SpellAssets;     // SO asset ref                  │
│    GameObject SkillPrefab;      // spawned per-cast               │
│    SkillKey DisplayCategory;    // hint for UI (Q/W/E/R icon)    │
│  }                                                                │
│                                                                   │
│  CBSAbility (existing, unchanged) fetched by AbilityId           │
│    snapshot into CachedAbilityData dict at match start           │
└───────────────────────────────────────────────────────────────────┘
```

### Key Interfaces

```csharp
// New — binding/settings (client-side, not networked)
public class KeybindMap
{
    Dictionary<KeyCode, byte> KeyToSlot;
    byte Resolve(KeyCode key);
    void Remap(KeyCode key, byte slotIndex);   // settings UI
}

// New — per-entity ability container (replaces SkillObjectDictionary binding)
public class AbilityComponent : NetworkBehaviour
{
    [Networked, Capacity(8)] public NetworkArray<AbilitySlot> Slots { get; }

    public void SetSlot(byte index, string abilityId);   // runtime swap
    public bool TryCast(byte slotIndex, out AbilityDefinition def);
    public void SwapPhase(IEnumerable<string> newAbilityIds);  // boss phase
}

// New — registry (boot-time asset catalog, readonly at runtime)
public static class AbilityRegistry
{
    bool TryGet(string abilityId, out AbilityDefinition def);
    IEnumerable<string> AllIds();   // for Ability Draft pool
}

// Deterministic per-match snapshot of CBS design data
public class AbilityDataSnapshot
{
    void BuildAtMatchStart(IEnumerable<string> usedAbilityIds);
    CBSAbility Get(string abilityId);   // readonly for match duration
}

// Existing (unchanged from ADR-0004) — reference only
public class ActorCombatAction { /* lifecycle untouched */ }
```

**Input struct migration** (`InputMessage`):

```csharp
// Before (current)
public struct InputMessage : INetworkInput
{
    public NetworkButtons Buttons;   // Q, W, E, R, Item, Recall flags
}

// After
public struct InputMessage : INetworkInput
{
    public NetworkButtons Buttons;   // Only non-ability buttons (Recall, B-to-base)
    public byte PressedSlot;         // 0xFF = no cast, else slot index
}
```

### Implementation Guidelines

- **`AbilityId` is the single source of truth** สำหรับ binding — ห้าม lookup
  ability ด้วย `SkillKey` ในโค้ดใหม่
- **`SkillKey` enum คงอยู่** แต่เป็น display/category hint เท่านั้น (เช่น render
  icon "R" บน ultimate slot, animation state name routing) ห้ามใช้เป็น dictionary
  key
- **`CBSAbility.SkillKey` field** deprecate เป็น "default/suggested category" —
  migrate value ให้ตรงกับ `AbilityDefinition.DisplayCategory` แทน และ remove
  field ใน CBS schema เมื่อ migration complete
- **Match snapshot rule**: อ่าน `CBSAbility` ผ่าน `AbilityDataSnapshot` เท่านั้น
  ห้ามเรียก `MetadataService.GetCustomData<CBSAbility>` ระหว่างแมตช์ (non-deterministic)
- **SO vs CBS rule** (sticky): `SpellObject` SO เก็บได้เฉพาะ asset refs + Unity-
  specific types ที่ CBS tool ไม่รองรับ (เช่น `LayerMask`) + field ที่ใช้คู่
  กับ types เหล่านั้น (เช่น `FactionType` คู่ `TargetMask`) ถ้า CBS tool รองรับ
  LayerMask ในอนาคต → ย้ายทั้งคู่ไป CBS
- **AI consumption**: `FuzzySkillAI` อ่าน weights ผ่าน `AbilityDataSnapshot` โดยใช้
  `AbilityId` ไม่ใช่ `SkillKey` priority queue — ทำให้ scoring ใช้ได้กับ
  monster/boss/hero อย่างเดียวกัน
- **Merlin variants**: `AbilityComponent.SetSlot(2, "merlin_q_fire")` เปลี่ยน
  element ใน-match ได้ผ่าน runtime swap — ระบบเดิมใช้ prefab naming ตรงกับ
  AbilityId จึง migration แบบ 1-to-1
- **Boss phase**: `AbilityComponent.SwapPhase(phase2AbilityIds)` แทน static
  prefab lookup

## Alternatives Considered

### Alternative 1: คง `SkillKey` enum binding (status quo)

- **Description**: ไม่เปลี่ยนแปลง binding layer ยอมรับข้อจำกัด
- **Pros**: Zero migration cost, no regression risk
- **Cons**: Block Ability Draft mode, block keybind remap, block variable slot
  count, เพิ่มหนี้ทุก hero ใหม่
- **Estimated Effort**: 0
- **Rejection Reason**: ปิดประตู feature ที่ user ระบุเป็น requirement
  (Ability Draft, skill swap ข้าม hero, keybind remap)

### Alternative 2: Photon Fusion FSM addon (https://doc.photonengine.com/fusion/current/addons/fsm)

- **Description**: ใช้ Photon Fusion FSM addon เป็น backbone ของ ability state
- **Pros**: Fusion-native sync, parallel/hierarchical machines out of the box
- **Cons**: ซ้ำซ้อนกับ `SkillStateMachine` (Animator-based) + `ActorCombatAction`
  ที่มีอยู่แล้วและทำงานถูกต้อง; lock-in กับ Photon addon; ไม่แก้ปัญหา
  binding (SkillKey ก็ยังอยู่)
- **Estimated Effort**: ปานกลาง + risk ของการรื้อ ADR-0004 pipeline
- **Rejection Reason**: แก้ผิดปัญหา — blocker คือ binding layer ไม่ใช่ state
  machine tech; existing FSM + utility AI hybrid เหมาะกว่า

### Alternative 3: Full GAS-style rewrite (Unreal-inspired)

- **Description**: สร้าง Gameplay Ability System ครบวงจร — abilities เป็น
  composable effect trees, tags, cues, attribute sets
- **Pros**: ยืดหยุ่นสูงสุด, pattern ที่พิสูจน์แล้วใน AAA titles
- **Cons**: Scope 2-3x เท่าของการแก้ binding; รื้อ `ActorCombatAction` ทั้งก้อน;
  ทำลาย work ที่ shipping อยู่
- **Estimated Effort**: สูง (1-2 milestones)
- **Rejection Reason**: Over-engineering; user requirement แก้ได้ด้วย scope
  เล็กกว่ามาก (แค่ binding layer)

### Alternative 4: Data-Driven Skill (ScriptableObject only) — revisit ADR-0004

- **Description**: ABR-0004 ปฏิเสธทางนี้ด้วยเหตุผล "MOBA skills ซับซ้อนเกินไป
  สำหรับ pure data"
- **Revisit**: ADR-0006 **ไม่ใช่ pure data** — เก็บ C# subclasses ของ
  `ActorCombatAction` ไว้ทั้งหมด (hero-specific behavior), เพิ่มแค่ data-driven
  *binding* ไม่ใช่ data-driven *behavior*
- **Conclusion**: ADR-0004 rejection rationale ไม่ขัดแย้งกับ ADR-0006

## Consequences

### Positive

- เปิดทาง: variable slot count, runtime swap, boss phase swap, Ability Draft,
  keybind remap, skill steal/item active, Rubick-style mechanics
- Unified AI model — `FuzzySkillAI` ใช้ CBS weights ต่อ AbilityId ได้กับทุก
  entity (monster/boss/hero) ไม่ต้องเขียน scoring logic แยก
- Designer tune AI per-ability ผ่าน CBS ได้ live (weights มีอยู่ใน `CBSAbility`
  อยู่แล้ว)
- `ActorCombatAction` pipeline จาก ADR-0004 ยังอยู่ — ลด migration risk
- Prefab asset structure เดิม (`Spell/{hero}/{hero}_{slot}.prefab`) ยังใช้ได้
  filename = AbilityId → migration 1-to-1

### Negative

- Migration touch ~30+ ไฟล์ที่ใช้ `SkillKey` — ต้อง coordinated refactor
- `CBSAbility.SkillKey` field ต้อง migrate CBS data (backend ops)
- `InputMessage` network struct เปลี่ยน shape → clients ต้องอัพเดต pair กับ
  server (ไม่ใช่ rolling deploy)
- เพิ่ม indirection 1 ชั้น (`KeybindMap → slot → AbilityId`) — new programmers
  ต้องเข้าใจ 3 layers แทน 1 enum

### Neutral

- `SkillKey` enum ยังอยู่ในโค้ดเป็น display hint — ไม่ได้หายไปเลย ลด churn
- Animation state routing ที่ใช้ `SkillKey` (เช่น `AnimationEvent.Skill_Q_Perform`)
  ยังใช้ได้ — animation layer ไม่ต้องเปลี่ยน

## Risks

| Risk | Probability | Impact | Mitigation |
|------|------------|--------|-----------|
| Desync จากอ่าน CBS กลางแมตช์ | ปานกลาง | สูง | `AbilityDataSnapshot` pattern + code review gate; grep ban `GetCustomData<CBSAbility>` นอก snapshot |
| Missed SkillKey usage ในไฟล์ที่ refactor ลืม | ปานกลาง | ปานกลาง | CI grep rule: `SkillObjectDictionary<SkillKey` = fail; per-file checklist ใน audit |
| Merlin element variant regression | ปานกลาง | สูง (playable) | Integration test ต่อ Merlin element; manual QA gate |
| Boss Ice Dragon phase swap regression | ต่ำ | สูง (playable) | Integration test phase1↔phase2; manual QA gate |
| `InputMessage` shape change breaks mid-match | ต่ำ | catastrophic | Forced client update; bump protocol version |
| Performance regression จาก string hash lookup | ต่ำ | ต่ำ | Cache AbilityDefinition ต่อ slot หลัง first resolve; benchmark gate |
| `ActorCombatAction` subclasses ยังอ้าง `SkillKey.Q` ใน logic | สูง | ต่ำ | เอาไว้เป็น display hint — ไม่ block lookup; migrate ช้าๆ เป็น opportunistic cleanup |

## Performance Implications

| Metric | Before | Expected After | Budget |
|--------|--------|---------------|--------|
| Ability dispatch latency | ~50μs (enum dict) | ~80μs (string dict + cache) | < 1ms |
| Memory per entity | ~200B (fixed 8 slots) | ~40B × N slots (variable) | < 500B avg |
| Network: InputMessage size | ~8B | ~9B (+1B slot index) | < 16B |
| GC per cast | 0 | 0 (cache AbilityDefinition ref) | 0 |

หมายเหตุ: string → `AbilityDefinition` lookup cache ต่อ slot (AbilityComponent
เก็บ resolved ref) ทำให้ real-time cost เทียบเท่าเดิม

## Migration Plan

**Phase 0: Audit (blocking ADR acceptance)**

- Run `/migration-audit` หรือ spawn `ai-programmer` + `lead-programmer` อ่านไฟล์ที่
  ใช้ `SkillKey` ทั้งหมด (30+ จาก grep) และ document touch list + per-file
  refactor strategy

**Phase 1: Foundation (parallel, non-breaking)**

1. เพิ่ม `AbilityRegistry` (boot-time catalog, อ่าน `Resources/Prefabs/Gameplay/Spell/**`)
2. เพิ่ม `AbilityComponent : NetworkBehaviour` (ยังไม่ใช้โดย Actor)
3. เพิ่ม `AbilityDataSnapshot` + build ตอน match start
4. เพิ่ม `KeybindMap` + settings UI (default bindings = SkillKey ปัจจุบัน)
5. Deploy parallel — ของเก่ายังทำงาน verify ไม่ regress

**Phase 2: Pilot migration (1 hero)**

1. เลือก hero ง่ายสุด (เสนอ `Hercules` — มี Q/R action files ชัดเจน)
2. Migrate Hercules ให้ route ผ่าน `AbilityComponent` แทน `SkillObjectDictionary`
3. Integration test: Hercules vs AI bot, vs human player, keybind remap
4. Sign-off gate ก่อน Phase 3

**Phase 3: Bulk migration**

1. Migrate heroes ที่เหลือ (19 heroes) — copy-paste pattern จาก Hercules
2. Migrate monsters (creep melee/range/jungle)
3. Migrate bosses (icedragon phase1/2, forestdragon, miniboss)
4. Migrate items (item_teleport_to_unit, item_heal_grenade, etc.)
5. `FuzzySkillAI` refactor: priority queue จาก `SkillKey` → weight+tag-based per-ability

**Phase 4: Deprecation**

1. Remove `SkillObjectDictionary<SkillKey, ...>` และ path เก่า
2. `CBSAbility.SkillKey` field → migrate data ให้ backfill `DisplayCategory` แล้ว
   remove field (backend schema migration)
3. Update ADR-0004 status → add note "binding layer updated by ADR-0006"
4. Add `SkillKey` enum เป็น `[Obsolete]` สำหรับ binding usage; คงไว้สำหรับ
   display/animation

**Rollback plan**: Phase 1-2 ไม่ break ของเก่า → revert เป็นการลบไฟล์ใหม่อย่างเดียว
Phase 3+ rollback = git revert per-hero commit (keep commits atomic per hero)
Phase 4 rollback = restore removed files จาก git history

## Validation Criteria

- [ ] All 20 heroes ใช้งานได้ผ่าน AbilityComponent post-migration (integration test)
- [ ] Merlin element variants (q_fire/ice/dark, w_fire/ice/dark, r_fire/ice/dark)
      สลับได้ runtime ผ่าน `SetSlot` (integration test)
- [ ] Boss Ice Dragon phase1→phase2 ability swap ทำงานถูกต้อง (integration test)
- [ ] Keybind remap (Q↔W) ใน settings ใช้ได้จริงและ persist (manual walkthrough)
- [ ] Bot AI (FuzzySkillAI) ใช้ได้กับ hero/monster/boss โดยโค้ดเดียวกัน
      (integration test)
- [ ] `AbilityComponent.Slots` supports 0..8 slots ที่ runtime (unit test)
- [ ] Zero `GetCustomData<CBSAbility>` calls ระหว่างแมตช์ post-migration (grep CI)
- [ ] Zero `SkillObjectDictionary<SkillKey, ...>` usage ใน production code
      post-Phase 4 (grep CI)
- [ ] Ability dispatch latency < 1ms at 10 bots × 5 abilities × 60Hz (benchmark)
- [ ] `InputMessage` size ≤ 16 bytes (Fusion bandwidth check)

## GDD Requirements Addressed

| GDD Document | System | Requirement | How This ADR Satisfies It |
|-------------|--------|-------------|--------------------------|
| `design/gdd/hero-system.md` | Hero roster | Scalability to 25+ heroes without per-hero binding debt | Data-driven binding ทำให้ hero ใหม่ = ใส่ AbilityId strings ใน Slots + prefab asset; ไม่แตะ enum หรือ core binding code |
| `design/gdd/ai-bot-system.md` | Bot AI / FuzzySkillAI | Per-ability tunable weights (damage/aoe/escape/heal/execute/buff) | Weights มีอยู่ใน CBSAbility แล้ว; ADR นี้ทำให้ AI ใช้ weight+tag scoring โดยไม่ผูกกับ hero identity หรือ SkillKey queue |
| `design/gdd/dungeon-mode.md` | Boss encounters | Phase-based ability swap | `AbilityComponent.SwapPhase()` แทน hardcoded prefab lookup |
| `design/gdd/creep-minion-system.md` | Creep/monster | Unified with hero ability model | `AbilityComponent` ใช้กับ creep ได้ (Slots = empty หรือ [auto_attack]) |
| (Future) `design/gdd/ability-draft-mode.md` | Ability Draft | ผู้เล่นเลือก ability จาก pool mid-match | `AbilityRegistry.AllIds()` เป็น draft pool; `SetSlot(index, abilityId)` ประกอบ loadout |
| (Future) `design/gdd/item-system.md` | Active items / spell stealer | Items ให้ ability ชั่วคราว | `AbilityComponent.SetSlot(ItemSlot, stolenAbilityId)` + revert on item expire |

## Related

- **Updates**: [ADR-0004: ActorCombatAction + SkillKey Pipeline](ADR-0004-actor-combat-action-skill-pipeline.md) — lifecycle unchanged; binding layer replaced
- **Depends on**: [ADR-0002: Photon Fusion 2 networking](ADR-0002-photon-fusion-2-networking.md), [ADR-0003: PlayFab + Azure Functions backend](ADR-0003-playfab-azure-functions-backend.md)
- **See also**: [ADR-0005: ItemAnimationType animation routing](ADR-0005-item-animation-type-routing.md) (Item skill routing ปัจจุบัน — จะเข้ากันกับ slot-based model)
- **Code references** (in `delta-unity/`):
  - `Assets/GameScripts/Commons/Enums/SkillKey.cs` (enum, demoted to display hint)
  - `Assets/GameScripts/Resources/SkillObjectDictionary.cs` (removed at Phase 4)
  - `Assets/GameScripts/Resources/SpellObject/SpellObject.cs` (SO layer — unchanged)
  - `Assets/GameScripts/Datas/DataModel/Metadata/CBS/CBSAbility.cs` (CBS data — `SkillKey` field deprecated at Phase 4)
  - `Assets/GameScripts/Gameplays/Vfxs/SkillObject.cs` (ability prefab behavior — unchanged)
  - `Assets/GameScripts/Gameplays/Characters/ActorCombatAction.cs` (ADR-0004 pipeline — unchanged)
  - `Assets/GameScripts/Gameplays/Characters/Actors/AIRule/FuzzySkillAI.cs` (AI — priority queue refactored at Phase 3)
- **Memory notes**: `feedback_balance_data_cbs.md` (SO vs CBS rule + LayerMask exception)
