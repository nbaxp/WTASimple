using DocumentFormat.OpenXml.Bibliography;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using WTA.Shared.Application;
using WTA.Shared.Attributes;
using WTA.Shared.Data;
using WTA.Shared.Domain;
using WTA.Shared.Extensions;
using WTA.Shared.Mappers;

namespace WTA.Shared.Controllers;

[GenericControllerNameConvention]
public class GenericController<TEntity, TModel, TListModel, TSearchModel, TImportModel, TExportModel> : BaseController, IResourceService<TEntity>
    where TEntity : BaseEntity
    where TModel : class
    where TListModel : class
    where TSearchModel : class
    where TImportModel : class
    where TExportModel : class
{
    public GenericController(ILogger<TEntity> logger, IRepository<TEntity> repository)
    {
        this.Logger = logger;
        this.Repository = repository;
    }

    public ILogger<TEntity> Logger { get; }
    public IRepository<TEntity> Repository { get; }

    [HttpGet]
    public virtual IActionResult Index()
    {
        return Json(typeof(PaginationModel<TSearchModel, TListModel>).GetViewModel());
    }

    [HttpPost, Multiple, Order(-4), HtmlClass("el-button--primary")]
    public virtual IActionResult Index([FromBody] PaginationModel<TSearchModel, TListModel> model)
    {
        var query = BuildQuery(model);
        model.TotalCount = query.Count();
        if (!string.IsNullOrEmpty(model.OrderBy))
        {
            query = query.OrderBy(model.OrderBy);
        }
        if (model.QueryAll)
        {
            model.PageSize = model.TotalCount;
        }
        else
        {
            query = query.Skip(model.PageSize * (model.PageIndex - 1)).Take(model.PageSize);
        }
        model.Items = query
            .ToList()
            .Select(o => o.ToObject<TListModel>())
            .ToList();
        return Json(model);
    }

    protected virtual IQueryable<TEntity> BuildQuery(PaginationModel<TSearchModel, TListModel> model)
    {
        var isTree = typeof(TEntity).IsAssignableTo(typeof(BaseTreeEntity<TEntity>));
        var query = this.Repository.AsNoTracking();
        query = query.Include();
        if (model.Query != null)
        {
            query = query.Where(model: model.Query);
        }
        if (isTree)
        {
            model.OrderBy ??= $"{nameof(BaseTreeEntity<TEntity>.ParentId)},{nameof(BaseEntity.Order)},{nameof(BaseEntity.CreatedOn)}";
        }

        return query;
    }

    [HttpPost, Order(-2), HtmlClass("el-button--primary")]
    public virtual IActionResult Details(Guid id)
    {
        var entity = this.Repository.AsNoTracking().FirstOrDefault(o => o.Id == id);
        var model = entity?.ToObject<TModel>();
        return Json(model);
    }

    [HttpGet]
    public IActionResult Create()
    {
        return Json(typeof(TModel).GetViewModel());
    }

    [HttpPost, Multiple, Order(-3), HtmlClass("el-button--success")]
    public virtual IActionResult Create([FromBody] TModel model)
    {
        if (this.ModelState.IsValid)
        {
            try
            {
                var entity = Activator.CreateInstance<TEntity>().FromModel(model);
                this.Repository.Insert(entity);
                this.Repository.SaveChanges();
                return NoContent();
            }
            catch (Exception ex)
            {
                return Problem(ex.Message);
            }
        }
        return Json(model);
    }

    [HttpGet]
    public virtual IActionResult Update(Guid id)
    {
        return Json(new
        {
            Schema = typeof(TModel).GetMetadataForType(),
            Model = this.Repository.Queryable().FirstOrDefault(o => o.Id == id)
        });
    }

    [HttpPost, Order(-1)]
    public virtual IActionResult Update([FromBody] TModel model)
    {
        if (this.ModelState.IsValid)
        {
            try
            {
                var id = model.GetPropertyValue<TModel, Guid>(nameof(BaseEntity.Id));
                var entity = this.Repository.Queryable().FirstOrDefault(o => o.Id == id);
                if (entity == null)
                {
                    this.ModelState.AddModelError($"{nameof(id)}", $"not found entity by {id}");
                }
                else
                {
                    entity.FromModel(model);
                    this.Repository.SaveChanges();
                    return NoContent();
                }
            }
            catch (Exception ex)
            {
                return Problem(ex.Message);
            }
        }
        return Json(model);
    }

    [HttpPost, Multiple, Order(0), HtmlClass("el-button--danger")]
    public virtual IActionResult Delete([FromBody] Guid[] guids)
    {
        try
        {
            this.Repository.Delete(o => guids.Contains(o.Id));
            this.Repository.SaveChanges();
            return NoContent();
        }
        catch (Exception ex)
        {
            return Problem(ex.Message);
        }
    }

    [HttpPost, Multiple, Order(-2), HtmlClass("el-button--primary")]
    public virtual IActionResult Import(IFormFile importexcelfile)
    {
        try
        {
            return NoContent();
        }
        catch (Exception ex)
        {
            return Problem(ex.Message);
        }
    }

    [HttpPost, Multiple, Order(-1), HtmlClass("el-button--warning")]
    public virtual IActionResult Export([FromBody] PaginationModel<TSearchModel, TListModel> model, bool includeAll = false, bool includeDeleted = false)
    {
        try
        {
            var query = this.BuildQuery(model);
            if (!includeAll)
            {
                query = query.Skip(model.PageSize * (model.PageIndex - 1)).Take(model.PageSize);
            }
            if (includeDeleted)
            {
                this.Repository.DisableSoftDeleteFilter();
            }
            return Json(query.ToList());
        }
        catch (Exception ex)
        {
            return Problem(ex.Message);
        }
    }
}
