# Bug Report

## Summary
**Title**: หลัง Recall (วาปกลับบ้าน) ตัวละครไม่เล่น animation ท่าเดิน
**ID**: BUG-0001
**Severity**: S3-Minor
**Priority**: P2-Next Sprint
**Status**: Open
**Reported**: 2026-05-11
**Reporter**: Tanapol Aekprapu

## Classification
- **Category**: Visual / Gameplay
- **System**: Animation State Machine / Recall (slot 7)
- **Frequency**: Always (สงสัย — รอ QA confirm)
- **Regression**: Unknown

## Environment
- **Build**: dev branch (post-Sprint 005 Phase 2 pilot)
- **Platform**: Unity Editor 2022.3.62f1 (รอ confirm)
- **Scene/Level**: Training / PvP map
- **Game State**: หลัง Recall complete → spawn ที่ base → ขยับตัวต่อ

## Reproduction Steps
**Preconditions**: เลือก hero ใดก็ได้, อยู่ใน match

1. กด Recall (slot 7) จนสำเร็จ — วาปกลับ base
2. หลัง warp กลับมา กดทิศทาง movement
3. ตัวละครเคลื่อนที่ แต่ animation ยังค้างที่ idle (ไม่เล่นท่าเดิน)

**Expected**: เมื่อขยับ ต้องเล่น locomotion (walk/run) animation ตามปกติ
**Actual**: ตัวเคลื่อนที่ แต่ animation ยังคงเป็น idle/post-recall pose

## Technical Context
- **Likely affected files** (in `delta-unity` repo): Recall ability action, Animator controller (locomotion blend tree), `ActorCombatAction` post-cast cleanup
- **Related systems**: Recall slot 7 binding (bound globally per ADR-0008 / S5-09), Animation state machine
- **Possible root cause**: Animator state ไม่ transition กลับ locomotion หลัง Recall finish — น่าจะ animation parameter (e.g. `IsCasting` / `Speed`) ไม่ reset หลัง action complete

## Evidence
- **Logs**: รอเก็บจาก reproduction
- **Visual**: รอ screenshot/clip

## Related Issues
- BUG-0002 (Anansi W idle stuck) — อาการคล้ายกัน อาจ root cause เดียวกัน (animator post-cast cleanup)
- [Sprint 005 S5-09](../../sprints/sprint-005.md) — Recall slot 7 binding work

## Notes
สงสัยเป็น animator parameter cleanup bug ร่วมกับ BUG-0002 — แนะนำ investigate ทั้งสองพร้อมกัน
