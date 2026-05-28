# SDD ハーネス フィードバック（TechTalentManagement プロジェクト振り返り）

**作成日**: 2026-05-28  
**対象プロジェクト**: TechTalentManagement（TASK-001〜TASK-088、88タスク）  
**対象ハーネス**: `.claude/skills/`・`.claude/hooks/`・`.github/workflows/ci.yml`

---

## エグゼクティブサマリー

スペック駆動開発（SDD）ハーネスを使用して 18 スプリント・88 タスクの開発を完了した。  
ゲート制度（design-create → task-start → test-write → task-done）の基本的な流れは機能したが、
以下の 8 点でハーネスの補完が不十分であり、人の判断・介入に多くを依存した。

---

## 発見された課題と改善

### 🔴 HIGH: E2Eテストゲートが形骸化している

**症状**:
- `tests/e2e/` に 30 ファイル・55 テストが生成されたが、47 テスト（85%）が `test.skip()`
- `/task-done` の STEP 2.5 ゲートはファイル存在のみを確認するため、全スキップ状態でも通過できた
- スキップ理由が記載されていないテストが多数存在した

**根本原因**:
- ゲートが「ファイルが存在するか」のみを判定し、「テストが実際に通過したか」を確認しない
- `test.skip()` に理由を書く規約がなく、何が「スキップ不可避」で何が「実装漏れ」か判別不能

**適用済み改善**:
- `/task-done` STEP 3 を「存在確認」から「通過率確認 + スキップ理由チェック」に変更
- `test.skip()` は `test.skip(true, '理由')` 形式を必須化
- スキップ理由が「外部サービス不可避」以外の場合は停止する

**CI への改善**:
- `.github/workflows/ci.yml` に E2E スキップ率チェックジョブを追加
- スキップ率 50% 超で警告（理由なし `test.skip()` はエラー扱い）

---

### 🔴 HIGH: playwright.config の baseURL がバックエンドを指している

**症状**:
- `playwright.config.ts` の `baseURL: 'http://localhost:5108'` はバックエンド API ポート
- フロントエンドは `http://localhost:3000` で動作するため、UI テストの `page.goto()` が機能しない
- テストが API に直接リクエストを送る構造になり、UI 動作の検証ができていなかった

**根本原因**:
- `/test-write` スキルのテンプレートで `baseURL` の使い分けが不明確
- プロジェクト初期化時に playwright.config.ts を生成した際に正しく設定されなかった

**適用済み改善**:
- `playwright.config.ts` の `baseURL` を `http://localhost:3000`（フロントエンド）に修正
- 環境変数 `PLAYWRIGHT_UI_BASE` でオーバーライド可能に
- `/test-write` スキルに「UI テスト vs API テストの URL 分離」セクション（STEP 5.5）を追加
- API テストは各ファイルで `const API_BASE = 'http://localhost:5108'` を直接定義するよう明記

---

### 🔴 HIGH: 認証未実装状態がハーネスで未考慮

**症状**:
- TASK-002（Entra ID SSO）は最初から「意図的延期」として実装されなかった
- 全 88 タスクの実装が `/select-user`（UI 上でユーザーを手動選択）で代替認証
- E2E テストで「認証後の操作」を検証できない状態が続いたが、ハーネスがこれを認識・記録しなかった

**根本原因**:
- `/task-start` に「現在の認証状態を確認・記録する」ステップがなかった
- 認証が動作しているかどうかを前提に CLAUDE.md が書かれていたが、実際は代替運用

**適用済み改善**:
- `/task-start` STEP 5.5「認証状態の確認」を追加
- 認証が未実装の場合、認証代替方法・影響するテスト・制約を CLAUDE.md に記録
- E2E テスト生成時（`/test-write`）に認証状態を引き継ぎ、認証依存 AC は理由付き skip

---

### 🟡 MEDIUM: main への直接コミットが常態化した

**症状**:
- シードデータ拡充（`DataSeeder.cs`）・Docker 設定修正・TASK-070 の一部が main に直接コミット
- git ログで `feat:` プレフィックスのコミットが PR なしで main に存在する（少なくとも 8 件）
- CLAUDE.md で「main への直接コミット・プッシュは禁止（フックで自動ブロック）」と記載があるが、フックは存在しなかった

