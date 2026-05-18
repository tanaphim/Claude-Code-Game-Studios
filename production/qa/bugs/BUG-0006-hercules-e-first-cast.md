# BUG-0006 — Hercules E (dash) first cast per match: no movement, no cooldown

**Filed**: 2026-05-18 (during Phase 2 soak manual verification, Sprint 006 Day 4)
**Status**: 🟠 **OPEN — ROOT CAUSE REVISED 2026-05-18 late-EOD** (Phase 2 dual-path duplicate spawn; fix decision pending — see 3 options below)
**Severity**: **S1** (upgraded from S2 — affects EVERY Hero with AbilityComponent attached since S5-10 commit `748ddd410f` 2026-05-14, not just Hercules E; workaround = cast twice, unshippable)
**Priority**: **P0** (cousin risk = ENTIRE Phase 3 batch 1 + all post-S5-10 heroes; blocks Phase 3 batch 1 implementation)
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

## Root cause — REVISED 2026-05-18 late-EOD (after live diagnostic + user observation)

🎯 **Actual root cause**: **Phase 2 dual-path retention creates duplicate `ActorCombatAction` spawn per Hero ability**

### Live diagnostic timeline (2026-05-18)

1. **Initial hypothesis**: `NetworkStatusEffect.cs:947` silent guard (`!Object.HasStateAuthority || IsDashing`) — added diagnostic logs to RequestDash + all 5 `Dash()` overload guard sites + HerculesEAction.OnPerformRelease
2. **First cast Play test (user)**: NO logs fired on first cast. NO `[S5-06] StateReleaseSlot dropped` warning either.
3. **Operator observation surfaced the actual root cause**: user noticed **2 instances of the same ActorCombatAction prefab spawned simultaneously** — one inside `base_avatar` (correct child), one outside (orphan).
4. **Code inspection confirmed**: `ActorCombat.OnStartup` (line 405-415) runs BOTH spawn paths unconditionally for Hero with AbilityComponent attached:
   - `CreateSkill()` (legacy) — spawns via `Runner.SpawnAsync` (line 361), assigns to legacy `m_Skill3Action` NetworkProperty (orphan)
   - `BootstrapSlotBindings()` (Phase 2 S5-09) — calls `AbilityComponent.BindSlot()` → `registry.CreateAction(abilityId, anchor: this)` (line 151 of AbilityComponent.cs) — spawns parented to base_avatar

### Why first cast fails

When user presses E:
- Input routes through one instance (likely legacy `Skill3` orphan) → animation plays on base_avatar's Animator
- AnimationEvent fires on base_avatar's Animator → calls `Skill_E_Perform()` → `StateReleaseSlot(Actor.Combat.GetActiveSlot(), ...)`
- `GetActiveSlot() = 3` (S5-21 SetActiveSlot did wire correctly)
- `GetSlotAction(3)` returns Phase 2 instance (under base_avatar) — but animation was actually played on legacy instance
- Phase 2 instance has no "currently casting" state → silent drop somewhere in the routing
- Result: animation plays visually, no `OnPerformRelease` reaches HerculesEAction, no dash, no CD trigger

Second cast works because by then one of the pipelines' state has settled / re-synced — operator did not investigate which one.

### Why prior diagnostics didn't catch this

- S5-10 Hercules manual playthrough sign-off (2026-05-14) tested each ability once per match — operator didn't notice "first cast fails, second cast works" pattern because the workaround (cast twice) is invisible without explicit tracking
- Multipeer harness `PrototypeTest.unity` uses a different setup (TestActor prefab without legacy CreateSkill path) — Pass #4 was always green
- BUG-0001 cousin verification looked at AnimatorStateSync, not spawn counts

### Original NetworkStatusEffect.cs:947 hypothesis — RULED OUT

The silent guard at `RequestDash` line 947 is **not** the bug. Live diagnostic confirmed RequestDash is **never called** on first cast (no `[BUG-0006 RequestDash CALLED]` log). The chain breaks earlier — between `Skill_E_Perform()` AnimationEvent and `HerculesEAction.OnPerformRelease()` — likely at `GetSlotAction(3)` returning the wrong instance, or at `Release()` finding no active state on the bound instance.

