# Menu & Lobby UI — Game Design Document

**System ID**: P4
**Version**: 0.1.0
**Status**: Draft — Reverse-documented from source
**Last Updated**: 2026-04-02
**Dependencies**: Matchmaking (FT6), Social System (M1), Data/Config System (F4)

---

## 1. Overview

Menu & Lobby UI คือหน้าจอหลักนอกแมตช์ ผู้เล่นเข้าถึงโหมดเกม, ห้อง Lobby, การตั้งค่า, และโปรไฟล์ผ่าน `HomeUI` ซึ่งเป็น Hub กลาง ระบบ UI ใช้ Scene Context pattern (`SceneContext.UI.Open<T>()`) เปิด/ปิด View แต่ละส่วน รองรับ Find Match และ Custom Room

---

## 2. Player Fantasy

ผู้เล่นเห็นหน้าจอหลักที่สะอาด เลือกโหมดที่ต้องการได้ทันที เห็นเพื่อนออนไลน์ รับ Invite ได้ และปรับตั้งค่าได้โดยไม่ต้องออกจาก Lobby

---

## 3. Detailed Rules

### 3.1 Scene Architecture

```
UIMenuView (Entry Point)
  ↓
HomeUI (Hub — manages all home views)
  ├── UIHomeWidgetView   — Game mode selection buttons
  ├── UISessionFindmatchView — Find Match flow
  ├── UIFriendPanel      — Friends list (Social)
  ├── UILoadingDialogView — Loading transitions
  └── UIErrorDialogView  — Error messages
```

---

### 3.2 Game Modes

| Mode | Type | MatchType |
|------|------|-----------|
| Ranked | PvP 5v5 | Findmatch |
| Casual | PvP 5v5 | Findmatch |
| Arcade | PvP variant | Findmatch |
| Training | Practice | Findmatch |
| Custom | Custom Room | Custom |
| Dungeon | PvE Co-op | (Dungeon flow) |
| Town | Social Hub | (Town flow) |

---

### 3.3 Session Creation & Join Flow

**Find Match**:
```
Player selects mode → UISessionFindmatchView
  → NetworkConnectionService.CreateLobby(CustomData)
  → Matchmaking queue (FT6)
```

**Custom Room**:
```
Player creates/joins custom → CustomData built
  → RunnerManager.CreateOrJoinAsync(CustomData)
  → Directly enter session
```

**CustomData Struct**:
```
MatchType   : Findmatch | Custom
GameMode    : Host | Client
SessionName : string
IsPartyFull : bool (capacity check for Find Match party)
IsRoomFull  : bool (capacity check for Custom)
MatchName   : string
```

---

### 3.4 Invite Acceptance Flow

```
OnReceivedInviteGame(customData, inviterData)  [SignalR event]
  ↓
UIInvitePanel แสดง Invite Dialog
  ↓
OnTryToJoinSession(CustomData)
  → ตรวจ IsPartyFull / IsRoomFull
  ↓ ถ้า OK
RunnerManager.CreateOrJoinAsync(CustomData)
```

**InviteInfo**:
```
SessionInfo  : JSON (CustomData)
Invities     : InviterData
  ├── PlayerID
  ├── PlayerName
  ├── PlayerRankPoint
  └── PlayerAvatar (AvatarID)
```

---

### 3.5 Loading & Error Dialogs

- **UILoadingDialogView**: เปิดระหว่าง Scene transition / session init
- **UIErrorDialogView**: แสดง error message; ปิดเองหรือให้ผู้เล่นกด dismiss

---

## 4. Formulas

ไม่มี Formula — ระบบเป็น UI State Machine ล้วน

---

## 5. Edge Cases

| สถานการณ์ | พฤติกรรม |
|-----------|---------|
| Join ห้องที่เต็ม | `IsRoomFull=true` → ปฏิเสธ + แสดง error |
| Invite ขณะอยู่ใน Match | ⚠️ ยังไม่พบ logic handle |
| Network ขาดระหว่าง Lobby | UIErrorDialog แสดง |
| PlayerName < 4 ตัวอักษร | Login button disabled (จาก UILoginView) |

---

## 6. Dependencies

| ระบบ | ความสัมพันธ์ |
|------|------------|
| **Matchmaking (FT6)** | Find Match queue เริ่มจาก Lobby |
| **Social System (M1)** | UIFriendPanel, Invite via SignalR |
| **Networking Core (F2)** | RunnerManager, NetworkConnectionService |
| **Account & Auth (M6)** | PlayerData.PlayerName, Avatar |

---

## 7. Tuning Knobs

| ค่า | ที่อยู่ | ผลกระทบ |
|-----|--------|---------|
| Mode availability | UIHomeWidgetView | โหมดใดเปิด/ปิด |
| Party/Room capacity | CustomData.IsPartyFull/IsRoomFull | ขีดจำกัดคนในห้อง |

---

## 8. Acceptance Criteria

- [ ] เลือกโหมดเกมทั้ง 7 ได้จากหน้าหลัก
- [ ] Find Match สร้าง Lobby และเข้า Matchmaking queue
- [ ] Custom Room สร้าง/เข้าร่วมห้องได้
- [ ] Invite จาก Friend แสดง Dialog; กด Accept เข้าห้องได้
- [ ] ห้องเต็ม → ปฏิเสธพร้อม error message
- [ ] Loading Dialog แสดงระหว่าง transition
- [ ] Error Dialog แสดงเมื่อ network/session ล้มเหลว

---

## Known Issues / TODO

- ⚠️ **Map ID Loading ไม่สมบูรณ์**: `// TODO SET MAP ID` comment ใน code
- ⚠️ **Player Profile Icon ไม่ Load**: `// TODO Load player profile` comment ใน code
- ⚠️ **Invite ระหว่างแมตช์**: ยังไม่พบ logic handle Invite ขณะอยู่ใน Game Session
