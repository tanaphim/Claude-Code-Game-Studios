# Matchmaking System — Game Design Document

**System ID**: FT6
**Version**: 0.1.0
**Status**: Draft — Reverse-documented from source
**Last Updated**: 2026-04-02
**Dependencies**: Networking Core (F2), Hero System (C2), Data-Config System (F3)

---

## 1. Overview

ระบบ Matchmaking ใช้ PlayFab Multiplayer Matchmaking + Photon Fusion รองรับ 2 เส้นทางหลัก: **Findmatch** (Ranked/Casual — จับคู่อัตโนมัติ) และ **Custom Lobby** (สร้างห้องเอง) ขั้นตอนทุกอย่างตั้งแต่การสร้าง Ticket, จับคู่, เลือก Hero, จนถึงเริ่มเกมควบคุมโดย Server เป็น Authoritative

---

## 2. Player Fantasy

ผู้เล่นเปิดเกม กด "หาแมตช์" แล้วรอไม่นาน — ระบบหาคู่ที่ฝีมือใกล้กัน เข้าหน้าเลือก Hero ได้อย่างรวดเร็ว เกมเริ่มได้ภายในเวลาที่เหมาะสม ไม่รู้สึกว่ารอนาน ในโหมด Ranked รู้สึกว่าคู่ต่อสู้ท้าทายพอดี ไม่ถูกเบียดหรือเจอคนอ่อนมากเกินไป

---

## 3. Detailed Rules

### 3.1 โหมดเกมที่รองรับ

| โหมด | ประเภท | Queue Name |
|------|--------|-----------|
| **Rank** | Findmatch — Competitive | QUEUE_RANK |
| **Casual** | Findmatch — Unranked | QUEUE_CASUAL |
| **Casual2** | Findmatch — Variant | QUEUE_CASUAL2 |
| **Midwar** | Findmatch — Special | QUEUE_MIDWAR |
| **Arcade** | Findmatch — Arcade | QUEUE_ARCADE |
| **Training** | Findmatch | QUEUE_TRAINING |
| **Custom** | Custom Lobby | — |
| **Dungeon** | PvE Private | — |
| **Town** | Social Hub | — |

**ขนาดทีมที่รองรับ**: 1v1, 2v2, 3v3, 4v4, **5v5** (Default)

---

### 3.2 Findmatch Flow (Ranked/Casual)

```
1. เลือกโหมด → สร้าง Matchmaking Ticket (PlayFab)
2. Poll ทุก 10 วินาที เพื่อตรวจสถานะ Ticket
3. State: WaitingForPlayers → WaitingForMatch → WaitingForServer → Matched → Started
4. Match ถูกพบ → ดึง IP/Port/Session จาก GetMatch
5. เชื่อมต่อ Photon Fusion Session
6. เข้าสู่ Lobby → เลือก Hero → เกมเริ่ม
```

**Ticket Timeout**:

| โหมด | Timeout |
|------|---------|
| Casual / Training / Arcade | 120 วินาที |
| Ranked | 3,000 วินาที (~50 นาที) |

**MMR Matching Window**:
- RankScore ±100 คะแนน
- BehaviorScore ±50 คะแนน

---

### 3.3 Custom Lobby Flow

```
1. Host สร้างห้อง → กำหนด Mode, TeamSize, Password (Optional)
2. RequestServerForCustomMode() → PlayFab สร้าง Dedicated Server
3. ผู้เล่นเข้าร่วม Photon Room โดยตรง (ไม่มี Ticket)
4. Host กำหนด Slot ทีมด้วยตัวเอง
5. Host เพิ่ม/ลบ Bot ได้
6. Host กด Start → เข้าสู่ Hero Selection
```

**Room Properties**:
- `IsVisible`: ห้องสาธารณะหรือไม่
- `Password`: รหัสเข้าห้อง (Optional)
- `MaxPlayers`: ตาม TeamSize (สูงสุด 10 คน + 2 Spectator)

---

### 3.4 Team Assignment

- ผู้เล่นถูกจัดเข้า **Team1 ก่อน** จนครบ 5 คน → ล้นไป Team2
- ไม่มีระบบ Balance ทีมตาม Skill ใน Lobby
- Spectator ได้รับ Slot แยกต่างหาก (สูงสุด 2 คน)

