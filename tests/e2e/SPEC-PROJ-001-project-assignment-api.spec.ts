import { test, expect } from '@playwright/test';

// SPEC-PROJ-001: プロジェクト・アサインメント履歴管理 — API 結合テスト（TASK-084）
// 仕様書: docs/spec/SPEC-PROJ-001-project-assignment.md
// 設計書: docs/design/SPEC-PROJ-001-design.md

const API = 'http://localhost:5108/api';

let managerId = '';   // 山田 一郎（Manager）
let talentId = '';    // 田中 太郎（Talent）
let talent2Id = '';   // 佐藤 花子（Talent）
let seniorId = '';    // 部長 一郎（Senior）
let skillItemId = ''; // TypeScript スキルアイテム

test.beforeAll(async ({ request }) => {
  const usersRes = await request.get(`${API}/users`);
  expect(usersRes.ok()).toBe(true);
  const users: Array<{ id: string; displayName: string; role: string }> = await usersRes.json();

  const yamada = users.find(u => u.displayName === '山田 一郎');
  if (!yamada) throw new Error('シードデータに山田 一郎 が見つかりません');
  managerId = yamada.id;

  const tanaka = users.find(u => u.displayName === '田中 太郎');
  if (!tanaka) throw new Error('シードデータに田中 太郎 が見つかりません');
  talentId = tanaka.id;

  const sato = users.find(u => u.displayName === '佐藤 花子');
  if (!sato) throw new Error('シードデータに佐藤 花子 が見つかりません');
  talent2Id = sato.id;

  const bucho = users.find(u => u.displayName === '部長 一郎');
  if (!bucho) throw new Error('シードデータに部長 一郎 が見つかりません');
  seniorId = bucho.id;

  const categoriesRes = await request.get(`${API}/skill-categories`);
  expect(categoriesRes.ok()).toBe(true);
  const categories: Array<{ id: string; name: string }> = await categoriesRes.json();
  const catFE = categories.find(c => c.name === 'フロントエンド');
  if (!catFE) throw new Error('フロントエンドカテゴリが見つかりません');

  const itemsRes = await request.get(`${API}/skill-categories/${catFE.id}/items`);
  expect(itemsRes.ok()).toBe(true);
  const items: Array<{ id: string; name: string }> = await itemsRes.json();
  const ts = items.find(i => i.name === 'TypeScript');
  if (!ts) throw new Error('TypeScript スキルアイテムが見つかりません');
  skillItemId = ts.id;
});

// ── AC-01: プロジェクト履歴 CRUD ─────────────────────────────────────────────

test.describe('AC-01: プロジェクト履歴 CRUD', () => {
  let assignmentId = '';

  // AC-01-01: プロジェクトの登録
  test('AC-01-01: タレントが自身のプロジェクトを登録できる（201）', async ({ request }) => {
    const res = await request.post(`${API}/project-assignments`, {
      data: {
        projectName: '社内ポータル刷新',
        role: 'バックエンドテックリード',
        startDate: '2024-04-01',
        endDate: null,
        description: '社内ポータル刷新プロジェクト',
        skillItemIds: [skillItemId],
      },
      headers: { 'X-User-Id': talentId },
    });
    expect(res.status()).toBe(201);
    const body = await res.json();
    expect(body.projectName).toBe('社内ポータル刷新');
    expect(body.role).toBe('バックエンドテックリード');
    expect(body.startDate).toBe('2024-04-01');
    expect(body.endDate).toBeNull();
    expect(body.isActive).toBe(true);
    expect(body.skills).toHaveLength(1);
    expect(body.skills[0].skillName).toBe('TypeScript');
    assignmentId = body.id;
  });

  // AC-01-01: 登録後に一覧で取得できる
  test('AC-01-01: 登録したプロジェクトが一覧に表示される（200）', async ({ request }) => {
    if (!assignmentId) test.skip(true, 'assignmentId が未取得');
    const res = await request.get(`${API}/project-assignments?talentId=${talentId}`, {
      headers: { 'X-User-Id': talentId },
    });
    expect(res.status()).toBe(200);
    const body: Array<{ id: string; projectName: string }> = await res.json();
    expect(Array.isArray(body)).toBe(true);
    const found = body.find(a => a.id === assignmentId);
    expect(found).toBeDefined();
    expect(found!.projectName).toBe('社内ポータル刷新');
  });

  // AC-01-02: プロジェクトの編集
  test('AC-01-02: タレントが自身のプロジェクト名を変更できる（200）', async ({ request }) => {
    if (!assignmentId) test.skip(true, 'assignmentId が未取得');
    const res = await request.put(`${API}/project-assignments/${assignmentId}`, {
      data: {
        projectName: '社内ポータル刷新（改）',
        role: 'バックエンドテックリード',
        startDate: '2024-04-01',
        endDate: null,
        description: '社内ポータル刷新プロジェクト（フェーズ2込み）',
        skillItemIds: [skillItemId],
      },
      headers: { 'X-User-Id': talentId },
    });
    expect(res.status()).toBe(200);
    const body = await res.json();
    expect(body.projectName).toBe('社内ポータル刷新（改）');
  });

  // 他のタレントが更新しようとすると 403
  test('AC-01-02: 他のタレントがプロジェクトを更新しようとすると 403', async ({ request }) => {
    if (!assignmentId) test.skip(true, 'assignmentId が未取得');
    const res = await request.put(`${API}/project-assignments/${assignmentId}`, {
      data: {
        projectName: '不正更新',
        role: 'テスト',
        startDate: '2024-04-01',
        endDate: null,
        description: null,
        skillItemIds: [],
      },
      headers: { 'X-User-Id': talent2Id },
    });
    expect(res.status()).toBe(403);
  });

  // AC-01-03: プロジェクトの削除（使用技術も含めてカスケード削除される）
  test('AC-01-03: タレントが自身のプロジェクトを削除すると 204 かつ一覧から消える', async ({ request }) => {
    if (!assignmentId) test.skip(true, 'assignmentId が未取得');

    // 削除実行
    const deleteRes = await request.delete(`${API}/project-assignments/${assignmentId}`, {
      headers: { 'X-User-Id': talentId },
    });
    expect(deleteRes.status()).toBe(204);

    // 一覧から消えている
    const listRes = await request.get(`${API}/project-assignments?talentId=${talentId}`, {
      headers: { 'X-User-Id': talentId },
    });
    const body: Array<{ id: string }> = await listRes.json();
    expect(body.find(a => a.id === assignmentId)).toBeUndefined();
  });
});

