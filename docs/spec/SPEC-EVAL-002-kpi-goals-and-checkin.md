# SPEC-EVAL-002: Mustの定量目標(KPI)・中間振り返り・フィードバック面談記録

| 項目 | 内容 |
|------|------|
| Spec ID | SPEC-EVAL-002 |
| バージョン | v1.0.0 |
| 作成日 | 2026-05-20 |
| ステータス | **approved** |
| オーナー | @KokiTokutake |

---

## 1. 背景・目的（Why）

### 1.1 解決する問題

現状の考課（SPEC-EVAL-001）では、Mustの目標に対して「達成度スコア（1〜5）」のみを評価するため、目標そのものの定量性・測定可能性が担保されていない。また、考課期間終了時の最終評価のみで、期中の進捗確認（チェックイン）の仕組みがない。上長から従業員へのフィードバック面談の内容も記録されていない。

### 1.2 ビジネス価値

- OKR・MBO の考え方を参考に、Must目標にKPI（数値指標）を設定することで、客観的な達成度評価が可能になる
- 期中チェックイン（四半期・月次）により、目標未達リスクを早期に検知し軌道修正できる
- フィードバック面談の記録をシステム化することで、評価の透明性と納得感を向上させる

### 1.3 対象ユーザー

| ユーザー | 権限 |
|---------|------|
| タレント（一般エンジニア） | 自分のKPI目標・チェックイン記録を閲覧、自己チェックインを入力 |
| 上長（マネージャー） | 配下タレントのKPI目標設定・チェックイン・フィードバック面談記録 |
| 管理者（人事評価チーム） | 全タレントのKPI・チェックイン状況を閲覧 |

---

## 2. 要件（What）

### REQ-01: Mustへの定量KPI設定 【Must】

Mustの各目標に、以下の要素で構成するKPIを設定できる：
- 指標名（例: プロジェクトデリバリー件数）
- 目標値（数値または定性基準）
- 単位（件・%・点など、フリーテキスト）
- 測定方法（どこで/どうやって確認するか、任意）
- 期限

考課期間の目標設定フェーズ（status: goal_setting）でのみ入力可能。

### REQ-02: 進捗チェックイン 【Must】

目標ごとに、現在の進捗値と進捗コメントを記録できる「チェックイン」機能を追加する。
チェックインは期中いつでも記録でき、時系列で一覧表示する。
チェックインの進捗率（目標値比）を自動計算して表示する。

### REQ-03: 期中リマインダー通知 【Should】

考課期間の中間時点（期間の50%経過時）に、未チェックインの目標がある場合、タレント・上長に通知する。

### REQ-04: フィードバック面談記録 【Must】

上長が考課期間の最終評価時に実施するフィードバック面談の内容を記録できる：
- 面談実施日
- タレントへの総合評価コメント
- 次期への期待事項
- タレントの反応・コメント（上長が記録）

### REQ-05: タレントへの評価開示制御 【Should】

最終評価のフィードバック面談記録は、「開示」操作を行った後にタレント本人が閲覧できるようにする。開示前は上長・MDCのみ閲覧可能。

> **SPEC-EVAL-001 との関係**: SPEC-EVAL-001 REQ-06（全段階承認完了後の開示）は評価コメント全体の可視化を定義する。本 REQ-05 はフィードバック面談記録（FeedbackMeeting）に限定した追加の開示制御であり、SPEC-EVAL-001 REQ-06 の承認完了後に上長が明示的な開示操作を行うことで公開される。

### REQ-06: 評価較正との連動 【Could】

チェックインデータをMDC-002の9ボックス評価のパフォーマンス入力に参考値として表示する。

---

## 3. スコープ外（作らないこと）

- リアルタイムKPIダッシュボード（外部BI連携）
- 目標の自動スコアリング・AI評価
- ピアフィードバック（360度評価）
- 給与・昇格の自動連動

---

## 4. 受入基準

### REQ-01: Mustへの定量KPI設定

**AC-01-01: KPIの入力と保存**
- Given: 考課期間が「目標設定中（goal_setting）」ステータスで、タレントがMust目標設定画面を開いている
- When: 目標タイトル「プロジェクトデリバリー件数」・目標値「12」・単位「件」・測定方法「PM報告書で確認」を入力して保存する
- Then: KPI情報が目標と紐付いて保存される

**AC-01-02: 評価期間外の入力制限**
- Given: 考課期間が「自己評価中（self_evaluation）」ステータスである
- When: タレントがKPI設定を変更しようとする
- Then: 「目標設定期間が終了しています」とメッセージが表示され、KPI値は編集できない

### REQ-02: 進捗チェックイン

**AC-02-01: チェックインの記録**
- Given: タレントがKPIの設定された目標の詳細画面を開いている
- When: 現在値「8」・コメント「Q2で4件完了、Q3でペース維持中」を入力してチェックインを保存する
- Then: チェックインが記録され、進捗率「67%（8/12件）」が自動計算されて表示される

