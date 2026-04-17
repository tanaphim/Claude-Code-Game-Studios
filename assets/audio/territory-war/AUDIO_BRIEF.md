# Territory War — Audio Production Brief

**Project**: Delta (MOBA + Territory War meta-game)
**Feature**: Territory War (FT10)
**Brief Version**: 1.0 — 2026-04-09
**Full Spec**: `design/gdd/audio-territory-war.md`

---

## Project Overview

Delta is a competitive MOBA game with a meta-game layer called **Territory War**. Players own cities on a world map of 1,000,000 cities and declare MOBA battles to expand their empire. The audio for Territory War must feel categorically different from the core MOBA — slower, political, and weighty.

**Player fantasy**: Ruler building an empire. Not a warrior in a fight.

---

## Sonic Identity

**Five adjectives**: Regal · Expansive · Tense · Grounded · Ceremonial

**Reference palette**:
- Age of Empires II HD (Bassirou Diagne): sparse plucked strings, distant horns, slow
- Crusader Kings III (Andreas Waldetoft): modal harmonies, solo cello, harpsichord, duduk
- Total War: Three Kingdoms map screen: low brass, percussion implying scale

**Avoid**: electronic elements, sub-bass drops, gaming energy, upward inflections

---

## Deliverable List

All files delivered as **dry 44100 Hz / 16-bit WAV**. No baked reverb unless noted.

### BGM — Music (11 files)

| File | Duration | Spec |
|---|---|---|
| `mus_territorywar_worldmap_explore_loop.wav` | 3.5–4 min seamless loop | D Dorian, 52–60 BPM. Solo duduk melody, lute/theorbo harmony, cello pad (pp), frame drum accent every 4–8 bars. Contemplative. Loop point must be inaudible. |
| `mus_territorywar_worldmap_tension_loop.wav` | 3.5–4 min seamless loop | Same as explore loop but: duduk melody removed, string pad moves from pp to mp, frame drum more regular (every 2 bars). Must loop cleanly at same bar position as explore loop for mid-bar crossfade. |
| `mus_territorywar_worldmap_empire_loop.wav` | 2–3 min seamless loop | 3 French horns in unison, long notes, very quiet (-12 dB target mix). Low-register brass chorale. Implies growing dominion without celebration. Additive layer played on top of explore/tension. |
| `mus_territorywar_worldmap_bells_loop.wav` | 2–3 min seamless loop | Very distant slow bell pattern. -14 dB target mix. Implies scale of a large empire. Each bell = a city. Additive layer. |
| `mus_territorywar_battle_overlay_loop.wav` | Match length loop (2–4 min) | 2-bar brass ostinato in D Dorian. -8 dB relative to MOBA combat BGM. Texture layer — should not compete with combat music melody. |
| `mus_territorywar_war_declared_sting.wav` | 6–8s, non-loop | 0.3s silence → single struck low bell (long decay) → 2 horns tritone apart (forte, 0.5s) → silence + reverb tail. No resolution. Emotional goal: interruption. |
| `mus_territorywar_war_scheduled_sting.wav` | 4–5s, non-loop | Ascending solo cello, 5 notes, deliberate. Ends on unresolved 7th chord. |
| `mus_territorywar_victory_sting.wav` | 10–12s, non-loop | Solo trumpet fanfare (4 notes, D major, ascending) → string swell (violins/violas/cellos) → held D major chord + bell overlay → fade. Ceremonial and earned, not energetic. |
| `mus_territorywar_defeat_sting.wav` | 8–10s, non-loop | Cello section D→C#→C (descending half steps, pp→ppp) → solo duduk phrase (4–5 notes, descending, unresolved) → silence. Somber but not mocking. Chapter closing. |
| `mus_territorywar_vassal_gained_sting.wav` | 5–6s, non-loop | Solo horn herald call + 2 bars low string pizzicato. Simple, functional, imperial. |
| `mus_territorywar_vassal_lost_sting.wav` | 4s, non-loop | Descending horn phrase. Short, matter-of-fact. |

---

### Ambient (4 files)

| File | Duration | Spec |
|---|---|---|
| `amb_territorywar_worldmap_base_loop.wav` | 90s seamless loop | Very low wind tone (air movement, not howl), distant crowd murmur (-20 dB, heavy reverb — indistinct), intermittent bird call (-18 dB). Target -28 LUFS. |
| `amb_territorywar_worldmap_empire_loop.wav` | 60s seamless loop | Distant market sounds, muffled bells, horses. All heavily reverbed, pitched down slightly. -24 LUFS. Should feel like memory, not literal environment. |
| `amb_territorywar_atwar_tension_loop.wav` | 45–60s seamless loop | Low string drone (one note, bowed cello, slightly detuned), slow breathing-like LFO on low double bass bowing, occasional distant single taiko hit (extreme reverb, -24 dB). -22 LUFS. |
| `amb_territorywar_cityowned_loop.wav` | 30–45s seamless loop | Quiet market sounds, subtle life activity. -26 LUFS for Owned state; will be played at -30 LUFS for Vassal state (same file, different volume). |

