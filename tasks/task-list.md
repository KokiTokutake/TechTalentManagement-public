# タスク一覧

> 最終更新: 2026-05-25 （/tasks-plan SPEC-MDC-005 により更新）

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

## Sprint 9 ✅ 完了

- **ゴール**: 9ボックス評価・1on1面談記録 バックエンド全実装（DB含む）
- **期間**: 未定
- **進捗**: 16 / 16 SP 完了

| ID | タイトル | ステータス | 担当者 | SP | 仕様書 | Issue | PR | ブランチ |
|----|---------|-----------|--------|:--:|--------|-------|:--:|----------|
| TASK-037 | [DB] NineBoxAssessment・CellDefinition スキーマ＋EF Core マイグレーション | ✅ done | @KokiTokutake | 2 | SPEC-MDC-002 | #38 | — | — |
| TASK-038 | [BE] 9ボックス評価 API（入力・更新・期間別取得） | ✅ done | @KokiTokutake | 3 | SPEC-MDC-002 | #39 | PR#54 | — |
| TASK-039 | [BE] 9ボックスマップ・フィルタ・検索 API | ✅ done | @KokiTokutake | 3 | SPEC-MDC-002 | #40 | PR#54 | — |
| TASK-040 | [BE] キャリブレーション API＋重点管理候補自動フラグ・通知 | ✅ done | @KokiTokutake | 3 | SPEC-MDC-002 | #41 | PR#54 | — |
| TASK-041 | [DB] OneOnOneMeeting・MeetingActionItem スキーマ＋マイグレーション | ✅ done | @KokiTokutake | 2 | SPEC-1ON1-001 | #42 | — | — |
| TASK-042 | [BE] 1on1記録 CRUD API（作成・更新・削除・一覧・詳細） | ✅ done | @KokiTokutake | 3 | SPEC-1ON1-001 | #43 | PR#57 | — |

## Sprint 10 ✅ 完了

- **ゴール**: 9ボックス全画面 UI＋1on1 バックエンド残り
- **期間**: 未定
- **進捗**: 14 / 14 SP 完了

| ID | タイトル | ステータス | 担当者 | SP | 仕様書 | Issue | PR | ブランチ |
|----|---------|-----------|--------|:--:|--------|-------|:--:|----------|
| TASK-043 | [BE] 宿題管理 API＋前回宿題自動引き継ぎ | ✅ done | @KokiTokutake | 2 | SPEC-1ON1-001 | #44 | PR#58 | feat/44-action-item-api |
| TASK-044 | [BE] 委員会連携 API（共有フラグ・サマリー）＋テンプレートマスタ | ✅ done | @KokiTokutake | 2 | SPEC-1ON1-001 | #45 | PR#59 | feat/45-committee-template-api |
| TASK-045 | [FE] 9ボックス評価入力フォーム UI（上長向け） | ✅ done | @KokiTokutake | 2 | SPEC-MDC-002 | #46 | PR#60 | feat/46-nine-box-input-ui |
| TASK-046 | [FE] 9ボックスマップ画面 UI（グリッド・ポップオーバー・期間切替・フィルタ） | ✅ done | @KokiTokutake | 5 | SPEC-MDC-002 | #47 | PR#61 | feat/47-nine-box-map-ui |
| TASK-047 | [FE] キャリブレーション画面 UI（評価分布・統計表示） | ✅ done | @KokiTokutake | 3 | SPEC-MDC-002 | #48 | PR#62 | feat/48-nine-box-calibration-ui |

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

## Sprint 12 ⬜ 未着手

- **ゴール**: タレントプロファイル（委員会事前資料）全実装
- **期間**: 未定
- **進捗**: 0 / 14 SP

| ID | タイトル | ステータス | 担当者 | SP | 仕様書 | Issue | PR | ブランチ |
|----|---------|-----------|--------|:--:|--------|-------|:--:|----------|
| TASK-053 | [DB] TalentManagerComment スキーマ＋EF Core マイグレーション | ✅ done | @KokiTokutake | 2 | SPEC-MDC-003 | — | #68 | feat/T-053-talent-manager-comment-schema |
| TASK-054 | [BE] タレントプロファイル集約 API + 育成コメント API | ✅ done | @KokiTokutake | 5 | SPEC-MDC-003 | — | #69 | feat/T-054-talent-profile-api |
| TASK-055 | [FE] タレントプロファイル画面 UI（集約ビュー・育成コメント入力・重点管理フラグ） | ✅ done | @KokiTokutake | 5 | SPEC-MDC-003 | — | #70 | feat/T-055-talent-profile-ui |
| TASK-056 | [TEST] タレントプロファイル 結合テスト・受入確認 | ✅ done | @KokiTokutake | 2 | SPEC-MDC-003 | — | #71 | feat/T-056-talent-profile-test |

