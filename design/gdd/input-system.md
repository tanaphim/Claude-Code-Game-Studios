# Input System — Game Design Document

**System ID**: F3
**Version**: 0.1.0
**Status**: Draft — Reverse-documented from source
**Last Updated**: 2026-04-02
**Dependencies**: Actor System (F1), Networking Core (F2)

---

## 1. Overview

ระบบ Input ใช้ **New Unity Input System** (Action-based) แบ่งเป็น 3 Input Map คือ Player (Combat), UI (Menu), Chat — สลับกันตามบริบท รองรับการ Remap ทุก Key ผ่าน UI ในเกม, Quick Cast ต่อทักษะ/ไอเทม, และบันทึกค่า Setting ผ่าน RuntimeSettings

---

## 2. Player Fantasy

ผู้เล่นกดได้ทันทีโดยไม่ต้องหยุดคิด — ทักษะตรงตำแหน่งเมาส์, A-Click ออโตโจมตีระหว่างเดิน, S หยุดทันที Quick Cast ทำให้ผู้เล่นระดับสูงแคสต์ได้เร็วขึ้นโดยไม่ต้องยืนยัน ผู้เล่นใหม่ใช้ Normal Cast แล้วเห็น Indicator ก่อนตัดสินใจ

---

## 3. Detailed Rules

### 3.1 Key Bindings ค่าเริ่มต้น

**ทักษะ**:

| Action | Key | หมายเหตุ |
|--------|-----|---------|
| Skill Q | Q | Quick Cast สลับได้ |
| Skill W | W | — |
| Skill E | E | — |
| Skill R (Ultimate) | R | — |
| Skill พิเศษ | T | Special Ability |
| เพิ่ม Rank Q | Ctrl+Q | ใช้ Skill Point |
| เพิ่ม Rank W | Ctrl+W | — |
| เพิ่ม Rank E | Ctrl+E | — |
| เพิ่ม Rank R | Ctrl+R | — |

**การเคลื่อนที่และการโจมตี**:

| Action | Key | หมายเหตุ |
|--------|-----|---------|
| เดิน / โจมตี | Right Click | Move หรือ Attack เป้าที่คลิก |
| Attack Move | A | เดินไปทิศทางนั้นพร้อม Attack สิ่งที่เจอ |
| Stop | S | หยุดทันที |
| Recall | B | Recall กลับฐาน |

**ไอเทม**:

| Action | Key |
|--------|-----|
| Item Slot 1–6 | ปุ่ม 1–6 |
| Item Slot 7 (Special) | ปุ่ม 7 |
| Spell D | D |
| Spell F | F |

**UI & กล้อง**:

| Action | Key | หมายเหตุ |
|--------|-----|---------|
| Scoreboard | Tab (กดค้าง) | แสดงสถิติทั้ง 2 ทีม |
| Toggle Camera Lock | Z | Lock/Unlock กล้องตาม Hero |
| Pan Camera | ←↑→↓ (Arrow) | เลื่อนกล้อง |
| Zoom | Mouse Wheel | Zoom In/Out |
| ปิด UI | Escape | Close Panel |
| Help | F1 | — |
| Dev Console | F10 | Conditional |

**Ping & สื่อสาร**:

| Action | Key |
|--------|-----|
| Ping บน Minimap | Left Click (Minimap) |
| Ctrl-Click Ping | Ctrl + Left Click |
| Alt-Click Ping | Alt + Left Click |
| Chat | Enter (Toggle) |
| เปลี่ยน Channel | Tab (ใน Chat) |

---

### 3.2 Input Maps

| Map | เปิดใช้เมื่อ |
|-----|------------|
| **Player** | ระหว่าง Gameplay ปกติ |
| **UI** | เปิด Settings / Shop / Menu |
| **Chat** | กด Enter เพิ่อเปิด Chat |

เปลี่ยน Map: `SwitchBindingMap(BindingMapKey.Player/UI/Chat)`
→ ป้องกัน Input ซ้อนกันระหว่าง Context

---

### 3.3 Input Blocking

Input ถูกบล็อกเมื่อ:
- Hero ตาย (`IsDead = true`)
- Chat เปิดอยู่ (Chat Map Active)
- UI เปิดอยู่ (UI Map Active)
- F10 Console เปิด
- `IsControllerActive = false`

Mouse / Camera Input ยังทำงานแม้ในบาง Blocked State (`m_ActionAlwaysActive`)

---

### 3.4 Quick Cast (Smartcast)

**Quick Cast = Cast ทันทีที่ตำแหน่งเมาส์เมื่อ Key Release**

| ค่า | Default | คำอธิบาย |
|-----|---------|---------|
| QuickQ | **true** | Q เปิด Quick Cast ตั้งแต่แรก |
| QuickW | false | — |
| QuickE | false | — |
| QuickR | false | — |
| Quick1–7 | false | ไอเทม Slot 1–7 |
| QuickD / QuickF | false | Spell D/F |

**Normal Cast vs Quick Cast**:
- Normal Cast: กดปุ่ม → แสดง Targeting Indicator → คลิกยืนยัน
- Quick Cast: กดปุ่ม → ยิงทันทีที่ตำแหน่ง Cursor (Trigger บน Key Release)

**Quick Cast Indicator**: เปิด/ปิดแยกต่างหาก (`isQuickCast_indicator`) — แสดง Preview ระหว่างกดค้าง

---

### 3.5 Key Remapping

