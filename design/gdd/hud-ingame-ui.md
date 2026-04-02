# HUD & In-Game UI — Game Design Document

**System ID**: P1
**Version**: 0.1.0
**Status**: Draft — Reverse-documented from source
**Last Updated**: 2026-04-02
**Dependencies**: Combat & Skills System (C1), Gold Economy (C3), Level/XP System (C5), Fog of War (FT4)

---

## 1. Overview

HUD ของ Delta ประกอบด้วย Character Console (ล่างกลาง), Minimap (ล่างขวา), Scoreboard (Tab), ระบบ Announcement + Kill Feed, Damage Numbers แบบ Floating, ระบบ Ping/Chat, และ Death Recap UI ข้อมูลทั้งหมดดึงจาก Network State โดยตรง และ Update แบบ Real-time

---

## 2. Player Fantasy

HUD ควรให้ข้อมูลครบโดยไม่บดบังมุมมองเกม ผู้เล่นอ่าน HP/Cooldown/ทอง ได้ทันทีโดยไม่ต้องคิด Damage Number ที่ลอยขึ้นต้องรู้สึกสนุกและสะใจ Minimap ต้องอ่านง่ายพอที่จะ Call กลยุทธ์ได้ในแว่บตาเดียว

---

## 3. Detailed Rules

### 3.1 Character Console (Main Panel — ล่างกลาง)

**`UICharacterConsoleView`** รวมทุกอย่างเกี่ยวกับ Hero ตัวเอง:

| Element | Component | คำอธิบาย |
|---------|-----------|---------|
| HP/MP/Shield Bars | `UILife` | แสดงแยก Physical/Magic/Universal Shield |
| ชื่อ + Avatar | Portrait Image | Hero ปัจจุบัน |
| Level + XP Bar | `UILevel` | Level ตัวเลข + Fill Bar |
| Skill Q/W/E/R | `UISkill[4]` | ไอคอน, Cooldown Radial, Cost, Level |
| Passive Skill | `UIPassive` | ไอคอน Passive |
| Ultimate Cooldown | WarpCooldown | ตัวนับพิเศษ |
| Inventory 1–6 + Special | `UIInGameShopItem[7]` | ไอคอนไอเทม + Cooldown |
| ทอง (Gold) | `m_Money` | TextMeshPro real-time |
| HP Regen / Resource Regen | `m_HPRegen`, `m_ResourceRegen` | ตัวเลข |
| Stat Panel | `UIStat` | PDMG, MDMG, PDEF, MDEF, ASPD, CD, CriRate, MSPD |
| Status Effects | `StatusEffectUI[]` | ไอคอน Buff/Debuff + Duration Fill + Stack |
| Shop Button | `m_ShopButton` | เปิดหน้าร้านค้า |

---

### 3.2 HP/MP Bar (`UILife`)

**แสดง**:
- HP Bar + ตัวเลข (CurrentHP / MaxHP)
- Resource Bar (Mana/Energy/Fury/Ferocity/Health ตามประเภท Hero)
- Shield Bars แยก 3 ชั้น: Universal / Physical / Magical

**สี HP Bar**:

| ประเภทหน่วย | สี |
|------------|---|
| ตัวเอง | เขียว (SelfColor) |
| พันธมิตร | ฟ้า (AvatarAllyColor) |
| ศัตรู | แดง (AvatarEnemyColor) |

**CC Bar**: แสดงเมื่อถูก Stun — มี Fill แสดงเวลาที่เหลือ

---

### 3.3 Skill Icons (`UISkill`)

แต่ละ Skill แสดง:
- **ไอคอน** ของ Skill
- **Key Binding** (Q/W/E/R)
- **Cooldown Radial** (circular fill = เวลาที่เหลือ)
- **Sub-Cooldown** (Secondary Phase เช่น Charge/Stack)
- **Cost** (ค่า Mana/Resource)
- **Skill Level** (Rank)
- **Hover Panel**: ชื่อ, คำอธิบาย, Cooldown, Cost, Level

---

### 3.4 Floating HP Bars เหนือหน่วย (`ActorUI`)

- **Hero**: ลอยสูง 3.5 หน่วย เหนือหัว
- **Tower**: ลอยสูง 5 หน่วย
- แสดง: HP Bar + Level + ชื่อ + Ultimate Indicator
- ซ่อนเมื่อ: ตาย, ถูก FOW ซ่อน, ตั้งค่า Hide

---

### 3.5 Damage Numbers (`FloatingDamageManager` + `UINumber`)

| ประเภท | สี | Animation |
|--------|---|-----------|
| Physical | ส้ม `#EE8C4C` | ลอยขึ้น + เฟดออก |
| Magical | ม่วง `#C860FF` | ลอยขึ้น + เฟดออก |
| Armor Piercing / True | เหลือง `#FFE770` | ลอยขึ้น + เฟดออก |
| Critical | แดง `#FF3B3B` | **ใหญ่กว่า** + ลอยขึ้น |
| HP Regen | เขียว `#88FF94` | ลอยขึ้น |
| Mana Regen | ฟ้า `#6CD1F1` | ลอยขึ้น |
| Gold ได้รับ | ทอง `#F9CB66` | SlideUp |

