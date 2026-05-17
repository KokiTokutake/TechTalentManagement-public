# AI 活用ワークフローガイド

> **前提環境**
> - エディタ: Visual Studio Code
> - AI CLI: Claude Code（`claude`）/ OpenAI Codex CLI（`codex`）
> - ターミナル: VSCode 統合ターミナル（Ctrl+` で起動）
> - 仕様書: `docs/spec/` に Markdown で管理済み

---

## 1. 基本的な考え方

仕様駆動開発における AI の役割は **「仕様書を読んでコードを書く実装者」** です。

```
人間の役割          AIの役割
──────────────      ──────────────────────────
Why を決める   →    Why を理解してコードを生成
What を書く    →    What に従って実装する
レビューする   →    実装・テスト・ドキュメント生成
承認する       →    リファクタリング・代替案提示
```

**Human-in-the-Loop 原則**: AI の出力は必ず人間がレビュー・承認する。

---

## 2. セットアップ

### 2.1 Claude Code CLI

```bash
# インストール（npm が必要）
npm install -g @anthropic-ai/claude-code

# 認証
claude auth

# 動作確認
claude --version

# プロジェクトルートで起動
claude
```

### 2.2 Codex CLI

```bash
# インストール
npm install -g @openai/codex

# 認証（OPENAI_API_KEY を環境変数に設定）
export OPENAI_API_KEY="sk-..."

# 動作確認
codex --version
```

### 2.3 VSCode 設定

```json
// .vscode/settings.json
{
  "terminal.integrated.env.windows": {
    "ANTHROPIC_API_KEY": "${env:ANTHROPIC_API_KEY}"
  },
  "terminal.integrated.env.osx": {
    "ANTHROPIC_API_KEY": "${env:ANTHROPIC_API_KEY}"
  }
}
```

> API キーは `.env` や VSCode の settings.json に直接書かない。
> OS の環境変数または パスワードマネージャーで管理する。

---

## 3. CLAUDE.md（プロジェクト設定）

Claude Code はプロジェクトルートの `CLAUDE.md` を自動で読み込みます。
このファイルで AI にプロジェクトの文脈を伝えます。

```markdown
# CLAUDE.md

## プロジェクト概要
（README.md の概要を記載）

## 仕様書の場所
- 仕様書: `docs/spec/` — 実装前に必ず参照すること
- 設計書: `docs/design/` — アーキテクチャ判断の根拠
- 憲章: `.constitution/Constitution.md` — 変更不可の原則

## コーディング規約
- 言語: [TypeScript / Python / Go 等]
- フォーマッター: [Prettier / Black / gofmt 等]
- テストフレームワーク: [Jest / pytest / Go test 等]

## 重要なルール
- コードより先に仕様書を確認すること
- 仕様書に書かれていない机能は勝手に追加しない
- 変更が仕様書の受入基準を満たすことを確認してからPRを出す
- テストは必ずセットで書く
```

→ `templates/CLAUDE.md` にプロジェクト用テンプレートあり

---

## 4. 実装工程のワークフロー

### ステップ1: 仕様書を確認する

```bash
# 対象仕様書を開く
code docs/spec/[feature-name].md
```

実装前に以下を確認する：
- [ ] 仕様書のステータスが `approved` になっている
- [ ] 受入基準が明確に書かれている
- [ ] 依存する仕様書・設計書を読んだ

### ステップ2: Issue・ブランチを作成する

```bash
# GitHub CLI で Issue 確認
gh issue view [番号]

# ブランチ作成
git checkout -b feat/[issue番号]-[機能名]
```

### ステップ3: Claude Code で実装する

VSCode のターミナルでプロジェクトルートから起動：

```bash
claude
```

**推奨プロンプトパターン（後述 §5 参照）** を使って実装を依頼する。

### ステップ4: 人間がレビューする

AI が生成したコードを必ずレビューする：

```bash
# 差分を確認
git diff

