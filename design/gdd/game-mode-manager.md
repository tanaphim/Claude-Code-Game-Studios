# Game Mode Manager — Game Design Document

**System ID**: FT7
**Version**: 0.1.0
**Status**: Draft — Reverse-documented from source
**Last Updated**: 2026-04-02
**Dependencies**: Matchmaking (FT6), Map & Objectives (FT2), Networking Core (F2)

---

## 1. Overview

Game Mode Manager ควบคุม Lifecycle ของแมตช์ทั้งหมด ตั้งแต่ Lobby → Hero Select → Gameplay → Results ผ่าน State Machine Server-Authoritative รองรับ 9 โหมดใน 3 กลุ่ม (Findmatch, Custom, Dungeon) แต่ละโหมดมี `NetworkGameModeBase` Subclass ของตัวเอง ทุก State Transition ถูก Broadcast ผ่าน RPC ไปยังทุก Client

---

## 2. Player Fantasy

ผู้เล่นไม่รู้สึกถึงความซับซ้อนของ State Machine — เห็นแค่: รอคนเข้าห้อง → กด Ready → เลือก Hero → Loading → เกมเริ่ม การเปลี่ยนโหมดควรลื่นไหล ไม่มีการค้างหรือ Timeout แปลกๆ และจบเกมแล้วเห็น Scoreboard ชัดเจน

---

## 3. Detailed Rules

### 3.1 โหมดเกมทั้งหมด

| โหมด | กลุ่ม | ขนาดทีม | คำอธิบาย |
|------|-------|---------|---------|
| **Rank** | Findmatch | 5v5 | Competitive — มีผลต่อ RankPoint |
| **Casual** | Findmatch | 5v5 | ไม่มีผลต่อ Rank |
| **Casual2** | Findmatch | 5v5 | Casual Variant |
| **Midwar** | Findmatch | 5v5 | แมตช์เร็ว |
| **Arcade** | Custom | Variable | กฎพิเศษ |
| **Training** | Custom | Variable | ฝึกกับ Bot; ข้าม Timer |
| **Custom** | Custom | 1v1 ถึง 5v5 | ห้องส่วนตัว |
| **Tutorial** | Custom | — | สอนผู้เล่นใหม่ |
| **Dungeon** | Dungeon | 1–4 | PvE Co-op |
| **Town** | Custom | — | Social Hub |

---

### 3.2 Game State Machine

```
None
 └─ WaitForPlayerJoin       ← รอผู้เล่นเข้าห้อง
     └─ WaitForPlayerReady  ← ผู้เล่นกด Ready
         └─ PickupAvatar    ← เลือก Hero (มี Timer)
             └─ ReadyToPlay ← Pre-game countdown
                 └─ SyncBattleResource  ← โหลด Resource
                     └─ PreparingResources ← Init ระบบ
                         └─ PreparingBeforeGameStart ← เตรียมสุดท้าย
                             └─ Start            ← Gameplay Active
                                 └─ End          ← แสดง Results
                                     └─ Terminate ← Cleanup
```

**กฎ Transition**:
- ทุก Transition ควบคุมโดย **Server** เท่านั้น
- Broadcast ผ่าน `RPC_BroadcastStateChange()` ไปทุก Client
- แต่ละ State มี Timer จาก `CBSConfigBattle` (ยกเว้น Start ไม่มี Timeout)

---

### 3.3 Timer ต่อ State

| State | CBS Key | หมายเหตุ |
|-------|---------|---------|
| WaitForPlayerJoin | WaitForPlayerJoinTime | หมด → Terminate |
| WaitForPlayerReady | WaitForPlayerReadyTime | หมด → Terminate |
| PickupAvatar | PickupAvatarTime | หมด → Auto-fill Hero |
| ReadyToPlay | ReadyToPlayTime | Training: ÷2 (เร็วขึ้น) |
| SyncBattleResource | BattleResourceTime | หมด → Force Load |
| PreparingResources | PreparingResourcesTime | — |
| PreparingBeforeGameStart | PreparingBeforeGameStartTime | — |
| End | EndTime | หมด → Terminate |
| Start | ไม่มี Timeout | รอ Win Condition |

---

### 3.4 เงื่อนไขชนะ / จบเกม

| เงื่อนไข | กลไก |
|---------|------|
| **ทำลาย Core ศัตรู** | `SetWinner(opposingTeam)` ทันที |
| **Surrender** | โหวต ≥ (teamSize+1)/2 → หน่วง 2 วินาที → ฝ่ายตรงข้ามชนะ |
| **ผู้เล่นทุกคน Disconnect** | Server ตรวจจับ → Terminate |

