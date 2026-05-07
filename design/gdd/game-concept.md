---
status: revised
source: C:\GitHub\delta-unity
date: 2026-04-27
revised_from: 2026-04-02 reverse-documented version
revision_basis: design/decisions/meeting-2026-04-23-tournament-pivot.md
---

# Delta — Game Concept

## Overview

Delta คือเกม MOBA แข่งขันจริงจังในมุมมอง Top-Down — แต่ละแมตช์ 5v5 จบใน
**15 นาที** บนแผนที่ 3 เลนพร้อมระบบป่า บอส หอคอย รองรับ 1v1/2v2/3v3
ตาม request. ผู้เล่นเลือกฮีโร่จาก roster 25+ ตัวใน 6 บทบาท. นอกสนามแข่ง
ผู้เล่นเลือก **ฝ่าย** จาก 5-6 ฝ่าย และร่วมสะสม **Fragment** ผ่าน
**Tournament** เพื่อกำหนดทิศทางของ Universe โดยมี **10 เมืองกลาง
(Neutral Cities)** เป็นพื้นที่ socialize และจุดดรอป Fragment. โหมด
**Personal Dungeon** (1-5 คน + บอท, MOBA combat) เสริม PvE experience.

## Player Fantasy

ผู้เล่นรู้สึกเหมือน **นักรบของฝ่ายตนที่ทุกชัยชนะมีความหมายต่อ Universe**
— ในแมตช์ คือนักยุทธศาสตร์ที่อ่านเกม ประสานงานทีม เลือกฮีโร่ที่เหมาะกับ
สถานการณ์. นอกแมตช์ คือสมาชิกฝ่ายที่ร่วมแย่งชิง Fragment เพื่อสะสม
~100 ล้าน รวมเป็นแหวนกำหนด "กฎของ Universe" ผู้เล่นที่เก่งพออาจกลายเป็น
ตัวละครในมังงะของเกม. ชัยชนะมาจากการตัดสินใจที่ดีกว่า ไม่ใช่แค่กดปุ่มเร็ว
กว่า — **และจากการเล่นอย่างมีน้ำใจ**.

## Design Pillars

1. **แข่งขันจริงจัง (Competitive Core)**
   — Ranked Tournament เป็นแก่นหลัก ทุกระบบออกแบบเพื่อรองรับการแข่งขัน
   ที่มีความหมาย มี skill ceiling สูง แต่เรียนรู้ได้

2. **ความลึกเชิงกลยุทธ์ (Strategic Depth)**
   — 6 บทบาทฮีโร่, ระบบวิสัยทัศน์ (Fog of War + พุ่มไม้), เศรษฐกิจทอง
   สร้างการตัดสินใจหลายชั้น *(หมายเหตุ: ไอเทมและ build ถูก simplify
   เพื่อลด overwhelm — ความลึกย้ายไปอยู่ที่ team play, hero matchup,
   meta-game)*

3. **ทีมเวิร์ค (Team Synergy)**
   — ชนะด้วยการประสานงาน ไม่ใช่ solo carry
   ระบบบทบาท (Tank/Support/Carry/Fighter/Assassin/Mage) บังคับให้ทีมสมดุล

4. **Universe-Spanning Meta-Game** *(แก้จาก "Living World")*
   — 5-6 ฝ่ายแย่งชิง Fragment จาก Tournament ทุกวัน, สะสมเพื่อกำหนดกฎ
   ของ Universe ทุก ~3 ปี. **10 เมืองกลาง** เป็นพื้นที่ socialize
   ระหว่างฝ่าย. ผลในเกม influence story ในมังงะ — ทำให้ทุกแมตช์มี
   ความหมายเกินแค่ MMR

5. **Anti-Toxicity Core** *(เสาใหม่)*
   — Toxicity ต้องน้อยกว่าทุก MOBA ในตลาด — เป็น **Key Feature** ของเกม.
   **Reputation System** หักคะแนนเมื่อพิมพ์คำหยาบ → ล็อค Ranked.
   Filter คำหยาบ + การลงโทษระบบทำให้ player base healthy
   เป็น marketing differentiator

## Core Mechanics

### การต่อสู้ (Combat)

- ฮีโร่ 25+ ตัว แต่ละตัวมี Passive + 4 สกิล (Q/W/E/R)
- สกิลมีหลายรูปแบบ: skillshot, AoE, targeted, buff/debuff
- Status effects: Stun, Silence, Bleeding ฯลฯ
- ระบบเลเวล + skill points ระหว่างแมตช์
- **เกมจบเร็ว**: 5v5 จบใน 15 นาที, ถึง Max Level ใน 10 นาที
  *(XP curve / Level cap solution — [Open Question], รอ 2026-05-07)*

### แผนที่และ Objective

- 3 เลน (Top/Mid/Bot) + ป่า 2 ฝั่ง (Blue/Red)
- หอคอยหลายชั้นต้องทำลายตามลำดับ
- ค่ายทหาร (Barracks) — ทำลายแล้วเกิด Super Creep
- บอส + Mini-Boss เป็น objective ทีม
- จุดฮีล (Healing Points) ที่ฐาน

### ระบบไอเทม

- 7 ระดับ: Starter > Potion > Boots > Basic > Epic > Legendary > Mythic
- ช่อง 6 ช่อง
- **Build ล็อคต่อฮีโร่** — ลด Overwhelm; ผู้เล่นเลือก path build ที่
  ออกแบบไว้
- **Item Stats simplified** — คงเฉพาะ Consumable + Skill Item ที่มี
  active effect (รายละเอียดใน item-system GDD revise)
- **Last Hit**: [Open Question — รอ proposal 2026-05-07]

### Fog of War & Vision

