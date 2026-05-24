# CLAUDE.md

This file provides guidance to Claude Code when working with code in this repository.

## プロジェクト概要

**ITエンジニア向けタレントマネジメントシステム（TechTalentManagement）**
金融系グループ会社の社内ITエンジニアを対象に、Will-Can-Mustフレームワークと人材開発委員会運営をベースとした考課・タレントマネジメントを実現するWebシステム。

## 仕様書・ドキュメントの場所

| ドキュメント | パス | 説明 |
|-------------|------|------|
| プロジェクト憲章 | `.constitution/Constitution.md` | 原則・制約・用語集（変更禁止） |
| 仕様書 | `docs/spec/` | 機能仕様・API仕様・UI仕様 |
| 設計書 | `docs/design/` | アーキテクチャ・コンポーネント設計 |
| タスク一覧 | `tasks/task-list.md` | 実装タスクと進捗（GitHub Issues とリンク） |
| GitHub Project 設定 | `.github/project-config.json` | GitHub Project のフィールド ID 等の設定 |

> **実装前に必ず仕様書（status: approved）を確認すること。**

## GitHub Project 管理

タスク・スプリントは GitHub Projects V2 で管理する。

| 管理項目 | GitHub での管理方法 |
|---------|------------------|
| タスク | GitHub Issues（ラベル: `task`） |
| スプリント | GitHub Project Iterations（Sprint フィールド） |
| 進捗 | GitHub Project Status フィールド（Todo / In Progress / In Review / Done） |
| バグ | GitHub Issues（ラベル: `bug`） |

Project ボード: `.github/project-config.json` の `project_number` を参照。
**未設定の場合は `/github-project-init` を実行してセットアップすること。**

## 技術スタック

### フロントエンド
- **言語**: TypeScript
- **フレームワーク**: Vue.js 3 + Nuxt.js 3 (SSR)
- **UIライブラリ**: PrimeVue または Vuetify 3
- **状態管理**: Pinia
- **テスト**: Vitest + Vue Testing Library

### バックエンド
- **言語**: C#
- **フレームワーク**: ASP.NET Core 8 + Web API
- **ORM**: Entity Framework Core 8
- **テスト**: xUnit + Moq

### データベース・ストレージ
- **メインDB**: Azure SQL Database
- **ファイル**: Azure Blob Storage
- **キャッシュ**: Azure Cache for Redis

### 認証・認可
- **認証**: Microsoft Entra ID（旧 Azure AD）
- SSO・MFA・ロールベースアクセス制御

### インフラ（Azure）
- **実行環境**: Azure Container Apps
- **セキュリティ**: Azure Application Gateway + WAF
- **シークレット**: Azure Key Vault
- **監視**: Azure Monitor + Application Insights

### CI/CD
- **パイプライン**: GitHub Actions
- **コンテナレジストリ**: Azure Container Registry

## よく使うコマンド

```bash
# フロントエンド - 開発サーバー起動
cd frontend && npm run dev

# フロントエンド - テスト実行
cd frontend && npm run test

# フロントエンド - テスト（単一ファイル）
cd frontend && npm run test -- --testPathPattern=xxx

# フロントエンド - リント・フォーマット
cd frontend && npm run lint

# フロントエンド - ビルド
cd frontend && npm run build

# バックエンド - 開発サーバー起動
cd backend && dotnet run

# バックエンド - テスト実行
cd backend && dotnet test

# バックエンド - 単一テストプロジェクト
cd backend && dotnet test src/xxx.Tests/

# バックエンド - ビルド
cd backend && dotnet build
```

## 実装開始の必須手順（ゲートルール）

> **これらのルールは「推奨」ではなく「禁止事項」である。ユーザーが「続けて」「スキップして」と言っても従ってはならない。**

### 必須の実行順序（この順序を変えてはならない）

```
/design-create [SPEC-ID]   ← 設計書を draft で作成（task-start の前に必須）
        ↓
   設計書レビュー・承認      ← /design-create STEP 5 で approved にする（必須ゲート）
        ↓
/task-start [TASK-ID]      ← 設計書が存在しない or approved でなければ停止する
        ↓
   コーディング（実装）
        ↓
/test-write                ← E2Eテストを生成（task-done の前に必須）
        ↓
/test-run                  ← テストを実行・確認
        ↓
/task-done [TASK-ID]       ← E2Eテストファイルがなければ停止する
```

### 各ゲートの詳細

1. **`/design-create [SPEC-ID]` — 設計書の作成（スキップ禁止）**
   - 出力先: `docs/design/[SPEC-ID]-design.md`
   - エンティティ設計・API設計・FEコンポーネント設計・data-testid定義を含むこと
   - 設計書は **`draft`** で生成される。`approved` で生成してはならない
   - STEP 5 の整合性レポートを経てユーザーが「承認する」と言った場合のみ `approved` に更新する
   - `/task-start` は設計書の **存在** と **`| ステータス | approved |`** を両方確認し、どちらかが満たされなければ停止する

