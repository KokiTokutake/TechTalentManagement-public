# タスク一覧

> 最終更新: 2026-05-20 （/tasks-plan SPEC-MDC-002 + SPEC-1ON1-001 により更新）

---

## Sprint 1 ✅ 完了

- **ゴール**: 骨格・認証・DB・目標設定API・自己評価API（Phase 1 前半）
- **期間**: 2026-05-17 〜 2026-05-23
- **進捗**: 12 / 17 SP 完了

| ID | タイトル | ステータス | 担当者 | SP | 仕様書 | Issue | PR | ブランチ |
|----|---------|-----------|--------|:--:|--------|-------|:--:|----------|
| TASK-001 | [INFRA] Nuxt.js + ASP.NET Core プロジェクト骨格セットアップ | ✅ done | @KokiTokutake | 3 | SPEC-EVAL-001 | — | — | feat/T-001-project-scaffold |
| TASK-002 | [INFRA] Entra ID SSO 認証・ロールベースアクセス制御（BE+FE） | ⬜ pending | @KokiTokutake | 5 | SPEC-EVAL-001 | — | — | feat/T-002-entra-id-auth |
| TASK-003 | [BE] DBスキーマ設計・EF Core マイグレーション | ✅ done | @KokiTokutake | 3 | SPEC-EVAL-001 | — | — | feat/T-003-db-schema |
| TASK-004 | [BE] 目標設定 API（CRUD） | ✅ done | @KokiTokutake | 3 | SPEC-EVAL-001 | — | — | feat/T-004-goal-api |
| TASK-005 | [BE] 自己評価 API（下書き保存・提出） | ✅ done | @KokiTokutake | 3 | SPEC-EVAL-001 | — | — | feat/T-005-self-evaluation-api |

## Sprint 2 ✅ 完了

- **ゴール**: 上長評価API・FE3画面・Phase 1 結合テスト（Phase 1 後半）
- **期間**: 2026-05-24 〜 2026-05-30
- **進捗**: 17 / 17 SP 完了

| ID | タイトル | ステータス | 担当者 | SP | 仕様書 | Issue | PR | ブランチ |
|----|---------|-----------|--------|:--:|--------|-------|:--:|----------|
| TASK-006 | [BE] 上長評価 API（入力・確定） | ✅ done | @KokiTokutake | 3 | SPEC-EVAL-001 | — | — | feat/T-006-manager-evaluation-api |
| TASK-007 | [FE] 目標設定画面 UI | ✅ done | @KokiTokutake | 3 | SPEC-EVAL-001 | — | — | feat/T-007-goal-setting-ui |
| TASK-008 | [FE] 自己評価入力画面 UI（自動下書き保存） | ✅ done | @KokiTokutake | 5 | SPEC-EVAL-001 | — | — | feat/T-008-self-evaluation-ui |
| TASK-009 | [FE] 上長評価入力画面 UI | ✅ done | @KokiTokutake | 3 | SPEC-EVAL-001 | — | — | feat/T-009-manager-evaluation-ui |
| TASK-010 | [TEST] Phase 1 結合テスト・受入確認 | ✅ done | @KokiTokutake | 3 | SPEC-EVAL-001 | — | — | feat/T-010-phase1-integration-test |

## Sprint 3 ✅ 完了

- **ゴール**: スキル管理機能 バックエンド全実装
- **期間**: 2026-05-31 〜 2026-06-06
- **進捗**: 11 / 11 SP 完了

| ID | タイトル | ステータス | 担当者 | SP | 仕様書 | Issue | PR | ブランチ |
|----|---------|-----------|--------|:--:|--------|-------|:--:|----------|
| TASK-011 | [BE] スキルマスター管理 API（SkillCategory / SkillItem CRUD） | ✅ done | @KokiTokutake | 3 | SPEC-SKILL-001 | — | — | feat/T-011-skill-master-api |
| TASK-012 | [BE] ユーザースキル CRUD API（追加・編集・削除・一覧） | ✅ done | @KokiTokutake | 3 | SPEC-SKILL-001 | — | — | — |
| TASK-013 | [BE] スキル承認・差し戻し API | ✅ done | @KokiTokutake | 3 | SPEC-SKILL-001 | — | — | — |
| TASK-014 | [BE] 組織スキルマップ API | ✅ done | @KokiTokutake | 2 | SPEC-SKILL-001 | — | — | — |

