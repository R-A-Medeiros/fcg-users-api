namespace FIAP.Domain.Communication.Responses;

public class ResponseUserRoleJson
{
    public Guid UserIdentifier { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
}
