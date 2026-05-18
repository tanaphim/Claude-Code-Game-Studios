# BUG-0005 — Multipeer Pass #4 parity regression (`actor not yet discovered on both peers`)

**Filed**: 2026-05-15 (during Sprint 006 S6-14 manual verification)
**Status**: ✅ **RESOLVED 2026-05-18 — delta-unity `dev` `eb1c4ea695` pushed; manual 3-step verify PASS**
**Severity**: S3 (test harness only; not player-facing; multipeer wire parity verification is dev tool)
**Owner**: network-programmer (tanapol)
**Fix commit**: [`eb1c4ea695`](https://github.com/radiuszon/delta-unity/commit/eb1c4ea695) — "BUG-0005: AbilityMultipeerRunner toggles PeerMode=Multiple at runtime"
**Resolution date**: 2026-05-18
**Verification**: 3-step manual on operator machine PASS (multipeer Pass #4 ✅; scene_initial → game_map ✅; retry without Unity restart → game_map ✅)

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

---

## Addendum 2026-05-18 — Root cause identified

### Root cause

`delta-unity` commit **`c7df9c9d05` "Fix PeerMode"** (2026-05-08 11:18, on `dev` branch) flipped `Assets/Photon/Fusion/Resources/NetworkProjectConfig.fusion`:

```
- "PeerMode": 1,    (Multiple — required for AbilityMultipeerRunner)
+ "PeerMode": 0,    (Single — production game mode)
```

Fusion 2 enum: `PeerModes.Single = 0`, `PeerModes.Multiple = 1`. With `Single` mode, Fusion only fully initializes one `NetworkRunner` per process. `AbilityMultipeerRunner` spawns a Host runner + a Client runner in-process; the second runner does not initialize its NetworkObject tracking, so `runner.GetAllNetworkObjects(list)` returns an empty list → `FindActorAbilityComponent` returns null for both → Pass #4 fails with the observed symptom `actor not yet discovered on both peers (host=False, client=False)`.

The commit message says "Fix PeerMode" with no body — likely intent was to switch to Single for the production game build (most Fusion games use Single), but the change unintentionally broke the dev multipeer harness on `dev` branch.

### Investigation timeline (2026-05-18)

1. Bisected git history in regression window (S5-10 evidence ~2026-05-08 → S6-14 manual run 2026-05-15)
2. Ruled out: TestActor prefab unchanged, PrototypeTest.unity unchanged, AbilityMultipeerRunner.cs unchanged (until S6-14 itself), AbilityComponent.cs unchanged, AbilityPrototypeDriver.cs unchanged, base_avatar.prefab unchanged in window
3. Diagnostic probe drafted for `FindActorAbilityComponent` to disambiguate empty-list vs missing-component vs replication-failure — **not run**; root cause found before probe was needed
4. Operator instinct ("peer mode = multiple มั้ย") pointed to `NetworkProjectConfig.fusion` — `git log` of that file surfaced commit `c7df9c9d05` as the sole change in the window
5. `git show HEAD:NetworkProjectConfig.fusion` confirmed committed state on `dev` is `PeerMode: 0`; working tree on operator's machine already had local edit to `PeerMode: 1` (uncommitted)

### Why S5-10 evidence (~2026-05-08) showed Pass #4 green despite the same date as the Fix PeerMode commit

S5-10 multipeer evidence was captured **before** commit `c7df9c9d05` landed at 11:18 on 2026-05-08. Editor.log captures pre-date the commit; no contradiction with the bisect result.

### Path A (initially recommended, then RETRACTED — would have recreated 2026-04-20 regression)

Initial plan was to revert `NetworkProjectConfig.fusion` to `PeerMode: 1 (Multiple)` because of a misread of the project's PeerMode history. The operator corrected this during investigation:

> "ก่อน 2026-05-08 ทั้ง project รันบน Multiple มาตลอด" was wrong. The committed config drifted to Multiple in commit `6938a52c2d` (2026-04-20, "Day 2 wrap-up: prototype scene + weaver access fix (ADR-0006 S2-14)") when the operator pushed local Multiple-mode setting from prototype work. Production matchmaking broke immediately. Operator reverted to Single in `c7df9c9d05` (2026-05-08, "Fix PeerMode") — production fix.

→ Path A (revert to Multiple in repo) would have **re-introduced the exact 2026-04-20 regression** that `c7df9c9d05` was the fix for. Empirical evidence from the operator's 2026-05-18 verification session confirmed this: with working-tree `PeerMode: 1`, production play sessions could enter `scene_game_map` only on the first attempt; subsequent retries stalled at `scene_game_mode` matchmaking lobby (3/3 retries observed in `Editor.log` — `Joining Lobby Shared RADIUS_SESSION_NAME_asia` followed by no progress). The multipeer harness Pass #4 went green, but production became unusable.

Path A is therefore **incorrect**. Repo config must stay `PeerMode: 0 (Single)`.

### Fix (chosen path: B — runtime PeerMode toggle in AbilityMultipeerRunner)

Production config in `NetworkProjectConfig.fusion` stays `PeerMode: 0 (Single)` — committed state on `dev` since `c7df9c9d05` is correct.

The multipeer harness toggles `NetworkProjectConfig.Global.PeerMode = Multiple` at runtime in `Start()` and restores `Single` in `OnDestroy()`. Implementation in [`AbilityMultipeerRunner.cs`](../../../../../delta-unity/Assets/GameScripts/Gameplays/Abilities/Testing/AbilityMultipeerRunner.cs):

```csharp
private async void Start()
{
    if (!ApplyDuplicateGuard()) return;

    // BUG-0005: production config is PeerMode=Single (matchmaking depends on it).
    // Multipeer harness needs Multiple to run 2 in-process NetworkRunners. Toggle
    // the global config here; restored to Single in OnDestroy so a subsequent
    // production scene load in the same Editor session is not affected.
    NetworkProjectConfig.Global.PeerMode = NetworkProjectConfig.PeerModes.Multiple;

    // ... rest unchanged
}

private void OnDestroy()
{
    ReleaseGuardIfPrimary();
    UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;

    // BUG-0005: restore production PeerMode so the next scene (e.g. scene_initial)
    // in this Editor session does not inherit Multiple mode.
    NetworkProjectConfig.Global.PeerMode = NetworkProjectConfig.PeerModes.Single;

    // ... rest unchanged
}
```

Why this works:
- `AbilityMultipeerRunner` is only present in `PrototypeTest.unity` (dev-only scene). Production scenes never load it → production sessions never hit the Multiple toggle.
- `NetworkProjectConfig.Global` is a mutable singleton in Fusion 2 (verified: `Assets/Photon/Fusion/Editor/FusionRunnerVisibilityControlsWindow.cs:171` reads it as mutable; weaver `Fusion.CodeGen.cs:3799` treats it as live).
- `OnDestroy()` restore handles the case where dev exits PrototypeTest and opens scene_initial in the same Editor session — without restore, `PeerMode=Multiple` would persist and reproduce the 2026-04-20 / 2026-05-18 production stall.

Verification protocol:
1. Open `PrototypeTest.unity` → Play → expect `[Multipeer-Parity] ✅ PASS #4` log within ~5s, no console errors
2. Stop → open `scene_initial.unity` → Play → expect production flow scene_login → metadata → game_mode → game_map → Map Loaded callback
3. Stop → Play again (without restarting Unity) → expect game_map reached again (proves Single mode is correctly restored)
4. If steps 2+3 fail with stall at scene_game_mode → `OnDestroy` restore did not run; investigate domain-reload / Editor-stop interaction

Commit (at delta-unity, branch `dev`):
- File: `AbilityMultipeerRunner.cs` only — **DO NOT** commit any change to `NetworkProjectConfig.fusion` (any local edit there must be `git checkout`-ed first)
- Suggested message:

```
BUG-0005: AbilityMultipeerRunner toggles PeerMode=Multiple at runtime

Production NetworkProjectConfig.fusion stays PeerMode=Single (matchmaking
depends on this; see commit 6938a52c2d → c7df9c9d05 incident 2026-04-20
where committing Multiple to dev broke production scene_game_mode lobby
join, requiring revert).

The multipeer harness needs Multiple to run two in-process NetworkRunners.
Setting NetworkProjectConfig.Global.PeerMode at runtime in Start() and
restoring Single in OnDestroy() satisfies both: production sessions never
load AbilityMultipeerRunner so they never see Multiple; multipeer dev
sessions get Multiple just for their lifetime.

Verified 2026-05-18 by manual run on operator machine (Tanapol):
- PrototypeTest → Pass #4 ✅
- scene_initial → game_map ✅
- retry without Unity restart → game_map ✅

Closes BUG-0005.
```

### Alternative paths considered (not chosen)

- **A — revert repo to Multiple**: RETRACTED — would recreate the 2026-04-20 production stall the operator already fixed once in `c7df9c9d05`. See "Path A (initially recommended, then RETRACTED)" above.
- **C — two NetworkProjectConfig files + scene-driven swap**: over-engineering. Adds asset-pipeline complexity for a single dev-only scene.
- **D — keep per-developer local config drift (status quo pre-2026-04-20)**: every team member silently runs `PeerMode: 0` locally, the repo state is unreliable. This is what was happening before `6938a52c2d` — a fragile, undocumented workflow. Path B replaces this with explicit code so the repo state is correct and the dev harness is self-contained.

### Probe (diagnostic) — added then removed

A 2-line `Debug.Log` probe was added to `FindActorAbilityComponent` on 2026-05-18 to disambiguate the empty-list vs missing-component case. The git-history bisect identified the root cause before the probe needed to be run, so the probe was reverted in the same session. `AbilityMultipeerRunner.cs` ends 2026-05-18 in its pre-probe state.

### Phase 3 batch 1 impact

BUG-0005 root cause is **PeerMode config / dev harness isolation, not Phase 2 / Phase 3 code path**. AC #6 ("Multipeer harness Pass #1-5 green") in S6-03/04/05/06 is gated only by the operator pulling the `AbilityMultipeerRunner.cs` runtime-toggle fix before running. No story-level AC relaxation needed; no batch 1 schedule impact.

### Process improvement candidates (Sprint 006 retrospective)

1. **`*.fusion` config commit policy**: any commit touching `Assets/Photon/Fusion/Resources/` or `*.fusion` config files should require a 1-paragraph body explaining the change and its scope (production vs dev harness vs both). The 2026-04-20 `6938a52c2d` commit ("Day 2 wrap-up: prototype scene + weaver access fix") quietly bundled a PeerMode flip with prototype work — body didn't mention it, breaking production for 18 days until `c7df9c9d05` fix. The 2026-05-08 `c7df9c9d05` ("Fix PeerMode") had no body at all, so when BUG-0005 surfaced on 2026-05-15 the reviewer had no way to know the commit's intent without bisect. Producer to consider as Sprint 006 retro item.

2. **Local-vs-repo config drift was a chronic invisible cost**: pre-2026-04-20, every team member silently ran `PeerMode: 0` locally while repo said `1`. This drift is invisible until someone commits their local state (`6938a52c2d` did) or someone tries to run the dev harness on a fresh checkout (multipeer Pass #4 was always broken on fresh checkouts). The runtime-toggle pattern in `AbilityMultipeerRunner.cs` is the structural fix; the retro item is to audit other `*.fusion` / `*.asset` config files for similar drift (e.g. `PhotonAppSettings.asset`, `*.unity` scene-level config). Tech debt candidate: TD-008 — "Audit Fusion/Photon config files for committed-vs-local drift, document one source of truth per setting".

3. **Investigation confidence calibration**: my initial confident assertion "Project รัน Multiple มาตลอด ~17 เดือน → safe to revert to Multiple" was wrong because I read commit history but not local-vs-effective state. The operator's correction ("commit พวกนี้เป็น commit จากคนอื่นในทีม บนเครื่องเขาตั้ง single ไว้") was the decisive datum. Process improvement: when investigating any Fusion/Photon config-driven regression, ask the operator "is local state different from repo state?" before recommending a config-level fix. Saved into auto-memory for future sessions.
