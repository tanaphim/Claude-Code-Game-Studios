# BUG-0008 — Teleport Ticket: movement broken + Idle animation stuck (multipeer)

**Filed**: 2026-05-18 (team report, Sprint 006 Day 4)
**Status**: 🔴 OPEN
**Severity**: **S2** (severe — movement broken whole match; respawn/restart match does NOT fix; player effectively griefed for entire game; multipeer-only)
**Priority**: **P1** (fix this sprint if cousin pattern surfaces during Phase 3 batch 1 multipeer playthroughs; otherwise P2 Sprint 007)
**Owner**: gameplay-programmer (TBD assignee)
**Affected**: Teleport Ticket item (likely `item_teleport_to_unit.prefab` in `Assets/Resources/Prefabs/Gameplay/Spell/Item/`)

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

## Likely root cause hypotheses

### H1 — `[Networked]` movement-blocker state leak (analogous to pre-fix BUG-0006 IsDashing hypothesis)

`NetworkTrait` / `NetworkStatusEffect` has several movement-blocking `[Networked]` flags:
- `IsDash` (line 111 in `NetworkStatusEffect.cs`)
- `IsKnockUp` (line 81)
- `IsKnockBack` (line 96)
- `IsDashing` (line 943)

If teleport item sets one of these to gate movement during transit, and the reset callback runs **only on initiating peer** (not recipient peer in multipeer), the affected client retains the flag set forever. The fact that `respawn does NOT fix` strongly suggests `[Networked]` state that survives despawn — typical of Fusion's serialized state, esp. if `Despawned()` doesn't reset it.

**Note**: Defensive reset of `IsDashing` + `DashT` was added to `NetworkTrait.Despawned()` in BUG-0006 fix commit `4ed9a04dda` (orthogonal change). If H1 turns out to apply to `IsKnockUp` or `IsKnockBack`, similar pattern would resolve.

### H2 — NavMeshAgent state mismatch between peers

`Actor.Driver.ActiveAgent(false/true)` toggles NavMeshAgent. If teleport handler:
- Calls `ActiveAgent(false)` to allow position teleport (NavMesh would prevent it otherwise)
- Calls `ActiveAgent(true)` to re-enable after teleport
- BUT the re-enable call only fires on the initiating peer

Then recipient peer client has NavMeshAgent disabled → movement input ignored → Idle animation locked.

### H3 — Animator parameter desync

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

- Likely affected: `Assets/Resources/Prefabs/Gameplay/Spell/Item/item_teleport_to_unit.prefab`
- Pattern reference: pre-fix BUG-0006 IsDashing leak hypothesis (ruled out for BUG-0006 but pattern plausible for teleport)
- BUG-0001 `AnimatorStateSync` fix (S5-19, commit `32e154d43a`) — pattern for clearing stale animator state on networked sync
