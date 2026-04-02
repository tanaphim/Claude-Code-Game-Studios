# AFK Detection — Game Design Document

**System ID**: M7
**Version**: 0.1.0
**Status**: Draft — Reverse-documented from source
**Last Updated**: 2026-04-02
**Dependencies**: Actor System (F1), Networking Core (F2), Input System (F3), AI/Bot System (FT5)

---

## 1. Overview

AFK Detection ตรวจจับผู้เล่นที่ไม่ได้ใช้ Input โดย `PlayerActivityTracker` (static) เก็บ `LastActivityTime` และ `ActorAFKHandle` (per-actor component) ตรวจสอบ Idle Time ทุก Network Tick เมื่อถึง Warning Threshold (180s) แสดง UI เตือน เมื่อถึง Kick Threshold (300s) ตัดการเชื่อมต่อและแทนที่ด้วย Bot โดย `AFKSystem` ค่า Threshold โหลดจาก `CBSConfigBattle`

---

## 2. Player Fantasy

ผู้เล่นคนอื่นไม่ต้องเล่นขาดคนเพราะเพื่อนร่วมทีม AFK — Bot จะเข้าแทนที่โดยอัตโนมัติ ผู้เล่นที่กลับมาได้รับการเตือนล่วงหน้าก่อนถูก Kick

---

## 3. Detailed Rules

### 3.1 Input Tracking

```
PlayerActivityTracker (static)
  LastActivityTime : float  (Time.time ณ Activity ล่าสุด)
  IdleTime         : float  = Time.time - LastActivityTime
  RecordActivity() : เรียกจาก Input handlers ทุกครั้งที่มี Input
  ResetTracker()   : เรียกเมื่อเกมเริ่ม (reset เป็น -1)
```

**Inputs ที่ trigger RecordActivity()**:
- Movement click
- Skill cast
- Item usage

ระบบ track เฉพาะ `GameState == Start`

---

### 3.2 AFK Thresholds (จาก CBSConfigBattle)

| Threshold | ค่า | พฤติกรรม |
|-----------|-----|---------|
| `AFKWarningTime` | **180 วินาที** (3 นาที) | แสดง Warning UI |
| `AFKKickTime` | **300 วินาที** (5 นาที) | Disconnect + Bot replacement |

ค่าโหลดจาก `CBSConfigBattle` metadata (Server Config)

---

### 3.3 Detection Loop (ActorAFKHandle.OnFixedUpdateNetwork)

```
ทุก Network Tick (เฉพาะ Local Player ที่มี Input Authority):
  idleTime = PlayerActivityTracker.IdleTime

  ถ้า idleTime >= DISCONNECT_THRESHOLD (300s):
    DisconnectPlayer()
      → m_HasDisconnected = true
      → ShowDisconnected() UI
      → RPC: Invoke_SetAFKStatus(isAFK=true, isDisconnect=true)

  ถ้า idleTime >= WARNING_THRESHOLD (180s) AND NOT m_HasShownWarning:
    ShowAFKWarning()
      → m_HasShownWarning = true
      → ShowWarning() UI + countdown
      → RPC: Invoke_SetAFKStatus(isAFK=true, isDisconnect=false)

  ถ้า idleTime < 1s AND m_HasShownWarning:
    HideAFKWarning()
      → m_HasShownWarning = false
      → HideWarning() UI
      → RPC: Invoke_SetAFKStatus(isAFK=false, isDisconnect=false)
```

---

### 3.4 Warning UI (UIWarningAndDisconnectedView)

| State | UI แสดง | ปุ่ม |
|-------|---------|------|
| Warning (180s–299s) | `m_WarningDisconnected` panel + countdown | — |
| Disconnected (300s+) | `m_Disconnected` panel | "Disconnected" → ออกเกม |
| Resume activity | ซ่อน Warning panel | — |

**Countdown Format**: `"MM:ss"` (ผ่าน `SetText(countdown)`)

---

### 3.5 Bot Replacement Flow (AFKSystem)

```
RPC: Invoke_SetAFKStatus(isAFK=true, isDisconnect=true)
  → ActorAvatar.IsAFK = true
  → AFKSystem.SetActiveBot(actor)
    → ถ้า IsAFK == true:
        AddComponent<BotActor>() ถ้าไม่มี
        bot.Setup(actor, ObjectiveLane.Mid)
    → ถ้า IsAFK == false:
        RemoveComponent<BotActor>() ถ้ามี
        Disable auto-attack
```

