using System.ComponentModel.DataAnnotations;
using IntraFlow.Domain.Requests;

namespace IntraFlow.Web.Models.Requests;

public sealed class CreateRequestViewModel
{
    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [MaxLength(2000)]
    public string Description { get; set; } = string.Empty;

    [Required]
    public RequestPriority Priority { get; set; }

    [Required]
    public int RequestTypeId { get; set; }

    public List<RequestTypeOption> RequestTypes { get; set; } = new();

    [Required]
    public string SubmitAction { get; set; } = "Draft";
}

public sealed class RequestTypeOption
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}