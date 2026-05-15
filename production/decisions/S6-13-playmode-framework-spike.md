# Decision: S6-13 — PlayMode Test Framework Spike Outcome

**Date**: 2026-05-15
**Sprint**: 006 (S6-13, Nice-to-Have spike, time-boxed 1.5d with 0.75d abort gate)
**Spike type**: Research + candidate implementation
**Status**: ✅ **WORKING — Sprint 007 follow-up filed** (Unity Test Runner validation 2026-05-15: 4/4 smoke tests PASS)

---

## Spike question

> Can we cover `NetworkBehaviour` methods with unit-level automated tests by spawning a minimal `NetworkRunner` fixture in PlayMode tests, instead of falling back to multipeer harness + manual playtest?

**Background** — Sprint 005 retrospective seeds Finding 1: `ActorCombat` is a Fusion `NetworkBehaviour`; instance methods reading networked properties (`Skill1..4`, `m_ActiveSlot`, `IsQuickQ..R`) cannot be exercised in EditMode. S5-02/S5-03 acceptance criteria specified "covered by unit test" — only partial coverage via static-helper extraction was achieved. The networked-state behavior fell to multipeer harness + manual playtest, which is where TD-006/TD-007 surfaced in Sprint 005 — late and expensive.

## Deliverables (authored 2026-05-15)

| Artifact | Path |
|---|---|
| Base fixture class | `delta-unity` → `Assets/UnitTests/TestRuntimeMode/NetworkRunnerFixture.cs` |
| Smoke test (4 cases) | `delta-unity` → `Assets/UnitTests/TestRuntimeMode/NetworkRunnerSmokeTest.cs` |
| This decision doc | `production/decisions/S6-13-playmode-framework-spike.md` |

## Approach

The fixture spawns a single-peer (`GameMode.Single`) NetworkRunner in `[UnitySetUp]` and shuts it down in `[UnityTearDown]`. Subclasses access the runner via the `Runner` property. Pattern is adapted from the proven `AbilityMultipeerRunner.cs` (which spawns 2 runners in a scene-mounted MonoBehaviour for the multipeer harness) — same `NetworkRunner.StartGame(args)` + `Shutdown()` lifecycle, but simplified to single-peer, no scene argument, no input provider.

### Smoke test coverage

The 4 smoke tests verify infrastructure only — they don't yet exercise domain `NetworkBehaviour` code:

1. `Runner_IsRunning_AfterFixtureSetUp` — runner reports `IsRunning == true` after StartGame
2. `Runner_HasLocalPlayer_InSinglePeerMode` — `LocalPlayer != PlayerRef.None`
3. `Runner_IsServer_InSinglePeerMode` — `IsServer == true` (single-peer is authoritative)
4. `TickOnce_AdvancesFrameWithoutError` — runner survives a single frame tick

If all 4 pass → infrastructure works → scale up. If any fail → infrastructure unviable → abort.

## Decision branches

### ✅ Working branch — all 4 smoke tests pass

**Interpretation**: Fusion 2 accepts test-context runner spawn; the fixture pattern is viable for scaling.

**Next steps (Sprint 007 or later, file a follow-up story)**:

1. **Add a "spawn-and-exercise" test pattern** — extend fixture with helper to register a test prefab (e.g., a minimal NetworkObject + `NetworkBehaviour` subclass dedicated to testing) and spawn it. Verify networked property writes propagate after `TickOnce()`.
2. **Tackle the DeltaService coupling** — most production `NetworkBehaviour`s (`ActorCombat`, `AbilityComponent`, etc.) depend on `DeltaService.I.GetService<T>()`. Three options:
   - (a) Stub `DeltaService` for tests (preferred if dependency footprint is small)
   - (b) Extract domain logic into static helpers that take dependencies as parameters (continuation of S5 pattern)
   - (c) Build a minimal `TestNetworkBehaviour` that demonstrates the pattern without touching domain services
3. **Codify in story-readiness gate** — once a real NetworkBehaviour test lands, story-readiness can require PlayMode coverage for "Logic with NetworkBehaviour state" stories instead of falling back to "covered by multipeer harness".
4. **Migrate retroactive coverage** — `ActorCombat.GetSlotAction`, `SetActiveSlot`, `GetActiveSlot` are top candidates (these surfaced TD-006 in S5).

**Sprint 007 candidate estimate for follow-up**: 1.5-2d depending on which DeltaService option is chosen.

