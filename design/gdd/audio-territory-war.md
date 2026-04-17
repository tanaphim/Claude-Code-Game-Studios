# Audio Design — Territory War (FT10)

**System ID**: FT10-Audio
**Version**: 1.0.0
**Status**: Draft — Pending Implementation
**Last Updated**: 2026-04-09
**Authors**: Audio Director · Sound Designer · Technical Artist · Gameplay Programmer
**Dependencies**: Audio System (F5), Territory War (FT10), Game Mode Manager (FT7)

---

## 1. Overview

เอกสารนี้ครอบคลุม audio design ทั้งหมดสำหรับ Territory War (FT10) — meta-game mode ที่ซ้อนอยู่บน World Map ขนาดใหญ่ ออกแบบให้ผู้เล่นรู้สึกเหมือน **ผู้ปกครองที่สร้างอาณาจักร** ผ่าน 3 ชั้น: World Map Exploration, War Declaration & Scheduling, และ MOBA Battle

ระบบ audio สำหรับ Territory War แบ่งเป็น 4 ส่วนหลัก:
1. **Sonic Identity & Music Direction** — กำหนดโดย Audio Director
2. **SFX Specifications** — กำหนดโดย Sound Designer
3. **Technical Implementation** — กำหนดโดย Technical Artist
4. **Code Integration** — กำหนดโดย Gameplay Programmer

---

## 2. Player Fantasy

ผู้เล่นได้ยินเสียงที่สะท้อนน้ำหนักและความหมายของการสร้างอาณาจักร — ไม่ใช่แค่เสียงเกม BGM บน World Map ทำให้รู้สึกว่าโลกนี้ใหญ่โตและเต็มไปด้วยผู้คน การประกาศสงครามมีเสียงที่รู้สึกเหมือน "บางอย่างเปลี่ยนไปแล้ว" ชัยชนะฟังดูสง่างามเหมือน Ceremony ไม่ใช่แค่ Game Over Screen

---

## 3. Sonic Identity

**Five defining adjectives**: Regal · Expansive · Tense · Grounded · Ceremonial

Territory War ต้องฟังดูแตกต่างจาก MOBA core อย่างชัดเจนในทุกชั้น ยกเว้น Battle เอง MOBA audio เน้น reactive และ combat-focused — Territory War เน้น slower, political, และ weighty

**Reference palette:**
- Age of Empires II HD (Bassirou Diagne): sparse plucked strings, distant horns, unhurried tempo
- Crusader Kings III (Andreas Waldetoft): modal harmonies (Dorian/Phrygian), solo cello, harpsichord, duduk
- Total War: Three Kingdoms map screen: low brass pedal tones, percussion ที่ implied scale

**What to avoid**: electronic elements, sub-bass drops, gaming energy. Territory War ต้องฟังดูเหมือน strategy ไม่ใช่ action

---

## 4. Music Direction

### 4.1 World Map Theme

| Parameter | Value |
|---|---|
| File | `mus_territorywar_worldmap_explore_loop.ogg` |
| Tempo | 52–60 BPM, no driving pulse |
| Instrumentation | Solo duduk/english horn (melody), lute/theorbo (harmony), cello pad (background), frame drum accent every 4–8 bars |
| Harmonic language | D Dorian mode — melancholy + nobility |
| Duration | 3.5–4 minute seamless loop |
| Mood | Contemplative — weight of a large world |

**⚠️ Technical note**: ระบบ BGM ปัจจุบันไม่มี crossfade (known issue F5) Calling code ต้องใช้ `AudioSourceExtensions.FadeOut/FadeIn` ก่อน `PlayMusic()` สำหรับ TW contexts

### 4.2 Adaptive BGM Layers

| Threshold | Additional Layer | File | Behavior |
|---|---|---|---|
| 5+ cities | Empire brass layer | `mus_territorywar_worldmap_empire_loop.ogg` | 3 French horns, very quiet (-12 dB relative), crossfade in over 8s |
| 20+ cities | Bells layer | `mus_territorywar_worldmap_bells_loop.ogg` | Slow bell pattern, -14 dB, implies scale of empire |
| Below threshold | Layer fades out | — | Crossfade out over 6s |

