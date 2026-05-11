# Bug Report

## Summary
**Title**: Anansi Skill W — หลังใช้สกิลจบ ตัวค้างท่า Idle ไม่ transition กลับ locomotion
**ID**: BUG-0002
**Severity**: S3-Minor
**Priority**: P2-Next Sprint
**Status**: Open
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
