namespace TechTalentManagement.Domain.Enums;

public enum NotificationType
{
    EvaluationPeriodStarted,     // NT-01: 考課期間開始
    SelfEvaluationDueSoon,       // NT-02: 自己評価期限3日前
    SelfEvaluationSubmitted,     // NT-03: 自己評価提出
    EvaluationRejected,          // NT-04: 考課差し戻し
    EvaluationApproved,          // NT-05: 考課確定
    KpiCheckinOverdue,           // NT-06: KPIチェックイン未実施
    NineBoxKeyPersonCandidate,   // NT-07: 重点管理候補フラグ
    SuccessionGapAlert,          // NT-08: 後継者ギャップアラート
    SuccessionCandidateAdded,    // NT-09: 後継者候補登録
    CompetencyEvaluationDueSoon  // NT-10: コンピテンシー評価期限前
}
