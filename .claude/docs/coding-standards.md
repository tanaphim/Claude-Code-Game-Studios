# Coding Standards

- All game code must include doc comments on public APIs
- Every system must have a corresponding architecture decision record in `docs/architecture/`
- Gameplay values must be data-driven (external config), never hardcoded
- All public methods must be unit-testable (dependency injection over singletons)
- Commits must reference the relevant design document or task ID
- **Verification-driven development**: Write tests first when adding gameplay systems.
  For UI changes, verify with screenshots. Compare expected output to actual output
  before marking work complete. Every implementation should have a way to prove it works.

# Design Document Standards

- All design docs use Markdown
- Each mechanic has a dedicated document in `design/gdd/`
- Documents must include these 8 required sections:
  1. **Overview** -- one-paragraph summary
  2. **Player Fantasy** -- intended feeling and experience
  3. **Detailed Rules** -- unambiguous mechanics
  4. **Formulas** -- all math defined with variables
  5. **Edge Cases** -- unusual situations handled
  6. **Dependencies** -- other systems listed
  7. **Tuning Knobs** -- configurable values identified
  8. **Acceptance Criteria** -- testable success conditions
- Balance values must link to their source formula or rationale

# Testing Standards

## Test Evidence by Story Type

All stories must have appropriate test evidence before they can be marked Done:

| Story Type | Required Evidence | Location | Gate Level |
|---|---|---|---|
| **Logic** (formulas, AI, state machines) | Automated unit test — must pass | `tests/unit/[system]/` | BLOCKING |
| **Integration** (multi-system) | Integration test OR documented playtest | `tests/integration/[system]/` | BLOCKING |
| **Visual/Feel** (animation, VFX, feel) | Screenshot + lead sign-off | `production/qa/evidence/` | ADVISORY |
| **UI** (menus, HUD, screens) | Manual walkthrough doc OR interaction test | `production/qa/evidence/` | ADVISORY |
| **Config/Data** (balance tuning) | Smoke check pass | `production/qa/smoke-[date].md` | ADVISORY |

## Automated Test Rules

- **Naming**: `[system]_[feature]_test.[ext]` for files; `test_[scenario]_[expected]` for functions
- **Determinism**: Tests must produce the same result every run — no random seeds, no time-dependent assertions
- **Isolation**: Each test sets up and tears down its own state; tests must not depend on execution order
- **No hardcoded data**: Test fixtures use constant files or factory functions, not inline magic numbers
  (exception: boundary value tests where the exact number IS the point)
- **Independence**: Unit tests do not call external APIs, databases, or file I/O — use dependency injection

## What NOT to Automate

- Visual fidelity (shader output, VFX appearance, animation curves)
- "Feel" qualities (input responsiveness, perceived weight, timing)
- Platform-specific rendering (test on target hardware, not headlessly)
- Full gameplay sessions (covered by playtesting, not automation)

## CI/CD Rules

- Automated test suite runs on every push to main and every PR
- No merge if tests fail — tests are a blocking gate in CI
- Never disable or skip failing tests to make CI pass — fix the underlying issue
- Engine-specific CI commands:
  - **Godot**: `godot --headless --script tests/gdunit4_runner.gd`
  - **Unity**: `game-ci/unity-test-runner@v4` (GitHub Actions)
  - **Unreal**: headless runner with `-nullrhi` flag

# Asset Naming Conventions

## Scene Files

**Rule**: Scene files use `lowercase_snake_case` and follow this prefix convention:

| Prefix | Purpose | Example |
|---|---|---|
| `scene_` | Production gameplay scenes | `scene_game_map.unity`, `scene_home_lobby.unity` |
| `test_scene_` | Manual test / dev-only scenes (not shipped) | `test_scene_ability_multipeer.unity` |
| `prototype_` | Throwaway prototype scenes (under `prototypes/`) | `prototype_combat_loop.unity` |

**Story planning gate**: When a sprint plan or story specifies "create new scene X.unity":

- **Default behavior** = create the new scene (do not assume the user has one in mind)
- **Exception** = if the story name or context implies modifying an existing scene (e.g. "polish HUD in scene_game_map"), grep for `scene_*.unity` first to confirm target — if multiple candidates match, ask the user before creating a new one
- **Naming approval**: scene names use the prefix table above; if a new prefix is needed, raise it in the sprint plan before the story starts

**Origin**: Sprint 003 retrospective action #3. Sprint 003 story "create new scene for X" produced ambiguity ("did the user already have a scene? should I create one?"). The default-to-create + grep-first-on-modify rule eliminates that round-trip.

## Other Assets

Standard PascalCase per [technical-preferences.md](technical-preferences.md):

- Prefabs / scriptable assets: `PascalCase.prefab`, `PascalCase.asset`
- Animator controllers / animation clips: `PascalCase.controller`, `PascalCase.anim`
- Materials / shaders: `PascalCase.mat`, `PascalCase.shader`

The lowercase_snake_case rule is **scenes-only**, because scenes are discoverable via Unity's hierarchy panel and the snake_case is friendlier in command-line and CI log contexts.
