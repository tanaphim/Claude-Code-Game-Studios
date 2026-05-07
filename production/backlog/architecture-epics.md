# Architecture Epics Backlog

**Created:** 2026-05-07
**Status:** Active backlog — re-prioritize at each sprint boundary
**Source:** User-raised refactor priorities (2026-05-07 session)

วิธีใช้: รายการนี้เก็บ epic-level architecture work ที่ใหญ่กว่า 1 sprint
หรือต้อง ADR. แต่ละ epic มี linkage กับ ADR ที่มีอยู่และ tier เพื่อช่วย
ตัดสินใจตอนวาง Sprint plan

---

## Tier Definitions

| Tier | ความหมาย | เกณฑ์ |
|------|---------|-------|
| **T1 — In flight** | กำลังทำอยู่ใน roadmap หลัก | อยู่ใน Phase 2/3/4 ของ ADR-0006 |
| **T2 — Next-up** | ทำหลัง Tier 1 ปิด, extends current refactor | ต้อง depend Phase 2 ship |
| **T3 — Independent** | ทำแยกได้, ไม่ block roadmap หลัก | ADR ของตัวเอง, ไม่กระทบ ability migration |
| **T4 — Discuss** | scope vague หรือ risk สูง, ต้องคุยก่อน plan | ต้องการ pain-point breakdown หรือ profiling baseline |

---

## Epic Index

| Epic | Title | Tier | Owner ADR | Earliest Sprint | Est |
|------|-------|------|-----------|-----------------|-----|
| **E-01** | Combat/Skill structure decoupling (script ↔ animation) | T1 | ADR-0006 Phase 2/3/4 | Sprint 005 (in flight) | 3.75d Phase 2, ~10d total |
| **E-02** | Slot-based input (key remap + cross-hero ability use) | T1 | ADR-0006 Phase 1b → Phase 4 | Sprint 005 (Hercules pilot) | continuation |
| **E-03** | Split Input/Move/Target out of `ActorCombatAction` | T2 | New ADR (post Phase 2) | Sprint 007+ | 5–8d |
| **E-04** | Avatar Mask — upper/lower body anim split | T3 | New ADR (small) | Sprint 008+ (when needed) | 2–3d |
| **E-05** | `Stat`/`StatusEffect`: NetworkObject → NetworkStruct | T4 | New ADR (gated by profiling) | TBD | 5–10d (high uncertainty) |
| **E-06** | `vfxObject`: NetworkObject → GameObject | T3 | **ADR-0007** (drafted) | Sprint 006–007 | 2–3d |
| **E-07** | Skill Object — non-rect/circle AOE shapes (cone, half-circle, etc.) | T3 | New ADR after E-08 | Sprint 008+ | 3–5d |
| **E-08** | Decouple `SkillObject`/`StatusEffect` metadata from `CBSAbility` | T2 | ADR-0006 Phase 3 (extend scope) | Sprint 006 | 3–5d |
| **E-09** | Replace/wrap CBS metadata system | T4 | TBD (needs pain-point audit) | TBD | unknown |

---

## E-01 — Combat/Skill structure decoupling

**Tier:** T1 (in flight)
**Linkage:** [ADR-0006 Unified Ability System](../../docs/architecture/ADR-0006-unified-ability-system.md), [Phase 2 Migration Plan](../../docs/architecture/ADR-0006-phase-2-migration-plan.md)

**ปัญหา:** script + animation ผูกกันที่ `SkillKey` enum + `Buttons.Q/W/E/R` literal
ทำให้ใช้สกิลตัวอื่นไม่ได้ และ animator clip ผูกกับ method name `PerformQ`/`ReleaseR` ตายตัว

**สิ่งที่ทำอยู่:**
- ✅ Phase 1b (Sprint 003 closed): `AbilityRegistry`, `AbilityComponent.BindSlot`,
  `AbilityDataSnapshot`, `KeybindMap`, `InputMessage.PressedSlot`
- 🟢 Phase 2 (Sprint 005, 3.75d): Hercules pilot — `AnimationEvent` Option A
  (keep 42 method names, route through `GetActiveSlot()`); 2 Hercules files refactored
