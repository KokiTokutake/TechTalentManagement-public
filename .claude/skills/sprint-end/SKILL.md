# SKILL: sprint-end — スプリント終了ウィザード

## 概要

アクティブなスプリントを終了し、未完了タスクの処理・メトリクス集計・
ふりかえり（KPT）の作成までを対話的に支援します。

---

## 実行タイミング

- スプリント終了日当日または翌営業日
- `retro-run` スキルの前処理として

---

## ウィザード手順

### STEP 1: アクティブスプリントの確認

`sdd-pm-data` の `sprints` から `status: "active"` のスプリントを特定します。

```
現在のスプリント:
  Sprint N「<ゴール>」
  期間: <開始日> 〜 <終了日>（残 N 日 / 期限超過 N 日）
  タスク: 完了 X / 合計 Y（Z SP 完了 / W SP 計画）
```

アクティブなスプリントがない場合:
```
アクティブなスプリントが見つかりません。
pm-setup または sprint-start を先に実行してください。
```

---

### STEP 2: 未完了タスクの確認と処理

```
未完了タスク（done 以外）:

  Task ID   | タイトル             | ステータス   | SP | 担当者
  ----------|---------------------|-----------|----|--------
  TASK-003  | ダッシュボードAPI    | in_progress | 5 | @bob
  TASK-005  | 通知コンポーネント   | pending     | 2 | @carol

質問: 各タスクをどう処理しますか？
  1 = 次のスプリントに持ち越す（carry over）
  2 = バックログに戻す
  3 = キャンセル（削除）

TASK-003 の処理 (1/2/3): 1  → 次スプリントに持ち越し
TASK-005 の処理 (1/2/3): 2  → バックログへ

確認: 持ち越しタスク: 1件 / バックログ戻し: 1件 / キャンセル: 0件
続けますか？ (Y/n)
```

---

### STEP 3: メトリクスの自動集計

スプリントのタスクデータから以下を自動計算して表示します:

```
📊 スプリントメトリクス（自動集計）:

  計画SP:        15 SP
  完了SP:        10 SP（達成率 67%）
  完了タスク数:   3 / 5 件
  持ち越しタスク: 1 件
  バックログ戻し: 1 件

  仕様先行率（Spec ID あり/全タスク）: 80%
```

追加で以下を質問します:

```
質問: バグ件数（このスプリント中に発生した課題 type=bug の数）: [自動集計値を提示]
質問: 平均 PR レビュー時間（分）を入力してください: 65
質問: CI パス率（%）を入力してください: 92
```

---

### STEP 4: スプリントの終了処理

以下の JavaScript をコンソール実行用に出力します:

```javascript
// コンソールで実行: スプリントを終了する
(function() {
  const data = JSON.parse(localStorage.getItem('sdd-pm-data') || '{}');
  const sprintId = '<対象スプリントID>';

  // スプリントを completed に
  data.sprints = data.sprints.map(s =>
    s.id === sprintId ? {...s, status: 'completed'} : s
  );

  // 持ち越しタスクを次スプリントIDに更新（次スプリント作成後に実施）
  // バックログ戻しタスクの sprintId をクリア
  const backlogTaskIds = [<バックログ戻しTaskID配列>];
  data.tasks = data.tasks.map(t =>
    backlogTaskIds.includes(t.id) ? {...t, sprintId: ''} : t
  );

  data.activityLog = [{
    timestamp: new Date().toISOString(),
    type: 'sprint_completed',
    text: `${sprintId} を終了しました`,
    user: data.project.members[0] || '@user'
  }, ...(data.activityLog || [])];

  localStorage.setItem('sdd-pm-data', JSON.stringify(data));
  console.log('✅ スプリント終了完了:', sprintId);
  location.reload();
})();
```

---

### STEP 5: ふりかえりの作成（KPT 入力）

```
質問: ふりかえりを今すぐ作成しますか？ (Y/n)
```

Y の場合、以下を対話的に収集します:

```
ファシリテーター: [メンバー一覧から選択]

--- Keep（うまくいったこと）---
Keep 1: 仕様書を事前に approved にしてから実装を開始できた
Keep 2: デイリースタンドアップの時間が短縮された
Keep 3: （Enter で終了）

--- Problem（問題・課題）---
Problem 1: テストカバレッジが低いまま PR を出すケースがあった
Problem 2: （Enter で終了）

--- Try（次スプリントで試すこと）---
Try 1: PR 作成前にテストカバレッジを 80% 以上にするルールを設ける
Try 2: （Enter で終了）

--- アクションアイテム ---
アクション 1 のテキスト: PR テンプレートにカバレッジチェック欄を追加
担当者: @alice
期限 (YYYY-MM-DD, Enter でスキップ): 2024-02-02
アクション 2: （Enter で終了）
```

収集したデータをコンソール実行用の JavaScript として出力します:

```javascript
// ふりかえりを作成する
(function() {
  const data = JSON.parse(localStorage.getItem('sdd-pm-data') || '{}');
  const retroId = 'RETRO-' + String((data.retros || []).length + 1).padStart(3, '0');
  const retro = {
    id: retroId,
    sprintId: '<スプリントID>',
    date: '<今日の日付>',
    facilitator: '<ファシリテーター>',
    participants: <参加者配列>,
    keeps:    [<Keepリスト>],
    problems: [<Problemリスト>],
    tries:    [<Tryリスト>],
    metrics: {
      completedSP:      <完了SP>,
      plannedSP:        <計画SP>,
      bugCount:         <バグ件数>,
      avgReviewMinutes: <平均レビュー時間>,
      specFirstRate:    <仕様先行率>,
      ciPassRate:       <CIパス率>
    },
    actionItems: [<アクションアイテムリスト>]
  };
  data.retros = [...(data.retros || []), retro];
  // スプリントに retroId をリンク
  data.sprints = data.sprints.map(s =>
    s.id === retro.sprintId ? {...s, retroId} : s
  );
  localStorage.setItem('sdd-pm-data', JSON.stringify(data));
  console.log('✅ ふりかえり作成完了:', retroId);
  location.reload();
})();
```

---

### STEP 6: 完了サマリー

```
✅ スプリント終了処理が完了しました！

Sprint:    <スプリント名>
期間:      <開始日> 〜 <終了日>
完了SP:    X / Y SP（達成率 Z%）
ふりかえり: RETRO-N を作成しました

次のアクション:
1. PM ツールで「ふりかえり」を確認し、アクションアイテムを共有する
2. 次スプリントを開始する場合は `sprint-start` スキルを実行する
3. 持ち越しタスクの優先度を見直す
4. アクションアイテムを GitHub Issues に追加する（必要であれば）
```

---

## 注意事項

- ふりかえりは当日中に作成することを推奨します（記憶が鮮明なうちに）
- 「Try → 次スプリントの Spec/タスクへの反映」を忘れないよう促してください:
  「Try のアクションアイテムは次スプリントの仕様書やタスクに反映しましたか？」
- 完了 SP が計画 SP の 60% 未満の場合は警告:
  「スプリントゴール達成率が低い状態です。
   タスク見積もりまたはゴール設定の見直しを Constitution.md の変更管理プロセスで検討してください。」

---

## 関連スキル

- `sprint-start` — 次スプリントの開始
- `retro-run` — ふりかえりファシリテーション（詳細版）
- `sdd-health` — SDD 遵守率・健全性チェック
- `pm-setup` — PM ツールの初期セットアップ
