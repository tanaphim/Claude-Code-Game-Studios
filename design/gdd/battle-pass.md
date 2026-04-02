# Battle Pass — Game Design Document

**System ID**: M2
**Version**: 0.1.0
**Status**: Draft — Reverse-documented from source
**Last Updated**: 2026-04-02
**Dependencies**: Data/Config System (F4), Account & Auth (M6), Statistics & History (M5)

---

## 1. Overview

Battle Pass เป็นระบบ Seasonal Progression ที่แบ่งรางวัลเป็น Free (Default) Track และ Premium Track ผู้เล่นได้รับ EXP จากการเล่นเกมและทำ Quest เพื่อเลื่อน Tier และรับรางวัล บริหารผ่าน CBS `CBSBattlePassModule` รองรับ Bank Rewards, Ticket system, Task/Quest และ Extra Levels

---

## 2. Player Fantasy

ผู้เล่นมีเป้าหมายระยะยาวที่ชัดเจนในแต่ละ Season — เห็นรางวัลล่วงหน้าทุก Tier, ทำ Quest รายวัน/รายสัปดาห์สะสม EXP, และรู้สึกคุ้มค่าเมื่อ Unlock ของ Premium หลังซื้อ Pass

---

## 3. Detailed Rules

### 3.1 UI Layout

```
UIBattlePassWindow
  ├── Tab: LEVELS    — รายการ Tier พร้อม Free/Premium rewards
  ├── Tab: TASKS     — Quest/Mission list (ถ้า TasksEnabled)
  ├── Tab: BANK      — Bank rewards side track (ถ้า BankEnabled)
  └── Tab: TICKETS   — Tickets ที่มี
```

---

### 3.2 Level / Progression

| Field | คำอธิบาย |
|-------|---------|
| PlayerLevel | Tier ปัจจุบันของผู้เล่น |
| ExpOfCurrentLevel | EXP ที่สะสมใน Tier นี้ |
| ExpStep | EXP ที่ต้องการต่อ 1 Tier |
| EndDate | DateTime? วันหมดอายุ Season |
| IsActive | Season เปิดอยู่หรือไม่ |
| PremiumAccess | ผู้เล่นซื้อ Premium หรือไม่ |
| ExtraLevelAccess | มีสิทธิ์รับ Extra Level rewards |

---

### 3.3 Reward Types

| ประเภท | รายละเอียด |
|--------|-----------|
| DEFAULT | Free Track — ทุกคนรับได้ |
| PREMIUM | Premium Track — ต้องซื้อ Pass |

**Reward Unlock Logic**:
```
Free Reward พร้อม Claim ถ้า:
  ProfilePassLevel >= LevelIndex
  AND NOT TimeLocked
  AND IsActive
  AND NOT ExtraLevelLocked (ถ้าเป็น Extra Level)

Premium Reward พร้อม Claim ถ้า:
  ProfilePassLevel >= LevelIndex
  AND PremiumAccess = true
  AND IsActive
  AND NOT TimeLocked
  AND NOT ExtraLevelLocked
```

---

### 3.4 Reward Collection Flow

```
กด Claim (Free) → GrantDefaultReward()
  → CBSBattlePassModule.GrantAwardToProfile(BattlePassID, LevelIndex, isPremium=false, callback)
  → Update BattlePassLevelInfo.IsDefaultRewardCollected = true
  → Re-render slot

กด Claim (Premium) → GrantPremiumReward()
  → CBSBattlePassModule.GrantAwardToProfile(..., isPremium=true, callback)
  → Update IsCollectedPremiumReward = true
```

---

### 3.5 Quest / Task System

- `UIBattlePassTaskUI` แสดง Quest แต่ละตัว (inherits `BaseTaskUI<CBSProfileTask>`)
- Task มี: ID, LockLevel, LevelFilter, Progress
- **Add Progress**: `CBSBattlePassModule.AddBattlePassTaskPoints()`
- **Claim Task Reward**: `CBSBattlePassModule.PickupBattlePassTaskReward()`
- แสดงเฉพาะถ้า `BattlePassInstance.TasksEnabled = true`