- รองรับเต็มรูปแบบผ่าน Unity Input System Rebinding
- UI: `HotKeySettingPanel` ในเมนู Settings
- บันทึกเป็น JSON ใน `StreamingAssets/Input.json`
- Binding key: `Inputbind` ใน RuntimeSettings (Persist)
- รีเซ็ตเป็น Default ได้

---

### 3.6 Controller Support

**ไม่รองรับ** — ปัจจุบันรองรับเฉพาะ Keyboard + Mouse

---

### 3.7 Command Architecture

```
PlayerGameplayInput (Singleton)
  ↓ Action Event
PlayerInputCommand
  ↓ Dictionary Dispatch
  ├─ m_ActionCombatMappings  → ทักษะ / เดิน (บล็อกเมื่อตาย)
  ├─ m_ActionAlwaysActive    → เมาส์ / กล้อง (ทำงานเสมอ)
  ├─ m_ChatMappings          → Chat
  └─ m_ConsoleMappings       → Dev Console
```

---

## 4. Formulas

### Quick Cast Trigger
```
ถ้า isQuickCast(action) == true:
  OnKeyPress → แสดง Indicator (ถ้า isQuickCast_indicator)
  OnKeyRelease → Cast ทันที
ถ้า false:
  OnKeyPress → แสดง Targeting Indicator
  OnClick → Confirm Cast
```

---

## 5. Edge Cases

| สถานการณ์ | พฤติกรรม |
|-----------|---------|
| กด Q ขณะ Hero ตาย | บล็อก — ไม่ส่ง Input |
| กด Q ขณะ Cooldown | ส่ง Input → Server ตรวจว่า Available หรือไม่ |
| กด Enter ใน Gameplay | เปิด Chat; Input ถัดไปถูก Route ไป Chat |
| กด Ctrl+Q ขณะไม่มี Skill Point | ส่งคำสั่ง → Server ตรวจสอบ SkillPoint |
| Remap ซ้ำกับ Key อื่น | Unity Input System ตรวจ Conflict (เปลี่ยนหรือแจ้งเตือน) |
| Tab กดค้าง (Scoreboard) vs Tab ใน Chat (เปลี่ยน Channel) | แยก Map — ไม่ Conflict |

---

## 6. Dependencies

| ระบบ | ความสัมพันธ์ |
|------|------------|
| **Networking Core (F2)** | InputMessage ส่งผ่าน Fusion INetworkInput ทุก Tick |
| **Actor System (F1)** | ตรวจ `IsDead`, `CanMove`, `CanAttack` ก่อน Execute Input |
| **Combat & Skills System (C1)** | Q/W/E/R → Trigger Skill; A → Attack Move |
| **Item System (FT1)** | 1–7, D, F → UseItemOnSlot() |
| **Movement & Navigation (C3)** | Right-Click → SetDestination(); S → Stop() |
| **Hero System (C2)** | Ctrl+Q/W/E/R → RankupSkill() |
| **Camera System (P5)** | Z → ToggleCameraLock(); Arrow/Wheel → Camera Control |
| **HUD & UI (P1)** | Tab → Scoreboard; Chat Enter → UIChatView |

---

## 7. Tuning Knobs

| ค่า | ที่อยู่ | ค่าปัจจุบัน | ผลกระทบ |
|-----|--------|------------|---------|
| Mouse Sensitivity | RuntimeSettings.Sensitivity | float | ความเร็วของ Cursor |
| Camera Sensitivity | RuntimeSettings.CameraSensitivity | float | ความเร็วของ Camera Mouse |
| Camera Keyboard Speed | RuntimeSettings.CameraKeyboardSensitivity | float | ความเร็วปุ่ม Arrow |
| Quick Cast Default (Q) | RuntimeSettings.QuickQ | true | Q เปิด Quick Cast ตั้งแต่แรก |
| Key Bindings | RuntimeSettings.Inputbind (JSON) | Default | Remap โดย Player |
| Auto Attack | RuntimeSettings.SetAutoAttack | bool | โจมตีอัตโนมัติ |

---

## 8. Acceptance Criteria

- [ ] Right-Click → Hero วิ่งไปตำแหน่งนั้น
- [ ] Q/W/E/R → Trigger Skill ที่ถูกต้อง
- [ ] A-Click → Hero เดินพร้อมโจมตีสิ่งที่เจอ
- [ ] S → Hero หยุดทันที
- [ ] B → เริ่ม Recall
- [ ] 1–7 → ใช้ไอเทม Slot ที่ตรงกัน
- [ ] Quick Cast Q: กด Q → ยิงทันที; Normal: กด Q → เห็น Indicator → คลิก
- [ ] Ctrl+Q → เพิ่ม Rank Skill Q (ถ้ามี Point)
- [ ] Tab กดค้าง → Scoreboard แสดง; ปล่อย → หาย
- [ ] Remap Key ผ่าน Settings UI → บันทึกและใช้งานได้ทันที
- [ ] Input ขณะ Hero ตาย → ไม่ทำงาน (ยกเว้น Camera)
- [ ] Chat เปิด → Gameplay Input บล็อก; ปิด Chat → กลับปกติ

---

## Known Issues / TODO

- ⚠️ **Controller**: ไม่รองรับ — ถ้าต้องการเพิ่ม Gamepad ต้องเพิ่ม Binding ใน .inputactions
- ⚠️ **Input Buffering**: Quick Cast Trigger บน Release — ไม่มีระบบ Buffer แยกต่างหาก ถ้ากด Key เร็วมากอาจ Miss
- ⚠️ **Spell D/F**: พบว่ามี Key D/F สำหรับ Spell พิเศษ แต่ยังไม่ชัดเจนว่าแมปกับ Item Slot ใดใน Inventory
