# 通知・アラートシステム 設計書

| 項目 | 内容 |
|------|------|
| Doc ID | DESIGN-SPEC-NOTIF-001 |
| 対応仕様書 | SPEC-NOTIF-001 |
| ステータス | approved |
| 作成日 | 2026-05-27 |
| 作成者 | @KokiTokutake |

---

## 1. エンティティ設計

### Notification

| プロパティ | 型 | 制約 | 説明 |
|-----------|-----|------|------|
| Id | Guid | PK | NewGuid() |
| UserId | Guid | FK → User.Id, Required | 通知受信者 |
| Type | NotificationType (enum) | Required | 通知種別 |
| Title | string | max:200, Required | 通知タイトル |
| Body | string | max:500, Required | 通知本文 |
| LinkUrl | string? | max:500, nullable | 遷移先URL（相対パス） |
| IsRead | bool | Default: false | 既読フラグ |
| ReadAt | DateTimeOffset? | nullable | 既読日時 |
| CreatedAt | DateTimeOffset | Required | 通知生成日時 |
| ExpiresAt | DateTimeOffset? | nullable | 有効期限（既読後90日。定期バッチで削除） |

**インデックス:**
- `(UserId, IsRead, CreatedAt DESC)` — 未読一覧の高速取得
- `(UserId, CreatedAt DESC)` — 全通知一覧の取得
- `(ExpiresAt)` — 期限切れ通知の定期削除バッチ用

**リレーション:**
- `UserId` → `User.Id` (OnDelete: Cascade) — ユーザー削除時に通知も削除

### NotificationType 列挙型

```csharp
public enum NotificationType
{
    EvaluationPeriodStarted,     // NT-01: 考課期間開始
    SelfEvaluationDueSoon,       // NT-02: 自己評価期限3日前
    SelfEvaluationSubmitted,     // NT-03: 自己評価提出
    EvaluationRejected,          // NT-04: 考課差し戻し
    EvaluationApproved,          // NT-05: 考課確定
    KpiCheckinOverdue,           // NT-06: KPIチェックイン未実施
    NineBoxKeyPersonCandidate,   // NT-07: 重点管理候補フラグ
    SuccessionGapAlert,          // NT-08: 後継者ギャップアラート
    SuccessionCandidateAdded,    // NT-09: 後継者候補登録
    CompetencyEvaluationDueSoon  // NT-10: コンピテンシー評価期限前
}
```

---

## 2. API設計

### GET /api/notifications

**クエリパラメータ:**
- `userId` (required): 通知受信者 ID
- `unreadOnly` (bool, default: false): 未読のみ絞り込み

**リクエストヘッダー:**
- `X-User-Id`: リクエスト元ユーザー ID

**レスポンス（200 OK）:**
```json
[
  {
    "id": "guid",
    "type": "EvaluationRejected",
    "title": "上長評価で差し戻されました",
    "body": "目標設定を修正して再提出してください。",
    "linkUrl": "/evaluation/self",
    "isRead": false,
    "readAt": null,
    "createdAt": "2026-05-27T10:00:00Z"
  }
]
```

**エラーレスポンス:**
| ステータス | 条件 |
|-----------|------|
| 403 | userId が自分以外 |

### GET /api/notifications/unread-count

**クエリパラメータ:** `userId` (required)
**レスポンス（200 OK）:** `{ "count": 3 }`
**エラー:** 403 (userId が自分以外)

### PUT /api/notifications/{id}/read

**リクエストヘッダー:** `X-User-Id`

**レスポンス（200 OK）:**
```json
{ "id": "guid", "isRead": true, "readAt": "2026-05-27T10:05:00Z" }
```

**エラーレスポンス:**
| ステータス | 条件 |
|-----------|------|
| 403 | 自分の通知以外を操作しようとした |
| 404 | 通知が存在しない |

### PUT /api/notifications/read-all

**クエリパラメータ:** `userId` (required)
**リクエストヘッダー:** `X-User-Id`
**レスポンス（200 OK）:** `{ "updatedCount": 5 }`
**エラー:** 403 (userId が自分以外)

