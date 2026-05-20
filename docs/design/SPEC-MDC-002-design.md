# 9ボックス評価 設計書

| 項目 | 内容 |
|------|------|
| Doc ID | DESIGN-SPEC-MDC-002 |
| 対応仕様書 | SPEC-MDC-002 |
| ステータス | approved |
| 作成日 | 2026-05-20 |
| 作成者 | @KokiTokutake |

---

## 1. エンティティ設計

TASK-037 で実装済み。確認事項を記載する。

### NineBoxAssessment

| プロパティ | 型 | 制約 | 説明 |
|-----------|-----|------|------|
| Id | Guid | PK | |
| TalentId | Guid | FK → User.Id, NOT NULL | 評価対象タレント |
| EvaluationPeriodId | Guid | FK → EvaluationPeriod.Id, NOT NULL | 考課期間 |
| AssessorId | Guid | FK → User.Id, NOT NULL | 評価した上長 |
| PerformanceLevel | enum (Low=1/Mid=2/High=3) | NOT NULL | パフォーマンス評価 |
| PotentialLevel | enum (Low=1/Mid=2/High=3) | NOT NULL | ポテンシャル評価 |
| PerformanceRationale | string | NULL, max:1000 | パフォーマンスコメント |
| PotentialRationale | string | NULL, max:1000 | ポテンシャルコメント |
| AssessedAt | DateTimeOffset | NOT NULL | 評価実施日時 |
| CreatedAt | DateTimeOffset | NOT NULL | |
| UpdatedAt | DateTimeOffset | NOT NULL | |

**インデックス:**
- `(TalentId, EvaluationPeriodId)` — UNIQUE（1期間1タレント1評価）
- `(EvaluationPeriodId, PerformanceLevel, PotentialLevel)` — マップ描画・集計用

**重点管理候補フラグの計算ロジック（DB列は持たない）:**
以下の3セルを「High Potential」として API レスポンス生成時に判定する:
- High Performance × High Potential
- High Performance × Mid Potential
- Mid Performance × High Potential

### NineBoxCellDefinition（マスタ）

TASK-037 でシードデータ込みで実装済み。9件固定。管理者による更新のみ許可。

---

## 2. API設計

### コントローラー: `NineBoxController`（`/api/nine-box`）

---

### 2-1. POST /api/nine-box — 評価入力（TASK-038）

**認可:** Manager（配下タレントのみ）

**リクエスト:**
```json
{
  "talentId": "uuid",
  "periodId": "uuid",
  "performanceLevel": "Low | Mid | High",
  "potentialLevel": "Low | Mid | High",
  "performanceRationale": "string | null",
  "potentialRationale": "string | null"
}
```

**レスポンス（201 Created）:**
```json
{
  "id": "uuid",
  "talentId": "uuid",
  "talentName": "田中 太郎",
  "periodId": "uuid",
  "performanceLevel": "High",
  "potentialLevel": "Mid",
  "performanceRationale": "...",
  "potentialRationale": "...",
  "isHighPotentialCandidate": true,
  "assessedAt": "2026-05-20T10:00:00Z"
}
```

**エラーレスポンス:**
| ステータス | 条件 |
|-----------|------|
| 400 | バリデーションエラー |
| 403 | 配下タレント以外への評価入力 / 評価期間外 |
| 409 | 同一期間に既に評価が存在する（→ PUT を使うよう案内）|

**副作用:** 保存後に `isHighPotentialCandidate` が true の場合、重点管理候補フラグを KeyPerson テーブルに自動連携（TASK-040 で実装）。

---

### 2-2. PUT /api/nine-box/{id} — 評価更新（TASK-038）

**認可:** Manager（自分が入力した評価のみ）

**リクエスト:**
```json
{
  "performanceLevel": "Low | Mid | High",
  "potentialLevel": "Low | Mid | High",
  "performanceRationale": "string | null",
  "potentialRationale": "string | null"
}
```

**レスポンス（200 OK）:** POST と同形式

**エラーレスポンス:**
| ステータス | 条件 |
|-----------|------|
| 403 | 他のアセッサーの評価を更新しようとした / 評価期間外 |
| 404 | 評価が存在しない |

---

### 2-3. GET /api/nine-box?periodId=&departmentId= — 9ボックスマップデータ取得（TASK-039）

