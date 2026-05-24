# /task-board — タスクボード状態確認

**使う場面**: 工程4・5（タスク管理・実装中）。スプリントの進捗を把握したいとき。朝会・進捗確認で使う。

---

## 目的

GitHub Project のアイテム状態をもとにタスクボードを表示し、
スプリントの進捗・ブロッカー・バーンダウンを可視化する。

---

## 実行手順

### STEP 0: GitHub Project 設定の確認

`.github/project-config.json` が存在し、`project_number` が設定されているか確認する:

```bash
test -f .github/project-config.json && \
  jq -e '.project_number != 0 and .project_number != null' .github/project-config.json
```

**存在しない・または未設定の場合:**

```
⛔ GitHub Project が設定されていません。

GitHub Project からタスクボードを表示できません。

セットアップを先に実行してください:
  /github-project-init → 既存プロジェクトに GitHub Project を後付けセットアップ

代替: tasks/task-list.md を読み込んでマークダウン形式で表示します。
続けますか？ (Y/n)
```

Y の場合は `tasks/task-list.md` の内容をボード形式で表示して終了する（GitHub との連動なし）。

### STEP 1: GitHub Project からデータを取得

`.github/project-config.json` を読み込み、Project データを取得する:

```bash
OWNER=$(jq -r '.owner' .github/project-config.json)
PROJECT_NUMBER=$(jq -r '.project_number' .github/project-config.json)

gh project item-list $PROJECT_NUMBER --owner $OWNER --format json > /tmp/project-items.json
```

### STEP 2: タスクボードの表示

取得したデータをもとにボードを表示する:

```markdown
## 📋 タスクボード（YYYY-MM-DD）

### スプリント: Sprint N「<ゴール>」（<開始日> 〜 <終了日>）
### 進捗: X / Y SP 完了（X%）
[████████░░░░░░░░] 50%

### 🔴 Todo（X SP）
| Issue | タスク名 | Spec | SP | 担当 | 備考 |
|-------|---------|------|----|----|------|
| #43 | ログイン UI 実装 | SPEC-AUTH-001 | 2 | @carol | #42 完了後に開始可 |

### 🟡 In Progress（X SP）
| Issue | タスク名 | Spec | SP | 担当 | ブランチ |
|-------|---------|------|----|----|---------|
| #42 | ログイン API 実装 | SPEC-AUTH-001 | 3 | @dev | feat/42-login-api |

### 🔵 In Review（X SP）
| Issue | タスク名 | PR | SP | 担当 |
|-------|---------|-----|----|----|
| #40 | 設計書更新 | #38 | 1 | @tl |

### ✅ Done（X SP）
| Issue | タスク名 | PR | SP |
|-------|---------|-----|---|
| #38 | Constitution.md 初版 | #35 | 1 |

### ⛔ ブロック中
| Issue | 理由 | 対応者 |
|-------|------|--------|
| - | - | - |
```

### STEP 3: 現在のスプリントのみ表示するフィルタ

スプリントが設定されている場合は現在の Iteration に絞り込む:

```bash
# 現在のスプリント名を確認
CURRENT_SPRINT="Sprint 2"

# スプリント内の Item のみ取得
cat /tmp/project-items.json | jq --arg sprint "$CURRENT_SPRINT" \
  '[.items[] | select(.fieldValues[] | select(.field.name == "Sprint") | .title == $sprint)]'
```

スプリントが設定されていない場合はバックログも含めて全 Issue を表示する。

### STEP 4: リスク・ブロッカーの確認

```
❓ 確認事項:
- ブロックされているタスクはありますか？
- 期限に間に合わないリスクがありますか？
```

ユーザーが「ブロックあり」と答えた場合:
- GitHub Issue にコメントを追記する
- `gh issue comment <番号> --body "⛔ ブロック中: <理由>"`

### STEP 5: バーンダウン確認

スプリント残日数と SP 残量からバーンダウンを計算して表示する:

```
📉 バーンダウン
スプリント残: X 日
残 SP: X / Y
1日あたりのペース: X SP/日（目標: Y SP/日）

⚠️ このペースでは完了しない可能性があります → スコープ調整を検討してください
```

### STEP 6: 次のアクション提案

状況に応じてアクションを提案する:

- Todo タスクがある場合: `/task-start TASK-XXX → 次のタスクを開始できます`
- In Review が詰まっている場合: `PR レビューが溜まっています。レビューを優先してください`
- ブロックがある場合: `ブロックを解消するために @xxx に確認が必要です`
- Done SP が多い場合: `進捗良好です！/spec-list approved で次のタスクを計画できます`

---

## GitHub Project へのリンク

```
🔗 GitHub Project ボード:
  https://github.com/orgs/<owner>/projects/<project_number>
```
