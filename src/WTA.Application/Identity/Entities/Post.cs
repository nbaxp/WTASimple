using WTA.Shared.Attributes;
using WTA.Shared.Domain;

namespace WTA.Application.Identity.Entities;

[Order(5)]
public class Post : BaseEntity
{
    public string Name { get; set; } = null!;
    public string Number { get; set; } = null!;
    public List<User> Users { get; set; } = new List<User>();
}
