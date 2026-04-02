---
status: reverse-documented
source: C:\GitHub\delta-unity\Assets\GameScripts\Gameplays\Cores\Objective\ObjectiveSystem.cs
date: 2026-04-02
---

# Gold Economy (C4)

> **Note**: เอกสารนี้ reverse-engineer จาก codebase ที่มีอยู่
> บันทึกพฤติกรรมปัจจุบันและเจตนาการออกแบบที่ได้รับการยืนยัน
> ส่วนที่ยังไม่ได้ออกแบบครบจะถูกระบุด้วย ⚠️ TODO

## 1. Overview

Gold Economy คือระบบเศรษฐกิจในแมตช์ที่กำหนดจังหวะการพัฒนาของผู้เล่น
ทองได้มาจากหลายแหล่ง (passive income, kill, objective) และใช้ซื้อไอเทมที่ทำให้
ฮีโร่แข็งแกร่งขึ้น ระบบนี้สร้าง tension ระหว่าง "farm เก็บทอง" กับ "fight ได้ bounty"

## 2. Player Fantasy

ผู้เล่นรู้สึกถึง **ความก้าวหน้าที่จับต้องได้** — ทองที่ได้จาก kill มากกว่า farm
ทำให้รู้สึก "ได้เปรียบ" ชัดเจน ทีมที่ทำ objective ได้ดีกว่าเติบโตเร็วกว่า
และผู้เล่นที่ feed ทอง (kill streak สูง) กลายเป็นเป้าหมายที่คุ้มค่า

## 3. Detailed Rules

### 3.1 หน่วยทอง

ระบบเก็บทองเป็น **internal units** และ **หาร 1,000 ก่อนแสดง**:

```
DisplayGold = CurrentGold / 1000
// แสดงเป็น "1.00k", "1.50k" ฯลฯ
```

ค่าทั้งหมดในเอกสารนี้ใช้ **internal units** ถ้าไม่ได้ระบุไว้เป็นอย่างอื่น

### 3.2 Gold Sources (แหล่งทอง)

#### ก. Initial Money (ทองเริ่มต้น)

| ค่า | รายละเอียด |
|-----|-----------|
| **จำนวน** | 1,000,000 (= แสดงเป็น 1,000k) |
| **เวลา** | ทันทีที่เกมเริ่ม (ครั้งเดียว) |
| **ใคร** | ทุกผู้เล่นทุกทีมเท่ากัน |

#### ข. Passive Income (ทองจากเวลา)

| ค่า | รายละเอียด |
|-----|-----------|
| **เริ่มต้น** | วินาทีที่ 60 หลังเกมเริ่ม |
| **Interval** | ทุก 5 วินาที |
| **จำนวน/tick** | `GoldIncreaseValue` (จาก CBSConfigBattle) ⚠️ ค่าจริงยังไม่ได้กำหนด |
| **ใคร** | ทุกผู้เล่นทุกทีมเท่ากัน |
| **ข้อยกเว้น** | ไม่มีใน Training mode |

#### ค. Hero Kill Bounty (ทองจากการฆ่าฮีโร่)

**Base Bounty ขึ้นกับ Kill Streak ของเหยื่อ:**

| Kill Streak เหยื่อ | Bounty |
|-------------------|--------|
| 0 | 300,000 |
| 1 | 400,000 |
| 2 | 500,000 |
| 3 | 600,000 |
| 4 | 700,000 |
| 5 | 800,000 |
| 6 | 900,000 |
| 7 | 1,000,000 |
| 8+ | 1,000,000 + ((streak − 7) × 100,000) |

**Base Bounty ลดลงเมื่อเหยื่อ Death Streak (ตายติดต่อ):**

| Death Streak เหยื่อ | Bounty |
|--------------------|--------|
| -1 | 270,000 |
| -2 | 240,000 |
| -3 | 210,000 |
| -4 | 180,000 |
| -5 | 150,000 |
| ≤ -6 | 120,000 (ขั้นต่ำ) |

**โบนัสเพิ่มเติม:**

