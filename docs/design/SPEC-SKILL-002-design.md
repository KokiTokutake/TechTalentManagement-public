# スキルレベル定義拡充（ITSS準拠）・コンピテンシー評価追加 設計書

| 項目 | 内容 |
|------|------|
| Doc ID | DESIGN-SPEC-SKILL-002 |
| 対応仕様書 | SPEC-SKILL-002 |
| ステータス | draft |
| 作成日 | 2026-05-24 |
| 作成者 | @KokiTokutake |

---

## 1. エンティティ設計

### 1.1 SkillLevelDefinition（新規）

| プロパティ | 型 | 制約 | 説明 |
|-----------|-----|------|------|
| SkillItemId | Guid | PK（複合）, FK → SkillItem.Id | スキルアイテムID |
| Level | int | PK（複合）, 1〜7 | ITSS準拠レベル |
| Description | string | max:1000, Required | レベルの説明テキスト |
| Examples | string? | max:2000 | 行動例（任意） |
| CreatedAt | DateTimeOffset | | |
| UpdatedAt | DateTimeOffset | | |

**複合主キー:** `(SkillItemId, Level)`

**リレーション:**
- `SkillItemId` → `SkillItem.Id` (OnDelete: Cascade) — スキルアイテム削除時にレベル定義も連動削除

---

### 1.2 CompetencyMaster（新規）

| プロパティ | 型 | 制約 | 説明 |
|-----------|-----|------|------|
| Id | string | PK (ULID) | |
| Category | CompetencyCategory (enum) | Required | Technology / Methodology / BusinessIndustry / Common |
| Name | string | max:100, Required | コンピテンシー名 |
| Description | string? | max:500 | 説明（任意） |
| DisplayOrder | int | Required | 表示順 |
| IsActive | bool | default:true | 有効フラグ（false=論理削除） |
| CreatedAt | DateTimeOffset | | |
| UpdatedAt | DateTimeOffset | | |

**インデックス:** `(Category, Name)` — UNIQUE

---

### 1.3 CompetencyEvaluation（新規）

| プロパティ | 型 | 制約 | 説明 |
|-----------|-----|------|------|
| Id | string | PK (ULID) | |
| TalentId | Guid | FK → User.Id | 評価対象タレント |
| EvaluatorId | Guid | FK → User.Id | 評価者 |
| CompetencyId | string | FK → CompetencyMaster.Id | コンピテンシーID |
| PeriodId | Guid | FK → EvaluationPeriod.Id | 評価期間 |
| Level | int | 1〜5, Required | コンピテンシーレベル |
| Comment | string? | max:500 | コメント（任意） |
| EvaluatorType | CompetencyEvaluatorType (enum) | Required | Self / Manager |
| CreatedAt | DateTimeOffset | | |
| UpdatedAt | DateTimeOffset | | |

**インデックス:** `(TalentId, EvaluatorId, CompetencyId, PeriodId, EvaluatorType)` — UNIQUE

**リレーション:**
- `TalentId` → `User.Id` (OnDelete: Restrict)
- `EvaluatorId` → `User.Id` (OnDelete: Restrict)
- `CompetencyId` → `CompetencyMaster.Id` (OnDelete: Restrict)
- `PeriodId` → `EvaluationPeriod.Id` (OnDelete: Restrict)

---

### 1.4 SkillLevelMigrationLog（新規）

移行実行の監査記録（評価データは機微情報のため変更履歴を保持）。

| プロパティ | 型 | 制約 | 説明 |
|-----------|-----|------|------|
| Id | Guid | PK | |
| ExecutedBy | Guid | FK → User.Id | 実行した管理者 |
| ExecutedAt | DateTimeOffset | Required | 実行日時 |
| AffectedCount | int | Required | 変換件数 |
| ConversionRuleJson | string | max:500 | 変換ルール記録（JSON文字列） |

**リレーション:** `ExecutedBy` → `User.Id` (OnDelete: Restrict)

---

### 1.5 既存エンティティの変更

**UserSkill.Level の制約拡張:**
- 現状: 1〜5
- 変更後: 1〜7（CHECK制約を更新するマイグレーション）

---

### 1.6 Enum 定義

