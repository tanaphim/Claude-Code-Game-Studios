# Item System — Game Design Document

**System ID**: FT1
**Version**: 0.1.0
**Status**: Draft — Reverse-documented from source
**Last Updated**: 2026-04-02
**Dependencies**: Actor System (F1), Combat & Skills System (C1), Gold Economy (C3), Data-Config System (F3)

---

## 1. Overview

ระบบไอเทมให้ผู้เล่นซื้อและจัดการอุปกรณ์ในเกมผ่านร้านค้าในแผนที่ สามารถซื้อได้เฉพาะเมื่ออยู่ที่ฐาน ไอเทมแบ่งเป็น 7 ระดับ (Starter → Mythic) มีระบบสูตรประกอบ (Recipe) ที่ช่วยลดราคาเมื่อมีชิ้นส่วนในคลัง ไอเทมแต่ละชิ้นมอบค่าสถิติและ Effect พิเศษแก่ Hero ผู้ถือ ทั้งแบบ Passive (ทำงานอัตโนมัติ) และ Active (ต้องกดใช้)

---

## 2. Player Fantasy

ผู้เล่นรู้สึกถึงการเติบโตอย่างต่อเนื่องตลอด Match — เริ่มต้นด้วยไอเทมราคาถูก สะสมทอง ประกอบสูตรทีละชิ้น จนครบ Build ที่วางแผนไว้ การเลือก Build ที่ตอบสนองต่อสถานการณ์ (ศัตรูสาย Physical / Magic / Hybrid) คือทักษะสำคัญที่แยกผู้เล่นระดับสูงออกจากระดับทั่วไป

---

## 3. Detailed Rules

### 3.1 โครงสร้าง Inventory

Hero มี Slot สำหรับไอเทมทั้งหมด **7 ช่อง**:

| Slot | ประเภท | รายละเอียด |
|------|--------|-----------|
| 0–5 | Regular Slot | ไอเทมทั่วไป 6 ช่อง |
| 6 | Special Slot | สำหรับไอเทม Stackable พิเศษ (Ward / Consumable) |

- Inventory "เต็ม" เมื่อ Slot 0–5 ครบทั้ง 6 ช่อง (Slot 6 ยังรับได้)
- Special Slot รับ Stack ได้สูงสุด **2** ชิ้น
- Item Skill ถูกสร้างอัตโนมัติเมื่อสวมใส่ไอเทม (7 Item Skill Slot รวม Special Slot)

### 3.2 ระดับไอเทม (ItemType)

| ระดับ | คำอธิบาย | กฎพิเศษ |
|-------|---------|---------|
| **Starter** | ไอเทมราคาถูกสุด เริ่มต้นเกม | — |
| **Potion** | Consumable ใช้แล้วหมด | ใช้ได้เฉพาะ**นอก**ฐาน; สูตร Refillable และ Non-refillable ไม่สามารถมีพร้อมกัน |
| **Boots** | เพิ่ม Move Speed | มีได้ **1 ชิ้น** เท่านั้น (รวม Tier 1+2); Tier 2 ต้องมี Tier 1 ในคลังก่อน |
| **Basic** | ไอเทมต่อสู้ทั่วไป | — |
| **Epic** | ไอเทม Rare | — |
| **Legendary** | ไอเทมหายากมาก | — |
| **Mythic** | สูงสุด มี Effect เฉพาะตัว | มีได้ **1 ชิ้น** ต่อ Hero เท่านั้น |

### 3.3 ระบบสูตรประกอบ (Recipe)

ไอเทมระดับสูงสามารถประกอบจากชิ้นส่วนที่มีใน Inventory:

```
ราคารวม (Composite) = ราคาไอเทมสำเร็จ - ราคาชิ้นส่วนที่มีอยู่
```

**ขั้นตอนการซื้อไอเทมที่มีสูตร**:
1. ระบบตรวจสอบว่ามีชิ้นส่วน Recipe ใน Inventory หรือไม่
2. ถ้ามี → ลบชิ้นส่วนออก, คิดแค่ส่วนต่าง
3. ถ้าไม่มี → คิดราคาเต็ม (ซื้อเป็น base item)
4. สูตรสามารถซ้อนได้ (ชิ้นส่วนที่ประกอบจากชิ้นส่วนอื่นอีกที)

### 3.4 ระบบ ItemAction

| Action | พฤติกรรม |
|--------|---------|
| **Passive** | ทำงานอัตโนมัติตลอดเวลาที่ถือครอง |
| **Active** | ต้องกดใช้; มี Cooldown |
| **Replace** | วางไอเทมบนพื้นที่แผนที่ (เช่น Ward) |
| **None** | ไม่มี Action พิเศษ |

### 3.5 กฎการซื้อ/ขาย

