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

## TD-010 — Phase 2 dual-path retention spawns duplicate `ActorCombatAction` per Hero ability

- **Origin**: S5-10 (2026-05-14, commit `748ddd410f` attached `AbilityComponent` to `base_avatar.prefab`); visually confirmed during BUG-0006 investigation Sprint 006 Day 4 (2026-05-18)
- **Status**: Documented; deferred to Phase 4 (Sprint 007+)
- **Description**: `ActorCombat.OnStartup` (line 405-415) runs BOTH `CreateSkill()` (legacy, line 301-395) AND `BootstrapSlotBindings()` (Phase 2 S5-09, line 429-455) unconditionally for Hero actors with `AbilityComponent` attached. Each path spawns a separate `ActorCombatAction` `NetworkObject` per ability:
  - **Legacy**: `Runner.SpawnAsync(...)` → orphan (no parent anchor), assigned to `Skill1..4` / `NormalAttack` / `Passive` / `SkillRecall` `NetworkProperties`
  - **Phase 2**: `AbilityComponent.BindSlot()` → `registry.CreateAction(abilityId, anchor: this)` → parented to `base_avatar`, registered in `Slots[]` dictionary
  
  Result: 2 instances of the same ability prefab per Hero, visible in Editor hierarchy (one inside `base_avatar`, one outside).
- **Impact**: **LOW — cosmetic / architectural concern only, NOT a functional bug.** BUG-0006 fix (delta-unity `4ed9a04dda`) confirmed the first-cast no-op was a Unity `AnimationEvent` vs `StateMachineBehaviour` timing race, NOT duplicate spawn. Abilities work correctly despite duplicates.
  
  Memory overhead: ~7 extra `NetworkObject`s per Hero × 10 heroes per match (5v5) = ~70 extras per match — well within Fusion's default `NetworkObject` budget (~5000). Bandwidth overhead is small because legacy spawns share the same prefab + replication metadata as Phase 2 spawns.
- **Detection scope (2026-05-18 audit)**: 672 references to `Combat.Skill1..4` / `NormalAttack` / `Passive` / `SkillRecall` `NetworkProperties` across 75 files (every hero ability file + UI views + `Actor` base classes + Bot logic).
- **Mitigation candidates** (decided during Phase 4 architectural session):
  - **Option A — Gate legacy `CreateSkill` for Hero with `AbilityComponent`**: skip legacy path when Phase 2 has spawned. 672-caller migration required → multi-sprint Phase 4 work. Architecturally correct (Phase 2 was always meant to replace legacy).
  - **Option B — Gate Phase 2 `BindSlot` to reuse legacy action**: blocked by timing issue (`CreateSkill` is async via `SpawnAsync`, `BootstrapSlotBindings` is sync — legacy `Skill1..4` properties are still `null` when `BindSlot` runs). Would require making `OnStartup` async or deferred-bind event pattern. Defeats Phase 2 architecture purpose.
  - **Option C — Despawn duplicate post-init**: requires directional decision (despawn legacy → 672 callers break; despawn Phase 2 → `Slots[]` dangling). Hacky; doesn't fix architectural issue.
  - **Recommended**: Option A as part of formal Phase 4 — retire `SkillKey` enum + 672 caller migration to `GetSlotAction(slot)` + remove legacy `CreateSkill` entirely.
- **Removal target**: **Phase 4 (Sprint 007+)** — formal architectural session: technical-director + lead-programmer decide migration completion criteria + Option A implementation across multi-sprint scope.
- **Phase 4 entry gate**: Phase 3 batch 4 closes (all 15 remaining heroes migrated per [EPIC.md](../production/epics/phase-3-hero-migration/EPIC.md) line 12).
- **Owner**: lead-programmer (Phase 4 driver) / gameplay-programmer (caller migration)
- **References**:
  - [BUG-0006](../production/qa/bugs/BUG-0006-hercules-e-first-cast.md) — duplicate spawn was the late-EOD-2 hypothesised root cause, ruled out by actual fix at delta-unity `4ed9a04dda`. Investigation audit (672 callers) lives in BUG-0006 commit history.
  - [Phase 3 hero migration EPIC](../production/epics/phase-3-hero-migration/EPIC.md) line 12 — plans Phase 4 retirement of dual-path
  - ADR-0006 §10 — Phase 2 → Phase 3 handover criteria (forward-handover gate to Phase 4 from Phase 3 close)

