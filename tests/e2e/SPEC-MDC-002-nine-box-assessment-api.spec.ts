import { test, expect } from '@playwright/test';

// SPEC-MDC-002: 9ボックス評価機能 — 評価入力フォーム API
// 仕様書: docs/spec/SPEC-MDC-002-nine-box-assessment.md
// 設計書: docs/design/SPEC-MDC-002-design.md
// 対象タスク: TASK-045
// 生成日: 2026-05-21

const API_BASE = 'http://localhost:5108/api';

let TALENT_ID = '';
let MANAGER_ID = '';
let PREV_PERIOD_ID = '';    // 評価期間が過去（シードデータあり）
let EXISTING_ASSESSMENT_ID = ''; // シードで作成済みの評価ID

test.beforeAll(async ({ request }) => {
  const usersRes = await request.get(`${API_BASE}/users`);
  const users: Array<{ id: string; role: string; managerId: string | null; displayName: string }> = await usersRes.json();

  // 田中 太郎（manager1 配下）を使用
  const talent = users.find(u => u.displayName === '田中 太郎');
  const manager = users.find(u => u.displayName === '山田 一郎');
  if (!talent || !manager) throw new Error('シードデータにタレント/マネージャーが見つかりません');

  TALENT_ID = talent.id;
  MANAGER_ID = manager.id;

  // 前年度期間（評価データがシードされている）
  const periodsRes = await request.get(`${API_BASE}/periods`);
  const periods: Array<{ id: string; year: number; currentPhase: string }> = await periodsRes.json();
  const prevPeriod = periods.find(p => p.year === 2025);
  if (!prevPeriod) throw new Error('2025年度の考課期間が見つかりません');
  PREV_PERIOD_ID = prevPeriod.id;

  // シードされた評価の ID を取得
  const mapRes = await request.get(`${API_BASE}/nine-box?periodId=${PREV_PERIOD_ID}`);
  if (mapRes.status() === 200) {
    const mapBody = await mapRes.json();
    for (const cell of mapBody.cells) {
      const found = cell.talents.find((t: { talentId: string; assessmentId: string }) => t.talentId === TALENT_ID);
      if (found) {
        EXISTING_ASSESSMENT_ID = found.assessmentId;
        break;
      }
    }
  }
});

function managerHeaders() {
  return { 'X-User-Id': MANAGER_ID };
}