### 4.3 War Declaration Sting

| Parameter | Value |
|---|---|
| File | `mus_territorywar_war_declared_sting.ogg` |
| Duration | 6–8 seconds, non-looping |
| Structure | 0.3s silence → low bell strike → 2 horns tritone apart (0.5s) → silence + reverb tail |
| Emotional goal | Interruption — "something has changed and it matters" |
| Routing | Announcer ch8 (cuts through regardless of other settings) |
| BGM duck | -6 dB for 3 seconds, recover over 2s |

### 4.4 War Scheduled Confirmation Sound

| Parameter | Value |
|---|---|
| File | `mus_territorywar_war_scheduled_sting.ogg` |
| Duration | 4–5 seconds |
| Structure | Ascending solo cello (5 notes, deliberate), ends on unresolved 7th chord |
| Emotional goal | Formal declaration being sealed — not triumphant |

### 4.5 Battle Music — Territory War Variant

| Parameter | Value |
|---|---|
| File | `mus_territorywar_battle_overlay_loop.ogg` |
| Description | 2-bar brass ostinato in D Dorian, plays OVER existing MOBA combat BGM |
| Volume | -8 dB relative to main combat BGM (texture, not competing melody) |
| When active | Only for 3v3, 5v5, 25v25 (not 1v1 — City Level 1 stays on standard MOBA music) |
| Trigger | `GameModeEvents.OnTWMatchOriginConfirmed` |

### 4.6 Victory Sting — Territory War Specific

| Parameter | Value |
|---|---|
| File | `mus_territorywar_victory_sting.ogg` |
| Duration | 10–12 seconds, non-looping |
| Structure | Solo trumpet fanfare (4 notes, D major) → string swell → held D major chord + bell overlay → fade |
| Emotional goal | Ceremonial and earned — ruler receiving tribute. Slower and more dignified than MOBA victory |

### 4.7 Defeat Sting — Territory War Specific

| Parameter | Value |
|---|---|
| File | `mus_territorywar_defeat_sting.ogg` |
| Duration | 8–10 seconds, non-looping |
| Structure | Cello section descending D→C#→C (pp) → solo duduk phrase (4–5 notes, descending, unresolved) → silence |
| Emotional goal | Chapter closing, not game over — reinforce "defeat is the start of planning a comeback" |

### 4.8 Post-Result Stings

| Event | File | Duration | Structure |
|---|---|---|---|
| Vassal Gained | `mus_territorywar_vassal_gained_sting.ogg` | 5–6s | Solo horn herald call + 2 bars low string pizzicato |
| Vassal Lost | `mus_territorywar_vassal_lost_sting.ogg` | 4s | Descending horn phrase, short, matter-of-fact |

---

## 5. Ambient Direction

### 5.1 World Map Ambient

| Parameter | Value |
|---|---|
| File | `amb_territorywar_worldmap_base_loop.ogg` |
| Duration | 90s seamless loop |
| Contents | Very low wind tone, distant crowd murmur (-20 dB, heavy reverb), intermittent bird call (-18 dB) |
| Spatial | 2D (player is above the map, not in it), ch10 Ambience |
| Target loudness | -28 LUFS (background texture only) |

**Empire ambient layer** (`amb_territorywar_worldmap_empire_loop.ogg`): market sounds, muffled bells, horses — all heavily reverbed — triggers at zoom into dense empire region. -24 LUFS.

### 5.2 City Inspector State Ambients

| City State | File | Fade In | Volume | Contents |
|---|---|---|---|---|
| Unclaimed | — | — | Silent | Silence implies emptiness |
| Owned | `amb_territorywar_cityowned_loop.ogg` | 2s | -26 LUFS | Quiet market sounds, sense of activity |
| At War | `amb_territorywar_atwar_tension_loop.ogg` | 4s | -22 LUFS | Low string drone (slightly detuned), slow LFO pad, distant taiko hit |
| Vassal | `amb_territorywar_cityowned_loop.ogg` | 2s | -30 LUFS | Same as Owned but quieter — functioning under constraint |

**On close**: All city inspector ambient layers fade out over 2 seconds.

---

## 6. Announcer Direction

### 6.1 Voice Character

