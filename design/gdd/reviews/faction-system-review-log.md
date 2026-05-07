# Faction System (FT11) — Review Log

> Revision history สำหรับ `design/gdd/faction-system.md`. Entry ใหม่สุดอยู่บนสุด.

## Review — 2026-04-27 — Verdict: APPROVED

- **Mode:** lean (single-session analysis, no specialist agents)
- **Scope signal:** L (4 hard downstream deps, 4 formulas, 11 core rules, new bottleneck per systems-index)
- **Specialists:** None (lean mode)
- **Blocking items:** 0 | Recommended: 6 | Nice-to-have: 3
- **Prior verdict resolved:** First review

**Summary:**
GDD คุณภาพระดับ production-ready สำหรับ V-Slice phase — structure แน่น, formulas
ทดสอบได้, state machine ครอบ edge cases ส่วนใหญ่, bidirectional consistency
ระบุชัด, open questions tracked พร้อม owner + trigger. Programmer สามารถเริ่ม
Logic ACs (TR-FAC-001 ถึง 018) ได้ทันทีพร้อม mock harness สำหรับ FT13/FT14.
ไม่มี blocking issues; recommended revisions เป็นคุณภาพชั้นที่สอง (terminology
precision, doc consistency, edge case coverage gaps แคบๆ).

**Recommended revisions (advisory):**
1. D1 wording — "Linear decay" คลาดเคลื่อน (จริงๆ คือ geometric/exponential decay)
2. Worked examples ใน D1/D4 ใช้ r=0.05 ไม่ตรงกับ default r=0.07 (G.1)
3. Decay behavior สำหรับ Switching player ที่ time-inactive — gap (IsDecayEligible
   note รวม Switching แต่ state table ห้าม Switching → Inactive transition)
4. FACTION_COUNT decrease 6→5 ไม่ระบุ explicit (infer ได้จาก EC-08 เท่านั้น)
5. TR-FAC-027 multi-faction batch scope — ไม่ระบุ sequential vs parallel
6. EC coverage ใน H.3 5/18 — EC-17/18 (floor edge boundaries) ทดสอบได้ตอนนี้
   ไม่ต้องรอ FT13

**Process recommendation:**
เก็บ recommended revisions ทั้ง 6 ข้อรอจนกว่า FT12/13/14 GDD จะเขียนใกล้เสร็จ
แล้วทำ pass เดียวพร้อม cross-GDD consistency check — efficient กว่า revise
ทีละข้อตอนนี้.

**Dependency status snapshot:**
- ✅ F4, M6 (upstream) — exists
- 🆕 FT12, FT13, FT14, M11 (downstream) — undesigned, acknowledged in GDD
- ✅ F2, M5, P1, P3 (soft) — exists, revise pending per F.5 note
