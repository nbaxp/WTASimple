namespace WTA.Shared.Domain;

public interface ISoftDeleted
{
    bool IsDeleted { get; set; }
}
