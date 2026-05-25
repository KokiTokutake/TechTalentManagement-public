# 後継者計画（サクセッションプラン）設計書

| 項目 | 内容 |
|------|------|
| Doc ID | DESIGN-SPEC-MDC-005 |
| 対応仕様書 | SPEC-MDC-005 |
| ステータス | approved |
| 作成日 | 2026-05-25 |
| 作成者 | @KokiTokutake |

---

## 1. エンティティ設計

### 1-1. CriticalPosition（新規）

重要ポジションマスタ。組織内の引き継ぎリスクが高い役割を管理する。

| プロパティ | 型 | 制約 | 説明 |
|-----------|-----|------|------|
| Id | Guid | PK | |
| PositionName | string | NOT NULL, max:200 | ポジション名 |
| CurrentHolderId | Guid? | FK → User.Id, NULL可 | 現担当者（空席時 null） |
| Importance | PositionImportance (enum) | NOT NULL | 重要度 |
| RequiredSkills | string? | max:2000, NULL可 | 要求スキル・コンピテンシー（自由記述） |
| IsActive | bool | NOT NULL, Default: true | 有効フラグ |
| CreatedBy | Guid | FK → User.Id, NOT NULL | 作成者 |
| CreatedAt | DateTimeOffset | NOT NULL | |
| UpdatedAt | DateTimeOffset | NOT NULL | |

**PositionImportance 列挙値:**
| 値 | 名前 | 説明 |
|----|------|------|
| 1 | Critical | 最重要（即座の代替困難） |
| 2 | High | 高重要度 |
| 3 | Medium | 中重要度 |

**インデックス:**
- `(Importance, IsActive)` — 重要度順一覧取得用
- `(CurrentHolderId)` — 現担当者検索用

**リレーション:**
- `CurrentHolderId` → `User.Id` (OnDelete: SetNull)
- `CreatedBy` → `User.Id` (OnDelete: Restrict)

---

### 1-2. SuccessionCandidate（新規）

後継者候補。ポジションごとに複数名登録可能。

| プロパティ | 型 | 制約 | 説明 |
|-----------|-----|------|------|
| Id | Guid | PK | |
| PositionId | Guid | FK → CriticalPosition.Id, NOT NULL | 対象ポジション |
| TalentId | Guid | FK → User.Id, NOT NULL | 候補タレント |
| Readiness | ReadinessLevel (enum) | NOT NULL | 準備度 |
| DevelopmentGap | string? | max:1000, NULL可 | 育成ギャップ |
| DevelopmentActions | string? | max:2000, NULL可 | 育成アクション（サマリー） |
| NominatedBy | Guid | FK → User.Id, NOT NULL | 推薦者 |
| NominatedAt | DateTimeOffset | NOT NULL | 推薦日時 |
| UpdatedAt | DateTimeOffset | NOT NULL | 最終更新日時（Readiness変更時に更新） |

**ReadinessLevel 列挙値:**
| 値 | 名前 | 説明 |
|----|------|------|
| 1 | ReadyNow | 今すぐ引き継ぎ可能（6ヶ月以内） |
| 2 | ReadySoon | 1〜2年で準備完了見込み |
| 3 | ReadyMidTerm | 3〜5年で準備完了見込み |

**インデックス:**
- `(PositionId, Readiness)` — ポジション別・Readiness別一覧取得用
- `(TalentId)` — タレントが候補のポジション検索用
- `(PositionId, TalentId)` — UNIQUE（同一ポジションへの重複登録防止）

**リレーション:**
- `PositionId` → `CriticalPosition.Id` (OnDelete: Cascade)
- `TalentId` → `User.Id` (OnDelete: Restrict)
- `NominatedBy` → `User.Id` (OnDelete: Restrict)

---

### 1-3. SuccessionDevelopmentAction（新規）

後継者候補の個別育成アクション（REQ-05: Could）。

| プロパティ | 型 | 制約 | 説明 |
|-----------|-----|------|------|
| Id | Guid | PK | |
| CandidateId | Guid | FK → SuccessionCandidate.Id, NOT NULL | 対象候補者 |
| Action | string | NOT NULL, max:500 | アクション内容 |
| Status | ActionStatus (enum) | NOT NULL, Default: NotStarted | 進捗ステータス |
| DueDate | DateOnly? | NULL可 | 期限 |
| CompletedAt | DateTimeOffset? | NULL可 | 完了日時 |
| CreatedAt | DateTimeOffset | NOT NULL | |
| UpdatedAt | DateTimeOffset | NOT NULL | |

