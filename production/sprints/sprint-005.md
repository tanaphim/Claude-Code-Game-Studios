# Sprint 5 — 2026-05-09 to 2026-05-22

## Sprint Goal

ปิด **Phase 2 Hercules pilot** (ability migration, Foundation → Production transition)
และ land 3 high-priority design decisions (R-21 Role Restriction, AI Bot fate, R-22/R-23
schedule) ที่ Sprint 004 retro flag ไว้ — เพื่อ unblock Sprint 006+ planning

**Theme:** "Phase 2 ability refactor + design decision sprint"

---

## Capacity

**Sprint window:** 14 calendar days (2026-05-09 → 2026-05-22)

### Per-member capacity

| Handle | Role | Calendar days | Velocity × | Effective days |
|--------|------|---------------|------------|----------------|
| `tanapol` | gameplay-programmer (lead, senior) | 14 | 1.0× | 14.0d |
| **Team total (gross)** | | | | **14.0d** |

### Buffer

- Buffer (20% of gross): **2.8d** สำรองสำหรับงานที่ไม่ได้วางแผน
- **Available effective: ~11.2d**

### Notes

- Sprint 005 มีคนเดียว (`tanapol`) — ส่วนสมาชิกคนอื่นใน [team roster](../../memory link via /onboard) ยังไม่ได้รับ assign ใน sprint นี้
- ถ้ามีสมาชิกเพิ่มกลางทาง → อัพเดต table นี้พร้อม velocity ของคนนั้น
- Re-evaluate velocity ตอน Sprint 005 retrospective — เก็บ actual vs estimate ต่อคนใน `production/velocity/`
- `assignee` field ใน `sprint-status.yaml` track ว่าใครรับ story ไหน — ใช้ git config `user.name` (first-name lowercase) เป็น handle

---

## Tasks

### Must Have (Critical Path — Phase 2 Hercules pilot)

| ID | Task | Agent/Owner | Est. Days | Dependencies | Acceptance Criteria |
|----|------|-------------|-----------|-------------|-------------------|
| S5-01 (revised, ADR-0008) | `CBSUnit.SlotQ/W/E/R/A/I` alias properties (read-only, wraps legacy fields) | gameplay-programmer | 0.25 | — | Aliases compile; EditMode tests pass for empty/non-empty/consolidation cases; existing CBS deserialization unaffected. **Note:** Original ADR-0006 §6.1 (CBSAbility.Slot) reverted 2026-05-08 — see ADR-0008. |
| S5-02 (P2-01) | `ActorCombat.GetSlotAction` + `AbilityComponent.GetSlotAction` facade | gameplay-programmer | 0.25 | — | Both facades return same action for slot lookup; covered by unit test |
| S5-03 (P2-02) | `ActorCombat.GetActiveSlot` + `IsQuickCast(byte slot)` accessor | gameplay-programmer | 0.25 | — | GetActiveSlot returns latest pressed slot; IsQuickCast respects per-slot setting |
| S5-04 (P2-04) | `ActorCombatAction` Pattern-A helper (`IsActiveSlotOwner`) — replace 5 blocks | gameplay-programmer | 0.5 | S5-01, S5-02 | All 5 owner-guard sites replaced; multipeer harness Pass #1-5 still pass |
| S5-05 (P2-05) | `ActorCombatAction` Pattern-B/C/D one-liner replacements (4 sites) | gameplay-programmer | 0.5 | S5-01..S5-03 | 4 sites replaced; Hercules manual playthrough passes |
| S5-06 (P2-06) | `AnimationEvent` Option A — wire 42 shim methods through `GetActiveSlot()` | gameplay-programmer | 0.5 | S5-03 | All 42 shim methods compile; animation events fire on correct slot |
| S5-07 (P2-07) | `HerculesRAction.GetInput` rewrite (PressedSlot path) | gameplay-programmer | 0.25 | S5-01 | Hercules R charge respects PressedSlot binding; release uses ReleasedSlot when wired |
| S5-08 (P2-08) | `HerculesWAction` slot-indexed sibling reads (5 sites) | gameplay-programmer | 0.25 | S5-02 | 5 sibling reads use GetSlotAction; behavior unchanged |
| S5-09 (P2-09, revised ADR-0008) | `Hercules` avatar bootstrap — `ActorCombat.OnStartup` reads `unit.SlotQ[0]..SlotI[0]` from CBSUnit and calls `AbilityComponent.BindSlot(slot, id)` × 6 (Q/W/E/R/A/I); slot 7 Recall bound globally | gameplay-programmer | 0.5 | S5-01 | 6 BindSlot calls in OnStartup, all reading from CBSUnit aliases (no hardcoded ability ids); PeerMode=Single playtest no errors |
| S5-10 (P2-10) | Manual playtest checklist + 1-match Training playthrough verification | qa-tester + gameplay-programmer | 0.5 | all S5-01..S5-09 | Hercules QWER playable end-to-end; multipeer harness passes; evidence in `production/qa/evidence/` |
| S5-21 (P2-polish) | TD-006 SetActiveSlot wiring + S5-06 40-shim re-migration (atomic) | gameplay-programmer | 0.25 | S5-06 ✅, S5-10 ✅ | `ResolveSlotFromSkillKey` helper + `OnPressButtons` SetActiveSlot call + 40 shims migrated; EditMode tests pass; production playtest 0 regressions; ships before Phase 3 kickoff 2026-05-21 |

**Must Have Subtotal: 4.0d** (~2.0d critical path serial; rest parallelizable)

**Critical path:** S5-01 → S5-04 → S5-09 → S5-10 → S5-21 (Phase 3 unlock)
**Parallelizable:** S5-02, S5-03, S5-06, S5-07, S5-08

### Should Have — Design decisions (Sprint 004 retro action #2, #3, #5)

| ID | Task | Agent/Owner | Est. Days | Dependencies | Acceptance Criteria |
|----|------|-------------|-----------|-------------|-------------------|
| S5-11 | **Item Role Restriction decision** (R-21) — implement gating OR remove `ItemObject.Positions[]`. Author ADR-0008 with chosen path | game-designer + gameplay-programmer | 1.0 | — | ADR-0008 status=Accepted; if implement → unit test on `AvailableToPurchase()`; if remove → schema migration noted |
| S5-12 | **AI Bot fate decision** — formally commit Sprint 006 with full focus OR descope to post-launch. Update sprint-006 backlog or move S4-09/S4-10 to `production/backlog/post-launch.md` | producer + creative-director | 0.5 | — | Decision documented; carryover ledger updated; Sprint 003 retro action #5 closed |
| S5-13 | **Confirm 3 Garen variant controllers** — production heroes or legacy test? If legacy → delete; if production → swap base or port states | art-director + lead-programmer | 0.5 | — | Decision documented in `item-system.md` §Known Issues; legacy controllers deleted OR base swapped |

**Should Have Subtotal: 2.0d**

### Nice to Have — Process + polish

