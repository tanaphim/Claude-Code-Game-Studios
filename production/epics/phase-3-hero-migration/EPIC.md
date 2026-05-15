# Epic: Phase 3 Hero Migration

**ADR**: [ADR-0006 §10 Phase 2 → Phase 3 Handover](../../../docs/architecture/ADR-0006-phase-2-migration-plan.md)
**Lessons Reference**: [Phase 2 Lessons Learned](../../../docs/architecture/phase-2-lessons-learned.md) — 5 codified patterns
**Pilot**: Sprint 005 S5-01..S5-21 (Hercules, complete 2026-05-14, soaking through 2026-05-21)
**Owner**: gameplay-programmer (`tanapol`)

---

## Goal

Migrate the remaining **15 production heroes** from legacy `SkillKey`-hardcoded paths to the slot-bound (`BoundSlot`/`PressedSlot`) pattern proven in the Phase 2 Hercules pilot. After Phase 3 completes, the dual-path retention layer (`BoundSlot != 0 ? slot : legacy SkillKey fallback`) can be deleted in Phase 4 along with the `SkillKey` enum itself.

## Production roster (16 code-ready heroes)

Cross-referenced from Google Sheets canonical roster + `delta-unity/Assets/GameScripts/Gameplays/Characters/*/`:

| Hero (canonical) | Code name | Abilities | Status |
|---|---|---|---|
| Hercules | Hercules | Q W E R + N + Passive | ✅ Phase 2 pilot DONE |
| **Horus** | Horus | Q W E R + I | 🎯 **S6-03 (batch 1)** — BUG-0001 cousin |
| **Volund** | Volund | Q W E R + I | 🎯 **S6-04 (batch 1)** — BUG-0001 cousin |
| **Guan Yu** | GuanYu | Q W E R + I + N | 🎯 **S6-05 (batch 1)** — BUG-0001 cousin |
| **Skadi** | Skadi | Q W E R + I | 🎯 **S6-06 (batch 1)** — clean control case |
| Anansi | Anansi | Q W E R + I + N | Batch 2 (S7) |
| Athena | Athena | Q W E R + N + P | Batch 2 |
| King Arthur | KingArthur | Q W E R + N + P | Batch 2 |
| Koschei | Koschei | Q W E R + N + P | Batch 2 |
| Lancelot | Lancelot | Q W E R + I + N | Batch 2 |
| Napoleon | Napoleon | Q W E R + I | Batch 2 |
| Jeon Woo-chi | WhoChi | Q W E R + I | Batch 2 (canonical name mismatch — flag for loc) |
| Wild Bill | WildBill | Q W E R + N + P | Batch 2 |
| Mehmed II | Mehmed | Q W E R + A + I | Batch 3 — unusual A variant |
| Merlin | Merlin | Q×3 W×3 R×3 + E + I + N | Batch 3 — 3-stance complexity |
| **Cupid** | Cupid | Q W E R + P | **Batch 4 (S8+)** — R-23 hold-the-line on AdditionalMoveSpeed |

## Sprint 006 Batch 1 plan (S6-03 → S6-06)

Stories run **sequentially** to allow each migration to test patterns before the next, but reuse the proven Phase 2 infrastructure (no new ADRs required for this batch):

| Story | Hero | Why chosen | Est. |
|---|---|---|---|
| S6-03 | Horus | BUG-0001 cousin (HorusE/HorusR flagged) | 0.5d |
| S6-04 | Volund | BUG-0001 cousin (VolundW flagged) | 0.5d |
| S6-05 | Guan Yu | BUG-0001 cousin (GuanYuE flagged) + has W/R sub-systems | 0.5d |
| S6-06 | Skadi | Clean control case (no BUG-0001 history; 5 abilities; smallest codebase) | 0.5d |
| S6-07 | Batch gate | 1-match playtest + multipeer Pass #1-5 across all 4 | 0.5d |

**Combined Must Have**: 2.5d (within sprint capacity, soak permitting)

## Dependencies (all must be ✅ before batch 1 begins)