| โบนัส | จำนวน | เงื่อนไข |
|-------|--------|---------|
| **First Blood** | +100,000 | kill แรกของเกม |
| **Consecutive Kill** | +(count−1) × 60,000 | kill หลายครั้งใน 12 วินาที |

**Kill Streak Rules:**
- Streak เพิ่ม 1 ต่อ kill
- Reset เป็น 0 หลังไม่มี kill ใน 15 วินาที
- Reset เป็น 0 หลัง Penta Kill (5 kills ติดกัน)

#### ง. Assist Gold (ทองจากการ assist)

```
AssistGold = ceil((BaseKillBounty / 2) / assistCount)
// สูงสุด 4 คน assist
```

ตัวอย่าง: Base bounty 300,000 + 2 assist = ceil(150,000 / 2) = 75,000 ต่อคน

#### จ. Minion/Creep Gold

| ประเภท | จำนวน | หมายเหตุ |
|--------|--------|---------|
| **Lane Minion** | จาก CBSUnit.Gold | ⚠️ รายละเอียดยังไม่ครบ |
| **Jungle Camp** | จาก CBSUnit.Gold | killer เท่านั้นที่ได้ |
| **WildBill passive** | สุ่ม 1–N | เฉพาะ WildBill, ต่อ minion kill |

> ⚠️ **TODO**: กำหนดค่า gold ของ minion แต่ละประเภท (lane / jungle camp / siege)

#### ฉ. Tower Destruction

| ค่า | รายละเอียด |
|-----|-----------|
| **จำนวน** | จาก CBSUnit.Gold ของ tower นั้น |
| **ใคร** | ทุก ally ที่อยู่ใกล้ในเวลา kill (ได้เต็มจำนวนทุกคน) |

> ไม่หาร — ทุกคนในทีมที่ร่วมทำลายได้ทองเต็มๆ

#### ช. Boss / Mini-Boss

| ค่า | รายละเอียด |
|-----|-----------|
| **จำนวน** | จาก CBSUnit.Gold |
| **ใคร** | แบ่งให้ทีม + killer ได้เพิ่มอีกส่วน |

#### ซ. เมื่อ Objective ฆ่า Hero (หอคอยหรือมอนสเตอร์ฆ่าฮีโร่)

| ค่า | รายละเอียด |
|-----|-----------|
| **Gold** | Assist gold เท่านั้น (50% base bounty แบ่งตามผู้ deal damage) |
| **Kill credit** | ไม่มี killer ที่เป็น player |

### 3.3 Gold Sinks (การใช้ทอง)

| การใช้ | จำนวน | เงื่อนไข |
|--------|--------|---------|
| **ซื้อไอเทม** | ราคาไอเทม (จาก CBSItemInGame) | ต้องอยู่ที่ base |
| **ขายไอเทม** | รับคืน SellPricePercent% | มีไอเทมในช่อง |
| **Buyback** | 10 (placeholder) | ⚠️ TODO: ออกแบบสูตรจริง |

### 3.4 Gold Display

| ค่า Internal | แสดงบน UI |
|-------------|----------|
| 0 | "0.00k" |
| 500,000 | "0.50k" |
| 1,000,000 | "1.00k" |
| 1,500,000 | "1.50k" |

**Floating Text:** แสดงเมื่อทองเปลี่ยนแปลง > 2,000 internal units

### 3.5 Networking

```
Deposit(amount)
  ↓
UpdateStatisticGold(actor, newGold, modifyGold, totalGold)  [RPC]
  ↓
ActivityData broadcast ไปทุก client
  ↓
UI อัปเดต + FloatingTextGold (ถ้า modifyGold > 2000)
  ↓
AddGoldInMinute() [ถ้า gold เพิ่ม]
```

**Stats ที่ track:**
- `Gold` — ทองปัจจุบัน
- `TotalGold` — ทองรวมทั้งเกม
- `GoldPerMinute` — เฉลี่ยต่อนาที

## 4. Formulas

### 4.1 Kill Bounty

