# Faction System

> **Status**: In Design
> **Author**: User + Claude Code agents
> **Last Updated**: 2026-04-27
> **System ID**: FT11
> **Implements Pillar**: Universe-Spanning Meta-Game (P4), Anti-Toxicity Core (P5 — supporting)
> **Source Decision**: [Tournament Pivot 2026-04-23](../decisions/meeting-2026-04-23-tournament-pivot.md)

## Overview

Faction System คือชั้นสังกัดถาวรของผู้เล่นแต่ละคน — ผู้เล่นเลือก **1 ใน 5–6 ฝ่าย**
[Open Question: 5 vs 6] ตั้งแต่สร้างบัญชี และสามารถเปลี่ยนฝ่ายได้ภายใต้เงื่อนไข
[Open Question: switch rules]. ฝ่ายไม่ครอบครองพื้นที่ใดบนแผนที่ (10 Neutral Cities
ยังเปิดให้ทุกฝ่ายเข้า) แต่ทำหน้าที่เป็น **container ของ shared progression** —
Fragment ที่สมาชิกหามาได้จะรวมเข้า pool ของฝ่าย และเมื่อฝ่ายใดสะสมครบ
~100M Fragment สมาชิกของฝ่ายนั้นจะร่วมกันกำหนด "กฎของ Universe" ในรอบถัดไป
(~3 ปี/cycle). ในทางผู้เล่น Faction คือ **สังกัดที่ทุกชัยชนะใน Tournament
มีน้ำหนักเกินตัวเอง** — ทีมใน Ranked อาจคละฝ่าย แต่ Fragment ที่ได้จะไหลกลับ
ไปฝ่ายของแต่ละคน ทำให้ทุกแมตช์เป็นทั้งการแข่งระดับบุคคลและการสนับสนุนฝ่าย
ไปพร้อมกัน. ในทางระบบ Faction คือ persistent player attribute ที่ระบบ
downstream (Tournament award routing, Fragment pool aggregation, Reputation)
อ่านเพื่อกำหนดพฤติกรรม.

## Player Fantasy

ผู้เล่นรู้สึกเหมือน **ผู้ถือธงในประวัติศาสตร์ที่ยังเขียนไม่เสร็จ** —
การเลือกฝ่ายไม่ใช่การติด tag เพื่อ filter เพื่อน แต่คือ **คำสาบาน** ว่าทุก
ชัยชนะของเราจะเป็นก้อนหินก่อนหนึ่งของวิหารที่ฝ่ายของเรากำลังสร้าง.

**Anchor moment:** วินาทีหลังจบ Tournament Ranked — หน้า post-match แสดง
Fragment ที่หามาได้ "ไหลเข้า" pool ของฝ่ายบนหน้า standings ตัวเลขรวมของฝ่าย
ขยับขึ้นทีละนิด เพราะการกระทำของเรารวมกับเพื่อนร่วมธงอีกหลายแสนคน. ความ
รู้สึกที่ต้องการคือ — *ฉันไม่ได้เล่นคนเดียว ทุก kill ทุก objective ทุก
Fragment กลายเป็นอิฐในกำแพงที่ใหญ่กว่าตัวเอง*

**Tone และภาษา:** Mythic แต่ติดดิน (mythic-but-grounded), oath-bound.
ใช้คำว่า *สมทบ, ส่งต่อ, ปักธง, สาบาน, วิหาร, ยุค* — ไม่ใช่ *พิชิต, ครอง,
ทำลาย*. ระบบพูดกับผู้เล่นในน้ำเสียงสง่างามแต่ไม่โอ่อ่า เช่น "Fragment
ของคุณถูกผนวกเข้ากับ [Faction] แล้ว — อีก 47.2M ก่อนวาระเริ่มต้น"

**Reference feeling:** ทหารที่เข้าใจว่าตนเป็นส่วนหนึ่งของกองทัพที่สู้ศึกข้าม
ชั่วอายุคน — ไม่ใช่ฮีโร่เดี่ยว ไม่ใช่ tribal warrior ที่เกลียดฝ่ายอื่น แต่คือ
*steward* ของมรดกที่จะส่งต่อให้ผู้เล่นรุ่นถัดไปใน cycle ใหม่.

**Pillar mapping:**
- **P4 Universe-Spanning Meta-Game (หลัก)** — Fantasy ขยาย scope ของ
  ชัยชนะจาก "ฉันชนะ" → "พวกเราเข้าใกล้การเขียนกฎ Universe มากขึ้น 1 ก้าว"
- **P3 Team Synergy (รอง)** — ขยายความหมายของ "ทีม" จาก 5 คนใน lobby
  เป็นล้านคนทั่ว Universe ที่ถือธงเดียวกัน
- **P5 Anti-Toxicity (สนับสนุน)** — เพราะ frame Faction เป็น *home/oath*
  ไม่ใช่ *enemy frame* ทำให้ cross-faction matchmaking ไม่ขัด fantasy
  (ทีมคนละฝ่ายไม่ใช่ศัตรู — เป็น "ทหารที่ถูกยืมตัว")

**สิ่งที่ Faction Fantasy *ไม่ใช่*:**
- ไม่ใช่ tribal/partisan rivalry ที่ผลักไปสู่ flame ระหว่างฝ่าย
- ไม่ใช่ celebrity/personal-legend fantasy (manga loop เป็น bonus layer
  ไม่ใช่ primary draw)
- ไม่ใช่ guild/clan แบบ MMO (ฝ่ายขนาดล้านคน ไม่ใช่กลุ่มเพื่อน 50 คน)

**Onboarding risk mitigation:** เพราะ F1 framing เสี่ยงรู้สึก abstract ตอน
เริ่มเกม ระบบต้องมี first-match callout ที่ทำให้ผู้เล่นเห็น *ทันที* ว่า
Fragment แรกของตนสมทบเข้า pool — และมี faction profile page ที่อธิบายว่า
~100M หมายถึงอะไร พร้อม trajectory ปัจจุบัน.

