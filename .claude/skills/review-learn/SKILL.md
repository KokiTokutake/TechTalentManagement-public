# /review-learn — PRレビュー指摘からの自動学習

**使う場面**: スプリント終了後、または定期的に。マージ済みPRのレビューコメントとdiffを分析し、プロジェクト固有の指摘パターンをDBに蓄積する。

---

## 目的

GitHub PRのレビューコメント・修正diff・承認フローを自動解析し、
「どんな指摘が多いか」「どう修正されたか」「どうすれば防げるか」を
`docs/guides/review-patterns.md` に蓄積する。

次回の仕様書作成・設計・実装時にこのDBを参照することで、既知の問題を事前に排除する。

---

## 前提条件

- `gh` コマンドが認証済みであること（`gh auth status`）
- GitHubリポジトリにマージ済みPRが存在すること
- `docs/guides/` ディレクトリが存在すること

---

## 実行手順

### STEP 1: 対象PRの選択

```
どのPRから学習しますか？

1. 直近N件のマージ済みPR（デフォルト: 10件）
2. 特定のPR番号を指定
3. 特定スプリントのPR（ブランチ名パターンで絞り込み）
4. 未学習のPRをすべて（前回実行以降のもの）

[Enter] でデフォルト（直近10件）
```

### STEP 2: PRデータの取得

選択に応じてPR一覧を取得する:

```bash
# 直近N件のマージ済みPR（レビュー情報付き）
gh pr list --state merged --limit 10 \
  --json number,title,mergedAt,headRefName,author,reviews,reviewDecision

# 特定PR
gh pr view {number} --json number,title,mergedAt,headRefName,reviews,reviewDecision
```

取得結果を表示:

```
📋 学習対象PR一覧

| PR# | タイトル | マージ日 | レビュー数 | 決定 |
|-----|---------|---------|-----------|------|
| #45 | feat: ログイン機能実装 | 2024-03-15 | 3件 | APPROVED |
| #44 | fix: パスワードリセット修正 | 2024-03-14 | 2件 | APPROVED |
| #43 | spec: ダッシュボード仕様書追加 | 2024-03-12 | 1件 | APPROVED |

これらのPRから学習を開始しますか？ (yes / no / 対象を変更)
```

### STEP 3: レビューコメントとdiffの取得

各PRについて以下を取得する:

```bash
# PRのレビューコメント（行コメント含む）
gh api repos/{owner}/{repo}/pulls/{pr}/comments \
  --jq '[.[] | {id, path, line, body, user: .user.login, created_at}]'

# PRレビュー（全体コメント）
gh api repos/{owner}/{repo}/pulls/{pr}/reviews \
  --jq '[.[] | select(.state != "PENDING") | {id, state, body, user: .user.login}]'

# PRのdiff（変更内容）
gh pr diff {pr}

# 変更ファイル一覧
gh api repos/{owner}/{repo}/pulls/{pr}/files \
  --jq '[.[] | {filename, status, additions, deletions, patch}]'
```

### STEP 4: パターンの抽出（AI解析）

取得したデータをAIで解析し、以下を抽出する:

**解析の観点:**

1. **指摘カテゴリの判定**
   - `architecture` — レイヤー設計・依存関係・モジュール分割
   - `security` — 認証・認可・入力検証・機密情報の扱い
   - `spec-quality` — 仕様書の不備・受入基準の曖昧さ
   - `data-model` — スキーマ設計・インデックス・整合性
   - `api-design` — RESTful設計・命名・バージョニング
   - `test` — テストの不足・テスト設計の問題
   - `implementation` — コーディングパターン・パフォーマンス・可読性
   - `documentation` — コメント・ドキュメントの不足

2. **指摘と修正の対応関係**
   - レビューコメントの内容
   - 対応するdiffの変更箇所
   - 修正前後のコードパターン

3. **再発防止策の生成**
   - この指摘を仕様書作成時に防ぐには？
   - 設計時に防ぐには？
   - 実装時に防ぐには？

**抽出例:**

```
PR #45 から以下のパターンを検出:

[PATTERN-NEW] カテゴリ: architecture
指摘: "Controller から直接 Repository を import しています。Service 層を経由してください"
修正: import を Service に変更、Controller の依存を整理
再発防止: 設計書でレイヤー依存関係図を必須化する

[PATTERN-EXISTING: ARCH-001] 既存パターンと類似（頻度 +1）

[PATTERN-NEW] カテゴリ: security
指摘: "パスワードをログに出力しています。機密情報はログ禁止"
修正: ログ出力から password フィールドを除去
再発防止: 実装チェックリストに「ログに機密情報を含まないか」を追加
```

### STEP 5: 既存DBとのマージ

`docs/guides/review-patterns.md` が存在する場合:

