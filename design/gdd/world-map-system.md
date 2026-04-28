# World Map (10 Neutral Cities)

> **Status**: In Design (revised post first review 2026-04-28)
> **Author**: User + Claude Code agents
> **Last Updated**: 2026-04-28
> **System ID**: FT12
> **Revisions**: 2026-04-28 — addressed 8 blocking items from /design-review (D1 ceiling+freshness fix, R12 atomicity, G.5 fairness window, G.6 knob constraint, R3.1 vignette rotation, R15 ambient affordances, OQ-9 stranger interaction)
> **Implements Pillar**: Universe-Spanning Meta-Game (P4 — primary), Team Synergy (P3 — supporting), Anti-Toxicity Core (P5 — supporting)
> **Source Decision**: [Tournament Pivot 2026-04-23](../decisions/meeting-2026-04-23-tournament-pivot.md)
> **Supersedes**: FT9 Town System (hard deprecate after FT12 approval)

## Overview

World Map (10 Neutral Cities) คือ **Universe Map mode** — open-world hub
นอกแมตช์ที่ผู้เล่นทุกฝ่ายเดินอยู่ร่วมกันใน 10 เมืองกลาง (ดาวเคราะห์)
ซึ่ง **ไม่มีฝ่ายใดครอบครอง** ทุกคนเข้าได้ ทุก operation นอกแมตช์ —
hero/skin selection, equipment management, lobby creation, Tournament
queue, Personal Dungeon entry, social/chat — เกิดขึ้นที่นี่ทั้งหมด.
แต่ละเมืองมี theme เฉพาะตัว [Open Question: themes] และทำหน้าที่เป็น
**stage ของ Fragment drop event** — Fragment ปรากฏแบบสุ่มตามดาวต่างๆ
ทั่ว Universe ทุกวัน และทุกฝ่ายยกทีมเข้า Tournament เพื่อแย่งชิง.
ในทางผู้เล่น 10 Cities คือ **บ้านกลางที่ทุกฝ่ายมาเจอกันโดยไม่ต้องเป็น
ศัตรู** — สังกัดไม่กำหนดทางเดิน, การพบเพื่อนข้ามฝ่ายเป็นเรื่องปกติ, และ
เมืองที่อยู่ตอนนั้น render badge/chat color ตาม `player.faction_id`
แต่ไม่กระทบสิทธิ์เข้าเมืองหรือ matchmaking pool. ในทางระบบ World Map
คือ **out-of-match aggregation layer** ที่รวม social hub, entry points
(Tournament/Dungeon), persistent commerce (equipment/hero/skin),
และ Fragment drop event display เข้าไว้ในประสบการณ์เดียว — และ
**replaces FT9 Town System** (deprecated หลัง FT12 approval).
ระบบนี้ไม่มี gameplay logic ใน MOBA match จริง (out-of-match scope only).

## Player Fantasy

ผู้เล่นรู้สึกเหมือน **ผู้ถือธงที่มาแวะพักในลานวิหารกลาง** —
สถานที่ที่คำสาบานของทุกฝ่ายได้รับการเคารพเท่ากัน ใครก็ตามที่
ก้าวเข้ามาในเมือง ไม่ว่าจะปักธงสีใด ก็เป็นแขกของลานนี้เสมอกัน.
การเดินอยู่ใน 10 Cities ไม่ใช่การเดินใน "เขตของฝ่ายเรา" หรือ
"เขตของศัตรู" — แต่เป็นการเดินใน **เขตคุ้มครองตามธรรมเนียม
เก่า** ที่ทำให้การชักดาบในนี้รู้สึกผิดที่ผิดทางทันที.

**Anchor moment:** ผู้เล่นยืนอยู่ที่จัตุรัสกลางเมือง เห็นผู้เล่น
ต่างฝ่ายเดินสวนกัน ธงสีต่างกันปักอยู่บนหลังเหมือนกัน ไม่มีใคร
ชักดาบ. แล้ว **เสียงระฆังของเมืองดังขึ้น** ประกาศว่า Fragment
Event กำลังจะเริ่ม ณ เมืองนี้ — ทุกคนในจัตุรัสหันมามองกัน
ครู่หนึ่ง รู้ว่าอีกชั่วโมงข้างหน้าจะเจอกันในสนาม Tournament.
ความรู้สึกที่ต้องการคือ — *ตอนนี้เราอยู่ลานเดียวกัน อีกหน่อย
เราจะอยู่คนละฝั่งสนาม — แต่ทั้งสองช่วงเวลาก็มีเกียรติพอกัน*

**Tone และภาษา:** Mythic-but-grounded เหมือน FT11 — ใช้คำว่า
*ลาน, แวะพัก, ระฆัง, จัตุรัส, ร่วมลาน, ผ่านทาง, ปักธงพัก,
สงบศึก* — ไม่ใช่ *ฐานทัพ, ยึด, ป้อม, แดนเรา, บุก*. ระบบพูดกับ
ผู้เล่นด้วยน้ำเสียงเหมือนผู้ดูแลวิหารที่สุภาพแต่หนักแน่น เช่น
"คุณกำลังเข้าสู่ลานเมือง [City Name] — ผู้ถือธงทุกฝ่ายร่วม
แวะพักที่นี่"

**Reference feeling:** นักเดินทางสายไหมที่แวะคาราวานเซอรายกลาง
ทะเลทราย — พ่อค้าจากอาณาจักรที่กำลังทำสงครามกัน นั่งดื่มชา
โต๊ะเดียวกันได้ เพราะที่นี่คือ "เขตคุ้มครอง" ตามธรรมเนียมเก่า.
ไม่ใช่เพราะกฎห้าม แต่เพราะ *การละเมิดธรรมเนียมในที่เช่นนี้คือ
การลดเกียรติของตัวเอง*

**Pillar mapping:**
- **P5 Anti-Toxicity Core (หลัก)** — Fantasy ทำให้การ flame ผู้เล่น
  ต่างฝ่ายในเมืองรู้สึกเหมือน *สบถในวิหาร* โดยไม่ต้องเขียน
  กฎห้าม. P5 ทำงานผ่าน fantasy ก่อน moderation
- **P4 Universe-Spanning Meta-Game (หลัก)** — เมืองเป็น stage
  ที่ Fragment drop event ปรากฏ ทำให้ทุกการพบเจอใน city
  มี mechanical weight ต่อ meta-game cycle ที่ใหญ่กว่า
- **P3 Team Synergy (รอง)** — การพบเพื่อนข้ามฝ่ายในลาน
  เปิดทางให้ cross-faction team formation ใน Tournament
  Casual ภายหลัง

**สิ่งที่ World Map Fantasy *ไม่ใช่*:**
- ไม่ใช่ "เมืองหลวงของฝ่ายเรา" — ไม่มีเมืองใดผูกกับฝ่าย, ทุก
  เมืองเป็นกลางเท่ากัน
- ไม่ใช่ "ตลาดนัด/lobby UI" — แม้จะมี commerce แต่ tone
  ของเมืองหนักกว่านั้น เป็นพื้นที่มีความหมายไม่ใช่ฉาก
  background
- ไม่ใช่ "MMO open world" — ไม่มี mob, ไม่มี combat, ไม่มี
  questing ; เป็น social hub ที่ผู้เล่น *เลือก* จะเดิน
  ไม่ใช่ *ต้อง* เดิน เพื่อ progression

**Onboarding risk mitigation:** เพราะ tone "ลานวิหาร" อาจ
รู้สึก abstract สำหรับผู้เล่นใหม่ที่คุ้นกับ MOBA lobby แบบ
LoL/DotA, การมาถึงเมืองแรกครั้งแรกควรมี **first-visit
moment** — ระฆังเมืองดังต้อนรับ + ข้อความเงียบๆ ที่อธิบายว่า
"นี่คือลานกลางที่ทุกฝ่ายมาเจอกัน" — ไม่ใช่ tooltip popup
แบบทั่วไป แต่เป็น environmental beat ที่ตั้ง tone ตั้งแต่
วินาทีแรก. รายละเอียดอยู่ใน UX spec ภายหลัง.

## Detailed Design

### Core Rules

**R1 — Neutrality Absolute**
ไม่มีฝ่ายใดครอบครอง, ล็อก, หรือได้รับสิทธิพิเศษใน city ใดๆ ทั้ง 10 เมือง.
ผู้เล่นทุกคนที่มี `player.faction_id` valid เข้าได้ทุกเมืองเสมอ ไม่ว่า
faction ranking, Fragment pool, หรือสถานะ Tournament จะเป็นอย่างไร.
*(P5 Anti-Toxicity foundation — neutrality เป็นกฎทางระบบ ไม่ใช่แค่ tone)*

**R2 — Universal Services (เหมือนกันทุกเมือง)**
ทุก city ให้บริการ Universal Services ชุดเดียวกัน:
- Hero/Skin Selection
- Equipment Management (Personal Dungeon equipment, **ไม่ใช่** MOBA items)
- Lobby Creation (Custom matches)
- Tournament Queue UI (entry point ไป FT13)
- Personal Dungeon Entry (FT8)
- Party Formation (cross-faction allowed — FT11 R3)
- Social/Chat (M1 + M11)
- Faction Profile / Standings viewing (FT11 read)

ไม่มี service ใดใน Universal Set ถูก gate ด้วย city, faction, หรือ progression.

**R3 — City-Exclusive Anchor Content**
แต่ละเมืองมี Anchor Content 1 รายการที่ไม่ซ้ำกับเมืองอื่น (ตัวอย่าง:
lore archive, cosmetic preview theatre, faction history hall) — รายการ
สมบูรณ์ [Open Question: Anchor Content ของ 10 cities, owner = narrative team].
**Anchor Content ห้ามเป็น progression gate** — ผู้เล่นที่ไม่เคยไปเมืองที่มี
Anchor X ต้องไม่สูญเสีย numeric/competitive advantage ใดๆ. โดยเฉพาะ
Anchor Content ห้าม grant: **XP, Fragment, Fragment multiplier, hero/equipment
stat bonus, faction-pool boost, Tournament-rank modifier, queue priority,
หรือ currency ใดๆ**. อนุญาตเฉพาะ cosmetics ที่ unlock ได้ผ่าน path อื่นด้วย,
lore reveals, และ flavor interactions ที่ไม่เปลี่ยน numeric state ของ player.
*(Anchor Content คือเหตุผลให้ผู้เล่นจำเมืองได้ ไม่ใช่กลไกบังคับ traversal)*

**R3.1 — Rotating Lore Vignette (repeat-visit hook)**
แต่ละ Anchor ต้องมี **lore vignette pool ขนาด ≥ 4 ชิ้น/เมือง** หมุนเวียน
ตามตารางเวลาคงที่ (default: รายสัปดาห์ ; knob `ANCHOR_VIGNETTE_ROTATION_DAYS`
ใน Section G). Vignette ปัจจุบันแสดงเมื่อผู้เล่น interact กับ Anchor —
ไม่มี reward, ไม่ใช่ quest, ไม่บังคับ. **เหตุผลของ rotation:** ป้องกัน
Anchor Content ทำงานเป็น one-and-done lore tourism ; ผู้เล่นที่กลับมา
เยือนเมืองหลังหลายสัปดาห์เห็น vignette ใหม่. Vignette pool authoring =
narrative ownership (OQ-3 ขยาย scope). Rotation เกิดบน server clock
(UTC), ทุก instance ของเมืองเห็น vignette ปัจจุบันพร้อมกัน.

