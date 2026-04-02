# Town System — Game Design Document

**System ID**: FT9
**Version**: 0.1.0
**Status**: Draft — Reverse-documented from source
**Last Updated**: 2026-04-02
**Dependencies**: Actor System (F1), Movement & Navigation (C3), Data-Config System (F3), Networking Core (F2)

---

## 1. Overview

Town เป็น Social Hub ระหว่าง Match สำหรับจัดการ Equipment, เลือก Hero/Skin, เข้า Dungeon, สร้าง Lobby, และพูดคุยกับผู้เล่นอื่น Town ทำงานเป็น Mode แยกต่างหาก (`ModeSelectionType.Town`) โหลดในฉาก `scene_town_map.unity`

---

## 2. Player Fantasy

Town คือ "บ้าน" ของผู้เล่นระหว่างแมตช์ — ที่ที่มาจัดเตรียมตัวก่อนลงสนาม, อัปเกรด Equipment, เดินเจอเพื่อน และเลือก Dungeon ที่จะลงด้วยกัน บรรยากาศผ่อนคลายต่างจากความตึงเครียดใน MOBA

---

## 3. Detailed Rules

### 3.1 สิ่งที่ทำได้ใน Town

| Feature | สถานะ | คำอธิบาย |
|---------|-------|---------|
| Equipment Management | ✅ มี | เลือก/สวม Equipment 6 Slot |
| Hero Selection | ✅ มี | เลือก Hero และ Skin |
| Dungeon Entry | ✅ มี | เข้าสู่ Dungeon Mode |
| Lobby Creation | ✅ มี | สร้างห้อง Custom |
| Chat | ✅ มี | RadiusChat ในพื้นที่ Town |
| NPC Interaction | ⚠️ Framework | โค้ดมีแต่ยังไม่ Implement ครบ |
| Quest System | ⚠️ Framework | EventTaskSystem.cs มีแต่ Content น้อย |
| Housing | ❌ ไม่มี | ยังไม่ Implement |

### 3.2 Equipment System ใน Town

Equipment สำหรับ Dungeon Mode — ไม่ใช่ไอเทมใน MOBA:

**6 Equipment Slot**:

| Slot | ประเภท |
|------|--------|
| Weapon | อาวุธ |
| Helmet | หมวก |
| Body | เกราะตัว |
| Pants | กางเกง |
| Gloves | ถุงมือ |
| Boots | รองเท้า |

**UI สี Stat**:
- Physical (AD, LifeSteal) → สีส้ม
- Magical (AP, CDR) → สีม่วง
- Speed → สีขาว
- Critical → สีแดง
- HP/Regen → สีเขียว
- MP/Regen → สีฟ้า
- Defense → สีพิเศษ

### 3.3 Hero & Skin Selection

- เลือก Hero ที่ตัวเองเป็นเจ้าของ
- เลือก Skin ที่ Unlock แล้ว
- เลือก Weapon Skin แยกต่างหาก
- Preview ก่อนยืนยัน

### 3.4 Navigation

- **Scene**: `scene_town_map.unity` (โหลด Async แบบ Additive)
- **Style**: 3D Town Kit (Medieval Theme)
- **Camera**: `CameraControl.cs` — Top-down view
- **Interaction**: LeanButton (Point-and-click)

### 3.5 Social Features

- **Chat**: Chat Channel ต่อ Session
- **Party System**: สร้าง Lobby / เชิญเพื่อน
- **Room Visibility**: Public หรือ Password-protected

---

## 4. Formulas

### Equipment Stat Apply (เหมือน Dungeon)
```
ใช้ CBSItemInGame.Key/Value/ModifierType → Actor.Trait.ApplyStat()
```

---

## 5. Edge Cases

| สถานการณ์ | พฤติกรรม |
|-----------|---------|
| เลือก Equipment แล้วเข้า MOBA | Equipment ไม่มีผล — ใช้เฉพาะ Dungeon |
| ออกจาก Town ขณะ Chat | Chat ปิดอัตโนมัติ |
| NPC ไม่มี Quest ให้ | UI ปิด Interaction |

---

## 6. Dependencies

| ระบบ | ความสัมพันธ์ |
|------|------------|
| **Dungeon Mode (FT8)** | Town เป็น Entry Point สู่ Dungeon |
| **Hero System (C2)** | เลือก Hero / Skin ใน Town |
| **Data-Config System (F3)** | Equipment Data จาก CBS |
| **Networking Core (F2)** | Party, Lobby, Chat |
| **Matchmaking (FT6)** | สร้าง Custom Lobby จาก Town |

---

## 7. Tuning Knobs

| ค่า | ที่อยู่ | ผลกระทบ |
|-----|--------|---------|
| Equipment Slot Count | Hardcode | 6 Slot |
| Town Chat Radius | RadiusChat | ระยะได้ยินใน Town |
| Scene Load Mode | Additive | ผลต่อ Transition |

---

## 8. Acceptance Criteria

- [ ] ผู้เล่นเข้า Town ได้หลัง Login
- [ ] Equipment 6 Slot เลือกและ Preview ได้
- [ ] Hero + Skin เลือกได้จาก Town
- [ ] ปุ่ม Dungeon Entry → เข้าสู่ Dungeon Mode
- [ ] สร้าง Custom Lobby จาก Town ได้
- [ ] Chat ใน Town ทำงาน

---

## Known Issues / TODO

- ⚠️ **NPC / Quest Content**: Framework มีแต่ยังไม่ Implement ครบ
- ⚠️ **Housing**: ยังไม่มีระบบ
- ⚠️ **Town Equipment vs MOBA Item**: ต้องทำให้ผู้เล่นเข้าใจชัดว่า Equipment ใน Town ใช้เฉพาะ Dungeon