**Surrender Vote**:
- Vote ใช้เวลา **10 วินาที**
- จำนวน Vote ที่ต้องการต่อทีม:

| ขนาดทีม | Vote ที่ต้องการ |
|---------|--------------|
| 1v1 | 1 |
| 2v2 | 2 |
| 3v3 | 2 |
| 4v4 | 3 |
| 5v5 | 3 |

---

### 3.5 ความแตกต่างระหว่างโหมด

**Training** (ต่างจากปกติ):
- ข้าม ReadyToPlay countdown (เร็วขึ้น 2x)
- Preload Object Pool ทั้งหมดก่อนเริ่ม
- UI พิเศษ `UITrainingMenuView` (Spawn Hero, Reset ฯลฯ)
- Max Bot Slots = 10

**Dungeon**:
- Max 4 ผู้เล่น (Co-op ไม่มี PvP)
- มีระบบ Equipment Loadout `SetupDungeonEquipment()`
- 3 ระดับความยาก: Easy / Medium / Hard
- Win Condition: ชนะ Objective ใน Dungeon (ไม่ใช่ Core)
- แผนที่ต่างกัน: `scene_dungeon_map.unity`

**Arcade / Midwar**:
- ⚠️ กฎเฉพาะยังไม่ได้เอกสาร — ต้องตรวจสอบ Server Config

---

### 3.6 สถิติแมตช์

**ระดับผู้เล่น (ต่อคน)**:

| Stat | คำอธิบาย |
|------|---------|
| Kill / Death / Assist | KDA พื้นฐาน |
| KillCreep | จำนวน CS (Creep Score) |
| Tower / Boss | Objective ที่มีส่วนร่วม |
| TotalDamageDealt | ความเสียหายรวมที่ทำ |
| TotalDamageTaken | ความเสียหายรวมที่รับ |
| HealOrShieldGiven | Support Contribution |
| GoldPerMinute | GPM |
| KDA Ratio | Kill / max(1, Death) |
| KillParticipation | % ของ Kill ทีมที่มีส่วน |

**สูตรคะแนน MVP**:
```
Score = (Kill×3 + Assist×2 - Death×2)
      + (Tower×2.5 + Boss×5)
      + (TotalDamageDealt/1000 + TotalDamageTaken/1000 + HealShield/1000)
```

**อัปเดต**: GPM ทุก 60 วินาที, Team Stats ทุก 10 วินาที

---

### 3.7 Scene Architecture

| Scene | ใช้เมื่อ |
|-------|---------|
| `scene_initial.unity` | Boot / Startup |
| `scene_home.unity` | Main Menu / Lobby |
| `scene_game_mode.unity` | Network Container (ทุก Session) |
| `scene_game_map.unity` | MOBA 5v5 Map |
| `scene_dungeon_map.unity` | Dungeon Map |

**โหลด Scene**: `LoadSceneAsync()` → รอ 90% → หน่วง 500ms → Activate

---

### 3.8 NetworkGameManager

ตัวจัดการหลักระดับ Network ดูแล:
- อ้างอิง `NetworkGameModeBase` ที่ Active อยู่
- Networked State: `GameState`, `GameStartTime`, `GameStateTimer`, `GameTimer`
- Parent Objects: Hero, Creep, Tower, Static, Skill
- Events: `EventStateChange`, `OnGameStateTimerChange`
- Win: `SetWinner(Team)` → `RPC_EndGame()` → State = End

---

## 4. Formulas

### Surrender Threshold
```
RequiredVotes = ceil((teamSize + 1) / 2)
= floor((teamSize + 1) / 2) + (teamSize % 2 == 0 ? 1 : 0)
```
ตัวอย่าง: 5v5 → (5+1)/2 = 3 votes

### MVP Score
```
Score = (K×3 + A×2 - D×2) + (Tower×2.5 + Boss×5) + ((DMGDealt + DMGTaken + Heal) / 1000)
```

---

## 5. Edge Cases

