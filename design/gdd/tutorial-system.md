# Tutorial System — Game Design Document

**System ID**: M9
**Version**: 0.1.0
**Status**: Draft — Reverse-documented from source ⚠️ STUB/INCOMPLETE
**Last Updated**: 2026-04-02
**Dependencies**: Hero System (C2), Combat & Skills (C1), Item System (FT1), Map & Objectives (FT2), HUD & UI (P1)

---

## 1. Overview

Tutorial System ในปัจจุบันแบ่งเป็นสองส่วน: (1) **Training Mode** — sandbox ที่ใช้งานได้จริง ให้ผู้เล่น practice กับ Dummy/Creep ที่ Spawn เองได้, ใช้ Cheat tools, เปลี่ยน Hero ได้; และ (2) **UIGuideTutorial** — UI framework สำหรับ Onboarding progression แต่ยังเป็น stub ที่ไม่มี content หรือ persistence จริงๆ

---

## 2. Player Fantasy

*(Design Intent — ยังไม่ implement)*

ผู้เล่นใหม่เรียนรู้ระบบ MOBA ทีละขั้นตอน — เคลื่อนที่, โจมตี, ใช้สกิล, ซื้อไอเทม, ทำ Objective — ผ่าน Tutorial ที่มีคำแนะนำชัดเจน และได้รับรางวัลเมื่อจบ Tutorial แต่ละด่าน

---

## 3. Detailed Rules

### 3.1 Training Mode (Implemented)

Training Mode เป็น Sandbox เต็มรูปแบบ เข้าถึงผ่าน Mode Selection → Training

**Session Flow**:
```
UIModeSelectionView.OnTrainingButton()
  → ModeTraining() สร้าง session:
      ModeSelectionType = Training
      MatchType = Findmatch
      SessionName = "training_{randomUID}"
      TeamSize = Five
  → NetworkTrainingMode เริ่ม Game
  → UITrainingMenuView เปิด (debug toolset)
```

---

### 3.2 Training Spawn Tools

| ประเภท | วิธีใช้ | จำกัด |
|--------|--------|-------|
| Dummy (Melee) | Click วางตำแหน่ง | Max 5 ตัว |
| Melee Creep | Click วางตำแหน่ง | — |
| Range Creep | Click วางตำแหน่ง | — |
| Siege Creep | Click วางตำแหน่ง | — |
| Super Creep | Click วางตำแหน่ง | — |
| Jungle Creep | Click วางตำแหน่ง | — |
| MiniBoss | ปุ่ม Spawn | — |
| Boss (Early/End game) | ปุ่ม Spawn | — |

**Kill All**: `OnKillObject()` — despawn Dummy/Creep/Jungle camps ทั้งหมด

---

### 3.3 Cheat Tools (Training Mode)

| Tool | ฟังก์ชัน | เงื่อนไข |
|------|---------|---------|
| Level Up (+1) | `OnIncreaseLevelupButtonClick()` | — |
| Max Level | `OnLevelMaxButtonClick()` — set exp=100000 | — |
| +1000 Gold | `OnIncreaseMoney1000ButtonClick()` | — |
| +5000 Gold | `OnIncreaseMoney5000ButtonClick()` | — |
| Reset Gold | `OnResetMoneyButtonClick()` — withdraw all | — |
| Kill Hero | `OnDeathButtonClick()` | — |
| Teleport | `OnTeleport(enable)` — click to teleport | — |
| God Mode | `OnImmortal(godmode)` | `#if RADIUS_CONSOLECOMMAND` |
| Unlimited Skills | `OnUseskillnolimit(enable)` | `#if RADIUS_CONSOLECOMMAND` |
| Add/Remove Status | `OnAddStatusEffect()` / `OnRemoveStatusEffect()` | — |

---

### 3.4 Hero Switching (Training Mode)

```
OnPickHeroButton() → แสดง Hero Picker Panel
  → SetupHero() — โหลด Hero ทั้งหมด (filtered by ShowAllHero flag)
  → แสดง icon, name, lane; Disabled ถ้าไม่ได้ Unlock

OnSelectHeroButton(avatarId):
  → Guard: m_IsSwitching (ป้องกัน rapid switch)
  → Despawn current actor (network)
  → Remove from AvatarSystem
  → Set new avatarId, skinId, weaponId
  → Wait 1s
  → Respawn new actor พร้อม initial gold
  → Update UI avatar display
  → Update camera + input binding

Search: OnSearchAvatarField(text) — filter by Hero name
Filter: OnLaneToggleChanged(toggle, isOn) — filter by CBSLane
```

---

### 3.5 Lane Configuration

| Toggle | Lane |
|--------|------|
| Top | CBSLane.Top |
| Jungle | CBSLane.Jungle |
| Mid | CBSLane.Mid |
| Bot | CBSLane.Bot |
| Support | CBSLane.Support |

