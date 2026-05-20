# 1on1面談記録機能 設計書

| 項目 | 内容 |
|------|------|
| Doc ID | DESIGN-SPEC-1ON1-001 |
| 対応仕様書 | SPEC-1ON1-001 |
| ステータス | approved |
| 作成日 | 2026-05-20 |
| 作成者 | @KokiTokutake |

---

## 1. エンティティ設計

### OneOnOneMeeting

| プロパティ | 型 | 制約 | 説明 |
|-----------|-----|------|------|
| Id | Guid | PK | |
| TalentId | Guid | FK → User.Id, NOT NULL | 面談対象タレント |
| ManagerId | Guid | FK → User.Id, NOT NULL | 実施上長 |
| ConductedAt | DateOnly | NOT NULL | 実施日 |
| Format | enum (InPerson=1/Online=2/Hybrid=3) | NOT NULL | 面談形式 |
| WillTopics | string | NULL, max:2000 | Will軸トピック |
| CanTopics | string | NULL, max:2000 | Can軸トピック |
| MustTopics | string | NULL, max:2000 | Must軸トピック |
| CurrentSituation | string | NULL, max:2000 | 近況・体調 |
| FreeText | string | NULL, max:5000 | その他メモ |
| ShareWithMdc | bool | NOT NULL, default:false | 委員会共有フラグ |
| IsDeleted | bool | NOT NULL, default:false | 論理削除フラグ |
| CreatedAt | DateTimeOffset | NOT NULL | |
| UpdatedAt | DateTimeOffset | NOT NULL | |

**インデックス:**
- `(TalentId, ConductedAt DESC)` — 一覧取得用
- `(ManagerId, TalentId)` — 権限チェック用

**リレーション:**
- `TalentId` → `User.Id` (OnDelete: Restrict)
- `ManagerId` → `User.Id` (OnDelete: Restrict)

---

### MeetingActionItem

| プロパティ | 型 | 制約 | 説明 |
|-----------|-----|------|------|
| Id | Guid | PK | |
| MeetingId | Guid | FK → OneOnOneMeeting.Id, NOT NULL | 紐付く面談 |
| Description | string | NOT NULL, max:500 | 宿題内容 |
| Status | enum (Pending=1/Done=2) | NOT NULL, default:Pending | ステータス |
| CompletedAt | DateTimeOffset | NULL | 完了日時 |
| CreatedAt | DateTimeOffset | NOT NULL | |

**インデックス:**
- `(MeetingId, Status)` — 未完了宿題取得用

**リレーション:**
- `MeetingId` → `OneOnOneMeeting.Id` (OnDelete: Cascade)

---

## 2. API設計

### コントローラー: `OneOnOnesController`（`/api/one-on-ones`）

---

### 2-1. GET /api/one-on-ones?talentId=&page=&limit= — 面談履歴一覧（TASK-042）

**認可:** Manager（配下タレントのみ）/ Senior / Admin

**クエリパラメータ:**
| パラメータ | 型 | 必須 | 説明 |
|-----------|-----|-----|------|
| talentId | uuid | ✓ | 対象タレント |
| page | int | - | ページ番号（default:1） |
| limit | int | - | 件数（default:20, max:100） |

**レスポンス（200 OK）:**
```json
{
  "items": [
    {
      "id": "uuid",
      "talentId": "uuid",
      "talentName": "田中 太郎",
      "managerId": "uuid",
      "managerName": "山田 一郎",
      "conductedAt": "2026-05-15",
      "format": "Online",
      "notesSummary": "Will: テックリードへの...",
      "actionItemCount": 3,
      "pendingActionItemCount": 1,
      "shareWithMdc": false,
      "createdAt": "2026-05-15T10:00:00Z"
    }
  ],
  "totalCount": 12,
  "page": 1,
  "limit": 20
}
```

---

### 2-2. POST /api/one-on-ones — 面談記録作成（TASK-042）

**認可:** Manager（配下タレントのみ）

**リクエスト:**
```json
{
  "talentId": "uuid",
  "conductedAt": "2026-05-15",
  "format": "Online | InPerson | Hybrid",
  "willTopics": "string | null",
  "canTopics": "string | null",
  "mustTopics": "string | null",
  "currentSituation": "string | null",
  "freeText": "string | null",
  "shareWithMdc": false,
  "actionItems": ["宿題テキスト1", "宿題テキスト2"]
}
```

**レスポンス（201 Created）:** 詳細レスポンス（2-3と同形式）

