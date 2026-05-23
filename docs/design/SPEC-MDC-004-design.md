# 委員会議事録の構造化（決定事項・アクションアイテム管理）設計書

| 項目 | 内容 |
|------|------|
| Doc ID | DESIGN-SPEC-MDC-004 |
| 対応仕様書 | SPEC-MDC-004 |
| ステータス | approved |
| 作成日 | 2026-05-23 |
| 作成者 | @KokiTokutake |

---

## 1. エンティティ設計

### 1-1. MdcDecision（新規）

MDCセッションの決定事項。フリーテキスト議事録（`CommitteeSession.Minutes`）を補完する構造化記録。

| プロパティ | 型 | 制約 | 説明 |
|-----------|-----|------|------|
| Id | Guid | PK | |
| SessionId | Guid | FK → CommitteeSession.Id, NOT NULL | 対象セッション |
| DecisionType | DecisionType (enum) | NOT NULL | 決定タイプ |
| Content | string | NOT NULL, max:2000 | 決定内容 |
| DecidedAt | DateOnly | NOT NULL | 決定日 |
| CreatedBy | Guid | FK → User.Id, NOT NULL | 作成者 |
| CreatedAt | DateTimeOffset | NOT NULL | |
| UpdatedAt | DateTimeOffset | NOT NULL | |

**DecisionType 列挙値:**
| 値 | 名前 | 説明 |
|----|------|------|
| 0 | KeyPersonAssign | 重点管理指定 |
| 1 | KeyPersonRemove | 重点管理解除 |
| 2 | DevelopmentPolicyChange | 育成方針変更 |
| 3 | TransferConsideration | 異動検討 |
| 4 | Other | その他 |

**インデックス:**
- `(SessionId)` — セッション別一覧取得用

**リレーション:**
- `SessionId` → `CommitteeSession.Id` (OnDelete: Cascade)
- `CreatedBy` → `User.Id` (OnDelete: Restrict)

---

### 1-2. MdcDecisionTalent（新規）

決定事項と対象タレントの中間テーブル（0〜多のリレーション）。

| プロパティ | 型 | 制約 | 説明 |
|-----------|-----|------|------|
| DecisionId | Guid | PK（複合）, FK → MdcDecision.Id | |
| TalentId | Guid | PK（複合）, FK → User.Id | 対象タレント |

**インデックス:**
- `(TalentId)` — タレント別決定履歴検索用

**リレーション:**
- `DecisionId` → `MdcDecision.Id` (OnDelete: Cascade)
- `TalentId` → `User.Id` (OnDelete: Restrict)

---

### 1-3. MdcActionItem（新規）

決定事項またはセッションに紐づくアクションアイテム。

| プロパティ | 型 | 制約 | 説明 |
|-----------|-----|------|------|
| Id | Guid | PK | |
| SessionId | Guid | FK → CommitteeSession.Id, NOT NULL | 所属セッション |
| DecisionId | Guid? | FK → MdcDecision.Id, NULL可 | 紐付き決定事項（任意） |
| Description | string | NOT NULL, max:500 | アクション内容 |
| AssigneeId | Guid | FK → User.Id, NOT NULL | 担当者 |
| DueDate | DateOnly? | NULL可 | 期限（未定の場合 null） |
| Status | ActionItemStatus (enum) | NOT NULL, Default: NotStarted | ステータス |
| CompletedAt | DateTimeOffset? | NULL可 | 完了日時 |
| CreatedAt | DateTimeOffset | NOT NULL | |
| UpdatedAt | DateTimeOffset | NOT NULL | |

**ActionItemStatus 列挙値:**
| 値 | 名前 | 説明 |
|----|------|------|
| 0 | NotStarted | 未着手 |
| 1 | InProgress | 進行中 |
| 2 | Done | 完了 |
| 3 | Cancelled | キャンセル |

**インデックス:**
- `(SessionId)` — セッション別一覧取得用
- `(AssigneeId, Status)` — 担当者別・ステータス別検索用
- `(DecisionId)` — 決定事項別一覧取得用