// ── AC-02: データ記録（EndDate・参画中フラグ） ─────────────────────────────

test.describe('AC-02: データ記録', () => {
  let activeAssignmentId = '';
  let closedAssignmentId = '';

  test.beforeAll(async ({ request }) => {
    // 参画中プロジェクト
    const activeRes = await request.post(`${API}/project-assignments`, {
      data: {
        projectName: '参画中テストプロジェクト',
        role: 'フロントエンドエンジニア',
        startDate: '2025-01-01',
        endDate: null,
        description: null,
        skillItemIds: [],
      },
      headers: { 'X-User-Id': talent2Id },
    });
    activeAssignmentId = (await activeRes.json()).id;

    // 終了済みプロジェクト
    const closedRes = await request.post(`${API}/project-assignments`, {
      data: {
        projectName: '終了済みテストプロジェクト',
        role: 'バックエンドエンジニア',
        startDate: '2023-01-01',
        endDate: '2023-12-31',
        description: null,
        skillItemIds: [],
      },
      headers: { 'X-User-Id': talent2Id },
    });
    closedAssignmentId = (await closedRes.json()).id;
  });

  // AC-02-01: 参画中プロジェクト（EndDate なし）
  test('AC-02-01: EndDate なしで登録すると isActive=true で返る', async ({ request }) => {
    if (!activeAssignmentId) test.skip(true, 'activeAssignmentId が未取得');
    const res = await request.get(`${API}/project-assignments?talentId=${talent2Id}`, {
      headers: { 'X-User-Id': talent2Id },
    });
    expect(res.status()).toBe(200);
    const body: Array<{ id: string; isActive: boolean; endDate: string | null }> = await res.json();
    const found = body.find(a => a.id === activeAssignmentId);
    expect(found).toBeDefined();
    expect(found!.isActive).toBe(true);
    expect(found!.endDate).toBeNull();
  });

  // AC-02-02: 終了済みプロジェクト
  test('AC-02-02: EndDate ありで登録すると isActive=false で返る', async ({ request }) => {
    if (!closedAssignmentId) test.skip(true, 'closedAssignmentId が未取得');
    const res = await request.get(`${API}/project-assignments?talentId=${talent2Id}`, {
      headers: { 'X-User-Id': talent2Id },
    });
    expect(res.status()).toBe(200);
    const body: Array<{ id: string; isActive: boolean; startDate: string; endDate: string | null }> = await res.json();
    const found = body.find(a => a.id === closedAssignmentId);
    expect(found).toBeDefined();
    expect(found!.isActive).toBe(false);
    expect(found!.startDate).toBe('2023-01-01');
    expect(found!.endDate).toBe('2023-12-31');
  });

  // クリーンアップ
  test.afterAll(async ({ request }) => {
    if (activeAssignmentId)
      await request.delete(`${API}/project-assignments/${activeAssignmentId}`, { headers: { 'X-User-Id': talent2Id } });
    if (closedAssignmentId)
      await request.delete(`${API}/project-assignments/${closedAssignmentId}`, { headers: { 'X-User-Id': talent2Id } });
  });
});

