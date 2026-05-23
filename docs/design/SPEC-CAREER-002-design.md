# Will-Can-Must深化（時間軸分割・Can強み/課題・やりがい）設計書

| 項目 | 内容 |
|------|------|
| Doc ID | DESIGN-SPEC-CAREER-002 |
| 対応仕様書 | SPEC-CAREER-002 |
| ステータス | approved |
| 作成日 | 2026-05-23 |
| 作成者 | @KokiTokutake |

---

## 1. エンティティ設計

### 1-1. WillCanMustEntry（新規）

タレントの Will / Can / Must を時間軸・カテゴリ別に構造化して保存するエンティティ。
既存の `CareerPlan`（SPEC-CAREER-001）は後方互換のため保持し、本エンティティで拡張する。

| プロパティ | 型 | 制約 | 説明 |
|-----------|-----|------|------|
| Id | Guid | PK | |
| TalentId | Guid | FK → User.Id, NOT NULL | 対象タレント |
| PeriodId | Guid | FK → EvaluationPeriod.Id, NOT NULL | 考課期間 |
| Category | WcmCategory (enum) | NOT NULL | Will / Can / Must |
| TimeHorizon | WcmTimeHorizon? (enum) | NULL 許容 | Will のみ使用 |
| CanType | CanType? (enum) | NULL 許容 | Can のみ使用 |
| Content | string | NOT NULL, max:2000 | 入力内容 |
| DisplayOrder | int | NOT NULL, default:0 | 表示順 |
| LinkedSkillItemId | Guid? | FK → SkillItem.Id, NULL 許容 | Can のスキルマスタ連携 |
| CreatedAt | DateTimeOffset | NOT NULL | |
| UpdatedAt | DateTimeOffset | NOT NULL | |

**WcmCategory 列挙値:**
```csharp
public enum WcmCategory { Will, Can, Must }
```

**WcmTimeHorizon 列挙値:**
```csharp
public enum WcmTimeHorizon { ShortTerm, MidTerm, LongTerm }
```

**CanType 列挙値:**
```csharp
public enum CanType { Strength, Challenge }
```

**インデックス:**
- `(TalentId, PeriodId, Category)` — 一覧取得の主要クエリ

**削除ルール:**
- `TalentId` → Restrict（ユーザー削除時はアプリ側で制御）
- `PeriodId` → Restrict
- `LinkedSkillItemId` → SetNull

**バリデーション:**
- `Category = Will` のとき `CanType` は必ず NULL
- `Category = Can` のとき `TimeHorizon` は必ず NULL
- `Category = Must` のとき `TimeHorizon`・`CanType` ともに NULL
- 同一 `(TalentId, PeriodId, Category, CanType)` の組み合わせで `DisplayOrder` は重複不可（アプリ側で制御）
- **上限制限**: `Category=Can` かつ同一 `CanType` は最大5件（アプリ層でチェック）

---

### 1-2. CareerValues（新規）

タレントの価値観・やりがい情報を保存するエンティティ。1タレント×1期間で1レコード（Upsert）。

| プロパティ | 型 | 制約 | 説明 |
|-----------|-----|------|------|
| Id | Guid | PK | |
| TalentId | Guid | FK → User.Id, NOT NULL | 対象タレント |
| PeriodId | Guid | FK → EvaluationPeriod.Id, NOT NULL | 考課期間 |
| ImportantThings | string? | NULL 許容, max:1000 | 仕事で大切にしていること |
| MotivationSituations | string? | NULL 許容, max:1000 | やる気が上がる状況 |
| AvoidSituations | string? | NULL 許容, max:1000 | 避けたい状況 |
| CreatedAt | DateTimeOffset | NOT NULL | |
| UpdatedAt | DateTimeOffset | NOT NULL | |

**インデックス:**
- `(TalentId, PeriodId)` — UNIQUE

**削除ルール:**
- `TalentId` → Restrict
- `PeriodId` → Restrict

---

## 2. API設計

### 2-1. WCM一覧取得

```
GET /api/career/wcm?talentId={guid}&periodId={guid}
```

**認可:**
| ロール | 条件 |
|--------|------|
| Talent | `talentId == 自分のId` のみ |
| Manager | 配下タレントのみ（`X-User-Id` で検証） |
| Senior / Executive / Admin | 全タレント可 |

