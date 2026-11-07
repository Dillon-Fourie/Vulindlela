
namespace Vulindlela.Portal.Models.Auth;

public class ApplicationUser
{
    public int Id { get; set; }

    public string Username { get; set; } = string.Empty;

    public string PasswordHash { get; set; } = string.Empty;

    public string Role { get; set; } = "Citizen";

    public string CitizenId { get; set; } = string.Empty;
}
