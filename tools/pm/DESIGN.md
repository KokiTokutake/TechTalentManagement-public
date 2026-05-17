# SDD Project Manager — 設計ドキュメント

> **目的**: このドキュメントは次の開発セッションで実装を継続できるよう、現状の設計・実装状況・未完了タスクを記録するものです。

---

## 1. プロダクト概要

SDD（仕様駆動開発）ハーネス専用の Web プロジェクト管理ツール。
各プロジェクトに `templates/tools/pm/` をコピーして `index.html` をブラウザで開くだけで動作する。

### 設計方針

| 方針 | 内容 |
|------|------|
| ゼロビルド | npm/webpack 不要。`file://` プロトコルで直接動作 |
| バニラ JS | ES5〜ES2019、外部ライブラリなし |
| データ永続化 | `localStorage` キー `sdd-pm-data` に JSON で保存 |
| グローバル名前空間 | `window.SDD.store`（データ層）/ `window.SDD.app`（UI層）|
| ルーティング | ハッシュ不使用。`navigate(view)` 関数で内部状態管理 |
| ES モジュール不使用 | `file://` 制約のため `<script src>` 順序依存 |

### ファイル構成

```
templates/tools/pm/
├── index.html          # HTML シェル（80行）
├── css/
│   └── style.css       # 全スタイル（516行）
└── js/
    ├── data.js         # データ層・CRUD（557行）
    └── app.js          # ビュー・ルーター・モーダル（1284行）
```

---

## 2. データモデル（localStorage スキーマ）

`localStorage.getItem('sdd-pm-data')` で取得できる JSON 構造:

```typescript
interface SDDStore {
  project: {
    name: string;
    description: string;
    members: string[];       // "@alice" 形式
    specPath: string;        // "docs/spec/"
    constitutionPath: string;
  };

  specs: Spec[];
  tasks: Task[];
  sprints: Sprint[];
  retros: Retro[];
  issues: Issue[];
  activityLog: ActivityEntry[];  // 最大200件（新着が先頭）
}

interface Spec {
  id: string;              // "SPEC-AUTH-001"
  title: string;
  status: 'draft' | 'review' | 'approved' | 'deprecated';
  category: string;        // "AUTH", "UI", "INFRA" 等
  priority: 'Must' | 'Should' | 'Could';
  owner: string;           // "@alice"
  file: string;            // "docs/spec/auth/spec-auth-001.md"
  description: string;
  acceptanceCriteria: string[];
  taskIds: string[];       // 紐付きタスクID（自動管理）
  createdAt: string;       // "YYYY-MM-DD"
  updatedAt: string;
}

interface Task {
  id: string;              // "TASK-001"
  title: string;
  specId: string;          // 紐付き Spec ID
  sprintId: string;        // 紐付き Sprint ID
  assignee: string;        // "@alice"
  status: 'pending' | 'in_progress' | 'review' | 'done';
  sp: number;              // ストーリーポイント
  issueNumber: number | null;  // GitHub Issue 番号
  branch: string;          // "feat/10-login-api"
  pr: number | null;       // GitHub PR 番号
  notes: string;
  createdAt: string;
  updatedAt: string;
}

interface Sprint {
  id: string;              // "SPRINT-001"
  name: string;            // "Sprint 1"
  goal: string;
  status: 'planning' | 'active' | 'completed';
  startDate: string;       // "YYYY-MM-DD"
  endDate: string;
  taskIds: string[];       // 紐付きタスクID（自動管理）
  velocity: number;        // 完了 SP 合計（完了時に記録）
  createdAt: string;
}

interface Retro {
  id: string;              // "RETRO-001"
  sprintId: string;
  sprintName: string;
  date: string;
  facilitator: string;
  participants: string;
  keep: string[];
  problem: string[];
  try_: string[];          // "try" は予約語のため try_ を使用
  sddRatings: {
    constitution: number;
    specify: number;
    plan: number;
    tasks: number;
    implement: number;
    verify: number;
    operation: number;
  };
  actionItems: ActionItem[];
  nextNotes: string;
  metrics: {
    completedSP: number;
    highBugs: number;
    specFirst: number;     // 仕様書先行割合 %
    prWithAC: number;      // 受入基準付き PR 割合 %
  };
  createdAt: string;
  updatedAt: string;
}

interface ActionItem {
  id: string;
  action: string;
  assignee: string;
  dueDate: string;
  status: 'pending' | 'done';
}

interface Issue {
  id: string;              // "ISSUE-001"
  title: string;
  type: 'bug' | 'blocker' | 'question' | 'risk';
  priority: 'High' | 'Medium' | 'Low';
  status: 'open' | 'in_progress' | 'resolved';
  specId: string;
  taskId: string;
  reporter: string;
  description: string;
  resolution: string;
  createdAt: string;
  updatedAt: string;
}

interface ActivityEntry {
  timestamp: string;       // ISO 8601
  type: string;            // "spec_created", "task_updated" 等
  text: string;
  user: string;
}
```

