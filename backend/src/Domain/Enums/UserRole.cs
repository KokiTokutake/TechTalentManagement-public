namespace TeckTalentManagement.Domain.Enums;

public enum UserRole
{
    Talent,    // タレント（評価対象エンジニア）
    Manager,   // 上長（第一評価者）
    Senior,    // 管理職（第二承認者）
    Executive, // 幹部長（最終承認者）
    Admin      // 管理者（人事評価チーム）
}