- **Lifespan**: 0.7 วินาที (Object Pool)
- **ทิศทาง**: ขวา / ซ้าย / ตรง ขึ้นอยู่กับตำแหน่ง

---

### 3.6 Minimap (`UIMinimapView`)

**แสดงบน Minimap**:
- Hero (ตัวเอง + พันธมิตร + ศัตรูที่มองเห็น)
- Lane Minion
- Tower (แยกสี 2 ทีม)
- Ping Markers
- Jungle Camp Icons
- Fog of War Layer

**Interaction**:
- **Left Click + Drag**: ย้ายกล้องไปพื้นที่นั้น
- **Right Click**: สั่งให้ Hero วิ่งไปจุดนั้น (เหมือนคลิกแผนที่)

---

### 3.7 Scoreboard (`UITeamsStatisticView`) — Tab Key

**ต่อผู้เล่น**:
- Avatar Portrait + ชื่อ
- K / D / A
- ทอง
- Level ปัจจุบัน
- ไอเทม 6 ช่อง
- Creep Score (CS)
- Respawn Status

**ต่อทีม**:
- Total Kill
- Tower ที่ทำลาย
- Boss/MiniBoss ที่ฆ่า
- Game Timer

**สี**: Team1 = ฟ้า, Team2 = แดง

---

### 3.8 Kill Feed & Announcements

**Kill Announcement** (`UIAnnouncementObject`):
- แสดง: ภาพ Killer + ภาพ Victim + ข้อความ
- Timeline: Fade In 1s → แสดง 2s → Fade Out 1s

**Kill Feed** (`UIKillFeedObject`):
- ขนาดเล็กกว่า Announcement
- เพิ่มเข้าคิวแสดงต่อเนื่อง

**Text Announcements** (`UIAnnouncement`):
- Queue-based (ทีละ 1 ข้อความ)
- แต่ละข้อความแสดง **1.1 วินาที**
- ใช้สำหรับ: First Blood, Kill Streak, Tower Destroyed, Boss Slain ฯลฯ

---

### 3.9 Ping System (`UIQuickMessageView`)

**ประเภท Ping**:

| Index | ประเภท | ความหมาย |
|-------|--------|---------|
| 0 | Retreat | ถอยกลับ |
| 1 | Going | กำลังไป / โจมตี |
| 2 | Help | ขอความช่วยเหลือ |
| 3 | Missing | ศัตรูหาย |
| 4 | Ping | Ping ทั่วไป |

**วิธีใช้**: Alt + Right-Click → Radial Menu → เลือกประเภท
**แสดงผล**: ไอคอน Ping ปรากฏบน Minimap และในโลก 3D, หายหลังระยะเวลาหนึ่ง
**Network**: ส่งผ่าน `RPC_SpawnPing(team, pingIndex, position)`

---

### 3.10 Chat System (`UIChatView` + `ChatInGameManager`)

- **Channels**: Team, Global, Private (หลาย Channel)
- **Auto-close**: ปิดอัตโนมัติหลังไม่ใช้งาน 5–7 วินาที
- **Message Types**: Text, Item Link, Sticker
- **Backend**: Photon Chat

---

### 3.11 Death Recap (`UIDetailOfDeathView`)

แสดงทันทีที่ Hero ตาย:
- ภาพและชื่อ Killer
- สัดส่วนความเสียหาย: Physical / Magical / True (Bar Chart)
- รายชื่อทุกคนที่ทำ Damage พร้อม % Contribution
- **ปุ่ม Buyback**: กด Respawn ทันทีโดยเสียทอง (10 ทอง ⚠️ TODO ต้องตรวจสอบสูตรจริง)

---

### 3.12 Stat Panel (`UIStat`)

| Stat | ย่อ | หมายเหตุ |
|------|-----|---------|
| Physical Damage | PDMG | |
| Magical Damage | MDMG | |
| Physical Defense | PDEF | Armor |
| Magical Defense | MDEF | MagicResist |
| Attack Speed | ASPD | |
| Cooldown Reduction | CD | แสดงเป็น % cap 45% |
| Critical Rate | CriRate | แสดงเป็น % |
| Movement Speed | MSPD | แสดงเป็น % Multiplier |

- สี Stat ที่ถูกแก้ไขโดยไอเทม: ต่างจาก Base

---

### 3.13 Status Effect Icons (`StatusEffectUI`)

แต่ละ Buff/Debuff:
- ไอคอน Effect
- Fill Amount = Duration ที่เหลือ
- Stack Count (ตัวเลข)
- Duration ตัวเลข "X.XX S"
- Hover → Tooltip แสดงชื่อ, คำอธิบาย, เวลาที่เหลือ

---

## 4. Formulas

### Skill Cooldown Display
```
CooldownFill = RemainingCooldown / MaxCooldown  (0.0 → 1.0)
แสดงเป็น Radial Fill บน Skill Icon
```

### XP Bar Fill
```
ExpFill = CurrentExp / ExpToNextLevel  (0.0 → 1.0)
Reset เป็น 0 เมื่อ Level Up
```

