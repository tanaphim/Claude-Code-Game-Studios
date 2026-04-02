# Item Shop UI — Game Design Document

**System ID**: P2
**Version**: 0.1.0
**Status**: Draft — Reverse-documented from source
**Last Updated**: 2026-04-02
**Dependencies**: Item System (FT1), Gold Economy (C3)

---

## 1. Overview

Item Shop UI เป็นหน้าร้านค้าในเกม MOBA เปิดผ่านปุ่มใน Character Console แสดงไอเทมแบ่งเป็น Category, ค้นหาได้, แสดง Recommended Build, และรองรับ Buy/Sell/Undo — ใช้งานได้เฉพาะเมื่ออยู่ที่ฐาน

---

## 2. Player Fantasy

ร้านค้าควรเปิดปิดได้เร็ว หาไอเทมที่ต้องการเจอภายใน 2 วินาที ซื้อแล้วเห็น Stats เปลี่ยนทันที ผู้เล่นใหม่ดู Recommended แล้วกด Buy ได้เลย ผู้เล่นเก่าค้นหาชื่อไอเทมหรือกรอง Role ได้รวดเร็ว

---

## 3. Detailed Rules

### 3.1 การเปิด/ปิด Shop

- **เปิด**: กดปุ่ม Shop ใน Character Console (`m_ShopButton`)
- **ปิด**: กดปุ่มซ้ำ หรือกด Escape
- **เงื่อนไข**: ใช้งานได้เฉพาะ **เมื่ออยู่ที่ฐาน** (`IsAtBase == true`)
- Shop ไม่เปิดอัตโนมัติ

### 3.2 Tabs และ Category

| Tab | เนื้อหา |
|-----|---------|
| **Recommended** | Build แนะนำตาม Role ของ Hero |
| **All Items** | ไอเทมทั้งหมด แยกหมวด |
| **Item Sets** | Build สำเร็จรูปสำหรับ Role |

**หมวดใน All Items**:
- Potion, Boots, Mythic, Legendary, Epic, Basic, Starter, Inventory

**Role Filter** (กรองใน All Items):
- All, Carry, Fighter, Mage, Assassin, Tank, Support

### 3.3 Search

- **2 Search Box**: ใน Recommended Tab และ All Items Tab แยกกัน
- **วิธีค้นหา**: Match ชื่อไอเทมขึ้นต้น (Prefix match)
- **Search Panel**: แสดงผลใน `m_SearchPanel` พร้อม Stats + Description

### 3.4 Recommended Items

- ดึงจาก `ItemReccommendObject` Metadata (ต่อ Role)
- แสดง Build Path พร้อม Component ก่อนหน้า
- Background สุ่มจาก 4 แบบ (`m_BGforSuggest[0–3]`)

### 3.5 Item Detail Panel

| Element | คำอธิบาย |
|---------|---------|
| ไอคอน | รูปไอเทม |
| ชื่อ | ชื่อไอเทม |
| ราคา | ทองที่ต้องใช้ซื้อ |
| Stats | ค่า Stat พร้อมไอคอนและสี |
| Description | คำอธิบาย Effect รวม Active/Mythic Passive |
| Recipe | ไอเทม Component ที่ต้องการ |
| Common Builds | ไอเทมที่มักซื้อคู่กัน |

### 3.6 Buy / Sell / Undo

**Buy**:
- กด Buy Button → ระบบตรวจ Gold, Inventory, และ `AvailableToPurchase()`
- ปุ่ม Disabled เมื่อ: ทองไม่พอ, Inventory เต็ม, หรือ Rule ถูกบล็อก (Mythic 2 ชิ้น ฯลฯ)
- ไม่มี Error Popup — ปุ่มเป็น Greyed Out เงียบๆ

**Sell**:
- เลือกไอเทมใน Inventory Slot → กด Sell Button
- ต้องอยู่ที่ฐาน

**Undo**:
- ย้อนรายการล่าสุด 1 รายการ (Buy หรือ Sell)
- ต้องอยู่ที่ฐาน
- History หายเมื่อออกจากฐาน
- ปุ่ม Disabled เมื่อ History ว่าง (`PurchaseState.UndoEmpty`)

### 3.7 Inventory Display ใน Shop

- แสดง 6 Slot + 1 Special Slot ในหน้า Shop
- แต่ละ Slot แสดง: ไอคอน, Cooldown (ถ้ามี), Stack Count
- คลิก Slot เพื่อดู Detail หรือ Sell

---

## 4. Formulas

### Buy Button Active Condition
```
BuyButton.interactable = isDetailVisible
                       AND IsAtBase
                       AND CanAfford (gold >= price)
                       AND !InventoryFull
                       AND AvailableToPurchase(item)
                       AND !IsProcessing
```

---

## 5. Edge Cases

| สถานการณ์ | พฤติกรรม |
|-----------|---------|
| กด Buy นอกฐาน | ปุ่มถูก Disable — กดไม่ได้ |
| ทองไม่พอ | ปุ่ม Buy Greyed Out ไม่มี Popup |
| Inventory เต็ม | ปุ่ม Buy Greyed Out |
| Undo หลังออกจากฐาน | ปุ่ม Undo Disabled (History ล้างแล้ว) |
| ค้นหาคำที่ไม่มีผลลัพธ์ | Search Panel ว่าง |

---

## 6. Dependencies

| ระบบ | ความสัมพันธ์ |
|------|------------|
| **Item System (FT1)** | ข้อมูลไอเทม, Buy/Sell Logic, Inventory |
| **Gold Economy (C3)** | ตรวจ Gold ก่อนซื้อ; Deduct/Refund |
| **HUD & UI (P1)** | Shop เปิดผ่าน Character Console |

---

## 7. Tuning Knobs

| ค่า | ที่อยู่ | ผลกระทบ |
|-----|--------|---------|
| ราคาไอเทม | CBS | ความง่ายในการซื้อ |
| SellPricePercent | CBS | ราคาขายคืน |
| Recommended Build | ItemReccommendObject | Build แนะนำต่อ Role |

---

## 8. Acceptance Criteria

- [ ] เปิด Shop ได้เฉพาะที่ฐาน; กด Shop Button หรือ Esc ปิดได้
- [ ] Tab Recommended แสดง Build ตาม Role ของ Hero ที่เล่น
- [ ] Search กรองไอเทมแบบ Real-time
- [ ] Item Detail Panel แสดง Stats + Description + Recipe ครบ
- [ ] Buy ไม่ได้เมื่อทองไม่พอ (ปุ่ม Greyed Out)
- [ ] Sell คืนทองตาม SellPricePercent
- [ ] Undo ย้อนรายการล่าสุดได้; Disabled เมื่อ History ว่าง

---

## Known Issues / TODO

- ⚠️ **ไม่มี Error Popup**: ผู้เล่นอาจไม่รู้สาเหตุว่าทำไม Buy ไม่ได้ (ทองไม่พอ? เต็ม? Rule?)
- ⚠️ **Role Filter Logic**: ตรวจสอบว่า Filter กรองตาม CBSUnit.Positions จริงหรือไม่