### ❌ Abort branch — smoke tests fail

**Possible failure modes**:

| Symptom | Likely cause |
|---|---|
| StartGame never returns Ok | Fusion 2 test-context restriction (no scene loaded / no app config) |
| Runner.IsRunning never becomes true | Network simulation loop not ticking in test mode |
| Teardown deadlocks | Shutdown() async pattern incompatible with `[UnityTearDown]` coroutine |
| NRE in NetworkSceneManagerDefault.AddComponent | Scene manager requires a managed scene |
| Project peer-mode toggle interaction | Project currently has PeerMode=Single (S4-P5); if toggled to Multiple, fixture may need adjustment |

**Next steps (formal adoption)**:

1. **Static-helper extraction = canonical pattern** — this becomes the project-wide convention for `NetworkBehaviour` testability. Codify in `coding-standards.md` or as a `coordination-rules.md` § Testing Conventions entry.
2. **Story-readiness language** — for Logic stories with NetworkBehaviour state, AC must explicitly state either:
   - "covered by static-helper extraction + EditMode test for the helper" (preferred), OR
   - "networked state behavior verified by multipeer harness Pass #4 + manual playtest" (fallback)
3. **Multipeer harness as gate** — increase reliance on `AbilityMultipeerRunner` Pass #1-5 for Phase 3 hero migrations (already the de-facto pattern in Sprint 005)
4. **Delete the spike artifacts** OR keep as documentation of "what was tried, why it didn't work" — recommend keep with prominent comment block

**No follow-up story needed** — abort is a closure.

## Risks identified during research

These are documented for future surveillance regardless of working/abort outcome:

1. **DeltaService coupling** — most production `NetworkBehaviour`s depend on the global `DeltaService.I` singleton. Test fixture cannot bootstrap the full service graph without scene-side initialization. Affects any attempt to test `ActorCombat`, `AbilityComponent`, hero actions, etc.
2. **Prefab registration** — `runner.Spawn(prefab, ...)` requires the prefab to be registered with `NetworkProjectConfig`. Test fixtures cannot register prefabs at runtime without touching the global config (which would leak into other tests).
3. **Single-peer `IsServer == true` assumption** — `GameMode.Single` makes the local peer authoritative; this is the standard Fusion behavior. Smoke test #3 will validate this.
4. **Async/coroutine bridging** — Fusion 2 lifecycle is async (`Task`); Unity Test Framework uses coroutines (`IEnumerator`). The fixture's `WaitForTask` helper bridges these with a timeout. Race-conditions around `IsCompleted` vs `IsCompletedSuccessfully` could surface; current code prefers `IsCompletedSuccessfully` for explicit success checks.
5. **Peer-mode-toggle workflow conflict** — project currently uses `PeerMode=Single` for production, `PeerMode=Multiple` for multipeer harness (S4-P5 manual toggle). If a developer leaves the toggle in Multiple, these fixtures will fail in unexpected ways. Future hardening: add a fixture pre-condition check + clear error message.
6. **asmdef precompiledReferences gotcha (3 compile rounds 2026-05-15)** — `UnitTestPlayMode.asmdef` had `"overrideReferences": true` with only `nunit.framework.dll` in `precompiledReferences`. Adding `"Fusion.Unity"` to `references` is necessary but NOT sufficient — `NetworkRunner` / `StartGameArgs` live in precompiled `Fusion.Runtime.dll` (not in the Fusion.Unity asmdef itself), and with `overrideReferences: true` they don't transitively flow through. **Round 1 fix**: add `Fusion.Runtime.dll` + `Fusion.Common.dll` to `precompiledReferences` (matches `UnitTestEditMode.asmdef` pattern). **Round 2 fix**: also need `Fusion.Log.dll` — `NetworkRunner` references `ILogSource` / `ILogDumpable` interfaces that live in this DLL (Fusion 2.0.6 split logging into a sub-assembly). EditMode asmdef does NOT need this because EditMode tests only use static helpers / non-runner types. **Round 3 fix**: name collision — both `Fusion.Assert` (runtime assertion helper for networked logic) and `NUnit.Framework.Assert` (test framework) exist in scope. Must add `using Assert = NUnit.Framework.Assert;` to disambiguate. **Lesson**: when scaling PlayMode tests that spawn `NetworkRunner` directly (vs. exercising static helpers), the full Fusion DLL stack is required — `Fusion.Runtime.dll` + `Fusion.Common.dll` + `Fusion.Log.dll` — plus the `Assert` alias. If `NetworkSceneManagerDefault` or other types surface further errors, add the relevant DLL from `Assets/Photon/Fusion/Assemblies/` (candidates: `Fusion.Sockets.dll`, `Fusion.Realtime.dll`).

