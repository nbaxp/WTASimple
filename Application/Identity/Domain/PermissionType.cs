using DocumentFormat.OpenXml.Wordprocessing;
using System.ComponentModel.DataAnnotations;

namespace WTA.Application.Identity.Domain;

public enum PermissionType
{
    [Display(Name = "模块")]
    Module = 10,

    [Display(Name = "分组")]
    Group = 20,

    [Display(Name = "资源")]
    Resource = 30,

    [Display(Name = "操作")]
    Operation = 40
}