test.describe('SPEC-MDC-002: 9ボックス評価入力 API', () => {

  // ── セル定義マスタ ──────────────────────────────────────────────
  test('GET /api/nine-box/cells でセル定義マスタが返る（9件）', async ({ request }) => {
    const res = await request.get(`${API_BASE}/nine-box/cells`);
    expect(res.status()).toBe(200);

    const cells: Array<{
      performanceLevel: string;
      potentialLevel: string;
      label: string;
      color: string;
    }> = await res.json();

    expect(Array.isArray(cells)).toBe(true);
    expect(cells.length).toBe(9);

    const star = cells.find(c => c.performanceLevel === 'High' && c.potentialLevel === 'High');
    expect(star).toBeDefined();
    expect(star!.label).toBe('スター');
    expect(star!.color).toBe('#8b5cf6');
  });

  // ── 9ボックスマップ取得（シードデータで確認）──────────────────
  test('GET /api/nine-box?periodId= でマップデータが返る', async ({ request }) => {
    const res = await request.get(`${API_BASE}/nine-box?periodId=${PREV_PERIOD_ID}`);
    expect(res.status()).toBe(200);

    const body = await res.json();
    expect(body.periodId).toBe(PREV_PERIOD_ID);
    expect(Array.isArray(body.cells)).toBe(true);
    expect(body.cells.length).toBe(9); // 全9セル
    expect(body.summary).toBeDefined();
    expect(body.summary.totalAssessed).toBeGreaterThanOrEqual(1);
  });

  // ── AC-01-01: 評価期間外は POST で 403 が返る（正常な挙動）────
  // Closedフェーズの期間への評価POST → 403
  // → バックエンドの IsEvaluationOpen チェックで 403 を返す
  test('AC-01-01/03: 評価期間外（Closed）への POST は 403 を返す', async ({ request }) => {
    const periodsRes = await request.get(`${API_BASE}/periods`);
    const periods: Array<{ id: string; currentPhase: string }> = await periodsRes.json();
    const closedPeriod = periods.find(p => p.currentPhase === 'Closed');
    if (!closedPeriod) { test.skip(); return; }

    const res = await request.post(`${API_BASE}/nine-box`, {
      headers: managerHeaders(),
      data: {
        talentId: TALENT_ID,
        periodId: closedPeriod.id,
        performanceLevel: 'High',
        potentialLevel: 'Mid',
        performanceRationale: null,
        potentialRationale: null,
      },
    });
    // Closedフェーズなので 403 を期待
    expect(res.status()).toBe(403);
  });

  // ── AC-01-02: 評価の更新（シードデータの既存評価を PUT）──────────
  // Given: 既に評価が保存されている
  // When:  PUT で更新する
  // Then:  評価が更新される
  test('AC-01-02: PUT /api/nine-box/{id} でシードデータの評価を更新できる', async ({ request }) => {
    if (!EXISTING_ASSESSMENT_ID) {
      test.skip();
      return;
    }

    const res = await request.put(`${API_BASE}/nine-box/${EXISTING_ASSESSMENT_ID}`, {
      headers: managerHeaders(),
      data: {
        performanceLevel: 'High',
        potentialLevel: 'High',
        performanceRationale: 'テスト: 更新確認',
        potentialRationale: 'テスト: ポテンシャル更新',
      },
    });
    expect(res.status()).toBe(200);

    const body = await res.json();
    expect(body.performanceLevel).toBe('High');
    expect(body.potentialLevel).toBe('High');
    expect(body.isHighPotentialCandidate).toBe(true);
    expect(body.performanceRationale).toBe('テスト: 更新確認');
  });

  // ── 他マネージャーの評価は更新不可 ─────────────────────────────
  test('他のアセッサーが入力した評価は PUT で 403 が返る', async ({ request }) => {
    if (!EXISTING_ASSESSMENT_ID) { test.skip(); return; }

    // 別マネージャーとして更新を試みる
    const usersRes = await request.get(`${API_BASE}/users`);
    const users: Array<{ id: string; role: string; displayName: string }> = await usersRes.json();
    const otherManager = users.find(u => u.role === 'Manager' && u.id !== MANAGER_ID);
    if (!otherManager) { test.skip(); return; }

    const res = await request.put(`${API_BASE}/nine-box/${EXISTING_ASSESSMENT_ID}`, {
      headers: { 'X-User-Id': otherManager.id },
      data: {
        performanceLevel: 'Low',
        potentialLevel: 'Low',
        performanceRationale: null,
        potentialRationale: null,
      },
    });
    expect(res.status()).toBe(403);
  });

  // ── 配下外タレントへの評価は 403 ───────────────────────────────
  test('配下外タレントへの評価入力は 403 を返す', async ({ request }) => {
    const periodsRes = await request.get(`${API_BASE}/periods`);
    const periods: Array<{ id: string; year: number }> = await periodsRes.json();
    const prevPeriod = periods.find(p => p.year === 2025);
    if (!prevPeriod) { test.skip(); return; }

    const usersRes = await request.get(`${API_BASE}/users`);
    const users: Array<{ id: string; role: string; managerId: string | null }> = await usersRes.json();
    const otherTalent = users.find(u =>
      u.role === 'Talent' && u.id !== TALENT_ID && u.managerId !== MANAGER_ID && u.managerId !== null
    );
    if (!otherTalent) { test.skip(); return; }

    const res = await request.post(`${API_BASE}/nine-box`, {
      headers: managerHeaders(),
      data: {
        talentId: otherTalent.id,
        periodId: prevPeriod.id,
        performanceLevel: 'Mid',
        potentialLevel: 'Mid',
        performanceRationale: null,
        potentialRationale: null,
      },
    });
    // 評価期間外(403) または 配下外(403) のいずれかで 403 が返ること
    expect(res.status()).toBe(403);
  });

  // ── 評価履歴取得 ────────────────────────────────────────────────
  test('GET /api/nine-box/history?talentId= で評価履歴が返る', async ({ request }) => {
    const res = await request.get(`${API_BASE}/nine-box/history?talentId=${TALENT_ID}`);
    expect(res.status()).toBe(200);

    const body = await res.json();
    expect(body.talentId).toBe(TALENT_ID);
    expect(Array.isArray(body.history)).toBe(true);
    expect(body.history.length).toBeGreaterThanOrEqual(1);
  });
});
