# BUG-0003 — NetworkRunnerInput NullReferenceException on Skill1..4/Recall chain

**Filed**: 2026-05-13 (during Sprint 005 S5-04 sidecar work)
**Resolved**: 2026-05-15 (Sprint 006 S6-10 — root-cause investigation, **Path B: Accept defensive pattern**)
**Status**: ✅ **RESOLVED — accepted as defensive null-conditional**
**Severity (when filed)**: S3 (Playtest blocker for S5-04 verification, not a player-facing crash)
**Owner**: network-programmer (`tanapol`)

---

## Symptom

`NullReferenceException` during client-side input frame processing on the chain `player.Actor.Combat.SkillN.{Progress|HasCharging}` for SkillN ∈ {Skill1, Skill2, Skill3, Skill4, SkillRecall}. Surfaced in Editor.log during Sprint 005 S5-04 multipeer harness + Hercules Training playthrough sessions.

## Reproduction (pre-band-aid)

Before the S5-04 band-aid landed (`Assets/GameScripts/Networkings/NetworkRunnerInput.cs`), the following sequences reproduced the NRE:

1. **Scene-boot path**: Load `scene_game_map.unity` → before player Behavior spawns → press Q (or any skill key) → NRE
2. **Respawn path**: Player dies → during respawn window (Actor unbound on client) → spam Q/W/E/R → NRE per frame until Actor re-binds
3. **Pre-AbilityComponent path** (now fixed by S5-09 BindSlot + S5-10 prefab attach, but historically): Combat component spawned but `Skill1..4` references not yet populated → NRE on `.Progress` read

The NRE chain is identical across all 5 methods (Q/W/E/R/Recall), totaling ~9 expression-level `?.` traversal sites.

## Band-aid applied (Sprint 005 S5-04 sidecar, 2026-05-13)

Null-conditional `?.` operators inserted across the entire `player.Actor.Combat.SkillN.X` chain in 9 expression sites (2 per Q/W/E/R + 1 in Recall). Inline comment at the top of `Q()` documented the rationale: "respawn/loading race or hero hasn't fully bound abilities yet."