**ActionStatus 列挙値:**
| 値 | 名前 | 説明 |
|----|------|------|
| 0 | NotStarted | 未着手 |
| 1 | InProgress | 進行中 |
| 2 | Done | 完了 |

> 既存の `MdcActionItem` の `ActionItemStatus` と類似するが、サクセッション専用のシンプルな3値とする。既存 enum を共用すると Cancelled が混入するため独立させる。

**インデックス:**
- `(CandidateId, Status)` — 候補者別・ステータス別一覧取得用

**リレーション:**
- `CandidateId` → `SuccessionCandidate.Id` (OnDelete: Cascade)

---

## 2. API設計

### 2-1. GET /api/critical-positions — 重要ポジション一覧

**認可:** Senior / Executive / Admin

**クエリパラメータ:**
| パラメータ | 型 | 説明 |
|-----------|-----|------|
| requesterId | Guid | 操作者ID（認可確認用） |
| includeInactive | bool | デフォルト false（false時は isActive=true のみ返す） |

**レスポンス（200 OK）:**
```json
{
  "positions": [
    {
      "id": "uuid",
      "positionName": "バックエンドテックリード",
      "currentHolder": { "id": "uuid", "displayName": "山田花子" },
      "importance": "Critical",
      "requiredSkills": "アーキテクチャ設計、チームリード経験",
      "isActive": true,
      "candidateCount": 3,
      "hasReadyNowCandidate": true,
      "createdAt": "2026-05-25T10:00:00Z",
      "updatedAt": "2026-05-25T10:00:00Z"
    }
  ]
}
```

**ソート:** Importance 昇順（Critical → High → Medium）、同一 Importance 内は PositionName 昇順

**エラーレスポンス:**
| ステータス | 条件 |
|-----------|------|
| 403 | requesterId が Senior / Executive / Admin 以外 |

---

### 2-2. POST /api/critical-positions — ポジション登録

**認可:** Senior / Executive / Admin

**リクエストボディ:**
```json
{
  "positionName": "バックエンドテックリード",
  "currentHolderId": "uuid または null",
  "importance": "Critical",
  "requiredSkills": "アーキテクチャ設計、チームリード経験"
}
```

**ヘッダー:** `X-User-Id: {requesterId}`

**レスポンス（201 Created）:**
```json
{
  "id": "uuid",
  "positionName": "バックエンドテックリード",
  "importance": "Critical",
  "createdAt": "2026-05-25T10:00:00Z"
}
```

**エラーレスポンス:**
| ステータス | 条件 |
|-----------|------|
| 400 | positionName が空または200文字超 |
| 403 | requesterId が Senior / Executive / Admin 以外 |

---

### 2-3. PUT /api/critical-positions/{id} — ポジション更新

**認可:** Senior / Executive / Admin

**リクエストボディ:**
```json
{
  "positionName": "バックエンドテックリード",
  "currentHolderId": "uuid または null",
  "importance": "High",
  "requiredSkills": "...",
  "isActive": true
}
```

**ヘッダー:** `X-User-Id: {requesterId}`

**レスポンス（200 OK）:** 更新後のポジションオブジェクト

**エラーレスポンス:**
| ステータス | 条件 |
|-----------|------|
| 403 | 権限なし |
| 404 | ポジション未存在 |

---

### 2-4. GET /api/critical-positions/{positionId}/candidates — 候補者一覧

**認可:** Senior / Executive / Admin

**クエリパラメータ:** `requesterId: Guid`

**レスポンス（200 OK）:**
```json
{
  "positionId": "uuid",
  "positionName": "バックエンドテックリード",
  "candidates": [
    {
      "id": "uuid",
      "talent": { "id": "uuid", "displayName": "田中太郎" },
      "readiness": "ReadySoon",
      "developmentGap": "アーキテクチャ設計経験が不足",
      "developmentActions": "社外勉強会参加、設計レビュー担当",
      "nominatedBy": { "id": "uuid", "displayName": "山田花子" },
      "nominatedAt": "2026-05-25T10:00:00Z",
      "updatedAt": "2026-05-25T10:00:00Z",
      "developmentActionItems": [
        {
          "id": "uuid",
          "action": "アーキテクチャ設計書のレビュー担当",
          "status": "InProgress",
          "dueDate": "2026-09-30",
          "completedAt": null
        }
      ]
    }
  ]
}
```

---

### 2-5. POST /api/critical-positions/{positionId}/candidates — 候補者追加

**認可:** Senior / Executive / Admin

**リクエストボディ:**
```json
{
  "talentId": "uuid",
  "readiness": "ReadySoon",
  "developmentGap": "アーキテクチャ設計経験が不足",
  "developmentActions": "社外勉強会参加"
}
```

