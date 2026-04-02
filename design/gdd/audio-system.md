# Audio System — Game Design Document

**System ID**: F5
**Version**: 0.1.0
**Status**: Draft — Reverse-documented from source
**Last Updated**: 2026-04-02
**Dependencies**: Data/Config System (F4), Combat & Skills (C1), Actor System (F1)

---

## 1. Overview

Audio System ของ Delta ใช้ `AudioManager` (Singleton) เป็นศูนย์กลางจัดการเสียงทุกประเภท ร่วมกับ `AudioPoolManager` สำหรับ Object Pool ของ AudioSource ระบบแบ่งช่องเสียงผ่าน Unity AudioMixer (DeltaAudioMixer) เป็น 12 channel ครอบคลุม BGM, SFX, Voice, Ping, Announcer, Ambience และช่อง Lobby แยกต่างหาก รองรับ Spatial 3D Audio สำหรับเสียงในเกม และ 2D Stereo สำหรับ UI/BGM

---

## 2. Player Fantasy

ผู้เล่นได้ยินเสียงที่ตรงกับ Action บนหน้าจอ — Skill ใหญ่มีเสียง Impact ชัดเจน, Hero แต่ละตัวมีเสียงพูดเป็นเอกลักษณ์, Announcer ประกาศ Kill Streak ให้รู้สึกตื่นเต้น ปรับ Volume แยก Category ได้ตามความชอบ

---

## 3. Detailed Rules

### 3.1 Architecture

```
AudioManager (Singleton)
├── AudioPoolManager (Object Pool — AudioSource)
├── AudioVoiceLine (Character Voice / Spatial)
├── AudioAmbientTrigger (Scene Ambient / Spatial)
├── AudioSendToAnotherPlayer (Network-synced / Spatial)
├── AudioEffect (Advanced: repeat, fade, pitch variance)
└── AudioSourceExtensions (FadeIn / FadeOut / CrossFade utils)
```

- **DeltaAudioMixer**: Unity AudioMixer asset ที่ `/Sounds/AudioMixer/DeltaAudioMixer.mixer`
- Audio Arrays ใน AudioManager: `m_MusicSounds[]`, `m_AnnouncerSounds[]`, `m_PingSounds[]`, `m_FxSounds[]`, `m_AmbienceSounds[]`
- ทุก Audio Call ตรวจ `DeltaService.BuildSettings.EnableSound` ก่อนเล่น

---

### 3.2 Mixer Channels

| Index | Channel | ใช้สำหรับ | Spatial |
|-------|---------|----------|---------|
| 4 | Heropick | Hero Selection audio | ไม่ |
| 7 | Music | BGM | ไม่ |
| 8 | Announcer | Kill/Objective announcer | ไม่ |
| 9 | Voice | Character voice lines | ใช่ |
| 10 | Ambience | Environmental ambient | ใช่ |
| 11 | SFX | Skill/Impact/Gameplay SFX | ใช่ |
| 12 | Ping | Map ping sounds | ไม่ |
| — | MasterMenu | Lobby Master | ไม่ |
| — | MusicMenu | Lobby BGM | ไม่ |
| — | EffectMenu | Lobby SFX | ไม่ |
| — | AmbientMenu | Lobby Ambient | ไม่ |

---

### 3.3 BGM System

- **Single-source model**: `m_MusicSource` AudioSource เดียว — `PlayMusic(name)` replace ทันที (ไม่มี Crossfade built-in)
- Route: `audioMixGroup[7]` (Music)
- Non-spatial: `dopplerLevel=0`, `spatialBlend=0`, `rolloffMode=Linear`
- `StopMusic()` → คืน AudioSource กลับ Pool

---

### 3.4 SFX Playback Methods

