import { test, expect } from '@playwright/test';

// SPEC-CAREER-002: Will-Can-Must深化 — API テスト（TASK-062）
// 仕様書: docs/spec/SPEC-CAREER-002-will-can-must-enhancement.md
// 設計書: docs/design/SPEC-CAREER-002-design.md

const API = 'http://localhost:5108/api';

let seniorId = '';
let managerId = '';
let talentId = '';
let periodId = '';
let skillItemId = '';

test.beforeAll(async ({ request }) => {
  const usersRes = await request.get(`${API}/users`);
  const users: Array<{ id: string; displayName: string; role: string }> = await usersRes.json();

  const senior = users.find(u => u.displayName === '部長 一郎');
  if (!senior) throw new Error('シードデータに部長 一郎 が見つかりません');
  seniorId = senior.id;

  const manager = users.find(u => u.displayName === '山田 一郎');
  if (!manager) throw new Error('シードデータに山田 一郎 が見つかりません');
  managerId = manager.id;

  const talent = users.find(u => u.displayName === '田中 太郎');
  if (!talent) throw new Error('シードデータに田中 太郎 が見つかりません');
  talentId = talent.id;

  const periodsRes = await request.get(`${API}/periods`);
  const periods: Array<{ id: string; year: number }> = await periodsRes.json();
  const latestPeriod = periods.sort((a, b) => b.year - a.year)[0];
  if (!latestPeriod) throw new Error('考課期間が見つかりません');
  periodId = latestPeriod.id;

  const categoriesRes = await request.get(`${API}/skill-categories`);
  const categories: Array<{ id: string; name: string }> = await categoriesRes.json();
  const feCat = categories.find(c => c.name === 'フロントエンド');
  if (feCat) {
    const itemsRes = await request.get(`${API}/skill-categories/${feCat.id}/items`);
    const items: Array<{ id: string; name: string }> = await itemsRes.json();
    const tsSkill = items.find(s => s.name === 'TypeScript');
    if (tsSkill) skillItemId = tsSkill.id;
  }
});

// ── AC-01: Will の時間軸分割 ──────────────────────────────────────────────────

test.describe('AC-01: Willの時間軸分割', () => {
  test('AC-01-01: 3軸のWillが periodId と紐付いて保存・取得できる', async ({ request }) => {
    // 保存
    const saveRes = await request.put(`${API}/career/wcm`, {
      data: {
        talentId,
        periodId,
        will: {
          shortTerm: [{ id: null, content: '直近6ヶ月でチームリードを経験したい' }],
          midTerm:   [{ id: null, content: '3年以内にアーキテクトになりたい' }],
          longTerm:  [{ id: null, content: '5年後には技術責任者を目指す' }],
        },
        can: { strengths: [], challenges: [] },
        must: [],
      },
      headers: { 'X-User-Id': talentId },
    });
    expect(saveRes.status()).toBe(200);
    const saved = await saveRes.json();
    expect(saved.will.shortTerm).toHaveLength(1);
    expect(saved.will.midTerm).toHaveLength(1);
    expect(saved.will.longTerm).toHaveLength(1);
    expect(saved.will.shortTerm[0].content).toBe('直近6ヶ月でチームリードを経験したい');

    // 取得でも確認
    const getRes = await request.get(`${API}/career/wcm?talentId=${talentId}&periodId=${periodId}`, {
      headers: { 'X-User-Id': talentId },
    });
    expect(getRes.status()).toBe(200);
    const got = await getRes.json();
    expect(got.talentId).toBe(talentId);
    expect(got.periodId).toBe(periodId);
    expect(got.will.shortTerm).toHaveLength(1);
    expect(got.will.midTerm).toHaveLength(1);
    expect(got.will.longTerm).toHaveLength(1);
  });

  test('AC-01-02: 中期Willのみ保存してもバリデーションエラーにならない', async ({ request }) => {
    const res = await request.put(`${API}/career/wcm`, {
      data: {
        talentId,
        periodId,
        will: {
          shortTerm: [],
          midTerm:   [{ id: null, content: '部分入力テスト' }],
          longTerm:  [],
        },
        can: { strengths: [], challenges: [] },
        must: [],
      },
      headers: { 'X-User-Id': talentId },
    });
    expect(res.status()).toBe(200);
    const body = await res.json();
    expect(body.will.shortTerm).toHaveLength(0);
    expect(body.will.midTerm).toHaveLength(1);
    expect(body.will.longTerm).toHaveLength(0);
  });
});

// ── AC-02: Can の強み・課題分割 ──────────────────────────────────────────────

