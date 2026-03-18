using System.ComponentModel.DataAnnotations;

namespace IntraFlow.Web.Models.Admin;

public class CreateRequestTypeViewModel
{
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [StringLength(500)]
    public string Description { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Default Approver")]
    public string DefaultApproverUserId { get; set; } = string.Empty;

    public List<ApproverOption> Approvers { get; set; } = new();
}

public class ApproverOption
{
    public string Id { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}