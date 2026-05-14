# Tech Debt Register

Living document tracking known shortcuts, deferred work, and architectural drift.
Each entry: ID, origin, description, impact, removal target, owner.

---

## TD-001 — ADR-0006 §5.3 `IsLeveled` misquote (Pattern C)

- **Origin**: S5-05 (2026-05-13)
- **Status**: Documented inline; cleanup deferred to Phase 3
- **Description**: ADR-0006 §5.3 sample code reads `if (!AbilityData.IsLeveled) return;` but `IsLeveled` lives on `AbilityDataSnapshot` (different class from `AbilityData` / `CBSAbility`). S5-05 implementation uses concrete `AbilityData.MaxRank <= 1` which is semantically equivalent per ADR's claim (A/Item/Recall have `cbs.MaxRank == 1`).
- **Impact**: LOW — equivalence holds for current CBS data. If a designer ever sets `MaxRank > 1` on an A/Item/Recall ability OR `MaxRank == 1` on a Q/W/E/R ability, behaviour diverges silently from the pre-S5-05 baseline.
- **Mitigation candidate**: Add `Debug.Assert` tripwire in `CheckEmpower` to surface misconfigured CBS in editor (rejected for S5-05 — defer to Phase 3 SkillKey deletion pass).
- **Removal target**: Phase 3 — when `SkillKey` enum is deleted and ADR-0006 is amended.
- **Owner**: gameplay-programmer

## TD-002 — Pattern B4 semantic shift: PressedSlot (just-pressed) vs Buttons.IsSet (held)

- **Origin**: S5-05 (2026-05-13)
- **Status**: Documented inline at `ActorCombatAction.cs:2078` (bot input path); behaviour verified clean in Training playthrough
- **Description**: Original `GetInputBot` used `input.Buttons.IsSet(Buttons.Q)` — true while button is **held**. New slot-bound path uses `input.PressedSlot != 0 && input.PressedSlot == BoundSlot` — true only for the frame the input was **just pressed**. Bot input pipeline populates both consistently after the S5-05 `BotActor.Auto/WithTarget` fix (`msg.PressedSlot = skill.BoundSlot`), so observable behaviour matches.
- **Impact**: LOW — only matters if a future change desyncs `PressedSlot` from `Buttons.IsSet` in the bot input pipeline. Legacy heroes (`BoundSlot == 0`) continue to use `IsSet` (held).
- **Mitigation candidate**: Migrate all heroes to BoundSlot path so the legacy branch can be deleted (Phase 3).
- **Removal target**: Phase 3 — SkillKey enum deletion + Hercules migration cleanup.
- **Owner**: gameplay-programmer

## TD-007 — `AbilityRegistry` not registered with `DeltaService` in production scene boot

