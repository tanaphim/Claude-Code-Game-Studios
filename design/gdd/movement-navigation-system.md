# Movement & Navigation System — Game Design Document

**System ID**: C3
**Version**: 0.1.0
**Status**: Draft — Reverse-documented from source
**Last Updated**: 2026-04-02
**Dependencies**: Actor System (F1), Input System (F3), Networking Core (F2)

---

## 1. Overview

ระบบการเคลื่อนที่ใช้รูปแบบ **Click-to-Move** (Right-Click) ผ่าน Unity NavMesh เป็น Pathfinding หลัก Hero และหน่วยทุกตัวมีความเร็วเป็น Stat ที่ปรับได้ มีระบบ Rotation Smoothing, Stuck Recovery อัตโนมัติ และ Mobility Abilities (Dash/Knockback/Knockup) ที่ทำงานผ่าน Physics Capsule Cast Input ถูกส่งผ่าน Photon Fusion เป็น Server-Authoritative ทุก Frame

---

## 2. Player Fantasy

การเคลื่อนที่ต้องรู้สึก "ตอบสนองทันที" — คลิกแล้วตัวละครวิ่งไปทันที ไม่ติดขัด ไม่ค้างในมุม Dash ต้องรู้สึกแรงและแม่นยำ ส่วน Knockback/Knockup ต้องสื่อถึงแรงกระแทกจริง ผู้เล่นระดับสูงสามารถใช้ Dash หลบทักษะและผ่านช่องว่างในสนามรบได้อย่างชำนาญ

---

## 3. Detailed Rules

### 3.1 Click-to-Move

- **ปุ่ม**: Right-Click บนพื้น
- **กลไก**: Raycast จาก Camera → Ground Layer → NavMesh Sample → `SetDestination()`
- **NavMesh Snap**: ลองจุดที่คลิกก่อน (radius 0.5f) → ถ้าไม่ได้ ขยายเป็น 12f → ถ้ายังไม่ได้ สุ่ม 6 วงกลม × 24 จุด (144 ตัวเลือก) → ใช้จุดที่ใกล้ที่สุดที่ถึงได้
- **Auto-repath**: เปิดอยู่เสมอ (`autoRepath = true`)

### 3.2 ค่าพื้นฐาน NavMesh Agent

| Parameter | ค่า | หมายเหตุ |
|-----------|-----|---------|
| Agent Radius | 0.25f units | ขนาดหน่วยบน NavMesh |
| Agent Height | 2.0f units | ความสูงสำหรับ Clearance |
| Stopping Distance (NavMesh) | 0.05f units | ระยะหยุดพื้นฐาน |
| Stopping Distance (Attack) | ขึ้นกับ Attack Range | ปรับต่อ Ability |
| Acceleration | 999f | หน่วงแทบไม่มี (ตอบสนองทันที) |
| Obstacle Avoidance | LowQualityObstacleAvoidance | หลีกเลี่ยง Hero อื่น |
| Avoidance Priority | 50 | ปรับได้ต่อประเภทหน่วย |

### 3.3 ความเร็ว (Move Speed)

- **Stat Key**: `StatKey.move_speed`
- **Default Linear Speed**: 4.0 หน่วย/วินาที
- **Angular Speed**: 1,800 องศา/วินาที (หมุนเกือบทันที)
- **Min/Max Speed**: กำหนดใน CBS (`CBSConfigBattle.MinMoveSpeed`, `MaxMoveSpeed`)
- **Buff Speed**: `AdditionalMoveSpeed` (Network Variable) — Override ความเร็วปกติเมื่อ > 0

**การ Scale ความเร็ว**:
```
Acceleration = (Accel + Decel) / 2  →  Agent.acceleration
ค่า Default: Accel = 10f, Decel = 4f  →  Agent.acceleration = 7f
```

**กฎพิเศษ**: หากหน่วยถูก Root/Stun/Sleep → ความเร็วถูก Override เป็น 0f ทันที

### 3.4 Rotation

- ใช้ `Quaternion.RotateTowards()` ทุก Frame
- **Angular Speed**: 1,800 องศา/วินาที — หมุนเต็ม 180° ใน ~0.1 วินาที
- ทิศทางคำนวณจาก `(nextPosition - currentPosition).normalized`

### 3.5 Stuck Recovery

ระบบตรวจจับการติดอัตโนมัติ:

| Parameter | ค่า |
|-----------|-----|
| Threshold | ไม่ขยับ > 0.0001f sqr magnitude |
| Timeout | 3.0 วินาที |
| Recovery Action | Repath ไปยัง Destination เดิมอัตโนมัติ |

### 3.6 Dash & Mobility Abilities

**ประเภท Mobility Effect**:

| ประเภท | ควบคุมได้ | ทิศทาง | หมายเหตุ |
|--------|----------|--------|---------|
| **Dash** | ผู้เล่น | กำหนดเอง | Hero ยังคุมได้ระหว่าง Dash |
| **DashWithMobility** | ผู้เล่น | กำหนดเอง | Version Async, รองรับ Arc |
| **Knockback** | ไม่ได้ | คนใช้กำหนด | `IsKnockBack = true` ระหว่างบิน |
| **Knockup** | ไม่ได้ | Vertical | Physics-based, มี Gravity |
| **Pull/Hook** | ไม่ได้ | ดึงเข้าหา | ถูกดึงไปทิศทางที่กำหนด |

**กฎการ Dash ผ่านกำแพง**:
- Dash ใช้ `CapsuleCast` ตรวจ Layer "Outsidewall" และ "Obstacle"
- ถ้าทางตรงชนกำแพง → ย้ายไปจุดก่อนชน (margin 0.002f)
- **ไม่สามารถ Dash ทะลุกำแพงหลัก** แต่อาจข้ามสิ่งกีดขวางขนาดเล็กได้

**Knockup Trajectory**:
```
displacement.y = (10 × t) + (-15 × t^force)
หยุดเมื่อ position.y ≤ 0
```

**ระหว่าง Dash** (`IsDash = true`): ยกเว้นการตรวจ Input ปกติ — NavMeshAgent ถูก Bypass

### 3.7 สิทธิ์การเคลื่อนที่ (CanMove / CanDash)

| Flag | ผลต่อ Movement |
|------|---------------|
| `CanMove = false` | NavMesh Speed = 0; ไม่รับ Move Input |
| `CanDash = false` | ยกเลิก Dash Ability ทันที |

**`CanMove = false` เมื่อ**: Stun, Root, Sleep, IsKnockBack
**`CanDash = false` เมื่อ**: Root, IsDash (กำลัง Dash อยู่), Fear, Charm

### 3.8 Camera Follow

- **โหมดเริ่มต้น**: ติดตาม Hero อัตโนมัติ (`m_IsFollow = true`)
- **Smoothing**: `Vector3.Lerp(ghostPos, heroPos, dt × 5f)`
- **ปลดล็อกกล้อง**: เลื่อน Mouse ไปขอบจอ (5px) หน่วง 0.5s → Pan

| Parameter | ค่า |
|-----------|-----|
| Follow Smoothing Speed | 5f |
| Pan Speed | 5f units/s |
| Camera Zoom Min | 7f |
| Camera Zoom Max | 10f |
| Edge Pan Threshold | 5 pixels |
| Edge Pan Delay | 0.5 วินาที |

---

## 4. Formulas

### Move Speed (ที่ส่งให้ NavMeshAgent)
```
if (!CanMove)         → Agent.speed = 0
elif AdditionalMoveSpeed > 0 → Agent.speed = AdditionalMoveSpeed
else                  → Agent.speed = move_speed stat value
```

### NavMesh Snap Fallback
```
Try 1: SamplePosition(click, radius=0.5f)
Try 2: SamplePosition(click, radius=12f)
Try 3: for ring in 1..6:
           for sample in 0..23:
               angle = (sample / 24) × 2π
               point = click + (ring × 2f) × (cos(angle), 0, sin(angle))
               if SamplePosition(point, 0.5f): return nearest valid point
```

### Knockup Displacement
```
y(t) = 10t + (-15 × t^force)
หยุดเมื่อ y ≤ 0
```

### Dash Arc Trajectory (Arc Dash)
```
x      = Distance(start, end)
r      = height/2 + x²/(8×height)     [ถ้า height == 0 → r = 0]
q      = 2 × arctan(2×height / x)     [ถ้า height == 0 → q = 0]
length = r × q × 2                    [ถ้า height == 0 → length = x]
time   = length / speed
```

### Rotation Rate
```
180° หมุนเสร็จใน = 180 / 1800 = 0.1 วินาที
```

---

## 5. Edge Cases

| สถานการณ์ | พฤติกรรม |
|-----------|---------|
| Right-click บน Unwalkable Area | NavMesh Snap หาจุดใกล้เคียง; ถ้าไม่เจอ → ใช้จุดสุดท้ายที่ Valid |
| Hero ถูก Root ระหว่าง Dash | CanDash = false แต่ถ้า Dash เริ่มแล้ว → จบ Dash ตามปกติ (Root ป้องกันการเริ่ม Dash ใหม่) |
| Knockback ชนกำแพง | หยุดทันทีที่จุดก่อนชน (margin 0.002f) |
| Hero ติดใน Geometry 3 วินาที | Auto-repath ไปยัง Destination เดิม |
| Dash ระยะสั้นมาก (< 0.01f) | `STOP_DASH_THRESHOLD` → หยุด Dash ทันที |
| Input ขณะเปิด Chat/Console | ยกเว้น Movement Input ทั้งหมด |
| Click ที่ Hero ศัตรู | `InputMessage.Target` = Actor นั้น → เดินแล้วโจมตีอัตโนมัติ |
| Minion เดินชนกัน | NavMesh LowQuality Avoidance — กระจายตัวตามลำดับ Priority |

