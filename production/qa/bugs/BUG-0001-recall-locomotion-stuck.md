# Bug Report

## Summary
**Title**: หลัง Recall (วาปกลับบ้าน) ตัวละครไม่เล่น animation ท่าเดิน
**ID**: BUG-0001
**Severity**: S3-Minor
**Priority**: P2-Next Sprint
**Status**: **Resolved** (Sprint 005 S5-19, 2026-05-14)
**Reported**: 2026-05-11
**Reporter**: Tanapol Aekprapu
**Investigation update**: 2026-05-14 (Sprint 005 S5-19)
**Resolution**: delta-unity PR #357 merged 2026-05-14 — playtest F3/F4 + Anansi W regression PASS

## Classification
- **Category**: Visual / Gameplay
- **System**: Animation State Machine / Recall (slot 7)
- **Frequency**: **Client-only + intermittent** (confirmed by reporter 2026-05-14 — NOT every Recall)
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

## Investigation 2026-05-14 (Sprint 005 S5-19)

### Code-side forensic findings

**Suspects ตัดออกแล้ว** (replicated state — client peer ได้รับค่าปกติ):
- `ActorAvatar.m_IsRecalling` เป็น `[Networked] NetworkBool` ([ActorAvatar.cs:80](../../../delta-unity/Assets/GameScripts/Gameplays/Characters/Actors/ActorAvatar.cs))
  → `SetRecalling()` รันบนทุก peer ผ่าน `ChangeProperty` handler (line 333)
- `ActorCombatAction.m_IsCasting` เป็น `[Networked] NetworkBool` ([ActorCombatAction.cs:208](../../../delta-unity/Assets/GameScripts/Gameplays/Characters/ActorCombatAction.cs))
  → IsCasting flag ตัวเองไม่ค้าง

**Suspects ที่ยังเหลือ** (server-only side effects บน client peer):
1. **`Damageable.OnRecall` event ยิงเฉพาะ host** ([ActorAvatar.cs:771-775](../../../delta-unity/Assets/GameScripts/Gameplays/Characters/Actors/ActorAvatar.cs))
   ```csharp
   if (IsServer == true && IsCancelRecall == false) {
       GetTeleportPosition();
       Damageable.OnRecall?.Invoke();   // server-only
   }
   ```
   → handler ใดๆ ที่ subscribe `OnRecall` แล้วทำ animator cleanup จะไม่รันบน client peer
2. **Animator SetBool/SetTrigger calls ที่อยู่ใน `if (Runner.IsServer)` block** — ยังไม่เจอ ต้อง grep ลงลึก
3. **Race condition**: `m_IsRecalling` flip → `SetRecalling()` → `StopRecalling()` toggle visual prefabs (line 782-784) **ก่อน** animator state machine ได้ tick update → state machine อาจ resolve ผิดทาง

### Symptom shape comparison vs BUG-0002

| Property | BUG-0002 (Anansi W) | BUG-0001 (Recall) |
|---|---|---|
| Client-only | ✅ | ✅ |
| Frequency | **เกิดทุกครั้ง 100%** | **เกิดบางครั้ง (intermittent)** |
| Root cause | Deterministic skip (`SkillObject.Return` early-return on client) | **ไม่ใช่ deterministic skip** — น่าจะ timing/race หรือ state-dependent |
| Fix pattern | Override `FixedUpdateNetwork` to auto-clear animator bool | **ไม่ apply ได้ตรง** — ต้องหา trigger condition ก่อน |

**สรุป**: ❌ Apply BUG-0002 pattern ไม่ได้ตรงๆ. Intermittent + client-only บ่งบอกว่า cleanup path มี race หรือ state-dependent gap

### Reproduction protocol (สำหรับ next investigator)

**ต้อง capture จาก Unity Editor ตอน bug เกิด**:

1. เปิด multipeer harness (host + client)
2. บน client window — เปิด **Animator window** (Window → Animation → Animator) ของตัวละคร hero
3. Cast Recall → รอ warp กลับ base → กดทิศทาง movement
4. **เมื่อ bug เกิด** (animation ค้าง idle ทั้งที่ตัวเดิน) — capture:
   - **Screenshot Animator window** เห็น state ปัจจุบัน + parameters panel (เห็นค่า bool/float ทุกตัว)
   - **Screenshot Animator graph** เห็น state ที่ active highlight อยู่
   - **Editor.log** ของ client peer ในช่วง 5 วินาทีก่อน-หลัง bug