```csharp
// Domain/Enums/CompetencyCategory.cs
public enum CompetencyCategory
{
    Technology,         // テクノロジコンピテンシー
    Methodology,        // メソドロジコンピテンシー
    BusinessIndustry,   // ビジネスインダストリコンピテンシー
    Common              // 共通コンピテンシー
}

// Domain/Enums/CompetencyEvaluatorType.cs
public enum CompetencyEvaluatorType
{
    Self,
    Manager
}
```

---

## 2. API設計

### 2.1 スキルレベル定義 API

#### GET /api/skill-masters/{skillItemId}/level-definitions

レベル定義取得。未設定スキルは標準フォールバックテキストを返す（`isFallback: true`）。

**認可:** 全ロール

**レスポンス（200 OK）:**
```json
{
  "skillItemId": "...",
  "levelDefinitions": [
    { "level": 1, "description": "概念的理解。学習中または経験ゼロ", "examples": null, "isFallback": true },
    { "level": 2, "description": "基礎的な作業を上位者のサポートで実施可能", "examples": null, "isFallback": true },
    { "level": 3, "description": "...", "examples": "...", "isFallback": false },
    ...
  ]
}
```

---

#### PUT /api/skill-masters/{skillItemId}/level-definitions

レベル定義一括保存（7レベル分まとめて送信）。

**認可:** Admin のみ

**リクエスト:**
```json
{
  "levelDefinitions": [
    { "level": 1, "description": "...", "examples": "..." },
    { "level": 2, "description": "...", "examples": null },
    ...
  ]
}
```

**レスポンス（200 OK）:** 保存後の定義一覧（上記GET と同形式）

**エラーレスポンス:**
| ステータス | 条件 |
|-----------|------|
| 400 | level範囲外（1〜7以外）、descriptionが空 |
| 403 | Admin以外のロール |
| 404 | SkillItemが存在しない |

---

### 2.2 コンピテンシーマスタ API

#### GET /api/competency-masters

コンピテンシーマスタ一覧取得（`isActive=true` のみ）。

**認可:** 全ロール

**レスポンス（200 OK）:**
```json
{
  "competencies": [
    { "id": "...", "category": "Technology", "name": "技術的問題解決", "description": "...", "displayOrder": 1 }
  ]
}
```

---

#### POST /api/competency-masters

コンピテンシー追加。

**認可:** Admin のみ

**リクエスト:**
```json
{ "category": "Technology", "name": "AI活用力", "description": "...", "displayOrder": 5 }
```

**レスポンス（201 Created）:** 作成したコンピテンシー

**エラーレスポンス:**
| ステータス | 条件 |
|-----------|------|
| 400 | バリデーションエラー |
| 403 | Admin以外 |
| 409 | 同一 Category + Name の重複 |

---

#### PUT /api/competency-masters/{id}

コンピテンシー更新（`isActive: false` で論理削除）。

**認可:** Admin のみ

**エラーレスポンス:**
| ステータス | 条件 |
|-----------|------|
| 403 | Admin以外 |
| 404 | コンピテンシー未存在 |
| 409 | 同一 Category + Name の重複 |

---

### 2.3 コンピテンシー評価 API

#### POST /api/competency-evaluations（Upsert）

コンピテンシー評価の一括保存（新規または更新）。

**認可:**
- Talent: 自分自身（`talentId == currentUserId`, `evaluatorType == Self`）
- Manager: 配下タレント（`talentId` が managedTalents に含まれる, `evaluatorType == Manager`）

**リクエスト:**
```json
{
  "talentId": "...",
  "periodId": "...",
  "evaluatorType": "Self",
  "evaluations": [
    { "competencyId": "...", "level": 3, "comment": "..." },
    { "competencyId": "...", "level": 4, "comment": null }
  ]
}
```

**レスポンス（200 OK）:** `{ "savedCount": 4 }`

**エラーレスポンス:**
| ステータス | 条件 |
|-----------|------|
| 400 | level範囲外（1〜5以外）|
| 403 | 対象タレントへのアクセス権なし |
| 404 | periodId / competencyId 未存在 |

---

#### GET /api/competency-evaluations?talentId=&periodId=

コンピテンシー評価取得（自己評価・上長評価の両方を含む）。

**認可:**
- Talent: 自分のみ（`talentId == currentUserId`）
- Manager / Senior / Admin: 配下または全タレント

