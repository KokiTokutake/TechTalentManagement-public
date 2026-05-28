# /task-start — タスク開始・作業コンテキストの準備

**使う場面**: 工程4・5（タスク管理・実装）。タスクの実装を開始するとき。ブランチ作成から仕様書読み込みまで一気に準備する。

> ⛔ **このスキルを実行せずに実装を開始してはならない（CLAUDE.md 必須ルール）。**
> ブランチが作成されるまでコードを一切書いてはならない。

---

## 目的

タスクに関連する仕様書・設計書・受入基準を読み込んだ上でブランチを作成し、
「何を作るべきか」を明確にした状態で実装を開始できるようにする。

---

## 実行手順

### STEP 1: GitHub Project 設定の確認

`.github/project-config.json` が存在し、`project_number` が設定されているか確認する:

```bash
test -f .github/project-config.json && \
  jq -e '.project_number != 0 and .project_number != null' .github/project-config.json
```

**存在しない・または `project_number` が 0 の場合、以下を表示して停止する:**

```
⛔ GitHub Project が設定されていません。

  .github/project-config.json が見つからないか、project_number が未設定です。

セットアップを先に実行してください:

  /github-project-init  → 既存プロジェクトに GitHub Project を後付けセットアップ
  /sdd-start            → 新規プロジェクトの初回セットアップ（リポジトリ作成から行う場合）
```

> `project-config.json` がなくても **ブランチ作成・仕様書読み込みだけ** 行う場合は続行できる。
> ただし STEP 10（GitHub Project ステータス更新）はスキップし、「`/github-project-init` でセットアップ後に手動でステータスを更新してください」と案内する。

### STEP 2: 現在のgit状態チェック（必須ゲート）

```bash
git branch --show-current
git status --short
```

**以下の状態を確認し、問題があれば中断する:**

| 状態 | 対応 |
|------|------|
| mainブランチ以外にいる | 「現在 `[branch]` ブランチにいます。別タスクが進行中ですか？」と確認してから続行 |
| 未コミット変更がある | 「未コミットの変更があります。コミットしてからタスクを開始してください」と警告して中断 |
| mainブランチ + 変更なし | 正常。STEP 3 へ進む |

### STEP 3: 未クローズの進行中タスク検出（必須）

`tasks/task-list.md` を読み込み、ステータスが「🏃 進行中」のタスクを列挙する。

```
⚠️ 以下のタスクが「進行中」のままです:
  TASK-038  [BE] 9ボックス評価 API（#52）
  TASK-039  [BE] 9ボックスマップ API（#53）

これらは実際に完了していますか？
  A) 完了している → task-list.md を ✅ done に更新し、GitHub Issue をクローズしてから続行
  B) まだ未完了   → そのタスクを先に完了させてください（/task-done）
  C) 無視して続行 → 説明を入力してください（記録用）
```

**判断基準:**
- ブランチが既に main にマージされている → 完了。ステータスを `✅ done` に更新してから続行
- ブランチがまだ生きている → 別タスクが本当に進行中。ユーザーに確認
- 判断できない場合 → `git log --oneline -20` でマージ済みかを確認

> このチェックにより「進行中」ステータスの放置を防ぐ。
> 進行中タスクがなければ STEP 4 へ進む。

### STEP 4: タスクの選択

引数でタスク ID が指定された場合はそのタスクを使用する。
`/task-start TASK-001` のように呼び出す。

指定がない場合は `tasks/task-list.md` から未着手タスクを表示し、どれを開始するか確認する:

```
📋 未着手のタスク:
  TASK-001  ログイン API エンドポイント実装  SP:3  [SPEC-AUTH-001]  #42
  TASK-002  ログイン画面 UI 実装             SP:2  [SPEC-AUTH-001]  #43  ※ TASK-001 に依存
  TASK-004  アカウントロック機能実装          SP:3  [SPEC-AUTH-001]  #45

どのタスクを開始しますか？
```

依存タスクが完了していない場合は「TASK-002 は TASK-001 が完了してから開始できます」と警告する。

### STEP 5: アサインの確認（必須）

選択したタスクの GitHub Issue にアサイン済みメンバーがいるか確認する。
Issue 番号が `tasks/task-list.md` に記載されている場合:

```bash
gh issue view <issue番号> --json assignees --jq '.assignees[].login'
```

現在の GitHub ユーザーを取得して照合する:

```bash
CURRENT_USER=$(gh api user --jq '.login')
```

| アサイン状態 | 対応 |
|------------|------|
| アサインなし | 正常。自分をアサインしてから続行する |
| 自分がアサイン済み | 正常。そのまま続行する |
| **自分以外がアサイン済み** | **確認を取る（下記）** |

**自分以外がアサイン済みの場合、以下を表示して必ず確認する:**

```
⚠️ このタスクはすでに @<assignee> にアサインされています。

  TASK-001: ログイン API エンドポイント実装（#42）
  アサイン: @<assignee>

このタスクを進めてよいですか？
  Y) はい → @<assignee> に連絡済み、または引き継ぎを確認した
  N) いいえ → 別のタスクを選ぶ
```

**N の場合は中断し、タスク選択（STEP 4）に戻る。**
**Y の場合のみ続行する。** アサインを自分に変更するかも確認する:

```
アサインを自分（@<current_user>）に変更しますか？
  Y) 変更する → gh issue edit <番号> --add-assignee @me --remove-assignee @<assignee>
  N) そのまま（@<assignee> のままにする）
```

GitHub Issue がない場合（`tasks/task-list.md` に Issue 番号なし）は、
`tasks/task-list.md` の担当列を確認し、空でなければ同様に確認を取る。

> このチェックは「タスクは誰でも進められる」という前提を維持しつつ、
> アサイン済みメンバーの作業との衝突・二重対応を防ぐために存在する。
> ユーザーが「スキップして」と言っても省略しない。

### STEP 5.5: 認証状態の確認（情報収集）

実装前に認証システムの状態を確認し、テスト設計に反映する:

```bash
# 本番認証（Entra ID 等）の実装状況を確認
grep -r "TASK.*SSO\|TASK.*Entra\|TASK.*Auth\|認証.*pending\|⬜.*認証" tasks/task-list.md 2>/dev/null | head -3
```

| 状態 | 対応 |
|------|------|
| 認証が実装済み（done） | 正常。テストで本物の認証フローを使う |
| 認証が未実装（pending/⬜） | **認証代替モードを記録する**（下記） |

**認証が未実装の場合（重要）:**
```
ℹ️ 認証システムが未実装です。

  以下を CLAUDE.md の「認証状態」セクションに記録します（なければ追記）:
  - 認証代替方法: /select-user（ユーザー選択 UI）
  - 影響するテスト: E2E テストで認証フローを省略し、select-user で代替する
  - 制約: 認証後ルーティング・トークン期限・権限チェックのテストは不可能

  E2E テスト生成時に /test-write へこの情報を引き継ぐこと。
  認証依存の受入基準（AC）は test.skip('認証未実装のため: TASK-XXX 完了後に再有効化') でスキップし、
  スキップ理由を明記する。
```

> このチェックはスキップせず、認証状態を必ず記録する。記録は次の /test-write で活用される。

### STEP 6: 関連ファイルの読み込み

選択したタスクに紐付く以下を読み込む:
- 対応する仕様書（Spec ID から `docs/spec/` を検索）
- 関連する設計書（`docs/design/`）
- Constitution.md のコーディングルールセクション
- CLAUDE.md の「認証状態」セクション（あれば）

### STEP 7: 設計書の存在・承認チェック（必須ゲート）

タスクに紐付く Spec ID に対して、以下を **両方** 確認する:

1. `docs/design/[SPEC-ID]-design.md` が存在すること
2. そのファイル内に `| ステータス | approved |` の行が含まれること

| 結果 | 対応 |
|------|------|
| 存在し、ステータスが `approved` | 正常。STEP 8 へ進む |
| 設計書が存在しない | **即時停止**（下記メッセージA） |
| 存在するがステータスが `draft` | **即時停止**（下記メッセージB） |

**メッセージA — 設計書が存在しない場合:**
```
⛔ 設計書が見つかりません。

  対象仕様書: [SPEC-ID]
  期待するパス: docs/design/[SPEC-ID]-design.md

実装を開始する前に設計書を作成・承認してください:

  /design-create [SPEC-ID]

設計書が approved になってから、再度 /task-start を実行してください。
```

