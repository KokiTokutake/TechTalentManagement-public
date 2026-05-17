/* =====================================================
   SDD Project Manager — data.js
   データ管理（CRUD + localStorage）
   ===================================================== */

'use strict';

window.SDD = window.SDD || {};

/* ── STORAGE KEY / VERSION ── */
const STORAGE_KEY = 'sdd-pm-data';
const DATA_VERSION = 2;

/* ── DEMO SEED DATA ── */
const SEED_DATA = {
  _version: DATA_VERSION,
  project: {
    name: 'サンプルプロジェクト',
    description: 'SDD ハーネスで管理するデモプロジェクト',
    members: ['@alice', '@bob', '@carol'],
    specPath: 'docs/spec/',
    constitutionPath: '.constitution/Constitution.md'
  },
  specs: [
    {
      id: 'SPEC-AUTH-001',
      title: 'ログイン機能',
      status: 'approved',
      category: 'AUTH',
      priority: 'Must',
      owner: '@alice',
      file: 'docs/spec/auth/spec-auth-001-login.md',
      description: 'メールアドレスとパスワードによるログイン機能。',
      acceptanceCriteria: [
        'Given 正しいメールとパスワード When ログインボタン押下 Then ダッシュボードへ遷移する',
        'Given 誤ったパスワード When ログインボタン押下 Then エラーメッセージを表示する',
        'Given 5回連続失敗 When ログイン試行 Then 30分アカウントロックされる'
      ],
      taskIds: ['TASK-001', 'TASK-002', 'TASK-004'],
      createdAt: '2024-01-01',
      updatedAt: '2024-01-10'
    },
    {
      id: 'SPEC-DASH-001',
      title: 'ダッシュボード表示',
      status: 'approved',
      category: 'UI',
      priority: 'Must',
      owner: '@bob',
      file: 'docs/spec/ui/spec-dash-001-dashboard.md',
      description: 'ログイン後に表示するダッシュボード。KPIとアクティビティを表示する。',
      acceptanceCriteria: [
        'Given ログイン済みユーザー When ダッシュボードにアクセス Then KPI カードが表示される',
        'Given アクティビティ When 10件以上存在する Then 最新10件のみ表示する'
      ],
      taskIds: ['TASK-003', 'TASK-005'],
      createdAt: '2024-01-05',
      updatedAt: '2024-01-22'
    },
    {
      id: 'SPEC-NOTIF-001',
      title: 'プッシュ通知',
      status: 'review',
      category: 'INFRA',
      priority: 'Could',
      owner: '@carol',
      file: 'docs/spec/infra/spec-notif-001-push.md',
      description: 'タスク更新・コメントに対するプッシュ通知。',
      acceptanceCriteria: [
        'Given タスクが更新された When 担当者がいる Then 担当者に通知を送る'
      ],
      taskIds: [],
      createdAt: '2024-01-12',
      updatedAt: '2024-01-24'
    },
    {
      id: 'SPEC-PROF-001',
      title: 'ユーザープロフィール編集',
      status: 'approved',
      category: 'USER',
      priority: 'Should',
      owner: '@bob',
      file: 'docs/spec/user/spec-prof-001-profile.md',
      description: 'アバター・表示名・メールアドレスの変更機能。',
      acceptanceCriteria: [
        'Given ログイン済みユーザー When プロフィールページへ遷移 Then 現在の情報が表示される',
        'Given 有効な入力 When 保存ボタン押下 Then 変更が即時反映される',
        'Given 不正なメール形式 When 保存ボタン押下 Then バリデーションエラーを表示する'
      ],
      taskIds: ['TASK-006', 'TASK-007'],
      createdAt: '2024-01-22',
      updatedAt: '2024-01-25'
    },
    {
      id: 'SPEC-SEARCH-001',
      title: 'グローバル検索',
      status: 'draft',
      category: 'UI',
      priority: 'Should',
      owner: '@alice',
      file: 'docs/spec/ui/spec-search-001-global-search.md',
      description: 'ユーザー・タスク・ドキュメントを横断検索する機能。',
      acceptanceCriteria: [
        'Given 検索キーワード When 3文字以上入力 Then リアルタイムサジェストを表示する'
      ],
      taskIds: ['TASK-008'],
      createdAt: '2024-01-26',
      updatedAt: '2024-01-26'
    }
  ],
  tasks: [
    {
      id: 'TASK-001',
      title: 'ログイン API 実装',
      specId: 'SPEC-AUTH-001',
      sprintId: 'SPRINT-001',
      assignee: '@alice',
      status: 'done',
      sp: 3,
      issueNumber: 10,
      branch: 'feat/10-login-api',
      pr: 11,
      notes: 'JWT 方式で実装済み',
      createdAt: '2024-01-08',
      updatedAt: '2024-01-15'
    },
    {
      id: 'TASK-002',
      title: 'ログイン画面 UI',
      specId: 'SPEC-AUTH-001',
      sprintId: 'SPRINT-001',
      assignee: '@bob',
      status: 'done',
      sp: 2,
      issueNumber: 12,
      branch: 'feat/12-login-ui',
      pr: 13,
      notes: '',
      createdAt: '2024-01-08',
      updatedAt: '2024-01-16'
    },
    {
      id: 'TASK-003',
      title: 'ダッシュボード コンポーネント実装',
      specId: 'SPEC-DASH-001',
      sprintId: 'SPRINT-001',
      assignee: '@carol',
      status: 'done',
      sp: 5,
      issueNumber: 14,
      branch: 'feat/14-dashboard',
      pr: 18,
      notes: 'Chart.js を採用',
      createdAt: '2024-01-10',
      updatedAt: '2024-01-19'
    },
    {
      id: 'TASK-004',
      title: 'ログアウト機能実装',
      specId: 'SPEC-AUTH-001',
      sprintId: 'SPRINT-001',
      assignee: '@alice',
      status: 'done',
      sp: 1,
      issueNumber: 15,
      branch: 'feat/15-logout',
      pr: 16,
      notes: '',
      createdAt: '2024-01-15',
      updatedAt: '2024-01-18'
    },
    {
      id: 'TASK-005',
      title: 'ダッシュボード API エンドポイント',
      specId: 'SPEC-DASH-001',
      sprintId: 'SPRINT-001',
      assignee: '@bob',
      status: 'done',
      sp: 3,
      issueNumber: 17,
      branch: 'feat/17-dashboard-api',
      pr: 19,
      notes: '',
      createdAt: '2024-01-16',
      updatedAt: '2024-01-19'
    },
    {
      id: 'TASK-006',
      title: 'プロフィール編集 UI',
      specId: 'SPEC-PROF-001',
      sprintId: 'SPRINT-002',
      assignee: '@bob',
      status: 'in_progress',
      sp: 3,
      issueNumber: 20,
      branch: 'feat/20-profile-ui',
      pr: null,
      notes: 'アバターアップロードのUIデザイン検討中',
      createdAt: '2024-01-22',
      updatedAt: '2024-01-27'
    },
    {
      id: 'TASK-007',
      title: 'プロフィール更新 API',
      specId: 'SPEC-PROF-001',
      sprintId: 'SPRINT-002',
      assignee: '@alice',
      status: 'review',
      sp: 2,
      issueNumber: 21,
      branch: 'feat/21-profile-api',
      pr: 22,
      notes: '',
      createdAt: '2024-01-22',
      updatedAt: '2024-01-28'
    },
    {
      id: 'TASK-008',
      title: 'グローバル検索 プロトタイプ',
      specId: 'SPEC-SEARCH-001',
      sprintId: 'SPRINT-002',
      assignee: '@carol',
      status: 'pending',
      sp: 5,
      issueNumber: 23,
      branch: '',
      pr: null,
      notes: '仕様書レビュー完了次第着手',
      createdAt: '2024-01-26',
      updatedAt: '2024-01-26'
    }
  ],
  sprints: [
    {
      id: 'SPRINT-001',
      name: 'Sprint 1',
      goal: 'ログイン機能とダッシュボード基盤の完成',
      startDate: '2024-01-08',
      endDate: '2024-01-19',
      status: 'completed',
      taskIds: ['TASK-001', 'TASK-002', 'TASK-003', 'TASK-004', 'TASK-005'],
      retroId: 'RETRO-001'
    },
    {
      id: 'SPRINT-002',
      name: 'Sprint 2',
      goal: 'プロフィール編集機能リリースとグローバル検索の仕様確定',
      startDate: '2024-01-22',
      endDate: '2024-02-02',
      status: 'active',
      taskIds: ['TASK-006', 'TASK-007', 'TASK-008'],
      retroId: null
    }
  ],
  retros: [
    {
      id: 'RETRO-001',
      sprintId: 'SPRINT-001',
      date: '2024-01-20',
      facilitator: '@alice',
      participants: ['@alice', '@bob', '@carol'],
      keeps: [
        'スプリント開始前に仕様書を全員でレビューしたことで、認識齟齬がゼロだった',
        'PR レビューを 24 時間以内に実施できた（平均 18 分）',
        '毎日のスタンドアップが 15 分以内に収まった'
      ],
      problems: [
        'TASK-003 のグラフライブラリ選定が遅れ、後半に集中した',
        'Safari バグ（ISSUE-001）の発覚が遅く、SP 内で解消できなかった',
        'ダッシュボード仕様の KPI 定義が曖昧で途中で確認が必要だった（ISSUE-002）'
      ],
      tries: [
        'スプリント計画時に「技術的不確実性」タスクを先に洗い出し、スパイクを設ける',
        '仕様書の受入基準に「ブラウザ互換性」を必ず明記するルールを追加する',
        'KPI 定義など数値系の仕様は承認前に PO がレビューするフローを作る'
      ],
      metrics: {
        completedSP: 11,
        plannedSP: 14,
        bugCount: 1,
        avgReviewMinutes: 18,
        specFirstRate: 100,
        ciPassRate: 92
      },
      actionItems: [
        { id: 'AI-001', text: '仕様書テンプレートに「ブラウザ互換性」セクションを追加する', owner: '@alice', dueDate: '2024-01-24', done: true },
        { id: 'AI-002', text: 'ISSUE-001（Safari バグ）を SPRINT-002 に持ち越してクローズする', owner: '@alice', dueDate: '2024-01-26', done: false },
        { id: 'AI-003', text: 'スプリント計画にスパイクタスク枠（1 SP）を設ける運用ルール策定', owner: '@bob', dueDate: '2024-01-24', done: true }
      ]
    }
  ],
  issues: [
    {
      id: 'ISSUE-001',
      title: 'ログイン後リダイレクトが稀に失敗する',
      type: 'bug',
      priority: 'High',
      status: 'open',
      specId: 'SPEC-AUTH-001',
      taskId: 'TASK-001',
      description: 'Safari でのみ再現。原因調査中。',
      resolution: '',
      reporter: '@alice',
      createdAt: '2024-01-15',
      resolvedAt: null
    },
    {
      id: 'ISSUE-002',
      title: 'ダッシュボード仕様の KPI 定義が不明確',
      type: 'question',
      priority: 'Medium',
      status: 'in_progress',
      specId: 'SPEC-DASH-001',
      taskId: '',
      description: '「直近アクティビティ」の期間定義が仕様書に記載なし。',
      resolution: '',
      reporter: '@carol',
      createdAt: '2024-01-16',
      resolvedAt: null
    }
  ],
  activityLog: [
    { timestamp: '2024-01-28T11:00:00', type: 'task_updated',    text: 'TASK-007 を review に移動しました', user: '@alice' },
    { timestamp: '2024-01-27T14:30:00', type: 'issue_created',   text: 'ISSUE-002 を in_progress に更新しました', user: '@bob' },
    { timestamp: '2024-01-26T10:00:00', type: 'spec_created',    text: 'SPEC-SEARCH-001 を作成しました', user: '@alice' },
    { timestamp: '2024-01-26T09:00:00', type: 'task_created',    text: 'TASK-008 を作成しました', user: '@carol' },
    { timestamp: '2024-01-22T09:00:00', type: 'sprint_started',  text: 'SPRINT-002 を開始しました', user: '@alice' },
    { timestamp: '2024-01-22T09:00:00', type: 'spec_created',    text: 'SPEC-PROF-001 を作成しました', user: '@bob' },
    { timestamp: '2024-01-20T15:00:00', type: 'retro_created',   text: 'RETRO-001（Sprint 1 ふりかえり）を作成しました', user: '@alice' },
    { timestamp: '2024-01-19T17:00:00', type: 'sprint_completed', text: 'SPRINT-001 を完了しました（11 / 14 SP）', user: '@alice' },
    { timestamp: '2024-01-19T16:00:00', type: 'task_updated',    text: 'TASK-005 を done に移動しました', user: '@bob' },
    { timestamp: '2024-01-19T15:00:00', type: 'task_updated',    text: 'TASK-003 を done に移動しました', user: '@carol' },
    { timestamp: '2024-01-16T15:30:00', type: 'issue_created',   text: 'ISSUE-002 を作成しました', user: '@carol' },
    { timestamp: '2024-01-16T10:00:00', type: 'task_created',    text: 'TASK-005 を作成しました', user: '@bob' },
    { timestamp: '2024-01-15T14:00:00', type: 'issue_created',   text: 'ISSUE-001 を作成しました', user: '@alice' },
    { timestamp: '2024-01-15T09:00:00', type: 'task_updated',    text: 'TASK-004 を review に移動しました', user: '@alice' },
    { timestamp: '2024-01-12T11:00:00', type: 'spec_updated',    text: 'SPEC-DASH-001 を review に更新しました', user: '@bob' },
    { timestamp: '2024-01-10T09:00:00', type: 'task_created',    text: 'TASK-003 を作成しました', user: '@carol' },
    { timestamp: '2024-01-08T09:00:00', type: 'sprint_started',  text: 'SPRINT-001 を開始しました', user: '@alice' },
    { timestamp: '2024-01-05T10:00:00', type: 'spec_created',    text: 'SPEC-DASH-001 を作成しました', user: '@bob' },
    { timestamp: '2024-01-01T09:00:00', type: 'spec_created',    text: 'SPEC-AUTH-001 を作成しました', user: '@alice' }
  ]
};

