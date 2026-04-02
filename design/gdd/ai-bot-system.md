# AI / Bot System — Game Design Document

**System ID**: FT5
**Version**: 0.1.0
**Status**: Draft — Reverse-documented from source
**Last Updated**: 2026-04-02
**Dependencies**: Hero System (C2), Combat & Skills System (C1), Movement & Navigation (C3)

---

## 1. Overview

ระบบ Bot ใช้ State Machine 11 State ร่วมกับ Fuzzy Utility AI สำหรับการตัดสินใจใช้ทักษะ Bot ทุกตัวทำงานบน Server เท่านั้น (Server-Authoritative) ปัจจุบัน Bot มีความสามารถขั้นพื้นฐาน — เดินตาม Lane, โจมตีศัตรูที่ใกล้, ใช้ทักษะตาม Priority, กลับฐานเมื่อ HP ต่ำ แต่ยังไม่มีระบบซื้อไอเทม, Coordination ทีม, หรือ Last-hit Creep

---

## 2. Player Fantasy

Bot ควรเล่นได้พอให้ผู้เล่นใหม่ฝึกได้โดยไม่เบื่อ — ไม่ง่ายจนพวกเขาไม่เรียนรู้ แต่ก็ไม่ยากจนหมดกำลังใจ AFK Detection ช่วยรักษาประสบการณ์ของทีม เมื่อ Bot เข้าแทน AFK ทีมยังพอสู้ได้ต่อ

---

## 3. Detailed Rules

### 3.1 Bot State Machine

| State | คำอธิบาย | เงื่อนไขเข้า |
|-------|---------|------------|
| **Idle** | หยุดนิ่ง | คำสั่ง Stop |
| **FindRoute** | คำนวณ Waypoint ถัดไป | เริ่มต้น หรือถึง Waypoint |
| **Walking** | เดินตาม Lane | หลัง FindRoute |
| **FindRouteOrWalk** | Path อัจฉริยะ | หลังเสีย Target หรือ Return |
| **FoundTarget** | สู้กับเป้าหมาย | พบศัตรูในระยะ |
| **HpLow** | HP < 30% → ถอยกลับ | HP ต่ำกว่า Threshold |
| **Recall** | Cast Recall 10 วินาที | หลัง HpLow |
| **RegenHp** | ฟื้น HP ที่ฐาน | Recall สำเร็จ |
| **Return** | กลับ Lane หลังหยุด AFK | Player กลับมาเล่น |
| **Stop** | หยุดสมบูรณ์ | กรณีพิเศษ |
| **Following** | (Legacy — ไม่ได้ใช้) | — |

**Flow หลัก**:
```
FindRoute → Walking → FindRoute (Loop)
                ↓
          FoundTarget (โจมตี)
                ├─ Target ตาย → FindRouteOrWalk
                ├─ HP < 30%  → HpLow → Recall → RegenHp
                └─ Chase > 3u → กลับ Lane
```

---

### 3.2 การตัดสินใจใช้ทักษะ (Fuzzy Utility AI)

**`FuzzySkillAI`** ให้คะแนนแต่ละทักษะก่อนตัดสินใจ:

| ปัจจัย | น้ำหนัก | คำอธิบาย |
|--------|---------|---------|
| damageWeight × (1 − enemyHP) | สูง vs HP ต่ำ | เน้นโจมตีเมื่อศัตรูใกล้ตาย |
| executeWeight (เมื่อ enemyHP < 40%) | สูง | ปิด Combo |
| aoeWeight × (nearbyEnemies / 3) | สูงเมื่อ Clump | AoE มีประสิทธิภาพมากขึ้น |
| escapeWeight × (1 − selfHP) | สูงเมื่อ HP ต่ำ | หนีเมื่อโดนโจมตี |
| healWeight × (1 − selfHP) | ขึ้นกับ HP | ฮีลตัวเอง |
| buffWeight | 1.0 โดนโจมตี / 0.5 ปกติ | Buff ป้องกัน |
| ระยะ (range factor) | ลดคะแนนตามระยะ | ทักษะใกล้ได้คะแนนสูงกว่า |

