using ClosedXML.Excel;
using ClosedXML.Graphics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Extensions;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Reflection;
using WTA.Infrastructure.Attributes;

namespace WTA.Infrastructure.ExportImport;

[Implement<IExportImportService>]
public class ClosedXmlExportImportService : IExportImportService
{
    public const string ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
    private readonly ILogger<ClosedXmlExportImportService> _logger;

    static ClosedXmlExportImportService()
    {
        using var fallbackFontStream = Assembly.GetExecutingAssembly().GetManifestResourceStream($"{WebApp.Current.Prefix}.Infrastructure.Resources.calibril.ttf");
        LoadOptions.DefaultGraphicEngine = DefaultGraphicEngine.CreateWithFontsAndSystemFonts(fallbackFontStream);
    }

    public ClosedXmlExportImportService(ILogger<ClosedXmlExportImportService> logger)
    {
        this._logger = logger;
    }

    public FileContentResult Export<TExportModel>(List<TExportModel> list)
    {
        try
        {
            using var workbook = new XLWorkbook();
            var name = typeof(TExportModel).GetCustomAttribute<DisplayAttribute>()?.Name ?? typeof(TExportModel).Name;
            var fileName = $"{name}_导出.xlsx";
            var ws = workbook.Worksheets.Add(name);
            ws.Style.Font.FontName = "宋体";
            //
            //Internal
            //
            var stream = new MemoryStream();
            workbook.SaveAs(stream);
            stream.Seek(0, SeekOrigin.Begin);
            var result = new FileContentResult(stream.ToArray(), ContentType)
            {
                FileDownloadName = fileName
            };
            return result;
        }
        catch (Exception ex)
        {
            this._logger.LogError(ex, ex.ToString());
            throw new Exception($"导出数据错误：{ex.Message}", ex);
        }
    }

    public FileContentResult GetImportTemplate<TImportModel>()
    {
        //TModelType=>File
        throw new NotImplementedException();
    }

    public IList<TImportModel> Import<TImportModel>(byte[] bytes)
    {
        try
        {
            var result = new List<TImportModel>();
            //
            using var workbook = new XLWorkbook(new MemoryStream(bytes));
            var type = typeof(TImportModel);
            var properties = GetPropertiesForImportModel(type).ToDictionary(o => o.GetCustomAttribute<ImporterHeaderAttribute>()?.Name ?? o.GetCustomAttribute<DisplayAttribute>()?.Name ?? o.Name);
            var name = type.GetCustomAttribute<DisplayAttribute>()?.Name ?? typeof(TImportModel).Name;
            var ws = workbook.Worksheets.FirstOrDefault();
            if (ws != null)
            {
                for (int i = 1; i < ws.RowsUsed().Count(); i++)
                {
                    var rowIndex = i + 1;
                    var row = ws.Row(rowIndex);
                    var model = Activator.CreateInstance<TImportModel>();
                    for (int j = 0; j < ws.ColumnsUsed().Count(); j++)
                    {
                        var columnIndex = j + 1;
                        var cell = row.Cell(columnIndex);
                        var value = cell.Value;
                        if (value.ToString() != "")
                        {
                            var headerName = ws.Cell(1, columnIndex).Value.ToString().Trim();
                            properties.TryGetValue(headerName, out PropertyInfo? property);
                            if (property != null)
                            {
                                var propertyType = property.PropertyType;
                                if (propertyType.IsEnum)
                                {
                                    var enumValue = Enum.GetNames(propertyType)
                                         .Select(o => new KeyValuePair<string, Enum>(o, (Enum)Enum.Parse(propertyType, o)))
                                         .Where(o => o.Value.GetDisplayName() == value.ToString())
                                         .Select(o => o.Value)
                                         .FirstOrDefault();
                                    property.SetValue(model, enumValue);
                                }
                                else if (propertyType.Name == nameof(Boolean))
                                {
                                    if (value.GetText() == "是")
                                    {
                                        property.SetValue(model, true);
                                    }
                                    else
                                    {
                                        property.SetValue(model, false);
                                    }
                                }
                                else
                                {
                                    var propertyValue = Convert.ChangeType(value.ToString(), propertyType, CultureInfo.InvariantCulture);
                                    property.SetValue(model, propertyValue);
                                }
                            }
                        }
                    }
                    result.Add(model);
                }
            }
            return result;
        }
        catch (Exception ex)
        {
            this._logger.LogError(ex, ex.ToString());
            throw new Exception($"导入数据错误：{ex.Message}", ex);
        }
    }

    private static PropertyInfo[] GetPropertiesForImportModel(Type type)
    {
        return type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.GetProperty)
             .Where(o => o.GetCustomAttribute<ImporterHeaderAttribute>(true) == null || !o.GetCustomAttribute<ImporterHeaderAttribute>()!.IsIgnore)
             .ToArray();
    }
}