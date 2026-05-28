# /sprint-end — スプリント終了ウィザード

**使う場面**: スプリント終了日当日または翌営業日。`/retro-run` の前処理として実行する。

---

## 目的

アクティブなスプリント（GitHub Project Iteration）を終了し、未完了 Issue の処理・メトリクス集計・ふりかえりの準備を行う。

---

## 実行手順

### STEP 0: GitHub Project 設定の確認

`.github/project-config.json` が存在し、`project_number` が設定されているか確認する:

```bash
test -f .github/project-config.json && \
  jq -e '.project_number != 0 and .project_number != null' .github/project-config.json
```

**存在しない・または未設定の場合、以下を表示して停止する:**

```
⛔ GitHub Project が設定されていません。

スプリント終了処理には GitHub Project が必要です。

先に実行してください:
  /github-project-init → 既存プロジェクトに GitHub Project を後付けセットアップ
```

### STEP 1: アクティブスプリントの確認

GitHub Project の現在の Iteration を確認する:

```bash
OWNER=$(jq -r '.owner' .github/project-config.json)
PROJECT_NUMBER=$(jq -r '.project_number' .github/project-config.json)

gh project item-list $PROJECT_NUMBER --owner $OWNER --format json \
  | jq '[.items[] | {
      title: .fieldValues[] | select(.field.name == "Sprint") | .iterationId,
      status: .fieldValues[] | select(.field.name == "Status") | .name,
      number: .content.number
    }] | group_by(.title)'
```

現在のスプリントを表示する:

```
現在のスプリント:
  Sprint N「<ゴール>」
  期間: <開始日> 〜 <終了日>（残 N 日 / 期限超過 N 日）
  タスク: 完了 X / 合計 Y（Z SP 完了 / W SP 計画）
```

アクティブなスプリントがない場合:
```
アクティブなスプリントが見つかりません。
/sprint-start でスプリントを開始してください。
```

---

### STEP 2: 未完了 Issue の確認と処理

スプリントに割り当てられているが "Done" でない Issue を列挙する:

```bash
gh project item-list $PROJECT_NUMBER --owner $OWNER --format json \
  | jq '.items[] | select(
      (.fieldValues[] | select(.field.name == "Sprint") | .title) == "<スプリント名>"
      and
      (.fieldValues[] | select(.field.name == "Status") | .name) != "Done"
    ) | {number: .content.number, title: .content.title, status: (.fieldValues[] | select(.field.name == "Status") | .name)}'
```

各未完了 Issue についてどう処理するか確認する:

```
未完了 Issue:

  #48  ダッシュボード API（In Progress）
  #49  通知コンポーネント（Todo）

各 Issue の処理方法を選んでください:
  1 = 次のスプリントに持ち越す（Issue をオープンのまま残す）
  2 = バックログに戻す（Sprint フィールドをクリア）
  3 = クローズ（キャンセル・不要）

#48 の処理 (1/2/3): 1  → 次スプリントに持ち越し
#49 の処理 (1/2/3): 2  → バックログへ

確認: 持ち越し: 1件 / バックログ戻し: 1件 / クローズ: 0件
続けますか？ (Y/n)
```

バックログに戻す Issue の Sprint フィールドをクリアする:

```bash
gh api graphql -f query='
  mutation($projectId: ID!, $itemId: ID!, $fieldId: ID!) {
    clearProjectV2ItemFieldValue(input: {
      projectId: $projectId
      itemId: $itemId
      fieldId: $fieldId
    }) {
      projectV2Item { id }
    }
  }
' -f projectId="$PROJECT_ID" \
  -f itemId="$ITEM_ID" \
  -f fieldId="$SPRINT_FIELD_ID"
```

クローズする Issue は `gh issue close <番号> --comment "スプリント終了によりクローズ"` で処理する。

---

### STEP 3: メトリクスの集計

GitHub Issue・Project データからメトリクスを算出して表示する:

