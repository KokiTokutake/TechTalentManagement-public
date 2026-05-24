# /sdd-start — 新プロジェクトへのSDD導入ウィザード

**使う場面**: 新しいプロジェクトに仕様駆動開発（SDD）を初めて導入するとき。プロジェクト開始時に一度だけ実行する。

---

## 目的

SDD ハーネステンプレートをベースに、このプロジェクト固有の Constitution.md・CLAUDE.md・GitHub リポジトリ・GitHub Projects を対話型で整備する。

---

## 実行手順

以下を順に実施する。各ステップで必ずユーザーに確認を取ってから次へ進む。

### STEP 1: プロジェクト基本情報の収集

以下をまとめて質問する:

```
1. プロジェクト名（例: ECサイトリニューアル）
2. GitHub リポジトリ名（例: ec-renewal）
3. GitHub オーナー（ユーザー名 or 組織名）
4. リポジトリの公開設定: public / private
5. プロジェクトの一言説明（何を作るか・なぜ作るか）
6. チーム規模: 小規模（3〜5人）/ 中規模（6〜30人）/ 大規模（50人以上）
7. 主な技術スタック（言語・フレームワーク・DB）
8. プロダクトオーナーの GitHub ハンドル（@xxx）
9. テックリードの GitHub ハンドル（@xxx）
10. リリース目標日（わかれば）
```

### STEP 2: GitHub リポジトリの作成

収集した情報をもとに GitHub リポジトリを作成する:

```bash
gh repo create <owner>/<repo-name> \
  --[public|private] \
  --description "<プロジェクト説明>" \
  --clone
```

作成後、リポジトリの URL を表示してユーザーに確認する。

### STEP 3: GitHub Project の作成・初期設定

GitHub Project (Projects V2) を作成し、SDD 用のフィールドを設定する:

```bash
# プロジェクトを作成
gh project create --owner <owner> --title "<プロジェクト名>"

# 作成された Project 番号を取得
gh project list --owner <owner> --format json | head -5
```

Project が作成されたら、以下のカスタムフィールドを追加する:

```bash
PROJECT_NUMBER=<作成された番号>

# Status フィールド（タスクの進捗管理）
gh project field-create $PROJECT_NUMBER --owner <owner> \
  --name "Status" --data-type SINGLE_SELECT \
  --single-select-options "Todo,In Progress,In Review,Done"

# Sprint フィールド（イテレーション管理）
gh project field-create $PROJECT_NUMBER --owner <owner> \
  --name "Sprint" --data-type ITERATION

# Story Points フィールド
gh project field-create $PROJECT_NUMBER --owner <owner> \
  --name "Story Points" --data-type NUMBER

# Spec ID フィールド
gh project field-create $PROJECT_NUMBER --owner <owner> \
  --name "Spec ID" --data-type TEXT
```

フィールド ID を取得して `.github/project-config.json` に保存する:

```bash
gh project field-list $PROJECT_NUMBER --owner <owner> --format json
```

### STEP 4: project-config.json の生成

取得したフィールド情報をもとに `.github/project-config.json` を生成する:

```json
{
  "owner": "<owner>",
  "repo": "<repo-name>",
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
```

### STEP 5: GitHub ラベルの初期設定

リポジトリに SDD 用のラベルを作成する:

```bash
# 仕様書ステータス
gh label create "spec:draft"     --color "FEF3C7" --description "仕様書ドラフト" --repo <owner>/<repo>
gh label create "spec:review"    --color "DBEAFE" --description "仕様書レビュー中" --repo <owner>/<repo>
gh label create "spec:approved"  --color "D1FAE5" --description "仕様書承認済み" --repo <owner>/<repo>

# タスク種別
gh label create "task"           --color "E5E7EB" --description "実装タスク" --repo <owner>/<repo>
gh label create "bug"            --color "FEE2E2" --description "バグ" --repo <owner>/<repo>
gh label create "design"         --color "EDE9FE" --description "設計タスク" --repo <owner>/<repo>

# 優先度
gh label create "priority:high"  --color "DC2626" --description "高優先度" --repo <owner>/<repo>
gh label create "priority:low"   --color "6B7280" --description "低優先度" --repo <owner>/<repo>
```

### STEP 6: Constitution.md の初版生成

収集した情報をもとに `.constitution/Constitution.md` のプレースホルダーを置き換えて生成する。
生成したら全文をユーザーに見せ「このまま保存しますか？修正しますか？」と確認する。

### STEP 7: CLAUDE.md の設定

`CLAUDE.md` の以下のセクションを質問して埋める:
- 技術スタック（言語・フレームワーク・DB・テスト・CI/CD）
- よく使うコマンド（開発サーバー起動・テスト実行・ビルド）
- ディレクトリ構造（`src/` 以下の主要フォルダ）

### STEP 8: Claude Code hooks のセットアップ確認

`.claude/settings.json` と `.claude/hooks/` が存在するか確認し、ない場合はテンプレートからコピーするよう案内する:

```
✅ Claude Code hooks の確認

  .claude/settings.json         — 存在確認済み / ⚠️ 見つかりません
  .claude/hooks/guard-dangerous-bash.sh  — 存在確認済み / ⚠️ 見つかりません
  .claude/hooks/guard-spec-edit.sh       — 存在確認済み / ⚠️ 見つかりません

hooks が設定されていると以下のガードが有効になります:
  - rm -rf / や git reset --hard などの破壊的コマンドのブロック
  - approved 仕様書への無断編集のブロック（/spec-change への誘導）

hooks ファイルをコピーしますか？ (yes / skip)
```

`yes` の場合、テンプレートから hooks ファイルをプロジェクトにコピーする。

### STEP 9: GitHub 設定チェックリスト表示

`docs/guides/github-setup.md` の規模別設定を参照し、チーム規模に合ったチェックリストを表示する。
ユーザーが完了した項目を ✅ で確認しながら進める。

### STEP 10: 完了サマリー

```
✅ SDD 導入セットアップ完了

作成・更新したファイル:
  .constitution/Constitution.md
  CLAUDE.md
  .github/project-config.json
  .claude/settings.json（hooks設定）

GitHub リソース:
  リポジトリ:  https://github.com/<owner>/<repo>
  Project:     https://github.com/orgs/<owner>/projects/<number>

有効になったガード:
  ⛔ 危険なbashコマンド（rm -rf, git reset --hard 等）の自動ブロック
  ⛔ approved 仕様書の無断編集ブロック → /spec-change への誘導

次のステップ:
  /charter-review  → Constitution.md の品質確認
  /spec-write      → 最初の仕様書を作成
  /tasks-plan      → 仕様書からタスク・GitHub Issues を作成
  /sdd-skills      → 使えるスキル一覧を確認
```

---

## 注意事項

- ユーザーが「スキップ」と言ったステップは省略してよい
- 生成したファイルはユーザーが確認してから保存する
- `gh` コマンドが使えない場合は `gh auth login` を案内する
- コミットは `add: SDD プロジェクト設定を初期化` のメッセージで提案する（実行はユーザーの承認後）
