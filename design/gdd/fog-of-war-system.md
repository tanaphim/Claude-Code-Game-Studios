# Fog of War & Vision System — Game Design Document

**System ID**: FT4
**Version**: 0.1.0
**Status**: Draft — Reverse-documented from source
**Last Updated**: 2026-04-02
**Dependencies**: Actor System (F1), Map & Objectives (FT2), Networking Core (F2)

---

## 1. Overview

ระบบ Fog of War ใช้ Grid-based Shadowcasting สร้าง Visibility Texture ครอบแผนที่ แสดงเฉพาะพื้นที่ที่ฝ่ายตัวเองมองเห็น ทุก Hero, Tower, Ward ให้ Vision ต่างกัน มีระบบ Bush ซ่อนตัว, Invisibility Status Effect, True Sight, Sentry Ward เพื่อ Counter และ AOI (Area of Interest) ระดับ Network สำหรับจำกัดข้อมูลที่ส่งให้แต่ละ Client

---

## 2. Player Fantasy

การมองไม่เห็นสร้างความตึงเครียด — ไม่รู้ว่าศัตรูอยู่ที่ไหน, อาจซ่อนในพุ่มไม้รอ Gank พิสูจน์ทักษะผู้เล่นระดับสูงที่วาง Ward อย่างชาญฉลาด ควบคุม Vision ในแผนที่ได้มากกว่า คือควบคุมข้อมูลได้มากกว่า และข้อมูลคือชัยชนะ

---

## 3. Detailed Rules

### 3.1 สถาปัตยกรรม FOW

**วิธีการ**: Grid-based + Shadowcasting
- แผนที่แบ่งเป็น Grid Tile (ค่า Default ~11×11 หน่วย)
- ทุก Revealer คำนวณ Line of Sight ด้วย Shadowcasting Algorithm
- ผลลัพธ์เป็น `fogField` (ETileVisibility: Revealed / Hidden)
- Render เป็น Texture → ครอบ Fog Plane บนแผนที่ + Minimap

**Update Rate**: 10 ครั้ง/วินาที (interval 0.1f), Texture Lerp 30 ครั้ง/วินาที

---

### 3.2 แหล่ง Vision (Revealers)

| ประเภท | ระยะ Vision | หมายเหตุ |
|--------|------------|---------|
| **Hero** | `SightRange` Stat | เคลื่อนที่ตามตัวละคร |
| **Tower** | Configurable + TrueSight 5f | ถาวร ไม่เคลื่อนที่ |
| **Observer Ward** | กำหนดใน Item Data | ชั่วคราว วางในแผนที่ |
| **Jungle Creep** | `SightRange` Stat | Team.Free — ให้ Vision ร่วมแก่ทีมผู้วาง |

**SightRange Stat**: เก็บเป็น `value / 100` (เช่น ค่า CBS = 1500 → ในระบบ = 15.0 หน่วย)

---

### 3.3 Bush System

**หลักการ**: เข้า Bush → ซ่อนจากศัตรูที่ไม่อยู่ใน Bush เดียวกัน

**ลำดับเหตุการณ์**:
1. Unit เข้า Trigger ของ Bush → `SetBushState(bushID, true)`
2. Renderer Material ถูก Fade (ซ่อนจากมุมมองศัตรู)
3. `IsInBush = true`, `BushId = bushID` (Networked)

**กฎการมองเห็นใน Bush**:

| ผู้สังเกต | เห็นหน่วยใน Bush หรือไม่ |
|----------|------------------------|
| ฝ่ายเดียวกัน | ✅ เห็นเสมอ |
| ศัตรูที่อยู่ใน Bush เดียวกัน | ✅ เห็น (sameBush = true) |
| ศัตรูที่อยู่ใน Bush ต่างกัน | ❌ ไม่เห็น |
| ศัตรูที่ไม่อยู่ใน Bush | ❌ ไม่เห็น |