| สถานการณ์ | พฤติกรรม |
|-----------|---------|
| WaitForPlayerJoin หมด Timer | Terminate — ห้องถูกยุบ |
| ผู้เล่นออกระหว่าง PickupAvatar | Slot ว่าง; ถ้า Bot Fill → Bot รับ Slot |
| ทุกคนใน 1 ทีม Disconnect | Server ตรวจ → Terminate หรือ Win ทีมที่เหลือ |
| Surrender Vote ไม่ครบ 10 วินาที | Vote Clear; สามารถเริ่ม Vote ใหม่ได้ |
| Dungeon ผู้เล่น 1 คน Disconnect | 3 คนที่เหลือเล่นต่อได้ (Co-op ไม่มี Team 2) |
| Training ไม่มีผู้เล่นทีม 2 | Bot เต็มทีม 2 ได้ (Max 10 Bot Slots) |
| BossEndGameTime ครบ 60 วินาที | Endgame Phase Activate (แยกจาก State Machine) |

---

## 6. Dependencies

| ระบบ | ความสัมพันธ์ |
|------|------------|
| **Matchmaking (FT6)** | Matchmaking ส่ง Mode + Session ให้ Game Mode เริ่ม |
| **Map & Objectives (FT2)** | Tower.Core ถูกทำลาย → `SetWinner()` ถูกเรียก |
| **Networking Core (F2)** | RPC, NetworkGameManager, Photon Session |
| **Data-Config (F3)** | `CBSConfigBattle` กำหนด Timer ทุกตัว |
| **AI/Bot System (FT5)** | Bot ใน Training/Custom ต้องมี AI Controller |
| **HUD & UI (P1)** | State Change → UI ปรับตาม State (Lobby, HeroSelect, HUD, Scoreboard) |
| **Statistics (M5)** | CalculateGameStatistic() ทำงานตอน State = End |
| **Surrender System (M8)** | SurrenderSystem.cs → `SetWinner()` เมื่อ Vote ผ่าน |

---

## 7. Tuning Knobs

| ค่า | CBS Key | ค่าปัจจุบัน | ผลกระทบ |
|-----|---------|------------|---------|
| WaitForPlayerJoin Timer | WaitForPlayerJoinTime | CBS | รอ Join สูงสุด |
| WaitForPlayerReady Timer | WaitForPlayerReadyTime | CBS | รอ Ready สูงสุด |
| Hero Select Timer | PickupAvatarTime | CBS | เวลาเลือก Hero |
| Pre-game Countdown | ReadyToPlayTime | CBS | Loading Screen ก่อนเกม |
| Endgame Phase Trigger | BossEndGameTime | 60 วินาที | EndGame Warning |
| Surrender Vote Duration | SurrenderSystem | 10 วินาที | ระยะเวลา Vote |
| Post-Match Results | EndTime | CBS | นานแค่ไหนก่อน Terminate |
| Dungeon Difficulty | NetworkDungeonMode | Easy/Med/Hard | ความยาก PvE |

---

## 8. Acceptance Criteria

- [ ] State Machine เปลี่ยน State ตาม Timer ครบทุก Transition
- [ ] ทุก Client รับ State Change พร้อมกันผ่าน RPC
- [ ] Timer หมดใน PickupAvatar → Auto-fill Hero ทุกคนที่ยังไม่เลือก
- [ ] Core ถูกทำลาย → State → End ทันที; Winner ถูกต้อง
- [ ] Surrender: Vote ครบ threshold ภายใน 10 วินาที → 2 วินาที → ทีมตรงข้ามชนะ
- [ ] Dungeon Mode: Max 4 ผู้เล่น; Win Condition ต่างจาก MOBA
- [ ] Training Mode: ข้าม Countdown; Preload สำเร็จก่อน Start
- [ ] Post-match Scoreboard แสดง KDA, Damage, Gold, MVP ถูกต้อง
- [ ] Scene โหลด/Unload ไม่ Freeze (ใช้ Async)

---

## Known Issues / TODO

- ⚠️ **Pause System**: `GameState.Pause` มีในโค้ดแต่ยังไม่ Implement — ถ้าต้องการต้องออกแบบ Network Pause
- ⚠️ **Arcade/Midwar Rules**: กฎพิเศษยังไม่ได้ Document — ต้องดึงจาก Server Config
- ⚠️ **BossEndGameTime = 60 วินาที**: ยืนยันว่า Hardcode หรือมาจาก CBS?
- ⚠️ **Dungeon Win Condition**: ระบุว่า "ชนะ Objective" แต่ยังไม่ได้ระบุว่า Objective ใด
- ⚠️ **Town Mode**: เป็น Social Hub ไม่ใช่ Combat — ยังไม่ได้ Document กฎเฉพาะ
