using TeckTalentManagement.Domain.Enums;

namespace TeckTalentManagement.Domain.Entities;

public class User
{
    public Guid Id { get; private set; }
    public string EntraObjectId { get; private set; } = string.Empty; // Entra ID の OID（T-002 で連携）
    public string DisplayName { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public UserRole Role { get; private set; }
    public Guid? ManagerId { get; private set; }

    public User? DirectManager { get; private set; }
    public ICollection<Goal> Goals { get; private set; } = [];
    public ICollection<SelfEvaluation> SelfEvaluations { get; private set; } = [];
    public ICollection<ManagerEvaluation> ManagerEvaluations { get; private set; } = [];

    private User() { }

    public static User Create(string entraObjectId, string displayName, string email, UserRole role, Guid? managerId = null)
    {
        return new User
        {
            Id = Guid.NewGuid(),
            EntraObjectId = entraObjectId,
            DisplayName = displayName,
            Email = email,
            Role = role,
            ManagerId = managerId
        };
    }
}