```bash
# 完了した Issue 数と SP を集計
gh project item-list $PROJECT_NUMBER --owner $OWNER --format json \
  | jq '[.items[] | select(
      (.fieldValues[] | select(.field.name == "Sprint") | .title) == "<スプリント名>"
    )] | {
      total: length,
      done: [.[] | select(.fieldValues[] | select(.field.name == "Status") | .name == "Done")] | length,
      total_sp: [.[].fieldValues[] | select(.field.name == "Story Points") | .number] | add,
      done_sp: [.[] | select(.fieldValues[] | select(.field.name == "Status") | .name == "Done") | .fieldValues[] | select(.field.name == "Story Points") | .number] | add
    }'
```

```
📊 スプリントメトリクス（自動集計）:

  計画SP:        15 SP
  完了SP:        10 SP（達成率 67%）
  完了 Issue:    3 / 5 件
  持ち越し:      1 件
  バックログ戻し: 1 件
```

追加で以下を質問する:

```
質問: バグ件数（このスプリント中に発生した bug ラベルの Issue 数）: [自動集計値]
質問: 平均 PR レビュー時間（分）を入力してください: 65
質問: CI パス率（%）を入力してください: 92
```

---

### STEP 4: ふりかえりの準備（KPT）

```
質問: ふりかえりを今すぐ作成しますか？ (Y/n)
```

Y の場合、以下を対話的に収集する:

```
--- Keep（うまくいったこと）---
Keep 1: 仕様書を事前に approved にしてから実装を開始できた
Keep 2: （Enter で終了）

--- Problem（問題・課題）---
Problem 1: テストカバレッジが低いまま PR を出すケースがあった
Problem 2: （Enter で終了）

--- Try（次スプリントで試すこと）---
Try 1: PR 作成前にテストカバレッジを 80% 以上にするルールを設ける
Try 2: （Enter で終了）
```

収集した KPT を `docs/guides/retrospective.md` に追記する（日付・スプリント名付き）。

アクションアイテムがある場合は GitHub Issue として登録することを提案する:

```
アクションアイテムを GitHub Issue として登録しますか？
例: 「PR テンプレートにカバレッジチェック欄を追加」

Y の場合:
gh issue create --title "[Retro Action] PR テンプレートにカバレッジチェック欄を追加" \
  --label "improvement" \
  --assignee "<担当者>"
```

---

### STEP 4.5: task-list.md と GitHub Project の同期チェック

GitHub Project の "Done" 件数と `tasks/task-list.md` の `✅ done` 件数に乖離がないか確認する:

```bash
# task-list.md の done 件数
grep -c "✅ done" tasks/task-list.md 2>/dev/null

# task-list.md の "未着手" ヘッダーと Done なタスク行の矛盾を確認
grep -n "⬜ 未着手\|進捗.*0 / " tasks/task-list.md | head -5
```

**乖離が見つかった場合:**
```
⚠️ task-list.md のスプリントヘッダーが実態と異なります。

  例: Sprint 18 が「⬜ 未着手 / 0 SP」だが、全タスクが ✅ done

  task-list.md を実態に合わせて更新してよいですか？ (Y/n)
```

Y の場合、スプリントヘッダーを `✅ 完了` に、進捗を実際の値に自動更新し、
サマリーセクションの集計値も修正する。

> task-list.md は手動更新が前提だが、sprint-end 時に自動同期することで乖離を防ぐ。

### STEP 5: 完了サマリー

```
✅ スプリント終了処理完了！

Sprint:   <スプリント名>
期間:     <開始日> 〜 <終了日>
完了SP:   X / Y SP（達成率 Z%）

GitHub Project:
  https://github.com/orgs/<owner>/projects/<number>

次のアクション:
1. GitHub Project ボードで持ち越しタスクを確認する
2. /retro-run でふりかえりを実施する（詳細版）
3. /sprint-start で次のスプリントを開始する
4. アクションアイテムの Issue が作成された場合は優先度を設定する
```

---

## 注意事項

- 完了 SP が計画 SP の 60% 未満の場合は警告:
  「スプリントゴール達成率が低い状態です。
   タスク見積もりまたはゴール設定の見直しを Constitution.md の変更管理プロセスで検討してください。」
- ふりかえりは当日中に作成することを推奨する（記憶が鮮明なうちに）

---

## 関連スキル

- `sprint-start` — 次スプリントの開始
- `retro-run` — ふりかえりファシリテーション（詳細版）
- `sdd-health` — SDD 遵守率・健全性チェック
- `task-board` — タスク一覧の確認
