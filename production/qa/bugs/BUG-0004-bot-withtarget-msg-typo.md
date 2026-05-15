# BUG-0004 — `BotActor.WithTarget` + `ActorDungeon.WithTarget` discard `msg2` (typo: `GetInputBot(msg)` should be `GetInputBot(msg2)`)

**Filed**: 2026-05-13 (during Sprint 005 S5-05 code review — flagged out-of-scope)
**Resolved**: 2026-05-15 (Sprint 006 S6-B1)
**Status**: ✅ **RESOLVED — typo fix landed in both sites**
**Severity**: S3 (silent feature loss for bot two-step skills; not a crash, not a S1/S2 player-facing bug)
**Owner**: gameplay-programmer (`tanapol`)

---

## Symptom

In bot input pipeline's `WithTarget(skill, key)` method, a two-step input sequence is constructed:
1. `msg` — "Open Skill" with `Buttons.Set(key, true)` + `PressedSlot = skill.BoundSlot`
2. `msg2` — "Use Skill" with `Buttons.Set(LeftClick, true)` + `Target = target`

The second call was `skill.GetInputBot(msg)` instead of `skill.GetInputBot(msg2)`, **silently discarding** the LeftClick + Target setup. Bots running skills that require the two-step (open → use) pattern would never actually fire the second step's target acquisition — they'd accidentally re-fire the open step.

## Reproduction

A bot using an `ActorCombatAction` skill whose `GetInputBot` reads `LeftClick` + `Target` from the second input frame will:
- Receive a duplicated "open" message (msg sent twice)
- Never receive the target acquisition message
- Silently fail to complete the cast

Observable in playtest as "bot starts charging skill but never releases on target". Hard to spot in normal gameplay because most bot logic uses the `Auto()` path (single-step), not `WithTarget()` two-step.

## Root cause

Plain variable-name typo. The code is structurally correct (msg2 is created, set up, then a call follows), but the call argument is `msg` instead of `msg2`. Likely a copy-paste from `Auto()` (which only uses one msg) that wasn't renamed.

**Two affected files** (the same typo, copy-pasted):

| File | Line | Method |
|---|---|---|
| `Assets/GameScripts/Gameplays/Characters/BotAvatar/BotActor.cs` | 716 | `WithTarget(ActorCombatAction, Buttons)` |
| `Assets/GameScripts/Gameplays/Characters/Actors/ActorDungeon.cs` | 401 | `WithTarget(ActorCombatAction, Buttons)` |

Sprint 005 S5-05 code review found only the BotActor occurrence; the cousin ActorDungeon site was uncovered during S6-B1 investigation 2026-05-15.

## Fix

Single-character change at each site (`msg` → `msg2` in the second `skill.GetInputBot(...)` call):

```diff
                 // Use Skill
                 InputMessage msg2 = new InputMessage();
                 msg2.Buttons.Set(Buttons.LeftClick, true);
                 msg2.Target = target;
-                skill.GetInputBot(msg);
+                skill.GetInputBot(msg2);
```

## Test plan

- ✅ Code review: 1-character fix, no logic change. Two identical fixes at the two sites.
- ⚠️ Manual playtest (user-side): observe a bot using a two-step skill (e.g., a skill that opens with a button press and uses with a target click). After fix, bot should complete the cast on target instead of stalling.
- ⚠️ Multipeer harness regression: existing Pass #1-5 in `production/qa/evidence/S5-10-multipeer.txt` were passing on the buggy code (because bot path was working via `Auto()` for most cases). Re-run is advisory only — fix doesn't regress; it activates a previously dormant path.
- **No EditMode test added** — the typo was in input-construction glue code without a unit-testable surface. Adding a test would require mocking `ActorCombatAction.GetInputBot` and asserting the second invocation receives a `msg2`-shaped struct — over-engineering for a 1-char fix. If a future story tightens bot input testing, this site is a good fixture target.

## Files touched (S6-B1)

- `Assets/GameScripts/Gameplays/Characters/BotAvatar/BotActor.cs` (delta-unity): 1-char fix at line 716
- `Assets/GameScripts/Gameplays/Characters/Actors/ActorDungeon.cs` (delta-unity): 1-char fix at line 401
- `production/qa/bugs/BUG-0004-bot-withtarget-msg-typo.md` (NEW — this file)
- `production/sprints/sprint-006.md` + `sprint-status.yaml`: S6-B1 → done

## Out of scope

- Refactoring the two-step pattern (Open + Use) into a helper to prevent future typos — scope creep
- Unifying `BotActor.WithTarget` and `ActorDungeon.WithTarget` (clear copy-paste) — Phase 3+ refactor candidate
- EditMode test fixture for bot input construction — future tooling story

## References

- [Sprint 005 plan §Progress](../../sprints/sprint-005.md) — S5-05 code review BUG-0004 filing note
- [Sprint 006 plan S6-B1](../../sprints/sprint-006.md) — fix story
- `Assets/GameScripts/Gameplays/Characters/BotAvatar/BotActor.cs` (delta-unity)
- `Assets/GameScripts/Gameplays/Characters/Actors/ActorDungeon.cs` (delta-unity)