**AC-02-02: チェックイン履歴の表示**
- Given: タレントAの目標に3回のチェックインが記録されている
- When: 目標の詳細画面を開く
- Then: チェックイン履歴が時系列（新しい順）で表示される（日時・現在値・コメント）

**AC-02-03: 進捗率の色分け表示**
- Given: チェックインデータが存在する
- When: 目標一覧でチェックイン進捗を確認する
- Then: 進捗率80%以上は緑、50〜79%は黄、49%以下は赤で表示される

### REQ-03: リマインダー通知

**AC-03-01: 中間リマインダー送信**
- Given: 考課期間の開始から50%の日程が経過した
- When: バッチジョブが実行される（日次）
- Then: チェックインが0回の目標を持つタレントと上長に通知（メールまたはシステム内通知）が送信される

### REQ-04: フィードバック面談記録

**AC-04-01: 面談記録の作成**
- Given: 考課期間が「最終評価中（final_evaluation）」ステータスで、上長がフィードバック面談記録画面を開いている
- When: 面談日・評価コメント・次期期待事項を入力して保存する
- Then: フィードバック面談記録が保存され、開示ステータスは「未開示」となる

### REQ-05: 評価開示制御

**AC-05-01: 開示操作**
- Given: フィードバック面談記録が保存されている（未開示状態）
- When: 上長が「タレントに開示」ボタンをクリックして確認ダイアログで「開示する」を選択する
- Then: 開示フラグがONになり、タレントが自分の評価コメント・フィードバック内容を閲覧できるようになる

**AC-05-02: 開示前の非表示**
- Given: フィードバック面談記録が未開示状態である
- When: タレントが自分の評価結果画面を開く
- Then: 「まだ開示されていません。上長のフィードバック面談をお待ちください」と表示され、評価コメントは表示されない

---

## 5. データモデル（概要）

```
GoalKpi {
  id: string (ULID)
  goalId: string → Goal.id
  metricName: string (max: 200)    # 指標名
  targetValue: decimal             # 目標値
  unit: string (max: 50)           # 単位
  measurementMethod: string | null (max: 500)  # 測定方法
  dueDate: date | null
  createdAt: datetime
  updatedAt: datetime
}

GoalCheckin {
  id: string (ULID)
  goalId: string → Goal.id
  kpiId: string | null → GoalKpi.id
  talentId: string → User.id
  currentValue: decimal | null     # 現在値
  progressPercent: decimal | null  # 自動計算: targetValue > 0 の場合 currentValue / targetValue * 100、targetValue = 0 の場合は null
  comment: string | null (max: 1000)
  checkedInAt: datetime
}

FeedbackMeeting {
  id: string (ULID)
  evaluationId: string → SelfEvaluation.id  # 自己評価レコードと紐付け
  talentId: string → User.id
  managerId: string → User.id
  conductedAt: date
  overallComment: string | null (max: 3000)   # 総合評価コメント
  nextPeriodExpectation: string | null (max: 2000)  # 次期への期待
  talentReactionNote: string | null (max: 1000)    # タレントの反応（上長記録）
  isDisclosed: boolean (default: false)
  disclosedAt: datetime | null
  createdAt: datetime
  updatedAt: datetime
}
```

---

## 6. APIエンドポイント（概要）

```
POST /api/goals/{goalId}/kpi                          → KPI設定（Talent/Manager、目標設定期間のみ）
PUT  /api/goals/{goalId}/kpi                          → KPI更新（同上）

POST /api/goals/{goalId}/checkins                     → チェックイン記録（Talent/Manager）
GET  /api/goals/{goalId}/checkins                     → チェックイン履歴取得

POST /api/feedback-meetings                           → フィードバック面談記録作成（Manager）
GET  /api/feedback-meetings?talentId=&periodId=       → 面談記録取得（Manager/MDC/Talent[開示済]）
PATCH /api/feedback-meetings/{id}/disclose            → 評価開示（Manager）
```

---

## 7. 非機能要件

| 項目 | 要件 |
|------|------|
| パフォーマンス | チェックイン記録・一覧取得を1秒以内に返す |
| セキュリティ | フィードバック面談記録は開示フラグに基づいてアクセス制御する。未開示はタレント本人に非表示 |
| 通知 | リマインダーはAzure Functions（TimerTrigger）で実装する |
| 監査ログ | 評価開示操作はAuditLogに記録する |

---

## 8. 関連仕様

- SPEC-EVAL-001: 考課ワークフロー（考課期間・目標設定フェーズの基盤）
- SPEC-CAREER-001: キャリアプラン管理（Mustの目標との紐付け）
- SPEC-MDC-002: 9ボックス評価（チェックインデータの参考連携）
- SPEC-1ON1-001: 1on1面談記録（フィードバック面談との区別・連携）

---

## 変更履歴

| バージョン | 日付 | 変更内容 | 変更者 |
|-----------|------|---------|--------|
| v1.0.0 | 2026-05-20 | 初版作成 | @KokiTokutake |
