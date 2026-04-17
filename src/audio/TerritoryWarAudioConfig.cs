using UnityEngine;
using UnityEngine.Audio;

/// <summary>
/// ScriptableObject configuration for all Territory War audio assets.
/// Assign all audio clips and mixer references in the Unity Inspector.
///
/// Create via: Assets → Create → Delta → Audio → Territory War Audio Config
///
/// Design reference: design/gdd/audio-territory-war.md
/// </summary>
[CreateAssetMenu(menuName = "Delta/Audio/Territory War Audio Config", fileName = "TerritoryWarAudioConfig")]
public class TerritoryWarAudioConfig : ScriptableObject
{
    // -------------------------------------------------------------------------
    // Mixer References
    // -------------------------------------------------------------------------

    [Header("Mixer")]
    [Tooltip("Reference to DeltaAudioMixer asset.")]
    public AudioMixer Mixer;

    [Tooltip("TWMusic_Base sub-group (World Map BGM primary source).")]
    public AudioMixerGroup MusicBaseGroup;

    [Tooltip("TWMusic_Additive sub-group (brass/bells additive layers).")]
    public AudioMixerGroup MusicAdditiveGroup;

    [Tooltip("Music channel group (ch7) for battle overlay.")]
    public AudioMixerGroup MusicGroup;

    [Tooltip("Ambience channel group (ch10) for city inspector ambient.")]
    public AudioMixerGroup AmbienceGroup;

    [Tooltip("SFX channel group (ch11) for gameplay SFX.")]
    public AudioMixerGroup SfxGroup;

    [Tooltip("Announcer channel group (ch8) for TW herald lines and critical notifications.")]
    public AudioMixerGroup AnnouncerGroup;

    [Header("Mixer Snapshots")]
    public AudioMixerSnapshot WorldMapSnapshot;
    public AudioMixerSnapshot WarDeclaredSnapshot;
    public AudioMixerSnapshot BattleSnapshot;

    // -------------------------------------------------------------------------
    // BGM — Music Loops & Stings
    // -------------------------------------------------------------------------

    [Header("BGM — World Map")]
    [Tooltip("World Map BGM explore variant. D Dorian, 52–60 BPM. Streaming.")]
    public AudioClip WorldMapExploreLoop;

    [Tooltip("World Map BGM tension variant (duduk removed, strings more prominent). Streaming.")]
    public AudioClip WorldMapTensionLoop;

    [Header("BGM — Additive Layers")]
    [Tooltip("Empire brass layer. Active when player owns 5+ cities. Route to MusicAdditiveGroup.")]
    public AudioClip WorldMapBrassLayer;

    [Tooltip("Empire bells layer. Active when player owns 20+ cities. Route to MusicAdditiveGroup.")]
    public AudioClip WorldMapBellsLayer;

    [Header("BGM — Battle")]
    [Tooltip("TW battle overlay loop. Plays on top of MOBA combat BGM for 3v3/5v5/25v25 TW matches.")]
    public AudioClip BattleOverlayLoop;

    [Header("BGM — Stings (non-looping)")]
    public AudioClip WarDeclaredSting;
    public AudioClip WarScheduledSting;
    public AudioClip VictorySting;
    public AudioClip DefeatSting;
    public AudioClip VassalGainedSting;
    public AudioClip VassalLostSting;

    // -------------------------------------------------------------------------
    // Ambient
    // -------------------------------------------------------------------------

    [Header("Ambient — World Map")]
    [Tooltip("World Map base ambient. 2D, ch10, -28 LUFS. Always active on World Map.")]
    public AudioClip WorldMapBaseAmbient;

    [Tooltip("Empire ambient layer. Active when zoomed into dense city region.")]
    public AudioClip WorldMapEmpireAmbient;

    [Header("Ambient — City Inspector States")]
    [Tooltip("City inspector ambient — Owned state. Market sounds, -26 LUFS.")]
    public AudioClip CityOwnedAmbient;

    [Tooltip("City inspector ambient — At War state. Tension drone, -22 LUFS.")]
    public AudioClip CityAtWarAmbient;

    // Unclaimed = silent (no clip)
    // Vassal uses CityOwnedAmbient at _vassalAmbientVolume (-30 LUFS)

    // -------------------------------------------------------------------------
    // Announcer — TW Herald Voice Lines
    // -------------------------------------------------------------------------

    [Header("Announcer — TW Herald Lines")]
    [Tooltip("Minimum 2 variants per event. PlayAnnouncer() selects randomly.")]
    public AudioClip[] AnnouncerWarDeclared;
    public AudioClip[] AnnouncerWarScheduled;
    public AudioClip[] AnnouncerBattleWarning;
    public AudioClip[] AnnouncerBattleStart;
    public AudioClip[] AnnouncerVictoryDefend;
    public AudioClip[] AnnouncerVassalGained;
    public AudioClip[] AnnouncerDefeatVassal;
    public AudioClip[] AnnouncerVassalFreed;
    public AudioClip[] AnnouncerAttackerForfeit;

    // -------------------------------------------------------------------------
    // SFX — Critical Notifications (priority = 0, PCM, cannot be voice-stolen)
    // -------------------------------------------------------------------------

    [Header("SFX — Critical (priority = 0)")]
    [Tooltip("CRITICAL: war declared against player. AudioSource.priority = 0. PCM format.")]
    public AudioClip[] NotifWarDeclared;

