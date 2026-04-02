# Networking Core — Game Design Document

**System ID**: F2
**Version**: 0.1.0
**Status**: Draft — Reverse-documented from source
**Last Updated**: 2026-04-02
**Dependencies**: Actor System (F1), Data-Config System (F3)

---

## 1. Overview

ระบบ Networking ใช้ **Photon Fusion 2** เป็น Framework หลัก ทำงานร่วมกับ **PlayFab Multiplayer Server** สำหรับ Dedicated Server Architecture แบบ Server-Authoritative ทุก Game Logic รันบน Server ส่วน Client รับผิดชอบเฉพาะการส่ง Input และรับ State Update ผ่าน [Networked] Properties และ RPC

---

## 2. Player Fantasy

ผู้เล่นไม่รู้สึกถึง Latency — การกดทักษะและการเคลื่อนที่ต้องรู้สึก Responsive แม้มี Ping สูง เมื่อหลุดการเชื่อมต่อระบบพยายาม Reconnect อัตโนมัติ ไม่ให้ผู้เล่นเสียสิทธิ์ที่ไม่ใช่ความผิดของตัวเอง

---

## 3. Detailed Rules

### 3.1 สถาปัตยกรรม Network

| ส่วน | รายละเอียด |
|------|-----------|
| **Framework** | Photon Fusion 2.x |
| **Architecture** | Server-Authoritative + Client Input |
| **Server Type** | Dedicated Server (PlayFab Multiplayer Agent) |
| **GameMode ที่ใช้** | `GameMode.Server` (Dedicated) / `GameMode.Host` (Peer-hosted) / `GameMode.Client` |
| **Simulation** | Fixed Tick-based Deterministic |

---

### 3.2 Connection Flow

```
1. PlayFab Authentication (CBS Login)
2. สร้าง NetworkRunner Instance
3. JoinSessionLobby(SessionLobby.Shared, lobbyName)
4. StartGame(StartGameArgs) → Photon Cloud
5. OnSessionStarted → Server spawns NetworkLobbyManager
6. ผู้เล่น Join → AssignPlayerSlot()
7. Hero Select → SyncBattleResource → Start
```

**Session Name Format**: `ssid_{ShortUID}`

---

### 3.3 InputMessage (INetworkInput)

Input ของผู้เล่นถูกส่งทุก Tick ผ่าน Struct:

```
InputMessage:
  NetworkButtons Buttons      ← ปุ่มที่กด (Bitmask)
  Vector3Compressed Position  ← ตำแหน่งคลิกในโลก
  NetworkBehaviourId Target   ← เป้าหมาย (ถ้าคลิกที่หน่วย)
  Vector2Compressed MouseScreenPosition
```

**Buttons Enum** (10 ปุ่ม):

| Button | การกระทำ |
|--------|---------|
| LeftClick | โจมตี / ใช้ทักษะที่เป้า |
| RightClick | เดิน / โจมตีอัตโนมัติ |
| A | A-click Attack Move |
| Q | Skill Q |
| W | Skill W |
| E | Skill E |
| R | Skill R (Ultimate) |
| S | Stop |
| B | ซื้อไอเทม (Base) |
| Recall | เรียก Recall |

**Input Collection**:
1. ตรวจว่า Controller Active และ `GameState == Start`
2. Raycast เพื่อหา Ground Position (m_GroundMask)
3. Raycast เพื่อหา Target Actor (m_ClickMask)
4. ส่งผ่าน Fusion `OnInput()` callback ทุก Tick

---

### 3.4 Networked State

**[Networked] Properties** หลักต่อ Player:

