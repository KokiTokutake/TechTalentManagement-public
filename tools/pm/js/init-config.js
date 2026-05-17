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

  /* 初期スプリント（最初にブラウザで開いたときに生成される） */
  initialSprints: [
    {
      id: 'SPRINT-001',
      name: 'Sprint 1',
      goal: '骨格・認証・DB・目標設定API・自己評価API（Phase 1 前半）',
      startDate: '2026-05-17',
      endDate: '2026-05-23',
      status: 'active',
      taskIds: ['TASK-001','TASK-002','TASK-003','TASK-004','TASK-005'],
      retroId: null
    },
    {
      id: 'SPRINT-002',
      name: 'Sprint 2',
      goal: '上長評価API・FE3画面・Phase 1 結合テスト（Phase 1 後半）',
      startDate: '2026-05-24',
      endDate: '2026-05-30',
      status: 'planning',
      taskIds: ['TASK-006','TASK-007','TASK-008','TASK-009','TASK-010'],
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
      description: '目標設定（期初）→ 中間レビュー（期中）→ 自己評価・上長評価（期末）→ 層別承認（上長→管理職→幹部長）→ フィードバック開示の通年考課ワークフロー。',
      acceptanceCriteria: [
        'Given タレントがログイン済みで目標登録期間内 When Will/Can/Mustで目標を入力し保存 Then 目標が保存され上長の一覧に表示される',
        'Given タレントがログイン済みで評価入力期間内 When 自己評価を入力し提出 Then 上長に提出され読み取り専用になる',
        'Given 上長がログイン済みでタレントの自己評価が提出済み When 上長評価を確定 Then 承認ワークフローが開始される',
        'Given 幹部長が最終承認を完了 Then タレントがフィードバックを参照できる',
        'Given タレントAがログイン済み When タレントBの評価ページに直接アクセス Then 403エラーが返る'
      ],
      taskIds: ['TASK-001','TASK-002','TASK-003','TASK-004','TASK-005','TASK-006','TASK-007','TASK-008','TASK-009','TASK-010'],
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
      status: 'pending',
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
      notes: 'Azure Portal でのアプリ登録が必要。Key Vault にシークレットを格納すること。',
      createdAt: '2026-05-17',
      updatedAt: '2026-05-17'
    },
    {
      id: 'TASK-003',
      title: '[BE] DBスキーマ設計・EF Core マイグレーション',
      specId: 'SPEC-EVAL-001',
      sprintId: 'SPRINT-001',
      assignee: '@KokiTokutake',
      status: 'pending',
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
      status: 'pending',
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
      status: 'pending',
      sp: 3,
      issueNumber: null,
      branch: 'feat/T-005-self-evaluation-api',
      pr: null,
      notes: 'REQ-02対応。提出後は編集不可（409）。評価内容はログに出力しないこと。',
      createdAt: '2026-05-17',
      updatedAt: '2026-05-17'
    },
    {
      id: 'TASK-006',
      title: '[BE] 上長評価 API（入力・確定）',
      specId: 'SPEC-EVAL-001',
      sprintId: 'SPRINT-002',
      assignee: '@KokiTokutake',
      status: 'pending',
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
      status: 'pending',
      sp: 3,
      issueNumber: null,
      branch: 'feat/T-007-goal-setting-ui',
      pr: null,
      notes: 'REQ-01対応。登録期間外はフォームを非活性化。',
      createdAt: '2026-05-17',
      updatedAt: '2026-05-17'
    },
    {
      id: 'TASK-008',
      title: '[FE] 自己評価入力画面 UI（自動下書き保存）',
      specId: 'SPEC-EVAL-001',
      sprintId: 'SPRINT-002',
      assignee: '@KokiTokutake',
      status: 'pending',
      sp: 5,
      issueNumber: null,
      branch: 'feat/T-008-self-evaluation-ui',
      pr: null,
      notes: 'REQ-02対応。入力3秒後に自動下書き保存（デバウンス）。保存状態を表示。',
      createdAt: '2026-05-17',
      updatedAt: '2026-05-17'
    },
    {
      id: 'TASK-009',
      title: '[FE] 上長評価入力画面 UI',
      specId: 'SPEC-EVAL-001',
      sprintId: 'SPRINT-002',
      assignee: '@KokiTokutake',
      status: 'pending',
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
      status: 'pending',
      sp: 3,
      issueNumber: null,
      branch: 'feat/T-010-phase1-integration-test',
      pr: null,
      notes: 'SPEC-EVAL-001 §4 の受入基準チェックリストをすべて手動確認。',
      createdAt: '2026-05-17',
      updatedAt: '2026-05-17'
    }
  ]
};