test.describe('AC-02: Canの強み・課題分割', () => {
  test('AC-02-01: 強み3件・課題2件がカテゴリ別に保存・取得できる', async ({ request }) => {
    const res = await request.put(`${API}/career/wcm`, {
      data: {
        talentId,
        periodId,
        will: { shortTerm: [], midTerm: [], longTerm: [] },
        can: {
          strengths: [
            { id: null, content: 'TypeScriptの型設計が得意' },
            { id: null, content: 'チームのコードレビューをリード' },
            { id: null, content: 'Azure インフラ設計の経験' },
          ],
          challenges: [
            { id: null, content: 'プレゼンテーション力を伸ばしたい' },
            { id: null, content: 'プロジェクトマネジメントを学びたい' },
          ],
        },
        must: [],
      },
      headers: { 'X-User-Id': talentId },
    });
    expect(res.status()).toBe(200);
    const body = await res.json();
    expect(body.can.strengths).toHaveLength(3);
    expect(body.can.challenges).toHaveLength(2);
    expect(body.can.strengths[0].content).toBe('TypeScriptの型設計が得意');
    expect(body.can.challenges[0].content).toBe('プレゼンテーション力を伸ばしたい');
  });

  test('AC-02-02: linkedSkillItemId でスキルマスタと紐付けられる', async ({ request }) => {
    if (!skillItemId) { test.skip(true, 'スキルアイテムが取得できませんでした'); return; }

    const res = await request.put(`${API}/career/wcm`, {
      data: {
        talentId,
        periodId,
        will: { shortTerm: [], midTerm: [], longTerm: [] },
        can: {
          strengths: [{ id: null, content: 'TypeScript', linkedSkillItemId: skillItemId }],
          challenges: [],
        },
        must: [],
      },
      headers: { 'X-User-Id': talentId },
    });
    expect(res.status()).toBe(200);
    const body = await res.json();
    expect(body.can.strengths[0].linkedSkillItemId).toBe(skillItemId);
    expect(body.can.strengths[0].linkedSkillName).toBe('TypeScript');
  });

  test('AC-02-03: 強みを6件送ると 400 エラーになる', async ({ request }) => {
    const res = await request.put(`${API}/career/wcm`, {
      data: {
        talentId,
        periodId,
        will: { shortTerm: [], midTerm: [], longTerm: [] },
        can: {
          strengths: [
            { id: null, content: '1件目' },
            { id: null, content: '2件目' },
            { id: null, content: '3件目' },
            { id: null, content: '4件目' },
            { id: null, content: '5件目' },
            { id: null, content: '6件目（超過）' },
          ],
          challenges: [],
        },
        must: [],
      },
      headers: { 'X-User-Id': talentId },
    });
    expect(res.status()).toBe(400);
    const body = await res.json();
    expect(body.error).toContain('強みは最大5件');
  });

  test('AC-02-03: 課題を6件送ると 400 エラーになる', async ({ request }) => {
    const res = await request.put(`${API}/career/wcm`, {
      data: {
        talentId,
        periodId,
        will: { shortTerm: [], midTerm: [], longTerm: [] },
        can: {
          strengths: [],
          challenges: [
            { id: null, content: '1件目' },
            { id: null, content: '2件目' },
            { id: null, content: '3件目' },
            { id: null, content: '4件目' },
            { id: null, content: '5件目' },
            { id: null, content: '6件目（超過）' },
          ],
        },
        must: [],
      },
      headers: { 'X-User-Id': talentId },
    });
    expect(res.status()).toBe(400);
    const body = await res.json();
    expect(body.error).toContain('課題は最大5件');
  });
});

// ── AC-03: 価値観・やりがい ───────────────────────────────────────────────────

