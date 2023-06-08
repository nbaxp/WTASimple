namespace WTA.Shared.Domain;

public abstract class BaseParentEntity<TChild> : BaseEntity where TChild : BaseEntity
{
    public List<TChild> Children { get; set; } = new List<TChild>();
}

public abstract class BaseChildEntity<TParent> : BaseEntity where TParent : BaseEntity
{
    public Guid ParentId { get; set; }
    public TParent Parent { get; set; } = null!;
}