// ── AC-03: 使用技術の紐付け ────────────────────────────────────────────────

test.describe('AC-03: 使用技術の紐付け', () => {
  let assignmentId = '';

  test('AC-03-01: 使用技術を紐付けてプロジェクトを登録すると skills に含まれる', async ({ request }) => {
    const res = await request.post(`${API}/project-assignments`, {
      data: {
        projectName: 'スキル紐付けテスト',
        role: 'フルスタックエンジニア',
        startDate: '2024-06-01',
        endDate: '2024-12-31',
        description: null,
        skillItemIds: [skillItemId],
      },
      headers: { 'X-User-Id': talentId },
    });
    expect(res.status()).toBe(201);
    const body = await res.json();
    expect(body.skills).toHaveLength(1);
    expect(body.skills[0].skillItemId).toBe(skillItemId);
    expect(body.skills[0].skillName).toBe('TypeScript');
    assignmentId = body.id;
  });

  test.afterAll(async ({ request }) => {
    if (assignmentId)
      await request.delete(`${API}/project-assignments/${assignmentId}`, { headers: { 'X-User-Id': talentId } });
  });
});

// ── AC-04: 上長参照 ───────────────────────────────────────────────────────

test.describe('AC-04: 上長参照', () => {
  let assignmentId = '';

  test.beforeAll(async ({ request }) => {
    const res = await request.post(`${API}/project-assignments`, {
      data: {
        projectName: '上長参照テスト用プロジェクト',
        role: 'バックエンドエンジニア',
        startDate: '2024-01-01',
        endDate: null,
        description: null,
        skillItemIds: [],
      },
      headers: { 'X-User-Id': talentId },
    });
    assignmentId = (await res.json()).id;
  });

  // AC-04-01: 上長（Manager）が参照できる
  test('AC-04-01: 上長（Manager）がタレントのプロジェクト履歴を参照できる（200）', async ({ request }) => {
    const res = await request.get(`${API}/project-assignments?talentId=${talentId}`, {
      headers: { 'X-User-Id': managerId },
    });
    expect(res.status()).toBe(200);
    const body: Array<{ id: string }> = await res.json();
    expect(body.find(a => a.id === assignmentId)).toBeDefined();
  });

  // Senior も参照できる
  test('AC-04-01: Senior がタレントのプロジェクト履歴を参照できる（200）', async ({ request }) => {
    const res = await request.get(`${API}/project-assignments?talentId=${talentId}`, {
      headers: { 'X-User-Id': seniorId },
    });
    expect(res.status()).toBe(200);
  });

  // 別タレントは参照できない
  test('AC-04-01: 別のタレントがプロジェクト履歴を参照しようとすると 403', async ({ request }) => {
    const res = await request.get(`${API}/project-assignments?talentId=${talentId}`, {
      headers: { 'X-User-Id': talent2Id },
    });
    expect(res.status()).toBe(403);
  });

  test.afterAll(async ({ request }) => {
    if (assignmentId)
      await request.delete(`${API}/project-assignments/${assignmentId}`, { headers: { 'X-User-Id': talentId } });
  });
});

// ── AC-05: 表示順（参画中が先頭、その後 StartDate DESC）────────────────────

test.describe('AC-05: 表示順', () => {
  const ids: string[] = [];

  test.beforeAll(async ({ request }) => {
    // 3件登録: 参画中(2025-06)、終了済み新(2024-09)、終了済み旧(2023-03)
    const data = [
      { projectName: '順序テスト_参画中', role: 'エンジニア', startDate: '2025-06-01', endDate: null },
      { projectName: '順序テスト_新旧テスト2024', role: 'エンジニア', startDate: '2024-09-01', endDate: '2024-12-31' },
      { projectName: '順序テスト_古い2023', role: 'エンジニア', startDate: '2023-03-01', endDate: '2023-08-31' },
    ];
    for (const d of data) {
      const res = await request.post(`${API}/project-assignments`, {
        data: { ...d, description: null, skillItemIds: [] },
        headers: { 'X-User-Id': talentId },
      });
      ids.push((await res.json()).id);
    }
  });

  // AC-05-01: 参画中が先頭、その後 StartDate DESC
  test('AC-05-01: 参画中が先頭、その後開始日の降順で表示される', async ({ request }) => {
    const res = await request.get(`${API}/project-assignments?talentId=${talentId}`, {
      headers: { 'X-User-Id': talentId },
    });
    expect(res.status()).toBe(200);
    const body: Array<{ projectName: string; isActive: boolean; startDate: string }> = await res.json();

    // テスト用の3件を抽出
    const testItems = body.filter(a => a.projectName.startsWith('順序テスト_'));
    expect(testItems.length).toBeGreaterThanOrEqual(3);

    // 先頭が参画中
    expect(testItems[0].isActive).toBe(true);
    expect(testItems[0].projectName).toBe('順序テスト_参画中');

    // 残りは StartDate 降順
    expect(testItems[1].startDate).toBe('2024-09-01');
    expect(testItems[2].startDate).toBe('2023-03-01');
  });

  test.afterAll(async ({ request }) => {
    for (const id of ids) {
      await request.delete(`${API}/project-assignments/${id}`, { headers: { 'X-User-Id': talentId } });
    }
  });
});

