# Interaction Pattern Library

> **Status**: Draft — Ready for Review
> **Author**: Tanapol Aekprapu + ux-designer
> **Last Updated**: 2026-04-17
> **Template**: Interaction Pattern Library
> **Game**: Delta (MOBA — PC/Steam, Keyboard/Mouse)

---

## Overview

Delta ใช้ UI patterns ที่สม่ำเสมอทั่วทั้งเกม ไลบรารีนี้เป็น single source of truth
สำหรับ interaction patterns ทุกอย่าง — ทั้ง designer และ engineer ใช้ร่วมกัน

**Platform**: PC (Steam) — Keyboard/Mouse primary
**Accessibility tier**: Basic (keyboard navigable, contrast compliant, no color-only indicators)
**Scope**: ทุก pattern ที่พบในระบบ Hero Select, HUD, Item Shop, และ Menu/Lobby UI

เมื่อ implement UI feature ใหม่ ให้ reuse pattern จากไลบรารีนี้ก่อนเสมอ
ถ้า pattern ที่ต้องการยังไม่มี ให้เพิ่มลงที่นี่ก่อน merge

---

## Pattern Catalog

| ID | Pattern Name | หมวด | ใช้ใน |
|----|-------------|------|-------|
| TAB_NAV | Tab Navigation | Navigation | Item Shop |
| MODE_GRID | Game Mode Selection Grid | Navigation | Menu/Lobby |
| HERO_CARD | Hero Grid Card | Selection | Hero Select |
| INV_SLOT | Inventory Slot | Selection | HUD, Item Shop |
| GREYED_CARD | Greyed-Out Disabled Card | Selection | Hero Select, Item Shop |
| CAROUSEL | Carousel Swipe | Selection | Hero Select (Skin) |
| SEARCH_INPUT | Search Input | Input/Action | Item Shop |
| LOCK_IN_BTN | Lock In Button | Input/Action | Hero Select |
| DISABLED_BTN | Disabled Button | Input/Action | Item Shop, HUD |
| UNDO_ACTION | Undo Action | Input/Action | Item Shop |
| RADIAL_MENU | Radial Context Menu | Input/Action | HUD (Ping) |
| COUNTDOWN | Countdown Timer | Feedback/State | Hero Select |
| RESOURCE_BAR | Resource Bar (HP/MP/Shield) | Feedback/State | HUD |
| COOLDOWN_RADIAL | Skill Cooldown Radial | Feedback/State | HUD |
| FLOAT_DMG | Floating Damage Numbers | Feedback/State | HUD |
| STATUS_ICON | Status Effect Icon | Feedback/State | HUD |
| HOVER_TOOLTIP | Hover Tooltip | Feedback/State | HUD, Item Shop |
| KILL_FEED | Kill Feed | Feedback/State | HUD |
| ANNOUNCE_BANNER | Timed Announcement Banner | Feedback/State | HUD |
| STAT_PANEL | Stat Display Panel | Data Display | HUD, Hero Select |
| TEAM_LINEUP | Team Lineup Panel | Data Display | Hero Select |
| SCOREBOARD | Scoreboard Overlay | Data Display | HUD |
| MINIMAP | Minimap | Data Display | HUD |
| ITEM_DETAIL | Item Detail Panel | Data Display | Item Shop |
| LOADING_DIALOG | Loading Dialog | Modal/Overlay | Menu/Lobby |
| ERROR_DIALOG | Error Dialog | Modal/Overlay | Menu/Lobby, Item Shop |
| INVITE_DIALOG | Invite Dialog | Modal/Overlay | Menu/Lobby |
| FRIEND_PANEL | Friend Panel | Modal/Overlay | Menu/Lobby |

---

## Patterns

### Navigation Patterns

#### TAB_NAV — Tab Navigation

**Category**: Navigation
**Used In**: Item Shop (`UIShopView`)

**Description**: Horizontal row of tab buttons ที่ switch เนื้อหาในพื้นที่ content area ด้านล่าง
tab ที่ active จะ highlight ชัดเจนกว่าตัวอื่น เนื้อหาเปลี่ยนทันทีเมื่อกด tab (ไม่มี animation transition)

**Specification**:
- Tab row อยู่ด้านบนของ content area
- Active tab: สีต่างจาก inactive (background highlight หรือ underline indicator)
- กด tab → content area เปลี่ยนทันที (no animation)
- Keyboard: Tab key navigates between tabs; Enter/Space selects
- เฉพาะ 1 tab ที่ active ได้ในเวลาเดียวกัน

