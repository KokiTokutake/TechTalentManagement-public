import { test, expect } from '@playwright/test';

// SPEC-1ON1-001: 1on1面談記録機能 — CRUD API
// 仕様書: docs/spec/SPEC-1ON1-001-one-on-one-meeting.md
// 設計書: docs/design/SPEC-1ON1-001-design.md
// 対象タスク: TASK-042
// 生成日: 2026-05-21

const API_BASE = 'http://localhost:5108/api';

// テスト用に実際のシードデータから取得したID（beforeAll で設定）
let TALENT_ID = '';
let MANAGER_ID = '';

test.beforeAll(async ({ request }) => {
  const res = await request.get(`${API_BASE}/users`);
  const users: Array<{ id: string; role: string; managerId: string | null }> = await res.json();

  const talent = users.find(u => u.role === 'Talent' && u.managerId !== null);
  const manager = users.find(u => u.id === talent?.managerId);

  if (!talent || !manager) throw new Error('シードデータにタレント/マネージャーが見つかりません');

  TALENT_ID = talent.id;
  MANAGER_ID = manager.id;
});

// 上長として POST するヘルパー
function managerHeaders() {
  return { 'X-User-Id': MANAGER_ID };
}

test.describe('SPEC-1ON1-001: 1on1記録 CRUD API', () => {

  // ── AC-01-01: 基本情報の入力と保存 ───────────────────────────────
  // Given: 上長がAPIを呼び出せる状態にある
  // When:  POST /api/one-on-ones に正しいリクエストを送る
  // Then:  201 Created と作成された面談記録が返る
  test('AC-01-01: POST /api/one-on-ones で面談記録を作成できる', async ({ request }) => {
    const res = await request.post(`${API_BASE}/one-on-ones`, {
      headers: managerHeaders(),
      data: {
        talentId: TALENT_ID,
        conductedAt: '2026-05-21',
        format: 'Online',
        willTopics: 'テックリードへのキャリアを話し合った',
        canTopics: 'TypeScriptの学習進捗を確認',
        mustTopics: 'Q3設計書完成に向けた計画',
        currentSituation: '体調良好。プロジェクト負荷は適度。',
        freeText: '次回はコードレビューの改善について話す',
        shareWithMdc: false,
        actionItems: ['TypeScript入門書を読み終える', 'PR作成ガイドラインを確認する'],
      },
    });

    expect(res.status()).toBe(201);

    const body = await res.json();
    expect(body.id).toBeTruthy();
    expect(body.talentId).toBe(TALENT_ID);
    expect(body.conductedAt).toBe('2026-05-21');
    expect(body.format).toBe('Online');
    expect(body.willTopics).toBe('テックリードへのキャリアを話し合った');
    expect(body.shareWithMdc).toBe(false);
    expect(body.actionItems).toHaveLength(2);
    expect(body.actionItems[0].status).toBe('Pending');
    expect(body.actionItems[0].completedAt).toBeNull();
  });

  // ── AC-01-02: 必須項目バリデーション ─────────────────────────────
  // Given: リクエストボディから conductedAt が欠落している
  // When:  POST /api/one-on-ones を送る
  // Then:  400 Bad Request が返る
  test('AC-01-02: conductedAt が未入力の場合 400 が返る', async ({ request }) => {
    const res = await request.post(`${API_BASE}/one-on-ones`, {
      headers: managerHeaders(),
      data: {
        talentId: TALENT_ID,
        format: 'InPerson',
        shareWithMdc: false,
        // conductedAt が欠落 → デフォルト値 0001-01-01 でバリデーションエラー
      },
    });

    expect(res.status()).toBe(400);
  });

  // ── AC-03-01: 一覧表示 ────────────────────────────────────────────
  // Given: talentId を持つ面談記録が複数存在する
  // When:  GET /api/one-on-ones?talentId=... を送る
  // Then:  200 OK と新しい順に並んだ一覧が返る
  test('AC-03-01: GET /api/one-on-ones で一覧が新しい順に返る', async ({ request }) => {
    await request.post(`${API_BASE}/one-on-ones`, {
      headers: managerHeaders(),
      data: { talentId: TALENT_ID, conductedAt: '2026-04-01', format: 'InPerson', shareWithMdc: false },
    });
    await request.post(`${API_BASE}/one-on-ones`, {
      headers: managerHeaders(),
      data: { talentId: TALENT_ID, conductedAt: '2026-05-01', format: 'Online', shareWithMdc: false },
    });

    const res = await request.get(`${API_BASE}/one-on-ones?talentId=${TALENT_ID}`);
    expect(res.status()).toBe(200);

    const body = await res.json();
    expect(body.items).toBeDefined();
    expect(body.totalCount).toBeGreaterThanOrEqual(2);
    expect(body.page).toBe(1);

    // 新しい順（ConductedAt DESC）を確認
    const dates: string[] = body.items.map((m: { conductedAt: string }) => m.conductedAt);
    const sorted = [...dates].sort((a, b) => b.localeCompare(a));
    expect(dates).toEqual(sorted);
  });

  // ── AC-03-01: ページネーション ────────────────────────────────────
  test('AC-03-01: GET /api/one-on-ones はページネーションに対応している', async ({ request }) => {
    const res = await request.get(`${API_BASE}/one-on-ones?talentId=${TALENT_ID}&page=1&limit=1`);
    expect(res.status()).toBe(200);

    const body = await res.json();
    expect(body.items.length).toBeLessThanOrEqual(1);
    expect(body.limit).toBe(1);
  });

  // ── GET 詳細 ─────────────────────────────────────────────────────
  test('GET /api/one-on-ones/{id} で面談記録の詳細を取得できる', async ({ request }) => {
    const created = await request.post(`${API_BASE}/one-on-ones`, {
      headers: managerHeaders(),
      data: {
        talentId: TALENT_ID,
        conductedAt: '2026-05-10',
        format: 'Hybrid',
        willTopics: '詳細テスト用Will',
        shareWithMdc: false,
      },
    });
    const { id } = await created.json();

    const res = await request.get(`${API_BASE}/one-on-ones/${id}`);
    expect(res.status()).toBe(200);

    const body = await res.json();
    expect(body.id).toBe(id);
    expect(body.format).toBe('Hybrid');
    expect(body.willTopics).toBe('詳細テスト用Will');
    expect(body.actionItems).toBeDefined();
  });

  // ── PUT 更新 ─────────────────────────────────────────────────────
  test('PUT /api/one-on-ones/{id} で面談記録を更新できる', async ({ request }) => {
    const created = await request.post(`${API_BASE}/one-on-ones`, {
      headers: managerHeaders(),
      data: { talentId: TALENT_ID, conductedAt: '2026-05-12', format: 'Online', shareWithMdc: false },
    });
    const { id } = await created.json();

    const res = await request.put(`${API_BASE}/one-on-ones/${id}`, {
      headers: managerHeaders(),
      data: {
        conductedAt: '2026-05-13',
        format: 'InPerson',
        willTopics: '更新後のWillトピック',
        shareWithMdc: true,
      },
    });
    expect(res.status()).toBe(200);

    const body = await res.json();
    expect(body.conductedAt).toBe('2026-05-13');
    expect(body.format).toBe('InPerson');
    expect(body.willTopics).toBe('更新後のWillトピック');
    expect(body.shareWithMdc).toBe(true);
  });

  // ── DELETE 論理削除 ───────────────────────────────────────────────
  test('DELETE /api/one-on-ones/{id} で論理削除される', async ({ request }) => {
    const created = await request.post(`${API_BASE}/one-on-ones`, {
      headers: managerHeaders(),
      data: { talentId: TALENT_ID, conductedAt: '2026-05-14', format: 'Online', shareWithMdc: false },
    });
    const { id } = await created.json();

    const delRes = await request.delete(`${API_BASE}/one-on-ones/${id}`, {
      headers: managerHeaders(),
    });
    expect(delRes.status()).toBe(204);

    // 論理削除後は取得できない（HasQueryFilter で除外される）
    const getRes = await request.get(`${API_BASE}/one-on-ones/${id}`);
    expect(getRes.status()).toBe(404);
  });

  // ── AC-02-01: 宿題の登録 ─────────────────────────────────────────
  // Given: 面談記録が存在する
  // When:  POST /api/one-on-ones/{id}/action-items で宿題を追加する
  // Then:  201 Created と Pending ステータスの宿題が返る
  test('AC-02-01: POST /api/one-on-ones/{id}/action-items で宿題を追加できる', async ({ request }) => {
    const created = await request.post(`${API_BASE}/one-on-ones`, {
      headers: managerHeaders(),
      data: { talentId: TALENT_ID, conductedAt: '2026-05-15', format: 'Online', shareWithMdc: false },
    });
    const { id } = await created.json();

    const res = await request.post(`${API_BASE}/one-on-ones/${id}/action-items`, {
      headers: managerHeaders(),
      data: { description: '概念検証を実施する' },
    });
    expect(res.status()).toBe(201);

    const item = await res.json();
    expect(item.description).toBe('概念検証を実施する');
    expect(item.status).toBe('Pending');
    expect(item.completedAt).toBeNull();
  });

  // ── AC-02-03: 宿題のステータス更新 ───────────────────────────────
  // Given: 未完了の宿題が存在する
  // When:  PATCH /api/action-items/{itemId}/status で Done にする
  // Then:  200 OK、status が Done、completedAt が設定される
  test('AC-02-03: PATCH /api/action-items/{itemId}/status で宿題を完了にできる', async ({ request }) => {
    const created = await request.post(`${API_BASE}/one-on-ones`, {
      headers: managerHeaders(),
      data: {
        talentId: TALENT_ID,
        conductedAt: '2026-05-16',
        format: 'Online',
        shareWithMdc: false,
        actionItems: ['完了テスト用宿題'],
      },
    });
    const meeting = await created.json();
    const itemId = meeting.actionItems[0].id;

    const res = await request.patch(`${API_BASE}/action-items/${itemId}/status`, {
      headers: managerHeaders(),
      data: { status: 'Done' },
    });
    expect(res.status()).toBe(200);

    const item = await res.json();
    expect(item.status).toBe('Done');
    expect(item.completedAt).not.toBeNull();

    // Pending に戻せることも確認
    const reopenRes = await request.patch(`${API_BASE}/action-items/${itemId}/status`, {
      headers: managerHeaders(),
      data: { status: 'Pending' },
    });
    const reopened = await reopenRes.json();
    expect(reopened.status).toBe('Pending');
    expect(reopened.completedAt).toBeNull();
  });

  // ── AC-06-01: 委員会用サマリー ────────────────────────────────────
  // Given: shareWithMdc=true の面談記録が存在する
  // When:  GET /api/talents/{talentId}/one-on-one-summary を送る
  // Then:  200 OK と sharedMeetings に共有フラグ付き記録のみが含まれる
  test('AC-06-01: GET /api/talents/{talentId}/one-on-one-summary で共有記録サマリーを取得できる', async ({ request }) => {
    await request.post(`${API_BASE}/one-on-ones`, {
      headers: managerHeaders(),
      data: { talentId: TALENT_ID, conductedAt: '2026-05-17', format: 'Online', shareWithMdc: true, willTopics: '委員会共有テスト' },
    });
    await request.post(`${API_BASE}/one-on-ones`, {
      headers: managerHeaders(),
      data: { talentId: TALENT_ID, conductedAt: '2026-05-18', format: 'Online', shareWithMdc: false },
    });

    const res = await request.get(`${API_BASE}/talents/${TALENT_ID}/one-on-one-summary`);
    expect(res.status()).toBe(200);

    const body = await res.json();
    expect(body.talentId).toBe(TALENT_ID);
    expect(body.totalMeetingCount).toBeGreaterThanOrEqual(2);
    expect(body.lastMeetingAt).not.toBeNull();
    expect(body.sharedMeetings.length).toBeGreaterThanOrEqual(1);
  });

  // ── 存在しないリソースへのアクセス ───────────────────────────────
  test('存在しない面談記録の GET は 404 を返す', async ({ request }) => {
    const fakeId = '00000000-0000-0000-0000-999999999999';
    const res = await request.get(`${API_BASE}/one-on-ones/${fakeId}`);
    expect(res.status()).toBe(404);
  });

  test('存在しないタレントへの一覧取得は 404 を返す', async ({ request }) => {
    const fakeId = '00000000-0000-0000-0000-999999999998';
    const res = await request.get(`${API_BASE}/one-on-ones?talentId=${fakeId}`);
    expect(res.status()).toBe(404);
  });

});
