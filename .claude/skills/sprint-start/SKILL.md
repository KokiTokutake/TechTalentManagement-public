# /sprint-start — 新スプリント開始ウィザード

**使う場面**: 工程4（タスク管理）。新しいスプリントを開始するとき。スプリントゴールの設定・タスクのアサイン・GitHub Project Iteration の作成を行う。

---

## 目的

GitHub Project の Iteration フィールドを使ってスプリントを管理する。
タスクを選択し、対応する GitHub Issues を Iteration（スプリント）に割り当てる。

---

## 実行手順

### STEP 1: GitHub Project 設定の確認

`.github/project-config.json` が存在し、`project_number` が設定されているか確認する:

```bash
test -f .github/project-config.json && \
  jq -e '.project_number != 0 and .project_number != null' .github/project-config.json
```

**存在しない・または未設定の場合、以下を表示して停止する:**

```
⛔ GitHub Project が設定されていません。

スプリント（Iteration）を作成するには GitHub Project が必要です。

先に実行してください:
  /github-project-init → 既存プロジェクトに GitHub Project を後付けセットアップ
```

### STEP 2: 現在の Iteration 状況を確認

既存の Iteration（スプリント）一覧を取得する:

```bash
PROJECT_ID=$(jq -r '.project_id' .github/project-config.json)
SPRINT_FIELD_ID=$(jq -r '.fields.sprint.id' .github/project-config.json)

gh api graphql -f query='
  query($projectId: ID!) {
    node(id: $projectId) {
      ... on ProjectV2 {
        fields(first: 20) {
          nodes {
            ... on ProjectV2IterationField {
              name
              configuration {
                startDate
                duration
                iterations { id title startDate duration }
              }
            }
          }
        }
      }
    }
  }
' -f projectId="$PROJECT_ID" \
  | jq '.data.node.fields.nodes[] | select(.name == "Sprint") | .configuration'
```

直近のスプリント名から次のスプリント番号を自動提案する。
直近のスプリントがまだ進行中の場合:

```
⚠️ Sprint N がまだ進行中です。先に /sprint-end を実行してください。
```

### STEP 3: スプリント情報の収集

以下を質問する:

```
質問1: スプリント名を教えてください。
       提案: 「Sprint <前回+1>」
       例: Sprint 2

質問2: このスプリントのゴールを教えてください。
       例: 「ダッシュボード機能の完成とパフォーマンス改善」

質問3: 開始日は？（デフォルト: 今日 YYYY-MM-DD）

質問4: 期間は何日間ですか？（デフォルト: 14日）
```

### STEP 4: GitHub Project Iteration の追加

Iteration の追加は `updateProjectV2Field` で行う。
**既存の Iteration リストに新しいものを追加して、フィールド設定ごと上書き**する仕組みのため、
まず現在の Iteration 一覧を取得してから追加する:

```bash
PROJECT_ID=$(jq -r '.project_id' .github/project-config.json)
SPRINT_FIELD_ID=$(jq -r '.fields.sprint.id' .github/project-config.json)

# 既存 Iteration の一覧を取得（title・startDate・duration のみ）
EXISTING_ITERATIONS=$(gh api graphql -f query='
  query($projectId: ID!) {
    node(id: $projectId) {
      ... on ProjectV2 {
        fields(first: 20) {
          nodes {
            ... on ProjectV2IterationField {
              name
              configuration {
                startDate
                duration
                iterations { title startDate duration }
              }
            }
          }
        }
      }
    }
  }
' -f projectId="$PROJECT_ID" \
  | jq '.data.node.fields.nodes[] | select(.name == "Sprint") | .configuration')

FIELD_START_DATE=$(echo "$EXISTING_ITERATIONS" | jq -r '.startDate')
FIELD_DURATION=$(echo "$EXISTING_ITERATIONS" | jq -r '.duration')

# 既存リストに新スプリントを追加したリストを構築
NEW_ITERATIONS=$(echo "$EXISTING_ITERATIONS" \
  | jq --arg title "<スプリント名>" \
       --arg date "<開始日 YYYY-MM-DD>" \
       --argjson dur <日数> \
  '.iterations + [{title: $title, startDate: $date, duration: $dur}]')

# フィールド設定を更新（iterationConfiguration ごと上書き）
RESULT=$(gh api graphql \
  -f query='
    mutation($fieldId: ID!, $startDate: Date!, $duration: Int!, $iterations: [ProjectV2Iteration!]!) {
      updateProjectV2Field(input: {
        fieldId: $fieldId
        iterationConfiguration: {
          startDate: $startDate
          duration: $duration
          iterations: $iterations
        }
      }) {
        projectV2Field {
          ... on ProjectV2IterationField {
            configuration {
              iterations { id title startDate duration }
            }
          }
        }
      }
    }
  ' \
  -f fieldId="$SPRINT_FIELD_ID" \
  -f startDate="$FIELD_START_DATE" \
  -F duration=$FIELD_DURATION \
  --input <(jq -n --argjson iters "$NEW_ITERATIONS" '{"iterations": $iters}'))

# 新しく追加した Iteration の ID を取得
NEW_ITERATION_ID=$(echo "$RESULT" \
  | jq -r '.data.updateProjectV2Field.projectV2Field.configuration.iterations[-1].id')

echo "✅ Iteration 作成完了: $NEW_ITERATION_ID"
```