// ── AC-08: 上長コメント ───────────────────────────────────────────────────

test.describe('AC-08: 上長コメント', () => {
  let assignmentId = '';

  test.beforeAll(async ({ request }) => {
    const res = await request.post(`${API}/project-assignments`, {
      data: {
        projectName: '上長コメントテスト用プロジェクト',
        role: 'テックリード',
        startDate: '2024-04-01',
        endDate: null,
        description: null,
        skillItemIds: [],
      },
      headers: { 'X-User-Id': talentId },
    });
    assignmentId = (await res.json()).id;
  });

  // AC-08-01: 上長（Manager）がコメントを追加できる
  test('AC-08-01: 上長（Manager）がプロジェクトにコメントを追加できる（200）', async ({ request }) => {
    if (!assignmentId) test.skip(true, 'assignmentId が未取得');
    const res = await request.put(`${API}/project-assignments/${assignmentId}/manager-comment`, {
      data: { comment: 'アーキテクチャ設計において主体的にリードし高い貢献を発揮した' },
      headers: { 'X-User-Id': managerId },
    });
    expect(res.status()).toBe(200);
    const body = await res.json();
    expect(body.managerComment).toBe('アーキテクチャ設計において主体的にリードし高い貢献を発揮した');
  });

  // コメント後に一覧でも確認できる
  test('AC-08-01: コメント後に一覧取得でも managerComment が含まれる', async ({ request }) => {
    if (!assignmentId) test.skip(true, 'assignmentId が未取得');
    const res = await request.get(`${API}/project-assignments?talentId=${talentId}`, {
      headers: { 'X-User-Id': managerId },
    });
    expect(res.status()).toBe(200);
    const body: Array<{ id: string; managerComment: string | null }> = await res.json();
    const found = body.find(a => a.id === assignmentId);
    expect(found).toBeDefined();
    expect(found!.managerComment).toBe('アーキテクチャ設計において主体的にリードし高い貢献を発揮した');
  });

  // タレント本人はコメントできない（403）
  test('AC-08-01: Talent がコメントしようとすると 403', async ({ request }) => {
    if (!assignmentId) test.skip(true, 'assignmentId が未取得');
    const res = await request.put(`${API}/project-assignments/${assignmentId}/manager-comment`, {
      data: { comment: '不正コメント' },
      headers: { 'X-User-Id': talentId },
    });
    expect(res.status()).toBe(403);
  });

  test.afterAll(async ({ request }) => {
    if (assignmentId)
      await request.delete(`${API}/project-assignments/${assignmentId}`, { headers: { 'X-User-Id': talentId } });
  });
});

// ── バリデーション ────────────────────────────────────────────────────────

test.describe('バリデーション', () => {
  test('必須項目（projectName）が空の場合 400 を返す', async ({ request }) => {
    const res = await request.post(`${API}/project-assignments`, {
      data: {
        projectName: '',
        role: 'エンジニア',
        startDate: '2024-01-01',
        endDate: null,
        description: null,
        skillItemIds: [],
      },
      headers: { 'X-User-Id': talentId },
    });
    expect(res.status()).toBe(400);
  });

  test('EndDate が StartDate より前の場合 400 を返す', async ({ request }) => {
    const res = await request.post(`${API}/project-assignments`, {
      data: {
        projectName: 'バリデーションテスト',
        role: 'エンジニア',
        startDate: '2024-06-01',
        endDate: '2024-05-01',
        description: null,
        skillItemIds: [],
      },
      headers: { 'X-User-Id': talentId },
    });
    expect(res.status()).toBe(400);
  });

  test('存在しない skillItemId を指定すると 400 を返す', async ({ request }) => {
    const res = await request.post(`${API}/project-assignments`, {
      data: {
        projectName: 'バリデーションテスト2',
        role: 'エンジニア',
        startDate: '2024-01-01',
        endDate: null,
        description: null,
        skillItemIds: ['00000000-0000-0000-0000-000000000000'],
      },
      headers: { 'X-User-Id': talentId },
    });
    expect(res.status()).toBe(400);
  });
});
