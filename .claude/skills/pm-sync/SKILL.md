# /pm-sync — PM ツール ↔ プロジェクトファイル 双方向同期

**使う場面**: PM ツール（tools/pm/index.html）とプロジェクトのマークダウンファイルを同期したいとき。
ブラウザの File System Access API と組み合わせることで完全な双方向同期が実現する。

---

## 目的

`tasks/pm-data.json`（ブラウザが自動書き込み）を起点に、
プロジェクト内のマークダウンファイルを最新状態に保つ。
逆方向（Markdown → pm-data.json）の同期も行い、ブラウザ側が最新データを取り込めるようにする。

---

## 実行手順

### STEP 1: pm-data.json の確認

```
tasks/pm-data.json を読む
```

存在しない場合:
```
❌ tasks/pm-data.json が見つかりません。

先にブラウザの PM ツールで以下を実行してください:
  1. tools/pm/index.html を Chrome/Edge で開く
  2. 設定（⚙️）→「プロジェクトフォルダを接続」
  3. このプロジェクトのルートフォルダを選択
  → 自動的に tasks/pm-data.json が作成されます
```

### STEP 2: プロジェクトファイルとの照合

以下のファイルを読む（存在する場合のみ）:
- `docs/spec/*.md` — 仕様書ファイル群（frontmatter の `status:` を参照）
- `tasks/task-list.md` — 既存のタスク一覧（差分確認用）
- `.constitution/Constitution.md` — プロジェクト憲章（用語集確認）

### STEP 3: tasks/task-list.md の再生成

pm-data.json の tasks・sprints データから `tasks/task-list.md` を生成・上書きする。

フォーマット:
```markdown
# タスク一覧

> 最終更新: YYYY-MM-DD（/pm-sync により生成）

---

## Sprint N（ステータス）

- **ゴール**: ...
- **期間**: YYYY-MM-DD 〜 YYYY-MM-DD
- **進捗**: X / Y SP 完了

| ID | タイトル | ステータス | 担当者 | SP | 仕様書 | Issue | PR | ブランチ |
|----|---------|-----------|--------|:--:|--------|-------|:--:|----------|
| TASK-001 | ... | ✅ done | @alice | 3 | SPEC-AUTH-001 | #10 | #11 | feat/10-xxx |

## サマリー
| 総タスク | 完了 | 進行中 | 未着手 | レビュー中 |
...
```

### STEP 4: 仕様書ステータスの照合

`docs/spec/` 内の各マークダウンファイルを読み、frontmatter または `## ステータス` 行から status を抽出する。

pm-data.json の specs と照合して:
- **差分があるもの**: どちらが新しいか確認（更新日を比較）
- **新しい仕様書がある**: pm-data.json に追加（ユーザーに確認）
- **削除された仕様書**: `deprecated` に変更（ユーザーに確認）

ユーザーに確認が必要な差分は一覧で提示し、承認を得てから変更する。

```
仕様書の差分が見つかりました:

| Spec ID | ファイルのステータス | PM ツールのステータス | 推奨アクション |
|---------|--------------------|--------------------|--------------|
| SPEC-AUTH-001 | approved | review | ファイルを優先（更新日が新しい） |
| SPEC-DASH-001 | draft | review | PM ツールを優先（更新日が新しい） |

この差分を適用しますか？（yes / no / 個別に選択）
```

### STEP 5: pm-data.json の更新（← 方向）

STEP 4 で承認された変更を pm-data.json に反映して上書き保存する。
これによりブラウザ側が「プロジェクトから取得」ボタンで最新データを取り込める。

### STEP 6: 同期レポートの出力

```markdown
## ✅ /pm-sync 完了（YYYY-MM-DD HH:MM）

### 更新したファイル
- tasks/task-list.md — X タスクを出力
- tasks/pm-data.json — 仕様書 X 件のステータスを更新

### タスクサマリー
| ステータス | 件数 | SP |
|-----------|------|----|
| 完了       | X    | X  |
| 進行中     | X    | X  |
| レビュー中 | X    | X  |
| 未着手     | X    | X  |

### 仕様書サマリー
| ステータス | 件数 |
|-----------|------|
| approved  | X    |
| review    | X    |
| draft     | X    |

### 次のアクション
→ ブラウザの PM ツールで「設定 → プロジェクトから取得」を実行してください
```

---

## 同期フロー図

```
ブラウザ（PM ツール）        プロジェクトファイル
      |                           |
      | ──自動プッシュ──▶          |
      |    tasks/pm-data.json     |
      |    tasks/task-list.md     |
      |                           |
      |      /pm-sync スキル実行   |
      |                     ◀────── docs/spec/*.md 読込
      |                     ────── tasks/task-list.md 更新
      |                     ────── tasks/pm-data.json 更新
      |                           |
      | ◀──「プロジェクトから取得」─ |
      |    pm-data.json 読込       |
      |                           |
```

---

## 注意事項

- `pm-data.json` が存在しない場合はブラウザ側で先に「フォルダ接続」を行うこと
- `tasks/task-list.md` は完全に上書きされる（コメント等は消える）
- 仕様書ファイルの status は frontmatter `status:` または `**ステータス**: ` の形式を認識する
- 差分が大きい場合は一度に全更新せず、ユーザーと段階的に確認しながら進める

---

## 関連スキル

```
/pm-setup    → PM ツールの初期設定
/sprint-end  → スプリント完了時の pm-data.json 更新
/spec-list   → 仕様書のステータス確認
```