- **Origin**: S5-10 (2026-05-14) — surfaced during Hercules Training playthrough in `scene_game_map.unity`
- **Status**: ✅ **RESOLVED** (2026-05-14, delta-unity@427629ea0d on `claude/s5-10-hercules-playtest`, pre-merge of PR #353)
- **Description**: S5-09 wired `ActorCombat.OnStartup` to call `AbilityComponent.BindSlot(slot, abilityId)` ×7 (Q/W/E/R/A/I/Recall) for Hero spawns. The new pipeline requires `AbilityRegistry` to be registered with `DeltaService` **before** Hero spawn. In production scene flow (`scene_game_map.unity`), this registration never happened — root cause: `AbilityRegistry` is a `DeltaBaseService` (`MonoBehaviour`) but was never added to `DeltaConfiguration.Services` list, so `DeltaService.RegisterService()` never instantiated it. Observed pre-fix: 14 warnings per match (2 hero spawns × 7 slots).
- **Impact (pre-fix)**: MEDIUM — Game was playable via S5-09 legacy `CreateSkill` dual-path fallback; new slot-binding pipeline (Phase 2 §6.1 / ADR-0008) was dormant in production. ADR-0006 §3 Phase 2 Exit Criterion #5 was satisfied at API level only, not end-to-end.
- **Resolution**: Created `Assets/Resources/Prefabs/Data/Services/AbilityRegistryService.prefab` (empty GameObject + `AbilityRegistry` MonoBehaviour, modeled on `KeybindMapService.prefab`) via Unity Editor, then added to `DeltaConfiguration.Services` list. `DeltaService.Awake()` → `RegisterService()` now instantiates the service at app boot, before any Hero `OnStartup` fires. Verified by user playtest: 14 BindSlot warnings → 0; Hercules QWER + A + Recall + Item still functional. **Phase 2 Exit Criterion #5 now fully satisfied end-to-end** — production scene exercises S5-09 BindSlot pipeline (was previously legacy-fallback-only).
- **Lessons learned**: 3rd surfacing this sprint of the "ADR specifies API + caller pair; only API lands" anti-pattern. Worth codifying in `control-manifest.md` per Sprint 005 retro.
- **Owner**: gameplay-programmer

## TD-006 — `ActorCombat.SetActiveSlot()` has no caller

- **Origin**: S5-06 (2026-05-13) — surfaced by manual VFX playtest after initial implementation passed CR + 44/44 EditMode
- **Status**: ✅ **RESOLVED** (2026-05-14, S5-21, delta-unity@claude/s5-21-setactiveslot-wiring)
- **Description**: ADR-0006 §6.2 specified that `ActorCombat.SetActiveSlot(byte slot)` would be "Updated via SetActiveSlot from ActorCombatAction input handlers when an ability triggers". S5-03 (P2-02) landed the API but the caller was never wired. As a result `m_ActiveSlot` stayed 0 for the entire match on every hero. Discovered during S5-06 playtest: VFX/SFX animation events were silently dropped.
- **Resolution**: S5-21 added `ResolveSlotFromSkillKey(SkillKey) → byte` static helper (8 cases: Q/W/E/R/A/I/Recall + Item→0) and wired `Actor.Combat.SetActiveSlot(slot)` into `ActorCombatAction.Progress` setter (line 402). Dual-path: BoundSlot if migrated (S5-04), else legacy SkillKey→slot map. After v1 attempt placed the call in `OnPressButtons` and broke Normal Attack (auto-target bypasses OnPressButtons by setting Progress=Attack1 directly at lines 2693/2770), v2 moved the call into the Progress setter — the single source of truth for state transitions. 40 AnimationEvent shims re-landed atomically. 116/116 EditMode tests pass; user-verified production playtest clean (NA damage restored, 0 "slot=0" warnings).
- **Lessons learned**: When wiring is "called from N entry points", instrument at the central state-transition point (here: Progress setter), not at one of the entry points. v1's mistake was assuming all ability activations go through OnPressButtons — auto-attack doesn't.
- **Owner**: gameplay-programmer

## TD-003 — Multi-skill chain race in `AnimationEvent.GetActiveSlot()` routing

- **Origin**: S5-06 (2026-05-13) — code-review unity-specialist F3
- **Status**: Accepted as known edge case; behaviour gate falls to S5-10 multipeer + playthrough
- **Description**: `GetActiveSlot()` returns the most-recently-set slot. If Q's animation is still playing an Empower frame when the player presses W, `m_ActiveSlot` is already 2 (W) → the Q animation event fires into W's slot. This is a pre-existing race not introduced by S5-06 (the legacy `StateRelease(SkillKey.Q, ...)` path had the same exposure via different mechanism), but it becomes more visible under slot-bound routing.
- **Impact**: LOW–MEDIUM — animation events typically fire on dedicated single-skill frames; chained-cast overlap is rare in practice. Worst case: visual/audio event triggers on wrong slot's callback (no gameplay state corruption since `StateRelease` itself is keyed correctly via resolved SkillKey).
- **Mitigation candidate**: Stamp slot ID on animator clip via parameter binding (Phase 3) OR track `m_ActiveSlot` as a per-skill stack instead of single slot.
- **Removal target**: Phase 3 — when SkillKey enum is retired and animator events carry explicit slot tags.
- **Owner**: gameplay-programmer

## TD-004 — `default(SkillKey)` true-path test gap in `TryResolveSlotRoute`

- **Origin**: S5-06 (2026-05-13) — code-review qa-tester Gap 3
- **Status**: Documented; trivial follow-up test
- **Description**: `AnimationEvent.TryResolveSlotRoute` happily returns `true` and passes `default(SkillKey)` through when all three booleans are true but `abilitySkillKey == default`. This is a theoretically reachable state (CBS data race during load) that no current test asserts. Downstream `StateRelease(SkillKey.None, ...)` behaviour is untested at this layer.
- **Impact**: LOW — `StateRelease` looks up `m_OnReleaseDictionary[SkillKey.None]` which will return null, leading to silent drop. No crash. But silent drops bury real bugs.
- **Mitigation candidate**: Add 1-line `[Test] test_TryResolveSlotRoute_DefaultSkillKey_AllConditionsTrue_ReturnsTrueWithDefault` per qa-tester recommendation.
- **Removal target**: Sprint 005 retro or Sprint 006 polish — 5-min task, fold into next test-touch PR.
- **Owner**: gameplay-programmer

## TD-005 — `Actor == null` pre-`Spawned()` null-guard in `StateReleaseSlot`

- **Origin**: S5-06 (2026-05-13) — code-review qa-tester Gap 4
- **Status**: Accepted as known limitation; defensive guard would prevent a class of pre-`Spawned()` NREs
- **Description**: All 40 migrated shim methods call `Actor.Combat.GetActiveSlot()` directly. If an animator event fires before `Spawned()` sets up the `Actor` reference, this NREs before `TryResolveSlotRoute` is even entered. Not testable in EditMode (requires Fusion runtime + animator-event timing). Multipeer harness + Hercules playthrough are the gate.
- **Impact**: LOW — Fusion lifecycle normally guarantees `Spawned()` runs before animator events. Risk window is narrow (asset import / scene unload edge cases).
- **Mitigation candidate**: Add `if (Actor == null) return;` at top of `StateReleaseSlot` (Phase 3 cleanup, or inline if surfaced in S5-10).
- **Removal target**: Phase 3, or earlier if multipeer reveals the NRE.
- **Owner**: gameplay-programmer

---

## Conventions
- IDs are permanent (TD-NNN). Never renumber.
- Remove entries when fixed; record removal in commit message.
- Severity scale: LOW (deferred OK), MEDIUM (next sprint), HIGH (this sprint).
