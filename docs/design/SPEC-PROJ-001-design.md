# プロジェクト・アサインメント履歴管理 設計書

| 項目 | 内容 |
|------|------|
| Doc ID | DESIGN-SPEC-PROJ-001 |
| 対応仕様書 | SPEC-PROJ-001 |
| ステータス | approved |
| 作成日 | 2026-05-26 |
| 作成者 | @KokiTokutake |

---

## 1. エンティティ設計

### ProjectAssignment

| プロパティ | 型 | 制約 | 説明 |
|-----------|-----|------|------|
| Id | Guid | PK | |
| TalentId | Guid | FK → User.Id, NOT NULL | 対象タレント |
| ProjectName | string | NOT NULL, max:200 | プロジェクト名 |
| Role | string | NOT NULL, max:100 | 役割（例: バックエンドエンジニア、テックリード） |
| StartDate | DateOnly | NOT NULL | 参画開始日 |
| EndDate | DateOnly? | NULL | 参画終了日（null = 現在も参画中） |
| Description | string? | NULL, max:1000 | プロジェクト概要・担当内容 |
| ManagerComment | string? | NULL, max:500 | 上長コメント（REQ-08） |
| ManagerCommentedById | Guid? | FK → User.Id, NULL | コメントした上長 |
| ManagerCommentedAt | DateTimeOffset? | NULL | コメント日時 |
| CreatedAt | DateTimeOffset | NOT NULL | |
| UpdatedAt | DateTimeOffset | NOT NULL | |

**インデックス:**
- `(TalentId, StartDate DESC)` — 一覧取得・並び順用
- `(TalentId, EndDate)` — 参画中フィルタ用

**リレーション:**
- `TalentId` → `User.Id` (OnDelete: Restrict)
- `ManagerCommentedById` → `User.Id` (OnDelete: SetNull)

**ソート仕様:**
- `EndDate IS NULL` のレコードを先頭に（参画中を優先）
- 同一 EndDate グループ内は `StartDate DESC`

---

### ProjectAssignmentSkill（中間テーブル）

| プロパティ | 型 | 制約 | 説明 |
|-----------|-----|------|------|
| Id | Guid | PK | |
| AssignmentId | Guid | FK → ProjectAssignment.Id, NOT NULL | |
| SkillItemId | Guid | FK → SkillItem.Id, NOT NULL | SPEC-SKILL-001 のスキルアイテム |

**インデックス:**
- `(AssignmentId, SkillItemId)` — UNIQUE（重複紐付け防止）

**リレーション:**
- `AssignmentId` → `ProjectAssignment.Id` (OnDelete: Cascade)
- `SkillItemId` → `SkillItem.Id` (OnDelete: Restrict)

---

## 2. API設計

### GET /api/project-assignments

タレントのプロジェクト履歴一覧を取得する。

**クエリパラメータ:**
| パラメータ | 型 | 必須 | 説明 |
|-----------|-----|------|------|
| talentId | Guid | ✓ | 対象タレントの UserId |

**レスポンス（200 OK）:**
```json
{
  "assignments": [
    {
      "id": "guid",
      "projectName": "次世代バンキングシステム",
      "role": "バックエンドテックリード",
      "startDate": "2024-04-01",
      "endDate": null,
      "isActive": true,
      "description": "マイクロサービス基盤の設計・実装リード",
      "managerComment": "アーキテクチャ設計で高い貢献",
      "skills": [
        { "skillItemId": "guid", "skillName": "Java", "categoryName": "バックエンド" }
      ],
      "createdAt": "2026-05-01T00:00:00Z",
      "updatedAt": "2026-05-01T00:00:00Z"
    }
  ]
}
```

**認可ルール:**
| ロール | 操作 |
|--------|------|
| Talent（本人） | ✅ 参照可 |
| Manager（部下の場合） | ✅ 参照可 |
| Senior / Executive / Admin | ✅ 参照可 |
| Talent（他人） | ❌ 403 |

**エラーレスポンス:**
| ステータス | 条件 |
|-----------|------|
| 400 | talentId 未指定 |
| 403 | 参照権限なし |

---

### POST /api/project-assignments

プロジェクト履歴を新規登録する。

**リクエストヘッダー:** `X-User-Id: {talentId}`

**リクエストボディ:**
```json
{
  "projectName": "次世代バンキングシステム",
  "role": "バックエンドテックリード",
  "startDate": "2024-04-01",
  "endDate": null,
  "description": "マイクロサービス基盤の設計・実装リード",
  "skillItemIds": ["guid1", "guid2"]
}
```

**レスポンス（201 Created）:**
```json
{ "id": "guid" }
```

