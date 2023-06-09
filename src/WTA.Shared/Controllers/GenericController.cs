using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WTA.Shared.Application;
using WTA.Shared.Attributes;
using WTA.Shared.Data;
using WTA.Shared.Domain;
using WTA.Shared.Extensions;
using WTA.Shared.Mappers;

namespace WTA.Shared.Controllers;

[GenericControllerNameConvention]
public class GenericController<TEntity, TModel> : BaseController, IResourceService<TEntity>
    where TEntity : BaseEntity
{
    public GenericController(IRepository<TEntity> repository)
    {
        this.Repository = repository;
    }

    public IRepository<TEntity> Repository { get; }

    [HttpGet]
    public IActionResult Index()
    {
        return Json(typeof(PaginationModel<TModel, TEntity>).GetMetadataForType());
    }

    [HttpPost, Multiple, Order(-4)]
    public IActionResult Index(PaginationModel<TModel, TEntity> model)
    {
        var isTree = typeof(TEntity).IsAssignableTo(typeof(BaseTreeEntity<TEntity>));
        var query = this.Repository.AsNoTracking();
        query = query.Include();
        query = query.Where(model: model.Query);
        if (isTree)
        {
            model.OrderBy ??= $"{nameof(BaseTreeEntity<TEntity>.ParentId)},{nameof(BaseEntity.Order)},{nameof(BaseEntity.CreatedOn)}";
        }
        model.TotalCount = query.Count();
        if (model.OrderBy != null)
        {
            query = query.OrderBy(model.OrderBy);
        }
        query = query.Skip(model.PageSize * (model.PageIndex - 1)).Take(model.PageSize);
        model.Items = query.ToList();
        return Json(model);
    }

    [HttpPost, Order(-2)]
    public IActionResult Details(Guid id)
    {
        var entity = this.Repository.AsNoTracking().FirstOrDefault(o => o.Id == id);
        var model = entity?.ToObject<TEntity>();
        return Json(model);
    }

    [HttpPost, Multiple, Order(-3)]
    public IActionResult Create(TEntity model)
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

    [HttpPost, Order(-1)]
    public IActionResult Update(TEntity model)
    {
        if (this.ModelState.IsValid)
        {
            try
            {
                var entity = this.Repository.Queryable().FirstOrDefaultAsync(o => o.Id == model.Id);
                if (entity == null)
                {
                    this.ModelState.AddModelError($"{nameof(model.Id)}", $"not found entity by {model.Id}");
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

    [HttpPost, Multiple, Order]
    public IActionResult Delete(Guid[] guids)
    {
        try
        {
            this.Repository.Delete(guids);
            return NoContent();
        }
        catch (Exception ex)
        {
            return Problem(ex.Message);
        }
    }

    [HttpPost, Multiple, Order(-2)]
    public IActionResult Import(Guid[] guids)
    {
        try
        {
            this.Repository.Delete(guids);
            return NoContent();
        }
        catch (Exception ex)
        {
            return Problem(ex.Message);
        }
    }

    [HttpPost, Multiple, Order(-1)]
    public IActionResult Export(Guid[] guids)
    {
        try
        {
            this.Repository.Delete(guids);
            return NoContent();
        }
        catch (Exception ex)
        {
            return Problem(ex.Message);
        }
    }
}