| Property | ประเภท | คำอธิบาย |
|----------|--------|---------|
| PlayfabId | NetworkString<_32> | ID ผู้เล่น |
| PlayerName | NetworkString<_32> | ชื่อ |
| Team | Team | ทีม |
| IsReady | NetworkBool | สถานะ Ready |
| AvatarId | NetworkString<_32> | Hero ที่เลือก |
| SkinId | NetworkString<_32> | Skin ที่เลือก |
| SceneLoaded | NetworkBool | โหลด Scene เสร็จ |
| Gold | int | ทองปัจจุบัน |
| Items | NetworkArray<NetworkString<_32>>[7] | Inventory |

**Change Detection**: `ChangeDetector.Source.SimulationState`

---

### 3.5 RPC Pattern

Fusion ใช้ `[Rpc]` attribute:

```csharp
// Broadcast ทุก Client
[Rpc] RPC_BroadcastStateChange(GameState state)

// ส่งหา Client เฉพาะ
[Rpc] RPC_TargetStateChange([RpcTarget] PlayerRef target, GameState state)
```

**Reliable Data (ทางเลือก RPC)**:
- Client → Server: `Runner.SendReliableDataToServer(key, data)`
- Server → Client: `Runner.SendReliableDataToPlayer(target, key, data)`
- ใช้สำหรับข้อมูลที่ต้องการการันตีว่าถึง (ไม่ใช่ Input)

---

### 3.6 Network Authority

| Object | Authority | กฎ |
|--------|-----------|---|
| NetworkGameManager | Server | Spawn + ควบคุม Game State |
| NetworkLobbyManager | Server | Spawn โดย Server เท่านั้น |
| Game Systems (Tower, Creep ฯลฯ) | Server | `if (IsServer) OnTick()` |
| Player Avatar | Server-owned + Client Input | Client ส่ง Input, Server Execute |
| NetworkGameObjectBehavior | Per-player (HasInputAuthority) | Player ควบคุม Object ของตัวเอง |

---

### 3.7 Disconnect & Reconnect

**Disconnect**:
- `NetworkRunner.CloudConnectionLost` → Trigger Reconnect หรือ Abort
- Server ตรวจ `OnPlayerLeft()` → Slot ว่าง

**Reconnect Flow**:
1. Session Data ถูก Cache ไว้ใน PersistentStorage
2. `TryReconnect()` → ตรวจสอบ Cache
3. `Reconnect()` → สร้าง Runner ใหม่ด้วย `GameMode.Client`
4. Join Session เดิมด้วย Session Name + Connection Token
5. หาก Cloud ยังไม่ Ready → รอด้วย `WaitUntil(() => runner.IsCloudReady)`
6. หาก Reconnect ล้มเหลว → `Abandon()` ล้าง Cache กลับ Lobby

**Connection Token**: บันทึก/โหลด Token สำหรับยืนยัน Identity เมื่อ Reconnect

---

### 3.8 PlayFab Server Integration

Server-side เท่านั้น (`#if RADIUS_SERVER`):

```
PlayFabMultiplayerAgent.Start()
→ ReadyForPlayers()
→ OnServerActive callback
  → GetGameServerConnectionInfo() → IP/Port
  → GetConfigSettings() → Match metadata
  → สร้าง CustomData → StartGame()
```

**Metadata ที่ Server อ่าน**:
- `match_type`: ประเภทแมตช์
- `mode_selection_type`: โหมดเกม
- `SessionCookieKey`: ข้อมูล Custom Match
- `PublicIpV4AddressKey`, `ServerListeningPort`: Connection info

---

### 3.9 Session States

```
BeforeJoinSession → BeforeCreateOrJoin → Created → Join
→ Start → (Gameplay) → End → Shutdown
```

กรณีพิเศษ: `Kick`, `Disband`, `Leave`, `Full`, `Cancel`, `NotAvailable`

---

## 4. Formulas

### Tick-based Delay
```
TicksForDelay(seconds) = ceil(seconds × runner.TickRate)
ตัวอย่าง: 0.2s × 60 tps = 12 ticks
```

### Max Players
```
MaxPlayersTotal = (TeamSize × 2) + SpectatorCount
```