**レスポンス（200 OK）:**
```json
{
  "talentId": "guid",
  "periodId": "guid",
  "will": {
    "shortTerm": [{ "id": "guid", "content": "string", "displayOrder": 0 }],
    "midTerm":   [{ "id": "guid", "content": "string", "displayOrder": 0 }],
    "longTerm":  [{ "id": "guid", "content": "string", "displayOrder": 0 }]
  },
  "can": {
    "strengths":  [{ "id": "guid", "content": "string", "displayOrder": 0, "linkedSkillItemId": "guid|null", "linkedSkillName": "string|null" }],
    "challenges": [{ "id": "guid", "content": "string", "displayOrder": 0, "linkedSkillItemId": "guid|null", "linkedSkillName": "string|null" }]
  },
  "must": [{ "id": "guid", "content": "string", "displayOrder": 0 }]
}
```

**エラー:**
| コード | 条件 |
|--------|------|
| 403 | 閲覧権限なし |
| 404 | talentId または periodId が存在しない |

---

### 2-2. WCM一括保存（Upsert）

```
PUT /api/career/wcm
Header: X-User-Id: {requesterId}
```

**認可:** Talent 本人のみ（`requesterId == talentId`）

**リクエスト:**
```json
{
  "talentId": "guid",
  "periodId": "guid",
  "will": {
    "shortTerm": [{ "id": "guid|null", "content": "string" }],
    "midTerm":   [{ "id": "guid|null", "content": "string" }],
    "longTerm":  [{ "id": "guid|null", "content": "string" }]
  },
  "can": {
    "strengths":  [{ "id": "guid|null", "content": "string", "linkedSkillItemId": "guid|null" }],
    "challenges": [{ "id": "guid|null", "content": "string", "linkedSkillItemId": "guid|null" }]
  },
  "must": [{ "id": "guid|null", "content": "string" }]
}
```

**処理:** 送信されたリスト全体で既存レコードを置換（削除→再挿入）。`id` が存在すれば更新、なければ新規作成。

**バリデーション:**
- `strengths` ≤ 5件、`challenges` ≤ 5件
- 各 `content` は 1〜2000 文字

**レスポンス（200 OK）:** WCM一覧取得と同じ形式

**エラー:**
| コード | 条件 |
|--------|------|
| 400 | バリデーションエラー（件数上限・文字数） |
| 403 | 本人以外が保存しようとした |

---

### 2-3. 価値観・やりがい取得

```
GET /api/career/values?talentId={guid}&periodId={guid}
```

**認可:** WCM一覧取得と同じルール

**レスポンス（200 OK）:**
```json
{
  "id": "guid|null",
  "talentId": "guid",
  "periodId": "guid",
  "importantThings": "string|null",
  "motivationSituations": "string|null",
  "avoidSituations": "string|null",
  "updatedAt": "datetime"
}
```

レコード未存在の場合は `id: null` で空のオブジェクトを返す（404 にしない）。

---

### 2-4. 価値観・やりがい保存（Upsert）

```
PUT /api/career/values
Header: X-User-Id: {requesterId}
```

**認可:** Talent 本人のみ

**リクエスト:**
```json
{
  "talentId": "guid",
  "periodId": "guid",
  "importantThings": "string|null",
  "motivationSituations": "string|null",
  "avoidSituations": "string|null"
}
```

**レスポンス（200 OK）:** 保存後の CareerValues オブジェクト（取得レスポンスと同形式）

---

## 3. フロントエンド設計

### 3-1. コンポーネント構成

```
frontend/
  pages/
    career/
      [userId].vue          # 既存ページ — WCM深化セクションを追加
  components/
    career/
      WillAxisSection.vue   # 短期・中期・長期 Will の3軸入力
      CanSection.vue        # 強み/課題タブ切替 + スキル選択
      CareerValuesSection.vue  # 価値観・やりがい入力
      MustSection.vue       # Must 入力 + 上長コンテキスト表示（読み取り専用）
  stores/
    wcm.ts                  # WillCanMustEntry CRUD
    careerValues.ts         # CareerValues CRUD
  types/
    wcm.ts                  # 型定義
```

---

### 3-2. Pinia ストア設計

