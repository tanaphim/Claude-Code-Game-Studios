# Unity AudioImporter Settings — TW BGM (Music Loops & Stings)

Apply to all files in: `assets/audio/territory-war/bgm/`

## Unity AudioImporter Configuration

```
Load Type:            Streaming
Compression Format:   Vorbis
Quality:              70 (0.0–1.0 scale → 0.70)
Sample Rate Setting:  Preserve Sample Rate
Force To Mono:        false
Load In Background:   true
Ambisonic:            false
```

## Platform Overrides

### Android / iOS
```
Load Type:            Compressed In Memory
Compression Format:   Vorbis
Quality:              65
```
*Reason: Streaming from storage is slower on mobile I/O. Keep compressed in memory for BGM on mobile to avoid hitches.*

### Standalone (PC/Mac)
Same as default (Streaming).

## Notes
- All loops must have seamless loop points set in Unity AudioClip inspector
- Stings (non-loop): set `Loop` = false
- `mus_territorywar_worldmap_explore_loop` and `mus_territorywar_worldmap_tension_loop` must loop at the same musical bar position for mid-bar crossfade to work correctly
