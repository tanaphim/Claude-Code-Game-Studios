# Hero Select UI — Game Design Document

**System ID**: P3
**Version**: 0.1.0
**Status**: Draft — Reverse-documented from source
**Last Updated**: 2026-04-02
**Dependencies**: Hero System (C2), Matchmaking (FT6), Game Mode Manager (FT7)

---

## 1. Overview

Hero Select UI เป็นหน้าจอเลือก Hero ก่อนเริ่มแมตช์ ผู้เล่นเลือก Hero จาก Grid, ดู Detail, แล้ว Lock In — จากนั้นเลือก Skin เพิ่มเติมได้ มี Timer นับถอยหลังจาก Server Config หาก Lock In ไม่ทันระบบ Auto-lock ให้ ไม่มีระบบ Ban Phase

---

## 2. Player Fantasy

ผู้เล่นเห็น Hero ที่ตัวเองชอบในหน้าจอที่เรียบง่าย เลือกได้เร็ว เห็นทีมของตัวเองและฝ่ายตรงข้ามเรียงกัน และยังมีเวลาเลือก Skin ที่ชอบก่อนเกมเริ่ม ผู้เล่นใหม่ดู Lane Filter แล้วเลือก Hero ตาม Role ที่ถนัดได้

---

## 3. Detailed Rules

### 3.1 Layout หน้าจอ

```
[ ทีม 1 (Lineup) ]    [ Hero Grid / Skin Panel ]    [ ทีม 2 (Lineup) ]
                         [ Timer Countdown ]
                         [ Ready / Lock In Button ]
```

- **ทีม 1**: `m_Team1Content` — Hero Icon ของ Ally เรียงตาม Slot
- **ทีม 2**: `m_Team2Content` — Hero Icon ของ Enemy เรียงตาม Slot
- **Format**: `{count}VS{count}` (เช่น "5VS5")

---

### 3.2 Hero Grid

- แสดงใน `m_HeroContent` — Grid Layout
- แต่ละการ์ด Hero แสดง:
  - ไอคอน Hero
  - ชื่อ Hero
  - Lane / Role (จาก `CBSLane`)
  - สถานะ Enabled / Greyed Out (Hero ที่ไม่ได้ Unlock จะ Greyed Out)
- กดการ์ด → แสดง Hero Detail Panel ตรงกลาง
- กดซ้ำหรือกด Lock In → ยืนยันการเลือก

---

### 3.3 Timer

- นับถอยหลังจาก `CBSConfigBattle.PickupAvatarTime` (Server Config)
- แสดงเป็นตัวเลขนับถอยหลัง
- เมื่อหมดเวลา → ระบบ Auto-lock Hero ที่ Highlight อยู่ → เข้าสู่ `ReadyToPlay` State

---

### 3.4 Lock In และ Skin Selection

**ขั้นตอน**:
1. ผู้เล่นเลือก Hero → กด Lock In
2. Hero Panel ถูก Deactivate
3. Skin Panel ถูก Activate → ผู้เล่นเลือก Skin ที่ Unlock แล้ว
4. เลือก Skin แบบ **Carousel Swipe** (`UISwipeTest` component)
5. Confirm → เข้าเกม

**หมายเหตุ**: Skin Selection เป็น Phase หลัง Lock In เท่านั้น — ไม่สามารถเปลี่ยน Hero ได้หลัง Lock In

---

### 3.5 Ban Phase

**ไม่มี** — ระบบ Ban Phase ยังไม่ได้ Implement

---

### 3.6 Auto-lock

- หาก Timer หมดก่อนที่ผู้เล่นจะ Lock In → ระบบเลือก Hero ที่ Highlighted ปัจจุบันให้อัตโนมัติ
- หากไม่มี Hero ที่ Highlighted → อาจเลือก Hero Default (⚠️ ต้องตรวจสอบ)
- หลัง Auto-lock → เข้าสู่ `ReadyToPlay` State ทันที

---

## 4. Formulas

### Timer
```
PickupTime = CBSConfigBattle.PickupAvatarTime  (Server-defined)
DisplayTimer = PickupTime - ElapsedTime
AutoLock = (DisplayTimer <= 0) → LockCurrentSelection()
```

---

## 5. Edge Cases

| สถานการณ์ | พฤติกรรม |
|-----------|---------|
| Timer หมด ก่อน Lock | Auto-lock Hero ที่ Highlight อยู่ |
| Hero ที่ไม่ได้ Unlock | Greyed Out — กดไม่ได้ |
| ผู้เล่นออกกลางคัน | ⚠️ ยังไม่พบ Logic Handle Disconnect ใน Hero Select |
| ทั้ง 2 ทีมเลือก Hero เดียวกัน | Duplicates อนุญาต (ไม่มี Exclusive Lock) |

---

## 6. Dependencies

| ระบบ | ความสัมพันธ์ |
|------|------------|
| **Hero System (C2)** | ข้อมูล Hero, Lane, Skin Ownership |
| **Matchmaking (FT6)** | ส่งผู้เล่นเข้าสู่ Hero Select หลัง Queue Found |
| **Game Mode Manager (FT7)** | GameState: HeroSelect → ReadyToPlay |
| **Data-Config System (F3)** | `CBSConfigBattle.PickupAvatarTime` |

---

## 7. Tuning Knobs

| ค่า | ที่อยู่ | ผลกระทบ |
|-----|--------|---------|
| Pick Timer | CBSConfigBattle.PickupAvatarTime | เวลาเลือก Hero |
| Hero Availability | CBS (Hero Ownership) | Hero ใดที่เล่นได้ |
| Skin Availability | CBS (Skin Ownership) | Skin ใดที่เลือกได้ |

---

## 8. Acceptance Criteria

- [ ] Grid แสดง Hero ทั้งหมด; Hero ที่ไม่ได้ Unlock จะ Greyed Out
- [ ] กด Hero → Detail Panel แสดง
- [ ] Lock In → Hero Panel ปิด; Skin Panel เปิด
- [ ] Skin Carousel Swipe ใช้งานได้
- [ ] Timer นับถอยหลังจาก CBSConfigBattle.PickupAvatarTime
- [ ] Timer หมด → Auto-lock อัตโนมัติ
- [ ] Lineup ทั้ง 2 ทีมอัปเดตแบบ Real-time เมื่อผู้เล่นแต่ละคน Lock In
- [ ] หลัง Lock In ทุกคน → เข้าเกม

---

## Known Issues / TODO

- ⚠️ **ไม่มี Ban Phase**: ถ้าต้องการเพิ่มในอนาคตต้องออกแบบ Flow และ Timer แยก
- ⚠️ **Auto-lock ไม่มี Hero Highlight**: ยังไม่ชัดเจนว่าระบบเลือก Hero อะไรเมื่อ Timer หมดโดยไม่มี Selection
- ⚠️ **Disconnect ใน Hero Select**: ยังไม่พบ Logic Handle ผู้เล่น Disconnect ก่อน Lock In
