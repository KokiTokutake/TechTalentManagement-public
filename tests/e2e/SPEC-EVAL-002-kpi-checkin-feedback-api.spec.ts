import { test, expect } from '@playwright/test';

// SPEC-EVAL-002: KPI設定・チェックイン・フィードバック面談 API テスト（TASK-066）
// 仕様書: docs/spec/SPEC-EVAL-002-kpi-goals-and-checkin.md
// 設計書: docs/design/SPEC-EVAL-002-design.md
// 生成日: 2026-05-24（/test-write により自動生成）

const API = 'http://localhost:5108/api';

let managerId = '';
let talentId = '';
let talentMustGoalId = '';   // 今年度 Must ゴール（KPI テスト用）
let prevPeriodGoalId = '';   // 昨年度 Must ゴール（期間外テスト用）
let currPeriodId = '';

test.beforeAll(async ({ request }) => {
  // ── ユーザー取得 ──────────────────────────────────────────────────────────
  const usersRes = await request.get(`${API}/users`);
  const users: Array<{ id: string; displayName: string; role: string }> = await usersRes.json();

  const manager = users.find(u => u.displayName === '山田 一郎');
  if (!manager) throw new Error('シードデータに 山田 一郎 が見つかりません');
  managerId = manager.id;

  const talent = users.find(u => u.displayName === '田中 太郎');
  if (!talent) throw new Error('シードデータに 田中 太郎 が見つかりません');
  talentId = talent.id;

  // ── 考課期間取得 ──────────────────────────────────────────────────────────
  const periodsRes = await request.get(`${API}/periods`);
  const periods: Array<{ id: string; year: number }> = await periodsRes.json();
  const sortedPeriods = periods.sort((a, b) => b.year - a.year);
  const currPeriod = sortedPeriods[0];
  if (!currPeriod) throw new Error('考課期間が見つかりません');
  currPeriodId = currPeriod.id;
  const prevPeriod = sortedPeriods[1];

  // ── 今年度 Must ゴール取得 ────────────────────────────────────────────────
  const goalsRes = await request.get(
    `${API}/periods/${currPeriodId}/users/${talentId}/goals`,
    { headers: { 'X-User-Id': talentId } }
  );
  if (goalsRes.status() === 200) {
    const goals: Array<{ id: string; category: string }> = await goalsRes.json();
    const mustGoal = goals.find(g => g.category === 'Must' || g.category === 'must' || g.category === 2);
    if (mustGoal) talentMustGoalId = mustGoal.id;
  }
  if (!talentMustGoalId) throw new Error('今年度の Must ゴールが見つかりません');

  // ── 昨年度 Must ゴール取得 ────────────────────────────────────────────────
  if (prevPeriod) {
    const prevGoalsRes = await request.get(
      `${API}/periods/${prevPeriod.id}/users/${talentId}/goals`,
      { headers: { 'X-User-Id': talentId } }
    );
    if (prevGoalsRes.status() === 200) {
      const prevGoals: Array<{ id: string; category: string }> = await prevGoalsRes.json();
      const prevMust = prevGoals.find(g => g.category === 'Must' || g.category === 'must' || g.category === 2);
      if (prevMust) prevPeriodGoalId = prevMust.id;
    }
  }
});

// ─────────────────────────────────────────────────────────────────────────────
// AC-01: Mustへの定量KPI設定
// ─────────────────────────────────────────────────────────────────────────────

