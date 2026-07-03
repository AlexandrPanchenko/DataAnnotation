using JetFlight.ApplicationDataAccess.Entities.DataContext;
using JetFlight.Shared;
using JetFlight.Shared.Models.Shared;
using Microsoft.EntityFrameworkCore;
using JetFlight.Shared.Models.Admins;
using Serilog;
using JetFlight.ApplicationDataAccess.Repository.DataContext;
using HandlebarsDotNet;
using Microsoft.Extensions.Options;
using JetFlight.Shared.Models.Roles;
using JetFlight.Shared.Extensions;
using JetFlight.Shared.UserContext;
using JetFlight.Shared.Constants;
using JetFlight.Service.Extensions;

namespace JetFlight.Service.Services;

public interface IAdminService
{
    Task<AuthenticateResponse?> Authenticate(AuthenticateEmailRequest model);
    Task<AdminCreateResponseDTO> CreateAdmin(AdminCreateDTO adminDTO);
    Task<ValidateResetPasswordResponseDTO> ValidatePasswordSetup(ValidateResetPasswordDTO validateResetPasswordDTO);
    Task<ValidateResetPasswordResponseDTO> SetupAdminPassword(ResetPasswordDTO resetPasswordDTO);
    Task<PagedListDTO<GetAdminDTO>> GetAdmins(PagingDTO pagingDTO, string? searchParam);
    Task<GetAdminDTO> GetById(int id);
    Task<GetFullAdminDTO> GetAllById(int id);
    public Task<Admin> GetAuthDdAdmin(int id);
    Task<AdminCreateResponseDTO> UpdateAdmin(AdminUpdateDTO admin);
    Task<ValidateResetPasswordResponseDTO> CreatePasswordResetLink(string authCode);
    Task<ValidateResetPasswordResponseDTO> RequestResetPassword(string email);
    Task<List<GetFullAdminDTO>> GetAllByPermissions(Permission pagePermissionType, List<PermissionLevel> crudList, string? searchParam = null);

}

public class AdminService : IAdminService
{
    private readonly IDataUnitOfWork _unitOfWork;
    private readonly INotificationService _mailService;
    private readonly IHtmlGenerationService _htmlGenerationService;
    private readonly IJwtUtils _jwtUtils;
    private readonly AppSettings _appSettings;
    private readonly IUserContext _userContext;

    public AdminService(
        IDataUnitOfWork unitOfWork,
        INotificationService mailService,
        IOptions<AppSettings> appSettings,
        IJwtUtils jwtUtils,
        IHtmlGenerationService htmlGenerationService,
        IUserContext userContext)
    {
        _appSettings = appSettings.Value;
        _mailService = mailService;
        _unitOfWork = unitOfWork;
        _jwtUtils = jwtUtils;
        _htmlGenerationService = htmlGenerationService;
        _userContext = userContext;
    }

    public async Task<AuthenticateResponse?> Authenticate(AuthenticateEmailRequest model)
    {
        var admin = await _unitOfWork.Admins.FindByMailAsync(model.Email);
        if (admin == null || (admin.Blocked.HasValue && admin.Blocked.Value)) return null;

        var salt = Convert.FromBase64String(admin.Salt);
        var hashedPassword = CryptographyHelper.HashPassword(model.Password, salt);
        if (hashedPassword.Equals(admin.Password))
        {
            var jwtToken = _jwtUtils.GenerateJwtToken(admin);
            await _unitOfWork.Save();
            Log.Information($"Successful Login using email and password for {model.Email}");
            return new AuthenticateResponse(jwtToken);
        }

        Log.Information($"Unsuccessful Login attempt using email and password for {model.Email}");

        return null;
    }
    public async Task<ICollection<PermissionGetDTO>> GetPermissions(int? adminID)
    {
        return await _unitOfWork.AdminRole.GetPermissionByAdminId(adminID)
            .Select(permission => new PermissionGetDTO
            {
                PermissionId = permission.Id,
                Role = new RoleDTO { Id = permission.RoleId, Name = permission.Role.Name, Description = permission.Role.Title },
                Admin = new AdminUpdateDTO { Id = permission.AdminId, FirstName = permission.Admin.FirstName, LastName = permission.Admin.LastName }
            }).ToListAsync();
    }

