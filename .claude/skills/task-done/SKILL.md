# /task-done — タスク完了・PR 作成

**使う場面**: 工程5（実装）。実装が完了し、PR を作成してレビューに出すとき。

---

## 目的

受入基準の達成確認・PR の作成・GitHub Issue のクローズ・Project ステータス更新を漏れなく実施し、
仕様書との整合性が担保された状態でレビューに出す。

---

## 実行手順

### STEP -1: GitHub Project 設定の確認

`.github/project-config.json` が存在し、`project_number` が設定されているか確認する:

```bash
test -f .github/project-config.json && \
  jq -e '.project_number != 0 and .project_number != null' .github/project-config.json
```

**存在しない・または未設定の場合:**

```
⚠️ .github/project-config.json が見つかりません。

GitHub Project のステータス更新（STEP 6）はスキップされます。
PR 作成・受入基準チェック・テスト確認は通常通り実行します。

GitHub Project を設定する場合:
  /github-project-init → 後付けセットアップ
```

`project-config.json` がなくても PR 作成までは進める。STEP 6・STEP 8 の Project 更新のみスキップする。

### STEP 0: ゲートチェック（必須・失敗時は即停止）

```bash
git branch --show-current
```

| 結果 | 対応 |
|------|------|
| `main` | **即時停止**。「mainブランチで実装しています。`/task-start [TASK-ID]` を実行してfeatureブランチで作業してください。PRを作成できません。」と表示し、STEP 1 以降を実行しない |
| `feat/...` など | 正常。STEP 1 へ進む |

> このゲートはユーザーが「続けて」と言っても通過させてはならない。

### STEP 1: 現在のブランチ・タスクの確認

現在のブランチ名からタスク ID と GitHub Issue 番号を特定し、対応するタスク情報を表示する。

```
現在のブランチ: feat/42-login-api
対応タスク: TASK-001（ログイン API エンドポイント実装）
GitHub Issue: #42
対応仕様書: SPEC-AUTH-001
```

### STEP 2: 受入基準のセルフチェック

仕様書の受入基準を一覧表示し、各項目を確認する:

```
📋 受入基準チェック（SPEC-AUTH-001）

以下を一つずつ確認してください:
[ ] Given: 正しいメールアドレスとパスワードが入力された → HTTP 200 と JWT が返る
[ ] Given: 誤ったパスワードが入力された → HTTP 401 が返る
[ ] ...

全て ✅ になりましたか？
```

未確認の項目がある場合は「まだ実装されていない受入基準があります。実装を完了してから再度 /task-done を実行してください」と伝えて**停止する**。

### STEP 2.5: E2Eテストファイルの存在チェック（必須ゲート）

タスクに紐付く Spec ID に対して、以下のパスにテストファイルが存在するか確認する:

```
tests/e2e/[SPEC-ID]-*.spec.ts
```

| 結果 | 対応 |
|------|------|
| テストファイルが存在する | 正常。STEP 3 へ進む |
| テストファイルが存在しない | **即時停止**（下記） |
| `[DB]` / `[DB Migration]` タグのタスク | 免除条件を確認（下記） |

**テストファイルが存在しない場合:**
```
⛔ E2Eテストファイルが見つかりません。

  対象仕様書: [SPEC-ID]
  期待するパス: tests/e2e/[SPEC-ID]-[feature-name].spec.ts

PR を作成する前に E2E テストを作成してください:

  /test-write

テストが全通過してから、再度 /task-done を実行してください。
```

> このゲートはユーザーが「スキップして」と言っても通過させてはならない。

### STEP 3: テストの実行・通過率チェック（必須ゲート）

テストを自動実行し、**結果ファイルを読み込んで通過率を計算する**（確認で止めない）:

```bash
# E2E テスト（JSON レポート出力必須）
npx playwright test tests/e2e/[SPEC-ID]-*.spec.ts --reporter=json 2>&1 | tail -5

# 結果ファイルが出力された場合は読み込む
cat tests/reports/results.json 2>/dev/null | python3 -c "
import sys, json
d = json.load(sys.stdin)
total = d.get('stats', {}).get('expected', 0)
passed = d.get('stats', {}).get('passed', 0)
skipped = d.get('stats', {}).get('skipped', 0)
failed = total - passed - skipped
print(f'通過: {passed} / 実行: {total-skipped} (スキップ: {skipped} / 失敗: {failed})')
" 2>/dev/null || echo "(結果ファイル未生成)"
```

**判定基準:**

| 状態 | 対応 |
|------|------|
| 全テスト通過（skip 0件） | 正常。STEP 4 へ進む |
| 一部スキップあり（skip > 0）| **必ず理由を確認する**（下記） |
| 失敗あり | **即時停止**。テストを修正してから再実行 |

**`test.skip()` が存在する場合の必須確認:**
```
⚠️ スキップされたテストが X 件あります。

スキップ理由を確認してください:
  A) 外部サービス（Azure Functions 等）が不要 → 適切。理由をコメントに記録して続行
  B) シードデータ未整備 → シードデータを追加してからテストを再実行
  C) 実装未完了 → 実装を完了してからテストを再実行
  D) テスト設計の問題 → テストを修正してから再実行

理由を教えてください（記録用）:
```

スキップ理由が「外部サービス不可避」以外の場合は**停止する**。
`test.skip()` にはスキップ理由を第2引数として必ず記録させる（例: `test.skip(true, 'Azure Functions TimerTrigger: E2E不可')`）。

> このゲートはユーザーが「スキップして」と言っても通過させてはならない。

### STEP 4: コードの最終確認