**ヘッダー:** `X-User-Id: {nominatedBy}`

**レスポンス（201 Created）:** 作成した候補者オブジェクト

**エラーレスポンス:**
| ステータス | 条件 |
|-----------|------|
| 400 | talentId / readiness が不正 |
| 403 | 権限なし |
| 404 | positionId が存在しない |
| 409 | 同一 positionId + talentId が既に存在する |

---

### 2-6. PUT /api/succession-candidates/{id} — Readiness/ギャップ更新

**認可:** Senior / Executive / Admin

**リクエストボディ:**
```json
{
  "readiness": "ReadyNow",
  "developmentGap": "概ね解消",
  "developmentActions": "引き継ぎ計画の策定"
}
```

**ヘッダー:** `X-User-Id: {requesterId}`

**レスポンス（200 OK）:** 更新後の候補者オブジェクト

---

### 2-7. DELETE /api/succession-candidates/{id} — 候補者除外

**認可:** Senior / Executive / Admin

**クエリパラメータ:** `requesterId: Guid`

**レスポンス（204 No Content）**

---

### 2-8. GET /api/succession-map — サクセッションマップデータ

**認可:** Senior / Executive / Admin

**クエリパラメータ:** `requesterId: Guid`

**レスポンス（200 OK）:**
```json
{
  "positions": [
    {
      "id": "uuid",
      "positionName": "バックエンドテックリード",
      "importance": "Critical",
      "currentHolder": { "id": "uuid", "displayName": "山田花子" },
      "readyNow": [
        { "id": "uuid", "displayName": "田中太郎", "candidateId": "uuid" }
      ],
      "readySoon": [
        { "id": "uuid", "displayName": "鈴木次郎", "candidateId": "uuid" }
      ],
      "readyMidTerm": [],
      "hasReadyNowGap": false
    }
  ]
}
```

> `hasReadyNowGap`: importance が Critical かつ readyNow が空の場合 true（AC-04-02 警告バッジ用）

---

### 2-9. GET /api/succession-map/suggest — 9ボックスHighPotential候補サジェスト

**認可:** Senior / Executive / Admin

**クエリパラメータ:**
| パラメータ | 型 | 説明 |
|-----------|-----|------|
| requesterId | Guid | 操作者ID |
| periodId | Guid | 考課期間ID |
| positionId | Guid | 除外済み候補を省くための対象ポジション |

**レスポンス（200 OK）:**
```json
{
  "suggestions": [
    {
      "id": "uuid",
      "displayName": "田中太郎",
      "potentialLevel": "High",
      "performanceLevel": "High",
      "alreadyCandidate": false
    }
  ]
}
```

> `potentialLevel` が High または Mid のタレントを返す。`alreadyCandidate: true` のものも返すが UI でグレーアウト表示。

---

### 2-10. GET /api/talents/{talentId}/succession-candidacies — タレントが候補のポジション一覧

**認可:** Senior / Executive / Admin

**クエリパラメータ:** `requesterId: Guid`

**レスポンス（200 OK）:**
```json
{
  "candidacies": [
    {
      "positionId": "uuid",
      "positionName": "バックエンドテックリード",
      "importance": "Critical",
      "readiness": "ReadySoon",
      "nominatedAt": "2026-05-25T10:00:00Z"
    }
  ]
}
```

---

### 2-11. POST /api/succession-candidates/{id}/development-actions — 育成アクション追加

**認可:** Senior / Executive / Admin

**リクエストボディ:**
```json
{
  "action": "アーキテクチャ設計書のレビュー担当",
  "dueDate": "2026-09-30"
}
```

**ヘッダー:** `X-User-Id: {requesterId}`

**レスポンス（201 Created）:** 作成した育成アクションオブジェクト

---

### 2-12. PUT /api/succession-development-actions/{id} — 育成アクション進捗更新

**認可:** Senior / Executive / Admin

**リクエストボディ:**
```json
{
  "status": "Done",
  "completedAt": "2026-09-15T10:00:00Z"
}
```

**レスポンス（200 OK）:** 更新後の育成アクションオブジェクト

---

## 3. フロントエンド設計

### 3-1. ページ・コンポーネント構成

```
pages/
  mdc/
    succession/
      index.vue           # サクセッションマップ（REQ-04）
      positions.vue       # 重要ポジション管理（REQ-01）
      [positionId].vue    # ポジション詳細・候補者管理（REQ-02）

components/
  succession/
    SuccessionMap.vue              # マップグリッド本体
    SuccessionMapCell.vue          # 各セル（Readiness × Position）
    CandidateBadge.vue             # 候補者バッジ
    AddCandidateDialog.vue         # 候補者追加ダイアログ（サジェスト含む）
    DevelopmentActionList.vue      # 育成アクション一覧
```

