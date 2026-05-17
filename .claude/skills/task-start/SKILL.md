# /task-start — タスク開始・作業コンテキストの準備

**使う場面**: 工程4・5（タスク管理・実装）。タスクの実装を開始するとき。ブランチ作成から仕様書読み込みまで一気に準備する。

---

## 目的

タスクに関連する仕様書・設計書・受入基準を読み込んだ上でブランチを作成し、
「何を作るべきか」を明確にした状態で実装を開始できるようにする。

---

## 実行手順

### STEP 1: タスクの選択

引数でタスク ID が指定された場合はそのタスクを使用する。
`/task-start TASK-001` のように呼び出す。

指定がない場合は `tasks/task-list.md` から未着手タスクを表示し、どれを開始するか確認する:

```
📋 未着手のタスク:
  TASK-001  ログイン API エンドポイント実装  SP:3  [SPEC-AUTH-001]
  TASK-002  ログイン画面 UI 実装             SP:2  [SPEC-AUTH-001]  ※ TASK-001 に依存
  TASK-004  アカウントロック機能実装          SP:3  [SPEC-AUTH-001]

どのタスクを開始しますか？
```

依存タスクが完了していない場合は「TASK-002 は TASK-001 が完了してから開始できます」と警告する。

### STEP 2: 関連ファイルの読み込み

選択したタスクに紐付く以下を読み込む:
- 対応する仕様書（Spec ID から `docs/spec/` を検索）
- 関連する設計書（`docs/design/`）
- Constitution.md のコーディングルールセクション

### STEP 3: ブランチの作成

```bash
# タスクの GitHub Issue 番号を確認してブランチ作成
git checkout main
git pull origin main
git checkout -b feat/[issue番号]-[タスク概要]

# 例:
git checkout -b feat/42-login-api-endpoint
```

Issue 番号がわからない場合は「GitHub Issue 番号は何ですか？わからない場合はスキップします」と確認する。

### STEP 4: 作業コンテキストのサマリー表示

```markdown
## 🚀 タスク開始: TASK-001

### 作業ブランチ
feat/42-login-api-endpoint

### 対応仕様書
SPEC-AUTH-001: ログイン機能（docs/spec/auth/spec-auth-001-login.md）

### このタスクで実装すること
- POST /api/auth/login エンドポイント
- メールアドレス + パスワードの認証処理
- JWT トークンの発行

### ✅ 完了条件（受入基準）
Given: 正しいメールアドレスとパスワードが入力された
When:  POST /api/auth/login を実行する
Then:  HTTP 200 と JWT トークンが返る

Given: 誤ったパスワードが入力された
When:  POST /api/auth/login を実行する
Then:  HTTP 401 と "認証に失敗しました" メッセージが返る

### 関連する設計
docs/design/component-design.md → AuthService クラス

### ⚠️ 注意事項
- パスワードは bcrypt でハッシュ化すること（Constitution.md: セキュリティ原則）
- エラーメッセージは仕様書の文言を使用すること
```

### STEP 5: task-list.md のステータス更新

```
tasks/task-list.md の TASK-001 ステータスを「未着手」→「進行中」に更新しますか？
```

---

## 次のステップ

```
/impl-guide → 実装の具体的なガイダンスを取得
/task-done  → 実装完了後に PR を作成
```