**メッセージB — 設計書が draft のまま承認されていない場合:**
```
⛔ 設計書がまだ承認されていません（ステータス: draft）。

  対象設計書: docs/design/[SPEC-ID]-design.md

設計書を /design-create の STEP 5 でレビュー・承認してから実装を開始してください。
承認後にステータスが approved に更新されたら、再度 /task-start を実行してください。
```

> このゲートはユーザーが「スキップして」「続けて」と言っても通過させてはならない。

### STEP 8: ブランチの作成（必須・スキップ不可）

対応する GitHub Issue 番号を `tasks/task-list.md` から取得し、ブランチを作成する。

```bash
git checkout main
git pull origin main

# Issue番号がある場合（推奨）
git checkout -b feat/<issue番号>-<タスク概要>
# 例: feat/42-login-api

# Issue番号がない場合（タスクIDを使う）
git checkout -b feat/T-<タスク番号>-<タスク概要>
# 例: feat/T-012-user-skill-api
```

ブランチ作成が完了したことを確認してから STEP 9 へ進む。
**ブランチが作成できない場合は実装を開始しない。**

### STEP 9: design ブランチのマージ（存在する場合）

対応 Spec ID の `design/[SPEC-ID]` ブランチが存在する場合、フィーチャーブランチにマージする:

```bash
git branch --list "design/[SPEC-ID]"
# 存在する場合
git merge design/[SPEC-ID] --no-ff -m "merge: design/[SPEC-ID] を取り込む"
```

| 結果 | 対応 |
|------|------|
| `design/[SPEC-ID]` が存在する | マージして STEP 10 へ進む |
| 存在しない | スキップして STEP 10 へ進む（設計書は既に main にある） |

### STEP 10: GitHub Project のステータス更新

`.github/project-config.json` から Project 情報を読み込み、対応する Issue のステータスを "In Progress" に更新する:

```bash
OWNER=$(jq -r '.owner' .github/project-config.json)
PROJECT_NUMBER=$(jq -r '.project_number' .github/project-config.json)
PROJECT_ID=$(jq -r '.project_id' .github/project-config.json)
STATUS_FIELD_ID=$(jq -r '.fields.status.id' .github/project-config.json)
IN_PROGRESS_ID=$(jq -r '.fields.status.options.in_progress' .github/project-config.json)

# Project 内の対応 Item ID を取得
ITEM_ID=$(gh project item-list $PROJECT_NUMBER --owner $OWNER --format json \
  | jq -r '.items[] | select(.content.number == <issue番号>) | .id')

# ステータスを "In Progress" に更新
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
  -f optionId="$IN_PROGRESS_ID"
```

### STEP 11: GitHub Issue へコメント追加

実装開始を記録するコメントを Issue に追加する:

```bash
gh issue comment <issue番号> \
  --body "🏃 実装開始: ブランチ \`feat/<issue番号>-<タスク概要>\` を作成しました。"
```

### STEP 12: 作業コンテキストのサマリー表示

```markdown
## タスク開始: TASK-001

### 作業ブランチ
feat/42-login-api（作成済み）

### GitHub Issue
#42: https://github.com/<owner>/<repo>/issues/42

### 対応仕様書
SPEC-AUTH-001: ログイン機能（docs/spec/auth/spec-auth-001-login.md）

### 対応設計書
docs/design/SPEC-AUTH-001-design.md（確認済み）

### このタスクで実装すること
- POST /api/auth/login エンドポイント
- メールアドレス + パスワードの認証処理
- JWT トークンの発行

### 完了条件（受入基準）
Given: 正しいメールアドレスとパスワードが入力された
When:  POST /api/auth/login を実行する
Then:  HTTP 200 と JWT トークンが返る
```

### STEP 13: task-list.md のステータス自動更新

確認なしに `tasks/task-list.md` の該当タスクのステータスを「未着手」→「🏃 進行中」に更新する。

---

## 次のステップ（この順序を必ず守ること）

```
1. コーディング（実装）
2. /test-write → E2Eテスト生成（必須・スキップ不可）
3. /test-run   → テスト実行・確認
4. /task-done  → PR 作成
```

> ⛔ `/test-write` をスキップして `/task-done` を実行することはできない。