### ID 採番ルール

```javascript
// nextId(prefix, list) — data.js:238
// 既存IDの末尾数値の最大値 + 1 を 3桁ゼロ埋めで採番
// 例: ["SPEC-001", "SPEC-003"] → "SPEC-004"
```

---

## 3. アーキテクチャ詳細

### data.js（データ層）

`window.SDD.store` オブジェクトに以下のメソッドが定義されている:

| メソッド | 説明 |
|---------|------|
| `load()` | localStorage から読込（失敗時は SEED_DATA にフォールバック） |
| `save()` | localStorage へ書込 |
| `reset()` | SEED_DATA で初期化 |
| `nextId(prefix, list)` | ID 採番 |
| `createSpec(fields)` | Spec 作成 + activityLog |
| `updateSpec(id, fields)` | Spec 更新（Spec→Task 紐付き自動管理） |
| `deleteSpec(id)` | Spec 削除 |
| `createTask(fields)` | Task 作成 + Spec/Sprint 双方向リンク |
| `updateTask(id, fields)` | Task 更新（旧リンク解除→新リンク設定） |
| `deleteTask(id)` | Task 削除 + Spec/Sprint リンク解除 |
| `createSprint(fields)` | Sprint 作成 |
| `updateSprint(id, fields)` | Sprint 更新（active 化時に他 Sprint を planning に戻す） |
| `deleteSprint(id)` | Sprint 削除 + Task の sprintId をクリア |
| `createRetro(fields)` | Retro 作成 |
| `updateRetro(id, fields)` | Retro 更新 |
| `deleteRetro(id)` | Retro 削除 |
| `createIssue(fields)` | Issue 作成 |
| `updateIssue(id, fields)` | Issue 更新 |
| `deleteIssue(id)` | Issue 削除 |
| `getActiveSprint()` | status === 'active' の Sprint を返す |
| `getSprintTasks(sprintId)` | Sprint に紐付くタスクを返す |
| `getRemainingDays(endDate)` | 残り日数を返す（null = 計算不可） |
| `calcSddScore()` | SDD 遵守スコア（0〜100）を計算 |
| `exportData()` | JSON Blob をダウンロード |
| `importData(file)` | FileReader で JSON を読込・適用 |

### app.js（UI 層）

#### ルーター

```javascript
const VIEWS = ['dashboard','specs','tasks','sprints','retros','issues','settings'];
navigate(view)  // nav active 切替 + renderView() 呼出
renderView(view) // topbar タイトル/アクション設定 + 各 render* 呼出
```

#### ビュー関数（各 render* が HTML 文字列を返す）

| 関数 | ビュー | 主な内容 |
|------|--------|---------|
| `renderDashboard()` | ダッシュボード | SDD スコアリング、4 KPI カード、スプリント進捗バー、仕様書・タスク内訳グラフ、未解決課題、アクティビティログ |
| `renderSpecs()` | 仕様書管理 | フィルタータブ、一覧テーブル（ステータスクリックで変更可）、詳細/編集/削除ボタン |
| `renderTasks()` | タスクボード | スプリントフィルター、カンバン（pending/in_progress/review/done 4列）、← → ボタンでステータス遷移 |
| `renderSprints()` | スプリント管理 | スプリントカード一覧、バーンダウン表、アクティブ/完了操作 |
| `renderRetros()` | ふりかえり | 一覧モード ↔ 詳細モード切替、KPT 表示、SDD 工程評価、アクションアイテム |
| `renderIssues()` | 課題・ブロッカー | フィルタータブ（未解決/all）、課題カード一覧、解決ボタン |
| `renderSettings()` | 設定 | プロジェクト情報編集、メンバー管理、データリセット/エクスポート/インポート |

#### モーダルシステム

```javascript
SDD.app.openModal(type, mode, id)
// type: 'spec' | 'task' | 'sprint' | 'retro' | 'issue'
// mode: 'new' | 'edit' | 'detail'
// id:   編集・詳細表示時の対象ID

MODAL_CONFIGS[type] = {
  title: (mode, data) => string,
  body:  (mode, data) => html_string,
  save:  (mode, id)   => boolean
}
```

