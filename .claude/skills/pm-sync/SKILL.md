# ⚠️ pm-sync — 廃止済み

このスキルは **廃止** されました。

ローカル PM ツール（`tools/pm/`）は GitHub Projects V2 に移行したため、
ローカルファイルと PM ツール間の同期処理は不要になりました。

---

## 移行先

GitHub Projects は GitHub 上でリアルタイムに状態を管理するため、同期処理は不要です。

| 旧機能（pm-sync） | 新機能 |
|-----------------|-------|
| pm-data.json の読み込み | `gh project item-list` |
| task-list.md の更新 | `/task-start` / `/task-done` が自動更新 |
| 仕様書ステータスの照合 | `/spec-list` + GitHub Issue ラベル |

---

## 関連スキル

- `/task-board` — GitHub Project のタスクボードを表示
- `/tasks-plan` — 仕様書からタスクを分割・GitHub Issues を作成
- `/sprint-start` — スプリント開始（GitHub Project Iteration）
- `/sprint-end` — スプリント終了