## Sprint 4 ✅ 完了

- **ゴール**: スキル管理機能 フロントエンド全画面・結合テスト
- **期間**: 2026-06-07 〜 2026-06-13
- **進捗**: 13 / 13 SP 完了

| ID | タイトル | ステータス | 担当者 | SP | 仕様書 | Issue | PR | ブランチ |
|----|---------|-----------|--------|:--:|--------|-------|:--:|----------|
| TASK-015 | [FE] スキルマスター管理画面 UI（Admin） | ✅ done | @KokiTokutake | 2 | SPEC-SKILL-001 | — | — | — |
| TASK-016 | [FE] 自分のスキル管理画面 UI（一覧・追加・編集・削除） | ✅ done | @KokiTokutake | 3 | SPEC-SKILL-001 | — | — | — |
| TASK-017 | [FE] スキル承認・差し戻し画面 UI（上長向け） | ✅ done | @KokiTokutake | 3 | SPEC-SKILL-001 | — | — | — |
| TASK-018 | [FE] 組織スキルマップ画面 UI | ✅ done | @KokiTokutake | 3 | SPEC-SKILL-001 | — | — | — |
| TASK-019 | [TEST] スキル管理 結合テスト・受入確認 | ✅ done | @KokiTokutake | 2 | SPEC-SKILL-001 | — | — | — |

## Sprint 5 ✅ 完了

- **ゴール**: キャリアプラン管理 バックエンド全実装
- **期間**: 2026-06-14 〜 2026-06-20
- **進捗**: 10 / 10 SP 完了

| ID | タイトル | ステータス | 担当者 | SP | 仕様書 | Issue | PR | ブランチ |
|----|---------|-----------|--------|:--:|--------|-------|:--:|----------|
| TASK-020 | [BE] キャリアプラン CRUD API（作成・編集・年度別履歴） | ✅ done | @KokiTokutake | 3 | SPEC-CAREER-001 | #3 | #10 | feat/3-career-plan-crud-api |
| TASK-021 | [BE] Will-Can-Must 統合ビュー API（Goal・Skill 集約） | ✅ done | @KokiTokutake | 3 | SPEC-CAREER-001 | #4 | #11 | feat/4-will-can-must-view-api |
| TASK-022 | [BE] 上長コメント API（追記・一覧） | ✅ done | @KokiTokutake | 2 | SPEC-CAREER-001 | #5 | #12 | feat/5-career-plan-comment-api |
| TASK-023 | [BE] 組織キャリアプラン俯瞰 API | ✅ done | @KokiTokutake | 2 | SPEC-CAREER-001 | #6 | #13 | feat/6-career-plan-org-overview-api |

## Sprint 6 ✅ 完了

- **ゴール**: キャリアプラン管理 フロントエンド全画面・結合テスト
- **期間**: 2026-06-21 〜 2026-06-27
- **進捗**: 10 / 10 SP 完了

| ID | タイトル | ステータス | 担当者 | SP | 仕様書 | Issue | PR | ブランチ |
|----|---------|-----------|--------|:--:|--------|-------|:--:|----------|
| TASK-024 | [FE] キャリアプランページ UI（統合ビュー・編集・履歴切替） | ✅ done | @KokiTokutake | 5 | SPEC-CAREER-001 | #7 | #14 | feat/7-career-plan-page-ui |
| TASK-025 | [FE] 組織キャリアプラン俯瞰ページ UI | ✅ done | @KokiTokutake | 2 | SPEC-CAREER-001 | #8 | #15 | feat/8-career-plan-org-overview-ui |
| TASK-026 | [TEST] キャリアプラン管理 結合テスト・受入確認 | ✅ done | @KokiTokutake | 3 | SPEC-CAREER-001 | #9 | #16 | feat/9-career-plan-integration-test |

## Sprint 7 ✅ 完了