**R4 — Single Presence + Persistent City**
ผู้เล่นอยู่ใน city เดียวเท่านั้น ณ เวลาใดก็ตาม. `last_city_id` เป็น
persistent server-authoritative field — เมื่อ disconnect/session expire
จะคงค่าเดิม. Reconnect spawn ที่ `last_city_id` ไม่ใช่ default city.
First-ever spawn (หลัง FT11 selection): hardcoded "starter city" [Open Question: เมืองใด].

**R5 — Cross-City Travel: Galaxy Map Overlay**
การย้ายระหว่างเมืองผ่าน **Galaxy Map UI overlay** (กด hotkey หรือ UI button
จากทุกที่ใน city). Map แสดง 10 เมืองในมุม overhead พร้อม Fragment Event
indicator ของแต่ละเมือง. คลิกเมืองปลายทาง → confirm → transition `GALAXY_MAP_TRANSITION_SECONDS`
(default 2.5s — ดู Section G).
ไม่มี physical portal ที่ต้องเดินไป, ไม่มี travel cost, ไม่มี faction
restriction. *(Galaxy Map ทำสองหน้าที่: travel UI + Fragment Event status board)*

**R6 — Travel-Queue Interaction**
ผู้เล่นที่อยู่ใน Tournament queue (state `InCity+Queued`) **ต้องยกเลิก
queue ก่อนเดินทาง** — ระบบแสดง confirmation dialog: *"การเดินทางจะยกเลิก
queue ปัจจุบัน — ดำเนินการต่อ?"*. ไม่มี "queue ตามไป city ใหม่" — queue
context ผูกกับ `origin_city_id` (ดู C2).
*(ป้องกัน silent queue loss + รักษา origin_city_id semantics ของ Returning state)*

**R7 — Faction Cosmetics: Read-Only Passthrough**
World Map อ่าน `player.faction_id` (FT11 R11) เพื่อ render badge, chat color,
และ faction-themed particle/banner เท่านั้น. FT12 ไม่มี write path ไปยัง
`faction_id` — ทุก visual faction state เป็น downstream consequence ของ FT11.

**R8 — Fragment Drop Event Display Contract**
เมื่อ FT14 ส่ง event payload ว่า Fragment Event เกิดที่ city X, FT12 แสดง
visual announcement ที่ city X ทุก instance พร้อมกัน:
- Fragment Event Monument กลาง plaza transform (particle + glow)
- เสียงระฆังเมืองดัง (diegetic, ไม่ใช่ UI sfx)
- Galaxy Map indicator ของ city X เปลี่ยนเป็น active state

FT12 ไม่คำนวณ drop logic, ไม่ตัดสิน winner, ไม่เก็บ Fragment state — เป็น
display layer เท่านั้น. FT14 = source of truth.
[Open Question: FT14 event schema — owner = FT14 author]

**R9 — Cross-Faction Party Formation + Co-location**
ผู้เล่นสามารถ invite ผู้เล่นต่างฝ่ายเข้า party ได้ภายใน city (ผ่าน M1).
Party formation ใน World Map ไม่ทำให้ Fragment routing เปลี่ยน — Fragment
ยังคง route ตาม `player.faction_id` ของแต่ละคน (FT11 R4).
**Co-location guarantee:** สมาชิก party ที่ travel ระหว่าง city พร้อมกัน
ถูก assign instance เดียวกันเสมอ (override soft cap ของ R13 หากจำเป็น).

**R10 — City Chat Scope: Proximity per Instance**
City chat = **proximity chat (RadiusChat)** scoped ต่อ city instance.
ไม่มี city-wide หรือ cross-instance chat channel ใน FT12.
- Faction chat → M1 ownership (ข้าม city ได้)
- Party chat → M1 ownership
- Global/announcement → M10 ownership

*(จำกัด toxicity surface — ไม่มี broadcast channel ที่ stranger จะ flame ได้)*

**R11 — Anti-Toxicity Enforcement Boundary**
FT12 ไม่มี combat, dueling, หรือ PvP mechanic ใดๆ ใน city space —
จัดการ violation ผ่าน M11 Reputation. Chat events จาก R10 pass ไปยัง M11 API
สำหรับ filter + reputation accounting. FT12 ไม่ implement filter เอง.

**R12 — Server Authority + Player Record Atomicity**
`last_city_id`, `last_city_entry_timestamp`, `current_instance_id`,
`ft12_migrated`, presence state ทุก field เป็น server-authoritative.
Client ส่ง teleport request → server validate → server update → server
broadcast presence ไปยัง instance channel. Client ไม่มี write authority
บน presence/location state.

**Atomicity contract:** ทั้ง 4 fields บน Player Record ต้องเขียนใน
**single transactional Azure Function call** (atomic write). Partial
failure ห้าม commit field ใดๆ — ทั้ง 4 fields rollback พร้อมกัน.

**Read-repair behavior (compensating):** ทุก consumer ของ
`current_instance_id` (รวม D1 step 1, R9 co-location, presence broadcast)
ต้อง resolve `instance_id` ผ่าน `LookupInstance(id, t)` ก่อนใช้งาน —
ถ้า instance ที่ id ชี้ไปไม่มีอยู่จริง (torn down / never spawned),
consumer ต้อง treat as null และ fall through ไป step ถัดไป (D1 step 2,
หรือ EC-13 fallback chain). ห้าม assume `current_instance_id` valid
โดยไม่ lookup. *(ป้องกัน dangling reference จาก partial-write หรือ
race ระหว่าง EC-04 teardown กับ co-location read)*

**R13 — Capacity: Soft Cap + Instance Sharding**
แต่ละ city มี soft cap `CITY_INSTANCE_SOFT_CAP` (default 150, parametric ใน CBS).
เมื่อ cap เต็ม → server spawn instance ใหม่ของ city เดิม (City_Astra_01,
City_Astra_02, …).
- Players เห็น **cross-instance counter** ที่ plaza entrance: *"ลานนี้ X / ลานร่วม Y"*
- Party co-location (R9) override cap ถ้าจำเป็น
- Fragment Event announcement (R8) broadcast ทุก instance พร้อมกัน
- Network/sharding implementation pattern: **defer to ADR** (FT12 ระบุแค่ behavior)

**R14 — FT9 Supersede Contract**
หลัง FT12 approved, FT9 Town System mark deprecated — ไม่มี parallel operation.
รายละเอียด feature migration list อยู่ใน Section C.3 Interactions (FT9 row).
ผู้เล่นที่มี save state จาก FT9 ก่อน patch: ดู Section E (EC สำหรับ migration).

**R15 — Plaza Ambient Social Affordances (linger reason)**
ทุก plaza ต้องมี **non-mechanical social affordances อย่างน้อย 4 ประเภท**
เพื่อให้ผู้เล่นมีเหตุผลอยู่ใน plaza ระหว่าง Fragment Event:
1. **Sit-on-bench / shared seating** — ม้านั่ง 2–3 ที่ที่ผู้เล่นนั่งร่วมกันได้,
   trigger sitting emote pose
2. **Emote spots** — จุด environmental ที่ trigger emote เฉพาะที่ (ไหว้
   หน้าระฆัง, ปักธงพักหน้า Banner Rack)
3. **Fountain / interactable focal point** — interaction passive ที่
   trigger particle/audio cue (โยนเหรียญ, แตะน้ำ)
4. **Ambient gathering hooks** — จุดที่ NPC + player co-mingle ใน
   formation ที่อ่านเป็น "ลานชุมนุม" จาก camera angle ปกติ

**กฎร่วมของ R15:**
- ห้าม grant XP, Fragment, currency, stat, queue priority ใดๆ (R3 rule
  ขยายไป R15)
- Affordance ทุกอย่าง opt-in — ผู้เล่นที่เดินผ่านไม่ trigger
- ห้าม block traversal — ทุก affordance วาง offset จาก main pathway
- Detail spec ของแต่ละ affordance defer ไป UX spec + Asset Spec phase

*(Rationale: P5 Anti-Toxicity + P3 Team Synergy fantasy require ผู้เล่น
*มีอยู่จริง* ใน plaza ไม่ใช่แค่ผ่านทาง ; ถ้าทุกคน teleport ออกทันทีหลังเปิด
service menu, fantasy "ลานวิหาร" ที่ Section B วาดไว้จะไม่ดำเนินการจริง)*

### States and Transitions

| State | คำอธิบาย | Trigger เข้า | Trigger ออก |
|---|---|---|---|
| `OutOfUniverse` | logged out / อยู่ที่ main menu — ไม่มี active world session | App launch / logout / session expire | Login success → `EnteringCity` (ปลายทาง = `last_city_id` หรือ starter city) |
| `EnteringCity` | กำลัง load city scene (transition state) | จาก `OutOfUniverse` (login), `ReturningFromMatch`, หรือ Galaxy Map travel จาก `InCity` | Scene load สำเร็จ → `InCity` ; load fail → `OutOfUniverse` + error toast |
| `InCity` | เดินอยู่ใน city ใด city หนึ่ง — state หลัก | Scene load เสร็จจาก `EnteringCity` | Galaxy Map travel → `EnteringCity` ; ยืนยัน Tournament queue → `InCity+Queued` ; เข้า Personal Dungeon → `InMatch` ; logout → `OutOfUniverse` |
| `InCity+Queued` | เดินอยู่ใน city เดิม + Tournament queue active ใน background | กดยืนยัน queue ขณะ `InCity` | Match found (FT13 signal) → `InMatch` (บันทึก `origin_city_id`) ; กด cancel queue → `InCity` ; พยายาม travel → R6 confirmation dialog |
| `InMatch` | อยู่ใน Tournament match (FT13) หรือ Personal Dungeon (FT8) — out of FT12 scope | Match found จาก `InCity+Queued` ; เข้า Personal Dungeon จาก `InCity` | Match end (win/loss/disconnect/surrender) → `ReturningFromMatch` |
| `ReturningFromMatch` | Post-match screen แสดงอยู่ (Fragment award, stats, Fragment Event update) | Match end signal | กด "กลับเมือง" / auto-timeout (~30s) → `EnteringCity` (ปลายทาง = `origin_city_id`) ; กด "เดินทางไป [Event City]" shortcut → `EnteringCity` (ปลายทาง = event city) |

**Invariants:**
- ผู้เล่นอยู่ใน **เพียง 1 state** ณ เวลาใดก็ตาม
- `InCity+Queued` เป็น state เดียวที่ background async signal (match found) ทำให้ transition อัตโนมัติ
- `EnteringCity` เป็น transient — ไม่มี persistent storage ; client crash ระหว่าง transition → reconnect spawn ที่ `last_city_id` (R4)
- `origin_city_id` snapshot ตอนเข้า `InMatch` — รับประกันการกลับเมืองเดิมแม้ FT14 spawn Fragment Event ที่อื่นระหว่างแมตช์
- Travel ขณะ `InCity+Queued` ต้อง explicit cancel (R6) — state machine ห้าม direct transition `InCity+Queued` → `EnteringCity`
- `InMatch` state ownership อยู่ที่ FT13/FT8 ; FT12 รอรับ end signal เท่านั้น (ไม่ poll, ไม่ probe)
- Ban event ขณะอยู่ใน city → handled ใน Section E (EC)