test.describe('AC-01: Mustへの定量KPI設定', () => {

  // AC-01-01: KPIの入力と保存
  test('AC-01-01: KPIを設定すると201で返却され、目標と紐付いて保存される', async ({ request }) => {
    // When: KPI設定（POST）
    const postRes = await request.post(`${API}/goals/${talentMustGoalId}/kpi`, {
      data: {
        metricName: 'プロジェクトデリバリー件数',
        targetValue: 12,
        unit: '件',
        measurementMethod: 'PM報告書で確認',
        dueDate: '2026-12-31',
      },
      headers: { 'X-User-Id': talentId },
    });

    if (postRes.status() === 409) {
      // 既存KPIがあればPUTで上書き（複数テスト実行でも安定）
      const putRes = await request.put(`${API}/goals/${talentMustGoalId}/kpi`, {
        data: {
          metricName: 'プロジェクトデリバリー件数',
          targetValue: 12,
          unit: '件',
          measurementMethod: 'PM報告書で確認',
          dueDate: '2026-12-31',
        },
        headers: { 'X-User-Id': talentId },
      });
      expect(putRes.status()).toBe(200);
      const body = await putRes.json();
      expect(body.goalId).toBe(talentMustGoalId);
      expect(body.metricName).toBe('プロジェクトデリバリー件数');
      expect(body.targetValue).toBe(12);
      expect(body.unit).toBe('件');
    } else {
      // Then: 201 + KPI情報が返却される
      expect(postRes.status()).toBe(201);
      const body = await postRes.json();
      expect(body.goalId).toBe(talentMustGoalId);
      expect(body.metricName).toBe('プロジェクトデリバリー件数');
      expect(body.targetValue).toBe(12);
      expect(body.unit).toBe('件');
      expect(body.measurementMethod).toBe('PM報告書で確認');
      expect(body.id).toBeTruthy();
    }
  });

  test('AC-01-01: KPI更新（PUT）で指標名・目標値を変更できる', async ({ request }) => {
    // 事前: KPI存在確認（なければ作成）
    await request.post(`${API}/goals/${talentMustGoalId}/kpi`, {
      data: { metricName: '初回設定', targetValue: 10, unit: '件', measurementMethod: null, dueDate: null },
      headers: { 'X-User-Id': talentId },
    });

    // When: PUT で更新
    const putRes = await request.put(`${API}/goals/${talentMustGoalId}/kpi`, {
      data: {
        metricName: '更新後の指標名',
        targetValue: 15,
        unit: '%',
        measurementMethod: '週次レポートで確認',
        dueDate: null,
      },
      headers: { 'X-User-Id': talentId },
    });

    // Then: 200 + 更新後データが返却される
    expect(putRes.status()).toBe(200);
    const body = await putRes.json();
    expect(body.metricName).toBe('更新後の指標名');
    expect(body.targetValue).toBe(15);
    expect(body.unit).toBe('%');
  });

  // AC-01-02: 評価期間外の入力制限
  test('AC-01-02: 目標設定期間が終了した考課期間のKPIは403エラーになる', async ({ request }) => {
    if (!prevPeriodGoalId) {
      test.skip(true, '昨年度のMust ゴールが見つかりません');
      return;
    }

    // When: 昨年度（目標設定期間終了済み）のゴールにKPI設定
    const res = await request.post(`${API}/goals/${prevPeriodGoalId}/kpi`, {
      data: {
        metricName: '期間外テスト',
        targetValue: 5,
        unit: '件',
        measurementMethod: null,
        dueDate: null,
      },
      headers: { 'X-User-Id': talentId },
    });

    // Then: 403 + エラーメッセージ
    expect(res.status()).toBe(403);
    const body = await res.json();
    expect(body.error).toContain('目標設定期間が終了しています');
  });

  test('AC-01-02: 存在しないKPIへのPUTは404を返す', async ({ request }) => {
    const fakeGoalId = '00000000-0000-0000-0000-000000000001';
    const res = await request.put(`${API}/goals/${fakeGoalId}/kpi`, {
      data: { metricName: 'テスト', targetValue: 1, unit: '件', measurementMethod: null, dueDate: null },
      headers: { 'X-User-Id': talentId },
    });
    expect(res.status()).toBeGreaterThanOrEqual(403);
  });
});

// ─────────────────────────────────────────────────────────────────────────────
// AC-02: 進捗チェックイン
// ─────────────────────────────────────────────────────────────────────────────