## Sprint 13 ⬜ 未着手

- **ゴール**: 委員会議事録の構造化（決定事項・アクションアイテム管理）全実装
- **期間**: 未定
- **進捗**: 0 / 10 SP

| ID | タイトル | ステータス | 担当者 | SP | 仕様書 | Issue | PR | ブランチ |
|----|---------|-----------|--------|:--:|--------|-------|:--:|----------|
| TASK-057 | [DB] MdcDecision・MdcDecisionTalent・MdcActionItem スキーマ＋マイグレーション | ✅ done | @KokiTokutake | 2 | SPEC-MDC-004 | — | #72 | feat/T-057-mdc-decision-schema |
| TASK-058 | [BE] 決定事項・アクションアイテム CRUD API + タレント別決定履歴 API | ✅ done | @KokiTokutake | 3 | SPEC-MDC-004 | — | #73 | feat/T-058-mdc-decision-api |
| TASK-059 | [FE] MDCセッション詳細 UI 拡張（決定事項・アクションアイテム・前回積み残し） | ✅ done | @KokiTokutake | 3 | SPEC-MDC-004 | — | [#74](https://github.com/KokiTokutake/TechTalentManagement/pull/74) | feat/T-059-mdc-session-detail-ui |
| TASK-060 | [TEST] 委員会議事録 結合テスト・受入確認 | ✅ done | @KokiTokutake | 2 | SPEC-MDC-004 | — | [#75](https://github.com/KokiTokutake/TechTalentManagement/pull/75) | feat/T-060-mdc-integration-test |

## Sprint 14 ⬜ 未着手

- **ゴール**: Will-Can-Must深化（時間軸分割・Can強み/課題・やりがい）全実装
- **期間**: 未定
- **進捗**: 0 / 12 SP

| ID | タイトル | ステータス | 担当者 | SP | 仕様書 | Issue | PR | ブランチ |
|----|---------|-----------|--------|:--:|--------|-------|:--:|----------|
| TASK-061 | [DB] WillCanMustEntry・CareerValues スキーマ＋マイグレーション | ✅ done | @KokiTokutake | 2 | SPEC-CAREER-002 | — | #76 | feat/T-061-wcm-schema |
| TASK-062 | [BE] WCM深化 API（時間軸別Will・強み/課題Can・価値観・やりがい） | ✅ done | @KokiTokutake | 3 | SPEC-CAREER-002 | — | #77 | feat/T-062-wcm-api |
| TASK-063 | [FE] キャリアプランページ UI 拡張（3軸Will・強み/課題Can・価値観セクション） | ✅ done | @KokiTokutake | 5 | SPEC-CAREER-002 | — | #78 | feat/T-063-wcm-ui |
| TASK-064 | [TEST] WCM深化 結合テスト・受入確認 | ✅ done | @KokiTokutake | 2 | SPEC-CAREER-002 | — | #79 | feat/T-064-wcm-integration-test |

## Sprint 15 ⬜ 未着手

- **ゴール**: Mustの定量KPI・中間振り返り・フィードバック面談記録 全実装
- **期間**: 未定
- **進捗**: 0 / 15 SP

| ID | タイトル | ステータス | 担当者 | SP | 仕様書 | Issue | PR | ブランチ |
|----|---------|-----------|--------|:--:|--------|-------|:--:|----------|
| TASK-065 | [DB] GoalKpi・GoalCheckin・FeedbackMeeting スキーマ＋マイグレーション | ✅ done | @KokiTokutake | 2 | SPEC-EVAL-002 | — | #81 | feat/T-065-kpi-checkin-feedback-schema |
| TASK-066 | [BE] KPI設定・チェックイン・フィードバック面談 API | ✅ done | @KokiTokutake | 5 | SPEC-EVAL-002 | — | #82 | feat/T-066-kpi-checkin-feedback-api |
| TASK-067 | [FE] KPI入力・チェックイン・フィードバック面談 UI | ✅ done | @KokiTokutake | 5 | SPEC-EVAL-002 | — | #83 | feat/T-067-kpi-checkin-feedback-ui |
| TASK-068 | [TEST] KPI・チェックイン・フィードバック面談 結合テスト・受入確認 | ✅ done | @KokiTokutake | 3 | SPEC-EVAL-002 | — | — | feat/T-068-kpi-checkin-integration-test |

## Sprint 16 ⬜ 未着手

- **ゴール**: スキルレベル定義拡充（ITSS準拠）・コンピテンシー評価 全実装
- **期間**: 未定
- **進捗**: 0 / 23 SP

| ID | タイトル | ステータス | 担当者 | SP | 仕様書 | Issue | PR | ブランチ |
|----|---------|-----------|--------|:--:|--------|-------|:--:|----------|
| TASK-069 | [DB] SkillLevelDefinition・CompetencyMaster・CompetencyEvaluation・SkillLevelMigrationLog スキーマ＋UserSkill.Level 制約拡張マイグレーション | ✅ done | @KokiTokutake | 2 | SPEC-SKILL-002 | #86 | #92 | feat/86-skill-level-definition-schema |
| TASK-070 | [BE] スキルレベル定義 API（GET/PUT）＋ コンピテンシーマスタ API（GET/POST/PUT） | ✅ done | @KokiTokutake | 3 | SPEC-SKILL-002 | #87 | #93 | feat/87-skill-level-competency-master-api |
| TASK-071 | [BE] コンピテンシー評価 API（Upsert/GET）＋ スキルギャップ API ＋ スキルマップ コンピテンシー API ＋ 移行ツール API（preview/execute） | ✅ done | @KokiTokutake | 5 | SPEC-SKILL-002 | #88 | #94 | feat/88-competency-evaluation-skill-gap-api |
| TASK-072 | [FE] スキルレベル参照 UI 拡張（my.vue: 1-7スケール・ツールチップ）＋ コンピテンシー評価ページ（competency.vue） | ✅ done | @KokiTokutake | 5 | SPEC-SKILL-002 | #89 | #95 | feat/89-skill-level-ui-competency-page |
| TASK-073 | [FE] スキルギャップ比較ページ（gap.vue）＋ スキルマップ コンピテンシータブ（map.vue 拡張）＋ 管理者ツール 3ページ（レベル定義・コンピテンシーマスタ・移行ツール） | ✅ done | @KokiTokutake | 5 | SPEC-SKILL-002 | #90 | #96 | feat/90-skill-gap-map-admin-ui |
| TASK-074 | [TEST] SPEC-SKILL-002 結合テスト・受入確認（AC-01-01〜AC-05-01 全9件） | ✅ done | @KokiTokutake | 3 | SPEC-SKILL-002 | #91 | — | feat/91-spec-skill-002-integration-test |

## Sprint 17 ⬜ 未着手

- **ゴール**: 後継者計画（サクセッションプラン）全実装
- **期間**: 未定
- **進捗**: 0 / 23 SP

| ID | タイトル | ステータス | 担当者 | SP | 仕様書 | Issue | PR | ブランチ |
|----|---------|-----------|--------|:--:|--------|-------|:--:|----------|
| TASK-075 | [DB] CriticalPosition・SuccessionCandidate・SuccessionDevelopmentAction スキーマ＋マイグレーション | ✅ done | @KokiTokutake | 2 | SPEC-MDC-005 | #98 | #104 | feat/98-succession-plan-schema |
| TASK-076 | [BE] 重要ポジション CRUD API（GET一覧/POST/PUT）＋タレント別候補ポジション API | ✅ done | @KokiTokutake | 3 | SPEC-MDC-005 | #99 | #105 | feat/99-critical-position-api |
| TASK-077 | [BE] 後継者候補 CRUD API（GET/POST/PUT/DELETE）＋サクセッションマップ API＋9ボックスサジェスト API | ✅ done | @KokiTokutake | 5 | SPEC-MDC-005 | #100 | — | feat/100-succession-candidate-api |
| TASK-078 | [FE] 重要ポジション管理ページ（positions.vue）＋ポジション詳細・候補者管理ページ（[positionId].vue） | 🔵 レビュー中 | @KokiTokutake | 5 | SPEC-MDC-005 | #101 | — | — |
| TASK-079 | [FE] サクセッションマップページ（index.vue）＋9ボックス連動サジェストダイアログ＋育成アクション管理 | ⬜ pending | @KokiTokutake | 5 | SPEC-MDC-005 | #102 | — | — |
| TASK-080 | [TEST] SPEC-MDC-005 結合テスト・受入確認（AC-01-01〜AC-04-02 全8件） | ⬜ pending | @KokiTokutake | 3 | SPEC-MDC-005 | #103 | — | — |

---

## サマリー

| 項目 | 値 |
|------|----|
| 総タスク数 | 80 |
| 完了 | 67 |
| 進行中 | 0 |
| 未着手 | 13 |
| レビュー中 | 0 |