**When to Use**: เมื่อมีเนื้อหา 2–5 หมวดที่เกี่ยวข้องกันในพื้นที่เดียว
**When NOT to Use**: มากกว่า 5 หมวด (ใช้ sidebar navigation แทน) หรือเนื้อหาที่ไม่เกี่ยวข้องกัน

---

#### MODE_GRID — Game Mode Selection Grid

**Category**: Navigation
**Used In**: Menu/Lobby (`UIHomeWidgetView`)

**Description**: Grid หรือ row ของปุ่ม Game Mode ที่ navigate ไปยัง context ที่แตกต่างกันอย่างสิ้นเชิง
แต่ละปุ่มเป็น large card-style button พร้อมชื่อโหมดและ optional description
แตกต่างจาก TAB_NAV ตรงที่ destination ต่างกัน ไม่ใช่แค่ content ในหน้าเดียวกัน

**Specification**:
- ปุ่มแต่ละอัน: Mode name + optional description/icon
- กด → navigate ไปยัง flow ของโหมดนั้น (Matchmaking queue, Town Hub ฯลฯ)
- Keyboard: Arrow keys navigate; Enter selects
- Disabled mode: แสดง greyed state (ใช้ GREYED_CARD pattern)

**When to Use**: Main hub screens ที่ผู้เล่นเลือก destination ที่ต่างกันโดยสิ้นเชิง
**When NOT to Use**: Options ทั้งหมดอยู่ใน context เดียวกัน (ใช้ TAB_NAV แทน)

---

### Selection & Grid Patterns

#### HERO_CARD — Hero Grid Card

**Category**: Selection
**Used In**: Hero Select (`m_HeroContent`)

**Description**: Card ใน grid แสดงข้อมูลสรุปของ hero 1 ตัว กดเพื่อ preview รายละเอียด
กดซ้ำหรือกด Lock In เพื่อยืนยัน มี state สำหรับ locked (greyed) และ selected (highlighted)

**Specification**:
- แต่ละ card: Hero icon + Hero name + Lane/Role tag
- States: Default / Hover / Selected (highlighted) / Locked (greyed out, non-interactive)
- กด card ครั้งแรก → แสดง Hero Detail Panel (preview)
- กด card ครั้งที่สอง หรือกด Lock In Button → confirm selection
- Keyboard: Arrow keys navigate grid; Enter selects/confirms
- Accessibility: ชื่อ hero ต้องอ่านได้ ไม่ใช่ icon อย่างเดียว (ไม่ใช้ color เป็นตัวบ่งชี้เดียว)

**When to Use**: Grid การเลือกที่มี items หลายสิบรายการ ต้องการ preview ก่อน confirm
**When NOT to Use**: น้อยกว่า 6 items (ใช้ list view หรือ radio buttons แทน)

---

#### INV_SLOT — Inventory Slot

**Category**: Selection
**Used In**: HUD (`UIInGameShopItem`), Item Shop

**Description**: Grid slot แสดง item ที่ equipped หรือ carried ไว้ แต่ละ slot แสดง item icon พร้อม
overlay สำหรับ cooldown, stack count, และสถานะ active/disabled กดเพื่อดู detail หรือ activate item

**Specification**:
- Slot ว่าง: แสดง empty placeholder background
- Slot มี item: Icon + optional cooldown overlay (ใช้ COOLDOWN_RADIAL) + stack count badge (ถ้ามี)
- กด slot → แสดง item detail หรือ trigger item action
- Cooldown state: icon dimmed + radial countdown (ใช้ COOLDOWN_RADIAL pattern)
- Keyboard: Tab navigates between slots; Enter activates

**When to Use**: ระบบ inventory หรือ loadout ที่มี fixed number of slots
**When NOT to Use**: Unlimited item lists (ใช้ scrollable list แทน)

---

#### GREYED_CARD — Greyed-Out Disabled Card

**Category**: Selection
**Used In**: Hero Select (hero ที่ไม่ได้ unlock), Item Shop (item ที่ซื้อไม่ได้)

**Description**: Card หรือ button ที่แสดงว่า option นี้มีอยู่แต่ไม่สามารถเลือกได้ตอนนี้
ใช้ visual dimming ร่วมกับ optional tooltip อธิบายสาเหตุ ไม่ซ่อน option เพื่อ communicate ความเป็นไปได้

