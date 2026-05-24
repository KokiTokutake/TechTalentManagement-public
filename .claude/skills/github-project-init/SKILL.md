# /github-project-init — 既存プロジェクトへの GitHub Project 後付けセットアップ

**使う場面**: すでに開発が進んでいるプロジェクトで GitHub Project がまだ作成されていないとき。
`/sdd-start` を使わずに始めたプロジェクト、または途中から GitHub Projects による管理に移行したいとき。

---

## 目的

既存の GitHub リポジトリに対して GitHub Projects V2 を作成し、
`tasks/task-list.md` の既存タスクを GitHub Issues + Project アイテムとしてインポートする。
完了後に `.github/project-config.json` を生成し、以降のスキルが正常動作できる状態にする。

---

## 実行手順

### STEP 1: 既存リポジトリの確認

```bash
# 現在のリポジトリ情報を確認
gh repo view --json name,owner,url

# 既に GitHub Project が存在するか確認
gh project list --owner <owner> --format json
```

既に Project が存在する場合:
```
⚠️ GitHub Project が既に存在します:
  #1「プロジェクト名」

既存の Project を使いますか？ (Y/n)
```

Y の場合は STEP 3（フィールド確認・設定）へスキップする。

### STEP 2: GitHub Project の作成

リポジトリのオーナーとリポジトリ名を確認し、Project を作成する:

```bash
# オーナーとリポジトリ名を取得
OWNER=$(gh repo view --json owner --jq '.owner.login')
REPO=$(gh repo view --json name --jq '.name')

# Project を作成
gh project create --owner $OWNER --title "<プロジェクト名>"

# 作成された Project 番号を確認
gh project list --owner $OWNER --format json | head -3
```

### STEP 3: 必須フィールドの確認と作成

既存 Project の場合はフィールドを確認し、不足しているものだけ追加する。

```bash
PROJECT_NUMBER=<番号>

# 現在のフィールド一覧を確認
gh project field-list $PROJECT_NUMBER --owner $OWNER --format json
```

以下のフィールドが存在しない場合のみ作成する:

```bash
# Status フィールド（存在しない場合）
gh project field-create $PROJECT_NUMBER --owner $OWNER \
  --name "Status" --data-type SINGLE_SELECT \
  --single-select-options "Todo,In Progress,In Review,Done"

# Sprint フィールド（存在しない場合）
gh project field-create $PROJECT_NUMBER --owner $OWNER \
  --name "Sprint" --data-type ITERATION

# Story Points フィールド（存在しない場合）
gh project field-create $PROJECT_NUMBER --owner $OWNER \
  --name "Story Points" --data-type NUMBER

# Spec ID フィールド（存在しない場合）
gh project field-create $PROJECT_NUMBER --owner $OWNER \
  --name "Spec ID" --data-type TEXT
```

### STEP 4: project-config.json の生成

フィールド情報を取得し `.github/project-config.json` を生成する:

```bash
# フィールド一覧と ID を取得
gh project field-list $PROJECT_NUMBER --owner $OWNER --format json
```

取得した ID を使って `.github/project-config.json` を生成する:

```bash
mkdir -p .github

cat > .github/project-config.json << 'EOF'
{
  "owner": "<owner>",
  "repo": "<repo>",
  "project_number": <number>,
  "project_id": "<PVT_xxx>",
  "fields": {
    "status": {
      "id": "<PVTSSF_xxx>",
      "options": {
        "todo": "<option-id>",
        "in_progress": "<option-id>",
        "in_review": "<option-id>",
        "done": "<option-id>"
      }
    },
    "sprint": {
      "id": "<PVTIF_xxx>"
    },
    "story_points": {
      "id": "<PVTNF_xxx>"
    },
    "spec_id": {
      "id": "<PVTTF_xxx>"
    }
  }
}
EOF
```

生成内容をユーザーに見せて確認する。

### STEP 5: 既存タスクのインポート確認

`tasks/task-list.md` が存在する場合:

```
tasks/task-list.md を検出しました。

既存のタスクを GitHub Issues としてインポートしますか？

  インポートすると:
  ✅ 各タスクが GitHub Issue になる
  ✅ 全 Issue が GitHub Project に追加される
  ✅ task-list.md に Issue 番号が追記される
  ✅ 以降のスキル（/task-start, /task-done 等）が GitHub と連動する

  インポートしない場合:
  ℹ️  新規タスクは /tasks-plan で今後 Issue として作成される
  ℹ️  既存タスクは task-list.md のみで管理が続く

インポートしますか？ (Y/n)
```

### STEP 6: 既存タスクの GitHub Issues 化（STEP 5 で Y の場合）

`tasks/task-list.md` を読み込み、「未着手」「進行中」「レビュー中」のタスクを Issue として作成する。
（「✅ done」のタスクはスキップするか、クローズ済み Issue として作成するかを確認する）

```bash
# 例: 各タスクに対して Issue を作成
gh issue create \
  --title "[TASK-001] タスク名" \
  --body "$(cat <<'EOF'
## 概要

対応仕様書: SPEC-XXX-001

## ストーリーポイント

3 SP

## 依存タスク

なし

---
> このISSUEは /github-project-init により既存タスクからインポートされました。
EOF
)" \
  --label "task"
```

作成した Issue を Project に追加する:

```bash
gh project item-add $PROJECT_NUMBER --owner $OWNER \
  --url https://github.com/<owner>/<repo>/issues/<number>
```

完了済みタスクの Issue は作成後すぐにクローズする:

```bash
gh issue close <number> --comment "既存の完了済みタスクをインポートしました。"
```

### STEP 7: task-list.md に Issue 番号を追記

インポートした各タスクの行に Issue 番号を追記する:

```markdown
| TASK-001 | ログイン API 実装 | SPEC-AUTH-001 | 3 | #42 | - | 未着手 |
```

### STEP 8: ラベルの初期設定（未作成の場合のみ）

```bash
# 既存ラベルを確認
gh label list --repo $OWNER/$REPO

# 不足しているラベルのみ作成
gh label create "task"          --color "E5E7EB" --description "実装タスク"
gh label create "bug"           --color "FEE2E2" --description "バグ"
gh label create "spec:draft"    --color "FEF3C7" --description "仕様書ドラフト"
gh label create "spec:review"   --color "DBEAFE" --description "仕様書レビュー中"
gh label create "spec:approved" --color "D1FAE5" --description "仕様書承認済み"
```

### STEP 9: 完了サマリー

```
✅ GitHub Project セットアップ完了！

GitHub Project: https://github.com/orgs/<owner>/projects/<number>
設定ファイル:  .github/project-config.json

インポート結果:
  Issues 作成: N 件（未着手 X・進行中 Y・完了 Z）
  Project に追加: N 件

次のステップ:
  /task-board   → GitHub Project のタスク状況を確認する
  /sprint-start → スプリントを開始する（Iteration を設定する）
  /task-start   → タスクを選んで実装を開始する
```

---

## 注意事項

- すでに GitHub Issue が存在するタスクは重複インポートしない
- `gh auth login` で認証が必要な場合は先に案内する
- `tasks/task-list.md` が存在しない場合は STEP 5〜7 をスキップする
- project-config.json の内容は `gh project field-list` の出力から正確にコピーすること
  （field ID が間違うとステータス更新が失敗する）

---

## 関連スキル

- `/sdd-start` — 新規プロジェクトの初回セットアップ（リポジトリ作成から行う場合）
- `/tasks-plan` — 仕様書から新規タスク・Issues を作成する
- `/sprint-start` — スプリント開始（GitHub Project Iteration）
- `/task-board` — GitHub Project のタスクボードを表示