## Detailed Design

### Core Rules

**R1 — Faction Selection (Onboarding)**
ผู้เล่นเลือก faction ครั้งเดียวระหว่าง Account Onboarding Flow ก่อนแมตช์แรก
ระบบบังคับ ข้ามไม่ได้. ไม่มี "no faction" state หลัง onboarding เสร็จ.

**R2 — Immutable-While-Locked Semantics**
`faction_id` เป็น server-authoritative field บน Player Record (M6) ไม่สามารถ
แก้ไขจาก client ได้. การเปลี่ยนต้องผ่าน Azure Function ที่ validate
`SWITCH_COOLDOWN_DAYS` และ state เท่านั้น.

**R3 — Cross-Faction Matchmaking Allowed**
Ranked Tournament teams สามารถประกอบด้วยผู้เล่นต่างฝ่ายได้. Faction
**ไม่ส่งผล** ต่อ matchmaking pool. *(Rationale: รักษา P5 Anti-Toxicity —
block cross-faction จะสร้าง hostile rivalry ขัด "oath-bound, not tribal" fantasy)*

**R4 — Fragment Routing per-Player**
เมื่อแมตช์จบ Fragment award แต่ละ unit จะ route ไปยัง `player.faction_id`
ของผู้เล่นคนนั้นเท่านั้น. **ไม่มี** กรณี "pool ผสม" ของทีม.
*(Deterministic — FT13 ไม่ต้องรู้ teammate faction)*

**R5 — Inactive Decay (Parametric)**
ผู้เล่นที่ไม่มีกิจกรรมเกิน `INACTIVE_DECAY_DAYS` จะถูก mark `Inactive`.
Fragment ที่ยัง uncommitted ของตนจะ decay อัตรา `DECAY_RATE_PCT` ต่อรอบ
(รอบ = `DECAY_INTERVAL_DAYS`) ไหลกลับ Central Pool — ไม่ถูกทำลาย.

**R6 — Switch Gating Contract**
การเปลี่ยน faction ต้องผ่าน cooldown `SWITCH_COOLDOWN_DAYS` เป็นขั้นต่ำ.
กลไกเพิ่มเติม (cost, end-of-cycle blackout) → Open Questions.

**R7 — Switching State Routing**
ระหว่างผู้เล่นอยู่ใน `Switching` state Fragment จากแมตช์ใหม่ route ไปยัง
**faction เดิม** จนกว่า switch จะ complete. *(Option A — deterministic, ไม่มี escrow)*

**R8 — Banned State Freeze**
ผู้เล่นที่ถูก ban (M11) Fragment ทั้ง routing **และ** decay จะหยุด (frozen).
*(ban ไม่ perma ปกติ — frozen ป้องกัน loss ของผู้เล่นที่กลับมา)*

**R9 — Server Authority**
`faction_id`, `faction_join_date`, `last_active_date`, `fragment_uncommitted`,
`state` ทุก field server-side เท่านั้น. Client อ่านได้ (read-only snapshot)
เขียนไม่ได้.

**R10 — Faction Count Parametric**
ระบบรองรับ `FACTION_COUNT` ∈ {5, 6} โดยไม่แก้โค้ด. Faction definitions
โหลดจาก CBS config (F4).

**R11 — No Territory Ownership**
Faction ไม่มี field territory ใดๆ. 10 Neutral Cities (FT12) อ่าน
`player.faction_id` เพื่อ cosmetics เท่านั้น.

### States and Transitions

| State | คำอธิบาย | Trigger เข้า | Trigger ออก |
|---|---|---|---|
| `Unaffiliated` | บัญชีใหม่ ยังไม่เลือก | Account created | Faction selected → `Active` |
| `Active` | สมาชิกปกติ มีกิจกรรม | Faction selected / match completed / switch completed / ban lifted | `last_active > INACTIVE_DECAY_DAYS` → `Inactive` ; switch request → `Switching` ; ban → `Banned` |
| `Inactive` | นิ่งนาน Fragment กำลัง decay | `last_active > INACTIVE_DECAY_DAYS` | match completed → `Active` ; ban → `Banned` |
| `Switching` | รอ cooldown หลัง switch request — Fragment ยัง route faction เดิม (R7); **ห้าม** transition ไป `Inactive` ระหว่างนี้ | Switch request submitted + validated | Cooldown หมด → `Active` (faction ใหม่) ; ban → `Banned` |
| `Banned` | M11 ban — frozen ทุกด้าน (R8) | M11 ban event | Ban lifted → `Active` (restore previous faction) |

**Invariants:**
- `Banned` ทับทุก state อื่น (ban event ลบ pending switch / inactive flag)
- `Switching` ทับ `Inactive` (R7 — cooldown ยังเดิน, decay ไม่ trigger)
- ผู้เล่นจะอยู่ **เพียง 1 state** ณ เวลาใดก็ตาม

### Interactions with Other Systems

#### FT12 World Map (10 Neutral Cities)

| Direction | Data | Owner | Authority |
|---|---|---|---|
| IN | `player.faction_id` (read on session start) | FT12 reads | Server snapshot, client renders cosmetics |
| OUT | — | — | — |

#### FT13 Tournament System

| Direction | Data | Owner | Authority |
|---|---|---|---|
| IN (snapshot) | `player.faction_id` ณ match-start (frozen ตลอดแมตช์ — ป้องกัน mid-match switch race) | FT13 caches at lobby | Server-issued |
| OUT | `(player_id, fragment_amount, match_id)` post-match → Faction routing endpoint | Faction System owns endpoint | Azure Function (server) |

#### FT14 Fragment & Meta-Game

