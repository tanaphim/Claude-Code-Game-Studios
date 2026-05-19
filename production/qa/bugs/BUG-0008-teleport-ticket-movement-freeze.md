# BUG-0008 — Teleport Ticket: movement broken + Idle animation stuck (multipeer)

**Filed**: 2026-05-18 (team report, Sprint 006 Day 4)
**Status**: ✅ **RESOLVED** 2026-05-19 (Sprint 006 Day 5 — same-day fix after surveillance trigger satisfied)
**Severity**: S2 (severe — movement broken whole match; respawn/restart match does NOT fix; player effectively griefed for entire game; multipeer-only)
**Priority**: was P1 ACTIVE Sprint 006 fix (escalated from P1 conditional 2026-05-19); resolved within escalation window
**Owner**: gameplay-programmer + network-programmer (assignee: tanapol)
**Affected**: Teleport Ticket item (`item_teleport_to_unit.prefab` in `Assets/Resources/Prefabs/Gameplay/Spell/Item/`) — and by extension, **any item-cast that lands the Item animator layer on its `Empty` rest state** (fix is layer-rest-state generic, not Teleport-specific)

---

## Symptom

1. Player uses Teleport Ticket item — teleport effect completes, player position changes to target as expected
2. **After teleport completes**: character cannot move (movement input ignored)
3. **Animator stuck** in Idle state — no Run/Walk transitions
4. **Persistent across recovery attempts**:
   - Respawn after death — bug **still present**
   - Match restart — bug **still present**
   - Only Unity restart (per cousin reporting pattern) might clear, untested

## Multipeer-only

Operator self-check 2026-05-18 in host-only / single-peer Editor Play mode: **could NOT reproduce**. Bug only surfaces with 2+ peers. Reproduction likely requires:
- 2 Unity Editor instances joining same Photon session, OR
- 1 Editor host + 1 built client, OR
- Build + run 2 separate clients

## Pre-existing — NOT regression from BUG-0006 fix

Team reported this bug **before 2026-05-18** (per operator confirmation). BUG-0006 fix touches `SkillStateMachine.Initialize()` — that path is for ability state machines, not item handlers, so unlikely to cause this.

## Root cause analysis (2026-05-19 — surveillance test reproduced bug)

**Reproduction confirmed via ParrelSync 2-Editor setup** during S6-07 batch gate BUG-0008 surveillance step (2026-05-19, Sprint 006 Day 5). Tester: tanapol.

**NEW EVIDENCE narrows root cause to H3 only**:

- **Client peer stuck at `Item_Casting` animator state** after teleport completes
- Animator does NOT transition out of `Item_Casting` back to Idle/Run/Walk
- Movement input ignored because animator state machine remains in `Item_Casting` (which blocks movement transitions by design)

**H1 (`[Networked]` movement-blocker state leak) — RULED OUT**:
If H1 were correct, animator would be at Idle (the movement-block flag prevents transitions OUT of Idle into Run/Walk). Observed state = Item_Casting, not Idle → flag-leak hypothesis cannot explain this.

**H2 (NavMeshAgent state mismatch) — RULED OUT** (same reasoning):
NavMeshAgent disabled would also leave animator at Idle, not Item_Casting.

**H3 (Animator parameter desync) — ✅ CONFIRMED**:
The animator parameter or trigger that controls `Item_Casting → Idle` exit transition is not being replicated/cleared on the client peer. Server completes the cast, transitions out; client never sees the exit signal.

## Cousin to BUG-0001 (S5-19 AnimatorStateSync family)

BUG-0001 (recall locomotion stuck) was resolved at S5-19 (`32e154d43a`) by extending `AnimatorStateSync` to clear stale state hashes when the host returns to locomotion. BUG-0008 is a **direct cousin** with the same shape but on a different animator layer/state:

| Bug | Stuck state | Layer | Resolved at |
|---|---|---|---|
| BUG-0001 | Recall (ability cast) | Ability layer | S5-19 commit `32e154d43a` |
| BUG-0008 | **Item_Casting** | **Item-cast layer** (suspected separate from ability layer) | **Sprint 006 Day 5+ — IN-PROGRESS** |

The fix shape is expected to be similar — extend `AnimatorStateSync` hash-clear coverage to include the Item_Casting state / item-cast layer. Either:
- The item-cast layer is separate from ability layer and not currently included in `AnimatorStateSync.HasAnyLayerStateHashChanged()` scan
- Or the item-cast state uses a different animator parameter type (trigger vs bool) that bypasses the existing clear mechanism