/* ── DATA STORE ── */
window.SDD.store = {
  data: null,

  load() {
    try {
      const raw = localStorage.getItem(STORAGE_KEY);
      if (raw) {
        const parsed = JSON.parse(raw);
        if ((parsed._version || 0) < DATA_VERSION) {
          console.info(`SDD: data version ${parsed._version||0} → ${DATA_VERSION}, resetting to seed.`);
          this.data = this._buildInitialData();
        } else {
          this.data = parsed;
        }
      } else {
        this.data = this._buildInitialData();
      }
      this.save();
    } catch(e) {
      console.warn('SDD: localStorage parse error, resetting.', e);
      this.data = this._buildInitialData();
      this.save();
    }
  },

  /* init-config.js が存在する場合はその設定を優先して初期データを構築する */
  _buildInitialData() {
    const cfg = window.SDD_INIT_CONFIG;
    if (!cfg) return JSON.parse(JSON.stringify(SEED_DATA));

    const base = JSON.parse(JSON.stringify(SEED_DATA));
    base.project = Object.assign({}, base.project, cfg.project);
    if (cfg.initialSpecs)   base.specs   = cfg.initialSpecs;
    if (cfg.initialTasks)   base.tasks   = cfg.initialTasks;
    if (cfg.initialSprints) base.sprints = cfg.initialSprints;
    base.retros = [];
    base.issues = [];
    base.activityLog = [
      { timestamp: new Date().toISOString(), type: 'project_init', text: 'プロジェクトを初期化しました（/pm-setup）', user: cfg.project.members[0] || '@owner' }
    ];
    base._version = DATA_VERSION;
    return base;
  },

  save() {
    localStorage.setItem(STORAGE_KEY, JSON.stringify(this.data));
    // Auto-sync to project files if connected
    if (window.SDD && window.SDD.fs && window.SDD.fs.dirHandle) {
      window.SDD.fs.autoSync().catch(() => {});
    }
  },

  reset() {
    this.data = this._buildInitialData();
    localStorage.setItem(STORAGE_KEY, JSON.stringify(this.data));
    if (window.SDD && window.SDD.fs && window.SDD.fs.dirHandle) {
      window.SDD.fs.autoSync().catch(() => {});
    }
  },

  /* ── helpers ── */
  nextId(prefix, list) {
    const nums = list
      .map(x => parseInt((x.id || '').replace(prefix + '-', ''), 10))
      .filter(n => !isNaN(n));
    const max = nums.length ? Math.max(...nums) : 0;
    return `${prefix}-${String(max + 1).padStart(3, '0')}`;
  },

  now() { return new Date().toISOString(); },
  today() { return new Date().toISOString().slice(0, 10); },

  log(type, text, user) {
    user = user || (this.data.project.members[0] || '@user');
    this.data.activityLog.unshift({
      timestamp: this.now(),
      type,
      text,
      user
    });
    if (this.data.activityLog.length > 200) {
      this.data.activityLog = this.data.activityLog.slice(0, 200);
    }
  },

  /* ── SPEC CRUD ── */
  createSpec(fields) {
    const id = fields.id || this.nextId('SPEC', this.data.specs);
    const spec = {
      id,
      title: fields.title || '',
      status: fields.status || 'draft',
      category: fields.category || '',
      priority: fields.priority || 'Should',
      owner: fields.owner || '',
      file: fields.file || '',
      description: fields.description || '',
      acceptanceCriteria: fields.acceptanceCriteria || [],
      taskIds: [],
      createdAt: this.today(),
      updatedAt: this.today()
    };
    this.data.specs.push(spec);
    this.log('spec_created', `${id} を作成しました`, fields.owner);
    this.save();
    return spec;
  },

  updateSpec(id, fields) {
    const idx = this.data.specs.findIndex(s => s.id === id);
    if (idx < 0) return null;
    Object.assign(this.data.specs[idx], fields, { updatedAt: this.today() });
    this.log('spec_updated', `${id} を更新しました`);
    this.save();
    return this.data.specs[idx];
  },

  deleteSpec(id) {
    const idx = this.data.specs.findIndex(s => s.id === id);
    if (idx < 0) return false;
    this.data.specs.splice(idx, 1);
    this.log('spec_deleted', `${id} を削除しました`);
    this.save();
    return true;
  },

  /* ── TASK CRUD ── */
  createTask(fields) {
    const id = this.nextId('TASK', this.data.tasks);
    const task = {
      id,
      title: fields.title || '',
      specId: fields.specId || '',
      sprintId: fields.sprintId || '',
      assignee: fields.assignee || '',
      status: fields.status || 'pending',
      sp: Number(fields.sp) || 1,
      issueNumber: fields.issueNumber ? Number(fields.issueNumber) : null,
      branch: fields.branch || '',
      pr: fields.pr ? Number(fields.pr) : null,
      notes: fields.notes || '',
      createdAt: this.today(),
      updatedAt: this.today()
    };
    this.data.tasks.push(task);
    // link spec
    if (task.specId) {
      const spec = this.data.specs.find(s => s.id === task.specId);
      if (spec && !spec.taskIds.includes(id)) spec.taskIds.push(id);
    }
    // link sprint
    if (task.sprintId) {
      const sprint = this.data.sprints.find(s => s.id === task.sprintId);
      if (sprint && !sprint.taskIds.includes(id)) sprint.taskIds.push(id);
    }
    this.log('task_created', `${id} を作成しました`, fields.assignee);
    this.save();
    return task;
  },

  updateTask(id, fields) {
    const idx = this.data.tasks.findIndex(t => t.id === id);
    if (idx < 0) return null;
    const old = this.data.tasks[idx];

    // unlink old spec/sprint if changed
    if (fields.specId !== undefined && fields.specId !== old.specId) {
      const oldSpec = this.data.specs.find(s => s.id === old.specId);
      if (oldSpec) oldSpec.taskIds = oldSpec.taskIds.filter(tid => tid !== id);
      const newSpec = this.data.specs.find(s => s.id === fields.specId);
      if (newSpec && !newSpec.taskIds.includes(id)) newSpec.taskIds.push(id);
    }
    if (fields.sprintId !== undefined && fields.sprintId !== old.sprintId) {
      const oldSp = this.data.sprints.find(s => s.id === old.sprintId);
      if (oldSp) oldSp.taskIds = oldSp.taskIds.filter(tid => tid !== id);
      const newSp = this.data.sprints.find(s => s.id === fields.sprintId);
      if (newSp && !newSp.taskIds.includes(id)) newSp.taskIds.push(id);
    }

    if (fields.sp !== undefined) fields.sp = Number(fields.sp);
    Object.assign(this.data.tasks[idx], fields, { updatedAt: this.today() });

    if (fields.status) {
      this.log('task_updated', `${id} を ${fields.status} に移動しました`);
    } else {
      this.log('task_updated', `${id} を更新しました`);
    }
    this.save();
    return this.data.tasks[idx];
  },

  deleteTask(id) {
    const idx = this.data.tasks.findIndex(t => t.id === id);
    if (idx < 0) return false;
    const task = this.data.tasks[idx];
    // unlink
    const spec = this.data.specs.find(s => s.id === task.specId);
    if (spec) spec.taskIds = spec.taskIds.filter(tid => tid !== id);
    const sprint = this.data.sprints.find(s => s.id === task.sprintId);
    if (sprint) sprint.taskIds = sprint.taskIds.filter(tid => tid !== id);
    this.data.tasks.splice(idx, 1);
    this.log('task_deleted', `${id} を削除しました`);
    this.save();
    return true;
  },

  /* ── SPRINT CRUD ── */
  createSprint(fields) {
    const id = this.nextId('SPRINT', this.data.sprints);
    const sprint = {
      id,
      name: fields.name || id,
      goal: fields.goal || '',
      startDate: fields.startDate || this.today(),
      endDate: fields.endDate || '',
      status: 'planned',
      taskIds: [],
      retroId: null
    };
    this.data.sprints.push(sprint);
    this.log('sprint_created', `${id} を作成しました`);
    this.save();
    return sprint;
  },

  activateSprint(id) {
    // deactivate others
    this.data.sprints.forEach(s => { if (s.status === 'active') s.status = 'completed'; });
    const sprint = this.data.sprints.find(s => s.id === id);
    if (!sprint) return null;
    sprint.status = 'active';
    sprint.startDate = this.today();
    this.log('sprint_started', `${id} を開始しました`);
    this.save();
    return sprint;
  },

  completeSprint(id) {
    const sprint = this.data.sprints.find(s => s.id === id);
    if (!sprint) return null;
    sprint.status = 'completed';
    this.log('sprint_completed', `${id} を終了しました`);
    this.save();
    return sprint;
  },

  updateSprint(id, fields) {
    const idx = this.data.sprints.findIndex(s => s.id === id);
    if (idx < 0) return null;
    Object.assign(this.data.sprints[idx], fields);
    this.save();
    return this.data.sprints[idx];
  },

  /* ── RETRO CRUD ── */
  createRetro(fields) {
    const id = this.nextId('RETRO', this.data.retros);
    const retro = {
      id,
      sprintId: fields.sprintId || '',
      date: fields.date || this.today(),
      facilitator: fields.facilitator || '',
      participants: fields.participants || [],
      keeps: [],
      problems: [],
      tries: [],
      metrics: {
        completedSP: 0,
        plannedSP: 0,
        bugCount: 0,
        avgReviewMinutes: 0,
        specFirstRate: 0,
        ciPassRate: 0
      },
      actionItems: []
    };
    this.data.retros.push(retro);
    // link sprint
    const sprint = this.data.sprints.find(s => s.id === retro.sprintId);
    if (sprint) sprint.retroId = id;
    this.log('retro_created', `${id} を作成しました`);
    this.save();
    return retro;
  },

  updateRetro(id, fields) {
    const idx = this.data.retros.findIndex(r => r.id === id);
    if (idx < 0) return null;
    Object.assign(this.data.retros[idx], fields);
    this.log('retro_updated', `${id} を更新しました`);
    this.save();
    return this.data.retros[idx];
  },

  /* ── ISSUE CRUD ── */
  createIssue(fields) {
    const id = this.nextId('ISSUE', this.data.issues);
    const issue = {
      id,
      title: fields.title || '',
      type: fields.type || 'question',
      priority: fields.priority || 'Medium',
      status: 'open',
      specId: fields.specId || '',
      taskId: fields.taskId || '',
      description: fields.description || '',
      resolution: '',
      reporter: fields.reporter || '',
      createdAt: this.today(),
      resolvedAt: null
    };
    this.data.issues.push(issue);
    this.log('issue_created', `${id} を作成しました`, fields.reporter);
    this.save();
    return issue;
  },

  updateIssue(id, fields) {
    const idx = this.data.issues.findIndex(i => i.id === id);
    if (idx < 0) return null;
    if (fields.status === 'resolved' && !this.data.issues[idx].resolvedAt) {
      fields.resolvedAt = this.today();
    }
    Object.assign(this.data.issues[idx], fields);
    this.log('issue_updated', `${id} を更新しました`);
    this.save();
    return this.data.issues[idx];
  },

  deleteIssue(id) {
    const idx = this.data.issues.findIndex(i => i.id === id);
    if (idx < 0) return false;
    this.data.issues.splice(idx, 1);
    this.log('issue_deleted', `${id} を削除しました`);
    this.save();
    return true;
  },

  /* ── PROJECT ── */
  updateProject(fields) {
    Object.assign(this.data.project, fields);
    this.save();
  },

  /* ── EXPORT / IMPORT ── */
  exportJSON() {
    return JSON.stringify(this.data, null, 2);
  },

  importJSON(jsonStr) {
    const parsed = JSON.parse(jsonStr);
    this.data = parsed;
    this.save();
  },

  /* ── COMPUTED ── */
  getActiveSprint() {
    return this.data.sprints.find(s => s.status === 'active') || null;
  },

  getSprintTasks(sprintId) {
    return this.data.tasks.filter(t => t.sprintId === sprintId);
  },

  getRemainingDays(endDate) {
    if (!endDate) return null;
    const diff = new Date(endDate) - new Date();
    return Math.max(0, Math.ceil(diff / 86400000));
  },

  calcSddScore() {
    const specs = this.data.specs;
    if (!specs.length) return 0;
    const approved = specs.filter(s => s.status === 'approved').length;
    const specFirstTasks = this.data.tasks.filter(t => t.specId).length;
    const totalTasks = this.data.tasks.length || 1;
    const specFirst = Math.round((specFirstTasks / totalTasks) * 100);
    const approvalRate = Math.round((approved / specs.length) * 100);
    return Math.round((specFirst * 0.5) + (approvalRate * 0.5));
  }
};
