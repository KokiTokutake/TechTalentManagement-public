# /spec-list — 仕様書一覧・状態確認

**使う場面**: 全工程。どの仕様書がどの状態にあるか確認したいとき。実装可能な仕様書を探すときにも使う。

---

## 目的

`docs/spec/` 配下の全仕様書のステータス・Spec ID・担当者・優先度を一覧表示し、
プロジェクト全体の要件定義の進捗を把握する。

---

## 実行手順

### STEP 1: 仕様書ファイルの収集

`docs/spec/` ディレクトリ配下の全 `.md` ファイルを読み込む（`_README.md` は除く）。
各ファイルから以下を抽出する:
- `status:` フィールド（draft / review / approved / deprecated）
- `spec-id:` フィールド
- `owner:` フィールド
- `priority:` フィールド（Must / Should / Could）
- ファイル名からタイトル

### STEP 2: 一覧表を出力

```markdown
## 仕様書一覧 （更新日: YYYY-MM-DD）

### 🟢 approved（実装可能）
| Spec ID | タイトル | 担当 | 優先度 | ファイル |
|---------|---------|------|--------|---------|
| SPEC-AUTH-001 | ログイン機能 | @po-handle | Must | docs/spec/auth/spec-auth-001-login.md |

### 🟡 review（レビュー中）
| Spec ID | タイトル | 担当 | 優先度 | ファイル |
...

### ⚪ draft（作成中）
| Spec ID | タイトル | 担当 | 優先度 | ファイル |
...

### 🔴 deprecated（廃止）
| Spec ID | タイトル | 廃止理由 | ファイル |
...

---
合計: approved X件 / review X件 / draft X件 / deprecated X件
実装可能な仕様書: X件
```

### STEP 3: フィルタリング（引数対応）

以下の引数でフィルタリングできる:

```
/spec-list approved     → approved のみ表示
/spec-list review       → review のみ表示
/spec-list draft        → draft のみ表示
/spec-list AUTH         → カテゴリ AUTH のみ表示
```

### STEP 4: アクション提案

表示後に状況に応じてアクションを提案する:

- `review` 状態の仕様書がある場合: 「`/spec-review [SPEC-ID]` でレビューできます」
- `draft` 状態が多い場合: 「`/spec-write` で仕様書を完成させましょう」
- `approved` が存在する場合: 「`/tasks-plan` でタスク分割に進めます」

---

## 出力例

```
📋 仕様書一覧（docs/spec/）

🟢 approved: 3件（実装可能）
  SPEC-AUTH-001  ログイン機能         @po  Must
  SPEC-USER-001  ユーザープロフィール  @po  Must
  SPEC-PAY-001   決済フロー           @po  Must

🟡 review: 1件
  SPEC-AUTH-002  ソーシャルログイン   @po  Should

⚪ draft: 2件
  SPEC-DASH-001  ダッシュボード       @po  Could
  SPEC-NOTIFY-001 通知機能            @po  Could

次のアクション:
  /spec-review SPEC-AUTH-002  → レビュー中の仕様書を確認
  /tasks-plan                 → approved の仕様書でタスク分割
```
