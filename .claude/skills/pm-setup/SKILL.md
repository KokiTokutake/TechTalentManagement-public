# ⚠️ pm-setup — 廃止済み

このスキルは **廃止** されました。

ローカル PM ツール（`tools/pm/`）は GitHub Projects V2 に置き換えられました。

---

## 移行先

プロジェクト管理は GitHub 上で一元管理します:

| 旧機能（ローカル PM ツール） | 新機能（GitHub Projects） |
|--------------------------|--------------------------|
| タスクボード（ローカル HTML） | GitHub Project ボード |
| スプリント管理（localStorage） | GitHub Project Iterations |
| タスク一覧 | GitHub Issues |
| メトリクス | GitHub Insights |

---

## セットアップ方法

```
/sdd-start → STEP 2〜4 で GitHub リポジトリ + Project を自動作成
```

既にリポジトリがある場合は手動で:

```bash
# GitHub Project を作成
gh project create --owner <owner> --title "<プロジェクト名>"

# Status フィールドを追加
gh project field-create <number> --owner <owner> \
  --name "Status" --data-type SINGLE_SELECT \
  --single-select-options "Todo,In Progress,In Review,Done"

# Sprint フィールドを追加（Iteration）
gh project field-create <number> --owner <owner> \
  --name "Sprint" --data-type ITERATION

# .github/project-config.json に設定を保存
```

---

## 関連スキル

- `/sdd-start` — 新プロジェクトの初回セットアップ（GitHub Project 作成を含む）
- `/sprint-start` — スプリント開始（GitHub Project Iteration を使用）
- `/task-board` — タスクボード確認（GitHub Project を参照）