**エラーレスポンス:**
| ステータス | 条件 |
|-----------|------|
| 400 | バリデーションエラー（conductedAt 未入力等） |
| 403 | 配下タレント以外への記録作成 |
| 404 | タレントが見つからない |

---

### 2-3. GET /api/one-on-ones/{id} — 面談記録詳細（TASK-042）

**認可:** Talent（自分の記録のみ）/ Manager（配下タレント）/ Senior / Admin

**レスポンス（200 OK）:**
```json
{
  "id": "uuid",
  "talentId": "uuid",
  "talentName": "田中 太郎",
  "managerId": "uuid",
  "managerName": "山田 一郎",
  "conductedAt": "2026-05-15",
  "format": "Online",
  "willTopics": "テックリードへのキャリアについて話し合った",
  "canTopics": "マイクロフロントエンドの学習進捗を確認",
  "mustTopics": "Q2までのアーキテクチャ設計書完成に向けた計画を確認",
  "currentSituation": "体調良好。プロジェクト負荷は適度。",
  "freeText": "次回は設計書レビューを予定",
  "shareWithMdc": false,
  "actionItems": [
    {
      "id": "uuid",
      "description": "マイクロフロントエンドの概念検証を実施する",
      "status": "Pending",
      "completedAt": null,
      "createdAt": "2026-05-15T10:00:00Z"
    }
  ],
  "createdAt": "2026-05-15T10:00:00Z",
  "updatedAt": "2026-05-15T10:00:00Z"
}
```

---

### 2-4. PUT /api/one-on-ones/{id} — 面談記録更新（TASK-042）

**認可:** Manager（自分が作成した記録のみ）

**リクエスト:** POST と同形式（actionItems は新規追加分のみ）

**レスポンス（200 OK）:** 詳細レスポンスと同形式

**エラーレスポンス:**
| ステータス | 条件 |
|-----------|------|
| 403 | 他のマネージャーの記録を更新しようとした |
| 404 | 記録が見つからない |

---

### 2-5. DELETE /api/one-on-ones/{id} — 面談記録削除（TASK-042）

**認可:** Manager（自分が作成した記録のみ）

**レスポンス:** 204 No Content（論理削除）

---

### 2-6. GET /api/one-on-ones/{id}/action-items — 宿題一覧（TASK-042）

**認可:** Manager / Senior / Admin

**レスポンス（200 OK）:**
```json
[
  {
    "id": "uuid",
    "description": "概念検証を実施する",
    "status": "Pending",
    "completedAt": null,
    "createdAt": "2026-05-15T10:00:00Z"
  }
]
```

---

### 2-7. POST /api/one-on-ones/{id}/action-items — 宿題追加（TASK-042）

**認可:** Manager（自分の記録のみ）

**リクエスト:**
```json
{ "description": "string" }
```

**レスポンス（201 Created）:** 追加した宿題オブジェクト

---

### 2-8. PATCH /api/action-items/{itemId}/status — 宿題ステータス更新（TASK-042）

**認可:** Manager

**リクエスト:**
```json
{ "status": "Pending | Done" }
```

**レスポンス（200 OK）:** 更新後の宿題オブジェクト

---

### 2-9. GET /api/talents/{talentId}/one-on-one-summary — 委員会用サマリー（TASK-042）

**認可:** Senior / Admin

**レスポンス（200 OK）:**
```json
{
  "talentId": "uuid",
  "talentName": "田中 太郎",
  "lastMeetingAt": "2026-05-15",
  "totalMeetingCount": 12,
  "sharedMeetings": [
    {
      "id": "uuid",
      "conductedAt": "2026-05-15",
      "notesSummary": "Will: テックリードへの..."
    }
  ]
}
```

---

## 3. フロントエンド設計

### コンポーネント構成

```
pages/
  one-on-ones/
    index.vue          # 面談履歴一覧
    [id].vue           # 面談記録詳細
    new.vue            # 新規面談記録作成

components/
  one-on-ones/
    MeetingList.vue          # 面談一覧テーブル
    MeetingForm.vue          # 面談記録フォーム
    MeetingDetail.vue        # 面談記録詳細ビュー
    ActionItemList.vue       # 宿題一覧
    ActionItemForm.vue       # 宿題追加フォーム
    PreviousMeetingPanel.vue # 前回記録サイドパネル
```

**ダッシュボード最終面談日の警告色表示（AC-03-02）:**

ダッシュボードコンポーネントは `GET /api/one-on-ones?talentId=&limit=1` で各タレントの最新面談1件を取得し、以下のロジックで警告色を判定する:

```typescript
// 30日以上前なら警告色（Tailwind: text-red-600）
const isOverdue = (conductedAt: string) =>
  dayjs().diff(dayjs(conductedAt), 'day') >= 30;
```

### data-testid 定義

| 要素 | data-testid | 用途 |
|------|-------------|------|
| 新規作成ボタン | `meeting-new-button` | 面談記録作成画面へ遷移 |
| 実施日入力 | `meeting-conducted-at-input` | 実施日（必須） |
| 形式セレクト | `meeting-format-select` | InPerson/Online/Hybrid |
| Willトピック入力 | `meeting-will-topics-input` | Will軸メモ |
| Canトピック入力 | `meeting-can-topics-input` | Can軸メモ |
| Mustトピック入力 | `meeting-must-topics-input` | Must軸メモ |
| 近況入力 | `meeting-situation-input` | 近況メモ |
| 自由記述入力 | `meeting-free-text-input` | その他メモ |
| 委員会共有トグル | `meeting-share-mdc-toggle` | 委員会共有フラグ |
| 保存ボタン | `meeting-save-button` | 保存 |
| 宿題追加入力 | `meeting-action-item-input` | 宿題テキスト入力 |
| 宿題追加ボタン | `meeting-action-item-add-button` | 宿題追加 |
| 宿題チェックボックス | `meeting-action-item-check-{id}` | 完了マーク |
| 面談一覧行 | `meeting-list-row-{id}` | 一覧の各行 |
| 削除ボタン | `meeting-delete-button` | 面談記録削除 |

### Pinia ストア設計

```typescript
// stores/oneOnOne.ts
interface OneOnOneState {
  meetings: MeetingListItem[];
  currentMeeting: MeetingDetail | null;
  previousMeeting: MeetingDetail | null;
  totalCount: number;
  page: number;
  loading: boolean;
  error: string | null;
}

// actions
// fetchMeetings(talentId, page) → GET /api/one-on-ones
// fetchMeeting(id) → GET /api/one-on-ones/{id}
// createMeeting(req) → POST /api/one-on-ones
// updateMeeting(id, req) → PUT /api/one-on-ones/{id}
// deleteMeeting(id) → DELETE /api/one-on-ones/{id}
// addActionItem(meetingId, description) → POST /api/one-on-ones/{id}/action-items
// updateActionItemStatus(itemId, status) → PATCH /api/action-items/{itemId}/status
```

---

## 4. サービス層設計

```
Application/OneOnOne/
  IOneOnOneService.cs
  OneOnOneService.cs
  Requests/
    CreateMeetingRequest.cs
    UpdateMeetingRequest.cs
    AddActionItemRequest.cs
    UpdateActionItemStatusRequest.cs
  Responses/
    MeetingListResponse.cs
    MeetingDetailResponse.cs
    ActionItemResponse.cs
    OneOnOneSummaryResponse.cs
```

---

## 5. 技術的判断の記録

| 決定事項 | 採用した選択 | 理由 | 不採用の選択肢 |
|---------|------------|------|--------------|
| notes フィールドの格納方式 | 5つの独立カラムとして格納 | 個別フィールドへのクエリ・集計が容易。JSON型は SQL Server でのインデックス対応が複雑 | JSON 型の単一カラムに格納 |
| 論理削除 | IsDeleted フラグ | データ保持要件7年間のため物理削除は不可 | 物理削除（論理削除なし） |
| 宿題の格納先 | 独立テーブル（MeetingActionItem） | 宿題の個別ステータス管理・完了日時記録が必要 | OneOnOneMeeting の JSON 配列カラムに格納 |
| ページネーション | offset ベース | シンプルで実装コストが低い。面談件数は多くて数百件 | カーソルベース |
| 委員会連携 | shareWithMdc フラグのみ管理（サマリー API で集約） | シンプルな実装で要件を満たせる。MDC専用テーブルは過剰 | MDCShareRecord 中間テーブル |
| ID 型（仕様書: ULID / 設計: Guid） | Guid を採用 | 既存コードベース（全エンティティ）が Guid 統一。EF Core + SQL Server での ULID は追加ライブラリが必要 | ULID（string） |
| REQ-05 テンプレート機能（Should）| 今 Sprint はスコープ外 | TASK-041 はスキーマ確立が目的。templateId カラムは将来 Sprint で ALTER TABLE として追加する | 初版から templateId カラムを含める |
