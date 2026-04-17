# Unity AudioImporter Settings — TW Announcer (Herald Voice Lines)

Apply to all files in: `assets/audio/territory-war/announcer/`

## Unity AudioImporter Configuration

```
Load Type:            Compressed In Memory
Compression Format:   Vorbis
Quality:              80 (0.0–1.0 scale → 0.80)
Sample Rate Setting:  Preserve Sample Rate
Force To Mono:        false
Load In Background:   false
```

*Reason: Voice lines are 3–5s long — too long for PCM (cost ~680 KB each), but intelligibility requires higher quality floor than ambient (Q80 vs Q65). Compressed in Memory = ~100 KB each. 18 files total ≈ 1.8 MB.*

## Platform Overrides

All platforms: same settings.

## Notes
- Deliver source files dry (no baked reverb) — hall reverb applied at AudioMixer level
- `AudioManager.PlayAnnouncer(eventName)` selects randomly from variants via `Array.FindAll`
- The search key is the event name WITHOUT variant suffix:
  - `"war_declared"` → matches `ann_tw_war_declared_01_th` and `ann_tw_war_declared_02_th`
- Audio clips must be registered in `AudioManager.m_AnnouncerSounds[]` with matching names
- TW herald voice uses a separate voice character from the MOBA announcer — do NOT reuse the same actor
