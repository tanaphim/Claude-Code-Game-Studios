# Epic: Test Infrastructure

**Status**: Active (Sprint 006+)
**Owner (role)**: lead-programmer + network-programmer
**Created**: 2026-05-15

## Scope

Stories ที่เกี่ยวกับ test harness, multipeer runner, PlayMode/EditMode framework, และ test infrastructure utilities ที่ไม่ผูกกับ system GDD ใด ๆ

## Stories

| ID | Title | Sprint | Status |
|----|-------|--------|--------|
| S6-14 | AbilityMultipeerRunner duplicate-Start guard | 006 | backlog |

## Future Candidates

- **S7-PROPOSED-PLAYMODE-SCALING** (filed in S6-13 decision doc) — scale NetworkRunnerFixture pattern to first domain NetworkBehaviour coverage
- **S7-PROPOSED-SCENE-RENAME** — Rename `PrototypeTest.unity` → `test_scene_ability_multipeer.unity` per coding-standards.md § Asset Naming Conventions (snake_case + `test_scene_` prefix). Use Unity Editor rename (preserve GUID) + `git mv` + grep for hard-coded `"PrototypeTest"` string refs. ~0.25d. Low priority — scene slated for replacement per AbilityMultipeerRunner.cs:41 ("Replaced in Phase 1b/Phase 2 by proper scene bootstrap"); may become moot.

## Governing ADRs

- ADR-0006 (Ability migration plan — Phase 1b origin of multipeer harness)

## Note

Epic นี้สร้างเป็น lightweight shell เพื่อ host S6-14 ตามกฎ catalog (`production/epics/*/EPIC.md` glob) — ขยายเพิ่มเมื่อมี story test-infrastructure เข้ามาสมทบ
