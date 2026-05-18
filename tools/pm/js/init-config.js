/* =====================================================
   SDD Project Manager — init-config.js
   プロジェクト固有の初期設定（/pm-setup により生成）
   このファイルを編集してプロジェクト設定を変更する。
   data.js の SEED_DATA より優先して適用される。
   ===================================================== */

window.SDD_INIT_CONFIG = {
  project: {
    name: 'ITエンジニア向けタレントマネジメントシステム',
    description: 'Will-Can-Mustフレームワークと人材開発委員会運営をベースとした考課・タレントマネジメントシステム。金融系グループ会社の社内ITエンジニア向け。',
    members: ['@KokiTokutake'],
    specPath: 'docs/spec/',
    constitutionPath: '.constitution/Constitution.md'
  },
  sprintDurationWeeks: 1,

  /* 初期スプリント */
  initialSprints: [
    {
      id: 'SPRINT-001',
      name: 'Sprint 1',
      goal: '骨格・認証・DB・目標設定API・自己評価API（Phase 1 前半）',
      startDate: '2026-05-17',
      endDate: '2026-05-23',
      status: 'completed',
      taskIds: ['TASK-001','TASK-002','TASK-003','TASK-004','TASK-005'],
      retroId: null
    },
    {
      id: 'SPRINT-002',
      name: 'Sprint 2',
      goal: '上長評価API・FE3画面・Phase 1 結合テスト（Phase 1 後半）',
      startDate: '2026-05-24',
      endDate: '2026-05-30',
      status: 'completed',
      taskIds: ['TASK-006','TASK-007','TASK-008','TASK-009','TASK-010'],
      retroId: null
    },
    {
      id: 'SPRINT-003',
      name: 'Sprint 3',
      goal: 'スキル管理機能 バックエンド全実装',
      startDate: '2026-05-31',
      endDate: '2026-06-06',
      status: 'active',
      taskIds: ['TASK-011','TASK-012','TASK-013','TASK-014'],
      retroId: null
    },
    {
      id: 'SPRINT-004',
      name: 'Sprint 4',
      goal: 'スキル管理機能 フロントエンド全画面・結合テスト',
      startDate: '2026-06-07',
      endDate: '2026-06-13',
      status: 'planning',
      taskIds: ['TASK-015','TASK-016','TASK-017','TASK-018','TASK-019'],
      retroId: null
    }
  ],

  /* 初期仕様書 */
  initialSpecs: [
    {
      id: 'SPEC-EVAL-001',
      title: 'エンジニア評価・考課ワークフロー',
      status: 'approved',
      category: 'EVAL',
      priority: 'Must',
      owner: '@KokiTokutake',
      file: 'docs/spec/SPEC-EVAL-001-evaluation-workflow.md',
      description: '目標設定（期初）→ 中間レビュー（期中）→ 自己評価・上長評価（期末）→ 層別承認 → フィードバック開示の通年考課ワークフロー。',
      acceptanceCriteria: [
        'Given タレントがログイン済みで目標登録期間内 When Will/Can/Mustで目標を入力し保存 Then 目標が保存され上長の一覧に表示される',
        'Given タレントがログイン済みで評価入力期間内 When 自己評価を入力し提出 Then 上長に提出され読み取り専用になる',
        'Given 上長がログイン済みでタレントの自己評価が提出済み When 上長評価を確定 Then 承認ワークフローが開始される',
        'Given タレントAがログイン済み When タレントBの評価ページに直接アクセス Then 403エラーが返る'
      ],
      taskIds: ['TASK-001','TASK-002','TASK-003','TASK-004','TASK-005','TASK-006','TASK-007','TASK-008','TASK-009','TASK-010'],
      createdAt: '2026-05-17',
      updatedAt: '2026-05-17'
    },
    {
      id: 'SPEC-SKILL-001',
      title: 'スキル・技術レベル管理',
      status: 'approved',
      category: 'SKILL',
      priority: 'Must',
      owner: '@KokiTokutake',
      file: 'docs/spec/SPEC-SKILL-001-skill-management.md',
      description: 'エンジニアのスキル・技術レベルを登録・管理し、上長承認と組織スキルマップ閲覧を実現する機能。',
      acceptanceCriteria: [
        'Given タレントがスキル管理ページを開いている When スキル種別・レベルを選択して保存 Then スキルが申請中状態で登録される',
        'Given 部下が申請中のスキルを持っている When 上長が承認操作を実行 Then スキルの状態が承認済みになり組織スキルマップに反映される',
        'Given 管理者がログインしている When スキルカテゴリを新規追加 Then カテゴリが作成されスキル追加フォームで選択できる',
        'Given 1件以上のタレントが使用中のスキル種別がある When 管理者がそのスキル種別の削除操作を実行 Then 使用中のスキルがありますエラーが表示され削除できない',
        'Given 管理職以上のユーザーがログインしている When 組織スキルマップページを開く Then 全エンジニアの承認済みスキルがカテゴリ別に表示される'
      ],
      taskIds: ['TASK-011','TASK-012','TASK-013','TASK-014','TASK-015','TASK-016','TASK-017','TASK-018','TASK-019'],
      createdAt: '2026-05-17',
      updatedAt: '2026-05-17'
    }
  ],

  /* 初期タスク */
  initialTasks: [
    {
      id: 'TASK-001',
      title: '[INFRA] Nuxt.js + ASP.NET Core プロジェクト骨格セットアップ',
      specId: 'SPEC-EVAL-001',
      sprintId: 'SPRINT-001',
      assignee: '@KokiTokutake',
      status: 'done',
      sp: 3,
      issueNumber: null,
      branch: 'feat/T-001-project-scaffold',
      pr: null,
      notes: 'docker compose up でFE・BE・DBが起動することが完了条件',
      createdAt: '2026-05-17',
      updatedAt: '2026-05-17'
    },
    {
      id: 'TASK-002',
      title: '[INFRA] Entra ID SSO 認証・ロールベースアクセス制御（BE+FE）',
      specId: 'SPEC-EVAL-001',
      sprintId: 'SPRINT-001',
      assignee: '@KokiTokutake',
      status: 'pending',
      sp: 5,
      issueNumber: null,
      branch: 'feat/T-002-entra-id-auth',
      pr: null,
      notes: '⛔ Azure環境なしのためプライベート開発中は永久スキップ。開発用認証（ユーザー選択UI）で代替済み。',
      createdAt: '2026-05-17',
      updatedAt: '2026-05-17'
    },
    {
      id: 'TASK-003',
      title: '[BE] DBスキーマ設計・EF Core マイグレーション',
      specId: 'SPEC-EVAL-001',
      sprintId: 'SPRINT-001',
      assignee: '@KokiTokutake',
      status: 'done',
      sp: 3,
      issueNumber: null,
      branch: 'feat/T-003-db-schema',
      pr: null,
      notes: 'EvaluationPeriod / Goal / SelfEvaluation / ManagerEvaluation テーブルを定義',
      createdAt: '2026-05-17',
      updatedAt: '2026-05-17'
    },
    {
      id: 'TASK-004',
      title: '[BE] 目標設定 API（CRUD）',
      specId: 'SPEC-EVAL-001',
      sprintId: 'SPRINT-001',
      assignee: '@KokiTokutake',
      status: 'done',
      sp: 3,
      issueNumber: null,
      branch: 'feat/T-004-goal-api',
      pr: null,
      notes: 'REQ-01対応。登録期間外アクセスは403を返すこと。',
      createdAt: '2026-05-17',
      updatedAt: '2026-05-17'
    },
    {
      id: 'TASK-005',
      title: '[BE] 自己評価 API（下書き保存・提出）',
      specId: 'SPEC-EVAL-001',
      sprintId: 'SPRINT-001',
      assignee: '@KokiTokutake',
      status: 'done',
      sp: 3,
      issueNumber: null,
      branch: 'feat/T-005-self-evaluation-api',
      pr: null,
      notes: 'REQ-02対応。提出後は編集不可（409）。',
      createdAt: '2026-05-17',
      updatedAt: '2026-05-17'
    },
    {
      id: 'TASK-006',
      title: '[BE] 上長評価 API（入力・確定）',
      specId: 'SPEC-EVAL-001',
      sprintId: 'SPRINT-002',
      assignee: '@KokiTokutake',
      status: 'done',
      sp: 3,
      issueNumber: null,
      branch: 'feat/T-006-manager-evaluation-api',
      pr: null,
      notes: 'REQ-03対応。自己評価提出済みの場合のみ入力可能。',
      createdAt: '2026-05-17',
      updatedAt: '2026-05-17'
    },
    {
      id: 'TASK-007',
      title: '[FE] 目標設定画面 UI',
      specId: 'SPEC-EVAL-001',
      sprintId: 'SPRINT-002',
      assignee: '@KokiTokutake',
      status: 'done',
      sp: 3,
      issueNumber: null,
      branch: 'feat/T-007-goal-setting-ui',
      pr: null,
      notes: 'REQ-01対応。Will/Can/Must 3カテゴリ表示。',
      createdAt: '2026-05-17',
      updatedAt: '2026-05-17'
    },
    {
      id: 'TASK-008',
      title: '[FE] 自己評価入力画面 UI（自動下書き保存）',
      specId: 'SPEC-EVAL-001',
      sprintId: 'SPRINT-002',
      assignee: '@KokiTokutake',
      status: 'done',
      sp: 5,
      issueNumber: null,
      branch: 'feat/T-008-self-evaluation-ui',
      pr: null,
      notes: 'REQ-02対応。入力3秒後に自動下書き保存（デバウンス）。',
      createdAt: '2026-05-17',
      updatedAt: '2026-05-17'
    },
    {
      id: 'TASK-009',
      title: '[FE] 上長評価入力画面 UI',
      specId: 'SPEC-EVAL-001',
      sprintId: 'SPRINT-002',
      assignee: '@KokiTokutake',
      status: 'done',
      sp: 3,
      issueNumber: null,
      branch: 'feat/T-009-manager-evaluation-ui',
      pr: null,
      notes: 'REQ-03対応。確定後は読み取り専用に切り替え。',
      createdAt: '2026-05-17',
      updatedAt: '2026-05-17'
    },
    {
      id: 'TASK-010',
      title: '[TEST] Phase 1 結合テスト・受入確認',
      specId: 'SPEC-EVAL-001',
      sprintId: 'SPRINT-002',
      assignee: '@KokiTokutake',
      status: 'done',
      sp: 3,
      issueNumber: null,
      branch: 'feat/T-010-phase1-integration-test',
      pr: null,
      notes: 'SPEC-EVAL-001 REQ-01〜03 の受入基準を EF Core InMemory で結合テスト。',
      createdAt: '2026-05-17',
      updatedAt: '2026-05-17'
    },
    {
      id: 'TASK-011',
      title: '[BE] スキルマスター管理 API（SkillCategory / SkillItem CRUD）',
      specId: 'SPEC-SKILL-001',
      sprintId: 'SPRINT-003',
      assignee: '@KokiTokutake',
      status: 'done',
      sp: 3,
      issueNumber: null,
      branch: 'feat/T-011-skill-master-api',
      pr: null,
      notes: 'Admin のみ CRUD 可。使用中の SkillItem 削除は 409。単体テスト 8件。',
      createdAt: '2026-05-17',
      updatedAt: '2026-05-17'
    },
    {
      id: 'TASK-012',
      title: '[BE] ユーザースキル CRUD API（追加・編集・削除・一覧）',
      specId: 'SPEC-SKILL-001',
      sprintId: 'SPRINT-003',
      assignee: '@KokiTokutake',
      status: 'pending',
      sp: 3,
      issueNumber: null,
      branch: null,
      pr: null,
      notes: 'REQ-01〜04対応。自己申告スキルの登録・編集・削除。TASK-011依存。',
      createdAt: '2026-05-17',
      updatedAt: '2026-05-17'
    },
    {
      id: 'TASK-013',
      title: '[BE] スキル承認・差し戻し API',
      specId: 'SPEC-SKILL-001',
      sprintId: 'SPRINT-003',
      assignee: '@KokiTokutake',
      status: 'pending',
      sp: 3,
      issueNumber: null,
      branch: null,
      pr: null,
      notes: 'REQ-05対応。上長が部下のスキル申告を承認・差し戻し。TASK-012依存。',
      createdAt: '2026-05-17',
      updatedAt: '2026-05-17'
    },
    {
      id: 'TASK-014',
      title: '[BE] 組織スキルマップ API',
      specId: 'SPEC-SKILL-001',
      sprintId: 'SPRINT-003',
      assignee: '@KokiTokutake',
      status: 'pending',
      sp: 2,
      issueNumber: null,
      branch: null,
      pr: null,
      notes: 'REQ-08・10対応。承認済みスキルのみ返す。Senior/Executive/Admin のみ閲覧可。TASK-012依存。',
      createdAt: '2026-05-17',
      updatedAt: '2026-05-17'
    },
    {
      id: 'TASK-015',
      title: '[FE] スキルマスター管理画面 UI（Admin）',
      specId: 'SPEC-SKILL-001',
      sprintId: 'SPRINT-004',
      assignee: '@KokiTokutake',
      status: 'pending',
      sp: 2,
      issueNumber: null,
      branch: null,
      pr: null,
      notes: 'REQ-06・07対応。Admin ロール限定画面。TASK-011依存。',
      createdAt: '2026-05-17',
      updatedAt: '2026-05-17'
    },
    {
      id: 'TASK-016',
      title: '[FE] 自分のスキル管理画面 UI（一覧・追加・編集・削除）',
      specId: 'SPEC-SKILL-001',
      sprintId: 'SPRINT-004',
      assignee: '@KokiTokutake',
      status: 'pending',
      sp: 3,
      issueNumber: null,
      branch: null,
      pr: null,
      notes: 'REQ-01〜04対応。カテゴリ別表示・承認状態の視覚化。TASK-012依存。',
      createdAt: '2026-05-17',
      updatedAt: '2026-05-17'
    },
    {
      id: 'TASK-017',
      title: '[FE] スキル承認・差し戻し画面 UI（上長向け）',
      specId: 'SPEC-SKILL-001',
      sprintId: 'SPRINT-004',
      assignee: '@KokiTokutake',
      status: 'pending',
      sp: 3,
      issueNumber: null,
      branch: null,
      pr: null,
      notes: 'REQ-05対応。差し戻し時はコメント必須。TASK-013・016依存。',
      createdAt: '2026-05-17',
      updatedAt: '2026-05-17'
    },
    {
      id: 'TASK-018',
      title: '[FE] 組織スキルマップ画面 UI',
      specId: 'SPEC-SKILL-001',
      sprintId: 'SPRINT-004',
      assignee: '@KokiTokutake',
      status: 'pending',
      sp: 3,
      issueNumber: null,
      branch: null,
      pr: null,
      notes: 'REQ-08対応。エンジニア×スキルのテーブル表示。Senior/Executive/Admin 限定。TASK-014依存。',
      createdAt: '2026-05-17',
      updatedAt: '2026-05-17'
    },
    {
      id: 'TASK-019',
      title: '[TEST] スキル管理 結合テスト・受入確認',
      specId: 'SPEC-SKILL-001',
      sprintId: 'SPRINT-004',
      assignee: '@KokiTokutake',
      status: 'pending',
      sp: 2,
      issueNumber: null,
      branch: null,
      pr: null,
      notes: 'SPEC-SKILL-001 受入基準を結合テストで確認。TASK-013・018依存。',
      createdAt: '2026-05-17',
      updatedAt: '2026-05-17'
    }
  ]
};