**State diagram (สรุปเส้นทาง):**

```
OutOfUniverse ──login──> EnteringCity ──load_done──> InCity
                                                       │
InCity ──queue_confirm──> InCity+Queued ──match_found──┤
                                                       ▼
InCity+Queued ──cancel──> InCity                    InMatch
                                                       │
InCity ──dungeon_enter──> InMatch                  match_end
                                                       │
                                                       ▼
                                             ReturningFromMatch
                                                       │
                                          gohome / event_shortcut
                                                       │
                                                       ▼
                                                EnteringCity
                                                       │
                                                       ▼
                                                    InCity
```

### Interactions with Other Systems

#### FT11 Faction System

| Direction | Data | Owner | Authority |
|---|---|---|---|
| IN | `player.faction_id` (read on city enter, on chat message, on banner render) | FT11 owns ; FT12 reads via snapshot | Server snapshot — never write |
| OUT | — (FT12 ไม่ส่งกลับ FT11) | — | — |

> **Boundary:** FT12 = pure consumer of `faction_id` for cosmetics เท่านั้น (FT11 R11). ไม่มี faction-gated access.

#### FT13 Tournament System

| Direction | Data | Owner | Authority |
|---|---|---|---|
| OUT (entry) | Player `(player_id, queue_type, origin_city_id)` → FT13 queue endpoint | FT13 owns endpoint ; FT12 sends | Azure Function (server) |
| IN (lifecycle) | `match_found(player_id, match_id)` → FT12 transitions player to `InMatch` ; `match_end(player_id, match_id, return_payload)` → FT12 transitions to `ReturningFromMatch` | FT13 emits ; FT12 listens | Server-side event bus |
| IN (post-match shortcut) | Active Fragment Event city list (สำหรับ "เดินทางไป [Event City]" shortcut บน post-match screen) | FT14 source ; FT13 forwards in `match_end` payload | Server-issued |

#### FT14 Fragment & Meta-Game

| Direction | Data | Owner | Authority |
|---|---|---|---|
| IN | `fragment_event_started(city_id, event_id, schema TBD)` → FT12 trigger Monument transform + Galaxy Map indicator | FT14 emits ; FT12 listens | Server event |
| IN | `fragment_event_ended(city_id, event_id)` → FT12 reset Monument | FT14 emits ; FT12 listens | Server event |
| OUT | — (FT12 ไม่เก็บ Fragment state, ไม่คำนวณ drop) | — | — |

> **Boundary note:** FT12 = display layer เท่านั้น. FT14 = source of truth สำหรับทุก Fragment-related state.
> [Open Question: FT14 event payload schema — owner = FT14 author]

#### M11 Reputation / Anti-Toxicity

| Direction | Data | Owner | Authority |
|---|---|---|---|
| OUT | Chat events จาก city RadiusChat (R10) → M11 filter + reputation accounting endpoint | M11 owns endpoint ; FT12 forwards | Azure Function |
| IN | `player_banned(player_id)` → FT12 force-exit player from city instance (ดู Section E EC สำหรับ behavior) | M11 emits ; FT12 listens | Server event |
| IN | `ban_lifted(player_id)` → ผู้เล่น login ครั้งถัดไป spawn `last_city_id` ปกติ | M11 emits ; FT12 = no special handling | Server event |

#### FT8 Personal Dungeon

| Direction | Data | Owner | Authority |
|---|---|---|---|
| OUT (entry) | Player `(player_id, dungeon_id, party_members)` → FT8 entry endpoint | FT8 owns ; FT12 sends | Server call |
| IN (lifecycle) | `dungeon_session_end(player_id, payload)` → FT12 transitions to `ReturningFromMatch` (เหมือน FT13 path) | FT8 emits ; FT12 listens | Server event |

#### FT9 Town System — Supersede Migration Table (R14 detail)

| FT9 Feature | Migration Status | FT12 Equivalent / Note |
|---|---|---|
| Equipment Management UI (6 slots) | ✅ Migrate as-is | Universal Service in every city plaza alcove (R2) |
| Hero/Skin Selection UI | ✅ Migrate as-is | Universal Service (R2) |
| Lobby Creation (Custom matches) | ✅ Migrate as-is | Universal Service (R2) |
| Personal Dungeon Entry | ✅ Migrate as-is | Universal Service (R2) — FT12 routes via FT8 contract |
| Tournament/Match queue entry | ✅ Migrate (was: from main menu) | Universal Service (R2) — FT12 routes via FT13 contract |
| RadiusChat (proximity chat) | ✅ Migrate as pattern | Per-instance proximity chat (R10) — radius value re-tuned for plaza density |
| Top-down camera + LeanButton click-to-move | ✅ Migrate pattern | Reused — same input model |
| `scene_town_map.unity` single scene | ❌ Replaced | One scene per city (10 scenes), Additive load |
| NPC framework (basic dialogue, no quest) | ⚠️ Partial migrate | Keeper NPC + ambient NPCs only — no quest progression |
| EventTaskSystem (FT9 quest framework) | ❌ Retire | Town quest progression dropped — no FT12 equivalent |
| Town-specific decoration/housing (never implemented) | ❌ Retire | Out of scope |
| `last_town_state` persistence | ⚠️ Migrate to `last_city_id` | One-time migration — see Section E (EC for FT9 → FT12 player state) |

> **Deprecation timing:** FT9 GDD mark "Deprecated — superseded by FT12" หลัง FT12 approved. ไม่มี parallel operation. Players ที่ login หลัง patch deploy → flow ผ่าน FT12 ทั้งหมด.

#### M1 Social System (Soft — service-layer)

| Direction | Data | Owner | Authority |
|---|---|---|---|
| IN/OUT | Party invites, party state, chat channels (faction/party), friend list — FT12 surfaces M1 services in city UI | M1 owns ; FT12 = UI consumer | M1 server |

> FT12 ไม่ implement social state ใหม่ — ทุก party/friend/chat-channel logic = M1 ownership. FT12 เพียง host UI surfaces.

#### M10 Announcement System (Soft)

| Direction | Data | Owner | Authority |
|---|---|---|---|
| IN | Global announcements (Tournament results, faction milestones) → FT12 displays as toast in city UI | M10 emits ; FT12 displays | Server broadcast |

#### M6 Account & Auth

| Direction | Data | Owner | Authority |
|---|---|---|---|
| IN | Session validation, `player.faction_id` (via FT11), `last_city_id` field on Player Record | M6 owns Player Record ; FT12 reads/writes `last_city_id` field | Server-authoritative — FT12 can write `last_city_id` only via Azure Function |

#### F4 Data/Config

| Direction | Data | Owner | Authority |
|---|---|---|---|
| IN | City definitions (10 entries: city_id, theme, anchor_content_ref, scene_path), tuning constants (`CITY_INSTANCE_SOFT_CAP`, `CITY_CHAT_RADIUS`, `STARTER_CITY_ID`, …) | F4 ownership ; CBS config | Server config |

#### F2 Networking Core (Soft — pattern deferred)

> Network/sharding implementation **defer to ADR** (R13). FT12 GDD ระบุ behavior ; F2 อาจถูก enhance หรือ FT12 อาจใช้ PlayFab + SignalR ตาม decision ของ ADR.

## Formulas

> FT12 เป็น behavior/state/UI system ที่ math content เบา. มี 2 formulas
> หลักที่เป็น algorithmic logic — ไม่ใช่ continuous math เหมือน FT11 D1 decay.
> ทุก threshold values อยู่ใน Section G (Tuning Knobs).

### D1 — `InstanceAssignment(player, city_id, t)` (instance picker)

เลือก city instance สำหรับ player เมื่อ enter `city_id` — เคารพ party
co-location (R9) ก่อน soft cap (R13)

```
InstanceAssignment(player, city_id, t):
  party = GetParty(player)                       // M1 lookup ; size 1 = solo
  party_size = party.member_count
  hard_ceiling = floor(CITY_INSTANCE_SOFT_CAP * CITY_INSTANCE_HARD_CEILING_RATIO)  // R9 + EC-06

  // (1) Party co-location override (R9) — gated on freshness, state, AND hard ceiling
  // EC-07 fix: predicate must require state == InCity (EnteringCity has transient instance_id)
  for each member in party:
    if member.current_instance_city_id == city_id
      AND member.current_instance_id is not null
      AND member.state == "InCity"                       // EC-07: exclude transient states
      AND member.last_heartbeat_age_seconds <= 5:        // EC-07 freshness window
      target = LookupInstance(member.current_instance_id, t)
      if target is null:
        continue                                         // stale ref — instance torn down
      // EC-06 hard ceiling enforcement (in-algorithm, not just prose)
      if (target.current_population + party_size) <= hard_ceiling:
        return target.instance_id                        // join existing party instance
      else:
        emit_signal("party_co_location_ceiling_exceeded", target.instance_id)
        break                                            // fall through to step 2

  // (2) Pack into existing instances ที่มีที่ว่างพอ (descending by population)
  candidates = ListInstancesOfCity(city_id, t)
  // EC-01 tie-break: primary desc by population, secondary asc by instance_id (lex)
  candidates.sort(by: (-current_population, instance_id_lex_asc))
  for each i in candidates:
    if (i.current_population + party_size) <= CITY_INSTANCE_SOFT_CAP:
      return i.instance_id                               // assign

  // (3) Fallback: spawn new instance
  new_id = SpawnNewInstance(city_id)
  return new_id
```

| Variable | Symbol | Type | Range | Description |
|---|---|---|---|---|
| `player` | — | object | valid Player Record | ผู้เล่นที่กำลัง enter city |
| `city_id` | — | enum | 1 ใน 10 cities | ปลายทาง |
| `party_size` | `s` | int | 1 – PARTY_MAX (M1 owns) | จำนวนสมาชิก party ที่ travel พร้อมกัน |
| `current_population` | `pop_i` | int | 0 – ∞ | ผู้เล่นใน instance i ปัจจุบัน |
| `CITY_INSTANCE_SOFT_CAP` | `C` | int | 50 – 500 | soft cap per instance (default 150) |
| `instance_id` | — | string | non-null UUID | ปลายทาง (existing หรือ new) |

**Output:** instance_id (เสมอ ; algorithm จบที่ spawn-new ถ้าจำเป็น) ; non-null guaranteed

**Worked example** (`C = 150`, party_size = 3):

State ก่อน assignment:
- City "Astra" มี 2 instances:
  - Astra_01: pop=148
  - Astra_02: pop=80
- ไม่มีสมาชิก party อยู่ใน Astra อยู่ก่อน

Trace:
- Step 1 (co-location): no match → skip
- Step 2: sort desc → [Astra_01 (148), Astra_02 (80)]
  - Astra_01: 148 + 3 = 151 > 150 → reject
  - Astra_02: 80 + 3 = 83 ≤ 150 → **assign Astra_02** ✓

ผล: party 3 คน → Astra_02 ; final populations [148, 83]

**Co-location example:** party 3 คน, 1 คนอยู่ Astra_01 อยู่แล้ว (rejoin หลัง crash):
- Step 1: co-location match → return Astra_01 ทันที (override soft cap)
- ผล: Astra_01 pop = 148 + 2 (อีก 2 คนที่เพิ่ง assign) = 150 ; ไม่ละเมิด R9 แม้ใกล้ cap

