# Territory War — SFX Specification

**System ID**: FT10  
**Audio Designer**: Sound Designer Agent  
**Last Updated**: 2026-04-09  
**Sonic Identity**: Regal, Expansive, Tense, Grounded, Ceremonial  
**Reference Palette**: Age of Empires II, Crusader Kings III, Total War  

---

## 1. Overview

This document specifies every sound effect for Territory War—the world map meta-game system where players own cities, declare wars, and battle to expand their empires. All SFX are designed to reinforce the regal-yet-grounded fantasy of commanding a growing domain: sounds are orchestral when consequential (war declaration, city purchase), earthy and spatial for world interactions (map panning, city hovers), and urgent for time-critical notifications (battle warnings).

**Audio Routing**:
- **SFX Channel** (ch11): `PlaySoundFx(name)` — 3D spatial, maxDist=15m, one-shot async
- **Announcer Channel** (ch8): `PlayAnnouncer(name)` — 2D, non-spatial, one-shot async
- **Ping Channel** (ch12): `PlayPing(index)` — 2D, non-spatial, one-shot async
- **All one-shots**: Managed by `AudioPoolManager` (auto-return after duration)

---

## 2. Sound Categories & Reference

### Sonic Palette Guidelines

| Category | Character | Instruments | Emotional Tone | Frequency Focus |
|----------|-----------|-------------|-----------------|-----------------|
| **World Navigation** | Subtle, low-register, continuous | Distant horns, low strings, earth drums | Expansive, worldly | 60–200 Hz (sub-bass), mids at 250–500 Hz |
| **City Interactions** | Clear, declarative, grounded | Struck metals, resonant wood, orchestral hits | Ownership, control | 1–6 kHz (presence peaks) |
| **War Declaration** | Ceremonial, powerful, anthemic | Full orchestra, war horns, timpani, low brass | Authority, consequence | 80–150 Hz (bass anchor), 2–4 kHz (brass clarity) |
| **State Changes** | Atmospheric, distinct per state | Ambient swells, tonal shifts, sparse instrumentation | Transition tension | Variable by state |
| **Notifications** | Piercing, attention-grabbing, musical | High-pitched bells, shimmering textures, stabs | Urgency without aggression | 2–8 kHz |
| **Social/Team** | Warm, inviting, celebratory | Uplifting horns, choral elements, resonant metals | Camaraderie, readiness | 200–800 Hz, 2–3 kHz |

---

## 3. World Map Navigation SFX

### 3.1 Map Pan/Drag

**Purpose**: Subtle audio feedback while player drags the map viewport, reinforces scale and worldliness.

| **File Name** | `map_pan_continuous_loop` |
|---|---|
| **Trigger** | Every frame while map drag active |
| **Sound Character** | Deep, sustained horn pad (French horns in unison) beneath a creeping low string glissando. Room ambience decay ~0.4s. Doppler effect audible as view pans. **No percussive attack.** Pure continuous motion sound. Tone: F2–C3 range. |
| **Duration (ms)** | Infinite loop (fade in/out with drag start/stop) |
| **Channel** | SFX (ch11) |
| **Spatial** | 2D (listener relative) — no 3D positioning needed |
| **Volume** | 0.35 (quiet, background layer) |
| **Pitch Variance** | None — steady continuous |
| **Variants** | 1 (single asset) |
| **Streaming** | No (short loop ~3–4 sec, loaded once) |
| **Special Notes** | Triggered on `OnMapDragStart()`, looped while dragging, crossfaded out on `OnMapDragEnd()` over 200ms. Pitch/playback speed may vary slightly (±5%) per drag speed for subtle immersion. |

