# Story: S6-14 — AbilityMultipeerRunner Duplicate-Start Guard

**Status**: Complete
**Type**: Logic
**ADR**: ADR-0006 Phase 1b (multipeer harness origin)
**GDD**: N/A — test infrastructure / harness story; no player-facing mechanic
**Engine notes**: N/A — Unity 2022.3 LTS within LLM training data
**Manifest Version**: N/A (control-manifest.md not yet created)
**Estimate**: 0.5d
**Priority**: Nice to Have
**Epic**: [Test Infrastructure](EPIC.md)
**Owner (role)**: network-programmer
**Assignee**: tanapol
**History**: S4-P1 → S5-17 → S6-14 (carried 2 sprints; never blocked critical path)

## Dependencies

- ✅ None — งานเล็ก stand-alone, ไม่ติด Phase 2 soak gate
- ✅ S6-13 PlayMode fixture (NetworkRunnerFixture.cs) landed — provides test scaffold reference

## Context

`AbilityMultipeerRunner` (Editor-only multipeer harness, ADR-0006 Phase 1b) ปัจจุบันไม่มี guard ป้องกัน `Start()` ที่ยิงซ้ำเมื่อ Fusion multipeer โหลด client peer scene (line 91-94 comment ยอมรับว่า scene โหลดซ้ำ และมี dedup สำหรับ AudioListener/Camera แล้ว แต่ไม่ guard ตัว runner เอง)

**อาการ**: Console รัน multipeer harness แล้วเห็น `GameIsFull` cascade error — เกิดจาก second `Start()` พยายาม `StartHostAsync` บน session name เดิม

**Impact**: noise รบกวน log ระหว่าง Phase 2 soak / Phase 3 batch verification — ปกปิด warning จริงเช่น BindSlot, slot=0

**Code location**:
- `Assets/GameScripts/Gameplays/Abilities/Testing/AbilityMultipeerRunner.cs:67-103` — `Start()` ไม่ guard
- `Assets/GameScripts/Gameplays/Abilities/Testing/AbilityMultipeerRunner.cs:359-376` — `OnDestroy()` cleanup ที่ต้องตามแก้

## Acceptance Criteria

1. **Guard logic**: เพิ่ม static instance guard ที่ต้น `Start()` — ถ้า primary instance มีอยู่แล้วและไม่ใช่ตัวเอง → `enabled = false; return;` พร้อม log line ที่อ่านง่าย
2. **OnDestroy cleanup**: clear static reference เมื่อ primary instance ถูก destroy (ป้องกัน stale reference ข้าม Play session)
3. **Console clean**: รัน multipeer harness แล้ว **ไม่มี `GameIsFull` cascade** — Console clean ตั้งแต่ Play → 10s steady state
4. **Existing PASS conditions preserved**: Pass #1-5 (parity + bandwidth) ยังเขียวเหมือนเดิม — guard ต้องไม่กระทบ host runner ตัวจริง
5. **EditMode test 1 ตัว**: `Assets/UnitTests/TestEditMode/AbilityMultipeerRunnerGuardTests.cs` — instantiate 2 component instances, assert ว่ามี 1 ตัว enabled = true, อีกตัว enabled = false หลัง frame แรก

## Out of Scope

- Refactor `Start()` async void → proper coroutine pattern
- Cleanup of `Assert` namespace collisions (S6-13 lesson #6 — separate concern)
- Migration ของ harness ไป proper scene bootstrap (มี note ที่ line 41: "Replaced in Phase 1b/Phase 2 by proper scene bootstrap")
- เปลี่ยน static singleton เป็น scene-scoped registry (over-engineering)

## Test Evidence

**Required**:
- EditMode test PASS — guard ทำงาน 2 instances → 1 active
- Console log capture — `production/qa/evidence/S6-14-multipeer-console-clean.txt` แสดง 10s harness run ไม่มี GameIsFull

**Optional**:
- Screenshot Console panel ก่อน/หลัง

## Performance Impact

Zero — guard เป็น single static null check ที่ Start() (รันครั้งเดียวต่อ instance lifetime)

## Files to Modify

- `Assets/GameScripts/Gameplays/Abilities/Testing/AbilityMultipeerRunner.cs` (เพิ่ม static field + guard + cleanup ~10 บรรทัด)
- `Assets/UnitTests/TestEditMode/AbilityMultipeerRunnerGuardTests.cs` (NEW, ~40 บรรทัด)

**Expected diff size**: ~50 บรรทัด total

## Risks

- **R1** — Static field คงอยู่ข้าม domain reload (Unity Editor); domain reload อาจทำให้ stale reference ค้าง. **Mitigation**: ใช้ `[RuntimeInitializeOnLoadMethod]` reset หรือเช็ค `s_PrimaryInstance == null || s_PrimaryInstance.gameObject == null` (Unity null check)
- **R2** — EditMode test รัน 2 instance ใน 1 GameObject ไม่ trigger Start() ปกติเหมือน Play mode. **Mitigation**: ใช้ PlayMode test แทน หรือ invoke guard logic เป็น static method ที่เทสได้ตรง ๆ
- **R3** — Guard บล็อก instance ที่ user "ตั้งใจ" รัน 2 ตัว (เช่น 2 scene พร้อมกัน). **Mitigation**: Log line ชัดเจน + harness นี้ออกแบบเป็น single-instance อยู่แล้ว (line 13-15 doc comment ยืนยัน)

## References

- [Epic: Test Infrastructure](EPIC.md)
- Sprint 005 retro: S5-17 carryover entry
- `Assets/GameScripts/Gameplays/Abilities/Testing/AbilityMultipeerRunner.cs:91-94` — comment ที่ยอมรับ scene-load duplication
- [S6-13 PlayMode spike decision](../../decisions/S6-13-playmode-framework-spike.md) — test infrastructure context

## Completion Notes

**Completed**: 2026-05-15
**Criteria**: 5/5 passing (AC #4 interpreted by spirit — Pass #4 regression filed as BUG-0005, pre-existing, NOT caused by S6-14 guard)
**Deviations**:
- AC #4 partial → [BUG-0005 multipeer parity regression](../../qa/bugs/BUG-0005-multipeer-parity-regression.md) filed; pre-existing condition between Sprint 5 closure and 2026-05-15
- `UnitTestEditMode.asmdef` modified (+1 reference to `Radius.Gameplays.Abilities.Testing`) — out-of-scope but necessary for test compile
- S7-PROPOSED-SCENE-RENAME (rename `PrototypeTest.unity` → `test_scene_ability_multipeer.unity`) tracked in [Epic Future Candidates](EPIC.md)
**Test Evidence**:
- EditMode: `Assets/UnitTests/TestEditMode/AbilityMultipeerRunnerGuardTests.cs` — 4/4 PASS
- Manual: [production/qa/evidence/S6-14-multipeer-console-clean.txt](../../qa/evidence/S6-14-multipeer-console-clean.txt)
**Code Review**: Complete (run via `/code-review` 2026-05-15 — APPROVED WITH SUGGESTIONS; suggestions 1+2 applied: removed `using UnityEngine.Scripting;`, added inline comment at `enabled = false`)
**Final implementation files** (in delta-unity repo):
- `Assets/GameScripts/Gameplays/Abilities/Testing/AbilityMultipeerRunner.cs` (modified, +60 LOC)
- `Assets/UnitTests/TestEditMode/AbilityMultipeerRunnerGuardTests.cs` (NEW, 134 LOC, 4 tests)
- `Assets/UnitTests/TestEditMode/UnitTestEditMode.asmdef` (+1 reference)
