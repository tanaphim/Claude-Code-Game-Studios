# Sprint 5 — 2026-05-09 to 2026-05-22

## Sprint Goal

ปิด **Phase 2 Hercules pilot** (ability migration, Foundation → Production transition)
และ land 3 high-priority design decisions (R-21 Role Restriction, AI Bot fate, R-22/R-23
schedule) ที่ Sprint 004 retro flag ไว้ — เพื่อ unblock Sprint 006+ planning

**Theme:** "Phase 2 ability refactor + design decision sprint"

---

## Capacity

- Total days: 14 (2 สัปดาห์)
- Buffer (20%): 2.8 วัน สำรองสำหรับงานที่ไม่ได้วางแผน
- Available: ~11 วัน

---

## Tasks

### Must Have (Critical Path — Phase 2 Hercules pilot)

| ID | Task | Agent/Owner | Est. Days | Dependencies | Acceptance Criteria |
|----|------|-------------|-----------|-------------|-------------------|
| S5-01 (revised, ADR-0008) | `CBSUnit.SlotQ/W/E/R/A/I` alias properties (read-only, wraps legacy fields) | gameplay-programmer | 0.25 | — | Aliases compile; EditMode tests pass for empty/non-empty/consolidation cases; existing CBS deserialization unaffected. **Note:** Original ADR-0006 §6.1 (CBSAbility.Slot) reverted 2026-05-08 — see ADR-0008. |
| S5-02 (P2-01) | `ActorCombat.GetSlotAction` + `AbilityComponent.GetSlotAction` facade | gameplay-programmer | 0.25 | — | Both facades return same action for slot lookup; covered by unit test |
| S5-03 (P2-02) | `ActorCombat.GetActiveSlot` + `IsQuickCast(byte slot)` accessor | gameplay-programmer | 0.25 | — | GetActiveSlot returns latest pressed slot; IsQuickCast respects per-slot setting |
| S5-04 (P2-04) | `ActorCombatAction` Pattern-A helper (`IsActiveSlotOwner`) — replace 5 blocks | gameplay-programmer | 0.5 | S5-01, S5-02 | All 5 owner-guard sites replaced; multipeer harness Pass #1-5 still pass |
| S5-05 (P2-05) | `ActorCombatAction` Pattern-B/C/D one-liner replacements (4 sites) | gameplay-programmer | 0.5 | S5-01..S5-03 | 4 sites replaced; Hercules manual playthrough passes |
| S5-06 (P2-06) | `AnimationEvent` Option A — wire 42 shim methods through `GetActiveSlot()` | gameplay-programmer | 0.5 | S5-03 | All 42 shim methods compile; animation events fire on correct slot |
| S5-07 (P2-07) | `HerculesRAction.GetInput` rewrite (PressedSlot path) | gameplay-programmer | 0.25 | S5-01 | Hercules R charge respects PressedSlot binding; release uses ReleasedSlot when wired |
| S5-08 (P2-08) | `HerculesWAction` slot-indexed sibling reads (5 sites) | gameplay-programmer | 0.25 | S5-02 | 5 sibling reads use GetSlotAction; behavior unchanged |
| S5-09 (P2-09, revised ADR-0008) | `Hercules` avatar bootstrap — `ActorCombat.OnStartup` reads `unit.SlotQ[0]..SlotI[0]` from CBSUnit and calls `AbilityComponent.BindSlot(slot, id)` × 6 (Q/W/E/R/A/I); slot 7 Recall bound globally | gameplay-programmer | 0.5 | S5-01 | 6 BindSlot calls in OnStartup, all reading from CBSUnit aliases (no hardcoded ability ids); PeerMode=Single playtest no errors |
| S5-10 (P2-10) | Manual playtest checklist + 1-match Training playthrough verification | qa-tester + gameplay-programmer | 0.5 | all S5-01..S5-09 | Hercules QWER playable end-to-end; multipeer harness passes; evidence in `production/qa/evidence/` |

**Must Have Subtotal: 3.75d** (~2.0d critical path serial; rest parallelizable)