## Likely root cause hypotheses (original 2026-05-18; H1+H2 now RULED OUT)

### H1 — `[Networked]` movement-blocker state leak (analogous to pre-fix BUG-0006 IsDashing hypothesis) — RULED OUT

`NetworkTrait` / `NetworkStatusEffect` has several movement-blocking `[Networked]` flags:
- `IsDash` (line 111 in `NetworkStatusEffect.cs`)
- `IsKnockUp` (line 81)
- `IsKnockBack` (line 96)
- `IsDashing` (line 943)

If teleport item sets one of these to gate movement during transit, and the reset callback runs **only on initiating peer** (not recipient peer in multipeer), the affected client retains the flag set forever. The fact that `respawn does NOT fix` strongly suggests `[Networked]` state that survives despawn — typical of Fusion's serialized state, esp. if `Despawned()` doesn't reset it.

**Note**: Defensive reset of `IsDashing` + `DashT` was added to `NetworkTrait.Despawned()` in BUG-0006 fix commit `4ed9a04dda` (orthogonal change). If H1 turns out to apply to `IsKnockUp` or `IsKnockBack`, similar pattern would resolve.

### H2 — NavMeshAgent state mismatch between peers — RULED OUT

`Actor.Driver.ActiveAgent(false/true)` toggles NavMeshAgent. If teleport handler:
- Calls `ActiveAgent(false)` to allow position teleport (NavMesh would prevent it otherwise)
- Calls `ActiveAgent(true)` to re-enable after teleport
- BUT the re-enable call only fires on the initiating peer

Then recipient peer client has NavMeshAgent disabled → movement input ignored → Idle animation locked.

### H3 — Animator parameter desync — ✅ CONFIRMED 2026-05-19

If teleport item triggers an animator parameter (e.g. `IsTeleporting=true`) and the clearing event only fires server-side without proper replication via `AnimatorStateSync`, the recipient client's animator stays in a state that blocks Run/Walk transitions.

BUG-0001 (RESOLVED in S5-19) addressed a similar pattern for ability animations via `AnimatorStateSync` stale-hash clear — teleport may use a different animator parameter path that doesn't get the same treatment.

## Reproduction (requires 2 clients)

1. Open 2 Unity Editor instances (or 1 Editor + 1 built client)
2. Both join same match in `scene_game_map.unity`
3. One player uses Teleport Ticket item (`item_teleport_to_unit.prefab`)
4. After teleport completes → try moving the teleported player
5. **Expected**: normal movement
6. **Actual**: movement input ignored, animator stuck in Idle
7. Verify persistence: respawn → still broken; restart match → still broken

## Investigation candidates

- `Assets/GameScripts/Gameplays/Items/` — find teleport item action implementation
- `Assets/Resources/Prefabs/Gameplay/Spell/Item/item_teleport_to_unit.prefab` — check inspector for any state-setter components
- `Assets/GameScripts/Gameplays/Cores/Stat/NetworkStatusEffect.cs` — audit movement-blocker `[Networked]` fields + reset paths
- `Assets/GameScripts/Gameplays/Characters/AnimatorStateSync.cs` — verify teleport-related animator parameters get the same hash-clear treatment as BUG-0001 fix
- Diff: how does the working `item_heal_grenade.prefab` (or other items) compare to teleport?

## Phase 3 batch 1 link

