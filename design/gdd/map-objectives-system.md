# Map & Objectives System — Game Design Document

**System ID**: FT2
**Version**: 0.1.0
**Status**: Draft — Reverse-documented from source
**Last Updated**: 2026-04-02
**Dependencies**: Actor System (F1), Combat & Skills System (C1), Gold Economy (C3), Level/XP System (C5), Data-Config System (F3)

---

## 1. Overview

แผนที่ Delta เป็นสนามรบแบบ Symmetric 5v5 มี 3 Lane (Top/Mid/Bottom) และพื้นที่ Jungle ตรงกลาง แต่ละทีมมีโครงสร้างป้องกันลำดับชั้น ได้แก่ Tower 3 ชั้นต่อ Lane, Inhibitor (Barrack) 2 ชิ้นต่อ Lane, และ Core ที่เป็นเป้าหมายชนะเกม ทุก Objective มีระบบ Scaling ตามเวลา, Minion Wave อัตโนมัติ, และ Jungle Camp ที่ Respawn พร้อม Buff พิเศษ

---

## 2. Player Fantasy

ผู้เล่นรู้สึกว่าแผนที่มีชีวิตและเปลี่ยนแปลงตลอดเวลา — Tower ทยอยพัง, Super Minion เริ่มไหล, Boss โผล่ช่วง Late Game สร้างแรงกดดัน ทุกการตัดสินใจ (ล้าง Jungle ก่อน / Push Lane / รุม Boss) มีน้ำหนัก และชัยชนะเกิดจากการแตก Core ของศัตรู ไม่ใช่การฆ่า Hero

---

## 3. Detailed Rules

### 3.1 โครงสร้างแผนที่

**รูปแบบ**: Symmetric — Team1 ด้านบวก, Team2 ด้านลบ (Mirror)
**ผู้เล่น**: 5 ต่อทีม (5v5)
**Lane**: 3 เส้น

| Lane | คำอธิบาย |
|------|---------|
| **Top** | เส้นบน |
| **Mid** | เส้นกลาง (สั้นที่สุด) |
| **Bottom** | เส้นล่าง |

นอกจาก Lane ยังมีพื้นที่ **Jungle** (ตรงกลาง) และ **Support** เป็น Role ที่ลงเส้น Bottom ร่วมกับ Carry

---

### 3.2 โครงสร้าง Tower ต่อทีม

แต่ละทีมมี Tower ทั้งหมดดังนี้:

**Lane Towers (3 Tier × 3 Lane = 9 Tower)**:

| Tower | Tier | Lane |
|-------|------|------|
| Top1 | Tier1 (Outer) | Top |
| Top2 | Tier2 (Inner) | Top |
| Top3 | Tier3 (Base) | Top |
| Mid1 | Tier1 | Mid |
| Mid2 | Tier2 | Mid |
| Mid3 | Tier3 | Mid |
| Bottom1 | Tier1 | Bottom |
| Bottom2 | Tier2 | Bottom |
| Bottom3 | Tier3 | Bottom |

**Inhibitor/Barrack (2 × 3 Lane = 6 Barrack)**:

| Barrack | Lane | ผลเมื่อพัง |
|---------|------|----------|
| TopMeleeBarrack | Top | Super Melee Minion ไหลใน Top Lane |
| TopRangeBarrack | Top | Super Range Minion ไหลใน Top Lane |
| MidMeleeBarrack | Mid | Super Melee Minion ไหลใน Mid Lane |
| MidRangeBarrack | Mid | Super Range Minion ไหลใน Mid Lane |
| BottomMeleeBarrack | Bottom | Super Melee Minion ไหลใน Bottom Lane |
| BottomRangeBarrack | Bottom | Super Range Minion ไหลใน Bottom Lane |

> Super Minion เริ่มไหล**เมื่อ Barrack ทั้ง 2 ใน Lane นั้นพังทั้งคู่**

**Base Structures**:

| Structure | Tier | หมายเหตุ |
|-----------|------|---------|
| CoreTower1 | Core | Tower ป้องกัน Core ชั้นที่ 1 |
| CoreTower2 | Core | Tower ป้องกัน Core ชั้นที่ 2 |
| Core | Core | **เป้าหมายหลัก — ทำลายเพื่อชนะเกม** |
| Fountain | Fountain | ฮีลผู้เล่นที่อยู่ในฐาน |