#### イベント委譲

`bindViewEvents(view)` がビュー切替のたびに `#content` にイベントリスナーを再設定する。
データ属性 `data-spec-edit`, `data-task-status`, `data-issue-resolve` 等でターゲットを識別。

---

## 4. CSS 設計

### カラーパレット（CSS カスタムプロパティ）

```css
--primary:     #1A237E  /* ネイビー（sdd-workflow.html と統一） */
--primary-mid: #3949AB
--primary-light:#E8EAF6
--sidebar-bg:  #263238  /* ダークグレー */
--bg:          #ECEFF1  /* ライトグレー */
--col-approved:#2E7D32
--col-draft:   #E65100
--col-review:  #1565C0
--col-high:    #C62828
--col-medium:  #E65100
--col-low:     #558B2F
```

### レイアウト構造

```
body
├── #sidebar (260px 固定、サイドバー)
└── #main (flex-grow)
    ├── .topbar (56px 固定、タイトル + アクションボタン)
    └── #content (スクロール可能、ビューごとに書換)
```

### グリッドユーティリティ

```css
.grid-2  /* 2列グリッド */
.grid-4  /* 4列グリッド（レスポンシブ: 幅 <900px で 2列） */
```

---

## 5. 実装状況（2026-05-16 時点）

### 完了済み機能

| 機能 | 状態 | 備考 |
|------|------|------|
| ダッシュボード表示 | ✅ | SDD スコア、KPI、スプリント進捗 |
| 仕様書 CRUD | ✅ | 一覧・追加・編集・削除・詳細・フィルター |
| 仕様書ステータス変更（クリック） | ✅ | テーブル内バッジクリックで次ステータスに遷移 |
| タスクボード（カンバン） | ✅ | 4列カンバン、← → で遷移 |
| スプリント CRUD | ✅ | 作成・編集・削除・アクティブ化・完了 |
| スプリント進捗（バーンダウン） | ✅ | SP 残量の日別テーブル表示 |
| ふりかえり CRUD | ✅ | KPT 記録、SDD 工程評価、アクションアイテム |
| 課題・ブロッカー管理 | ✅ | 種別/優先度フィルター、解決操作 |
| 設定画面 | ✅ | プロジェクト情報、メンバー、エクスポート/インポート/リセット |
| アクティビティログ | ✅ | CRUD 操作を自動記録（最大 200件） |
| データ永続化 | ✅ | localStorage |
| デモデータ | ✅ | SEED_DATA（仕様書 3件・タスク 5件・スプリント 1件） |

### 未検証・要確認事項

| 項目 | 優先度 | 詳細 |
|------|--------|------|
| ブラウザ実動作確認 | 高 | `file://` で index.html を開いてすべてのビューを操作する |
| モーダル保存後のビュー更新 | 高 | `saveModal()` → `navigate()` でリフレッシュされるか確認 |
| タスクステータス ← → ボタン | 高 | カンバン上の遷移ボタンが動作するか確認 |
| スプリント「完了」時の velocity 記録 | 中 | `updateSprint` で velocity を計算して記録しているか確認 |
| ふりかえり アクションアイテム追加 | 中 | 詳細モードでの追加/完了操作 |
| レスポンシブ（幅 < 900px） | 低 | `.grid-4` が 2列に崩れるか確認 |
| `retro.try_` フィールド名 | 中 | JS 予約語 `try` を `try_` にしているが、モーダルフォームの `id="m-retro-try"` と整合しているか確認 |

---

## 6. 既知の潜在バグ

### badge() 関数の status マッピング重複

```javascript
// app.js:20-30
// label オブジェクトに 'in_progress' と 'in_progress2' の2つが存在する
// 'in_progress2' は使われていない可能性が高い → 削除候補
const label = {
  in_progress:'進行中',
  in_progress2:'in_progress',  // ← 不要の可能性
};
```

### currentView の同期

```javascript
// app.js:1188
SDD.app.currentView = 'dashboard';
// navigate() を呼んでも currentView が更新されていない
// saveModal() → currentViewFromType() は type ベースのマッピングなので問題ないが、
// 他の箇所で currentView を参照する場合は注意
```

---

## 7. 未実装の改善候補

優先度順:

1. **スプリントバーンダウンチャート（グラフ）**: 現在は日別テーブル。Canvas 折れ線グラフに変更すると可視性が上がる
2. **仕様書詳細モーダルの受入基準表示**: 現状は `description` のみ表示。`acceptanceCriteria` 配列を箇条書きで表示する
3. **タスクフィルター（担当者・仕様書）**: カンバンの上部にフィルターを追加
4. **SDD スコア内訳表示**: ダッシュボードでスコアをクリックすると内訳を表示するモーダル
5. **ふりかえりへの自動データ連携**: スプリント完了時に completedSP を retro.metrics に自動設定
6. **印刷・PDF エクスポート**: `window.print()` 用のプリント CSS
7. **キーボードショートカット**: `n` で新規作成モーダル、`Esc` でモーダル閉じ

---

## 8. 関連スキル

PM ツールと連携する Claude Code スキル（`templates/.claude/skills/` 配下）:

| スキル | ファイル | 役割 |
|--------|---------|------|
| `/pm-setup` | `pm-setup/SKILL.md` | PM ツールの初期設定ウィザード |
| `/sprint-start` | `sprint-start/SKILL.md` | 新スプリント開始（対話型） |
| `/sprint-end` | `sprint-end/SKILL.md` | スプリント終了・ふりかえり連携 |

---

## 9. 次セッションでの作業手順

### ステップ 1: 動作確認

```
1. エクスプローラーで templates/tools/pm/index.html を開く
2. 各ビューをクリックして表示確認
3. 仕様書を1件追加して保存 → 一覧に反映されるか確認
4. タスクの ← → ボタンでカンバン移動を確認
5. 設定 → 「リセット」でデモデータに戻ることを確認
```

### ステップ 2: バグ修正

上記「未検証・要確認事項」で問題が見つかった箇所を修正する。

### ステップ 3: 機能追加

「未実装の改善候補」から優先度の高いものを実装する。

### ステップ 4: sdd-workflow.html の更新

`templates/docs/sdd-workflow.html` のスキルインデックスセクションに以下 3 スキルを追加:
- `/pm-setup`
- `/sprint-start`
- `/sprint-end`

---

## 10. プロジェクト全体の成果物一覧

```
templates/
├── .claude/
│   ├── CLAUDE.md               # テンプレート用 CLAUDE.md
│   └── skills/                 # 25 スキル（全スキル一覧は下記）
├── .constitution/
│   └── Constitution.md         # プロジェクト憲章テンプレート
├── .github/
│   ├── CODEOWNERS
│   ├── ISSUE_TEMPLATE/
│   └── PULL_REQUEST_TEMPLATE/
├── docs/
│   ├── sdd-workflow.html       # 業務フロー図（スイムレーン・スキルバッジ付き）
│   ├── spec/
│   │   └── spec-template.md
│   └── guides/
│       ├── onboarding-roadmap.md
│       ├── checklists.md
│       └── retrospective.md
├── tasks/
│   └── task-list.md
└── tools/
    └── pm/                     # Web PM ツール（本ドキュメント対象）
        ├── DESIGN.md           # 本ファイル
        ├── index.html
        ├── css/style.css
        └── js/
            ├── data.js
            └── app.js
```

### 25 スキル一覧

| # | スキル | カテゴリ |
|---|--------|---------|
| 1 | `/sdd-start` | 導入 |
| 2 | `/sdd-health` | 導入 |
| 3 | `/sdd-skills` | 導入 |
| 4 | `/charter-create` | 工程1 |
| 5 | `/charter-review` | 工程1 |
| 6 | `/spec-write` | 工程2 |
| 7 | `/spec-review` | 工程2 |
| 8 | `/spec-list` | 工程2 |
| 9 | `/spec-change` | 工程2 |
| 10 | `/design-create` | 工程3 |
| 11 | `/design-review` | 工程3 |
| 12 | `/tasks-plan` | 工程4 |
| 13 | `/task-start` | 工程5 |
| 14 | `/task-done` | 工程5 |
| 15 | `/task-board` | 工程5 |
| 16 | `/impl-guide` | 工程5 |
| 17 | `/impl-check` | 工程5 |
| 18 | `/accept-run` | 工程6 |
| 19 | `/spec-trace` | 工程6 |
| 20 | `/release-prep` | 工程7 |
| 21 | `/incident-handle` | 工程7 |
| 22 | `/retro-run` | 工程7 |
| 23 | `/pm-setup` | PM ツール |
| 24 | `/sprint-start` | PM ツール |
| 25 | `/sprint-end` | PM ツール |
| 26 | `/pm-sync` | PM ツール |