If root cause = `[Networked]` movement-block state leak in `NetworkTrait`/`NetworkStatusEffect`, **all heroes are equally affected** (state is on `Actor.Trait`, not per-hero). Phase 3 batch 1 multipeer playthroughs (AC #6) should include Teleport Ticket use as part of the standard test routine — surfaces this bug early without needing a separate investigation cycle.

## References

- Affected item: `Assets/Resources/Prefabs/Gameplay/Spell/Item/item_teleport_to_unit.prefab`
- Pattern reference: pre-fix BUG-0006 IsDashing leak hypothesis (ruled out for BUG-0006 but pattern plausible for teleport — both H1 + H2 ultimately ruled out for BUG-0008 too)
- BUG-0001 `AnimatorStateSync` fix (S5-19, commit `32e154d43a`) — direct pattern parent; BUG-0008 fix extends the same `SynchronizeStates()` clear-hash logic to upper-layer rest states

---

## Resolution (2026-05-19 — Sprint 006 Day 5)

### Fix details

**File**: `Assets/GameScripts/Gameplays/Characters/AnimatorStateSync.cs`
**Lines changed**: 2 logical changes (1 declaration + 1 condition extension); ~6 lines including documentation comments

```diff
 private static readonly int IdleHash = Animator.StringToHash("Idle");
 private static readonly int RunHash = Animator.StringToHash("Run");
 private static readonly int DeadHash = Animator.StringToHash("Dead");
+private static readonly int EmptyHash = Animator.StringToHash("Empty");
 // ...

-if (stateHash == IdleHash || stateHash == RunHash) //|| stateHash == DeadHash
+if (stateHash == IdleHash || stateHash == RunHash || stateHash == EmptyHash) //|| stateHash == DeadHash
 {
     // BUG-0001 + BUG-0008: clear stale ability/item StateHash...
 }
```

### Root cause (verified)

H3 (Animator parameter desync) confirmed by symptom: client peer stuck at `Item_Casting` state. Host exited normally.

S5-19's BUG-0001 fix clears stale `StateHash` in `SynchronizeStates()` only when host's animator returns to `Idle` or `Run` (base-layer locomotion). Verified in `RadiusBasicLocomotion.controller`:

- Item layer's `m_DefaultState` = state with `m_Name: Empty` (file ID `-1704394266355094815`)
- 3 "Empty" states exist in the controller (all share the same `Animator.StringToHash("Empty")` short-name hash)
- When host's Item layer transitions out of `Item_Casting` and returns to `Empty` (Item-layer rest), the BUG-0001 fix condition didn't match (Empty != Idle, Empty != Run)
- → Item layer's stale `Item_Casting` hash stayed in `_state.States[itemLayerIndex]`
- → Client's `UpdateStates()` kept force-replaying `Item_Casting` (non-zero hash → applied)
- → Client animator could never leave `Item_Casting` → movement input ignored (state machine blocks Idle/Run transitions while in Item_Casting)

### Fix shape rationale

Mirrors BUG-0001 S5-19 pattern exactly — adds one more "rest state" hash to the clear list. The fix is layer-rest-generic: any upper layer whose host state is `Empty` will have its stale stateHash cleared, freeing the client animator to be driven by its own input. No per-layer specialization needed because:

1. `Empty` is the Unity convention for upper-layer "nothing playing" rest state
2. Clearing a stateHash to 0 only releases client; doesn't break anything (UpdateStates skips 0-hash entries)
3. Base layer doesn't have an "Empty" state (it has Idle/Run/Walk/Dead/etc.) so no risk of over-clearing base-layer locomotion

### Verification (user-tested 2026-05-19)

ParrelSync 2-Editor setup:
1. Re-reproduced original bug pre-fix (client stuck Item_Casting)
2. Applied 2-line fix to AnimatorStateSync.cs
3. Recompiled, restarted ParrelSync clones
4. Re-test: cast Teleport Ticket from host → both peers continue teleport
5. **Post-teleport client peer moves normally + animator exits Item_Casting cleanly** ✅

User confirmed: "ปกติแล้วครับ" (back to normal).

### Cousin family update — BUG-0001 AnimatorStateSync family

BUG-0008 is now confirmed as the **5th cousin** of the BUG-0001 AnimatorStateSync stale-hash family. Previous 4 cousins (HorusE, HorusR, VolundW, GuanYuE) were all on **base layer** (ability cast → locomotion return) — resolved by the original S5-19 Idle/Run check. BUG-0008 was the first **upper-layer** cousin (Item layer cast → Item rest return) — resolved by extending the same check to include the Empty hash.

The fix pattern is now generic across all layers. Future cousin candidates: any other upper layer that has an `Empty` (or similar) rest state. The fix already covers them.

### Sprint 006 impact

- Surveillance trigger at S6-07 batch gate (PM triage 2026-05-18) satisfied via real-2-peer ParrelSync test 2026-05-19
- BUG-0008 escalated → fixed same-day per escalation protocol
- Sprint 006 burn impact: ~0.5d (investigation + 2-line fix + verification) — well within remaining buffer
- Phase 3 batch 1 final story (S6-07 batch gate) closes with surveillance verdict = **ESCALATED-AND-RESOLVED-IN-SPRINT**

