# タスク一覧

> 最終更新: 2026-05-17

---

## Sprint 1 🏃 アクティブ

- **ゴール**: 骨格・認証・DB・目標設定API・自己評価API（Phase 1 前半）
- **期間**: 2026-05-17 〜 2026-05-23
- **合計**: 17 SP

| ID | タイトル | ステータス | 担当者 | SP | 仕様書 | ブランチ | 備考 |
|----|---------|-----------|--------|:--:|--------|----------|------|
| T-001 | [INFRA] Nuxt.js + ASP.NET Core プロジェクト骨格セットアップ | 🔵 in_progress | @KokiTokutake | 3 | SPEC-EVAL-001 | feat/T-001-project-scaffold | docker compose up でFE・BE・DBが起動することが完了条件 |
| T-002 | [INFRA] Entra ID SSO 認証・ロールベースアクセス制御（BE+FE） | ⬜ pending | @KokiTokutake | 5 | SPEC-EVAL-001 | feat/T-002-entra-id-auth | Azure Portal でのアプリ登録が必要。Key Vault にシークレットを格納すること。 |
| T-003 | [BE] DBスキーマ設計・EF Core マイグレーション | ⬜ pending | @KokiTokutake | 3 | SPEC-EVAL-001 | feat/T-003-db-schema | EvaluationPeriod / Goal / SelfEvaluation / ManagerEvaluation テーブルを定義 |
| T-004 | [BE] 目標設定 API（CRUD） | ⬜ pending | @KokiTokutake | 3 | SPEC-EVAL-001 | feat/T-004-goal-api | REQ-01対応。登録期間外アクセスは403を返すこと。 |
| T-005 | [BE] 自己評価 API（下書き保存・提出） | ⬜ pending | @KokiTokutake | 3 | SPEC-EVAL-001 | feat/T-005-self-evaluation-api | REQ-02対応。提出後は編集不可（409）。評価内容はログに出力しないこと。 |

---

## Sprint 2 📋 計画中

- **ゴール**: 上長評価API・FE3画面・Phase 1 結合テスト（Phase 1 後半）
- **期間**: 2026-05-24 〜 2026-05-30
- **合計**: 17 SP

| ID | タイトル | ステータス | 担当者 | SP | 仕様書 | ブランチ | 備考 |
|----|---------|-----------|--------|:--:|--------|----------|------|
| T-006 | [BE] 上長評価 API（入力・確定） | ⬜ pending | @KokiTokutake | 3 | SPEC-EVAL-001 | feat/T-006-manager-evaluation-api | REQ-03対応。自己評価提出済みの場合のみ入力可能。 |
| T-007 | [FE] 目標設定画面 UI | ⬜ pending | @KokiTokutake | 3 | SPEC-EVAL-001 | feat/T-007-goal-setting-ui | REQ-01対応。登録期間外はフォームを非活性化。 |
| T-008 | [FE] 自己評価入力画面 UI（自動下書き保存） | ⬜ pending | @KokiTokutake | 5 | SPEC-EVAL-001 | feat/T-008-self-evaluation-ui | REQ-02対応。入力3秒後に自動下書き保存（デバウンス）。保存状態を表示。 |
| T-009 | [FE] 上長評価入力画面 UI | ⬜ pending | @KokiTokutake | 3 | SPEC-EVAL-001 | feat/T-009-manager-evaluation-ui | REQ-03対応。確定後は読み取り専用に切り替え。 |
| T-010 | [TEST] Phase 1 結合テスト・受入確認 | ⬜ pending | @KokiTokutake | 3 | SPEC-EVAL-001 | feat/T-010-phase1-integration-test | SPEC-EVAL-001 §4 の受入基準チェックリストをすべて手動確認。 |

---

## サマリー

| 項目 | 値 |
|------|----|
| 総タスク数 | 10 |
| 完了 | 0 |
| 進行中 | 0 |
| 未着手 | 10 |
| 合計SP | 34 |
