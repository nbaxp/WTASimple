using System.ComponentModel.DataAnnotations;
using WTA.Shared.Domain;

namespace WTA.Application.Identity.Entities;

[Display(Name = "部门")]
public class Department : BaseTreeEntity<Department>
{
}