**Specification**:
- Visual: opacity ลดเหลือ ~40–50% + desaturate
- กดไม่ได้ — pointer-events disabled
- Hover/Focus: optional tooltip แสดงสาเหตุ (เช่น "Locked — purchase to unlock")
- Accessibility: ต้องมี tooltip หรือ label อธิบาย — ไม่ใช้ color เป็นตัวบ่งชี้เดียว
- ต้องแสดงให้เห็นว่า option มีอยู่ เพื่อ communicate ความเป็นไปได้ของเกม

**When to Use**: Content ที่ยังไม่ได้ unlock หรือ action ที่ไม่ available ชั่วคราว
**When NOT to Use**: Content ที่ผู้เล่นไม่ควรรู้ว่ามีอยู่ (ซ่อนดีกว่า)

---

#### CAROUSEL — Carousel Swipe

**Category**: Selection
**Used In**: Hero Select — skin selection (`UISwipeTest`)

**Description**: Horizontal sequence ของ items ที่ navigate ด้วย swipe หรือ arrow buttons
แสดง item ปัจจุบันที่ center โดดเด่น พร้อม peek ของ item ก่อนหน้า/ถัดไปที่ขอบ

**Specification**:
- แสดง 1 item ที่ center + partial peek ของ item ที่อยู่ติดกัน
- Navigate: ลาก/swipe ซ้าย-ขวา หรือกด arrow buttons
- Keyboard: Arrow Left/Right navigates; item เปลี่ยนทันที
- Dot indicator หรือ counter (เช่น "3/7") แสดง position ใน sequence
- End of sequence: หยุดที่ item แรก/สุดท้าย — ไม่ wrap around
- Locked item: แสดง GREYED_CARD style + lock icon

**When to Use**: Browse collection ที่ต้องการ visual focus ที่ item ปัจจุบัน
**When NOT to Use**: มากกว่า 20 items (scroll grid ดีกว่า), หรือ items ที่ต้อง compare พร้อมกัน

---

### Input & Action Patterns

#### SEARCH_INPUT — Search Input

**Category**: Input/Action
**Used In**: Item Shop (2 search boxes แยกกันตาม tab)

**Description**: Text input field ที่ filter list แบบ real-time ตามที่ผู้เล่นพิมพ์ ใช้ prefix match
ผลลัพธ์แสดงใน search panel แยกจาก main list

**Specification**:
- Input field พร้อม placeholder text (เช่น "Search items...")
- Filter: real-time prefix match ทุกครั้งที่ character เปลี่ยน
- ผลลัพธ์แสดงใน search panel — ไม่ modify main list
- Empty result: แสดง empty state ("No results found")
- Clear button (×) เมื่อมีข้อความ → clear input → กลับ main list
- Keyboard: Escape clear input; Enter ไม่ submit (real-time เท่านั้น)
- Accessibility: label ชัดเจน; focus indicator visible

**When to Use**: List มากกว่า ~12 items ที่ผู้เล่นต้องการหา item ที่รู้ชื่ออยู่แล้ว
**When NOT to Use**: List สั้น (ใช้ filter buttons แทน)

---

#### LOCK_IN_BTN — Lock In Button

**Category**: Input/Action
**Used In**: Hero Select

**Description**: Primary action button ที่ commit ผู้เล่นกับการเลือกปัจจุบัน — **irreversible** ภายใน phase
กดแล้ว phase เปลี่ยน ไม่สามารถย้อนกลับได้

**Specification**:
- ขนาดใหญ่กว่าปุ่มทั่วไป — เป็น primary call-to-action
- Enabled เฉพาะเมื่อมี selection อยู่ (ใช้ DISABLED_BTN เมื่อไม่มี selection)
- กด → trigger phase transition ทันที (Hero panel ปิด, Skin panel เปิด)
- Keyboard: Enter ยืนยัน; Space ยืนยัน
- ไม่มี confirmation dialog — กดแล้ว commit ทันที (timer pressure context)
- ใกล้ timer หมด: optional urgency visual (เช่น pulse animation)

**When to Use**: Action ที่ irreversible ภายใน phase แต่ user ตั้งใจทำ
**When NOT to Use**: Action destructive ที่ไม่คาดหมาย (ใช้ confirmation dialog แทน)

---

#### DISABLED_BTN — Disabled Button (Greyed Out)

**Category**: Input/Action
**Used In**: Item Shop (Buy/Sell/Undo), HUD (Buyback), Hero Select (Lock In เมื่อไม่มี selection)

**Description**: Button ที่มีอยู่แต่กดไม่ได้เพราะเงื่อนไขไม่ครบ แสดง disabled state ชัดเจน
โดยไม่แสดง error popup — ผู้เล่นเห็น state และรู้ว่าต้องทำอะไรก่อน

