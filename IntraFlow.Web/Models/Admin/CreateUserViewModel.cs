using System.ComponentModel.DataAnnotations;

namespace IntraFlow.Web.Models.Admin;

public class CreateUserViewModel
{
    [Required]
    public string Name { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Role { get; set; } = string.Empty;

    public List<string> Roles { get; set; } = new();
}