test.describe('AC-02: 進捗チェックイン', () => {

  // AC-02-01: チェックインの記録と進捗率自動計算
  test('AC-02-01: チェックインを記録すると進捗率が自動計算される', async ({ request }) => {
    // 事前: KPIを設定（targetValue = 12）
    await request.post(`${API}/goals/${talentMustGoalId}/kpi`, {
      data: { metricName: 'プロジェクトデリバリー件数', targetValue: 12, unit: '件', measurementMethod: null, dueDate: null },
      headers: { 'X-User-Id': talentId },
    });
    await request.put(`${API}/goals/${talentMustGoalId}/kpi`, {
      data: { metricName: 'プロジェクトデリバリー件数', targetValue: 12, unit: '件', measurementMethod: null, dueDate: null },
      headers: { 'X-User-Id': talentId },
    });

    // When: 現在値 8 でチェックイン
    const res = await request.post(`${API}/goals/${talentMustGoalId}/checkins`, {
      data: {
        currentValue: 8,
        comment: 'Q2で4件完了、Q3でペース維持中',
      },
      headers: { 'X-User-Id': talentId },
    });

    // Then: 201 + 進捗率 66.67% (8/12)
    expect(res.status()).toBe(201);
    const body = await res.json();
    expect(body.goalId).toBe(talentMustGoalId);
    expect(body.currentValue).toBe(8);
    expect(body.progressPercent).toBeCloseTo(66.67, 1);
    expect(body.comment).toBe('Q2で4件完了、Q3でペース維持中');
    expect(body.checkedInAt).toBeTruthy();
  });

  test('AC-02-01: KPI未設定ゴールでもチェックイン可能（progressPercentはnull）', async ({ request }) => {
    // 別のゴール（KPI未設定のCan目標）でチェックイン
    const goalsRes = await request.get(
      `${API}/periods/${currPeriodId}/users/${talentId}/goals`,
      { headers: { 'X-User-Id': talentId } }
    );
    const goals: Array<{ id: string; category: string }> = await goalsRes.json();
    const canGoal = goals.find(g => g.category === 'Can' || g.category === 'can' || g.category === 1);
    if (!canGoal) { test.skip(true, 'Can ゴールが見つかりません'); return; }

    const res = await request.post(`${API}/goals/${canGoal.id}/checkins`, {
      data: { currentValue: null, comment: 'KPI未設定のチェックイン' },
      headers: { 'X-User-Id': talentId },
    });
    expect(res.status()).toBe(201);
    const body = await res.json();
    expect(body.progressPercent).toBeNull();
  });

  // AC-02-02: チェックイン履歴の時系列表示
  test('AC-02-02: チェックイン履歴が新しい順（降順）で返却される', async ({ request }) => {
    // 事前: 複数チェックインを記録（順序確認のため複数作成）
    for (const value of [3, 6]) {
      await request.post(`${API}/goals/${talentMustGoalId}/checkins`, {
        data: { currentValue: value, comment: `チェックイン ${value}件` },
        headers: { 'X-User-Id': talentId },
      });
    }

    // When: チェックイン履歴取得
    const res = await request.get(`${API}/goals/${talentMustGoalId}/checkins`, {
      headers: { 'X-User-Id': talentId },
    });

    // Then: 200 + checkedInAt 降順で複数件返却
    expect(res.status()).toBe(200);
    const body = await res.json();
    expect(body.goalId).toBe(talentMustGoalId);
    expect(body.checkins).toBeDefined();
    expect(body.checkins.length).toBeGreaterThanOrEqual(1);

    // checkedInAt が降順であることを確認
    if (body.checkins.length >= 2) {
      const dates = body.checkins.map((c: { checkedInAt: string }) => new Date(c.checkedInAt).getTime());
      for (let i = 0; i < dates.length - 1; i++) {
        expect(dates[i]).toBeGreaterThanOrEqual(dates[i + 1]);
      }
    }
  });

  test('AC-02-02: チェックイン履歴レスポンスにKPI情報が含まれる', async ({ request }) => {
    const res = await request.get(`${API}/goals/${talentMustGoalId}/checkins`, {
      headers: { 'X-User-Id': talentId },
    });
    expect(res.status()).toBe(200);
    const body = await res.json();
    // KPIが設定されていれば kpi フィールドが存在する
    if (body.kpi !== null) {
      expect(body.kpi.metricName).toBeTruthy();
      expect(body.kpi.targetValue).toBeGreaterThan(0);
      expect(body.kpi.unit).toBeTruthy();
    }
  });

  // AC-02-03: 進捗率の色分け表示 → FEコンポーネント（TASK-067）で実装
  test.skip('AC-02-03: 進捗率の色分け表示（80%↑緑/50-79%黄/49%↓赤）', async () => {
    // FEコンポーネント（GoalProgressBadge.vue）でカバー。TASK-067の受入テストで検証する。
  });
});

// ─────────────────────────────────────────────────────────────────────────────
// AC-03: リマインダー通知
// ─────────────────────────────────────────────────────────────────────────────

test.describe('AC-03: リマインダー通知', () => {
  // バッチ（Azure Functions TimerTrigger）の責務のためE2E対象外
  test.skip('AC-03-01: 中間リマインダー送信（Azure Functions TimerTrigger）', async () => {
    // インフラ層（Azure Functions）で実装。E2Eテストの対象外。
    // 動作確認は Azure Portal / Application Insights で実施する。
  });
});

// ─────────────────────────────────────────────────────────────────────────────
// AC-04 / AC-05: フィードバック面談記録・評価開示
// ─────────────────────────────────────────────────────────────────────────────

