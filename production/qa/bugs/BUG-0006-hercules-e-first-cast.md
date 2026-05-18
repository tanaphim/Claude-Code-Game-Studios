# BUG-0006 — Hercules E (dash) first cast per match: no movement, no cooldown

**Filed**: 2026-05-18 (during Phase 2 soak manual verification, Sprint 006 Day 4)
**Status**: 🟠 **OPEN — ROOT CAUSE LOCATED 2026-05-18** (smoking gun at `NetworkStatusEffect.cs:947`, fix scheduled for next session)
**Severity**: S2 (high — feature broken on first use; workaround = cast twice)
**Priority**: **P0** (upgraded 2026-05-18 — cousin risk HIGH; blocks Phase 3 batch 1 dash-bearing heroes Horus E + Volund W if H4b confirmed)
**Owner**: gameplay-programmer (tanapol)

---

## Symptom

ใน `scene_game_map.unity` Training match กับ Hercules:

- **First cast E (dash) per match**: animation อาจ trigger แต่ avatar ไม่เคลื่อน, cooldown ไม่เริ่มนับ — ability "กิน" input แต่ไม่ execute path เต็ม
- **Second cast onwards**: dash ทำงานปกติ, cooldown trigger ปกติ
- เกิด **ทุก match** หลัง `scene_game_map.unity` load (ไม่จำกัดแค่ Editor session แรก, ไม่ใช่ first-time-only)

## Scope (ruled out from operator data 2026-05-18)

| Hypothesis | Status | Evidence |
|------------|--------|----------|
| BoundSlot timing / BindSlot pipeline regression | ❌ Ruled out | Q/W/R first cast ทุก match ทำงานปกติ — pipeline ทำงาน |
| Silent pipeline failure (BindSlot not registered, slot=0) | ❌ Ruled out | Console **ไม่มี warning** ตอน first cast E |
| Hercules-E-specific OR shared dispatcher with E entrypoint | ✅ Likely scope | Only E affected; Q/W/R/NA/Recall/Item ทำงาน |

## Root cause — IDENTIFIED 2026-05-18

🎯 **Smoking gun**: [`NetworkStatusEffect.cs:947`](../../../../../delta-unity/Assets/GameScripts/Gameplays/Cores/Stat/NetworkStatusEffect.cs:947)

```csharp
public void RequestDash(float range, float speed, Action callback = null, bool blockObstacle = false)
{
    if (!Object.HasStateAuthority || IsDashing) return;  // ← silent no-op, no warning
    ...
    DashStart = transform.position;
    DashEnd = target;
    DashT = 0f;
    IsDashing = true;
    ...
}
```

`IsDashing` is `[Networked] NetworkBool` (line 943). The early-return guard fails the dash silently — exactly matching the symptom (animation plays, no avatar movement, no Console warning).

### Sub-hypothesis (which guard branch fails on first cast?)

**H4a — `Object.HasStateAuthority == false` on first cast**
First AnimationEvent fires before NetworkBehaviour authority transfer is stable. Likely if `NetworkStatusEffect` is on a separate NetworkBehaviour that lags the main avatar's authority assignment by 1-2 ticks.

- **Test path**: add `Debug.Log($"[RequestDash] HasAuthority={Object.HasStateAuthority} IsDashing={IsDashing}")` at line 947 → observe first-vs-second cast values
- **Likelihood**: medium — authority should be stable by the time animation event fires, but multipeer + Phase 2 BindSlot pipeline adds ticks before first input is accepted

**H4b — `IsDashing == true` leaked across match (MOST LIKELY)**
If a previous match ended with `IsDashing = true` (player died mid-dash, despawn before `EndDash()` reset, etc.), and `IsDashing` is not reset on `Spawned()` of the next match's NetworkStatusEffect → new match's first cast guards out. Second cast: state has replicated back to false somehow OR another code path reset it.

- **Test path**: add `Debug.Log($"[NetworkStatusEffect.Spawned] IsDashing initial={IsDashing}")` in `Spawned()` callback → check if IsDashing is true at spawn time
- **Likelihood**: HIGH — matches "every match" pattern (user confirmed Q1); `[Networked]` state can persist on pooled/reused NetworkBehaviours if the framework doesn't reset on respawn
- **Fix**: explicit `IsDashing = false; DashT = 0f;` in `Spawned()` callback