**リレーション:**
- `SessionId` → `CommitteeSession.Id` (OnDelete: Cascade)
- `DecisionId` → `MdcDecision.Id` (OnDelete: SetNull)
- `AssigneeId` → `User.Id` (OnDelete: Restrict)

---

## 2. API設計

### 2-1. GET /api/mdc-sessions/{sessionId}/decisions — 決定事項一覧取得

**認可:** Senior / Admin / Executive（Manager は配下関連のみ — 今回は Senior 以上に限定）

**レスポンス（200 OK）:**
```json
[
  {
    "id": "uuid",
    "sessionId": "uuid",
    "decisionType": "KeyPersonAssign",
    "content": "Q3から集中育成対象に指定",
    "decidedAt": "2026-05-23",
    "createdBy": "uuid",
    "createdAt": "2026-05-23T10:00:00Z",
    "updatedAt": "2026-05-23T10:00:00Z",
    "talents": [
      { "talentId": "uuid", "talentName": "田中 太郎" }
    ],
    "actionItems": [
      { "id": "uuid", "description": "...", "status": "NotStarted", "assigneeId": "uuid", "assigneeName": "...", "dueDate": "2026-06-30" }
    ]
  }
]
```

**エラーレスポンス:**
| ステータス | 条件 |
|-----------|------|
| 403 | 権限なし |
| 404 | sessionId が存在しない |

---

### 2-2. POST /api/mdc-sessions/{sessionId}/decisions — 決定事項作成

**認可:** Senior / Admin / Executive

**リクエスト:**
```json
{
  "decisionType": "KeyPersonAssign",
  "content": "Q3から集中育成対象に指定",
  "decidedAt": "2026-05-23",
  "talentIds": ["uuid1", "uuid2"]
}
```

**レスポンス（201 Created）:**
```json
{
  "id": "uuid",
  "sessionId": "uuid",
  "decisionType": "KeyPersonAssign",
  "content": "Q3から集中育成対象に指定",
  "decidedAt": "2026-05-23",
  "createdBy": "uuid",
  "createdAt": "2026-05-23T10:00:00Z",
  "updatedAt": "2026-05-23T10:00:00Z",
  "talents": [],
  "actionItems": []
}
```

**エラーレスポンス:**
| ステータス | 条件 |
|-----------|------|
| 400 | バリデーションエラー（content 空、talentId 不正等） |
| 403 | 権限なし |
| 404 | sessionId が存在しない |

---

### 2-3. PUT /api/mdc-decisions/{id} — 決定事項更新

**認可:** Senior / Admin / Executive

**リクエスト:**
```json
{
  "decisionType": "DevelopmentPolicyChange",
  "content": "育成方針を変更",
  "decidedAt": "2026-05-23",
  "talentIds": ["uuid1"]
}
```

**レスポンス（200 OK）:** 更新後の `MdcDecisionDto`

**エラーレスポンス:**
| ステータス | 条件 |
|-----------|------|
| 400 | バリデーションエラー |
| 403 | 権限なし |
| 404 | id が存在しない |

---

### 2-4. POST /api/mdc-decisions/{decisionId}/action-items — アクションアイテム追加

**認可:** Senior / Admin / Executive

**リクエスト:**
```json
{
  "description": "田中さんとの月次1on1を設定する",
  "assigneeId": "uuid",
  "dueDate": "2026-06-30"
}
```

**レスポンス（201 Created）:**
```json
{
  "id": "uuid",
  "sessionId": "uuid",
  "decisionId": "uuid",
  "description": "田中さんとの月次1on1を設定する",
  "assigneeId": "uuid",
  "assigneeName": "鈴木 二郎",
  "dueDate": "2026-06-30",
  "status": "NotStarted",
  "completedAt": null,
  "createdAt": "2026-05-23T10:00:00Z",
  "updatedAt": "2026-05-23T10:00:00Z"
}
```

**エラーレスポンス:**
| ステータス | 条件 |
|-----------|------|
| 400 | バリデーションエラー（description 空等） |
| 403 | 権限なし |
| 404 | decisionId が存在しない |

---

### 2-5. GET /api/mdc-sessions/{sessionId}/action-items — セッション内アクションアイテム一覧