**根本原因**:
- 宣言と実装が乖離していた（「フックで自動ブロック」とあるが実際のフックはなかった）
- 運用変更（シードデータ・Docker）はタスクフロー外であり、ブランチを作るべきか曖昧

**適用済み改善**:
- `.claude/hooks/guard-main-commit.sh` を新規作成（main への git commit を検知して警告）
- **手動対応が必要**: `.claude/settings.json` の `PreToolUse[Bash].hooks` に以下を追加してください:
  ```json
  {
    "type": "command",
    "command": "bash .claude/hooks/guard-main-commit.sh"
  }
  ```
- 運用変更（シードデータ等）も `chore/` ブランチ経由で PR を作成するよう CLAUDE.md に明示することを推奨

---

### 🟡 MEDIUM: task-list.md と GitHub Project の乖離が放置された

**症状**:
- Sprint 18 は `tasks/task-list.md` で「⬜ 未着手 / 0 SP」のまま全タスクが ✅ done になっていた
- サマリーセクションが「完了 67 / 未着手 21」という古い値のまま放置されていた
- タスクボード（`/task-board`）の出力と task-list.md の内容が矛盾していた

**根本原因**:
- `/task-done` は GitHub Project のステータスを更新するが、`task-list.md` のスプリントヘッダーは更新しない
- スプリントヘッダーの更新は手動に依存しており、スキップされやすい

**適用済み改善**:
- `/sprint-end` STEP 4.5「task-list.md と GitHub Project の同期チェック」を追加
- スプリント終了時に乖離を自動検出し、修正を提案する

---

### 🟡 MEDIUM: CI に E2E テストが存在しなかった

**症状**:
- `.github/workflows/ci.yml` は backend ビルド・テスト・frontend ビルド・型チェックのみ
- `/test-write` で生成した 30 ファイルの E2E テストは CI で一度も実行されなかった
- テストが実際に通過しているかを PR 時に自動確認する仕組みがなかった

**根本原因**:
- E2E テストには Docker Compose で起動した DB・バックエンド・フロントエンドが必要であり、CI での実行が難しい
- ハーネスが CI の E2E 実行を想定していなかった

**適用済み改善**:
- `ci.yml` に `e2e-check` ジョブを追加（Docker 不要な静的チェック）:
  - テストファイルのスキップ率計算（50% 超で警告）
  - 理由なし `test.skip()` の検出（エラー表示）
- **将来改善（ハーネスへの提案）**: Docker Compose を使った E2E 実行ジョブのテンプレートを `/test-write` スキルに追加する

---

### 🟢 LOW: スプリント日付が形骸化した

**症状**:
- Sprint 7〜18（12 スプリント）が事実上同一のセッション内で完了した
- `task-list.md` の sprint 期間が「未定」のままのものが多かった
- 14 日スプリントの計画に対し、実際の実装速度が大幅に異なる

**根本原因**:
- ソロ AI 駆動開発では人間チームの「速度制約」が存在しないため、スプリントは形式的な区切りになる
- ハーネスのスプリント計画はチーム開発を想定しており、AI ソロ開発には適していない

**改善提案（ハーネスへのフィードバック）**:
- スプリントを「時間軸」ではなく「機能単位の完了」で区切る「フィーチャースプリント」モードを追加する
- `/sprint-start` に「ソロ AI 開発モード」オプションを追加し、日付管理を省略可能にする
- あるいは `tasks/task-list.md` の sprint 日付フィールドをオプションに変更する

---

### 🟢 LOW: 設計書作成（/design-create）の形式的な適用

**症状**:
- 設計書が必須ゲートとして機能したが、一部タスクでは設計書のレビューが形式的だった
- 設計書 → approved → task-start の流れは守られたが、「設計の質」を問うチェックが少なかった

**改善提案**:
- `/design-create` の STEP 0（過去レビューパターン確認）を強化し、前回の指摘を自動引用する
- 設計書テンプレートに「前回 PR レビューでの指摘事項」セクションを追加する

---

### 🟢 LOW: Docker 統合検証がスキルフローに存在しない

