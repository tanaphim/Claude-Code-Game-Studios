# Sprint 003 Retrospective — 2026-04-21

**Sprint:** `sprint-003` (2026-05-02 → 2026-05-15)
**Goal:** ส่งมอบ ADR-0006 Phase 1b Foundation + AI Bot initial behaviors
**Outcome:** Phase 1b 7/7 ✅ • AI Bot 0/2 (deferred to Sprint 004 by user)
**Tag:** `phase-1b-complete` (Delta-Project @ `5266755`, delta-unity @ `1022c87d`)

---

## 1. Velocity — actual vs planned

### Must Have (Phase 1b — `P1B-01..P1B-07`)

| Story | Planned | Actual | Δ | Notes |
|-------|--------:|-------:|--:|-------|
| S3-01 Multipeer harness | 2.0 | ~2.0 | 0 | First story — set Phase 1b verification baseline (Pass #4/#5) |
| S3-02 AbilityRegistry | 1.0 | ~1.0 | 0 | 5 EditMode tests; uncovered Resources I/O perf concern (deferred polish) |
| S3-03 AbilityDataSnapshot | 0.5 | ~0.5 | 0 | Bigger scope than expected (added AbilitySnapshotService); fit anyway |
| S3-04 BindSlot real | 1.0 | ~1.0 | 0 | Required Option C decision (dual-path retention) — recovered from "literal plan" mismatch in <30 min |
| S3-05 InputMessage slot | 0.5 | ~0.5 | 0 | Soft-verified (Option A) due to environment block; harness coverage transitively sufficient |
| S3-06 KeybindMap | 1.0 | ~0.7 | -0.3 | Most of class was Phase 1a-complete; Phase 1b only added prefab + DeltaConfig + Editor UI |
| S3-07 Phase 2 audit | 0.5 | ~0.5 | 0 | Documentation-only; line-anchoring done with grep |
| **Subtotal** | **6.5** | **~6.2** | **-0.3** | Slight under-estimate; buffer absorbed |

### Should Have (AI Bot)

| Story | Planned | Actual | Notes |
|-------|--------:|-------:|-------|
| S3-08 AI Bot item-buying | 2.0 | 0 | Deferred to Sprint 004 by user 2026-04-21 |
| S3-09 AI Bot Difficulty | 1.0 | 0 | Deferred to Sprint 004 by user 2026-04-21 |

### Sprint totals

- **Planned:** 9.5d (6.5 Must + 3.0 Should) within 11d capacity
- **Actual:** ~6.2d Must Have shipped • 3.0d Should Have deferred
- **Velocity attainment:** 100% on Must Have • 0% on Should Have (by design — descope)

---

## 2. What went well

### Process / planning

- **Critical-path discipline.** Phase 1b had clear exit criteria (Pass #4 + #5 + 4 production artifacts). Every closure note tied back to that gate. No scope creep into Phase 2 territory.
- **"Plan deviation" pattern.** S3-04 (BindSlot Option C dual-path) and S3-05 (InputMessage Option A soft-verify) both encountered reality that contradicted the literal plan text. We surfaced the mismatch to the user with options A/B/C, picked one, documented the rationale in the ADR closure note, moved on. Zero rework, full traceability.
- **Soft verification escalation.** When live production playtest was blocked (Version mismatch + STUN timeout in S3-05), we didn't fake the verification — we documented the constraint, justified that harness coverage is transitively sufficient, and explicitly deferred end-to-end trace to Phase 2 Hercules.
- **Dev branch merge timing.** Pulled `origin/dev` mid-sprint (between S3-04 and S3-05) — rebased cleanly with zero conflicts on the second merge (vs 16 prefab conflicts in the first). Catching up early prevented compounding drift.

### Engineering

- **Test coverage where it counts.** 12 EditMode tests landed (5 Registry + 7 Snapshot). Avoided forcing tests on Fusion-runtime-bound code (BindSlot, Spawn happy-path) — coverage delegated correctly to harness + Phase 2.
- **Anchor-based `CreateAction` overload (S3-04).** The decision to add `CreateAction(string, NetworkBehaviour anchor)` alongside the `Actor` overload unblocked AbilityComponent without breaking the Actor-typed callers. Clean facade that Phase 2 will inherit.
- **FNV-1a hash determinism.** S3-03's hash is sorted-key + bit-exact float reinterpret — verified via two services consuming same data in different insertion order producing identical output. Catches snapshot drift at runtime.
- **Editor inspector vs full UGUI panel (S3-06).** Picking Option A (Editor placeholder) instead of Option C (full Controls.unity scene) saved ~60 min while still meeting the rebind workflow acceptance for QA. Runtime UI properly deferred to Sprint 004 polish backlog.

### Documentation

- **Closure-note convention** in ADR §4.x. Every story added a "**Closure (date) — Option X**" subsection right next to the planned acceptance — readers see plan + reality + decision in one read. Worth carrying into Phase 2.
- **Phase 2 plan published with line-anchored audit** (S3-07). Grep + manual review against HEAD `1022c87dbb` gave us the actual edit surface (2 Hercules files, ~155-line ActorCombatAction reduction). Phase 2 starts with no archaeology debt.

---

## 3. What didn't go well

- **Sprint 002 carryover (S2-01..S2-08) deferred a 3rd time.** Animator + bug-hunting batch never landed in Sprint 003 — and was never meant to (Plan A scope decision pre-Sprint 003). But this is now 3 sprints of slip. Sprint 004 makes it Must Have — if it slips again, root-cause review needed.
- **Production playtest environment was unusable.** Version mismatch + Photon STUN timeout blocked end-to-end verification of S3-05. We worked around with soft-verify, but **this is a recurring blocker** — if Sprint 004's AI Bot work also can't reach a real match, that's velocity risk. (Sprint 004 risk table flags this.)
- **Asset-side merge conflicts (round 1).** First `origin/dev` pull had 16 character prefab conflicts. Resolved with `--theirs` (correct call given our branch didn't author character work), but this is a class of conflict that will recur every dev merge while ability work stays on a long-lived feature branch. Mitigation: merge dev more frequently.
- **`PrototypeTest.unity` vs `AbilityMultipeer.unity` naming drift.** S3-01 was supposed to create a new scene `AbilityMultipeer.unity`; instead the user wired `AbilityMultipeerRunner` onto the existing `PrototypeTest.unity`. Documentation referenced the wrong name for ~24 hours until corrected. **Lesson:** when story plan says "new scene," confirm with user whether to create-or-attach before writing the closure note.
- **Duplicate `AbilityMultipeerRunner.Start()` bug.** Fusion multipeer scene-clone behavior surfaced a duplicate-Start cascade (`GameIsFull` + `NetworkObjectSpawnException`). Pass #4/#5 weren't affected (first session succeeded), so we flagged as polish item — but it's been visible since S3-01 and remained in `S4-P1` backlog. Should have allocated 30 min to fix in S3-04 while harness was fresh.

---

## 4. Patterns worth keeping

These are session-level patterns the user + agents converged on by trial. Worth promoting to project conventions for Sprint 004+:

1. **Option A/B/C decision menu when plan-vs-reality drift detected.** Whenever the literal plan can't be executed verbatim, surface 2–3 concrete alternatives with effort estimates + plan-fidelity ratings, recommend one, wait for user pick. Used in S3-04 (Option C), S3-05 (Option A), S3-06 (Option A). Zero rework from these decisions.

2. **Closure note structure** (ADR §4.x):
   - 1 paragraph "what shipped"
   - Table mapping each acceptance criterion → ✅ / 🟡 PARTIAL / ⏸ DEFERRED
   - Out-of-scope explicit list with target sprint
   - Verification limitations (if soft-verified)

3. **Commit message structure** for closure commits (`ADR-XXXX <phase>: close <story> <option>`):
   - Body lists implementation deliverables
   - Body lists deferred items + sprint placement
   - "Linked delta-unity commit: <hash>" cross-reference (because work spans 2 repos)

4. **Soft-verify documentation.** Three-part pattern: (a) what we verified, (b) what we couldn't verify and why (environment/scope), (c) where the missing coverage will land (which future story or harness). Used in S3-05 closure.

5. **Editor placeholder over runtime UI when production env is blocked.** S3-06 demonstrated that an Editor inspector is sufficient placeholder for QA workflow when match access is unreliable — saves UI scene cost without losing functionality.

---

## 5. Action items for Sprint 004

| # | Action | Owner | When |
|---|---|---|---|
| 1 | ~~Investigate Photon STUN timeout + Version mismatch~~ ✅ **RESOLVED 2026-05-07** — Root cause: `NetworkProjectConfig.fusion` had `PeerMode=Multiple` (set during S3-01 multipeer harness work) leaking into production scene. Reverting to `PeerMode=Single` for production playtest restores connectivity. S4-09 AI Bot + Phase 2 P2-10 playtest unblocked. Workflow friction (manual toggle) tracked as `S4-P5`. See [peer-mode-toggle workflow doc](../../docs/dev-workflow/peer-mode-toggle.md). | network-programmer + producer | ~~Sprint 004 day 1~~ — closed pre-sprint |
| 2 | Fix `AbilityMultipeerRunner` duplicate-Start cascade (S4-P1) — only 0.5d, but compounds future harness debugging | network-programmer | Sprint 004 polish slot |
| 3 | ~~Document scene-naming convention~~ ✅ **RESOLVED 2026-05-15** — Sprint 006 S6-08 landed convention in [coding-standards.md § Asset Naming Conventions](../../.claude/docs/coding-standards.md) (prefix table: `scene_`/`test_scene_`/`prototype_` + default-to-create-but-grep-first-on-modify rule). | producer | ~~Add to `coordination-rules.md` or `coding-standards.md`~~ — done |
| 4 | ~~Schedule frequent `origin/dev` merge cadence on `feature/refactor-ability-claude`~~ ✅ **RESOLVED 2026-05-15** — Sprint 006 S6-08 codified the rule in [coordination-rules.md § Branch Hygiene](../../.claude/docs/coordination-rules.md) (every 2-3 sprint days threshold + 5-day escalation gate + producer retro audit). | lead-programmer | ~~Sprint 004 +Sprint 005~~ — done |
| 5 | Sprint 002 carryover (S4-01..S4-08) — if any story slips again in Sprint 004, root-cause review required before Sprint 005 plans | producer | End of Sprint 004 |

> **Action #5 resolution (2026-05-15)** — ✅ **CLOSED**. The S4-01..S4-08 animator+bug batch did not slip in Sprint 004 (closed 8/8 Must Have, 2-day burn — see [Sprint 004 retro](sprint-004.md)). The rule was subsequently extended in spirit to the AI Bot carryover (S2-09/10 → S3-08/09 → S4-09/10 → S5-12, 4 carryovers). That extension closed via Sprint 006 S6-01 / S5-12 Path B descope to post-launch backlog (`production/decisions/S5-12-ai-bot-fate.md`). No further AI Bot defer permitted; carryover ledger closed.

---

## 6. Sprint 005 readiness check

Phase 2 (Hercules pilot) is sprint-ready entering Sprint 005:

- ✅ Migration plan published (`docs/architecture/ADR-0006-phase-2-migration-plan.md`)
- ✅ All 6 Phase 1b artifacts have a documented Phase 2 consumer (Entry Context table §2)
- ✅ 10-story work breakdown (P2-01..P2-10) totalling 3.75d
- ✅ Critical path identified (~2.0d serial)
- ✅ Phase 1b → Phase 2 gate: tag `phase-1b-complete` exists on both repos as immutable baseline

**Risk to track:** Sprint 004's animator + bug stories may push Sprint 005 start later than planned. Phase 2 is small (3.75d) so it can fit even a compressed Sprint 005, but mid-Sprint 004 retro should reconfirm.

---

## 7. References

- [Sprint 003 plan](../sprints/sprint-003.md)
- [Sprint 004 plan](../sprints/sprint-004.md)
- [ADR-0006 Phase 1b Implementation Plan](../../docs/architecture/ADR-0006-phase-1b-implementation.md)
- [ADR-0006 Phase 2 Migration Plan](../../docs/architecture/ADR-0006-phase-2-migration-plan.md)
- Tag `phase-1b-complete` (Delta-Project @ `5266755`, delta-unity @ `1022c87d`)
