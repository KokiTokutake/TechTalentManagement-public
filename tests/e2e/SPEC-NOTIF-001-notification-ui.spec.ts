import { test, expect } from '@playwright/test';

// SPEC-NOTIF-001: 通知・アラートシステム — UI E2E テスト（TASK-087）
// 仕様書: docs/spec/SPEC-NOTIF-001-notification-system.md
// 設計書: docs/design/SPEC-NOTIF-001-design.md
// 生成日: 2026-05-28（/test-write により自動生成）

const API = 'http://localhost:5108/api';

let talentId = '';

// ── セットアップ: タレント ID を取得 ──────────────────────────────────────
test.beforeAll(async ({ request }) => {
  const res = await request.get(`${API}/users`);
  const users: Array<{ id: string; displayName: string; role: string }> = await res.json();
  const tanaka = users.find(u => u.displayName === '田中 太郎');
  if (!tanaka) throw new Error('シードデータに田中 太郎 が見つかりません');
  talentId = tanaka.id;
});

// ヘルパー: テスト用通知を API で作成する（内部サービス経由で直接 DB 挿入は不可なので
// SelfEvaluation Submit などのトリガーを利用する代わりに、このテストでは
// unread-count と list API のモック応答を Playwright route でインターセプトする）

// ── AC-01-01: 未読バッジの表示 ───────────────────────────────────────────

test('AC-01-01: 未読通知が3件あるとベルに「3」バッジが表示される', async ({ page }) => {
  // Given: API を intercept して未読3件を返す
  await page.route(`${API}/notifications/unread-count*`, async route => {
    await route.fulfill({ json: { count: 3 } });
  });

  // When
  await page.goto(`/?userId=${talentId}`);
  await page.evaluate((id) => {
    localStorage.setItem('currentUser', JSON.stringify({ id, displayName: '田中 太郎', role: 'Talent' }));
  }, talentId);
  await page.goto(`/?userId=${talentId}`);

  // Then
  const badge = page.locator('[data-testid="notification-unread-badge"]');
  await expect(badge).toBeVisible();
  await expect(badge).toContainText('3');
});

// ── AC-01-02: 未読がない場合はバッジを非表示 ────────────────────────────

test('AC-01-02: 未読通知が0件のときバッジが表示されない', async ({ page }) => {
  // Given: API を intercept して未読0件を返す
  await page.route(`${API}/notifications/unread-count*`, async route => {
    await route.fulfill({ json: { count: 0 } });
  });

  await page.goto(`/?userId=${talentId}`);
  await page.evaluate((id) => {
    localStorage.setItem('currentUser', JSON.stringify({ id, displayName: '田中 太郎', role: 'Talent' }));
  }, talentId);
  await page.goto(`/?userId=${talentId}`);

  // Then: バッジが存在しないか非表示
  const badge = page.locator('[data-testid="notification-unread-badge"]');
  await expect(badge).toBeHidden();
});

// ── AC-02-01: 通知ドロップダウンの表示 ──────────────────────────────────

test('AC-02-01: ベルをクリックするとドロップダウンに通知一覧が表示される', async ({ page }) => {
  const mockNotifications = [
    {
      id: 'notif-001',
      type: 'EvaluationApproved',
      title: '上長評価が確定されました',
      body: 'あなたの自己評価に対する上長評価が確定されました。',
      linkUrl: null,
      isRead: false,
      readAt: null,
      createdAt: new Date(Date.now() - 3600000).toISOString(),
    },
    {
      id: 'notif-002',
      type: 'SelfEvaluationSubmitted',
      title: '自己評価が提出されました',
      body: '田中 太郎 が自己評価を提出しました。',
      linkUrl: null,
      isRead: true,
      readAt: new Date(Date.now() - 7200000).toISOString(),
      createdAt: new Date(Date.now() - 7200000).toISOString(),
    },
  ];

  await page.route(`${API}/notifications/unread-count*`, async route => {
    await route.fulfill({ json: { count: 1 } });
  });
  await page.route(`${API}/notifications?*`, async route => {
    await route.fulfill({ json: mockNotifications });
  });

  await page.goto(`/?userId=${talentId}`);
  await page.evaluate((id) => {
    localStorage.setItem('currentUser', JSON.stringify({ id, displayName: '田中 太郎', role: 'Talent' }));
  }, talentId);
  await page.goto(`/?userId=${talentId}`);

  // When: ベルをクリック
  await page.click('[data-testid="notification-bell"]');

  // Then: ドロップダウンが表示される
  const dropdown = page.locator('[data-testid="notification-dropdown"]');
  await expect(dropdown).toBeVisible();

  // 未読通知が表示される
  const unreadItem = page.locator('[data-testid="notification-item-notif-001"]');
  await expect(unreadItem).toBeVisible();
  await expect(unreadItem).toContainText('上長評価が確定されました');

  // 既読通知も表示される
  const readItem = page.locator('[data-testid="notification-item-notif-002"]');
  await expect(readItem).toBeVisible();
});

// ── AC-03-01: 通知クリックで既読化 ──────────────────────────────────────

