# Social System — Game Design Document

**System ID**: M1
**Version**: 0.1.0
**Status**: Draft — Reverse-documented from source
**Last Updated**: 2026-04-02
**Dependencies**: Networking Core (F2), Account & Auth (M6), Data/Config System (F4)

---

## 1. Overview

Social System จัดการ Friend List, Chat (Lobby + In-Game), Invite, Block และ Online Status ผ่าน CBS FriendsModule และ CBS ChatModule โดย Real-time events ส่งผ่าน SignalR service (`SignalRService`) รองรับทั้ง Group Chat (ห้อง) และ Private Chat (1-to-1)

---

## 2. Player Fantasy

ผู้เล่นเห็นเพื่อนออนไลน์ได้ทันที ชวนเข้าห้องได้ด้วยคลิกเดียว แชทกับเพื่อนระหว่างรอแมตช์ และบล็อกคนที่ไม่ต้องการได้

---

## 3. Detailed Rules

### 3.1 Architecture

```
SignalRService (Real-time events)
  ├── OnReceivedFriendRequest
  └── OnReceivedInviteGame(customData, inviterData)

CBS Modules:
  ├── CBSFriendsModule  — Friends CRUD
  └── CBSChatModule     — Chat (group + private)

DeltaModule (Custom):
  ├── InvitePlayerToLobby(DeltaInvitePlayerToLobbyRequest)
  └── SendBlockRequest(profileID)
```

---

### 3.2 Friend List

**โหลด Friends**:
```
LoadFriendList()
  → CBSFriendsModule.GetFriends(CBSProfileConstraints, callback)
    Constraints: LoadAvatar=true, LoadLevel=true, LoadOnlineStatus=true,
                 LoadStatistics=true, LoadProfileData=true
  → แบ่ง list เป็น Online / Offline
  → โหลด Pending Requests: GetRequestedFriends()
```

**Friend Status**:
```csharp
Dictionary<string, UIFriendProfile> m_Friends  // PlayFabId → UIFriendProfile
OnlineStatus.IsOnline                          // bool
friend.Statistics["Casual_Matchs"]            // match stats
```

**Add Friend**:
```
ค้นหาชื่อ → Mnemonic.ToPlayFabId(name)
  → FriendsModule.SendFriendsRequest(profileID, callback)
  → แสดง success/error panel
```

**Remove Friend**:
```
FriendsModule.RemoveFriend(PlayFabId, callback)
```

---

### 3.3 Chat System

| ประเภท | Channel | Poll Rate | History |
|--------|---------|-----------|---------|
| Lobby Group Chat | Group Room | 500ms | 100 messages |
| In-game Team Chat | Group Room | 500ms | 100 messages |
| Private Chat (1-to-1) | Per-friend | 500ms | 100 messages |

**Chat Instance**:
```
ActiveChat: ChatInstance             // Current active group chat
ActiveChatPrivates: Dictionary<string, ChatInstance>  // Per-friend private chats
MaxMessagesToLoad = 100
UpdateIntervalMilliseconds = 500f
```

**Private Chat Flow**:
```
Friend กด Chat → m_ChatWindow.CreateFriendChat(friend)
  → CBSChat.GetOrCreatePrivateChatWithProfile(friendID)
  → Load history → แสดงใน RadiusChatInGameWindow
```

**In-game Chat**: แสดงผ่าน `UIChatView` มี channel toggle (All / Team)

---

### 3.4 Invite System

**ส่ง Invite**:
```
กด Invite Friend → UIInvitePartyFriend
  → DetlaModule.InvitePlayerToLobby(DeltaInvitePlayerToLobbyRequest)
    { SessionInfo: CustomData JSON, InviterData: {PlayerID, Name, RankPoint, AvatarID} }
  → ส่งผ่าน SignalR
```

**รับ Invite** (บน Recipient):
```
SignalRService.OnReceivedInviteGame fires
  → UIInvitePanel / UIInviteDialogPanel แสดง
  → Invite เก็บใน InviteCollections (List<InviteInfo>)
  → กด Accept → OnTryToJoinSession(CustomData)
```

---

### 3.5 Block System

```
กด Block ใน Friend Detail
  → DetlaModule.SendBlockRequest(profileID, callback)
  → อัปเดต UIFriendOverviewBlockList
```

---

### 3.6 Online Status

- `LoadOnlineStatus=true` ใน `CBSProfileConstraints` — โหลดพร้อม Friend List
- แสดงเป็น Online/Offline indicator ใน `UIFriendProfile`
- ไม่มี real-time status update — อัปเดตเมื่อ Refresh Friend List

---

## 4. Formulas

ไม่มี Formula — ระบบเป็น API call และ UI state ล้วน

---

## 5. Edge Cases

| สถานการณ์ | พฤติกรรม |
|-----------|---------|
| Add Friend ตัวเอง | ⚠️ ยังไม่พบ validation |
| Friend Request ซ้ำ | CBS จัดการ duplicate |
| Invite ห้องเต็ม | `IsRoomFull=true` → ปฏิเสธ |
| Block ขณะเป็น Friend | ⚠️ ไม่ชัดเจนว่า Remove Friend ด้วยหรือไม่ |
| Chat offline friend | Private chat history ยังดูได้ |
| SignalR disconnect | ⚠️ ไม่พบ reconnect logic สำหรับ SignalR |

---

## 6. Dependencies

| ระบบ | ความสัมพันธ์ |
|------|------------|
| **Account & Auth (M6)** | PlayFab ProfileID, PlayerData |
| **Networking Core (F2)** | SignalR real-time events |
| **Notification System (M4)** | SignalR events trigger notifications |
| **Menu & Lobby UI (P4)** | Invite flow เชื่อมกับ Lobby join |

---

## 7. Tuning Knobs

| ค่า | ที่อยู่ | ค่าปัจจุบัน | ผลกระทบ |
|-----|--------|------------|---------|
| Chat History Size | ChatService | **100 messages** | ประวัติ Chat โหลด |
| Chat Poll Rate | ChatService | **500ms** | ความถี่อัปเดต Chat |
| Friend Constraints | CBSProfileConstraints | LoadAll=true | ข้อมูลที่โหลดพร้อม Friend |

---

## 8. Acceptance Criteria

- [ ] Friend List แสดง Online/Offline แยกกัน
- [ ] เพิ่มเพื่อนด้วยชื่อผู้เล่น; รับ/ปฏิเสธ Request ได้
- [ ] ลบเพื่อนได้
- [ ] Group Chat ใน Lobby ทำงาน; อัปเดตทุก 500ms
- [ ] Private Chat กับเพื่อนทำงาน; ประวัติ 100 messages
- [ ] In-game Chat มี All/Team channel toggle
- [ ] Invite เพื่อนเข้า Lobby; ผู้รับเห็น Dialog
- [ ] Block ผู้เล่นได้; ปรากฏใน Block List
- [ ] New friend request trigger SignalR notification

---

## Known Issues / TODO

- ⚠️ **Online Status ไม่ Real-time**: แสดงสถานะ ณ เวลาโหลด — ไม่อัปเดตอัตโนมัติ
- ⚠️ **Block ≠ Remove Friend อัตโนมัติ**: ไม่ชัดว่า Block ลบ Friend ด้วยหรือไม่
- ⚠️ **SignalR Reconnect**: ไม่พบ logic reconnect ถ้า SignalR หลุด
- ⚠️ **Self-Add Validation**: ไม่พบ guard สำหรับการเพิ่มตัวเองเป็นเพื่อน