---

### Announcer — TW Herald Voice Lines (18 files)

**Voice character**: Male or female (either). Formal, measured, slightly ominous. Royal herald. Slower pace than action-game announcer. No upward inflections. No excitement. Gravitas. Deliver files **dry** — reverb applied at engine level.

**Minimum 2 variants per event** (named `_01_th`, `_02_th`).

| Event | File Pattern | Line (Thai) | Tone |
|---|---|---|---|
| War declared against player | `ann_tw_war_declared_0N_th.wav` | "เมืองของท่านถูกท้าทาย" | Cold, formal |
| War scheduled confirmed | `ann_tw_war_scheduled_0N_th.wav` | "กำหนดการสงครามได้รับการยืนยัน" | Measured |
| Battle warning 60s | `ann_tw_battle_warning_0N_th.wav` | "สงครามจะเริ่มในอีกหนึ่งนาที" | Urgent but controlled |
| Battle starting now | `ann_tw_battle_start_0N_th.wav` | "สงครามเริ่มต้นแล้ว" | Declarative, final |
| Victory — city defended | `ann_tw_victory_defend_0N_th.wav` | "ท่านได้ปกป้องดินแดนของตน" | Dignified |
| Victory — vassal gained | `ann_tw_vassal_gained_0N_th.wav` | "เมืองใหม่อยู่ภายใต้การปกครองของท่าน" | Imperial, satisfied |
| Defeat — city lost to vassal | `ann_tw_defeat_vassal_0N_th.wav` | "เมืองของท่านตกอยู่ภายใต้ผู้อื่น" | Somber, never mocking |
| Vassal freed | `ann_tw_vassal_freed_0N_th.wav` | "เมืองขึ้นหลุดพ้นแล้ว" | Neutral statement |
| Attacker forfeits (no players) | `ann_tw_attacker_forfeit_0N_th.wav` | "ผู้ท้าทายไม่ปรากฏตัว — ชัยชนะเป็นของท่าน" | Matter-of-fact |

---

### SFX — Sound Effects (30 files)

All SFX: dry, no baked reverb. Deliver as 44100 Hz / 16-bit WAV.

#### World Map Navigation

| File | Sound Character | Duration |
|---|---|---|
| `sfx_tw_nav_map_pan_loop.wav` | Deep sustained horn pad (French horns) + creeping low string glissando. Room decay ~0.4s. Loop. | Loop |
| `sfx_tw_nav_zoom_in_01/02.wav` | Ascending crystalline shimmer (shimmering bells E4→B4). Attack 80ms, sustain 140ms, decay 100ms. | 320ms |
| `sfx_tw_nav_zoom_out_01/02.wav` | Descending hollow wind (bowed glass bowl A4→E4). Decay 200ms. | 370ms |
| `sfx_tw_city_hover_01/02.wav` | Single struck temple bowl, concert A (440Hz), rich harmonics. Attack 20ms, sustain to 600ms. ±1 semitone pitch variance. | 600ms |
| `sfx_tw_city_click_01/02.wav` | Bass drum stroke + hi-hat layer, 120ms punch. Transient-heavy. | 320ms |

#### City Interaction

| File | Sound Character | Duration |
|---|---|---|
| `sfx_tw_city_inspect_open_01/02.wav` | Ascending arpeggio on warm bells (C–E–G). Reverb tail 300ms. | 450ms |
| `sfx_tw_city_inspect_close_01/02.wav` | Descending chord release (G–E–C, same bells). Gentle. | 300ms |
| `sfx_tw_city_purchase_01/02/03.wav` | Orchestral fanfare: timpani strike + 3-note brass ascent + string accent. Resonant, ceremonial. 3 variants. | 820ms |
| `sfx_tw_city_upgrade_01/02.wav` | Orchestral swell (strings + brass chord, rising). Less fanfare than purchase. | 700ms |
| `sfx_tw_city_name_confirm_01/02.wav` | Single glockenspiel strike (C6). Quick decay. ±0.5 semitone pitch variance. | 540ms |

#### War Declaration Flow

| File | Sound Character | Duration |
|---|---|---|
| `sfx_tw_war_declare_01/02.wav` | Full orchestra stab: timpani + brass + string accent. Bold, decisive. *Will duck BGM -8 dB.* | 560ms |
| `sfx_tw_war_window_slot_01/02.wav` | Major third rise (two bell tones). Quick, affirming. | 330ms |
| `sfx_tw_war_window_confirm_01/02.wav` | Orchestral resolution chord (brass + strings, C major). Formal. | 450ms |
| `sfx_tw_war_cancel_01/02.wav` | Descending minor chord (strings pizz). Subdued. | 550ms |

#### City State Changes

| File | Sound Character | Duration |
|---|---|---|
| `sfx_tw_state_atwar_01/02.wav` | Two taiko strikes (crescendo) + tension string flutter. | 900ms |
| `sfx_tw_state_vassal_01/02.wav` | Cello section glissando (D→A, pp→ppp). Somber, no crash. | 1050ms |
| `sfx_tw_state_liberated_01/02.wav` | Ascending trumpet fanfare (4 notes, major, bright). | 750ms |
| `sfx_tw_state_vassal_released_01/02.wav` | Single horn chord hit (major, declarative). | 410ms |

