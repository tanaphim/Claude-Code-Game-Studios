---
status: reverse-documented
source: C:\GitHub\delta-unity
date: 2026-04-02
---

# Delta — Game Concept

## Overview

Delta คือเกม MOBA แข่งขันจริงจังในมุมมอง Top-Down ที่ผู้เล่นเลือกฮีโร่จาก
roster 25+ ตัวใน 6 บทบาท เพื่อต่อสู้แบบทีม 3v3 และ 5v5 บนแผนที่ 3 เลน
พร้อมระบบป่า บอส และหอคอย โดยมีโหมด Dungeon (PvE เดี่ยว) และ Town Hub
(พื้นที่โซเชียล + ภารกิจ) เสริมประสบการณ์นอกสนามแข่ง

## Player Fantasy

ผู้เล่นรู้สึกเหมือน **นักยุทธศาสตร์ในสนามรบ** — อ่านเกม ประสานงานทีม
เลือกฮีโร่และไอเทมที่เหมาะกับสถานการณ์ แล้วเอาชนะด้วยฝีมือและกลยุทธ์
ชัยชนะมาจากการตัดสินใจที่ดีกว่า ไม่ใช่แค่กดปุ่มเร็วกว่า

## Design Pillars

1. **แข่งขันจริงจัง (Competitive Core)**
   — Ranked เป็นแก่นหลัก ทุกระบบออกแบบเพื่อรองรับการแข่งขัน
   ที่มีความหมาย มี skill ceiling สูง แต่เรียนรู้ได้

2. **ความลึกเชิงกลยุทธ์ (Strategic Depth)**
   — ไอเทม 7 ระดับพร้อมคราฟ, 6 บทบาทฮีโร่, ระบบวิสัยทัศน์ (Fog of War + พุ่มไม้),
   เศรษฐกิจทอง (ฆ่า/ฟาร์ม/passive income) สร้างการตัดสินใจหลายชั้น

3. **ทีมเวิร์ค (Team Synergy)**
   — ชนะด้วยการประสานงาน ไม่ใช่ solo carry
   ระบบบทบาท (Tank/Support/Carry/Fighter/Assassin/Mage) บังคับให้ทีมสมดุล

4. **มีอะไรทำนอกสนามแข่ง (Living World)**
   — Town Hub มีภารกิจ NPC และพื้นที่โซเชียล, Dungeon Mode
   สำหรับเล่นคนเดียว, Battle Pass และระบบ customization
   ทำให้ผู้เล่นมีเหตุผลกลับมาเล่นนอกเหนือจาก ranked

## Core Mechanics

### การต่อสู้ (Combat)

- ฮีโร่ 25+ ตัว แต่ละตัวมี Passive + 4 สกิล (Q/W/E/R)
- สกิลมีหลายรูปแบบ: skillshot, AoE, targeted, buff/debuff
- Status effects: Stun, Silence, Bleeding ฯลฯ
- ระบบเลเวล + skill points ระหว่างแมตช์

### แผนที่และ Objective

- 3 เลน (Top/Mid/Bot) + ป่า 2 ฝั่ง (Blue/Red)
- หอคอยหลายชั้นต้องทำลายตามลำดับ
- ค่ายทหาร (Barracks) — ทำลายแล้วเกิด Super Creep
- บอส + Mini-Boss เป็น objective ทีม
- จุดฮีล (Healing Points) ที่ฐาน

### ระบบไอเทม

- 7 ระดับ: Starter > Potion > Boots > Basic > Epic > Legendary > Mythic
- คราฟ/รวมไอเทมจากชิ้นย่อย
- ช่อง 6 ช่อง — ต้องเลือกให้เหมาะกับสถานการณ์

### Fog of War & Vision

- ระบบ Area of Interest (AOI) — เห็นเฉพาะบริเวณทีม
- พุ่มไม้ซ่อนตัว (Bush) บล็อกการมองเห็นศัตรู
- Minimap + ระบบ Ping สื่อสารกับทีม

### โหมดเกม

| โหมด | ประเภท | รายละเอียด |
|------|--------|-----------|
| Ranked | PvP แข่งขัน | โหมดหลัก, มีอันดับ |
| Casual | PvP สบายๆ | ฝึกซ้อมกับคนจริง |
| Dungeon | PvE เดี่ยว | ด่าน/เวฟ สำหรับเล่นคนเดียว |
| Arcade | PvP กำหนดเอง | สร้างห้องเอง ตั้งกฎเอง |
| Training | ฝึกซ้อม | เล่นกับบอท |
| Town | โซเชียล + ภารกิจ | Hub พบเพื่อน ทำภารกิจ NPC |

### ระบบโซเชียลและ Metagame

- ระบบเพื่อน + แคลน
- แชทข้อความ (Photon Chat)
- แจ้งเตือนแบบ real-time (SignalR)
- Battle Pass ตามฤดูกาล
- Customization ฮีโร่ (สกิน/ชุด)

## Target Audience

- **หลัก**: ผู้เล่น MOBA ที่ต้องการทางเลือกใหม่จาก LoL/DotA/MLBB
- **รอง**: ผู้เล่นที่ชอบ competitive team games แต่อยากได้ PvE content ด้วย

## Technology Stack

- **Engine**: Unity 2022.3.62f1 (URP)
- **Networking**: Photon Fusion (real-time multiplayer)
- **Backend**: Azure Functions + PlayFab
- **Real-time**: Azure SignalR Service
- **Platform**: PC (Steam)

## Scope

- **สถานะปัจจุบัน**: กำลังพัฒนา (Production)
- **ขนาดโค้ด**: 1,139 C# scripts
- **ระบบที่เสร็จแล้ว**: Combat, Items, Fog of War, Networking, UI,
  Matchmaking, Social, Town, Dungeon, Battle Pass
- **ระบบที่ต้องพัฒนาต่อ**: [ระบุเพิ่มเติมภายหลัง]