| Direction | Data | Owner | Authority |
|---|---|---|---|
| IN | Decay parameters from CBS config | FT14 reads via F4 | Server config |
| OUT | Per-faction pool totals; per-player `uncommitted_fragments`; inactive member list (decay batch) | Faction owns player state + eligibility flags ; FT14 owns aggregation | Decay batch = scheduled Azure Function |

> **Boundary note:** Faction System ไม่ aggregate pool เอง — เพียง maintain
> `state` + `eligibility` flag. FT14 เป็น consumer ที่ aggregate pool จริง.

#### M11 Reputation / Anti-Toxicity

| Direction | Data | Owner | Authority |
|---|---|---|---|
| IN | `player.faction_id` (เพื่อ apply faction-level reputation modifier — TBD scope) | M11 reads | Server snapshot |
| OUT | Ban event → Faction transitions player to `Banned` ; ban-lifted → restore previous state | Faction owns state transition; M11 calls Faction State endpoint | Azure Function |

## Formulas

> Default values สำหรับ tuning constants ทั้งหมดอยู่ใน Section G.

### D1 — `InactiveDecay(c)` (per-player per-cycle)

Linear decay บน *remaining balance* (geometric series → 0 จนเจอ floor)

```
DecayAmount(c)      = uncommitted_f(c) × DECAY_RATE_PCT
uncommitted_f(c+1)  = uncommitted_f(c) − DecayAmount(c)

Floor rule:
  ถ้า uncommitted_f(c) < MIN_FRAGMENT_FLOOR
    → DecayAmount = uncommitted_f(c)  (return ทั้งหมด, set 0, หยุด decay)
```

| Variable | Symbol | Type | Range | Description |
|---|---|---|---|---|
| `uncommitted_f(c)` | `u_c` | float | 0 – ∞ | Fragment uncommitted ณ ต้น cycle c |
| `DECAY_RATE_PCT` | `r` | float | 0.0 – 1.0 | สัดส่วน decay ต่อ cycle |
| `MIN_FRAGMENT_FLOOR` | `f_min` | float | 0 – 100 | Floor หยุด micro-decay (default 1.0) |
| `DecayAmount(c)` | `d_c` | float | 0 – `u_c` | Fragment ที่ return Central Pool ใน cycle นี้ |

**Output range:** `[0, u_c]` ต่อ cycle ; ผลรวมทุก cycle ≤ original ; converge → 0 เมื่อต่ำกว่า floor.

**Worked example** (`u_0 = 10000`, `r = 0.05`, `f_min = 1.0`):
- Cycle 1: decay 500 → remaining 9,500
- Cycle 2: decay 475 → remaining 9,025
- … ดำเนินต่อจน `u_c < 1.0` → return ทั้งหมด, set 0

Decay หยุดเมื่อผู้เล่น transition กลับ `Active` (match completed).

### D2 — `IsActive(player, t)` (binary activity gate)

Binary gate ใน v1. `last_active_date` raw เก็บไว้สำหรับ FT14 dashboard derive distribution ภายหลัง

```
IsActive(player, t) = (t − player.last_active_date) ≤ INACTIVE_DECAY_DAYS
```

| Variable | Symbol | Type | Range | Description |
|---|---|---|---|---|
| `t` | `t` | datetime | server-valid | เวลา server ปัจจุบัน |
| `last_active_date` | `t_la` | datetime | server-valid | วันที่แมตช์ล่าสุด (FT13 update) |
| `INACTIVE_DECAY_DAYS` | `T_in` | int | 1 – 365 | threshold วัน |
| `IsActive` | — | bool | {true, false} | true = `Active` ; false = ผ่าน criteria → `Inactive` |

**Output:** boolean ; transitions ตาม Section C state table.

### D3 — `SwitchEligibility(player, t)` (boolean gate)

```
SwitchEligible(player, t) =
    (player.state == Active)
    AND (t − player.last_switch_date) ≥ SWITCH_COOLDOWN_DAYS
    AND NOT InBlackoutWindow(t)

InBlackoutWindow(t) = CycleDaysRemaining(t) ≤ BLACKOUT_DAYS_BEFORE_END
```

| Variable | Symbol | Type | Range | Description |
|---|---|---|---|---|
| `player.state` | — | enum | {Unaffiliated, Active, Inactive, Switching, Banned} | ต้องเป็น `Active` |
| `last_switch_date` | `t_ls` | datetime / NULL | server-valid or NULL | NULL = ยังไม่เคย switch |
| `SWITCH_COOLDOWN_DAYS` | `T_cd` | int | 30 – 365 | cooldown ขั้นต่ำ |
| `CycleDaysRemaining(t)` | — | int | 0 – ~1095 | derived จาก cycle_end_date |
| `BLACKOUT_DAYS_BEFORE_END` | `T_bo` | int | 0 – 90 | window ก่อน cycle จบ (default **0** — feature ปิดเริ่มต้น) |
| `SwitchEligible` | — | bool | {true, false} | true = อนุญาต submit |

**NULL handling:** ถ้า `last_switch_date == NULL` ให้ `(t − t_ls) = ∞` → cooldown = true.

**Worked example** (default `T_cd = 90`, `T_bo = 0`):
- ผู้เล่น `last_switch_date = 2026-01-01`, `t = 2026-04-15` → 104 ≥ 90 ✅
- Cycle จบ 2027-12-31, `T_bo = 0` → InBlackoutWindow = false (always)
- รวม: `SwitchEligible = true`

### D4 — `FactionPoolDecayRate(f, t)` (derived diagnostic)

Metric สำหรับ FT14 dashboard / ops — *ไม่ใช่* tuning knob

**Exact:**
```
FactionPoolDecayRate_exact(f, t) =
    Σ DecayAmount(p) for all p in faction f where IsActive(p,t) == false
    ─────────────────────────────────────────────────────────────────────
    TotalUncommittedFragments(f, t)
```