    public async Task<AdminCreateResponseDTO> UpdateAdmin(AdminUpdateDTO admin)
    {
        var result = await _unitOfWork.Admins.GetById(admin.Id);
        var updateAdminResponse = new AdminCreateResponseDTO();

        if (result != null)
        {
            if (!string.IsNullOrEmpty(admin.LastName)) result.LastName = admin.LastName;
            if (!string.IsNullOrEmpty(admin.FirstName)) result.FirstName = admin.FirstName;
            if (admin.Blocked != null) result.Blocked = admin.Blocked;
            if (result.PhoneNumber != admin.PhoneNumber) result.PhoneNumber = admin.PhoneNumber;
            if (!string.IsNullOrEmpty(admin.Password))
            {
                result.Password = CryptographyHelper.HashPassword(admin.Password, out var salt);
                result.Salt = Convert.ToBase64String(salt);
            }
            if (!string.IsNullOrEmpty(admin.Email) && result.Email != admin.Email)
            {
                var adminToUpdate = await _unitOfWork.Admins.FindByMailAsync(admin.Email);
                if (adminToUpdate != null && adminToUpdate.Id != admin.Id)
                {
                    updateAdminResponse.Errors.Add("Користувач з такою поштою вже існує");
                }
                if (!admin.Email.IsValidEmail())
                {
                    updateAdminResponse.Errors.Add($"{admin.Email} не є валідною адресою");
                }
                if (!admin.Roles.Any())
                {
                    updateAdminResponse.Errors.Add("Мінімум одна роль повинна бути додана до користувача");
                }
                result.Email = admin.Email;
            }
            result.UpdatedAt = DateTime.UtcNow;


            var adminRoles = await _unitOfWork.AdminRole.GetAll().Where(x => x.AdminId == admin.Id).ToListAsync();
            var rolesToCreate = admin.Roles.FindAll(id => !adminRoles.Select(o => o.RoleId).Contains(id));
            var rolesToDelete = adminRoles.FindAll(o => !admin.Roles.Contains(o.RoleId));

            var newRoles = new List<AdminToRole>();
            rolesToCreate.ForEach(roleId => newRoles.Add(new AdminToRole
            {
                AdminId = admin.Id,
                RoleId = roleId
            }));

            await _unitOfWork.AdminRole.AddRange(newRoles);
            rolesToDelete.ForEach(o => _unitOfWork.AdminRole.Remove(o));

        }
        else
        {
            updateAdminResponse.Errors.Add("Користувач не існує");
        }

        if (!updateAdminResponse.Errors.Any())
        {

            await _unitOfWork.Save();

            if (result != null)
            {
                return new AdminCreateResponseDTO
                {
                    Item = new GetAdminDTO { Id = result.Id, FirstName = result.FirstName, LastName = result.LastName }
                };
            }
        }
        return updateAdminResponse;
    }

    public async Task<PagedListDTO<GetAdminDTO>> GetAdmins(PagingDTO pagingDTO, string? searchParam)
    {

        var searchParts = searchParam?.ToLower().Split(' ', StringSplitOptions.RemoveEmptyEntries) ?? Array.Empty<string>();

        var query = _unitOfWork.Admins.GetAll()
                .Where(x => x.Blocked == false);

        foreach (var part in searchParts)
        {
            query = query.Where(x =>
                x.FirstName.ToLower().Contains(part)
                || x.LastName.ToLower().Contains(part)
                || x.Email.ToLower().Contains(part));
        }


        var result = await query.GetPagedListAsync(pagingDTO, ToDTO);
        return result;
    }

    private GetAdminDTO ToDTO(ApplicationDataAccess.Entities.DataContext.Admin admin)
    {
        return new GetAdminDTO
        {
            Email = admin.Email,
            FirstName = admin.FirstName,
            LastName = admin.LastName,
            PhoneNumber = admin.PhoneNumber,
            Blocked = admin.Blocked.HasValue ? admin.Blocked.Value : true,
            Id = admin.Id,
            CreatedAt = admin.CreatedAt ?? null,
            Roles = _unitOfWork.AdminRole.GetPermissionByMail(admin.Email.Trim()).Select(x => new Shared.Models.Roles.RoleDTO
            {
                Id = x.Role.Id,
                Name = x.Role.Name,
                Description = x.Role.Title,

            }).ToList()
        };
     }