**`stores/wcm.ts`**
```typescript
interface WcmState {
  wcm: WcmDto | null        // 取得済み WCM データ
  loading: boolean
  saving: boolean
  error: string | null
}

// actions
fetchWcm(talentId, periodId): Promise<void>
saveWcm(req: SaveWcmRequest): Promise<void>
$reset(): void
```

**`stores/careerValues.ts`**
```typescript
interface CareerValuesState {
  values: CareerValuesDto | null
  loading: boolean
  saving: boolean
  error: string | null
}

// actions
fetchValues(talentId, periodId): Promise<void>
saveValues(req: SaveCareerValuesRequest): Promise<void>
$reset(): void
```

---

### 3-3. data-testid 定義

| 要素 | data-testid | 備考 |
|------|-------------|------|
| WCM セクション全体 | `wcm-section` | |
| 短期 Will テキストエリア（1件目） | `will-short-term-0` | インデックス付き |
| 中期 Will テキストエリア（1件目） | `will-mid-term-0` | |
| 長期 Will テキストエリア（1件目） | `will-long-term-0` | |
| 長期 Will 入力ガイドメッセージ | `will-guide-message` | 長期未入力時のみ表示 |
| 強み Can テキストエリア（1件目） | `can-strength-0` | |
| 課題 Can テキストエリア（1件目） | `can-challenge-0` | |
| 強み Can 「追加」ボタン | `can-strength-add-button` | |
| 課題 Can 「追加」ボタン | `can-challenge-add-button` | |
| Can 上限エラーメッセージ | `can-limit-message` | 5件超過時に表示 |
| スキルから選択ボタン | `can-skill-select-button` | |
| Must テキストエリア（1件目） | `must-0` | |
| 上長期待表示エリア | `must-manager-context` | 読み取り専用 |
| 価値観セクション | `career-values-section` | |
| 大切にしていること入力 | `values-important-things` | |
| やる気が上がる状況入力 | `values-motivation-situations` | |
| 避けたい状況入力 | `values-avoid-situations` | |
| WCM 保存ボタン | `wcm-save-button` | |
| 価値観 保存ボタン | `values-save-button` | |
| 保存成功トースト | `wcm-save-success` | |

---

## 4. 技術的判断の記録

| 決定事項 | 採用した選択 | 理由 | 不採用の選択肢 |
|---------|------------|------|--------------|
| PeriodId の型 | FK → EvaluationPeriod.Id (Guid) | Goal 等と同じパターン。PeriodYear (int) は CareerPlan の暫定実装だった | PeriodYear: int のまま継続 |
| WCM 保存方式 | 一括 PUT（全置換） | 項目の追加・削除・並び替えが混在するため、差分管理より全置換が実装・テストしやすい | PATCH（差分更新） |
| CareerValues の主キー | Guid PK + UNIQUE(TalentId, PeriodId) | Upsert しやすく、GET で空レコードを返せる | 複合 PK |
| CareerPlan の扱い | 保持・アーカイブ（削除しない） | 仕様書 §7 の後方互換要件。既存データの安全性を優先 | CareerPlan の旧フィールドに上書き |
| Can 上限チェック | アプリ層のみ（DB 制約なし） | 件数制限はビジネスルールであり DB 制約で表現するより柔軟性が高い | CHECK 制約 |

---

## 5. 仕様書との対応表

| 受入基準 | 対応する設計箇所 |
|---------|----------------|
| AC-01-01: 3軸の入力と保存 | WillCanMustEntry (TimeHorizon: Short/Mid/Long) + PUT /api/career/wcm |
| AC-01-02: 部分入力の許容 | Content は NOT NULL だが、各軸のリストが空配列でも保存可能 |
| AC-01-03: 入力ガイド表示 | `will-guide-message` (longTerm が空のとき表示) |
| AC-02-01: 強みと課題の入力 | WillCanMustEntry (CanType: Strength/Challenge) |
| AC-02-02: スキルマスタからの選択 | LinkedSkillItemId FK + `can-skill-select-button` |
| AC-02-03: 上限制限（最大5件） | アプリ層バリデーション + `can-limit-message` |
| AC-03-01: 価値観セクションの入力 | CareerValues エンティティ + PUT /api/career/values |
| AC-04-01: 上長コメントの表示 | GET /api/career/wcm レスポンスに managerExpectedRole を含める（TalentManagerComment から集約） |
