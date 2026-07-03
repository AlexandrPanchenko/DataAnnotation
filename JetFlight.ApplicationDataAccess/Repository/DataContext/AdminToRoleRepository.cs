using JetFlight.ApplicationDataAccess.Entities.DataContext;

using Microsoft.EntityFrameworkCore;

namespace JetFlight.ApplicationDataAccess.Repository.DataContext;

public interface IAdminToRoleRepository : IGenericDataRepository<AdminToRole>
{
    public IQueryable<AdminToRole> GetPermissionByMail(string email);
    public IQueryable<AdminToRole> GetPermissionByAdminId(int? adminId);
    public IQueryable<int> GetRoleIdByAdminId(int adminId);
}

public class AdminToRoleRepository : DataGenericRepository<AdminToRole>, IAdminToRoleRepository
{
    public readonly ApplicationDataAccess.ApplicationDataContext _dbContext;

    public AdminToRoleRepository(ApplicationDataAccess.ApplicationDataContext context) : base(context)
    {
        _dbContext = context;
    }

    public IQueryable<AdminToRole> GetPermissionByMail(string email)
    {
        return _dbContext.AdminRoles.Include(x => x.Role).Include(x=>x.Admin)
            .Where(x => x.Role.isActive == true && x.Admin.Email.ToLower() == email.ToLower());
    }

    public IQueryable<AdminToRole> GetPermissionByAdminId(int? adminId)
    {
        return _dbContext.AdminRoles.Where(permission => adminId == null || permission.AdminId == adminId);
    }

    public IQueryable<int> GetRoleIdByAdminId(int adminId)
    {
        return _dbContext.AdminRoles.Where(r => r.Admin.Id == adminId).Include(x => x.Role).Where(r => r.Role.isActive == true)
            .Select(x => x.RoleId);
    }
}