### D2 — `IsCityOverflow(city_id, t)` (new-instance trigger)

Boolean predicate — ใช้ตรวจว่า city ทุก instance เต็มหรือยัง (เพื่อ pre-warm
instance ใหม่ ก่อน next assignment fail)

```
IsCityOverflow(city_id, t):
  instances = ListInstancesOfCity(city_id, t)
  if instances.is_empty():
    return true                                  // edge: city ยังไม่มี instance
  return all(i.current_population >= CITY_INSTANCE_SOFT_CAP for i in instances)
```

| Variable | Symbol | Type | Range | Description |
|---|---|---|---|---|
| `instances` | — | list | 0 – ∞ | active instances ของ city ปัจจุบัน |
| `current_population` | `pop_i` | int | 0 – ∞ | ผู้เล่นใน instance i |
| `CITY_INSTANCE_SOFT_CAP` | `C` | int | 50 – 500 | จาก D1 / G |
| `IsCityOverflow` | — | bool | {true, false} | true = ทุก instance ≥ cap (หรือไม่มี instance) |

**Output:** boolean ; true = ต้อง spawn instance ใหม่ก่อน assignment ครั้งถัดไป

**Use case:** server scheduler ที่ run ทุก ~5s ตรวจ overflow ของ 10 cities
ขนาน — เมื่อ true ให้ pre-warm instance ใหม่ (ลด assignment latency จาก
"spawn-on-demand" → "ready-to-assign"). หาก scheduler ช้า D1 step 3 ยัง
รับประกัน fallback spawn อยู่ดี — D2 = optimization, ไม่ใช่ correctness gate.

**Worked example** (`C = 150`):

State A: City "Solis" มี 3 instances pop = [150, 150, 142]
- all ≥ 150? → false (Solis_03 มี 142) → return false ; ไม่ pre-warm

State B: City "Solis" มี 3 instances pop = [150, 150, 150]
- all ≥ 150? → true → return true → scheduler trigger SpawnNewInstance("Solis")

State C: City "Nova" ยังไม่มี instance (everyone offline)
- instances.is_empty() → true → return true → scheduler pre-warm Nova_01 (อาจ skip ในการ optimization จริง — ขึ้นกับ idle policy ใน ADR)

> **Note:** จำนวน instance ทั้งหมดต่อ city ไม่มี hard cap ใน formula — limit
> มาจาก server resource budget (defer to ADR). Operational alert threshold
> ระบุใน Section G.

## Edge Cases

> รูปแบบ: **If [condition]**: [resolution]. *(rationale ถ้าไม่ obvious)*

### Boundary / D1-D2 numerical conditions

- **EC-01** **If D1 step 2 sort พบ instances ที่ pop เท่ากัน (tie)**: tie-break
  ด้วย `instance_id` lexicographic ascending. รับประกัน determinism ข้าม
  server node. *(จำเป็นสำหรับ test reproducibility)*
- **EC-02** **If best-fit instance มี pop = `CAP − 1` แต่ party_size ≥ 2**:
  D1 step 2 reject → fallback SpawnNewInstance แม้จะ "เสีย" 1 slot ใน 2
  instances. ยอมรับ instance proliferation นิดหน่อย ดีกว่าซับซ้อน partial-fit logic.
- **EC-03** **If `SpawnNewInstance` ล้มเหลว (resource exhausted)**: D1 step 3
  return error → client เห็น toast *"เมือง [X] หนาแน่นชั่วคราว — ลองใหม่ใน 30s"* ;
  state ยังคง `InCity` (เมืองเดิม) ; travel ไม่ commit. *(reject ดีกว่า relax cap
  ที่อาจ cascade overflow)*
- **EC-04** **If D2 (overflow predicate) trigger pre-warm แต่ instance ใหม่
  ไม่มีผู้เล่นเข้าใน `IDLE_INSTANCE_TEARDOWN_SECONDS`**: server teardown
  instance เปล่า. รายละเอียด threshold ใน Section G.

### Concurrency / Race conditions

- **EC-05** **If party 5 คน trigger Galaxy Map พร้อมกันไปคนละ city**: leader
  destination wins ; non-leader members เห็น auto-redirect dialog *"Party leader
  [Name] กำลังเดินทางไป [City] — ตามไป?"* (ยืนยันก่อน). *(deterministic +
  เคารพ player autonomy)*
- **EC-06** **If party formed ข้าม city (member A ใน Astra, B ใน Solara) แล้ว A
  travel ไป Solara ขณะ B's instance เต็ม**: D1 step 1 co-locate กับ B (override
  cap per R9) แต่ enforce **hard ceiling** = `CAP × 1.2` (default 180) ;
  ถ้าเกิน hard ceiling → travel reject + dialog (เหมือน EC-03). รายละเอียดใน Section G.
- **EC-07** **If D1 step 1 อ่าน `current_instance_id` ของ party member ที่
  เพิ่ง logout (stale)**: D1 fall through ไป step 2 (pack) — ใช้ snapshot
  freshness window 5 วินาที, ถ้า last_heartbeat > 5s → ignore co-location candidate.
- **EC-08** **If chat message in-flight ขณะ ban event มาถึง**: chat pipeline
  = **synchronous filter-before-broadcast** ผ่าน M11 endpoint. message ของ
  banned player ที่ยัง pending → drop. *(ป้องกัน last-word flame ก่อน ban)*
- **EC-09** **If match-found countdown trigger ขณะ player พิมพ์ใน text input
  (loadout name, friend search)**: ระบบ defer countdown popup 3 วินาที
  ถ้า input field มี focus ; modal ขึ้นเมื่อ blur หรือเมื่อเหลือ ≤5s. *(ป้องกัน
  Enter key conflict)*
- **EC-10** **If Galaxy Map double-tap ระหว่าง `EnteringCity` transition**:
  input layer block ทุก nav input ตลอด `EnteringCity` state. server
  ignore second teleport request ที่ arrive ก่อน first จบ.

### State transition edge cases

- **EC-11** **If ban event มาถึงขณะ player `InCity`**: state transition →
  `OutOfUniverse` ทันที + force-exit instance + chat message kicked broadcast
  *"[Player] ออกจากลานเมืองโดยกำเนิด"* (ไม่ระบุเหตุผลเพื่อรักษา dignity).
  เมื่อ ban-lifted: login ครั้งถัดไป spawn `last_city_id` ปกติ (R4).
- **EC-12** **If match-end signal มาถึงขณะ player ยังอยู่ `EnteringCity`**:
  buffer signal ; complete city load ก่อน → จบ load ทันที transition ไป
  `ReturningFromMatch` (skip `InCity`). *(ป้องกัน dropped post-match payload)*
- **EC-13** **If `origin_city_id` corrupt/null หลัง match**: fallback chain:
  (1) `last_city_id` ของ player record, (2) `STARTER_CITY_ID`. log warning
  สำหรับ ops investigate.
- **EC-14** **If network drop ระหว่าง `EnteringCity` load**: client retry
  3 ครั้ง ; ถ้ายัง fail → return `OutOfUniverse` + error. server hold
  `current_instance_id` slot ไว้ `ENTERING_HOLD_SECONDS` (default 60s)
  เผื่อ reconnect — เกิน → release slot.

### Lifecycle / Migration

- **EC-15** **If FT9 player login หลัง FT12 deploy (one-time migration)**:
  `last_town_state` → ignore ; `last_city_id` ← `STARTER_CITY_ID` ; equipment
  state migrate as-is (ผูกกับ Player Record ไม่ใช่ Town). One-shot — flag
  `ft12_migrated=true` ป้องกัน double-migrate.
- **EC-16** **If server restart / rolling deploy ขณะมี players ใน cities**:
  ทุก `InCity` player → soft kick → reconnect prompt → login ใหม่ spawn
  `last_city_id` (ใน new instance ของ city เดิม). pending writes ของ
  `last_city_id` flush ก่อน restart (graceful drain).
- **EC-17** **If account deletion ขณะ `InCity`**: server cleanup hook —
  remove จาก instance roster ทันที + decrement `current_population` ;
  ถ้าเป็น last player ใน instance + instance is pre-warmed → trigger EC-04
  teardown.
- **EC-18** **If `EnteringCity` state ค้างนานกว่า `ENTERING_TIMEOUT_SECONDS`**
  (slow client load, default 30s): server force-fail load → client return
  `OutOfUniverse` + error + suggest retry. instance slot release.

### Live ops / Performance

- **EC-19** **If launch day flood — 5000+ new players spawn ที่
  `STARTER_CITY_ID` ใน 10 วินาที**: D2 reactive ตามไม่ทัน → server-side
  **launch-mode policy**: pre-warm `STARTER_CITY_INSTANCE_PREWARM_COUNT`
  (default 5) instances ของ starter city ก่อน server accept connections.
  Default 5 รองรับ 750 concurrent new players (5 × 150).
- **EC-20** **If Fragment Event spike — 1000+ players travel ไป city เดียว
  ใน 5 วินาทีหลัง announce**: D2 trigger rapid pre-warm ของ city target
  (rate-limited spawn ป้องกัน server thundering herd). announce-to-event-
  start delay = `EVENT_ANNOUNCE_LEAD_SECONDS` (default 30s) ให้ pre-warm
  spread load.
- **EC-21** **If Anchor Content concentration — popular Anchor city ดึง
  Fragment Event บ่อย ทำให้เมืองอื่นร้าง**: FT14 ownership (event distribution
  policy = FT14 GDD scope). FT12 expose alert metric — `CityVisitFairnessIndex`
  (Section G threshold). Re-engagement campaign ผ่าน live ops.
- **EC-22** **If city pop = 0 off-peak (fantasy fails — empty plaza)**:
  ambient NPC floor `MIN_AMBIENT_NPC_PER_CITY` (default 5) — non-functional
  background characters เดิน + idle animations. ทำให้ city รู้สึกมีชีวิตแม้
  no real players. รายละเอียด design ใน Asset Spec.

### Identity / Time / Authority

- **EC-23** **If player faction switch (FT11 R6) ขณะ `InCity`**: badge/banner
  update **live** สำหรับ player คนอื่นใน instance เดียวกัน — broadcast presence
  delta. Chat color ของข้อความเก่า **ไม่** retroactively เปลี่ยน (immutable post-send).
- **EC-24** **If client clock skew vs server**: ทุก timestamp (event time,
  countdown, transition) ใช้ **server time UTC** (R12). client display time
  derive จาก server-issued event time, ไม่ใช่ local clock.
- **EC-25** **If Fragment Event signal arrives ที่ region servers ต่างเวลา**
  (cross-region clock drift): event timestamp = single authoritative UTC
  จาก FT14 ; ทุก region replay ระฆัง + Monument transform synced ที่ event_time
  (offset compensation client-side).

### UX / Visual

- **EC-26** **If Fragment Event Monument อยู่นอก viewport ขณะ announce**:
  audio (ระฆัง) ดังเป็น diegetic 360-degree sound + directional indicator
  บนขอบหน้าจอ ("northwest, in this city") + Galaxy Map indicator pulse.
  ระฆัง = primary cue, Monument visual = secondary.