    public async Task<GetAdminDTO> GetById(int id)
    {
        var admin = await _unitOfWork.Admins.GetAll().Where(x => x.Id == id && x.Blocked == false).FirstOrDefaultAsync()
            ?? throw new Exception($"No admin found with id {id}");

        var adminDto = new GetAdminDTO
        {
            Email = admin.Email,
            FirstName = admin.FirstName,
            LastName = admin.LastName,
            Id = admin.Id,
            PhoneNumber = admin.PhoneNumber,
            Blocked = admin.Blocked.HasValue ? admin.Blocked.Value : true,
            CreatedAt = admin.CreatedAt ?? null,
            Roles = _unitOfWork.AdminRole.GetPermissionByMail(admin.Email.Trim()).Select(x => new Shared.Models.Roles.RoleDTO
            {
                Id = x.Role.Id,
                Name = x.Role.Name,
                Description = x.Role.Title,

            }).ToList()

        };
        return adminDto;
    }

    public async Task<List<GetFullAdminDTO>> GetAllByPermissions(Permission pagePermissionType, List<PermissionLevel> crudList, string? searchParam = null)
    {
        var searchParts = !string.IsNullOrEmpty(searchParam) ? searchParam.ToLower().Split(' ', StringSplitOptions.RemoveEmptyEntries) : null;
        var users = string.IsNullOrEmpty(searchParam)
            ? await _unitOfWork.Admins.GetAll().Where(x => x.Blocked == false).ToListAsync()
            : _unitOfWork.Admins.GetAll().ToList().Where(x => x.Blocked == false && searchParts.All(part => (x.FirstName.ToLower().Contains(part.ToLower())
                        || (x.LastName.ToLower().Contains(part.ToLower()))
                        || (x.Email.ToLower().Contains(part.ToLower()))))).ToList();

        // Convert PermissionType list to byte list
        var crudByteList = crudList?.Select(c => (byte)c).ToList();

        var result = new List<GetFullAdminDTO>();

        foreach (var user in users)
        {
            var roles = _unitOfWork.AdminRole.GetPermissionByMail(user.Email.Trim())
                .Select(x => new RoleDTO
                {
                    Id = x.Role.Id,
                    Name = x.Role.Name,
                    Description = x.Role.Title,
                }).ToList();

            var filteredRoles = roles
                .Select(role => new GetRoleFullResponse
                {
                    Id = role.Id,
                    Name = role.Name,
                    Title = role.Description,
                    Permission = _unitOfWork.RolesPermission.GetRolePermissions(role.Id)
                       .Where(p => p.EntityType == pagePermissionType.ToString() && (crudByteList == null || crudByteList.Contains(p.Crud.Value)))
                        .Select(x => new GetRolePermissionsResponse
                        {
                            Id = x.Id,
                            Title = x.Title,
                            Crud = x.Crud,
                            CreatedAt = x.CreatedAt,
                            EntityType = Enum.Parse<Permission>(x.EntityType),
                            UpdatedAt = x.UpdatedAt,
                        }).ToList()
                })
                .Where(role => role.Permission.Any())
                .ToList();

            var isSuperAdmin = user.IsSuperadmin ?? false;

            // Додаємо користувача або якщо в нього є потрібні права, або якщо він SuperAdmin
            if (filteredRoles.Any() || isSuperAdmin)
            {
                var userDto = new GetFullAdminDTO
                {
                    Email = user.Email,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Id = user.Id,
                    SuperAdmin = isSuperAdmin,
                    PhoneNumber = user.PhoneNumber,
                    Blocked = user.Blocked.HasValue ? user.Blocked.Value : true,
                    CreatedAt = user.CreatedAt ?? null,
                    Roles = filteredRoles
                };

                result.Add(userDto);
            }
        }

        return result;
    }

