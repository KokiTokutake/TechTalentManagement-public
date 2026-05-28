# /test-write — 仕様書の受入基準から Playwright テストを生成

**使う場面**: 工程5（実装）完了後、PR 作成前。仕様書の受入基準を Playwright E2E テストに自動変換する。
「仕様書 → テスト → 実装」の Spec-as-Test を実現するコアスキル。

---

## 目的

仕様書（docs/spec/）の受入基準（Given/When/Then 形式）を読み取り、
Playwright のテストコードを自動生成する。
テストは「仕様書が要求する振る舞い」を機械的に検証するドキュメントとして機能する。

---

## 実行手順

### STEP 1: 対象仕様書の特定

```
どの仕様書のテストを生成しますか？

1. Spec ID を入力（例: SPEC-AUTH-001）
2. 「全部」と入力 → approved ステータスの全仕様書
3. 「現在のタスク」と入力 → 実装中のタスクに紐付く仕様書

選択してください:
```

### STEP 2: 仕様書と設計書の読み込み

以下を確認する:
- `docs/spec/[対象仕様書].md` — 受入基準・前提条件・要件
- `docs/design/` — コンポーネント設計（テストのセレクター情報を取得）
- `CLAUDE.md` — テスト設定（フレームワーク・ベース URL など）
- `playwright.config.ts` または `playwright.config.js`（存在する場合）

### STEP 3: テスト構造の分析

受入基準を解析して以下を判断する:

**テストタイプの分類:**
```
UI 操作を含む → Playwright E2E テスト
API のみ     → Playwright API テスト（request フィクスチャ）
複合         → 両方生成
```

**受入基準のマッピング:**
```
Given（前提条件）→ test.beforeEach / page.goto() / ログイン処理
When（操作）    → page.click() / page.fill() / page.waitFor()
Then（期待結果） → expect(page.locator(...)).toBeVisible() など
```

### STEP 4: テストファイルの生成

**出力先:** `tests/e2e/[spec-id]-[feature-name].spec.ts`

**生成フォーマット:**
```typescript
import { test, expect } from '@playwright/test';

// SPEC-AUTH-001: ログイン機能
// 仕様書: docs/spec/auth/spec-auth-001-login.md
// 生成日: YYYY-MM-DD（/test-write により自動生成）

test.describe('SPEC-AUTH-001: ログイン機能', () => {

  // AC-1: 正常ログイン
  // Given 正しいメールとパスワード When ログインボタン押下 Then ダッシュボードへ遷移する
  test('正しい認証情報でログインするとダッシュボードへ遷移する', async ({ page }) => {
    // Given
    await page.goto('/login');
    
    // When
    await page.fill('[data-testid="email"]', 'test@example.com');
    await page.fill('[data-testid="password"]', 'password123');
    await page.click('[data-testid="login-button"]');
    
    // Then
    await expect(page).toHaveURL('/dashboard');
    await expect(page.locator('[data-testid="welcome-message"]')).toBeVisible();
  });

  // AC-2: 誤ったパスワード
  // Given 誤ったパスワード When ログインボタン押下 Then エラーメッセージを表示する
  test('誤った認証情報でエラーメッセージが表示される', async ({ page }) => {
    // Given
    await page.goto('/login');
    
    // When
    await page.fill('[data-testid="email"]', 'test@example.com');
    await page.fill('[data-testid="password"]', 'wrong-password');
    await page.click('[data-testid="login-button"]');
    
    // Then
    await expect(page.locator('[data-testid="error-message"]')).toBeVisible();
    await expect(page.locator('[data-testid="error-message"]')).toContainText('メールアドレスまたはパスワードが正しくありません');
    await expect(page).toHaveURL('/login'); // リダイレクトしない
  });

  // AC-3: アカウントロック
  // Given 5回連続失敗 When ログイン試行 Then 30分アカウントロックされる
  test('5回連続失敗でアカウントがロックされる', async ({ page }) => {
    // Given / When
    await page.goto('/login');
    for (let i = 0; i < 5; i++) {
      await page.fill('[data-testid="email"]', 'test@example.com');
      await page.fill('[data-testid="password"]', 'wrong-password');
      await page.click('[data-testid="login-button"]');
    }
    
    // Then
    await expect(page.locator('[data-testid="lockout-message"]')).toBeVisible();
    await expect(page.locator('[data-testid="lockout-message"]')).toContainText('30分');
  });

});
```

