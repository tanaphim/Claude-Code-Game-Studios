# Camera System — Game Design Document

**System ID**: P5
**Version**: 0.1.0
**Status**: Draft — Reverse-documented from source
**Last Updated**: 2026-04-02
**Dependencies**: Actor System (F1), Input System (F3), HUD & UI (P1)

---

## 1. Overview

Camera System ใช้ Cinemachine Virtual Camera แบบ Top-down ติดตาม Hero ของผู้เล่นด้วย Lerp Smoothing รองรับ Edge Pan (เมาส์ชิดขอบจอ), Zoom In/Out, Lock/Unlock กล้อง, Camera Shake เมื่อเกิดเหตุการณ์พิเศษ, Sepia Effect เมื่อ Hero ตาย และ Spectator Mode

---

## 2. Player Fantasy

กล้องติดตาม Hero อย่างนุ่มนวล ไม่กระตุก ผู้เล่นสามารถ Pan ไปดูจุดอื่นบนแผนที่ได้รวดเร็วโดยเลื่อนเมาส์ไปชิดขอบ หรือล็อคกล้องเพื่อ Focus ที่ Hero ตัวเองตลอดเวลา Shake เมื่อเกิด Skill ใหญ่ทำให้รู้สึกถึงผลกระทบ

---

## 3. Detailed Rules

### 3.1 Camera Architecture

- **Engine**: Cinemachine Virtual Camera + `CinemachineFramingTransposer`
- **View**: Top-down (มุมมองจากด้านบน)
- **Follow Target**: `GhostFollower` GameObject — ตำแหน่ง Lerp ระหว่างกล้องกับ Hero

---

### 3.2 Follow & Lerp

| ค่า | รายละเอียด |
|-----|-----------|
| Follow Target | GhostFollower |
| Follow Speed | `m_CameraSpeedToDestination` = **5f** |
| Lerp Mode | `Vector3.Lerp` ทุก Frame (Smooth Damp) |

- เมื่อ **Camera Locked** (`Z` Toggle): GhostFollower ติดตาม Hero ตลอดเวลา
- เมื่อ **Camera Unlocked**: GhostFollower หยุดนิ่ง; ผู้เล่น Pan ได้อิสระ

---

### 3.3 Zoom

| ค่า | รายละเอียด |
|-----|-----------|
| Min Zoom (ซูมเข้า) | **7** หน่วย |
| Max Zoom (ซูมออก) | **10** หน่วย |
| Input | Mouse Scroll Wheel |

Zoom ควบคุมผ่าน `CinemachineFramingTransposer.CameraDistance`

---

### 3.4 Edge Pan

เมื่อเมาส์เข้าใกล้ขอบหน้าจอ กล้องจะเลื่อนไปทิศทางนั้น:

| ค่า | รายละเอียด |
|-----|-----------|
| Threshold | **5 pixels** จากขอบจอ |
| Activation Delay | **0.5 วินาที** (ป้องกัน Accidental Pan) |
| Pan Speed (Mouse) | **5f** |
| Pan Speed (Keyboard Arrow) | **10f** |

**World Bounds (Overscan)**:
- Top: **8f** (ขยาย Bound ด้านบน)
- Right / Bottom / Left: **0f** (ไม่ขยาย)

---

### 3.5 Camera Lock Toggle

- **Key**: `Z` (Toggle)
- **Locked**: GhostFollower ติดตาม Hero ทุก Frame → กล้องอยู่กับ Hero เสมอ
- **Unlocked**: GhostFollower หยุด → ผู้เล่น Pan ได้; กดใกล้กลางจอหรือ Lock อีกครั้งเพื่อกลับ

---

### 3.6 Camera Shake

| ค่า | รายละเอียด |
|-----|-----------|
| Library | **EZCameraShake** |
| Intensity | **5** |
| Duration | **1.0 วินาที** |
| Trigger | Skill ขนาดใหญ่ / เหตุการณ์พิเศษ |

`CameraShaker.Instance.ShakeOnce(5f, 1f, ...)` เรียกจาก VFX/Skill Events

---

### 3.7 Death Effect (Sepia)

เมื่อ Hero ตาย:
- Beautify Post-processing Plugin ถูกเปิดใช้
- `Beautify.sepia = 1f` → ภาพทั้งจอเป็นสี Sepia (น้ำตาล)
- เมื่อ Respawn / Buyback → `sepia = 0f` รีเซ็ต

---

### 3.8 Spectator Mode

เมื่อผู้เล่นเสียชีวิตและรอ Respawn:
- `SetSpectatorFollower()` — เปลี่ยน Follow Target เป็น Ally ที่ยังมีชีวิต
- ผู้เล่นสามารถ Pan กล้องอิสระด้วย Edge Pan ขณะ Spectate
- Sepia Effect ยังคงอยู่ระหว่าง Death

---

