namespace WTA.Shared.Application;

public class PaginationModel<TSearchModel, TListModel>
{
    public int PageIndex { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string? OrderBy { get; set; }
    public int TotalCount { get; set; }
    public bool QueryAll { get; set; }
    public List<TListModel> Items { get; set; } = new List<TListModel>();
    public TSearchModel Query { get; set; } = Activator.CreateInstance<TSearchModel>();
}
