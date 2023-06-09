using System.ComponentModel.DataAnnotations;
using WTA.Shared.Attributes;

namespace WTA.Shared.Domain;

public abstract class BaseTreeEntity<T> : BaseEntity
    where T : BaseEntity
{
    public List<T> Children { get; set; } = new List<T>();

    public string Name { get; set; } = null!;

    public string Number { get; set; } = null!;

    public T? Parent { get; set; }

    [Navigation]
    public Guid? ParentId { get; set; }

    [ScaffoldColumn(false)]
    public string? InternalPath { get; set; }
}