# テストを実行
[npm test / pytest / go test ./...]

# 仕様書の受入基準を満たしているか確認
code docs/spec/[feature-name].md
```

### ステップ5: PR を作成する

```bash
# コミット
git add [files]
git commit -m "feat: [機能名]（#[issue番号]）"

# PR 作成（GitHub CLI）
gh pr create --template .github/pull_request_template.md
```

---

## 5. プロンプトパターン集

### 5.1 実装依頼（基本形）

```
以下の仕様書をもとに実装してください。

【仕様書】
（docs/spec/[feature-name].md の内容を添付、または @docs/spec/[feature-name].md で参照）

【設計書】
（docs/design/architecture.md の該当部分）

【条件】
- 仕様書の要件（What）のみを実装する。余分な機能は追加しない
- テストを必ずセットで書く
- 受入基準をすべて満たすこと
- 既存コードのスタイル・命名規則に合わせること
```

### 5.2 仕様書レビュー依頼

```
以下の仕様書をレビューしてください。

【仕様書】
（仕様書の内容）

【確認観点】
- 明確性: 曖昧な表現・解釈が割れる箇所はないか
- 完全性: Why・What・受入基準がすべて揃っているか
- 一貫性: Constitution.md の原則と矛盾していないか
- 実装可能性: 技術的に実現可能か
- 将来性: 今後の拡張を妨げないか

問題点があれば具体的に指摘し、改善案を提示してください。
```

### 5.3 タスク分割依頼

```
以下の仕様書と設計書をもとに、実装タスクを分割してください。

【仕様書】（内容を貼り付け）
【設計書】（内容を貼り付け）

【条件】
- 1タスク = 1PR = レビュー80分以内で完了できる粒度
- 各タスクにSpec IDを紐付ける
- 見積もりポイント（1/2/3/5/8）を付与する
- 依存関係があれば明示する
- tasks/task-list.md の表形式で出力する
```

### 5.4 テストコード生成依頼

```
以下の仕様書の受入基準に対応するテストコードを生成してください。

【仕様書の受入基準】
（受入基準セクションを貼り付け）

【既存のテストファイル】
（既存テストのパターンを参照させる）

【条件】
- 正常系・異常系・エッジケースをすべてカバーする
- 受入基準のIDをテスト名に含める（例: REQ-01_正常系）
- Given-When-Then の構造で書く
```

### 5.5 リファクタリング依頼

```
以下のコードをリファクタリングしてください。

【対象コード】（コードを貼り付け）
【関連仕様書】（仕様書パス）

【条件】
- 仕様書の受入基準を満たしたままにする
- 外部インターフェース（API・関数シグネチャ）は変えない
- テストが通ることを確認してから提示する
- 変更理由をコメントに残す
```

---

## 6. Codex CLI の使い方

Codex は対話モードよりも **ワンショットでのコード生成・変換** に向いています。

```bash
# ファイルの内容をもとにコード生成
codex "docs/spec/feature.md を読んで src/feature.ts を実装して"

# 既存コードのリファクタリング
codex "src/legacy.ts を TypeScript 化して。仕様は docs/spec/xxx.md を参照"

# テスト生成
codex "src/feature.ts のユニットテストを tests/feature.test.ts に生成して"
```

---

## 7. よくある失敗パターンと対処

| 失敗パターン | 対処 |
|-----------|------|
| AI が仕様にない機能を追加した | プロンプトに「仕様書にない機能は追加しない」を明記 |
| 生成コードが既存スタイルと合わない | CLAUDE.md にコーディング規約を記載する |
| AI の出力を確認せずマージした | PR テンプレートの受入基準チェックを必ず埋める |
| 仕様書が古くてコードと乖離した | 実装後に仕様書を更新する（Spec-as-Code の原則） |
| テストなしで実装した | プロンプトに「テスト必須」を毎回明記する |
