# SPEC-EVAL-002 設計書（Mustの定量KPI・中間振り返り・フィードバック面談記録）

| 項目 | 内容 |
|------|------|
| Doc ID | DESIGN-SPEC-EVAL-002 |
| 対応仕様書 | SPEC-EVAL-002 |
| ステータス | approved |
| 作成日 | 2026-05-24 |
| 作成者 | @KokiTokutake |

---

## 1. エンティティ設計

### 1-1. GoalKpi（新規）

Must目標（Goal）に1:1で紐付く KPI（定量指標）設定エンティティ。
Goal.Category = Must のみ対象だが、DB制約はアプリ層で制御する。

| プロパティ | 型 | 制約 | 説明 |
|-----------|-----|------|------|
| Id | Guid | PK | |
| GoalId | Guid | FK → Goal.Id, NOT NULL, UNIQUE | 1目標に1KPI |
| MetricName | string | NOT NULL, max:200 | 指標名（例: プロジェクトデリバリー件数） |
| TargetValue | decimal | NOT NULL | 目標値 |
| Unit | string | NOT NULL, max:50 | 単位（件・%・点など） |
| MeasurementMethod | string? | NULL 許容, max:500 | 測定方法 |
| DueDate | DateOnly? | NULL 許容 | 期限 |
| CreatedAt | DateTimeOffset | NOT NULL | |
| UpdatedAt | DateTimeOffset | NOT NULL | |

**インデックス:**
- `GoalId` — UNIQUE（1目標に1KPIのみ）

**削除ルール:**
- `GoalId` → Restrict（目標削除前に KPI を削除する必要あり、アプリ層で制御）

---

### 1-2. GoalCheckin（新規）

目標ごとの進捗チェックイン記録。期中いつでも複数回記録可能。

| プロパティ | 型 | 制約 | 説明 |
|-----------|-----|------|------|
| Id | Guid | PK | |
| GoalId | Guid | FK → Goal.Id, NOT NULL | 対象目標 |
| KpiId | Guid? | FK → GoalKpi.Id, NULL 許容 | KPI参照（KPI未設定時はNULL） |
| UserId | Guid | FK → User.Id, NOT NULL | チェックイン実施者（タレントまたは上長） |
| CurrentValue | decimal? | NULL 許容 | 現在値（KPIありの場合に入力） |
| ProgressPercent | decimal? | NULL 許容 | 自動計算: TargetValue > 0 の場合 CurrentValue / TargetValue × 100 |
| Comment | string? | NULL 許容, max:1000 | 進捗コメント |
| CheckedInAt | DateTimeOffset | NOT NULL | チェックイン日時 |

**インデックス:**
- `(GoalId, CheckedInAt DESC)` — チェックイン履歴の時系列取得

**削除ルール:**
- `GoalId` → Restrict
- `KpiId` → SetNull（KPI削除時にチェックインレコードを保持）
- `UserId` → Restrict

**ProgressPercent 計算ロジック（アプリ層）:**
```csharp
if (kpi != null && kpi.TargetValue > 0 && currentValue.HasValue)
    progressPercent = currentValue.Value / kpi.TargetValue * 100m;
else
    progressPercent = null;
```

---

### 1-3. FeedbackMeeting（新規）

考課期間の最終評価時に上長が実施するフィードバック面談の記録。
`SelfEvaluation` に1:1で紐付け、タレント×考課期間の単位で1レコード。

| プロパティ | 型 | 制約 | 説明 |
|-----------|-----|------|------|
| Id | Guid | PK | |
| SelfEvaluationId | Guid | FK → SelfEvaluation.Id, NOT NULL, UNIQUE | 1自己評価に1面談記録 |
| TalentId | Guid | FK → User.Id, NOT NULL | 面談対象タレント |
| ManagerId | Guid | FK → User.Id, NOT NULL | 実施した上長 |
| ConductedAt | DateOnly | NOT NULL | 面談実施日 |
| OverallComment | string? | NULL 許容, max:3000 | 総合評価コメント |
| NextPeriodExpectation | string? | NULL 許容, max:2000 | 次期への期待事項 |
| TalentReactionNote | string? | NULL 許容, max:1000 | タレントの反応（上長が記録） |
| IsDisclosed | bool | NOT NULL, default:false | 開示フラグ |
| DisclosedAt | DateTimeOffset? | NULL 許容 | 開示操作日時 |
| CreatedAt | DateTimeOffset | NOT NULL | |
| UpdatedAt | DateTimeOffset | NOT NULL | |