**Specification**:
- Visual: opacity ลดลง + desaturate
- กดไม่ได้ — pointer-events disabled
- ไม่แสดง error popup เมื่อพยายามกด (silent fail by design)
- Hover: optional tooltip อธิบายเงื่อนไขที่ต้องครบ
- กลับมา enabled ทันทีเมื่อเงื่อนไขครบ (reactive)
- Accessibility: aria-disabled; tooltip อธิบายสาเหตุ

**When to Use**: เงื่อนไขที่ต้องครบก่อนทำ action (ทองไม่พอ, inventory เต็ม, ไม่มี selection)
**When NOT to Use**: เมื่อ action พร้อมแต่ผู้เล่นยังไม่ได้กด

---

#### UNDO_ACTION — Undo Action

**Category**: Input/Action
**Used In**: Item Shop

**Description**: Button ที่ย้อนรายการล่าสุด 1 รายการ ใช้ได้เฉพาะภายใน context เดียว (อยู่ที่ฐาน)
ออกจาก context → history หาย ไม่ใช่ global undo

**Specification**:
- Button "Undo" แสดง label ชัดเจน
- Enabled: เมื่อมี history ที่ย้อนได้
- DISABLED_BTN: เมื่อ history ว่างหรือออกจาก valid context
- กด → ย้อน action ล่าสุดทันที (buy → refund, sell → return item)
- History: 1 รายการเท่านั้น (ไม่ใช่ multi-level undo)
- History หายเมื่อออกจาก base area

**When to Use**: Short-window reversible actions ภายใน session เดียว
**When NOT to Use**: Actions ที่ต้องการ multi-step undo หรือ undo ข้ามเวลา

---

#### RADIAL_MENU — Radial Context Menu

**Category**: Input/Action
**Used In**: HUD — Ping system (`UIQuickMessageView`)

**Description**: Circular menu ที่ปรากฏที่ตำแหน่ง cursor เมื่อ hold trigger key แต่ละ option วางในทิศทาง
ต่างๆ รอบ center ผู้เล่นเลือกโดย drag/hover แล้วปล่อย key เพื่อ confirm

**Specification**:
- Trigger: Hold Alt + Right-click → menu ปรากฏที่ cursor position
- Options: วางใน 4–8 ทิศทางรอบ center point
- เลือก: drag/hover ไปยัง option → highlight; ปล่อย key → confirm
- Cancel: ปล่อย key ที่ center → ไม่เลือก option ใด
- แสดงชั่วคราวเท่านั้น — หายเมื่อ key ถูกปล่อย
- ⚠️ Accessibility gap: ยังไม่มี keyboard-only path สำหรับ Ping (ดู Gaps section)

**When to Use**: Quick selection ใน real-time context ที่มี options 4–8 ต้องกดเร็ว
**When NOT to Use**: มากกว่า 8 options หรือ context ที่ต้องการ keyboard navigation

---

### Feedback & State Patterns

#### COUNTDOWN — Countdown Timer

**Category**: Feedback/State
**Used In**: Hero Select (`CBSConfigBattle.PickupAvatarTime`)

**Description**: Numeric display นับถอยหลังจากค่า server-driven ลงไปยัง 0
เมื่อถึง 0 trigger action อัตโนมัติ แสดงความเร่งด่วนของ action ที่ผู้เล่นต้องทำ

**Specification**:
- แสดงตัวเลขนับถอยหลัง (format: วินาทีเต็ม หรือ MM:SS)
- Update ทุก frame หรือทุกวินาที
- Visual urgency: เมื่อเวลาเหลือน้อย (~10 วินาที) เปลี่ยนสีและ/หรือ animation
- ถึง 0 → trigger auto-action ทันที (ไม่รอ user input)
- Accessibility: ค่า timer ต้องอ่านได้เป็น text ไม่ใช่ visual only

**When to Use**: มี time limit ที่ trigger system behavior เมื่อหมดเวลา
**When NOT to Use**: Time display ที่ informational เท่านั้น (ใช้ clock display แทน)

---

#### RESOURCE_BAR — Resource Bar (HP/MP/Shield)

**Category**: Feedback/State
**Used In**: HUD (`UILife`), floating HP bars (`ActorUI`)

**Description**: Fill bar แสดง resource ปัจจุบันเป็น % ของ max value พร้อม numeric label
ใช้สีที่แตกต่างกันตาม type และ team affiliation update แบบ real-time จาก NetworkTrait

