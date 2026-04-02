# Notification System — Game Design Document

**System ID**: M4
**Version**: 0.1.0
**Status**: Draft — Reverse-documented from source
**Last Updated**: 2026-04-02
**Dependencies**: Networking Core (F2), Social System (M1), Account & Auth (M6)

---

## 1. Overview

Notification System แสดงข้อความแจ้งเตือนแบ่งเป็น 3 Category (News, Inbox, Updates) ผ่าน `UINotificationWindow` โดย Inbox รองรับ Reward claiming เชื่อมกับ CBS `CBSNotificationModule` สำหรับข้อมูล Notification และ SignalR (`SignalRService`) สำหรับ Real-time events เช่น Friend Request และ Game Invite

---

## 2. Player Fantasy

ผู้เล่นได้รับข่าวสาร Event ใหม่, รับรางวัล Inbox, และรู้ว่าเพื่อนเชิญเข้าเกม โดยไม่ต้องออกจากหน้าจอหลัก

---

## 3. Detailed Rules

### 3.1 Notification Categories

| Category | คำอธิบาย | Badge นับจาก |
|----------|---------|------------|
| News | ข่าวสาร / Event จาก Developer | `!Read` |
| Inbox | รางวัล / ข้อความส่วนตัว | `!ReadAndRewarded()` |
| Updates | อัปเดตเกม / แจ้งเตือนทั่วไป | `!Read` |

---

### 3.2 UI Architecture

```
UINotificationWindow (Main Controller)
  ├── NotificationTabListener    — Tab switching (News/Inbox/Updates)
  ├── NotificationTitleScroller  — Scrollable notification list
  │     └── NotificationSlot[]   — List items
  └── NotificationDrawer         — Detail view (title, date, body, reward)
```

---

### 3.3 Load Flow

```
UINotificationWindow.OnOpen()
  → GetNotificationList() × 3 categories (TargetLoadApi = 3)
    → CBSNotificationModule.GetNotificationList(category, callback)
  → DrawNotificationList(notifications)
    → Spawn NotificationSlot prefab per notification
  → UpdateMessagesBadge(notifications, type)
    → Update badge count on home widget
```

---

### 3.4 Notification Detail & Actions

```
กด Notification → OnSelectNotification(slot)
  → NotificationDrawer.Draw(notification, slot)
    → แสดง Title, Date, Body, Reward info
  → MarkAsRead() → NotificationCenter.MarkNotificationAsRead(instanceID)
  → ClaimHandler() [ถ้า Inbox + มี Reward]
    → NotificationCenter.ClaimNotificationReward(instanceID)
    → Update: Inventory, Level/XP, Currency
  → RemoveHandler() → NotificationCenter.RemoveNotification(instanceID)
```

**Reward Update Triggers หลัง Claim**:
- `InventoryViewCbs.refreshinventory?.Invoke()`
- `UIProfileView.RefreashLevel?.Invoke()`
- `CBSPlayerCurrency.Refreash?.Invoke()`

---

### 3.5 Reward Types (Inbox)

- BundledItems (Item bundle)
- Lootboxes
- VirtualCurrencies
- Experience

---

### 3.6 Sorting

- Dropdown: `0 = Newest First`, `1 = Oldest First`
- `SortGameObjects(dropdown)` จัดเรียง NotificationSlot ใน scroll list

---

### 3.7 Real-time Notifications (SignalR)

SignalR endpoint: `https://rds-cbs-function-app.azurewebsites.net/api/negotiate`

| Event | Callback | Action |
|-------|---------|--------|
| ReceivedFriendRequest | `SignalRService.OnReceivedFriendRequest` | Refresh Friend List |
| ReceivedInviteGame | `SignalRService.OnReceivedInviteGame(customData, inviterData)` | แสดง Invite Dialog |

---

### 3.8 Badge Management

- `UIHomeWidgetView.NotificationBadge` visibility ขึ้นกับ UnreadCount
- UnreadCount = ผลรวมของ unread notifications ทุก category

---

## 4. Formulas

```
UnreadCount (News/Updates) = count where !notification.Read
UnreadCount (Inbox)        = count where !notification.ReadAndRewarded()
```

---

## 5. Edge Cases

| สถานการณ์ | พฤติกรรม |
|-----------|---------|
| Claim reward ล้มเหลว | ⚠️ ไม่พบ error handling ชัดเจน — popup viewer only |
| Notification ไม่มีรางวัล | ปุ่ม Claim ซ่อน |
| Network หลุดระหว่าง load | CBS API return error — list ว่าง |
| MarkAsRead ล้มเหลว | Badge อาจไม่ลด ⚠️ |
| SignalR disconnect | ไม่มี retry logic |

---

## 6. Dependencies

| ระบบ | ความสัมพันธ์ |
|------|------------|
| **Networking Core (F2)** | SignalR WebSocket connection |
| **Social System (M1)** | Friend Request notifications via SignalR |
| **Account & Auth (M6)** | PlayFab ID สำหรับ SignalR connection |
| **Menu & Lobby UI (P4)** | `UIHomeWidgetView.NotificationBadge` |
| **Battle Pass (M2)** | Level/currency update หลัง reward claim |

---

## 7. Tuning Knobs

| ค่า | ที่อยู่ | ผลกระทบ |
|-----|--------|---------|
| Notification Categories | CBS Config | News / Inbox / Updates |
| Reward Types | CBS Notification Data | ประเภทรางวัลใน Inbox |
| SignalR Endpoint | SignalRService | URL ของ Azure SignalR Hub |

---

## 8. Acceptance Criteria

- [ ] Notification Window แสดง 3 tabs (News, Inbox, Updates)
- [ ] Badge แสดงจำนวน Unread บน home widget
- [ ] กด Notification → Detail view แสดง
- [ ] Mark as Read ลด Badge count
- [ ] Inbox reward Claim ได้; Inventory/Currency/Level อัปเดต
- [ ] Sort Newest/Oldest ทำงาน
- [ ] Friend Request → SignalR → UI refresh Friend List
- [ ] Game Invite → SignalR → Invite Dialog แสดง

---

## Known Issues / TODO

- ⚠️ **Claim Error Handling**: ไม่มี UI error state ถ้า claim ล้มเหลว
- ⚠️ **SignalR No Retry**: ถ้า connection ล้มเหลว ไม่มี reconnect logic
- ⚠️ **NotificationUnittes**: Class ซ้ำซ้อน (redundant accessor singleton)
- ⚠️ **OnEnable Refresh Disabled**: Comment out — Notification ไม่ refresh เมื่อ panel เปิด auto
