# Next-Session Prompts

**Created:** 2026-04-21 (end of session that shipped Phase 1b)
**Status:** Phase 1b SHIPPED + Sprint 004 planned + tag `phase-1b-complete` pushed

วิธีใช้: copy-paste prompt ที่ตรงกับ track ที่อยากทำต่อ ใส่ใน Claude session ใหม่

---

## 🟢 Prompt มาตรฐาน (ทุก track — opening)

```
อ่าน production/session-state/active.md ก่อนเพื่อ recover context
จาก session ก่อน (Phase 1b shipped, Sprint 004 planned, tag
phase-1b-complete pushed) แล้วรายงานสถานะปัจจุบันให้ฟัง — มี
ปัญหา / dirty state / commits ที่ยังไม่ push อะไรไหม

หลังจากนั้นรอคำสั่งจากผมว่าจะทำ track ไหนต่อ
```

---

## 🅰️ Track A — Sprint 004 kickoff (carryover batch)

```
อ่าน production/session-state/active.md + production/sprints/sprint-004.md
+ production/retrospectives/sprint-003.md §5 (action items)

เป้าหมาย: เริ่ม Sprint 004 — animator + bugs + AI Bot batch

อยากเริ่มจาก action item #1 ใน retrospective ก่อน:
investigate Photon STUN timeout + Version mismatch ที่ block ตัว
production playtest ใน editor (เป็น blocker ของ S4-09 AI Bot)

ช่วย scout codebase หาว่า Version check + STUN config อยู่ที่ไหน
แล้วเสนอ 2-3 hypothesis ของ root cause + วิธี reproduce แต่ละอัน
```

---

## 🅱️ Track B — Sprint 005 prep / Phase 2 Hercules pilot

```
อ่าน production/session-state/active.md + docs/architecture/ADR-0006-phase-2-migration-plan.md

เป้าหมาย: เริ่ม Phase 2 Hercules pilot ล่วงหน้าก่อน Sprint 005
(ส่วนที่ไม่ depend Sprint 004 carryover)

P2-03 (CBSAbility.Slot field + EffectiveSlot shim + SkillKeyToSlot
mapper, 0.25d) ไม่ depend อะไรเลย — เริ่มได้ทันที

ช่วยทำตามนี้:
1. อ่าน CBSAbility.cs ปัจจุบัน + AbilityDataSnapshot.cs (เป็น reader)
2. propose schema change (additive, non-breaking ตาม plan §6.1)
3. รอ approval ก่อน edit
```

---

## 🅲 Track C — Phase 1b polish

```
อ่าน production/session-state/active.md + production/sprints/sprint-004.md
§"Nice to Have" (S4-P1..S4-P4)

เป้าหมาย: ปิด polish item ที่ cheap ที่สุดก่อน — S4-P1
AbilityMultipeerRunner duplicate-Start guard (~0.5d)

Bug นี้ flag ไว้ตั้งแต่ S3-01: Fusion multipeer clone scene root
ทำให้ Start() ยิง 2 ครั้ง → GameIsFull cascade. Pass #4/#5 ไม่
กระทบ แต่ console รก

ช่วย:
1. อ่าน AbilityMultipeerRunner.cs (ที่ delta-unity feature branch)
2. propose 2 options ของ guard (static bootstrapped flag VS
   NetworkRunner.Instances.Count check)
3. รอ approval ก่อน implement
```

---

## 🔧 Prompt สำหรับ commit history check (กลับมาหลังหายไปนาน)

```
checkout branch ปัจจุบันแล้วรันคำสั่ง:
- git status (ทั้ง Delta-Project worktree + /c/GitHub/delta-unity)
- git log --oneline -5 ของทั้ง 2 repos
- git fetch origin + ดู ahead/behind dev

แล้วรายงานให้ฟังว่า:
- มี commit ใหม่บน dev ที่ควร merge ไหม
- มี dirty state ไหน ที่อาจเป็นงานค้างจาก session เก่า
- branch sync กับ origin หรือไม่
```

---

## 📝 Prompt ทั่วไป — สรุป progress รวม

```
อ่าน production/session-state/active.md + production/retrospectives/sprint-003.md

สรุปให้ฟังเป็นภาษาไทย 1 หน้า:
- Phase 1b ส่งมอบอะไรบ้าง
- Sprint 004 มีงานอะไรบ้าง
- Sprint 005 (Phase 2) เตรียมอะไรไว้แล้ว
- มี open questions / blockers อะไรค้างจาก session ก่อน

รายงานเสร็จแล้วรอคำสั่ง
```

---

## 💡 คำแนะนำลำดับการใช้

1. **เริ่มทุก session** ด้วย **Prompt มาตรฐาน** (🟢)
2. **ถ้าหายไปนาน** (≥1 สัปดาห์) — เพิ่ม **Prompt commit history check** (🔧) ก่อน track หลัก
3. **อยากดู progress รวม** ก่อนตัดสินใจ — ใช้ **Prompt ทั่วไป** (📝)
4. **เลือก track** ตาม priority:
   - มี team พร้อม Unity Editor / animator / CBS dashboard → **Track A**
   - อยาก lean ไป Phase 2 ก่อน → **Track B** (P2-03 หิวแสนหิว ทำได้ทันที)
   - มีเวลาน้อย / อยาก quick win → **Track C** (S4-P1 ~0.5d)

---

## 🔗 Quick reference

### Branches
- Delta-Project: `claude/hardcore-meninsky-191ea2` @ `2239a74` (origin synced)
- delta-unity: `feature/refactor-ability-claude` @ `1022c87dbb` (origin synced)

### Tag
- `phase-1b-complete` — ทั้ง 2 repos (immutable Phase 1b baseline)

### Key docs
- `production/session-state/active.md` — current state checkpoint
- `production/sprints/sprint-004.md` — next sprint plan
- `production/retrospectives/sprint-003.md` — what we learned
- `docs/architecture/ADR-0006-phase-2-migration-plan.md` — Phase 2 ready
- `docs/architecture/ADR-0006-phase-1b-implementation.md` — Phase 1b closure notes