### Defensive fix retained (not root cause but good hygiene)

A defensive change is retained in the BUG-0006 commit (delta-unity, pending commit):
- `NetworkTrait.Despawned()` now resets `IsDashing = false; DashT = 0f;` via new private helper `NetworkStatusEffect.ResetRequestDashState()` — mirroring the existing `m_IsDash = false` pattern
- Rationale: if `IsDashing` networked state ever leaks across match (different bug class, not BUG-0006), this prevents the silent-guard at line 947 from blocking RequestDash. Safe + matches existing pattern. Cheap insurance.

All diagnostic logs (RequestDash entry log, 5 Dash overload guard logs, HerculesEAction entry log) **reverted** to keep delta-unity diff minimal.

### Smoking gun (original — RULED OUT, kept for reference)
🎯 ~~**Initial smoking gun**~~: [`NetworkStatusEffect.cs:947`](../../../../../delta-unity/Assets/GameScripts/Gameplays/Cores/Stat/NetworkStatusEffect.cs:947)

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

### Cousin scan — both dash APIs share the same vulnerability pattern (2026-05-18 EOD-late)

**KEY FINDING**: `NetworkStatusEffect.cs` has **TWO separate dash state systems**, both with the same silent-guard vulnerability shape:

| API | State var | Declared at | Used by |
|-----|-----------|-------------|---------|
| `RequestDash(float range, ...)` | `IsDashing` (NetworkBool) | line 943 | Hercules E, Hercules R, Horus E, Wild Bill E |
| `Dash(Actor / Vector3, speed, height, callback)` (and 3 overloads) | `IsDash` (NetworkBool) | line 111 | Horus Q, Horus R, Volund W, Guan Yu E, (others indirect) |

Both guard patterns:
```csharp
// Line 538, 547, 575, 598, 760 (Dash family)
if (IsDash) return;  // silent no-op

// Line 947 (RequestDash)
if (!Object.HasStateAuthority || IsDashing) return;  // silent no-op
```

**Implication**: if BUG-0006 root cause is H4b (state leak across match), **both** `IsDash` and `IsDashing` are vulnerable — they're independent `[Networked]` state vars but share the same lifecycle weakness. Fix must reset **both** in `Spawned()` callback.

### Phase 3 batch 1 hero scan (2026-05-18 EOD-late grep)

| Hero / Ability | API | State var | BUG-0006 risk |
|----------------|-----|-----------|---------------|
| Hercules E (S5 pilot) | RequestDash | IsDashing | 🔴 Confirmed |
| Hercules R (S5 pilot) | RequestDash | IsDashing | 🟡 Same API, not manually verified yet |
| **Horus E (S6-03)** | RequestDash | IsDashing | 🔴 HIGH |
| Horus Q (S6-03) | Dash(Actor,…) | IsDash | 🟡 Same pattern |
| Horus R (S6-03) | Dash(Vector3,…) | IsDash | 🟡 Same pattern |
| **Volund W (S6-04)** | Dash(Vector3,…) | IsDash | 🟡 Same pattern |
| **Guan Yu E (S6-05)** | Dash(Actor,…) | IsDash | 🟡 Same pattern |
| **Skadi (S6-06)** | **(none)** | — | ✅ **CLEAN — 0 dash calls** |
| Wild Bill E (batch 2) | RequestDash | IsDashing | 🟡 Future verify |
| Athena E, Anansi E + W (batch 2) | Dash overloads | IsDash | 🟡 Future verify |

### Fix scope (revised)

1. Reset **`IsDash = false`** in `NetworkStatusEffect.Spawned()` callback (or wherever spawn-init happens)
2. Reset **`IsDashing = false; DashT = 0f;`** in same place
3. Add `Debug.LogWarning` to BOTH guard branches:
   - `if (IsDash) { Debug.LogWarning("[Dash] guard fired IsDash=true"); return; }` (lines 538, 547, 575, 598, 760)
   - `if (!Object.HasStateAuthority || IsDashing) { Debug.LogWarning($"[RequestDash] guard HasAuth={...} IsDashing={...}"); return; }` (line 947)
