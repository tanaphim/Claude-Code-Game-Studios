# Bug Report

## Summary
**Title**: Anansi Skill W — หลังใช้สกิลจบ ตัวค้างท่า Idle ไม่ transition กลับ locomotion
**ID**: BUG-0002
**Severity**: S3-Minor
**Priority**: P2-Next Sprint
**Status**: Resolved (delta-unity@91697bf78e on `dev`, 2026-05-12)
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
