# Creep & Minion System — Game Design Document

**System ID**: FT3
**Version**: 0.1.0
**Status**: Draft — Reverse-documented from source
**Last Updated**: 2026-04-02
**Dependencies**: Actor System (F1), Combat & Skills System (C1), Map & Objectives (FT2), Movement & Navigation (C3)

---

## 1. Overview

ระบบ Creep & Minion จัดการหน่วย NPC ทั้งหมดในสนาม ได้แก่ Lane Minion (สปอนตาม Wave อัตโนมัติ), Jungle Monster (แคมป์ Respawn ได้), และ Boss/MiniBoss (Objective หลัก) แต่ละประเภทมี State Machine AI, ระบบ Aggro, และ Leash ที่แตกต่างกัน ค่าสถิติและ Timing ทั้งหมดมาจาก CBS ไม่ Hardcode ในโค้ด

---

## 2. Player Fantasy

Minion ควรรู้สึกเหมือน "แนวรบที่มีชีวิต" — ไหลตาม Lane ไปเรื่อยๆ, ผลักดันหอคอย, บังคับศัตรูต้องตอบสนอง ผู้เล่น Jungle รู้สึกถึงความตื่นเต้นในการล่า Monster รับ Buff แล้วลงช่วย Lane Boss และ MiniBoss เป็นช่วงเวลาที่ทั้งทีมต้องร่วมมือ — ใครกล้าลงก่อนอาจถูก Steal, ใครแน่กว่าชนะ

---

## 3. Detailed Rules

### 3.1 ประเภทหน่วย

| ประเภท | MonsterType | กลุ่ม | สปอนที่ไหน |
|--------|-------------|-------|-----------|
| MeleeCreep | MeleeCreep | IsTrooper | Lane Wave |
| RangeCreep | RangeCreep | IsTrooper | Lane Wave |
| SeigeCreep | SeigeCreep | IsTrooper | Siege Wave (ทุก 5 Wave) |
| SuperCreep | SuperCreep | IsTrooper | Lane Wave (หลัง Barrack พัง) |
| JungleCreep | JungleCreep | IsJungle | Jungle Camp |
| Boss | Boss | IsBoss | Boss Spawn Point |
| MiniBoss | MiniBoss | IsMiniBoss | MiniBoss Spawn Point |

---

### 3.2 Lane Wave System

**Wave Composition**:
- แต่ละ Wave มี: Melee × N + Range × N + Siege × N (ตาม CBS Amount)
- **Siege Wave**: ทุก Wave ที่ `wave % 5 == 0` — มี SeigeCreep เพิ่ม
- **Super Creep**: สปอนร่วมกับ Wave ปกติ ถ้า Barrack ของทีมศัตรูในเส้นนั้นพังแล้ว

**Spawn Timing**:
- ระหว่าง Creep แต่ละตัวในกลุ่ม: **0.1 วินาที**
- ระหว่างประเภท (Melee → Range → Siege): **0.3 วินาที**
- ระหว่าง Wave: ตาม `CBSConfigCreep.CooldownCreepWave`

**Leader/Follower System**:
- Creep ตัวแรกของแต่ละ (Team, Lane, Wave) = **Leader**
- ตัวอื่น = **Follower** — ติดตาม Leader ห่าง **1.5 หน่วย**
- ถ้า Leader ตาย → Follower ตัวแรกสืบตำแหน่ง Leader ต่อ (รับ Destination ต่อ)

**Wave Counter**:
- Wave นับตั้งแต่เริ่มเกม ไม่มี Upper Limit (Wave ไปเรื่อยๆ)
- Siege Interval: Wave 5, 10, 15, 20, ...

---

### 3.3 Lane Creep AI State Machine

```
Idle
  └─ เกมเริ่ม → FindRoute
      └─ คำนวณ Path → Walking
          ├─ ถึง Waypoint → FindRoute (ต่อ)
          ├─ เจอศัตรูในระยะ → FoundTarget
          │     ├─ ศัตรูอยู่ใน Bush/Invisible → ExitChase → Return
          │     ├─ chase ไกลเกิน Leash → ExitChase → Return
          │     └─ ฆ่าเป้าแล้ว → Walking / FindRoute
          └─ ถึงปลายทาง (หอคอย/ฐาน) → โจมตี Structure
```