    [Tooltip("CRITICAL: battle starting now. AudioSource.priority = 0. PCM format.")]
    public AudioClip[] NotifBattleStart;

    // -------------------------------------------------------------------------
    // SFX — World Map Navigation
    // -------------------------------------------------------------------------

    [Header("SFX — World Map Navigation")]
    [Tooltip("Continuous loop during map drag. Low frequency horn pad + string glissando.")]
    public AudioClip MapPanLoop;

    public AudioClip[] MapZoomIn;
    public AudioClip[] MapZoomOut;
    public AudioClip[] CityHover;
    public AudioClip[] CityClick;

    // -------------------------------------------------------------------------
    // SFX — City Interaction
    // -------------------------------------------------------------------------

    [Header("SFX — City Interaction")]
    public AudioClip[] CityInspectOpen;
    public AudioClip[] CityInspectClose;

    [Tooltip("3 variants recommended for high-frequency event.")]
    public AudioClip[] CityPurchase;

    public AudioClip[] CityUpgrade;
    public AudioClip[] CityNameConfirm;

    // -------------------------------------------------------------------------
    // SFX — War Declaration Flow
    // -------------------------------------------------------------------------

    [Header("SFX — War Declaration Flow")]
    [Tooltip("Player initiates war. Ducks BGM -8 dB / 2s.")]
    public AudioClip[] WarDeclareButton;

    public AudioClip[] WarWindowSlot;
    public AudioClip[] WarWindowConfirm;
    public AudioClip[] WarCancel;

    // -------------------------------------------------------------------------
    // SFX — City State Changes
    // -------------------------------------------------------------------------

    [Header("SFX — City State Changes")]
    public AudioClip[] StateAtWar;
    public AudioClip[] StateVassal;
    public AudioClip[] StateLiberated;
    public AudioClip[] StateVassalReleased;

    // -------------------------------------------------------------------------
    // SFX — Non-Critical Notifications
    // -------------------------------------------------------------------------

    [Header("SFX — Notifications (non-critical)")]
    public AudioClip[] NotifBattleWarning;
    public AudioClip NotifResultPing;

    // -------------------------------------------------------------------------
    // SFX — Social / Invite
    // -------------------------------------------------------------------------

    [Header("SFX — Social")]
    public AudioClip[] SocialInviteReceived;
    public AudioClip[] SocialJoinTeam;

    [Tooltip("Rate-limited: max 1 per 2 seconds. See TerritoryWarAudioController._teammateJoinRateLimitSecs.")]
    public AudioClip[] SocialTeammateJoined;

    // -------------------------------------------------------------------------
    // Tuning Knobs (all configurable without code changes)
    // -------------------------------------------------------------------------

    [Header("Tuning — Snapshot Transitions")]
    [Range(0.5f, 5f)] public float WarDeclaredSnapshotDuration = 3f;
    [Range(0.5f, 5f)] public float TensionCrossfadeDuration    = 2f;
    [Range(0.5f, 5f)] public float WorldMapSnapshotDuration    = 1.5f;

    [Header("Tuning — Empire Scale Thresholds")]
    [Min(1)] public int BrassLayerCityThreshold = 5;
    [Min(1)] public int BellsLayerCityThreshold = 20;
    [Range(0.5f, 10f)] public float AdditiveLayerFadeDuration = 2f;

    [Header("Tuning — City Inspector Volumes")]
    [Range(0f, 1f)] public float OwnedAmbientVolume  = 0.55f;
    [Range(0f, 1f)] public float VassalAmbientVolume = 0.35f;
    [Range(0f, 1f)] public float AtWarAmbientVolume  = 0.65f;

    [Header("Tuning — Rate Limiting")]
    [Range(0.5f, 5f)] public float TeammateJoinRateLimitSecs = 2f;

    [Header("Tuning — BGM Ducking")]
    [Range(-20f, 0f)] public float WarDeclaredBgmDuckDb  = -6f;
    [Range(-20f, 0f)] public float WarButtonBgmDuckDb    = -8f;
    [Range(-20f, 0f)] public float CityPurchaseBgmDuckDb = -3f;

    // -------------------------------------------------------------------------
    // Validation
    // -------------------------------------------------------------------------

#if UNITY_EDITOR
    private void OnValidate()
    {
        ValidateArrayMinLength(nameof(AnnouncerWarDeclared), AnnouncerWarDeclared, 2);
        ValidateArrayMinLength(nameof(AnnouncerBattleStart), AnnouncerBattleStart, 2);
        ValidateArrayMinLength(nameof(NotifWarDeclared), NotifWarDeclared, 2);
        ValidateArrayMinLength(nameof(NotifBattleStart), NotifBattleStart, 2);
        ValidateArrayMinLength(nameof(CityPurchase), CityPurchase, 3);

        if (BellsLayerCityThreshold <= BrassLayerCityThreshold)
            UnityEngine.Debug.LogWarning(
                $"[TerritoryWarAudioConfig] BellsLayerCityThreshold ({BellsLayerCityThreshold}) " +
                $"should be greater than BrassLayerCityThreshold ({BrassLayerCityThreshold}).",
                this);
    }

    private void ValidateArrayMinLength(string fieldName, AudioClip[] clips, int minLength)
    {
        if (clips == null || clips.Length < minLength)
            UnityEngine.Debug.LogWarning(
                $"[TerritoryWarAudioConfig] {fieldName} requires at least {minLength} variant(s). " +
                $"Current: {(clips?.Length ?? 0)}.",
                this);
    }
#endif
}