    public async Task<GetFullAdminDTO> GetAllById(int id)
    {
        var admin = await _unitOfWork.Admins.GetAll().Where(x => x.Id == id && x.Blocked == false).FirstOrDefaultAsync();
        if (admin == null)
        {
            throw new Exception($"No admin found with id {id}");
        }

        var roles = _unitOfWork.AdminRole.GetPermissionByMail(admin.Email.Trim()).Select(x => new Shared.Models.Roles.RoleDTO
        {
            Id = x.Role.Id,
            Name = x.Role.Name,
            Description = x.Role.Title,

        }).ToList();

        var adminDto = new GetFullAdminDTO
        {
            Email = admin.Email,
            FirstName = admin.FirstName,
            LastName = admin.LastName,
            Id = admin.Id,
            SuperAdmin = admin.IsSuperadmin ?? false,

            PhoneNumber = admin.PhoneNumber,
            Blocked = admin.Blocked.HasValue ? admin.Blocked.Value : true,
            CreatedAt = admin.CreatedAt ?? null,
            Roles = roles
                .Select(role => new GetRoleFullResponse
                {
                    Id = role.Id,
                    Name = role.Name,
                    Title = role.Description,
                    Permission = _unitOfWork.RolesPermission.GetRolePermissions(role.Id).Select(x => new GetRolePermissionsResponse
                    {
                        Id = x.Id,
                        Title = x.Title,
                        Crud = x.Crud,
                        CreatedAt = x.CreatedAt,
                        EntityType = Enum.Parse<Permission>(x.EntityType),
                        UpdatedAt = x.UpdatedAt,
                    }).ToList()
                }).ToList()

        };
        return adminDto;
    }
    public async Task<AdminCreateResponseDTO> CreateAdmin(AdminCreateDTO adminDTO)
    {
        var createAdminResponse = new AdminCreateResponseDTO();
        if (string.IsNullOrEmpty(adminDTO.Email))
        {
            createAdminResponse.Errors.Add("Необхідно вказати пошту");
        }
        if (!string.IsNullOrEmpty(adminDTO.Email) && await _unitOfWork.Admins.Any(x => x.Email.Trim().ToLower() == adminDTO.Email.Trim().ToLower() && x.Blocked == false))
        {
            createAdminResponse.Errors.Add("Користувач з введеною поштою вже існує");
        }
        if (!string.IsNullOrEmpty(adminDTO.PhoneNumber) && await _unitOfWork.Admins.Any(x => x.PhoneNumber.Trim().ToLower() == adminDTO.PhoneNumber.Trim().ToLower() && x.Blocked == false))
        {
            createAdminResponse.Errors.Add("Користувач з введеним номером телефону вже існує");
        }
        if (!string.IsNullOrEmpty(adminDTO.Email) && !adminDTO.Email.IsValidEmail())
        {
            createAdminResponse.Errors.Add($"{adminDTO.Email} не валідна пошта");
        }
        if (string.IsNullOrEmpty(adminDTO.FirstName) || string.IsNullOrEmpty(adminDTO.LastName))
        {
            createAdminResponse.Errors.Add("Імя або/та Призвище не вказано");
        }

        if (createAdminResponse.Errors.Any()) return createAdminResponse;

        if (await _unitOfWork.Admins.Any(x => x.Email.Trim().ToLower() == adminDTO.Email.Trim().ToLower() && x.Blocked == true))
        {

            var adminToRecreate = await _unitOfWork.Admins.GetAll().Where(x => x.Email.Trim().ToLower() == adminDTO.Email.Trim().ToLower() && x.Blocked == true).FirstAsync();

            var res = await UpdateAdmin(new AdminUpdateDTO
            {
                Blocked = false,
                Email = adminDTO.Email,
                FirstName = adminDTO.FirstName,
                LastName = adminDTO.LastName,
                Id = adminToRecreate.Id,
                PhoneNumber = adminDTO.PhoneNumber,
                Roles = adminDTO.Roles
            });

            await SendPasswordEmailNotification(adminToRecreate.Id, adminDTO.FirstName, adminDTO.Email);

            return new AdminCreateResponseDTO
            {
                Item = new GetAdminDTO { Id = res.Item.Id }
            };
        }

        var password = CryptographyHelper.HashPassword("test123", out byte[] salt);

        var admin = new ApplicationDataAccess.Entities.DataContext.Admin()
        {
            Email = adminDTO.Email,
            FirstName = adminDTO.FirstName,
            PhoneNumber = adminDTO.PhoneNumber,
            LastName = adminDTO.LastName,
            Password = password,
            Salt = salt == null ? null : Convert.ToBase64String(salt),
            CreatedBy = _userContext.AdminId,
            Blocked = false,
            CreatedAt = DateTime.UtcNow
        };

        var result = await _unitOfWork.Admins.Add(admin);

        await _unitOfWork.Save();

        var adminRole = new List<AdminToRole>();

        adminDTO.Roles.ForEach(role => adminRole.Add(new AdminToRole()
        {
            AdminId = result.Id,
            RoleId = role,
        }));

        await _unitOfWork.AdminRole.AddRange(adminRole);
        await _unitOfWork.Save();

        await SendPasswordEmailNotification(admin.Id, admin.FirstName, adminDTO.Email);

        return new AdminCreateResponseDTO
        {
            Item = new GetAdminDTO { Id = admin.Id }
        };
    }