**レスポンス（200 OK）:**
```json
{
  "talentId": "...",
  "periodId": "...",
  "evaluations": [
    {
      "competencyId": "...",
      "competencyName": "技術的問題解決",
      "category": "Technology",
      "selfEvaluation": { "level": 3, "comment": "...", "updatedAt": "..." },
      "managerEvaluation": { "level": 4, "comment": "...", "updatedAt": "..." },
      "gap": 1
    }
  ]
}
```

---

### 2.4 スキルギャップ API

#### GET /api/skills/gap?talentId=&periodId=&threshold=2

スキル・コンピテンシーの自己評価と上長評価のギャップ一覧。

**認可:** Talent（自分のみ）、Manager / Senior / Admin

**レスポンス（200 OK）:**
```json
{
  "talentId": "...",
  "periodId": "...",
  "gapThreshold": 2,
  "skills": [
    {
      "skillItemId": "...",
      "skillItemName": "Java",
      "selfLevel": 4,
      "managerLevel": 2,
      "gap": -2,
      "requiresAttention": true
    }
  ],
  "competencies": [
    {
      "competencyId": "...",
      "competencyName": "コミュニケーション",
      "category": "Common",
      "selfLevel": 3,
      "managerLevel": 4,
      "gap": 1,
      "requiresAttention": false
    }
  ]
}
```

---

### 2.5 スキルマップ コンピテンシー API

#### GET /api/skill-map/competency?periodId=&departmentId=

コンピテンシーヒートマップデータ取得。

**認可:** MDC / Senior / Admin

**レスポンス（200 OK）:**
```json
{
  "periodId": "...",
  "categories": [
    {
      "category": "Technology",
      "competencies": [
        {
          "competencyId": "...",
          "competencyName": "技術的問題解決",
          "averages": [
            { "label": "バックエンド", "avgLevel": 3.2, "count": 5 },
            { "label": "フロントエンド", "avgLevel": 2.8, "count": 4 }
          ]
        }
      ]
    }
  ]
}
```

---

### 2.6 スキル移行ツール API

#### GET /api/admin/skill-migration/preview

変換プレビュー取得（実際には変換しない）。

**認可:** Admin のみ

**レスポンス（200 OK）:**
```json
{
  "conversionRule": { "1": 1, "2": 2, "3": 3, "4": 5, "5": 7 },
  "affectedCount": 142,
  "preview": [
    { "talentName": "山田太郎", "skillName": "Java", "currentLevel": 4, "newLevel": 5 }
  ]
}
```

---

#### POST /api/admin/skill-migration/execute

スケール変換を実行（冪等でない。二重実行防止は呼び出し側で制御）。

**認可:** Admin のみ

**レスポンス（200 OK）:**
```json
{
  "success": true,
  "affectedCount": 142,
  "migrationLogId": "...",
  "executedAt": "2026-05-24T10:00:00Z"
}
```

**エラーレスポンス:**
| ステータス | 条件 |
|-----------|------|
| 403 | Admin以外 |
| 409 | 移行ログが既に存在する（二重実行防止） |

---

## 3. フロントエンド設計

### 3.1 コンポーネント構成

```
pages/
  skills/
    my.vue                      # 既存拡張: レベル選択を1-7に変更 + 定義ツールチップ追加
    map.vue                     # 既存拡張: コンピテンシータブ追加
    competency.vue              # 新規: コンピテンシー評価ページ（タレント・上長）
    gap.vue                     # 新規: スキルギャップ比較ページ
  admin/
    skills/
      level-definitions.vue     # 新規: スキルレベル定義管理（Admin）
      competency-master.vue     # 新規: コンピテンシーマスタ管理（Admin）
      migration.vue             # 新規: スケール移行ツール（Admin）

components/
  skill/
    SkillLevelTooltip.vue       # 新規: レベル定義ツールチップ/モーダル
    CompetencyEvalForm.vue      # 新規: コンピテンシー評価フォーム
    CompetencyHeatmap.vue       # 新規: コンピテンシーヒートマップ（スキルマップ用）
    SkillGapChart.vue           # 新規: ギャップ比較バーチャート
    SkillLevelDefForm.vue       # 新規: レベル定義編集フォーム（Admin）
    MigrationPreviewTable.vue   # 新規: 移行プレビューテーブル（Admin）
```

---

### 3.2 Pinia ストア設計

