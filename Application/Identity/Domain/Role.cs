﻿using WTA.Infrastructure.Domain;

namespace WTA.Application.Domain;

public class Role : BaseEntity
{
    public string Name { get; set; } = null!;
    public string Number { get; set; } = null!;
    public bool IsSystem { get; set; }
    public List<UserRole> UserRoles { get; set; } = new List<UserRole>();
    public List<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
}