**Audio Technique**:
- French horns (4-part blend) at 60–100 Hz fundamental, slight swell every 2 seconds
- String glissando (8 violins in thirds, F#2→G3, linear 3-second sweep)
- Room reverb impulse response (medium hall, 400ms pre-delay, 0.5s tail)
- Gentle lowpass filter sweep (20 Hz/second) if dragging accelerates

---

### 3.2 Map Zoom In

**Purpose**: Satisfying, ascending feedback as the player zooms the camera closer to the map.

| **File Name** | `map_zoom_in_ascend` |
|---|---|
| **Trigger** | Mousewheel up (zoom in) OR pinch gesture (mobile) |
| **Sound Character** | Ascending crystalline shimmer (shimmering bells, analogue synth rise). Starts muffled, quickly clears into presence. Attack: 80ms, sustain: 140ms, decay: 100ms. Pitch glissando: E4→B4. Tone: metallic, ethereal, non-threatening. Room reverb light (plate 0.3s). |
| **Duration (ms)** | 320 total (80 attack + 140 sustain + 100 decay) |
| **Channel** | SFX (ch11) |
| **Spatial** | 2D (listener-relative) |
| **Volume** | 0.50 |
| **Pitch Variance** | None — deterministic glissando |
| **Variants** | 1 |
| **Streaming** | No (short, one-shot) |
| **Special Notes** | Non-repeating. Pitch glissando is integral to the sound; do not randomize. |

**Audio Technique**:
- Sine wave cluster (E4, G#4, B4, D5) with envelope shaping
- Lowpass cutoff automationn: closed (2 kHz) at start, opens to 8 kHz at sustain, decays back to 3 kHz
- Plate reverb (0.3s decay)
- Compressor (-12 dB GR at peak) to prevent harshness

---

### 3.3 Map Zoom Out

**Purpose**: Descending, pulling-back sensation as the player zooms the camera away from the map.

| **File Name** | `map_zoom_out_descend` |
|---|---|
| **Trigger** | Mousewheel down (zoom out) OR reverse pinch gesture |
| **Sound Character** | Descending, hollow wind sound (layered wind samples). Starts clear at presence, loses definition as it descends. Attack: 100ms, sustain: 120ms, decay: 150ms. Pitch glissando: B4→E3. Tone: airy, vast, echoing. Room reverb medium (cathedral, 0.6s). |
| **Duration (ms)** | 370 total (100 + 120 + 150) |
| **Channel** | SFX (ch11) |
| **Spatial** | 2D |
| **Volume** | 0.45 |
| **Pitch Variance** | None — deterministic glissando |
| **Variants** | 1 |
| **Streaming** | No |
| **Special Notes** | Non-repeating. Inverse complement to zoom-in sound. Should feel like pulling the camera back from a model. |

**Audio Technique**:
- Bandpass-filtered pink noise, center frequency gliding B4→E3 over 320ms
- Highpass filter closes from 10 kHz to 200 Hz (tracking pitch descent)
- Cathedral reverb (0.6s decay, 1.2s pre-delay for spaciousness)
- No attack kick; starts at sustain level (fade-in envelope)

---

### 3.4 City Node Hover

**Purpose**: Subtle, warm confirmation when the player hovers over a city node. Indicates interactivity.

| **File Name** | `city_hover_tone` |
|---|---|
| **Trigger** | Mouse enters city node collider (UI raycast) |
| **Sound Character** | Single struck temple bowl (tam-tam), resonant, warm. Immediate attack (20ms), sustain audible until ~600ms. Pitch: concert A (440 Hz fundamental + rich harmonics 880 Hz, 1320 Hz, 1760 Hz). Room reverb intimate (small hall, 0.4s). No artifacting. Organic, wooden tone quality. |
| **Duration (ms)** | 600 (attack 20ms, sustain/decay gradual) |
| **Channel** | SFX (ch11) |
| **Spatial** | 2D (listener-relative, not world-space) |
| **Volume** | 0.40 |
| **Pitch Variance** | None — fixed pitch |
| **Variants** | 1 |
| **Streaming** | No |
| **Special Notes** | Plays once per hover enter. Does NOT repeat while hovering. Debounced: minimum 200ms between repeated hovers on same node. |

**Audio Technique**:
- Tam-tam / gong strike (real recording or high-quality synthesis)
- Pitch centered at 440 Hz with natural harmonic series present
- Very slight pitch decay (natural, not EQ sweep) due to harmonic damping
- Small room reverb (tile booth, 0.35s)
- No compression (preserve transient clarity)

---

### 3.5 City Node Click / Select

**Purpose**: Affirming, definitive click that confirms the player has selected a city node and is entering interaction mode.

| **File Name** | `city_click_seal` |
|---|---|
| **Trigger** | Left-click on city node (confirmed selection) |
| **Sound Character** | Orchestral impact: struck bass drum (low attack, 40–60 Hz) + closed hi-hat scrape (5–8 kHz bright transient, ~80ms). Layered: bass drum sustains 200ms, hat decays to silence in 120ms. Together: authoritative, seal-of-ownership tone. Room reverb tight (0.25s, medium hall). No washy quality. |
| **Duration (ms)** | 320 (bass drum sustain 200ms, hat decay overlaid to 320ms total) |
| **Channel** | SFX (ch11) |
| **Spatial** | 2D |
| **Volume** | 0.60 (noticeable but not startling) |
| **Pitch Variance** | None |
| **Variants** | 1 |
| **Streaming** | No |
| **Special Notes** | Strong attack needed for tactile feedback. Dual-layer design (bass + treble) provides richness without mud. |

**Audio Technique**:
- Layer 1: Orchestral bass drum, mallet strike, 50 Hz fundamental, bandpassed 30–200 Hz
- Layer 2: Closed hi-hat or small cymbal scrape, bright transient at 6 kHz
- Combined in stereo (L/R spread ~50ms for width without panning)
- Tight hall reverb (0.25s, no pre-delay)
- Moderate compression (1:2 ratio, -6 dB GR, fast attack) to glue layers

---

## 4. City Interaction SFX

### 4.1 City Inspect Panel Open

**Purpose**: Welcoming, opening gesture as the city inspection/management panel slides onto screen.

| **File Name** | `city_inspect_panel_open` |
|---|---|
| **Trigger** | City panel transitions from hidden to visible (slide-in animation starts) |
| **Sound Character** | Ascending arpeggio of orchestral bells (glockenspiel-like, bright but warm), A3–C#4–E4 (A major triad, ~3 bell strikes over 400ms, evenly spaced). Each bell has ~150ms decay. Stereo spread (L bell → center bell → R bell). Reverb: small room (0.2s, intimate). |
| **Duration (ms)** | 450 (arpeggio 400ms + tail 50ms) |
| **Channel** | SFX (ch11) |
| **Spatial** | 2D |
| **Volume** | 0.55 |
| **Pitch Variance** | None — fixed pitches |
| **Variants** | 1 |
| **Streaming** | No |
| **Special Notes** | Tempo of arpeggio should sync with UI panel animation duration (typically 300–500ms). If panel animation is faster/slower, adjust sound timing proportionally. |

**Audio Technique**:
- Glockenspiel samples (3 pitches: A3, C#4, E4) or FM bell synthesis
- Each bell triggered with ~130ms spacing
- Individual bell decay envelope (attack 40ms, decay 150ms) with reverb tail
- Stereo panning: L bell −30%, center 0%, R bell +30%
- No compression (preserve resonance)

---

### 4.2 City Inspect Panel Close

**Purpose**: Closing, settling gesture as the panel exits the screen.

| **File Name** | `city_inspect_panel_close` |
|---|---|
| **Trigger** | City panel transitions from visible to hidden (slide-out animation) |
| **Sound Character** | Descending, soft chord release (warm strings, A2–E2–C#2, played together then fading). Sustain only 200ms, then crossfaded to silence. No attack; soft fade-in at start. Tone: mellow, rounded, non-harsh. Room reverb intimate (0.3s). |
| **Duration (ms)** | 300 total (fade-in 50ms, sustain 150ms, fade-out 100ms) |
| **Channel** | SFX (ch11) |
| **Spatial** | 2D |
| **Volume** | 0.40 (quieter than open, less assertive) |
| **Pitch Variance** | None |
| **Variants** | 1 |
| **Streaming** | No |
| **Special Notes** | Soft, non-jarring closure. Inverse of panel-open (both pitch and energy-wise). |

**Audio Technique**:
- Warm pad (viola/cello unison, A2–E2–C#2 played together, no chords)
- Fade-in envelope (50ms) to avoid click
- Natural decay of string samples (no abrupt cutoff)
- Plate reverb (0.3s decay, intimate)
- Low-pass filter at 3 kHz to reduce edge

---

### 4.3 City Purchase (Buying Unclaimed City)

**Purpose**: Triumphant, satisfying confirmation of acquisition. This is a major milestone—the player now owns a city.

| **File Name** | `city_purchase_fanfare_short` |
|---|---|
| **Trigger** | Purchase transaction confirmed; city state changes from Unclaimed → Owned |
| **Sound Character** | Brief orchestral fanfare: three rising trumpet/horn stabs (brass section, F3→A3→D4, each ~200ms, separated by 100ms silence). Each stab has snappy attack (30ms), sustain (120ms), decay (50ms). Underlying timpani roll (low frequency, 60–80 Hz, building under the horns). Final decay with reverb tail ~0.4s. Tone: regal, triumphant, ceremonial. |
| **Duration (ms)** | 820 total (300ms + 100ms + 300ms + 100ms + reverb tail 120ms) |
| **Channel** | SFX (ch11) |
| **Spatial** | 2D |
| **Volume** | 0.75 (prominent, celebratory) |
| **Pitch Variance** | None — fixed pitches |
| **Variants** | 1 (or 2–3 variants with minor melody tweaks, see variants section below) |
| **Streaming** | No |
| **Special Notes** | This is a reward sound—make it feel earned and exciting. Should be clearly heard even if BGM is present. May warrant ducking BGM by -6 dB during playback. |

**Audio Technique**:
- Trumpet/horn layer: bright brass samples, EQ peak at 2.5 kHz for presence
- Timpani roll: continuous roll building from pp to mp over 800ms
- Reverb: medium hall (0.4s decay, 200ms pre-delay)
- Compressor (1:1.5 ratio, -8 dB GR at peak) to control brass peaks
- Master filter cutoff automation: slight brightening during sustain phase

**Variants**:
- **Variant A (base)**: F3→A3→D4 (as described)
- **Variant B**: F3→A3→C#4 (minor variation)
- **Variant C**: D3→F#3→B3 (transposed variant)
- **Strategy**: On first purchase, use A. Subsequent purchases randomly choose A, B, or C to avoid repetition.

---

### 4.4 City Upgrade Confirmation

**Purpose**: Affirmative, constructive tone when a city is upgraded (level increases).

| **File Name** | `city_upgrade_confirmation` |
|---|---|
| **Trigger** | City upgrade transaction completes; City Level increases |
| **Sound Character** | Rising, building orchestral swell: crescendo from pp to mf over 400ms (strings swelling, French horns entering at 250ms mark). Peak held briefly (100ms), then decayed over 200ms. Tone: constructive, hopeful, building strength. No pitched melody—pure harmonic swell on a root chord. Room reverb medium (0.45s). |
| **Duration (ms)** | 700 total (crescendo 400ms + sustain 100ms + decay 200ms) |
| **Channel** | SFX (ch11) |
| **Spatial** | 2D |
| **Volume** | 0.65 |
| **Pitch Variance** | None |
| **Variants** | 1 (swell is harmonic, no melody variation needed) |
| **Streaming** | No |
| **Special Notes** | Smooth, continuous crescendo—no percussive attacks. Pure orchestral swell. Should feel like the city is "growing." |

**Audio Technique**:
- String pad (violins pp, violas, cellos, all on root chord C major)
- French horn entry at 250ms (adds 2nd partial harmonics)
- Volume envelope: crescendo 0–400ms (exponential curve), sustain 400–500ms, decay 500–700ms
- Reverb: medium hall (0.45s decay)
- Compressor (1:2 ratio, -10 dB GR at peak) to smooth crescendo

---

### 4.5 City Name Set / Confirm

**Purpose**: Light, affirming tone when a player sets or confirms a custom name for a city.

| **File Name** | `city_name_confirm_chime` |
|---|---|
| **Trigger** | City rename input confirmed (Submit button clicked, name saved to DB) |
| **Sound Character** | Bright, single-pitched chime (bell-tone, pitched at D5 / 587 Hz). Attack ~40ms, sustain ~200ms with slight pitch decay (natural damping), reverb tail ~300ms. Tone: clear, pleasant, non-aggressive. Similar in character to notification sounds but warmer. |
| **Duration (ms)** | 540 (attack 40ms + sustain 200ms + reverb tail 300ms) |
| **Channel** | SFX (ch11) |
| **Spatial** | 2D |
| **Volume** | 0.50 |
| **Pitch Variance** | None — fixed pitch D5 |
| **Variants** | 1 |
| **Streaming** | No |
| **Special Notes** | Complementary to city_click_seal but higher-pitched and gentler, indicating a soft confirmation rather than authoritative selection. |

**Audio Technique**:
- Pitched bell sample or FM bell synthesis (D5 / 587 Hz, natural harmonics)
- Attack envelope 40ms (sharp but not abrasive)
- Decay with pitch roll-off (natural to bells)
- Plate reverb (0.3s decay)
- No compression needed (bell transient is natural)

---

## 5. War Declaration Flow SFX

### 5.1 War Declaration Button Click (Initiating War)

**Purpose**: Authoritative, momentous tone when a player declares war on another city. This is a major game action.

| **File Name** | `war_declaration_initiate` |
|---|---|
| **Trigger** | War declaration button confirmed (attack city dialog closes, war state enters "Declared") |
| **Sound Character** | Deep, powerful orchestral impact: full orchestra stab (low brass F1–C2 foundation, mid-range strings swelling, high woodwinds sharp attack). Attack 60ms, sustain 200ms, decay 300ms. Underlying timpani roll (40–80 Hz range, 300ms crescendo beneath brass attack). Reverb: large concert hall (0.6s decay, 200ms pre-delay). Tone: regal, consequential, war-like but controlled—not chaotic. |
| **Duration (ms)** | 560 total (impact 60ms attack + 200ms sustain + 300ms decay) |
| **Channel** | SFX (ch11) |
| **Spatial** | 2D |
| **Volume** | 0.80 (prominent, unambiguous) |
| **Pitch Variance** | None |
| **Variants** | 1 |
| **Streaming** | No |
| **Special Notes** | This is a critical moment in gameplay—ensure it's clearly heard. Recommend BGM duck of -8 dB during this sound to prevent muddiness. This sound may benefit from slight lowpass filtering (cutoff ~5 kHz) to maintain clarity even in presence of other UI sounds. |

**Audio Technique**:
- Bass brass layer: trombone + tuba (F1–C2), EQ peak at 80 Hz
- Mid-range strings: violins + violas (G2–D3), EQ peak at 250 Hz
- High woodwinds: flutes + oboes (G4–D5), fast attack at 60ms
- Timpani roll: continuous 40–80 Hz, crescendo under brass
- Concert hall reverb (0.6s decay, 200ms pre-delay)
- Compressor (1:2 ratio, -12 dB GR at peak) to control dynamic range
- Lowpass filter at 5 kHz (gradual rolloff, not steep) to reduce harshness

---

### 5.2 War Window Time Slot Selected

**Purpose**: Confirming gesture when the attacker selects a time slot within the defender's War Window.

| **File Name** | `war_window_timeslot_selected` |
|---|---|
| **Trigger** | Time slot clicked/confirmed in the war schedule calendar dialog |
| **Sound Character** | Ascending, satisfied harmonic resolution: major third rise (C4→E4, played together then releasing). Attack ~80ms, sustain 150ms, decay 100ms. Tone: affirming, resolved, musical. Reverb light (0.25s). Similar energy to city_hover_tone but with more movement. |
| **Duration (ms)** | 330 total (80ms attack + 150ms sustain + 100ms decay) |
| **Channel** | SFX (ch11) |
| **Spatial** | 2D |
| **Volume** | 0.50 |
| **Pitch Variance** | None |
| **Variants** | 1 |
| **Streaming** | No |
| **Special Notes** | Gentle confirmation. Not aggressive or overly triumphant—just affirming that the selection was accepted. |

**Audio Technique**:
- Major third interval: pitched bell or string samples (C4 + E4 played together)
- Attack envelope 80ms (medium snappiness)
- Sustain 150ms, then natural decay
- Small room reverb (0.25s decay)
- No compression needed

---

### 5.3 War Window Schedule Confirmed

**Purpose**: Definitive, locked-in confirmation when both attacker and defender have selected times and the war is now scheduled.

| **File Name** | `war_schedule_confirmed_seal` |
|---|---|
| **Trigger** | War transitions from "Declared" → "Scheduled" state (both parties have confirmed time) |
| **Sound Character** | Orchestral resolution chord (major triad: C3–E3–G3, all struck together). Attack 50ms, sustain 250ms, decay 150ms. Fuller, richer tone than individual chimes—suggests finality and commitment. Tone: ceremonial, binding, official. Room reverb medium (0.35s). |
| **Duration (ms)** | 450 total (50ms + 250ms + 150ms) |
| **Channel** | SFX (ch11) |
| **Spatial** | 2D |
| **Volume** | 0.65 |
| **Pitch Variance** | None |
| **Variants** | 1 |
| **Streaming** | No |
| **Special Notes** | Stronger and more "sealing" than the time-slot selection. This is the point of no return—both parties are committed to the scheduled war. |

**Audio Technique**:
- Orchestral chord (C3–E3–G3): pitched bells or string samples, rich harmonic content
- All three pitches struck together (perfect simultaneous attack at 50ms)
- Reverb: medium hall (0.35s decay, ~100ms pre-delay)
- EQ peak at ~330 Hz for harmonic richness
- No compression (preserve resonance)

---

### 5.4 War Withdrawn / Cancelled

**Purpose**: Somber, release tone when a war is cancelled or withdrawn by either party.

| **File Name** | `war_withdrawn_release` |
|---|---|
| **Trigger** | War state transitions from "Declared" or "Scheduled" back to no-war (player withdraws declaration, or mutual cancellation agreed) |
| **Sound Character** | Descending, hollow chord release (minor triad: A2–C3–E3, played together then fading). Fade-in envelope at start (avoid click), sustain 200ms, decay over 300ms. Tone: somber, melancholic, release. Reverb medium (0.4s, slightly longer than positive events). |
| **Duration (ms)** | 550 total (fade-in 50ms + sustain 200ms + decay 300ms) |
| **Channel** | SFX (ch11) |
| **Spatial** | 2D |
| **Volume** | 0.50 (quieter, less prominent than declaration) |
| **Pitch Variance** | None |
| **Variants** | 1 |
| **Streaming** | No |
| **Special Notes** | Inverse of schedule-confirmed sound. Lower pitches, minor tonality, longer decay. Conveys finality without triumph. |

**Audio Technique**:
- Minor triad samples (A2–C3–E3), warm pad character
- Fade-in envelope 50ms to avoid click
- Sustain 200ms, then exponential decay over 300ms
- Reverb: medium hall (0.4s decay)
- Lowpass filter at 2 kHz (warm, without harshness)
- No compression

---

## 6. City State Change SFX (World Map Events)

### 6.1 City Enters "At War" State

**Purpose**: Alert and dramatic tone when a city on the world map transitions to "At War" (visible to all players). War is now imminent.

| **File Name** | `city_state_at_war_alert` |
|---|---|
| **Trigger** | City state changes: Owned/Vassal → At War (when war time is locked in and battle is scheduled within the next hours) |
| **Sound Character** | Urgent, escalating tone: low frequency pulse (40 Hz sine wave, stuttering every 400ms) beneath a rising orchestral swell (French horns, A2→D3, over 600ms). No aggressive attack—more building dread than immediate threat. Attack: 100ms, sustain/crescendo: 600ms, decay: 200ms. Reverb: large hall (0.5s). Tone: tense, ominous, consequential. |
| **Duration (ms)** | 900 total (build 700ms + decay 200ms) |
| **Channel** | SFX (ch11) |
| **Spatial** | 2D |
| **Volume** | 0.70 (clearly audible, important event) |
| **Pitch Variance** | None — deterministic glissando |
| **Variants** | 1 |
| **Streaming** | No |
| **Special Notes** | This sound is for *all* players who see the city on the map—even spectators not involved in the war. It's a world-map event notification. Different from battle-starting sounds, which happen client-side during the actual battle. |

**Audio Technique**:
- Sub-bass pulse: 40 Hz sine wave, 200ms on / 200ms off, stuttering pattern
- Orchestral swell: French horns (A2→D3), crescendo from pp to mp over 600ms
- Attack envelope on horns: 100ms (gradual increase, not snappy)
- Decay over 200ms to silence
- Large hall reverb (0.5s decay, 150ms pre-delay)
- Compressor (1:1.5 ratio, -6 dB GR) to manage dynamic range

---

### 6.2 City Transitions to "Vassal" (Defeat Result)

**Purpose**: Somber, resigned tone when a city loses a war and becomes vassal to the victor. A major setback, but not complete loss (the player still owns the city).

| **File Name** | `city_state_vassal_defeat` |
|---|---|
| **Trigger** | City state changes: Owned → Vassal (after losing a territorial war) |
| **Sound Character** | Descending, heavy orchestral sigh: low strings (cello + bass, descending from D2→F#1 over 800ms, slow glissando). Underneath, soft timpani roll (60 Hz) decaying from mf to pp. Tone: resigned, heavy, sorrowful. No attack—soft fade-in at start. Reverb: large, empty hall (0.7s decay, 200ms pre-delay). Overall: grave, final, but not catastrophic. |
| **Duration (ms)** | 1050 total (fade-in 50ms + descent 800ms + decay 200ms) |
| **Channel** | SFX (ch11) |
| **Spatial** | 2D |
| **Volume** | 0.60 |
| **Pitch Variance** | None — deterministic glissando |
| **Variants** | 1 |
| **Streaming** | No |
| **Special Notes** | Longer duration than other state changes, reflecting the gravity of vassalization. Slow, melancholic descent. Should feel like the city's autonomy is being slowly drained. |

**Audio Technique**:
- String descent: cello + bass samples (D2→F#1), slow linear pitch glissando over 800ms
- Timpani roll: 60 Hz fundamental, soft roll building under strings then decaying
- Fade-in envelope 50ms (no click)
- Large hall reverb (0.7s decay, spacious feel)
- Lowpass filter at 800 Hz (warm, somber, no high-frequency energy)
- No compression (preserve organic string timbre)

---

### 6.3 City Transitions Back to "Owned" from Vassal (Victory/Liberation)

**Purpose**: Triumphant, liberating tone when a city breaks free from vassalization by winning a war or being released by the overlord.

| **File Name** | `city_state_owned_liberation` |
|---|---|
| **Trigger** | City state changes: Vassal → Owned (after winning a war against the overlord, or overlord releases it voluntarily) |
| **Sound Character** | Ascending, triumphant orchestral resolution: rising trumpet fanfare (C3→E3→G3→C4, four ascending notes, 150ms each note). Underlying timpani roll (crescendo from pp to mp, 600ms total). Full chord swell beneath the trumpet line. Tone: victorious, liberating, ceremonial. Reverb: grand hall (0.5s). |
| **Duration (ms)** | 750 total (fanfare 600ms + chord swell 400ms, overlapping + decay 100ms) |
| **Channel** | SFX (ch11) |
| **Spatial** | 2D |
| **Volume** | 0.75 (prominent, celebratory) |
| **Pitch Variance** | None |
| **Variants** | 1 |
| **Streaming** | No |
| **Special Notes** | Longer and more triumphant than a simple state change—liberation is a major victory. Recommend BGM duck of -6 dB. |

**Audio Technique**:
- Trumpet fanfare: four ascending pitches (C3, E3, G3, C4), 150ms each
- Each note attack ~40ms, sustain ~80ms, decay 30ms
- Underlying timpani roll: 60 Hz, crescendo 0–600ms beneath trumpet
- Full chord swell: strings (C major triad, C3–E3–G3), crescendo alongside timpani
- Grand hall reverb (0.5s decay, 200ms pre-delay)
- Compressor (1:2 ratio, -10 dB GR) for control

---

### 6.4 Vassal Released by Ruler Voluntarily

**Purpose**: Neutral, official tone when a ruling player releases a vassal city back to independence (administrative action, no conflict).

| **File Name** | `vassal_released_voluntary` |
|---|---|
| **Trigger** | Overlord clicks "Release Vassal" button; city state changes: Vassal → Owned (for the original owner) |
| **Sound Character** | Clear, declarative resolution chord (major triad: F3–A3–C4, struck together). Attack 60ms, sustain 200ms, decay 150ms. Tone: official, fair, balanced. Reverb light (0.3s). Similar to war_schedule_confirmed but slightly higher-pitched and warmer (F major vs C major). |
| **Duration (ms)** | 410 total (60ms + 200ms + 150ms) |
| **Channel** | SFX (ch11) |
| **Spatial** | 2D |
| **Volume** | 0.55 |
| **Pitch Variance** | None |
| **Variants** | 1 |
| **Streaming** | No |
| **Special Notes** | Administrative action, not conflict-related. More neutral than liberation (no fanfare), but still positive. Clear and decisive, like a stamp of approval. |

**Audio Technique**:
- Major triad (F3–A3–C4): pitched bells or string samples
- All three pitches struck together (simultaneous attack at 60ms)
- Reverb: small hall (0.3s decay)
- EQ peak at ~370 Hz for warmth
- No compression needed

---

## 7. Notification Sounds

### 7.1 War Declared Against You (Attention Grabber)

**Purpose**: Urgent, attention-grabbing notification when the player receives an incoming war declaration from an opponent.

| **File Name** | `notification_war_declared_against` |
|---|---|
| **Trigger** | Network event received: player is target of a war declaration |
| **Sound Character** | Piercing, high-pitched double stab (two orchestral impacts separated by 200ms silence). First stab: high strings + cymbals (8–10 kHz content), attack 40ms. Second stab: slightly lower (timpani + low brass foundation), attack 50ms. Both sustain briefly (80ms), decay quickly (100ms). Tone: alarm, urgency, without being harsh. Reverb: tight (0.2s). |
| **Duration (ms)** | 460 total (stab 40ms attack + 80ms sustain + 100ms decay + 200ms silence + stab 50ms + 80ms + 100ms) |
| **Channel** | Announcer (ch8) — NOT SFX, because this is a notification sound that should prioritize clarity over spatial immersion |
| **Spatial** | 2D (non-spatial) |
| **Volume** | 0.70 (loud enough to interrupt, but not painful) |
| **Pitch Variance** | None |
| **Variants** | 1 |
| **Streaming** | No |
| **Special Notes** | This is a **critical notification** — must not be suppressed by normal volume settings. Should play even if most SFX are muted (may be routed to a "critical notifications" submix if available). Two-stab pattern creates a "double-take" effect—ensures player notices. |

**Audio Technique**:
- Stab 1: High-pitched strings (D5–A5) + bright cymbal crash (8–10 kHz peak)
- Stab 2: Timpani (60 Hz) + low brass (F2–C3)
- Tight transient attack on both stabs
- Reverb: tight hall (0.2s decay, no pre-delay)
- EQ: high-pass filter at 100 Hz, low-pass peak at 4 kHz (reduce harshness, maintain urgency)
- Compressor (1:2 ratio, -8 dB GR) to manage peak levels

---

### 7.2 Battle Warning 60 Seconds

**Purpose**: Timely notification alerting players that their war battle will begin in 60 seconds. Less urgent than immediate start, but important timing cue.

| **File Name** | `notification_battle_warning_60s` |
|---|---|
| **Trigger** | War state: "Scheduled" → 60 seconds before battle start time |
| **Sound Character** | Rising, anticipatory tone: ascending bell-like melody (D4→E4→F#4→G4, evenly spaced, 250ms each note). Sustained low pad underneath (A2, pp, providing a grounding bass note). Tone: building excitement, not alarm. Reverb: small hall (0.3s). |
| **Duration (ms)** | 1050 total (melody 4 notes × 250ms = 1000ms + sustain/decay 50ms) |
| **Channel** | Announcer (ch8) |
| **Spatial** | 2D |
| **Volume** | 0.55 (noticeable but not startling; matches announcement priority) |
| **Pitch Variance** | None |
| **Variants** | 1 |
| **Streaming** | No |
| **Special Notes** | Rising melody creates forward momentum. Time signature should feel like a 4-beat count. Longer than the "war declared" notification, reflecting the less-urgent nature of a future event. |

**Audio Technique**:
- Ascending melody: bell samples (D4, E4, F#4, G4), each triggered with ~50ms attack
- Individual note decay: ~200ms each
- Underlying pad: low A2 pad (pp), starts with melody, sustains through, decays at end
- Reverb: small hall (0.3s decay)
- No compression needed on pad, light compression on melody (1:1.5 ratio, -4 dB GR)

---

### 7.3 Battle Starting Now

**Purpose**: Immediate, urgent alert that the battle is happening right now—pull the player into action.

| **File Name** | `notification_battle_starting_now` |
|---|---|
| **Trigger** | War state: "Scheduled" → "In Battle" transition (battle timer hits 0, all players transported to match) |
| **Sound Character** | Aggressive, action-driving orchestral hit: full ensemble strike (horns, strings, percussion all together). Attack 60ms, sustain 150ms, decay 200ms. Underlying timpani roll (80 Hz, building from start through 300ms). Tone: urgent, aggressive, action-focused. Reverb: medium hall (0.4s). This is the "go!" moment. |
| **Duration (ms)** | 410 total (ensemble 60ms + 150ms + 200ms) |
| **Channel** | Announcer (ch8) |
| **Spatial** | 2D |
| **Volume** | 0.75 (very prominent, pulls player's attention) |
| **Pitch Variance** | None |
| **Variants** | 1 |
| **Streaming** | No |
| **Special Notes** | This sound bridges the meta-game (Territory War map) to the core game (MOBA battle). It's the signal to shift mental context. Critical notification—must play clearly. Recommend coordinating with UI design so this sound plays *after* the scene transition is complete (to avoid overlap with loading sounds). |

**Audio Technique**:
- Orchestral hit: French horns (C3–F3) + strings (divisi, C2–G3) + woodwinds (C4–D5)
- All layers triggered together (simultaneous attack at 60ms)
- Timpani roll: 80 Hz fundamental, crescendo 0–300ms, sustain with ensemble, decay with ensemble
- Reverb: medium hall (0.4s decay, 100ms pre-delay)
- Compressor (1:2 ratio, -12 dB GR at peak) to glue ensemble together
- EQ: slight presence peak at 2 kHz for clarity

---

### 7.4 War Result Notification Ping (Generic, Before Full Sting)

**Purpose**: Quick, neutral confirmation sound when a war result is received (before the full sting animation/sound plays). Acts as a "data received" ping.

| **File Name** | `notification_war_result_ping` |
|---|---|
| **Trigger** | War result determined (Core destroyed, all bots defeated, or attacker withdrawal). Plays *before* the victory/defeat sting in the battle UI. |
| **Sound Character** | Single, bright, quick ping: pitched bell (F#4 / 740 Hz), attack ~30ms, sustain ~100ms, decay ~80ms. Tone: clean, metallic, affirming. Reverb minimal (0.15s). Similar in character to ping sounds but on Announcer channel for clarity. |
| **Duration (ms)** | 210 total (30ms + 100ms + 80ms) |
| **Channel** | Ping (ch12) |
| **Spatial** | 2D |
| **Volume** | 0.50 |
| **Pitch Variance** | None |
| **Variants** | 1 |
| **Streaming** | No |
| **Special Notes** | Light, quick acknowledgment. Not a result sound itself—just confirms data receipt. The actual victory/defeat sting comes after (handled by in-game audio system, not Territory War). |

**Audio Technique**:
- Pitched bell or digital chime (F#4, bright but warm)
- Attack envelope 30ms (snappy, clear)
- Decay 80ms (quick, punchy)
- Minimal reverb (0.15s, no pre-delay)
- No compression needed (transient-dependent sound)

---

## 8. Social / Invite SFX

### 8.1 Receive War Party Invite

**Purpose**: Friendly, inviting notification when another player invites the player to join their war team.

| **File Name** | `social_party_invite_received` |
|---|---|
| **Trigger** | Network event received: incoming war team invitation (another player recruiting for their war team) |
| **Sound Character** | Warm, uplifting musical chime: three-note ascending melody on warm bells (C4→E4→G4, 150ms each note). No percussion, pure melodic. Sustain each note 120ms before next note triggers. Tone: friendly, welcoming, positive. Reverb light (0.25s). Similar to city_inspect_panel_open but with warmer character. |
| **Duration (ms)** | 550 total (melody 450ms + sustain/decay 100ms) |
| **Channel** | SFX (ch11) |
| **Spatial** | 2D |
| **Volume** | 0.50 |
| **Pitch Variance** | None |
| **Variants** | 1 |
| **Streaming** | No |
| **Special Notes** | Social sounds are less urgent than notifications, so lower volume and simpler tone. Positive, inviting character—should make the player want to accept. |

**Audio Technique**:
- Warm bell melody: three pitches (C4, E4, G4) triggered sequentially with 150ms spacing
- Each bell attack ~50ms, decay ~100ms
- Reverb: small room (0.25s decay)
- EQ: slight boost at 500 Hz for warmth
- No compression needed

---

### 8.2 Accept War Party Invite / Join Team

**Purpose**: Affirming, joining gesture when the player accepts an invite and joins a war team.

| **File Name** | `social_team_join_confirm` |
|---|---|
| **Trigger** | Player clicks "Accept Invite" or confirms joining a war team; local UI confirmation |
| **Sound Character** | Major chord swell with gentle percussion undertone: C major triad (C3–E3–G3) on warm pads, swelling from pp to mf over 300ms. Soft brushed cymbal (warm, not bright) playing a quarter-note pattern beneath (4 hits, evenly spaced). Tone: warm, celebratory, communal. Reverb medium (0.35s). |
| **Duration (ms)** | 500 total (swell 300ms + decay 200ms) |
| **Channel** | SFX (ch11) |
| **Spatial** | 2D |
| **Volume** | 0.55 |
| **Pitch Variance** | None |
| **Variants** | 1 |
| **Streaming** | No |
| **Special Notes** | More substantial than a simple chime—joining a team is a meaningful commitment. Gentle percussion adds texture without aggression. |

**Audio Technique**:
- Warm pad chord: strings (C3–E3–G3), crescendo from pp to mf
- Brushed cymbal: soft rolls, quarter-note hits starting at ~100ms mark
- Attack envelope on chord: ~100ms (gradual swell)
- Decay over 200ms to silence
- Reverb: medium hall (0.35s decay)
- EQ: gentle boost at 300 Hz for warmth, roll-off above 6 kHz (no harshness)
- Light compression (1:1.5 ratio, -4 dB GR) to smooth swell

---

### 8.3 New Player Joins Your War Team

**Purpose**: Positive confirmation when a new teammate accepts the player's invite and joins the player's war team.

| **File Name** | `social_teammate_joined_announce` |
|---|---|
| **Trigger** | Network event received: another player has accepted the invite and joined the player's war team |
| **Sound Character** | Ascending, celebratory two-note chime: lower note (C4) held briefly, then higher note (E5) strikes and sustains. First note 200ms, second note 300ms. Clean, bright, celebratory tone. Reverb light (0.2s). Pure melodic, no percussion. |
| **Duration (ms)** | 520 total (C4 200ms + E5 300ms + decay 20ms) |
| **Channel** | SFX (ch11) |
| **Spatial** | 2D |
| **Volume** | 0.50 |
| **Pitch Variance** | None |
| **Variants** | 1 |
| **Streaming** | No |
| **Special Notes** | Two-note pattern (lower then higher) creates a "level-up" feel. Each teammate join gets this sound, but repeating it multiple times (e.g., 5+ joiners) may require muting or rate-limiting to avoid spam. Recommend max once per 2 seconds per team. |

**Audio Technique**:
- Two-note melody: C4 (lower, warm) and E5 (higher, bright)
- First note: attack ~40ms, sustain 200ms, decay 20ms
- Second note: attack ~50ms (after first note ends), sustain 300ms, decay with reverb tail
- Light reverb (0.2s decay)
- EQ: no special coloring needed (clean melodic)
- No compression needed

---

## 9. Complete SFX File Manifest & Trigger Summary

### 9.1 Master SFX Table

| **SFX Name** | **File Name** | **Duration (ms)** | **Channel** | **Spatial** | **Volume** | **Variants** | **Priority** | **Max Concurrency** |
|---|---|---|---|---|---|---|---|---|
| **A. WORLD MAP NAVIGATION** |
| Map Pan/Drag | `map_pan_continuous_loop` | ∞ (loop) | SFX ch11 | 2D | 0.35 | 1 | Low | 1 |
| Map Zoom In | `map_zoom_in_ascend` | 320 | SFX ch11 | 2D | 0.50 | 1 | Medium | 1 |
| Map Zoom Out | `map_zoom_out_descend` | 370 | SFX ch11 | 2D | 0.45 | 1 | Medium | 1 |
| City Hover | `city_hover_tone` | 600 | SFX ch11 | 2D | 0.40 | 1 | Low | 1 (debounced 200ms) |
| City Click/Select | `city_click_seal` | 320 | SFX ch11 | 2D | 0.60 | 1 | Medium | 1 |
| **B. CITY INTERACTIONS** |
| Inspect Panel Open | `city_inspect_panel_open` | 450 | SFX ch11 | 2D | 0.55 | 1 | Medium | 1 |
| Inspect Panel Close | `city_inspect_panel_close` | 300 | SFX ch11 | 2D | 0.40 | 1 | Medium | 1 |
| City Purchase | `city_purchase_fanfare_short` | 820 | SFX ch11 | 2D | 0.75 | 3 | High | 1 |
| City Upgrade | `city_upgrade_confirmation` | 700 | SFX ch11 | 2D | 0.65 | 1 | Medium | 1 |
| City Name Confirm | `city_name_confirm_chime` | 540 | SFX ch11 | 2D | 0.50 | 1 | Low | 1 |
| **C. WAR DECLARATION FLOW** |
| War Declaration Initiate | `war_declaration_initiate` | 560 | SFX ch11 | 2D | 0.80 | 1 | Critical | 1 |
| War Window Slot Selected | `war_window_timeslot_selected` | 330 | SFX ch11 | 2D | 0.50 | 1 | Medium | 1 |
| War Schedule Confirmed | `war_schedule_confirmed_seal` | 450 | SFX ch11 | 2D | 0.65 | 1 | High | 1 |
| War Withdrawn | `war_withdrawn_release` | 550 | SFX ch11 | 2D | 0.50 | 1 | Medium | 1 |
| **D. CITY STATE CHANGES** |
| City At War | `city_state_at_war_alert` | 900 | SFX ch11 | 2D | 0.70 | 1 | High | 1 |
| City → Vassal | `city_state_vassal_defeat` | 1050 | SFX ch11 | 2D | 0.60 | 1 | High | 1 |
| City → Owned (Liberated) | `city_state_owned_liberation` | 750 | SFX ch11 | 2D | 0.75 | 1 | High | 1 |
| Vassal Released | `vassal_released_voluntary` | 410 | SFX ch11 | 2D | 0.55 | 1 | Medium | 1 |
| **E. NOTIFICATIONS** |
| War Declared Against You | `notification_war_declared_against` | 460 | Announcer ch8 | 2D | 0.70 | 1 | **Critical** | 1 |
| Battle Warning 60s | `notification_battle_warning_60s` | 1050 | Announcer ch8 | 2D | 0.55 | 1 | High | 1 |
| Battle Starting Now | `notification_battle_starting_now` | 410 | Announcer ch8 | 2D | 0.75 | 1 | **Critical** | 1 |
| War Result Ping | `notification_war_result_ping` | 210 | Ping ch12 | 2D | 0.50 | 1 | Medium | 1 |
| **F. SOCIAL/TEAM** |
| Party Invite Received | `social_party_invite_received` | 550 | SFX ch11 | 2D | 0.50 | 1 | Low | 1 |
| Team Join Confirm | `social_team_join_confirm` | 500 | SFX ch11 | 2D | 0.55 | 1 | Medium | 1 |
| Teammate Joined | `social_teammate_joined_announce` | 520 | SFX ch11 | 2D | 0.50 | 1 | Low | 4 (rate-limited: max 1 per 2s) |

**Total Unique SFX Files**: 28 files (including 3 variants of city_purchase_fanfare_short = 30 actual audio assets)

---

### 9.2 Ducking Rules (BGM Impact)

When certain sounds play, the Background Music (BGM) should be reduced to prevent masking and maintain clarity.

| **Sound(s)** | **Trigger Category** | **BGM Duck Amount** | **Attack Time** | **Recovery Time** | **Recovery Curve** | **Notes** |
|---|---|---|---|---|---|---|
| `city_purchase_fanfare_short` | Reward/major action | −6 dB | 100 ms | 500 ms | Linear | Player reward moment; BGM should step back noticeably |
| `war_declaration_initiate` | Critical moment | −8 dB | 150 ms | 600 ms | Exponential (0.7) | Most important gameplay action; significant duck needed |
| `war_schedule_confirmed_seal` | Major commitment | −4 dB | 100 ms | 400 ms | Linear | Less severe than declaration; quick recovery |
| `city_state_owned_liberation` | Victory moment | −6 dB | 100 ms | 500 ms | Linear | Celebratory; similar to purchase fanfare |
| `city_state_at_war_alert` | Alert/transition | −3 dB | 80 ms | 300 ms | Linear | Light duck; more of a world-map alert than critical moment |
| `notification_war_declared_against` | Critical notification | −5 dB | 80 ms | 400 ms | Linear | Pulls attention; moderate duck |
| `notification_battle_starting_now` | Action start | −4 dB | 100 ms | 300 ms | Linear | Bridges to gameplay; light duck, quick recovery |

**Default BGM Target Level**: 0 dB (reference)  
**All ducks apply to**: BGM channel (ch7) in DeltaAudioMixer  
**Implementation**: AudioManager should check for playing Territory War sounds and apply ducking via mixer groups.

---

### 9.3 Critical Notification List (Must-Not-Suppress Sounds)

These sounds **must** play clearly regardless of user volume settings and should not be affected by global "SFX mute" toggles:

1. **`notification_war_declared_against`** — War declaration alert (highest priority)
2. **`notification_battle_starting_now`** — Battle start signal (must hear to join battle)
3. **`war_declaration_initiate`** (when player initiates) — Player's own critical action
4. **`city_state_at_war_alert`** (if watching a city) — World-map event of significance

**Recommendation**: Route these to a dedicated "Critical Alerts" mixer subgroup with its own master volume slider (separate from main SFX volume), allowing players to keep important notifications audible even if they've muted other SFX.

---

### 9.4 Concurrency & Rate Limiting

| **Sound Group** | **Max Concurrent Instances** | **Rate Limit** | **Fallback Behavior** | **Notes** |
|---|---|---|---|---|
| Map navigation (pan/zoom/hover) | 1 per interaction type | None | Ignore new request | Only one hover sound at a time; zooming cancels pan loop |
| City interactions (inspect, purchase, upgrade) | 1 | None | Kill previous, play new | Sequential UI actions; no overlap expected |
| War declaration flow | 1 | None | Ignore new (war in progress) | Cannot declare multiple wars simultaneously |
| State changes | 4 | 1 per city per 5 sec | Rate-limit further requests | Multiple cities can transition simultaneously (e.g., 4 cities go At War), but same city cannot transition twice in 5 seconds |
| Notifications | 2 | None | Queue or play immediately | War declared + 60s warning might overlap; allow both |
| Social/team sounds | 4 | 1 per 2 sec | Mute additional sounds | Multiple teammates joining = multiple sounds, but rate-limit to avoid spam |

---

## 10. Mixing & Mastering Notes

### 10.1 Target Loudness Levels

| **Content Type** | **Target LUFS** | **Peak dBFS** | **Headroom** |
|---|---|---|---|
| Individual SFX | −20 to −18 LUFS | −6 dB | 6 dB |
| BGM (when not ducked) | −14 LUFS | −3 dB | 3 dB |
| Announcer (critical notifications) | −16 LUFS | −4 dB | 4 dB |
| **Mixed Master** (all channels) | −14 LUFS | −1 dB | 1 dB |

**Compliance**: Target −14 LUFS for final mix (streaming platform standard).

### 10.2 EQ Zones (Frequency Masking Avoidance)

To prevent sounds from masking each other or the BGM, assign each sound category to a frequency "zone":

| **Frequency Zone** | **Content** | **Primary Boost/Cut** | **Rationale** |
|---|---|---|---|
| **Sub-Bass (20–60 Hz)** | Timpani rolls, bass drums, war declaration low end | Neutral (preserve) | Foundation layer; critical for weight/gravitas |
| **Bass (60–250 Hz)** | French horns, war state changes, low strings | Slight boost (+2 dB at 100 Hz) | Regal, grounded tone; avoid masking vocals/dialogue |
| **Low-Mid (250–500 Hz)** | Strings, pad foundations, map pan undertone | Neutral or slight cut (−1 dB at 300 Hz) | Can muddy if boosted; careful balance |
| **Mids (500 Hz–2 kHz)** | Chimes, bells, bright orchestral hits | Slight cut at 1 kHz (−2 dB) | Reduce harshness; preserve musicality |
| **Presence (2–4 kHz)** | War declaration brass, orchestral stabs, clarity | Targeted boost at 2.5 kHz (+3 dB, Q=1.5) | Clarity for important moments |
| **Brilliance (4–10 kHz)** | Cymbals, high bells, shimmer effects | Neutral to slight cut (−1 dB above 6 kHz) | Avoid ear fatigue; maintain clarity without harshness |
| **Air (10–20 kHz)** | Ambient reverb tails, subtle shimmer | Neutral | Preserve space without artifacting |

### 10.3 Reverb Strategy

| **Sound Category** | **Reverb Type** | **Decay Time** | **Pre-delay** | **Dry/Wet Mix** | **Purpose** |
|---|---|---|---|---|---|
| Map navigation | Small room | 0.3–0.4 s | 0 ms | 70% dry / 30% wet | Space without distance illusion |
| City interactions | Small room to intimate | 0.2–0.35 s | 0–100 ms | 75% dry / 25% wet | Intimate, contained UI space |
| War declaration | Concert hall | 0.5–0.6 s | 150–200 ms | 60% dry / 40% wet | Expansive, ceremonial, consequential |
| State changes | Variable (medium to large) | 0.4–0.7 s | 100–150 ms | 65% dry / 35% wet | Atmospheric, transition-supporting |
| Notifications | Minimal | 0.15–0.25 s | 0 ms | 90% dry / 10% wet | Clarity, immediate attention |
| Social/team | Small room | 0.25–0.3 s | 0–50 ms | 75% dry / 25% wet | Friendly, contained space |

---

## 11. Implementation & Audio Pipeline

### 11.1 Asset Organization

All Territory War SFX assets should be organized as:

```
assets/audio/sfx/territory-war/
├── world-map-navigation/
│   ├── map_pan_continuous_loop.wav
│   ├── map_zoom_in_ascend.wav
│   ├── map_zoom_out_descend.wav
│   ├── city_hover_tone.wav
│   └── city_click_seal.wav
├── city-interactions/
│   ├── city_inspect_panel_open.wav
│   ├── city_inspect_panel_close.wav
│   ├── city_purchase_fanfare_short_variantA.wav
│   ├── city_purchase_fanfare_short_variantB.wav
│   ├── city_purchase_fanfare_short_variantC.wav
│   ├── city_upgrade_confirmation.wav
│   └── city_name_confirm_chime.wav
├── war-declaration-flow/
│   ├── war_declaration_initiate.wav
│   ├── war_window_timeslot_selected.wav
│   ├── war_schedule_confirmed_seal.wav
│   └── war_withdrawn_release.wav
├── city-state-changes/
│   ├── city_state_at_war_alert.wav
│   ├── city_state_vassal_defeat.wav
│   ├── city_state_owned_liberation.wav
│   └── vassal_released_voluntary.wav
├── notifications/
│   ├── notification_war_declared_against.wav
│   ├── notification_battle_warning_60s.wav
│   ├── notification_battle_starting_now.wav
│   └── notification_war_result_ping.wav
└── social-team/
    ├── social_party_invite_received.wav
    ├── social_team_join_confirm.wav
    └── social_teammate_joined_announce.wav
```

**Total Assets**: 30 files (28 unique + 2 variants of city_purchase_fanfare)

### 11.2 AudioManager Integration (Pseudocode)

```csharp
// Territory War SFX trigger examples

// A. World Map Navigation
OnMapDragStart() → PlaySoundFx("map_pan_continuous_loop"); // loops
OnMapDragEnd() → StopSoundFx("map_pan_continuous_loop"); // fade out 200ms
OnMapZoomIn() → PlaySoundFx("map_zoom_in_ascend"); // 320ms one-shot
OnMapZoomOut() → PlaySoundFx("map_zoom_out_descend"); // 370ms one-shot

// B. City Interactions
OnCityHoverEnter() → PlaySoundFx("city_hover_tone"); // 600ms, debounced 200ms
OnCityClicked() → PlaySoundFx("city_click_seal"); // 320ms one-shot
OnInspectPanelOpen() → PlaySoundFx("city_inspect_panel_open"); // 450ms
OnInspectPanelClose() → PlaySoundFx("city_inspect_panel_close"); // 300ms

// C. Purchase (with variant selection)
OnCityPurchaseConfirmed() → {
    variant = Random.Range(0, 3); // 0, 1, or 2
    PlaySoundFx($"city_purchase_fanfare_short_variant{['A','B','C'][variant]}");
    DuckBGM(-6dB, 100ms); // attack
    RecoverBGM(500ms); // recovery
}

// D. War Declaration
OnWarDeclared() → {
    PlaySoundFx("war_declaration_initiate");
    DuckBGM(-8dB, 150ms);
    RecoverBGM(600ms);
}

// E. Notifications
OnWarDeclaredAgainstPlayer() → PlayAnnouncer("notification_war_declared_against"); // critical
OnBattleWarning60s() → PlayAnnouncer("notification_battle_warning_60s");
OnBattleStarting() → PlayAnnouncer("notification_battle_starting_now"); // critical

// F. Social
OnPartyInviteReceived() → PlaySoundFx("social_party_invite_received");
OnTeammateJoined() → {
    // Rate limit: max once per 2 seconds
    if (!lastTeammateJoinSound || Time.time - lastTeammateJoinSound > 2f) {
        PlaySoundFx("social_teammate_joined_announce");
        lastTeammateJoinSound = Time.time;
    }
}
```

---

## 12. Audio Design Rationale

### Design Principles Applied

1. **Regal, Expansive, Tense, Grounded, Ceremonial Palette**
   - Regal: Orchestral instruments (French horns, trumpets, timpani), major/minor tonality, harmonic richness
   - Expansive: Reverb spaces (concert halls, medium rooms), longer decay times for ambience
   - Tense: Ascending glissandos (zoom in), dissonant anticipation (battle warning), sub-bass pulses
   - Grounded: Orchestral rather than synthetic, acoustic instrument recordings, earth tones (low-frequency foundation)
   - Ceremonial: Fanfares, formal chord progressions, definitive attacks and releases

2. **Clear Spatial Hierarchy**
   - Map navigation: Subtle, listener-relative 2D sounds (no worldspace positioning)
   - City/war interactions: Clear, affirming UI sounds with presence peaks
   - Notifications: Piercing, attention-grabbing sounds on high-priority channels
   - Result: Player always knows what's a UI interaction vs. what's a world event

3. **Emotional Arc**
   - Hovering/selecting: Warm, inviting (encourages interaction)
   - Purchasing: Triumphant (reward moment)
   - War declaration: Powerful, consequential (reflects gravity of action)
   - Losing a city: Somber, resigned (reflects loss without catastrophe)
   - Liberation: Triumphant (major victory)
   - Notifications: Urgent but not jarring (time-critical without stress)

4. **Avoid Repetition Fatigue**
   - Purchase fanfare: 3 variants (A/B/C) with melody variations—players hear different fanfares across sessions
   - Other major sounds: Single definitive version (war declaration, state changes) to maintain impact
   - Rate limiting on social sounds to prevent spam

---

## 13. Acceptance Criteria

- [ ] All 28 unique SFX files created and integrated into Territory War system
- [ ] Each SFX plays on correct channel (SFX ch11, Announcer ch8, Ping ch12) with correct spatial settings
- [ ] BGM ducking rules implemented: purchase fanfare (-6 dB), war declaration (-8 dB), etc.
- [ ] City purchase fanfare: 3 variants randomly selected, no repetition across quick purchases
- [ ] Notifications routed to critical path: war_declared_against and battle_starting_now play clearly even when SFX volume is lowered
- [ ] Concurrent sound limits enforced: max 1 hover per 200ms, max 4 teammate-joined per 2 seconds
- [ ] Reverb and EQ applied per spec: war declaration in concert hall (0.6s), city interactions in small room (0.2–0.35s)
- [ ] All sounds meet loudness target: individual SFX at −20 to −18 LUFS, master mix at −14 LUFS
- [ ] State change sounds triggered correctly: city→At War, Vassal, Owned, etc.
- [ ] Duration and pitch specified for all sounds; no ambiguous descriptions remain
- [ ] Audio designer and audio-director review completed before production audio recording begins

---

## 14. Known Deviations & Future Considerations

### Audio Middleware Integration
This spec assumes Unity's built-in AudioManager with AudioMixer groups. If transitioning to Wwise or FMOD in the future:
- Implement Territory War sound events as FMOD Events with corresponding triggers
- Route to Wwise busses instead of Unity mixer groups
- Implement RTPC automation for ducking (currently hardcoded in C#)

### 25v25 Battle Ambience
This spec covers Territory War UI and meta-game sounds only. The actual 25v25 MOBA battle will have its own audio design (handled by combat audio specialist), including:
- Ambient battle chatter (crowd, construction, siege)
- Larger-scale impact sounds (siege weapons, strategic ability effects)
- **Not included in this spec**; to be coordinated with combat audio specialist

### Voice Lines (Future)
If Territory War adds announcer voice lines (e.g., "War declared against City of Kings!"), those will be spec'd separately with the voice recording team. Current spec uses instrumental sounds only.

---

## Appendix A: Reference Recordings (Inspiration)

**Sonic Palette References** (for sound designer to reference during production):

| Reference | Title | Timestamp | Element | Use Case |
|---|---|---|---|---|
| **Age of Empires II** | Civilization UI | 0:00–0:20 | Orchestral stabs, triumphant fanfare | War declaration inspiration |
| **Age of Empires II** | Game Over (Victory) | 0:00–0:30 | Ascending horns, timpani, resolution | City liberation fanfare |
| **Crusader Kings III** | Map ambient | 0:00–1:00 | Low-register horn pad, string glissando | Map pan continuous loop |
| **Crusader Kings III** | Realm UI open/close | 0:00–0:10 | Ascending/descending chimes, musical resolution | Panel open/close |
| **Total War: Warhammer** | Battle announcement | 0:00–0:15 | Piercing orchestral stab, war horn | Battle starting notification |
| **Total War: Warhammer** | Victory fanfare | 0:00–0:20 | Full orchestra, timpani, regal major chord | Victory/liberation moments |

---

## Appendix B: Tuning Knobs (Data Config)

These values should be externalized to the Data/Config System (F4) for easy tuning during balance playtests:

| Parameter | Current Value | Config Key | Safe Range | Notes |
|---|---|---|---|---|
| City hover debounce delay | 200 ms | `tw_city_hover_debounce_ms` | 100–500 ms | Lower = more responsive, higher = fewer false triggers |
| City purchase fanfare variant selection | Random (0–2) | `tw_purchase_variant_mode` | "random" / "round_robin" | Switch to round-robin if random feels too chaotic |
| BGM duck amount (war declaration) | −8 dB | `tw_bgm_duck_war_decl_db` | −6 to −10 dB | Higher magnitude = more dramatic |
| BGM duck attack time | 150 ms | `tw_bgm_duck_attack_ms` | 100–300 ms | Faster = more noticeable transition |
| Teammate joined rate limit | 1 per 2 sec | `tw_teammate_sound_ratelimit_sec` | 1–5 sec | Prevents notification spam in large team scenarios |
| State change concurrency limit | 4 simultaneous | `tw_state_change_max_concurrent` | 2–8 | Limits overlapping "At War" alerts from multiple cities |

---

**Document Complete**  
**Status**: Ready for Audio Production  
**Next Step**: Pass to audio-director for sonic palette approval, then to recording/mixing team for asset creation.

