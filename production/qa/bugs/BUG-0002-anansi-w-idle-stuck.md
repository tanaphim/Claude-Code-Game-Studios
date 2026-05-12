# Bug Report

## Summary
**Title**: Anansi Skill W — หลังใช้สกิลจบ ตัวค้างท่า Idle ไม่ transition กลับ locomotion
**ID**: BUG-0002
**Severity**: S3-Minor
**Priority**: P2-Next Sprint
**Status**: **Verified Fixed** (2026-05-12)
**Reported**: 2026-05-11
**Reporter**: Tanapol Aekprapu

## Classification
- **Category**: Visual / Gameplay
- **System**: Anansi hero / Skill W / Animation State Machine
- **Frequency**: Always (สงสัย — รอ QA confirm)
- **Regression**: Unknown

## Environment
- **Build**: dev branch (post-Sprint 005 Phase 2 pilot)
- **Platform**: Unity Editor 2022.3.62f1 (รอ confirm)
- **Scene/Level**: Training / PvP map
- **Game State**: ขณะ control Anansi และใช้ skill W

## Reproduction Steps
**Preconditions**: เลือก Anansi, อยู่ใน match

1. กดสกิล W
2. รอ animation จบ
3. กดทิศทาง movement หลัง W จบ
4. ตัวค้างท่า Idle — ไม่เล่น locomotion animation

**Expected**: หลัง W จบ → return to locomotion state ปกติ (idle เมื่อยืน, walk/run เมื่อขยับ)
**Actual**: Animator ค้างที่ Idle pose ไม่ transition แม้ unit จะเคลื่อนที่

## Technical Context
- **Likely affected files** (in `delta-unity` repo): `AnansiWAction` (หรือชื่อใกล้เคียง), Anansi Animator controller, post-cast state cleanup
- **Related systems**: Animation event shim methods (S5-06 — 42 shim methods wired through `GetActiveSlot()`)
- **Possible root cause**:
  - Animator parameter (e.g. `IsCasting`, `SkillW`) ไม่ reset หลัง action complete
  - หรือ exit transition จาก W state ไม่ trigger
  - อาจเกี่ยวกับ S5-06 (AnimationEvent shim) ถ้า exit event ไม่ fire

## Evidence
- **Logs**: รอเก็บจาก reproduction
- **Visual**: รอ clip

## Related Issues
- BUG-0001 (Recall locomotion stuck) — อาการคล้ายกัน อาจ root cause เดียวกัน
- [Sprint 005 S5-06](../../sprints/sprint-005.md) — AnimationEvent shim wiring

## Notes
ขอตรวจสอบว่า W ใช้ AnimationEvent ปิด state หรือไม่ — ถ้าใช่ และ event fire ผิด slot หลัง S5-06 → regression candidate

---

## Investigation Findings (2026-05-11 — gameplay-programmer, partial)

**Root cause confidence: NEEDS-RUNTIME-TEST**
**S5-06 regression: UNDETERMINED — needs runtime test**

### Key finding

Anansi's W SkillKey ไม่ได้ฝังใน Unity asset (`AnansiW.asset`) — **resolve จาก PlayFab CBS dashboard ตอน runtime** ทำให้ static analysis ตอบไม่ได้ว่า Anansi W ใช้ `SkillKey.W` มาตรฐานหรือ key อื่น

### Working hypothesis

Anansi W น่าจะใช้ `SkillKey.W` (เพราะ bind อยู่บน W key) → case `W` ใน `ActorCombatAction.OnPerformExit` switch มีอยู่แล้ว → ปัญหา **ไม่ใช่** root cause เดียวกับ BUG-0001 (missing switch case)

ถ้าสมมติฐานถูก → ปัญหาต้องอยู่ที่:
1. `AnansiWAction` มี override ที่ skip base cleanup, หรือ
2. Anansi animator controller — W state ไม่มี exit transition ที่ viable กลับ locomotion, หรือ
3. Anansi W ใช้ AnimationEvent ปิด state และ S5-06 (42 shim methods rewired ผ่าน `GetActiveSlot()`) ทำให้ event fire ผิด slot/ผิด timing → **S5-06 regression suspect**

### Runtime test ที่ unblock ได้

ใส่ debug log ใน `ActorCombatAction.OnPerform` หรือ `StartSpellCast`:

```csharp
Debug.Log($"[BUG-0002] Anansi W start — SkillKey={AbilityData.SkillKey}, Hero={Actor.name}");
```

แล้วเล่น Anansi → กด W → ดู console:

- **ถ้า log = `SkillKey.W`** → INDEPENDENT จาก BUG-0001 → ต้อง investigate Anansi W override / animator exit / S5-06 shim ต่อ
- **ถ้า log ≠ `SkillKey.W`** (เช่น Recall / I / Item / Unknown) → SHARED root cause กับ BUG-0001 → fix BUG-0001 อาจครอบทั้งคู่

### Status

**Blocked pending runtime test.** Static investigation จาก gameplay-programmer agent (2 รอบ, total ~63 tool calls) ไม่สามารถ resolve ได้ static-only.

---

## Investigation Update (2026-05-12) — Re-opened after user clarified reproduction

