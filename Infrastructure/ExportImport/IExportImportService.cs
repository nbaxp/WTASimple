using Microsoft.AspNetCore.Mvc;

namespace WTA.Infrastructure.ExportImport;

public interface IExportImportService
{
    FileContentResult Export<TModel>(List<TModel> list);

    FileContentResult GetImportTemplate<TImportModel>();

    IList<TImportModel> Import<TImportModel>(byte[] bytes);
}