```typescript
// stores/skillLevelDefinition.ts
interface SkillLevelDefinitionState {
  definitions: Map<string, SkillLevelDefinition[]>; // skillItemId → definitions
  loading: boolean;
  error: string | null;
}
actions: fetchDefinitions(skillItemId), saveDefinitions(skillItemId, defs)

// stores/competencyMaster.ts
interface CompetencyMasterState {
  competencies: CompetencyMaster[];
  loading: boolean;
  error: string | null;
}
actions: fetchCompetencies(), addCompetency(data), updateCompetency(id, data)

// stores/competencyEvaluation.ts
interface CompetencyEvaluationState {
  evaluations: CompetencyEvaluationResult[];
  loading: boolean;
  saving: boolean;
  error: string | null;
}
actions: fetchEvaluations(talentId, periodId), saveEvaluations(payload)

// stores/skillGap.ts
interface SkillGapState {
  gapData: SkillGapResult | null;
  threshold: number;
  loading: boolean;
  error: string | null;
}
actions: fetchGap(talentId, periodId, threshold)
```

---

### 3.3 data-testid 定義

| 要素 | data-testid | 対応 AC |
|------|-------------|---------|
| レベル定義ツールチップトリガー（?アイコン） | `skill-level-tooltip-trigger` | AC-02-01 |
| レベル定義ツールチップ内容 | `skill-level-tooltip-content` | AC-02-01 |
| スキルレベルセレクト | `skill-level-select` | AC-01-01 |
| レベル定義保存ボタン（Admin） | `skill-level-def-save-button` | AC-01-01 |
| コンピテンシー評価タブ（スキルページ） | `competency-tab` | AC-03-01 |
| コンピテンシーレベルセレクト | `competency-level-select-{competencyId}` | AC-03-01 |
| コンピテンシー評価保存ボタン | `competency-eval-save-button` | AC-03-01 |
| コンピテンシーマスタ追加ボタン（Admin） | `competency-add-button` | AC-03-02 |
| コンピテンシー名入力（Admin） | `competency-name-input` | AC-03-02 |
| コンピテンシーカテゴリ選択（Admin） | `competency-category-select` | AC-03-02 |
| スキルギャップ行 | `skill-gap-row-{skillItemId}` | AC-04-01 |
| ギャップ値表示 | `skill-gap-value-{skillItemId}` | AC-04-01 |
| 要確認セクション | `gap-attention-section` | AC-04-02 |
| スキルマップ コンピテンシータブ | `skillmap-competency-tab` | AC-05-01 |
| コンピテンシーヒートマップコンテナ | `competency-heatmap` | AC-05-01 |
| 移行プレビュー確認ボタン（Admin） | `migration-preview-button` | AC-01-03 |
| 移行実行ボタン（Admin） | `migration-execute-button` | AC-01-03 |
| 移行確認ダイアログ「実行する」ボタン | `migration-confirm-execute-button` | AC-01-03 |

---

## 4. 技術的判断の記録

| 決定事項 | 採用した選択 | 理由 | 不採用の選択肢 |
|---------|------------|------|--------------|
| SkillLevelDefinition PK | 複合PK (SkillItemId, Level) | ULID不要。シンプルな参照パターン（常に levelで絞り込む） | ULID/Guid 単独PK |
| SkillLevelDefinition FK OnDelete | Cascade | スキルアイテム削除時にレベル定義も不要。Restrictだと削除が困難になる | Restrict, SetNull |
| CompetencyEvaluation level スケール | 1〜5 | スキル評価との連動表示を考慮。コンピテンシーは行動評価であり ITSS 7段階は過剰 | 1〜7スケール |
| コンピテンシー評価保存 | POST Upsert 一括（evaluations 配列） | フロントエンドが「全項目まとめて保存」する UI パターンに最適。1件ずつPATCHは複数回 HTTP が発生 | PUT 1件ずつ / PATCH 差分 |
| フォールバック定義 | API側で汎用テキストを返す（DBに保存しない） | Admin 設定なしでも動作保証。DB へ全スキルに事前挿入するとマスタ肥大化 | DB全スキルに事前挿入 |
| 移行ログ | 専用テーブル（SkillLevelMigrationLog） | 評価データは機微情報（労働基準法・7年保持）。変更操作の監査証跡が必須 | ログファイルのみ / AuditLog 汎用テーブル |
| 二重実行防止 | MigrationLog 存在チェックで 409 | シンプル。再実行は別トランザクションで行う設計にする | 冪等実行（レベル上書き再実行OK） |
