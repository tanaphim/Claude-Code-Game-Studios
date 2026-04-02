# Account & Auth System — Game Design Document

**System ID**: M6
**Version**: 0.1.0
**Status**: Draft — Reverse-documented from source
**Last Updated**: 2026-04-02
**Dependencies**: Networking Core (F2), Data/Config System (F4)

---

## 1. Overview

ระบบ Account & Auth ใช้ PlayFab เป็น Backend หลัก ผ่าน CBS (Cloud Backend System) Wrapper `FabAuth` รองรับ Login หลายช่องทาง (Device ID, Email/Password, Steam, Google, Apple, Facebook) หลัง Login จะเรียก Azure Cloud Function `PostAuth` เพื่อ Initialize ข้อมูลผู้เล่น ข้อมูล Session บันทึกใน `PlayerData` (Singleton JSON serialized) ผ่าน `PlayerService`

---

## 2. Player Fantasy

ผู้เล่นเข้าเกมได้เร็วด้วย Device ID อัตโนมัติ ไม่ต้องสร้าง Account ก่อน สามารถเชื่อมกับ Steam/Google/Apple เพื่อ Cross-device progress ภายหลังได้ กลับมาเล่นบนอุปกรณ์เดิมโดยข้อมูลยังครบ

---

## 3. Detailed Rules

### 3.1 Login Methods

| วิธีการ | Script | Platform |
|--------|--------|---------|
| Device ID (Auto) | `FabAuth.LoginWithDevice()` | Android / iOS / Editor |
| Email + Password | `FabAuth.LoginWithMailAndPassword()` | ทุก Platform |
| Username + Password | `FabAuth.LoginWithUserNameAndPassword()` | ทุก Platform |
| Custom ID | `PlayfabAuthenticator.Login(loginName)` | Internal / Debug |
| Steam | `PlayfabAuthenticator.LoginSteam()` | PC (Steam) |
| Google | `FabAuth.LoginWithGoogle(serverAuthCode)` | Android |
| Apple | `FabAuth.LoginWithApple(identityToken)` | iOS |
| Facebook | `FabAuth.LoginWithFacebook(accessToken)` | ทุก Platform |
| OpenID | `FabAuth.LoginWithOpenID(connectionID, IDToken)` | Enterprise |
| PlayStation | `FabAuth.LoginWithPlaystation(authCode, ...)` | PS4/PS5 |
| Server Bypass | `PlayfabAuthenticator.BypassLogin()` | Server Mode |

---

### 3.2 Login Flow

```
UILoginView (Input: PlayerName ≥ 4 chars)
  ↓
PlayerService.PlayerData.DetlaLogin(name, isClone, onSuccess, onFail)
  ↓
FabAuth.LoginWithDevice()  ← (หรือ Method ที่เลือก)
  ↓
PlayFabClientAPI.LoginWith[Platform]
  ↓  (success)
FabAuth.PostAuthProccess()
  ↓
Azure Function: AzureFunctions.PostAuthMethod
  Params: ProfileID, NewlyCreated, AuthGenerateName, RandomNamePrefix,
          PreloadPlayerLevel, PreloadAccountData, PreloadClan, LoadItems
  ↓  (result)
PlayerData.Playfab = loginResult
PlayerData.IsLoged = true
  ↓
PlayerService.Update() → IsDirty → SavePlayer()
  ↓
PersistentStorage.SetObject(key: "PlayerData-{userID}")
  ↓
PlayerDataChanged event fired
```

---

### 3.3 Device ID Resolution

ลำดับความสำคัญ:
1. `ApplicationSettings.CustomDeviceID` (ถ้า HasCustomDeviceID = true)
2. `Guid.NewGuid()` (ถ้า UseRandomDeviceID = true)
3. `SystemInfo.deviceUniqueIdentifier` (default)
4. Editor เพิ่ม hash ของ `Application.dataPath` (แยก instance)
5. Multi-client: เพิ่ม `.{clientIndex}` suffix ถ้า PlayerData ถูก lock

---

### 3.4 PlayerData (Session Container)

| Field | Type | คำอธิบาย |
|-------|------|---------|
| `IsLoged` | bool | สถานะ Login |
| `m_Playfab` | LoginResult | PlayFab Session (SessionTicket, PlayFabId) |
| `m_LoginToken` | EntityTokenResponse | CBS Backend Token |
| `m_PlayerName` | string | Display Name |
| `m_UserId` | string | Device / Local ID |
| `m_UnityId` | string | Unity Services Player ID |
| `m_Account` | GetPlayerCombinedInfoResultPayload | Stats, Inventory, Profile |
| `inventory` | CBSGetInventoryResult | Owned Items |
| `SteamAvatar` | ulong | Steam Account ID |

**Dirty Pattern**: `IsDirty = true` เมื่อ Property เปลี่ยน → `PlayerService.Update()` Save

---

### 3.5 CBS Modules (Lazy-loaded หลัง Login)

- `CBSAuth` (IAuth) — FabAuth instance
- `ProfileModule` (IProfile) — Display Name, Avatar
- `CBSInventory`, `CBSItemModule` — Item Ownership
- `FriendsModule` — Friends List
- `CurrencyModule` — Virtual Currency
- `LeaderboardModule`, `AchievementModule`