**Target Priority ของ Lane Creep**:
1. หน่วยที่โจมตีตัวเองหรือ Leader ก่อน (Aggro Target)
2. หน่วยศัตรูที่อยู่ในระยะ Detection (scan ทุก 0.1 วินาที)
3. ถ้าจำนวน Attacker > Target → แบ่ง Round-Robin (Even Distribution)
4. ถ้าจำนวน Target > Attacker → โจมตีเป้าเดียวกัน (Focused Fire)

**Leash Range**:
- Lane Creep: **3 หน่วย** จากจุดเริ่ม Chase
- เมื่อระยะ > Leash → `ExitChase()` → กลับ Path เดิม

---

### 3.4 Jungle Monster AI

**Idle Behavior**:
- เริ่มต้น: หยุดนิ่งที่ Spawn Point (BotState.Idle)
- ไม่ Patrol

**Aggro**:
- ใช้ `Physics.OverlapSphereNonAlloc()` ตรวจ Detection Radius = `colliderMagnitude × 0.7`
- Aggro เมื่อ Hero เข้า Radius หรือโจมตี Monster
- **Group Aggro**: ถ้า Leader ถูกโจมตี → สมาชิกทุกตัวในแคมป์รวมตัว (`Assemble()`)

**Leash Range**:
- **6 หน่วย** จาก Spawn Point
- เมื่อ Chase ไกลเกิน Leash → กลับ Spawn, ฟื้น HP ระหว่างทาง

**Death ของ Leader**:
- Leader ตาย → สมาชิกทั้งแคมป์ถูก Kill ทันที
- Respawn Timer เริ่มนับ → ทั้งแคมป์สปอนใหม่พร้อมกัน

---

### 3.5 Boss & MiniBoss AI

| ค่า | Boss | MiniBoss |
|-----|------|---------|
| Identity (default) | `boss_icedragon_phase1` | `boss_foresdragon` |
| Leash Range | 8 หน่วย | 8 หน่วย |
| Keep Target Radius | 18 หน่วย | 18 หน่วย |
| Drop Target Radius | 30 หน่วย | 30 หน่วย |

**Combat Pattern System**:
Boss/MiniBoss ใช้ Pattern-Based AI (ไม่ใช่ Simple Auto-Attack):

| Pattern | ลำดับการกระทำ |
|---------|-------------|
| Pattern 1 | Attack → Skill |
| Pattern 2 | Attack → Attack → Skill |
| Pattern 3 | Skill → Attack |
| Pattern 4 | Attack → Skill → Attack |

- `m_RandomizePattern = true` → สุ่มเลือก Pattern
- `m_StepDelay = 0.22s` ระหว่าง Step
- `m_PostCastIdle = 0.70s` หลัง Cast Skill
- `m_ChargeSecondsOverride = 5s` ระยะเวลา Charge Skill

**Boss Death Rewards**:
- ทีมที่ Kill → ได้รับ Buff ผ่าน `boss_icedragon_phase1_buff`
- ดรอป Item Box ที่ Spawn Point
- Respawn หลัง `CooldownBoss` / `CooldownMiniBoss` ครบ

**Endgame Phase**:
- มี `SpawnBossEndgame()` → Boss รูปแบบพิเศษ (index สุดท้ายใน BossMonsters array)
- Trigger ตาม Game State ไม่ใช่ HP

---

### 3.6 Super Creep

- สปอนร่วมกับ Wave ปกติ (ไม่แทนที่ Minion ปกติ)
- Condition: `IsBarrackOpen(lane, opposingTeam)` → Barrack ทั้ง 2 ใน Lane นั้นพังแล้ว
- จำนวน: 1 ตัวต่อ Barrack ที่พังต่อ Wave → ถ้าพังทั้ง 2 → ได้ 2 ตัวต่อ Wave
- สถิติสูงกว่า Normal Creep อย่างมีนัยสำคัญ (ค่าจาก CBS)

---

### 3.7 Reinforcement System

ระบบเสริมกองกำลังพิเศษ (นอกเหนือ Wave ปกติ):
- `CreepReinforcementTeam` ติดตาม Extra Spawn ต่อทีม
- สปอน Melee/Range/Siege/Super Creep เพิ่มได้ระหว่างเกม
- ประกาศผ่าน `AnnounceType.ItemCreepSpawn`
- (ใช้ผ่านไอเทมหรือ Event พิเศษ)

---

### 3.8 Death & Despawn