- ⏸ Phase 3: hero migration recipe (~12 heroes) + drain non-Hercules call sites
- ⏸ Phase 4: ลบ `SkillKey` enum + `Buttons.Q/W/E/R` ทิ้ง

**Decision:** ห้ามทำ big-bang refactor — pilot 1 hero ก่อน, codify recipe, แล้วทยอย

---

## E-02 — Slot-based input (key remap + ability swap)

**Tier:** T1 (in flight, merged into E-01)
**Linkage:** ADR-0006 Phase 1b/2/3, KeybindMap service (S3-06)

**ปัญหา:** ปุ่มตายตัว, รับ skill จาก `Buttons.IsSet(Buttons.X)` ตรงๆ → สลับปุ่มไม่ได้,
ยืม skill ตัวอื่นไม่ได้

**สิ่งที่ทำอยู่:** เป็น sub-goal ของ E-01 — Phase 1b ส่ง KeybindMap + PressedSlot แล้ว,
Phase 2 wire Hercules, Phase 3 wire ที่เหลือ. ไม่แยก epic

**Status:** จบเมื่อ E-01 จบ (Phase 4 ลบ enum)

---

## E-03 — Split Input/Move/Target out of ActorCombatAction

**Tier:** T2 (after Phase 2 ships)
**Linkage:** Extends Phase 2 §5 Pattern A/B/C/D extraction; New ADR needed

**ปัญหา:** `ActorCombatAction` เป็น god class — รับ input + move + target + cooldown +
state machine + animation event ทั้งหมดในไฟล์เดียว (~2000+ บรรทัด)

**Proposed approach:**
- Component split: `AbilityInputBinding`, `AbilityMovementController`, `AbilityTargeting`
- Pilot บน Hercules หลัง Phase 2 stable ≥1 สัปดาห์
- ใช้ Pattern A/B/C/D extraction ของ Phase 2 เป็น stepping stone

**Entry criteria:** Phase 2 closed + Hercules in dev branch ≥1 week, no slot-related bugs

**Why T2 not T1:** ใหญ่เกิน Phase 2 scope, และต้องเห็น Phase 2 facade pattern stable
ก่อนถึงจะรู้ว่า component boundary ควรอยู่ตรงไหน

---

## E-04 — Avatar Mask (upper/lower body anim split)

**Tier:** T3 (independent, defer)
**Linkage:** None ที่มีอยู่ — Unity Animator feature ล้วน, New ADR ขนาดเล็ก

**ปัญหา:** ตอนนี้ animation full-body → สกิลที่อยากให้โจมตีระหว่างวิ่งทำไม่ได้
(เช่น ranged hero ยิงพร้อม strafe, หรือ Hercules cleave ระหว่างเดิน)

**Why defer:**
- ไม่กระทบ architecture (เป็น Unity Animator config)
- Hercules ตัวแรกยังไม่ design ให้ใช้ feature นี้
- ถ้าทำตอนนี้ + Phase 3 เปลี่ยน animator structure (state names, layer count) →
  อาจต้องแก้ซ้ำ
- ไม่ block อะไรเลย

**Entry criteria:** มี hero ที่ design ให้ต้องใช้ (lock spec ก่อน implement)
**Engine note:** Unity 2022.3 LTS — Avatar Mask + Animator Layer weight = stable API

---

## E-05 — Stat/StatusEffect: NetworkObject → NetworkStruct ⚠️

**Tier:** T4 (gated by profiling)
**Linkage:** [ADR-0006 Phase 2 §1 Out of scope](../../docs/architecture/ADR-0006-phase-2-migration-plan.md) "epic backlog item #5"; New ADR needed

**ปัญหาที่สงสัย:** `Stat` + `StatusEffect` เป็น `NetworkObject` per-instance →
spawn cost สูง, replication overhead, GC pressure ตอน status apply/remove

**⚠️ ห้ามทำโดยไม่มี profiling baseline ก่อน:**
- ตอนนี้ "รู้สึกว่ามันหนัก" ≠ "วัดแล้วว่ามันหนัก"
- Touches replication ของ **ทุก actor** = high blast radius
- Premature optimization = waste sprint capacity + risk regression