### 3.9 Minimap Camera Sync

- Minimap ใช้ Camera แยกต่างหาก (Top-down Full Map)
- Smooth time: **0.18 วินาที** (Lerp ตาม Player Camera Position)

---

## 4. Formulas

### Follow Lerp
```
GhostFollower.position = Lerp(GhostFollower.position, Hero.position, speed × deltaTime)
  where speed = m_CameraSpeedToDestination = 5f
```

### Edge Pan Detection
```
isPanning = (mouseX < 5 || mouseX > Screen.width - 5
          || mouseY < 5 || mouseY > Screen.height - 5)
        AND (edgePanHeldTime >= 0.5s)

CameraPosition += panDirection × panSpeed × deltaTime
  where panSpeed = 5f (mouse) or 10f (keyboard arrow)
```

### Zoom Clamp
```
targetDistance = Clamp(targetDistance + scrollDelta, MinZoom=7, MaxZoom=10)
```

---

## 5. Edge Cases

| สถานการณ์ | พฤติกรรม |
|-----------|---------|
| กด Z ขณะเมาส์ที่ขอบ | Unlock → Pan ทำงาน; Lock → กลับ Follow Hero |
| Hero ตาย | Sepia เปิด; Spectator Mode; ยัง Pan ได้ |
| Respawn / Buyback | Sepia ปิด; กล้อง Snap กลับ Hero |
| เมาส์ขยับออกจากขอบเร็ว | Delay 0.5s ป้องกัน Accidental Pan |
| Scroll Wheel สุด Min/Max | Zoom Clamp ที่ 7/10 |
| เปิด Shop UI | Camera ยังทำงาน (m_ActionAlwaysActive) |

---

## 6. Dependencies

| ระบบ | ความสัมพันธ์ |
|------|------------|
| **Actor System (F1)** | Follow Target = Hero Transform |
| **Input System (F3)** | Z → ToggleLock; Arrow → Pan; Wheel → Zoom |
| **HUD & UI (P1)** | Minimap ใช้ Camera Position; Death UI เปิดพร้อม Sepia |
| **Combat & Skills System (C1)** | Skill Events Trigger Camera Shake |

---

## 7. Tuning Knobs

| ค่า | ที่อยู่ | ค่าปัจจุบัน | ผลกระทบ |
|-----|--------|------------|---------|
| Follow Speed | CameraControl.m_CameraSpeedToDestination | 5f | ความนุ่มนวลของ Follow |
| Min Zoom | CameraControl.MinZoom | 7 | ซูมเข้าได้แค่ไหน |
| Max Zoom | CameraControl.MaxZoom | 10 | ซูมออกได้แค่ไหน |
| Edge Pan Threshold | CameraControl | 5px | ระยะจากขอบที่กระตุ้น Pan |
| Edge Pan Delay | CameraControl | 0.5s | หน่วงกันกด Accidental |
| Pan Speed (Mouse) | CameraControl | 5f | ความเร็ว Edge Pan |
| Pan Speed (Keyboard) | CameraControl | 10f | ความเร็ว Arrow Pan |
| Camera Shake Intensity | CameraShaker | 5f | ความแรง Shake |
| Camera Shake Duration | CameraShaker | 1.0s | ระยะเวลา Shake |
| Minimap Smooth Time | MinimapCamera | 0.18s | Lerp ความนุ่มนวล Minimap |
| World Bound Top | CameraControl | 8f | ขยาย Overscan ด้านบน |

---

## 8. Acceptance Criteria

- [ ] กล้องติดตาม Hero แบบ Smooth Lerp; ไม่กระตุก
- [ ] Scroll Wheel → Zoom ระหว่าง 7–10
- [ ] เมาส์ชิดขอบ 5px เกิน 0.5s → Pan เริ่มทำงาน
- [ ] Arrow Key → Pan กล้อง (เร็วกว่า Mouse Edge Pan)
- [ ] Z → Toggle Camera Lock/Unlock
- [ ] Camera Lock: กล้องตาม Hero เสมอ
- [ ] Hero ตาย → Sepia Effect เปิด
- [ ] Hero Respawn → Sepia Effect ปิด
- [ ] Skill ใหญ่ → Camera Shake 5 intensity, 1 วินาที
- [ ] Spectator Mode: ติดตาม Ally ที่มีชีวิต + Pan ได้

---

## Known Issues / TODO

- ⚠️ **Beautify Plugin Dependency**: Sepia Effect ขึ้นอยู่กับ Third-party Plugin (Beautify) — ต้องระวัง Unity Version Compatibility
- ⚠️ **EZCameraShake**: Third-party Plugin เช่นกัน — ต้องตรวจสอบ Version Support
- ⚠️ **Edge Pan ขณะ UI เปิด**: Shop / Settings เปิดอยู่ — ต้องตรวจสอบว่า Edge Pan ถูกบล็อกหรือไม่