- **ゴール**: 人材開発委員会機能 バックエンド全実装
- **期間**: 2026-05-24 〜 2026-05-30
- **進捗**: 12 / 12 SP 完了

| ID | タイトル | ステータス | 担当者 | SP | 仕様書 | Issue | PR | ブランチ |
|----|---------|-----------|--------|:--:|--------|-------|:--:|----------|
| TASK-027 | [BE] DBスキーマ拡張・EF Core マイグレーション（KeyPerson / DevelopmentPlan / CommitteeSession） | ✅ done | @KokiTokutake | 3 | SPEC-MDC-001 | #17 | #27 | feat/17-db-schema-mdc |
| TASK-028 | [BE] 重点管理人物 API（指定・解除・一覧・自分の状況） | ✅ done | @KokiTokutake | 3 | SPEC-MDC-001 | #18 | #28 | feat/18-key-person-api |
| TASK-029 | [BE] 育成方針 API（新規・バージョン管理・履歴一覧） | ✅ done | @KokiTokutake | 3 | SPEC-MDC-001 | #19 | #29 | feat/19-development-plan-api |
| TASK-030 | [BE] 委員会セッション API（作成・一覧・詳細）＋ 委員会サマリー API | ✅ done | @KokiTokutake | 3 | SPEC-MDC-001 | #20 | #30 | feat/20-committee-session-api |

## Sprint 8 ✅ 完了

- **ゴール**: 人材開発委員会機能 フロントエンド全画面・結合テスト
- **期間**: 2026-05-31 〜 2026-06-06
- **進捗**: 16 / 16 SP 完了

| ID | タイトル | ステータス | 担当者 | SP | 仕様書 | Issue | PR | ブランチ |
|----|---------|-----------|--------|:--:|--------|-------|:--:|----------|
| TASK-031 | [FE] 重点管理人物一覧・指定・解除画面 UI（Senior/Executive/Admin） | ✅ done | @KokiTokutake | 3 | SPEC-MDC-001 | #21 | #31 | feat/21-key-person-ui |
| TASK-032 | [FE] 育成方針入力・閲覧・履歴画面 UI（Manager 入力 / Talent・Senior 閲覧） | ✅ done | @KokiTokutake | 3 | SPEC-MDC-001 | #22 | #32 | feat/22-development-plan-ui |
| TASK-033 | [FE] 委員会セッション管理画面 UI（作成・一覧・詳細） | ✅ done | @KokiTokutake | 3 | SPEC-MDC-001 | #23 | #33 | feat/23-committee-session-ui |
| TASK-034 | [FE] 委員会向けサマリービュー画面 UI | ✅ done | @KokiTokutake | 2 | SPEC-MDC-001 | #24 | #34 | feat/24-committee-summary-ui |
| TASK-035 | [FE] タレント向けマイページ統合（重点管理バッジ・育成方針表示をキャリアプランページに追加） | ✅ done | @KokiTokutake | 2 | SPEC-MDC-001 | #25 | #35 | feat/25-talent-mypage-integration |
| TASK-036 | [TEST] 人材開発委員会機能 結合テスト・受入確認 | ✅ done | @KokiTokutake | 3 | SPEC-MDC-001 | #26 | #36 | feat/26-mdc-integration-test |

## Sprint 9 ⬜ 未着手

- **ゴール**: 9ボックス評価・1on1面談記録 バックエンド全実装（DB含む）
- **期間**: 未定
- **進捗**: 0 / 16 SP

| ID | タイトル | ステータス | 担当者 | SP | 仕様書 | Issue | PR | ブランチ |
|----|---------|-----------|--------|:--:|--------|-------|:--:|----------|
| TASK-037 | [DB] NineBoxAssessment・CellDefinition スキーマ＋EF Core マイグレーション | ✅ done | @KokiTokutake | 2 | SPEC-MDC-002 | #38 | — | — |
| TASK-038 | [BE] 9ボックス評価 API（入力・更新・期間別取得） | 🏃 進行中 | @KokiTokutake | 3 | SPEC-MDC-002 | #39 | — | — |
| TASK-039 | [BE] 9ボックスマップ・フィルタ・検索 API | 🏃 進行中 | @KokiTokutake | 3 | SPEC-MDC-002 | #40 | — | — |
| TASK-040 | [BE] キャリブレーション API＋重点管理候補自動フラグ・通知 | 🏃 進行中 | @KokiTokutake | 3 | SPEC-MDC-002 | #41 | — | — |
| TASK-041 | [DB] OneOnOneMeeting・MeetingActionItem スキーマ＋マイグレーション | ✅ done | @KokiTokutake | 2 | SPEC-1ON1-001 | #42 | — | — |
| TASK-042 | [BE] 1on1記録 CRUD API（作成・更新・削除・一覧・詳細） | ✅ done | @KokiTokutake | 3 | SPEC-1ON1-001 | #43 | — | PR#57 |