**ลำดับ Priority Queue**:
```
R (Ultimate) → Q → W → E
```
→ เลือกทักษะที่ Score สูงสุดและ Available

---

### 3.3 Targeting

- **ระยะ Scan**: 3.5 หน่วย (Physics.OverlapSphere)
- **Rate**: ทุก **0.2 วินาที** (Server Tick)
- **Priority**: ศัตรูที่ใกล้ที่สุดในระยะ (ไม่แยก Hero vs Minion)
- **กฎ Ignore**: Invisible (`IsInvisibility`) และ ใน Bush (`IsInBush`) ถูกข้าม
- **Chase Limit**: ถ้าไล่ตามไกล > **3 หน่วย** จากจุดเริ่ม Chase → ล้าง Target กลับ Lane

---

### 3.4 การเดิน (Pathfinding)

- ใช้ **Waypoint-based** ตามเส้น Lane (ไม่ใช่ NavMesh เดียวกับ Hero)
- Waypoint ถูกกำหนดล่วงหน้าต่อ Lane (Top/Mid/Bottom/Jungle/Support)
- Destination Threshold: **5 หน่วย** (ถึงแล้วไปต่อ)
- Base Waypoint: จุด Spawn ของทีม (ใช้ Return กลับฐาน)

---

### 3.5 AFK Detection & Bot Replacement

**Timer**:

| เหตุการณ์ | เวลา |
|---------|------|
| แสดง AFK Warning | 180 วินาที (3 นาที) |
| Kick ออกจากเกม | 300 วินาที (5 นาที) |

**ขั้นตอน Bot เข้าแทน**:
1. `IsAFK = true` → `BotActor` Component ถูกเพิ่มเข้า Avatar
2. Bot ถูกกำหนด Lane = Mid (Default)
3. Bot เข้า State `Return` → กลับฐานก่อน
4. Server รัน AI สำหรับ Avatar นั้นแทน Input ของ Player

**Player กลับมา**:
1. `IsAFK = false` → ลบ `BotActor` Component
2. Player Input กลับมาควบคุมตามปกติ

---

### 3.6 Skill Point Auto-distribution

เมื่อ Bot Level Up → แจก Skill Point อัตโนมัติ:
- Priority: R → E → W → Q
- ใช้กฎ Max Rank เดียวกับ Player (ceil(Level/2) สำหรับ Q/W/E, floor(Level/6) สำหรับ R)

---

### 3.7 สิ่งที่ Bot ยังไม่มี

| Feature | สถานะ |
|---------|-------|
| ซื้อไอเทม | ❌ ไม่มี |
| Last-hit Minion | ❌ ไม่มี |
| Coordination กับ Bot ด้วยกัน | ❌ ไม่มี |
| Objective awareness (Boss/Tower) | ❌ ไม่มี |
| Gank / Roaming | ❌ ไม่มี |
| ระดับความยาก | ❌ ค่า Hardcode ทั้งหมด |
| Collision Avoidance | ❌ โค้ดมีแต่ถูก Comment Out |

---

## 4. Formulas

### Fuzzy Skill Score
```
Score = (damageW × (1−enemyHP))
      + (executeW × clamp(0.4−enemyHP, 0, 1) × 2)
      + (aoeW × clamp(nearbyEnemies/3, 0, 1))
      + (escapeW × (1−selfHP) × isUnderAttack)
      + (healW × (1−selfHP))
      + (buffW × (isUnderAttack ? 1.0 : 0.5))
      × (1 − distance/skillRange)  [range penalty]
```

### AFK Bot Priority (Lane)
```
ถ้า Player AFK:
  Bot Lane = Mid (Default)
  Bot State = Return → กลับฐานก่อน
```

---

## 5. Edge Cases

| สถานการณ์ | พฤติกรรม |
|-----------|---------|
| Bot HP < 30% ระหว่างสู้ | ออกจาก FoundTarget → HpLow → Recall ทันที |
| Target หาย (Invisible/Bush) | ล้าง Target → FindRouteOrWalk |
| Recall ถูกขัด (ถูกโจมตี) | ⚠️ ต้องตรวจสอบว่า Bot ยกเลิก Recall เมื่อโดน Damage หรือไม่ |
| Player กลับมาหลัง Bot HP ต่ำ | Bot ถูกลบ → Player ควบคุมด้วย HP ต่ำ |
| Bot ถูก CC (Stun/Root) | AI ตรวจ `CanMove/CanAttack` ก่อน Execute |
| Skill ทุกตัว Cooldown | Bot ใช้ Normal Attack เท่านั้น |

