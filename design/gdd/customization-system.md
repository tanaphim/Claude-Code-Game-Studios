# Customization System — Game Design Document

**System ID**: M3
**Version**: 0.1.0
**Status**: Draft — Reverse-documented from source
**Last Updated**: 2026-04-02
**Dependencies**: Hero System (C2), Data/Config System (F4), Account & Auth (M6)

---

## 1. Overview

Customization System จัดการ Hero Skins, Weapon Skins และ Town Avatar Decorations ผ่าน CBS Inventory (`CBSSkins`, `CBSAvatars` categories) สกินแต่ละตัวมี `SkinObject` (ScriptableObject) เก็บ Model, VFX, Animation สำหรับทุก Skill ของ Hero นั้น รองรับ 4 ระดับ Rarity (Common → Legendary) การเปลี่ยนสกินในเกมใช้ `SkinSystem.CreateAvatarSkin()` ที่ swap Model + Animator

---

## 2. Player Fantasy

ผู้เล่นแสดงตัวตนด้วยสกิน Hero ที่ชื่นชอบ เห็น Splash Art สวยงาม และ VFX สกิลที่แตกต่างจาก Default ตามระดับ Rarity Town Avatar ปรับแต่งได้ตาม Preset ส่วนตัว

---

## 3. Detailed Rules

### 3.1 Skin Rarity Tiers

| Tier | รายละเอียด |
|------|-----------|
| Common | สกินพื้นฐาน |
| Rare | สกินระดับกลาง |
| Epic | สกินพิเศษ มี VFX ต่าง |
| Legendary | สกินสูงสุด VFX และ Animation เฉพาะ |

Tier เก็บใน `CBSSkin._skinTier` (CBS Item Custom Data)

---

### 3.2 SkinObject (Asset)

```
SkinObject : BaseResource {
  Name                      : string
  Model                     : GameObject (Hero model prefab)
  IconWallpaper             : Sprite

  // VFX per Skill:
  EnterVfxDictionary        : SkillVfxDictionary   [Enter state VFX]
  ReleaseVfxDictionaryList  : List<SkillVfxDictionary>  [Release combo VFX, 1-indexed]
  HitVfxDictionary          : SkillVfxDictionary   [Hit VFX]
  SkillObjectDictionary     : SkillVfxDictionary   [Projectiles]
  SkillVfxList              : List<SkillVfxDetail>
}

// Accessors:
GetSkillObject(skillKey, skillState, releaseId)
GetEnterVfx(skillKey, skillState)
GetReleaseVfx(skillKey, skillState, releaseId)
GetHitVfx(skillKey, skillState)
GetSkillVfx(skillKey, skillState, clipState, releaseId) → List<VFX>
```

---

### 3.3 SkinDataObject (Metadata)

```
SkinDataObject : ScriptableObject {
  SkinName        : string
  MinimapIcon     : Sprite
  BigIcon, MediumIcon, SmallIcon, HorizontalIcon, SplashArtIcon : Sprite
  ModelShop       : GameObject (Shop preview model)
  Tier            : SkinTier (Common/Rare/Epic/Legendary)
}
```

---

### 3.4 Skin Ownership

- CBS Category: **"CBSSkins"** (Hero skins) และ **"CBSAvatars"** (Hero unlocks)
- เช็ค Ownership: `PlayerData.GetCategoryOwner("CBSSkins")` → `Dictionary<ItemID, bool>`
- Item ID format: `s_{heroName}_{skinName}` (inferred)
- Default skin: ไม่มีใน Inventory — filter ด้วย `!Contains("default")` ใน UI

---

### 3.5 Apply Skin (In-Game)

```
SkinSystem.CreateAvatarSkin(avatarObjId, skinId, actor)
  → Fetch SkinObject via MetadataService
  → ChangeModel(skinObject.Model, options, actor)
    → Destroy old Animator
    → Instantiate new Model
    → actor.Animation.Animator = new model's Animator
  → await 500ms + frame sync
```

---

### 3.6 Apply Weapon Skin

```
SkinSystem.CreateWeaponSkin(avatarObjId, skinWeaponId, transform, actor)
  → Destroy existing weapon child (index 1+)
  → Instantiate new weapon prefab at transform
  → actor.Animation.WeaponAnimator = new weapon's Animator
```

**Weapon Skin Item ID format**: `sw_{avatarName}_default` (default), `sw_{avatarName}_{skinName}` (custom)