### 3-2. ルーティング

| パス | ページ | 説明 |
|------|--------|------|
| `/mdc/succession` | index.vue | サクセッションマップ |
| `/mdc/succession/positions` | positions.vue | 重要ポジション管理 |
| `/mdc/succession/positions/[positionId]` | [positionId].vue | ポジション詳細 |

### 3-3. 状態管理ストア設計

```typescript
// stores/succession.ts
interface SuccessionState {
  positions: CriticalPosition[]
  mapData: SuccessionMapResponse | null
  loading: boolean
  error: string | null
}
// actions: fetchPositions, fetchMap, createPosition, updatePosition
// addCandidate, updateCandidate, removeCandidate
// fetchSuggestions, addDevelopmentAction, updateDevelopmentAction
```

### 3-4. data-testid 定義

| 要素 | data-testid | 用途 |
|------|-------------|------|
| ポジション追加ボタン | `position-add-button` | ポジション管理ページ |
| ポジション名入力 | `position-name-input` | ポジション追加フォーム |
| 重要度セレクト | `position-importance-select` | ポジション追加フォーム |
| ポジション保存ボタン | `position-save-button` | ポジション追加フォーム |
| ポジション一覧行 | `position-row-{id}` | ポジション一覧 |
| 候補者追加ボタン | `candidate-add-button` | ポジション詳細 |
| タレント検索入力 | `candidate-talent-select` | 候補者追加ダイアログ |
| Readinessセレクト | `candidate-readiness-select` | 候補者追加ダイアログ |
| 候補者追加確定ボタン | `candidate-confirm-button` | 候補者追加ダイアログ |
| 推薦候補を表示ボタン | `candidate-suggest-button` | 候補者追加ダイアログ |
| サジェストリスト | `candidate-suggest-list` | 9ボックス連動リスト |
| Readiness更新ボタン | `candidate-readiness-save-button` | ポジション詳細 |
| サクセッションマップ | `succession-map` | マップ全体コンテナ |
| マップセル | `map-cell-{positionId}-{readiness}` | マップグリッドセル |
| 後継者未定バッジ | `map-gap-badge-{positionId}` | Critical Ready Now 空警告 |

---

## 4. 技術的判断の記録

| 決定事項 | 採用した選択 | 理由 | 不採用の選択肢 |
|---------|------------|------|--------------|
| ActionStatus の扱い | 専用 enum `ActionStatus` を新設（NotStarted/InProgress/Done 3値） | `MdcActionItem.ActionItemStatus` は Cancelled を含む4値で意味合いが異なる | 既存 enum 共用 |
| サジェスト API | 専用エンドポイント `GET /api/succession-map/suggest` | 9ボックスデータと候補追加済み判定を一括返却するため独立した方が効率的 | クライアント側で組み合わせ |
| hasReadyNowGap フラグ | サーバー側で計算してレスポンスに含める | AC-04-02 の警告ロジックをサーバーに閉じ込め、クライアントの条件分岐を削減 | クライアント側で計算 |
| 候補者の上限 | 制限なし | 仕様書 AC-02-02 に「上限制限なし」と明記 | 上限設定 |
| マップページのルート | `/mdc/succession` をサクセッションマップ、`/mdc/succession/positions` をポジション管理 | マップがメインユースケース（REQ-04: Should）のため最短パスに配置 | ポジション管理をトップに配置 |

---

## 5. 受入基準マッピング

| AC ID | 対応する設計要素 |
|-------|----------------|
| AC-01-01 | POST /api/critical-positions → `CriticalPosition` 作成 |
| AC-01-02 | GET /api/critical-positions → Importance 昇順ソート |
| AC-02-01 | POST /api/critical-positions/{positionId}/candidates → `SuccessionCandidate` 作成 |
| AC-02-02 | 同エンドポイント・UNIQUE制約は positionId+talentId のみ（件数制限なし） |
| AC-02-03 | PUT /api/succession-candidates/{id} → Readiness 更新、UpdatedAt 記録 |
| AC-03-01 | GET /api/succession-map/suggest → PotentialLevel High/Mid タレント返却 |
| AC-04-01 | GET /api/succession-map → positions[].readyNow / readySoon / readyMidTerm グリッド |
| AC-04-02 | GET /api/succession-map → hasReadyNowGap フラグ → `map-gap-badge-{positionId}` |
