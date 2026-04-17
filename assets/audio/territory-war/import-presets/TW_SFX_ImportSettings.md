# Unity AudioImporter Settings — TW SFX (Sound Effects)

Apply to all files in: `assets/audio/territory-war/sfx/ui/` and `sfx/gameplay/`

## Unity AudioImporter Configuration

```
Load Type:            Decompress On Load
Compression Format:   ADPCM
Sample Rate Setting:  Preserve Sample Rate
Force To Mono:        false
Load In Background:   false
```

*Reason: SFX must fire with zero latency. ADPCM decodes faster than Vorbis with near-lossless quality for short transients. Decompress on load ensures no per-trigger decode overhead.*

## Critical SFX Override

Apply to: `assets/audio/territory-war/sfx/critical/`

```
Load Type:            Decompress On Load
Compression Format:   PCM
Sample Rate Setting:  Preserve Sample Rate
```

*Reason: `war_declared` and `battle_starting_now` are gameplay-critical — players depend on these for real-time war decisions. PCM = zero decode latency, maximum fidelity. These 2 files total ~1 MB — PCM cost is acceptable.*

## Platform Overrides

All platforms: same settings (SFX are already small; no platform distinction needed).

## Notes
- `sfx_tw_nav_map_pan_loop` — set `Loop` = true (continuous during map drag, stopped on drag end)
- All other SFX: `Loop` = false
- SFX played via `AudioManager.PlaySoundFx(name)` → AudioPool → ch11 (SFX mixer group)
- Critical SFX played directly with `AudioSource.priority = 0` via `TerritoryWarAudioController.PlayCriticalNotification()`