- **EC-27** **If first-visit environmental beat trigger ก่อน scene fully
  loaded** (texture pop-in): block trigger จนกว่า `scene.is_streaming_complete`
  + 0.5s grace period. *(รักษา fantasy ไม่ให้ break ที่ first impression)*

> **Coverage:** 27 ECs cover all holes identified by 4 specialists (game/level/
> ux/systems-designer rounds). Subset ที่จะใช้เป็น H.3 priority ACs: 8-10 รายการ
> (เลือกใน Section H).

## Dependencies

### F.1 Upstream (FT12 depends on)

| System | Type | Interface | Status |
|---|---|---|---|
| **F4 Data/Config** | Hard | โหลด: 10 city definitions (city_id, theme, anchor_content_ref, scene_path), tuning constants (`CITY_INSTANCE_SOFT_CAP`, `CITY_CHAT_RADIUS`, `STARTER_CITY_ID`, `STARTER_CITY_INSTANCE_PREWARM_COUNT`, `IDLE_INSTANCE_TEARDOWN_SECONDS`, `EVENT_ANNOUNCE_LEAD_SECONDS`, `MIN_AMBIENT_NPC_PER_CITY`, …) จาก CBS | ✅ designed |
| **M6 Account & Auth** | Hard | `last_city_id`, `last_city_entry_timestamp`, `current_instance_id`, `ft12_migrated` flag ฝังใน Player Record ; M6 ครอบ session validation + reconnect flow | ✅ designed |
| **FT11 Faction System** | Hard | Read `player.faction_id` (cosmetics only — badge, chat color, banner). FT12 = pure consumer ; FT11 R11 ระบุ contract | ✅ Approved |

### F.2 Downstream (Systems that depend on FT12)

| System | Type | Interface (FT12 → System) | Status |
|---|---|---|---|
| **FT13 Tournament System** | Hard | OUT: `(player_id, queue_type, origin_city_id)` → FT13 queue endpoint. IN: `match_found`, `match_end` events. FT12 = entry point + state-handoff bridge | 🆕 undesigned |
| **FT14 Fragment & Meta-Game** | Hard | IN: `fragment_event_started/ended(city_id, event_id, schema TBD)`. FT12 = display layer (Monument, Galaxy Map indicator, ระฆัง audio). FT14 = source of truth | 🆕 undesigned |
| **M11 Reputation / Anti-Toxicity** | Hard | OUT: chat events from city RadiusChat → M11 filter+accounting endpoint (synchronous filter-before-broadcast per EC-08). IN: `player_banned` event → force-exit (EC-11) | 🆕 undesigned |
| **FT8 Personal Dungeon** | Hard | OUT: `(player_id, dungeon_id, party_members)` → FT8 entry endpoint. IN: `dungeon_session_end` → `ReturningFromMatch` | 🔄 needs revision |

### F.3 Soft / Cross-cutting

| System | Type | Interface | Status |
|---|---|---|---|
| **M1 Social System** | Soft | FT12 surfaces M1 services (party invite, friend list, faction/party chat) ใน city UI. FT12 ไม่ implement social state ใหม่ | ✅ designed |
| **M10 Announcement System** | Soft | IN: global announcements (Tournament results, faction milestones) → FT12 displays as toast | ✅ designed |
| **F2 Networking Core** | Soft (deferred) | Network/sharding implementation pattern = ADR-level decision (R13). อาจใช้ Photon Fusion, PlayFab + SignalR, หรือ hybrid — defer | ✅ designed (ADR pending) |
| **FT6 Matchmaking** | Soft (transitive) | FT13 owns matchmaking ; FT12 calls FT13's queue endpoint ที่อาจ delegate ไป FT6. ไม่มี direct interface | ✅ designed |
| **C2 Hero System / M3 Customization** | Soft (presentation) | Hero/Skin selection UI ใน city = surface ของ C2/M3 data — pure UI consumer | ✅ designed (revise pending) |

### F.4 Deferred / Open

- **FT9 Town System** — DEPRECATED in pivot ; FT12 hard-supersedes. Migration table ใน C3. หลัง FT12 approved → FT9 GDD mark deprecated, no parallel operation.
- **F5 Audio System** — Ambient soundscape ของแต่ละ city (เสียงระฆังเฉพาะเมือง, ambient SFX) ผูกกับ city theme [Open Question]. Defer audio spec จนกว่า themes กำหนด.
- **M2 Battle Pass** — relationship กับ Fragment system [Open Question per FT11 OQ-4]. ถ้า Battle Pass ใส่ entry point ใน city → soft FT12 dependency. Defer until M2 retrofit.

### F.5 Bidirectional consistency notes

> **🆕 Undesigned downstream (FT13, FT14, M11):** เมื่อ author GDD แต่ละตัว ต้อง list "FT12 World Map" ใน Dependencies section ตามตาราง F.2.
>
> **✅ Designed systems ที่ต้อง revise (เพิ่ม "depended on by FT12" ใน Dependencies):**
> - F4 Data/Config — เพิ่ม city definitions schema + tuning knobs ใน managed config
> - M6 Account & Auth — เพิ่ม `last_city_id`, `current_instance_id`, `ft12_migrated` ใน Player Record schema section
> - FT11 Faction — เพิ่ม FT12 ใน "depended on by" list (FT11 F.5 retrofit pass already noted)
> - M1 Social — เพิ่ม FT12 ใน UI surface consumers
> - M10 Announcement — เพิ่ม FT12 ใน toast display channels
> - F2 Networking — note FT12 ADR pending สำหรับ pattern selection
> - FT8 Personal Dungeon — เพิ่ม FT12 entry path (note: FT8 อยู่ใน "Needs Revision" list ตาม pivot อยู่แล้ว)
> - C2 Hero / M3 Customization — note FT12 = surface consumer
>
> **⛔ Deprecated:** FT9 Town System — mark deprecated ใน systems-index หลัง FT12 approved. FT9 GDD เพิ่ม deprecation header pointing ไป FT12.
>
> รวมเข้า scope retrofit pass ของ pivot 2026-04-23 ([change-impact GDD Work Tracker](../../docs/architecture/change-impact-2026-04-23-tournament-pivot.md)).

## Tuning Knobs

> ทุก knob configurable ผ่าน CBS (F4) ; ไม่ hardcode. Defaults ตาม
> level-designer + ux-designer + systems-designer recommendations.

### G.1 Capacity & Sharding

| Knob | Default | Safe Range | What it affects | Live-tune? |
|---|---|---|---|---|
| `CITY_INSTANCE_SOFT_CAP` | **150** | 50 – 500 | จำนวน player ต่อ instance ที่ D1 จะ pack เข้าได้. ต่ำเกิน = instance proliferation, network overhead ; สูงเกิน = avatar density chaos, RadiusChat flood, AOI culling overload | ⚠️ Verify network load ก่อน change |
| `CITY_INSTANCE_HARD_CEILING_RATIO` | **1.2** (→180) | 1.0 – 1.5 | Multiplier ของ soft cap สำหรับ R9 party co-location override. กันไม่ให้ instance เดียวรองรับ party หลายชุดจน balloon | ❌ Principle-driven |
| `STARTER_CITY_ID` | **TBD** *(placeholder — รอ leadership)* | 1 ใน 10 cities | เมืองแรกสำหรับ first-ever spawn (R4) และ fallback ใน EC-13. ค่าจริง = decision ของ narrative + leadership | ❌ One-time per cycle |
| `STARTER_CITY_INSTANCE_PREWARM_COUNT` | **5** | 1 – 20 | จำนวน instance ของ starter city ที่ pre-warm ก่อน server accept connections (EC-19). 5 รองรับ 750 concurrent new players | ⚠️ Verify ก่อน launch event |
| `IDLE_INSTANCE_TEARDOWN_SECONDS` | **300** (5 นาที) | 60 – 1800 | Window ที่ instance pop=0 ก่อน teardown (EC-04). ต่ำเกิน = thrashing เมื่อมี player สลับเข้า ; สูงเกิน = waste resource | ✅ Live-tune |
| `D2_SCHEDULER_TICK_SECONDS` | **5** | 1 – 6 | ความถี่ที่ D2 overflow predicate run. ต่ำ = pre-warm responsive แต่ scheduler load สูง ; สูง = D1 step 3 fallback บ่อย. **Hard upper bound 6**: G.6 constraint `EVENT_ANNOUNCE_LEAD_SECONDS ≥ 5×tick` ต้องเป็น CBS validation rule — ที่ default LEAD=30, tick > 6 จะละเมิด | ⚠️ Verify scheduler budget |

### G.2 Travel & Transition

| Knob | Default | Safe Range | What it affects | Live-tune? |
|---|---|---|---|---|
| `GALAXY_MAP_TRANSITION_SECONDS` | **2.5** | 1.0 – 5.0 | Fade out → load → fade in duration ของ Galaxy Map travel (R5). สั้นเกิน = jarring, ผู้เล่นไม่มีเวลารับ "เปลี่ยนเมือง" feeling ; ยาวเกิน = friction | ✅ A/B หลัง playtest |
| `ENTERING_HOLD_SECONDS` | **60** | 30 – 300 | Server hold instance slot ระหว่าง player `EnteringCity` reconnect (EC-14). ต่ำเกิน = ผู้เล่นที่ network drop ต้อง re-pick instance ; สูงเกิน = ghost slot | ✅ Live-tune |
| `ENTERING_TIMEOUT_SECONDS` | **30** | 15 – 120 | Max time ที่ `EnteringCity` state ค้างได้ก่อน server force-fail (EC-18). ขึ้นกับ scene streaming budget | ⚠️ Match ต่อ asset pipeline budget |

### G.3 Event & Lifecycle

| Knob | Default | Safe Range | What it affects | Live-tune? |
|---|---|---|---|---|
| `EVENT_ANNOUNCE_LEAD_SECONDS` | **30** | 10 – 300 | Delay จาก Fragment Event announce → event เริ่มจริง (EC-20). ให้ pre-warm instance + spread thundering herd ของ player travel ; สั้นเกิน = server spike, ยาวเกิน = ลด urgency | ✅ A/B กับ event participation |
| `MIN_AMBIENT_NPC_PER_CITY` | **5** | 0 – 20 | Floor non-functional NPCs ที่เดิน/idle ในเมือง (EC-22). ป้องกัน "empty plaza" fantasy break ใน off-peak | ✅ Live-tune ตาม city pop data |
| `ANCHOR_VIGNETTE_ROTATION_DAYS` | **7** | 1 – 30 | Cadence ของ Anchor lore vignette rotation (R3.1). ต่ำ = vignette pool หมดเร็ว (narrative authoring overhead สูง) ; สูง = ผู้เล่นเห็นซ้ำบ่อย ทำลาย repeat-visit hook | ✅ Live-tune ตาม narrative budget |
| `ANCHOR_VIGNETTE_POOL_MIN` | **4** | 4 – 12 | จำนวน vignette ขั้นต่ำต่อ Anchor (R3.1). Floor 4 รองรับ 1 รอบเดือนที่ rotation=7 วัน | ❌ Content-driven |

### G.4 Spatial / Render

