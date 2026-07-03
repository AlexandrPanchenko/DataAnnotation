using JetFlight.ApplicationDataAccess.Repository.DataContext;
using JetFlight.Shared.Extensions;
using JetFlight.Shared.UserContext;
using Microsoft.EntityFrameworkCore;
using System.Reflection;
using System.Runtime.Serialization;

namespace JetFlight.Service.Services;
public interface IAuthorizeService
{
    Task<Dictionary<Permission, PermissionLevel>> GetAdminPermissions(int adminId);
}

public class AuthorizeService : IAuthorizeService
{
    private readonly IDataUnitOfWork _unitOfWork;
    private readonly IAdminService _adminService;


    public AuthorizeService(IDataUnitOfWork unitOfWork, IAdminService adminService)
    {
        _unitOfWork = unitOfWork;
        _adminService = adminService;

    }

    public async Task<Dictionary<Permission, PermissionLevel>> GetAdminPermissions(int adminId)
    {
        var admin = await _unitOfWork.Admins.GetById(adminId);

        if (admin == null || admin.Blocked.GetValueOrDefault())
        {
            throw new ArgumentException("Адмін не знайдений або заблокованний.");
        }

        var allPermissions = Enum.GetValues<Permission>();

        if (admin.IsSuperadmin.GetValueOrDefault())
        {
            return allPermissions.ToDictionary(x => x, x => PermissionLevel.Delete);
        }

        var fullAdminDTO = await _adminService.GetAllById(admin.Id);

        var permissions = fullAdminDTO.Roles
            .SelectMany(x => x.Permission.Where(x => x.Crud.HasValue))
            .GroupBy(x => x.EntityType)
            .ToDictionary(x => x.Key, x => (PermissionLevel)x.Max(x => x.Crud!.Value));

        return permissions;
    }
}
