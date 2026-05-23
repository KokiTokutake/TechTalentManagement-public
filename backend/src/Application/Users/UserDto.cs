namespace TechTalentManagement.Application.Users;

public record UserDto(
    Guid Id,
    string DisplayName,
    string Email,
    string Role,
    Guid? ManagerId
);