| Knob | Default | Safe Range | What it affects | Live-tune? |
|---|---|---|---|---|
| `AOI_RENDER_RADIUS_UNIT` | **20** | 10 – 50 | Radius รอบ camera ที่ render avatar เต็ม (level-designer recommendation). เกิน radius = "presence glow" บนพื้น. ต่ำเกิน = เมืองรู้สึกร้าง ; สูงเกิน = render cost cap | ⚠️ Verify GPU budget per platform |
| `CITY_CHAT_RADIUS_UNIT` | **15** | 5 – 30 | RadiusChat range ใน city plaza (R10). ต่ำกว่า FT9 default (เมื่อก่อน RadiusChat ออกแบบสำหรับเมืองโล่ง) เพื่อกันข้อความ flood ที่ density 150 player | ✅ A/B กับ chat readability data |
| `CENTRAL_PLAZA_RADIUS_UNIT` | **30** | 20 – 50 | Radius ของ plaza กลางเมือง — กำหนดพื้นที่ social gathering. ต่ำเกิน = แออัด, สูงเกิน = ผู้เล่นกระจายและ fantasy เสีย | ❌ Principle-driven (level design) |

### G.5 Diagnostic Thresholds (ops dashboard hooks)

| Knob | Default | Safe Range | What it affects | Live-tune? |
|---|---|---|---|---|
| `CITY_VISIT_FAIRNESS_INDEX_ALERT` | **0.3** (gini-style) | 0.1 – 0.7 | Threshold ของความไม่เท่ากันของ city traffic distribution (EC-21). **Computation:** Gini coefficient ของ `unique_visitors_per_city` (10 cities — รวมเมืองที่ visits=0) ; sample = rolling 24h window ; aggregate = daily snapshot at 00:00 UTC. **Trigger:** 7 consecutive daily samples ≥ threshold ⇒ "Anchor concentration warning" → re-engagement campaign / event redistribution review | ✅ Live-tune |
| `CITY_INSTANCE_OVERFLOW_RATE_ALERT` | **0.05** (5%) | 0.01 – 0.20 | Fraction ของ travel attempts ที่ trigger D1 step 3 (SpawnNewInstance fallback). > threshold ⇒ pre-warm policy ไม่ effective ; pre-warm count ต้อง bump | ✅ Live-tune |

### G.6 Knob Interactions

- `CITY_INSTANCE_SOFT_CAP` × `AOI_RENDER_RADIUS_UNIT` → effective avatar
  density บนหน้าจอ. ปรับ cap สูงขึ้นโดยไม่ลด render radius = GPU spike
- `CITY_INSTANCE_SOFT_CAP` × `CITY_CHAT_RADIUS_UNIT` → จำนวน chat broadcast
  per second per player. ที่ CAP=150, RADIUS=15 ผู้เล่น ~30-40 คนใน radius
  → tolerable. ที่ CAP=300, RADIUS=15 → 60-80 คน → chat flood
- `EVENT_ANNOUNCE_LEAD_SECONDS` × `D2_SCHEDULER_TICK_SECONDS` → effective
  pre-warm response time. ถ้า scheduler tick ใกล้ lead time → race
  เกิด overflow ก่อน pre-warm. **Rule: lead ≥ 5 × tick — CBS ต้อง enforce
  เป็น validation rule (reject config ที่ละเมิด ก่อน apply)** ; safe ranges
  ของ tick (1–6) คำนวณจาก default LEAD=30 ; ถ้า ops จะลด LEAD ต่ำกว่า 30,
  CBS validator ลด max tick ตามสูตร floor(LEAD/5) ก่อน accept
- `STARTER_CITY_INSTANCE_PREWARM_COUNT` × `IDLE_INSTANCE_TEARDOWN_SECONDS`
  → ของเสีย launch ; pre-warm สูง + teardown ช้า = idle resource หลัง
  launch peak. แนะนำ launch playbook: bump prewarm สำหรับ launch day
  แล้ว revert
- `CITY_INSTANCE_HARD_CEILING_RATIO` × ขนาด party สูงสุด (M1 owns) →
  ขีดจำกัด party-co-location flood. Verify M1 PARTY_MAX × HARD_CEILING_RATIO
  ไม่เกิน CITY_INSTANCE_SOFT_CAP × 2

> **Knob ที่ไม่อยู่ใน FT12 (อ้างถึงแต่ owned by อื่น):**
> - `FACTION_COUNT` → FT11 G.3 (FT12 อ่านเพื่อ render badge variety)
> - `PARTY_MAX` → M1 (FT12 ใช้ใน D1 party_size logic)
> - Fragment Event spawn rules → FT14 (FT12 รับ event เท่านั้น)

## Visual/Audio Requirements

> FT12 มี visual/audio surface ใหญ่ — identify touch-points ใน GDD นี้
> เพื่อ feed Art Bible + Asset Spec phase. รายละเอียด art direction +
> per-asset specs จะถูกระบุใน Asset Spec หลัง Art Bible approve.

### Visual touch-points

- **Central Plaza** (R3 spec) — radius `CENTRAL_PLAZA_RADIUS_UNIT` (30 unit),
  open-sky, floor pattern เปลี่ยนตาม city theme แต่ขนาดคงที่
- **8 fixed landmarks per city** (Tournament Queue Terminal, Personal Dungeon
  Gate, Equipment Alcove, Hero/Skin Shrine, Fragment Event Monument,
  Interstellar Gate, Faction Banner Racks, Keeper NPC) — ตำแหน่งสัมพัทธ์
  เหมือนกันทุก city ; สถาปัตยกรรมเฉพาะ theme
- **Fragment Event Monument** (R8 + B Anchor moment) — dormant sculpture →
  active state มี particle, glow, height transform เมื่อ event เริ่ม
- **Faction Banner Racks** (R7) — ปักธงอัตโนมัติตาม `faction_id` ของผู้เล่น
  ใน instance ; passive ambient ไม่ interactive ; live update เมื่อ
  faction switch (EC-23)
- **Galaxy Map overlay** (R5) — overhead view ของ 10 cities ; Fragment
  Event indicator ; faction-themed cosmetics ตาม `faction_id`
- **Player faction badge + chat color** (R7) — render บนหลังตัวละคร +
  chat message UI
- **AOI presence glow** (EC: density) — avatar นอก `AOI_RENDER_RADIUS_UNIT`
  render เป็น light point บนพื้น แทน full mesh
- **First-visit environmental beat** (B onboarding) — diegetic text
  appearance + ambient lighting cue ; trigger หลัง scene ready (EC-27)
- **Ambient NPC** (EC-22) — `MIN_AMBIENT_NPC_PER_CITY` non-functional NPCs
  เดิน + idle animations ใน plaza

### Audio touch-points

- **City bell** (R8 + B + EC-26) — diegetic 360-degree sound ; primary cue
  สำหรับ Fragment Event announce ; **เสียงเฉพาะตัวต่อ city** (theme-bound)
- **Ambient soundscape ต่อ city** — soundscape เฉพาะตัว 10 cities ตาม theme
  [Open Question: themes]
- **Travel transition audio** — fade-out cue + arrival cue สำหรับ Galaxy Map
  travel (R5)
- **Plaza ambient** — chatter, footsteps, faction banner cloth flap, fountain
  หรือ ambient ตาม theme

### City theme dependency

10 city themes คือ **blocking dependency** บน Art production — ไม่ใช่
nice-to-have. ถ้า theme spec ไม่ deliver ก่อน art begin → fallback risk
คือ "10 cities reskin texture-only" ซึ่งทำลาย spatial identity (ดู Section E
EC-22 + level-designer note ใน specialist round)

> **📌 Asset Spec** — Visual/Audio touch-points ถูก identify แล้ว. หลัง
> Art Bible approve, run `/asset-spec system:world-map-system` เพื่อ
> produce per-asset visual descriptions, dimensions, และ generation prompts
> จาก section นี้.

## UI Requirements

> FT12 มี UI touch-points จำนวนมาก — identify ใน GDD นี้เพื่อ feed UX spec
> phase. รายละเอียด screen flow + interaction patterns อยู่ใน UX specs
> ภายหลัง.

### UI surfaces (ระบุ touch-points ; รายละเอียดใน UX spec)

| UI Surface | Source | Notes |
|---|---|---|
| **Galaxy Map overlay** | R5 | Cross-city travel UI + Fragment Event status board ; hotkey + UI button trigger |
| **City Menu** (Keeper-mediated) | C3 + ux first-visit flow | รวม entry points ของ Universal Services 8 ตัว ; เปิดผ่าน Keeper NPC interaction |
| **Tournament Queue UI** | R2 | Casual / Ranked / Custom selection + queue status ; routes via FT13 contract |
| **Equipment Management UI** | R2 + FT9 migrate (C3 table) | 6-slot equipment สำหรับ Personal Dungeon (ไม่ใช่ MOBA) |
| **Hero/Skin Selection UI** | R2 + FT9 migrate | reuse C2/M3 surfaces |
| **Lobby Creation UI** | R2 + FT9 migrate | Custom match config |
| **Personal Dungeon Entry UI** | R2 + FT8 contract | Dungeon select + party invite |
| **Faction Profile / Standings widget** | R2 + FT11 read | Fragment pool, member count, Faction goals |
| **Cross-instance counter** | R13 | "ลานนี้ X / ลานร่วม Y" ที่ plaza entrance |
| **Travel-Queue confirmation dialog** | R6 | "การเดินทางจะยกเลิก queue ปัจจุบัน — ดำเนินการต่อ?" |
| **Spawn-failure toast** | EC-03 | "เมือง [X] หนาแน่นชั่วคราว — ลองใหม่ใน 30s" |
| **Match-found countdown modal** | EC-09 | 10s countdown ; defer 3s ถ้า text input focused |
| **Fragment Event toast** | EC-26 | Directional indicator + "northwest, in this city" |
| **Post-match "เดินทางไป [Event City]" shortcut** | C2 ReturningFromMatch | Shortcut button บน post-match screen |
| **Party invite banner** | M1 surface | Persistent banner, 30s timeout |
| **Faction badge (in-world)** | R7 | บนหลัง avatar ทุก player ใน instance |
| **First-visit environmental beat text** | B onboarding | Non-popup, fade in/out |

### UI priority hierarchy (EC-09 referent)

Priority order (สูง → ต่ำ):
1. Match-found countdown (interrupt ทุกอย่าง)
2. Party invite banner (time-sensitive)
3. Fragment Event toast (informational)
4. Chat (passive background)

### Accessibility / platform notes

[TO BE CONFIGURED — ดู `.claude/docs/technical-preferences.md` Input section
ที่ยังเป็น TBD ; ระบุ platform constraints หลัง leadership decision]

> **📌 UX Flag — World Map (FT12)**: ระบบนี้มี UI touch-points จำนวนมาก
> (17 surfaces). ใน Pre-Production phase ให้รัน `/ux-design` ต่อ screen
> ตามลำดับ priority:
> 1. `/ux-design galaxy-map`
> 2. `/ux-design city-menu`
> 3. `/ux-design tournament-queue`
> 4. `/ux-design fragment-event-display`
> 5. `/ux-design post-match-screen` (อาจ co-author กับ FT13)
>
> Stories ที่อ้างถึง UI ต้อง cite `design/ux/[screen].md` ไม่ใช่ GDD นี้
> โดยตรง.

## Acceptance Criteria