---

### 3.3 เงื่อนไขชนะ (Win Condition)

**ทีมที่ทำลาย Core ของศัตรูเป็นผู้ชนะ**

เมื่อ Core ถูกทำลาย → `NetworkGameManager.SetWinner(winnerTeam)` → เกมจบทันที

---

### 3.4 Tower Targeting Priority

Tower จะเลือกเป้าหมายตามลำดับความสำคัญ:

| ลำดับ | เป้าหมาย |
|-------|---------|
| 1 | Hero ศัตรูที่กำลังโจมตี Hero ฝ่ายเดียวกัน (Aggressor) |
| 2 | Siege Creep |
| 3 | Melee Creep |
| 4 | Range Creep |
| 5 | Hero ศัตรูที่ไม่ได้โจมตีใคร (Passive) |
| 6 | ศัตรูที่ใกล้ที่สุด |

กฎเพิ่มเติม:
- Tower **ไม่โจมตี** Jungle Creep
- Tower ใช้ Raycast ตรวจ Line of Sight ก่อนโจมตี
- **Warning Radius**: 12 หน่วย (แจ้งเตือน — สว่าง/เสียง)
- **Attack Radius**: 8 หน่วย (ช่วงโจมตีจริง)

---

### 3.5 Tower Buff ให้ Hero

เมื่อ Hero ฝ่ายเดียวกันอยู่ใกล้ Tower จะได้รับ:
- **+20 Armor**
- **+5 HP Regen ต่อวินาที**

Buff หายเมื่อออกจากระยะ Tower

---

### 3.6 Tower HP Stage

Tower แสดงสถานะความเสียหายผ่าน Visual ตาม HP:

| Stage | HP เหลือ |
|-------|---------|
| Intact | > 66% |
| Stage 2 | 33–66% |
| Stage 3 | 1–33% |
| Destroyed | ≤ 0% |

Tower ที่ถูกทำลาย**ไม่ Respawn** (ยกเว้น Inhibitor — ดูส่วน 3.2 หมายเหตุ)

---

### 3.7 Minion Wave System

**ประเภท Minion**:

| ประเภท | คำอธิบาย |
|--------|---------|
| MeleeCreep | สู้ระยะประชิด |
| RangeCreep | โจมตีระยะไกล |
| SeigeCreep | โจมตี Structure แรง ทน |
| SuperCreep | Super Minion หลัง Barrack พัง |
| JungleCreep | Neutral monster ใน Jungle |
| Boss | Objective หลักช่วง Late Game |
| MiniBoss | Objective รอง |

**Wave Composition**:
- แต่ละ Wave มี: MeleeCreep (N ตัว) + RangeCreep (N ตัว) + SeigeCreep (N ตัว)
- จำนวนต่อ Wave กำหนดใน CBS (`CBSMeleeCreepAmount`, `CBSRangeCreepAmount`, `CBSSiegeCreepAmount`)
- **Siege Wave**: ทุก 5 Wave (wave % 5 == 0) — Siege Creep ตัวพิเศษมาในคลื่นนั้น

**Wave Mechanics**:
- Minion ในแต่ละ Wave มีระบบ Leader/Follower (ตัว Lead path, ตัวอื่น Follow)
- เดินตาม Waypoints ที่กำหนดไว้ต่อ Lane

---

### 3.8 Jungle System

**พื้นที่ Jungle** ของแต่ละทีมมี Camp ดังนี้:

| Camp | ตำแหน่ง | Buff |
|------|---------|------|
| BlueJungleB | Blue Side Bottom Jungle | Blue Buff (Ability Power Bonus) |
| BlueJungleR | Blue Side Red Buff Camp | Red Buff (Attack Damage Bonus) |
| RedJungleB | Red Side Bottom Jungle | Blue Buff equivalent |
| RedJungleR | Red Side Red Buff Camp | Red Buff equivalent |

**Major Objectives**:

| Objective | ประเภท | หมายเหตุ |
|-----------|--------|---------|
| **Boss** | MonsterType.Boss | Major — ทีม Kill ได้ Buff ทั้งทีม |
| **MiniBoss** | MonsterType.MiniBoss | Minor — Bounty สูง |

---

### 3.9 การ Scaling ตามเวลา

ทุกหน่วยในเกมมีค่า Stat เพิ่มขึ้นตามเวลา:

| หน่วย | Stat ที่ Scale |
|-------|--------------|
| Lane Creep | HP (`CreepLaneIncHp`), ATK (`CreepLaneIncAtk`) |
| Jungle Creep | HP (`CreepJungleIncHp`), ATK (`CreepJungleIncAtk`), AP (`CreepJungleIncDamage`) |
| Tower | Stats เพิ่มตาม `TowerIncreaseStatTimer` |

Interval ของ Scaling กำหนดใน CBS (`CreepLaneIncreaseStatTimer`, `CreepJungleIncreaseStatTimer`)

---

### 3.10 End Game Mechanic

- ช่วง Late Game มี **EndGame Phase** ที่ Trigger ตาม `BossEndGameTime`
- ก่อน Activate 10 วินาที → เข้าสู่ `EndgamePrepTriggered` (แจ้งเตือนผู้เล่น)
- หลัง Activate → `EndgameActivated` (เปลี่ยน Game State)

---

## 4. Formulas

### Tower Buff Radius
```
Warning Radius = TowerAttackRange + (TowerSize / 2) + 4   = ~12 units
Attack Radius  = TowerAttackRange + (TowerSize / 2)       = ~8 units
```

### Tower Damage
ใช้สูตรเดียวกับ Physical Damage จากระบบ Combat:
```
TowerDamage = DamagePhysicalRadiusFormula(baseDamage, targetDefense)
```
ลด Damage กับ Hero 50%, กับ Objective 30% (ดู Combat & Skills System C1)

### Hero Spawn Position
```
SpawnPos = BaseSpawnPoint ± Random(-3, +3) units  [constrained to NavMesh]
```

---

## 5. Edge Cases

| สถานการณ์ | พฤติกรรม |
|-----------|---------|
| Tower ถูกทำลาย | ไม่ Respawn; ลบออกจาก Tower Dictionary |
| Barrack พังเพียง 1 ใน Lane | Super Minion ยังไม่ออก — ต้องพังทั้ง 2 |
| Tower โจมตี Hero ที่มี Stealth | ขึ้นอยู่กับ Vision/Reveal system |
| Minion เดินชน Obstacle | ใช้ NavMesh หลีกเลี่ยงอัตโนมัติ |
| Boss ถูก KS (Kill Steal) | ทีมที่ตี Last Hit ได้รับ Buff |
| Jungle Creep ที่ยังไม่ Spawn ครั้งแรก | ไม่มีผลต่อ Tower targeting |
| Tower ตรวจ LOS ไม่เจอเป้า | ข้ามไปหาเป้าถัดไปตาม Priority |
| Hero เดินผ่าน Tower Range โดยไม่โจมตีใคร | Tower จัดเป็น Passive Priority (ลำดับ 5) |

---

## 6. Dependencies

| ระบบ | ความสัมพันธ์ |
|------|------------|
| **Actor System (F1)** | Tower, Minion, Boss ทั้งหมด Extend Actor Base Class |
| **Combat & Skills System (C1)** | Tower damage ผ่าน Physical Damage pipeline |
| **Gold Economy (C3)** | Tower / Minion / Boss ให้ทองเมื่อถูกทำลาย |
| **Level/XP System (C5)** | Minion / Tower ให้ XP เมื่อถูกทำลาย |
| **Data-Config System (F3)** | ทุก Spawn Timer / Stat / Amount มาจาก CBS (`CBSConfigCreep`, `CBSConfigBattle`) |
| **Hero System (C2)** | Hero Spawn/Respawn ที่ Base Spawn Point; Recall กลับ Base |
| **Photon Fusion (F5)** | Tower State, Creep Wave, Jungle Respawn ทั้งหมด Server-Authoritative |

---

## 7. Tuning Knobs

