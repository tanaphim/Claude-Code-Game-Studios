# UX Spec: Hero Select UI

> **Status**: Draft — Ready for Review
> **Author**: Tanapol Aekprapu + ux-designer
> **Last Updated**: 2026-04-17
> **Journey Phase(s)**: Post-Matchmaking → Pre-Game
> **GDD Reference**: design/gdd/hero-select-ui.md (P3)
> **Template**: UX Spec

---

## Purpose & Player Need

ผู้เล่นต้องเลือก hero 1 ตัวจาก roster 25+ ตัว และเลือก skin ก่อน timer หมด
หน้าจอนี้เป็นจุดเดียวที่ผู้เล่น commit hero choice และ customize appearance ก่อนเกมเริ่ม

**Player arrives wanting to**: เลือก hero ที่เหมาะกับทีมของตัวเองและ counter ทีมศัตรู
ภายในเวลาจำกัด โดยมีข้อมูลเพียงพอสำหรับการตัดสินใจ

**ถ้า screen นี้ยากใช้**: ผู้เล่นเสียเวลา navigate แทน strategy → ถูก auto-lock
hero ที่ไม่ต้องการ → frustrating experience โดยเฉพาะใน Ranked mode

**ผู้เล่นใหม่ need**: filter ด้วย role/lane เพื่อหา hero ที่เหมาะสมได้เร็ว
โดยไม่ต้อง browse ทุกตัว

---

## Player Context on Arrival

ผู้เล่นเพิ่งผ่าน Matchmaking queue (1–50 นาที ขึ้นอยู่กับโหมด) และถูก redirect
มาที่หน้านี้โดยอัตโนมัติเมื่อ Match Found — **ไม่ได้มาเอง**

**Emotional state**: กึ่ง time-pressured — รู้ว่ามี timer แต่ยังมีเวลาพิจารณา
- ผู้เล่นเก่า: รู้ hero ที่ต้องการล่วงหน้า → มาเพื่อ execute
- ผู้เล่นใหม่: ยังไม่มั่นใจ → ต้องการ guidance ผ่าน role/lane filter

**Context ที่นำมา**:
- รู้ game mode ที่เลือก (Ranked/Casual/etc.) และ team size
- บางส่วนตัดสินใจ hero ล่วงหน้าระหว่างรอ queue แล้ว
- ยังไม่รู้ว่าทีมศัตรู pick hero อะไร (ไม่มี ban phase)

---

## Navigation Position

`Menu/Lobby → Find Match (Matchmaking Queue) → Match Found → **Hero Select** → Game`

Context-dependent screen — เข้าถึงได้เฉพาะเมื่อ Matchmaking พบ Match แล้วเท่านั้น
ไม่สามารถเข้าจาก main menu โดยตรง

---

## Entry & Exit Points

### Entry

| Entry Source | Trigger | Player carries this context |
|---|---|---|
| Matchmaking System | `GameState: HeroSelect` — server push | Game mode, team size, player data |
| Custom Room | Custom session start | Custom room config |

### Exit

| Exit Destination | Trigger | Notes |
|---|---|---|
| Game (In-Match) | ทุกผู้เล่น Lock In → `ReadyToPlay` | **One-way** — ไม่กลับมา Hero Select |
| Game (In-Match) via Auto-lock | Timer หมด → `ReadyToPlay` | **One-way** — ระบบเลือกให้ |
| Menu/Lobby (disconnect) | Player disconnect ก่อน Lock In | ⚠️ Unhandled disconnect flow — known issue จาก GDD |

---

## Layout Specification

### Information Hierarchy

| Priority | Information | Visibility |
|---|---|---|
| 1 | Timer countdown | Always visible — persistent |
| 2 | Hero grid | Always visible — primary content area |
| 3 | Lock In button | Always visible — primary action |
| 4 | Team lineups (ทั้ง 2 ทีม) | Always visible — strategy info |
| 5 | Hero detail panel | Conditional — แสดงเมื่อ select hero |
| 6 | Lane/Role filter | Always visible — above grid |
| 7 | Skin carousel | Phase 2 only — หลัง Lock In |
| 8 | Format label | Persistent — "5VS5" |

---

### Layout Zones

**Option C: Hybrid** — Team Lineups ขอบซ้าย-ขวา, content กลาง stacked vertically

