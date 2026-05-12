# Bug Report

## Summary
**Title**: หลัง Recall (วาปกลับบ้าน) ตัวละครไม่เล่น animation ท่าเดิน
**ID**: BUG-0001
**Severity**: S3-Minor
**Priority**: P3-Backlog (downgraded from P2 — see Investigation Update 2)
**Status**: Investigation Paused — Deferred to Sprint 006
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

---

## Investigation Update 2 (2026-05-12) — Proposed fix INVALIDATED, root cause unconfirmed

### Fix proposal #1 (StopRecalling) — INVALIDATED

User reported: `StopRecalling()` legacy — debug log ใน function ไม่เคย trigger ตอน Recall จบปกติ → fix ที่ paste ใน StopRecalling **dead code path** → ไม่มีผลใดๆ

Confirmed: ระบบใหม่ใช้ `RecallAction` (subclass ของ `ActorCombatAction`) เป็น authoritative path; `StopRecalling` ถูก trigger เฉพาะ damage/cancel paths บางตัว ไม่ใช่ recall-complete flow ปกติ

### Hypothesis revision: race condition

หลัง deeper trace พบว่า:
- `OnPerformRelease` (server) → `IsStop = true`
- `CheckViable` ครั้งถัดมา → `SetViable(Recall, false)`
- **Same tick** → `OnStateExit` (`ActorCombatAction.cs:2882`) → `IsStop = false` (reset for next ability)
- ครั้งถัดมา → `CheckViable` → `SetViable(Recall, true)` **กลับเป็น true ก่อน Unity Animator update sample parameter**
- Race: ถ้า Animator update มาช้า → state machine ไม่เห็น false transition → stuck

### Verification attempt (Method 2 + Method 3)

ทำ:
1. **Race amplifier** — เพิ่ม `Thread.Sleep(5)` ใน CheckViable หลัง `SetViable(Recall, false)` (`#if UNITY_EDITOR`) — ขยาย race window
2. **Passive detector** — เพิ่ม `DetectBugStuckRecall()` ใน `ActorAvatar.OnFixedUpdateNetworkState` ที่ log error เมื่อ `IsMoving=true && Recall_Viable=true && !IsRecalling > 0.3s`

### Result (2-peer playtest, ~5-10 นาที, 30-40 รอบ Recall)

- **Detector events: 115 logged** — แต่ animator stuck **ไม่เห็นด้วยตาเลย** ตลอด test
- **First detection at Tick 4319, first OnPerformRelease at Tick 6130** — detector fire **ก่อน Recall ใดๆ จบ 30+ วินาที** → false positive 100%
- **2 unique animator state hashes** เด่นชัด (`-6471121`: 59 ครั้ง, `-1373074065`: 56 ครั้ง) — ส่วนใหญ่ probably เป็น locomotion states ไม่ใช่ Recall state
- **Recall_Viable=true เป็น default state** เมื่อ skill ใช้ได้ → ไม่ใช่ proxy ที่ดีของ stuck animator

### Conclusions

1. **Race condition hypothesis ไม่ verify ผ่าน amplifier** — 5ms delay + 30-40 recalls ไม่ produce bug
2. **Detector proxy ผิด** — false-positive 100% (เพราะ Recall_Viable=true เป็น default behavior ไม่ใช่ stuck indicator)
3. **Bug "บางครั้ง" จริงๆ** — ไม่ reproduce ผ่าน stress test ปกติ + race forcing

### Possible alternative root causes (untested)

- **Specific input timing** — pattern กดเฉพาะ ที่ amplifier ไม่ stress
- **Photon Fusion prediction rollback** — ไม่ใช่ทุก session ที่ replay sequence เดียวกัน
- **Animator controller transition condition** — เงื่อนไข exit transition จาก Recall state อาจมี hidden gate
- **State authority race** — ใน Fusion shared mode ระดับลึกกว่า animator parameter timing

### Defer to Sprint 006 — recommended action

**Task**: Animator state machine architecture review สำหรับ Recall state (และ skill states ทั่วระบบ)

**Scope:**
1. เปิด `RadiusBasicLocomotion.controller` ใน Unity Animator window
2. Review exit transitions ของ Recall state — เงื่อนไขครบไหม
3. ตรวจสอบ `Recall_Viable` dual-role conflict (entry condition vs exit signal)
4. พิจารณา redesign: ใช้ Trigger parameter (one-shot) สำหรับ exit แทน bool toggle
5. Designer ที่เซ็ต animator ควรเป็น primary investigator

**Estimate (Sprint 006)**: 1-2d animator review + 0.5d implement + 0.5d test = ~2-3d

**Detection re-enable**: ถ้า bug เกิดบ่อยขึ้นใน playtest อนาคต → re-add detector แต่ใช้ animator state hash proxy ที่ accurate (ต้อง identify Recall state hash ก่อน)

### Files reverted (no production changes)

- `C:\GitHub\delta-unity\Assets\GameScripts\Gameplays\Characters\ActorCombatAction.cs` (CheckViable Thread.Sleep removed)
- `C:\GitHub\delta-unity\Assets\GameScripts\Gameplays\Characters\Common\RecallAction.cs` (Debug.Log removed)
- `C:\GitHub\delta-unity\Assets\GameScripts\Gameplays\Characters\Actors\ActorAvatar.cs` (detector + counter removed)

ไม่มี code change commit ใน delta-unity จาก investigation นี้.