---

### 3.6 Ticket System

- Ticket เป็น item ที่ช่วยเลื่อน Level หรือ bypass
- `UITicketScroller` / `UITicketSlot` แสดงจำนวน Ticket ที่มี
- แสดงเฉพาะใน Tab TICKETS

---

### 3.7 Extra Levels

- `BattlePassLevel.IsExtraLevel = true` สำหรับ Level นอกเหนือ Main track
- ต้องการ `ExtraLevelAccess = true` ถึงรับรางวัล
- แสดงด้วย `ExtraLineColor` ใน UI

---

### 3.8 Level UI Width

```
ExpandWidthPerReward = 190f   // ความกว้าง card ขยายตามจำนวน reward
```

---

## 4. Formulas

### EXP Progress
```
ProgressRatio = ExpOfCurrentLevel / ExpStep   (0.0 – 1.0)
```

### Time Display
```
BattlePassUtils.GetFrameTimeLabel(endDate)    → countdown string
BattlePassUtils.GetRewardLimitLabel(timerDate) → reward deadline string
```

---

## 5. Edge Cases

| สถานการณ์ | พฤติกรรม |
|-----------|---------|
| Season หมดอายุ | `IsActive=false` — Claim ไม่ได้ |
| Reward มี Time Limit | `GetLimitDate()` ตรวจก่อน Claim |
| Extra Level ไม่มี Access | Slot แสดงแต่ Claim ไม่ได้ |
| Claim ไปแล้ว | `IsRewardCollected=true` — ปุ่ม Claim ซ่อน/disabled |
| Free player กด Premium reward | ปุ่ม Premium disabled (PremiumAccess=false) |
| Task ที่ LockLevel > PlayerLevel | Task แสดง lock state |

---

## 6. Dependencies

| ระบบ | ความสัมพันธ์ |
|------|------------|
| **Account & Auth (M6)** | PlayerData, CBS Token สำหรับ API calls |
| **Data/Config (F4)** | CBS Config สำหรับ Season ID, ExpStep, RewardTable |
| **Statistics (M5)** | AccountLevelInfo, Stats สำหรับ Task progress |

---

## 7. Tuning Knobs

| ค่า | ที่อยู่ | ผลกระทบ |
|-----|--------|---------|
| ExpStep | CBS BattlePass Config | EXP ที่ต้องการต่อ Tier |
| Season EndDate | CBS BattlePass Config | วันหมด Season |
| TasksEnabled | BattlePassInstance | เปิด/ปิด Quest system |
| BankEnabled | BattlePassInstance | เปิด/ปิด Bank rewards |
| Premium Price | CBS Store | ราคา Premium Pass |
| ExpandWidthPerReward | UIBattlePassLevelSlot | ขนาด UI card |

---

## 8. Acceptance Criteria

- [ ] Level/Tier แสดงพร้อม Free และ Premium rewards เคียงกัน
- [ ] Free rewards Claim ได้ถ้า PlayerLevel ถึง Tier นั้น
- [ ] Premium rewards Claim ได้เฉพาะ Premium player
- [ ] EXP Progress bar แสดงสัดส่วนถูกต้อง
- [ ] Season countdown แสดงเวลาที่เหลือ
- [ ] Quest Tab แสดง task พร้อม progress; Claim reward ได้
- [ ] Ticket Tab แสดงจำนวน Ticket ที่มี
- [ ] Reward ที่ Claim แล้วไม่แสดงปุ่ม Claim อีกครั้ง
- [ ] Extra Level ต้องการ ExtraLevelAccess

---

## Known Issues / TODO

- ⚠️ **Bank Rewards**: UI พร้อม แต่ logic ยังไม่ชัดเจนว่า Bank reward Unlock เมื่อไหร่
- ⚠️ **Ticket Usage**: ยังไม่พบ logic ใช้ Ticket เพื่อเลื่อน Level
- ⚠️ **Season End Behavior**: ไม่ชัดว่า Unclaimed rewards หายหรือ grace period หลัง EndDate
