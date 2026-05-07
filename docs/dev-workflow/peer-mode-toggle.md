# Photon Fusion PeerMode — Multipeer ↔ Production Toggle

**Date:** 2026-05-07
**Discovery sprint:** Sprint 003 (S3-05 soft-verify root cause)
**Affects:** delta-unity repo, `Assets/Photon/Fusion/Resources/NetworkProjectConfig.fusion`

---

## TL;DR

Switch `PeerMode` in `NetworkProjectConfig.fusion` between **Single (`0`)** and
**Multiple (`1`)** depending on what scene you are running:

| Scene type | Required PeerMode | File value |
|---|---|---|
| **Production / real match scene** (login → lobby → match) | Single | `"PeerMode": 0` |
| **Multipeer test harness** (`AbilityMultipeerRunner` Pass #4/#5) | Multiple | `"PeerMode": 1` |

**ลืม toggle = ปัญหา ที่จะเจอ:**
- ตั้ง Multiple แต่รัน production scene → STUN timeout + Photon "Version mismatch"
  errors → เข้า match ไม่ได้
- ตั้ง Single แต่รัน multipeer harness → harness ไม่ spawn second peer → Pass #4/#5 fail

---

## Background — why two modes exist

Photon Fusion 2 supports two peer-hosting models in a single Unity Editor process:

- **Single (`PeerMode: 0`)** — one `NetworkRunner` per process. Production runtime
  runs this. STUN/NAT-discovery uses one socket; matchmaking + Photon Cloud
  connection works normally.
- **Multiple (`PeerMode: 1`)** — multiple `NetworkRunner` instances co-exist in the
  same process, each with its own scene clone. Used by the S3-01 multipeer harness
  to simulate Host + Client without launching two builds.

When PeerMode is Multiple, Fusion expects callers to manage multiple peers. If a
production scene (which only spawns one runner) starts under Multiple mode, the
peer system mis-allocates sockets and Photon Realtime returns errors that look
like "STUN timeout" or version-related disconnects.

---

## How to toggle today (manual)

1. Open delta-unity repo → `Assets/Photon/Fusion/Resources/NetworkProjectConfig.fusion`
2. Edit the `"PeerMode"` value: `0` for Single, `1` for Multiple
3. Save → Unity Editor will reimport the asset
4. Restart Play Mode

**ห้าม commit toggle change** unless intentionally changing the project default.
The default for `dev` and `feature/refactor-ability-claude` branches is `PeerMode: 0`
(Single = production correct).

---

## Common workflow patterns

### Pattern A — Working on production playtest only
- Keep `PeerMode: 0` always. No action needed.

### Pattern B — Working on multipeer harness only (S3-01 pattern)
- Set `PeerMode: 1` at start of session.
- Do **not** commit the change.
- At end of session, `git checkout` the file to restore Single before pushing
  branch updates.

### Pattern C — Switching between both within one session
- Toggle manually each time.
- Watch the Console: if you see Photon connection errors mentioning version /
  STUN / disconnect during what should be production scene → check PeerMode first.

---

## Future automation (S4-P5 Nice to Have)

Manual toggle is friction-prone. Polish backlog item `S4-P5` proposes one of:

- **Editor menu**: `Tools → Network → Switch to Multipeer Mode / Production Mode`
  — flips PeerMode + saves asset
- **Scene-bound override**: hook on multipeer harness scene `OnEnable` that sets
  PeerMode to Multiple at runtime (revert on scene exit)
- **Per-scene scriptable config**: separate `NetworkProjectConfig` assets, swap
  in via build pipeline / Editor preference

Decision deferred until S4-P5 is pulled (Sprint 004 buffer or later).

---

## Diagnostic checklist

If production playtest fails with network errors, before deeper investigation:

1. ✅ Open `NetworkProjectConfig.fusion`, confirm `"PeerMode": 0`
2. ✅ Verify no uncommitted change on the file (`git diff Assets/Photon/Fusion/Resources/NetworkProjectConfig.fusion`)
3. ✅ Restart Editor Play Mode (PeerMode change takes effect on Runner.StartGame)
4. If still failing → escalate to environmental hypothesis (Photon dashboard,
   region, firewall) — see `production/qa/investigations/` for any deeper logs

---

## References

- [Sprint 003 Retrospective](../../production/retrospectives/sprint-003.md) §5 action item #1 — root cause discovery
- [Sprint 004 plan](../../production/sprints/sprint-004.md) — `S4-P5` polish item
- [ADR-0006 Phase 2 Migration Plan](../architecture/ADR-0006-phase-2-migration-plan.md) §3 Exit Criteria — manual playtest requirement (P2-10)
- Photon Fusion 2 docs: https://doc.photonengine.com/fusion/current/manual/peer-mode (verify version against pinned engine if links drift)