test('AC-03-01: 未読通知をクリックすると既読になりバッジ数が減る', async ({ page }) => {
  const mockNotification = {
    id: 'notif-read-test',
    type: 'EvaluationApproved',
    title: '上長評価が確定されました',
    body: 'あなたの評価が確定されました。',
    linkUrl: null,
    isRead: false,
    readAt: null,
    createdAt: new Date().toISOString(),
  };

  await page.route(`${API}/notifications/unread-count*`, async route => {
    await route.fulfill({ json: { count: 1 } });
  });
  await page.route(`${API}/notifications?*`, async route => {
    await route.fulfill({ json: [mockNotification] });
  });
  await page.route(`${API}/notifications/notif-read-test/read`, async route => {
    await route.fulfill({
      status: 200,
      json: { id: 'notif-read-test', isRead: true, readAt: new Date().toISOString() },
    });
  });

  await page.goto(`/?userId=${talentId}`);
  await page.evaluate((id) => {
    localStorage.setItem('currentUser', JSON.stringify({ id, displayName: '田中 太郎', role: 'Talent' }));
  }, talentId);
  await page.goto(`/?userId=${talentId}`);

  // バッジが表示されている
  await expect(page.locator('[data-testid="notification-unread-badge"]')).toBeVisible();

  // ベルをクリックしてドロップダウンを開く
  await page.click('[data-testid="notification-bell"]');
  await expect(page.locator('[data-testid="notification-dropdown"]')).toBeVisible();

  // When: 未読通知をクリック
  await page.click('[data-testid="notification-item-notif-read-test"]');

  // Then: バッジが消える（ストアの unreadCount が 0 になる）
  await expect(page.locator('[data-testid="notification-unread-badge"]')).toBeHidden();
});

// ── AC-03-02: 一括既読（ドロップダウン） ────────────────────────────────

test('AC-03-02: 「すべて既読」ボタンをクリックするとバッジが消える', async ({ page }) => {
  const mockNotifications = [
    {
      id: 'bulk-notif-001',
      type: 'EvaluationApproved',
      title: '通知1',
      body: '本文1',
      linkUrl: null,
      isRead: false,
      readAt: null,
      createdAt: new Date().toISOString(),
    },
    {
      id: 'bulk-notif-002',
      type: 'SelfEvaluationSubmitted',
      title: '通知2',
      body: '本文2',
      linkUrl: null,
      isRead: false,
      readAt: null,
      createdAt: new Date(Date.now() - 1000).toISOString(),
    },
  ];

  await page.route(`${API}/notifications/unread-count*`, async route => {
    await route.fulfill({ json: { count: 2 } });
  });
  await page.route(`${API}/notifications?*`, async route => {
    await route.fulfill({ json: mockNotifications });
  });
  await page.route(`${API}/notifications/read-all*`, async route => {
    await route.fulfill({ status: 200, json: { updatedCount: 2 } });
  });

  await page.goto(`/?userId=${talentId}`);
  await page.evaluate((id) => {
    localStorage.setItem('currentUser', JSON.stringify({ id, displayName: '田中 太郎', role: 'Talent' }));
  }, talentId);
  await page.goto(`/?userId=${talentId}`);

  // バッジ「2」が表示
  await expect(page.locator('[data-testid="notification-unread-badge"]')).toHaveText('2');

  // ベルを開く
  await page.click('[data-testid="notification-bell"]');
  await expect(page.locator('[data-testid="notification-read-all-button"]')).toBeVisible();

  // When: 一括既読をクリック
  await page.click('[data-testid="notification-read-all-button"]');

  // Then: バッジが非表示
  await expect(page.locator('[data-testid="notification-unread-badge"]')).toBeHidden();
});

// ── 通知一覧ページ ──────────────────────────────────────────────────────

test('通知一覧ページ: /notifications に遷移するとリストが表示される', async ({ page }) => {
  const mockNotifications = [
    {
      id: 'page-notif-001',
      type: 'EvaluationApproved',
      title: '一覧ページテスト通知',
      body: '通知一覧ページの表示確認用',
      linkUrl: null,
      isRead: false,
      readAt: null,
      createdAt: new Date().toISOString(),
    },
  ];

  await page.route(`${API}/notifications/unread-count*`, async route => {
    await route.fulfill({ json: { count: 1 } });
  });
  await page.route(`${API}/notifications?*`, async route => {
    await route.fulfill({ json: mockNotifications });
  });

  await page.goto(`/notifications?userId=${talentId}`);
  await page.evaluate((id) => {
    localStorage.setItem('currentUser', JSON.stringify({ id, displayName: '田中 太郎', role: 'Talent' }));
  }, talentId);
  await page.goto(`/notifications`);

  // Then: 通知一覧コンテナが表示される
  const list = page.locator('[data-testid="notification-list"]');
  await expect(list).toBeVisible();

  // 通知アイテムが表示される
  await expect(page.locator('[data-testid="notification-item-page-notif-001"]')).toBeVisible();
  await expect(page.locator('[data-testid="notification-item-page-notif-001"]')).toContainText('一覧ページテスト通知');
});

test('通知一覧ページ: 「すべて既読にする」ボタンで全既読になる', async ({ page }) => {
  const mockNotifications = [
    {
      id: 'all-read-001',
      type: 'EvaluationApproved',
      title: '一括既読テスト',
      body: '本文',
      linkUrl: null,
      isRead: false,
      readAt: null,
      createdAt: new Date().toISOString(),
    },
  ];

  await page.route(`${API}/notifications/unread-count*`, async route => {
    await route.fulfill({ json: { count: 1 } });
  });
  await page.route(`${API}/notifications?*`, async route => {
    await route.fulfill({ json: mockNotifications });
  });
  await page.route(`${API}/notifications/read-all*`, async route => {
    await route.fulfill({ status: 200, json: { updatedCount: 1 } });
  });

  await page.evaluate((id) => {
    localStorage.setItem('currentUser', JSON.stringify({ id, displayName: '田中 太郎', role: 'Talent' }));
  }, talentId);
  await page.goto(`/notifications`);

  // When: 一括既読ボタンをクリック
  const readAllBtn = page.locator('[data-testid="notification-read-all-button"]');
  await expect(readAllBtn).toBeVisible();
  await readAllBtn.click();

  // Then: バッジが消える
  await expect(page.locator('[data-testid="notification-unread-badge"]')).toBeHidden();
});
