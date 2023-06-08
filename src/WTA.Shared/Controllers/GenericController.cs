using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WTA.Shared.Application;
using WTA.Shared.Data;
using WTA.Shared.Domain;
using WTA.Shared.Extensions;
using WTA.Shared.Mappers;

namespace WTA.Shared.Controllers;

[GenericControllerNameConvention]
public class GenericController<TEntity> : BaseController, IResourceService<TEntity>
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
        return Json(typeof(PaginationModel<TEntity, TEntity>).GetMetadataForType());
    }

    [HttpPost]
    public IActionResult Index(PaginationModel<TEntity, TEntity> model)
    {
        var query = this.Repository.AsNoTracking();
        model.OrderBy ??= nameof(BaseEntity.Order);
        if (model.OrderBy != null)
        {
            query = query.OrderBy(model.OrderBy);
        }
        model.TotalCount = query.Count();
        query = query.Skip(model.PageSize * (model.PageIndex - 1)).Take(model.PageSize);
        model.Items = query.ToList()
            .Select(o => o.ToObject<TEntity>())
            .ToList();
        return Json(model);
    }

    [HttpPost]
    public IActionResult Details(Guid id)
    {
        var entity = this.Repository.AsNoTracking().FirstOrDefault(o => o.Id == id);
        var model = entity?.ToObject<TEntity>();
        return Json(model);
    }

    [HttpPost]
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

    [HttpPost]
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

    [HttpPost]
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
}