**認可:** Senior / Admin / Executive / 担当者本人（Manager）

**クエリパラメータ:** `?status=NotStarted` （省略時は全件）

**レスポンス（200 OK）:** `MdcActionItemDto[]`（期限昇順）

---

### 2-6. PATCH /api/mdc-action-items/{id}/status — ステータス更新

**認可:** Senior / Admin / Executive / 担当者本人

**リクエスト:**
```json
{ "status": "Done" }
```

**レスポンス（200 OK）:** 更新後の `MdcActionItemDto`

**ロジック:**
- `status = Done` → `CompletedAt = DateTimeOffset.UtcNow`
- `status = Cancelled` → `CompletedAt = DateTimeOffset.UtcNow`
- その他 → `CompletedAt = null`

**エラーレスポンス:**
| ステータス | 条件 |
|-----------|------|
| 403 | 権限なし（担当者でも Senior でもない） |
| 404 | id が存在しない |

---

### 2-7. GET /api/talents/{talentId}/mdc-decisions — タレント別決定履歴

**認可:** Senior / Admin / Executive / Manager（配下タレントのみ）

**レスポンス（200 OK）:** `MdcDecisionDto[]`（decidedAt 降順）

---

### 2-8. GET /api/users/{userId}/action-items — 担当者別アクションアイテム一覧

**認可:** Senior / Admin / 担当者本人

**クエリパラメータ:** `?status=pending`（pending = NotStarted または InProgress）

**レスポンス（200 OK）:** `MdcActionItemDto[]`（dueDate 昇順、null は末尾）

---

## 3. フロントエンド設計

### 3-1. ページ・コンポーネント構成

```
pages/
  mdc/
    sessions/
      [sessionId].vue        # 既存。決定事項・AIセクションを追加拡張

components/
  mdc/
    MdcDecisionList.vue      # 決定事項一覧＋追加ボタン
    MdcDecisionForm.vue      # 決定事項入力フォーム（モーダル内）
    MdcActionItemList.vue    # アクションアイテム一覧（ステータス更新機能付き）
    MdcActionItemForm.vue    # アクションアイテム入力フォーム
    MdcCarryoverSection.vue  # 前回積み残しセクション
  talent-profile/
    TalentProfileMdcDecisions.vue  # タレントプロファイル内の決定履歴セクション（追加）
```

### 3-2. Pinia ストア設計

```typescript
// stores/mdcDecision.ts
const decisions = ref<MdcDecisionDto[]>([])
const actionItems = ref<MdcActionItemDto[]>([])
const carryoverItems = ref<MdcActionItemDto[]>([])
const loading = ref(false)
const saving = ref(false)

async function fetchDecisions(sessionId: string): Promise<void>
async function createDecision(sessionId: string, data: CreateDecisionRequest): Promise<void>
async function updateDecision(id: string, data: UpdateDecisionRequest): Promise<void>
async function fetchActionItems(sessionId: string): Promise<void>
async function addActionItem(decisionId: string, data: CreateActionItemRequest): Promise<void>
async function updateActionItemStatus(id: string, status: ActionItemStatus): Promise<void>
async function fetchCarryoverItems(sessionId: string): Promise<void>
```

### 3-3. data-testid 定義