- ระบบ Area of Interest (AOI) — เห็นเฉพาะบริเวณทีม
- พุ่มไม้ซ่อนตัว (Bush) บล็อกการมองเห็นศัตรู
- Minimap + ระบบ Ping สื่อสารกับทีม

### โหมดเกม

| โหมด | ประเภท | รายละเอียด |
|------|--------|-----------|
| **Tournament — Ranked** | PvP แข่งขัน | โหมดหลัก, Top 10% ได้ Fragment |
| **Tournament — Casual** | PvP สบายๆ | เล่นสนุก ไม่มี Stake |
| **Tournament — Custom** | PvP กำหนดเอง | 1v1 / 2v2 / 3v3 / 5v5 ตาม request |
| **Universe Map** | Open World Hub | 10 เมืองกลาง — socialize, Fragment drop, faction movement |
| **Personal Dungeon** | PvE 1-5 คน | Side Quest, MOBA combat, มี bot ถ้าเล่นเดี่ยว |
| **Training** | ฝึกซ้อม | เล่นกับบอท |

*หมายเหตุ:*
- *Town System ถูกแทนด้วย 10 Cities (deprecated)*
- *Arcade รวมเข้า Tournament Custom*
- *25v25 mode: deferred per 2026-04-23 decision*

### Meta-Game Loop (Faction → Fragment → Universe)

```
[เลือกฝ่าย 1 ใน 5-6]  →  [เล่น Tournament Ranked]  →  [Top 10% ได้ Fragment]
        ↑                                                        │
  [เปลี่ยนฝ่าย                                                   ▼
   มีเงื่อนไข]                                       [ฝ่ายสะสม Fragment ร่วม]
                                                                 │
                          [Fragment Daily Drop ตาม 10 Cities] ←──┤
                                                                 │
                          [Wager: เดิมพัน Fragment ชิงของคนอื่น] ←┤
                                                                 ▼
                                                  [ฝ่ายแรกถึง ~100M Fragment]
                                                                 │
                                                                 ▼
                                                  [รวมเป็นแหวน → กำหนดกฎ Universe]
                                                                 │
                                                                 ▼
                                          [Reset Cycle ~3 ปี → influence มังงะ]
```

- Universe-level cycle: ~3 ปีต่อรอบ ([Open Question])
- ผู้เล่น Top จะกลายเป็นตัวละครในมังงะของเกม

### ระบบโซเชียลและ Metagame

- **Faction Membership** — สังกัด 1 ใน 5-6 ฝ่าย, เปลี่ยนได้มีเงื่อนไข
- **Reputation System** — คะแนน behavior, ล็อค Ranked ถ้าต่ำ
- ระบบเพื่อน + แคลน
- แชทข้อความ (Photon Chat) — ผ่าน **Reputation Filter** (anti-toxicity)
- แจ้งเตือนแบบ real-time (SignalR)
- Battle Pass ตามฤดูกาล — [Open Question: ความสัมพันธ์กับ Fragment system]
- Customization ฮีโร่ (สกิน/ชุด)

## Target Audience

- **หลัก**: ผู้เล่น MOBA ที่ต้องการทางเลือกใหม่จาก LoL/DotA/MLBB ที่
  **toxicity ต่ำกว่า** + เกม **จบเร็ว (15 นาที)**
- **รอง**: ผู้เล่นที่ชอบ competitive team games + meta-game ระดับ
  Universe (faction warfare + manga influence)
- **Positioning**: cleaner-than-LoL, faster-than-DotA, deeper-meta-than-MLBB

## Technology Stack

- **Engine**: Unity 2022.3.62f1 (URP)
- **Networking**: Photon Fusion (real-time multiplayer)
- **Backend**: Azure Functions + PlayFab
- **Real-time**: Azure SignalR Service
- **Platform**: PC (Steam)

## Scope

- **สถานะปัจจุบัน**: Production + Tournament Pivot (decision 2026-04-23)
- **ขนาดโค้ด**: 1,139 C# scripts
- **ระบบที่เสร็จแล้ว**: Combat, Items, Fog of War, Networking, UI,
  Matchmaking, Social, Town, Dungeon, Battle Pass *(กำลัง revise)*
- **ระบบใหม่ที่ต้องเขียน GDD**: Faction, World Map (10 Cities), Tournament,
  Fragment, Wager, Reputation/Anti-Toxicity, Manga Influence Loop
- **ระบบเดิมที่ต้อง revise**: Item, Gold Economy, Level/XP,
  Game Mode Manager, Dungeon, Hero, Battle Pass
- **Deprecated**: Territory War (FT10), Citizen (FT10a), Mercenary (FT10b),
  Town System (replaced by 10 Cities)
- **Deferred**: 25v25 mode

## Open Questions

- Theme เฉพาะตัวของแต่ละเมืองทั้ง 10
- จำนวนฝ่ายที่แน่นอน (5 หรือ 6)
- เงื่อนไขการเปลี่ยนฝ่าย
- Item Drop ของ Personal Dungeon ใช้ทำอะไร
- Reset Cycle ของ Universe (3 ปี?)
- Last Hit: ตัด vs ลด (รอ 2026-05-07)
- Solution ให้เกมจบ 15 นาที + Max Level 10 นาที (รอ 2026-05-07)
- Solution Anti-Toxicity เชิงระบบเพิ่มเติม (รอ 2026-05-07)
- Manga Influence Loop: criteria ผู้เล่นเก่งพอที่จะเป็นตัวละคร
- Battle Pass relationship กับ Fragment system
- 25v25 mode: deferred — kept on roadmap หรือ deprecate?
- Town System: hard deprecate vs soft keep แยกจาก 10 Cities?