| ค่า | CBS Key | ค่าปัจจุบัน | หมายเหตุ |
|-----|---------|------------|---------|
| Minion Wave Interval | CooldownCreepWave | CBS | วินาทีระหว่าง Wave |
| First Wave Time | FirstDeploy.CreepAtTime | CBS | วินาทีหลังเกมเริ่ม |
| Jungle Respawn Interval | CooldownCreepJungle | CBS | แยกตาม Camp |
| Boss Initial Spawn | CooldownSpawn.BossSpawn | CBS | — |
| Boss Respawn | CooldownBoss | CBS | หลัง Kill ครั้งแรก |
| MiniBoss Initial Spawn | CooldownSpawn.MiniBossSpawn | CBS | — |
| MiniBoss Respawn | CooldownMiniBoss | CBS | — |
| Minion Per Wave | CBSMeleeCreepAmount/CBSRangeCreepAmount/CBSSiegeCreepAmount | CBS | แยกตามประเภท |
| Lane Creep HP Scaling | CreepLaneIncHp + Interval | CBS | เพิ่มทุก N วินาที |
| Lane Creep ATK Scaling | CreepLaneIncAtk + Interval | CBS | — |
| Jungle Creep Scaling | CreepJungleIncHp/Atk/Damage + Interval | CBS | — |
| Tower Buff Armor | Hardcode | +20 | เปลี่ยนได้เฉพาะโค้ด |
| Tower Buff HP Regen | Hardcode | +5/s | เปลี่ยนได้เฉพาะโค้ด |
| Warning Radius Offset | Hardcode | +4 units | เปลี่ยนได้เฉพาะโค้ด |
| EndGame Trigger Time | BossEndGameTime | CBS | — |
| Siege Wave Frequency | Hardcode | ทุก 5 Wave | เปลี่ยนได้เฉพาะโค้ด |

---

## 8. Acceptance Criteria

- [ ] Minion Wave spawn ใน 3 Lane พร้อมกัน ตาม Wave Timer ที่กำหนด
- [ ] Siege Wave โผล่ทุก 5 Wave
- [ ] Tower โจมตีตาม Priority: Aggressor Hero > Siege > Melee > Range > Passive Hero > Closest
- [ ] Tower **ไม่โจมตี** Jungle Creep
- [ ] Tower Buff (+20 Armor, +5 HP/s) Active เมื่อ Hero อยู่ใกล้, หายเมื่อออก
- [ ] Barrack พังทั้ง 2 ใน Lane → Super Minion เริ่ม Spawn ถาวรใน Lane นั้น
- [ ] Core ถูกทำลาย → `SetWinner()` ถูกเรียก → เกมจบ
- [ ] Boss / MiniBoss Respawn หลัง Cooldown ครบ
- [ ] Tower ที่ถูกทำลายไม่ Respawn
- [ ] Creep / Jungle / Tower Scale Stats ตาม Timer ที่กำหนดใน CBS
- [ ] EndGame Phase Trigger ตาม BossEndGameTime พร้อม Prep warning 10 วินาที
- [ ] Hero Spawn ที่ Base ใน NavMesh ที่ถูกต้อง (±3 units offset)
- [ ] Network Sync: Tower destroyed state / Wave count / Camp respawn ถูก Sync ทุก Client

---

## Known Issues / TODO

- ⚠️ **Inhibitor Respawn**: โค้ดระบุว่า Barrack "ไม่ Respawn" — ต้องยืนยันว่าเป็น Design Intent หรือ Feature ที่ยังไม่ได้ทำ
- ⚠️ **Tower HP/Damage Values**: ยังไม่มีตัวเลขจาก CBS — ต้องดึงจาก Dashboard
- ⚠️ **Boss Buff Details**: ยังไม่ชัดเจนว่า Boss ให้ Buff อะไรแก่ทีม (ระยะเวลา, ค่า Stat)
- ⚠️ **Super Minion Stats**: ค่า HP/ATK ของ SuperCreep ยังไม่ได้เอกสาร
- ⚠️ **Jungle Buff (Blue/Red)**: ชื่อ Effect และค่าตัวเลขยังไม่ได้ยืนยัน
