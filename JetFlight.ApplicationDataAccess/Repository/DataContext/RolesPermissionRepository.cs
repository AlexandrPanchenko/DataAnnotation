
namespace JetFlight.ApplicationDataAccess.Repository.DataContext
{
    using JetFlight.ApplicationDataAccess.Entities.DataContext;
    using Microsoft.EntityFrameworkCore;
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;

    public interface IRolesPermissionRepository : IGenericDataRepository<RolesPermission>
    {
        IQueryable<RolesPermission> GetRolePermissions(int id);
        void UpdateRolePermissions(AdminRole role, List<int> permissionsIds);
    }

    public class RolesPermissionRepository : DataGenericRepository<RolesPermission>, IRolesPermissionRepository
    {
        public readonly ApplicationDataAccess.ApplicationDataContext _dbContext;

        public RolesPermissionRepository(ApplicationDataAccess.ApplicationDataContext context) : base(context)
        {
            _dbContext = context;
        }
        public IQueryable<RolesPermission> GetRolePermissions(int id)
        {
            return _dbContext.RolePermission.Include(x => x.Permissions).Where(x => x.RoleId == id).Select(x => x.Permissions);
        }

        public void UpdateRolePermissions(AdminRole role, List<int> permissionsIds)
        {
            var selectedPermissions = _dbContext.RolesPermissions
                .Where(x => permissionsIds.Contains(x.Id))
                .GroupBy(x => x.EntityType)
                .Select(x => x.OrderByDescending(x => x.Crud).First().Id)
                .ToList();

            var existingPermissions = role.RoleToPermissions
                .Select(p => p.PermissionsId)
                .ToList();

            var permissionsToAdd = selectedPermissions.Except(existingPermissions).ToList();
            var permissionsToRemove = existingPermissions.Except(selectedPermissions).ToList();

            var state = _context.Entry(role).State;

            if (state == EntityState.Unchanged && (permissionsToAdd.Any() || permissionsToRemove.Any()))
            {
                _context.Entry(role).State = EntityState.Modified;
            }

            foreach (var permissionId in permissionsToAdd)
            {
                role.RoleToPermissions.Add(new RoleToPermission
                {
                    PermissionsId = permissionId
                });
            }

            foreach (var permissionId in permissionsToRemove)
            {
                var rolePermission = role.RoleToPermissions
                    .FirstOrDefault(p => p.RoleId == role.Id && p.PermissionsId == permissionId);

                if (rolePermission != null)
                {
                    role.RoleToPermissions.Remove(rolePermission);
                }
            }
        }
    }
}