| Method | Channel | Spatial | Pattern |
|--------|---------|---------|---------|
| `PlayMusic(name)` | 7 (Music) | ไม่ | Persistent — replace ก่อนหน้า |
| `PlayAnnouncer(name)` | 8 (Announcer) | ไม่ | One-shot async, random variant |
| `PlayPing(index)` | 12 (Ping) | ไม่ | One-shot async, index-based |
| `PlaySoundFx(name)` | 11 (SFX) | ไม่ | One-shot async |
| `PlayVoiceLine(obj, name)` | 9 (Voice) | ใช่ (maxDist=25) | One-shot async, block overlap |
| `PlayDyingSound(obj)` | 9 (Voice) | ใช่ | One-shot async |
| `PlayAvatarPickSoundFx(obj)` | 4 (Heropick) | ไม่ | Dual: voice + SFX parallel |
| Ambient Trigger | 10 (Ambience) | ใช่ (maxDist=35) | Loop on Start |
| Network Character | 11/9 | ใช่ (maxDist=75) | On-demand sync |

---

### 3.5 Object Pool

- `AudioPoolManager`: Queue-based, `m_InitialPoolSize = 10`
- `GetAudioSource(parent)` → Dequeue หรือสร้างใหม่
- `Return(source)` → Stop, clear clip, set inactive, re-enqueue
- One-shot SFX: async `await WaitForSeconds(clip.length)` แล้ว Return อัตโนมัติ

---

### 3.6 Voice Lines

- **VoiceLineObject** (ScriptableObject): `Sound[] sounds` — array ของ clip พร้อม name, volume, pitch, loop
- ค้นหาด้วย `Array.Find(sounds, s => s.name == "Pick")` หรือ `Array.FindAll(..., Contains)`
- Voice Line ทับซ้อนกันไม่ได้ (`isPlaying` flag block)
- `PlayDyingSound` ค้นหา clip ชื่อ "Die"
- Spatial: `spatialBlend=1`, `maxDistance=25`, `rolloffMode=Linear`

**ชื่อ Sound มาตรฐาน**: "Pick", "Pick Sfx", "Die", "Speech", "Grunting"

---

### 3.7 Spatial Audio Config

| ประเภท | spatialBlend | maxDistance | rolloff |
|--------|------------|-------------|---------|
| Voice lines | 1 (3D) | 25 | Linear |
| Ambient | 1 (3D) | 35 | Linear |
| Character attacks | 1 (3D) | 15 | Linear |
| Network character | 1 (3D) | 75 | Linear |
| Music / Ping / Announcer | 0 (2D) | — | — |

---

### 3.8 Advanced AudioEffect

- Component-based; รองรับ: random pitch (BasePitch ± MaxPitchChange), fade in/out, delay, repeat, ForceBehaviour modes
- ForceBehaviour: `None`, `ForceAny`, `ForceDifferentSetup`, `ForceSameSetup`

---

### 3.9 Settings & Persistence

**Volume ปรับผ่าน**: `SettingManager` → `AudioManager.ChangeValue(MixerParam, float)`

**dB Conversion**: `volumeDb = Lerp(-80f, 0, value / 100f)` (Linear 0–100 → dB)

**Settings Keys** (ทั้งหมด 12 channels × 2 scope):
- In-game: `isMaster`, `isMusic`, `isAnnouncer`, `isSoundFx`, `isAmbience`, `isPing`, `isVoice`
- Lobby: `isMasterLobby`, `isMusicLobby`, `isSoundFxLobby`, `isAmbienceLobby`, `isHeropick`
- Slider values: MasterVolume, MusicVolume, SoundFxVolume, etc.

**Save Flow**: `SettingManager.tempSettings` → Confirm → `DeltaService.RuntimeSettings.Options.SaveChanges()`

---

## 4. Formulas

### Volume (Linear → dB)
```
volumeDb = Mathf.Lerp(-80f, 0f, value / 100f)
  where value = 0..100 (slider)
  0   → -80 dB (mute)
  100 → 0 dB (reference)
```

### Pitch Variance (AudioEffect)
```
pitch = BasePitch + Random.Range(-MaxPitchChange, +MaxPitchChange)
```