    private async Task SendPasswordEmailNotification(int adminId, string name, string email)
    {
        var ResetPasswordData = new ResetPassword()
        {
            AuthCode = Guid.NewGuid().ToString(),
            IsActive = true,
            AdminId = adminId,
            CreatedDateTime = DateTime.UtcNow
        };
        await _unitOfWork.ResetPasswords.Add(ResetPasswordData);
        await _unitOfWork.Save();

        string subject = "Admin Password setup";
        var resetPasswordUrl = $"{_appSettings.BaseAdminUrl}/auth/set-password?AuthCode={ResetPasswordData.AuthCode}";
        var emailBody = await _htmlGenerationService.GenerateSetupPasswordEmail(name, resetPasswordUrl);

        var emailRequest = new EmailMessage
        {
            To = new List<string> { email },
            From = EmailFrom.Admin,
            Subject = subject,
            Body = emailBody,
        };

        await _mailService.SendEmailAsync(emailRequest);
    }

    public async Task<ValidateResetPasswordResponseDTO> ValidatePasswordSetup(ValidateResetPasswordDTO validateResetPasswordDTO)
    {
        var records = await _unitOfWork.ResetPasswords.Find(x => x.AuthCode == validateResetPasswordDTO.VerificationCode && x.IsActive == true).ToListAsync();

        if (records.Any(x => DateTime.UtcNow.Subtract(x.CreatedDateTime).TotalHours < 24))
        {
            return new ValidateResetPasswordResponseDTO { Success = true };
        }
        else
        {
            return new ValidateResetPasswordResponseDTO { Success = false, Message = "Посилання не активне" };
        }
    }

    public async Task<ValidateResetPasswordResponseDTO> SetupAdminPassword(ResetPasswordDTO resetPasswordDTO)
    {
        var resetPasswordRecord = await _unitOfWork.ResetPasswords.GetResetPasswords(resetPasswordDTO.AuthCode).FirstOrDefaultAsync();

        if (resetPasswordRecord != null && DateTime.UtcNow.Subtract(resetPasswordRecord.CreatedDateTime).TotalHours < 24)
        {
            var adminExist = await _unitOfWork.Admins.Find(p => p.Email.ToLower() == resetPasswordRecord.Admin.Email.ToLower()).FirstOrDefaultAsync();
            if (adminExist != null)
            {
                var password = CryptographyHelper.HashPassword(resetPasswordDTO.NewPassword, out byte[] salt);
                adminExist.Password = password;
                adminExist.Salt = salt == null ? null : Convert.ToBase64String(salt);
                resetPasswordRecord.IsActive = false;
                await _unitOfWork.Save();
                return new ValidateResetPasswordResponseDTO { Success = true };
            }
            else
            {
                return new ValidateResetPasswordResponseDTO { Success = false, Message = "Користувач не існує" };
            }
        }
        else
        {
            return new ValidateResetPasswordResponseDTO { Success = false, Message = "Посилання не активне" };
        }
    }

    public async Task<ValidateResetPasswordResponseDTO> CreatePasswordResetLink(string authCode)
    {
        var resetPasswordRecord = await _unitOfWork.ResetPasswords.GetResetPasswords(authCode).FirstOrDefaultAsync();

        if (resetPasswordRecord != null)
        {
            var adminExist = await _unitOfWork.Admins.Find(p => p.Email.ToLower() == resetPasswordRecord.Admin.Email.ToLower() && p.Blocked != true).FirstOrDefaultAsync();
            if (adminExist != null)
            {
                await SendPasswordEmailNotification(adminExist.Id, adminExist.FirstName, adminExist.Email);
                return new ValidateResetPasswordResponseDTO { Success = true };
            }
            else
            {
                return new ValidateResetPasswordResponseDTO { Success = false, Message = "Користувач не існує" };
            }
        }
        else
        {
            return new ValidateResetPasswordResponseDTO { Success = false, Message = "Посилання не активне" };
        }
    }
    public async Task<ValidateResetPasswordResponseDTO> RequestResetPassword(string email)
    {
        var adminExist = await _unitOfWork.Admins.Find(p => p.Email.ToLower() == email.ToLower() && p.Blocked != true).FirstOrDefaultAsync();
        if (adminExist != null)
        {
            await SendPasswordEmailNotification(adminExist.Id, adminExist.FirstName, adminExist.Email);
            return new ValidateResetPasswordResponseDTO { Success = true };
        }
        else
        {
            return new ValidateResetPasswordResponseDTO { Success = false, Message = "Користувач не існує" };
        }

    }

    public async Task<Admin> GetAuthDdAdmin(int id)
    {
        return await _unitOfWork.Admins.GetById(id);
    }
}

