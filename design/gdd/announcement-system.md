# Announcement System — Game Design Document

**System ID**: M10
**Version**: 0.1.0
**Status**: Draft — Reverse-documented from source
**Last Updated**: 2026-04-02
**Dependencies**: Combat & Skills (C1), Map & Objectives (FT2), Networking Core (F2), Audio System (F5)

---

## 1. Overview

Announcement System แสดงผลเหตุการณ์สำคัญในแมตช์ผ่านสองช่องทาง: **Announcement Banner** (กลางจอ, มีเสียง Announcer) และ **Kill Feed** (ด้านข้าง, แสดงประวัติ Kill) ระบบรับ `ActivityData` จาก Server แล้วจำแนก `AnnounceType` เพื่อสร้าง `KillFeedMessage` แล้ว Queue ไปแสดงผลผ่าน `UIAnnoucementView`

---

## 2. Player Fantasy

ผู้เล่นรับรู้เหตุการณ์สำคัญทันที — "First Blood!", "Triple Kill!", "Boss Defeated!" — โดยมีเสียง Announcer ประกอบ ทำให้ช่วงเวลา Highlight รู้สึกตื่นเต้น Kill Feed ด้านข้างให้ข้อมูลว่าใครฆ่าใครตลอดแมตช์

---

## 3. Detailed Rules

### 3.1 Announce Types

```csharp
enum AnnounceType {
    None, Execute, TowerDestroy, FirstBlood, General,
    BossKill, DragonKill, CreepSpawn, ItemCreepSpawn,
    MiniBossSpawn, BossSpawn
}
```

---

### 3.2 Kill Streak Announcements (KillStreak — reset เมื่อตาย)

| KillStreak | Announcement | Background | Sound |
|-----------|--------------|-----------|-------|
| 1 | "X Slain Y" | 0 (Normal) | "Slain" |
| 2 | "X Double Kill Y" | 1 (Double) | "Double Kill" |
| 3 | "X Triple Kill Y" | 2 (Triple) | "Triple Kill" |
| 4 | "X Quadra Kill Y" | 3 (Quadra) | "Quadra Kill" |
| 5+ | "X Annihilation Y" | 4 (Anni) | "Annihilation" |

---

### 3.3 Kill-per-Life Announcements (Killper1Life — ไม่ reset จนกว่าจะตาย)

| Killper1Life | Announcement | Sound |
|-------------|--------------|-------|
| 3 | "X Ultimate Y" | "Ultimate" |
| 4 | "X Onslaught Y" | "Onslaught" |
| 5–6 | "X Bloodbath Y" | "Bloodbath" |
| 7 | "X Legendary Kill Y" | "Legendary Kill" |
| 8–15 | "X Dominating Y" | "Dominating" |
| 16+ | "X Immortal Y" | "Immortal" |

---

### 3.4 Shut Down

- Trigger: ผู้โจมตีฆ่า victim ที่มี `Killper1Life ≥ 3`
- Announcement: "X SHUT DOWN Y"
- Background: 5 (Shut Down)
- Sound: "SHUT DOWN"

---

### 3.5 Objective Announcements

| AnnounceType | Announcement | Background | IsAnnounce | IsKillFeed | Sound |
|-------------|--------------|-----------|-----------|-----------|-------|
| FirstBlood | "X FIRST BLOOD Y" | 0 | ✅ | ✅ | "FIRST BLOOD" |
| TowerDestroy | "X DESTROY Y" | 0 | ❌ | ✅ | "DESTROY" |
| Execute | "X EXECUTED Y" | 0 | ❌ | ✅ | "EXECUTED" |
| BossKill | "BOSS DEFEATED" | 2 | ✅ | ❌ | "Announcer_Boss_Defeated" |
| DragonKill | "DRAGON SLAYED" | 2 | ✅ | ❌ | "Announcer_Boss_Defeated" |

---

### 3.6 Spawn Announcements (RPC Broadcast)

| AnnounceType | Message | IsAnnounce |
|-------------|---------|-----------|
| CreepSpawn | "SpawnCreep" | ✅ |
| ItemCreepSpawn | "ItemSpawnCreep" | ✅ |
| MiniBossSpawn | "SpawnMiniBoss" | ✅ |
| BossSpawn | "SpawnBoss" | ✅ |

---

### 3.7 Display Pipeline

```
Gameplay Event → ActivityData (Caster, Victim, AnnounceType)
  ↓
AnnouncementSystem.Announce(data, caster, victim)
  ↓ Route by AnnounceType
Create KillFeedMessage
  {KillBGNum, Message, SoundName, IsKillFeed, IsAnnounce, KillerName, VictimName}
  ↓
NetworkGameModeBase.WithUI<UIAnnoucementView>(ui => ui.OnGameAnnoucementMessageReceived(msg))
  ↓
UIAnnoucementView:
  IsAnnounce=true → aanounceQueue (enqueue)
  IsKillFeed=true → killfeedQueue (enqueue)
  ↓
IncommingAnnounce() coroutine:
  Dequeue → DisplayAnnouncement()
    → Spawn "ui_announcement" (pool)
    → Set text, killer/victim icons, background sprite
    → AudioManager.PlayAnnouncer(soundName)
    → ui.Play() [FadeIn 1s → Hold 2s → FadeOut 1s]
    → Wait 5s total → next announcement
  ↓
InCommingKillFeed() coroutine:
  Dequeue → DisplayKillfeed()
    → Spawn "ui_killfeed" (pool)
    → Set icons, team indicator (friendly/enemy color)
    → ui.Play() [FadeIn → Hold 0.5s → FadeOut]
    → Update UIDetailOfDeathView
```

