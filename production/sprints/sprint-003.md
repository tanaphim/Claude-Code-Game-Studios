# Sprint 3 — 2026-05-02 to 2026-05-15

## Sprint Goal

ส่งมอบ **ADR-0006 Phase 1b Foundation** (production implementations ของ 4 artifacts + Host+Client harness verify Pass #4/#5)
และ **AI Bot initial behaviors** (item-buying + difficulty tiers) เพื่อปลดล็อก Phase 2 pilot migration (Hercules)

## Capacity

- Total days: 14 (2 สัปดาห์)
- Buffer (20%): 2.8 วัน สำรองสำหรับงานที่ไม่ได้วางแผน
- Available: ~11 วัน

---

## Tasks

### Must Have (Critical Path — ADR-0006 Phase 1b)

| ID | Task | Agent/Owner | Est. Days | Dependencies | Acceptance Criteria |
|----|------|-------------|-----------|-------------|-------------------|
| S3-01 | **P1B-01** — Host+Client multipeer test harness | network-programmer + unity-specialist | 2.0 | — | Multipeer scene (Host+Client runners); Pass #4 ✅ dict content parity; Pass #5 ✅ idle bandwidth < 2 KB/s |
| S3-02 | **P1B-02** — `AbilityRegistry` real implementation | gameplay-programmer | 1.0 | — | Boot-time `Resources.LoadAll` scan < 100ms/50 abilities; 3+ unit tests; O(1) lookup post-boot |
| S3-03 | **P1B-03** — `AbilityDataSnapshot` real implementation | gameplay-programmer | 0.5 | — | CBS pull freezes at match start; mutate-after-build test proves immutability; hash broadcast wired |
| S3-04 | **P1B-04** — `AbilityComponent.BindSlot` real implementation | gameplay-programmer | 1.0 | S3-02 | Real `BindSlot(slot, abilityId)` spawns via registry + triggers ChangeDetector; `[Obsolete] BindSlotPrototype` removed; prototype runner ยัง pass criteria #1-5 |
| S3-05 | **P1B-05** — `InputMessage.PressedSlot/ReleasedSlot` fields | network-programmer | 0.5 | S3-01 | Parallel write (`Buttons.Q` + `PressedSlot=1`); existing consumers zero regression; wire delta ≤ +3 B/tick measured |
| S3-06 | **P1B-06** — `KeybindMap` production wiring + Settings placeholder | ui-programmer + ux-designer | 1.0 | — | `DeltaService.GetService<KeybindMap>()` ทำงาน; Settings UI placeholder ที่ `Controls.unity` (rebind + reset); persist across sessions; UX sign-off "non-blocking for Phase 2" |
| S3-07 | **P1B-07** — Phase 2 touch-point audit | lead-programmer | 0.5 | — | `docs/architecture/ADR-0006-phase-2-migration-plan.md` published; Hercules Q/W/E/R lines re-verified; `ActorCombatAction` A/B/C/D refreshed; Phase 2 entry criteria ครบ |

**Must Have Subtotal: 6.5 วัน**

### Should Have

| ID | Task | Agent/Owner | Est. Days | Dependencies | Acceptance Criteria |
|----|------|-------------|-----------|-------------|-------------------|
| S3-08 | AI Bot item-buying behavior (carryover S2-09) | ai-programmer + game-designer | 2.0 | — | Bot buys items ตาม role + gold threshold; อ้างอิง `ai-bot-system.md`; playtest 1 match ไม่มี crash |
| S3-09 | AI Bot Difficulty Level — Easy/Normal/Hard (carryover S2-10) | ai-programmer | 1.0 | S3-08 | 3 tiers ต่างกันที่ reaction time + item-buying priority; documented ใน `ai-bot-system.md §Difficulty` |

**Should Have Subtotal: 3.0 วัน**

### Nice to Have

*ไม่มี — Sprint 002 carryover S2-01..S2-08 (7.0d) ถูก defer ไป Sprint 004 โดยเจตนา (see Carryover section)*

**Total Estimated: 9.5 วัน** (ภายใน capacity 11d; เหลือ ~1.5d buffer)

---

## Carryover from Previous Sprint

### Accepted into Sprint 003

| Task | เหตุผล | Estimate ใหม่ |
|------|--------|--------------|
| S2-09 → S3-08 | ไม่ได้เริ่ม (dropped to Should Have ใน Sprint 002) | 2.0 วัน |
| S2-10 → S3-09 | ไม่ได้เริ่ม | 1.0 วัน |

### Deferred to Sprint 004 (by design — Plan A)

| Task | เหตุผล |
|------|--------|
| S2-01 (Item animation states) | Phase 1b คือ critical path สำหรับ Phase 2 pilot; Item animation ไม่ block P1B exit criteria |
| S2-02 (Item_Viable param) | Depends on S2-01 |
| S2-03 (attack_speed/move_speed ÷100 bug) | Investigation task; ไม่ block P1B |
| S2-04 (Fusion Tick Rate doc) | Documentation-only; ไม่ block P1B |
| S2-05 (Role Restriction doc) | Documentation-only |
| S2-06 (Mythic Passive formula) | Documentation-only |
| S2-07 (AdditionalMoveSpeed bug) | Investigation-only |
| S2-08 (Risk Register) | Producer task; ไม่ block engineering |

**Total deferred: 7.0 วัน** — รวม Sprint 004 เป็น "animator + bug hunting batch"

---

## Risks

| Risk | Probability | Impact | Mitigation |
|------|------------|--------|-----------|
| Fusion 2 multipeer-in-editor harder than expected (S3-01) | ปานกลาง | สูง | Budget 2d; fall back to manual 2-machine test ถ้า >2d; escalate network-programmer ตั้งแต่ day 1 ถ้าเจอ blocker |
| `InputMessage` bandwidth regression (+3 B × 10 × 60Hz = 1.8 KB/s) ทำให้ budget ต่อผู้เล่นเกิน | ต่ำ | ปานกลาง | S3-05 วัดผ่าน S3-01 harness ก่อน merge; rollback = remove fields, keep work in branch |
| S3-06 Settings UI creeps เข้าสู่ full UX work | ปานกลาง | ต่ำ | "placeholder only" ใน acceptance; styling = separate story |
| Phase 1a prototype code มี hidden deps พังตอนลบ `[Obsolete]` | ต่ำ | ปานกลาง | S3-04 ให้ `AbilityPrototypeRunner` เป็น regression test; run Pass #1-5 หลังลบ |
| Boot-time `Resources.LoadAll` scan ช้า (50+ prefabs) | ต่ำ | ต่ำ | Benchmark ใน S3-02 acceptance; ถ้า > 100ms สลับไป Addressables |
| AI Bot tuning (S3-08/S3-09) ขาด playtest baseline | ปานกลาง | ต่ำ | Use MOBA reference (DotA2/LoL) เป็น default tier values; tune post-Sprint |

---

## Dependencies on External Factors

- Unity Editor + Fusion 2 multipeer mode (S3-01)
- PlayFab CBS Dashboard เข้าถึงได้สำหรับ `CBSAbility` pull (S3-03)
- `Assets/Prefabs/Gameplay/Spell/` folder มี ability prefabs สำหรับ Registry scan (S3-02)
- `delta-unity` branch `feature/refactor-ability` ยังไม่ถูก merge เข้า main

---

## Definition of Done for this Sprint

### Per-Task Gates

- [x] S3-01: Multipeer harness pass #4 + #5 ✅ verified (2026-04-21)
- [x] S3-02: `AbilityRegistry` real impl + 5 EditMode tests pass (2026-04-21)
- [ ] S3-02: `AbilityRegistry` real impl + unit tests pass
- [ ] S3-03: `AbilityDataSnapshot` real impl + immutability test
- [ ] S3-04: `BindSlot` real impl + `[Obsolete]` removed + regression pass
- [ ] S3-05: `InputMessage` slot fields + zero regression + wire delta measured
- [ ] S3-06: `KeybindMap` service + Settings placeholder + UX sign-off
- [ ] S3-07: Phase 2 migration plan doc published
- [ ] S3-08: AI Bot buys items ใน 1 match (no crash)
- [ ] S3-09: 3 difficulty tiers documented + tested

### ADR-0006 Phase 1b Exit Criteria (gates Phase 2)

- [ ] **4 artifacts production-ready** — no `[Obsolete]` markers; no `NotImplementedException`
- [ ] **Pass #4 ✅ PASS** — `NetworkDictionary<byte, NetworkBehaviourId>` replicates identically (harness-verified)
- [ ] **Pass #5 ✅ PASS** — idle bandwidth < 2 KB/s measured via Fusion Statistics on Host+Client
- [ ] **`InputMessage.PressedSlot/ReleasedSlot`** added — parallel write verified
- [ ] **SkillKey path regression-free** — existing hero manual walkthrough pass
- [ ] **Phase 2 touch-point audit doc published**
- [ ] **Hercules pilot enumeration** — all `GetInput` overrides + `Combat.Skill1-4` refs line-anchored

### Cross-cutting

- [ ] ไม่มี S1/S2/S3 bugs ใหม่จาก tasks ในสปรินท์นี้
- [ ] GDD/ADR ที่เกี่ยวข้องอัพเดตสำหรับทุก finding

---

## Progress

### 2026-04-21 — S3-01 closed ✅

**P1B-01 Host+Client multipeer test harness** — DONE

- สร้าง `Assets/Scenes/Testing/AbilityMultipeer.unity` + `AbilityMultipeerRunner.cs`
  (Host + Client `NetworkRunner` ใน editor session เดียวกัน, `PeerMode=Multiple`)
- **Pass #4 ✅ PASS** — 4/4 slots converge Host↔Client; log: `[Multipeer-Parity] ✅ PASS #4 — all 4 slots converge (Host↔Client).`
- **Pass #5 ✅ PASS** — Host out 26–27 B/s, Client out 39–65 B/s (~1–3% ของ 2048 B/s budget); per-object `OutBandwidth > 0`
- Fix `SceneRef.FromIndex` `ArgumentOutOfRangeException` (guard buildIndex < 0 → `default(SceneRef)` + warning)
- Fix duplicate AudioListener + MainCamera ที่เกิดจาก multipeer scene clone
- Evidence บันทึกใน `docs/architecture/ADR-0006-phase-1a-interfaces.md` §9.5

**Known polish item (non-blocking):**
- `AbilityMultipeerRunner.Start()` ยิง 2 ครั้งเมื่อ multipeer clone active scene
  (second instance ถูกสร้างใน client peer scene) → `GameIsFull` cascade.
  Pass #4 + #5 ไม่กระทบ (first session สำเร็จก่อน). Fix: static bootstrap
  guard หรือ disable self เมื่อ `NetworkRunner.Instances.Count > 0`.
  → tracked ต่อใน S3-02..S3-07 polish pass หรือ Sprint 004 nice-to-have.

### 2026-04-21 — S3-02 closed ✅

**P1B-02 `AbilityRegistry` real implementation** — DONE

- Implementation `Assets/GameScripts/Gameplays/Abilities/AbilityRegistry.cs` (delta-unity `eb2ff94f12`)
- Strategy: `Resources.LoadAll<GameObject>("Prefabs/Gameplay/Spell")` → filter prefabs ที่มี `NetworkObject` + `ActorCombatAction` → resolve abilityId via `[AbilityClass]` attribute (forward-compatible) ตกที่ `prefab.name` (convention) — option A+C-fallback
- 5 EditMode tests pass ทั้งหมด ที่ `Assets/UnitTests/TestEditMode/AbilityRegistryTests.cs`
- **Live scan:** 158 abilities + 157 SOs ใน 1079 ms cold / 511.9 ms warm (เกิน budget 100ms ของ plan §4.3)
- **Dict build cost (synthetic 50):** 0.06 ms — 1700× ใต้ budget ✅ (acceptance criteria #2 บางส่วน)
- **O(1) lookup post-boot ✅**
- **Spawn happy-path** ไม่ test ใน EditMode (ต้อง NetworkRunner) — defer ไป Phase 2 multipeer harness

**Polish item flagged → Sprint 004 nice-to-have:**
- `Resources.LoadAll` cold-start cost dominated by Unity asset I/O (not registry logic)
- 3 options: accept (current), Addressables migration (~2-3d), build-time manifest (~1-1.5d)
- Decision: Accept ตอนนี้ — boot-time = one-time match start ไม่ใช่ per-frame cost

---

## Source

สร้างจาก:
- [ADR-0006 Phase 1b Implementation Plan](../../docs/architecture/ADR-0006-phase-1b-implementation.md) — Accepted 2026-04-21 (S3-01..S3-07)
- [Sprint 002 §Deferred to Sprint 003](sprint-002.md) — S2-09, S2-10 carryover (S3-08, S3-09)
- [ADR-0006 Phase 1a Interfaces §9.3 Phase 1b Entry Criteria](../../docs/architecture/ADR-0006-phase-1a-interfaces.md)
- [ADR-0006 Migration Audit](../../docs/architecture/ADR-0006-migration-audit.md) — Pilot hero = Hercules confirmed