**Specification**:
- Fill amount: `CurrentValue / MaxValue` (0.0 → 1.0)
- Numeric label: "CurrentValue / MaxValue" หรือตัวเลขเดียว
- สีตาม type: HP (self=เขียว, ally=ฟ้า, enemy=แดง), MP (ฟ้า), Shield (ขาว/เทา)
- Multiple layers: HP + Resource + Shield bars ซ้อนกัน
- Real-time update ตาม NetworkTrait
- Accessibility: ค่าต้องอ่านได้เป็น text ด้วย (ไม่ใช้ bar อย่างเดียว); ไม่ใช้ color เป็นตัวบ่งชี้เดียว

**When to Use**: Resource ที่ลดได้/เพิ่มได้ตลอดเวลา (HP, Mana, Charge)
**When NOT to Use**: Progress ที่ไม่ลดลง (ใช้ progress bar แทน)

---

#### COOLDOWN_RADIAL — Skill Cooldown Radial

**Category**: Feedback/State
**Used In**: HUD (`UISkill`), inventory slot cooldowns (`UIInGameShopItem`)

**Description**: Circular fill overlay บน icon แสดง remaining cooldown sweeping จาก full → 0
icon dimmed ระหว่าง cooldown, brightens เมื่อพร้อมใช้

**Specification**:
- Fill: `RemainingCooldown / MaxCooldown` (1.0 → 0.0 clockwise sweep from top)
- Icon: dimmed เมื่อ cooldown > 0; full brightness เมื่อ ready
- Numeric overlay: optional "Xs" countdown text ที่ center
- Ready: optional pulse animation ครั้งเดียวเมื่อ cooldown หมด
- Accessibility: ค่า cooldown ต้องอ่านได้เป็น text บน hover (ใช้ HOVER_TOOLTIP)

**When to Use**: Cooldown หรือ charge time บน icon
**When NOT to Use**: Long duration timers (ใช้ COUNTDOWN แทน)

---

#### FLOAT_DMG — Floating Damage Numbers

**Category**: Feedback/State
**Used In**: HUD (`FloatingDamageManager`, `UINumber`)

**Description**: Temporary animated numbers ที่ปรากฏที่ตำแหน่ง world-space ของ target
แต่ละ number มีสีตาม type และ animate ลอยขึ้นแล้วหายไป

**Specification**:
- Lifespan: 0.7 วินาที (object pool)
- Animation: ลอยขึ้น + fade out
- สีตาม type: Physical=ส้ม `#EE8C4C`, Magical=ม่วง `#C860FF`, True=เหลือง `#FFE770`, Critical=แดง `#FF3B3B` + ใหญ่กว่า, Heal=เขียว `#88FF94`, Gold=ทอง `#F9CB66`
- Direction: ขวา/ซ้าย/ตรง ตาม screen position ของ target
- Accessibility: pure visual feedback — HP bar แสดงค่า numeric เป็น primary

**When to Use**: Combat feedback ที่ instant และ satisfying
**When NOT to Use**: Information ที่ผู้เล่นต้องบันทึก (ใช้ text log แทน)

---

#### STATUS_ICON — Status Effect Icon

**Category**: Feedback/State
**Used In**: HUD (`StatusEffectUI`)

**Description**: Icon แสดง buff/debuff ที่ active อยู่บน hero พร้อม duration fill, stack count, และ tooltip
แสดงเป็น row ที่เพิ่ม/หายตาม active effects

**Specification**:
- แต่ละ icon: Effect image + COOLDOWN_RADIAL fill แสดง duration ที่เหลือ
- Stack count badge: ตัวเลข ถ้ามี stacks มากกว่า 1
- Duration text: "X.XX S" แสดงใน/ใกล้ icon
- Hover/Focus → HOVER_TOOLTIP: ชื่อ + คำอธิบาย + เวลาที่เหลือ
- หายทันทีเมื่อ duration หมด
- Accessibility: tooltip อ่านได้ด้วย keyboard focus

**When to Use**: Temporary status effects ที่มี duration และ/หรือ stacks
**When NOT to Use**: Permanent stats (ใช้ STAT_PANEL แทน)

---

#### HOVER_TOOLTIP — Hover Tooltip

**Category**: Feedback/State
**Used In**: HUD (skill icons, status effects), Item Shop (item stats)

**Description**: Panel ที่ปรากฏเมื่อ hover หรือ focus บน element แสดงข้อมูลเพิ่มเติม
หายทันทีเมื่อ cursor/focus ออกจาก element