User clarified ว่า bug **เป็นทุกครั้งที่ client peer** (deterministic, not intermittent) — เปลี่ยน picture จาก rare race เป็น **server/client asymmetry**

### Root cause (confirmed via static analysis)

**File**: `C:\GitHub\delta-unity\Assets\GameScripts\Gameplays\Vfxs\SkillObject.cs:956-985`

`SkillObject.Return()` method ที่ trigger `m_OnFinish?.Invoke(this)` (line 974) มี early return:

```csharp
public void Return(bool isForce = false)
{
    if (Runner.IsClient) return;  // ← line 958 — server-only guard
    ...
    m_OnFinish?.Invoke(this);  // ← line 974
}
```

**Asymmetry:**

| Step | Server | Client |
|------|--------|--------|
| 1. Anansi W start | `OnPerformEnter` → `IsLoop = true` (per-peer animator callback) | `OnPerformEnter` → `IsLoop = true` ✅ |
| 2. W animation จบ | `SkillObject.Return()` → `OnFinish` → `IsLoop = false` ✅ | `Return()` early-return at line 958 ❌ — **`IsLoop` ค้าง true** |

→ Client animator parameter `Loop` (`LoopKey` ที่ `ActorCombatAction.cs:521`) ค้าง `true` หลัง W → state machine stuck → Idle pose

### Why deterministic (vs BUG-0001 intermittent)

ไม่ใช่ race — เป็น **structural code path** ที่ guard ตรงๆ. ทุกครั้ง W จบบน client = ทุกครั้ง IsLoop ค้าง

### Other abilities likely affected (Sprint 006)

`IsLoop` pattern เดียวกันใช้ใน 4 abilities อื่น — น่าจะมีอาการเดียวกันบน client (ยังไม่ได้ verify):
- `GuanYuEAction.cs` (line 20: IsLoop=true, line 28: IsLoop=false in DashEnd callback — server-side)
- `HorusEAction.cs` (line 14, 24 — same pattern)
- `HorusRAction.cs` (line 31, 39)
- `VolundWAction.cs` (line 21, 116)

Recommend: เปิด Sprint 006 task เพื่อ apply pattern เดียวกันให้ 4 abilities — ระวัง side effect ของ `OnFinish` ของแต่ละตัว (เช่น `VolundEAction.OnFinish` เรียก `StartMainCooldown` ที่ network-mutating)

---

## Fix Applied (2026-05-12)

**File**: `C:\GitHub\delta-unity\Assets\GameScripts\Gameplays\Characters\Anansi\AnansiWAction.cs`

**Approach**: Override `FixedUpdateNetwork` ใน AnansiWAction ให้ทำ per-peer auto-cleanup เมื่อ ability ออกจาก active W state — ไม่พึ่ง `SkillObject.Return` (server-only)

**Diff:**

```csharp
// Added override after existing OnFinish method
public override void FixedUpdateNetwork()
{
    base.FixedUpdateNetwork();

    if (Actor == null || Actor.Animation == null || Actor.Animation.Running == null) return;
    if (!IsLoop) return;

    bool inActiveWState =
        Progress == SkillState.Perform ||
        Progress == SkillState.Perform2 ||
        Progress == SkillState.Perform3 ||
        Progress == SkillState.Perform4 ||
        Progress == SkillState.Perform5;

    if (!inActiveWState)
    {
        IsLoop = false;
    }
}
```

**ทำไม safe:**
- `FixedUpdateNetwork` รันทุก peer; code หลัง `base.FixedUpdateNetwork()` execute เสมอ (base early-return ไม่หยุด derived class)
- `IsLoop` getter/setter ใน `ActorCombatAction.cs:521-527` wraps local animator `SetBool` (LoopKey) — ไม่ใช่ networked mutation
- `Progress` เป็น Networked → proxy clients เห็น Progress=None หลัง W chain จบ → cleanup trigger ทุก peer
- Idempotent — เรียกซ้ำไม่มี side effect
- Guard `inActiveWState` ป้องกัน reset กลางคัน (เก็บ true ระหว่าง Perform → Perform2 chain เมื่อ hook hit hero)

## Closure Record

**Closed**: 2026-05-12
**Resolution**: Fixed — เพิ่ม `FixedUpdateNetwork` override ใน `AnansiWAction` ที่ auto-clear `IsLoop` เมื่อ Progress ออกจาก Perform[1-5] — ทำงานทุก peer, ไม่พึ่ง server-only `SkillObject.Return` callback
**Fix commit / PR**: TBD (ใน delta-unity repo, ยังไม่ commit ที่นี่)
**Verified by**: Tanapol Aekprapu (user playtest)
**Verification details**:
- ✅ 2-peer test — client peer ขยับหลัง W จบ → locomotion เล่นปกติ (เดิมค้าง Idle)
- ✅ Anansi W → hit enemy hero (Perform → Perform2 hook → Perform4 damage chain) — ครบทุก phase
- ✅ Anansi W → hit tower (Perform → Perform3 dash chain) — เล่นปกติ
- ✅ Host peer regression — เล่นได้ปกติ (idempotent)
**Regression test**: Manual playtest (ดูข้างบน) — automated test ต้อง PlayMode framework ที่โปรเจกต์ยังไม่มี (สอดคล้องกับ Sprint 005 finding #1)
**Status**: Closed
