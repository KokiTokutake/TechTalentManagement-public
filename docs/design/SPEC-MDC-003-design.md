# タレントプロファイル（委員会事前資料）設計書

| 項目 | 内容 |
|------|------|
| Doc ID | DESIGN-SPEC-MDC-003 |
| 対応仕様書 | SPEC-MDC-003 |
| ステータス | approved |
| 作成日 | 2026-05-23 |
| 作成者 | @KokiTokutake |

---

## 1. エンティティ設計

### 1-1. TalentManagerComment（新規）

上長がタレントごと・考課期間ごとに入力する育成コメント。

| プロパティ | 型 | 制約 | 説明 |
|-----------|-----|------|------|
| Id | Guid | PK | |
| TalentId | Guid | FK → User.Id, NOT NULL | 評価対象タレント |
| ManagerId | Guid | FK → User.Id, NOT NULL | コメント入力した上長 |
| PeriodId | Guid | FK → EvaluationPeriod.Id, NOT NULL | 考課期間 |
| CurrentChallenges | string? | NULL, max:2000 | 現在の課題 |
| DevelopmentPolicy | string? | NULL, max:2000 | 育成方針 |
| ExpectedRole | string? | NULL, max:1000 | 期待役割・ポジション（1〜3年後） |
| CreatedAt | DateTimeOffset | NOT NULL | |
| UpdatedAt | DateTimeOffset | NOT NULL | |

**インデックス:**
- `(TalentId, PeriodId)` — UNIQUE（1期間1タレント1コメント）
- `(ManagerId)` — 上長別一覧取得用

**リレーション:**
- `TalentId` → `User.Id` (OnDelete: Restrict)
- `ManagerId` → `User.Id` (OnDelete: Restrict)
- `PeriodId` → `EvaluationPeriod.Id` (OnDelete: Restrict)

### 1-2. TalentProfile（集約ビューモデル — DBテーブルなし）

プロファイル画面は既存エンティティからデータを集約する読み取り専用ビュー。新規テーブルは作成しない。

| セクション | 取得元エンティティ |
|-----------|-----------------|
| 基本情報 | User |
| 9ボックス評価 | NineBoxAssessment + NineBoxCellDefinition |
| Will-Can-Must | CareerPlan |
| スキルサマリー（上位5件） | UserSkill + SkillItem + SkillCategory |
| 面談記録サマリー（最新3件） | OneOnOneMeeting |
| 育成コメント | TalentManagerComment（本仕様で新規追加） |
| 重点管理フラグ | KeyPerson |
| MDC決定事項履歴 | SPEC-MDC-004 で実装（MdcDecision） |

### 1-3. KeyPerson（既存 — 変更なし）

`KeyPerson` エンティティはSPEC-MDC-001で実装済み。プロファイル画面からも同一エンドポイントを利用する。PATCH エンドポイントのみ追加。

---

## 2. API設計

### コントローラー: `TalentProfileController`（`/api/talents/{talentId}`）

---

### 2-1. GET /api/talents/{talentId}/profile — プロファイル集約取得

**認可:** Manager（配下タレントのみ）/ Senior / Admin

**クエリパラメータ:**
| パラメータ | 型 | 必須 | 説明 |
|-----------|-----|------|------|
| periodId | Guid | No | 省略時は最新の考課期間を使用 |

**レスポンス（200 OK）:**
```json
{
  "talentId": "uuid",
  "talentName": "田中 太郎",
  "role": "Talent",
  "periodId": "uuid",
  "periodName": "2026年度",
  "isKeyPerson": true,
  "nineBox": {
    "performanceLevel": "High",
    "potentialLevel": "Mid",
    "cellLabel": "高業績者",
    "cellColor": "#3b82f6",
    "assessedAt": "2026-04-01T00:00:00Z"
  },
  "willCanMust": {
    "longTermGoal": "...",
    "mediumTermGoal": "...",
    "currentFocus": "..."
  },
  "skillSummary": [
    { "skillName": "TypeScript", "categoryName": "言語", "level": 4 }
  ],
  "meetingSummary": [
    { "id": "uuid", "conductedAt": "2026-03-15", "notesSummary": "..." }
  ],
  "managerComment": {
    "currentChallenges": "...",
    "developmentPolicy": "...",
    "expectedRole": "..."
  }
}
```