---

## 6. Dependencies

| ระบบ | ความสัมพันธ์ |
|------|------------|
| **Hero System (C2)** | Bot ควบคุม Actor ของ Hero; ใช้ Skill Set เดียวกัน |
| **Combat & Skills System (C1)** | FuzzySkillAI ประเมิน Skill; ใช้ `IsAvailableToUse()` |
| **Movement & Navigation (C3)** | Bot ใช้ `Actor.Driver.SetDestination()` เดียวกัน แต่ผ่าน Waypoint แทน NavMesh Click |
| **Networking Core (F2)** | Bot Execute บน Server; State Sync ผ่าน [Networked] |
| **Map & Objectives (FT2)** | Lane Waypoint มาจาก `NetworkGameManager.SpawnPoint` |
| **Level/XP System (C5)** | Bot Level Up → Auto-distribute Skill Points |

---

## 7. Tuning Knobs

| ค่า | ที่อยู่ | ค่าปัจจุบัน | ผลกระทบ |
|-----|--------|------------|---------|
| Low HP Threshold | BotActor | 30% | HP เท่าไหร่ถึงถอยกลับ |
| Skill Cooldown Delay | BotActor | 20 วินาที | ความถี่ Bot ใช้ทักษะ |
| Skill Queue Delay | BotActor | 0.1 วินาที | ช่วงระหว่าง Skill ใน Queue |
| Destination Threshold | BotActor | 5 หน่วย | ระยะ "ถึง Waypoint" |
| Return (Leash) Radius | BotActor | 3 หน่วย | Chase ไกลสุดก่อนกลับ Lane |
| Target Scan Radius | AvatarSystem | 3.5 หน่วย | ระยะตรวจศัตรู |
| Scan Rate | AvatarSystem | 0.2 วินาที | ความถี่ตรวจสอบ |
| Recall Duration | BotActor | 10 วินาที | เวลา Cast Recall |
| AFK Warning | ActorAFKHandle | 180 วินาที | แจ้งเตือนก่อน Kick |
| AFK Kick | ActorAFKHandle | 300 วินาที | Kick ออก |

---

## 8. Acceptance Criteria

- [ ] Bot เดินตาม Lane Waypoints จนถึง Tower ศัตรู
- [ ] Bot โจมตีศัตรูในระยะ 3.5 หน่วยโดยอัตโนมัติ
- [ ] Bot ใช้ทักษะตาม Priority (R > Q > W > E) เมื่อ Available
- [ ] Bot HP < 30% → Recall กลับฐาน
- [ ] Bot ไล่ตามศัตรูไกลเกิน 3 หน่วย → หยุดและกลับ Lane
- [ ] AFK 180 วินาที → แสดง Warning
- [ ] AFK 300 วินาที → Kick; Bot เข้าแทนใน Avatar นั้น
- [ ] Player กลับมา → Bot ถูกลบ; Input ควบคุมได้ตามปกติ
- [ ] Bot ไม่โจมตี Invisible / In-Bush Enemy

---

## Known Issues / TODO

- ⚠️ **ไม่มีระบบซื้อไอเทม**: Bot เล่น 0 ไอเทมตลอดเกม → อ่อนแอมาก Late Game
- ⚠️ **Collision Avoidance Disabled**: Botหลาย ตัวในพื้นที่แคบอาจซ้อนกัน
- ⚠️ **ไม่มี Difficulty Level**: ทุกค่า Hardcode — ต้องออกแบบ Difficulty Multiplier
- ⚠️ **Recall Interrupt**: ยังไม่ชัดเจนว่า Bot ยกเลิก Recall เมื่อโดน Damage หรือไม่
- ⚠️ **Lane Assignment**: Bot AFK ถูกกำหนด Lane = Mid เสมอ ไม่ใช่ Lane เดิมของ Player