> **Format:** Given-When-Then. **TR-ID:** `TR-WMS-NNN`.
> **Story types:** Logic = unit test (BLOCKING) ; Integration = integration
> test (BLOCKING) ; Performance = load test (ADVISORY pre-milestone) ;
> Manual = smoke check (ADVISORY)
>
> **Coverage:** 35 criteria — Core Rules 15, Formulas 6, Edge Cases 9,
> Cross-System 3, Performance 2, Manual 2. — ดู H.7 สำหรับ criteria ที่
> defer รอ FT13/FT14/M11.

### H.1 Core Rules (R1–R14)

| TR-ID | Type | Given-When-Then |
|---|---|---|
| TR-WMS-001 (R1) | Logic | **Given** player ที่มี `faction_id` = "faction_b" (minority faction) **When** ส่ง TravelRequest ไปยัง city ใดก็ได้ใน 10 เมือง **Then** HTTP 200 ; `EnteringCity` state transition สำเร็จ ; ไม่มี faction-gate error |
| TR-WMS-002 (R2) | Logic | **Given** player state = `InCity` อยู่ใน city ใดก็ได้ **When** query available services **Then** response รวม 8 services ครบ (Hero/Skin, Equipment, LobbyCreation, TournamentQueue, PersonalDungeon, PartyFormation, SocialChat, FactionProfileView) ; ไม่มี service ที่ `available=false` |
| TR-WMS-003 (R3) | Logic | **Given** player ที่ไม่เคยเยือน city "Solis" (เมืองที่มี Anchor Content X) **When** compare numeric state ก่อนและหลังที่ player อื่น visit Solis แล้ว interact กับ Anchor Content X **Then** ไม่มีการเปลี่ยนแปลงใน XP, Fragment, Fragment multiplier, hero/equipment stats, faction pool, Tournament rank, queue priority, หรือ currency ของ player ที่ไม่ได้ไป |
| TR-WMS-004 (R4) | Logic | **Given** player มี `last_city_id` = "city_astra" **When** session expire แล้ว reconnect **Then** `EnteringCity` spawn destination = "city_astra" ; ไม่ใช่ `STARTER_CITY_ID` |
| TR-WMS-005 (R4 first-ever) | Logic | **Given** player ที่เพิ่ง complete FT11 faction selection (first-ever spawn) **When** enter World Map ครั้งแรก **Then** spawn destination = `STARTER_CITY_ID` (จาก CBS) ; `last_city_id` เขียนเป็น `STARTER_CITY_ID` บน Player Record |
| TR-WMS-006 (R5) | **Integration** | **Given** player state = `InCity`, `GALAXY_MAP_TRANSITION_SECONDS` = 2.5 **When** player คลิก city ปลายทาง → confirm บน Galaxy Map overlay **Then** `EnteringCity` state ตั้งขึ้นภายใน 200ms ; scene load complete + `InCity` state ที่ปลายทางใน ≤ 2.5 + 1.0s (transition + load tolerance) ; ไม่มี physical portal ที่ต้องเดินไป |
| TR-WMS-007 (R6) | Logic | **Given** player state = `InCity+Queued` **When** player เปิด Galaxy Map และเลือกเมืองปลายทาง **Then** system แสดง confirmation dialog ที่มีข้อความยืนยันยกเลิก queue ; state ยังคง `InCity+Queued` จนกว่าผู้เล่นกด confirm ; ถ้ากด confirm → queue ยกเลิก + `EnteringCity` transition |
| TR-WMS-008 (R7) | Logic | **Given** player state = `InCity`, `faction_id` = "faction_c" **When** server evaluate write path จาก FT12 module **Then** ไม่มี SQL/API call ใดที่ write `faction_id` field ; FT12 source code ไม่มี setter/mutation สำหรับ `faction_id` |
| TR-WMS-009 (R8) | **Integration** | **Given** mock FT14 ส่ง `fragment_event_started(city_id="city_nova", event_id="evt_001")` **When** FT12 รับ event **Then** ภายใน 500ms: (1) Monument ของ city_nova ทุก instance emit `transform_active` signal ; (2) Galaxy Map indicator ของ city_nova = `active` ; (3) audio event `city_bell_ring` trigger ; FT12 ไม่เขียน Fragment state ใดๆ |
| TR-WMS-010 (R9) | **Integration** | **Given** party 3 คน (A ใน city_astra_01 pop=100, B+C travel จากนอก) **When** B+C travel ไป city_astra พร้อมกัน **Then** D1 step 1 co-locate → B+C ถูก assign city_astra_01 เหมือน A ; `current_instance_id` ของ B,C = "city_astra_01" |
| TR-WMS-011 (R10) | Logic | **Given** player A ใน city_astra_01 ส่ง RadiusChat message **When** player B ใน city_astra_02 (instance ต่างกัน) **Then** B ไม่ได้รับ message ; ไม่มี cross-instance chat leak ; faction chat ของ B ยังรับได้ปกติ (M1 scope) |
| TR-WMS-012 (R11) | Logic | **Given** player ใน `InCity` ส่ง chat message ด้วยเนื้อหา flagged **When** M11 filter-before-broadcast endpoint รับ request **Then** message ไม่ broadcast ไปยัง recipient ใดๆ ใน city instance ; FT12 ไม่มี internal filter logic (delegate ทั้งหมดไป M11) ; ไม่มี PvP/duel API ที่เรียกได้ใน city context |
| TR-WMS-013 (R12) | Logic | **Given** client ส่ง `TeleportRequest(city_id="city_solis")` โดยตรงโดยไม่ผ่าน Azure Function validation **Then** server reject ; `last_city_id` ไม่เปลี่ยน ; `current_instance_id` ไม่เปลี่ยน ; HTTP 403 |
| TR-WMS-014 (R13) | Logic | **Given** city_astra มี 1 instance pop=150 (`CITY_INSTANCE_SOFT_CAP`=150), solo player travel มา **When** D1 step 2 evaluate Astra_01 **Then** 150 + 1 = 151 > 150 → reject step 2 ; D1 step 3 SpawnNewInstance("city_astra") → player assigned Astra_02 ; Astra_02 initial pop=1 |
| TR-WMS-015 (R14) | **Integration** | **Given** player มี FT9 `last_town_state` บน Player Record (pre-patch) + `ft12_migrated` flag ยังไม่ set **When** player login หลัง FT12 patch deploy **Then** `last_city_id` = `STARTER_CITY_ID` ; equipment state มาครบ (ไม่หาย) ; `ft12_migrated` = true ; login ที่ 2 ไม่ re-migrate (flag ป้องกัน double-migrate) |

### H.2 Formulas (D1, D2)

| TR-ID | Type | Given-When-Then |
|---|---|---|
| TR-WMS-016 (D1 worked example) | Logic | **Given** `CITY_INSTANCE_SOFT_CAP`=150, city_astra มี Astra_01(pop=148) + Astra_02(pop=80), party_size=3, ไม่มี party member ใน Astra อยู่ก่อน **When** D1 run **Then** step 1 skip (no co-location) ; step 2 sort desc → [Astra_01(148), Astra_02(80)] ; Astra_01: 148+3=151>150 reject ; Astra_02: 80+3=83≤150 → assign Astra_02 ; return "astra_02" |
| TR-WMS-017 (D1 boundary exact) | Logic | **Given** `CITY_INSTANCE_SOFT_CAP`=150, instance_X pop=147, party_size=3 **When** D1 step 2 evaluate instance_X **Then** 147+3=150 ≤ 150 → **accept** ; return instance_X (boundary inclusive) |
| TR-WMS-018 (D1 boundary off-by-one) | Logic | **Given** `CITY_INSTANCE_SOFT_CAP`=150, instance_X pop=148, party_size=3 **When** D1 step 2 evaluate instance_X **Then** 148+3=151 > 150 → **reject** ; D1 fallback ไป step 3 SpawnNewInstance |
| TR-WMS-019 (D1 tie-break EC-01) | Logic | **Given** city_nova มี Nova_01(pop=100) + Nova_02(pop=100), party_size=1 **When** D1 step 2 sort desc (tie) **Then** tie-break ด้วย instance_id lexicographic ascending → Nova_01 evaluate ก่อน ; return "nova_01" (ไม่สุ่ม ; deterministic ทุก run) |
| TR-WMS-020 (D2 false) | Logic | **Given** city_solis มี [Solis_01(pop=150), Solis_02(pop=150), Solis_03(pop=142)], `CITY_INSTANCE_SOFT_CAP`=150 **When** D2 evaluate **Then** all ≥ 150? → false (Solis_03=142) ; return false ; ไม่ trigger pre-warm |
| TR-WMS-021 (D2 true + empty) | Logic | **Given** city_nova ไม่มี instance ใดๆ (`instances.is_empty()=true`) **When** D2 evaluate **Then** return true ; scheduler trigger SpawnNewInstance("city_nova") |

### H.3 Edge Cases (priority subset)

| TR-ID | Type | Given-When-Then |
|---|---|---|
| TR-WMS-022 (EC-02 D1 near-full reject) | Logic | **Given** `CITY_INSTANCE_SOFT_CAP`=150, instance_Y pop=149, party_size=2 **When** D1 step 2 evaluate instance_Y **Then** 149+2=151>150 → reject ; SpawnNewInstance trigger ; instance_Y pop ยังคง 149 (ไม่เพิ่ม) |
| TR-WMS-023 (EC-03 spawn failure) | Logic | **Given** server resource exhausted → `SpawnNewInstance` return error **When** D1 step 3 invoke **Then** client เห็น toast ข้อความขึ้นต้นด้วย "เมือง [X] หนาแน่นชั่วคราว" ; player state ยังคง `InCity` (เมืองเดิม) ; travel ไม่ commit ; `last_city_id` ไม่เปลี่ยน |
| TR-WMS-024 (EC-07 stale co-location) | Logic | **Given** party member B มี `last_heartbeat` > 5s (just logged out), A travel ไป city ที่ B เคยอยู่ **When** D1 step 1 evaluate co-location candidate B **Then** B ถูก ignore (freshness window fail) ; D1 fall through ไป step 2 (pack) ; ไม่ assign instance ที่ B เคยอยู่โดยอัตโนมัติ |
| TR-WMS-025 (EC-08 ban + in-flight chat) | **Integration** | **Given** player X ส่ง chat message ที่ยัง pending filter ใน M11 pipeline **When** M11 ส่ง `player_banned(player_X)` มาถึงพร้อมกัน **Then** pending message ถูก drop ; message ไม่ broadcast ไปยัง recipient ใดๆ ; ไม่มี "last word" escape |
| TR-WMS-026 (EC-10 Galaxy Map double-tap) | Logic | **Given** player อยู่ใน `EnteringCity` transition state **When** player กด Galaxy Map nav input อีกครั้ง (double-tap) **Then** input layer block ; second TeleportRequest ถูก server reject ถ้า arrive ก่อน first complete ; state ยังคง `EnteringCity` ไม่เปลี่ยน |
| TR-WMS-027 (EC-11 ban while InCity) | **Integration** | **Given** player state = `InCity` **When** M11 ส่ง `player_banned(player_id)` **Then** state transition → `OutOfUniverse` ภายใน 1s ; player ถูก force-exit จาก instance roster ; `current_population` ของ instance ลดลง 1 ; broadcast message ใน instance ไม่มีคำว่า "banned" / ไม่ระบุเหตุผล |
| TR-WMS-028 (EC-15 FT9 migration idempotent) | **Integration** | **Given** player มี `ft12_migrated`=true อยู่แล้ว (migration เคยทำแล้ว) **When** player login อีกครั้ง **Then** migration logic ไม่รัน ; `last_city_id` ไม่ถูก reset ; equipment state ไม่เปลี่ยน |
| TR-WMS-029 (EC-23 faction switch live) | **Integration** | **Given** player A อยู่ใน city_astra_01 ร่วมกับ player B **When** A complete faction switch (FT11 R6) ขณะ `InCity` **Then** B เห็น badge/banner ของ A update ภายใน 2s (presence delta broadcast) ; chat messages เก่าของ A ใน instance ยัง render ด้วย color faction เดิม (immutable post-send) |
| TR-WMS-030 (EC-19 launch flood) | **Integration** | **Given** `STARTER_CITY_INSTANCE_PREWARM_COUNT`=5, `CITY_INSTANCE_SOFT_CAP`=150 ; pre-warmed ก่อน server accept connections **When** 750 new players spawn ที่ `STARTER_CITY_ID` พร้อมกัน **Then** ทุก player ได้รับ instance assignment ≤ 200ms ; ไม่มี player ที่เห็น toast "หนาแน่นชั่วคราว" (5×150=750 รองรับพอดี) |