Territory War requires a **separate announcer voice** from the combat MOBA announcer.

**Character brief**: Herald/advisor — formal, measured, slightly ominous. Royal herald announcing court news. Pacing slower than MOBA announcer. No upward inflections. No excitement. Gravitas.

**Reverb treatment**: Light hall reverb (0.8s decay, early reflections at -12 dB relative to dry). Applied at AudioMixer level — source files must be delivered **dry**.

**Language**: Thai first (th). English if bilingual launch scope.

### 6.2 Announcer Event List

All files: `ann_tw_[event]_[variant]_[lang].ogg` — minimum 2 variants per event.

| Event | File Pattern | Thai Placeholder | Tone |
|---|---|---|---|
| War Declared — received | `ann_tw_war_declared_0N_th.ogg` | "เมืองของท่านถูกท้าทาย" | Cold, formal |
| War Scheduled — confirmed | `ann_tw_war_scheduled_0N_th.ogg` | "กำหนดการสงครามได้รับการยืนยัน" | Measured |
| Battle Starting — 60s | `ann_tw_battle_warning_0N_th.ogg` | "สงครามจะเริ่มในอีกหนึ่งนาที" | Urgent but controlled |
| Battle Starting — now | `ann_tw_battle_start_0N_th.ogg` | "สงครามเริ่มต้นแล้ว" | Declarative |
| Victory — city defended | `ann_tw_victory_defend_0N_th.ogg` | "ท่านได้ปกป้องดินแดนของตน" | Dignified |
| Victory — vassal gained | `ann_tw_vassal_gained_0N_th.ogg` | "เมืองใหม่อยู่ภายใต้การปกครองของท่าน" | Imperial, satisfied |
| Defeat — city to vassal | `ann_tw_defeat_vassal_0N_th.ogg` | "เมืองของท่านตกอยู่ภายใต้ผู้อื่น" | Somber |
| Vassal freed | `ann_tw_vassal_freed_0N_th.ogg` | "เมืองขึ้นหลุดพ้นแล้ว" | Neutral statement |
| Attacker forfeits | `ann_tw_attacker_forfeit_0N_th.ogg` | "ผู้ท้าทายไม่ปรากฏตัว — ชัยชนะเป็นของท่าน" | Matter-of-fact |

**Priority rule**: TW announcer lines interrupt (not queue behind) any active MOBA announcer when TW UI is foreground. When MOBA match is active, TW lines queue with minimum 2-second gap.

---

## 7. SFX Specifications

### 7A. World Map Navigation SFX

| File | Trigger | Sound Character | Duration (ms) | Channel | Spatial | Volume | Pitch Var | Variants |
|---|---|---|---|---|---|---|---|---|
| `sfx_tw_nav_map_pan_loop.ogg` | Map drag (continuous) | Deep sustained horn pad (French horns) + low string glissando. Room decay ~0.4s | Loop | ch11 | 2D | 0.3 | None | 1 |
| `sfx_tw_nav_zoom_in_01.ogg` | Zoom in | Ascending crystalline shimmer (shimmering bells E4→B4). Attack 80ms, decay 100ms | 320 | ch11 | 2D | 0.5 | None | 2 |
| `sfx_tw_nav_zoom_out_01.ogg` | Zoom out | Descending hollow wind (bowed glass bowl A4→E4). Decay 200ms | 370 | ch11 | 2D | 0.5 | None | 2 |
| `sfx_tw_city_hover_01.ogg` | City node hover | Single struck temple bowl (concert A, 440Hz, rich harmonics). Attack 20ms, sustain to 600ms | 600 | ch11 | 2D | 0.45 | ±1 semitone | 2 |
| `sfx_tw_city_click_01.ogg` | City node click | Bass drum stroke + hi-hat layer (120ms total punch). Transient-heavy | 320 | ch11 | 2D | 0.6 | None | 2 |

### 7B. City Interaction SFX