## User-side validation checklist

To resolve the working/abort decision, run in Unity Editor:

1. Open `delta-unity` project
2. Confirm `NetworkProjectConfig.PeerMode == Single` (Project Settings → Fusion)
3. Open Window → General → Test Runner
4. Switch to **PlayMode** tab
5. Locate `NetworkRunnerSmokeTest` (in `Radius.UnitTests.PlayMode` namespace)
6. Run all 4 tests
7. Record outcome:
   - **All 4 PASS** → ✅ Working branch — file follow-up story for scaling
   - **Any FAIL** → ❌ Abort branch — record failure mode, adopt canonical static-helper pattern, delete or annotate the spike artifacts

Expected runtime: ~10-15 seconds total (each test does StartGame + Shutdown, ~3s each).

## Acceptance criteria (S6-13 closure)

- ✅ Candidate fixture + smoke tests authored (this work)
- ✅ Decision doc with both branches + scaling roadmap (this file)
- ✅ **User-side validation 2026-05-15: 4/4 PASS** (Runner_IsRunning_AfterFixtureSetUp, Runner_HasLocalPlayer_InSinglePeerMode, Runner_IsServer_InSinglePeerMode, TickOnce_AdvancesFrameWithoutError)
- ✅ **Working branch confirmed** — Sprint 007 follow-up filed (see "Sprint 007 follow-up" section below)

## Sprint 007 follow-up (working branch outcome)

**Filed 2026-05-15 as S7-PROPOSED-PLAYMODE-SCALING** — to be confirmed during Sprint 007 planning. Tentative title: "Scale PlayMode NetworkRunner fixture to first NetworkBehaviour domain coverage".

**Scope** (1.5-2.0d estimate per spike research):

1. **Add spawn-and-exercise pattern** — extend `NetworkRunnerFixture` with `protected NetworkObject SpawnTestObject(NetworkObject prefab)` helper that registers + spawns a prefab, returns the spawned instance for the test to exercise
2. **Build minimal `TestNetworkBehaviour` + matching prefab** — a NEW domain-free NetworkBehaviour with 1-2 `[Networked]` properties and 1-2 RPCs, used as the "Hello World" of NetworkBehaviour testing. Verifies write → tick → read propagation works at unit-test scale
3. **First production-NetworkBehaviour test** — most-bang-for-buck candidate: `ActorCombat.GetSlotAction(byte)` + `SetActiveSlot(byte)` cycle (these surfaced TD-006 in Sprint 005). Will require DeltaService stub OR routing through a static helper.
4. **DeltaService coupling decision** — pick path among (a) stub DeltaService for tests, (b) extract static helpers further, (c) keep TestNetworkBehaviour scope only and leave domain coverage to multipeer harness. Decision lands in Sprint 007 ADR or decision doc.
5. **Codify PlayMode coverage in `/story-readiness`** — once a real NetworkBehaviour test lands, story-readiness gate can require PlayMode coverage for "Logic with NetworkBehaviour state" stories instead of falling back to multipeer harness.

**Owner candidates**: lead-programmer + gameplay-programmer (`tanapol`)

**Sprint 007 readiness check**: requires Phase 3 hero migration batch 1 (S6-03..06) to be closed first — Phase 3 stories provide concrete `ActorCombat` test candidates that will exercise the scaled fixture immediately.

## References

- [Sprint 006 plan S6-13](../sprints/sprint-006.md)
- [Sprint 005 plan §Retrospective Seeds](../sprints/sprint-005.md) Finding 1 — NetworkBehaviour EditMode testability gap
- [Phase 2 Lessons Learned](../../docs/architecture/phase-2-lessons-learned.md) — static-helper extraction pattern (used 4× in Sprint 005)
- `AbilityMultipeerRunner.cs` (delta-unity) — source of the StartGame + Shutdown pattern this fixture adapts
- `UnitTestPlayMode.asmdef` (delta-unity) — existing PlayMode test asmdef already references `Fusion.Unity`
- Sprint 004 retro action #1 — Photon STUN/PeerMode root cause closure (S4-P5 toggle workflow)