**Specification**:
- Trigger: Mouse hover หรือ keyboard focus
- Delay: ~300ms ก่อนแสดง (ป้องกัน flash)
- Position: ใกล้ element แต่ไม่บัง content สำคัญ
- Content: ขึ้นอยู่กับ element (ชื่อ, description, stats, cooldown, สาเหตุ disabled)
- หายทันทีเมื่อ cursor/focus ออก
- Accessibility: content ต้องอ่านได้ด้วย screen reader

**When to Use**: ข้อมูลเสริมที่มีประโยชน์แต่ไม่ต้องแสดงตลอดเวลา
**When NOT to Use**: ข้อมูล MUST-SEE ที่ผู้เล่นต้องเห็นเสมอ (แสดง inline แทน)

---

#### KILL_FEED — Kill Feed

**Category**: Feedback/State
**Used In**: HUD (`UIKillFeedObject`)

**Description**: Queued log ของ events (kills, objectives) แสดงเป็น row เล็กๆ ที่ขอบหน้าจอ
แต่ละ entry fade in → แสดงระยะสั้น → fade out โดย entries ใหม่ push entries เก่าขึ้น

**Specification**:
- Position: corner ของหน้าจอ (ไม่บัง gameplay area สำคัญ)
- แต่ละ entry: Killer portrait + event icon + Victim portrait
- Queue-based: entries แสดงต่อเนื่อง ไม่ overlap
- Duration ต่อ entry: ~4–5 วินาที (fade in + display + fade out)
- Max visible: 5–6 entries พร้อมกัน
- Accessibility: informational เท่านั้น — ไม่ต้องการ action

**When to Use**: Event log ที่ transient และ contextual ระหว่างเกม
**When NOT to Use**: Events ที่ต้องการ attention ทันที (ใช้ ANNOUNCE_BANNER แทน)

---

#### ANNOUNCE_BANNER — Timed Announcement Banner

**Category**: Feedback/State
**Used In**: HUD (`UIAnnouncement`, `UIAnnouncementObject`)

**Description**: Banner กลางหน้าจอประกาศ significant events (First Blood, Tower Destroyed, Boss Slain)
Fade in → แสดงสั้น → Fade out ทีละ 1 ข้อความตาม queue

**Specification**:
- Position: กลางหน้าจอด้านบน — prominent
- Kill Announcement: Killer portrait + Victim portrait + text; Fade In 1s → Display 2s → Fade Out 1s
- Text Announcement: event text; Duration 1.1 วินาที
- Queue: แสดงทีละ 1 ข้อความถัดไปรอก่อน
- Audio: ควรมีเสียง announcement ประกอบ
- Accessibility: text contrast สูง; ควรมี audio alternative

**When to Use**: Significant game events ที่ทุกผู้เล่นควรรับรู้ทันที
**When NOT to Use**: Events ทั่วไปที่เกิดบ่อย (ใช้ KILL_FEED แทน)

---

### Data Display Patterns

#### STAT_PANEL — Stat Display Panel

**Category**: Data Display
**Used In**: HUD (`UIStat`), Hero Select (hero complexity ratings)

**Description**: Grid ของ stat abbreviations + values รองรับสีพิเศษสำหรับ stats ที่ถูกแก้ไข
โดย items/buffs

**Specification**:
- แต่ละ row: stat abbreviation + value (อาจมี icon ประกอบ)
- Modified stat: สีต่างจาก base value (buffed=เหลือง, debuffed=แดง)
- Read-only ไม่มี interaction โดยตรง
- Hover ดู full stat name via HOVER_TOOLTIP
- Update เมื่อ item/buff เปลี่ยน (ไม่ใช่ continuous real-time)

**When to Use**: Character stats, hero attributes, item comparisons
**When NOT to Use**: Real-time dynamic values (ใช้ RESOURCE_BAR แทน)

---

#### TEAM_LINEUP — Team Lineup Panel

**Category**: Data Display
**Used In**: Hero Select (`m_Team1Content`, `m_Team2Content`)

**Description**: Side panels แสดง hero icons ของทุกผู้เล่นในแต่ละทีม เรียงตาม player slot
update แบบ real-time เมื่อผู้เล่นแต่ละคน Lock In

**Specification**:
- 2 panels: Team 1 (ซ้าย) และ Team 2 (ขวา)
- แต่ละ slot: Hero portrait เมื่อ locked in; placeholder เมื่อยังไม่เลือก
- Format label: `{N}VS{N}` (เช่น "5VS5")
- Real-time update ผ่าน network เมื่อผู้เล่น lock in
- Read-only ไม่มี interaction

**When to Use**: Multiplayer pre-game screens ที่ต้องแสดงสถานะทีม real-time
**When NOT to Use**: Single-player contexts