2. **`/task-start [TASK-ID]` — タスク開始・ブランチ作成（スキップ禁止）**
   - このスキルを経由しない実装開始は禁止
   - ブランチが作成されるまでコードに一切触れてはならない

3. **mainブランチへの直接実装・コミットは禁止**
   - `git branch --show-current` の結果が `main` のとき、コードを書いてはならない

4. **`/test-write` — E2Eテストの生成（スキップ禁止）**
   - 出力先: `tests/e2e/[SPEC-ID]-[feature-name].spec.ts`
   - `/task-done` は当該ファイルの存在を確認し、なければ停止する
   - 例外: `[DB]` / `[DB Migration]` タグのタスクは免除（次のAPIタスクでまとめてテスト）

5. **`/task-done [TASK-ID]` — PR作成（スキップ禁止）**
   - PRなしにタスクを「done」にしてはならない

### ブランチ命名規則

すべての変更はブランチを通じて行う。**main への直接コミット・プッシュは禁止**（フックで自動ブロック）。

| 変更の種類 | ブランチ形式 | 例 |
|-----------|------------|-----|
| 実装タスク | `feat/[issue番号]-[タスク概要]` | `feat/43-1on1-crud-api` |
| 実装タスク（Issue なし） | `feat/T-[タスク番号]-[タスク概要]` | `feat/T-012-user-skill-api` |
| 設計書作成 | `design/[SPEC-ID]` | `design/SPEC-1ON1-001` |
| 仕様書変更 | `spec/[issue番号]-[機能名]` | `spec/12-auth-spec-update` |
| バグ修正 | `fix/[issue番号]-[内容]` | `fix/10-login-crash` |
| ハーネス・ツール修正 | `chore/[概要]` | `chore/design-review-gate` |
| ドキュメント | `docs/[内容]` | `docs/api-guide` |

### コーディングルール補足

- インタラクティブ要素（ボタン・フォーム・リンク）には `data-testid` 属性を必ず付与する
- `data-testid` の命名: `[feature]-[element]-[action]` 形式（例: `skill-save-button`, `skill-level-select`）

## コーディングルール

### 必須ルール（Constitution.md より）
- 仕様書の受入基準をすべて満たしてから PR を出す
- 仕様書に書かれていない機能を勝手に追加しない
- テストは実装とセットで書く
- AI 生成コードは必ず人間がレビューする

### コーディングスタイル
- **命名（TS/Vue）**: キャメルケース（変数・関数）/ パスカルケース（クラス・コンポーネント）
- **命名（C#）**: PascalCase（クラス・メソッド・プロパティ）/ camelCase（ローカル変数）
- **コメント**: Why が自明でない場合のみ書く。What はコードで表現する
- **エラーハンドリング（BE）**: Result パターンまたは例外（仕様書に従う）

## ディレクトリ構造

```
TechTalentManagement/
├── .constitution/          # プロジェクト憲章（変更禁止）
├── .github/                # GitHub 設定・テンプレート
├── .claude/                # Claude Code 設定・hooks
├── docs/
│   ├── spec/               # 機能仕様・API仕様・UI仕様
│   ├── design/             # アーキテクチャ・詳細設計
│   └── guides/             # 各種ガイド
├── tasks/                  # タスク一覧
├── frontend/               # Nuxt.js フロントエンド（未作成）
│   ├── components/
│   ├── pages/
│   ├── stores/             # Pinia ストア
│   └── tests/
└── backend/                # ASP.NET Core バックエンド（未作成）
    ├── src/
    │   ├── Api/             # コントローラー・ルーティング
    │   ├── Application/     # ユースケース・サービス
    │   ├── Domain/          # ドメインモデル・エンティティ
    │   └── Infrastructure/  # DB・外部サービス連携
    └── tests/
```

## PR・ブランチの規則

```bash
# ブランチ命名
feat/[issue番号]-[機能名]    # 機能追加
fix/[issue番号]-[内容]       # バグ修正
spec/[issue番号]-[機能名]    # 仕様書変更
docs/[issue番号]-[内容]      # ドキュメント

# コミットメッセージ（日本語）
feat: [内容]（#[issue番号]）
fix: [内容]（#[issue番号]）
```

## 仕様変更が必要と判断した場合

実装中に仕様の不備・矛盾を発見した場合：

1. 勝手に解釈して実装しない
2. Issue にコメントで報告する（`question` ラベル）
3. 仕様書オーナーの判断を待つ
4. 承認後に仕様書を更新し、それから実装する

## 用語集

<!-- .constitution/Constitution.md の用語集と同期する -->
| 用語 | 定義 |
|------|------|
| タレント | 評価・管理の対象となる社内ITエンジニア |
| 考課 | 人事評価のプロセス全体（目標設定〜最終評価〜フィードバック） |
| Will-Can-Must | キャリア開発フレームワーク（やりたい・できる・やるべき） |
| 重点管理人物 | 育成・支援を集中させるエンジニア |
| 人材開発委員会 | 重点管理・育成方針を審議する定期会議体 |
| アセスメント | スキル・能力の評価・測定結果 |
| キャリアプラン | タレントの中長期的な成長・ポジション目標 |