### Announcer Variant Selection
```
variants = Array.FindAll(m_AnnouncerSounds, s => s.name == soundName)
chosen = variants[Random.Range(0, variants.Length)]
```

---

## 5. Edge Cases

| สถานการณ์ | พฤติกรรม |
|-----------|---------|
| `PlayMusic` ถูกเรียกซ้ำ | เพลงเก่าถูกตัดทันที (ไม่มี Crossfade) |
| Sound ไม่พบใน Array | Return โดยไม่ error (silent fail) |
| `EnableSound = false` | Audio call ทั้งหมดถูก skip |
| Voice Line กำลัง play | บล็อก call ใหม่ (`isPlaying` flag) |
| Pool ว่าง | สร้าง AudioSource ใหม่อัตโนมัติ |
| Scene unload | Ambient loop อาจ bleed (ไม่มี auto-stop) ⚠️ |

---

## 6. Dependencies

| ระบบ | ความสัมพันธ์ |
|------|------------|
| **Combat & Skills (C1)** | Trigger SFX / Voice Lines เมื่อ Skill activate |
| **Announcement System (M10)** | เรียก `PlayAnnouncer(soundName)` |
| **Actor System (F1)** | `AudioSendToAnotherPlayer` ติดกับ Actor |
| **Data/Config (F4)** | `BuildSettings.EnableSound`, `RuntimeSettings` |
| **Hero Select UI (P3)** | `PlayAvatarPickSoundFx` เมื่อ Lock In |

---

## 7. Tuning Knobs

| ค่า | ที่อยู่ | ค่าปัจจุบัน | ผลกระทบ |
|-----|--------|------------|---------|
| Default Music Volume | SettingManager | 100 (0 dB) | ความดังเพลง |
| Voice maxDistance | AudioVoiceLine | 25 | รัศมีได้ยิน Voice Line |
| Ambient maxDistance | AudioAmbientTrigger | 35 | รัศมี Ambient |
| Combat SFX maxDistance | ActorCombatAction | 15 | รัศมีเสียง Skill |
| Network Audio maxDistance | AudioSendToAnotherPlayer | 75 | รัศมีเสียง Network |
| Pool Initial Size | AudioPoolManager | 10 | จำนวน AudioSource เริ่มต้น |
| Voice Line cleanup delay | AudioVoiceLine | clip.length + 5s | หน่วงหลัง Voice จบ |

---

## 8. Acceptance Criteria

- [ ] BGM เล่นได้, หยุดได้, เปลี่ยนเพลงได้
- [ ] SFX trigger จาก Skill — ได้ยินในรัศมี 15 หน่วย
- [ ] Announcer เล่น variant แบบสุ่มตาม Sound Name
- [ ] Voice Line ไม่ทับซ้อนกัน (isPlaying block)
- [ ] Volume slider ทุก Category ทำงาน; บันทึกและโหลดได้
- [ ] Hero Pick เสียง Voice + SFX พร้อมกัน
- [ ] Ambient loop เล่นใน Scene; spatial decay ตามระยะ
- [ ] `EnableSound = false` → เงียบทุก channel

---

## Known Issues / TODO

- ⚠️ **ไม่มี BGM Crossfade**: เพลงสับเปลี่ยนทันที — calling code ต้องจัดการ Fade เอง
- ⚠️ **Parallel AudioManager สอง ตัว**: `AudioManager` และ `AudioManagerPointer` อยู่ร่วมกัน — อาจ conflict
- ⚠️ **Ambient ไม่มี Scene Cleanup**: Ambient loop อาจยังเล่นหลัง Scene unload
- ⚠️ **Magic Number Mixer Index**: ใช้ `audioMixGroup[7]` แทนชื่อ — เปราะต่อการจัดเรียงใหม่
- ⚠️ **Typo**: `ResetPasswort`, `FogotPassword` (ใน Auth, ไม่ใช่ Audio — บันทึกไว้เฉย ๆ)
- ⚠️ **Voice Line +5s delay**: `await WaitForSeconds(clip.length + 5f)` — เหตุผลไม่ชัดเจน