- Enable/Disable Creep spawn per lane: `OnEnableCreepLaneTop/Mid/Bottom(bool)`

---

### 3.6 UIGuideTutorial (Stub — ไม่ Implement จริง)

UI framework สำหรับ Onboarding progression:

```
m_Button[]        — Tutorial stage buttons (array)
m_IsLock[]        — Lock indicators per stage
m_IsClear[]       — Completion indicators
m_TextComplete[]  — "Complete" labels

OnClearMission(levelIndex):
  → Mark stage complete
  → Unlock next stage
```

**Skip Tutorial Popup**: "Do you want to skip the tutorial? If you skip the tutorial, you can't get the rewards."

⚠️ **ไม่มี content จริง**: `test()` hardcoded `levelAt=1`, ไม่มี persistence, ไม่มี step-by-step guidance

---

## 4. Formulas

### Initial Gold (Training)
```
InitialGold = CBSConfigBattle.InitialMoney  (Server Config)
```

### Level Up Experience Table
```
ExpTable = CBSBattleLevelExp  (List<ExpPerLevel>)
MaxLevelExp = 100000 (hard-coded in OnLevelMaxButtonClick)
```

---

## 5. Edge Cases

| สถานการณ์ | พฤติกรรม |
|-----------|---------|
| Spawn > 5 Dummies | ถูก block (DUMMY_LIMIT = 5) |
| Hero switch ระหว่าง Skill | m_IsSwitching guard ป้องกัน |
| Cheat tools ใน non-DEBUG build | God Mode / Unlimited Skills ต้องการ `#if RADIUS_CONSOLECOMMAND` |
| Tutorial completion | ⚠️ ไม่บันทึก — Client-side only |
| Tutorial reward | ⚠️ Popup mention แต่ไม่มี logic |

---

## 6. Dependencies

| ระบบ | ความสัมพันธ์ |
|------|------------|
| **Hero System (C2)** | Hero switching, Avatar data |
| **Combat & Skills (C1)** | Skill testing, Status effects |
| **Item System (FT1)** | Gold manipulation, Item testing |
| **Creep/Minion System (FT3)** | Creep/Boss spawning |
| **AI/Bot System (FT5)** | Bot Creep behavior ใน Training |
| **Level/XP (C5)** | Level up cheats |

---

## 7. Tuning Knobs

| ค่า | ที่อยู่ | ค่าปัจจุบัน | ผลกระทบ |
|-----|--------|------------|---------|
| Dummy Limit | UITrainingMenuView | **5** | Max Dummy พร้อมกัน |
| Initial Gold | CBSConfigBattle.InitialMoney | Server Config | ทองเริ่มต้น Training |
| Max Level EXP | UITrainingMenuView | **100000** | Level สูงสุดใน Training |
| ShowAllHero | BuildSettings | bool | เห็น Hero ที่ไม่ได้ Unlock |

---

## 8. Acceptance Criteria

**Training Mode**:
- [ ] เข้า Training Mode จากหน้า Mode Selection
- [ ] Spawn Dummy / Creep types ได้ด้วย click
- [ ] Dummy limit 5 ตัว
- [ ] Level Up, Max Level, Gold cheats ทำงาน
- [ ] เปลี่ยน Hero ได้ใน Training; Hero ใหม่ spawn ได้ใน 2 วินาที
- [ ] Kill All ลบ enemies ทั้งหมด
- [ ] Teleport click ทำงาน
- [ ] Boss / MiniBoss Spawn ทำงาน

**Tutorial (เมื่อ Implement)**:
- [ ] ขั้นตอน Tutorial แนะนำ: Movement → Skill → Item → Objective
- [ ] Tutorial Progress บันทึกกับ Player Account
- [ ] Skip Tutorial แสดง confirmation popup
- [ ] Reward มอบให้เมื่อจบ Tutorial แต่ละ stage

---

## Known Issues / TODO

- ⚠️ **CRITICAL: Tutorial Content ไม่มี**: UIGuideTutorial เป็น stub — ไม่มี guided steps, hints, หรือ objectives
- ⚠️ **CRITICAL: Tutorial Persistence ไม่มี**: Progress เป็น client-side เท่านั้น — reset เมื่อปิดเกม
- ⚠️ **Tutorial Rewards ไม่ Implement**: Skip popup กล่าวถึงรางวัลแต่ไม่มี logic
- ⚠️ **Typo**: `SetpHero()` ควรเป็น `SetupHero()`
- ⚠️ **Bug**: `AddListener` แทน `RemoveListener` ใน UITrainingMenuView บางจุด
- ⚠️ **Bot Lane Hard-code**: `ObjectiveLane.Mid` สำหรับ Creep Bot ใน Training
- ⚠️ **800+ Line Script**: UITrainingMenuView ควรแยกเป็น DummySpawner, CheatPanel, HeroSelector