---

### 3.6 RPC Payload

```csharp
InputNetworkMessage {
    ActionId  = ScriptRefference.Avatar
    ParamBool1 = isAFK           // AFK status
    ParamBool2 = isDisconnect    // Trigger bot replacement
    MethodName = "Invoke_SetAFKStatus"
}
```

---

## 4. Formulas

```
IdleTime = Time.time - PlayerActivityTracker.LastActivityTime

ShowWarning  ← IdleTime >= AFKWarningTime  (180s)
Disconnect   ← IdleTime >= AFKKickTime     (300s)
HideWarning  ← IdleTime < 1s (player resumed)
```

---

## 5. Edge Cases

| สถานการณ์ | พฤติกรรม |
|-----------|---------|
| ผู้เล่น resume ที่ 299s | HideWarning(); รอต่อโดยไม่มี cooldown |
| Bot replace แล้ว player reconnect | ⚠️ RPC_PlayerResume stub — ยังไม่ implement |
| `EnableAFK = false` (Build Setting) | ระบบ AFK ทั้งหมดถูก skip |
| GameState ≠ Start | ไม่ track Input / ไม่ trigger AFK |
| Spectator | ไม่ได้รับผลจาก AFK (ตาม RPC check) |

---

## 6. Dependencies

| ระบบ | ความสัมพันธ์ |
|------|------------|
| **Input System (F3)** | `RecordActivity()` เรียกจาก Input handlers |
| **Actor System (F1)** | `ActorAFKHandle` component บน Actor |
| **AI/Bot System (FT5)** | `BotActor.Setup()` แทนที่ผู้เล่น AFK |
| **Networking Core (F2)** | RPC `Invoke_SetAFKStatus` |
| **Data/Config (F4)** | `CBSConfigBattle.AFKWarningTime/AFKKickTime` |

---

## 7. Tuning Knobs

| ค่า | ที่อยู่ | ค่าปัจจุบัน | ผลกระทบ |
|-----|--------|------------|---------|
| AFKWarningTime | CBSConfigBattle | **180 วินาที** | เวลาก่อนเตือน |
| AFKKickTime | CBSConfigBattle | **300 วินาที** | เวลาก่อน Kick + Bot |
| EnableAFK | BuildSettings | true | เปิด/ปิดทั้งระบบ |
| Resume threshold | ActorAFKHandle | **1 วินาที** | Input ต่ำกว่าค่านี้ถือว่า Active |

---

## 8. Acceptance Criteria

- [ ] ผู้เล่นไม่ใช้ Input 180s → Warning UI + countdown แสดง
- [ ] Countdown format "MM:ss" แสดงเวลาที่เหลือก่อน Kick
- [ ] ผู้เล่นกลับมา Input ก่อน 300s → Warning ซ่อน
- [ ] ผู้เล่นไม่ใช้ Input 300s → Disconnected UI + ออกเกม
- [ ] Bot replace ผู้เล่น AFK ที่ถูก Kick ทันที
- [ ] Bot ถูก assign ให้กับ Lane ของผู้เล่น (ไม่ hard-code Mid)
- [ ] `EnableAFK = false` → ระบบไม่ทำงาน
- [ ] ค่า Threshold โหลดจาก CBSConfigBattle (ไม่ hard-code)

---

## Known Issues / TODO

- ⚠️ **RPC Stubs ว่าง**: `RPC_PlayerAFK`, `RPC_PlayerDisconnected`, `RPC_PlayerResume` เป็น empty methods
- ⚠️ **Bot Lane Hard-coded**: `bot.Setup(actor, ObjectiveLane.Mid)` — ควรใช้ Lane จริงของผู้เล่น
- ⚠️ **Reconnect ไม่ complete**: ผู้เล่นที่ reconnect หลัง Bot แทนที่ — ยังไม่มี logic คืน control
- ⚠️ **ไม่มี AFK Penalty**: ไม่มี suspension / cooldown สำหรับผู้เล่น AFK ซ้ำๆ
- ⚠️ **No Cooldown หลัง Resume**: ผู้เล่นสามารถ AFK แล้ว resume แล้ว AFK ใหม่ได้ไม่จำกัด
