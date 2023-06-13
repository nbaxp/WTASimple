namespace WTA.Shared.Domain;

public interface IAuditEntity
{
    DateTime? CreatedOn { get; set; }
    string? CreatedBy { get; set; }
    DateTime? UpdatedOn { get; set; }
    string? UpdatedBy { get; set; }
    DateTime? DeletedOn { get; set; }
    string? DeletedBy { get; set; }
}
