# Surrender System — Game Design Document

**System ID**: M8
**Version**: 0.1.0
**Status**: Draft — Reverse-documented from source
**Last Updated**: 2026-04-02
**Dependencies**: Networking Core (F2), Game Mode Manager (FT7)

---

## 1. Overview

ระบบ Surrender ให้ทีมใดทีมหนึ่งโหวตยอมแพ้ระหว่างแมตช์ ผ่าน `SurrenderSystem` (NetworkBehaviour) ที่ sync state ผ่าน Photon Fusion เมื่อผู้เล่น initiate, RPC broadcast UI Vote Panel ไปยังสมาชิกในทีม ผู้เล่นกด Agree/Reject ภายใน 10 วินาที ถ้าเสียงโหวต "ใช่" ถึง `(teamSize + 1) / 2` เกม Server กำหนด Winner แล้วจบแมตช์

---

## 2. Player Fantasy

ทีมที่ตามหลังมากสามารถจบเกมได้อย่างสุภาพโดยไม่ต้องรอถูก destroy Core ผู้เล่นทุกคนในทีมมีสิทธิ์โหวต และเห็นสถานะโหวตของเพื่อนร่วมทีม Real-time

---

## 3. Detailed Rules

### 3.1 Initiation

- ผู้เล่นกดปุ่ม Surrender → `SetSurrenderOpen(teamIndex)` ถูกเรียก
  - `0` = Team 1 (Blue) เปิดโหวต
  - `1` = Team 2 (Red) เปิดโหวต
- ตรวจสอบ: ยังไม่มีโหวตเปิดอยู่ (`IsSurrenderBlueVoteOpenning` / `IsSurrenderRedVoteOpenning`)
- **ไม่มีเงื่อนไขเวลาขั้นต่ำ** ก่อน Surrender ⚠️ (ยังไม่ implement)

---

### 3.2 Vote Window

```
SetSurrenderOpen(team)
  → RPC_SendSurrenderUI(team)  [StateAuthority → All]
    → Spawn UI Panel ("ui_surrender_blue" หรือ "ui_surrender_red")
    → Set IsSurrenderBlueVoteOpenning = true
    → VotetimeCount(team) [10 วินาที countdown]
```

- UI Panel แสดงเฉพาะผู้เล่นในทีมที่โหวต (ตรวจ team ใน RPC)
- ตำแหน่ง Panel: hard-coded ที่ (910, 440, 0)

---

### 3.3 Voting

| ค่า IsSurrender | ความหมาย |
|----------------|---------|
| 0 | ยังไม่โหวต |
| 1 | โหวต Yes (Surrender) |
| 2 | โหวต No (Reject) |

- กด **Agree** → `m_self.SetSurrender(1)` → Networked property update
- กด **Reject** → `m_self.SetSurrender(2)`
- ปุ่มถูก Disable หลังโหวต (ป้องกัน double-vote)
- สมาชิกทีมแต่ละคนเห็น Vote Box ของเพื่อนอัปเดต Real-time

---

### 3.4 Vote Count Check

`SurrenderSystem.CheckSurrenderBoolCount()` ทำงาน async:
```
foreach player in GameMode.PlayerInstances:
  ถ้า IsSurrender == 1 → นับโหวต Yes ตาม Team

voteRequirement = (teamSize + 1) / 2   // majority, rounded up
ถ้า Team2.yesVotes >= voteRequirement:
  winner = Team1  (Team2 surrender → Team1 ชนะ)
ถ้า Team1.yesVotes >= voteRequirement:
  winner = Team2

ถ้าถึง requirement:
  IsSurrenderVoted = true
  await 2 วินาที
  NetworkGameManager.Instance.SetWinner(winner)
```

---

### 3.5 Timer & Expiry

- `VotetimeCount(team)`: รอ 10 วินาที
- ถ้าหมดเวลา:
  - Reset flag (`IsSurrenderBlueVoteOpenning = false`)
  - Clear vote lists
  - UI Panel ปิดตัว (`Clear()`)
- ผู้เล่นที่โหวตแล้วถูก Reset: `m_self.SetSurrender(0)` เมื่อ UI ปิด

---

### 3.6 Timer UI

| เวลาเหลือ | สี indicator |
|----------|-------------|
| ≥ 5 วินาที | Circle เขียว |
| < 5 วินาที | Circle แดง |

fillAmount = `m_Timer / 10f` (0.0–1.0)

---

### 3.7 Surrender Outcome

