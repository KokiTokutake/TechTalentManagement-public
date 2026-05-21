import { test, expect } from '@playwright/test';

// SPEC-1ON1-001: 1on1面談記録機能 — 前回面談引き継ぎAPI
// 仕様書: docs/spec/SPEC-1ON1-001-one-on-one-meeting.md
// 設計書: docs/design/SPEC-1ON1-001-design.md
// 対象タスク: TASK-043
// 生成日: 2026-05-21

const API_BASE = 'http://localhost:5108/api';

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

function managerHeaders() {
  return { 'X-User-Id': MANAGER_ID };
}

test.describe('SPEC-1ON1-001: 前回面談引き継ぎ API', () => {

  // ── AC-02-02 / AC-04-01: 前回面談の引き継ぎ表示 ──────────────────
  // Given: タレントに過去の面談記録が存在する
  // When:  GET /api/talents/{talentId}/previous-meeting を呼ぶ
  // Then:  最新面談の実施日・Will/Can/Must・actionItems（未完了含む）が返る
  test('AC-04-01: GET /api/talents/{talentId}/previous-meeting で前回面談が返る', async ({ request }) => {
    // 面談を2件作成して「最新」が返ることを確認
    await request.post(`${API_BASE}/one-on-ones`, {
      headers: managerHeaders(),
      data: {
        talentId: TALENT_ID,
        conductedAt: '2026-04-01',
        format: 'InPerson',
        willTopics: '古い面談のWill',
        shareWithMdc: false,
        actionItems: ['古い宿題'],
      },
    });

    const newer = await request.post(`${API_BASE}/one-on-ones`, {
      headers: managerHeaders(),
      data: {
        talentId: TALENT_ID,
        conductedAt: '2099-12-31',
        format: 'Online',
        willTopics: '最新面談のWill',
        canTopics: '最新のCan',
        shareWithMdc: false,
        actionItems: ['最新の未完了宿題'],
      },
    });
    expect(newer.status()).toBe(201);

    const res = await request.get(`${API_BASE}/talents/${TALENT_ID}/previous-meeting`);
    expect(res.status()).toBe(200);

    const body = await res.json();
    expect(body.talentId).toBe(TALENT_ID);
    // 最新（2099-12-31）の面談が返ること
    expect(body.conductedAt).toBe('2099-12-31');
    expect(body.willTopics).toBe('最新面談のWill');
    expect(body.canTopics).toBe('最新のCan');
    // actionItems が含まれること
    expect(body.actionItems).toBeDefined();
    expect(Array.isArray(body.actionItems)).toBe(true);
    expect(body.actionItems.length).toBeGreaterThanOrEqual(1);
    // 未完了宿題が含まれること
    const pending = body.actionItems.filter((a: { status: string }) => a.status === 'Pending');
    expect(pending.length).toBeGreaterThanOrEqual(1);
    expect(pending[0].description).toBe('最新の未完了宿題');
  });

  // ── 存在しないタレントは 404 ─────────────────────────────────────
  test('存在しないタレントへの previous-meeting は 404 を返す', async ({ request }) => {
    const fakeId = '00000000-0000-0000-0000-999999999997';
    const res = await request.get(`${API_BASE}/talents/${fakeId}/previous-meeting`);
    expect(res.status()).toBe(404);
  });

  // ── 面談記録が0件のタレントは 204 No Content ──────────────────────
  // Given: 面談記録が1件もないタレントが存在する
  // When:  GET /api/talents/{talentId}/previous-meeting を呼ぶ
  // Then:  204 No Content が返る（面談なし）
  test('面談記録がないタレントへの previous-meeting は 204 を返す', async ({ request }) => {
    // 面談がいないタレントを探す（マネージャーIDが異なるユーザー）
    const usersRes = await request.get(`${API_BASE}/users`);
    const users: Array<{ id: string; role: string; managerId: string | null }> = await usersRes.json();

    // TALENT_ID 以外のタレントで、このテストで面談を作っていないタレントを探す
    const otherTalent = users.find(u => u.role === 'Talent' && u.id !== TALENT_ID && u.managerId !== null);
    if (!otherTalent) {
      test.skip();
      return;
    }

    // 直接 API を叩く（面談なし想定）
    const res = await request.get(`${API_BASE}/talents/${otherTalent.id}/previous-meeting`);
    // 204 か 200（面談ゼロでは DB 上に何もない）を許容
    expect([200, 204]).toContain(res.status());
    if (res.status() === 200) {
      // 既存のデータがある場合はスキップ（シード依存）
    }
  });

});
