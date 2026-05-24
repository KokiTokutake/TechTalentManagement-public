# /tasks-plan — 承認済み仕様書からのタスク分割

**使う場面**: 工程4（タスク分割）。approved になった仕様書をもとにスプリントのタスクを計画するとき。

---

## 目的

「1タスク = 1PR = レビュー80分以内」の粒度で仕様書をタスクに分解し、
`tasks/task-list.md` の更新と GitHub Issues の作成・Project への追加を行う。

---

## 実行手順

### STEP 1: 対象仕様書の確認

`/spec-list approved` の結果を表示し、「どの仕様書をタスク分割しますか？」と確認する。
複数選択可。スプリントゴールを確認する（「今回のスプリントで完成させたいことは何ですか？」）。

### STEP 2: 設計書の確認

`docs/design/` を確認し、設計書が存在する場合は読み込んでタスク分割の参考にする。

### STEP 3: タスク分割の実施

選択した仕様書の各要件・受入基準をもとにタスクを生成する。

**分割のルール**（厳守）:
- 1タスクはレビュアーが80分以内でレビューできる量
- フロントエンド・バックエンド・テストは別タスクに分ける
- 1タスクに複数の受入基準が含まれる場合は分割を検討する
- インフラ・設定変更は独立したタスクにする

**タスクの構成要素**:
- タスク名（動詞から始まる）
- 対応 Spec ID
- 担当者
- ストーリーポイント（1 / 2 / 3 / 5 / 8）
- 依存タスク

### STEP 4: ストーリーポイント見積もり

生成したタスクのストーリーポイントを以下の基準で確認・調整する:

| SP | 目安 |
|----|------|
| 1  | 1〜2時間（軽微な修正・設定変更） |
| 2  | 半日程度（シンプルな機能の実装） |
| 3  | 1日程度（中程度の機能の実装） |
| 5  | 2〜3日（複雑な機能・複数コンポーネント） |
| 8  | 1週間（大規模機能 → 分割を強く推奨） |

SP 8 のタスクがある場合: 「このタスクは大きすぎます。さらに分割することを推奨します。分割しますか？」と確認する。

### STEP 5: タスク一覧の確認

生成したタスク一覧を表示しユーザーに確認を求める:

```markdown
## スプリントタスク一覧（草案）

| # | タスク名 | Spec ID | SP | 担当 | 依存 |
|---|---------|---------|----|----|------|
| TASK-001 | ログイン API エンドポイント実装 | SPEC-AUTH-001 | 3 | - | - |
| TASK-002 | ログイン画面 UI 実装 | SPEC-AUTH-001 | 2 | - | TASK-001 |
| TASK-003 | ログイン機能の結合テスト | SPEC-AUTH-001 | 2 | - | TASK-001, TASK-002 |
| TASK-004 | アカウントロック機能実装 | SPEC-AUTH-001 | 3 | - | TASK-001 |

合計: 10 SP
```

「このタスク分割でよいですか？追加・変更はありますか？」と確認する。

### STEP 6: task-list.md への書き込み

確認が取れたら `tasks/task-list.md` を更新する（Issue 番号は STEP 7 完了後に追記する）。

### STEP 7: GitHub Issues の作成

まず `.github/project-config.json` が存在するか確認する:

```bash
test -f .github/project-config.json && \
  jq -e '.project_number != 0 and .project_number != null' .github/project-config.json
```

**存在しない・または未設定の場合:**

```
⚠️ .github/project-config.json が見つかりません。

GitHub Issues の作成・Project への追加（STEP 7〜9）はスキップします。
タスク一覧は tasks/task-list.md のみに保存されます。

後から GitHub Project を設定する場合:
  /github-project-init → 既存タスクを Issues としてインポートすることもできます
```

この場合は STEP 6 で `tasks/task-list.md` を更新した時点で終了する。

**存在する場合、続けて** `.github/project-config.json` からオーナー・リポジトリ情報を読み込む。

各タスクについて GitHub Issue を作成する:

```bash
# 例: TASK-001
gh issue create \
  --title "[TASK-001] ログイン API エンドポイント実装" \
  --body "$(cat <<'EOF'
## 概要

SPEC-AUTH-001 の受入基準に基づくログイン API の実装。

## 対応仕様書

- Spec ID: SPEC-AUTH-001
- パス: docs/spec/auth/spec-auth-001-login.md

## 受入基準

- Given: 正しいメールアドレスとパスワードが入力された
  When: POST /api/auth/login を実行する
  Then: HTTP 200 と JWT トークンが返る
- Given: 誤ったパスワードが入力された
  When: POST /api/auth/login を実行する
  Then: HTTP 401 と "認証に失敗しました" メッセージが返る

## 依存タスク

なし

## ストーリーポイント

3 SP
EOF
)" \
  --label "task,spec:approved" \
  --assignee "<担当者のGitHubハンドル（未定の場合は省略）>"
```

作成された Issue の URL・番号を記録する。

### STEP 8: GitHub Project への追加

`.github/project-config.json` から `project_number` を取得し、全 Issue を Project に追加する:

```bash
PROJECT_NUMBER=$(jq -r '.project_number' .github/project-config.json)
OWNER=$(jq -r '.owner' .github/project-config.json)

# 各 Issue を Project に追加
gh project item-add $PROJECT_NUMBER --owner $OWNER \
  --url https://github.com/<owner>/<repo>/issues/<issue-number>
```

### STEP 9: task-list.md に Issue 番号を追記

作成された GitHub Issue 番号を `tasks/task-list.md` の各タスク行に追記する:

```markdown
| TASK-001 | ログイン API エンドポイント実装 | SPEC-AUTH-001 | 3 | #42 | - | 未着手 |
```

---

## 出力ファイル

- `tasks/task-list.md`（更新）
- GitHub Issues（各タスク1件ずつ）
- GitHub Project アイテム（全 Issue を追加）

---

## 次のステップ

```
/sprint-start → スプリントを開始し、GitHub Project Iteration を設定する
/task-board   → 作成したタスクの一覧を確認
/task-start   → タスクを選んで実装を開始
```