**Lane Creep ตาย**:
1. หยุด Collider, ลบ HP Bar, เล่น Death Animation
2. Clear Status Effects ทั้งหมด
3. ลบ Minimap Icon
4. รอ **5 วินาที** → Despawn (Network-Sync)
5. ทอง/XP ให้ตามกฎ Gold Economy / Level-XP System

**Jungle Monster ตาย**:
1. Leader ตาย → สมาชิกทั้งแคมป์ถูก Kill ทันที
2. Camp ถูก Mark `IsAlive = false`
3. Respawn Timer เริ่มนับ

---

### 3.9 Mind Control Interaction

- หาก `Trait.IsMindControl == true` → AI Exit Early ทุก State
- Creep ที่ถูก Mind Control ตามคำสั่ง Caster ไม่ใช่ AI เดิม

---

## 4. Formulas

### Scaling Stat ตามเวลา (Lane Creep)
```
CurrentHP  = BaseHP  + (CreepLaneIncHp  × เวลาที่ผ่านไป / CreepLaneIncreaseStatTimer)
CurrentATK = BaseATK + (CreepLaneIncAtk × เวลาที่ผ่านไป / CreepLaneIncreaseStatTimer)
```

### Scaling Stat ตามเวลา (Jungle Creep)
```
CurrentHP  = BaseHP   + (CreepJungleIncHp     × ...)
CurrentATK = BaseATK  + (CreepJungleIncAtk    × ...)
CurrentAP  = BaseAP   + (CreepJungleIncDamage × ...)
```

### Siege Wave Trigger
```
isSiegeWave = (wave % 5 == 0)
```

### Super Creep Count per Wave
```
superCreepCount = count of destroyed barracks in lane (0, 1, or 2)
```

### Follower Formation Position
```
FollowPoint = LeaderPosition - (Leader.Forward × 1.5f × followerIndex)
```

### Boss Target Reacquisition
```
keepTarget  = currentTargetDistance ≤ 18f
dropTarget  = currentTargetDistance > 30f
```

---

## 5. Edge Cases

| สถานการณ์ | พฤติกรรม |
|-----------|---------|
| Leader ถูกกักตัวด้วย CC | Follower รอ; ถ้า Leader ตาย → Follower แรกเป็น Leader |
| Creep วิ่งเข้าพุ่มไม้ (Bush) | ไม่หาย — แค่ Enemy มองไม่เห็น (Fog of War) |
| Minion Chase เข้า Bush ตาม Hero | ถ้า Hero อยู่ใน Bush/Invisible → Creep `ExitChase()` กลับ Lane |
| Boss ถูกดึงออกนอก Leash 8 หน่วย | Boss กลับ Spawn ทันที; HP ฟื้นหรือ Reset? ⚠️ ต้องตรวจสอบ |
| Jungle Leader ตายโดยไม่มี Follower | แคมป์ว่าง → Respawn Timer เริ่มทันที |
| Super Creep + Normal Wave ใน Lane เดียวกัน | สปอนพร้อมกัน ต่อท้าย Wave ปกติ |
| Wave Overflow (creep จาก Wave ก่อนยังไม่ตาย) | Wave ใหม่สปอนตาม Timer ปกติ ไม่รอ Wave เก่าตาย |
| Reinforcement ระหว่าง Wave | เพิ่ม Creep เข้า Lane ทันทีโดยไม่รอ Wave Timer |
| Creep ถูก Mind Control | AI Bypass ทั้งหมด; คืนสู่ปกติเมื่อ Effect หมด |

---

## 6. Dependencies

| ระบบ | ความสัมพันธ์ |
|------|------------|
| **Actor System (F1)** | Creep ทั้งหมด Extend Actor; ใช้ Trait (HP/Stats/CC) และ Driver (NavMesh) |
| **Combat & Skills System (C1)** | Damage pipeline, Status Effects; Boss ใช้ ActorCombatAction สำหรับ Skill |
| **Map & Objectives (FT2)** | Spawn Points, Lane Waypoints, Barrack State, Boss Spawn Location |
| **Movement & Navigation (C3)** | NavMeshAgent; Formation PathFinding; Stuck Recovery |
| **Gold Economy (C3)** | Kill Bounty ต่อ Creep Type |
| **Level/XP System (C5)** | XP ให้เมื่อฆ่า Creep; Sharing ระหว่าง Hero ใกล้เคียง |
| **Data-Config System (F3)** | CBSUnit (stats), CBSConfigCreep (timers), CBSCreepAmount (spawn count) |
| **Fog of War (FT4)** | Creep Vision; Jungle Creep ซ่อนจาก Enemy จนกว่าจะถูก Reveal |