**ซื้อ (Buy)**:
- ต้องอยู่ที่ฐานเท่านั้น
- ตรวจสอบทองเพียงพอ → Validation → หักทอง → เพิ่มใน Slot
- หาก Inventory เต็ม (Slot 0–5 ครบ) → รับเฉพาะไอเทม IsSpecialSlot

**ขาย (Sell)**:
- ต้องอยู่ที่ฐานเท่านั้น
- ราคาขาย = `floor(ราคาซื้อ × SellPricePercent / 100)`
- ไอเทม Stack: ลด Stack 1 ชิ้น (ไม่ Clear Slot)
- ไอเทมทั่วไป: Clear Slot ทั้งหมด

**Undo**:
- ระบบเก็บประวัติการซื้อ/ขายไว้ (History)
- กด Undo → ย้อนรายการล่าสุด 1 รายการ
- Undo Buy → ขายคืน, Undo Sell → ซื้อคืน
- History ถูกล้างเมื่อออกจากฐาน

### 3.6 ระบบ Stack & Refill

- **IsStack**: ไอเทมสะสมในช่องเดียวได้ (เช่น Potion)
- **IsRefillable**: Charge คืนค่าอัตโนมัติ
  - ที่ฐาน: Refill ทันที
  - ในสนาม: มี `RefillCoolDown` (วินาที) ระหว่าง Charge
- Special Slot: Stack สูงสุด 2 เสมอ

### 3.7 Mythic Passive Bonus

ไอเทม Mythic มี `MythicItemEffect` — Effect พิเศษที่ให้ Bonus Stats เพิ่มเติม โดยคำนวณจาก ItemType และ TargetType ที่กำหนด (เช่น โบนัสตาม Role ของ Hero หรือจำนวนไอเทม Epic/Legendary ที่ถือ)

---

## 4. Formulas

### ราคาขาย
```
SellGold = floor(PurchasePrice × SellPricePercent / 100)
```

### ราคาประกอบ (Composite)
```
CraftPrice = ItemFullPrice - Σ(ComponentPrice สำหรับชิ้นส่วนที่มีใน Inventory)
```
- ถ้า CraftPrice = 0 → ไม่มีสูตร (base item)
- ถ้า CraftPrice > 0 → มีสูตร, ลบชิ้นส่วนออกก่อนวาง

### การแปลงค่าสถิติ

| Stat | ModifierType = Flat | ModifierType = Percent |
|------|---------------------|----------------------|
| attack_speed | ÷ 100 (เสมอ) | ÷ 100 (เสมอ) |
| move_speed | ÷ 100 (เสมอ) | ÷ 100 (เสมอ) |
| hp_regen, mp_regen | ใช้ตรงๆ | ÷ 100 |
| สถิติอื่นๆ | ใช้ตรงๆ | ÷ 100 |

> ⚠️ **หมายเหตุ**: attack_speed และ move_speed ถูก Hardcode ให้ ÷100 เสมอ โดยไม่คำนึง ModifierType ในโค้ด

---

## 5. Edge Cases

| สถานการณ์ | พฤติกรรม |
|-----------|---------|
| ซื้อไอเทมนอกฐาน | ไม่อนุญาต → ไม่มีผลใดๆ |
| ใช้ Potion ที่ฐาน | ไม่อนุญาต |
| ซื้อ Mythic ชิ้นที่ 2 | ถูกบล็อกโดย AvailableToPurchase() |
| Inventory เต็ม + ซื้อ Ward | Ward ไปที่ Special Slot (Slot 6) ได้ถ้ายังไม่เต็ม |
| Observer Ward (ID: `itemingame_Observward`) | มีกฎ Hardcode — สามารถเข้า Special Slot ได้แม้ Inventory เต็ม |
| ซื้อ Boots ชิ้นที่ 2 | ถูกบล็อก ยกเว้นเป็นการ Upgrade Tier 1 → Tier 2 ที่มีอยู่ |
| Potion Refillable + Non-refillable พร้อมกัน | ถูกบล็อก — ไม่อนุญาตให้มีทั้งสองแบบพร้อมกัน |
| Recipe ชิ้นส่วนอยู่ใน Special Slot | ระบบนับ Special Slot เมื่อคำนวณ Component |
| Undo หลังออกจากฐาน | ไม่สามารถ Undo ได้ — History ถูกล้างแล้ว |
| Stack สูงสุดใน Special Slot | 2 ชิ้น Hardcode — ไม่สามารถเปลี่ยนผ่าน Config ได้ |

---

## 6. Dependencies