```
既存パターン数: 12
新規パターン: 2件
既存パターンへの頻度追加: 3件

変更内容を確認してDBに追記しますか？ (yes / no / 内容を編集)
```

ユーザーが `yes` を選択した場合のみDBを更新する。

### STEP 6: DBの更新

`docs/guides/review-patterns.md` を以下の形式で更新する:

```markdown
# レビュー指摘パターンDB

<!-- /review-learn により自動更新 YYYY-MM-DD -->
最終更新: YYYY-MM-DD | 総パターン数: N | 総指摘件数: M | 学習済みPR数: K

## カテゴリ別サマリー

| カテゴリ | パターン数 | 総指摘件数 | 最近30日の傾向 |
|---------|-----------|-----------|--------------|
| architecture | 3 | 12 | 📈 増加 |
| security | 2 | 5 | → 横ばい |
| spec-quality | 4 | 8 | 📉 減少 |
| test | 2 | 4 | → 横ばい |
| implementation | 1 | 3 | — |

## パターン一覧

### [ARCH-001] レイヤー間の直接依存
**カテゴリ**: architecture
**指摘頻度**: 5回
**最終確認**: YYYY-MM-DD
**学習元PR**: #12, #18, #24, #31, #45

**指摘コメント例**:
> "Controller から直接 Repository を呼んでいます。Service 層を経由してください"
> "この import は依存の逆転が起きています。抽象に依存してください"

**修正パターン**:
```
Before: UserController → UserRepository (直接)
After:  UserController → UserService → UserRepository
```

**再発防止策**:
- [ ] 設計書にレイヤー依存関係図を必須記載
- [ ] `/design-review` Agent A（アーキテクチャ）が自動検出
- [ ] `impl-check` で import パスのレイヤー違反をチェック

---

### [SEC-001] ログへの機密情報出力
**カテゴリ**: security
**指摘頻度**: 3回
**最終確認**: YYYY-MM-DD
**学習元PR**: #22, #38, #45

**指摘コメント例**:
> "password フィールドがログに出力されています。機密情報はログ禁止"
> "ユーザーIDとメールをデバッグログに出さないでください"

**修正パターン**:
```
Before: console.log('login attempt', { email, password })
After:  console.log('login attempt', { email })  // password は除外
```

**再発防止策**:
- [ ] 仕様書の非機能要件に「ログ禁止フィールド一覧」を記載
- [ ] `/design-review` Agent B（セキュリティ）が自動検出
- [ ] レビューチェックリストに追加
```

### STEP 7: 学習済みPRの記録

`docs/guides/review-patterns.md` 末尾に学習済みPRを記録し、次回の `/review-learn` で重複処理を防ぐ:

```markdown
## 学習済みPR

| PR# | タイトル | マージ日 | 学習日 | 抽出パターン数 |
|-----|---------|---------|--------|--------------|
| #45 | feat: ログイン機能実装 | 2024-03-15 | 2024-03-20 | 2件 |
| #44 | fix: パスワードリセット修正 | 2024-03-14 | 2024-03-20 | 1件 |
```

### STEP 8: 次のアクション案内

```
✅ 学習完了！

DBに追加されたパターン: 2件
更新されたパターン（頻度++）: 3件

次のステップ:
  /review-patterns       → 蓄積パターンを確認・検索
  /spec-write            → 次の仕様書作成（パターンDB参照あり）
  /design-create         → 設計書作成（パターンDB参照あり）
  /retro-run             → スプリント振り返りにパターン傾向を反映
```

---

## カテゴリ対応表

| カテゴリID | 名称 | 主な検出元スキル |
|-----------|------|----------------|
| `architecture` | アーキテクチャ | `/design-review` Agent A |
| `security` | セキュリティ | `/design-review` Agent B |
| `data-model` | データモデル | `/design-review` Agent C |
| `api-design` | API設計 | `/design-review` Agent D |
| `test` | テスト | `/design-review` Agent E, `/test-run` |
| `spec-quality` | 仕様書品質 | `/spec-review`, `/design-review` Agent F |
| `implementation` | 実装 | `/impl-check` |
| `documentation` | ドキュメント | `/impl-check` |

---

## 自動実行の推奨タイミング

```
スプリント終了時:   /sprint-end → /review-learn → /retro-run
定期実行（週次）:   /review-learn（直近7日のPR）
大型リリース後:     /review-learn（リリースPR全件）
```

---

## 関連スキル

```
/review-patterns → 蓄積パターンの閲覧・検索
/retro-run       → スプリント振り返りへのパターン反映
/spec-write      → 仕様書作成前にパターンDB参照
/design-create   → 設計書作成前にパターンDB参照
/impl-check      → 実装チェック時にパターンDB参照
```