4. Verify Hercules E + R first cast clean post-fix (manual smoke + 3 matches in a row)
5. Verify cousin: load Horus → cast Q + E + R first cast each match → confirm no first-cast bug

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

### Phase 3 batch 1 kickoff decision (revised 2026-05-18 late-EOD — POST root cause revision)

The earlier "Skadi-first ordering" plan (when root cause was thought to be dash-API-specific) is **NO LONGER VALID**. Actual root cause = Phase 2 dual-path duplicate spawn affects EVERY Hero with AbilityComponent (including Skadi). All Phase 3 batch 1 stories now inherit the bug equally.

**Skadi is NOT a clean control case anymore**: even with 0 dash calls, Skadi will still suffer first-cast no-op on Q/W/E/R because the dual-path spawn affects every ability via the routing chain (Skill_X_Perform AnimationEvent → StateReleaseSlot → GetSlotAction returns wrong instance).

### Fix options — REVISED 2026-05-18 late-EOD-2 (after legacy-reader audit)

**Audit data (`grep` against delta-unity `Assets/GameScripts/**/*.cs` 2026-05-18)**:

| Legacy NetworkProperty | References | Unique files |
|------------------------|------------|--------------|
| `Combat.Skill1..4` | 403 | 50 |
| `Combat.NormalAttack` | 211 | 55 |
| `Combat.Passive` | 36 | 18 |
| `Combat.SkillRecall` | 22 | 10 |
| **Total** | **~672** | **~75 unique** |

Distribution: ActorCombatAction core, **every Hero ability file** (Hercules, Horus, Volund, Guan Yu, Skadi + 30 others), 5 UI view files, Actor base classes, Bot logic.

---

**Option 1 — Gate legacy `CreateSkill` for Hero with AbilityComponent**: ❌ **NOT FEASIBLE for Sprint 006**

- Would null out `Skill1..4`/`NormalAttack`/`Passive`/`SkillRecall` for Hero → 672 callers across 75 files must migrate to `GetSlotAction(slot)`
- **This is effectively Phase 4 work** (retire SkillKey enum + migrate to slot-based exclusively, ADR-0006 §10 forward-handover)
- Estimated: multi-sprint refactor (Sprint 007+); not a single-session fix
- **Recommendation**: defer to Sprint 007+ as formal Phase 4 kickoff

**Option 2 — Gate Phase 2 `BindSlot` to reuse legacy action**: ⚠️ **TIMING BLOCKER**

`ActorCombat.OnStartup` timing prevents reuse:
```csharp
CreateSkill();              // void return, async SpawnAsync inside — NOT awaited
BootstrapSlotBindings(...);  // runs immediately, Skill1..4 still null
```

`BindSlot` cannot reuse a legacy action that hasn't spawned yet → spawns its own → duplicate persists after legacy async completes.

Fix would require:
- (2a) Make `OnStartup` async + await `CreateSkill` before `BootstrapSlotBindings` — lifecycle change, risk of late init
- (2b) Make `BindSlot` deferred (subscribe to spawn-completion callback, bind retroactively) — adds complexity

Either path: ~0.5-1d implementation + careful Hercules/multipeer/cousin verify.

- 👍 Minimal blast radius for callers (672 still read Skill1..4)
- 👎 Lifecycle change risk; Phase 2 architecture becomes thin wrapper around legacy
- **Recommendation**: viable if explicit decision to defer Phase 4 to Sprint 007+

**Option 3 — Despawn duplicate in `OnStartup` post-init**: ⚠️ **DIRECTIONAL DECISION REQUIRED**

Need to choose which duplicate to despawn:
- Despawn **legacy orphan** → `Skill1..4` NetworkProperties become null → **672 callers break game-wide**
- Despawn **Phase 2 anchored** → `Slots[]` dictionary holds dangling NetworkBehaviourId → AnimationEvent routing chain still broken

Neither choice is clean. Option 3 alone is **not safe** without parallel migration of either the 672 callers (effectively Option 1) or the routing chain (partial Option 2).

- 👎 Both directions cause secondary breakage
- **Recommendation**: NOT recommended as standalone fix

---

### Strategic finding (escalation candidate)