---

#### SCOREBOARD — Scoreboard Overlay

**Category**: Data Display
**Used In**: HUD (`UITeamsStatisticView`) — Tab key

**Description**: Overlay แสดงสถิติของทุกผู้เล่นในแมตช์ Toggle ด้วย Tab key แสดงชั่วคราว

**Specification**:
- Trigger: Tab hold → แสดง; ปล่อย Tab → หาย (หรือ Toggle)
- ต่อผู้เล่น: Portrait + ชื่อ + K/D/A + Gold + Level + Items 6 ช่อง + CS + Respawn status
- ต่อทีม: Total Kills + Towers + Boss kills + Game Timer
- สี: Team 1 = ฟ้า, Team 2 = แดง
- Read-only ระหว่างแมตช์

**When to Use**: Match statistics ที่ access on-demand
**When NOT to Use**: ข้อมูลที่ต้องเห็นตลอดเวลา (ใช้ HUD elements แทน)

---

#### MINIMAP — Minimap

**Category**: Data Display
**Used In**: HUD (`UIMinimapView`) — มุมขวาล่าง

**Description**: Small map แสดง overview ของสนามพร้อม positions ของ heroes, towers, minions,
และ FOW layer รองรับ interaction เพื่อ navigate กล้องหรือ command hero

**Specification**:
- Position: มุมล่างขวา
- แสดง: Heroes (same team + visible enemies) + Towers (2 team colors) + Minions + Pings + Jungle icons + FOW
- Left Click + Drag → ย้ายกล้อง
- Right Click → สั่ง Hero วิ่งไปจุดนั้น
- Real-time update ตาม game state
- Accessibility: supplementary — core gameplay ใช้ main view ได้โดยไม่ต้องพึ่ง minimap

**When to Use**: Real-time game map overview + quick navigation
**When NOT to Use**: Context ที่ไม่มี spatial game world

---

#### ITEM_DETAIL — Item Detail Panel

**Category**: Data Display
**Used In**: Item Shop

**Description**: Side panel แสดงข้อมูลครบของ item ที่ selected ปรากฏเมื่อ select item จาก grid
หรือ search results มี Buy/Sell buttons ในตัว

**Specification**:
- แสดง: Icon + Name + Price + Stats (with icons+สี) + Description + Recipe + Common builds
- ปรากฏที่ fixed position (ไม่ใช่ popup)
- Buy/Sell buttons ใน panel นี้ (ใช้ DISABLED_BTN เมื่อ conditions ไม่ครบ)
- Empty state เมื่อยังไม่ได้ select (placeholder text)
- Read-mostly — interaction ผ่าน Buy/Sell buttons เท่านั้น

**When to Use**: Item/content ที่มีข้อมูลหลายมิติต้องการ dedicated reading space
**When NOT to Use**: Items ที่มีข้อมูลน้อย (ใช้ HOVER_TOOLTIP แทน)

---

### Modal & Overlay Patterns

#### LOADING_DIALOG — Loading Dialog

**Category**: Modal/Overlay
**Used In**: Menu/Lobby (`UILoadingDialogView`)

**Description**: Blocking overlay แสดงระหว่าง async operations (scene transitions, session init)
ป้องกัน user interaction ระหว่างรอ หายเองเมื่อ operation เสร็จ

**Specification**:
- Blocks UI ด้านล่างทั้งหมด
- แสดง: loading indicator + optional status text
- ไม่มีปุ่มปิด — system controls เปิด/ปิดเอง
- Accessibility: แจ้ง screen reader ว่า app กำลัง loading (aria-busy)

**When to Use**: Async operations ที่ user ต้องรอและทำอย่างอื่นไม่ได้
**When NOT to Use**: Background operations ที่ไม่ block interaction

---

#### ERROR_DIALOG — Error Dialog

**Category**: Modal/Overlay
**Used In**: Menu/Lobby (`UIErrorDialogView`), Item Shop (implicit)

**Description**: Modal dialog แสดง error message ที่ผู้เล่นต้อง acknowledge ก่อนดำเนินการต่อ
มีปุ่ม dismiss ชัดเจน

**Specification**:
- Modal: blocks background
- แสดง: error icon + error message (human-readable, ไม่ใช่ error code)
- ปุ่ม Dismiss/OK — กด → dialog หาย
- บางกรณีหายอัตโนมัติหลัง timeout
- Accessibility: focus trap ภายใน dialog; focus กลับไป trigger element เมื่อปิด