test.describe('AC-03: 価値観・やりがい', () => {
  test('AC-03-01: 価値観3項目を保存・取得できる', async ({ request }) => {
    const saveRes = await request.put(`${API}/career/values`, {
      data: {
        talentId,
        periodId,
        importantThings:       '誠実なコミュニケーションを大切にする',
        motivationSituations:  'チームで難しい課題を解決したとき',
        avoidSituations:       '成長機会のない単純作業の繰り返し',
      },
      headers: { 'X-User-Id': talentId },
    });
    expect(saveRes.status()).toBe(200);
    const saved = await saveRes.json();
    expect(saved.talentId).toBe(talentId);
    expect(saved.importantThings).toBe('誠実なコミュニケーションを大切にする');
    expect(saved.motivationSituations).toBe('チームで難しい課題を解決したとき');
    expect(saved.avoidSituations).toBe('成長機会のない単純作業の繰り返し');

    // GETでも確認
    const getRes = await request.get(`${API}/career/values?talentId=${talentId}&periodId=${periodId}`, {
      headers: { 'X-User-Id': talentId },
    });
    expect(getRes.status()).toBe(200);
    const got = await getRes.json();
    expect(got.importantThings).toBe('誠実なコミュニケーションを大切にする');
    expect(got.id).not.toBeNull();
  });

  test('AC-03-01: 未保存の価値観はid:nullの空オブジェクトを返す（404にならない）', async ({ request }) => {
    const otherPeriodRes = await request.get(`${API}/periods`);
    const periods: Array<{ id: string; year: number }> = await otherPeriodRes.json();
    const oldPeriod = periods.sort((a, b) => a.year - b.year)[0];
    if (!oldPeriod || oldPeriod.id === periodId) { test.skip(true, '前年度期間が見つかりません'); return; }

    // 佐藤花子（talent2）の前年度期間は未保存のはず
    const talent2 = (await (await request.get(`${API}/users`)).json() as Array<{ id: string; displayName: string }>)
      .find(u => u.displayName === '佐藤 花子');
    if (!talent2) { test.skip(true, '佐藤 花子 が見つかりません'); return; }

    const res = await request.get(`${API}/career/values?talentId=${talent2.id}&periodId=${oldPeriod.id}`, {
      headers: { 'X-User-Id': seniorId },
    });
    expect(res.status()).toBe(200);
    const body = await res.json();
    expect(body.id).toBeNull();
  });

  test('AC-03-01: Upsert - 2回保存すると内容が上書きされる', async ({ request }) => {
    const talent2 = (await (await request.get(`${API}/users`)).json() as Array<{ id: string; displayName: string }>)
      .find(u => u.displayName === '佐藤 花子');
    if (!talent2) { test.skip(true, '佐藤 花子 が見つかりません'); return; }

    await request.put(`${API}/career/values`, {
      data: { talentId: talent2.id, periodId, importantThings: '初回保存', motivationSituations: null, avoidSituations: null },
      headers: { 'X-User-Id': talent2.id },
    });

    const res2 = await request.put(`${API}/career/values`, {
      data: { talentId: talent2.id, periodId, importantThings: '2回目保存', motivationSituations: null, avoidSituations: null },
      headers: { 'X-User-Id': talent2.id },
    });
    expect(res2.status()).toBe(200);
    const body = await res2.json();
    expect(body.importantThings).toBe('2回目保存');
  });
});

// ── AC-04: Manager 上長コンテキスト表示 ──────────────────────────────────────

test.describe('AC-04: 上長コンテキスト表示', () => {
  test('AC-04-01: WCM取得レスポンスにmanagerExpectedRoleフィールドが含まれる', async ({ request }) => {
    const res = await request.get(`${API}/career/wcm?talentId=${talentId}&periodId=${periodId}`, {
      headers: { 'X-User-Id': talentId },
    });
    expect(res.status()).toBe(200);
    const body = await res.json();
    expect('managerExpectedRole' in body).toBe(true);
  });
});

// ── 認可チェック ───────────────────────────────────────────────────────────────

test.describe('認可チェック', () => {
  test('他タレントのWCMをManagerは閲覧できる', async ({ request }) => {
    const res = await request.get(`${API}/career/wcm?talentId=${talentId}&periodId=${periodId}`, {
      headers: { 'X-User-Id': managerId },
    });
    expect(res.status()).toBe(200);
  });

  test('SeniorはすべてのタレントのWCMを閲覧できる', async ({ request }) => {
    const res = await request.get(`${API}/career/wcm?talentId=${talentId}&periodId=${periodId}`, {
      headers: { 'X-User-Id': seniorId },
    });
    expect(res.status()).toBe(200);
  });

  test('他タレントのWCMをTalentは保存できない（403）', async ({ request }) => {
    const talent2 = (await (await request.get(`${API}/users`)).json() as Array<{ id: string; displayName: string }>)
      .find(u => u.displayName === '佐藤 花子');
    if (!talent2) { test.skip(true, '佐藤 花子 が見つかりません'); return; }

    const res = await request.put(`${API}/career/wcm`, {
      data: {
        talentId: talent2.id,
        periodId,
        will: { shortTerm: [{ id: null, content: '他人のWCMを保存しようとする' }], midTerm: [], longTerm: [] },
        can: { strengths: [], challenges: [] },
        must: [],
      },
      headers: { 'X-User-Id': talentId }, // 田中が佐藤のデータを保存しようとする
    });
    expect(res.status()).toBe(403);
  });
});