**Phase 2 architecture intent vs reality**:
- ADR-0006/0008 design = Phase 2 BindSlot path **replaces** legacy CreateSkill for Hero (gated by `AbilityComponent` presence)
- S5-10 commit `748ddd410f` (2026-05-14) attached `AbilityComponent` to `base_avatar.prefab` **but did not gate `CreateSkill`** — both paths now run unconditionally for every Hero
- "Dual-path retention" documented in ADRs was intended for **transition state** (1 path active per hero based on migration progress) — not **always-on dual-spawn**

**Sprint 006 retro escalation (NEW item — promote to TD-010)**:
- Phase 2 dual-path retention was not gated when `AbilityComponent` attached → silent regression since 2026-05-14
- Need formal "migration completion criteria" doc + `/story-readiness` gate check ("if AbilityComponent attached, has legacy CreateSkill been gated for this hero?")
- Phase 4 kickoff (retire SkillKey enum + 672 caller migration) becomes Sprint 007 candidate with multi-sprint scope

### Recommended path forward

**Sprint 006 (current)**:
- **Defer BUG-0006 fix to Sprint 007** — no Option 1/2/3 ships cleanly within Sprint 006 budget without architectural commitment
- **Accept BUG-0006 as known regression** for Phase 3 batch 1 — workaround = cast twice; soak verdict = PASS WITH NOTES + known BUG-0006
- **Update Phase 3 batch 1 stories AC #7**: explicitly document "first-cast no-op known regression — workaround cast twice — tracked in BUG-0006"
- **Continue Phase 3 batch 1 implementation** (S6-03 Horus → S6-06 Skadi) after soak verdict 2026-05-21

**Sprint 007**:
- **Architectural session**: technical-director + lead-programmer decide Option 1 (Phase 4 start) vs Option 2 (transitional reuse) vs combined approach
- Estimated Sprint 007 capacity dedication: 1-2 stories (~2-3d) for BUG-0006 fix + migration foundation
- Then continue Phase 3 batch 2 (Anansi, Athena, KingArthur, etc.)

**Risk**: Phase 3 batch 1 ships with known first-cast bug → playtest evidence will record workaround usage → not shippable to public but acceptable for internal soak / dev playtest until Sprint 007 fix lands.

### Phase 3 batch 1 kickoff decision (final reading)

**Phase 3 batch 1 is BLOCKED in entirety** until BUG-0006 is RESOLVED. No Hero migration story (S6-03/04/05/06) can complete cleanly while duplicate spawn exists, because every per-hero AC #7 manual playthrough will reproduce the first-cast bug.

**Revised critical path**:
```
2026-05-19 to 2026-05-20 (2 days)
   ↓
   BUG-0006 fix session — decide Option 1/2/3, implement, verify Hercules E + R + Q + W (regression check)
   ↓
2026-05-21 soak verdict — combined with BUG-0006 RESOLVED → soak GREEN
   ↓
   Phase 3 batch 1: S6-03 Horus → S6-04 Volund → S6-05 Guan Yu → S6-06 Skadi → S6-07 batch gate
   0.5d × 5 = 2.5d
   ↓
2026-05-26 batch 1 complete (within Sprint 006, ends 2026-05-28)
```

Burn projection: ~4.85d (Day-4 close) + 1d (BUG-0006 fix) + 2.5d (batch 1) = **~8.35d** vs Sprint 006 budget 11d (with Should/Nice slack) = within budget but tight.

Final decision by tanapol.

## References

- [Phase 2 Lessons Learned](../../../docs/architecture/phase-2-lessons-learned.md)
- [ADR-0006 §10 Phase 2 → Phase 3 handover](../../../docs/architecture/ADR-0006-phase-2-migration-plan.md)
- [Sprint 006 plan](../../sprints/sprint-006.md)
- [Sprint 005 S5-21 — SetActiveSlot wiring + AnimationEvent 40-shim migration](../../sprints/sprint-005.md) — possible interaction site
- [Sprint 005 Hercules playthrough evidence](../evidence/sprint-005-hercules-playthrough.md) — Phase 2 exit sign-off (did not catch this regression; manual playthrough at S5-10 may not have spawned + cast E across multiple matches)