**Required precursors before plan:**
1. Profile match จริง (10 min, 2 player) — bandwidth, CPU, GC
2. ระบุ stat/status events ต่อนาทีใน steady-state combat
3. ตั้ง budget — ถ้าปัจจุบันต่ำกว่า budget แล้ว = defer indefinite
4. ถ้าเกิน budget → เขียน ADR + migration plan + rollback

**Estimate:** 5–10d (high uncertainty — มี hidden coupling เยอะ)

---

## E-06 — vfxObject: NetworkObject → GameObject ⭐

**Tier:** T3 (recommended near-term)
**Linkage:** **[ADR-0007](../../docs/architecture/ADR-0007-vfx-object-network-decoupling.md)** (drafted)

**ปัญหา:** `vfxObject` ถูก spawn เป็น `NetworkObject` แต่ส่วนใหญ่เป็น visual-only
(particle, decal, hit flash) → กิน network bandwidth + spawn cost โดยไม่จำเป็น

**Proposed approach (เห็นใน ADR-0007):**
- Audit ทุก `vfxObject` — categorize: (a) pure visual / (b) needs network state
- (a) → `GameObject` + local instantiate via animation event หรือ ability lifecycle hook
- (b) → keep `NetworkObject` (เช่น damage zone ที่ persist หลาย tick)

**Why recommend near-term:**
- Small, low-risk (additive — old path ยังทำงานระหว่าง migrate)
- Clear win (ลด bandwidth + GC)
- ไม่ block Phase 2/3 — ทำคู่ขนานได้

**Earliest sprint:** Sprint 006 หรือ 007 (หลัง Sprint 005 Phase 2 stable)

---

## E-07 — Skill Object: non-rect/circle AOE shapes

**Tier:** T3 (depends on E-08)
**Linkage:** Feature work, New ADR หลัง E-08

**ปัญหา:** AOE shapes ปัจจุบันรองรับแค่ rectangle + circle → ไม่มี cone, half-circle,
ring, arbitrary mesh

**Why depends on E-08:** ถ้าทำตอนนี้ จะเพิ่ม shape config field ลงใน `CBSAbility`
อีก ทำให้ E-08 (แยก metadata จาก CBSAbility) ต้อง migrate ของเพิ่มอีกชั้น

**Sequence:** E-08 ปิดก่อน (แยก SkillObject metadata) → E-07 เพิ่ม shape system
ใน SkillObject ที่ decoupled แล้ว

**Estimate:** 3–5d (geometry helpers + editor authoring + targeting integration)

---

## E-08 — Decouple SkillObject/StatusEffect metadata จาก CBSAbility

**Tier:** T2 (extends Phase 3 scope)
**Linkage:** [ADR-0006 Phase 2 §6.3](../../docs/architecture/ADR-0006-phase-2-migration-plan.md) — "Phase 3 introduces editor migration tool for ~25 SkinObject assets"

**ปัญหา:** `CBSAbility` กลายเป็น god record — เก็บ ability data + skill object refs +
status effect refs + vfx refs + animation triggers ทั้งหมด → designers แก้ field
หนึ่งกระทบหลายระบบ, code ที่อ่าน CBSAbility ต้อง understand ทุก layer

**Proposed approach:**
- Phase 3 plan ขยาย scope: ไม่ใช่แค่ `SkinObject` migration tool แต่รวม
  `SkillObjectDictionary` + `SkillVfxDictionary` + `StatusEffectDictionary`
- แต่ละ category มี SO หรือ CBS record แยกของตัวเอง
- `CBSAbility` เก็บแค่ reference ID (ตาม convention ที่ memory ระบุไว้:
  "SO เก็บ asset refs + AbilityId; CBS เก็บ designer-editable values")

**Entry criteria:** Phase 2 closed (migration tooling pattern proven บน 25 SkinObject ก่อน)

**Sequence ก่อน E-07/E-09:** E-08 มาก่อน E-07 (shape system needs clean SkillObject)
และก่อน E-09 (ลด blast radius ของ CBS replacement)

---

## E-09 — Replace/wrap CBS metadata system ⚠️

