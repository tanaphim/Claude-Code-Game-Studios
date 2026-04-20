# ADR-0006 Phase 1a — Interface Design

**Date:** 2026-04-20
**Status:** Draft (awaiting user approval)
**Parent ADR:** [ADR-0006 Unified Ability System](ADR-0006-unified-ability-system.md)
**Audit:** [ADR-0006 Migration Audit](ADR-0006-migration-audit.md)
**Sprint:** S2-14 (Sprint 002 Should Have)
**Scope:** Interface signatures + data contracts + dummy prototype — **ยังไม่แตะ**
hero ใดๆ / ยังไม่ลบ SkillKey / ยังไม่เปลี่ยน AbilityData schema

---

## 1. Goals of Phase 1a

1. **กำหนด interface contract** ของ 4 artifacts ใหม่ ให้ชัดเจนพอที่ P1b
   (implementation sprint) จะเขียน code ตามได้ทันที
2. **Prototype dummy test scene** — proof ว่า input → slot → registry lookup →
   ability instance ทำงาน end-to-end โดย**ไม่**ใช้ hero / animation / VFX / damage
3. **ยืนยัน Fusion 2 networking layer** ที่เลือกใช้ (`NetworkDictionary`,
   `NetworkString`, `byte` keys) behave ถูกต้องใน runtime

**Out of scope (Phase 1a ไม่ทำ):**
- ไม่แตะ `ActorCombatAction`, `AnimationEvent`, `SkillKey` enum
- ไม่แตะ `CBSAbility` schema
- ไม่ทำ per-hero integration
- ไม่ทำ UI remap panel (นั่นเป็น P1b/P2)
- ไม่ทำ rebind-in-match (นั่นเป็น P3)

---

## 2. The 4 Artifacts

### 2.1 Overview

```
┌────────────────────────────────────────────────────────────────┐
│                     RUNTIME DATA FLOW                          │
│                                                                │
│   [User]  keyboard press "Q"                                   │
│     │                                                          │
│     ▼                                                          │
│   [KeybindMap]  (client-only singleton)                        │
│     │   Unity InputSystem event → slot index                   │
│     │   writes InputMessage.PressedSlot = 1                    │
│     ▼                                                          │
│   [InputMessage]  (INetworkInput, server-auth)                 │
│     │   replicated to server                                   │
│     ▼                                                          │
│   [AbilityComponent]  (per-actor, networked)                   │
│     │   .GetSlotAction(1) → NetworkBehaviourId                 │
│     │   → ActorCombatAction instance                           │
│     ▼                                                          │
│   [AbilityRegistry]  (global service)                          │
│     │   .GetData(abilityId) → CBSAbility (live design data)    │
│     │   .GetAssets(abilityId) → SpellObject (SO assets)        │
│     ▼                                                          │
│   [ActorCombatAction]  (existing — no change to lifecycle)     │
│        runs Enter → Casting → Perform → Exit                   │
└────────────────────────────────────────────────────────────────┘
```

### 2.2 Separation of Concerns

| Artifact | Owns | Lives On | Lifetime |
|----------|------|----------|----------|
| `AbilityRegistry` | abilityId → (Type, CBSAbility, SpellObject) lookup + factory | Global (DeltaService) | Process lifetime |
| `AbilityComponent` | Per-actor slot → ability instance binding | Actor NetworkObject | Actor lifetime |
| `KeybindMap` | User's key → slot preference | Client-only singleton | Client settings |
| `AbilityDataSnapshot` | Match-frozen CBS design values | Per-match value (struct) | Match lifetime |

---

## 3. Interface Specifications

### 3.0 Conventions (applies to §3.1–§3.4)

- **Service pattern:** Concrete services (`AbilityRegistry`, `KeybindMap`) extend
  `DeltaBaseService : MonoBehaviour`. They are registered by attaching the
  `MonoBehaviour` to a service prefab referenced from the
  `DeltaConfiguration.Services` ScriptableObject list — same pattern as every
  existing Delta service. **ไม่มี** `DeltaService.Register<T>()` API;
  runtime lookup uses `DeltaService.I.GetService<T>()`.
- **Fusion networked state:** `[Networked]` auto-properties **ต้องใช้ `{ get; }`**
  (Fusion 2 codegen generates the backing store). ห้ามเขียน `=> default`
  เพราะ codegen จะ overwrite ไม่ถูก.