```
LEFT PANEL   |   CENTER PANEL              |   RIGHT PANEL
Team 1 Lineup|   Filter Pills + Search     |   Team 2 Lineup
             |   Hero Grid                 |
             |   Hero Detail Panel         |
             |   Timer + Lock In Button    |
```

---

### Component Inventory

#### Zone: Left Panel — Team 1 Lineup

| Component | Content | Interactive | Pattern |
|---|---|---|---|
| Team 1 header | "ทีม 1" + player count | No | — |
| Hero portrait slots | Hero icon (locked) / placeholder | No | TEAM_LINEUP |
| Player name labels | ชื่อผู้เล่นแต่ละ slot | No | — |

#### Zone: Right Panel — Team 2 Lineup

| Component | Content | Interactive | Pattern |
|---|---|---|---|
| Team 2 header | "ทีม 2" + player count | No | — |
| Hero portrait slots | Hero icon (locked) / placeholder | No | TEAM_LINEUP |
| Player name labels | ชื่อผู้เล่นแต่ละ slot | No | — |

#### Zone: Center — Filter + Hero Grid

| Component | Content | Interactive | Pattern |
|---|---|---|---|
| Lane/Role filter pills | All, Tank, Fighter, Mage, Assassin, Carry, Support | Yes | FILTER_PILLS ⚠️ (to formalize) |
| Search input | ค้นหาชื่อ hero (optional) | Yes | SEARCH_INPUT |
| Hero grid cards | 25+ hero cards | Yes | HERO_CARD |
| Greyed hero cards | Hero ที่ไม่ได้ unlock | No (blocked) | GREYED_CARD |

#### Zone: Center — Hero Detail Panel (conditional: show when hero selected)

| Component | Content | Interactive | Pattern |
|---|---|---|---|
| Hero portrait large | Hero art | No | HERO_DETAIL_PANEL ⚠️ (to formalize) |
| Hero name + role tags | ชื่อ + role/lane labels | No | — |
| Complexity rating bars | ATK/DEF/CC/Mobility/Utility/Complexity (0–10) | No | STAT_PANEL |
| Skill icons row | Passive + Q/W/E/R icons | Hover → tooltip | HOVER_TOOLTIP |

#### Zone: Center — Timer + Action Bar

| Component | Content | Interactive | Pattern |
|---|---|---|---|
| Format label | "5VS5" | No | — |
| Countdown timer | เวลาที่เหลือ (นับถอยหลัง) | No | COUNTDOWN |
| Lock In button | "Lock In" — enabled เมื่อมี selection | Yes | LOCK_IN_BTN |
| Lock In disabled | เมื่อยังไม่มี selection | No | DISABLED_BTN |

#### Zone: Center — Skin Carousel (Phase 2: after Lock In)

| Component | Content | Interactive | Pattern |
|---|---|---|---|
| Skin carousel | สกินที่ unlock | Yes (swipe/arrows) | CAROUSEL |
| Skin preview art | ภาพ skin ขนาดใหญ่ | No | — |
| Skin name + tier label | ชื่อ + tier (Common/Rare/Ultimate/Legendary) | No | — |
| Confirm button | "Confirm" → เข้าเกม | Yes | LOCK_IN_BTN (same pattern) |
| Locked skin cards | สกินที่ยังไม่ unlock | No | GREYED_CARD |

> ⚠️ **New patterns to formalize before implementation**: FILTER_PILLS, HERO_DETAIL_PANEL
> → เพิ่มใน `design/ux/interaction-patterns.md` ก่อน merge

---

### ASCII Wireframe

#### Phase 1: Hero Selection

```
┌──────────────────────────────────────────────────────────────────────┐
│                          HERO SELECT                                  │
├─────────────┬─────────────────────────────────────┬──────────────────┤
│  TEAM 1     │ [All][Tank][Fight][Mage][Assa][Carr][Sup]  [🔍]        │  TEAM 2      │
│             │                                      │                  │
│  [?] P1     │  ┌──┐┌──┐┌──┐┌──┐┌──┐┌──┐┌──┐┌──┐ │  [?] P6          │
│  [?] P2     │  │H1││H2││H3││H4││H5││H6││H7││H8│ │  [?] P7          │
│  [?] P3     │  └──┘└──┘└──┘└──┘└──┘└──┘└──┘└──┘ │  [?] P8          │
│  [?] P4     │  ┌──┐┌──┐┌──┐┌──┐ ...               │  [?] P9          │
│  [?] P5     │  └──┘└──┘└──┘└──┘                   │  [?] P10         │
│             ├─────────────────────────────────────┤                  │
│   5VS5      │  Hero: HERO NAME  • Tank • Top Lane  │   5VS5           │
│             │  ████ ATK  ████ DEF  ███ CC           │                  │
│             │  Skills: [P] [Q] [W] [E] [R]          │                  │
│             ├─────────────────────────────────────┤                  │
│             │      ⏱ 00:45    [   LOCK IN   ]       │                  │
└─────────────┴─────────────────────────────────────┴──────────────────┘
```