---

## 5. Edge Cases

| สถานการณ์ | พฤติกรรม |
|-----------|---------|
| Client หลุดระหว่าง Hero Select | Slot ว่าง; Server Auto-fill Bot หรือรอ Reconnect |
| Client หลุดระหว่าง Gameplay | Cache Session; ลอง Reconnect อัตโนมัติ |
| Reconnect ล้มเหลว | `Abandon()` → ล้าง Cache → กลับ Lobby |
| Server ถูก Shutdown (PlayFab) | `OnShutdown` callback → ปิด Session อย่าง Graceful |
| PlayFab Maintenance | `OnMaintenance` callback → แจ้งผู้เล่น |
| Session เต็ม (`IsFull`) | ผู้เล่นใหม่ไม่สามารถ Join ได้ |
| Input ในขณะ Chat เปิด | Input Collection ถูก Skip |

---

## 6. Dependencies

| ระบบ | ความสัมพันธ์ |
|------|------------|
| **Actor System (F1)** | Actor ทุกตัวเป็น NetworkObject; ใช้ [Networked] สำหรับ Stats |
| **Data-Config System (F3)** | CBSConfigBattle กำหนด Timer; MetadataService ใช้ใน Auth |
| **Matchmaking (FT6)** | PlayFab Matchmaking → Session Name → Photon Connection |
| **Game Mode Manager (FT7)** | NetworkGameManager ควบคุม GameState Machine |
| **Input System (F3)** | PlayerGameplayInput → NetworkRunnerInput → InputMessage |
| **ระบบอื่นๆ ทั้งหมด** | ทุกระบบ Networked ขึ้นอยู่กับ F2 |

---

## 7. Tuning Knobs

| ค่า | ที่อยู่ | ค่าปัจจุบัน | ผลกระทบ |
|-----|--------|------------|---------|
| Tick Rate | Fusion Project Settings | Config-based | Simulation Precision vs Bandwidth |
| Session Player Limit | CustomData.GetMaxPlayersIncSpectator | TeamSize × 2 + Spectator | Room Capacity |
| Reconnect Timeout | WaitUntil(IsCloudReady) | — | รอ Cloud ได้นานแค่ไหน |
| Reliable Data Key | ReliableKey pattern | Per-message | ป้องกัน Duplicate |
| GameMode | StartGameArgs.GameMode | Config | Server/Host/Client |
| Region | PhotonAppSettings.FixedRegion | Config | Latency |

---

## 8. Acceptance Criteria

- [ ] Client เชื่อมต่อ Session สำเร็จหลัง PlayFab Auth
- [ ] Input ทุก Tick ถูกส่งและ Execute บน Server
- [ ] `[Networked]` Properties Sync ทุก Client ทันที
- [ ] RPC Broadcast ถึงทุก Client ใน Session
- [ ] Client Disconnect → Cache Session → Reconnect สำเร็จ
- [ ] Reconnect ล้มเหลว → กลับ Lobby ไม่ Crash
- [ ] PlayFab Server Agent ทำงานได้ (ReadyForPlayers, OnServerActive)
- [ ] Session เต็ม → ผู้เล่นใหม่ถูก Reject

---

## Known Issues / TODO

- ⚠️ **Tick Rate ไม่ได้ระบุตัวเลข**: ใช้ `runner.TickRate` แต่ไม่พบค่า Default ใน Config — ต้องตรวจ Fusion Project Settings
- ⚠️ **Client-side Prediction**: ไม่พบ Implementation ชัดเจน — Relying on Fusion built-in interpolation
- ⚠️ **Lag Compensation**: ไม่มี Explicit Lag Compensation นอกจาก Tick-based timing
- ⚠️ **Spectator Sync**: Spectator สามารถเห็นทั้ง 2 ทีม — ต้องตรวจสอบว่า AOI ถูก Bypass สำหรับ Spectator อย่างไร
