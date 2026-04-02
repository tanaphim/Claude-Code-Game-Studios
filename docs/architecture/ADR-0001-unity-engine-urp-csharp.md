# ADR-0001: Unity 2022.3.62f1 + URP + C# as Game Engine

## Status

Accepted

## Date

2024-01-01 (reconstructed from codebase)

## Decision Makers

Core development team

## Context

### Problem Statement

Delta เป็น MOBA ที่ต้องรองรับผู้เล่นหลายคนพร้อมกัน (5v5), visual effects จำนวนมาก,
และ cross-platform deployment บน PC และ Mobile ทีมมีประสบการณ์กับ Unity และต้องเลือก
engine + rendering pipeline ที่เหมาะสมกับ scope ของโปรเจกต์

### Current State

ไม่มี engine ก่อนหน้า — เป็นการตัดสินใจเริ่มต้นโปรเจกต์

### Constraints

- ทีมมีประสบการณ์ Unity / C# อยู่แล้ว
- ต้องรองรับ Mobile (iOS/Android) และ PC
- ต้องใช้ Photon Fusion 2 (networking library ที่รองรับเฉพาะ Unity)
- LTS release ต้องมีความเสถียรเพียงพอสำหรับ production

### Requirements

- Scripting ด้วย C# (strongly-typed, performant)
- Rendering pipeline รองรับ custom shaders + post-processing
- รองรับ asset streaming สำหรับ MOBA map ขนาดใหญ่
- Mobile-friendly rendering budget
- Active support + security patches (LTS)

## Decision

ใช้ **Unity 2022.3 LTS** เป็น engine หลัก, **C#** เป็นภาษา script,
และ **Universal Render Pipeline (URP)** เป็น rendering pipeline

### Architecture

```
Unity 2022.3.62f1 (LTS)
├── Language: C# (.NET Standard 2.1)
├── Rendering: URP (Universal Render Pipeline)
│   ├── Custom URP Shaders (HLSL)
│   ├── Post-Processing Stack v3
│   └── Shader Graph
├── Physics: PhysX (3D) / Box2D (2D)
├── Asset Pipeline: Unity Asset Pipeline v2
└── Build: Unity Build System (Cloud Build ready)
```

### Key Interfaces

```csharp
// MonoBehaviour base — ทุก gameplay component ใช้
public class MyComponent : MonoBehaviour { }

// ScriptableObject — data-driven config
[CreateAssetMenu]
public class MyConfig : ScriptableObject { }

// URP Shader — ทุก custom shader ใช้ URP pipeline
Shader "Custom/MyShader" {
    SubShader { Tags { "RenderPipeline" = "UniversalPipeline" } }
}
```

### Implementation Guidelines

- ใช้ Unity 2022.3.x LTS เท่านั้น — ห้าม upgrade เป็น 2023.x หรือ Unity 6 โดยไม่มี ADR ใหม่
- Shader ทั้งหมดต้องเขียนเป็น URP-compatible — ห้ามใช้ Built-in pipeline shaders
- C# version ตาม .NET Standard 2.1 — รองรับ async/await, Span<T>, IEnumerable
- Assembly Definition (.asmdef) ทุก major module เพื่อลด compile time

## Alternatives Considered

### Alternative 1: Unreal Engine 5

- **Description**: ใช้ UE5 + Blueprint/C++ + Niagara VFX
- **Pros**: Nanite/Lumen rendering คุณภาพสูง, marketplace assets, ทีมใหญ่รองรับ
- **Cons**: ทีมไม่มีประสบการณ์ C++, Photon Fusion 2 ไม่รองรับ UE5, binary project ทำให้ merge ยาก, mobile performance หนักกว่า
- **Estimated Effort**: สูงกว่า 3–4x เพราะต้อง learn C++/Blueprints
- **Rejection Reason**: ไม่มี Photon Fusion 2, ทีมไม่มี UE expertise

### Alternative 2: Godot 4

- **Description**: ใช้ Godot 4 + GDScript/C#
- **Pros**: Open source, lightweight, GDScript เรียนง่าย
- **Cons**: Photon Fusion 2 ไม่รองรับ Godot, ecosystem เล็กกว่า, mobile support อยู่ระหว่างพัฒนา
- **Estimated Effort**: ปานกลาง แต่ต้องพัฒนา networking เอง
- **Rejection Reason**: ไม่มี Photon Fusion 2 support

### Alternative 3: Unity HDRP

- **Description**: ใช้ HDRP แทน URP สำหรับ visual quality สูงกว่า
- **Pros**: Ray tracing, volumetric lighting, visual quality ระดับ AAA
- **Cons**: Mobile ไม่รองรับ HDRP, performance budget สูงกว่ามาก, MOBA ไม่ต้องการ visual quality ระดับนั้น
- **Rejection Reason**: Mobile support ไม่มี, overkill สำหรับ MOBA

## Consequences

### Positive

- ทีมเริ่มงานได้ทันทีโดยไม่ต้อง ramp-up
- Photon Fusion 2 ทำงานได้ทันที
- URP รองรับทั้ง PC และ Mobile ด้วย rendering pipeline เดียว
- LTS 2022.3 มี patch จนถึงปี 2025+

### Negative

- LTS 2022.3 ไม่มี Unity 6 features (Entities Graphics, Multiplayer Center)
- URP มี feature น้อยกว่า HDRP — บาง visual effect ทำได้ยากกว่า

### Neutral

- ต้องเขียน URP-compatible shaders ทั้งหมด (ไม่สามารถใช้ legacy shaders)

## Risks

| Risk | Probability | Impact | Mitigation |
|------|------------|--------|-----------|
| Unity เปลี่ยน API แบบ breaking | ต่ำ (LTS) | สูง | Pin version, ทำ upgrade test ก่อน |
| LTS หมด support | ต่ำ | ปานกลาง | มี ADR upgrade path ไว้ล่วงหน้า |

## Performance Implications

| Metric | Budget |
|--------|--------|
| Target framerate | TBD |
| Draw calls | TBD |
| Memory ceiling | TBD |

## Validation Criteria

- [x] โปรเจกต์ build ได้บน PC และ Mobile
- [x] Photon Fusion 2 integrate ได้กับ Unity 2022.3
- [x] URP shaders render ถูกต้องบนทุก platform target

## Related

- [ADR-0002: Photon Fusion 2 for networking](ADR-0002-photon-fusion-2-networking.md)
- [ADR-0003: PlayFab + Azure Functions backend](ADR-0003-playfab-azure-functions-backend.md)
- `.claude/docs/technical-preferences.md`
