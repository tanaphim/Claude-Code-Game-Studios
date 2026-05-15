# BUG-0005 — Multipeer Pass #4 parity regression (`actor not yet discovered on both peers`)

**Filed**: 2026-05-15 (during Sprint 006 S6-14 manual verification)
**Status**: 🔴 **OPEN — pre-existing regression, root cause unknown**
**Severity**: S3 (test harness only; not player-facing; multipeer wire parity verification is dev tool)
**Owner**: network-programmer (TBD assignee)

---

## Symptom

`AbilityMultipeerRunner` harness no longer passes Pass #4 (slot parity Host↔Client). All 4 retry attempts log:

```
[Multipeer-Parity] attempt N: actor not yet discovered on both peers (host=False, client=False).
```

Final verdict:
```
[Multipeer-Parity] ❌ FAILED after retries — Slots dict did not converge between Host and Client.
```

`FindActorAbilityComponent(runner)` returns null for **both** Host and Client runners → no NetworkObject with `AbilityComponent` visible to either peer via `GetAllNetworkObjects`.

## Historical baseline

Pass #4 was green in [production/qa/evidence/S5-10-multipeer.txt](../evidence/S5-10-multipeer.txt) (Sprint 5, ~2026-05-08):

```
[Multipeer-Parity] slot=1 ✅ host=(id=[Object:[Id:1027], Behaviour:0],'proto.ability.q') client=(id=[Object:[Id:1027], Behaviour:0],'proto.ability.q')
[Multipeer-Parity] slot=2 ✅ host=(id=[Object:[Id:1028], Behaviour:0],'proto.ability.w') client=(id=[Object:[Id:1028], Behaviour:0],'proto.ability.w')
[Multipeer-Parity] slot=3 ✅ host=(id=[Object:[Id:1029], Behaviour:0],'proto.ability.e') client=(id=[Object:[Id:1029], Behaviour:0],'proto.ability.e')
[Multipeer-Parity] slot=4 ✅ host=(id=[Object:[Id:1030], Behaviour:0],'proto.ability.r') client=(id=[Object:[Id:1030], Behaviour:0],'proto.ability.r')
[Multipeer-Parity] ✅ PASS #4 — all 4 slots converge (Host↔Client).
```

→ Regression introduced **between Sprint 5 closure and Sprint 6 Day-1 (2026-05-15)**.

## NOT caused by S6-14

S6-14 (AbilityMultipeerRunner duplicate-Start guard) changed only:

- Added static guard (`_isRunning`, `_isPrimary`) + `ApplyDuplicateGuard()` + `ReleaseGuardIfPrimary()`
- Guarded `Start()` entry + `OnDestroy()` reset
- Added `[InternalsVisibleTo("UnitTestEditMode")]` for test access
- Added `[RuntimeInitializeOnLoadMethod]` reset hook

S6-14 does NOT touch: TestActor spawn, `FindActorAbilityComponent`, `GetAllNetworkObjects`, parity verification logic, BindSlot pipeline.

Primary runner proceeds through `Start()` normally (verified by `[Multipeer-HOST] Host online.` + `[Multipeer-CLIENT] Client connected.` + `[Multipeer] Both runners online.` logs after S6-14 landed). The `BindSlotForTestHarness` calls also still fire (4× in latest Editor.log) — host-side slot binding works; only `GetAllNetworkObjects` returns no AbilityComponent-bearing NetworkObject.

## Suspected causes (to investigate)

1. **Phase 2 Hercules migration side effect** — slot binding rewrite changed when `AbilityComponent` attaches to `NetworkObject`; harness's pre-Phase-2 TestActor prefab may use stale pattern
2. **Fusion config drift** — `NetworkProjectConfig` may have changed between S5 and S6 (PeerMode, scene refs)
3. **TestActor prefab data drift** — `m_TestActorPrefab` field may be unassigned in current scene, OR prefab itself was edited (4× `BindSlotForTestHarness` calls succeed but no AbilityComponent surfacing in GetAllNetworkObjects — suspicious mismatch)
4. **Scene `PrototypeTest.unity`** — flagged for rename in EPIC.md S7-PROPOSED-SCENE-RENAME; may be misconfigured

## Reproduction

1. Open `PrototypeTest.unity` (or whichever scene has AbilityMultipeerRunner GameObject)
2. Verify `m_TestActorPrefab` is assigned in Inspector
3. Press Play → wait 5s → observe Console
4. Expected: `[Multipeer-Parity] ✅ PASS #4` log
5. Actual: `❌ FAILED after retries`

## Workaround

None. Pass #5 (bandwidth idle <2048 B/s) still works. Pass #4 must be skipped manually during multipeer testing until BUG-0005 is investigated.

## Test plan (when investigated)

- Bisect git history between S5-10 evidence date (~2026-05-08) and 2026-05-15 to find regression commit
- Add `NetworkRunner.GetAllNetworkObjects` count log to `FindActorAbilityComponent` for diagnosis
- Cross-check TestActor prefab Inspector state vs Sprint 5 baseline (if .meta archive available)
- Verify `BindSlotForTestHarness` is being called on the actor that gets spawned by `m_HostRunner.Spawn(m_TestActorPrefab, ...)` and not a phantom local actor

## Files involved (suspected)

- `Assets/GameScripts/Gameplays/Abilities/Testing/AbilityMultipeerRunner.cs:269-274` (FindActorAbilityComponent)
- `Assets/GameScripts/Gameplays/Abilities/AbilityComponent.cs:227` (BindSlotForTestHarness call site — confirmed fires 4× in Editor.log)
- TestActor prefab (path TBD via Inspector / scene file)

## References

- [S6-14 story (closing with AC #4 interpretation by spirit)](../../epics/test-infrastructure/S6-14-multipeer-duplicate-start-guard.md)
- [S5-10 multipeer evidence (Pass #4 green baseline)](../evidence/S5-10-multipeer.txt)
- [S6-14 evidence (current run log)](../evidence/S6-14-multipeer-console-clean.txt)
- [Phase 2 lessons learned](../../../docs/architecture/phase-2-lessons-learned.md)