#### Phase 2: Skin Selection (หลัง Lock In)

```
┌──────────────────────────────────────────────────────────────────────┐
│  TEAM 1 (locked)           ⏱ 00:30             TEAM 2 (...)          │
├─────────────┬─────────────────────────────────────┬──────────────────┤
│  [H1] P1 ✓  │   ◀  [DEFAULT] [SKIN A] [SKIN B▒]  ▶                  │  [?] P6       │
│  [H2] P2 ✓  │                                      │  ...             │
│  ...        │   [Large skin preview area]           │                  │
│             │                                      │                  │
│             │   "Default"  [Common]                 │                  │
│             │                                      │                  │
│             │           [   CONFIRM   ]            │                  │
└─────────────┴─────────────────────────────────────┴──────────────────┘
```

> ▒ = locked skin (GREYED_CARD pattern)

---

## States & Variants

| State / Variant | Trigger | What Changes |
|---|---|---|
| **Default — Browsing** | เข้า Hero Select screen | Hero grid แสดงเต็ม, ไม่มี hero selected, Lock In disabled |
| **Hero Previewing** | กด hero card ครั้งแรก | Hero detail panel แสดง, card highlighted, Lock In enabled |
| **Skin Selection (Phase 2)** | กด Lock In สำเร็จ | Hero grid/detail ปิด, Skin carousel เปิด |
| **Waiting (others)** | ผู้เล่นเอง confirm แต่รอคนอื่น | Skin phase active, Team lineup ยัง update real-time |
| **Auto-lock** | Timer หมด + ยังไม่ Lock | ระบบ lock hero ที่ highlighted; ถ้าไม่มี selection → default hero ⚠️ |
| **Auto-lock (Skin phase)** | Timer หมดระหว่าง Skin phase | ระบบ confirm skin ปัจจุบัน → transition to game |
| **Timer urgency** | Timer ≤ 10 วินาที | Timer color เปลี่ยน (red) + pulse animation |
| **Loading** | Hero data ยังโหลดไม่เสร็จ | Skeleton/spinner แทน hero grid |
| **Disconnected player** | ผู้เล่นในทีม disconnect ก่อน Lock | ⚠️ Unhandled — open question (ดู Open Questions) |

---

## Interaction Map

Input methods: **Keyboard/Mouse** (PC/Steam). No gamepad requirement for this screen.

| Component | Action | Input | Immediate Feedback | Outcome |
|---|---|---|---|---|
| Lane/Role filter pill | Click | Left click / Enter | Pill highlights; grid refilters | Hero grid filters by role |
| Hero card (unlocked) | Click once | Left click / Enter | Card highlighted; detail panel appears | Hero Previewing state |
| Hero card (unlocked) | Click twice | Left click / Enter | Lock In animation | Equivalent to Lock In button |
| Hero card (locked) | Hover | Mouse hover | HOVER_TOOLTIP: "Locked" | No state change |
| Hero detail panel — skill icon | Hover | Mouse hover | HOVER_TOOLTIP: skill name + description | No state change |
| Lock In button | Click | Left click / Enter | Button pulse; phase transition animation | Skin Selection state |
| Lock In button (disabled) | Hover | Mouse hover | HOVER_TOOLTIP: "Select a hero first" | No action |
| Hero grid | Keyboard navigation | Arrow keys | Focus ring moves between cards | Card focused |
| Skin carousel | Navigate | ◀▶ arrow buttons / Left+Right arrow keys | Skin slides; preview updates | Current skin changes |
| Skin carousel locked item | Hover | Mouse hover | HOVER_TOOLTIP: "Locked — purchase to unlock" | No state change |
| Confirm button | Click | Left click / Enter | Button pulse | Transition to game |