---

### 3.7 Town Avatar Customization

ผู้เล่นปรับแต่ง Avatar ใน Town ด้วย SkinnedMesh parts:

| Category | Load Path |
|----------|-----------|
| Head | `Settings/CharacterCustomizePart/HeadList` |
| Hair | `Settings/CharacterCustomizePart/HairList` |
| Face | `Settings/CharacterCustomizePart/FaceList` |
| Hand | `Settings/CharacterCustomizePart/HandList` |
| Shoe | `Settings/CharacterCustomizePart/ShoeList` |
| Back | `Settings/CharacterCustomizePart/BackList` |
| Shirt | `Settings/CharacterCustomizePart/ShirtList` |
| Pants | `Settings/CharacterCustomizePart/PantList` |

- Index 0 = "None" (ไม่ใส่)
- Network sync ผ่าน `NetWorkCharacterCustomizeRPC`

---

### 3.8 VFX Index (ReleaseId)

- ReleaseId 1-indexed (1, 2, 3 = combo chain)
- VFX list per event: หลาย VFX เล่นพร้อมกันได้

---

### 3.9 Inventory View

```
InventorySkinHeader.setup(avatar)
  → ItemModule.AllItems.Where(Category == "CBSSkins")
  → filter: ItemID.Contains(heroName) && StartsWith("s_") && !Contains("default")
  → สำหรับแต่ละ skin: GetInventoryOwner(skinID) → bool
  → Instantiate InventoryCbsSkinCard สำหรับสกินที่ Owned
```

---

## 4. Formulas

ไม่มี Formula — ระบบเป็น Asset lookup และ Model swap ล้วน

---

## 5. Edge Cases

| สถานการณ์ | พฤติกรรม |
|-----------|---------|
| Hero ไม่มีสกินเพิ่ม | แสดงเฉพาะ Default (filter ออก) |
| SkinObject ไม่พบ | ⚠️ ไม่พบ null check — อาจ exception |
| Model swap ระหว่าง Skill | await 500ms ป้องกัน animation glitch |
| Town part index 0 | "None" — ซ่อน mesh |
| Weapon skin ไม่ได้ซื้อ | Default weapon ใช้แทน |

---

## 6. Dependencies

| ระบบ | ความสัมพันธ์ |
|------|------------|
| **Hero System (C2)** | AvatarObject ลิงก์กับ SkinObject |
| **Account & Auth (M6)** | CBS Inventory skin ownership |
| **Data/Config (F4)** | MetadataService.ItemModule สำหรับ catalog |
| **Actor System (F1)** | `actor.Animation.Animator` swap target |
| **Combat & Skills (C1)** | Skill VFX ดึงจาก SkinObject ขณะ skill fire |

---

## 7. Tuning Knobs

| ค่า | ที่อยู่ | ผลกระทบ |
|-----|--------|---------|
| Skin Tier | CBSSkin._skinTier | Rarity ของสกิน |
| Model Swap Delay | SkinSystem | 500ms — ป้องกัน glitch |
| ReleaseId count | SkinObject.ReleaseVfxDictionaryList | จำนวน combo VFX |
| Skin Item ID prefix | CBS Catalog | ระบบค้นหาสกินในคลัง |

---

## 8. Acceptance Criteria

- [ ] Inventory แสดงสกิน Hero ที่ Owned ทั้งหมด
- [ ] กด Skin Card → Hero ใช้สกินนั้นใน Match
- [ ] SkinObject แตกต่างกันตาม Tier (VFX, Model)
- [ ] Weapon Skin เปลี่ยน Model อาวุธและ Animator
- [ ] Town Avatar Customization บันทึกและ sync ผ่าน Network
- [ ] Default skin ไม่แสดงใน Inventory (filter ออก)
- [ ] Splash Art และ Icons แสดงถูกต้องตาม SkinDataObject

---

## Known Issues / TODO

- ⚠️ **SkinObject Null Guard**: ไม่พบ null check เมื่อ SkinObject ไม่พบ — อาจ NullReferenceException
- ⚠️ **Default Skin Filter**: ใช้ `!Contains("default")` string — เปราะต่อ naming convention เปลี่ยน
- ⚠️ **Town Customization Persistence**: ไม่ชัดว่า customization save กับ PlayerData หรือ CBS
- ⚠️ **Skin Preview in Shop**: ModelShop ใน SkinDataObject อาจต้องการ Preview Scene แยก
