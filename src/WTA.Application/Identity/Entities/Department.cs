using WTA.Shared.Attributes;
using WTA.Shared.Domain;

namespace WTA.Application.Identity.Entities;

[Order(4)]
public class Department : BaseTreeEntity<Department>
{
    public List<User> Users { get; set; } = new List<User>();
}
