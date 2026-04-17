# Unity AudioImporter Settings — TW Ambient (Ambient Loops)

Apply to all files in: `assets/audio/territory-war/ambient/`

## Unity AudioImporter Configuration

```
Load Type:            Streaming
Compression Format:   Vorbis
Quality:              65 (0.0–1.0 scale → 0.65)
Sample Rate Setting:  Preserve Sample Rate
Force To Mono:        false
Load In Background:   true
```

*Reason: Ambient loops are 45–90s long. Streaming avoids the 5–10 MB RAM cost of decompression. Quality 65 is sufficient — ambient is background texture and sits very low in the mix (-22 to -28 LUFS).*

## Platform Overrides

All platforms: same settings.

## Notes
- All ambient files: `Loop` = true
- `spatialBlend = 0` (2D) for all TW World Map ambients — player is viewing a map, not standing in the world
- Streamed in `AudioAmbientTrigger` component; ensure `OnDestroy()` cleanup is implemented (known bug in existing system — see audio-system.md Known Issues)
- `amb_territorywar_cityowned_loop` is used for both Owned (-26 LUFS) and Vassal (-30 LUFS) city states at different volume levels — same clip, different target volume in `TerritoryWarAudioController`