**Tier:** T4 (vague — needs pain-point audit)
**Linkage:** TBD — depends on E-08 outcome + pain-point audit

**ปัญหาที่ user ระบุ:** "CBS ใช้ค่อนข้างยาก"

**ทำไมยังไม่ plan:**
- "ใช้ยาก" = อะไร? — UX dashboard? schema rigidity? versioning? authoring loop slow?
  no diff/branch? designer ไม่กล้าแก้?
- CBS = PlayFab Cloud Build Service backend → "เปลี่ยนระบบ" = เปลี่ยน backend =
  data migration risk สูง + designer workflow disruption
- E-08 อาจแก้ pain ส่วนใหญ่อยู่แล้ว (ลด field ที่ designers ต้องแตะ)

**Possible approaches (ก่อนตัดสินใจ scope):**
- **Option A — Authoring wrapper:** เก็บ CBS เป็น runtime backend แต่สร้าง Unity
  Editor tool ให้ designers แก้ผ่าน UI ใน Unity → push ขึ้น CBS อัตโนมัติ
- **Option B — SO-first authoring:** ScriptableObject เป็น source-of-truth,
  CBS sync จาก SO ผ่าน build pipeline (CBS = distribution layer เท่านั้น)
- **Option C — Replace CBS:** เปลี่ยน backend ทั้งหมด (เช่น Git-based config repo,
  Notion API, custom Azure Functions endpoint) — high risk
- **Option D — Do nothing:** ถ้า E-08 แก้ pain ส่วนใหญ่ → CBS UX ยอมรับได้

**Required before planning:**
1. Pain-point audit — สัมภาษณ์ designers, list 5 painful workflows ที่สุด
2. รอ E-08 ส่งมอบก่อน → re-audit ดูว่า pain ลดเหลือเท่าไหร่
3. ค่อยตัดสิน Option A/B/C/D

---

## Sequencing Summary (Recommended Roadmap)

```
Sprint 004 ────────────────────────────── carryover (animator + bugs + AI Bot)
Sprint 005 ── E-01/E-02 Phase 2 Hercules pilot (3.75d)
Sprint 006 ── E-01/E-02 Phase 3 prep + E-08 plan (extend Phase 3) + E-06 ADR
Sprint 007 ── E-06 vfxObject migration (2–3d) + Phase 3 hero rollout begins
Sprint 008 ── E-03 Component split ADR (after 1 week Phase 2 stable)
Sprint 009 ── E-04 avatar mask (when first hero needs it)
Sprint 010 ── E-07 AOE shapes (after E-08 closes)
TBD       ── E-05 NetworkStruct (after profiling shows need)
TBD       ── E-09 CBS replacement (after E-08 + pain audit)
```

**ห้ามทำพร้อมกัน:** E-01 + E-03 + E-08 (ทั้ง 3 ตัวแตะ ability stack — Phase 2 เสี่ยง)
**ทำคู่ขนานได้:** E-04, E-06, E-05 (independent stacks)

---

## Re-evaluation triggers

Re-prioritize backlog เมื่อ:
- Phase 2 (Sprint 005) ปิด → ขยับ E-03/E-08 จาก T2 → T1
- มี profiling data ใหม่ → ขยับ E-05 จาก T4 → T3 หรือ defer permanent
- มี pain-point audit ของ CBS → ขยับ E-09 จาก T4 → T2/T3
- Sprint 004 carryover ค้าง 4th sprint → freeze new epics, focus on closure

---

## References

- [ADR-0006 Unified Ability System](../../docs/architecture/ADR-0006-unified-ability-system.md)
- [ADR-0006 Phase 2 Migration Plan](../../docs/architecture/ADR-0006-phase-2-migration-plan.md)
- [ADR-0007 vfxObject Network Decoupling](../../docs/architecture/ADR-0007-vfx-object-network-decoupling.md) — drafted Sprint 006 candidate
- [Sprint 004 plan](../sprints/sprint-004.md)
- [Memory: SO เก็บ assets, CBS เก็บ design values](../../../C--GitHub-Delta-Project/memory/feedback_balance_data_cbs.md) — convention ที่ใช้ pattern E-08
