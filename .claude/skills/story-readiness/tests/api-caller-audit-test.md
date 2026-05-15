# /story-readiness — API+Caller Pair Audit Test Cases

**Purpose**: Verify the API+Caller Pair Audit checklist item (introduced 2026-05-15, S6-09) fires correctly — catching phantom-caller stories like S5-06/TD-006, while not producing false positives on stories where the AC's callee is in-scope or self-contained.

**Test format**: Each case below is a synthetic story snippet + the expected `/story-readiness` verdict for the API+Caller Pair Audit checklist item specifically. (Other checklist items are out of scope for this test — the audit gate is the only thing being validated.)

---

## Case 1 — Phantom callee (expected: BLOCKED)

### Synthetic story

```markdown
# Story: S6-T01 — Slot binding helper for migrated heroes

**Type**: Logic
**ADR**: ADR-0006 §6.2
**Estimate**: 0.25d

## Acceptance Criteria

1. `AbilityComponent.BindSlot(byte slot, string abilityId)` method exists
   and registers the slot mapping.
2. **`ActorCombat.OnStartup` calls `AbilityComponent.BindSlot(slot, id)` ×6
   for Q/W/E/R/A/I slots** (caller is wired in prior story S6-T00).
3. EditMode test verifies BindSlot survives consolidation.
```

### Expected gate behavior

- **Trigger detected**: AC #2 names callee (`AbilityComponent.BindSlot`) AND expected caller (`ActorCombat.OnStartup`, "prior story S6-T00")
- **Procedure**: grep `Assets/GameScripts/.../ActorCombat.cs` for `BindSlot(` invocation
- **Outcome (assuming S6-T00 not landed)**: no caller found → **BLOCKED**
- **Verdict text**:
  > "Phantom-caller: `AbilityComponent.BindSlot` is declared but no invocation site found in `ActorCombat.OnStartup` (named caller). Story AC #2 assumes prior story S6-T00 already wired this. Either (a) expand this story to include the OnStartup wiring, or (b) confirm S6-T00 has landed before assigning this story."

### Why this matters

This is the exact S5-06 / TD-006 anti-pattern: AC said "caller is wired" but the wiring story was never landed. The gate would have caught it and prevented the 40-shim migration revert.

---

## Case 2 — Caller in same story scope (expected: PASS)

### Synthetic story

```markdown
# Story: S6-T02 — Add SetActiveSlot wiring in Progress setter

**Type**: Logic
**ADR**: ADR-0006 §6.2
**Estimate**: 0.4d

## Acceptance Criteria

1. **`Actor.Combat.SetActiveSlot(slot)` is called from the `Progress` setter
   in `ActorCombatAction`** when ability transitions to a non-None state.
2. Dual-path: BoundSlot first, else SkillKey→slot map fallback.
3. EditMode test for ResolveSlotFromSkillKey static helper.
```

### Expected gate behavior

- **Trigger detected**: AC #1 names callee (`SetActiveSlot`) AND caller (`Progress` setter in `ActorCombatAction`)
- **Procedure**: identify that the caller is what this story is ADDING (not assumed to exist)
- **Outcome**: PASS — the wiring is the story's own work scope, not a phantom prerequisite
- **Verdict text**:
  > "API+Caller pair: `SetActiveSlot` ← `ActorCombatAction.Progress` setter. The caller is this story's own scope (the developer will add the call as part of AC #1). No phantom-caller risk."

### Why this matters

Avoids false positives. Without this distinction the gate would block stories that legitimately introduce new wirings (most refactor stories). Distinguish "AC asserts a prior wiring" vs "AC asserts work to be done in this story".

---

## Case 3 — No cross-module wiring claim (expected: AUTO-PASS)

### Synthetic story

```markdown
# Story: S6-T03 — Tune Hercules Q damage curve

**Type**: Config/Data
**ADR**: N/A — pure balance tuning
**Estimate**: 0.1d

## Acceptance Criteria

1. CBS Hercules Q `BaseDamage` field changes from 80 to 95 at rank 1.
2. Damage curve verified in-game: rank 1=95, rank 2=110, rank 3=125.
3. Smoke check confirms ability still casts without console errors.
```

### Expected gate behavior