---

## TD-011 — Passive ability sibling-skill lookup hardcodes `Actor.Combat.Skill1..4` index (Pattern 6 promotion)

- **Origin**: S6-04 (2026-05-19) — surfaced during Phase 3 batch 1 Volund migration code audit; **2nd confirmation case** (first was HorusI in S6-03 2026-05-18). Reaches promotion threshold defined in [phase-2-lessons-learned.md § Pattern 6](architecture/phase-2-lessons-learned.md). **3rd case** added 2026-05-19 (S6-05 GuanYu audit — GuanYuR active ability, not passive); confirms Pattern 6 scope extends beyond passive-only. **4th case + new sub-shape** added 2026-05-19 (S6-06 Skadi audit — service-hub variant; 6 instances across SkadiQ/W/E; broke "clean baseline" expectation).
- **Severity**: **LOW** — code-smell, not a bug. Functionally correct under current convention (Q=Skill1, W=Skill2, E=Skill3, R=Skill4). Risk surfaces only if slot order is ever rebound dynamically (item swap, future hero variants with non-standard layout).
- **Description**:
  Four heroes use sibling-skill direct reads via `Actor.Combat.Skill1..4` index property, which represents the legacy "slot = position in array" assumption that Phase 2 (ADR-0008) specifically migrated away from. Pattern surfaces in **3 distinct sub-shapes**: (1) passive cooldown read/write, (2) passive/active event subscription, and (3) service-hub method invocation via type-coupling. Confirmed cases:

  | Hero | File | Lines | Ability type | Sub-shape | Operations |
  |---|---|---|---|---|---|
  | HorusI | `HorusIAction.cs:79-82` | 4 | Passive (I) | Cooldown read/write | Read `Skill1.Rank` / `IsMainCooldown` / `RemainingMainCooldown` + write `MainCooldown` (passive reduces Q CD on normal-attack hit) |
  | VolundI | `VolundIAction.cs:28-30` | 3 | Passive (I) | Event subscription | Subscribe `Skill1/2/4.OnHitTarget +=` event (passive applies stack on Q/W/R hit) |
  | GuanYuR | `R/GuanYuRAction.cs:24` | 1 | **Active (R)** | Event subscription | Subscribe `Skill3.OnHitLockTarget += ApplyStatusEffect` (R's Initialize sets up CHALLENGER status on E hit) |
  | **SkadiQ + SkadiW + SkadiE** | `SkadiQAction.cs:19`, `SkadiWAction.cs:18`, `SkadiEAction.cs:22-23,31-32` | **6** | **Active (Q/W/E)** | **Service-hub via type-coupling** | (a) Q hit calls `Actor.Combat.Skill3.GetComponent<SkadiEAction>()?.PassiveCooldown1(target)`; (b) W hit calls `.PassiveCooldown2(target)`; (c) E's methods write back to `Actor.Combat.Skill1.MainCooldown` and `Actor.Combat.Skill2.MainCooldown`. Q/W → E (via Skill3 slot index + `GetComponent` type cast) → Q/W (cooldown write-back via Skill1/2). Circular cross-ability dependency. |

- **Proposed solution**:
  Add `ActorCombat.GetSkillBySlot(Slot slot)` API returning the `ActorCombatAction` bound to that slot role (null if no skill bound). Migrate both confirmed cases to use the API; the slot enum (Q/W/E/R/I) is the source of truth, not array index.

  ```csharp
  // Proposed (lives in ActorCombat or wherever Skill1..4 are defined)
  public ActorCombatAction GetSkillBySlot(Slot slot);  // null if no skill bound to that slot
  ```

- **Impact**:
  Architectural completeness — closes the last remaining "Phase 1 indexing" anti-pattern that Phase 2 didn't address (Phase 2 focused on input→slot routing and animation-event slot routing; sibling-skill lookup was overlooked because Hercules pilot had no passive of this shape).

  No runtime behavior change expected post-migration. Cosmetic / architectural concern only.

- **Cross-hero scan needed before story authoring**:
  ```bash
  grep -rn "Actor.Combat.Skill[1-4]" C:/GitHub/delta-unity/Assets/GameScripts/Gameplays/Characters/
  ```
  Likely cousins: Anansi passive, Cupid passive, Merlin 3-stance passive. (GuanYu already audited — GuanYuR confirmed, others clean.) Result determines scope of Pattern 6 migration story (1-2 heroes = small story; 5+ heroes = epic).

  **Updated scope expectation**: Pattern 6 now confirmed in both passive AND active ability contexts AND service-hub patterns → `GetSkillBySlot(Slot)` API must work from any ability class context (not just `*IAction.cs`).

  **Service-hub sub-shape implications** (added 2026-05-19 after S6-06 Skadi audit):

  The Skadi case is more refactor-intensive than the other sub-shapes because:
  - Q/W → E call uses BOTH `Skill3` slot index AND `GetComponent<SkadiEAction>()` type cast → migration must replace both the slot lookup AND consider whether `GetSkillBySlot` returns the right type or requires an `as` cast
  - E acts as a "service hub" exposing public methods (`PassiveCooldown1`, `PassiveCooldown2`) → if these are intentional design (service pattern), the migration should preserve them; if they're an emergent shape (one-off helper), they can be inlined or moved
  - Circular dependency: Q→E→Q (cooldown). Refactor must not introduce stale-state read/write order issues

  **Recommended `GetSkillBySlot` API extension for service-hub case**:

  ```csharp
  public ActorCombatAction GetSkillBySlot(Slot slot);
  public T GetSkillBySlot<T>(Slot slot) where T : ActorCombatAction;  // typed variant for service-hub case
  ```

- **Removal target**: **Sprint 007 or Sprint 008** — dedicated Pattern 6 migration story. Author after cross-hero scan completes. Story creation criteria:
  - 4 confirmed cases (HorusI + VolundI + GuanYuR + Skadi-Q/W/E) — 14 total instances across batch 1 alone ≥ threshold ✅
  - Cross-hero scan reveals batch 2/3/4 scope (small story vs epic)
  - `GetSkillBySlot` API design reviewed by lead-programmer (decide static-helper vs instance method; decide where `Slot` enum lives; decide whether typed variant `GetSkillBySlot<T>` is added for service-hub cases like Skadi)
  - API must support 3 sub-shapes:
    - **Read-style** (HorusI cooldown reads): `qSkill.Rank`, `qSkill.MainCooldown = …`
    - **Event-subscription style** (VolundI + GuanYuR): `qSkill.OnHitTarget += …` / `qSkill.OnHitLockTarget += …`
    - **Service-hub style** (Skadi): typed access to call hero-specific public methods, e.g. `(eSkill as SkadiEAction)?.PassiveCooldown1(target)`

- **Owner**: lead-programmer (API design) + gameplay-programmer (per-hero migration)

- **References**:
  - [Pattern 6 documentation](architecture/phase-2-lessons-learned.md) — full pattern shape + proposed API
  - [BUG-0009](../production/qa/bugs/BUG-0009-horus-q-stale-target-nre.md) — separate concern (BUG-0009 was about HorusQ stale `Target`, not Pattern 6; both lived in Horus files but are distinct issues)
  - [Phase 3 batch 1 evidence](../production/qa/evidence/sprint-006-phase-3-batch1.md) — all four surfacings (HorusI 2026-05-18, VolundI 2026-05-19, GuanYuR 2026-05-19, Skadi-Q/W/E 2026-05-19)
  - [Phase 3 hero migration EPIC](../production/epics/phase-3-hero-migration/EPIC.md) — context for batch ordering / when Pattern 6 story fits

---

## Conventions
- IDs are permanent (TD-NNN). Never renumber.
- Remove entries when fixed; record removal in commit message.
- Severity scale: LOW (deferred OK), MEDIUM (next sprint), HIGH (this sprint).