**Approximation** (uniform `u` across members):
```
FactionPoolDecayRate(f, t) ≈ DECAY_RATE_PCT × InactiveFraction(f, t)

InactiveFraction(f, t) = inactive_member_count / total_member_count
```

| Variable | Symbol | Type | Range | Description |
|---|---|---|---|---|
| `r` | — | float | 0.0 – 1.0 | จาก D1 |
| `InactiveFraction(f,t)` | `φ` | float | 0.0 – 1.0 | สัดส่วน inactive |
| `FactionPoolDecayRate` | — | float | 0.0 – `r` | สัดส่วน uncommitted ทั้ง faction ที่ decay ใน cycle |

**Worked example:** faction 1M สมาชิก, 20% inactive, `r = 0.05` → ≈ 1% per cycle.

**Use case:** dashboard alert — rate > 2% ต่อเนื่อง 3 cycles ⇒ faction health warning.

> **Banned state:** ไม่มี formula พิเศษ — R8 (Section C) ระบุ frozen ชัดเจนแล้ว.
> Implementation gate `IsDecayEligible(p) = (p.state ∈ {Active, Inactive, Switching}) AND IsActive == false` อยู่ใน code/state-machine layer, ไม่ใช่ Section D.

## Edge Cases

> รูปแบบ: **If [condition]**: [resolution]. *(rationale ถ้าไม่ obvious)*

### Boundary / Zero conditions

- **EC-01** **If `uncommitted_f` ลดลงเป็น 0 จาก decay**: clamp ที่ 0 (ห้ามติดลบ),
  หยุด decay จนกว่าจะ transition กลับ `Active` แล้วมี Fragment ใหม่ route เข้า.
- **EC-17** **If `uncommitted_f` ลดต่ำกว่า `MIN_FRAGMENT_FLOOR` กลาง cycle**:
  floor rule (D1) trigger — return ทั้งหมด, set 0, หยุด decay. Player ที่กลับมา
  Active ภายหลังจะเริ่มสะสมจาก 0 ปกติ.
- **EC-18** **If player return `Active` หลัง floor hit แต่ก่อน batch ถัดไป**:
  batch ถัดไปต้องไม่ decay ซ้ำ — state เปลี่ยนเป็น `Active` แล้ว, decay ตัด.

### Concurrency / Race conditions

- **EC-02** **If decay batch run ขณะ match-end Fragment routing ยัง pending**:
  batch ใช้ **snapshot-at-batch-start** ของ `uncommitted_f`. Fragment ที่ route
  หลัง snapshot ไม่ถูก decay ในรอบนี้. ต้องใช้ ETag-based optimistic
  concurrency บน PlayFab entity record.
- **EC-04** **If match-end Fragment routing function เรียกพร้อม decay batch
  บน `uncommitted_f` field เดียวกัน**: ใช้ versioned/atomic write —
  write ที่ ETag ไม่ตรงต้อง retry (read-modify-write loop, max retries
  TBD ใน implementation spec).
- **EC-12** **If Azure Function ใดๆ เขียน `uncommitted_f` พร้อมกัน**:
  ครอบโดย EC-02/EC-04 — ETag concurrency คือ contract.
- **EC-13** **If switch completion เรียกพร้อม match-end routing ของแมตช์ที่
  เริ่มก่อน switch**: match-end ใช้ `faction_id` ที่ FT13 cache ตอน
  match-start (ดู Section C — FT13 interaction). Switch completion
  **ไม่ retro-route** Fragment ของแมตช์ที่ in-progress.

### State transition edge cases

- **EC-03** **If ban event มาถึงระหว่าง player อยู่ `Switching`**: state เปลี่ยน
  เป็น `Banned` ทันที, switch request ถูก **ยกเลิก**, cooldown ที่ใช้ไป
  แล้ว**ไม่ refund**. เมื่อ ban lifted player กลับ `Active` ใน faction
  **เดิม** ก่อน switch. *(ป้องกัน ban-as-cooldown-suspend exploit)*
- **EC-15** **If ban event มาถึงระหว่างแมตช์กำลัง run (ก่อน Fragment award
  compute)**: Fragment ของแมตช์นั้น **drop ทิ้งทั้งหมด** (ไม่ route,
  ไม่ escrow). *(Ban = ผู้เล่นไม่มีสิทธิ์ Fragment ของแมตช์นั้น — punitive)*
- **EC-16** **If switch submit เวลาที่ `InBlackoutWindow = false` แต่ก่อน
  switch complete window เปิด**: switch complete ตามปกติ.
  BLACKOUT ป้องกัน *submit* เท่านั้น ไม่ใช่ *complete*.

### Cycle boundary

- **EC-05** **If cycle reset เกิดขึ้น (100M reached or timeout)**: behavior
  ของ `uncommitted_f` ถูก **กำหนดโดย FT14 Fragment & Meta-Game GDD**.
  Faction System expose endpoint สำหรับ FT14 trigger reset/flush;
  ไม่ own semantics. *(See FT14)*
- **EC-06** **If switch in-flight ณ วันสุดท้ายของ cycle**: switch complete
  ตามปกติข้าม cycle ได้ — Faction System ไม่ block. *(Cycle reset
  ไม่ force-cancel switch)*
- **EC-09** **If ผู้เล่นใหม่สร้างบัญชี 1 วันก่อน cycle จบ**: ทุก rule เดิม
  ใช้ปกติ. Fragment น้อยมากแต่ valid. ไม่มี edge เพิ่มเติม.
- **EC-10** **If `Unaffiliated` player ข้าม cycle reset**: ยังคง
  `Unaffiliated`. เลือก faction ใน cycle ใหม่ตามปกติ.

### Live ops

- **EC-07** **If `FACTION_COUNT` เพิ่ม 5 → 6**: ผู้เล่น `Active`/`Inactive`
  ไม่กระทบ. ผู้เล่น `Unaffiliated` เห็น 6 ตัวเลือกทันทีจาก CBS config.