| ID | Task | Agent/Owner | Est. Days | Dependencies | Acceptance Criteria |
|----|------|-------------|-----------|-------------|-------------------|
| S5-14 | Codify **5-min existence check rule** (S4 retro action #1) ใน `coordination-rules.md` หรือ `/sprint-plan` skill | producer | 0.25 | — | Rule documented; example template added |
| S5-15 | Carry-forward S3-retro action #3: scene-naming convention doc | producer | 0.25 | — | `coordination-rules.md` updated |
| S5-16 | Carry-forward S3-retro action #4: `origin/dev` merge cadence metric (track per Sprint 005) | lead-programmer | 0.25 | — | Metric defined; tracked starting Sprint 005 |
| S5-17 (S4-P1) | `AbilityMultipeerRunner` duplicate-Start guard | network-programmer | 0.5 | — | Console clean ตอน multipeer harness run — ไม่มี GameIsFull cascade |
| S5-18 (S4-P2) | `AbilityRegistry` boot-time optimization (Resources.LoadAll → Addressables) | unity-addressables-specialist | 1.5 | — | Cold-start scan < 100 ms / 158 prefabs |

**Nice to Have Subtotal: 2.75d** (pull only ถ้า Must + Should เสร็จเร็ว)

### Bugs (filed during sprint)

| ID | Bug | Agent/Owner | Est. Days | Dependencies | Acceptance Criteria |
|----|-----|-------------|-----------|-------------|-------------------|
| S5-19 | [BUG-0001](../qa/bugs/BUG-0001-recall-locomotion-stuck.md) — Recall post-warp locomotion animation ไม่เล่น | gameplay-programmer + technical-artist | 0.5 | — | Animator transitions back to locomotion after Recall; manual playtest confirms walk/run plays |
| S5-20 | [BUG-0002](../qa/bugs/BUG-0002-anansi-w-idle-stuck.md) — Anansi W ค้างท่า Idle หลัง cast จบ | gameplay-programmer + technical-artist | 0.5 | — | Animator exits W state correctly; locomotion resumes; manual playtest passes |

**Investigation note:** BUG-0001 และ BUG-0002 มีอาการ post-cast animator-state-stuck เหมือนกัน — แนะนำ investigate ร่วมเพื่อหา root cause ก่อนเขียน fix แยก

---

## Story Details

### S5-05 — Pattern-B/C/D one-liner replacements

**Type**: Logic
**ADR**: ADR-0006 §5.2 (Pattern B), §5.3 (Pattern C), §5.4 (Pattern D); revised per ADR-0008
**Manifest Version**: N/A (control-manifest.md not yet created)

#### Context

ADR-0006 §5 identifies 4 distinct refactor patterns in `ActorCombatAction`. S5-04
landed Pattern A (`IsActiveSlotOwner` helper, 5 sites). S5-05 lands the remaining
three patterns — each is a smaller one-line rewrite leveraging facades already
landed in S5-02/S5-03 and the `BoundSlot` networked property from S5-04.

**Pattern B (4 sites) — Input-to-slot binding** (ADR-0006 §5.2):
- B1: `ActorCombatAction.cs` ~line 1749–1754 (press path)
- B2: ~line 1764–1769 (release path)
- B3: ~line 1781–1786 (charge-press variant)
- B4: ~line 1909 (single-line condition — `input.Buttons.IsSet(Buttons.Q/W/E/R/Recall)` match)

Replacement (ADR-0006 §5.2, revised per ADR-0008 — `AbilityData.Slot` reverted,
use `BoundSlot` from S5-04 instead):

```csharp
// Press:    if (input.PressedSlot != 0 && input.PressedSlot == BoundSlot) { ... }
// Release:  if (input.ReleasedSlot != 0 && input.ReleasedSlot == BoundSlot) { ... }
```

**Pattern C (1 site) — Rank-up exclusions** (ADR-0006 §5.3):
- C1: `ActorCombatAction.cs` ~line 2115/2125/2133 region

```csharp
// Old: if (SkillKey == SkillKey.A || SkillKey.Item || SkillKey.Recall) return;
// New: if (AbilityData.MaxRank <= 1) return;
```

**Correction (2026-05-13)**: ADR-0006 §5.3 referenced `IsLeveled` which is on
`AbilityDataSnapshot` — not on `CBSAbility` (`AbilityData` on `ActorCombatAction`).
The concrete check is `MaxRank <= 1` (A/Item/Recall have `cbs.MaxRank == 1` per
ADR §5.3 equivalence claim).

**Pattern D (1 site) — Quick-cast settings** (ADR-0006 §5.4):
- D1: `ActorCombatAction.cs` ~line 1790–1795

```csharp
// Old: if (Actor.Combat.IsQuickQ && SkillKey.Q || IsQuickW && SkillKey.W || ...)
// New: if (Actor.Combat.IsQuickCast(BoundSlot)) { ... }
```

**S5-04 dual-path consistency**: Like S5-04, all 3 patterns must handle the
legacy heroes (`BoundSlot == 0`) path. Suggested approach: keep legacy SkillKey
check as fallback when `BoundSlot == 0` (same dual-path strategy as
`IsActiveSlotOwner` + `ResolveLegacyOwnerBySkillKey`), removable in Phase 3.

#### Acceptance Criteria

1. **Pattern B (4 sites)**: All 4 sites use `PressedSlot/ReleasedSlot == BoundSlot`
   for migrated heroes (Hercules); legacy SkillKey fallback retained when
   `BoundSlot == 0`. Verified via code review.
2. **Pattern C (1 site)**: SkillKey chain replaced with `!IsLeveled` check.
   `IsLeveled` already populated by S3-03 — no new field needed.
3. **Pattern D (1 site)**: Quick-cast chain replaced with `IsQuickCast(BoundSlot)`
   facade (added in S5-03). Legacy SkillKey fallback retained when `BoundSlot == 0`.
4. **Multipeer harness Pass #1–5 still pass** — regression gate. Bandwidth
   ≤ 65 B/s preserved (no new networked state added).
5. **Hercules QWER playable in Training match**: cast, deal damage, no input
   errors, 1-match playthrough completes without regression vs. pre-S5-05 baseline.
6. **EditMode tests pass**: existing `ActorCombatActionOwnerGuardTests` (S5-04)
   stay green; if a pure helper is extracted for Pattern D's `IsQuickCast`-fallback
   logic, add a test (otherwise N/A).

#### Out of Scope

- Removing legacy SkillKey fallback path (Phase 3 cleanup)
- Pattern A refactor (done in S5-04)
- `HerculesRAction` / `HerculesWAction` sites (done in S5-07/S5-08)
- `AnimationEvent` shim wiring (S5-06)
- `AbilityComponent` prefab attach on Hero prefab (gates S5-10, deferred to manual step)

#### Test Evidence

**Required**:
- Multipeer harness log: `production/qa/evidence/S5-05-multipeer.txt` — capture
  Pass #1–5 (parity + bandwidth)
- Hercules playthrough note: `production/qa/evidence/S5-05-playthrough.md`
  documenting 1-match Training session result + screenshot of HUD post-match

**Optional**:
- EditMode test additions (if helper extraction warranted) under
  `Assets/UnitTests/TestEditMode/`

#### Performance Impact

No performance impact expected — refactor is pure code transformation (same
runtime behavior, fewer branches). Hot-path branch count drops marginally
(7-way SkillKey switch → 1-way slot equality + 1 fallback branch).

#### Files to Modify

- `Assets/GameScripts/Gameplays/Characters/ActorCombatAction.cs` (4+1+1 = 6 sites)
- No new files expected (helpers, if any, → same file as S5-04 pattern)

---

### S5-06 — AnimationEvent Option A (43 shim methods)

**Type**: Logic
**ADR**: ADR-0006 Phase 2 Migration Plan §6.2 (AnimationEvent Option A confirmation)
**Manifest Version**: N/A (control-manifest.md not yet created)

#### Context

`AnimationEvent.cs` เป็น `INetworkActor` helper ที่รับ callback จาก animator
clips ของฮีโร่ทุกตัว (Hercules pilot + heroes ทั้ง ~25 ตัวใน Phase 3) ผ่าน
43 method shims ที่ animator state เรียกชื่อตรง ๆ (e.g. animator state
`Q_Perform` มี Animation Event เรียก `Skill_Q_Perform`).

ปัจจุบันทุก shim เรียก `StateRelease(SkillKey.X, SkillState.Y, param)` ซึ่ง
hardcode SkillKey literal. **Option A** จาก audit (ADR §6.2):

```csharp
// Before:
public void Skill_Q_Perform(int param) => StateRelease(SkillKey.Q, SkillState.Perform, param);
// After:
public void Skill_Q_Perform(int param) => StateReleaseSlot(Actor.Combat.GetActiveSlot(), SkillState.Perform, param);
```

**เหตุผลที่เลือก Option A:**
- **Zero animator clip edits** — ทั้ง 25+ hero clips ยัง bind ชื่อ method เดิม (`Skill_Q_Perform`, etc.)
- ไม่ต้อง rename / re-bind / migrate SkinObject dictionary
- เปลี่ยนแค่ method body — diff เล็ก, rollback ง่าย
- (Option B/C ต้อง rebind animator events → rejected)

**Implementation note (ค้นพบจาก survey 2026-05-13):**
- `GetActiveSlot()` มีอยู่แล้วใน `ActorCombat:221` (เพิ่มใน S5-03) — return latest pressed slot 1-7
- `StateReleaseSlot(byte, SkillState, int)` **ยังไม่มี** — ต้องเพิ่มใหม่ใน AnimationEvent.cs
- Dictionary registration APIs (`AddReleaseEvent`/`AddEnterEvent`/`AddExitEvent`) ยัง key ด้วย `SkillKey` — `StateReleaseSlot` ต้อง resolve `SkillKey` จาก slot เพื่อ lookup dictionary เดิม (หรือเพิ่ม slot-keyed dictionary parallel — ตัดสินใจตอน implement)
- Legacy heroes (Anansi/Merlin/Garen — `BoundSlot==0`): `GetActiveSlot()` ปัจจุบัน return อะไร? ต้องตรวจ + กำหนด fallback (dual-path ตาม pattern S5-04/S5-05)

#### Acceptance Criteria

1. **`StateReleaseSlot(byte slot, SkillState state, int id)` method ใหม่** — รับ slot index แทน SkillKey; route ไปยัง dictionary callback ที่มีอยู่ผ่านการ resolve SkillKey จาก slot (`Actor.Combat.GetSlotAction(slot)?.AbilityData?.SkillKey`) หรือ slot-keyed dictionary parallel
2. **40 shim methods** เปลี่ยนจาก `StateRelease(SkillKey.X, ...)` → `StateReleaseSlot(Actor.Combat.GetActiveSlot(), ...)`:
   - 6 A's (Attack_1Event, _2Event, _3Event, Skill_A_Empower, _Empower2, _Empower3)
   - 8 Q's, 8 W's, 8 E's, 8 R's (Perform/2/3/4/5 + Empower/2/3 each)
   - 1 Recall, 1 Item
   - **3 Skill_I_Empower/2/3 retained on legacy `StateRelease(SkillKey.I, ...)`** per code-review finding F2: Innate/Passive abilities have no `SetActiveSlot` write path; migrating would silently route animation events to the most-recently-cast slot (Q/W/E/R). Phase 3 unblocks once passive slot-binding path exists.
3. **Item aliases (4 methods)** — `Skill_Item_{Recall,Consume,Spell,Attack}_Perform` ยังเรียก `Skill_Item_Perform(param)` เหมือนเดิม (ไม่ต้องแก้ — wrapper layer)
4. **Dual-path fallback** — ถ้า `GetActiveSlot()` return 0 (legacy hero / no active skill), fall back ไปเรียก legacy `StateRelease(SkillKey.X, ...)` หรือ no-op พร้อม warning log (decide ตอน implement; document inline)
5. **No animator clip touches** — ตรวจ Hercules animator clip events ก่อน/หลัง: bind ชื่อ method เดิมไม่เปลี่ยน
6. **EditMode tests pass** — `AnimationEventSlotTests.cs` (NEW): mock `GetActiveSlot()` แล้ว call ทั้ง 43 shims, assert `StateReleaseSlot` ได้รับ slot ถูกต้อง + SkillState ถูกต้อง
7. **Multipeer harness Pass #1–5 ยัง green** — bandwidth ไม่เปลี่ยน (pure runtime substitution)
8. **Hercules Training playthrough** — cast Q/W/E/R/A/Recall ครบ 6 abilities, animation event fire ทุกจุด, ไม่มี NRE ใน Editor.log

#### Out of Scope

- ลบ `StateRelease(SkillKey, ...)` method เดิม — เก็บไว้เป็น legacy fallback (Phase 3 cleanup)
- ลบ `m_OnReleaseDictionary` / `m_OnEnterDictionary` / `m_OnExitDictionary` SkillKey keys — เก็บไว้ (Phase 3)
- เปลี่ยน `OnEnter(SkillKey, ...)` / `OnExit(SkillKey, ...)` signatures — Phase 3 candidate
- Migrate `AddReleaseEvent(SkillKey, callback)` registration API → slot-based — Phase 3
- `SkillObjectDictionary` / `SkillVfxDictionary` / `SkinObject` rekey — Phase 3 (ADR §6.3)
- Non-hero AnimationEvent users (monster/boss animations) — ยังใช้ legacy path
- Recall edge case (`SkillKey.Recall` → slot 7 hard-bind) — verify mapping ใน implement; ถ้า GetActiveSlot ไม่ track Recall ให้ใช้ legacy fallback

#### Test Evidence

**Required:**
- EditMode tests: `Assets/UnitTests/TestEditMode/AnimationEventSlotTests.cs` (NEW)
  - 43 cases — one per shim method (assert correct slot routing)
  - 1 case — `GetActiveSlot() == 0` fallback path
  - 4 cases — Item aliases route through `Skill_Item_Perform`
- Multipeer log: `production/qa/evidence/S5-06-multipeer.txt` — Pass #1–5
- Hercules playthrough note: `production/qa/evidence/S5-06-playthrough.md` — 1-match Training session

**Optional:**
- Static helper extraction (e.g., `ResolveSkillKeyFromSlot(byte)`) ถ้า dual-path logic ซับซ้อน → เพิ่ม unit test ตามลำดับ pattern S5-04/S5-05

#### Performance Impact

ไม่มี — pure code substitution; runtime path เพิ่ม 1 indirection (`GetActiveSlot()` call ที่ return cached `m_ActiveSlot` byte field) ต่อ animation event. Animation events fire ~5-10 ครั้งต่อ skill cast → negligible (<1µs total per cast).

#### Files to Modify

- `Assets/GameScripts/Gameplays/Characters/AnimationEvent.cs`
  - 1 method addition: `StateReleaseSlot(byte, SkillState, int)` (~15 lines)
  - 43 method body changes (1-liner each → ~43 lines diff)
  - **Total**: ~60 lines diff
- `Assets/UnitTests/TestEditMode/AnimationEventSlotTests.cs` (NEW, ~150–200 lines, ~48 test cases)

#### Dual-Path Strategy

ตาม pattern ที่ใช้ใน S5-04/S5-05 (`IsActiveSlotOwner`, `IsInputSlotMatch`):

```csharp
private void StateReleaseSlot(byte slot, SkillState state, int id)
{
    // Slot-bound path (Hercules + migrated heroes)
    if (slot != 0)
    {
        var action = Actor.Combat.GetSlotAction(slot);
        if (action?.AbilityData != null)
        {
            StateRelease(action.AbilityData.SkillKey, state, id);
            return;
        }
    }
    // Legacy fallback: GetActiveSlot returned 0 → no active skill / unmigrated hero
    // Phase 3: delete this branch when all heroes migrated
}
```

**Removal target:** Phase 3 — เมื่อ `SkillKey` enum ถูกลบ และ dictionaries ย้ายเป็น slot-keyed

#### Dependencies

- **S5-03** ✅ (`GetActiveSlot` + `GetSlotAction` facades — DONE 2026-05-10)
- ไม่มี blocker

#### Risks

- **R1 (per ADR §8)**: `GetActiveSlot()` return wrong slot ระหว่าง multi-skill chain (e.g., Q ยังเล่น animation อยู่ตอนกด W) → animation event ของ Q routes ไปที่ W's slot.
  **Mitigation**: EditMode test simulate Q→W press sequence, assert AnimationEvent fired ระหว่าง Q animation ยัง return Q's slot. Verify `m_ActiveSlot` semantics ใน `ActorCombat`.
- **R2**: `SkillKey.A` / `SkillKey.I` / `SkillKey.Recall` / `SkillKey.Item` ไม่มี slot binding ที่ชัดเจนใน BindSlot pipeline — `GetActiveSlot()` อาจ return 0 ขณะ animation fire.
  **Mitigation**: Test ทั้ง 4 SkillKey types; ถ้า fail → enhance dual-path fallback หรือ keep legacy SkillKey-based path สำหรับ A/I/Recall/Item

---

### S5-21 — TD-006 SetActiveSlot wiring + S5-06 40-shim re-migration

**Type**: Logic (Phase 2 polish — must ship before Phase 3 starts 2026-05-21)
**ADR**: ADR-0006 Phase 2 §6.2 (Option A) + §3 Exit Criterion #6 (AnimationEvent)
**Manifest Version**: N/A (control-manifest.md not yet created)
**Dependencies**: S5-06 ✅ (infrastructure landed PR #351), S5-10 ✅ (TD-007 resolved, BindSlot pipeline live)

#### Context

S5-06 landed `StateReleaseSlot` dispatcher + `TryResolveSlotRoute` pure helper + 12 tests as **infrastructure-only** because the 40-shim migration broke production VFX. Root cause (TD-006): ADR-0006 §6.2 promised `ActorCombat.SetActiveSlot()` would be wired in `ActorCombatAction` input handlers, but the caller was never landed in S5-03. With no writer for `m_ActiveSlot`, `GetActiveSlot()` returns 0 → dispatcher dual-path fallback drops every animation event in shipping builds.

S5-21 closes the gap: **wire the caller, then re-land the 40-shim migration atomically**.

#### Key code locations (from survey)

- `Assets/GameScripts/Gameplays/Characters/ActorCombatAction.cs:511` — `[Networked] byte BoundSlot { get; private set; }` (S5-04 added)
- `Assets/GameScripts/Gameplays/Characters/ActorCombatAction.cs:2222` — `OnPressButtons(InputMessage input)` — central press handler, 5 call sites (B1/B2/B3/D/B4 from S5-05). **Insertion point for SetActiveSlot.**
- `Assets/GameScripts/Gameplays/Characters/ActorCombat.cs:131` — `GetSlotAction(byte)` facade with slot mapping (1=Q, 2=W, 3=E, 4=R, 5=A, 6=I, 7=Recall).
- `Assets/GameScripts/Gameplays/Characters/ActorCombat.cs:233` — `SetActiveSlot(byte)` — has `HasStateAuthority` guard.
- `Assets/GameScripts/Gameplays/Characters/AnimationEvent.cs:167` — `StateReleaseSlot` dispatcher (no-op infrastructure from PR #351).
- `Assets/GameScripts/Gameplays/Characters/AnimationEvent.cs:213-303` — 43 shim methods (currently 40 on legacy + 3 Skill_I_* on legacy per F2 retention).

#### Implementation

**Step 1 — Add `ResolveSlotFromSkillKey` static helper** (`ActorCombatAction.cs`, near other Resolve* helpers):

```csharp
/// <summary>
/// Maps a <see cref="SkillKey"/> to its corresponding slot index for legacy
/// (unmigrated) heroes whose <see cref="BoundSlot"/> is still 0. Slot indexing
/// matches <see cref="ActorCombat.ResolveSlotAction"/> (1=Q, 2=W, 3=E, 4=R,
/// 5=A, 6=I, 7=Recall). Returns 0 for SkillKey.Item / None.
/// Phase 3: delete when SkillKey enum is retired (ADR-0006 §10).
/// </summary>
public static byte ResolveSlotFromSkillKey(SkillKey key) => key switch
{
    SkillKey.Q => 1,
    SkillKey.W => 2,
    SkillKey.E => 3,
    SkillKey.R => 4,
    SkillKey.A => 5,
    SkillKey.I => 6,
    SkillKey.Recall => 7,
    _ => 0,
};
```

**Step 2 — Wire SetActiveSlot in `OnPressButtons`** (`ActorCombatAction.cs:2222`, at top of method):

```csharp
protected void OnPressButtons(InputMessage input)
{
    // S5-21 (TD-006 fix): Update active slot for animation event routing.
    // Dual-path: BoundSlot if migrated (S5-04 SetBoundSlot via ISlotBinder),
    // else legacy SkillKey→slot map for unmigrated heroes. Phase 3: drop the
    // legacy fallback when SkillKey enum is retired (ADR-0006 §10).
    var slot = BoundSlot != 0 ? BoundSlot : ResolveSlotFromSkillKey(AbilityData.SkillKey);
    if (slot != 0) Actor.Combat.SetActiveSlot(slot);

    // ... existing body unchanged ...
}
```

**Step 2 REVISED (post-v1 regression, 2026-05-14):** Initial plan above placed SetActiveSlot in `OnPressButtons`, but Unity playtest revealed that **Normal Attack auto-target flow sets `Progress = SkillState.Attack1` directly at lines 2693/2770 — bypassing `OnPressButtons` entirely** → m_ActiveSlot stayed 0 for Normal Attack → 30+ "slot=0" warnings + no damage popups.

The fix: move SetActiveSlot into the `Progress` setter (line 402) — single source of truth for "ability is now active". Every state transition path (manual press, auto-attack, R-charge release, passive triggers) writes through it. Server-only guard (`Runner.IsServer`) already in place.

```csharp
public SkillState Progress
{
    set
    {
        if (Object == null || !Object.IsValid) return;
        if (m_Progress == value) return;
        if (Runner.IsServer)
        {
            m_Progress = value;

            // S5-21 (TD-006 fix): Update active slot whenever ability transitions
            // to a non-None state. Catches auto-attack path that bypasses
            // OnPressButtons. Skip None so late animation events still route.
            if (value != SkillState.None && AbilityData != null)
            {
                var activeSlot = BoundSlot != 0
                    ? BoundSlot
                    : ResolveSlotFromSkillKey(AbilityData.SkillKey);
                if (activeSlot != 0) Actor.Combat.SetActiveSlot(activeSlot);
            }
        }
    }
}
```

`OnPressButtons` keeps a 3-line comment pointing at the Progress setter — no behavioural change in OnPressButtons.

**Step 3 — Re-land 40-shim migration in `AnimationEvent.cs`** (lines 213-303):

- Flip 40 shim bodies from `StateRelease(SkillKey.X, ...)` → `StateReleaseSlot(Actor.Combat.GetActiveSlot(), ...)`
- **Retain** 3 Skill_I_* shims on legacy `StateRelease(SkillKey.I, ...)` (F2 — passive has no `OnPressButtons` call path, so SetActiveSlot never fires for slot 6)
- **Retain** 4 Item aliases unchanged (they route through `Skill_Item_Perform` which IS one of the 40)
- **Remove** S5-06 inline comment "S5-06 scope revert" — replace with concise S5-21 done note

#### Acceptance Criteria

1. **`ResolveSlotFromSkillKey(SkillKey) → byte` static helper** added with 7 active cases + default 0 fallback. Pure function, zero Unity deps, EditMode-testable.
2. **`OnPressButtons` calls `Actor.Combat.SetActiveSlot(slot)`** at top of method, with dual-path slot resolution (BoundSlot first, else SkillKey map fallback).
3. **40 shim methods migrated** in `AnimationEvent.cs` to `StateReleaseSlot(Actor.Combat.GetActiveSlot(), ...)`. 3 Skill_I_* + 4 Item aliases + `Attack_Event` (empty) unchanged.
4. **EditMode tests pass** — new `ActorCombatActionSlotResolverTests.cs` (8 cases: Q/W/E/R/A/I/Recall + Item-returns-0). Existing 108 tests stay green.
5. **No production-breaking regression**: VFX/SFX/animation events fire correctly in `scene_game_map.unity` for Hercules QWER + A + Recall + Item. 14 BindSlot warnings → still 0 (TD-007 fix from S5-10 stays effective).
6. **No animator clip touches** — 43 method names preserved.
7. **Multipeer Pass #1–5 still green** — bandwidth ≤65 B/s preserved (no new networked-state added; SetActiveSlot is an existing networked write).

#### Out of Scope

- Removing legacy `StateRelease(SkillKey, ...)` method (Phase 3 cleanup — still called by 3 Skill_I_* and as dispatch target from `StateReleaseSlot`)
- Migrating `AddReleaseEvent(SkillKey, ...)` registration API to slot-based (Phase 3)
- Touching `OnEnter`/`OnExit` SkillKey signatures (Phase 3)
- Removing `SkillKey` enum (Phase 3/4)
- PlayMode test framework for Fusion NetworkBehaviour (Sprint 006 candidate — too expensive for this story)
- Bot input path SetActiveSlot — `BotActor.Auto/WithTarget` already populates `PressedSlot` per S5-05 fix; if bots route through `OnPressButtons` too, they get SetActiveSlot for free. If not, defer to Sprint 006 polish.

#### Test Evidence

**Required:**
- EditMode tests: `Assets/UnitTests/TestEditMode/ActorCombatActionSlotResolverTests.cs` (NEW, 8 cases)
- Manual playthrough: confirm `scene_game_map.unity` Console has 0 `BindSlot not registered` warnings (was the TD-007 fix from S5-10) and no new error/warning spikes.
- Multipeer log capture: `production/qa/evidence/S5-21-multipeer.txt` — Pass #1–5

**Optional:**
- Extend `AbilityMultipeerRunner` parity check to assert `m_ActiveSlot` updates after simulated press (≤15 min effort; surfaces same regression if it reoccurs)

#### Performance Impact

Negligible. `OnPressButtons` is called once per ability press (~10 Hz max per player). Adding 1 byte field write + 1 enum switch is sub-microsecond. `[Networked]` write replicates at Fusion tick rate (~30 Hz) — bandwidth delta ≤2 B/s (already accounted for in S5-04 Pass #5 budget).

#### Files to Modify

- `Assets/GameScripts/Gameplays/Characters/ActorCombatAction.cs` (~12 lines added: helper + OnPressButtons insertion)
- `Assets/GameScripts/Gameplays/Characters/AnimationEvent.cs` (40 shim bodies + 1 comment block replaced)
- `Assets/UnitTests/TestEditMode/ActorCombatActionSlotResolverTests.cs` (NEW, ~80 lines)

#### Dual-Path Strategy & Phase 3 Removal

Two dual-paths exist after this story:
1. **OnPressButtons slot resolution**: `BoundSlot != 0 ? BoundSlot : ResolveSlotFromSkillKey(AbilityData.SkillKey)`
   - Removal: Phase 3 hero migration drains BoundSlot=0 heroes → fallback unused → delete legacy branch.
2. **AnimationEvent.StateReleaseSlot fallback**: `slot != 0 ? route : warn` (LogWarning gated under `#if UNITY_EDITOR || DEVELOPMENT_BUILD` per S5-06 F1)
   - Removal: Phase 3 same trigger — when BoundSlot=0 is impossible, this branch is dead.

#### Risks

- **R1**: Multi-skill chain race (Q animation frame fires after W pressed → reads slot=2 not slot=1).
  **Mitigation**: Documented as TD-003 (Phase 3 candidate — single-slot tracker by design). Multipeer + manual playtest confirms current cast cadence (~10 Hz max) doesn't trigger the race in practice.
- **R2**: `BotActor` input path doesn't route through `OnPressButtons` → bot abilities silently broken.
  **Mitigation**: Verify in manual playtest that bot Hercules (vs human Hercules) animations + damage fire. If gap, scope expand or revert.
- **R3**: `AbilityData == null` at `OnPressButtons` entry → NRE in `ResolveSlotFromSkillKey(AbilityData.SkillKey)`.
  **Mitigation**: Existing code at `OnPressButtons:2225` already dereferences `AbilityData.SkillKey` — so this NRE path was already present pre-S5-21. No new risk.

---

### S5-10 — Manual playtest + Phase 2 closeout gate

**Type**: Integration (playtest gate — no automated tests; manual evidence only)
**ADR**: ADR-0006 Phase 2 §3 (Exit Criteria) + §10 (Phase 2 → Phase 3 Handover)
**Manifest Version**: N/A (control-manifest.md not yet created)
**Dependencies**: S5-01..S5-09 ✅ (S5-06 partial-land per ADR §7 — `S5-06 AnimationEvent` was parallelizable; does not block Hercules pilot)

#### Context

Final gate ของ Phase 2 Hercules pilot. แม้ EditMode tests + multipeer
harness Pass #4–5 จะ regress green ใน S5-04/S5-05 แล้ว, ADR §3 Exit
Criteria กำหนดว่า Phase 2 จะปิดเมื่อ **Hercules เล่นได้ end-to-end ใน
real match scene** + manual playthrough confirms cast/charge/cancel
flows identical to pre-migration.

S5-09 deferred manual `AbilityComponent` prefab attach ไปที่ S5-10 —
warning log fires until the attach lands (Editor-only, gated on `Actor.ObjectType == TargetType.Hero` per S5-04 sidecar).

#### Manual steps (user execution in Unity Editor)

**Step 1 — `AbilityComponent` prefab attach (S5-09 deferred):**
- Open Hercules Hero prefab (under `Assets/Resources/Prefabs/Gameplay/Character/Hero/` or similar)
- Add Component → `AbilityComponent` (namespace `Radius.Gameplays.Abilities`)
- Apply prefab override (Inspector > Overrides > Apply All)
- Verify: subsequent Play session no longer logs `BootstrapSlotBindings: AbilityComponent missing` warning

**Step 2 — Multipeer harness Pass #1–5 re-verify (post-merge sanity):**
- Open `AbilityMultipeerRunner` scene
- Run harness 3 sessions back-to-back
- Capture Editor.log → `production/qa/evidence/S5-10-multipeer.txt`
- Confirm 0 errors, "Both runners online" × 3, bandwidth ≤65 B/s

**Step 3 — Hercules QWER Training match:**
- Boot Training scene with Hercules + 1 bot opponent
- Cast Q / W / E / R sequentially — verify damage, VFX, animation, cooldown
- Use Normal Attack (A) for full chain (Attack_1/2/3)
- Use Recall to fountain
- Use Item from inventory (recall/consume/spell/attack variants if available)
- Confirm: no Console errors, FPS stays in pre-Phase-2 range
- Capture: post-match HUD screenshot + brief notes per ability

#### Acceptance Criteria (per ADR §3 + QA plan)

1. Match boots to playable state — Hercules joins, bot ready, no Spawned() NRE
2. All 4 Hercules abilities (Q/W/E/R) usable mid-match — cast, deal damage, no input errors, animator transitions clean
3. Normal Attack (A) + Recall + Item variants play correct animation + trigger correct effect
4. No crash, no Console error spike (warnings OK; document any new ones — distinguish from BUG-0001/BUG-0003)
5. Multipeer harness Pass #1–5 all green — bandwidth ≤65 B/s preserved
6. FPS stays within ±10% of pre-Phase-2 baseline (no perf regression)
7. `AbilityComponent` attach removes `BootstrapSlotBindings` missing-component warning
8. Evidence files exist with sign-off note: gameplay-programmer + qa-tester (user wears both hats for this sprint)

#### Out of Scope

- S5-06 functional migration (deferred to S5-21 per TD-006)
- Recall `ReleasedSlot` full migration (ADR §7 known limitation — Phase 3 cleanup)
- Non-Hercules heroes (Anansi / Merlin / Garen / etc. — ยังใช้ legacy path until Phase 3)
- BUG-0001 Recall locomotion (separate ticket S5-19; may surface during playthrough — document but don't block)
- BUG-0003 NetworkRunnerInput NRE root cause (band-aid already applied; root-cause investigation = Sprint 006)
- Garen variant validation (S5-13)

#### Test Evidence

**Required:**
- `production/qa/evidence/sprint-005-hercules-playthrough.md` (NEW) — Per-ability checklist + screenshots + sign-off
- `production/qa/evidence/S5-10-multipeer.txt` (NEW) — Multipeer Pass #1–5 log capture

**Optional:**
- Screen recording (mp4) — if recording feasible, attach link/path in playthrough doc

#### Phase 2 → Phase 3 Handover (ADR §10)

Phase 3 starts when S5-10 ships AND:
1. Hercules live on dev branch for ≥1 week with no slot-related bugs raised by QA
2. `ActorCombat.GetSlotAction` facade has ≥1 non-Hercules call site (proof API generalizes)
3. `AbilityDataSnapshot.EffectiveSlot` verified for all CBS records via audit script
4. Pattern-A helper (`IsActiveSlotOwner`) peer-reviewed + documented in `control-manifest.md`

#### Files to Modify (planning docs only — no game code change in S5-10)

- `production/qa/evidence/sprint-005-hercules-playthrough.md` (NEW, evidence doc)
- `production/qa/evidence/S5-10-multipeer.txt` (NEW, log capture)
- `production/sprints/sprint-005.md` (Progress + Must Have closeout)
- `production/sprint-status.yaml` (S5-10 → done; Sprint 005 Must Have 10/10)
- Hercules Hero prefab (.prefab) — manual edit in Unity Editor (1 component addition)

---

## Carryover from Previous Sprint

### Accepted into Sprint 005

| Task | เหตุผล | From Sprint | Estimate |
|------|--------|-------------|----------|
| S5-01..S5-10 (P2-01..P2-10) | Phase 2 plan ที่ deferred จาก Sprint 004 ตามแผน ADR-0006 | 004 (deferred by design) | 3.75d |
| S5-11 R-21 decision | Retro action #2 — blocks S4-09 AI Bot priority list | 004 retro | 1.0d |
| S5-12 AI Bot fate | Retro action #3 — 3-sprint carryover triggers root-cause | 004 retro | 0.5d |
| S5-13 Garen variants | Retro action #5 | 004 retro | 0.5d |
| S5-14..S5-16 process docs | Retro actions #1, S3-retro #3, S3-retro #4 | 004 + 003 retro | 0.75d |

### Deferred from Sprint 005 to Sprint 006+

| Task | เหตุผล |
|------|--------|
| S4-09 AI Bot item-buying (2.0d) | รอ S5-11 (R-21 Role Restriction decision) ก่อน — design ของ Bot item priority depends on whether role gating enforced |
| S4-10 AI Bot Difficulty (1.0d) | depends S4-09 |
| R-22 / R-23 implementation (stat /100 + AdditionalMoveSpeed rename) | Phase 2 scope already 3.75d — schedule with Phase 3 หรือ dedicated balance pass; ADR-only ใน Sprint 005 ถ้ามีเวลา |

---

## Risks

| Risk | Probability | Impact | Mitigation |
|------|------------|--------|-----------|
| Phase 2 P2-04 Pattern-A helper extraction misses edge case → Hercules behavior regression | Low | High | Multipeer harness Pass #1-5 + manual Hercules playthrough เป็น regression gate (S5-10) |
| `CBSAbility.Slot` PlayFab CBS dashboard update ต้อง designer ทำ | Medium | High | `EffectiveSlot` shim derives from `SkillKey` เมื่อ Slot==0 → code unblock; designer fill on-deck |
| Hercules R charge release ยังใช้ `Buttons.R` (ReleasedSlot deferred S3-05) | Medium | Medium | Phase 2 keeps as known limitation; full removal Phase 3 |
| S5-11 R-21 decision ไม่ landed → blocks Sprint 006 AI Bot kickoff | Medium | Medium | Time-box S5-11 ที่ 1d; ถ้าไม่ตัดสินใจได้ใน 1d → escalate creative-director |
| Sprint 004 estimation pattern repeat (overestimate reverse-doc) | Low | Low | Phase 2 stories เป็น real implementation ไม่ใช่ reverse-doc → estimate น่าจะ accurate |

---

## Dependencies on External Factors

- **PlayFab CBS dashboard access** — designer ต้องอัพเดต `CBSAbility.Slot` field after S5-01 schema change
- **Photon PeerMode** — ต้อง `Single` mode สำหรับ S5-09/S5-10 production playtest (S4-P5 workflow doc)
- **Multipeer harness** — รัน Pass #1-5 หลัง S5-04, S5-05, S5-09 เป็น regression check

---

## Definition of Done for this Sprint

### Per-Task Gates

- [ ] All Must Have tasks (S5-01..S5-10) completed
- [ ] All tasks pass acceptance criteria
- [ ] **QA plan exists** (`production/qa/qa-plan-sprint-005.md`) — required for Phase 2
- [ ] Logic stories (S5-01, S5-02, S5-03) have passing unit tests
- [ ] Integration story (S5-10) has playtest evidence in `production/qa/evidence/`
- [ ] Smoke check passed (`/smoke-check sprint`)
- [ ] QA sign-off report: APPROVED or APPROVED WITH CONDITIONS
- [ ] No S1 or S2 bugs in Hercules playthrough
- [ ] ADR-0006 Phase 2 → Phase 3 handover criteria met (§10)
- [ ] ADR-0008 (Role Restriction) Status=Accepted
- [ ] AI Bot fate decision documented
- [ ] Code reviewed and merged to main

### Cross-cutting

- [ ] No new TODO/FIXME accumulation > +5 from baseline (Sprint 004: TODO=14)
- [ ] Risk register updated if Phase 2 reveals new risks
- [ ] Sprint 005 retrospective generated at sprint end

---

## Progress

- **2026-05-12 — S5-09 COMPLETE** (delta-unity@claude/s5-09-hercules-bootstrap). ActorCombat.OnStartup now reads CBSUnit.SlotQ..SlotI aliases and dispatches to ISlotBinder.BindSlot ×6 + slot 7 Recall (Hero only). Introduced `ISlotBinder` interface in Radius asmdef to break Radius↔Abilities asmdef cycle (user-approved Option A). Dual-path retention: legacy `CreateSkill` continues to operate. 12/12 EditMode tests pass. Manual prefab attach (Unity Editor: drop AbilityComponent onto Hero prefab) deferred to S5-10 — warning log fires until done.
- **2026-05-12 — BUG-0002 RESOLVED** (delta-unity@91697bf78e on dev) — Anansi W animator stuck idle on client peer. Tracked as S5-20 in sprint-status.yaml.
- **2026-05-13 — S5-04 COMPLETE** (delta-unity@claude/s5-04-pattern-a-helper). `IsActiveSlotOwner()` helper with dual-path: slot-indexed (`[Networked] BoundSlot` + `ResolveIsActiveSlotOwner` reference equality) for migrated heroes (Hercules), legacy `ResolveLegacyOwnerBySkillKey` fallback when `BoundSlot==0` for unmigrated heroes (Anansi/Merlin/Garen/etc.). All 5 owner-guard sites in Enter/Release/Exit/FixedUpdateNetwork/OnFixedUpdateState replaced. 14/14 EditMode tests pass. Multipeer harness Pass #4 (parity) + #5 (bandwidth ≤65 B/s) verified. ADR-0006 §5.1 sample code stale post-ADR-0008 (`AbilityData.Slot` reverted) — user-approved option [D] applied; ADR amendment candidate for Phase 3 backlog.
- **2026-05-13 — Bonus work landed in S5-04 branch** (user-approved sidecar fixes):
  - **S5-09 follow-up:** `ActorCombat.BootstrapSlotBindings` warning gated on `Actor.ObjectType == TargetType.Hero` — eliminates log spam from creep/tower/monster/objective spawns (warning was intended for missing Hero AbilityComponent only).
  - **BUG-0003 band-aid:** `NetworkRunnerInput.cs` Q/W/E/R/Recall methods — 8 sites null-conditional `?.` patched. NRE was pre-existing (observed before S5-04 branch) but blocked S5-04 playtest verification. Filed at `production/qa/bugs/BUG-0003-network-runner-input-nre.md` with root-cause investigation plan; root cause TBD in Sprint 006.

- **2026-05-13 — S5-05 COMPLETE** (delta-unity, uncommitted on top of dev). Pattern-B/C/D one-liner replacements landed: `IsInputSlotMatch` (instance, dual-path) + `ResolveLegacySkillKeyButtonMatch` (static, EditMode-testable) + `IsQuickCastForBoundSlot` (instance) + `ResolveLegacyQuickCast` (static). 6 sites replaced (B1/B2/B3/D in `GetInput` pressed/released paths, B4 in `GetInputBot`, C `MaxRank <= 1` rank-up gate). 18 new EditMode tests pass (108/108 assembly green). Code review (unity-specialist) caught a **BLOCKING** bot regression: `BotActor.Auto` / `WithTarget` never populated `msg.PressedSlot` — fix landed (set `msg.PressedSlot = skill.BoundSlot;` in both methods). Multipeer Pass #1-5 (3 sessions "Both runners online") + Hercules Training playthrough with bots clean (Editor.log 0 S5-05 NRE). Deviations: Pattern C `AbilityData.IsLeveled` (ADR §5.3) → `MaxRank <= 1` (concrete field on `CBSAbility`); Pattern B4 semantic shift (slot path `PressedSlot` vs legacy `Buttons.IsSet`). BUG-0004 filed (pre-existing `BotActor.WithTarget` `msg`→`msg2` typo — out of S5-05 scope).

- **2026-05-13 — S5-06 INFRASTRUCTURE-ONLY (migration deferred)** (delta-unity@claude/s5-06-animation-event-option-a). Initial implementation migrated 40 shim methods to `StateReleaseSlot(Actor.Combat.GetActiveSlot(), ...)`, passed 44/44 EditMode + APPROVED code review. **Manual playtest in Unity Editor revealed VFX/SFX animation events were silently dropped on all heroes** — root cause: ADR-0006 §6.2 promised `ActorCombat.SetActiveSlot()` would be wired in `ActorCombatAction` input handlers, but that caller was never landed in S5-03. With no writer for `m_ActiveSlot`, `GetActiveSlot()` returns 0 for every hero, dual-path fallback fires, animation event dropped. **All 43 shim migrations reverted**; production behaviour fully restored. **Foundation retained**: `StateReleaseSlot(byte, SkillState, int)` dispatcher + pure static `TryResolveSlotRoute(...)` helper + 12 EditMode tests stay in file as no-op infrastructure ready for follow-up. **New tech debt TD-006**: SetActiveSlot wiring missing (HIGH priority — gates S5-21 + Phase 3 Option A). Code-review findings F1 (LogWarning gate) + F2 (Skill_I_* legacy) preserved in code as belt-and-braces for when migration retries.

- **2026-05-14 — S5-21 COMPLETE — TD-006 closed, Phase 2 fully shipped** (delta-unity@claude/s5-21-setactiveslot-wiring). Wired `Actor.Combat.SetActiveSlot()` into the `Progress` setter (single source of truth for state transitions) + added `ResolveSlotFromSkillKey` static helper (8 cases: Q/W/E/R/A/I/Recall + Item→0) + re-landed 40-shim migration in `AnimationEvent.cs` (3 Skill_I_* + 4 Item aliases retained on legacy per F2 + alias chain). **v2 fix learning**: initial v1 placed SetActiveSlot in `OnPressButtons`, but playtest revealed Normal Attack auto-target sets `Progress = SkillState.Attack1` directly at lines 2693/2770 — bypassing OnPressButtons → 30+ "slot=0" warnings + no NA damage. Moving to Progress setter caught all state-transition paths atomically. 8 new EditMode tests pass (116/116 in `Radius.Tests.Characters`); user-verified post-fix: Normal Attack damage restored, Console clean of `[S5-06] StateReleaseSlot ... slot=0` warnings, Hercules QWER all functional. **TD-006 RESOLVED**. Phase 2 Hercules pilot now fully shipped — Phase 2 Exit Criterion #6 (AnimationEvent Option A) ✅ promoted from PARTIAL to PASS.

- **2026-05-14 — S5-10 PASS — Phase 2 Hercules pilot closeout** (delta-unity@claude/s5-10-hercules-playtest, PR #353). Manual prefab attach: `AbilityComponent` added to `base_avatar.prefab` (single Hero template — propagates to all 25+ heroes, no per-hero edit). Multipeer Pass #1-5 verified: Pass #4 (parity) explicit ✅ in Editor.log (`slot=1..4 host↔client converge`); Pass #5 (bandwidth ≤65 B/s) baseline carry-forward from S5-04/S5-05 (zero networked-state delta on dev since). Hercules QWER + A + Recall + Item all functional in `scene_game_map.unity` (user confirmed "ปกติ" via S5-09 dual-path). **Pre-merge fix landed (2026-05-14)**: TD-007 surfaced during playtest (14 `BindSlot` warnings/match — `AbilityRegistry` not in `DeltaService.Services` list). Resolved by creating `Assets/Resources/Prefabs/Data/Services/AbilityRegistryService.prefab` (via Unity Editor) and adding to `DeltaConfiguration.Services` (12 → 13). Verified: 14 warnings → 0; Phase 2 Exit Criterion #5 now fully satisfied end-to-end (production scene exercises S5-09 BindSlot pipeline, was previously legacy-fallback-only). **TD-006** (SetActiveSlot caller missing) remains → S5-21 for AnimationEvent 40-shim migration retry. Cosmetic errors documented as known (S5-17 Photon `GameIsFull` cascade, BUG-0003 NRE band-aid). Evidence: `production/qa/evidence/sprint-005-hercules-playthrough.md` + `S5-10-multipeer.txt`. Sign-off: gameplay-programmer + qa-tester (tanapol both hats). **Phase 2 closure: PASS — 1-week soak begins 2026-05-14 → 2026-05-21 per ADR §10 handover gate.**

### Must Have status
- ✅ S5-01, S5-02, S5-03, S5-07, S5-08 — done (prior sessions)
- ✅ S5-09 — done (2026-05-12; TD-007 follow-up surfaced in S5-10)
- ✅ S5-04 — done (2026-05-13)
- ✅ S5-05 — done (2026-05-13)
- ⚠️ S5-06 — infrastructure landed (PR #351); 40-shim migration completed in S5-21.
- ✅ **S5-10 — PASS** (2026-05-14): Phase 2 Hercules pilot end-to-end gate satisfied. TD-007 fixed pre-merge.
- ✅ **S5-21 — COMPLETE** (2026-05-14): TD-006 RESOLVED. SetActiveSlot wired in Progress setter (single source of truth); 40-shim migration landed atomically. Phase 2 Exit Criterion #6 (AnimationEvent Option A) promoted PARTIAL → PASS.

**Sprint 005 Must Have: 10/10 PASS** ✅ — Phase 2 Hercules pilot fully shipped. Phase 3 unblocked.

---

## Source

- [Sprint 004 retrospective](../retrospectives/sprint-004.md) — 6 action items
- [ADR-0006 Phase 2 Migration Plan §7](../../docs/architecture/ADR-0006-phase-2-migration-plan.md) — P2-01..P2-10 work breakdown
- [ADR-0008 Slot Binding via CBSUnit](../../docs/architecture/ADR-0008-slot-binding-via-cbsunit.md) — supersedes ADR-0006 §6.1; affects S5-01 + S5-09 scope (2026-05-08)
- [Risk Register R-21..R-23](../risk-register/risk-register.md) — Open design decisions
- [Sprint 003 retrospective](../retrospectives/sprint-003.md) §5 — carry-forward actions #3, #4

## Mid-Sprint Pivot Log

**2026-05-08 — ADR-0006 §6.1 → ADR-0008 (CBSAbility.Slot superseded)**

S5-01 was implemented (additive `CBSAbility.Slot` field + `EffectiveSlot` shim + `SkillKeyToSlot` mapper + 7 unit tests) per ADR-0006 §6.1. User design review surfaced that slot binding should be sourced from `CBSUnit` (per-hero kit), not `CBSAbility` (per-ability slot self-declaration). Implementation reverted; ADR-0008 written; S5-01 scope reduced to alias properties on `CBSUnit`; S5-09 scope updated to read from CBSUnit aliases. Net Sprint 005 estimate unchanged (~3.75d Must Have).

Reverted code (in delta-unity repo): `CBSAbility.cs`, `AbilityDataSnapshot.cs`, deleted `AbilitySlotTests.cs`. No commits to dev branch — change was on worktree only.

## Retrospective Seeds (carry-forward to Sprint 005 retro)

**Finding 1 — NetworkBehaviour EditMode testability gap (S5-02 + S5-03)**

`ActorCombat` is a Fusion `NetworkBehaviour`. Instance methods that read networked properties (`Skill1..4`, `IsQuickQ..R`, `m_ActiveSlot`) cannot be exercised in EditMode without a live `NetworkRunner`. No existing test fixture in the project instantiates a Fusion type. S5-02 (`GetSlotAction`) and S5-03 (`IsQuickCast`) acceptance criteria specified "covered by unit test" — partially satisfied via static-helper extraction (`ResolveSlotAction`, `ResolveQuickCast`) which are pure and EditMode-testable. `GetActiveSlot` / `SetActiveSlot` networked-state behaviour falls back to multipeer harness + Hercules manual playtest (S5-10) for coverage.

**Process improvement candidate:** sprint planning should distinguish "Logic stories with pure C# state" (full EditMode coverage possible) from "Logic stories with NetworkBehaviour state" (requires PlayMode framework or static-helper extraction). Currently classified together as "Logic" by `/qa-plan`.

**Sprint 006 backlog candidate:** investigate establishing a PlayMode test framework that can spawn a minimal `NetworkRunner` for unit-level coverage of NetworkBehaviour subclasses. Estimate ~1-2d. If too expensive, codify "static-helper extraction for switch logic" as the canonical pattern.
