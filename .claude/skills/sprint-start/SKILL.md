# SKILL: sprint-start — 新スプリント開始ウィザード

## 概要

新しいスプリントを対話的に設定し、SDD Project Manager のデータに反映するとともに
GitHub Issues・ブランチ・タスクボードの準備を整えます。

---

## 実行タイミング

- 前スプリントが完了またはふりかえりが終わったとき
- プロジェクト初回スプリントを開始するとき

---

## ウィザード手順

### STEP 1: 前スプリントの確認

```
現在アクティブなスプリントがある場合:
「Sprint N が進行中です。先に sprint-end スキルを実行してください。」
→ 中断する。
```

`tools/pm/js/data.js` が存在する場合、`sdd-pm-data` の `sprints` 配列を確認し、
`status: "active"` のスプリントがないかチェックします。

---

### STEP 2: スプリント情報の収集

```
質問1: スプリント名を教えてください。
       提案: 「Sprint <前回+1>」（前スプリントの番号から自動計算）
       例: Sprint 2

質問2: このスプリントのゴールを教えてください。
       例: 「ダッシュボード機能の完成とパフォーマンス改善」

質問3: 開始日は？（デフォルト: 今日 YYYY-MM-DD）

質問4: 終了日は？（デフォルト: 開始日 + プロジェクトのスプリント期間）
```

---

### STEP 3: タスクの選択

```
backloog（status: pending でスプリント未割り当て）のタスク一覧を表示:

  番号  | Task ID   | タイトル             | SP | Spec ID       | 担当者
  ------|-----------|---------------------|----|---------------|--------
  1     | TASK-005  | ダッシュボードAPI    | 3  | SPEC-DASH-001 | @bob
  2     | TASK-006  | 通知コンポーネント   | 2  | SPEC-NOTIF-001| @carol
  ...

質問: このスプリントに含めるタスク番号をカンマ区切りで入力してください。
      例: 1, 2, 4
      （Enter でスキップ → 全タスクを backloog のままにする）

確認: 選択したタスクの合計 SP: 5 SP
      続けますか？ (Y/n)
```

---

### STEP 4: GitHub Issues の確認（オプション）

```
質問: GitHub Issues とリンクしますか？ (Y/n)
```

Y の場合:

```bash
# スプリントラベルを作成
gh label create "sprint:<スプリント名>" --color "0075ca" --description "Sprint N のタスク"

# 各タスクの issueNumber が設定されていれば自動でラベルを付与
gh issue edit <issueNumber> --add-label "sprint:<スプリント名>"
```

---

### STEP 5: PM ツールへのデータ反映

`tools/pm/js/` 配下に以下の操作を実行するスクリプトを出力し、
ユーザーに「ブラウザコンソールで実行するか、PM ツール上で手動設定してください」と案内します。

```javascript
// コンソールで実行: スプリントを作成してアクティブ化
(function() {
  const data = JSON.parse(localStorage.getItem('sdd-pm-data') || '{}');
  const sprintId = 'SPRINT-' + String((data.sprints || []).length + 1).padStart(3, '0');
  const sprint = {
    id: sprintId,
    name: "<スプリント名>",
    goal: "<ゴール>",
    startDate: "<開始日>",
    endDate: "<終了日>",
    status: "active",
    taskIds: [<選択タスクID配列>],
    retroId: null
  };
  data.sprints = (data.sprints || []).map(s => s.status === 'active' ? {...s, status:'completed'} : s);
  data.sprints.push(sprint);
  // タスクにスプリントIDを付与
  data.tasks = (data.tasks || []).map(t =>
    sprint.taskIds.includes(t.id) ? {...t, sprintId} : t
  );
  data.activityLog = [{
    timestamp: new Date().toISOString(),
    type: 'sprint_started',
    text: `${sprintId} を開始しました`,
    user: data.project.members[0] || '@user'
  }, ...(data.activityLog || [])];
  localStorage.setItem('sdd-pm-data', JSON.stringify(data));
  console.log('✅ スプリント開始完了:', sprintId);
  location.reload();
})();
```

---

### STEP 6: 完了サマリー

```
✅ スプリント開始準備が完了しました！

Sprint:   <スプリント名>
ゴール:   <ゴール>
期間:     <開始日> 〜 <終了日>（N 日間）
タスク:   N 件 / 合計 X SP
担当者:   @alice (3SP), @bob (2SP)

次のアクション:
1. PM ツールで「スプリント管理」を確認する
2. GitHub Issues に sprint ラベルが付いているか確認する
3. 各担当者がブランチを作成して実装を開始する
   例: git checkout -b feat/<issueNumber>-<機能名>
4. デイリースタンドアップで PM ツールのタスクボードを参照する
```

---

## 注意事項

- スプリントゴールは仕様書（Constitution.md・承認済み Spec）と整合していること
- 未承認の Spec に紐付くタスクをスプリントに含める場合は警告を表示:
  「TASK-XXX は未承認の SPEC-XXX-XXX (status: draft) に紐付いています。
   スプリント開始前に仕様書を承認することを推奨します。」

---

## 関連スキル

- `pm-setup` — PM ツールの初期セットアップ
- `sprint-end` — スプリント終了とふりかえりへの橋渡し
- `spec-review` — 仕様書のレビュー支援
- `task-board` — タスク一覧の確認・更新