- **EC-08** **If live ops พยายามลบ faction ที่มีสมาชิก > 0**:
  **ไม่อนุญาต — explicit constraint**. Faction ที่มีสมาชิก > 0 ห้ามลบ.
  การลบ faction ทำได้เฉพาะตอน cycle reset boundary (กำหนดโดย FT14).
  *(ลด implementation complexity; ไม่ต้องมี migration logic)*

### Identity / Account

- **EC-11** **If account merge และ 2 accounts มี `faction_id` ต่างกัน**:
  ผู้เล่นถูก force re-select faction หลัง merge สำเร็จ. `uncommitted_f`
  ของทั้ง 2 accounts รวมเข้าด้วยกันใน account ที่ merge แล้ว, ไม่ flush.
  *(ลด arbitration complexity; ผู้เล่นเลือกเองชัดเจน)*

### Time / Authority

- **EC-14** **If client clock ไม่ตรง server**: ทุก timestamp
  (`last_active_date`, `last_switch_date`, decay window) ใช้
  **server time UTC** เท่านั้น. Client clock ไม่มีผล (R9 ครอบ).

## Dependencies

### F.1 Upstream (Faction depends on)

| System | Type | Interface | Status |
|---|---|---|---|
| **F4 Data/Config** | Hard | โหลด: faction definitions (`faction_id`, theme, cosmetics manifest), tuning constants (`INACTIVE_DECAY_DAYS`, `DECAY_RATE_PCT`, `DECAY_INTERVAL_DAYS`, `SWITCH_COOLDOWN_DAYS`, `BLACKOUT_DAYS_BEFORE_END`, `MIN_FRAGMENT_FLOOR`, `FACTION_COUNT`) จาก CBS | ✅ designed |
| **M6 Account & Auth** | Hard | `faction_id` ฝังใน Player Record; M6 ครอบ identity, account-merge resolution flow (EC-11 force re-select); M6 emit `account_merged` event ที่ Faction ต้อง subscribe | ✅ designed |

### F.2 Downstream (Systems that depend on Faction)

| System | Type | Interface (Faction → System) | Status |
|---|---|---|---|
| **FT12 World Map (10 Cities)** | Hard | Read `player.faction_id` on session start (cosmetic only — chat color, badge, faction theming in cities) | 🆕 undesigned |
| **FT13 Tournament System** | Hard | Snapshot `faction_id` at match-start (frozen during match per EC-13); post-match call Faction routing endpoint with `(player_id, fragment_amount, match_id)` | 🆕 undesigned |
| **FT14 Fragment & Meta-Game** | Hard | Aggregate per-faction pool from Faction's `uncommitted_f` totals; consume `IsActive` flag for decay batch eligibility; **own** cycle reset semantics (EC-05); call Faction's reset endpoint at cycle boundary | 🆕 undesigned |
| **M11 Reputation / Anti-Toxicity** | Hard | Emit ban/lift events → Faction transitions player to `Banned`/restore; optional read `faction_id` for faction-level reputation modifier (TBD scope) | 🆕 undesigned |

### F.3 Soft / Cross-cutting

| System | Type | Interface | Status |
|---|---|---|---|
| **F2 Networking Core** | Soft | Faction state changes flow via PlayFab/Azure (not Photon Fusion). F2 not required for basic faction ops. May be enhanced if in-match faction badges broadcast via Photon | ✅ designed |
| **M5 Statistics & History** | Soft | Faction expose read-only aggregate endpoints (faction win-rate, member contribution leaderboard). M5 to consume after its own revise — defer interface detail until M5 retrofit | ✅ designed (revise pending) |
| **P1 HUD & In-Game UI** | Soft (presentation) | Display `player.faction_id` badge in HUD scoreboard; teammate/opponent faction icons. No state ownership — pure render | ✅ designed |
| **P3 Hero Select UI** | Soft (presentation) | Display faction badge next to player name in lobby | ✅ designed |

### F.4 Deferred / Open

- **FT9 Town System** — DEPRECATED in pivot (replaced by FT12 10 Cities). No dependency.
- **M2 Battle Pass** — relationship with Fragment system [Open Question]; if Battle Pass tasks award Fragments, routing flows through Faction (R4). Defer until M2 retrofit.

### F.5 Bidirectional consistency notes

> ระบบ downstream ทั้ง 4 (FT12/FT13/FT14/M11) เป็น 🆕 undesigned. เมื่อ author
> GDD แต่ละตัว ต้อง list "Faction System (FT11)" ใน Dependencies section ของ
> แต่ละไฟล์ตามตารางด้านบน.
>
> สำหรับ ✅ designed systems (F4, M6, F2, M5, P1, P3): ต้อง **revise**
> Dependencies sections ของไฟล์เหล่านี้เพื่อ list FT11 ในรายการ
> "depended on by" — เพิ่มใน scope retrofit pass ของ pivot 2026-04-23
> (ใน change-impact-2026-04-23-tournament-pivot.md GDD Work Tracker).

## Tuning Knobs

> Defaults คำนวณตาม specialist recommendations (systems-designer + economy-designer)
> + half-life mapping. ทุก knob configurable ผ่าน CBS (F4); ไม่ hardcode.

### G.1 Decay Knobs

