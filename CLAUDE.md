# Claude Code Game Studios -- Game Studio Agent Architecture

Indie game development managed through 48 coordinated Claude Code subagents.
Each agent owns a specific domain, enforcing separation of concerns and quality.

## Technology Stack

- **Engine**: Unity 2022.3.62f1
- **Language**: C#
- **Rendering**: URP (Universal Render Pipeline)
- **Networking**: Photon Fusion 2
- **Backend**: PlayFab (CBS) + Azure Functions
- **Version Control**: Git with trunk-based development
- **Build System**: Unity Build System (Unity Cloud Build ready)
- **Asset Pipeline**: Unity Asset Pipeline v2

> **Note**: Use `unity-specialist` and its sub-specialists (`unity-shader-specialist`,
> `unity-ui-specialist`, `unity-dots-specialist`, `unity-addressables-specialist`) for
> all engine-specific work.

## Project Structure

@.claude/docs/directory-structure.md

## Engine Version Reference

@docs/engine-reference/unity/VERSION.md

## Technical Preferences

@.claude/docs/technical-preferences.md

## Coordination Rules

@.claude/docs/coordination-rules.md

## Collaboration Protocol

**User-driven collaboration, not autonomous execution.**
Every task follows: **Question -> Options -> Decision -> Draft -> Approval**

- Agents MUST ask "May I write this to [filepath]?" before using Write/Edit tools
- Agents MUST show drafts or summaries before requesting approval
- Multi-file changes require explicit approval for the full changeset
- No commits without user instruction

See `docs/COLLABORATIVE-DESIGN-PRINCIPLE.md` for full protocol and examples.

> **First session?** If the project has no engine configured and no game concept,
> run `/start` to begin the guided onboarding flow.

## Coding Standards

@.claude/docs/coding-standards.md

## Context Management

@.claude/docs/context-management.md
