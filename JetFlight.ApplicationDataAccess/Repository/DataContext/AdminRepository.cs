using JetFlight.ApplicationDataAccess.Entities.DataContext;
using Microsoft.EntityFrameworkCore;

namespace JetFlight.ApplicationDataAccess.Repository.DataContext;

public interface IAdminRepository : IGenericDataRepository<Admin>
{
    Task<Admin> FindByMailAsync(string mail);
    IQueryable<Admin> GetBySearchParam(string searchParam);
}

public class AdminRepository : DataGenericRepository<Admin>, IAdminRepository
{

    public readonly ApplicationDataAccess.ApplicationDataContext _dbContext;

    public AdminRepository(ApplicationDataAccess.ApplicationDataContext context) : base(context)
    {
        _dbContext = context;
    }

    public async Task<Admin> FindByMailAsync(string mail)
    {
        return await _context.Admins.FirstOrDefaultAsync(x => x.Email.ToLower() == mail.ToLower());
    }

    public IQueryable<Admin> GetBySearchParam(string searchParam)
    {
        return _context.Admins.Where(x => x.FirstName.ToLower().Contains(searchParam.ToLower())
                                         || x.LastName.ToLower().Contains(searchParam.ToLower())
                                         || x.Email.ToLower().Contains(searchParam.ToLower()));
    }

}