| Knob | Default | Safe Range | What it affects | Live-tune? |
|---|---|---|---|---|
| `INACTIVE_DECAY_DAYS` | **21** | 14 – 60 | จำนวนวัน inactive ก่อนเข้าสู่ `Inactive` state. ต่ำเกินไป (< 14) ลงโทษ casual; สูงเกินไป (> 60) ลด anti-hoarding pressure | ✅ A/B หลัง launch จาก DAU/MAU data |
| `DECAY_RATE_PCT` | **0.07** (7%) | 0.03 – 0.15 | สัดส่วน decay ต่อรอบ. Pair กับ INTERVAL ให้ half-life ≈ 30 วัน. ต่ำเกิน = ไม่กดดัน hoarders; สูงเกิน = ผู้เล่นกลับมารู้สึก loss อย่างแรง | ✅ A/B sensitive to session frequency |
| `DECAY_INTERVAL_DAYS` | **3** | 1 – 30 | ความถี่ของ decay batch. ต่ำ = liquidity flow ต่อเนื่อง, batch load สูง; สูง = spike ใหญ่ ผู้เล่นรู้สึก unpredictable | ⚠️ Verify Azure batch load รับได้ก่อน change |
| `MIN_FRAGMENT_FLOOR` | **1.0** | 0.1 – 100 | ค่า floor ที่ floor rule (D1) trigger return ทั้งหมด. ป้องกัน infinite micro-decay tail | ❌ Principle-driven |

**Half-life calculation:** `(1 - 0.07)^10 ≈ 0.484` → 30 วัน เหลือ ~48%.
หลัง 90 วัน เหลือ ~11%. หลัง 180 วัน ≈ 1.3% (effectively flushed).

### G.2 Switch Knobs

| Knob | Default | Safe Range | What it affects | Live-tune? |
|---|---|---|---|---|
| `SWITCH_COOLDOWN_DAYS` | **90** | 30 – 365 | Cooldown ขั้นต่ำระหว่าง switch. ต่ำ = faction-hop following winners (ทำลาย commitment fantasy); สูง = lock ผู้เล่นนานเกิน | ❌ Principle-driven |
| `BLACKOUT_DAYS_BEFORE_END` | **0** | 0 – 90 | Window ก่อน cycle จบที่ lock switch (anti-bandwagon). 0 = feature ปิด; เปิดใน live tuning หาก data แสดง late-cycle defection | ✅ A/B หลัง 1st cycle progresses |

**Switch frequency check** (default `T_cd=90`): ใน 3-year cycle (1095 วัน)
ผู้เล่น switch ได้สูงสุด ~12 ครั้ง — มากเกินพอสำหรับเปลี่ยนใจ, น้อยพอสำหรับ commitment.

### G.3 Faction Definition Knobs

| Knob | Default | Safe Range | What it affects | Live-tune? |
|---|---|---|---|---|
| `FACTION_COUNT` | **5** *(placeholder — รอ leadership decision)* | {5, 6} | จำนวน faction ทั้งหมด. โหลดจาก CBS ; เพิ่ม 5→6 รองรับ live (EC-07) ; ลดต่ำกว่าจำนวนสมาชิก ห้าม (EC-08) | ⚠️ Cycle boundary only |

> **Note:** `FACTION_COUNT=5` ในที่นี้คือ default เพื่อให้ระบบเริ่มทำงานได้.
> **ค่าจริงต้องตัดสินจาก leadership** ก่อน production. ดู Open Questions.

### G.4 Diagnostic Thresholds (FT14 dashboard hooks)

| Knob | Default | Safe Range | What it affects | Live-tune? |
|---|---|---|---|---|
| `FACTION_HEALTH_DECAY_ALERT_PCT` | **0.02** (2%) | 0.01 – 0.10 | Threshold ของ `FactionPoolDecayRate` (D4) ที่ trigger ops alert "faction unhealthy". > threshold ต่อเนื่อง 3 cycles ⇒ re-engagement campaign | ✅ Live-tune |

### G.5 Knob Interactions

- `DECAY_RATE_PCT` × `DECAY_INTERVAL_DAYS` → effective half-life. ปรับ knob เดียว
  อาจทำให้ effective rate เปลี่ยนเกินคาด — ปรับเป็น pair.
- `INACTIVE_DECAY_DAYS` ใกล้ `SWITCH_COOLDOWN_DAYS` → ผู้เล่น switch แล้วเข้า
  Inactive ทันที (Switching state ครอบ — EC-16); แต่ถ้า INACTIVE สูงกว่า
  COOLDOWN มาก ก็ลด edge นี้.
- `BLACKOUT_DAYS_BEFORE_END` > 0 + `SWITCH_COOLDOWN_DAYS` สูง → ผู้เล่นใกล้
  end-of-cycle lock จาก switch รอบ cycle ใหม่; ระวัง combined lock window.

## Visual/Audio Requirements

*Skipped at GDD authoring time — Faction System เป็น backend/data layer.
Visual touch-points (post-match Fragment flow animation, faction badge,
selection screen) จะถูกระบุใน UX spec (`design/ux/faction-selection.md`,
`design/ux/post-match-result.md`) ในขั้น Pre-Production และ asset spec
หลัง art bible approve.*

## UI Requirements

*Skipped at GDD authoring time — Faction มี UI touch-points (Faction Selection
screen, Faction Profile page, Faction Standings widget, post-match Fragment
flow callout) แต่ระบุรายละเอียดใน UX spec ภายหลัง.*

> **📌 UX Flag — Faction System**: เมื่อเข้า Pre-Production ให้รัน
> `/ux-design faction-selection` และ `/ux-design faction-profile` —
> เพิ่ม screen list ใน `design/ux/` ก่อนเขียน epics. Stories ที่อ้างถึง
> UI ต้อง cite `design/ux/[screen].md` ไม่ใช่ GDD นี้โดยตรง.

## Acceptance Criteria

> **Format:** Given-When-Then. **TR-ID:** `TR-FAC-NNN`.
> **Story types:** Logic = unit test (BLOCKING) ; Integration = integration
> test (BLOCKING) ; Performance = load test (ADVISORY pre-milestone) ;
> Manual = smoke check (ADVISORY)
>
> **Coverage:** 28 criteria — Core Rules 11, Formulas 7, Edge Cases 5,
> Cross-system 3, Performance 2 + Manual 1.

