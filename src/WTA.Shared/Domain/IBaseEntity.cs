namespace WTA.Shared.Domain;

public interface IBaseEntity
{
    Guid Id { get; set; }
    bool IsDeleted { get; set; }
    string ConcurrencyStamp { get; set; }
    string? TenantId { get; set; }
}