| ระบบ | ความสัมพันธ์ |
|------|------------|
| **Actor System (F1)** | `Actor.Trait.ApplyStat()` ใช้เพิ่ม/ลบค่า Stats จากไอเทม |
| **Combat & Skills System (C1)** | ItemEffect หลายตัวทำงานผ่าน `ActorCombatAction`; Damage pipeline ใช้ Item stats (Lethality, Pierce ฯลฯ) |
| **Gold Economy (C3)** | ใช้ `Withdraw()` / `Deposit()` ระบบทอง; ราคาไอเทมมาจาก CBS |
| **Data-Config System (F3)** | ข้อมูลไอเทมทั้งหมดมาจาก `CBSItemInGame` ผ่าน MetadataService |
| **Level/XP System (C5)** | ไม่มีการ Lock ไอเทมตาม Level (ซื้อได้ทุก Tier ตั้งแต่ต้น) |
| **Hero System (C2)** | Hero แต่ละ Role มีข้อจำกัด `Positions` ว่าไอเทมใดใช้ได้บ้าง |
| **Photon Fusion (F5)** | `NetworkHeroInventory` ใช้ NetworkArray + NetworkBehaviourId สำหรับ Sync ทุก Client |

---

## 7. Tuning Knobs

| ค่า | ที่อยู่ | ค่าปัจจุบัน | หมายเหตุ |
|-----|--------|------------|---------|
| ราคาไอเทม (Price) | CBS → CBSItemInGame.Price | ตามการตั้งค่า | หน่วยภายใน (แสดงผล ÷ 1000) |
| เปอร์เซ็นต์ขายคืน (SellPricePercent) | CBS → CBSItemInGame.SellPricePercent | 1–100 | ตั้งต่อไอเทม |
| Cooldown Refill | CBS → CBSItemInGame.RefillCoolDown | ตามการตั้งค่า | หน่วย: วินาที |
| Special Slot Max Stack | Hardcode | 2 | เปลี่ยนได้เฉพาะในโค้ด |
| จำนวน Inventory Slot | Hardcode | 7 (6+1) | เปลี่ยนได้เฉพาะในโค้ด |
| สถิติไอเทม (Key/Value/ModifierType) | CBS → CBSItemInGame lists | ตามการตั้งค่า | แก้ได้ผ่าน Dashboard |
| สูตรประกอบ (Recipe) | CBS → CBSItemInGame.Recipe | ตามการตั้งค่า | List of Component IDs |
| Effect ของไอเทม | CBS → CBSItemInGame.Effect | ตามการตั้งค่า | ≥168 Effect ที่รองรับ |
| ไอเทม Mythic ต่อ Hero | Hardcode | 1 | เปลี่ยนได้เฉพาะในโค้ด |
| Boots ต่อ Hero | Hardcode | 1 | เปลี่ยนได้เฉพาะในโค้ด |

---

## 8. Acceptance Criteria

- [ ] ซื้อไอเทมได้เฉพาะเมื่ออยู่ที่ฐาน; นอกฐานกด Buy ไม่มีผล
- [ ] ไอเทมที่มีสูตร → ลบชิ้นส่วนออกและคิดราคาส่วนต่างถูกต้อง
- [ ] Undo ย้อนรายการล่าสุด 1 รายการ; History หายเมื่อออกจากฐาน
- [ ] Stats ของไอเทม (Flat/Percent) ถูกใช้กับ Actor ทันทีเมื่อสวมใส่/ถอด
- [ ] Mythic: ซื้อชิ้นที่ 2 ถูกบล็อก; Mythic Passive Bonus คำนวณถูกต้อง
- [ ] Boots: มีได้ 1 ชิ้น; Upgrade Tier 1→Tier 2 ได้ถ้ามี Tier 1 อยู่
- [ ] Special Slot รับ Stack สูงสุด 2; Ward ไปที่ Special Slot ได้แม้ Slot 0–5 เต็ม
- [ ] Potion ใช้ได้เฉพาะนอกฐาน; Refillable ไม่สามารถมีพร้อมกับ Non-refillable
- [ ] Item Skill ถูกสร้าง/ทำลายตาม Equip/Unequip อัตโนมัติ
- [ ] ราคาขายคืนถูกคำนวณตาม `SellPricePercent` จาก CBS
- [ ] ร้านค้าแสดงไอเทมแยกตาม Tab Type และ Role Filter ถูกต้อง
- [ ] Network Sync: การเปลี่ยน Inventory สะท้อนทุก Client ผ่าน NetworkArray

---

## Known Issues / TODO

- ⚠️ **Role Restriction**: ฟิลด์ `Positions` (Role[]) ใน ItemObject มีอยู่ แต่ต้องตรวจสอบว่า AvailableToPurchase() บังคับใช้จริงหรือไม่
- ⚠️ **Mythic Passive Formula**: ยังไม่มีเอกสารระบุว่า MythicItemEffect คำนวณ Bonus ตาม ItemType/TargetType อย่างไรในเชิงตัวเลข
- ⚠️ **attack_speed / move_speed Hardcode**: การ ÷100 เสมออาจเป็น Bug หรือ Design — ต้องยืนยันว่าค่าใน CBS ตั้งเป็น 0–1 หรือ 0–100
