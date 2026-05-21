import { test, expect } from '@playwright/test';

// SPEC-1ON1-001: 1on1面談記録機能 — テンプレートマスタ API
// 仕様書: docs/spec/SPEC-1ON1-001-one-on-one-meeting.md
// 設計書: docs/design/SPEC-1ON1-001-design.md
// 対象タスク: TASK-044
// 生成日: 2026-05-21

const API_BASE = 'http://localhost:5108/api';

test.describe('SPEC-1ON1-001: テンプレートマスタ API', () => {

  // ── AC-05-01: テンプレート一覧取得 ────────────────────────────────
  // Given: 上長が新規面談記録作成画面を開こうとしている
  // When:  GET /api/one-on-one-templates を呼ぶ
  // Then:  テンプレート一覧（Will-Can-Must振り返り・スキル開発・キャリアプラン）が返る
  test('AC-05-01: GET /api/one-on-one-templates でテンプレート一覧が返る', async ({ request }) => {
    const res = await request.get(`${API_BASE}/one-on-one-templates`);
    expect(res.status()).toBe(200);

    const templates: Array<{
      id: string;
      name: string;
      willPlaceholder: string | null;
      canPlaceholder: string | null;
      mustPlaceholder: string | null;
    }> = await res.json();

    // 3種類のテンプレートが返ること
    expect(Array.isArray(templates)).toBe(true);
    expect(templates.length).toBe(3);

    // 各テンプレートに必須フィールドが含まれること
    for (const t of templates) {
      expect(t.id).toBeDefined();
      expect(typeof t.id).toBe('string');
      expect(t.name).toBeDefined();
      expect(typeof t.name).toBe('string');
    }
  });

  test('テンプレート一覧に Will-Can-Must振り返りが含まれる', async ({ request }) => {
    const res = await request.get(`${API_BASE}/one-on-one-templates`);
    const templates: Array<{ id: string; name: string }> = await res.json();

    const wcm = templates.find(t => t.id === 'will-can-must');
    expect(wcm).toBeDefined();
    expect(wcm!.name).toBe('Will-Can-Must振り返り');
  });

  test('テンプレート一覧にスキル開発が含まれる', async ({ request }) => {
    const res = await request.get(`${API_BASE}/one-on-one-templates`);
    const templates: Array<{ id: string; name: string }> = await res.json();

    const skill = templates.find(t => t.id === 'skill-development');
    expect(skill).toBeDefined();
    expect(skill!.name).toBe('スキル開発');
  });

  test('テンプレート一覧にキャリアプランが含まれる', async ({ request }) => {
    const res = await request.get(`${API_BASE}/one-on-one-templates`);
    const templates: Array<{ id: string; name: string }> = await res.json();

    const career = templates.find(t => t.id === 'career-plan');
    expect(career).toBeDefined();
    expect(career!.name).toBe('キャリアプラン');
  });

  test('各テンプレートに willPlaceholder・canPlaceholder・mustPlaceholder が含まれる', async ({ request }) => {
    const res = await request.get(`${API_BASE}/one-on-one-templates`);
    const templates: Array<{
      id: string;
      willPlaceholder: string | null;
      canPlaceholder: string | null;
      mustPlaceholder: string | null;
    }> = await res.json();

    for (const t of templates) {
      expect(typeof t.willPlaceholder === 'string' || t.willPlaceholder === null).toBe(true);
      expect(typeof t.canPlaceholder === 'string' || t.canPlaceholder === null).toBe(true);
      expect(typeof t.mustPlaceholder === 'string' || t.mustPlaceholder === null).toBe(true);
    }
  });

});