**認可:** Senior / Admin（全タレント）、Manager（配下タレントのみ）

**クエリパラメータ:**
| パラメータ | 型 | 必須 | 説明 |
|-----------|-----|-----|------|
| periodId | uuid | ✓ | 考課期間 |
| departmentId | uuid | - | 部門フィルタ |
| role | string | - | ロールフィルタ（Talent/Manager 等）|
| searchName | string | - | タレント名の部分一致検索 |
| comparePeriodId | uuid | - | 前期比較用（前期のperiodId）|

**レスポンス（200 OK）:**
```json
{
  "periodId": "uuid",
  "periodName": "2026年度通期考課",
  "cells": [
    {
      "performanceLevel": "High",
      "potentialLevel": "High",
      "label": "スター",
      "color": "#8b5cf6",
      "count": 2,
      "talents": [
        {
          "assessmentId": "uuid",
          "talentId": "uuid",
          "talentName": "田中 太郎",
          "initials": "田太",
          "role": "Talent",
          "performanceRationale": "...",
          "potentialRationale": "...",
          "isHighPotentialCandidate": true,
          "previousCell": {
            "performanceLevel": "Mid",
            "potentialLevel": "High"
          },
          "movement": "up"
        }
      ]
    }
  ],
  "summary": {
    "totalAssessed": 15,
    "totalTalents": 18,
    "highPotentialCount": 4
  }
}
```

`movement` の値: `"up"` / `"down"` / `"same"` / `null`（前期データなし）

---

### 2-4. GET /api/nine-box/calibration?periodId= — キャリブレーションデータ（TASK-040）

**認可:** Senior / Admin

**レスポンス（200 OK）:**
```json
{
  "periodId": "uuid",
  "teams": [
    {
      "managerId": "uuid",
      "managerName": "山田 一郎",
      "talentCount": 4,
      "assessedCount": 4,
      "distribution": {
        "highHigh": 1, "highMid": 0, "highLow": 1,
        "midHigh": 1, "midMid": 1, "midLow": 0,
        "lowHigh": 0, "lowMid": 0, "lowLow": 0
      },
      "avgPerformanceScore": 2.5,
      "avgPotentialScore": 2.25
    }
  ],
  "overall": {
    "avgPerformanceScore": 2.3,
    "avgPotentialScore": 2.1,
    "distribution": { "..." }
  }
}
```

---

### 2-5. GET /api/nine-box/history?talentId= — 個人評価履歴（TASK-039）

**認可:** Senior / Admin / 対象タレントの直属 Manager

**レスポンス（200 OK）:**
```json
{
  "talentId": "uuid",
  "talentName": "田中 太郎",
  "history": [
    {
      "periodId": "uuid",
      "periodName": "2025年度通期考課",
      "performanceLevel": "Mid",
      "potentialLevel": "High",
      "label": "原石",
      "assessedAt": "2025-12-01T00:00:00Z"
    },
    {
      "periodId": "uuid",
      "periodName": "2026年度通期考課",
      "performanceLevel": "High",
      "potentialLevel": "High",
      "label": "スター",
      "assessedAt": "2026-05-20T00:00:00Z"
    }
  ]
}
```

---

### 2-6. GET /api/nine-box/cells — セル定義マスタ（TASK-038）

**認可:** 全ロール

**レスポンス（200 OK）:**
```json
[
  {
    "performanceLevel": "High",
    "potentialLevel": "High",
    "label": "スター",
    "description": "業績・ポテンシャルともに最高水準。...",
    "color": "#8b5cf6"
  }
]
```

---

### 2-7. POST /api/nine-box/flag-candidates — 重点管理候補フラグ更新（TASK-040）

内部処理として POST /api/nine-box の副作用で自動実行。外部から直接呼ばれることは想定しないが、Admin 向けに手動再計算エンドポイントとして公開する。

**認可:** Admin

**リクエスト:**
```json
{ "periodId": "uuid" }
```

**処理内容:**
1. 指定期間の High Potential セル（3セル）に該当するタレントを取得
2. 前期の候補リストと差分を計算
3. 新たに候補になったタレントを KeyPerson 候補として推薦フラグを立てる
4. 候補から外れたタレントのフラグを解除
5. 変化があった場合、Admin（人事評価チーム）への通知を生成

---

## 3. フロントエンド設計

### コンポーネント構成