**Critical path:** S5-01 → S5-04 → S5-09 → S5-10
**Parallelizable:** S5-02, S5-03, S5-06, S5-07, S5-08

### Should Have — Design decisions (Sprint 004 retro action #2, #3, #5)

| ID | Task | Agent/Owner | Est. Days | Dependencies | Acceptance Criteria |
|----|------|-------------|-----------|-------------|-------------------|
| S5-11 | **Item Role Restriction decision** (R-21) — implement gating OR remove `ItemObject.Positions[]`. Author ADR-0008 with chosen path | game-designer + gameplay-programmer | 1.0 | — | ADR-0008 status=Accepted; if implement → unit test on `AvailableToPurchase()`; if remove → schema migration noted |
| S5-12 | **AI Bot fate decision** — formally commit Sprint 006 with full focus OR descope to post-launch. Update sprint-006 backlog or move S4-09/S4-10 to `production/backlog/post-launch.md` | producer + creative-director | 0.5 | — | Decision documented; carryover ledger updated; Sprint 003 retro action #5 closed |
| S5-13 | **Confirm 3 Garen variant controllers** — production heroes or legacy test? If legacy → delete; if production → swap base or port states | art-director + lead-programmer | 0.5 | — | Decision documented in `item-system.md` §Known Issues; legacy controllers deleted OR base swapped |

**Should Have Subtotal: 2.0d**

### Nice to Have — Process + polish