```bash
git diff --stat main
git status
```

未コミットの変更がある場合は警告し、コミットするよう促す。

### STEP 5: PR の作成

Issue 番号を含む PR タイトルで作成する（GitHub が自動で Issue にリンクする）:

```bash
gh pr create \
  --title "feat: [TASK-ID] [タスクタイトル]（SPEC-XXX-001）" \
  --body "$(cat <<'EOF'
## 概要

SPEC-XXX-001 の受入基準を実装する。

Closes #<issue番号>

## 対応仕様書

- Spec ID: SPEC-XXX-001
- パス: docs/spec/xxx/spec-xxx-001-feature.md

## 対応設計書

docs/design/SPEC-XXX-001-design.md

## 受入基準チェック

- [x] Given: ... When: ... Then: ...
- [x] Given: ... When: ... Then: ...

## テスト

- E2Eテスト: tests/e2e/SPEC-XXX-001-feature.spec.ts（N件 全通過）
- ユニットテスト: N件 全通過

## 変更ファイル

<!-- git diff --stat main の結果 -->
EOF
)" \
  --label "enhancement"
```

内容を表示してユーザーに確認してから実行する。

**`Closes #<issue番号>`** を PR 本文に含めることで、PR マージ時に Issue が自動的にクローズされる。

### STEP 6: GitHub Project のステータス更新（In Review）

PR 作成後、対応する Project アイテムのステータスを "In Review" に更新する:

```bash
OWNER=$(jq -r '.owner' .github/project-config.json)
PROJECT_NUMBER=$(jq -r '.project_number' .github/project-config.json)
PROJECT_ID=$(jq -r '.project_id' .github/project-config.json)
STATUS_FIELD_ID=$(jq -r '.fields.status.id' .github/project-config.json)
IN_REVIEW_ID=$(jq -r '.fields.status.options.in_review' .github/project-config.json)

ITEM_ID=$(gh project item-list $PROJECT_NUMBER --owner $OWNER --format json \
  | jq -r '.items[] | select(.content.number == <issue番号>) | .id')

gh api graphql -f query='
  mutation($projectId: ID!, $itemId: ID!, $fieldId: ID!, $optionId: String!) {
    updateProjectV2ItemFieldValue(input: {
      projectId: $projectId
      itemId: $itemId
      fieldId: $fieldId
      value: { singleSelectOptionId: $optionId }
    }) {
      projectV2Item { id }
    }
  }
' -f projectId="$PROJECT_ID" \
  -f itemId="$ITEM_ID" \
  -f fieldId="$STATUS_FIELD_ID" \
  -f optionId="$IN_REVIEW_ID"
```

### STEP 7: レビュアーの設定

CODEOWNERS の設定に基づいてレビュアーを確認する。
ソロ開発の場合はスキップしてよい。

### STEP 8: タスクステータスの自動更新

`tasks/task-list.md` の該当タスクを「進行中」→「🔵 レビュー中」に更新する。

他に「🏃 進行中」のタスクがあれば列挙して確認する:

```
⚠️ 他に「進行中」のタスクがあります:
  TASK-039  [BE] 9ボックスマップ API（#53）

これらもこのPRに含まれますか？
  Y) はい → まとめて 🔵 レビュー中 に更新する
  N) いいえ → そのまま進行中として残す
```

### STEP 9: CI確認・マージ確認

PR 作成後、CI の通過を待ってからマージを提案する:

```bash
gh pr checks <PR番号>
```

| 結果 | 対応 |
|------|------|
| CI 全通過 | ユーザーに「マージしますか？」と確認する |
| CI 失敗 | 「CI が失敗しています。修正してから再度確認してください」と伝えて**停止する** |

**ユーザーが「マージする」と回答した場合のみ**、以下を実行する:

```bash
gh pr merge <PR番号> --squash --delete-branch
git checkout main
git pull origin main
```

マージ後、`Closes #<issue番号>` により GitHub Issue が自動クローズされる。

Project アイテムのステータスが自動で "Done" にならない場合は手動更新する:

```bash
DONE_ID=$(jq -r '.fields.status.options.done' .github/project-config.json)

gh api graphql -f query='
  mutation($projectId: ID!, $itemId: ID!, $fieldId: ID!, $optionId: String!) {
    updateProjectV2ItemFieldValue(input: {
      projectId: $projectId
      itemId: $itemId
      fieldId: $fieldId
      value: { singleSelectOptionId: $optionId }
    }) {
      projectV2Item { id }
    }
  }
' -f projectId="$PROJECT_ID" \
  -f itemId="$ITEM_ID" \
  -f fieldId="$STATUS_FIELD_ID" \
  -f optionId="$DONE_ID"
```

`tasks/task-list.md` の該当タスクを「✅ done」に更新する。

### STEP 10: 完了報告

```
✅ マージ完了

PR:     https://github.com/<owner>/<repo>/pull/<番号>
Issue:  #<番号>（クローズ済み）
ブランチ: feat/<番号>-<タスク概要> → main（スカッシュマージ・削除済み）
E2Eテスト: N件 全通過

次のアクション:
  /task-board でタスクの進捗を確認できます
  次のタスクは /task-start [TASK-ID] で開始してください
```

---

## 注意事項

- STEP 0 のゲートはいかなる場合も省略・バイパスしない
- STEP 2.5 のゲートはいかなる場合も省略・バイパスしない（`[DB]`免除条件を除く）
- PR 本文に `Closes #<issue番号>` を含めることで Issue の自動クローズを活用する
- マージは必ずユーザーの承認を得てから実行する