```
pages/
  mdc/
    nine-box/
      index.vue          # 9ボックスマップ画面（Senior/Admin/Manager）
      input.vue          # 評価入力フォーム（Manager）
      calibration.vue    # キャリブレーション画面（Senior/Admin）

components/
  nine-box/
    NineBoxGrid.vue      # 3×3グリッド本体
    NineBoxCell.vue      # 各セル（タレントバッジ一覧）
    TalentBadge.vue      # タレントのイニシャルバッジ
    TalentPopover.vue    # バッジホバー時のポップオーバー
    NineBoxFilters.vue   # 期間・部門・ロールフィルタ
    CalibrationTable.vue # キャリブレーション統計テーブル
```

### data-testid 定義

| 要素 | data-testid | 用途 |
|------|-------------|------|
| グリッド全体 | `nine-box-grid` | グリッドの描画確認 |
| 各セル | `nine-box-cell-{perf}-{pot}` | 例: `nine-box-cell-high-mid` |
| タレントバッジ | `nine-box-talent-{talentId}` | 特定タレントの配置確認 |
| 期間セレクタ | `nine-box-period-select` | 期間切替 |
| 部門フィルタ | `nine-box-dept-filter` | 部門絞込 |
| パフォーマンス選択 | `nine-box-performance-select` | 評価入力フォーム |
| ポテンシャル選択 | `nine-box-potential-select` | 評価入力フォーム |
| パフォーマンスコメント | `nine-box-performance-comment` | 評価入力フォーム |
| ポテンシャルコメント | `nine-box-potential-comment` | 評価入力フォーム |
| 保存ボタン | `nine-box-save-button` | 評価保存 |
| 移動インジケータ | `nine-box-movement-{talentId}` | 前期比較の矢印 |
| 人数バッジ | `nine-box-count-{perf}-{pot}` | 各セルの人数 |

### Pinia ストア設計

```typescript
// stores/nineBox.ts
interface NineBoxState {
  mapData: NineBoxMapResponse | null;
  calibrationData: CalibrationResponse | null;
  cellDefinitions: NineBoxCellDefinition[];
  selectedPeriodId: string | null;
  selectedComparePeriodId: string | null;
  filters: {
    departmentId: string | null;
    role: string | null;
    searchName: string;
  };
  loading: boolean;
  error: string | null;
}

// actions
// fetchMap(periodId, filters) → GET /api/nine-box
// fetchCalibration(periodId) → GET /api/nine-box/calibration
// saveAssessment(req) → POST /api/nine-box
// updateAssessment(id, req) → PUT /api/nine-box/{id}
// fetchCellDefinitions() → GET /api/nine-box/cells
```

---

## 4. サービス層設計

```
Application/NineBox/
  INineBoxService.cs
  NineBoxService.cs
  Requests/
    CreateNineBoxAssessmentRequest.cs
    UpdateNineBoxAssessmentRequest.cs
  Responses/
    NineBoxMapResponse.cs
    NineBoxCalibrationResponse.cs
    NineBoxHistoryResponse.cs
    NineBoxCellResponse.cs
```

**isHighPotentialCandidate の判定ロジック（NineBoxService 内）:**
```csharp
private static bool IsHighPotentialCandidate(PerformanceLevel perf, PotentialLevel pot) =>
    (perf == PerformanceLevel.High && pot >= PotentialLevel.Mid) ||
    (perf == PerformanceLevel.Mid && pot == PotentialLevel.High);
```

---

## 5. 技術的判断の記録

| 決定事項 | 採用した選択 | 理由 | 不採用の選択肢 |
|---------|------------|------|--------------|
| isHighPotentialFlag のDB列 | 持たない（APIで動的計算） | 判定ロジック変更時にデータ不整合が生じるリスクを避ける | DB列として保持（更新漏れリスク）|
| 前期比較データの取得 | comparePeriodId クエリパラメータで1リクエスト内で処理 | N+1クエリを防ぎ、フロント側の状態管理を単純化 | フロントで2回リクエストして差分計算 |
| キャリブレーション統計 | サーバーサイドで計算して返す | 全データをフロントに渡してクライアント計算するとデータ量が多い | クライアントサイド集計 |
| 重点管理候補の自動フラグ | POST 評価保存の副作用として同期実行 | データの即時一貫性を優先 | 非同期バックグラウンドジョブ（インフラ依存が増える）|