### H.4 Cross-System Integration

| TR-ID | Type | Given-When-Then |
|---|---|---|
| TR-WMS-031 (FT11 read-only contract) | **Integration** | **Given** mock FT11 ให้ `player.faction_id`="faction_d" **When** player enter city และ FT12 render badge + chat color **Then** badge แสดง faction_d visual ; FT12 ไม่มี write call ไปยัง FT11 endpoint ใดๆ ตลอด session ; FT11 Player Record ไม่เปลี่ยนแปลง |
| TR-WMS-032 (FT13 queue handoff — mock) | **Integration** | **Given** mock FT13 endpoint พร้อมรับ ; player state = `InCity` ที่ city_astra **When** player กด confirm Tournament queue **Then** FT12 ส่ง `(player_id, queue_type, origin_city_id="city_astra")` ไปยัง mock FT13 endpoint ; player state = `InCity+Queued` ; `origin_city_id` snapshot = "city_astra" |
| TR-WMS-033 (M11 RadiusChat forward — mock) | **Integration** | **Given** mock M11 filter endpoint พร้อมรับ ; player ส่ง RadiusChat message ใน city_astra_01 **When** message submit **Then** FT12 forward message ไปยัง mock M11 endpoint ก่อน broadcast (synchronous) ; ถ้า M11 return `{blocked:true}` → message ไม่ broadcast ; ถ้า return `{blocked:false}` → broadcast ไปเฉพาะ player ใน city_astra_01 radius |

### H.5 Performance / Load (ADVISORY — pre-milestone gate)

| TR-ID | Type | Given-When-Then |
|---|---|---|
| TR-WMS-034 (EC-19 launch throughput) | **Performance** | **Given** staging ตั้ง `STARTER_CITY_INSTANCE_PREWARM_COUNT`=5, `CITY_INSTANCE_SOFT_CAP`=150 **When** load test inject 5,000 TravelRequest ไปยัง `STARTER_CITY_ID` ใน 10s window **Then** p95 instance assignment latency ≤ 500ms ; D1 step 3 SpawnNewInstance invoke rate < 10% ของ requests ; ไม่มี HTTP 5xx |
| TR-WMS-035 (EC-20 Fragment Event spike) | **Performance** | **Given** `EVENT_ANNOUNCE_LEAD_SECONDS`=30, `D2_SCHEDULER_TICK_SECONDS`=5 **When** Fragment Event announce trigger แล้ว 1,000 TravelRequest ไปยัง event city ใน 5s หลัง announce **Then** D2 pre-warm trigger ≥ 1 ครั้งภายใน 5s แรกหลัง announce ; p95 assignment latency ≤ 500ms ; ไม่มี thundering-herd SpawnNewInstance burst > 3 instance ต่อ tick |

### H.6 Manual Smoke (ADVISORY)

- **Manual-WMS-01 (B — first-visit fantasy):** QA login ด้วยบัญชีใหม่ที่ผ่าน FT11 faction selection แล้ว → เข้า World Map ครั้งแรก → verify: (1) ระฆังเมืองดัง, (2) ข้อความ environmental beat ปรากฏ (ไม่ใช่ tooltip popup), (3) ข้อความระบุว่าเมืองนี้เป็น "ลานกลาง" ตาม tone Section B → ไม่มี texture pop-in ขณะ beat play (EC-27 grace period).

- **Manual-WMS-02 (EC-22 — off-peak ambient NPC):** QA เข้า city ที่ `current_population`=0 (real player) ใน off-peak window → verify: (1) ambient NPC จำนวน ≥ `MIN_AMBIENT_NPC_PER_CITY` (default 5) ปรากฏบน plaza, (2) NPC มี idle/walk animation active, (3) NPC ไม่ respond ต่อ input และไม่มี interaction prompt.

### H.7 Deferred until downstream GDDs designed

รายการต่อไปนี้ต้องการ GDD ของระบบ downstream ก่อนจึงจะเขียน AC ที่ testable ได้:

- **EC-05 (party split travel race)** — ต้องการ M1 party state contract เพื่อ define "leader destination wins" verification path
- **EC-06 (party co-location hard ceiling)** — ต้องการ M1 `PARTY_MAX` value confirmed; AC boundary value ขึ้นกับ `M1.PARTY_MAX × HARD_CEILING_RATIO`
- **EC-09 (match countdown + input focus)** — ต้องการ FT13 GDD: countdown duration + match-found signal format
- **EC-12 (match-end signal arrives during EnteringCity)** — ต้องการ FT13 `match_end` payload schema
- **EC-13 (origin_city_id corrupt)** — ต้องการ FT13 match lifecycle spec
- **EC-20 Fragment Event spike (server-side rate limiting)** — TR-WMS-035 ทดสอบ behavior ด้าน FT12 ได้แล้ว ; full integration test รอ ADR + F2 spec
- **EC-25 (cross-region clock drift compensation)** — ต้องการ FT14 event timestamp schema + region topology จาก F2/ops

## Open Questions

> รวม decisions ที่ยังไม่ตัดสินใน GDD นี้ (defer ไว้). แต่ละข้อมี owner +
> trigger event ที่ปลด open status

### OQ-1 — `STARTER_CITY_ID`
- **Owner:** Leadership / Narrative Director
- **Default ใน GDD:** TBD placeholder (Section G.1)
- **Trigger:** Narrative team commit theme spec ของ 10 cities → leadership
  เลือกเมืองแรก
- **Impact:** R4 (first-ever spawn), EC-13 (fallback), EC-19 (launch flood prewarm)

### OQ-2 — Theme เฉพาะตัวของ 10 cities
- **Owner:** Narrative Director + World Builder + Art Director
- **Default:** ไม่มี — placeholder เท่านั้น
- **Trigger:** Pre-production narrative pass + art bible faction archetype section
- **Impact:** R3 (Anchor Content design), Visual/Audio (theme-bound bell sound,
  ambient soundscape), EC-22 (ambient NPC design), Art production blocking
- **Cross-ref:** อ้างถึงใน [game-concept.md](game-concept.md) Open Questions

### OQ-3 — Anchor Content รายการสมบูรณ์ของ 10 cities
- **Owner:** Narrative Director + Game Designer
- **Default ใน GDD:** ระบบ Anchor Content design เสร็จ (R3) แต่ content list ว่าง
- **Trigger:** หลัง OQ-2 (themes) lock ; narrative + design design specific Anchors
- **Impact:** R3 detail ; UI surface count (Anchor screen ต่อ city)

### OQ-4 — FT14 Fragment Event payload schema
- **Owner:** FT14 Fragment & Meta-Game GDD author
- **Default ใน GDD:** assume minimal schema `(city_id, event_id)` ใน R8
- **Trigger:** FT14 GDD authoring
- **Impact:** R8, TR-WMS-009 (mock spec อาจ revise), EC-25 (clock skew compensation)

### OQ-5 — Network/sharding pattern (ADR-level)
- **Owner:** Technical Director + Network Programmer
- **Default ใน GDD:** behavior ระบุ (R13) ; pattern (Photon Fusion / PlayFab+SignalR / hybrid) defer
- **Trigger:** ADR phase ก่อน implementation
- **Impact:** R13 implementation ; F.3 F2 Networking Core dependency status ;
  EC-19/20 server-side rate limiting policy

### OQ-6 — M2 Battle Pass entry point ใน city
- **Owner:** Live-Ops Designer (เมื่อ M2 retrofit)
- **Question:** Battle Pass ใส่ entry point ใน Universal Services list (R2) หรือไม่?
- **Trigger:** M2 retrofit (อยู่ใน change-impact GDD Work Tracker)
- **Impact:** R2 (เพิ่ม service) ; UI Requirements (เพิ่ม UI surface)
- **Cross-ref:** สอดคล้องกับ FT11 OQ-4

### OQ-7 — Platform / Input constraints
- **Owner:** Producer + Leadership
- **Question:** target platforms (PC, console, mobile, web) + input methods
  (mouse+keyboard, gamepad, touch) ส่งผลต่อ Galaxy Map UI, City Menu navigation,
  Keeper interaction model
- **Default ใน GDD:** assume PC + mouse+keyboard (FT9 pattern)
- **Trigger:** Leadership decision ก่อน Pre-Production
- **Impact:** UI Requirements platform notes ; Galaxy Map input method
- **Cross-ref:** [`technical-preferences.md`](../../.claude/docs/technical-preferences.md) Input section

### OQ-8 — Post-FT9 deprecation parallel-operation period
- **Owner:** Producer + Engineering
- **Question:** FT9 ถูก hard-deprecate (R14) ทันที FT12 launch หรือมี soft
  deprecation window?
- **Default ใน GDD:** ไม่มี parallel operation (ทันที)
- **Trigger:** Engineering review + production planning
- **Impact:** EC-15 migration urgency ; FT9 GDD deprecation header timing

### OQ-9 — Cross-faction stranger interaction affordance
- **Owner:** UX Designer + Game Designer
- **Question:** ใน plaza, ผู้เล่นต่างฝ่ายที่ไม่รู้จักกันมี affordance อะไร
  ในการ acknowledge ซึ่งกันและกันโดยไม่ต้องผ่าน chat / party invite?
  (ตัวเลือกที่พิจารณา: faction greeting emote กดที่ player target,
  shared-bench co-sit recognition cue, ฯลฯ)
- **Default ใน GDD:** ยังไม่มี — strangers จาก factions ต่างกันมี spatial
  proximity แต่ไม่มี designed interaction. R15 ambient affordances
  (bench, emote spot) เป็นจุดเริ่มต้น แต่ยังไม่ระบุ targeted stranger
  recognition
- **Trigger:** /ux-design plaza-affordances + /ux-design city-menu phase
- **Impact:** Section B Player Fantasy ("ทุกฝ่ายมาเจอกันโดยไม่ต้องเป็น
  ศัตรู") — ถ้าไม่มี affordance, framing นี้รับได้แค่ระดับ presence
  ไม่ใช่ encounter ; ส่งผลต่อ P3 Team Synergy supporting pillar
- **Cross-ref:** R15 ambient affordances (linger reason) ; UX spec
  ภายหลัง