### STEP 5: セレクター注記の追加

セレクターが不確かな箇所には TODO コメントを付加する:

```typescript
// TODO: 実際の data-testid 属性を確認してください
// 推奨: コンポーネントに data-testid="login-button" を追加
await page.click('[data-testid="login-button"]');
```

**data-testid がない場合の代替セレクター（優先順）:**
1. `data-testid="..."` — 最優先（テスト専用属性）
2. `role="button", { name: "ログイン" }` — アクセシビリティロール
3. `text("ログイン")` — テキスト
4. CSS クラス — 最終手段（変更リスクが高い）

### STEP 5.5: テストタイプ別のベース URL 設定

テストを生成する前に、テストタイプに応じた URL を明示する:

| テストタイプ | baseURL | 使用変数 |
|------------|---------|---------|
| UI テスト（page.goto 等） | `http://localhost:3000`（フロントエンド） | `const UI_BASE = 'http://localhost:3000'` |
| API テスト（request.get 等） | `http://localhost:5108`（バックエンド） | `const API_BASE = 'http://localhost:5108/api'` |

**生成するテストコードの先頭に必ず明示する:**

```typescript
// URL 設定（CLAUDE.md の設定に従う）
const UI_BASE = process.env.PLAYWRIGHT_UI_BASE ?? 'http://localhost:3000';
const API_BASE = process.env.PLAYWRIGHT_API_BASE ?? 'http://localhost:5108';
```

UI テストで `page.goto('/path')` を使う場合は `playwright.config.ts` の `baseURL` を **フロントエンドポート（3000）** に設定し、API テストは URL を明示的に組み立てること。

### STEP 6: playwright.config の確認・生成

`playwright.config.ts` が存在しない場合は生成を提案する:

```typescript
// playwright.config.ts（提案）
import { defineConfig } from '@playwright/test';

// フロントエンド（UI テスト）とバックエンド（API テスト）の URL を分離
// UI テスト: page.goto('/path') → baseURL（フロントエンド）を使用
// API テスト: 各テストファイルで API_BASE を直接指定（playwright.config の baseURL は使わない）
export default defineConfig({
  testDir: './tests/e2e',
  fullyParallel: false,
  retries: 1,
  reporter: [
    ['html', { outputFolder: 'tests/reports/html', open: 'never' }],
    ['json', { outputFile: 'tests/reports/results.json' }],
    ['list'],
  ],
  use: {
    baseURL: process.env.PLAYWRIGHT_UI_BASE ?? 'http://localhost:3000', // フロントエンド
    trace: 'on-first-retry',
    screenshot: 'only-on-failure',
    video: 'on-first-retry',
    actionTimeout: 10000,
    navigationTimeout: 30000,
  },
});
```

> ⚠️ `baseURL` を バックエンドポート（例: 5108）に設定してはならない。
> UI テストの `page.goto()` はフロントエンドを対象とする。API テストは各ファイルで URL を直接指定する。

### STEP 7: 生成結果の確認

```markdown
## ✅ テストファイル生成完了

| ファイル | テスト数 | カバーする受入基準 |
|---------|---------|-----------------|
| tests/e2e/spec-auth-001-login.spec.ts | 3 | AC-1, AC-2, AC-3 |

### ⚠️ 確認が必要なセレクター
- `[data-testid="login-button"]` — 実装確認が必要（3箇所）
- `[data-testid="lockout-message"]` — 実装確認が必要（1箇所）

### 次のステップ
1. セレクターの TODO コメントを確認・修正
2. /test-run でテストを実行
3. 全テスト通過後に /impl-check → /task-done
```

---

## data-testid 追加の指針

テストを安定させるため、実装時に以下を推奨する:

```html
<!-- NG: クラスやタグのみ -->
<button class="btn-primary">ログイン</button>

<!-- OK: data-testid を追加 -->
<button class="btn-primary" data-testid="login-button">ログイン</button>
```

Constitution.md の「コーディングルール」に以下を追記することを推奨:
```
- インタラクティブ要素には data-testid 属性を付与する
- data-testid の命名: [component]-[action] 形式（例: login-button, user-email-input）
```

---

## 関連スキル

```
/spec-write  → 受入基準の作成（テスト生成の元データ）
/test-run    → 生成したテストの実行
/impl-check  → テスト結果を含めた実装確認
/task-done   → テスト通過後の PR 作成
```