**When to Use**: Errors ที่ user ต้องรับรู้ก่อนดำเนินการต่อ
**When NOT to Use**: Minor non-blocking errors (ใช้ inline validation แทน)

---

#### INVITE_DIALOG — Invite Dialog

**Category**: Modal/Overlay
**Used In**: Menu/Lobby (`UIInvitePanel`)

**Description**: Modal dialog เมื่อได้รับ game invite จากเพื่อน แสดงข้อมูล inviter
และตัวเลือก Accept/Decline

**Specification**:
- Triggered by: SignalR event `OnReceivedInviteGame`
- แสดง: Inviter avatar + name + game mode/room info + Accept + Decline buttons
- Accept → `OnTryToJoinSession(CustomData)` → ตรวจ room availability
- Decline → dialog ปิด; ไม่มีผลต่อ game state
- Room เต็ม → ERROR_DIALOG
- Accessibility: focus trap; keyboard navigable Accept/Decline

**When to Use**: Async invitations จาก social system ที่ต้องการ user decision
**When NOT to Use**: System notifications ที่ไม่ต้องการ decision

---

#### FRIEND_PANEL — Friend Panel

**Category**: Modal/Overlay
**Used In**: Menu/Lobby (`UIFriendPanel`)

**Description**: Side panel/drawer แสดง friends list พร้อม online status และ actions
เปิด/ปิดได้โดยไม่ block main navigation (non-modal)

**Specification**:
- Toggle: ปุ่มเปิด/ปิด panel
- แสดง: friend list เรียงตาม online status + name + avatar
- Actions: Invite to game, Send message
- ไม่ block main UI เมื่อเปิด (non-modal)
- Real-time online status update ผ่าน SignalR
- Accessibility: focus management เมื่อ open/close; keyboard navigable

**When to Use**: Social features ที่ต้องการ access ตลอดเวลาจาก main screens
**When NOT to Use**: In-game screens (ใช้ chat เท่านั้นระหว่างแมตช์)

---

## Gaps & Patterns Needed

Patterns ที่พบจาก GDD analysis แต่ยังไม่ได้ formalize ไว้ในไลบรารีนี้
เพิ่มเมื่อ feature นั้น enter implementation pipeline

| Pattern | พบที่ | Priority | หมายเหตุ |
|---------|-------|----------|---------|
| **FILTER_PILLS** — Role/Lane filter buttons | Item Shop, Hero Select | HIGH | ใช้ใน 2 screens — ควร formalize ก่อน implement Hero Select |
| **HERO_DETAIL_PANEL** — Preview panel เมื่อ select hero card | Hero Select | HIGH | Panel กลาง แสดง hero stats/skills ก่อน Lock In |
| **CHAT_INPUT** — In-game chat panel with auto-close | HUD (`UIChatView`) | MEDIUM | Auto-close 5–7s, blocks movement input เมื่อเปิด |
| **DEATH_RECAP** — Death summary overlay | HUD (`UIDetailOfDeathView`) | MEDIUM | แสดงทันทีเมื่อ hero ตาย — Killer + damage breakdown |
| **NOTIFICATION_TOAST** — Transient system notifications | Notification System GDD | LOW | ยังไม่ได้ design |

### Accessibility Gaps

| Gap | Pattern | Severity | แนวทางแก้ |
|-----|---------|---------|-----------|
| ไม่มี keyboard path สำหรับ Ping | RADIAL_MENU | Basic tier gap | เพิ่ม Quick Ping hotkeys (เช่น Alt+1..5) เป็น alternative |

---

## Open Questions

1. **FILTER_PILLS behavior**: เมื่อ select role filter ใน Hero Select — filter แบบ exclusive (1 role เท่านั้น) หรือ multi-select ได้?
2. **HERO_DETAIL_PANEL content**: แสดง stats + skills ครบหรือเฉพาะ summary? ต้องตัดสินใจก่อน implement Hero Select UX spec
3. **CAROUSEL wrap-around**: ใน Skin Selection ปัจจุบัน wrap around ที่ end หรือ stop? (ตาม spec ปัจจุบัน: stop — แต่ยืนยันจาก `UISwipeTest` implementation)
4. **Keyboard equivalent สำหรับ Ping**: จะใช้ Alt+1..5 หรือ design อื่น? ต้องตัดสินใจก่อนปิด accessibility gap
5. **Item Shop error feedback**: ปัจจุบัน buy disabled ไม่มี error popup — ผู้เล่นใหม่อาจไม่รู้สาเหตุ (⚠️ known issue จาก GDD) ควรเพิ่ม tooltip อธิบายหรือ error state?
