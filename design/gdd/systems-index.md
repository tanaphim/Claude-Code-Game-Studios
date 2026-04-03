---
status: reverse-documented
source: C:\GitHub\delta-unity
date: 2026-04-02
---

# Delta — Systems Index

## Overview

เอกสารนี้แจกแจงระบบทั้งหมดของเกม Delta พร้อม dependencies, ลำดับความสำคัญ,
และลำดับการเขียน GDD เป็นแผนที่หลักสำหรับการออกแบบและพัฒนา

**จำนวนระบบทั้งหมด**: 37
**สถานะ**: Reverse-documented จาก codebase ที่มีอยู่ (1,139 C# scripts)

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
| FT2 | Map & Objectives | 3 เลน, หอคอย, ค่ายทหาร, บอส/Mini-Boss, จุดฮีล | จาก concept |
| FT3 | Creep/Minion System | สร้างคลื่นมินเนี่ยน, ป่า camps, scaling ตามเวลา, Super Creep | จาก concept |
| FT4 | Fog of War | ระบบ AOI visibility, พุ่มไม้, vision revealers | จาก concept |
| FT5 | AI/Bot System | บอทควบคุม CPU — pathfinding, targeting, ใช้สกิล (FuzzySkillAI) | จากโค้ด |
| FT6 | Matchmaking | จับคู่ Ranked/Casual, lobby, team assignment, hero draft | จาก concept |
| FT7 | Game Mode Manager | จัดการโหมดเกมทั้งหมด (Ranked, Casual, Arcade, Training, Dungeon, Town) | จาก concept |
| FT8 | Dungeon Mode | PvE เดี่ยว — ด่าน/เวฟ, ระบบมอนสเตอร์, progression | จาก concept |
| FT9 | Town System | Hub โซเชียล — NPC, ภารกิจ, customization, พบเพื่อน | จาก concept |
| FT10 | Territory War | Meta-game layer — World Map 1M+ เมือง, ซื้อเมืองด้วย Premium Currency, ประกาศสงคราม, สู้แบบ MOBA (1v1/3v3/5v5/25v25), ผู้แพ้กลายเป็นเมืองขึ้น | ใหม่ |
| FT10a | Citizen System | Sub-system ของ FT10 — ผู้เล่นสมัครเป็นพลเมืองของเมือง, เจ้าเมืองเรียกพลเมืองออกรบได้ | ใหม่ |
| FT10b | Mercenary System | Sub-system ของ FT10 — ตลาดทหารรับจ้าง, ผู้เล่นลงทะเบียนรับจ้าง, เจ้าเมืองจ้างมาเสริมทีมสงคราม | ใหม่ |

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
└── [FT9] Town System                ← F1, C3, F4, F2

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
└── [M10] Announcement System        ← C1, FT2, F2
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
| FT7 | Game Mode Manager | จัดการ 6 โหมด |
| P1 | HUD & In-Game UI | ผู้เล่นอ่านข้อมูลในเกมจากที่ไหน |

### Alpha — ระบบ gameplay ที่เหลือ

| ID | ระบบ |
|----|------|
| F2 | Networking Core |
| F3 | Input System |
| FT5 | AI/Bot System |
| FT8 | Dungeon Mode |
| FT9 | Town System |
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
| C2 | Hero System | ✅ Done | design/gdd/hero-system.md |
| C3 | Movement & Navigation | ✅ Done | design/gdd/movement-navigation-system.md |
| C4 | Gold Economy | ✅ Done | design/gdd/gold-economy.md |
| C5 | Level/XP System | ✅ Done | design/gdd/level-xp-system.md |
| FT1 | Item System | ✅ Done | design/gdd/item-system.md |
| FT2 | Map & Objectives | ✅ Done | design/gdd/map-objectives-system.md |
| FT3 | Creep/Minion System | ✅ Done | design/gdd/creep-minion-system.md |
| FT4 | Fog of War | ✅ Done | design/gdd/fog-of-war-system.md |
| FT5 | AI/Bot System | ✅ Done | design/gdd/ai-bot-system.md |
| FT6 | Matchmaking | ✅ Done | design/gdd/matchmaking-system.md |
| FT7 | Game Mode Manager | ✅ Done | design/gdd/game-mode-manager.md |
| FT8 | Dungeon Mode | ✅ Done | design/gdd/dungeon-mode.md |
| FT9 | Town System | ✅ Done | design/gdd/town-system.md |
| P1 | HUD & In-Game UI | ✅ Done | design/gdd/hud-ingame-ui.md |
| P2 | Item Shop UI | ✅ Done | design/gdd/item-shop-ui.md |
| P3 | Hero Select UI | ✅ Done | design/gdd/hero-select-ui.md |
| P4 | Menu & Lobby UI | ✅ Done | design/gdd/menu-lobby-ui.md |
| P5 | Camera System | ✅ Done | design/gdd/camera-system.md |
| M1 | Social System | ✅ Done | design/gdd/social-system.md |
| M2 | Battle Pass | ✅ Done | design/gdd/battle-pass.md |
| M3 | Customization | ✅ Done | design/gdd/customization-system.md |
| M4 | Notification System | ✅ Done | design/gdd/notification-system.md |
| M5 | Statistics & History | ✅ Done | design/gdd/statistics-history.md |
| M6 | Account & Auth | ✅ Done | design/gdd/account-auth-system.md |
| M7 | AFK Detection | ✅ Done | design/gdd/afk-detection.md |
| M8 | Surrender System | ✅ Done | design/gdd/surrender-system.md |
| M9 | Tutorial System | ✅ Done (stub) | design/gdd/tutorial-system.md |
| M10 | Announcement System | ✅ Done | design/gdd/announcement-system.md |
| FT10 | Territory War | 🔍 In Review | design/gdd/territory-war.md |
| FT10a | Citizen System | Not Started | — |
| FT10b | Mercenary System | Not Started | — |