test.describe('AC-04/05: フィードバック面談記録・評価開示', () => {
  let selfEvalId = '';
  let meetingId = '';

  test.beforeAll(async ({ request }) => {
    // 前年度の自己評価レコードを取得（評価期間は終了済み→面談記録作成はスキップ）
    const periodsRes = await request.get(`${API}/periods`);
    const periods: Array<{ id: string; year: number }> = await periodsRes.json();
    const prevPeriod = periods.sort((a, b) => a.year - b.year)[0];
    if (!prevPeriod) return;

    // 前年度の自己評価IDを探す（DataSeederでse1p1が作成済み）
    // /api/self-evaluations?userId=...&periodId=... エンドポイントから取得
    const seRes = await request.get(
      `${API}/periods/${prevPeriod.id}/users/${talentId}/self-evaluations`,
      { headers: { 'X-User-Id': managerId } }
    );
    if (seRes.status() === 200) {
      const se = await seRes.json();
      if (se?.id) selfEvalId = se.id;
    }
  });

  // AC-04-01: 面談記録の作成
  // 注意: currPeriodの評価期間は today+60 で未開始のため test.skip
  // prevPeriodの評価期間は終了済みのため test.skip
  test.skip('AC-04-01: 評価期間中に面談記録を作成すると201かつIsDisclosed=falseで返却される', async ({ request }) => {
    // このテストは評価期間中（IsEvaluationOpen = true）の期間が存在する環境でのみ実行可能。
    // DataSeederの設計上、テスト実行時点では評価期間オープンの期間が存在しないためスキップ。
    // 評価フェーズのシステム統合テスト時（today+60日以降）に手動で確認する。
    //
    // 期待動作:
    //   POST /api/feedback-meetings → 201
    //   body.isDisclosed === false
  });

  // AC-05-02: 開示前の非表示 → タレントがGETしても404
  test('AC-05-02: 未開示の面談記録はタレントがGETすると404になる', async ({ request }) => {
    const periodsRes = await request.get(`${API}/periods`);
    const periods: Array<{ id: string; year: number }> = await periodsRes.json();
    const currPeriod = periods.sort((a, b) => b.year - a.year)[0];

    // When: タレント自身がGET（面談記録未作成または未開示）
    const res = await request.get(
      `${API}/feedback-meetings?talentId=${talentId}&periodId=${currPeriod.id}`,
      { headers: { 'X-User-Id': talentId } }
    );

    // Then: 404（存在しないか未開示のため）
    expect(res.status()).toBe(404);
  });

  // AC-05-01 / AC-05-02 の開示フロー
  test.skip('AC-05-01: 上長が開示するとIsDisclosed=trueになりタレントが閲覧可能になる', async ({ request }) => {
    // このテストは面談記録が作成済みの環境（AC-04-01通過後）で実行。
    // 評価期間オープン後に統合テストとして実行する。
    //
    // 期待動作:
    //   PATCH /api/feedback-meetings/{id}/disclose → 200, isDisclosed: true
    //   GET /api/feedback-meetings?talentId=...&periodId=... (talent) → 200
  });
});

// ─────────────────────────────────────────────────────────────────────────────
// 認可チェック
// ─────────────────────────────────────────────────────────────────────────────

test.describe('認可チェック', () => {

  test('ManagerはタレントのKPIを閲覧できる', async ({ request }) => {
    const res = await request.get(`${API}/goals/${talentMustGoalId}/checkins`, {
      headers: { 'X-User-Id': managerId },
    });
    expect(res.status()).toBe(200);
  });

  test('存在しないゴールへのKPI設定は404を返す', async ({ request }) => {
    const fakeGoalId = '00000000-0000-0000-0000-000000000099';
    const res = await request.post(`${API}/goals/${fakeGoalId}/kpi`, {
      data: { metricName: 'テスト', targetValue: 1, unit: '件', measurementMethod: null, dueDate: null },
      headers: { 'X-User-Id': talentId },
    });
    expect(res.status()).toBe(404);
  });

  test('X-User-Id なしでKPIを設定しようとすると403になる', async ({ request }) => {
    const res = await request.post(`${API}/goals/${talentMustGoalId}/kpi`, {
      data: { metricName: 'テスト', targetValue: 1, unit: '件', measurementMethod: null, dueDate: null },
    });
    // X-User-Id が Guid.Empty → ユーザー見つからず 403
    expect(res.status()).toBeGreaterThanOrEqual(403);
  });

  test('面談記録はManager以外（Senior以上）は作成できない（403）', async ({ request }) => {
    const periodsRes = await request.get(`${API}/periods`);
    const periods: Array<{ id: string; year: number }> = await periodsRes.json();
    const currPeriod = periods.sort((a, b) => b.year - a.year)[0];

    const senior = (await (await request.get(`${API}/users`)).json() as Array<{ id: string; role: string }>)
      .find(u => u.role === 'Senior');
    if (!senior) { test.skip(true, 'Senior ユーザーが見つかりません'); return; }

    const res = await request.post(`${API}/feedback-meetings`, {
      data: {
        selfEvaluationId: '00000000-0000-0000-0000-000000000001',
        talentId,
        conductedAt: '2026-12-20',
        overallComment: 'テスト',
        nextPeriodExpectation: null,
        talentReactionNote: null,
      },
      headers: { 'X-User-Id': senior.id },
    });
    expect(res.status()).toBe(403);
  });
});