---

## Events Fired

| Player Action | Event Fired | Payload | Notes |
|---|---|---|---|
| Select hero | `HeroSelected` | HeroID, PlayerID | Broadcast to team (real-time lineup update) |
| Lock In hero | `HeroLockIn` | HeroID, PlayerID, timestamp | Persistent — cannot undo |
| Confirm skin | `SkinConfirmed` | HeroID, SkinID, PlayerID | Persistent |
| Filter by role | — (local UI state only) | — | No network event |
| Search input | — (local filter only) | — | No network event |
| Auto-lock triggered | `HeroAutoLocked` | HeroID, PlayerID, reason | Fired by server when timer expires |
| Timer expired | `SelectTimerExpired` | PlayerID | Server-side event |

---

## Transitions & Animations

| Transition | Animation | Duration | Notes |
|---|---|---|---|
| Screen enter | Fade in from black | ~0.3s | หลัง Matchmaking connect |
| Hero card select | Detail panel fades/slides in | ~0.2s | Responsive feel |
| Lock In confirm | Button pulse + panel swap (grid → carousel) | ~0.3s | Clear phase change |
| Skin carousel slide | Horizontal slide | ~0.15s | Linear ease — no bounce |
| Team lineup update | Hero portrait fades into slot | ~0.2s | Real-time when team member locks |
| Timer urgency (≤10s) | Timer text pulses red + size increase | Repeating | Color + size (not color only) |
| Screen exit → Game | Fade to black | ~0.5s | → Loading screen |
| Auto-lock flash | Lock In button flash + "Auto-locked" overlay | ~0.5s | Inform player clearly |

> ⚠️ Motion consideration: Carousel slides and panel transitions must use linear/ease-out easing.
> No screen shake, bounce, or scale effects that could cause motion sickness.
> All animations must respect a **Reduced Motion** setting in game options (fallback: instant cut).

---

## Data Requirements

| Data | Source System | Read / Write | Notes |
|---|---|---|---|
| Hero roster list | Hero System (C2) / CBS | Read | โหลดก่อนเข้าหน้าจอ |
| Hero unlock status (per player) | Hero System (C2) / CBS | Read | กำหนด enabled vs greyed |
| Hero stats & complexity ratings | Hero System (C2) / CBSUnit | Read | แสดงใน detail panel |
| Hero skill icons & descriptions | Hero System (C2) | Read | แสดงใน detail panel |
| Pick timer value | Data-Config (F3) / `CBSConfigBattle.PickupAvatarTime` | Read | Server-authoritative |
| Player skin ownership | Customization System (M3) / CBS | Read | locked vs unlocked skins |
| Team lineup — real-time picks | Game Mode Manager (FT7) / Photon Fusion | Read (real-time) | network state sync |
| Player's hero selection | → Game Mode Manager (FT7) | Write | Trigger: Lock In |
| Player's skin selection | → Customization System (M3) | Write | Trigger: Confirm |
| Game mode + team size | Matchmaking (FT6) | Read | แสดง format label ("5VS5") |

> ⚠️ Architecture constraints:
> - Timer ต้องมาจาก server config เท่านั้น — ห้าม local timer ของ UI
> - Lock In และ Confirm ต้องส่งผ่าน network — ห้าม local state write
> - UI อ่านได้เท่านั้น (display only) — ห้าม UI เป็น owner ของ hero selection state

---

## Accessibility

**Tier**: Basic — keyboard navigable, contrast compliant, no color-only indicators

### Keyboard Navigation Path

1. Tab → Filter pills group → Arrow Left/Right to switch role filter
2. Tab → Hero grid → Arrow keys navigate cards; Enter selects (preview)
3. Enter again on selected card OR Tab → Lock In button → Enter (confirm)
4. Skin phase: Tab → carousel Left arrow → Arrow keys change skin → Tab → Confirm → Enter

### Checklist

- ✅ Keyboard navigable: all interactive elements reachable via Tab / Arrow / Enter
- ✅ Focus indicators: visible focus ring on all cards, buttons, filter pills
- ✅ Color independence: locked hero/skin = greyed opacity + tooltip (not color-only)
- ✅ Timer urgency: color change + text size pulse (not color-only — both cues together)
- ✅ Team lineup: hero portrait + text placeholder (not empty colored slot only)
- ✅ Stat bars: numeric label alongside bar (not bar-only)
- ✅ Reduced motion: all transitions linear/ease-out; game settings must include Reduced Motion → instant cut fallback
- ✅ Locked content tooltip: always explains why (e.g., "Locked — purchase to unlock")
- ✅ Button disabled state: tooltip explains precondition (e.g., "Select a hero first")