**エラーレスポンス:**
| ステータス | 条件 |
|-----------|------|
| 403 | Manager が配下以外のタレントにアクセス |
| 404 | talentId が存在しない |

---

### 2-2. PUT /api/talents/{talentId}/manager-comment — 育成コメント保存

**認可:** Manager（配下タレントのみ）

**クエリパラメータ:**
| パラメータ | 型 | 必須 | 説明 |
|-----------|-----|------|------|
| periodId | Guid | Yes | 考課期間ID |

**リクエスト:**
```json
{
  "currentChallenges": "string | null",
  "developmentPolicy": "string | null",
  "expectedRole": "string | null"
}
```

**レスポンス（200 OK）:**
```json
{
  "id": "uuid",
  "talentId": "uuid",
  "periodId": "uuid",
  "currentChallenges": "...",
  "developmentPolicy": "...",
  "expectedRole": "...",
  "updatedAt": "2026-05-23T00:00:00Z"
}
```

**ロジック:** `(TalentId, PeriodId)` でUPSERT（存在すれば UPDATE、なければ INSERT）

**エラーレスポンス:**
| ステータス | 条件 |
|-----------|------|
| 400 | バリデーションエラー（文字数超過など） |
| 403 | Manager が配下以外のタレントに操作 |
| 404 | talentId / periodId が存在しない |

---

### 2-3. GET /api/mdc-sessions/{sessionId}/profiles — セッション対象タレント一括取得

**認可:** Senior / Admin

**レスポンス（200 OK）:**
```json
[
  { /* TalentProfileDto × n件 */ }
]
```

---

### 2-4. PATCH /api/talents/{talentId}/key-person-flag — 重点管理フラグ設定

**認可:** Senior / Admin

**リクエスト:**
```json
{
  "isActive": true,
  "reason": "string | null"
}
```

**レスポンス（200 OK）:**
```json
{
  "talentId": "uuid",
  "isActive": true,
  "updatedAt": "2026-05-23T00:00:00Z"
}
```

**ロジック:**
- `isActive: true` → `KeyPerson.Create()` or 既存レコードを再アクティブ化
- `isActive: false` → `KeyPerson.Deactivate()`

---

## 3. フロントエンド設計

### 3-1. ページ・コンポーネント構成

```
pages/
  mdc/
    talent-profile/
      [talentId].vue          # タレントプロファイル画面（メイン）

components/
  talent-profile/
    TalentProfileHeader.vue      # 基本情報・重点管理バッジ・フラグ操作ボタン
    TalentProfileNineBox.vue     # 9ボックス評価セクション
    TalentProfileWillCanMust.vue # Will-Can-Mustセクション
    TalentProfileSkills.vue      # スキルサマリーセクション（上位5件）
    TalentProfileMeetings.vue    # 面談サマリーセクション（最新3件）
    TalentProfileManagerComment.vue  # 育成コメント入力フォーム（Manager向け）
    TalentProfilePeriodSelect.vue    # 考課期間セレクタ
```

### 3-2. Pinia ストア設計

```typescript
// stores/talentProfile.ts
const profile = ref<TalentProfileDto | null>(null)
const loading = ref(false)
const selectedPeriodId = ref<string>('')

async function fetchProfile(talentId: string, periodId?: string): Promise<void>
async function saveManagerComment(talentId: string, periodId: string, data: ManagerCommentRequest): Promise<void>
async function toggleKeyPerson(talentId: string, isActive: boolean, reason?: string): Promise<void>
```

### 3-3. data-testid 定義