**Slot Structure**:
```
Team1Slots[0..4] + Team2Slots[0..4] + SpectatorSlots[0..4]
แต่ละ Slot มี: ObjectName, PlayerRef, IsBot, Slot, Team, IsLeader
```

---

### 3.5 Bot Filling

- ถ้า Slot ว่าง → เพิ่ม Bot ได้ผ่าน `RPC_EnableBot()`
- Bot มี Avatar ID, Team, Name ("Bot0", "Bot1", ...)
- Host กำหนด Avatar ของ Bot ด้วย `RPC_AssignBot()`
- Bot สปอนเหมือน Player ปกติใน Gameplay

---

### 3.6 Game State Flow

| State | คำอธิบาย |
|-------|---------|
| **WaitForPlayerJoin** | รอผู้เล่นเข้าห้อง |
| **WaitForPlayerReady** | ผู้เล่นกด Ready |
| **PickupAvatar** | เลือก Hero (มี Timer) |
| **ReadyToPlay** | Server ยืนยัน + Auto-fill Hero ที่ไม่เลือก |
| **SyncBattleResource** | โหลด Resource |
| **PreparingResources** | Initialize ระบบ |
| **PreparingBeforeGameStart** | เตรียมก่อนเริ่ม |
| **Start** | เกมเริ่ม |

**เงื่อนไขเริ่มเกม**:
- Casual Findmatch: ผู้เล่นที่ไม่ใช่ Host ทุกคน Ready
- Custom: Host กด Start ได้ทันที
- Server เป็นผู้เปลี่ยน State เสมอ

---

### 3.7 Hero Selection

- **รูปแบบ**: Direct Pick (ไม่มี Ban Phase)
- **Duplicate**: ผู้เล่น 2 คนใน **ทีมเดียวกันเลือก Hero ซ้ำได้** (ไม่มี Lock)
- **Timeout**: กำหนดผ่าน `GameStateTimer` ของ Server
- **Auto-fill**: ถ้าไม่เลือกทัน → Server ใส่ `avatar_lancelot` ให้อัตโนมัติ
- หลัง Lock Hero → เลือก Skin ได้

---

### 3.8 Reconnect

- Session ข้อมูลถูก Cache ไว้ใน `CachedResumeData`
- ผู้เล่น Disconnect → Slot ยังคงอยู่ (ไม่ถูกลบทันที)
- Reconnect ได้ผ่าน `PlayerStatus.Resume` flag
- ถ้า Game เริ่มแล้ว (`GameStart == true`) → ผู้เล่นใหม่ที่ไม่ใช่การ Reconnect จะถูก Disconnect

---

### 3.9 Rank System

- **Stat ที่ใช้**: `RankPoint` (int) + `BehaviorScore`
- Ranked Ticket ใช้ `rankScore` ในการ Match
- Win/Loss บันทึกหลังแมตช์ผ่าน `CalculateGameStatistic()`
- `RankEXP` ให้หลัง Match เพื่อ Progression
- **Post-Match**: บันทึก KDA, Damage, ไอเทม, MVP ผ่าน `SaveMatchHistory()`

---

## 4. Formulas

### MMR Matching Range
```
MatchCondition:
  |playerMMR - candidateMMR| ≤ 100
  |playerBehavior - candidateBehavior| ≤ 50
```

### MaxPlayers
```
MaxPlayersTotal = TeamSize × 2
MaxPlayersIncSpectator = (TeamSize × 2) + SpectatorCount
```

### Matchmaking Poll Interval
```
ตรวจสถานะ Ticket ทุก 10 วินาที
Loop จนกว่า: Matched / Canceled / Timeout
```

---

## 5. Edge Cases

| สถานการณ์ | พฤติกรรม |
|-----------|---------|
| Ticket หมด Timeout | Matchmaking ถูก Cancel; ผู้เล่นกลับหน้าเลือกโหมด |
| ผู้เล่นออกระหว่าง Hero Selection | Slot ว่าง → Server อาจใส่ Bot |
| ไม่เลือก Hero ทันเวลา | Server Auto-fill `avatar_lancelot` |
| ทีมเดียวกันเลือก Hero ซ้ำ | อนุญาต (ไม่มีการป้องกัน) |
| Reconnect หลังเกมเริ่ม | ได้หากมี `CachedResumeData`; ถ้าไม่มี → Disconnect |
| ผู้เล่นใหม่พยายาม Join ห้องที่เกมเริ่มแล้ว | ถูก Disconnect ทันที |
| Custom Lobby มีแต่ Bot | ได้ — Host เปิดเกมกับ Bot ล้วนได้ |
| Spectator Slot เต็ม | ผู้เล่นไม่สามารถเข้าเป็น Spectator ได้ (Max 2) |

