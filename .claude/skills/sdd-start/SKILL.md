# /sdd-start — 新プロジェクトへのSDD導入ウィザード

**使う場面**: 新しいプロジェクトに仕様駆動開発（SDD）を初めて導入するとき。プロジェクト開始時に一度だけ実行する。

---

## 目的

SDD ハーネステンプレートをベースに、このプロジェクト固有の Constitution.md・CLAUDE.md・GitHub 設定を対話型で整備する。

---

## 実行手順

以下を順に実施する。各ステップで必ずユーザーに確認を取ってから次へ進む。

### STEP 1: プロジェクト基本情報の収集

以下をまとめて質問する:

```
1. プロジェクト名（例: ECサイトリニューアル）
2. プロジェクトの一言説明（何を作るか・なぜ作るか）
3. チーム規模: 小規模（3〜5人）/ 中規模（6〜30人）/ 大規模（50人以上）
4. 主な技術スタック（言語・フレームワーク・DB）
5. プロダクトオーナーの GitHub ハンドル（@xxx）
6. テックリードの GitHub ハンドル（@xxx）
7. リリース目標日（わかれば）
```

### STEP 2: Constitution.md の初版生成

収集した情報をもとに `.constitution/Constitution.md` のプレースホルダーを置き換えて生成する。
生成したら全文をユーザーに見せ「このまま保存しますか？修正しますか？」と確認する。

### STEP 3: CLAUDE.md の設定

`CLAUDE.md` の以下のセクションを質問して埋める:
- 技術スタック（言語・フレームワーク・DB・テスト・CI/CD）
- よく使うコマンド（開発サーバー起動・テスト実行・ビルド）
- ディレクトリ構造（`src/` 以下の主要フォルダ）

### STEP 4: Claude Code hooks のセットアップ確認

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

`yes` の場合、テンプレートから hooks ファイルをプロジェクトにコピーする:

```bash
mkdir -p .claude/hooks
# テンプレートから hooks をコピー（ハーネスのパスに応じて変更）
cp [harness-path]/templates/.claude/settings.json .claude/settings.json
cp [harness-path]/templates/.claude/hooks/guard-dangerous-bash.sh .claude/hooks/
cp [harness-path]/templates/.claude/hooks/guard-spec-edit.sh .claude/hooks/
chmod +x .claude/hooks/*.sh
```

### STEP 5: GitHub 設定チェックリスト表示

`docs/guides/github-setup.md` の規模別設定を参照し、チーム規模に合ったチェックリストを表示する。
ユーザーが完了した項目を ✅ で確認しながら進める。

### STEP 6: 完了サマリー

```
✅ SDD 導入セットアップ完了

作成・更新したファイル:
  .constitution/Constitution.md
  CLAUDE.md
  .claude/settings.json（hooks設定）
  .claude/hooks/guard-dangerous-bash.sh
  .claude/hooks/guard-spec-edit.sh

有効になったガード:
  ⛔ 危険なbashコマンド（rm -rf, git reset --hard 等）の自動ブロック
  ⛔ approved 仕様書の無断編集ブロック → /spec-change への誘導

次のステップ:
  /charter-review  → Constitution.md の品質確認
  /spec-write      → 最初の仕様書を作成
  /sdd-skills      → 使えるスキル一覧を確認
```

---

## 注意事項

- ユーザーが「スキップ」と言ったステップは省略してよい
- 生成したファイルはユーザーが確認してから保存する
- コミットは `add: SDD プロジェクト設定を初期化` のメッセージで提案する（実行はユーザーの承認後）