| File | Trigger | Sound Character | Duration (ms) | Channel | Spatial | Volume | Pitch Var | Variants |
|---|---|---|---|---|---|---|---|---|
| `sfx_tw_city_inspect_open_01.ogg` | Inspector panel opens | Ascending arpeggio on warm bells (3 notes, C-E-G). Reverb tail 300ms | 450 | ch11 | 2D | 0.55 | None | 2 |
| `sfx_tw_city_inspect_close_01.ogg` | Inspector panel closes | Descending chord release (G-E-C, same bells). Gentle | 300 | ch11 | 2D | 0.45 | None | 2 |
| `sfx_tw_city_purchase_01.ogg` | City purchased | Orchestral fanfare: timpani strike + 3-note brass ascent + string accent. Resonant and ceremonial | 820 | ch11 | 2D | 0.75 | None | 3 |
| `sfx_tw_city_upgrade_01.ogg` | City upgrade confirmed | Orchestral swell (strings + brass chord, rising). Less fanfare than purchase | 700 | ch11 | 2D | 0.65 | None | 2 |
| `sfx_tw_city_name_confirm_01.ogg` | City name set | Single bright chime (glockenspiel strike, C6). Quick decay | 540 | ch11 | 2D | 0.5 | ±0.5 semitone | 2 |

### 7C. War Declaration Flow SFX

| File | Trigger | Sound Character | Duration (ms) | Channel | Spatial | Volume | Pitch Var | Priority |
|---|---|---|---|---|---|---|---|---|
| `sfx_tw_war_declare_01.ogg` | Player initiates war | Full orchestra stab: timpani + brass + string accent. Bold and decisive. (**Ducks BGM -8 dB / 2s**) | 560 | ch11 | 2D | 0.8 | None | Critical |
| `sfx_tw_war_window_slot_01.ogg` | Time slot selected | Major third rise (two bell tones). Quick, affirming | 330 | ch11 | 2D | 0.5 | None | Normal |
| `sfx_tw_war_window_confirm_01.ogg` | War schedule confirmed | Orchestral resolution chord (brass + strings, C major). Formal | 450 | ch11 | 2D | 0.6 | None | Normal |
| `sfx_tw_war_cancel_01.ogg` | War withdrawn | Descending minor chord (strings pizz). Subdued, no drama | 550 | ch11 | 2D | 0.5 | None | Normal |

### 7D. City State Change SFX

| File | Trigger | Sound Character | Duration (ms) | Channel | Spatial | Volume | Pitch Var | Priority |
|---|---|---|---|---|---|---|---|---|
| `sfx_tw_state_atwar_01.ogg` | City enters At War | Urgent escalating pulse: two taiko strikes (crescendo) + tension string flutter | 900 | ch11 | 2D | 0.7 | None | Normal |
| `sfx_tw_state_vassal_01.ogg` | City becomes Vassal (defeat) | Descending heavy sigh: cello section glissando (D→A, pp→ppp). Somber, no crash | 1050 | ch11 | 2D | 0.65 | None | Normal |
| `sfx_tw_state_liberated_01.ogg` | City recaptured / liberated | Ascending trumpet fanfare (4 notes, major, bright). Shorter than victory sting | 750 | ch11 | 2D | 0.7 | None | Normal |
| `sfx_tw_state_vassal_released_01.ogg` | Vassal released by ruler | Declarative major chord (horns, single hit). Functional, not celebratory | 410 | ch11 | 2D | 0.55 | None | Normal |

### 7E. Notification Sounds

| File | Trigger | Sound Character | Duration (ms) | Channel | Spatial | Volume | Priority |
|---|---|---|---|---|---|---|---|
| `sfx_tw_notif_war_declared_01.ogg` | War declared against player | Double stab alert: two struck metal bowls (staggered 80ms). Attention-grabbing but not panic | 460 | **ch8** | 2D | 0.85 | **CRITICAL** |
| `sfx_tw_notif_battle_warning_01.ogg` | Battle in 60 seconds | Ascending bell melody (3 notes, arpeggiated). Urgent rhythm | 1050 | ch8 | 2D | 0.8 | **CRITICAL** |
| `sfx_tw_notif_battle_start_01.ogg` | Battle starting now | Aggressive orchestral hit: full ensemble stab, fff. No ambiguity | 410 | **ch8** | 2D | 0.9 | **CRITICAL** |
| `sfx_tw_notif_result_ping_01.ogg` | War result available | Quick bright ping (glockenspiel, high register). Brief acknowledgment | 210 | ch12 | 2D | 0.6 | Normal |