### 認可ルール

| ロール | 操作 |
|--------|------|
| 全ロール（Talent/Manager/Senior/Executive/Admin） | 自分の通知のみ参照・既読化 |

### 通知生成（内部サービス）

通知の生成は `INotificationService.CreateAsync()` で行い、各ドメインサービスから呼び出す。

```csharp
// 使用例（EvaluationService から呼び出し）
await notificationService.CreateAsync(
    userId: talentId,
    type: NotificationType.EvaluationRejected,
    title: "上長評価で差し戻されました",
    body: "目標設定を修正して再提出してください。",
    linkUrl: "/evaluation/self"
);
```

---

## 3. フロントエンド設計

### コンポーネント構成

```
components/
  notification/
    NotificationBell.vue          # ヘッダーベルアイコン + 未読バッジ + ドロップダウン
    NotificationDropdown.vue      # 最新10件ドロップダウンリスト
    NotificationItem.vue          # 通知アイテム1件
pages/
  notifications/
    index.vue                     # 通知一覧ページ（/notifications）
stores/
  notification.ts                 # Pinia ストア
```

### data-testid 定義

| 要素 | data-testid | 用途 |
|------|-------------|------|
| 通知ベルアイコン | `notification-bell` | ヘッダーのベルボタン |
| 未読件数バッジ | `notification-unread-badge` | 未読数表示 |
| 通知ドロップダウン | `notification-dropdown` | ドロップダウン本体 |
| 通知アイテム行 | `notification-item-{id}` | 各通知行 |
| 一括既読ボタン | `notification-read-all-button` | 一括既読 |
| 通知一覧ページ | `notification-list` | 一覧ページのコンテナ |

### 状態管理ストア設計

```typescript
// stores/notification.ts（Pinia）
interface NotificationState {
  notifications: Notification[]
  unreadCount: number
  loading: boolean
  error: string | null
}
// actions: fetchNotifications, fetchUnreadCount,
//          markAsRead, markAllAsRead, $reset
```

---

## 4. 技術的判断の記録

| 決定事項 | 採用した選択 | 理由 | 不採用の選択肢 |
|---------|------------|------|--------------|
| 通知のリアルタイム配信 | DB ポーリング（1分間隔） | v1 はシンプルな実装で開始。WebSocket/SSE は v2 | WebSocket、Server-Sent Events |
| 通知の削除タイミング | ExpiresAt フィールドで管理・定期バッチ削除 | 論理削除不要・パフォーマンスを維持 | 論理削除（IsDeleted フラグ） |
| 通知生成の実行箇所 | 各サービスから `INotificationService` を呼び出す | レイヤー分離・テスト容易性 | ドメインイベント + ハンドラー |
| userId の認可 | X-User-Id ヘッダーと クエリ userId を比較 | 既存 API と統一したパターン | JWT クレーム直接参照 |

---

## 5. 受入基準との対応

| 受入基準 | 設計での対応 |
|---------|-----------|
| AC-01-01: 未読バッジ表示 | `GET /api/notifications/unread-count` → `NotificationBell.vue` |
| AC-01-02: 未読0件でバッジ非表示 | unreadCount === 0 のとき `notification-unread-badge` を非表示 |
| AC-02-01: 通知一覧新着順・未読区別 | `GET /api/notifications` で `CreatedAt DESC` ソート、`isRead` で背景色制御 |
| AC-03-01: クリックで既読化・遷移 | `PUT /api/notifications/{id}/read` → `linkUrl` で画面遷移 |
| AC-03-02: 一括既読 | `PUT /api/notifications/read-all` |
| AC-04-01: 差し戻し通知（NT-04） | `EvaluationService.Reject()` → `INotificationService.CreateAsync(EvaluationRejected)` |
| AC-04-02: 後継者ギャップアラート（NT-08） | `SuccessionCandidateService` → `INotificationService.CreateAsync(SuccessionGapAlert)` |
| AC-04-03: 重点管理候補フラグ通知（NT-07） | `NineBoxService.Calibrate()` → `INotificationService.CreateAsync(NineBoxKeyPersonCandidate)` |