### STEP 5: タスクの選択とスプリント割り当て

`tasks/task-list.md` からバックログ（Iteration 未割り当て・未着手）のタスクを表示する:

```
📋 バックログ（スプリント未割り当て）:

  番号  | Task ID   | タイトル             | SP | Spec ID       | Issue
  ------|-----------|---------------------|----|---------------|------
  1     | TASK-005  | ダッシュボードAPI    | 3  | SPEC-DASH-001 | #48
  2     | TASK-006  | 通知コンポーネント   | 2  | SPEC-NOTIF-001| #49

このスプリントに含めるタスク番号をカンマ区切りで入力してください。
例: 1, 2
（Enter でスキップ → 全タスクをバックログのままにする）

選択したタスクの合計 SP: 5 SP
続けますか？ (Y/n)
```

選択した各 Issue を作成した Iteration に割り当てる:

```bash
OWNER=$(jq -r '.owner' .github/project-config.json)
PROJECT_NUMBER=$(jq -r '.project_number' .github/project-config.json)

for ISSUE_NUMBER in <選択した Issue 番号のリスト>; do
  ITEM_ID=$(gh project item-list $PROJECT_NUMBER --owner $OWNER --format json \
    | jq -r --argjson n $ISSUE_NUMBER '.items[] | select(.content.number == $n) | .id')

  gh api graphql -f query='
    mutation($projectId: ID!, $itemId: ID!, $fieldId: ID!, $iterationId: String!) {
      updateProjectV2ItemFieldValue(input: {
        projectId: $projectId
        itemId: $itemId
        fieldId: $fieldId
        value: { iterationId: $iterationId }
      }) {
        projectV2Item { id }
      }
    }
  ' -f projectId="$PROJECT_ID" \
    -f itemId="$ITEM_ID" \
    -f fieldId="$SPRINT_FIELD_ID" \
    -f iterationId="$NEW_ITERATION_ID"
done
```

### STEP 6: スプリントラベルの付与

選択した Issue にスプリントラベルを付与する（任意だが推奨）:

```bash
# スプリントラベルを作成（初回のみ）
gh label create "sprint:<スプリント名>" \
  --color "0075ca" \
  --description "<スプリント名> のタスク"

# 各 Issue にラベルを付与
gh issue edit <issue番号> --add-label "sprint:<スプリント名>"
```

### STEP 7: 完了サマリー

```
✅ スプリント開始完了！

Sprint:      <スプリント名>
ゴール:      <ゴール>
期間:        <開始日> 〜 <終了日>（N 日間）
タスク:      N 件 / 合計 X SP
Iteration ID: <ID>

GitHub Project:
  https://github.com/orgs/<owner>/projects/<number>

次のアクション:
1. GitHub Project ボードでタスクの割り当てを確認する
2. 各担当者が /task-start [TASK-ID] でタスクを開始する
3. /task-board でスプリントの進捗を確認する（朝会・進捗確認）
```

---

## 注意事項

- Iteration 追加の仕組み: `updateProjectV2Field` で既存リストに追記してフィールドごと上書きする。既存 Iteration のデータは消えないが、`id` は付与されたもののみ `title`・`startDate`・`duration` を渡せば保持される
- スプリントゴールは Constitution.md・承認済み Spec と整合していること
- 未承認の Spec に紐付くタスクを含める場合は警告を表示する:
  「TASK-XXX は未承認の SPEC-XXX-XXX (status: draft) に紐付いています。
   スプリント開始前に仕様書を承認することを推奨します。」
- `gh auth login` で認証が必要な場合は先に案内する

---

## 関連スキル

- `tasks-plan` — タスクの作成・GitHub Issues 生成
- `sprint-end` — スプリント終了処理
- `task-board` — タスク一覧の確認
- `task-start` — タスク実装の開始