#### Notifications (non-critical)

| File | Sound Character | Duration |
|---|---|---|
| `sfx_tw_notif_battle_warning_01/02.wav` | Ascending bell melody (3 notes, arpeggiated). Urgent rhythm. | 1050ms |
| `sfx_tw_notif_result_ping_01.wav` | Quick glockenspiel ping (high register). Brief. | 210ms |

#### Critical Notifications (PCM — zero-latency required)

> ⚠️ These two files are gameplay-critical. Players depend on them for real-time war information. Deliver as PCM (not Vorbis compressed). Maximum clarity.

| File | Sound Character | Duration |
|---|---|---|
| `sfx_tw_notif_war_declared_01/02.wav` | Double stab alert: two struck metal bowls (staggered 80ms). Attention-grabbing but not panic-inducing. | 460ms |
| `sfx_tw_notif_battle_start_01/02.wav` | Aggressive orchestral hit: full ensemble stab, fff. No ambiguity — battle has begun. | 410ms |

#### Social / Invite

| File | Sound Character | Duration |
|---|---|---|
| `sfx_tw_social_invite_01/02.wav` | Warm ascending chime melody (C–E–G–C, 3 bells). Inviting. | 550ms |
| `sfx_tw_social_join_01/02.wav` | Major chord swell (strings) + brushed cymbal accent. | 500ms |
| `sfx_tw_social_teammate_01/02.wav` | Two-note celebratory chime (E5–G5). Short and warm. | 520ms |

---

## Loudness Targets Summary

| Category | Target LUFS | Peak Ceiling |
|---|---|---|
| BGM loops | -18 LUFS | -1 dBTP |
| BGM additive layers | -26 LUFS | -3 dBTP |
| Stings (non-loop) | -14 LUFS | -1 dBTP |
| Ambient loops | -28 LUFS | -6 dBTP |
| Announcer lines | -16 LUFS | -1 dBTP |
| SFX (standard) | -20 LUFS | -3 dBTP |
| SFX (critical) | -18 LUFS | -1 dBTP |

---

## Delivery Format

- **Format**: 44100 Hz / 16-bit WAV, dry (no baked reverb)
- **Stereo**: All files stereo except voice lines (mono acceptable)
- **Naming**: Exactly as listed above — names are referenced directly in Unity
- **Variants**: Numbered `_01`, `_02`, `_03` as specified
- **Language suffix**: Thai files use `_th` suffix

---

## Asset Checklist

Use this checklist to track production progress:

### BGM (11 files)
- [ ] `mus_territorywar_worldmap_explore_loop`
- [ ] `mus_territorywar_worldmap_tension_loop`
- [ ] `mus_territorywar_worldmap_empire_loop`
- [ ] `mus_territorywar_worldmap_bells_loop`
- [ ] `mus_territorywar_battle_overlay_loop`
- [ ] `mus_territorywar_war_declared_sting`
- [ ] `mus_territorywar_war_scheduled_sting`
- [ ] `mus_territorywar_victory_sting`
- [ ] `mus_territorywar_defeat_sting`
- [ ] `mus_territorywar_vassal_gained_sting`
- [ ] `mus_territorywar_vassal_lost_sting`

### Ambient (4 files)
- [ ] `amb_territorywar_worldmap_base_loop`
- [ ] `amb_territorywar_worldmap_empire_loop`
- [ ] `amb_territorywar_atwar_tension_loop`
- [ ] `amb_territorywar_cityowned_loop`

### Announcer (18 files — 9 events × 2 variants)
- [ ] `ann_tw_war_declared_01/02_th`
- [ ] `ann_tw_war_scheduled_01/02_th`
- [ ] `ann_tw_battle_warning_01/02_th`
- [ ] `ann_tw_battle_start_01/02_th`
- [ ] `ann_tw_victory_defend_01/02_th`
- [ ] `ann_tw_vassal_gained_01/02_th`
- [ ] `ann_tw_defeat_vassal_01/02_th`
- [ ] `ann_tw_vassal_freed_01/02_th`
- [ ] `ann_tw_attacker_forfeit_01/02_th`

### SFX (30 files)
- [ ] Navigation: map pan loop, zoom in ×2, zoom out ×2, city hover ×2, city click ×2
- [ ] City: inspect open ×2, inspect close ×2, purchase ×3, upgrade ×2, name confirm ×2
- [ ] War flow: declare ×2, window slot ×2, window confirm ×2, cancel ×2
- [ ] State changes: at war ×2, vassal ×2, liberated ×2, vassal released ×2
- [ ] Notifications: battle warning ×2, result ping ×1, war declared ×2 (CRITICAL), battle start ×2 (CRITICAL)
- [ ] Social: invite ×2, join ×2, teammate ×2

**Total: 63 files**