5. ลอง trigger bug ในเงื่อนไขต่างๆ เพื่อหา pattern:
   - Recall ตอนยืนเฉย vs ตอนกำลังเดิน vs ตอนกำลัง attack
   - Cast Recall → cancel → cast ใหม่ → complete
   - Recall ตอน HP ต่ำ vs full HP
   - ทำซ้ำ 10 ครั้ง — log ครั้งที่ bug เกิด vs ไม่เกิด

**ข้อมูลที่ต้องการเพื่อปิดเคส**:
- Animator parameter ตัวไหนค้างที่ค่าผิด (เทียบกับ working case)
- Animator state ตัวไหนค้าง (เทียบกับ Locomotion state ที่ควรเป็น)
- Pattern ของ state ตอนทริกเกอร์ (ขยับ/ไม่ขยับ ก่อน Recall)

### Cousin bugs (Sprint 005 S5-20 commit notes 91697bf78e)

Same client-peer cleanup gap pattern อาจมีใน:
- GuanYuE, HorusE, HorusR, VolundW (per BUG-0002 fix commit message)

ถ้าหา root cause BUG-0001 ได้แล้ว → audit cousin abilities ใน batch เดียวกัน

### Recommended path

- **ทางหลัก**: รอ reporter (Tanapol) repro 1 ครั้งใน Editor + capture screenshots → ผู้ดูแลทำ targeted fix (~0.3d เพิ่ม)
- **ทางสำรอง**: Defer ไป Sprint 006 รวมกับ cleanup-pattern audit (GuanYuE/HorusE/HorusR/VolundW) เป็น batch task

---

## Resolution 2026-05-14 (Sprint 005 S5-19)

**Fix shipped**: [delta-unity PR #357](https://github.com/radiuszon/delta-unity/pull/357) — merged 2026-05-14

### Actual root cause (confirmed by playtest)

`AnimatorStateSync.SynchronizeStates()` intentionally skipped writing `_state.States` when host
was in Idle/Run (lines 138-140 pre-fix) — locomotion is driven locally per peer to avoid
continuous NormalizedTime sync. But the **previously written ability StateHash was never
cleared**, leaving stale data in networked state.

On client peers (`Runner.IsClient && !HasInputAuthority`), `UpdateStates()` reads the stale
hash every Render tick. If the client's local animator already transitioned to Run (input-driven),
`localHash != stateHash` → `stateChanged=true` → `Animator.Play(stale ability hash, ...)` force-replays
the finished ability state, masking the locomotion blend tree.

**Intermittency explained**: race between client's local input-driven transition and the next
Render() tick. If local animator hasn't transitioned yet → skip force replay → escapes.
If it has → re-pinned to stale state → stuck.

### Fix

Single source-of-truth fix in `AnimatorStateSync.SynchronizeStates()` (delta-unity@32e154d43a):
when host enters Idle/Run, clear stored `StateHash` to 0. Client's `UpdateStates()` already skips
`stateHash == 0` (line 170), releasing the client animator to its own input-driven locomotion path.
Guarded by `existing.StateHash != 0` to avoid spamming networked state every tick.

**Bandwidth cost**: +12 bytes per ability-end transition (Photon Fusion delta compression on
single `StateData` slot + SyncTick). Negligible vs 65 B/s budget.

### Playtest verification (2026-05-14)

- ✅ **F3** Recall + WASD spam during channel → locomotion plays normally post-warp
- ✅ **F4** Hold direction 0.5s before warp completes → no stick
- ✅ **Anansi W regression** (BUG-0002 fix) → still cleans up correctly on client
- (Smoke tests Hercules QWER, other heroes — not formally captured but no new bugs reported)

### Cousin bugs — likely resolved by the same fix

Per BUG-0002 fix commit message (`91697bf78e`), same client-peer cleanup gap pattern was
suspected in: **GuanYuE, HorusE, HorusR, VolundW**.

Because the AnimatorStateSync fix is at the root (every ability that ends in Idle/Run),
all of these are **expected to be resolved** by the same PR. **Not formally verified** —
needs ad-hoc playtest as those abilities are encountered in normal play. File a new bug
if any of them still stick post-S5-19.

### Tech debt note

`HasAnyLayerStateHashChanged()` still returns true while host is in Run (current Run hash
≠ stored 0), which keeps `_pendingEventSyncTicks` retry loop futilely firing every auto-sync
interval (2s) with no actual network writes. CPU cost is small but non-zero. Considered out
of scope for BUG-0001 — file a separate optimization task if profiling shows it matters.