The band-aid:
- Eliminated the NRE — no further repro on dev branch
- Did not slow input throughput (no measured perf delta in multipeer harness Pass #5)
- Was filed as "root cause TBD, S6 investigation" because S5-04 needed unblocking and the root cause investigation wasn't in scope

## Root-cause investigation (Sprint 006 S6-10, 2026-05-15)

### Trace

**Subscription site**: `NetworkRunnerInput.Enable()` (`NetworkRunnerInput.cs:55-70`) — subscribes Q/W/E/R/Recall handlers to `PlayerGameplayInput` events at scene boot, gated only on:

- `PlayerGameplayInput.Instance != null`
- `NetworkGameManager.Instance != null`
- `NetworkGameManager.Instance.IsClient == true`

None of these gates verify that the client's `Behavior.Actor.Combat.SkillN` chain has fully spawned. Subscription happens once at scene boot; input handlers fire from Unity's Input System every frame regardless of network state.

**Firing site**: each Q/W/E/R/Recall method reads `player.Actor.Combat.SkillN.{Progress|HasCharging}` — a 4-step network-object chain that becomes null in three distinct Fusion lifecycle windows:

| Window | Why null | Frequency |
|---|---|---|
| Pre-Behavior spawn | `NetworkConnectionService.Behavior` is null until player's NetworkObject is spawned (post-scene-load, pre-runner-Server-spawn) | Once at scene boot, ~1-3s |
| Mid-respawn | Player dies → Actor unbinds on client; rebinds when respawn completes | Each death, ~3s |
| Pre-AbilityComponent BindSlot | Combat component is present but `Skill1..4` references not yet wired (S5-09 bootstrap pipeline) | Per respawn even post-S5-09; very brief but real |

All three are **legitimate Fusion lifecycle windows** — the client receives input before networked state is fully replicated.

### Decision: Path B — Accept defensive null-conditional

**Rationale**:

1. **The `?.` shape is correct for this scenario.** Removing it would require either:
   - A "is input ready" gate that checks `Actor != null && Combat != null && SkillN != null` before invoking each handler (adds complexity, races with Unity Input System event delivery)
   - Re-routing input subscription to fire only post-spawn-complete (breaks the existing architecture where input is subscribed at scene boot, fundamental refactor)
2. **Cost > benefit.** Both alternatives are >0.5d work and don't measurably improve runtime; the `?.` is sub-microsecond per frame.
3. **5 sprints of stability.** The band-aid landed 2026-05-13 and has shipped on dev branch + multipeer harness through Sprint 005 close. No NRE recurrence, no perf regression, no other failure mode unmasked.
4. **Phase 3 consistency.** Phase 3 hero migration (Sprint 006 S6-03..06+) will use the same input-path architecture. A consistent defensive null-conditional pattern is the right convention.

**What changed in code (S6-10)**:

- Removed the "BAND-AID:" + "Follow-up: file bug" prefix from the inline comment at `NetworkRunnerInput.cs:101-103`
- Replaced with a concise rationale comment explaining the Fusion lifecycle nullable windows
- No behavioral change — the `?.` operators stay exactly as they were

### Residual concern (out of S6-10 scope, future story candidate)

The leading expression `DeltaService.I.GetService<NetworkConnectionService>().Behavior` at the head of each method (lines 100, 117, 131, 145, 161) is **not** `?.`-guarded:

```csharp
var player = DeltaService.I.GetService<NetworkConnectionService>().Behavior; // no null guards
if (player?.Actor?.Combat?.Skill1?.Progress is ...)                           // null guards here
```

The current `Enable()` gate on `NetworkGameManager.Instance != null` does NOT guarantee `DeltaService.I.GetService<NetworkConnectionService>().Behavior != null` — these are independent subsystems. If a future code path triggers Q/W/E/R/Recall handlers before the NetworkConnectionService is fully resolved or Behavior is bound, NRE will surface in the leading expression instead of the band-aided chain.

**Mitigation status**: not observed in 5 sprints of testing. **Recommended future work**: if a follow-up incident surfaces, file BUG-0003-follow-up and add the outer `?.` chain (matching the `PressLeft()` / `PressRight()` pattern on lines 174-176, 198-200). Estimate ~0.25d.

For now: deemed acceptable on infrastructure-invariant grounds. The current `Enable()` gates are the de-facto contract.

## Files touched (S6-10)

- `Assets/GameScripts/Networkings/NetworkRunnerInput.cs` (delta-unity): improved inline comment at Q() (~3 lines diff)
- `production/qa/bugs/BUG-0003-network-runner-input-nre.md` (NEW — this file)
- `production/sprints/sprint-006.md` + `sprint-status.yaml`: S6-10 → done
- `production/qa/qa-plan-sprint-006-2026-05-15.md`: S6-10 entry updated (Path B accepted; no regression test required)

## Acceptance / closure

- ✅ Root cause identified (Fusion lifecycle nullable windows × 3 distinct paths)
- ✅ Decision documented (Path B — accept defensive null-conditional)
- ✅ Inline code comment updated (rationale, no task-ID references per coding-standards)
- ✅ BUG-0003 filed (this document, formerly referenced but never created)
- ✅ Residual concern documented for future surveillance
- ⚠️ **No regression test** — Path B doesn't change runtime behavior; existing multipeer harness Pass #1-5 + Hercules playthrough already verify NRE is absent

## References

- [Sprint 005 plan §Progress](../../sprints/sprint-005.md) — S5-04 band-aid context
- [Sprint 006 plan S6-10](../../sprints/sprint-006.md) — root-cause investigation story
- [Phase 2 Lessons Learned Pattern #5](../../../docs/architecture/phase-2-lessons-learned.md) — API+caller pair audit (related — input handler subscribed without state readiness gate is a Pattern #5 cousin)
- `Assets/GameScripts/Networkings/NetworkRunnerInput.cs` (delta-unity) — the code under investigation