---

## 7. Tuning Knobs

| ค่า | CBS Key | ค่าปัจจุบัน | ผลกระทบ |
|-----|---------|------------|---------|
| Wave Interval | CooldownCreepWave | CBS | ความเร็วในการ Push Lane |
| Creep Count ต่อ Wave | CBSMeleeCreepAmount / CBSRangeCreepAmount / CBSSiegeCreepAmount | CBS | ความหนาแน่นของ Lane |
| Lane Creep HP/ATK ต่อ Wave | CreepLaneIncHp / CreepLaneIncAtk + Timer | CBS | ความแข็งแกร่งยุค Late Game |
| Jungle Respawn | CooldownCreepJungle | CBS | ความถี่ในการ Farm Jungle |
| Boss Respawn | CooldownBoss | CBS | — |
| MiniBoss Respawn | CooldownMiniBoss | CBS | — |
| Lane Creep Leash | Hardcode | 3 หน่วย | เปลี่ยนได้เฉพาะโค้ด |
| Jungle Leash | Hardcode | 6 หน่วย | เปลี่ยนได้เฉพาะโค้ด |
| Boss Leash | Hardcode | 8 หน่วย | เปลี่ยนได้เฉพาะโค้ด |
| Follower Spacing | Hardcode | 1.5 หน่วย | เปลี่ยนได้เฉพาะโค้ด |
| Siege Wave Frequency | Hardcode | ทุก 5 Wave | เปลี่ยนได้เฉพาะโค้ด |
| Creep Despawn Delay | Hardcode | 5 วินาที | เปลี่ยนได้เฉพาะโค้ด |
| Boss Pattern Randomize | ActorBoss.m_RandomizePattern | true | ถ้า false → Sequential |

---

## 8. Acceptance Criteria

- [ ] Lane Minion สปอนใน 3 Lane พร้อมกันตาม Wave Timer; ทุก Wave มี Melee + Range
- [ ] Siege Wave สปอน SiegeCreep ทุก 5 Wave (Wave 5, 10, 15...)
- [ ] Super Creep สปอนหลัง Barrack ทั้ง 2 ใน Lane พัง; ปรากฏร่วมกับ Wave ปกติ
- [ ] Lane Creep Chase Hero แล้ว Leash กลับเมื่อห่างเกิน 3 หน่วยจากจุดเริ่ม Chase
- [ ] Jungle Monster Group Aggro: ตีตัวนึงแล้วทั้งแคมป์รุม
- [ ] Jungle Leader ตาย → สมาชิกทั้งแคมป์ตายพร้อมกัน → Camp Respawn ตาม Timer
- [ ] Boss ใช้ Pattern AI (ไม่ใช่ Auto-Attack ธรรมดา); สลับ Attack/Skill ตาม Pattern
- [ ] Boss Kill → ทีม Killer ได้ Buff; Boss Respawn ตาม CooldownBoss
- [ ] Creep ใน Bush → Hero ศัตรูมองไม่เห็น (Fog of War)
- [ ] Creep ตาย → Body อยู่ 5 วินาที → Despawn (Sync ทุก Client)
- [ ] Leader ตาย → Follower แรกเป็น Leader ต่อทันที ไม่ติดค้าง
- [ ] Mind Control Creep → ไม่ทำงาน AI; คืนสู่ปกติเมื่อ Effect หมด

---

## Known Issues / TODO

- ⚠️ **Boss HP Reset on Leash**: ไม่ชัดเจนว่า Boss ฟื้น HP เต็มเมื่อ Leash กลับหรือไม่ — ต้องยืนยัน
- ⚠️ **Creep Base Stats**: ค่า HP/ATK/Armor ของแต่ละ Creep Type ยังไม่ได้ดึงจาก CBS Dashboard
- ⚠️ **Super Creep Stats**: ยังไม่มีตัวเลขเปรียบเทียบกับ Normal Creep
- ⚠️ **Boss Phase 2**: มีการ Spawn BossEndgame variant — ต้องเอกสารรายละเอียดพฤติกรรมและเงื่อนไข Trigger
- ⚠️ **Jungle Buff Values**: Blue Buff (AP Bonus) และ Red Buff (AD Bonus) ยังไม่ได้ระบุตัวเลขและระยะเวลา