### Why no warning?
The early-return at line 947 is silent — `return;` with no logging. **First diagnostic fix candidate**: change to `if (!Object.HasStateAuthority || IsDashing) { Debug.LogWarning($"[RequestDash] guard failed HasAuthority={Object.HasStateAuthority} IsDashing={IsDashing}"); return; }` to surface future occurrences.

### Cousin scan — `RequestDash` callers in codebase
(grep `RequestDash` in `Assets/GameScripts/Gameplays/Characters/**/*.cs`):

- `HerculesEAction.cs:18` — this bug
- `HerculesRAction.cs:88` — likely same guard failure
- `HorusEAction.cs:16` — **Phase 3 batch 1 (S6-03)**
- `WildBillEAction.cs:14` — batch 2

Indirect (via `Dash()` overloads at NetworkStatusEffect.cs:535/544/758): Athena E, Anansi E + W, Volund W, Horus Q + R — need to verify whether they go through the same `IsDashing` guard.

## Reproduction

1. Open `scene_initial.unity` → Play
2. Login → Training mode → เลือก Hercules
3. Spawn ใน `scene_game_map.unity`
4. Wait ~2s for full spawn + bind (until UI fully loads)
5. Press **E once** → observe:
   - Animation อาจเล่น
   - Avatar ไม่ dash (ยืนกับที่)
   - Cooldown UI ไม่เริ่มนับ
6. Press **E second time** → observe: dash + cooldown ทำงานปกติ
7. Repeat: Stop Play → Play again → first E cast in new match ก็ขัดข้องอีก

## Files involved (confirmed 2026-05-18)

- ✅ **`Assets/GameScripts/Gameplays/Cores/Stat/NetworkStatusEffect.cs:945-961`** — `RequestDash` silent guard (PRIMARY); also lines 940-943 (`[Networked]` dash state), line 1085-1090 (`EndDash` reset)
- 🟡 NetworkStatusEffect's `Spawned()` callback — need to check whether `IsDashing` is reset on spawn (H4b fix point)
- 🟡 Avatar respawn / pool teardown — if Hercules avatar uses pooling, check whether NetworkStatusEffect state survives respawn
- ⚪ `Assets/GameScripts/Gameplays/Characters/Hercules/HerculesEAction.cs:18` — call site only; **action file itself is correct** (clean API call). Bug is in shared NetworkStatusEffect API → why cousin risk = HIGH

## Test plan (for fix)

- **EditMode test**: instantiate Hercules + cast E + assert velocity applied + CD set on first cast (no second-cast warmup)
- **Manual**: 3 matches in a row, first E cast in each → all should dash + CD start
- **Multipeer**: verify fix works on both Host and Client peer (in case AnimationEvent dispatch differs by authority)

## Phase 3 batch 1 impact

**🟠 UPGRADED 2026-05-18 EOD: MEDIUM-HIGH** (was LOW)

Smoking gun at [NetworkStatusEffect.cs:947](../../../../../delta-unity/Assets/GameScripts/Gameplays/Cores/Stat/NetworkStatusEffect.cs:947) is a **shared API** (`RequestDash` on the shared `NetworkStatusEffect` component on `Actor.Trait`). All heroes that call `RequestDash` inherit the same first-cast silent guard:

| Hero / Ability | Sprint | Risk |
|----------------|--------|------|
| Hercules E (dash) | S5 (Phase 2 pilot) | 🔴 Confirmed (this bug) |
| Hercules R | S5 (Phase 2 pilot) | 🟡 Suspect — uses `RequestDash` with `blockObstacle=true`; not yet manually verified |
| **Horus E** | **S6-03 (Phase 3 batch 1)** | 🔴 **HIGH** — direct `RequestDash` caller |
| Horus Q + R | S6-03 (Phase 3 batch 1) | 🟡 Suspect — indirect dash via `Dash()` overloads |
| **Volund W** | **S6-04 (Phase 3 batch 1)** | 🔴 **HIGH** — largest action file 118 lines; dash-bearing |
| Guan Yu | S6-05 (Phase 3 batch 1) | 🟡 Check — Guan Yu W has sub-system (`GuanYuWOnHit.cs`, `RotateGO.cs`); needs grep for RequestDash |
| Skadi | S6-06 (Phase 3 batch 1) | 🟢 Likely safe — clean control case, no BUG-0001 history; needs grep |
| Wild Bill E | S7+ (batch 2) | 🟡 Suspect — direct `RequestDash` caller |
| Athena E, Anansi E + W | S7+ (batch 2) | 🟡 Suspect — indirect dash via `Dash()` overloads |

**Action items before Phase 3 batch 1 implementation**:
1. ✅ Identify root cause = `NetworkStatusEffect.cs:947` silent guard (DONE 2026-05-18)
2. ⏳ Fix the silent guard (add diagnostic log + reset `IsDashing` on Spawned() if leak confirmed) — **next session**, est ~0.5d
3. ⏳ Re-verify Hercules E first cast post-fix
4. ⏳ Cousin verification: Hercules R + manual first-cast probe before Horus E migration begins
5. ⏳ Update Phase 3 batch 1 stories AC #7 to explicitly include "first cast E per match works without prior warmup" verification step

## Soak verdict reading (Phase 2 → Phase 3 handover, ADR-0006 §10)

### Initial reading (2026-05-18 Day 4, before code investigation)
PASS WITH NOTES — assumed Hercules-E-specific, Phase 3 batch 1 unblocked.

### Updated reading (2026-05-18 Day 4 EOD, after smoking gun located)
**🟠 PASS WITH NOTES + BLOCKER for Phase 3 batch 1 dash heroes**:
- Phase 2 pipeline (BindSlot/StateReleaseSlot/multipeer) is **GREEN** per Q/W/R control proof — unchanged
- BUG-0006 root cause is **shared API** (`NetworkStatusEffect.RequestDash`), not Hercules-local logic → Phase 3 batch 1 heroes inheriting this API (Horus E + Volund W direct, Guan Yu indirect) will have **the same bug** unless fixed first
- Workaround exists (cast twice) — playable but unshippable for new player UX
- Soak Day 4 of 7 — full sign-off on 2026-05-21 with explicit BUG-0006 status (RESOLVED before Phase 3 ideal; OPEN with mitigation acceptable)

### Phase 3 batch 1 kickoff decision (revised)
**Option A — Block until BUG-0006 fixed**: ideal; ~0.5d fix + verify Hercules first; Phase 3 starts 2026-05-22 (1 day after soak verdict)
**Option B — Proceed with batch 1, fix BUG-0006 parallel**: Horus E and Volund W will exhibit first-cast bug; per-hero AC #7 playtest will surface them but they would be marked Complete with known regression — risky
**Option C — Reorder Phase 3 batch 1**: start with **S6-06 Skadi (control case, likely no `RequestDash` use)** to validate non-dash pipeline; defer S6-03 Horus / S6-04 Volund until BUG-0006 fixed

**Recommendation**: **Option A** — fix BUG-0006 first (~0.5d, fits soak window). Cleaner end-to-end test for Phase 3 batch 1 + avoids regression contamination across multiple heroes simultaneously. Final decision by tanapol.

## References

- [Phase 2 Lessons Learned](../../../docs/architecture/phase-2-lessons-learned.md)
- [ADR-0006 §10 Phase 2 → Phase 3 handover](../../../docs/architecture/ADR-0006-phase-2-migration-plan.md)
- [Sprint 006 plan](../../sprints/sprint-006.md)
- [Sprint 005 S5-21 — SetActiveSlot wiring + AnimationEvent 40-shim migration](../../sprints/sprint-005.md) — possible interaction site
- [Sprint 005 Hercules playthrough evidence](../evidence/sprint-005-hercules-playthrough.md) — Phase 2 exit sign-off (did not catch this regression; manual playthrough at S5-10 may not have spawned + cast E across multiple matches)
