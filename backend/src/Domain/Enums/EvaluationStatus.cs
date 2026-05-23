namespace TechTalentManagement.Domain.Enums;

public enum EvaluationStatus
{
    NotStarted,          // 未着手
    GoalRegistered,      // 目標登録済み
    SelfEvaluationDraft, // 自己評価下書き中
    SelfEvaluationSubmitted, // 自己評価提出済み
    ManagerEvaluated,    // 上長評価完了
    PendingManagerApproval,  // 管理職承認待ち
    PendingExecutiveApproval, // 幹部長承認待ち
    Approved,            // 全承認完了（フィードバック開示）
    Rejected             // 差し戻し
}