**Critical notifications** (`war_declared_01`, `battle_start_01`): routed via `AudioSource.priority = 0` — cannot be voice-stolen.

### 7F. Social / Invite SFX

| File | Trigger | Sound Character | Duration (ms) | Channel | Spatial | Volume | Rate Limit |
|---|---|---|---|---|---|---|---|
| `sfx_tw_social_invite_01.ogg` | Receive war party invite | Warm ascending chime melody (3 bells, C-E-G-C). Inviting, not urgent | 550 | ch11 | 2D | 0.6 | None |
| `sfx_tw_social_join_01.ogg` | Accept invite / join team | Major chord swell (strings) with brushed cymbal accent | 500 | ch11 | 2D | 0.65 | None |
| `sfx_tw_social_teammate_01.ogg` | New player joins war team | Two-note celebratory chime (E5-G5). Short and warm | 520 | ch11 | 2D | 0.55 | **1/2 seconds** |

---

## 8. Audio Priority & Mix Targets

### 8.1 Absolute Priority Order

1. Territory War Announcer lines (time-sensitive: war declared, battle start)
2. Music sting (war declared, victory, defeat)
3. BGM (World Map or battle)
4. Ambience
5. UI SFX

### 8.2 BGM Ducking Rules

| Trigger Sound | Duck Amount | Duration | Recovery |
|---|---|---|---|
| War declaration sting | -6 dB | 3s (via mixer snapshot) | 2s linear |
| War declare button SFX | -8 dB | 2s | 1s linear |
| City purchase SFX | -3 dB | 1s | 0.5s |
| Critical notifications | No duck — they are the priority | — | — |

### 8.3 Loudness Targets

| Category | Target LUFS | Peak Ceiling |
|---|---|---|
| BGM World Map loops | -18 LUFS integrated | -1 dBTP |
| BGM battle overlay layer | -26 LUFS integrated | -3 dBTP |
| Stings (war, victory, defeat) | -14 LUFS integrated | -1 dBTP |
| Ambient (all TW ambs) | -28 LUFS integrated | -6 dBTP |
| Announcer lines | -16 LUFS integrated | -1 dBTP |
| UI SFX | -20 LUFS integrated | -3 dBTP |

### 8.4 Mixer Snapshots

| Snapshot | When | Key Changes |
|---|---|---|
| `TW_WorldMap` | World Map scene active | TWMusic_Base 0 dB, TWMusic_Additive muted |
| `TW_WarDeclared` | War declaration received | TWMusic_Base -6 dB over 3s |
| `TW_Battle` | TW-origin MOBA match | TWMusic_Base -3 dB, allow battle overlay |

---

## 9. Adaptive Audio Rules

### Rule 1 — War Declaration Tension Ramp

**Trigger**: `TerritoryWarEvents.OnWarDeclaredAgainstPlayer`

1. Play war declared sting on ch8
2. Duck `TWMusic_Base` channel -6 dB / 3s via `TW_WarDeclared` snapshot
3. Crossfade World Map BGM: explore variant → tension variant over 2s
4. Tension variant stays until `OnBattleResolved` — then crossfade back over 6s

### Rule 2 — Empire Scale BGM Layering

**Trigger**: `TerritoryWarEvents.OnCityCountChanged(int newCount)`

| City Count | Action |
|---|---|
| < 5 | Base loop only |
| ≥ 5 | Fade in brass additive layer over 8s |
| ≥ 20 | Fade in bells additive layer over 8s |
| Drop below threshold | Fade out corresponding layer over 6s |

Both additive layers use persistent `AudioSource` objects owned by `TerritoryWarAudioController` (not from AudioPool).

### Rule 3 — City Inspector Ambient State Response

**Trigger**: `TerritoryWarEvents.OnCityInspectorOpened(CityState)`

Ambient changes per city state — see Section 5.2. On close: fade out all city inspector ambient over 2s.

---

## 10. Technical Implementation

### 10.1 Audio Bus Structure

No new top-level mixer channels required. Reuse existing channels with 2 new sub-groups added to `DeltaAudioMixer`:

```
DeltaAudioMixer
└── Music (ch7)
    ├── TWMusic_Base        ← World Map BGM primary source
    └── TWMusic_Additive    ← Brass/bells additive layer sources
```

**Exposed mixer parameters**: `TWMusic_Base_Volume` (default 0 dB), `TWMusic_Additive_Volume` (default -80 dB muted)

### 10.2 TerritoryWarAudioController Architecture

**File**: `src/gameplay/territory-war/TerritoryWarAudioController.cs`
**Lifecycle**: Scene-scoped MonoBehaviour (NOT DontDestroyOnLoad)
**Event subscription**: `OnEnable`/`OnDisable` pattern

**Controller-owned AudioSources** (not from pool — persistent looping sources):
```csharp
private AudioSource _additiveSourceBrass;   // tw_bgm_brass, ch7 TWMusic_Additive
private AudioSource _additiveSourceBells;   // tw_bgm_bells, ch7 TWMusic_Additive
private AudioSource _battleOverlaySource;   // tw_battle_overlay, ch7
private AudioSource _ambientSource;         // city inspector ambient, ch10
```

**Events subscribed**:
- `TerritoryWarEvents.OnWarDeclaredAgainstPlayer`
- `TerritoryWarEvents.OnWarScheduled`
- `TerritoryWarEvents.OnBattleStarting`
- `TerritoryWarEvents.OnBattleResolved`
- `TerritoryWarEvents.OnCityInspectorOpened(CityState)`
- `TerritoryWarEvents.OnCityInspectorClosed`
- `TerritoryWarEvents.OnCityCountChanged(int)`
- `GameModeEvents.OnTWMatchOriginConfirmed`

### 10.3 Required Changes to Existing Files

**Blocking (must fix before TW audio):**

| File | Change Required |
|---|---|
| `AudioManager.cs` | Replace `audioMixGroup[N]` with named lookup (cached at Awake) |
| `AudioManager.cs` | Expose `public AudioSource MusicSource { get; private set; }` |
| `AudioManager.cs` | Add `StopAnnouncer()` and `ResumeAnnouncer()` methods |
| `AudioManager.cs` | Expose `public AudioPoolManager AudioPool { get; private set; }` |
| `AudioAmbientTrigger.cs` | Add `OnDestroy()` with FadeOut + pool return (fix ambient bleed) |
| `AudioSourceExtensions.cs` | Add `CrossFade(AudioSource, AudioClip, float)` extension coroutine |

**Non-blocking (quality improvements):**

| File | Change |
|---|---|
| `AudioPoolManager.cs` | Set `m_InitialPoolSize = 32` (from 10) |
| Unity AudioSettings init | `numRealVoices = 32`, `numVirtualVoices = 128`, `dspBufferSize = 512` |

### 10.4 Memory Budget

| Category | Count | Load Type | RAM |
|---|---|---|---|
| BGM streams (buffer only) | 5 | Streaming | 1.3 MB |
| Ambient streams (buffer) | 5 | Streaming | 0.6 MB |
| SFX decompressed | 28 | Decompress on Load (ADPCM) | 9.5 MB |
| Critical SFX | 2 | Decompress on Load (PCM) | 1.0 MB |
| Announcer compressed | 18 | Compressed in Memory (Vorbis Q80) | 1.8 MB |
| **Total TW audio** | | | **14.2 MB** |

Target: < 20 MB. Budget is within target with 5.8 MB headroom.

### 10.5 Compression Settings

| Category | Format | Quality |
|---|---|---|
| BGM (streaming) | Vorbis | Q70 |
| Ambient loops | Vorbis | Q65 |
| SFX (28 files) | ADPCM | — |
| Critical SFX (2 files) | PCM | — (zero latency required) |
| Announcer (18 files) | Vorbis | Q80 |

All source files: 44100 Hz / 16-bit WAV. Do not mix sample rates with existing MOBA assets.

---

## 11. TerritoryWarEvents Interface Requirements

`TerritoryWarManager` (FT10) must expose these events:

```csharp
public static class TerritoryWarEvents
{
    public static event Action OnWarDeclaredAgainstPlayer;
    public static event Action OnWarScheduled;
    public static event Action OnBattleStarting;
    public static event Action<WarResult> OnBattleResolved;
    public static event Action<CityState> OnCityInspectorOpened;
    public static event Action OnCityInspectorClosed;
    public static event Action<int> OnCityCountChanged;
}

public static class GameModeEvents
{
    public static event Action OnTWMatchOriginConfirmed;
}

public enum CityState { Unclaimed, Owned, AtWar, Vassal }
public enum WarResult { Victory, Defeat }
```

---

## 12. Asset File Manifest

### Music (11 files — `assets/audio/territory-war/bgm/`)

```
mus_territorywar_worldmap_explore_loop.ogg
mus_territorywar_worldmap_tension_loop.ogg
mus_territorywar_worldmap_empire_loop.ogg       ← brass additive
mus_territorywar_worldmap_bells_loop.ogg         ← bells additive
mus_territorywar_war_declared_sting.ogg
mus_territorywar_war_scheduled_sting.ogg
mus_territorywar_battle_overlay_loop.ogg
mus_territorywar_victory_sting.ogg
mus_territorywar_defeat_sting.ogg
mus_territorywar_vassal_gained_sting.ogg
mus_territorywar_vassal_lost_sting.ogg
```

### Ambient (5 files — `assets/audio/territory-war/ambient/`)

```
amb_territorywar_worldmap_base_loop.ogg
amb_territorywar_worldmap_empire_loop.ogg
amb_territorywar_atwar_tension_loop.ogg
amb_territorywar_cityowned_loop.ogg
```

*(Unclaimed = silent — no file needed)*

### Announcer (18 files — `assets/audio/territory-war/announcer/`)

```
ann_tw_war_declared_01_th.ogg / 02_th.ogg
ann_tw_war_scheduled_01_th.ogg / 02_th.ogg
ann_tw_battle_warning_01_th.ogg / 02_th.ogg
ann_tw_battle_start_01_th.ogg / 02_th.ogg
ann_tw_victory_defend_01_th.ogg / 02_th.ogg
ann_tw_vassal_gained_01_th.ogg / 02_th.ogg
ann_tw_defeat_vassal_01_th.ogg / 02_th.ogg
ann_tw_vassal_freed_01_th.ogg / 02_th.ogg
ann_tw_attacker_forfeit_01_th.ogg / 02_th.ogg
```

### SFX (30 files — `assets/audio/territory-war/sfx/`)

```
sfx_tw_nav_map_pan_loop.ogg
sfx_tw_nav_zoom_in_01.ogg / 02.ogg
sfx_tw_nav_zoom_out_01.ogg / 02.ogg
sfx_tw_city_hover_01.ogg / 02.ogg
sfx_tw_city_click_01.ogg / 02.ogg
sfx_tw_city_inspect_open_01.ogg / 02.ogg
sfx_tw_city_inspect_close_01.ogg / 02.ogg
sfx_tw_city_purchase_01.ogg / 02.ogg / 03.ogg
sfx_tw_city_upgrade_01.ogg / 02.ogg
sfx_tw_city_name_confirm_01.ogg / 02.ogg
sfx_tw_war_declare_01.ogg / 02.ogg
sfx_tw_war_window_slot_01.ogg / 02.ogg
sfx_tw_war_window_confirm_01.ogg / 02.ogg
sfx_tw_war_cancel_01.ogg / 02.ogg
sfx_tw_state_atwar_01.ogg / 02.ogg
sfx_tw_state_vassal_01.ogg / 02.ogg
sfx_tw_state_liberated_01.ogg / 02.ogg
sfx_tw_state_vassal_released_01.ogg / 02.ogg
sfx_tw_notif_war_declared_01.ogg / 02.ogg      ← CRITICAL priority=0
sfx_tw_notif_battle_warning_01.ogg / 02.ogg
sfx_tw_notif_battle_start_01.ogg / 02.ogg       ← CRITICAL priority=0
sfx_tw_notif_result_ping_01.ogg
sfx_tw_social_invite_01.ogg / 02.ogg
sfx_tw_social_join_01.ogg / 02.ogg
sfx_tw_social_teammate_01.ogg / 02.ogg
```

**Total assets: 64 files**

---

