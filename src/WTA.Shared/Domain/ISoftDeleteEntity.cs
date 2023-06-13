namespace WTA.Shared.Domain;

public interface ISoftDeleteEntity
{
    bool IsDeleted { get; set; }
}