| 要素 | data-testid | 説明 |
|------|-------------|------|
| ページ全体ラッパー | `talent-profile-page` | |
| 重点管理バッジ | `talent-profile-key-person-badge` | AC-03-01 |
| 重点管理指定ボタン | `talent-profile-key-person-toggle-button` | AC-03-02 |
| 確認ダイアログ確定ボタン | `talent-profile-key-person-confirm-button` | AC-03-02 |
| 期間セレクタ | `talent-profile-period-select` | AC-01-03 |
| 9ボックスセクション | `talent-profile-nine-box-section` | AC-01-01 |
| 未評価表示 | `talent-profile-nine-box-empty` | AC-01-02 |
| WCMセクション | `talent-profile-wcm-section` | AC-01-01 |
| スキルサマリーセクション | `talent-profile-skills-section` | AC-01-01 |
| 面談サマリーセクション | `talent-profile-meetings-section` | AC-01-01 |
| 育成コメントセクション | `talent-profile-manager-comment-section` | AC-02-01 |
| 現在の課題入力 | `talent-profile-challenges-input` | AC-02-01 |
| 育成方針入力 | `talent-profile-policy-input` | AC-02-01 |
| 期待役割入力 | `talent-profile-expected-role-input` | AC-02-01 |
| 保存ボタン | `talent-profile-comment-save-button` | AC-02-01 |
| 前期から複製ボタン | `talent-profile-copy-prev-button` | AC-02-02 |
| PDF出力ボタン | `talent-profile-pdf-export-button` | AC-04-01 |

---

## 4. 技術的判断の記録

| 決定事項 | 採用した選択 | 理由 | 不採用の選択肢 |
|---------|------------|------|--------------|
| プロファイルのDB表現 | 集約ビューモデル（DBテーブルなし） | 既存エンティティの集約で十分。マテリアライズドビューは更新整合性が複雑になる | マテリアライズドビュー / キャッシュテーブル |
| 育成コメントのUPSERT | `(TalentId, PeriodId)` UNIQUE制約 + アプリ側UPSERT | 1期間1タレント1コメントが自然な業務ルール | 複数バージョン管理（履歴テーブル）— 仕様にバージョン管理の記述なし |
| 重点管理フラグ操作 | 既存KeyPerson APIを利用。PATCHエンドポイントのみ追加 | KeyPersonエンティティはSPEC-MDC-001で実装済み。新エンドポイントで重複を避ける | 新KeyPersonFlagエンティティの作成 — 不要な重複 |
| PDF出力 | ブラウザの `window.print()` + 印刷用CSS（Should要件） | サーバーサイドPDF生成ライブラリ不要、実装コスト最小 | Puppeteer / wkhtmltopdf — インフラ依存が増加する |
| セッション対象プロファイル一括取得 | 既存の1件取得APIをN回呼び出し（フロントエンド側） | バックエンドの一括APIはSPEC-MDC-001のセッション管理と重複する。N≦20件程度なので問題なし | バックエンド一括取得API — Over-engineering |

---

## 5. 受入基準との対応確認

| AC ID | 受入基準概要 | 対応する設計箇所 |
|-------|------------|---------------|
| AC-01-01 | 全セクション集約表示 | GET /api/talents/{id}/profile + 各コンポーネント |
| AC-01-02 | 未評価時「未評価」表示 | `nineBox: null` のとき `talent-profile-nine-box-empty` 表示 |
| AC-01-03 | 期間セレクタで切り替え | `talent-profile-period-select` + periodId クエリパラメータ |
| AC-02-01 | 育成コメント保存 | PUT /api/talents/{id}/manager-comment + `talent-profile-comment-save-button` |
| AC-02-02 | 前期コメント複製 | フロントエンドで前期データをロードして入力欄にセット (`talent-profile-copy-prev-button`) |
| AC-03-01 | 重点管理バッジ表示 | `isKeyPerson: true` のとき `talent-profile-key-person-badge` 表示 |
| AC-03-02 | フラグ設定・解除 | PATCH /api/talents/{id}/key-person-flag + 確認ダイアログ |
| AC-04-01 | PDF生成・ダウンロード | `window.print()` + 印刷用CSS |
| AC-04-02 | フッターに機密表示 | 印刷用CSS `@media print` で固定フッター |
| AC-05-01 | スライド形式表示 | `pages/mdc/sessions/[sessionId].vue` からプロファイルページへカルーセルナビ |