### H.1 Core Rules (R1–R11)

| TR-ID | Type | Given-When-Then |
|---|---|---|
| TR-FAC-001 (R1) | Logic | **Given** บัญชีใหม่ state=`Unaffiliated` **When** พยายามเข้า first match โดยยังไม่ผ่าน faction selection **Then** block + force selection screen ; state ยัง `Unaffiliated` |
| TR-FAC-002 (R2) | Logic | **Given** state=`Active`, faction_id="faction_a" **When** client ส่ง direct PATCH bypass Azure Function **Then** HTTP 403 ; faction_id ยังคง "faction_a" |
| TR-FAC-003 (R3) | Logic | **Given** ผู้เล่น 5 คน faction ต่างกันทั้งหมด **When** ส่งเข้า matchmaking pool **Then** จับคู่สำเร็จสร้าง lobby ได้ ; ไม่มี "faction mismatch" error |
| TR-FAC-004 (R4) | Logic | **Given** A(faction_x) + B(faction_y) ทีมเดียวกัน, match จบ **When** mock FT13 ส่ง Fragment award **Then** A's Fragment → faction_x เท่านั้น ; B's → faction_y เท่านั้น ; ไม่มี cross-route |
| TR-FAC-005 (R5) | Logic | **Given** state=Active, last_active = today−22d, INACTIVE_DECAY_DAYS=21 **When** IsActive evaluate **Then** false ; transition → `Inactive` |
| TR-FAC-006 (R6) | Logic | **Given** Active, last_switch=45d ago, COOLDOWN=90 **When** ส่ง switch request **Then** error "SWITCH_COOLDOWN_NOT_ELAPSED" ; state ไม่เปลี่ยน |
| TR-FAC-007 (R7) | Logic | **Given** state=`Switching` (faction_old → faction_new), match จบ **When** Fragment award **Then** route → faction_old |
| TR-FAC-008 (R8) | Logic | **Given** state=`Banned`, uncommitted_f=500 **When** decay batch run **Then** uncommitted_f ยังคง 500 ; ไม่มี decay |
| TR-FAC-009 (R9) | Logic | **Given** client PATCH last_active_date / fragment_uncommitted / state direct **When** server validate **Then** HTTP 403 ทุก field ; ค่าไม่เปลี่ยน |
| TR-FAC-010 (R10) | **Integration** | **Given** CBS config ตั้ง FACTION_COUNT=6 (ไม่แก้ code) **When** Unaffiliated เปิด selection screen **Then** แสดง 6 ตัวเลือก ; CBS โหลดครบ 6 definitions |
| TR-FAC-011 (R11) | Logic | **Given** faction_x player ใน Neutral City **When** query faction record **Then** ไม่มี field territory/city_owned ; FT12 อ่านแค่ faction_id |

### H.2 Formulas (D1–D4)

| TR-ID | Type | Given-When-Then |
|---|---|---|
| TR-FAC-012 (D1 normal) | Logic | **Given** uncommitted_f=10000, RATE=0.07, FLOOR=1.0 **When** decay batch 1 cycle **Then** DecayAmount=700.0 (±0.001) ; new uncommitted_f=9300.0 |
| TR-FAC-013 (D1 floor) | Logic | **Given** uncommitted_f=0.8, FLOOR=1.0, RATE=0.07 **When** decay batch **Then** DecayAmount=0.8 ; uncommitted_f=0.0 ; ไม่ติดลบ ; decay หยุด |
| TR-FAC-014 (D2 boundary true) | Logic | **Given** last_active = today−21d, threshold=21 **When** IsActive evaluate **Then** true *(edge: day 21 = ไม่เกิน)* |
| TR-FAC-015 (D2 boundary false) | Logic | **Given** last_active = today−22d, threshold=21 **When** IsActive evaluate **Then** false |
| TR-FAC-016 (D3 NULL switch) | Logic | **Given** Active, last_switch_date=NULL, BLACKOUT=0 **When** SwitchEligibility evaluate **Then** true (NULL ≡ ∞) |
| TR-FAC-017 (D3 blackout) | Logic | **Given** Active, cooldown ผ่าน, BLACKOUT=30, CycleDaysRemaining=20 **When** SwitchEligibility evaluate **Then** false (InBlackoutWindow=true) |
| TR-FAC-018 (D4 metric) | Logic | **Given** faction 1000 members, 200 inactive (20%), RATE=0.05 **When** คำนวณ FactionPoolDecayRate (approx) **Then** 0.01 (±0.0001) |

### H.3 Edge Cases (priority subset)

| TR-ID | Type | Given-When-Then |
|---|---|---|
| TR-FAC-019 (EC-02) | **Integration** | **Given** Inactive, uncommitted_f=1000, batch snapshot taken at T **When** match-end +200 writes ก่อน batch ผล (ETag mismatch) **Then** batch retry ด้วย ETag ใหม่ ; final = 1200 − DecayAmount(1000) ; ไม่มี stale overwrite |
| TR-FAC-020 (EC-03) | Logic | **Given** Switching (a→b) cooldown ผ่านไป 30/90d **When** M11 ส่ง ban **Then** state=Banned ; switch ยกเลิก ; cooldown 30d **ไม่ refund** ; ban-lift → Active ใน faction_a (ไม่ใช่ b) |
| TR-FAC-021 (EC-08) | Logic | **Given** faction_a มีสมาชิก ≥ 1 **When** admin endpoint try delete **Then** error "FACTION_HAS_MEMBERS" ; faction_a ยังอยู่ ; สมาชิกยัง faction_id="faction_a" |
| TR-FAC-022 (EC-11) | **Integration** | **Given** A(faction_x, 500) + B(faction_y, 300) merge สำเร็จ — M6 emit `account_merged` **When** Faction รับ event **Then** state=`Unaffiliated` ; uncommitted_f merged=800 (ไม่ flush) ; faction_id ว่างจน re-select |
| TR-FAC-023 (EC-15) | **Integration** | **Given** Active in-match (pre-award), uncommitted_f=200 **When** M11 ban event ระหว่าง match, match จบ, FT13 ส่ง award **Then** award **drop** ; uncommitted_f ยังคง 200 ; state=Banned |