---

### 3.8 Deduplication

- `m_KillFeedMessages`: Dictionary<ActorId, KillFeedMessage>
- Kill ซ้ำจากผู้โจมตีคนเดียวกัน: ครั้งแรก `IsKillFeed=true`, ครั้งต่อไป `IsKillFeed=false`
- ผู้โจมตีต่างคนกัน: ไม่ถูก deduplicate (แสดงได้ทั้งคู่)

---

### 3.9 Network Events

| RPC | Trigger | Visibility |
|-----|---------|-----------|
| `RPC_Announce` | Server → All | Spawn/Objective events |
| `RPC_Reconnected` | Server → All | Same-team เท่านั้น |
| `RPC_Disconnected` | Server → All | Same-team เท่านั้น |

---

### 3.10 Background Sprite Mapping

| KillBGNum | ใช้สำหรับ |
|----------|---------|
| 0 | Normal kill / First Blood / Executed / Destroyed |
| 1 | Double Kill |
| 2 | Triple Kill / Boss Killed / High Impact |
| 3 | Quadra Kill |
| 4 | Annihilation (5+ streak) |
| 5 | Shut Down |

---

## 4. Formulas

### Kill Display Duration
```
Announcement: fadeIn(1s) + hold(2s) + fadeOut(1s) = 4s per announcement
Queue interval: 5 วินาที ระหว่าง Announcement
Kill Feed: fadeIn + hold(0.5s) + fadeOut (instant queue — no delay)
```

---

## 5. Edge Cases

| สถานการณ์ | พฤติกรรม |
|-----------|---------|
| Kill streak 4 แล้ว kill อีก | ทับ Quadra ด้วย Annihilation |
| Killper1Life ต่ำกว่า 3 | ไม่มี Kill-per-life announcement |
| Victim.Killper1Life ≥ 3 แล้วถูกฆ่า | SHUT DOWN แทน Kill streak |
| Queue เต็มขณะ Announce กำลังแสดง | Queue ทำงาน one-by-one (ไม่ interrupt) |
| Icon ไม่ถูกโหลด (ItemModule miss) | ซ่อน icon อย่างเงียบ (null check) |
| Reconnect/Disconnect | แสดงเฉพาะทีมเดียวกัน |

---

## 6. Dependencies

| ระบบ | ความสัมพันธ์ |
|------|------------|
| **Combat & Skills (C1)** | ส่ง ActivityData เมื่อ Kill / Execute |
| **Map & Objectives (FT2)** | ส่ง ActivityData เมื่อ Tower / Boss ถูกทำลาย |
| **Networking Core (F2)** | RPC Broadcast ไปยัง All clients |
| **Audio System (F5)** | `PlayAnnouncer(soundName)` สำหรับทุก Announcement |
| **HUD & UI (P1)** | `UIAnnoucementView` อยู่ใน HUD hierarchy |

---

## 7. Tuning Knobs

| ค่า | ที่อยู่ | ค่าปัจจุบัน | ผลกระทบ |
|-----|--------|------------|---------|
| Announcement hold time | UIAnnouncementObject.displayTime | **2s** | เวลาค้างบนจอ |
| Announcement fade in/out | UIAnnouncementObject | **1s each** | ความนุ่มนวล fade |
| Announcement queue interval | IncommingAnnounce() | **5s** | ระยะห่างระหว่าง banner |
| Kill feed display time | UIKillFeedObject.displayTime | **0.5s** | เวลาค้างของ kill feed |
| Kill streak thresholds | AnnouncementSystem | ดูตาราง §3.2 | เปลี่ยน milestone |
| Kill-per-life thresholds | AnnouncementSystem | ดูตาราง §3.3 | เปลี่ยน milestone |
| Shut down threshold | AnnouncementSystem | **Killper1Life ≥ 3** | ขั้นต่ำก่อน Shut Down |

---

## 8. Acceptance Criteria

- [ ] First Blood แสดง Banner + เล่นเสียง "FIRST BLOOD" เพียงครั้งเดียวต่อแมตช์
- [ ] Kill Streak 1–5+ แสดง Banner + เสียงที่ถูกต้อง
- [ ] Kill-per-life 3, 4, 5-6, 7, 8-15, 16+ แสดง Banner ที่ถูกต้อง
- [ ] Shut Down แสดงเมื่อฆ่า Victim ที่ Killper1Life ≥ 3
- [ ] Tower Destroy / Boss Killed แสดงใน Kill Feed / Banner ตามลำดับ
- [ ] Announcement Banner ปรากฏที่กลางจอ Fade in/out นุ่มนวล
- [ ] Kill Feed ด้านข้างอัปเดต Real-time
- [ ] Spawn Announcements (Boss/MiniBoss) broadcast ถึงทุกคน
- [ ] Reconnect/Disconnect message แสดงเฉพาะทีมเดียวกัน

---

## Known Issues / TODO

- ⚠️ **Typo**: `UIAnnoucementView` (ควรเป็น AnnouncementView) — consistent throughout codebase
- ⚠️ **Typo**: `anouncetext` (ควรเป็น announcementText)
- ⚠️ **RPC NetworkRunner**: `RPC_Reconnected/Disconnected` รับ `NetworkRunner` parameter แต่ไม่ใช้
- ⚠️ **Spectator ใน Reconnect**: filter แค่ team — ไม่ handle spectator edge case
- ⚠️ **Hard-coded display times**: displayTime, fadeIn, fadeOut ไม่ configurable จาก CBS/Config