**インデックス:**
- `SelfEvaluationId` — UNIQUE
- `(TalentId, IsDisclosed)` — タレント向け一覧取得

**削除ルール:**
- `SelfEvaluationId` → Restrict
- `TalentId` → Restrict
- `ManagerId` → Restrict

---

## 2. API設計

### 2-1. POST /api/goals/{goalId}/kpi — KPI設定

**認可:** Talent（自分の目標）/ Manager（配下タレントの目標）、考課期間が `goal_setting` のみ

**リクエスト:**
```json
{
  "metricName": "プロジェクトデリバリー件数",
  "targetValue": 12,
  "unit": "件",
  "measurementMethod": "PM報告書で確認",
  "dueDate": "2026-12-31"
}
```

**レスポンス（201 Created）:**
```json
{
  "id": "uuid",
  "goalId": "uuid",
  "metricName": "プロジェクトデリバリー件数",
  "targetValue": 12,
  "unit": "件",
  "measurementMethod": "PM報告書で確認",
  "dueDate": "2026-12-31",
  "createdAt": "2026-05-24T00:00:00Z",
  "updatedAt": "2026-05-24T00:00:00Z"
}
```

**エラーレスポンス:**
| ステータス | 条件 |
|-----------|------|
| 400 | バリデーションエラー（metricName空・targetValue負値など） |
| 403 | 権限なし、または考課期間が goal_setting 以外 |
| 404 | goalId が存在しない |
| 409 | KPIが既に存在する（PUT を使うこと） |

---

### 2-2. PUT /api/goals/{goalId}/kpi — KPI更新

**認可:** 2-1 と同じ（goal_setting 期間のみ）

**リクエスト:** POST と同一スキーマ  
**レスポンス（200 OK）:** POST と同一スキーマ

**エラーレスポンス:**
| ステータス | 条件 |
|-----------|------|
| 400 | バリデーションエラー |
| 403 | 権限なし、または期間外 |
| 404 | goalId または KPI が存在しない |

---

### 2-3. POST /api/goals/{goalId}/checkins — チェックイン記録

**認可:** Talent（自分の目標）/ Manager（配下タレントの目標）、期間問わず記録可

**リクエスト:**
```json
{
  "currentValue": 8,
  "comment": "Q2で4件完了、Q3でペース維持中"
}
```

**レスポンス（201 Created）:**
```json
{
  "id": "uuid",
  "goalId": "uuid",
  "kpiId": "uuid or null",
  "userId": "uuid",
  "currentValue": 8,
  "progressPercent": 66.67,
  "comment": "Q2で4件完了、Q3でペース維持中",
  "checkedInAt": "2026-05-24T00:00:00Z"
}
```

**エラーレスポンス:**
| ステータス | 条件 |
|-----------|------|
| 400 | バリデーションエラー |
| 403 | 権限なし |
| 404 | goalId が存在しない |

---

### 2-4. GET /api/goals/{goalId}/checkins — チェックイン履歴取得

**認可:** Talent（自分）/ Manager（配下タレント）/ Admin（全員）

**レスポンス（200 OK）:**
```json
{
  "goalId": "uuid",
  "kpi": {
    "id": "uuid",
    "metricName": "プロジェクトデリバリー件数",
    "targetValue": 12,
    "unit": "件"
  },
  "checkins": [
    {
      "id": "uuid",
      "userId": "uuid",
      "currentValue": 8,
      "progressPercent": 66.67,
      "comment": "Q2で4件完了、Q3でペース維持中",
      "checkedInAt": "2026-05-24T00:00:00Z"
    }
  ]
}
```

---

### 2-5. POST /api/feedback-meetings — フィードバック面談記録作成

**認可:** Manager（自分が担当する配下タレントのみ）、考課期間が `final_evaluation` のみ