**症状**:
- シードデータが最後になってまとめて追加された（全機能実装後）
- フロントエンドが Docker でバックエンドに接続できないバグが最終段階で発見された
- 実装 → 単体テスト → PR の流れで Docker 統合環境での検証がなかった

**改善提案**:
- `/task-done` の STEP 3 後に「Docker 統合確認チェック」を任意ステップとして追加:
  ```
  ❓ Docker Compose での動作確認を行いましたか？
    Y) docker compose up --build で対象機能を確認済み
    N) スキップ（次のタスク後にまとめて確認する）
  ```
- シードデータは機能実装と同一タスク（または専用の `[SEED]` タグタスク）として計画する

---

## ハーネスリポジトリへの反映手順

このドキュメントで記載した改善は以下のファイルに適用済みです:

| ファイル | 変更内容 |
|---------|---------|
| `.claude/skills/task-done/SKILL.md` | STEP 3 をテスト通過率チェックに強化 |
| `.claude/skills/task-start/SKILL.md` | STEP 5.5 認証状態の確認を追加 |
| `.claude/skills/test-write/SKILL.md` | STEP 5.5 URL分離・STEP 6 baseURL修正 |
| `.claude/skills/sprint-end/SKILL.md` | STEP 4.5 task-list.md 同期チェックを追加 |
| `.claude/hooks/guard-main-commit.sh` | 新規作成（main への直接コミットを警告） |
| `.github/workflows/ci.yml` | E2E スキップ率チェックジョブを追加 |
| `playwright.config.ts` | baseURL をフロントエンドポートに修正 |

### ハーネスリポジトリへの Cherry-pick 手順

```bash
# このリポジトリから変更を抽出してハーネスリポジトリに適用する場合:

# 1. 変更されたスキルファイルをコピー
cp .claude/skills/task-done/SKILL.md /path/to/harness/.claude/skills/task-done/SKILL.md
cp .claude/skills/task-start/SKILL.md /path/to/harness/.claude/skills/task-start/SKILL.md
cp .claude/skills/test-write/SKILL.md /path/to/harness/.claude/skills/test-write/SKILL.md
cp .claude/skills/sprint-end/SKILL.md /path/to/harness/.claude/skills/sprint-end/SKILL.md

# 2. 新規フックをコピー
cp .claude/hooks/guard-main-commit.sh /path/to/harness/.claude/hooks/

# 3. ハーネスの settings.json に guard-main-commit.sh を手動追加（セルフモディフィケーションに該当するため自動化不可）
# PreToolUse[Bash].hooks に以下を追加:
# { "type": "command", "command": "bash .claude/hooks/guard-main-commit.sh" }
```

### 手動対応が必要な箇所（自動適用できなかった変更）

1. **`.claude/settings.json` への `guard-main-commit.sh` 追加**  
   セルフモディフィケーション制限によりブロックされました。以下を手動で追加してください:
   ```json
   {
     "matcher": "Bash",
     "hooks": [
       { "type": "command", "command": "bash .claude/hooks/guard-dangerous-bash.sh" },
       { "type": "command", "command": "bash .claude/hooks/guard-main-commit.sh" }
     ]
   }
   ```

---

## 全体的な評価

| 観点 | 評価 | コメント |
|------|------|---------|
| ゲート制度（設計→実装→テスト→PR） | ★★★★☆ | 基本的な流れは機能。テストゲートの品質が課題 |
| GitHub Project 連携 | ★★★☆☆ | Issue/PR 連携は機能。task-list.md との乖離が惜しい |
| E2E テスト品質保証 | ★★☆☆☆ | ファイル生成まで自動化されたが、実際の通過確認が弱い |
| ブランチ・PR ワークフロー | ★★★☆☆ | 主要タスクは PR 経由。運用変更の例外が常態化 |
| CI/CD 連携 | ★★☆☆☆ | ビルド・型チェックのみ。E2E は CI で未実行 |
| スプリント計画の実態 | ★★☆☆☆ | AI ソロ開発の速度に計画が追いつかなかった |

**総合評価**: SDD の骨格は正しく機能した。次のプロジェクトでは「テストの実効性」と「main 保護の実装」を優先して改善することを推奨する。
