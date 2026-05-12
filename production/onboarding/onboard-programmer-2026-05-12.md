# Onboarding: Junior Gameplay Programmer (Sprint 005)

## Project Summary

**Delta Project** — MOBA-style game บน Unity 2022.3.62f1 (URP, C#, Photon Fusion 2, PlayFab backend).
Project แบ่งเป็น 2 repo:
- **delta-unity** — Unity project จริง (โค้ดทั้งหมดอยู่นี่)
- **my-game / Delta-Project** (repo นี้) — design hub + production management (GDDs, ADRs, sprints, bugs)

ปัจจุบันอยู่ที่ **Sprint 005 (2026-05-09 → 2026-05-22)** — Phase 2 ของ ability system migration (Hercules เป็น pilot hero)

## Your Role: Gameplay Programmer

แปลง design docs เป็น code ที่ data-driven, มี unit test, และตามสเปคเป๊ะ
- **Reports to:** `lead-programmer`
- **รับสเปคจาก:** `game-designer`, `systems-designer`
- **คุยข้างเคียง:** `network-programmer` (Fusion), `ui-programmer`, `ai-programmer`

## Workflow แบบ Claude Code (สำคัญมาก — คุณไม่เคยใช้)

**Collaboration Protocol:** Question → Options → Decision → Draft → Approval

ห้าม agent (รวมตัวคุณเอง) เขียน/แก้ไฟล์โดยไม่ขออนุญาตก่อน — ทุกการแก้ไฟล์ต้องถาม "May I write this to [filepath]?" และรอ user ตอบ "yes"

**Pattern พื้นฐานเวลาทำ story:**
1. รัน `/story-readiness [story-id]` — เช็คว่า story พร้อม implement มั้ย
2. รัน `/dev-story [story-id]` — implementation พร้อม test
3. รัน `/code-review` — ตรวจคุณภาพโค้ด
4. รัน `/story-done` — ปิด story + อัพเดต status

## Project Architecture (สำคัญสำหรับ Sprint 005)

### Ability System (ADR-0004, ADR-0006, ADR-0008)
- **ADR-0004** — Actor/Combat/Action/Skill pipeline (architecture พื้นฐาน)
- **ADR-0006** — Unified Ability System migration (Phase 2 = sprint นี้)
- **ADR-0008** — Slot Binding via CBSUnit (supersedes ADR-0006 §6.1 — pivot ตอน 2026-05-08)
  - **Key concept:** slot binding (Q/W/E/R/A/I) มาจาก `CBSUnit` (per-hero kit) ไม่ใช่ `CBSAbility` (per-ability) — ห้ามใช้ pattern แบบเก่า
- **ADR-0005** — Item/Animation type routing
- **ADR-0007** — VFX/Object/Network decoupling

### Data Model (จาก project memory)
- **ScriptableObjects** = เก็บ asset refs + AbilityId เท่านั้น
- **CBS (PlayFab Content)** = ทุก designer-editable field (stats, tags, targeting)
- **ห้าม hardcode** gameplay values

### Key Directories
| Directory | Contents | Your Interaction |
|-----------|----------|-----------------|
| `src/` | source code (in delta-unity repo) | เขียน/แก้ |
| `design/gdd/` | 40+ system GDDs | อ่านก่อนเขียน code |
| `docs/architecture/` | ADRs (binding decisions) | ต้องอ่านก่อน implement |
| `production/sprints/` | sprint plans + stories | งานคุณอยู่นี่ |
| `production/qa/bugs/` | bug tickets | ดู BUG-0001 ก่อน |
| `tests/` | unit/integration tests | เขียนทุก gameplay logic |

### Key Files to Read First
| File | Priority |
|------|----------|
| `CLAUDE.md` | 🔴 ต้องอ่าน |
| `docs/architecture/ADR-0008-slot-binding-via-cbsunit.md` | 🔴 ต้องอ่าน (sprint นี้ขึ้นอยู่กับมัน) |
| `docs/architecture/ADR-0006-phase-2-migration-plan.md` | 🔴 ต้องอ่าน |
| `production/sprints/sprint-005.md` | 🔴 ต้องอ่าน |
| `design/gdd/combat-skills-system.md` | 🟡 อ่านก่อนแตะ ability |
| `design/gdd/hero-system.md` | 🟡 |
| `.claude/agents/gameplay-programmer.md` | 🟡 (สเปคบทบาทคุณ) |

## Current Sprint Context (Sprint 005 — กำลังเดิน)

**Sprint Goal:** Phase 2 Hercules pilot — refactor ability system ให้ใช้ CBSUnit slot binding

### Stories ว่าง — คุณรับได้เลย
- **S5-04** (0.5d) — `ActorCombatAction` Pattern-A helper (`IsActiveSlotOwner`) — replace 5 blocks
- **S5-05** (0.5d) — Pattern-B/C/D one-liner replacements (4 sites)
- **S5-06** (0.5d) — `AnimationEvent` Option A wire 42 shim methods
- **BUG-0001** (S5-19, 0.5d) — Recall locomotion stuck — ยังไม่ปิด (BUG-0002 เพิ่งปิดไป)

### Already Done (Sprint 005)
S5-01, S5-02, S5-03, S5-07, S5-08, S5-09, BUG-0002

### Blocked
S5-10 (manual playtest) รอ S5-04..S5-06 เสร็จก่อน

**แนะนำ first task:** S5-05 (Pattern-B/C/D one-liner) — small, isolated, deps พร้อม

## Common Pitfalls (junior ต้องรู้)

1. **อย่าแก้ไฟล์โดยไม่ถาม** — Claude Code agents ต้อง ask permission ทุกครั้ง
2. **อย่าใช้ Claude เป็น autonomous coder** — propose architecture ก่อน, รอ approval, แล้วค่อยเขียน
3. **`CBSAbility.Slot` ห้ามใช้** — ถูก revert ตอน 2026-05-08 (ดู Mid-Sprint Pivot Log ใน sprint-005.md)
4. **NetworkBehaviour testability gap** — `ActorCombat` เป็น Fusion NetworkBehaviour, EditMode test ไม่ครอบคลุม networked state — pattern คือ extract static helpers (เช่น `ResolveSlotAction`) แล้ว test ตัวนั้น
5. **2 repos** — โค้ดอยู่ใน delta-unity, แต่ docs/sprints อยู่ใน my-game/Delta-Project — อย่าหลง
6. **ADR-0008 > ADR-0006 §6.1** — ถ้าเจอความขัดแย้ง ADR-0008 ชนะ

## First Tasks (Day 1-3)

1. **Day 1 — อ่าน docs**
   - CLAUDE.md, sprint-005.md, ADR-0006/0007/0008
   - GDDs: combat-skills-system, hero-system, actor-system
   - .claude/agents/gameplay-programmer.md
2. **Day 1 ตอนบ่าย — ลอง Claude Code**
   - รัน `/sprint-status` ดู snapshot
   - รัน `/help` ถามว่าควรทำอะไรต่อ
   - รัน `/story-readiness S5-05` เช็คว่าพร้อมทำมั้ย
3. **Day 2 — pick up S5-05**
   - รัน `/dev-story S5-05`
   - ทำตาม Workflow (propose → approval → implement → test)
4. **Day 3 — code review + close**
   - `/code-review` → `/story-done S5-05`
   - ถ้าเร็ว → หยิบ S5-04 หรือ BUG-0001 ต่อ

## Questions to Ask

- ใครเป็น lead-programmer คนปัจจุบัน (มนุษย์)?
- มี Unity Editor scene/prefab ไหนที่ต้อง attach manually หลัง S5-09 (warning log ยังขึ้นอยู่)?
- PlayFab CBS dashboard access — ใครถือ?
- Multipeer harness รันยังไง? (S5-10 ต้องใช้)
- งาน scene/prefab merge — convention ปัจจุบันคืออะไร?
