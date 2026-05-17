# SKILL: pm-setup — SDD プロジェクト管理ツール セットアップウィザード

## 概要

`templates/tools/pm/` にある SDD Project Manager を新しいプロジェクトにコピーし、
プロジェクト固有の初期設定を対話的に行うウィザードです。

---

## 実行タイミング

- 新規プロジェクトで SDD ハーネスを導入した直後
- `sdd-start` スキル完了後のフォローアップとして実行
- プロジェクト管理ツールをリセットしたい場合

---

## ウィザード手順

### STEP 1: ファイルのコピー

```bash
# ハーネスリポジトリ内の PM ツールをプロジェクトにコピー
cp -r <ハーネスパス>/templates/tools/pm/ <プロジェクトパス>/tools/pm/
```

**確認事項**:
- `tools/pm/index.html` が存在するか
- `tools/pm/css/style.css` が存在するか
- `tools/pm/js/data.js` および `app.js` が存在するか

---

### STEP 2: プロジェクト情報の収集（AI が対話的に質問する）

以下を順番にユーザーに質問してください:

```
質問1: プロジェクト名は何ですか？
       例: 「ECサイトリニューアル」「社内ポータル刷新」

質問2: プロジェクトの概要を一言で教えてください。
       例: 「既存の PHP 製 EC サイトを Next.js で再構築する」

質問3: チームメンバーの GitHub ハンドルを教えてください（カンマ区切り）。
       例: @alice, @bob, @carol

質問4: 仕様書ディレクトリのパスを教えてください。
       デフォルト: docs/spec/

質問5: スプリント期間は何週間ですか？
       デフォルト: 2週間
```

---

### STEP 3: 初期データの生成

収集した情報をもとに、`tools/pm/js/data.js` 内の `SEED_DATA` を上書きする
カスタム初期設定ファイル `tools/pm/js/init-config.js` を生成します。

```javascript
// tools/pm/js/init-config.js（自動生成）
window.SDD_INIT_CONFIG = {
  project: {
    name: "<プロジェクト名>",
    description: "<概要>",
    members: ["@alice", "@bob"],     // 収集した値
    specPath: "docs/spec/",          // 収集した値
    constitutionPath: ".constitution/Constitution.md"
  },
  sprintDurationWeeks: 2             // 収集した値
};
```

`index.html` に `<script src="js/init-config.js"></script>` を追加し、
`data.js` の `SEED_DATA.project` を `window.SDD_INIT_CONFIG.project` で
上書きするよう `load()` 関数を修正します。

---

### STEP 4: 初期スプリントの作成（オプション）

```
質問: 最初のスプリントを今すぐ設定しますか？ (Y/n)
```

Y の場合:
```
質問: Sprint 1 のゴールを教えてください。
質問: 開始日は？（YYYY-MM-DD、デフォルト: 今日）
質問: 終了日は？（デフォルト: 開始日 + スプリント期間）
```

---

### STEP 5: 完了確認

```
✅ セットアップが完了しました！

📁 ツールの場所: tools/pm/index.html
🌐 ブラウザで開く: file:///path/to/project/tools/pm/index.html

次のステップ:
1. ブラウザで tools/pm/index.html を開く
2. 「仕様書管理」で最初の仕様書（Constitution の内容をもとに）を追加する
3. 「スプリント管理」でスプリントを開始する
4. `sprint-start` スキルを実行してスプリントの詳細を設定する
```

---

## 注意事項

- `data.js` の `SEED_DATA` を直接編集するのではなく、`init-config.js` で上書きすることで
  ツールのアップデート時にマージが容易になります。
- `tools/pm/` ディレクトリは `.gitignore` に追加してもよいですが、
  チームで共有する場合はリポジトリに含めることを推奨します。
- データは `localStorage` の `sdd-pm-data` キーに保存されます。
  別マシンで利用する場合は「設定 → JSON をダウンロード」でエクスポートし、
  移行先で「JSON を読み込む」でインポートしてください。

---

## 関連スキル

- `sdd-start` — SDD ハーネス全体の初期セットアップ
- `sprint-start` — スプリント開始の対話ウィザード
- `sprint-end` — スプリント終了とふりかえりへの橋渡し
