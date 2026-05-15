# Agent Coordination Rules

1. **Vertical Delegation**: Leadership agents delegate to department leads, who
   delegate to specialists. Never skip a tier for complex decisions.
2. **Horizontal Consultation**: Agents at the same tier may consult each other
   but must not make binding decisions outside their domain.
3. **Conflict Resolution**: When two agents disagree, escalate to the shared
   parent. If no shared parent, escalate to `creative-director` for design
   conflicts or `technical-director` for technical conflicts.
4. **Change Propagation**: When a design change affects multiple domains, the
   `producer` agent coordinates the propagation.
5. **No Unilateral Cross-Domain Changes**: An agent must never modify files
   outside its designated directories without explicit delegation.

## Model Tier Assignment

Skills and agents are assigned to model tiers based on task complexity:

| Tier | Model | When to use |
|------|-------|-------------|
| **Haiku** | `claude-haiku-4-5-20251001` | Read-only status checks, formatting, simple lookups — no creative judgment needed |
| **Sonnet** | `claude-sonnet-4-6` | Implementation, design authoring, analysis of individual systems — default for most work |
| **Opus** | `claude-opus-4-6` | Multi-document synthesis, high-stakes phase gate verdicts, cross-system holistic review |

Skills with `model: haiku`: `/help`, `/sprint-status`, `/story-readiness`, `/scope-check`,
`/project-stage-detect`, `/changelog`, `/patch-notes`, `/onboard`

Skills with `model: opus`: `/review-all-gdds`, `/architecture-review`, `/gate-check`

All other skills default to Sonnet. When creating new skills, assign Haiku if the
skill only reads and formats; assign Opus if it must synthesize 5+ documents with
high-stakes output; otherwise leave unset (Sonnet).

## Subagents vs Agent Teams

This project uses two distinct multi-agent patterns:

### Subagents (current, always active)
Spawned via `Task` within a single Claude Code session. Used by all `team-*` skills
and orchestration skills. Subagents share the session's permission context, run
sequentially or in parallel within the session, and return results to the parent.

**When to spawn in parallel**: If two subagents' inputs are independent (neither
needs the other's output to begin), spawn both Task calls simultaneously rather
than waiting. Example: `/review-all-gdds` Phase 1 (consistency) and Phase 2
(design theory) are independent — spawn both at the same time.

### Agent Teams (experimental — opt-in)
Multiple independent Claude Code *sessions* running simultaneously, coordinated
via a shared task list. Each session has its own context window and token budget.
Requires `CLAUDE_CODE_EXPERIMENTAL_AGENT_TEAMS=1` environment variable.

**Use agent teams when**:
- Work spans multiple subsystems that will not touch the same files
- Each workstream would take >30 minutes and benefits from true parallelism
- A senior agent (technical-director, producer) needs to coordinate 3+ specialist
  sessions working on different epics simultaneously

**Do not use agent teams when**:
- One session's output is required as input for another (use sequential subagents)
- The task fits in a single session's context (use subagents instead)
- Cost is a concern — each team member burns tokens independently

**Current status**: Not yet used in this project. Document usage here when first adopted.

## Parallel Task Protocol

When an orchestration skill spawns multiple independent agents:

1. Issue all independent Task calls before waiting for any result
2. Collect all results before proceeding to dependent phases
3. If any agent is BLOCKED, surface it immediately — do not silently skip
4. Always produce a partial report if some agents complete and others block

## Sprint Planning Rules

### 5-minute existence check (mandatory before estimating reverse-doc / carryover stories)

**Rule**: Before assigning an estimate to any story that targets pre-existing code, the planner MUST spend ≤5 minutes verifying current implementation status.

**Origin**: Sprint 004 retrospective action #1. Sprint 002 estimated 2.5d for `S4-01/02` (animator + Item_Viable parameter work), actual = 0d — both pre-existed in the shared `RadiusBasicLocomotion.controller`. The wrong estimate carried for 3 sprints as phantom planning weight. Sprint 004 retrospective §11 codified this as the "existence check as planning gate" process improvement.

**How to apply**: Use the template below for any story imported from a carryover plan or framed as "investigate / verify / document existing X". If verification confirms the work is already done, close the story as "verified" without sprint allocation. If verification reveals partial implementation, re-scope before estimating.

**Template** (paste into story file or sprint plan side-notes during planning):

```text
Story: [ID + title]
Target class/file: [e.g. RadiusBasicLocomotion.controller, ItemObject.cs]
Existence check (5-min budget):
  - [ ] Grep target symbol/path — found at [file:line] OR not found
  - [ ] Read target — implementation appears [complete / partial / absent]
  - [ ] Cross-check — any caller of the target? at [file:line]
Verdict:
  [ ] Pre-existing → close as "verified", no estimate
  [ ] Partial → re-scope before estimate (specify what's missing)
  [ ] Absent → estimate as fresh implementation
```

**When this rule does not apply**:
- Greenfield design work (new system, new GDD)
- New feature stories that explicitly create new code paths
- Stories with `Type: Logic` whose AC names a specific formula not present in the codebase

**Skill integration**: `/story-readiness` should ask for this verification artifact before issuing READY verdict on any story flagged as reverse-doc or carryover. `/sprint-plan` skill description includes a reminder to run existence checks before estimating Should/Nice Have carryover items.

## Branch Hygiene

### `origin/dev` merge cadence (long-lived feature branches)

**Rule**: Long-lived feature branches (those expected to live >1 sprint, e.g. `feature/refactor-ability-claude`) must merge `origin/dev` into themselves at least **every 2-3 sprint days**.

**Origin**: Sprint 003 retrospective action #4. Long-lived refactor branches (`feature/refactor-ability-claude` was the trigger case during Phase 1b) drift quickly when `origin/dev` advances with other work. Late merges produced large asset-conflict surprises that consumed planning capacity.

**How to apply**:
- Owner of the long-lived branch tracks last-merge date in their personal sprint notes or branch description
- If the gap exceeds 3 sprint days, the owner triggers a merge in the next available slot (typically end-of-day before logging off)
- Conflicts that take >30 min to resolve are escalated to lead-programmer for an "emergency rebase" call
- This rule applies to working branches in `delta-unity` first; Delta-Project worktree branches are usually short-lived and exempt

**Metric tracked**: each long-lived feature branch records merge cadence in its first commit message of each sprint week (e.g. `merge cadence: last merged origin/dev 2026-05-12 (3 sprint days ago, on schedule)`). Producer audits this at sprint retros.

**Escalation threshold**: if a single long-lived branch reaches 5+ sprint days without a merge, it becomes a sprint blocker — producer either schedules a merge slot mid-sprint or splits the branch into a smaller landable chunk.

**When this rule does not apply**:
- Throwaway prototype branches (in `prototypes/`)
- Bug-fix branches expected to land within 1 sprint day
- Tag/release branches (frozen by design)
