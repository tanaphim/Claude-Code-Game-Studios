---
status: reverse-documented
source: C:\GitHub\delta-unity
date: 2026-04-02
last-revised: 2026-04-23
revision-source: design/decisions/meeting-2026-04-23-tournament-pivot.md
---

# Delta — Systems Index

## Overview

เอกสารนี้แจกแจงระบบทั้งหมดของเกม Delta พร้อม dependencies, ลำดับความสำคัญ,
และลำดับการเขียน GDD เป็นแผนที่หลักสำหรับการออกแบบและพัฒนา

**จำนวนระบบทั้งหมด**: 41 (37 เดิม − 3 deprecate + 7 ใหม่)
**สถานะ**: Reverse-documented จาก codebase ที่มีอยู่ (1,139 C# scripts) +
revised หลัง [Tournament Pivot 2026-04-23](../decisions/meeting-2026-04-23-tournament-pivot.md)

> **Pivot 2026-04-23:** Territory War (FT10) + sub-systems (FT10a/FT10b) ถูกแทนด้วย
> Tournament + Faction + World Map (10 cities) + Fragment + Wager + Reputation +
> Manga Influence Loop. ดู [change-impact report](../../docs/architecture/change-impact-2026-04-23-tournament-pivot.md)
> สำหรับ ADR impact analysis

---

## Systems Enumeration

### Foundation (ระบบพื้นฐาน — ไม่ขึ้นกับระบบอื่น)

| ID | ระบบ | คำอธิบาย | ที่มา |
|----|------|---------|-------|
| F1 | Actor System | ระบบ entity พื้นฐานของตัวละคร/สิ่งมีชีวิตทุกอย่างในเกม (stats, health, state) | จากโค้ด |
| F2 | Networking Core | ชั้น networking พื้นฐาน (Photon Fusion) — connection, room, state sync | จาก concept |
| F3 | Input System | รับ input จากผู้เล่น (keyboard, mouse, controller) | โดยนัย |
| F4 | Data/Config System | ระบบโหลดข้อมูล config, stat tables, ค่าสมดุลจากไฟล์ภายนอก | โดยนัย |
| F5 | Audio System | ระบบเสียง (SFX, เพลง, UI sounds) | จากโค้ด |

### Core (ระบบแกน — ขึ้นกับ Foundation)

| ID | ระบบ | คำอธิบาย | ที่มา |
|----|------|---------|-------|
| C1 | Combat & Skills System | คำนวณความเสียหาย, hit detection, status effects, สกิล Q/W/E/R, cooldown, skill state machine | จาก concept |
| C2 | Hero System | ฮีโร่ 25+ ตัว — stats per level, บทบาท (6 roles), scaling, skill sets เฉพาะตัว | จาก concept |
| C3 | Movement & Navigation | การเคลื่อนที่ตัวละคร, pathfinding, collision | โดยนัย |
| C4 | Gold Economy | ระบบทอง (passive income, last hit, kill reward, assist gold) | จาก concept |
| C5 | Level/XP System | ระบบเลเวลระหว่างแมตช์ + skill points | จาก concept |

### Feature (ระบบฟีเจอร์ — ขึ้นกับ Core)

| ID | ระบบ | คำอธิบาย | ที่มา |
|----|------|---------|-------|
| FT1 | Item System | ไอเทม 7 ระดับ, คราฟ/รวม, 6 ช่อง, item effects (Lifeline, Cleave ฯลฯ) | จาก concept |
| FT1-ANIM | Item Animation System | Animation routing ต่อ ItemAnimationType, AnimatorOverrideController per hero/weapon, placeholder clips | Implemented |
| FT2 | Map & Objectives | 3 เลน, หอคอย, ค่ายทหาร, บอส/Mini-Boss, จุดฮีล | จาก concept |
| FT3 | Creep/Minion System | สร้างคลื่นมินเนี่ยน, ป่า camps, scaling ตามเวลา, Super Creep | จาก concept |
| FT4 | Fog of War | ระบบ AOI visibility, พุ่มไม้, vision revealers | จาก concept |
| FT5 | AI/Bot System | บอทควบคุม CPU — pathfinding, targeting, ใช้สกิล (FuzzySkillAI) | จากโค้ด |
| FT6 | Matchmaking | จับคู่ Ranked/Casual, lobby, team assignment, hero draft | จาก concept |
| FT7 | Game Mode Manager | จัดการโหมดเกมทั้งหมด (Ranked, Casual, Arcade, Training, Dungeon, Town) | จาก concept |
| FT8 | Dungeon Mode | PvE เดี่ยว — ด่าน/เวฟ, ระบบมอนสเตอร์, progression | จาก concept |
| FT9 | Town System | Hub โซเชียล — NPC, ภารกิจ, customization, พบเพื่อน | จาก concept |
| ~~FT10~~ | ~~Territory War~~ | ⛔ **Superseded 2026-04-23** by FT11/FT12/FT13/FT14 — see [decision](../decisions/meeting-2026-04-23-tournament-pivot.md) | deprecated |
| ~~FT10a~~ | ~~Citizen System~~ | ⛔ **Removed 2026-04-23** (never authored) | deprecated |
| ~~FT10b~~ | ~~Mercenary System~~ | ⛔ **Removed 2026-04-23** (never authored) | deprecated |
| FT11 | Faction System | 5–6 ฝ่าย, ผู้เล่นเลือกฝ่าย, switch rules, faction-shared Fragment pool | pivot 2026-04-23 |
| FT12 | World Map (10 Neutral Cities) | 10 ดาว/เมืองกลาง — socialize hub, theme เฉพาะตัว, Fragment drop locations | pivot 2026-04-23 |
| FT13 | Tournament System | MOBA 5v5 หลัก (+1v1/2v2/3v3 on request), Casual/Ranked, bracket + matchmaking, Top 10% Ranked → Fragment | pivot 2026-04-23 |
| FT14 | Fragment & Meta-Game System | Daily Fragment drops, faction pool aggregation, ~100M → ring → "กฎ Universe", reset cycle (~3 ปี) | pivot 2026-04-23 |
| FT15 | Wager Mode | เมื่อ Fragment ประจำวันหมด — วาง Fragment ของตัวเองเดิมพันชิงของผู้อื่น (escrow) | pivot 2026-04-23 |

### Presentation (ระบบ UI/UX — ห่อหุ้มระบบ gameplay)

| ID | ระบบ | คำอธิบาย | ที่มา |
|----|------|---------|-------|
| P1 | HUD & In-Game UI | HP/MP bar, minimap, skill icons, scoreboard, chat, ping | โดยนัย |
| P2 | Item Shop UI | หน้าร้านค้าในเกม, ค้นหา, แนะนำ, ซื้อ/ขาย | โดยนัย |
| P3 | Hero Select UI | หน้า draft/เลือกฮีโร่, แสดงบทบาท, สกิล | โดยนัย |
| P4 | Menu & Lobby UI | หน้าหลัก, lobby, settings, โปรไฟล์ | โดยนัย |
| P5 | Camera System | กล้องติดตามผู้เล่น, free camera (Z), camera shake | จากโค้ด |

### Meta/Polish (ระบบ metagame และ polish)

| ID | ระบบ | คำอธิบาย | ที่มา |
|----|------|---------|-------|
| M1 | Social System | เพื่อน, แคลน, แชท, เชิญเข้าเกม, บล็อค | จาก concept |
| M2 | Battle Pass | progression ตามฤดูกาล, ภารกิจ, รางวัล | จาก concept |
| M3 | Customization | สกิน/ชุดฮีโร่ ตามระดับ (Common > Legendary), อุปกรณ์ Town | จาก concept |
| M4 | Notification System | แจ้งเตือน real-time (SignalR) — เพื่อน online, เชิญ, ข่าว | จากโค้ด |
| M5 | Statistics & History | สถิติแมตช์, ประวัติการเล่น, KDA, win rate | จากโค้ด |
| M6 | Account & Auth | ล็อกอิน, สร้างบัญชี, เชื่อม Steam, PlayFab | จากโค้ด |
| M7 | AFK Detection | ตรวจจับผู้เล่นที่ไม่ทำอะไร, ลงโทษ/แทนที่บอท | จากโค้ด |
| M8 | Surrender System | โหวตยอมแพ้ระหว่างแมตช์ | จากโค้ด |
| M9 | Tutorial System | สอนผู้เล่นใหม่ — การเคลื่อนที่, สกิล, ไอเทม, objective | โดยนัย |
| M10 | Announcement System | ประกาศ kill streak, first blood, objective taken | จากโค้ด |
| M11 | Reputation / Anti-Toxicity System | Reputation score, หักคะแนนเมื่อพิมพ์คำหยาบ → ล็อคจาก Ranked, chat filter integration | pivot 2026-04-23 |
| M12 | Manga Influence Loop | ผลในเกม → influence story ในมังงะ; ผู้เล่นเก่งอาจกลายเป็นตัวละคร [🔮 Deferred — รอ proposal] | pivot 2026-04-23 |

---

## Dependency Map

```
Layer 0 — Foundation
├── [F1] Actor System
├── [F2] Networking Core
├── [F3] Input System
├── [F4] Data/Config System
└── [F5] Audio System

Layer 1 — Core
├── [C3] Movement & Navigation       ← F1, F3, F2
├── [C1] Combat & Skills System      ← F1, F2, F3
├── [C2] Hero System                 ← F1, C1, F4
├── [C4] Gold Economy                ← F1, F2, F4
└── [C5] Level/XP System             ← F1, F2, F4

Layer 2 — Feature
├── [FT1] Item System                ← C2, C4, C1, F4
├── [FT2] Map & Objectives           ← F1, C1, F2
├── [FT3] Creep/Minion System        ← F1, C1, FT2, C3
├── [FT4] Fog of War                 ← F1, FT2, F2
├── [FT5] AI/Bot System              ← C2, C1, C3
├── [FT6] Matchmaking                ← F2, C2, F4
├── [FT7] Game Mode Manager          ← FT6, FT2, F2
├── [FT8] Dungeon Mode               ← FT7, FT5, C1, FT2
├── [FT9] Town System                ← F1, C3, F4, F2
├── [FT11] Faction System            ← F4, M6
├── [FT12] World Map (10 Cities)     ← FT11, FT9, F2
├── [FT13] Tournament System         ← FT6, FT7, FT11, F2
├── [FT14] Fragment & Meta-Game      ← FT13, FT11, F4
└── [FT15] Wager Mode                ← FT14, F4

Layer 3 — Presentation
├── [P1] HUD & In-Game UI            ← C1, C4, C5, FT4
├── [P2] Item Shop UI                ← FT1, C4
├── [P3] Hero Select UI              ← C2, FT6
├── [P4] Menu & Lobby UI             ← FT6, M1, F4
└── [P5] Camera System               ← F1, F3, FT2

Layer 4 — Meta/Polish
├── [M1] Social System               ← F2, M6, F4
├── [M2] Battle Pass                 ← F4, M6, M5
├── [M3] Customization               ← C2, F4, M6
├── [M4] Notification System         ← F2, M1, M6
├── [M5] Statistics & History        ← C1, C4, C5, M6
├── [M6] Account & Auth              ← F2, F4
├── [M7] AFK Detection               ← F1, F2, F3
├── [M8] Surrender System            ← F2, FT7
├── [M9] Tutorial System             ← C2, C1, FT1, FT2, P1
├── [M10] Announcement System        ← C1, FT2, F2
├── [M11] Reputation/Anti-Toxicity   ← M1, M6, F4
└── [M12] Manga Influence Loop       ← FT14 (deferred)
```

### ระบบคอขวด (High-Risk Bottlenecks)

| ระบบ | จำนวนที่พึ่งพา | ความเสี่ยง |
|------|---------------|-----------|
| Actor System (F1) | 17 ระบบ | สูงมาก |
| Networking Core (F2) | 16 ระบบ | สูงมาก |
| Combat & Skills (C1) | 12 ระบบ | สูง |
| Data/Config (F4) | 11 ระบบ | สูง |
| Hero System (C2) | 7 ระบบ | กลาง |
| Map & Objectives (FT2) | 7 ระบบ | กลาง |
| Faction System (FT11) | 4 ระบบ (FT12, FT13, FT14, M11) | กลาง — **new bottleneck post-pivot** |
| Tournament System (FT13) | 2 ระบบ (FT14, FT15) | กลาง |

---

## Priority Tiers

### MVP — ต้องมีเอกสารก่อน (แก่นของเกม)

| ID | ระบบ | เหตุผล |
|----|------|--------|
| F1 | Actor System | ทุกอย่างพึ่งพา — ต้องเข้าใจก่อน |
| F4 | Data/Config System | กำหนดว่าค่าสมดุลเก็บที่ไหน/อย่างไร |
| C1 | Combat & Skills System | แก่นของ MOBA — damage, สกิล, status effects |
| C2 | Hero System | ฮีโร่ 25+ ตัว ต้องมีมาตรฐานการออกแบบ |
| C4 | Gold Economy | เศรษฐกิจในแมตช์กำหนดจังหวะเกม |
| C5 | Level/XP System | power curve ต้องสมดุลกับ Gold + Items |
| FT1 | Item System | 7 ระดับ + คราฟ — ซับซ้อน ต้องบันทึกสูตร |
| FT2 | Map & Objectives | กำหนดว่าแมตช์จบอย่างไร |

### Vertical Slice — gameplay loop ครบวงจร

| ID | ระบบ | เหตุผล |
|----|------|--------|
| C3 | Movement & Navigation | การเคลื่อนที่พื้นฐาน |
| FT3 | Creep/Minion System | เลนไม่มีความหมายถ้าไม่มีมินเนี่ยน |
| FT4 | Fog of War | กลยุทธ์หลักของ MOBA |
| FT6 | Matchmaking | ผู้เล่นเข้าเกมได้อย่างไร |
| FT7 | Game Mode Manager | จัดการโหมด (5v5 standard, 25v25 deferred) |
| FT11 | Faction System | foundation ของ Tournament/World Map/Fragment |
| FT13 | Tournament System | core meta loop ใหม่ (แทน FT10) |
| P1 | HUD & In-Game UI | ผู้เล่นอ่านข้อมูลในเกมจากที่ไหน |

### Alpha — ระบบ gameplay ที่เหลือ

| ID | ระบบ |
|----|------|
| F2 | Networking Core |
| F3 | Input System |
| FT5 | AI/Bot System |
| FT8 | Dungeon Mode |
| FT9 | Town System |
| FT12 | World Map (10 Neutral Cities) |
| FT14 | Fragment & Meta-Game System |
| FT15 | Wager Mode |
| M11 | Reputation / Anti-Toxicity System |
| P2 | Item Shop UI |
| P3 | Hero Select UI |
| P5 | Camera System |

### Full Vision — Meta/Polish

| ID | ระบบ |
|----|------|
| F5 | Audio System |
| P4 | Menu & Lobby UI |
| M1 | Social System |
| M2 | Battle Pass |
| M3 | Customization |
| M4 | Notification System |
| M5 | Statistics & History |
| M6 | Account & Auth |
| M7 | AFK Detection |
| M8 | Surrender System |
| M9 | Tutorial System |
| M10 | Announcement System |
| M12 | Manga Influence Loop *(deferred — รอ proposal)* |

---

## Recommended GDD Design Order

| ลำดับ | ID | ระบบ | Tier | Dependencies |
|-------|----|------|------|-------------|
| 1 | F1 | Actor System | MVP | — |
| 2 | F4 | Data/Config System | MVP | — |
| 3 | C1 | Combat & Skills System | MVP | F1, F2, F3 |
| 4 | C2 | Hero System | MVP | F1, C1, F4 |
| 5 | C4 | Gold Economy | MVP | F1, F2, F4 |
| 6 | C5 | Level/XP System | MVP | F1, F2, F4 |
| 7 | FT1 | Item System | MVP | C2, C4, C1, F4 |
| 8 | FT2 | Map & Objectives | MVP | F1, C1, F2 |
| 9 | C3 | Movement & Navigation | V-Slice | F1, F3, F2 |
| 10 | FT3 | Creep/Minion System | V-Slice | F1, C1, FT2, C3 |
| 11 | FT4 | Fog of War | V-Slice | F1, FT2, F2 |
| 12 | FT6 | Matchmaking | V-Slice | F2, C2, F4 |
| 13 | FT7 | Game Mode Manager | V-Slice | FT6, FT2, F2 |
| 14 | P1 | HUD & In-Game UI | V-Slice | C1, C4, C5, FT4 |
| 15 | F2 | Networking Core | Alpha | — |
| 16 | F3 | Input System | Alpha | — |
| 17 | FT5 | AI/Bot System | Alpha | C2, C1, C3 |
| 18 | FT8 | Dungeon Mode | Alpha | FT7, FT5, C1, FT2 |
| 19 | FT9 | Town System | Alpha | F1, C3, F4, F2 |
| 20 | P2 | Item Shop UI | Alpha | FT1, C4 |
| 21 | P3 | Hero Select UI | Alpha | C2, FT6 |
| 22 | P5 | Camera System | Alpha | F1, F3, FT2 |
| 23 | F5 | Audio System | Full | — |
| 24 | P4 | Menu & Lobby UI | Full | FT6, M1, F4 |
| 25 | M6 | Account & Auth | Full | F2, F4 |
| 26 | M1 | Social System | Full | F2, M6, F4 |
| 27 | M5 | Statistics & History | Full | C1, C4, C5, M6 |
| 28 | M2 | Battle Pass | Full | F4, M6, M5 |
| 29 | M3 | Customization | Full | C2, F4, M6 |
| 30 | M4 | Notification System | Full | F2, M1, M6 |
| 31 | M7 | AFK Detection | Full | F1, F2, F3 |
| 32 | M8 | Surrender System | Full | F2, FT7 |
| 33 | M10 | Announcement System | Full | C1, FT2, F2 |
| 34 | M9 | Tutorial System | Full | C2, C1, FT1, FT2, P1 |
| **Pivot 2026-04-23 — author after retrofit ของ existing GDDs:** ||||
| 35 | FT11 | Faction System | V-Slice | F4, M6 |
| 36 | FT12 | World Map (10 Cities) | Alpha | FT11, FT9 |
| 37 | FT13 | Tournament System | V-Slice | FT6, FT7, FT11 |
| 38 | FT14 | Fragment & Meta-Game | Alpha | FT13, FT11, F4 |
| 39 | FT15 | Wager Mode | Alpha | FT14 |
| 40 | M11 | Reputation / Anti-Toxicity | Alpha | M1, M6, F4 |
| 41 | M12 | Manga Influence Loop | Full *(deferred)* | FT14 |

---

## Progress Tracker

| ID | ระบบ | สถานะ | GDD Path |
|----|------|-------|----------|
| F1 | Actor System | ✅ Done | design/gdd/actor-system.md |
| F2 | Networking Core | ✅ Done | design/gdd/networking-core.md |
| F3 | Input System | ✅ Done | design/gdd/input-system.md |
| F4 | Data/Config System | ✅ Done | design/gdd/data-config-system.md |
| F5 | Audio System | Not Started | — |
| C1 | Combat & Skills System | ✅ Done | design/gdd/combat-skills-system.md |
| C2 | Hero System | 🔄 Needs Revision (build lock per hero) | design/gdd/hero-system.md |
| C3 | Movement & Navigation | ✅ Done | design/gdd/movement-navigation-system.md |
| C4 | Gold Economy | 🔄 Needs Revision (last-hit cut/reduce) | design/gdd/gold-economy.md |
| C5 | Level/XP System | 🔄 Needs Revision (Max Level ใน 10 นาที) | design/gdd/level-xp-system.md |
| FT1 | Item System | 🔄 Needs Revision (build lock, simplify stats) | design/gdd/item-system.md |
| FT2 | Map & Objectives | ✅ Done | design/gdd/map-objectives-system.md |
| FT3 | Creep/Minion System | ✅ Done | design/gdd/creep-minion-system.md |
| FT4 | Fog of War | ✅ Done | design/gdd/fog-of-war-system.md |
| FT5 | AI/Bot System | ✅ Done | design/gdd/ai-bot-system.md |
| FT6 | Matchmaking | ✅ Done | design/gdd/matchmaking-system.md |
| FT7 | Game Mode Manager | 🔄 Needs Revision (5v5 standard, 25v25 deferred) | design/gdd/game-mode-manager.md |
| FT8 | Dungeon Mode | 🔄 Needs Revision (Personal/Side Quest 1-5 + bot) | design/gdd/dungeon-mode.md |
| FT9 | Town System | ⛔ Deprecated 2026-04-27 — superseded by FT12 (per FT12 R14, Q1=A hard deprecate) | design/gdd/town-system.md |
| P1 | HUD & In-Game UI | ✅ Done | design/gdd/hud-ingame-ui.md |
| P2 | Item Shop UI | ✅ Done | design/gdd/item-shop-ui.md |
| P3 | Hero Select UI | ✅ Done | design/gdd/hero-select-ui.md |
| P4 | Menu & Lobby UI | ✅ Done | design/gdd/menu-lobby-ui.md |
| P5 | Camera System | ✅ Done | design/gdd/camera-system.md |
| M1 | Social System | ✅ Done | design/gdd/social-system.md |
| M2 | Battle Pass | 🔄 Needs Revision (relationship กับ Fragment) | design/gdd/battle-pass.md |
| M3 | Customization | ✅ Done | design/gdd/customization-system.md |
| M4 | Notification System | ✅ Done | design/gdd/notification-system.md |
| M5 | Statistics & History | ✅ Done | design/gdd/statistics-history.md |
| M6 | Account & Auth | ✅ Done | design/gdd/account-auth-system.md |
| M7 | AFK Detection | ✅ Done | design/gdd/afk-detection.md |
| M8 | Surrender System | ✅ Done | design/gdd/surrender-system.md |
| M9 | Tutorial System | ✅ Done (stub) | design/gdd/tutorial-system.md |
| M10 | Announcement System | ✅ Done | design/gdd/announcement-system.md |
| ~~FT10~~ | ~~Territory War~~ | ⛔ Superseded 2026-04-23 (mark in GDD) | design/gdd/territory-war.md |
| ~~FT10a~~ | ~~Citizen System~~ | ⛔ Removed 2026-04-23 (never authored) | — |
| ~~FT10b~~ | ~~Mercenary System~~ | ⛔ Removed 2026-04-23 (never authored) | — |
| FT11 | Faction System | ✅ Approved (2026-04-27, lean review) | design/gdd/faction-system.md |
| FT12 | World Map (10 Neutral Cities) | ⛔ MAJOR REVISION NEEDED (third review 2026-04-30 + qa-lead coverage closure 2026-04-30) — **25 blockers / 29 recommended** across 8 root-cause clusters incl. 3 pillar-threats (P3 R8 bell, P5 repeat-visit, P3-vs-P4 OQ-10), methodology failure on capacity math, R12 PlayFab API mismatch. qa-lead added 6 net-new blockers (3 missing AC sets, production/qa/ absent, PlayFab rate limit, TR-048 untestable). narrative-director still pending. Revision must be top-down walkthrough-driven | design/gdd/world-map-system.md |
| FT13 | Tournament System | 🆕 Not Started (pivot 2026-04-23) | — |
| FT14 | Fragment & Meta-Game System | 🆕 Not Started (pivot 2026-04-23) | — |
| FT15 | Wager Mode | 🆕 Not Started (pivot 2026-04-23) | — |
| M11 | Reputation / Anti-Toxicity System | 🆕 Not Started (pivot 2026-04-23) | — |
| M12 | Manga Influence Loop | 🔮 Deferred (รอ proposal) | — |