---

## 6. Dependencies

| ระบบ | ความสัมพันธ์ |
|------|------------|
| **Networking Core (F2)** | Photon Fusion Session, Room Management, RPC System |
| **Hero System (C2)** | รายชื่อ Hero ที่ใช้ในการ Pick; CBSUnit.IsEnable |
| **Data-Config System (F3)** | CBSUnit Hero Data, Build Settings (ShowAllHero) |
| **Game Mode Manager (FT7)** | Matchmaking ส่ง Mode ต่อให้ FT7 จัดการ Game Loop |
| **Account & Auth (M6)** | ต้องล็อกอินก่อน Matchmake; ข้อมูล RankPoint/BehaviorScore |
| **AI/Bot System (FT5)** | Bot ที่ใส่ใน Slot ต้องมี AI Controller |
| **Statistics (M5)** | Post-Match บันทึก KDA, ทอง, MVP |

---

## 7. Tuning Knobs

| ค่า | ที่อยู่ | ค่าปัจจุบัน | ผลกระทบ |
|-----|--------|------------|---------|
| Casual Ticket Timeout | PlayfabMatchmaking | 120 วินาที | รอคิวสั้นสุด |
| Ranked Ticket Timeout | PlayfabMatchmaking | 3,000 วินาที | รอคิวนานสุด |
| Poll Interval | PlayfabMatchmaking | 10 วินาที | ความเร็วตอบสนองคิว |
| MMR Match Window | Hardcode | ±100 | ความแม่นยำการจับคู่ |
| Behavior Score Window | Hardcode | ±50 | คุณภาพผู้เล่น |
| Max Team Size | CustomData | 5 ต่อทีม | ขนาดแมตช์ |
| Max Spectators | CustomData | 2 | ผู้ชม |
| Default Hero | Hardcode | avatar_lancelot | Hero Auto-fill |
| Hero Select Timer | NetworkGameManager.GameStateTimer | CBS | เวลาเลือก Hero |

---

## 8. Acceptance Criteria

- [ ] กด Casual Queue → Ticket ถูกสร้าง; Poll ทุก 10 วินาที; Cancel ได้
- [ ] Casual Ticket หมด 120 วินาที → กลับหน้าหลักพร้อม Error
- [ ] Ranked ใช้ RankScore ±100 จับคู่
- [ ] Match พบ → เชื่อมต่อ Photon Session อัตโนมัติ; เข้า Hero Selection
- [ ] ไม่เลือก Hero ทัน → ได้ `avatar_lancelot`; เกมเริ่มได้
- [ ] Custom Lobby: Host เพิ่ม/ลบ Bot ได้; Start เกมได้ทันที
- [ ] ผู้เล่น Disconnect ระหว่าง Lobby → Slot ว่าง; สามารถ Reconnect ได้
- [ ] เกมเริ่มแล้ว: ผู้เล่นใหม่เข้าไม่ได้; Reconnect เดิมได้
- [ ] Post-Match: KDA, ทอง, MVP บันทึกลง History

---

## Known Issues / TODO

- ⚠️ **No Ban Phase**: ปัจจุบันไม่มีระบบ Ban Hero — ถ้าต้องการเพิ่มต้องออกแบบใหม่
- ⚠️ **Duplicate Hero**: ผู้เล่น 2 คนเลือก Hero เดียวกันได้ — อาจต้องการ Lock ในอนาคต
- ⚠️ **BehaviorScore**: Code มีอยู่แต่ Implementation ถูก Comment Out — ยังไม่ Active
- ⚠️ **Rank Tier Names**: RankPoint มีอยู่แต่ยังไม่พบ Tier definition (Bronze/Silver/Gold ฯลฯ)
- ⚠️ **Hero Ownership Check**: `ShowAllHero` Flag ใน Build Settings — ยังไม่ชัดเจนว่า Production ใช้แบบไหน