```
ถ้า streak >= 8:  bounty = 1,000,000 + ((streak − 7) × 100,000)
ถ้า streak == 7:  bounty = 1,000,000
ถ้า streak == 6:  bounty = 900,000
ถ้า streak == 5:  bounty = 800,000
ถ้า streak == 4:  bounty = 700,000
ถ้า streak == 3:  bounty = 600,000
ถ้า streak == 2:  bounty = 500,000
ถ้า streak == 1:  bounty = 400,000
ถ้า streak == 0:  bounty = 300,000

ถ้า deathStreak == -1: bounty = 270,000
ถ้า deathStreak == -2: bounty = 240,000
ถ้า deathStreak == -3: bounty = 210,000
ถ้า deathStreak == -4: bounty = 180,000
ถ้า deathStreak == -5: bounty = 150,000
ถ้า deathStreak < -5:  bounty = 120,000
```

| ตัวแปร | คำอธิบาย | ช่วงค่า |
|--------|---------|---------|
| streak | kill streak ของเหยื่อ | 0–∞ |
| deathStreak | death streak ของเหยื่อ | 0 ถึง < -5 |
| bounty | ทอง internal units | 120,000–∞ |

### 4.2 Assist Gold

```
AssistGold = ceil((BaseBounty / 2) / min(assistCount, 4))
```

| ตัวแปร | คำอธิบาย |
|--------|---------|
| BaseBounty | bounty ของ kill นั้น (ก่อน first blood / consecutive) |
| assistCount | จำนวนคนที่ deal damage ก่อน kill (max 4) |

### 4.3 Consecutive Kill Bonus

```
bonus = max(0, (count − 1) × 60,000)
// window: 12 วินาที
```

| count | bonus |
|-------|-------|
| 1 | 0 |
| 2 | 60,000 |
| 3 | 120,000 |
| 4 | 180,000 |
| N | (N−1) × 60,000 |

### 4.4 Passive Income per Match Minute

```
ticksPerMinute = 60 / GoldRepeatTime = 60 / 5 = 12 ticks/นาที
goldPerMinute  = GoldIncreaseValue × 12
```

> ⚠️ GoldIncreaseValue ยังไม่ได้กำหนดค่าจริง

## 5. Edge Cases

| สถานการณ์ | พฤติกรรม |
|-----------|---------|
| Kill streak reset หลัง 15 วินาที | streak กลับเป็น 0, bounty กลับเป็น 300,000 |
| Penta Kill (5 kills ติดกัน) | streak reset เป็น 0 หลัง Penta |
| เหยื่อ death streak ต่ำมาก (<-5) | bounty ขั้นต่ำ 120,000 ไม่ลดต่อกว่านี้ |
| First blood ตาม kill bounty | bounty ปกติ + flat 100,000 (สะสม) |
| Tower kill ขณะไม่มีผู้เล่น | gold ไปให้ killer คนเดียว |
| Assist count = 0 | ไม่มี assist gold |
| Assist count > 4 | คำนวณจาก cap 4 คน |
| Buyback gold ไม่พอ | ปุ่ม disabled |
| Training mode passive gold | ไม่มี passive income |
| WildBill สุ่ม gold = 1 | ได้ 1 unit (= 0.001k แสดง) |
| Minion killed โดย Tower/Monster | gold ไม่ถูกแจก (ไม่มี killer เป็น player) |
| Undo purchase | คืนทองเต็มจำนวน (ใช้ flag totalGold = -1) |

## 6. Dependencies

| ระบบ | ความสัมพันธ์ |
|------|-------------|
| **Actor System (F1)** | ActorStatistic เก็บ kill/death streak สำหรับ bounty |
| **Data/Config (F4)** | CBSConfigBattle (InitialMoney, GoldStartTime, GoldRepeatTime), CBSUnit.Gold |
| **Networking Core (F2)** | UpdateStatisticGold RPC sync ทุก client |
| **Hero System (C2)** | NetworkHeroInventory เก็บและใช้ทอง |
| **Combat & Skills (C1)** | Kill event → trigger gold distribution |
| **Item System (FT1)** | ← ใช้ทองในการซื้อไอเทม |
| **Map & Objectives (FT2)** | ← Tower/Boss gold อ้างอิง objective data |
| **HUD & In-Game UI (P1)** | ← แสดง gold, floating text, GPM |

## 7. Tuning Knobs

