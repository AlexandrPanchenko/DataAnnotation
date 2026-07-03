using JetFlight.ApplicationDataAccess.Repository.DataContext;
using JetFlight.Shared.Models.Roles;
using JetFlight.Shared.Models.Shared;
using JetFlight.Shared.UserContext;
using Microsoft.EntityFrameworkCore;
using SendGrid.Helpers.Errors.Model;
using System.Data;
using JetFlight.Shared.Constants;
using JetFlight.Shared.Models.LogHistory;
using Newtonsoft.Json;

namespace JetFlight.Service.Services
{
    public interface IRoleService
    {
        Task<List<GetRolePermissionsResponse>> GetAllRolesPermissions();
        Task<GetRolesResponse> GetRoles();
        Task<GetRoleFullResponse> GetRole(int id);
        Task<RoleCreateResponse> CreateRole(RoleCreateRequest role);
        Task<RoleUpdateResponse> UpdateRole(RoleUpdateRequest role);
        Task<DeleteResponseDTO> DeleteRole(int roleId);
    }
    public class RoleService : IRoleService
    {
        private readonly IDataUnitOfWork _unitOfWork;
        private readonly ILogHistoryService _logHistoryService;
        private readonly IUserContext _userContext;

        public RoleService(
            IDataUnitOfWork unitOfWork,
            ILogHistoryService logHistoryService,
            IUserContext userContext)
        {
            _unitOfWork = unitOfWork;
            _logHistoryService = logHistoryService;
            _userContext = userContext;
        }
        public async Task<GetRolesResponse> GetRoles()
        {
            var roles = await _unitOfWork.Roles
                .GetAll()
                .Where(x => x.isActive)
                .OrderByDescending(x => x.Id)
                .ToListAsync();

            var rolesResponse = roles.ToList()
                .Select(role => new GetRoleFullResponse
                {
                    Id = role.Id,
                    Name = role.Name,
                    Title = role.Title,
                    Permission = _unitOfWork.RolesPermission.GetRolePermissions(role.Id).Select(x => new GetRolePermissionsResponse
                    {
                        Id = x.Id,
                        Title = x.Title,
                        Crud = x.Crud,
                        CreatedAt = x.CreatedAt,
                        EntityType = Enum.Parse<Permission>(x.EntityType),
                        UpdatedAt = x.UpdatedAt,
                    }).ToList()
                });


            var response = new GetRolesResponse
            {
                Total = roles.Count(),
                Roles = rolesResponse.ToList()
            };

            return response;
        }

        public async Task<GetRoleFullResponse> GetRole(int id)
        {
            var role = await _unitOfWork.Roles.GetById(id);
            if (role.isActive == false) throw new NotFoundException();
            var response = new GetRoleFullResponse
            {
                Id = role.Id,
                Name = role.Name,
                Title = role.Title,
                Permission = _unitOfWork.RolesPermission.GetRolePermissions(id).Select(x => new GetRolePermissionsResponse
                {
                    Id = x.Id,
                    Title = x.Title,
                    Crud = x.Crud,
                    CreatedAt = x.CreatedAt,
                    EntityType = Enum.Parse<Permission>(x.EntityType),
                    UpdatedAt = x.UpdatedAt,
                }).ToList()
            };
            return response;
        }

        public async Task<RoleCreateResponse> CreateRole(RoleCreateRequest role)
        {
            var response = new RoleCreateResponse();
            var request = new ApplicationDataAccess.Entities.DataContext.AdminRole
            {
                Name = role.Name,
                Title = role.Title,
                isActive = true
            };
            var result = await _unitOfWork.Roles.Add(request);
            if (role.RolePermissions.Any())
            {
                _unitOfWork.RolesPermission.UpdateRolePermissions(result, role.RolePermissions.Select(m => m.Id).ToList());
            }

            await _unitOfWork.Save(skipLogHistory: true);
            await _logHistoryService.AddAsync(new LogMessage
            {
                AdminId = _userContext.AdminId,
                EntityType = "AdminRole",
                UpdatedFrom = null,
                UpdatedTo = JsonConvert.SerializeObject(new AdminRoleLogHistoryDTO
                {
                    Id = result.Id,
                    Name = result.Name,
                    Title = result.Title,
                    isActive = result.isActive,
                    RoleToPermissions = result.RoleToPermissions.Select(x => new RoleToPermissionHistoryDto
                    {
                        PermissionsId = x.Id
                    }).ToList(),
                }, new JsonSerializerSettings()
                {
                    NullValueHandling = NullValueHandling.Ignore,
                }),
                EntityId = result.Id,
                Action = ActionConstant.Inserted,
                Date = DateTime.UtcNow,
            });

            response.Item = new GetRoleResponse()
            {
                Id = result.Id
            };
            return response;
        }

        public async Task<DeleteResponseDTO> DeleteRole(int roleId)
        {
            var roleResponseDto = new DeleteResponseDTO();
            var roleToRemove = await _unitOfWork.Roles.GetById(roleId);
            if (roleToRemove == null || roleToRemove.isActive == false)
            {
                roleResponseDto.Errors.Add("Роль не знайдена");
                return roleResponseDto;
            }

            roleToRemove.isActive = false;
            await _unitOfWork.Save();
            roleResponseDto.Result = true;
            return roleResponseDto;
        }

        public async Task<RoleUpdateResponse> UpdateRole(RoleUpdateRequest role)
        {
            var result = await _unitOfWork.Roles.Find(x => x.Id == role.Id)
                .Include(x => x.RoleToPermissions)
                .FirstOrDefaultAsync();

            var response = new RoleUpdateResponse();
            if (result.isActive == false) response.Errors.Add("Роль не активна");
            if (result != null)
            {
                if (!string.IsNullOrEmpty(role.Name)) result.Name = role.Name;
                if (!string.IsNullOrEmpty(role.Title)) result.Title = role.Title;

                if (!response.Errors.Any())
                {
                    _unitOfWork.RolesPermission.UpdateRolePermissions(result, role.RolePermissions.Select(m => m.Id).ToList());
                    await _unitOfWork.Save();
                    response.Item = new GetRoleResponse()
                    {
                        Id = role.Id
                    };
                }
            }
            return response;

        }

        public async Task<List<GetRolePermissionsResponse>> GetAllRolesPermissions()
        {
            var permissions = await _unitOfWork.RolesPermission.GetAll().ToListAsync();

            var rolesResponse = permissions.ToList()
                .Select(role => new GetRolePermissionsResponse
                {
                    Id = role.Id,
                    Crud = role.Crud,
                    Title = role.Title,
                    EntityType = Enum.Parse<Permission>(role.EntityType)

                }).ToList();

            return rolesResponse;
        }
    }
}