**Bush Action Cooldown**: หลังกระทำ (ถูกโจมตี/ใช้ทักษะ?) → Cooldown **5 วินาที** ก่อนที่ระบบจะ Re-hide อีกครั้งแม้ยังอยู่ใน Bush

---

### 3.4 Invisibility / Stealth

**วิธี**: Status Effect `"invisibility"` → ซ่อน Mesh Renderer, SkinnedMeshRenderer, VFX ทั้งหมด
- `IsInvisibility = true` → หน่วยหายไปจากมุมมองศัตรู
- Animator ยังทำงานปกติ (keepAnimatorStateOnDisable = true)

**Tower กับ Invisible**:
- Tower **ไม่เห็น** Invisible Unit ตามปกติ
- Tower เห็นเฉพาะหน่วยที่มีสถานะ `IsRevel = true` (ถูก Reveal)

**ที่ Reveal Invisible**:

| วิธี | กลไก | รายละเอียด |
|------|------|-----------|
| **Sentry Ward** | `WardDetection.cs` | ตรวจ Invisible Unit ในระยะ; ใส่ Status `"revel"` |
| **Tower Retaliate** | `csTowerVisionAgent` | Reveal ชั่วคราว **1.75 วินาที** หลัง Tower ถูกโจมตี |
| **Tower Hold Grace** | `csTowerVisionAgent` | Reveal **0.75 วินาที** หลัง Tower Lock เป้า |
| **True Sight Radius** | Tower radius = **5 หน่วย** | เห็นทะลุ Invisibility ภายใน Radius |

**Status `"revel"`**: เมื่อ Active → หน่วยนั้นถูกบังคับให้มองเห็นได้ ไม่ว่าจะ Invisible หรือไม่

---

### 3.5 Ward System

**Observer Ward** (`ItemObserveWard.cs`):
- ไอเทม IsSpecialSlot = true ประเภท Replace (วางบนพื้น)
- สร้าง `FogRevealer` ชั่วคราวที่จุดวาง
- ให้ Vision ตาม Item Data ที่กำหนด

**Sentry Ward** (`WardDetection.cs`):
- ไม่ได้ให้ Vision ทั่วไป — ตรวจ Invisible เท่านั้น
- `Physics.OverlapSphereNonAlloc()` ทุก Frame ในระยะ `detectionRadius`
- ใส่ `"revel"` Effect ให้ Invisible Unit ที่อยู่ในระยะ
- เมื่อ Unit ออกจากระยะ → ลบ Effect
- Ward หายเมื่อ Owner ตาย → ยกเลิก Reveal ทั้งหมด

---

### 3.6 Day/Night Cycle

- **Cycle**: 5 นาที ต่อรอบ (CycleDuration = 300 วินาที)
- **ผลต่อ Gameplay**: ไม่มี — Vision Range เท่าเดิมทั้งกลางวันกลางคืน
- **ผลต่อ Visual**:
  - กลางคืน: Fog Alpha = **0.95** (มืดกว่า)
  - กลางวัน: Fog Alpha = **0.82** (สว่างกว่า)

> ⚠️ Day/Night เป็น Visual เท่านั้น ยังไม่มีผลต่อ Mechanic

---

### 3.7 Network AOI (Area of Interest)

ระดับ Network จำกัดข้อมูลที่ส่งให้แต่ละ Client ตาม AOI Radius ป้องกัน Cheat และลด Bandwidth:

| ประเภทหน่วย | AOI Radius |
|------------|-----------|
| Hero | 12 หน่วย |
| Tower (Static) | 8 หน่วย |
| Creep | 8 หน่วย |
| Skill / Projectile | 4 หน่วย |

**กฎ AOI**:
- ฝ่ายเดียวกัน → เห็นข้อมูลทุกอย่างเสมอ
- ฝ่ายศัตรู → เห็นเฉพาะหน่วยที่อยู่ใน AOI Radius
- Update ทุก **0.25 วินาที**