**バリデーション:**
- `projectName`: 必須、max:200
- `role`: 必須、max:100
- `startDate`: 必須、未来日付不可（今日以前）
- `endDate`: nullable、指定する場合は `startDate` 以降
- `skillItemIds`: 存在する SkillItem.Id のみ許可

**認可:** リクエストユーザーのみ自分の履歴を登録可

---

### PUT /api/project-assignments/{id}

プロジェクト履歴を更新する。

**リクエストボディ:** POST と同じ構造

**レスポンス（200 OK）:**
```json
{ "id": "guid" }
```

**エラーレスポンス:**
| ステータス | 条件 |
|-----------|------|
| 403 | 本人以外が更新しようとした |
| 404 | 対象レコードが存在しない |

---

### DELETE /api/project-assignments/{id}

プロジェクト履歴を物理削除する。

**レスポンス:** 204 No Content

**エラーレスポンス:**
| ステータス | 条件 |
|-----------|------|
| 403 | 本人以外が削除しようとした |
| 404 | 対象レコードが存在しない |

---

### PUT /api/project-assignments/{id}/manager-comment

上長がプロジェクト履歴にコメントを追加・更新する。

**リクエストボディ:**
```json
{
  "comment": "アーキテクチャ設計において主体的にリードし高い貢献を発揮した"
}
```

**レスポンス（200 OK）:**
```json
{ "id": "guid" }
```

**認可:** リクエストユーザーが対象タレントの上長（Manager ロール）であること

**エラーレスポンス:**
| ステータス | 条件 |
|-----------|------|
| 403 | 上長以外が操作しようとした |
| 404 | 対象レコードが存在しない |

---

## 3. フロントエンド設計

### コンポーネント構成

```
pages/
  profile/
    projects.vue          # プロジェクト履歴管理ページ（本人用）

components/
  project/
    ProjectCard.vue       # プロジェクト1件の表示カード
    ProjectFormDialog.vue # 追加・編集ダイアログ
```

**タレントプロファイル統合:**
```
pages/
  mdc/
    talent/
      [id].vue            # 既存ページにプロジェクト履歴セクションを追加
```

---

### Pinia ストア設計

```typescript
// stores/project.ts
interface ProjectState {
  assignments: ProjectAssignmentDto[]
  loading: boolean
  error: string | null
}

// actions
fetchAssignments(talentId: string): Promise<void>
createAssignment(payload: CreateAssignmentRequest): Promise<void>
updateAssignment(id: string, payload: UpdateAssignmentRequest): Promise<void>
deleteAssignment(id: string): Promise<void>
saveManagerComment(id: string, comment: string): Promise<void>
$reset(): void
```

---

### data-testid 定義

| 要素 | data-testid | 用途 |
|------|-------------|------|
| プロジェクト追加ボタン | `project-add-button` | E2Eテスト |
| プロジェクト名入力 | `project-name-input` | E2Eテスト |
| 役割入力 | `project-role-input` | E2Eテスト |
| 開始日入力 | `project-start-date-input` | E2Eテスト |
| 終了日入力 | `project-end-date-input` | E2Eテスト |
| 使用技術セレクト | `project-skill-select` | E2Eテスト |
| 保存ボタン | `project-save-button` | E2Eテスト |
| プロジェクト行 | `project-row-{id}` | E2Eテスト |
| 参画中ラベル | `project-active-badge` | E2Eテスト |
| 削除ボタン | `project-delete-button` | E2Eテスト |
| 削除確認ボタン | `project-delete-confirm-button` | E2Eテスト |
| 上長コメント入力 | `project-manager-comment-input` | E2Eテスト |
| 上長コメント保存 | `project-manager-comment-save-button` | E2Eテスト |

---

## 4. 技術的判断の記録

| 決定事項 | 採用した選択 | 理由 | 不採用の選択肢 |
|---------|------------|------|--------------|
| 削除方式 | 物理削除 | プロジェクト履歴は機密性が低く、論理削除のオーバーヘッドが不要 | 論理削除（他エンティティとの一貫性） |
| SkillItem 紐付け | 別テーブル（ProjectAssignmentSkill） | 複数スキルの紐付け・スキルマップ連携（REQ-07）のため正規化が必要 | JSON 配列カラム |
| ソート | EndDate IS NULL を先頭、その後 StartDate DESC | 「参画中」プロジェクトが最も重要な情報であるため | 単純な StartDate DESC |
| 上長コメント | ProjectAssignment 本体に ManagerComment カラム | コメントは1件のみで履歴不要。別テーブルより JOIN コストが低い | 別テーブルでコメント履歴管理 |

---