### H.4 Cross-System Interaction

| TR-ID | Type | Given-When-Then |
|---|---|---|
| TR-FAC-024 (FT13 routing) | **Integration** | **Given** mock FT13 ส่ง `(p1, 150, m123)` ไป Faction routing endpoint **When** endpoint process **Then** uncommitted_f ของ p1 +150 ; HTTP 200 ; idempotency key m123 ป้องกัน double-routing ถ้าส่งซ้ำ |
| TR-FAC-025 (FT14 batch) | **Integration** | **Given** faction_x มี 3 inactive: [100, 200, 300], RATE=0.07, mock FT14 trigger batch **When** batch run **Then** decay [7, 14, 21] อิสระ ; aggregate=42 ; new=[93, 186, 279] |
| TR-FAC-026 (M11 ban-lift) | **Integration** | **Given** state=Banned, previous_faction="faction_a" บันทึก **When** M11 ส่ง ban-lifted event **Then** state=Active ; faction_id="faction_a" restored ; routing+decay ทำงานปกติ |

### H.5 Performance / Load (ADVISORY — pre-milestone gate)

| TR-ID | Type | Given-When-Then |
|---|---|---|
| TR-FAC-027 (decay throughput) | **Performance** | **Given** 100,000 inactive players ใน 1 faction, staging spec = production **When** decay batch run **Then** complete ≤ 60s ; ไม่มี timeout ; ETag conflict < 1% of writes |
| TR-FAC-028 (read latency) | **Performance** | **Given** 1,000 concurrent reads of `player.faction_state` **When** load test fire **Then** p95 ≤ 200ms ; p99 ≤ 500ms ; error rate < 0.1% |

### H.6 Manual Smoke

- **Manual-FAC-01 (EC-14 client clock):** QA ตั้ง client clock ผิด → verify
  server timestamp บน Player Record ไม่เปลี่ยน. *(หาก D2/D3 ใช้ client time
  แสดง bug)*

### H.7 Deferred until downstream GDDs designed

- **EC-13 mid-match switch race** — ทดสอบเมื่อ FT13 GDD approved
  (ต้องการ real match-start cache)
- **D4 dashboard alert threshold** — เพิ่มใน FT14 dashboard ACs ภายหลัง

## Open Questions

> รวม decisions ที่ยังไม่ตัดสินใน GDD นี้ (defer ไว้). แต่ละข้อมี owner +
> trigger event ที่ปลด open status

### OQ-1 — `FACTION_COUNT` (5 vs 6)
- **Owner:** Leadership / Creative Director
- **Default ใน GDD:** 5 (placeholder ใน Section G.3)
- **Trigger:** Leadership decision ก่อน production launch
- **Impact:** UI standings layout, faction theme/lore design (FT12), matchmaking dist analysis

### OQ-2 — Switch gating mechanism เพิ่มเติม (cost / approval)
- **Owner:** Game Designer + Economy Designer
- **GDD ปัจจุบันรองรับ:** cooldown-only (R6, default 90d)
- **Trigger:** ผลจาก meeting 2026-05-07 (anti-bandwagon proposal) หรือ playtest data
- **Impact:** Section C R6, Section G.2 (เพิ่ม knob), formula D3 (เพิ่ม sub-condition)

### OQ-3 — `BLACKOUT_DAYS_BEFORE_END` activation
- **Owner:** Game Designer
- **Default:** 0 (feature ปิด)
- **Trigger:** ดู late-cycle defection data หลัง 1st cycle progresses (ปี 1 ของ 3)
- **Impact:** Section G.2 knob change ; ต้อง coordinate กับ FT14 cycle pacing

### OQ-4 — M2 Battle Pass ↔ Fragment relationship
- **Owner:** Live-Ops Designer (เมื่อ M2 retrofit)
- **Question:** ถ้า Battle Pass tasks award Fragments, route ผ่าน Faction (R4) ใช่ไหม?
- **Trigger:** M2 retrofit (อยู่ใน change-impact GDD Work Tracker)
- **Impact:** Section F.4 dependency clarification

### OQ-5 — Faction theme / identity / lore
- **Owner:** Narrative Director + World Builder
- **Note:** ระบบ Faction definition โหลดจาก CBS (R10) — schema มีแล้ว แต่ content (ชื่อฝ่าย, สี, สัญลักษณ์, ปรัชญา, manga influence hooks) ยังไม่กำหนด
- **Trigger:** Pre-production narrative pass + art bible faction archetype section
- **Impact:** ไม่กระทบ schema/code ; กระทบ UI/asset spec ภายหลัง

### OQ-6 — M11 faction-level reputation modifier (scope)
- **Owner:** Game Designer + Anti-Toxicity lead
- **Question:** Reputation มี faction-level component ไหม? (เช่น faction รวม reputation modifier หรือ punishment factor)
- **Default ใน GDD:** เปิด interface ไว้ (Section F.2 — "optional read faction_id") แต่ไม่บังคับ
- **Trigger:** M11 Reputation GDD authoring
- **Impact:** Section C+F (ปรับ M11 interaction table)

### OQ-7 — Cycle reset semantics for `uncommitted_f`
- **Owner:** FT14 Fragment & Meta-Game GDD author
- **Default ใน GDD:** Faction ไม่ own — defer to FT14 (EC-05)
- **Trigger:** FT14 GDD authoring
- **Impact:** Section E EC-05 (เปลี่ยนจาก "see FT14" → resolved behavior)