**Spectator**: เปิดให้เห็นทั้ง 2 ทีม หรือ Follow ทีมใดทีมหนึ่ง

---

### 3.8 Visibility Priority

ระบบตรวจสอบ Visibility ตามลำดับ:

```
1. Team Check        → ฝ่ายเดียวกันเสมอ = มองเห็น
2. FOW Grid          → อยู่ใน Revealed Tile หรือไม่?
3. Bush              → หน่วยใน Bush ต่างกับ Viewer?
4. Invisibility      → มี Status "invisibility"?
5. Revel             → มี Status "revel"? → Override ข้อ 4
6. True Sight        → อยู่ใน TrueSight Radius ของ Tower?
```

---

## 4. Formulas

### Vision Range Check
```
visible = distance(revealer, target)² ≤ (SightRange + 1)²
```

### Visibility Final (Enemy Unit)
```
finalVisible = VisibleByLOD
            AND VisibleByFog
            AND NOT (VisibleByStatus OR VisibleByHiding OR VisibleByBush)
```

### Bush Re-hide Logic
```
ถ้า CooldownHit expired AND IsInBush AND BushId != ""
→ SetBushState(BushId, true)  // re-hide
```

### Temporary Reveal (Tower)
```
Retaliate: Reveal(radius=5f, duration=1.75s) หลังถูกโจมตี
HoldGrace:  Reveal(radius=5f, duration=0.75s) หลัง Lock เป้า
```

---

## 5. Edge Cases

| สถานการณ์ | พฤติกรรม |
|-----------|---------|
| Hero เดินออกจาก Bush ทันที | หาย 5 วินาที (Bush Cooldown) → Re-hide ถ้ายังอยู่ใน Bush |
| Hero Invisible + อยู่ใน Bush | ทั้งสองซ้อนกัน — ศัตรูมองไม่เห็นยิ่งยาก |
| Tower โจมตี Invisible Hero | ได้เฉพาะเมื่อ IsRevel = true หรืออยู่ใน TrueSight 5 หน่วย |
| Ward วางในพุ่มไม้ (Bush) | Ward ให้ Vision ปกติแต่ไม่ทำให้ Hero ใน Bush นั้นถูกมองเห็น |
| Sentry Ward ตาย | Reveal Effect หายทันทีจาก Target ทั้งหมด |
| 2 Hero ของทีมต่างกันอยู่ใน Bush เดียวกัน | เห็นกันทั้งคู่ (sameBush check) |
| Hero ใช้ Blink/Dash ข้าม Bush | อาจไม่ Trigger Enter/Exit ถูกต้อง ⚠️ ต้องทดสอบ |
| กลางคืน: Vision Range | เท่ากลางวัน — แค่ Fog มืดกว่า |
| FOW ปิดใน Build Settings | Minimap Fog ถูก Disable ด้วย |

---

## 6. Dependencies

| ระบบ | ความสัมพันธ์ |
|------|------------|
| **Actor System (F1)** | `Actor.Trait.IsInBush`, `IsInvisibility`, `IsRevel`, `SightRange` Stat |
| **Map & Objectives (FT2)** | Bush Trigger ในแผนที่, Tower Vision, Grid Layout |
| **Networking Core (F2)** | AOI Manager ควบคุมข้อมูล Networked ที่ส่งแต่ละ Client; `NetworkBush`, `NetworkStatusEffect` |
| **Item System (FT1)** | Observer Ward และ Sentry Ward เป็นไอเทมที่ใช้งาน |
| **Combat & Skills System (C1)** | Invisibility/Reveal เป็น Status Effects ใน Pipeline |
| **HUD & UI (P1)** | Minimap ใช้ fogPlaneTextureLerpBuffer โดยตรง |
| **Creep/Minion System (FT3)** | Jungle Creep (Team.Free) เป็น Vision Revealer; Creep อยู่ใน Bush ใช้กฎเดียวกัน |

