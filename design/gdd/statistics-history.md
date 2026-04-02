# Statistics & History — Game Design Document

**System ID**: M5
**Version**: 0.1.0
**Status**: Draft — Reverse-documented from source
**Last Updated**: 2026-04-02
**Dependencies**: Combat & Skills (C1), Gold Economy (C4), Level/XP (C5), Account & Auth (M6)

---

## 1. Overview

Statistics & History ติดตามสถิติผู้เล่นแบบถาวรผ่าน PlayFab Statistics API ครอบคลุม KDA, Win/Loss, MVP Count แยกตามโหมด (Casual/Ranked) และประวัติแมตช์แบบละเอียดรวม Items ที่ใช้, Damage, Gold แต่ละแมตช์ ข้อมูลโหลดพร้อม Login และ cache ใน `PlayerData`

---

## 2. Player Fantasy

ผู้เล่นดูสถิติของตัวเองได้ เห็น KDA ใน Rank/Casual แยกกัน, Win Rate, ประวัติแมตช์ล่าสุด และ Item Build ที่ใช้ในแต่ละเกม

---

## 3. Detailed Rules

### 3.1 Stat Keys (PlayFab Statistics)

| Key | คำอธิบาย |
|-----|---------|
| `Total_Casual_Matchs` | จำนวนแมตช์ Casual ทั้งหมด |
| `Total_Casual_Wins` | ชนะ Casual |
| `Total_Casual_Loses` | แพ้ Casual |
| `Total_Casual_MVP` | จำนวนครั้งได้ MVP ใน Casual |
| `Total_Rank_Matchs` | จำนวนแมตช์ Ranked |
| `Total_Rank_Wins` | ชนะ Ranked |
| `Total_Rank_Loses` | แพ้ Ranked |
| `Total_Rank_MVP` | MVP ใน Ranked |
| `Rank_Point` | MMR / Rank Point ปัจจุบัน |

---

### 3.2 Match History Data Model

**MatchHistory (Summary)**:
```
MatchID     : string
Time        : double (Unix timestamp)
Kills       : int
Deaths      : int
Assists     : int
KDA         : float (calculated)
```

**BattleHistory (Detailed)**:
```
AvatarId    : string (Hero ที่ใช้)
MatchId     : string
Mode        : string (game mode)
Teams       : List<PlayerTeam>
MatchDetail : object
```

**PlayerTeam**:
```
Team        : string
KDA         : KDA struct
Gold        : int (total team gold)
Players     : List<Player>
```

**Player (per-match)**:
```
PlayerName, AvatarId, Rank (position)
KDA         : { K, D, A }
Gold, GoldPerMinute
Damage, DamagePerMinute
Items       : List<Item> (build used)
KillStreaks, InhibitorDestroys, TowerDestroys
MinionKills, JungleKills
Wards       : { Used, Destroyed, Bought Control }
DamageTypes : { Physical, Magic, True } × { Dealt, Taken }
```

---

### 3.3 Load Flow

```
Login → PlayerData populated
  → PlayerData.GetMatchHistory(callback)
    → PlayFab (Azure Function) returns FunctionDeltaHistoryResult
    → PlayerData.MatchHistory = result

UIMatchHistoryInHome.Load()
  → loop MatchHistory.History (List<MatchHistory>)
  → Instantiate UIHistoryCard per match
  → card.SetUp(match) → display KDA, mode, result
```

---

### 3.4 Filter by Mode

- Dropdown: `0=All`, `1=Casual`, `2=Ranked`
- Calculate totals จาก PlayFab Statistics dictionary
- Win Rate = `winCount / (winCount + loseCount)` formatted as `"P0"` (percentage)

---

### 3.5 Stats Display

- `UIStats.cs` — Hero stats ละเอียด
- `UIStatsBar.cs` — Progress bar per stat
- `BattleHistoryOverview.cs` — Summary stats panel
- `UI_StatsChart.cs` — Chart visualization
- Level/XP bar: จาก `AccountLevelInfo` (previousExp, currentExp, nextLevelExp)

---

## 4. Formulas

### Win Rate
```
winRate = winCount / (winCount + loseCount)
        = 0 ถ้า (winCount + loseCount) == 0
Display: winRate.ToString("P0")  e.g., "67%"
```

### KDA
```
KDA = (Kills + Assists) / max(Deaths, 1)
```

---

## 5. Edge Cases

| สถานการณ์ | พฤติกรรม |
|-----------|---------|
| Deaths = 0 | KDA = (K+A) / 1 (ป้องกัน divide-by-zero) |
| ไม่มีประวัติแมตช์ | แสดง empty state (ไม่มี card) |
| Total Matches = 0 | Win Rate = 0% |
| Stat key ไม่พบ | `GetStatisticValue(key)` returns 0 |
| History โหลดล้มเหลว | ⚠️ ไม่พบ error handling ชัดเจน |

---

## 6. Dependencies

| ระบบ | ความสัมพันธ์ |
|------|------------|
| **Account & Auth (M6)** | PlayerData.Statistics, PlayerData.MatchHistory |
| **Combat & Skills (C1)** | KDA, Damage stats จาก Combat |
| **Gold Economy (C4)** | Gold, GoldPerMinute ใน Match History |
| **Level/XP (C5)** | AccountLevelInfo สำหรับ XP bar |
| **Battle Pass (M2)** | Battle Pass tier ดู AccountLevelInfo |

---

## 7. Tuning Knobs

| ค่า | ที่อยู่ | ผลกระทบ |
|-----|--------|---------|
| Stat Keys | PlayFab Title Data | ชื่อ stat ที่ track |
| History Record Count | Azure Function | จำนวน Match History ที่เก็บ |

---

## 8. Acceptance Criteria

- [ ] หน้า Stats แสดง Total Matches, Wins, Losses, Win Rate แยก Casual/Ranked
- [ ] MVP Count แสดงถูกต้องต่อโหมด
- [ ] Match History แสดงรายการแมตช์พร้อม KDA, Mode, Result
- [ ] Filter Dropdown (All/Casual/Ranked) กรอง History ถูกต้อง
- [ ] กดดู Match Detail เห็น Item Build, Damage, Gold ของทุกคน
- [ ] Win Rate คำนวณถูกต้อง; แสดงเป็น % (ทศนิยม 0 ตำแหน่ง)
- [ ] Deaths=0 → KDA ไม่ crash

---

## Known Issues / TODO

- ⚠️ **ไม่มี Real-time Stat Update**: Stat cache ณ เวลา Login — ต้อง Re-login เพื่อดูข้อมูลล่าสุด
- ⚠️ **History Error Handling**: ไม่พบ UI error state ถ้า history load ล้มเหลว
- ⚠️ **`Casual_Matchs` Key ซ้ำ**: มีทั้ง `Casual_Matchs` และ `Total_Casual_Matchs` — อาจเป็น legacy key
