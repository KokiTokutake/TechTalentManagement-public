# /test-run — Playwright テストの実行と結果レポート

**使う場面**: 工程5（実装）完了後、工程6（検証・受入）前。
`/test-write` で生成したテストを実行し、結果を仕様書と照合する。

---

## 目的

Playwright テストを実行し、受入基準の充足状況をスコア付きで可視化する。
失敗したテストは仕様書の AC ID と紐付けて原因特定を支援する。

---

## 前提条件

以下が揃っていること:
- `package.json` に `@playwright/test` が含まれている
- `playwright.config.ts` が存在する
- `tests/e2e/` にテストファイルが存在する（`/test-write` 実行済み）
- 開発サーバーが起動している（または `webServer` 設定済み）

---

## 実行手順

### STEP 1: 実行対象の確認

```
実行するテストを選んでください:

1. 全テスト（tests/e2e/**/*.spec.ts）
2. 特定の Spec ID（例: SPEC-AUTH-001）
3. 現在のタスクに紐付くテスト

[オプション]
- ヘッドレス実行（デフォルト）
- ブラウザ表示あり（--headed）
- デバッグモード（--debug）
```

### STEP 2: 事前確認

```bash
# 依存関係の確認
npx playwright --version

# ブラウザのインストール確認
npx playwright install --dry-run
```

インストールされていない場合:
```
⚠️ Playwright ブラウザが未インストールです。
以下を実行してください: npx playwright install chromium
```

### STEP 3: テストの実行

**全テスト実行:**
```bash
npx playwright test --reporter=json,html
```

**特定 Spec のテスト:**
```bash
npx playwright test spec-auth-001 --reporter=json,html
```

**失敗したテストのみ再実行:**
```bash
npx playwright test --last-failed
```

実行中は以下をリアルタイムで表示:
```
🧪 実行中: tests/e2e/spec-auth-001-login.spec.ts
  ✅ 正しい認証情報でログインするとダッシュボードへ遷移する (1.2s)
  ✅ 誤った認証情報でエラーメッセージが表示される (0.8s)
  ❌ 5回連続失敗でアカウントがロックされる (timeout)
```

### STEP 4: 結果の分析

`tests/reports/results.json` を解析して以下を生成する:

```markdown
## 🧪 テスト実行結果（YYYY-MM-DD HH:MM）

### サマリー
| 項目 | 値 |
|------|-----|
| 総テスト数 | 8 |
| ✅ 通過 | 6 |
| ❌ 失敗 | 1 |
| ⚠️ スキップ | 1 |
| 実行時間 | 12.4s |
| 通過率 | 75% |

### 仕様書別カバレッジ
| Spec ID | 受入基準数 | テスト数 | 通過 | ステータス |
|---------|----------|---------|------|---------|
| SPEC-AUTH-001 | 3 | 3 | 2/3 | ⚠️ 要修正 |
| SPEC-DASH-001 | 2 | 2 | 2/2 | ✅ 全通過 |

### ❌ 失敗したテスト

#### spec-auth-001-login.spec.ts > 5回連続失敗でアカウントがロックされる
- **対応する受入基準**: AC-3（SPEC-AUTH-001）
- **エラー**: TimeoutError: Locator `[data-testid="lockout-message"]` が見つかりません
- **スクリーンショット**: tests/reports/screenshots/auth-lockout-failed.png
- **推定原因**: `data-testid="lockout-message"` が未実装 または セレクターが異なる
- **推奨アクション**: 
  1. ロックアウトメッセージコンポーネントに `data-testid="lockout-message"` を追加
  2. または `/test-write` でセレクターを修正

### ⚠️ スキップされたテスト
- spec-notif-001-push.spec.ts — `test.skip` マーク（未実装機能）
```

### STEP 5: 対話型の失敗分析（失敗がある場合）

失敗テストごとに以下を確認する:

```
❌ テスト失敗: 「5回連続失敗でアカウントがロックされる」

エラー詳細:
  TimeoutError: locator('[data-testid="lockout-message"]') の待機がタイムアウトしました

スクリーンショットを確認しましたか？ (yes / no)
> yes

原因の候補:
  A) data-testid="lockout-message" が実装されていない
  B) セレクターが実際の実装と異なる
  C) ロックアウト機能自体が未実装
  D) テスト環境の問題（タイムアウト値・速度）

どれに当てはまりますか？ (A / B / C / D / 詳細を説明)
> A

→ 対処方法:
  実装ファイルでロックアウトメッセージに data-testid を追加してください:
  <div data-testid="lockout-message">...メッセージ...</div>
  
  修正後に再実行: /test-run
```

### STEP 6: テスト結果の保存

```
テスト結果を保存しますか？ (yes / no)
> yes
```

`docs/guides/test-report-YYYY-MM-DD.md` に保存する（`/test-report` と同形式）。

### STEP 7: 次のアクション判定

**全テスト通過の場合:**
```
✅ 全テスト通過！

次のステップ:
  /impl-check → 実装が仕様書を完全に満たしているか確認
  /task-done  → PR を作成してレビュー依頼
```

**失敗がある場合:**
```
⚠️ X 件のテストが失敗しています。

対応方法:
  A) 実装を修正して /test-run を再実行
  B) テストのセレクターを修正して /test-run を再実行
  C) 仕様書の受入基準が間違っている → /spec-change で仕様を更新

どれを行いますか？
```

---

## Playwright 設定テンプレート

`playwright.config.ts` が存在しない場合に生成する:

```typescript
import { defineConfig, devices } from '@playwright/test';

export default defineConfig({
  testDir: './tests/e2e',
  outputDir: './tests/reports/artifacts',
  fullyParallel: false,
  retries: process.env.CI ? 2 : 0,
  workers: process.env.CI ? 1 : undefined,
  reporter: [
    ['html',  { outputFolder: 'tests/reports/html', open: 'never' }],
    ['json',  { outputFile: 'tests/reports/results.json' }],
    ['list']
  ],
  use: {
    baseURL: process.env.BASE_URL || 'http://localhost:3000',
    trace: 'on-first-retry',
    screenshot: 'only-on-failure',
    video: 'on-first-retry',
    actionTimeout: 10000,
    navigationTimeout: 30000,
  },
  projects: [
    { name: 'chromium', use: { ...devices['Desktop Chrome'] } },
    // { name: 'firefox', use: { ...devices['Desktop Firefox'] } }, // 必要に応じて
  ],
  webServer: {
    // 開発サーバー自動起動（環境に合わせて変更）
    // command: 'npm run dev',
    // url: 'http://localhost:3000',
    // reuseExistingServer: true,
  },
});
```

---

## CI/CD 連携

GitHub Actions での自動実行例:

```yaml
# .github/workflows/e2e-test.yml
name: E2E Tests
on:
  pull_request:
    paths:
      - 'src/**'
      - 'tests/e2e/**'

jobs:
  test:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      - uses: actions/setup-node@v4
        with: { node-version: '20' }
      - run: npm ci
      - run: npx playwright install --with-deps chromium
      - run: npx playwright test --reporter=json,html
      - uses: actions/upload-artifact@v4
        if: always()
        with:
          name: playwright-report
          path: tests/reports/
```

---

## 関連スキル

```
/test-write  → テストコードの生成（先に実行）
/test-report → テスト結果の詳細レポート生成
/impl-check  → テスト通過後の最終実装確認
/spec-change → テスト失敗が仕様の問題だった場合
```