| ค่า | ที่เก็บ | ผลกระทบ |
|-----|--------|---------|
| InitialMoney (1,000,000) | CBSConfigBattle | ทองเริ่มต้น — ยิ่งสูงผู้เล่นซื้อไอเทมได้เร็วกว่า |
| GoldStartTime (60s) | CBSConfigBattle | ดีเลย์ passive income — ยิ่งนานเปิดเกมช้า |
| GoldRepeatTime (5s) | CBSConfigBattle | ความถี่ passive — ลด = income เร็วขึ้น |
| GoldIncreaseValue | CBSConfigBattle | ⚠️ ต้องกำหนด |
| Base Kill Bounty (300,000) | Code constant | ยิ่งสูง kill มีค่ามากขึ้น snowball มากขึ้น |
| First Blood Bonus (100,000) | Code constant | ยิ่งสูง early kill สำคัญขึ้น |
| Consecutive Kill Window (12s) | Code constant | กว้างขึ้น = bonus ได้บ่อยขึ้น |
| Consecutive Kill Bonus (60,000/kill) | Code constant | ยิ่งสูง snowball มากขึ้น |
| Min Bounty (120,000) | Code constant | ป้องกันเหยื่อที่ feed จากไม่มีค่าเลย |
| Tower gold | CBSUnit.Gold per tower | ยิ่งสูง objective play มีค่ามากขึ้น |
| WildBill gold range | CBSAbility config | ความแปรปรวนของ income WildBill |

## 8. Acceptance Criteria

| # | เกณฑ์ | วิธีทดสอบ |
|---|-------|----------|
| 1 | ทุกผู้เล่นได้ InitialMoney ที่ game start | Integration: spawn → ตรวจ gold = 1,000,000 |
| 2 | Passive gold เริ่มวินาที 60 | Integration: รอ 60s → ตรวจ gold เพิ่ม |
| 3 | Passive gold ทุก 5 วินาที | Integration: นับ tick rate |
| 4 | Kill bounty ถูกต้องตาม streak table | Unit: streak 0–8 → ตรวจ bounty ตรงตาราง |
| 5 | Death streak ลด bounty ถูกต้อง | Unit: death -1 ถึง -6 → ตรวจ bounty |
| 6 | First blood +100,000 ครั้งแรกเท่านั้น | Integration: kill ครั้งแรก → +100,000, ครั้งสอง → ไม่มี |
| 7 | Consecutive bonus ถูกต้อง | Unit: 3 kills ใน 12s → +120,000 |
| 8 | Assist gold แบ่งถูกต้อง | Unit: 2 assists, 300k bounty → 75,000 each |
| 9 | Tower gold ไปทุก ally ที่อยู่ใกล้ | Integration: ทำลาย tower 3 คน → ทุกคนได้ทอง |
| 10 | Training mode ไม่มี passive gold | Integration: Training → passive gold = 0 |
| 11 | Floating text แสดงเมื่อ gold > 2,000 | Integration: รับ gold → ตรวจ floating text |
| 12 | Gold sync ถูกต้องทุก client | Integration: รับ gold → ตรวจ UI ทุก client ตรงกัน |
| 13 | Buyback ปิดเมื่อทองไม่พอ | UI test: ตาย + gold = 0 → buyback disabled |
| 14 | GoldPerMinute track ถูกต้อง | Integration: เล่น 2 นาที → GPM = total/2 |
| 15 | Kill streak reset หลัง 15 วินาที | Integration: kill → รอ 16s → bounty กลับเป็น 300k |

## 9. Known Issues / TODO

| # | ปัญหา | ความสำคัญ |
|---|-------|----------|
| 1 | **GoldIncreaseValue** ยังไม่ได้กำหนดค่าจริง | สูง |
| 2 | **Minion Gold** รายละเอียดยังไม่ครบ (มีในโค้ดแต่ยังไม่ครบ) | สูง |
| 3 | **Buyback Cost** ยังเป็น placeholder (10 units) — ต้องออกแบบสูตรจริง | กลาง |
| 4 | **WildBill random gold** — ช่วงสุ่ม N ยังไม่ได้กำหนด | ต่ำ |