### Damage Number Direction
```
ถ้า target อยู่ด้านซ้ายของ Screen Center → GoLeft()
ถ้า target อยู่ด้านขวา → GoRight()
```

---

## 5. Edge Cases

| สถานการณ์ | พฤติกรรม |
|-----------|---------|
| Skill ยังไม่ Rank Up (Level ต่ำ) | ไอคอน Skill แสดงแต่ Cooldown Display ถูก Disable |
| Shield หมด (= 0) | Shield Bar ซ่อน / Fill = 0 |
| ตาย → HP = 0 | HP Bar ซ่อน; Death Recap ปรากฏ |
| Invisible (ฝ่ายเดียวกัน) | HP Bar เห็นได้ตามปกติ |
| Invisible (ศัตรู) | HP Bar ซ่อน พร้อมกับ Model |
| Ping ถูกวางซ้ำจุดเดิม | ทับกัน (ไม่มีการ Merge) |
| Chat Input เปิดอยู่ | Movement Input ถูกบล็อก |
| Buyback ทองไม่พอ | ปุ่ม Buyback Disabled |

---

## 6. Dependencies

| ระบบ | ความสัมพันธ์ |
|------|------------|
| **Combat & Skills System (C1)** | Cooldown ของ Skill, Damage Type → Damage Number สี |
| **Gold Economy (C3)** | Gold Display, Buyback cost |
| **Level/XP System (C5)** | Level + XP Bar |
| **Item System (FT1)** | Inventory Slot Display, Item Cooldown |
| **Fog of War (FT4)** | ซ่อน HP Bar เมื่อหน่วยไม่ Visible |
| **Actor System (F1)** | HP/MP/Shield ค่ามาจาก NetworkTrait |
| **Networking Core (F2)** | Ping RPC, Chat (Photon Chat), State Sync |
| **Map & Objectives (FT2)** | Tower Status บน Minimap, Objective Announcement |

---

## 7. Tuning Knobs

| ค่า | ที่อยู่ | ค่าปัจจุบัน | ผลกระทบ |
|-----|--------|------------|---------|
| Announcement Fade In | Hardcode | 1.0 วินาที | ความเร็ว Fade |
| Announcement Display | Hardcode | 2.0 วินาที | เวลาแสดง |
| Announcement Fade Out | Hardcode | 1.0 วินาที | ความเร็ว Fade |
| Text Message Duration | Hardcode | 1.1 วินาที | ข้อความ Announcement |
| Floating Damage Lifespan | Hardcode | 0.7 วินาที | เวลาเลข Damage ลอย |
| Chat Auto-close | Hardcode | 5–7 วินาที | ปิดอัตโนมัติ |
| HP Bar Y Offset (Hero) | Hardcode | 3.5 หน่วย | ความสูง HP Bar |
| HP Bar Y Offset (Tower) | Hardcode | 5.0 หน่วย | — |
| CD Reduction Cap (Display) | UIStat | 45% | แสดงผลสูงสุด |

---

## 8. Acceptance Criteria

- [ ] HP/MP/Shield Bar อัปเดต Real-time ตาม NetworkTrait
- [ ] Skill Q/W/E/R แสดง Cooldown Radial หมุนถูกต้องเมื่อใช้
- [ ] Damage Number ลอยขึ้นพร้อม สี ถูกต้องตาม DamageType
- [ ] Critical Hit → ตัวเลข Damage ใหญ่กว่าปกติ
- [ ] Minimap: คลิกขวา → Hero วิ่งไป; คลิกซ้าย → กล้องย้าย
- [ ] Ping: Alt+Click → Radial Menu → เลือกได้ → แสดงบน Minimap ทุก Client
- [ ] Chat Input เปิด → Movement Input ถูกบล็อก; ปิด → กลับปกติ
- [ ] Death Recap แสดง Killer + Damage Breakdown ทันที
- [ ] Buyback Button Active เมื่อทองพอ; Disabled เมื่อทองไม่พอ
- [ ] Scoreboard (Tab): แสดง KDA + ทอง + ไอเทม + Level ทุกผู้เล่น
- [ ] Status Effect Icon แสดง Stack + Duration Fill + Tooltip ถูกต้อง
- [ ] HP Bar เหนือหน่วยซ่อนเมื่อหน่วยนั้นอยู่ใน Fog

---

## Known Issues / TODO

- ⚠️ **Camera Lock Button**: พบ CameraManager.m_IsFollow แต่ยังไม่ชัดเจนว่า Hotkey/Button ใดที่ Toggle
- ⚠️ **Buyback Cost Formula**: แสดง 10 ทองใน UI — ต้องยืนยันสูตรจริงกับ Hero System
- ⚠️ **Ping Auto-destroy Timer**: พบ `timeDestroy` แต่ค่าตัวเลขจริงยังไม่ได้ยืนยัน
- ⚠️ **Team Buff Icons (StatusBuffUI)**: แสดง Red/Blue Buff Timer แต่ค่าตัวเลขขึ้นอยู่กับ Jungle System