**リクエスト:**
```json
{
  "selfEvaluationId": "uuid",
  "talentId": "uuid",
  "conductedAt": "2026-12-20",
  "overallComment": "今期は目標を着実に達成し...",
  "nextPeriodExpectation": "来期はチームリードへの挑戦を...",
  "talentReactionNote": "前向きに受け止めている様子"
}
```

**レスポンス（201 Created）:**
```json
{
  "id": "uuid",
  "selfEvaluationId": "uuid",
  "talentId": "uuid",
  "managerId": "uuid",
  "conductedAt": "2026-12-20",
  "overallComment": "今期は目標を着実に達成し...",
  "nextPeriodExpectation": "来期はチームリードへの挑戦を...",
  "talentReactionNote": "前向きに受け止めている様子",
  "isDisclosed": false,
  "disclosedAt": null,
  "createdAt": "2026-05-24T00:00:00Z",
  "updatedAt": "2026-05-24T00:00:00Z"
}
```

**エラーレスポンス:**
| ステータス | 条件 |
|-----------|------|
| 400 | バリデーションエラー |
| 403 | 権限なし（他ManagerのタレントまたはAdmin以外）、または期間が final_evaluation 以外 |
| 404 | selfEvaluationId / talentId が存在しない |
| 409 | 同一 selfEvaluationId に対する面談記録が既に存在する |

---

### 2-6. GET /api/feedback-meetings — 面談記録取得

**クエリパラメータ:** `talentId` (必須) / `periodId` (必須)  
**認可:** Manager（配下タレントのみ）/ Admin（全員）/ Talent（自分かつ開示済みのみ）

**レスポンス（200 OK）:**
```json
{
  "id": "uuid",
  "talentId": "uuid",
  "managerId": "uuid",
  "conductedAt": "2026-12-20",
  "overallComment": "...",
  "nextPeriodExpectation": "...",
  "talentReactionNote": "...",
  "isDisclosed": false,
  "disclosedAt": null
}
```

- Talent ロールかつ `isDisclosed = false` の場合: 404 を返す（存在を隠蔽）

---

### 2-7. PATCH /api/feedback-meetings/{id}/disclose — 評価開示

**認可:** Manager（自分が作成した面談記録のみ）

**リクエスト:** ボディなし  
**レスポンス（200 OK）:**
```json
{
  "id": "uuid",
  "isDisclosed": true,
  "disclosedAt": "2026-05-24T00:00:00Z"
}
```

**エラーレスポンス:**
| ステータス | 条件 |
|-----------|------|
| 403 | 権限なし |
| 404 | 対象 id が存在しない |
| 409 | 既に開示済み |

---

## 3. フロントエンド設計

### 3-1. コンポーネント構成

```
pages/
  goals/
    [goalId].vue          # 目標詳細ページ（KPI表示 + チェックインパネル）

components/
  goals/
    GoalKpiForm.vue       # KPI入力フォーム（目標設定画面に組み込み）
    GoalKpiDisplay.vue    # KPI表示（読み取り専用）
    GoalCheckinPanel.vue  # チェックイン一覧 + 新規入力フォーム
    GoalProgressBadge.vue # 進捗率バッジ（一覧で表示）
  evaluation/
    FeedbackMeetingForm.vue   # フィードバック面談記録フォーム（上長向け）
    FeedbackMeetingView.vue   # フィードバック面談閲覧（タレント向け）
    FeedbackDiscloseBadge.vue # 開示状態バッジ
```

### 3-2. data-testid 定義