- ✅ Phase 2 Hercules pilot end-to-end (ADR-0006 §3 Exit Criteria 1-6 all PASS)
- ⏳ Phase 2 soak verdict (ends 2026-05-21) — no Hercules regressions filed
- ✅ `ActorCombat.OnStartup` reads CBSUnit.SlotQ..SlotI aliases (S5-09)
- ✅ `ActorCombat.GetSlotAction` + `IsQuickCast(byte)` facades (S5-02, S5-03)
- ✅ `ActorCombatAction.IsActiveSlotOwner` Pattern-A helper (S5-04)
- ✅ Pattern B/C/D one-liner replacements landed (S5-05)
- ✅ `AnimationEvent.StateReleaseSlot` 40-shim migration (S5-21)
- ✅ `Actor.Combat.SetActiveSlot` wired in `ActorCombatAction.Progress` setter (S5-21)
- ✅ `AbilityComponent` attached on `base_avatar.prefab` (S5-10 — propagates to all 25+ heroes)
- ✅ `AbilityRegistryService.prefab` in `DeltaConfiguration.Services` (S5-10 TD-007 fix)
- ✅ BUG-0001 fix at `AnimatorStateSync` (S5-19 PR #357 merged into dev)

## Per-story common shape

Every Phase 3 hero migration follows the same skeleton — only hero-specific code paths differ:

1. **CBS data audit** — verify `CBSUnit.SlotQ..SlotI` for the hero has correct ability IDs (PlayFab dashboard side; designer task or async user-side check)
2. **Code audit** — scan hero's `*Action.cs` files for hardcoded `SkillKey` references, hardcoded slot literals, or sibling-skill reads via `Combat.SkillN` direct properties (vs `GetSlotAction(slot)` facade)
3. **Pattern replacements** — apply Patterns A-D from Phase 2 (`IsActiveSlotOwner`, `IsInputSlotMatch`, `IsQuickCastForBoundSlot`, `MaxRank <= 1`) where the hero has those code shapes. Most heroes have far less SkillKey hardcoding than Hercules — many migrations are no-ops at the code level
4. **Bootstrap verification** — confirm `ActorCombat.OnStartup` BindSlot pipeline fires for the hero in production scene (no `BindSlot not registered` warnings)
5. **AnimationEvent verification** — confirm `StateReleaseSlot` dispatcher routes correctly for hero's animation events (verify the S5-21 fix applies)
6. **Multipeer parity** — Pass #1-5 green, bandwidth ≤65 B/s
7. **Manual playthrough** — Q/W/E/R + Normal Attack + Recall + Item all functional in `scene_game_map.unity`
8. **EditMode tests** — minimal slot-binding tests (per-hero file optional; shared base test class candidate for batch 2+)

## Risks shared across Phase 3 batch 1

- **R1 — Hero with unmigrated bot path**: bot using a Phase 3 hero may hit BUG-0004-style typos (S5-05 cousin) or PressedSlot=0 issues. **Mitigation**: S6-B1 fix shipped; spot-check bot for each migrated hero
- **R2 — CBS data row missing for hero**: if CBS Hero record lacks SlotQ..SlotI fill, BindSlot will fail silently for unfilled slots. **Mitigation**: S5-09 ISlotBinder logs warnings on `BindSlot not registered`; treat as hero-data ticket if surfaces
- **R3 — Hero has unique ability variant not covered by S5 patterns**: e.g., Mehmed's A variant (out of batch 1 scope). **Mitigation**: defer such heroes to later batches; document new pattern in `phase-2-lessons-learned.md`
- **R4 — Soak verdict surfaces regression**: if Phase 2 soak fails 2026-05-21, batch 1 blocks until fix lands. **Mitigation**: file P0 bug; pre-empt batch 1; sprint plan absorbs delay (Should Have / Nice slack covers ~3-4 days)

## Phase 3 → Phase 4 handover criteria (forward-looking)

Phase 4 (delete dual-path + retire SkillKey enum) starts when:
- All 15 remaining heroes migrated (batches 1-4)
- No `BoundSlot == 0` code path executed in production playtest for 1+ week
- No hero ability action file uses `SkillKey.X` literals (audit-grep clean)
- ADR-0006 §10 forward-handover gate criteria authored

Out of scope for this epic — file Phase 4 epic when batch 4 closes.

## Stories

- [S6-03 — Horus migration](S6-03-horus-migration.md)
- [S6-04 — Volund migration](S6-04-volund-migration.md)
- [S6-05 — Guan Yu migration](S6-05-guanyu-migration.md)
- [S6-06 — Skadi migration (control case)](S6-06-skadi-migration.md)
- S6-07 — Batch 1 playtest gate (inline in sprint plan; future: dedicated file)

## References

- [ADR-0006 Phase 2 Migration Plan](../../../docs/architecture/ADR-0006-phase-2-migration-plan.md) — §10 handover gate; pattern definitions §5-6
- [ADR-0008 Slot Binding via CBSUnit](../../../docs/architecture/ADR-0008-slot-binding-via-cbsunit.md) — slot binding source of truth
- [Phase 2 Lessons Learned](../../../docs/architecture/phase-2-lessons-learned.md) — 5 patterns codified, Pattern #5 promoted to /story-readiness gate (S6-09)
- [Sprint 005 plan §S5-09 Hercules bootstrap](../../sprints/sprint-005.md) — template story for migration shape
- [Sprint 006 plan](../../sprints/sprint-006.md) — Phase 3 batch 1 scope
- [Hero design sheets](https://docs.google.com/spreadsheets/d/1G8cKwnO8UEV_fhsF_bocO7vUTjPF4jDKXzxj6tGulds/edit?gid=392521638) — canonical roster