```
SetWinner(losingTeam) → GameMode จัดการ End Game flow
  → GameState = End
  → ผู้ชนะ/แพ้ Screen แสดง
```

---

## 4. Formulas

### Vote Requirement
```
voteRequirement = (teamSize + 1) / 2   // Integer division

ตัวอย่าง:
  2 ผู้เล่น → requirement = 1.5 → 1 (เกินครึ่ง)
  3 ผู้เล่น → requirement = 2
  4 ผู้เล่น → requirement = 2.5 → 2
  5 ผู้เล่น → requirement = 3
```

### Timer Display
```
fillAmount = m_Timer / 10f
colorThreshold = 5f  (เขียว ↔ แดง)
```

---

## 5. Edge Cases

| สถานการณ์ | พฤติกรรม |
|-----------|---------|
| Timer หมดก่อนครบโหวต | โหวตยกเลิก, Flags reset, สามารถ initiate ใหม่ได้ |
| `IsSurrenderVoted = true` แล้ว | CheckSurrenderBoolCount() return ทันที |
| `GameState == End` | UI Clear ทันที (ไม่รอ 2 วินาที) |
| ผู้เล่น Disconnect ระหว่างโหวต | vote ของเขาไม่ถูกนับ (ไม่อยู่ใน PlayerInstances) |
| โหวต Reject > 50% | ⚠️ Code มีอยู่แต่ comment out — ยังไม่ทำงาน |
| Spectator | ถูก check ใน RPC — ไม่แสดง UI |

---

## 6. Dependencies

| ระบบ | ความสัมพันธ์ |
|------|------------|
| **Networking Core (F2)** | Photon Fusion RPC + [Networked] properties |
| **Game Mode Manager (FT7)** | `SetWinner()`, `GameState`, `PlayerInstances` |
| **Actor System (F1)** | `NetworkGameObjectBehavior` — `IsSurrender` property |

---

## 7. Tuning Knobs

| ค่า | ที่อยู่ | ค่าปัจจุบัน | ผลกระทบ |
|-----|--------|------------|---------|
| Vote Window | VotetimeCount() | **10 วินาที** | เวลาให้โหวต |
| Post-surrender delay | CheckSurrenderBoolCount() | **2 วินาที** | หน่วงก่อนจบเกม |
| Vote requirement | CheckSurrenderBoolCount() | **(teamSize+1)/2** | เสียงข้างมาก |
| Timer color threshold | UISurrenderView | **5 วินาที** | จุดเปลี่ยน เขียว→แดง |
| UI position | RPC_SendSurrenderUI | **(910, 440, 0)** | ตำแหน่ง Panel |
| Min game time | ยังไม่ implement | — | เวลาขั้นต่ำก่อน Surrender |

---

## 8. Acceptance Criteria

- [ ] ผู้เล่นในทีมเปิด Vote Panel ได้
- [ ] UI แสดงเฉพาะสมาชิกทีมเดียวกัน
- [ ] Timer นับถอยหลัง 10 วินาที; เขียว → แดงเมื่อ < 5 วินาที
- [ ] กด Agree → `IsSurrender = 1`; กด Reject → `IsSurrender = 2`
- [ ] ปุ่มถูก Disable หลังโหวต
- [ ] Vote Box แสดง Agree/Reject ของสมาชิกแต่ละคน Real-time
- [ ] เมื่อ Yes votes ≥ `(teamSize+1)/2` → เกมจบ, ทีมชนะถูกกำหนด
- [ ] Timer หมด → Vote ยกเลิก, Flags reset
- [ ] ไม่สามารถโหวตซ้ำใน Window เดิม

---

## Known Issues / TODO

- ⚠️ **ไม่มีเวลาขั้นต่ำ**: สามารถ Surrender ได้ตั้งแต่เกมเริ่ม — ควรเพิ่ม minimum game time (เช่น 10 นาที)
- ⚠️ **Reject Vote Logic Incomplete**: Code ตรวจ reject votes ≥ 50% มีอยู่แต่ถูก comment out
- ⚠️ **m_VotedPlayerBlue/Red Lists**: Lists ถูก populate แต่ไม่ถูกใช้ใน `CheckSurrenderBoolCount()` — iterate `PlayerInstances` แทน (อาจเป็น dead code)
- ⚠️ **Button Listener Pattern**: `RemoveListener + AddListener` ทุก frame บน `OnEnable` — inefficient
- ⚠️ **UI Position Hard-coded**: (910, 440, 0) ไม่รองรับ resolution ต่างๆ