---

### 3.6 Account Linking / Unlinking

รองรับ Link/Unlink Platform บนบัญชีเดิม:
- Facebook, Apple, Google, Steam

```
FabAuth.LinkFacebookAccount(accessToken, callbacks)
FabAuth.UnlinkGoogleAccount(callbacks)
```

---

### 3.7 Login Modes (RunMode)

| RunMode | พฤติกรรม |
|---------|---------|
| REMOTE_SERVER | Auto-login: "REMOTE_SERVER" |
| LOCAL_SERVER | Auto-login: "LOCAL_SERVER" |
| LOCAL_CLIENT | โหลดชื่อจาก PlayerPrefs "PP_PlayerName" |
| REMOTE_CLIENT | โหลดชื่อจาก PlayerPrefs "PP_PlayerName" |
| ParrelSync Clone | ใช้ key "PP_PlayerName_Clone" |

---

### 3.8 Logout & Shutdown

- `FabAuth.Logout()` → `PlayFabClientAPI.ForgetAllCredentials()`
- `PlayerService.Shutdown()`:
  1. Unlock PlayerData
  2. SavePlayer()
  3. ClearAll events
  4. `ForgetAllCredentials()`
  5. `SteamAPI.Shutdown()` (ถ้า Steam)

---

### 3.9 Post-Login Data Load

`FunctionPostLoginRequest` ส่งไปยัง Azure Function:
```
ProfileID          : PlayFab Player ID
AuthGenerateName   : auto-generate nickname ถ้าใหม่
NewlyCreated       : new player flag
RandomNamePrefix   : prefix สำหรับชื่อ auto-gen
PreloadPlayerLevel : โหลด Level data
PreloadAccountData : โหลด Profile/Stats
PreloadClan        : โหลด Clan membership
LoadItems          : SINGLE_CALL หรือ SEPARATE_CALL
```

---

## 4. Formulas

ไม่มี Formula ทางคณิตศาสตร์ — ระบบ Auth เป็น State Machine ล้วน

---

## 5. Edge Cases

| สถานการณ์ | พฤติกรรม |
|-----------|---------|
| Device ID ซ้ำกัน (Multi-client) | เพิ่ม `.{clientIndex}` suffix |
| Network ล้มเหลวระหว่าง Login | `Failed` event → UI แสดง error |
| ผู้เล่นใหม่ (NewlyCreated=true) | Azure Function สร้าง Profile + auto-name |
| Session หมดอายุ | ต้อง Re-login (ไม่มี auto-refresh ชัดเจน) ⚠️ |
| Server Mode | `BypassLogin()` ด้วย CustomID "Server" |
| PlayerData locked | Multi-client: สร้าง instance ใหม่พร้อม suffix |

---

## 6. Dependencies

| ระบบ | ความสัมพันธ์ |
|------|------------|
| **Networking Core (F2)** | PlayFab API calls ผ่าน Network |
| **Data/Config (F4)** | `BuildSettings`, `RuntimeSettings`, `PersistentStorage` |
| **Social System (M1)** | `FriendsModule` โหลดหลัง Login |
| **Customization (M3)** | `GetCategoryOwner("CBSAvatars"/"CBSSkins")` |
| **Statistics (M5)** | Stats ใน `m_Account` payload |
| **Battle Pass (M2)** | `AchievementModule` โหลดหลัง Login |

---

## 7. Tuning Knobs

| ค่า | ที่อยู่ | ผลกระทบ |
|-----|--------|---------|
| TitleId | PlayFabSettings.staticSettings | PlayFab Project ID |
| PostAuthMethod | AzureFunctions constant | Azure Function name |
| CreateAccount flag | LoginRequest | auto-create on first login |
| PlayerName min length | UILoginView | 4 chars (hard-coded) |
| Random name prefix | FunctionPostLoginRequest | prefix ของ auto-gen name |

---

## 8. Acceptance Criteria

- [ ] Device ID Login ทำงานบน Android / iOS / PC
- [ ] Email/Password Registration และ Login ทำงาน
- [ ] Steam Login ทำงานบน PC (Steam platform)
- [ ] PostAuth Azure Function ถูกเรียกหลัง Login สำเร็จ
- [ ] PlayerData บันทึกและโหลดจาก PersistentStorage ได้
- [ ] Logout เคลียร์ Credentials ทั้งหมด
- [ ] Multi-client (ParrelSync): แต่ละ Client ใช้ Device ID แยกกัน
- [ ] ผู้เล่นใหม่ได้รับชื่อ Auto-generated

---

## Known Issues / TODO

- ⚠️ **Typos ในโค้ด**: `ResetPasswort` (ควรเป็น ResetPassword), `FogotPassword` (ควรเป็น ForgotPassword)
- ⚠️ **Session Expiry**: ไม่พบ auto-refresh logic — อาจต้อง re-login ถ้า session หมด
- ⚠️ **Steam Auth Comment**: `SteamNetworkingIdentity` ถูก comment out — Steam route อาจไม่สมบูรณ์
- ⚠️ **Azure Function Dependency**: Post-login processing ขึ้นกับ Azure entirely — ล้มเหลวถ้า Function ลง