| 要素 | data-testid | AC |
|------|-------------|-----|
| 決定事項一覧ラッパー | `mdc-decision-list` | AC-01-01 |
| 決定事項追加ボタン | `mdc-decision-add-button` | AC-01-01 |
| 決定タイプ選択 | `mdc-decision-type-select` | AC-01-01 |
| 決定内容入力 | `mdc-decision-content-input` | AC-01-01 |
| 対象タレント選択 | `mdc-decision-talent-select` | AC-01-02 |
| 決定事項保存ボタン | `mdc-decision-save-button` | AC-01-01 |
| アクションアイテム一覧 | `mdc-action-item-list` | AC-02-01 |
| アクションアイテム追加ボタン | `mdc-action-item-add-button` | AC-02-01 |
| AI内容入力 | `mdc-action-item-description-input` | AC-02-01 |
| AI担当者選択 | `mdc-action-item-assignee-select` | AC-02-01 |
| AI期限入力 | `mdc-action-item-due-date-input` | AC-02-01 |
| AI保存ボタン | `mdc-action-item-save-button` | AC-02-01 |
| AIステータス更新（個別） | `mdc-action-item-status-{id}` | AC-02-02 |
| 期限超過バッジ | `mdc-overdue-badge` | AC-02-03 |
| 前回積み残しセクション | `mdc-carryover-section` | AC-03-01 |
| 積み残し件数バッジ | `mdc-carryover-count` | AC-03-01 |
| 積み残し完了ボタン（個別） | `mdc-carryover-done-{id}` | AC-03-02 |
| 積み残しキャンセルボタン（個別） | `mdc-carryover-cancel-{id}` | AC-03-02 |
| タレントプロファイル MDC決定履歴 | `talent-profile-mdc-decisions-section` | AC-04-01 |

---

## 4. 技術的判断の記録

| 決定事項 | 採用した選択 | 理由 | 不採用の選択肢 |
|---------|------------|------|--------------|
| ID 型 | Guid（既存コードに統一） | 仕様書は ULID と記載しているが、既存の `CommitteeSession` が Guid を使用しており混在を避ける | ULID — 導入コストに見合うメリットが現時点では少ない |
| MdcDecision と CommitteeSession の紐付け | FK（Cascade Delete） | セッション削除時に決定事項も合わせて削除するのが自然 | 論理削除 — 5年間保持要件はアーカイブで対応可 |
| MdcActionItem の DecisionId | NULL 可 | 仕様書に「セッション直接紐付けも可」と明記されているため | 必須 FK — 意思決定なしのアクション（例：前回引き継ぎ時の新規追加）を考慮 |
| 前回積み残し取得方法 | API でセッションの前回セッションを特定し未完了 AI を返す | フロントエンドの実装をシンプルに保てる | フロントエンドで全セッションを取得してフィルタ — 通信コスト増 |
| REQ-06（エクスポート）の実装 | 今回スコープ外（Could 要件） | TASK-057〜060 の SP が既に 10 あり、優先度の低い Could 要件を後回しにする | 実装 — スプリントの余力次第で次スプリントに回す |
| タレント別決定履歴の表示場所 | `TalentProfileMdcDecisions.vue` コンポーネントを新規追加し、タレントプロファイルページに組み込む | SPEC-MDC-003 のプロファイルページを拡張するのが最小変更 | 専用ページを新設 — ナビゲーションが増えUX悪化 |

---

## 5. 受入基準との対応確認

| AC ID | 受入基準概要 | 対応する設計箇所 |
|-------|------------|---------------|
| AC-01-01 | 決定事項の登録 | POST /api/mdc-sessions/{id}/decisions + `mdc-decision-save-button` |
| AC-01-02 | 複数タレントへの関連付け | MdcDecisionTalent 中間テーブル + `mdc-decision-talent-select` |
| AC-02-01 | アクションアイテムの追加 | POST /api/mdc-decisions/{id}/action-items + `mdc-action-item-save-button` |
| AC-02-02 | ステータス更新 | PATCH /api/mdc-action-items/{id}/status + `mdc-action-item-status-{id}` |
| AC-02-03 | 期限超過の警告表示 | フロントで `dueDate < today && status != Done/Cancelled` を判定 + `mdc-overdue-badge` |
| AC-03-01 | 前回積み残し自動表示 | `MdcCarryoverSection.vue` + GET /api/mdc-sessions/{sessionId}/action-items（前セッション） |
| AC-03-02 | 積み残しの完了/キャンセル操作 | PATCH /api/mdc-action-items/{id}/status + `mdc-carryover-done-{id}` / `mdc-carryover-cancel-{id}` |
| AC-04-01 | タレント別決定履歴表示 | GET /api/talents/{id}/mdc-decisions + `TalentProfileMdcDecisions.vue` |
| AC-05-01 | 担当者別未完了AI表示 | GET /api/users/{id}/action-items?status=pending（既存ダッシュボードに追加） |
