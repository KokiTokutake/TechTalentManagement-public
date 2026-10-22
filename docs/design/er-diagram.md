# ER 図（全体）

エンティティ（`backend/src/Domain/Entities/`、37個）の外部キー項目から作成した、主要な関連の一覧。
項目の詳細は、各機能の設計書（`docs/design/SPEC-*-design.md`）を参照。

## 評価・目標（考課フロー）

```mermaid
erDiagram
    User ||--o{ User : "上長 (ManagerId)"
    EvaluationPeriod ||--o{ Goal : has
    User ||--o{ Goal : owns
    Goal ||--o{ GoalKpi : has
    Goal ||--o{ GoalCheckin : "進捗チェックイン"
    GoalKpi ||--o{ GoalCheckin : "KPI別"
    EvaluationPeriod ||--o{ SelfEvaluation : has
    User ||--o{ SelfEvaluation : writes
    SelfEvaluation ||--o{ SelfEvaluationItem : has
    Goal ||--o{ SelfEvaluationItem : "目標別の自己評価"
    SelfEvaluation ||--o{ ManagerEvaluation : "上長評価"
    SelfEvaluation ||--o{ FeedbackMeeting : "フィードバック面談"
    CompetencyMaster ||--o{ CompetencyEvaluation : "コンピテンシー評価"
    EvaluationPeriod ||--o{ CompetencyEvaluation : in
```

## タレントマネジメント（9ボックス・後継者・人材開発委員会）

```mermaid
erDiagram
    EvaluationPeriod ||--o{ NineBoxAssessment : in
    User ||--o{ NineBoxAssessment : "対象タレント / 評価者"
    NineBoxCellDefinition ||--o{ NineBoxAssessment : "セル定義 (9件固定)"
    User ||--o{ KeyPerson : "重点管理人物"
    KeyPerson ||--o{ DevelopmentPlan : has
    CriticalPosition ||--o{ SuccessionCandidate : "後継候補"
    User ||--o{ SuccessionCandidate : is
    SuccessionCandidate ||--o{ SuccessionDevelopmentAction : "育成アクション"
    CommitteeSession ||--o{ MdcDecision : "決定事項"
    MdcDecision ||--o{ MdcActionItem : "アクション"
    User ||--o{ TalentManagerComment : "タレント所見"
    EvaluationPeriod ||--o{ TalentManagerComment : in
```

## スキル・キャリア・1on1

```mermaid
erDiagram
    SkillCategory ||--o{ SkillItem : has
    SkillItem ||--o{ SkillLevelDefinition : "レベル定義 (ITSS)"
    User ||--o{ UserSkill : has
    SkillItem ||--o{ UserSkill : "習得スキル"
    UserSkill ||--o{ UserSkillHistory : "変更履歴"
    User ||--o{ ProjectAssignment : "アサイン"
    ProjectAssignment ||--o{ ProjectAssignmentSkill : uses
    SkillItem ||--o{ ProjectAssignmentSkill : "求められるスキル"
    User ||--o{ CareerPlan : writes
    CareerPlan ||--o{ CareerPlanComment : has
    User ||--o{ WillCanMustEntry : "Will-Can-Must"
    SkillItem ||--o{ WillCanMustEntry : "紐づくスキル"
    EvaluationPeriod ||--o{ WillCanMustEntry : in
    User ||--o{ OneOnOneMeeting : "本人 / 上長"
    OneOnOneMeeting ||--o{ MeetingActionItem : has
    User ||--o{ Notification : receives
```

## 設計上の特徴

- **期間 (`EvaluationPeriod`) を軸にした履歴管理。** 同一期間に同一タレントの評価は1件だけにする（例：9ボックスは `(TalentId, EvaluationPeriodId)` を一意にしている）。
- **派生値は DB に持たない。** 例：重点管理候補フラグは、保存せず API の応答生成時に計算する（`SPEC-MDC-002-design.md`）。
- **履歴は別テーブルに残す。** スキルの変更は `UserSkillHistory` に、スキルレベル定義の移行は `SkillLevelMigrationLog` に記録する。
- **マスタと実績を分ける。** 9ボックスのセル定義、コンピテンシー、スキルカテゴリは、マスタとして独立させている。
