import { defineConfig } from '@playwright/test';

// UI テスト: page.goto('/path') はフロントエンド（3000）を対象とする
// API テスト: 各テストファイルで const API_BASE = 'http://localhost:5108' を直接指定する
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
    baseURL: process.env.PLAYWRIGHT_UI_BASE ?? 'http://localhost:3000',
    trace: 'on-first-retry',
    screenshot: 'only-on-failure',
    actionTimeout: 10000,
    navigationTimeout: 30000,
  },
});