## Sprint 10 ⬜ 未着手

- **ゴール**: 9ボックス全画面 UI＋1on1 バックエンド残り
- **期間**: 未定
- **進捗**: 0 / 14 SP

| ID | タイトル | ステータス | 担当者 | SP | 仕様書 | Issue | PR | ブランチ |
|----|---------|-----------|--------|:--:|--------|-------|:--:|----------|
| TASK-043 | [BE] 宿題管理 API＋前回宿題自動引き継ぎ | ✅ done | @KokiTokutake | 2 | SPEC-1ON1-001 | #44 | PR#58 | feat/44-action-item-api |
| TASK-044 | [BE] 委員会連携 API（共有フラグ・サマリー）＋テンプレートマスタ | 🏃 進行中 | @KokiTokutake | 2 | SPEC-1ON1-001 | #45 | — | feat/45-committee-template-api |
| TASK-045 | [FE] 9ボックス評価入力フォーム UI（上長向け） | ✅ done | @KokiTokutake | 2 | SPEC-MDC-002 | #46 | PR#62 | feat/46-nine-box-input-ui |
| TASK-046 | [FE] 9ボックスマップ画面 UI（グリッド・ポップオーバー・期間切替・フィルタ） | ✅ done | @KokiTokutake | 5 | SPEC-MDC-002 | #47 | PR#61 | feat/47-nine-box-map-ui |
| TASK-047 | [FE] キャリブレーション画面 UI（評価分布・統計表示） | 🏃 進行中 | @KokiTokutake | 3 | SPEC-MDC-002 | #48 | — | feat/48-nine-box-calibration-ui |

## Sprint 11 ⬜ 未着手

- **ゴール**: 1on1全画面 UI＋結合テスト → スプリントゴール達成
- **期間**: 未定
- **進捗**: 0 / 13 SP

| ID | タイトル | ステータス | 担当者 | SP | 仕様書 | Issue | PR | ブランチ |
|----|---------|-----------|--------|:--:|--------|-------|:--:|----------|
| TASK-048 | [FE] 1on1記録作成・編集画面 UI（テンプレート選択・宿題入力） | ✅ done | @KokiTokutake | 3 | SPEC-1ON1-001 | #49 | — | feat/49-one-on-one-create-edit-ui |
| TASK-049 | [FE] 面談履歴一覧 UI＋前回引き継ぎサイドパネル | ✅ done | @KokiTokutake | 3 | SPEC-1ON1-001 | #50 | — | feat/50-one-on-one-history-ui |
| TASK-050 | [FE] 委員会連携フラグ・MDCサマリー反映 UI | ✅ done | @KokiTokutake | 2 | SPEC-1ON1-001 | #51 | — | feat/51-one-on-one-mdc-ui |
| TASK-051 | [TEST] 9ボックス評価 結合テスト・受入確認 | ✅ done | @KokiTokutake | 3 | SPEC-MDC-002 | #52 | — | feat/52-nine-box-integration-test |
| TASK-052 | [TEST] 1on1面談記録 結合テスト・受入確認 | ✅ done | @KokiTokutake | 2 | SPEC-1ON1-001 | #53 | PR#67 | feat/53-one-on-one-integration-test |

---

## サマリー

| 項目 | 値 |
|------|----|
| 総タスク数 | 52 |
| 完了 | 35 |
| 進行中 | 0 |
| 未着手 | 17 |
| レビュー中 | 0 |