| ID | Task | Agent/Owner | Est. Days | Dependencies | Acceptance Criteria |
|----|------|-------------|-----------|-------------|-------------------|
| S5-14 | Codify **5-min existence check rule** (S4 retro action #1) ใน `coordination-rules.md` หรือ `/sprint-plan` skill | producer | 0.25 | — | Rule documented; example template added |
| S5-15 | Carry-forward S3-retro action #3: scene-naming convention doc | producer | 0.25 | — | `coordination-rules.md` updated |
| S5-16 | Carry-forward S3-retro action #4: `origin/dev` merge cadence metric (track per Sprint 005) | lead-programmer | 0.25 | — | Metric defined; tracked starting Sprint 005 |
| S5-17 (S4-P1) | `AbilityMultipeerRunner` duplicate-Start guard | network-programmer | 0.5 | — | Console clean ตอน multipeer harness run — ไม่มี GameIsFull cascade |
| S5-18 (S4-P2) | `AbilityRegistry` boot-time optimization (Resources.LoadAll → Addressables) | unity-addressables-specialist | 1.5 | — | Cold-start scan < 100 ms / 158 prefabs |

**Nice to Have Subtotal: 2.75d** (pull only ถ้า Must + Should เสร็จเร็ว)

### Bugs (filed during sprint)

| ID | Bug | Agent/Owner | Est. Days | Dependencies | Acceptance Criteria |
|----|-----|-------------|-----------|-------------|-------------------|
| S5-19 | [BUG-0001](../qa/bugs/BUG-0001-recall-locomotion-stuck.md) — Recall post-warp locomotion animation ไม่เล่น | gameplay-programmer + technical-artist | 0.5 | — | Animator transitions back to locomotion after Recall; manual playtest confirms walk/run plays |
| S5-20 | [BUG-0002](../qa/bugs/BUG-0002-anansi-w-idle-stuck.md) — Anansi W ค้างท่า Idle หลัง cast จบ | gameplay-programmer + technical-artist | 0.5 | — | Animator exits W state correctly; locomotion resumes; manual playtest passes |

**Investigation note:** BUG-0001 และ BUG-0002 มีอาการ post-cast animator-state-stuck เหมือนกัน — แนะนำ investigate ร่วมเพื่อหา root cause ก่อนเขียน fix แยก

---

## Carryover from Previous Sprint

### Accepted into Sprint 005

| Task | เหตุผล | From Sprint | Estimate |
|------|--------|-------------|----------|
| S5-01..S5-10 (P2-01..P2-10) | Phase 2 plan ที่ deferred จาก Sprint 004 ตามแผน ADR-0006 | 004 (deferred by design) | 3.75d |
| S5-11 R-21 decision | Retro action #2 — blocks S4-09 AI Bot priority list | 004 retro | 1.0d |
| S5-12 AI Bot fate | Retro action #3 — 3-sprint carryover triggers root-cause | 004 retro | 0.5d |
| S5-13 Garen variants | Retro action #5 | 004 retro | 0.5d |
| S5-14..S5-16 process docs | Retro actions #1, S3-retro #3, S3-retro #4 | 004 + 003 retro | 0.75d |

### Deferred from Sprint 005 to Sprint 006+

| Task | เหตุผล |
|------|--------|
| S4-09 AI Bot item-buying (2.0d) | รอ S5-11 (R-21 Role Restriction decision) ก่อน — design ของ Bot item priority depends on whether role gating enforced |
| S4-10 AI Bot Difficulty (1.0d) | depends S4-09 |
| R-22 / R-23 implementation (stat /100 + AdditionalMoveSpeed rename) | Phase 2 scope already 3.75d — schedule with Phase 3 หรือ dedicated balance pass; ADR-only ใน Sprint 005 ถ้ามีเวลา |

---

## Risks

| Risk | Probability | Impact | Mitigation |
|------|------------|--------|-----------|
| Phase 2 P2-04 Pattern-A helper extraction misses edge case → Hercules behavior regression | Low | High | Multipeer harness Pass #1-5 + manual Hercules playthrough เป็น regression gate (S5-10) |
| `CBSAbility.Slot` PlayFab CBS dashboard update ต้อง designer ทำ | Medium | High | `EffectiveSlot` shim derives from `SkillKey` เมื่อ Slot==0 → code unblock; designer fill on-deck |
| Hercules R charge release ยังใช้ `Buttons.R` (ReleasedSlot deferred S3-05) | Medium | Medium | Phase 2 keeps as known limitation; full removal Phase 3 |
| S5-11 R-21 decision ไม่ landed → blocks Sprint 006 AI Bot kickoff | Medium | Medium | Time-box S5-11 ที่ 1d; ถ้าไม่ตัดสินใจได้ใน 1d → escalate creative-director |
| Sprint 004 estimation pattern repeat (overestimate reverse-doc) | Low | Low | Phase 2 stories เป็น real implementation ไม่ใช่ reverse-doc → estimate น่าจะ accurate |

---

## Dependencies on External Factors

- **PlayFab CBS dashboard access** — designer ต้องอัพเดต `CBSAbility.Slot` field after S5-01 schema change
- **Photon PeerMode** — ต้อง `Single` mode สำหรับ S5-09/S5-10 production playtest (S4-P5 workflow doc)
- **Multipeer harness** — รัน Pass #1-5 หลัง S5-04, S5-05, S5-09 เป็น regression check

---

## Definition of Done for this Sprint

### Per-Task Gates

- [ ] All Must Have tasks (S5-01..S5-10) completed
- [ ] All tasks pass acceptance criteria
- [ ] **QA plan exists** (`production/qa/qa-plan-sprint-005.md`) — required for Phase 2
- [ ] Logic stories (S5-01, S5-02, S5-03) have passing unit tests
- [ ] Integration story (S5-10) has playtest evidence in `production/qa/evidence/`
- [ ] Smoke check passed (`/smoke-check sprint`)
- [ ] QA sign-off report: APPROVED or APPROVED WITH CONDITIONS
- [ ] No S1 or S2 bugs in Hercules playthrough
- [ ] ADR-0006 Phase 2 → Phase 3 handover criteria met (§10)
- [ ] ADR-0008 (Role Restriction) Status=Accepted
- [ ] AI Bot fate decision documented
- [ ] Code reviewed and merged to main

### Cross-cutting

- [ ] No new TODO/FIXME accumulation > +5 from baseline (Sprint 004: TODO=14)
- [ ] Risk register updated if Phase 2 reveals new risks
- [ ] Sprint 005 retrospective generated at sprint end

---

## Progress

- **2026-05-12 — S5-09 COMPLETE** (delta-unity@claude/s5-09-hercules-bootstrap). ActorCombat.OnStartup now reads CBSUnit.SlotQ..SlotI aliases and dispatches to ISlotBinder.BindSlot ×6 + slot 7 Recall (Hero only). Introduced `ISlotBinder` interface in Radius asmdef to break Radius↔Abilities asmdef cycle (user-approved Option A). Dual-path retention: legacy `CreateSkill` continues to operate. 12/12 EditMode tests pass. Manual prefab attach (Unity Editor: drop AbilityComponent onto Hero prefab) deferred to S5-10 — warning log fires until done.
- **2026-05-12 — BUG-0002 RESOLVED** (delta-unity@91697bf78e on dev) — Anansi W animator stuck idle on client peer. Tracked as S5-20 in sprint-status.yaml.

### Must Have status
- ✅ S5-01, S5-02, S5-03, S5-07, S5-08 — done (prior sessions)
- ✅ S5-09 — done (2026-05-12, this session)
- ⏳ S5-04, S5-05, S5-06 — ready to pick up (all deps satisfied)
- ⏳ S5-10 — blocked until S5-04..S5-06 land + manual AbilityComponent prefab attach

---

## Source

- [Sprint 004 retrospective](../retrospectives/sprint-004.md) — 6 action items
- [ADR-0006 Phase 2 Migration Plan §7](../../docs/architecture/ADR-0006-phase-2-migration-plan.md) — P2-01..P2-10 work breakdown
- [ADR-0008 Slot Binding via CBSUnit](../../docs/architecture/ADR-0008-slot-binding-via-cbsunit.md) — supersedes ADR-0006 §6.1; affects S5-01 + S5-09 scope (2026-05-08)
- [Risk Register R-21..R-23](../risk-register/risk-register.md) — Open design decisions
- [Sprint 003 retrospective](../retrospectives/sprint-003.md) §5 — carry-forward actions #3, #4

## Mid-Sprint Pivot Log

**2026-05-08 — ADR-0006 §6.1 → ADR-0008 (CBSAbility.Slot superseded)**

S5-01 was implemented (additive `CBSAbility.Slot` field + `EffectiveSlot` shim + `SkillKeyToSlot` mapper + 7 unit tests) per ADR-0006 §6.1. User design review surfaced that slot binding should be sourced from `CBSUnit` (per-hero kit), not `CBSAbility` (per-ability slot self-declaration). Implementation reverted; ADR-0008 written; S5-01 scope reduced to alias properties on `CBSUnit`; S5-09 scope updated to read from CBSUnit aliases. Net Sprint 005 estimate unchanged (~3.75d Must Have).

Reverted code (in delta-unity repo): `CBSAbility.cs`, `AbilityDataSnapshot.cs`, deleted `AbilitySlotTests.cs`. No commits to dev branch — change was on worktree only.

## Retrospective Seeds (carry-forward to Sprint 005 retro)

**Finding 1 — NetworkBehaviour EditMode testability gap (S5-02 + S5-03)**

`ActorCombat` is a Fusion `NetworkBehaviour`. Instance methods that read networked properties (`Skill1..4`, `IsQuickQ..R`, `m_ActiveSlot`) cannot be exercised in EditMode without a live `NetworkRunner`. No existing test fixture in the project instantiates a Fusion type. S5-02 (`GetSlotAction`) and S5-03 (`IsQuickCast`) acceptance criteria specified "covered by unit test" — partially satisfied via static-helper extraction (`ResolveSlotAction`, `ResolveQuickCast`) which are pure and EditMode-testable. `GetActiveSlot` / `SetActiveSlot` networked-state behaviour falls back to multipeer harness + Hercules manual playtest (S5-10) for coverage.

**Process improvement candidate:** sprint planning should distinguish "Logic stories with pure C# state" (full EditMode coverage possible) from "Logic stories with NetworkBehaviour state" (requires PlayMode framework or static-helper extraction). Currently classified together as "Logic" by `/qa-plan`.

**Sprint 006 backlog candidate:** investigate establishing a PlayMode test framework that can spawn a minimal `NetworkRunner` for unit-level coverage of NetworkBehaviour subclasses. Estimate ~1-2d. If too expensive, codify "static-helper extraction for switch logic" as the canonical pattern.