- **Trigger detected**: none — no AC names a cross-module caller relationship
- **Outcome**: AUTO-PASS — checklist item skipped
- **Verdict text**:
  > "API+Caller pair: N/A — no cross-module wiring asserted in AC."

### Why this matters

Config/Data and quick-design stories rarely involve cross-module wiring. The gate must not produce noise on these (which would erode trust in the skill).

---

## Case 4 — Callee doesn't exist either (expected: falls through to OTHER gaps, NOT phantom-caller)

### Synthetic story

```markdown
# Story: S6-T04 — Use AbilityScheduler.QueueCast in cooldown stories

**Type**: Logic
**ADR**: ADR-future
**Estimate**: 0.5d

## Acceptance Criteria

1. **`ActorCombatAction.UseSkill` calls `AbilityScheduler.QueueCast(ability)`**
   to queue casts during cooldown windows.
2. EditMode test verifies queue ordering.
```

### Expected gate behavior

- **Procedure**: grep for `AbilityScheduler.QueueCast` — callee declaration not found anywhere
- **Outcome**: This is NOT a phantom-caller (callee doesn't exist), it's a missing-prerequisite gap
- **Verdict text** (this audit item):
  > "API+Caller pair: callee `AbilityScheduler.QueueCast` not found in codebase. This is a missing-prerequisite gap, not a phantom-caller. (See other checklist items — ADR Accepted check, GDD requirement check — for proper handling. Falls through here.)"
- **Other gates may produce**: NEEDS WORK / BLOCKED via ADR-not-Accepted or missing GDD requirement

### Why this matters

The audit gate only catches the specific phantom-caller pattern. Other gaps (missing prerequisite stories, undefined APIs) are caught by existing checklist items. This case verifies the audit doesn't overreach.

---

## Case 5 — Vague prose mention (expected: AUTO-PASS, no trigger)

### Synthetic story

```markdown
# Story: S6-T05 — Refactor combat input handler

**Type**: Logic
**Estimate**: 0.3d

## Context

We need to refactor the input handler. The old code uses `Buttons.IsSet`
checks throughout `ActorCombatAction`; we want to consolidate on the
slot-based API.

## Acceptance Criteria

1. All 5 sites in `ActorCombatAction` that use `Buttons.IsSet` are replaced
   with `BoundSlot == PressedSlot` check.
2. EditMode tests pass.
```

### Expected gate behavior

- **Trigger detected**: AC #1 mentions methods/properties but NOT a cross-module wiring assertion ("X is wired in Y" / "method M is invoked from Z by prior story")
- **Outcome**: AUTO-PASS — same-module refactor, not a phantom-caller pattern
- **Verdict text**:
  > "API+Caller pair: N/A — AC describes in-module substitution, not cross-module wiring assertion."

### Why this matters

Distinguishes "the story modifies an existing call site" from "the story assumes a caller has already been wired". The former is the dev's normal work; the latter is the phantom-caller risk.

---

## Manual run protocol

When this skill is updated, run these 5 cases manually:

1. Create each synthetic story as a temp file (e.g. `tests/fixtures/S6-T01-phantom.md`)
2. Run `/story-readiness tests/fixtures/S6-T01-phantom.md`
3. Verify the API+Caller pair audit checklist item produces the expected verdict
4. Repeat for cases 2-5
5. Delete temp files

If any case produces an unexpected verdict, file a skill bug — the audit logic in `SKILL.md` needs adjustment (likely the trigger-detection heuristic).

---

## Trigger keywords reference

The audit checklist item is triggered by AC text containing any of these patterns:

- "wired in [module/method]"
- "called from [module/method]"
- "invoked from [module/method]"
- "[caller] calls [callee]"
- "[caller] consumes [callee]'s [property]"
- "method [name] from prior story"
- "as set up in [prior story]"
- explicit reference to "previous story" / "prior story" / story IDs (e.g., "S5-03") combined with a callee name

These are heuristics, not exhaustive — the skill should err toward asking the user when ambiguous rather than producing false positives.

---

## Changelog

- **2026-05-15** — Initial 5 test cases authored as part of Sprint 006 S6-09. Cases cover: phantom callee (BLOCKED), in-scope caller (PASS), no wiring claim (AUTO-PASS), missing callee (falls through), vague prose (AUTO-PASS).