---

## 6. Dependencies

| ระบบ | ความสัมพันธ์ |
|------|------------|
| **Actor System (F1)** | `Actor.Driver` (NavMeshAgent wrapper), `Actor.Trait.CanMove/CanDash`, `move_speed` Stat |
| **Input System (F3)** | `PlayerGameplayInput` → `NetworkRunnerInput` → `InputMessage` |
| **Networking Core (F2)** | `InputMessage` ส่งผ่าน Photon Fusion; Server executes `SetDestination()` |
| **Combat & Skills System (C1)** | Dash/Knockback/Knockup สร้างผ่าน `NetworkStatusEffect`; `MoveToAction()` ใช้ใน Auto-attack |
| **Map & Objectives (FT2)** | NavMesh อ้างอิงกับ Map Layout; Layer "Outsidewall" / "Obstacle" กำหนดใน Scene |
| **Item System (FT1)** | Items ที่มี `move_speed` Stat หรือ Dash Effect ปรับค่าผ่าน `Trait.ApplyStat()` |

---

## 7. Tuning Knobs

| ค่า | ที่อยู่ | ค่าปัจจุบัน | ผลกระทบ |
|-----|--------|------------|---------|
| Base Move Speed | ActorDriver.LinearSpeed | 4.0 f/s | ความเร็วพื้นฐานทุกหน่วย |
| Min/Max Move Speed | CBS → CBSConfigBattle | CBS | Clamp ความเร็วสูงสุด/ต่ำสุด |
| Angular Speed | ActorDriver.AngularSpeed | 1800°/s | ความไวในการหมุน |
| Stuck Recovery Timeout | Hardcode | 3.0s | นานแค่ไหนก่อน Auto-repath |
| NavMesh Snap Radius | Hardcode | 0.5f / 12f | ความยืดหยุ่นในการคลิกพื้นที่ไม่เรียบ |
| Camera Follow Speed | CameraManager | 5f | ความลื่นไหลของกล้องตาม Hero |
| Camera Zoom Range | CameraManager | 7–10 | ระยะมอง |
| Dash Collision Margin | Hardcode | 0.002f | ระยะห่างจากกำแพงเมื่อ Dash หยุด |
| Agent Radius | Hardcode | 0.25f | ขนาดบน NavMesh; กระทบ Avoidance |

---

## 8. Acceptance Criteria

- [ ] Right-Click บนพื้น → Hero วิ่งไปยังจุดนั้นทันที
- [ ] Click บน Unwalkable Area → Hero วิ่งไปจุดใกล้เคียงที่ถึงได้แทน
- [ ] Hero ถูก Stun/Root → ความเร็ว = 0, ไม่รับ Move Command
- [ ] Dash ชนกำแพง → หยุดก่อนชนทุกครั้ง (ไม่ทะลุ)
- [ ] Knockup ใช้สูตร `10t − 15t^f` และลงพื้นเมื่อ y ≤ 0
- [ ] Stuck 3 วินาที → Auto-repath อัตโนมัติ
- [ ] กล้องติดตาม Hero ด้วย Lerp Smooth; ปลดได้เมื่อเลื่อน Mouse ไปขอบ
- [ ] Input ขณะเปิด Chat → Movement ไม่ถูก Trigger
- [ ] Click บน Hero ศัตรู → เดินเข้าหาและโจมตีอัตโนมัติ
- [ ] Network: Movement ทุก Client Sync กับ Server ทุก Tick

---

## Known Issues / TODO

- ⚠️ **Minion Formation**: ไม่มีระบบ Formation Movement จริง — Minion แต่ละตัว Path ของตัวเอง; อาจทำให้ดูไม่เป็นระเบียบ
- ⚠️ **Dash ผ่านสิ่งกีดขวางขนาดเล็ก**: CapsuleCast อาจ Miss สิ่งกีดขวางที่เล็กกว่า Capsule — ต้องทดสอบกับ Map จริง
- ⚠️ **AdditionalMoveSpeed Override**: ถ้า Item/Buff ตั้ง AdditionalMoveSpeed > 0 จะ Override ความเร็วทั้งหมด — อาจเกิด Bug เมื่อมีหลาย Buff พร้อมกัน
- ⚠️ **LowQuality Obstacle Avoidance**: Minion จำนวนมากในบริเวณแคบอาจทำให้ติดกัน