## 13. Unit Test Plan (15 Tests)

| Suite | Test | Description |
|---|---|---|
| CrossfadeToTensionVariant | 1.1 | Snapshot transition is triggered with correct duration |
| | 1.2 | BGM clip is swapped after crossfade halfway point |
| | 1.3 | Both snapshot AND clip swap trigger together |
| OnCityCountChanged | 2.1 | Brass layer activates at threshold (5) |
| | 2.2 | Brass layer does NOT activate below threshold (4) |
| | 2.3 | Bells layer activates at 20 |
| | 2.4 | At count=5, brass active but bells silent |
| | 2.5 | Both deactivate when count drops below 5 |
| | 2.6 | Count drops from 25 to 8: bells off, brass stays |
| CityInspectorAmbient | 3.1 | Unclaimed → ambient silent |
| | 3.2 | Owned → market clip at `_ownedAmbientVolume` |
| | 3.3 | AtWar → tension drone at `_atWarAmbientVolume` |
| | 3.4 | Vassal → subdued clip at `_vassalAmbientVolume` |
| | 3.5 | Inspector closed → target volume=0 regardless of state |
| Rate Limiting | 4.1 | First call plays |
| | 4.2 | Call within 2s suppressed |
| | 4.3 | Call after 2s allowed |
| | 4.4 | Rate limit does not affect different clip name |
| Critical Priority | 5.1 | `war_declared_against_you` gets `priority=0` |
| | 5.2 | `battle_starting_now` gets `priority=0` |
| | 5.3 | Normal SFX does NOT get priority=0 |

---

## 14. Open Questions

| คำถาม | Owner | สถานะ |
|---|---|---|
| Does `PlayAnnouncer` have an internal queue? (affects `ResumeAnnouncer()` implementation) | Lead Programmer | ⚠️ TBD |
| Actual AudioMixerGroup names in DeltaAudioMixer.mixer (for named lookup migration) | Technical Artist | ⚠️ TBD |
| Exact asset name strings for `tw_bgm_brass`, `tw_bgm_bells`, `tw_battle_overlay` (must match `m_MusicSounds[]` entries) | Technical Artist | ⚠️ TBD |
| Should critical notifications use a dedicated "Critical Alerts" AudioMixerGroup instead of `priority=0`? | Lead Programmer + Audio Director | ⚠️ TBD |
| Who owns City Inspector panel visibility — UI Controller or TerritoryWarManager? (determines who fires `OnCityInspectorOpened`) | Lead Programmer | ⚠️ TBD |

---

## 15. Acceptance Criteria

- [ ] World Map BGM plays seamlessly on World Map scene load (explore loop, Dorian, 52–60 BPM)
- [ ] BGM crossfades smoothly between explore and tension variants on war declaration (no hard cut)
- [ ] Brass additive layer fades in when player owns 5+ cities; fades out when drops below threshold
- [ ] Bells additive layer fades in at 20+ cities; independent of brass layer
- [ ] War declared sting plays on ch8 when `OnWarDeclaredAgainstPlayer` fires; BGM ducks -6 dB
- [ ] City inspector ambient changes within 2 seconds of opening inspector based on CityState
- [ ] Victory sting plays on war resolution (victory); Defeat sting plays on defeat — both distinct from MOBA stings
- [ ] Battle overlay loop active during 3v3/5v5/25v25 TW-origin matches; NOT during 1v1
- [ ] Territory War announcer voice is distinct from MOBA announcer (different voice character)
- [ ] All 9 announcer event types play correct lines; 2 variants per event (no repetition on back-to-back)
- [ ] `war_declared_against_you` and `battle_starting_now` cannot be suppressed by voice stealing (`priority=0` enforced)
- [ ] Team join chime fires at most once per 2 seconds regardless of how many players join rapidly
- [ ] All TW ambient layers stop cleanly when transitioning from World Map to MOBA scene (no audio bleed)
- [ ] Total TW audio RAM usage < 20 MB at runtime
- [ ] AudioPool size ≥ 32 — no auto-grow allocations during 25v25 battle + TW notifications
- [ ] `AudioManager` uses named mixer group lookup (no `audioMixGroup[N]` magic numbers)