---

## Localization Considerations

| Element | Max Length | Priority | Notes |
|---|---|---|---|
| "Lock In" button label | ~8 chars | HIGH | Narrow button — German "Bestätigen" = 10 chars |
| "Confirm" button label | ~8 chars | HIGH | Same constraint |
| Role/Lane filter pills | ~8 chars each | HIGH | Pills are narrow — must have truncation or wrapping policy |
| Hero names (card) | ~12 chars | MEDIUM | Truncate with ellipsis if longer |
| Skill descriptions | ~120 chars | LOW | Tooltip — flexible width |
| Format label "5VS5" | Fixed (numeric) | None | Not translated |
| "Auto-locked" notification | ~20 chars | MEDIUM | Shown briefly — must fit in center banner |

> ⚠️ HIGH PRIORITY: Define max-width and overflow behavior (truncate vs wrap) for filter pills
> and action buttons before art handoff. A 40% text expansion (EN→DE) will break pill layout.

---

## Acceptance Criteria

- [ ] Screen เปิดได้ภายใน 2 วินาที หลัง `GameState: HeroSelect` trigger
- [ ] Hero grid แสดง heroes ทั้งหมด; heroes ที่ไม่ได้ unlock แสดง greyed out และกดไม่ได้
- [ ] กด hero card ครั้งแรก → Hero detail panel แสดงพร้อม name, role, complexity stats, skill icons
- [ ] Lock In button: disabled เมื่อไม่มี selection; enabled เมื่อมี selection (reactive ทันที)
- [ ] กด Lock In → Hero grid/detail ปิด; Skin carousel เปิดภายใน 0.5 วินาที
- [ ] Skin carousel: แสดง skins ที่ unlock; skins ที่ locked แสดง greyed out พร้อม tooltip อธิบาย
- [ ] Timer นับถอยหลังจาก `CBSConfigBattle.PickupAvatarTime` (server-sourced ไม่ใช่ local)
- [ ] Timer ≤ 10 วินาที → timer เปลี่ยนสีแดง + pulse animation
- [ ] Timer = 0 → ระบบ auto-lock hero ที่ highlighted; แสดง "Auto-locked" notification ที่ center
- [ ] Team lineup ทั้ง 2 ทีม update แบบ real-time เมื่อผู้เล่นแต่ละคน Lock In
- [ ] Role filter pills กรอง hero grid ถูกต้องตาม role — "All" แสดงทุกตัว
- [ ] Keyboard navigation ครอบคลุมทุก interactive element (Tab/Arrow/Enter — ไม่ต้องใช้ mouse)
- [ ] ทุก interactive element มี visible focus indicator ที่เห็นได้ชัด
- [ ] ไม่มี color-only state indicator — ทุก state มี text, icon, หรือ shape ประกอบด้วยเสมอ

---

## Open Questions

1. **Auto-lock ไม่มี selection**: ถ้า Timer หมดและไม่มี hero ที่ highlighted เลย — ระบบเลือก default hero อะไร? (⚠️ known issue จาก GDD)
2. **Disconnect ใน Hero Select**: ผู้เล่น disconnect ก่อน Lock In ควรทำอะไร? — Auto-fill ด้วย bot? Kick ออก? Reconnect? (⚠️ known issue จาก GDD)
3. **Filter pills: exclusive หรือ multi-select?** — กด "Tank" แล้วกด "Fighter" → แสดง Tank เท่านั้น หรือ Tank+Fighter พร้อมกัน?
4. **Search input**: จำเป็นต้องมีใน Hero Select หรือไม่? 25+ heroes อาจพอ browse ด้วย filter pill ได้
5. **Skin carousel wrap**: ที่ end of skin list — wrap กลับ first หรือ stop? (ระบุใน CAROUSEL pattern ว่า stop แต่ต้องยืนยัน)
6. **Reduced Motion setting**: มีใน game settings แล้วหรือไม่? ถ้ายังไม่มีต้องเพิ่ม
