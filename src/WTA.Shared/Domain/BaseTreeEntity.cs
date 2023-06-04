using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace WTA.Shared.Domain;

public abstract class BaseTreeEntity<T> : BaseEntity
    where T : class
{
    public List<T> Children { get; set; } = new List<T>();

    [Display]
    public string Name { get; set; } = null!;

    [Display]
    public string Number { get; set; } = null!;

    public T? Parent { get; set; }

    [HiddenInput]
    public Guid? ParentId { get; set; }

    [HiddenInput]
    public string InternalPath { get; set; } = null!;
}