| 要素 | data-testid | 用途 |
|------|-------------|------|
| KPI 指標名入力 | `kpi-metric-name-input` | AC-01-01 |
| KPI 目標値入力 | `kpi-target-value-input` | AC-01-01 |
| KPI 単位入力 | `kpi-unit-input` | AC-01-01 |
| KPI 測定方法入力 | `kpi-measurement-method-input` | AC-01-01 |
| KPI 期限入力 | `kpi-due-date-input` | AC-01-01 |
| KPI 保存ボタン | `kpi-save-button` | AC-01-01 |
| KPI 入力制限メッセージ | `kpi-edit-disabled-message` | AC-01-02 |
| チェックイン 現在値入力 | `checkin-current-value-input` | AC-02-01 |
| チェックイン コメント入力 | `checkin-comment-input` | AC-02-01 |
| チェックイン 保存ボタン | `checkin-save-button` | AC-02-01 |
| 進捗率表示 | `checkin-progress-percent` | AC-02-01 |
| チェックイン履歴リスト | `checkin-history-list` | AC-02-02 |
| チェックイン履歴アイテム | `checkin-history-item` | AC-02-02 |
| 進捗バッジ（目標一覧） | `goal-progress-badge` | AC-02-03 |
| フィードバック 面談日入力 | `feedback-conducted-at-input` | AC-04-01 |
| フィードバック 評価コメント入力 | `feedback-overall-comment-input` | AC-04-01 |
| フィードバック 次期期待事項入力 | `feedback-next-period-expectation-input` | AC-04-01 |
| フィードバック タレント反応入力 | `feedback-talent-reaction-note-input` | AC-04-01 |
| フィードバック 保存ボタン | `feedback-save-button` | AC-04-01 |
| 開示ボタン | `feedback-disclose-button` | AC-05-01 |
| 開示確認ダイアログ | `feedback-disclose-dialog` | AC-05-01 |
| 開示確認 実行ボタン | `feedback-disclose-confirm-button` | AC-05-01 |
| 未開示メッセージ | `feedback-not-disclosed-message` | AC-05-02 |

### 3-3. Pinia ストア設計

```typescript
// stores/goalKpi.ts
interface GoalKpiState {
  kpiByGoalId: Record<string, GoalKpiDto | null>;
  loading: boolean;
}

actions:
  fetchKpi(goalId: string): Promise<void>
  saveKpi(goalId: string, data: GoalKpiRequest): Promise<void>
```

```typescript
// stores/goalCheckin.ts
interface GoalCheckinState {
  checkinsByGoalId: Record<string, GoalCheckinDto[]>;
  loading: boolean;
}

actions:
  fetchCheckins(goalId: string): Promise<void>
  addCheckin(goalId: string, data: GoalCheckinRequest): Promise<void>
```

```typescript
// stores/feedbackMeeting.ts
interface FeedbackMeetingState {
  meetings: Record<string, FeedbackMeetingDto | null>; // key: talentId_periodId
  loading: boolean;
}

actions:
  fetchMeeting(talentId: string, periodId: string): Promise<void>
  createMeeting(data: FeedbackMeetingRequest): Promise<void>
  disclose(meetingId: string): Promise<void>
```

---

## 4. 技術的判断の記録

| 決定事項 | 採用した選択 | 理由 | 不採用の選択肢 |
|---------|------------|------|--------------|
| ID 型 | Guid（.NET Guid.NewGuid()） | 既存エンティティ全て Guid を採用。仕様書の ULID は実装層で Guid に統一 | ULID（仕様書記載） |
| GoalKpi の関係 | Goal に 1:1（UNIQUE制約） | 1目標に複数KPIは管理複雑化を招く。将来的な複数KPI対応は仕様変更で行う | Goal に 1:N |
| GoalCheckin の KpiId | NULL 許容（SetNull） | KPI未設定の目標でもチェックインを許容。KPI削除時にチェックイン履歴を保持 | KpiId NOT NULL |
| ProgressPercent | アプリ層で計算・保存 | DB集計コストを避け、保存時の値を固定することで履歴の整合性を保証 | 取得時に動的計算 |
| FeedbackMeeting FK | SelfEvaluationId（UNIQUE） | 自己評価と1:1で紐付けることで、タレント×期間の一意性を SelfEvaluation 側で保証 | TalentId + EvaluationPeriodId の複合UNIQUE |
| 開示制御（Talent アクセス） | GET 時に 404 返却 | isDisclosed=false の存在をタレントに知らせない（情報漏洩防止）。403 より 404 が適切 | 403 返却 |
| リマインダー通知（REQ-03） | Azure Functions TimerTrigger（日次バッチ） | 仕様書 §7 非機能要件に明記。TASK-066 の API 実装タスクには含まず、別途 Functions プロジェクトで実装 | Hangfire / SignalR |
| チェックイン記録者 UserId | X-User-Id ヘッダから取得 | 他 API と統一。Talent・Manager どちらでも記録可能 | リクエストボディで指定 |
