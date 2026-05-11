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

---

## Investigation Findings (2026-05-11 — REVISED with user reproduction context)

**Root cause confidence: HIGH (network-aware revision)**
**Regression vs Sprint 005 (S5-06/07/09): NO** — pre-existing networking timing issue

### Critical reproduction context (from user)

> "ปกติจะเจอบนฝั่ง client ถ้ารันจอเดียวแบบ host จะไม่เจอ"

ปัญหาเกิดเฉพาะ **remote client peer** — host (single-screen) ไม่เจอ → ปัญหาเป็น **network replication timing**, ไม่ใช่ pure animator logic

### Defect (revised root cause)

`IsStop` คือ `[Networked] NetworkBool` (`ActorCombatAction.cs:451-461`) — replicate จาก server → client มี frame delay

**Animator exit flow ของ Recall:**
1. กด Recall → animator transition เข้า Recall state (`Recall_Viable=true`)
2. Animation จบ → `RecallAction.OnPerformRelease` ตั้ง `IsStop = true` (server-authoritative)
3. `CheckViable()` (`ActorCombatAction.cs:2206`) ถูกเรียกใน `OnFixedUpdateState` → เมื่อ `IsStop=true` → `SetViable(Recall, false)` → animator exit Recall state

**ปัญหาบน remote client:**
- `IsStop` replicate มา client มี delay
- Recall ability instance อาจถูก despawn / id-guard cull (`ActorCombatAction.cs:1010, 1038` — `Actor.Combat.SkillRecall.Id != Id` early return) **ก่อน** client เห็น `IsStop=true`
- ผลคือ `CheckViable` บน client ไม่เคย call `SetViable(Recall, false)` → `Recall_Viable` ค้าง `true` บน animator → state stuck post-warp

**บน host (state+input authority รวมในเครื่องเดียว):** ไม่มี replication delay → ทำงานปกติ → no bug

### Evidence

- `C:\GitHub\delta-unity\Assets\GameScripts\Gameplays\Characters\ActorCombatAction.cs:451-461` — `IsStop` เป็น `[Networked] NetworkBool`
- `C:\GitHub\delta-unity\Assets\GameScripts\Gameplays\Characters\ActorCombatAction.cs:2206-2284` — `CheckViable` ใช้ `IsStop` decide `SetViable(false)` (line 2277-2280)
- `C:\GitHub\delta-unity\Assets\GameScripts\Gameplays\Characters\ActorCombatAction.cs:1010,1038` — Recall ability id-guard early return ใน `OnFixedUpdateState`
- `C:\GitHub\delta-unity\Assets\GameScripts\Gameplays\Characters\ActorAnimation.cs:29` — `Recall_Viable` animator hash
- `C:\GitHub\delta-unity\Assets\GameScripts\Gameplays\Characters\Actors\ActorAvatar.cs:333-334` — `m_IsRecalling` networked prop change → trigger `SetRecalling()` callback **ทุก peer**
- `C:\GitHub\delta-unity\Assets\GameScripts\Gameplays\Characters\Actors\ActorAvatar.cs:769-787` — `StopRecalling` ไม่ touch animator (BUG site)

### Why previous fix proposal was wrong

อาทย์ตอน first-pass สับสน — agent เสนอเพิ่ม `case SkillKey.Recall` ใน `OnPerformExit` switch (line 2928) เพื่อ reset `SpeedAnimMultiplyRecall` — แต่:
- **`SpeedAnimMultiplyRecall` ไม่มีอยู่ใน animator** (ดู `ActorAnimation.GetEventTimeToCastTime` line 564-600 ไม่มี case Recall — Recall ไม่ใช้ event-time-to-cast path)
- การ `SetFloat` parameter ที่ไม่มี → Unity ignore silently → no-op
- Fix เดิมจะไม่ช่วยอะไรเลย

---

## Proposed Fix

**ไฟล์เดียว, ~3 บรรทัด** — ใน `ActorAvatar.StopRecalling()` ที่ `C:\GitHub\delta-unity\Assets\GameScripts\Gameplays\Characters\Actors\ActorAvatar.cs:769-787`

### Diff

```diff
 private void StopRecalling()
 {
     if (IsServer == true && IsCancelRecall == false)
     {
         GetTeleportPosition();
         Damageable.OnRecall?.Invoke();
     }

     if (HasInput == true && !IsCastingItem)
     {
         NetworkGameModeBase.WithUI<UIRecallView>(ui => { ui.Cooldown.Stop(); });
     }

     m_RecallObj.SetActive(false);
     m_RecallStartObj.SetActive(false);
     m_RecallEndObj.SetActive(true);

+    // BUG-0001 fix: explicit client-side animator exit. StopRecalling runs on every peer
+    // via m_IsRecalling networked property change (ActorAvatar.cs:333-334). Avoids relying
+    // on IsStop networked replication timing — on remote clients the Recall ability
+    // instance can despawn before CheckViable sees IsStop=true, leaving Recall_Viable
+    // stuck at true.
+    Animation?.Running?.SetViable(SkillKey.Recall, false);
+
     IsCancelRecall = false;
 }
```

### ทำไม fix นี้

- `SetRecalling()` callback (`ActorAvatar.cs:333-334`) trigger บน **ทุก peer** เมื่อ `m_IsRecalling` replicate (ไม่พึ่ง `IsStop` replication timing)
- `SetViable(SkillKey.Recall, false)` คือ canonical exit path (ใช้ใน `CheckViable` `ActorCombatAction.cs:2279` อยู่แล้ว)
- เป็น local animator `SetBool` — idempotent + safe ถ้าเรียกซ้ำ
- ไม่แตะ animator controller, `OnPerformExit`, หรือ `RecallAction`

### Risks (LOW)

- ถ้ามี edge case ที่ `m_IsRecalling=false` แล้วยังต้องการ `Recall_Viable=true` (เช่น Recall cancel แต่ animator ยังต้องเล่น cancel anim) → จะ break — **ไม่เจอ case นี้จาก static analysis** แต่ designer ที่เซ็ต animator ควร confirm
- เรียกซ้ำกับ host ตอน Recall จบปกติ (CheckViable+StopRecalling ต่างก็ set false) — idempotent, no harm

### Test Plan (manual, 2-peer)

1. Host peer + remote client peer connect, ทั้งคู่เลือก hero
2. **Client peer** กด Recall → wait warp complete → กดทิศทาง movement
3. ✅ **Expected:** ฝั่ง client เห็น locomotion animation เล่นทันที (เดิมค้าง Idle)
4. ✅ ฝั่ง host เห็น remote client เคลื่อนที่พร้อม locomotion animation
5. **Regression check:** cancel-recall (รับ damage ระหว่าง warp) → ฝั่ง client ต้องกลับ locomotion ตามปกติ
6. **Cross-check:** ทดสอบ host peer recall เพื่อ confirm ไม่ regression (idempotent SetViable)

**Estimate:** 0.5-1 ชม. (code change 5 นาที + 2-peer playtest 30 นาที + cancel-recall regression 15 นาที)

### Shared with BUG-0002?

**INDEPENDENT** — Anansi W ไม่ผ่าน `m_IsRecalling` lifecycle และ SkillKey ต่างกัน. BUG-0002 ต้อง runtime test แยก (ดู BUG-0002 file).