---

## 7. Tuning Knobs

| ค่า | ที่อยู่ | ค่าปัจจุบัน | ผลกระทบ |
|-----|--------|------------|---------|
| SightRange ต่อ Hero | CBS → CBSUnit.SightRange | CBS (÷100) | ระยะมองของ Hero แต่ละตัว |
| Tower TrueSight Radius | Hardcode | 5 หน่วย | ระยะที่ Tower เห็นทะลุ Invisible |
| Tower Retaliate Duration | Hardcode | 1.75 วินาที | นานแค่ไหนหลัง Tower โดนตี |
| Tower Hold Grace Duration | Hardcode | 0.75 วินาที | Grace Period หลัง Lock เป้า |
| Bush Re-hide Cooldown | Hardcode | 5 วินาที | หน่วงก่อน Re-hide หลัง Action |
| Day/Night Cycle | FogofWarManager.CycleDuration | 300 วินาที | ความยาวรอบ Day/Night |
| FOW Grid Resolution | csFogWar | scanSpacingPerUnit = 0.25 | ความละเอียดของ LOD |
| FOW Update Rate | csFogWar.updateInterval | 0.1 วินาที | ความถี่คำนวณ Visibility |
| AOI Radius — Hero | Hardcode | 12 หน่วย | ระยะ Network Visibility ของ Hero |
| AOI Update Interval | Hardcode | 0.25 วินาที | ความถี่ Update AOI |
| Night Fog Alpha | Hardcode | 0.95 | ความมืดกลางคืน |
| Day Fog Alpha | Hardcode | 0.82 | ความมืดกลางวัน |

---

## 8. Acceptance Criteria

- [ ] พื้นที่ที่ไม่มี Revealer → แสดงเป็น Fog บนแผนที่และ Minimap
- [ ] Hero เข้า Bush → ศัตรูที่ไม่อยู่ใน Bush นั้นมองไม่เห็น
- [ ] 2 Hero ต่างทีมใน Bush เดียวกัน → เห็นกันทั้งคู่
- [ ] Hero ที่ Invisible → Tower ไม่โจมตี (ยกเว้นอยู่ใน TrueSight Radius 5 หน่วย)
- [ ] Sentry Ward วาง → Invisible Hero ในระยะถูก Reveal; ออกจากระยะ → Reveal หาย
- [ ] Observer Ward สร้าง Vision Circle ที่จุดวาง
- [ ] Tower ถูกโจมตี → Reveal ในระยะ 5 หน่วย นาน 1.75 วินาที
- [ ] AOI: ข้อมูล Hero ศัตรูที่อยู่ > 12 หน่วย ไม่ถูกส่งให้ Client
- [ ] Minimap แสดง Fog ตรงกับ Fog บนแผนที่
- [ ] Day/Night เปลี่ยน Alpha Fog เท่านั้น ไม่เปลี่ยน Vision Range

---

## Known Issues / TODO

- ⚠️ **Bush + Blink**: การ Dash/Blink ข้ามพุ่มไม้อาจไม่ Trigger Enter/Exit ถูกต้อง — ต้องทดสอบ
- ⚠️ **Day/Night Gameplay**: ปัจจุบัน Visual เท่านั้น — ยังไม่มีผลต่อ Vision Range; อาจเพิ่มใน Future
- ⚠️ **`decressvision` Status Effect**: มีในโค้ด แต่ไม่พบ Implementation ที่ใช้งานจริง
- ⚠️ **Ward Placement Stats**: ระยะ Vision ของ Observer Ward ยังไม่ได้ดึงจาก CBS Dashboard
- ⚠️ **Sentry Ward Detection Radius**: ค่าตัวเลขระบุว่า "Configurable" แต่ยังไม่ได้ยืนยันค่าใน CBS