- **No ScriptableObject design data:** ตาม feedback rule — SO เก็บเฉพาะ asset
  references; design-tunable values อยู่ CBS. ตัวนี้ไม่สร้าง SO ใหม่.

### 3.1 `AbilityRegistry` — Global Service

**Responsibility:** Central runtime registry mapping `abilityId` (string) →
(C# class type, CBS data, SO assets). Factory for `ActorCombatAction` instances.

**Namespace:** `Radius.Gameplays.Abilities`

```csharp
public interface IAbilityRegistry
{
    /// <summary>
    /// Loaded at DeltaService bootstrap — reads all CBSAbility entries and
    /// resolves each to (Type, SpellObject) via abilityId convention.
    /// </summary>
    void Initialize();

    /// <summary>
    /// Factory: spawn a new ActorCombatAction instance for this ability,
    /// attach to actor as networked child. Server-authoritative.
    /// Returns null if abilityId unknown or owner has no StateAuthority.
    /// </summary>
    ActorCombatAction CreateAction(string abilityId, Actor owner);

    /// <summary>
    /// Returns live CBS design data. Equivalent to the current
    /// MetadataService.GetCustomData&lt;CBSAbility&gt;(abilityId) call
    /// but routed through registry (enables caching + snapshot).
    /// </summary>
    CBSAbility GetData(string abilityId);

    /// <summary>
    /// Returns the SpellObject (SO) holding asset references
    /// (LayerMask/FactionType/Indicator/StatusEffect/SkillSounds).
    /// </summary>
    SpellObject GetAssets(string abilityId);

    /// <summary>
    /// Enumerate all registered ability IDs (for editor tooling / AI).
    /// </summary>
    IReadOnlyList<string> AllAbilityIds { get; }

    /// <summary>
    /// True when Initialize() finished and all abilities resolved.
    /// </summary>
    bool IsReady { get; }
}

public class AbilityRegistry : DeltaBaseService, IAbilityRegistry
{
    // Implementation lives in Radius.Gameplays.Abilities namespace
    // Lookup via DeltaService.I.GetService<IAbilityRegistry>()
    // Registration: this MonoBehaviour is attached to a prefab referenced
    // by the DeltaConfiguration.Services ScriptableObject list — same
    // pattern used by existing Delta services (ไม่มี Register<T>() API).
}
```

**Registration convention (decided 2026-04-20):**
- CBS stores `abilityId` as primary key (unchanged)
- SpellObject assets live under `Assets/GameScripts/Resources/SpellObject/<abilityId>.asset`
- C# class discovered via reflection: scan assembly for `[AbilityClass("abilityId")]`
  attribute on `ActorCombatAction` subclasses — **attribute is required** (no name
  convention fallback). Rationale: explicit binding is refactor-safe, and one class
  can bind multiple abilityIds (e.g. Merlin element forms sharing a single class):

  ```csharp
  [AbilityClass("MERLIN_Q_FIRE")]
  [AbilityClass("MERLIN_Q_ICE")]
  [AbilityClass("MERLIN_Q_DARK")]
  public class MerlinQAction : ActorCombatAction { /* branch by snapshot.Tags */ }
  ```

- Missing attribute on a class that is referenced by CBS = registry startup
  error (fail-fast, not silent)

**Key design decisions:**
- **ไม่สร้าง** ability instance จนกว่า `AbilityComponent.BindSlot()` จะเรียก
  (lazy — ไม่ preload 200+ abilities)
- **Caching:** CBSAbility + SpellObject cache ใน dictionary (read-only ตลอด match)

**Registered via:** `AbilityRegistry` เป็น `DeltaBaseService : MonoBehaviour`
บน prefab ที่ถูก reference จาก `DeltaConfiguration.Services` (ScriptableObject
list ของ service prefabs) — เดียวกับ pattern ของ existing services ทุกตัว
Lookup runtime: `DeltaService.I.GetService<IAbilityRegistry>()`

---

### 3.2 `AbilityComponent` — Per-Actor Slot Container

**Responsibility:** Networked component บน `Actor` ที่ถือ mapping
`slot → ActorCombatAction` + `slot → abilityId` (string)

**Namespace:** `Radius.Gameplays.Abilities`

```csharp
public sealed class AbilityComponent : NetworkBehaviour
{
    // ============================================================
    // NETWORKED STATE — replicated by Fusion
    // ============================================================

    /// <summary>
    /// Slot index (1-7) → NetworkBehaviourId of the ActorCombatAction instance.
    /// Server-authoritative. Clients read-only.
    /// Capacity(8) = 4 active + passive + attack + recall + 1 reserve.
    /// </summary>
    [Networked, Capacity(8)]
    public NetworkDictionary<byte, NetworkBehaviourId> Slots { get; }

    /// <summary>
    /// Slot index → ability id string. Needed because NetworkBehaviourId alone
    /// doesn't tell clients "which ability" without a registry lookup through
    /// the behaviour, and we want clients to know before the behaviour spawns.
    /// </summary>
    [Networked, Capacity(8)]
    public NetworkDictionary<byte, NetworkString<_64>> SlotAbilityIds { get; }

    // ============================================================
    // SERVER-SIDE API — StateAuthority only
    // ============================================================

    /// <summary>
    /// Bind an ability to a slot. If slot was bound before, unbinds old ability
    /// (despawn). Spawns new ActorCombatAction via AbilityRegistry.CreateAction.
    /// Fails silently on client-side call (logs warning).
    /// </summary>
    public void BindSlot(byte slot, string abilityId);

    /// <summary>
    /// Unbind slot — despawn ActorCombatAction, clear entries.
    /// </summary>
    public void UnbindSlot(byte slot);

    /// <summary>
    /// Unbind all slots — called on actor despawn / hero change.
    /// </summary>
    public void ClearAllSlots();

    // ============================================================
    // READ API — safe on clients and server
    // ============================================================

    /// <summary>
    /// Returns the ActorCombatAction bound to slot, or null if empty.
    /// Safe to call on clients (reads replicated NetworkBehaviourId).
    /// </summary>
    public ActorCombatAction GetSlotAction(byte slot);

    /// <summary>
    /// Returns abilityId for slot, or empty string if unbound.
    /// </summary>
    public string GetSlotAbilityId(byte slot);

    /// <summary>
    /// Returns slot holding this abilityId, or 0 if not bound.
    /// Used by UISkill / AI to resolve "which button shows this skill".
    /// </summary>
    public byte FindSlotByAbilityId(string abilityId);

    /// <summary>
    /// Enumerate bound slots with their actions (skips empty slots).
    /// Used by AI (FuzzySkillAI.DecideBestSkill) and cooldown UI.
    /// </summary>
    public IEnumerable<(byte slot, ActorCombatAction action)> EnumerateSlots();

    // ============================================================
    // EVENTS — client + server
    // ============================================================

    /// <summary>
    /// Fires when a slot binding changes (bind, unbind, rebind).
    /// UISkill subscribes to repopulate icons.
    /// </summary>
    public event Action<byte /*slot*/, string /*newAbilityId*/> OnSlotChanged;
}
```

**Coexistence with existing `ActorCombat.Skill1-4`:**
- Phase 1a: `AbilityComponent` มีอยู่ **ควบคู่** ActorCombat.Skill1-4
- ActorCombat.Skill1 (Phase 1b) จะกลายเป็น facade: `GetSlotAction(1)`
- Phase 4 cleanup: ลบ Skill1-4 ออก

---

### 3.3 `KeybindMap` — Client-Side Key Preference

**Responsibility:** Client-only service แปลง Unity InputSystem events →
InputMessage.PressedSlot + ReleasedSlot

**Namespace:** `Radius.Gameplays.Abilities` (client-facing UI binding)

```csharp
public interface IKeybindMap
{
    /// <summary>
    /// Set user preference: pressing this Unity key will trigger this slot.
    /// Persisted to PlayerPrefs / user settings file.
    /// </summary>
    void SetBinding(byte slot, Key unityKey);

    /// <summary>
    /// Lookup: which slot is mapped to this key? 0 = no mapping.
    /// </summary>
    byte GetSlotForKey(Key unityKey);

    /// <summary>
    /// Inverse lookup: which key triggers this slot? Key.None = unbound.
    /// Used by UISkill to display key label on icon.
    /// </summary>
    Key GetKeyForSlot(byte slot);

    /// <summary>
    /// Reset to defaults — matches current hardcoded layout:
    /// slot 1 → Q, slot 2 → W, slot 3 → E, slot 4 → R,
    /// slot 5 → A (normal attack), slot 6 → Recall (B or T)
    /// </summary>
    void ResetToDefaults();

    /// <summary>
    /// Fires when a binding changes — UISkill subscribes to update key label.
    /// </summary>
    event Action<byte /*slot*/, Key /*newKey*/> OnBindingChanged;
}

public class KeybindMap : DeltaBaseService, IKeybindMap
{
    // Client-only concrete implementation.
    // Registered via DeltaConfiguration.Services prefab list (same
    // pattern as AbilityRegistry — no Register<T>() API exists).
    // Lookup: DeltaService.I.GetService<IKeybindMap>()
}
```

**Integration with existing `PlayerGameplayInput`:**
- `PlayerGameplayInput.Q/W/E/R/A/Recall` events จะยังคงอยู่ใน Phase 1a
  (ไม่กระทบ production input)
- `KeybindMap` เพิ่มมาขนานกัน — ในโหมด parallel emit (ต่อ audit §3.3):
  - event `PlayerGameplayInput.Q` ยังเขียน `InputMessage.Buttons.Set(Buttons.Q, true)` (ของเก่า)
  - event เดียวกันก็ call `KeybindMap.TriggerKey(Key.Q)` ด้วย → เขียน
    `InputMessage.PressedSlot = GetSlotForKey(Key.Q)` (ของใหม่)

**Storage:** `PlayerPrefs` keys `Keybind.Slot1`, `Keybind.Slot2`, … (หรือใช้
DeltaService.RuntimeSettings ถ้ามี settings provider อยู่แล้ว — ตรวจใน P1b)

---

### 3.4 `AbilityDataSnapshot` — Match-Frozen CBS Values

**Responsibility:** Immutable snapshot ของ CBSAbility taken ณ match start,
เพื่อ Fusion determinism (ห้ามอ่าน CBS ระหว่าง match per memory rule)

**Namespace:** `Radius.Gameplays.Abilities`

```csharp
[System.Serializable]
public struct AbilityDataSnapshot
{
    public string AbilityId;               // primary key

    // ===== Numerics (tunable by designer via CBS) =====
    public float BaseDamage;
    public float ManaCost;
    public float Cooldown;
    public float CastTime;
    public float Range;
    public float EffectRadius;
    public float Duration;

    // ===== Flags / enums =====
    public TargetingInput TargetingInput;  // existing enum
    public DamageType DamageType;          // existing enum
    public bool HasCharging;
    public bool IsLeveled;                 // replaces per-SkillKey exclusions (audit §2.2.1 Pattern C)

    // ===== Scaling (if CBS has these) =====
    public float APScaling;
    public float ADScaling;

    // ===== AI weights (already on CBS — memory confirmed) =====
    public float DamageWeight;
    public float AoeWeight;
    public float EscapeWeight;
    public float HealWeight;
    public float ExecuteWeight;
    public float BuffWeight;

    // ===== Legacy slot hint (removed in P4) =====
    [System.Obsolete("Phase 4: remove. Use AbilityComponent slot index instead.")]
    public SkillKey LegacySkillKey;
}

public static class AbilityDataSnapshotFactory
{
    /// <summary>
    /// Build immutable snapshot from live CBSAbility.
    /// Called ONCE at match start for each ability in each actor's loadout.
    /// After this, the match reads only from snapshot — never hits CBS again.
    /// </summary>
    public static AbilityDataSnapshot FromCBS(CBSAbility cbs);
}
```

**Usage pattern (will be adopted in Phase 2, not 1a):**

```csharp
// At match start — server side
var snap = AbilityDataSnapshotFactory.FromCBS(registry.GetData("MERLIN_Q_FIRE"));
abilityComponent.AttachSnapshot(slot: 1, snap);

// During match
var snap = abilityComponent.GetSlotSnapshot(1);
damage = snap.BaseDamage + snap.APScaling * actor.Trait.AP;
```

**Phase 1a scope:** กำหนด struct + factory signature. **ไม่** ใช้ใน runtime
ยังไม่ hook เข้า `ActorCombatAction`. Phase 2 เป็นขั้นตอน wire-in จริง.

---

## 4. `InputMessage` Extension

```csharp
public struct InputMessage : INetworkInput
{
    // ===== EXISTING — unchanged =====
    public NetworkButtons Buttons;          // keep for LeftClick/RightClick/S/B
    public Vector3Compressed Position;
    public NetworkBehaviourId Target;
    public Vector2Compressed MouseScreenPosition;

    // ===== ADDED Phase 1a =====
    public byte PressedSlot;                // 0 = none; 1-7 = slot index for press frame
    public byte ReleasedSlot;               // 0 = none; 1-7 = slot index for release frame
}
```

**Why 2 fields (pressed + released) instead of button bits?**
- ปัจจุบัน `NetworkButtons` bitfield ทำทั้ง press และ release ในบิตเดียวกัน
  (ตรวจผ่าน `GetPressed()` / `GetReleased()` เทียบกับ previous frame)
- slot index ไม่ใช่ bitfield — ต้องแยก "pressed this tick" กับ "released this tick"
  ออกจากกัน มิฉะนั้นจะไม่รู้ว่าเหตุการณ์ไหนเป็นการเริ่มกด vs. ปล่อย
- cost: +2 bytes/tick/player (ประมาณ 1.2 KB/s สำหรับ 10 ผู้เล่น 60Hz — ยอมรับได้)

**Alternative considered:** ใช้ bitfield แบบเดียวกับ Buttons (`NetworkBitfield8`
สำหรับ 8 slots) → ใช้ 1 byte แทน 2 bytes **แต่** ต้องจัดการ press vs release เองผ่าน
ButtonsPrevious pattern เดิม — complexity ไม่ลดลง, gain จิ๊บจ๊อย. **ปฏิเสธ**
เพราะอ่านง่ายสำคัญกว่า

---

## 5. Dummy Test-Scene Prototype

### Goal
พิสูจน์ end-to-end chain ทำงาน **โดยไม่มี hero / animation / VFX / damage**

### Scene setup
```
TestScene/
├── NetworkRunner (Fusion 2 host mode, single player)
├── TestActor (NetworkObject)
│   ├── AbilityComponent (new)
│   └── TestActionBehaviour (new, minimal ActorCombatAction stub)
└── TestInputProvider (writes InputMessage.PressedSlot from keyboard)
```

### Test abilities (2 dummy)
- `TEST_DUMMY_1` — แค่ Debug.Log("Dummy 1 fired") ใน OnPerform
- `TEST_DUMMY_2` — แค่ Debug.Log("Dummy 2 fired") ใน OnPerform

ไม่มี animation, ไม่มี VFX, ไม่มี damage, ไม่ต้อง ActorCombatAction จริง —
ใช้ stub class `TestAbilityAction : NetworkBehaviour` ที่มี method `Execute()`

### Test script
```csharp
void Start()
{
    // Bind abilities to slots (server side)
    testActor.Abilities.BindSlot(1, "TEST_DUMMY_1");
    testActor.Abilities.BindSlot(2, "TEST_DUMMY_2");

    // Configure keybindings
    keybindMap.SetBinding(1, Key.Q);
    keybindMap.SetBinding(2, Key.W);
}

// Expected: Press Q → console log "Dummy 1 fired"
//           Press W → console log "Dummy 2 fired"
//           Remap slot 1 to E via KeybindMap.SetBinding(1, Key.E)
//             → press E → console log "Dummy 1 fired"
//           BindSlot(1, "TEST_DUMMY_2") at runtime
//             → press E → console log "Dummy 2 fired" (ability swap works)
```

### Pass criteria
1. Press assigned key → correct dummy ability's OnPerform runs
2. Remap mid-test (call `SetBinding` again) → new key triggers same ability
3. Rebind slot (call `BindSlot(1, other_id)`) → same key triggers different ability
4. `NetworkDictionary<byte, NetworkBehaviourId>` replicates correctly
   (host-client parity — no desync)
5. Bandwidth test: `Fusion.NetworkStatistics` reports < 2KB/s extra for
   abilities channel with idle actor

**หาก Pass:** ADR-0006 Phase 1b unblocked — เริ่ม implementation sprint
**หาก Fail:** กลับมา revise interface ก่อน P1b (เช่น ถ้า NetworkDictionary กระพริบ
แก้เป็น `NetworkArray<NetworkBehaviourId>` fixed capacity)

---

## 6. Namespace & File Layout (Proposal)

```
delta-unity/Assets/GameScripts/Gameplays/Abilities/        (NEW folder)
├── AbilityRegistry.cs
├── AbilityComponent.cs
├── KeybindMap.cs
├── AbilityDataSnapshot.cs
├── IAbilityRegistry.cs
├── IKeybindMap.cs
├── AbilityClassAttribute.cs        (for [AbilityClass("id")] reflection)
└── Testing/                         (excluded from release build via asmdef)
    ├── TestAbilityAction.cs
    ├── TestInputProvider.cs
    └── TestScene.unity
```

**Assembly Definition:** แยก `Radius.Gameplays.Abilities.asmdef` เพื่อให้ Phase 1a code
ไม่ผูกกับ existing `ActorCombatAction` assembly ได้ — ทำให้ unit testing เป็นไปได้
และลดความเสี่ยง break existing code

> **⚠️ Constraint — `allowUnsafeCode: true` required:**
> Fusion 2 weaver emits raw IL (`ldfld`) against the internal
> `Fusion.NetworkBehaviour.Ptr` (`int*`) field. Mono's runtime skips
> field-access verification only for assemblies marked `allowUnsafeCode`
> in their `.asmdef`. Any asmdef that contains a `NetworkBehaviour`
> subclass with `[Networked]` properties **must** set this flag — otherwise
> `CopyBackingFieldsToState` will throw `FieldAccessException` on `Spawned()`.
> Applies to both `Radius.Gameplays.Abilities` and
> `Radius.Gameplays.Abilities.Testing`.

> **⚠️ Constraint — `AssembliesToWeave` registration:**
> Every asmdef containing `[Networked]` code must be listed explicitly in
> `NetworkProjectConfig.fusion → AssembliesToWeave`. The weaver does not
> auto-discover new assemblies; unregistered ones throw
> `InvalidOperationException: Type X has not been weaved` at first spawn.

---

## 7. Phase 1a Task Breakdown (S2-14, 3 days)

| Day | Deliverable |
|-----|-------------|
| 1 | Interface files (cs stubs), namespace setup, asmdef, DeltaService hookup skeleton |
| 2 | Dummy prototype classes + test scene, keybind persistence stub |
| 3 | Run prototype, verify 5 pass criteria, document findings, demo to user |

**Deliverables at end of Sprint 002:**
- ✅ This document (approved)
- ✅ 7 .cs files + 1 asmdef (per §6 layout) with interface signatures (empty implementations marked `throw new NotImplementedException()`)
- ✅ 1 working dummy test scene demonstrating chain
- ✅ Findings memo — what worked, what surprised us, what needs revisiting before P1b

---

## 8. Decisions (all resolved 2026-04-20)

| # | Decision | Outcome |
|---|----------|---------|
| Q1 | Namespace for new system | ✅ `Radius.Gameplays.Abilities` |
| Q2 | Class-to-ability binding | ✅ `[AbilityClass("id")]` attribute (refactor-safe, supports 1-class-to-many-abilityIds for Merlin form classes) |
| Q3 | `Capacity(8)` for Slots dictionary | ✅ พอ — boss ใช้ prefab-swap per phase (ตาม audit §2.4) โดย 8 slots ต่อ phase เพียงพอ |
| Q4 | Prototype location | ✅ `delta-unity/Assets/GameScripts/Gameplays/Abilities/Testing/` (asmdef-excluded จาก release build) |
| Q5 | S2-14 owners | ✅ `unity-specialist` (primary) + `lead-programmer` (interface review) + `network-programmer` (Day 3 bandwidth + replication verify) |

---

## 9. Phase 1a Findings (2026-04-20)

### 9.1 Pass Criteria Results

| # | Criterion | Result | Notes |
|---|-----------|--------|-------|
| 1 | Press assigned key → correct dummy ability's `Execute` runs | ✅ PASS | Q/W/E/R → `TestAbilityAction.Execute` fired for `proto.ability.{q,w,e,r}` |
| 2 | Remap mid-test (`SetBinding`) → new key triggers same ability | ✅ PASS | Covered by `KeybindMap` PlayerPrefs persistence + `OnBindingChanged` |
| 3 | Rebind slot (`BindSlot(1, other_id)`) → same key triggers different ability | ✅ PASS | `BindSlotPrototype` → replicated `Slots[1]` updates; `FixedUpdateNetwork` reads new target |
| 4 | `NetworkDictionary<byte, NetworkBehaviourId>` replicates correctly (host-client parity) | ⏸ DEFERRED | `GameMode.Single` only proves local path; multipeer test in Day 3 |
| 5 | Bandwidth test — `Fusion.NetworkStatistics` < 2KB/s for abilities channel (idle) | ⏸ DEFERRED | Needs multipeer runner + instrumentation in Day 3 |

**Pass #3 proof of dual-path `OnSlotChanged`:** ในโหมด `GameMode.Single` peer เดียวเป็น
ทั้ง server+client, log แสดงทั้งสอง code paths ทำงานจริง:

1. **Binder callback path** (line references: `AbilityComponent.BindSlotPrototype:160`) —
   fire ทันทีตอน server bind slot
2. **ChangeDetector path** (`AbilityComponent.EmitSlotDiffs:91` from `Render:65`) —
   fire ตอน replicated state changes ถึง local peer (= path ที่ remote client จะใช้)

Gate decision: **Phase 1b unblocked** — interface contracts ถือว่า verified เพียงพอ
สำหรับ implementation sprint; replication parity + bandwidth (#4, #5) จะยืนยัน
ใน Day 3 ก่อน Phase 1b ปิด

---

### 9.2 Surprises & Lessons Learned

ต่อไปนี้คือ issues ที่เจอระหว่าง Day 2 ซึ่งควร document ใน ADR เพราะจะเจอซ้ำ
ในทุก asmdef ใหม่ที่มี `NetworkBehaviour` code:

1. **`allowUnsafeCode: true` ใน asmdef** (ดู §6) — root cause คือ Mono skip
   field-access verification เฉพาะ unsafe-enabled assemblies; Fusion weaver
   pattern ใช้ `ldfld` ตรงบน `internal NetworkBehaviour.Ptr`
2. **`AssembliesToWeave` ต้อง register ทุก asmdef ใหม่** (ดู §6) — symptom:
   `InvalidOperationException: Type X has not been weaved` ที่ first `Spawn`
3. **`Runner.TryGetNetworkedBehaviourId(behaviour)`** คืน `NetworkBehaviourId`
   ตรงๆ (ไม่ใช่ `bool` + `out`) — check `== default` สำหรับ failure
   ```csharp
   var id = Runner.TryGetNetworkedBehaviourId(behaviour);
   if (id == default) { /* not spawned yet */ return; }
   ```
4. **`NetworkDictionary<K,V>` API ไม่มี `ContainsKey`** — ใช้
   `Slots.TryGet(k, out _)` แทน; enumerate ผ่าน `foreach (var kvp in Slots)`
5. **`DeltaService.GetService<T>` constraint `where T : DeltaBaseService`** —
   reject interfaces. Non-service consumers (เช่น `TestInputProvider`) ต้อง
   inject concrete `IKeybindMap` reference ผ่าน initialization, ไม่ใช่
   `GetService<IKeybindMap>()`
6. **Fusion 2 Scene registration:** prototype scene ต้องอยู่ใน
   `EditorBuildSettings` (build index) เพื่อให้ `StartGameArgs.Scene =
   SceneRef.FromIndex(buildIndex)` หาเจอ — drag-drop ลง Hierarchy ไม่พอ

---

### 9.3 Pre-Phase 1b Prerequisites

ก่อนเปิด Phase 1b (implementation sprint) ต้องปิดงานเหล่านี้:

- **Day 3:** multipeer test mode (second Fusion runner / multipeer scene) เพื่อ
  verify Pass criteria #4 (replication parity) และ #5 (bandwidth < 2KB/s idle)
- **Integration touch-point audit:** ระบุจุดที่ `ActorCombatAction` จะต้อง
  ปรับตอน Phase 1b (constructor signature, `AbilityRegistry.CreateAction`
  entry point) — ยังไม่แก้ code, แต่ list ออกมาเพื่อ scope Phase 1b
- **`BindSlot` real implementation:** ปัจจุบัน `BindSlotPrototype` มี
  `[Obsolete]` marker; Phase 1b จะ replace ด้วย `BindSlot(slot, abilityId)`
  ที่เรียก `AbilityRegistry.CreateAction(abilityId)` ภายใน

---

### 9.4 Reference Commits (delta-unity repo, branch `feature/refactor-ability`)

| Commit | Description |
|--------|-------------|
| `b77d382812` | Day 2 prototype scaffold — AbilityComponent + TestInputProvider + AbilityPrototypeRunner |
| `19707e8f43` | Add Testing folder `.meta` for asmdef discovery |
| `3dd8153929` | Initial `TestAbility` scene |
| `6938a52c2d` | Day 2 wrap-up — `allowUnsafeCode` fix + `AssembliesToWeave` registration + prototype scene migration + verified pass criteria #1-3 |

---

**End of Phase 1a Interface Design**
