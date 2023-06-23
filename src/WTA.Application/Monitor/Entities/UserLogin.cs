using System.ComponentModel.DataAnnotations;
using WTA.Application.Monitor.Controllers;
using WTA.Shared.Attributes;
using WTA.Shared.Domain;

namespace WTA.Application.Monitor.Entities;

[Order(1)]
[SystemMonitor]
public class UserLogin : BaseEntity
{
    public string ConnectionId { get; set; } = null!;
    public string UserName { get; set; } = null!;

    [Required]
    public DateTime? Login { get; set; }

    public DateTime? Logout { get; set; }

    [Required]
    public bool? IsOnline { get; set; }

    public string? UserAgent { get; set; }
}
